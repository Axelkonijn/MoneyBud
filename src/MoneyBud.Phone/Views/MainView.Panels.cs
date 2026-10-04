using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MoneyBud.Domain;
using MoneyBud.Presentation;

namespace MoneyBud.Phone.Views;

/// <summary>
/// What is on each panel: the lists, the forms, the budget and the accounts. Every figure, word and
/// list comes from <see cref="MoneyBudApp"/> and its forms; every act goes through them, or through
/// <see cref="PhoneScreen"/> where it also moves a panel. The layout is the approved prototype's
/// (plan for increment 14, D8).
/// </summary>
public sealed partial class MainView
{
    private readonly List<TextBlock> _saveLines = [];
    private readonly List<(Grid Page, bool TopInset, bool Footer)> _formPages = [];
    private Action? _refreshForms;
    private bool _budgetWaiting;

    private void RebuildAll()
    {
        if (!_opened)
        {
            return;
        }

        // Only what is on screen: a panel is built afresh as it is pulled in (BuildPanel).
        if (IncomeSide.IsVisible)
        {
            BuildEntryList(PhonePanel.Income);
        }

        if (ExpenseSide.IsVisible)
        {
            BuildEntryList(PhonePanel.Expenses);
        }

        if (BudgetSheet.IsVisible)
        {
            BuildBudget();
        }

        if (AccountsSide.IsVisible)
        {
            BuildAccounts();
            if (App.IsHistoryOpen)
            {
                BuildHistory();
            }
        }

        _refreshForms?.Invoke();
        foreach (var (page, top, footer) in _formPages)
        {
            Fit(page, top, footer);
        }
    }

    private void BuildPanel(PhonePanel panel)
    {
        switch (panel)
        {
            case PhonePanel.Income:
            case PhonePanel.Expenses:
                BuildEntryList(panel);
                break;
            case PhonePanel.Budget:
                BuildBudget();
                break;
            case PhonePanel.Accounts:
                BuildAccounts();
                if (App.IsHistoryOpen)
                {
                    BuildHistory();
                }

                break;
        }
    }

    /// <summary>A panel's page: a heading that stays, the save line under it when there is one, and everything under that scrolling.</summary>
    private Grid Page(Control header, Control body, bool topInset = true, Control? footer = null)
    {
        var grid = new Grid { RowDefinitions = RowDefinitions.Parse("Auto,Auto,*,Auto") };
        grid.Children.Add(header);

        var save = SaveLineBlock();
        save.Margin = new Thickness(20, 0, 20, 6);
        Grid.SetRow(save, 1);
        grid.Children.Add(save);

        var scroller = new ScrollViewer { Content = body, Padding = new Thickness(0, 0, 0, _keyboard) };
        Grid.SetRow(scroller, 2);
        grid.Children.Add(scroller);

        if (footer is not null)
        {
            Grid.SetRow(footer, 3);
            grid.Children.Add(footer);
        }

        Fit(grid, topInset, footer is not null);
        return grid;
    }

    /// <summary>Keeps a page clear of the phone's status bar and navigation bar.</summary>
    private void Fit(Grid page, bool topInset, bool footer)
    {
        page.Children[0].Margin = new Thickness(20, (topInset ? _safe.Top : 0) + 14, 10, 8);
        if (page.Children[2] is ScrollViewer { Content: Control body })
        {
            body.Margin = new Thickness(16, 0, 16, (footer ? 0 : _safe.Bottom) + 36);
        }
    }

    private static Grid Header(string title, string? subtitle, params Control[] actions)
    {
        var titles = Ui.Stack(1, Ui.Text(title, "h1"));
        if (subtitle is not null)
        {
            titles.Children.Add(Ui.Text(subtitle, "muted"));
        }

        var row = Ui.Row(0, actions);
        row.VerticalAlignment = VerticalAlignment.Center;
        return Ui.Columns("*,Auto", titles, row);
    }

    /// <summary>A save line for a panel's header: shown with a panel over the home screen, as ruled.</summary>
    private TextBlock SaveLineBlock()
    {
        var block = Ui.Text("", "faint");
        block.TextWrapping = TextWrapping.Wrap;
        block.TextTrimming = TextTrimming.None;
        _saveLines.RemoveAll(b => TopLevel.GetTopLevel(b) is null);
        _saveLines.Add(block);
        ShowSaveLine(block);
        return block;
    }

    private void ShowSaveLine(TextBlock block)
    {
        var line = App.SaveLine;
        block.Text = line;
        block.IsVisible = line is not null;
        block.Classes.Set("danger", App.IsUnsaved);
        block.Classes.Set("positive", !App.IsUnsaved);
        if (block == SaveLine)
        {
            foreach (var other in _saveLines)
            {
                ShowSaveLine(other);
            }
        }
    }

    // ---- Income and expenses: the lists --------------------------------------------------------

    private void BuildEntryList(PhonePanel panel)
    {
        var income = panel == PhonePanel.Income;
        var overview = App.Overview;
        var body = new StackPanel();

        var total = Ui.Stack(0,
            Ui.Text(Tekst.Euro(income ? overview.IncomeTotal : overview.ExpenseTotal), "big"),
            Ui.Text(income ? Tekst.IncomeTotalCaption : Tekst.ExpenseTotalCaption, "muted"));
        total.Margin = new Thickness(4, 4, 0, 4);
        body.Children.Add(total);

        var rows = income
            ? overview.Incomes.Select(l => (l.Date, Row: IncomeRow(l)))
            : overview.Expenses.Select(l => (l.Date, Row: ExpenseRow(l, overview)));
        var any = false;
        foreach (var day in rows.GroupBy(r => r.Date))
        {
            any = true;
            body.Children.Add(Ui.Caption(Tekst.DayHeading(day.Key, App.Ledger.Today)));
            body.Children.Add(Ui.List(day.Select(r => r.Row)));
        }

        if (!any)
        {
            var nothing = Ui.Text(income ? Tekst.NoIncomes : Tekst.NoExpenses, "muted");
            nothing.Margin = new Thickness(4, 24, 0, 0);
            body.Children.Add(nothing);
        }

        var hint = Ui.Text(income ? Tekst.SwipeForNewIncome : Tekst.SwipeForNewExpense, "faint");
        hint.HorizontalAlignment = HorizontalAlignment.Center;
        hint.Margin = new Thickness(0, 28, 0, 0);
        body.Children.Add(hint);

        (income ? IncomeList : ExpenseList).Child = Page(Header(income ? Tekst.Incomes : Tekst.Expenses, App.PeriodTitle), body);
    }

    private Control ExpenseRow(ExpenseLine line, PeriodOverview overview)
    {
        var slice = overview.Rows.FirstOrDefault(r => r.Name == line.Category)?.SliceIndex;
        var dot = Dot(slice);
        dot.Margin = new Thickness(0, 0, 14, 0);

        var primary = line.Label ?? line.Category;
        var secondary = line.AccountName is { } account ? $"{line.Category} · {account}" : line.Category;
        var texts = Ui.Stack(2, Ui.Text(primary, "row"), Ui.Text(secondary, "muted"));
        return Ui.Plain(Ui.Columns("Auto,*,Auto", dot, texts, Amount(line.AmountText, line.RepeatLabel)), () => _screen.OpenEntry(line), new Thickness(16, 13));
    }

    private Control IncomeRow(IncomeLine line)
    {
        var dot = Ui.ColouredDot("Positive");
        dot.Margin = new Thickness(0, 0, 14, 0);
        var texts = Ui.Stack(2, Ui.Text(line.Label, "row"));
        if (line.AccountName is { } account)
        {
            texts.Children.Add(Ui.Text(account, "muted"));
        }

        return Ui.Plain(Ui.Columns("Auto,*,Auto", dot, texts, Amount(line.AmountText, line.RepeatLabel)), () => _screen.OpenEntry(line), new Thickness(16, 13));
    }

    /// <summary>An entry's amount, with the grey <i>maandelijks</i> under it on the latest occurrence of a repeat.</summary>
    private static Control Amount(string amount, string? repeats)
    {
        var figure = Ui.Text(amount, "row");
        figure.HorizontalAlignment = HorizontalAlignment.Right;
        var right = Ui.Stack(2, figure);
        if (repeats is not null)
        {
            var label = Ui.Text(repeats, "faint");
            label.HorizontalAlignment = HorizontalAlignment.Right;
            right.Children.Add(label);
        }

        right.VerticalAlignment = VerticalAlignment.Center;
        return right;
    }

    // ---- Income and expenses: the forms --------------------------------------------------------
    //
    // Built once, and kept: every field shows and sets the shared form, which fills itself when an
    // entry is loaded and empties itself when one goes through. Only the chips are made again, when
    // what they offer or which is chosen changes.

    private void BuildForms()
    {
        BuildIncomeForm();
        BuildExpenseForm();
    }

    private void BuildIncomeForm()
    {
        var form = App.IncomeForm;
        var title = Ui.Text("", "h1");
        var submit = Ui.Round(Ui.Plus, () => Act(_screen.SubmitIncome));
        var label = Ui.Linked(Ui.Field(Tekst.ExampleIncome), form, nameof(IncomeForm.Label), () => form.Label, v => form.Label = v);
        var amount = AmountField(form, nameof(IncomeForm.Amount), () => form.Amount, v => form.Amount = v);
        var dates = new ContentControl();
        var accounts = new ContentControl();
        var repeats = new ContentControl();
        var remove = RemoveButton();

        var body = Ui.Stack(0,
            Ui.Caption(Tekst.Label), label,
            Ui.Caption(Tekst.Amount), amount,
            Ui.Caption(Tekst.Date), dates,
            Ui.Caption(Tekst.Account), accounts,
            Ui.Caption(Tekst.Frequency), repeats,
            remove);

        void Refresh()
        {
            title.Text = form.IsEditing ? Tekst.ChangeIncome : Tekst.NewIncome;
            submit.Content = Ui.Icon(form.IsEditing ? Ui.Tick : Ui.Plus, "OnAccent", 20, 2.6);
            remove.IsVisible = form.IsEditing;
            dates.Content = DateChips(form.Date, allowFuture: true, d => form.Date = d);
            accounts.Content = AccountChips(form.ChosenAccount, a => form.ChosenAccount = a);
            repeats.Content = FrequencyChips(form.ChosenFrequency, form.CanChangeFrequency, f => form.ChosenFrequency = f);
        }

        Follow(form, Refresh, nameof(IncomeForm.Label), nameof(IncomeForm.Amount));
        Refresh();
        var page = Page(Ui.Columns("*,Auto", title, submit), body);
        _formPages.Add((page, true, false));
        IncomeFormPanel.Child = page;
    }

    private void BuildExpenseForm()
    {
        var form = App.ExpenseForm;
        var title = Ui.Text("", "h1");
        var submit = Ui.Round(Ui.Plus, () => Act(_screen.SubmitExpense));
        var label = Ui.Linked(Ui.Field(Tekst.ExampleExpense), form, nameof(ExpenseForm.Label), () => form.Label, v => form.Label = v);
        var category = Ui.Linked(Ui.Field(Tekst.ChooseOrTypeCategory), form, nameof(ExpenseForm.Category), () => form.Category, v => form.Category = v);
        var suggestions = new ContentControl { Margin = new Thickness(0, 10, 0, 0) };
        var amount = AmountField(form, nameof(ExpenseForm.Amount), () => form.Amount, v => form.Amount = v);
        var dates = new ContentControl();
        var accounts = new ContentControl();
        var repeats = new ContentControl();
        var remove = RemoveButton();

        var body = Ui.Stack(0,
            Ui.Caption(Tekst.Label), label,
            Ui.Caption(Tekst.Category), category, suggestions,
            Ui.Caption(Tekst.Amount), amount,
            Ui.Caption(Tekst.Date), dates,
            Ui.Caption(Tekst.Account), accounts,
            Ui.Caption(Tekst.Frequency), repeats,
            remove);

        void Refresh()
        {
            title.Text = form.IsEditing ? Tekst.ChangeExpense : Tekst.NewExpense;
            submit.Content = Ui.Icon(form.IsEditing ? Ui.Tick : Ui.Plus, "OnAccent", 20, 2.6);
            remove.IsVisible = form.IsEditing;
            suggestions.Content = SuggestionChips(form.Category, name => form.Category = name);
            dates.Content = DateChips(form.Date, allowFuture: false, d => form.Date = d);
            accounts.Content = AccountChips(form.ChosenAccount, a => form.ChosenAccount = a, form.IsAccountLocked);
            repeats.Content = FrequencyChips(form.ChosenFrequency, form.CanChangeFrequency, f => form.ChosenFrequency = f);
        }

        Follow(form, Refresh, nameof(ExpenseForm.Label), nameof(ExpenseForm.Amount));
        Refresh();
        var page = Page(Ui.Columns("*,Auto", title, submit), body);
        _formPages.Add((page, true, false));
        ExpenseFormPanel.Child = page;
    }

    /// <summary>Remakes a form's chips when anything but its typed text changes, and after every redraw.</summary>
    private void Follow(INotifyPropertyChanged form, Action refresh, params string[] typed)
    {
        form.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is { } name && !typed.Contains(name))
            {
                refresh();
            }
        };
        _refreshForms += refresh;
    }

    private Button RemoveButton()
    {
        var remove = Ui.Pill(Tekst.Remove, () => Act(_screen.RemoveEntry), "DangerGhost");
        remove.HorizontalAlignment = HorizontalAlignment.Center;
        remove.Margin = new Thickness(0, 28, 0, 0);
        return remove;
    }

    private static TextBox AmountField(INotifyPropertyChanged form, string property, Func<string?> get, Action<string?> set)
    {
        var amount = Ui.Linked(Ui.Field("0,00"), form, property, get, set);
        amount.Classes.Add("amount");
        var euro = Ui.Text("€", "h2", "figure");
        euro.Margin = new Thickness(16, 0, 0, 0);
        euro.Res(TextBlock.ForegroundProperty, "Muted");
        amount.InnerLeftContent = euro;
        TextInputOptions.SetContentType(amount, TextInputContentType.Number);
        return amount;
    }

    /// <summary>
    /// The date: <i>Vandaag</i> — the form's empty date, which means today — <i>Gisteren</i>, or
    /// another day from a calendar (plan D8). An expense's calendar ends at today, since a future
    /// expense is refused; an income's does not.
    /// </summary>
    private Control DateChips(DateTime? date, bool allowFuture, Action<DateTime?> set)
    {
        var today = App.Ledger.Today.ToDateTime(TimeOnly.MinValue);
        var yesterday = today.AddDays(-1);
        var other = date is { } d && d.Date != today && d.Date != yesterday ? Tekst.DayName(DateOnly.FromDateTime(d)) : Tekst.OtherDate;
        var chosen = date is null || date.Value.Date == today ? Tekst.Today : date.Value.Date == yesterday ? Tekst.Yesterday : other;
        return Ui.Chips([Tekst.Today, Tekst.Yesterday, other], chosen, picked =>
        {
            if (picked == Tekst.Today)
            {
                set(null);
            }
            else if (picked == Tekst.Yesterday)
            {
                set(yesterday);
            }
            else
            {
                PickDate(date ?? today, allowFuture, day => set(day));
            }
        });
    }

    /// <summary>
    /// The accounts, in the strip's order, the one the form holds chosen. Locked, as the form decides,
    /// only the chosen one can be tapped: a backed category's expense is on its account (§12).
    /// </summary>
    private Control AccountChips(Account chosen, Action<Account> set, bool locked = false)
    {
        var accounts = App.AccountChoices;
        return Ui.Chips(accounts.Select(a => a.Name), chosen.Name, picked => set(accounts.First(a => a.Name == picked)),
            locked ? name => name == chosen.Name : null);
    }

    /// <summary><i>Herhalen</i>: fixed on <i>Eenmalig</i> for an earlier occurrence, as the form decides.</summary>
    private static Control FrequencyChips(FrequencyChoice chosen, bool canChange, Action<FrequencyChoice> set) =>
        Ui.Chips(FrequencyChoice.All.Select(c => c.Text), chosen.Text,
            picked => set(FrequencyChoice.All.First(c => c.Text == picked)), _ => canChange);

    /// <summary>The categories offered, narrowing as a name is typed (the desktop's suggestions, §12).</summary>
    private Control SuggestionChips(string? typed, Action<string> pick)
    {
        var panel = new WrapPanel();
        foreach (var name in App.SuggestionsFor(typed))
        {
            var slice = App.Overview.Rows.FirstOrDefault(r => r.Name == name)?.SliceIndex;
            panel.Children.Add(Ui.Suggestion(name, slice, () => pick(name)));
        }

        return panel;
    }

    /// <summary>An act from a button: the keyboard goes away first, so the redraw that follows can rebuild what was typed in.</summary>
    private void Act(Action act)
    {
        Unfocus();
        act();
    }

    // ---- Budget --------------------------------------------------------------------------------

    private bool IsTypingIn(Visual panel) =>
        TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is TextBox box
        && box.GetVisualAncestors().Contains(panel);

    private void BuildBudget()
    {
        if (!_opened)
        {
            return;
        }

        // Rebuilding would take the keyboard away mid-word: it waits until the field is left.
        if (IsTypingIn(BudgetSheet))
        {
            if (!_budgetWaiting)
            {
                _budgetWaiting = true;
                BudgetSheet.AddHandler(LostFocusEvent, BudgetFieldLeft);
            }

            return;
        }

        var grid = new Grid { RowDefinitions = RowDefinitions.Parse("Auto,Auto,*") };
        grid.Children.Add(Ui.Handle());
        var save = SaveLineBlock();
        save.Margin = new Thickness(20, 0, 20, 4);
        Grid.SetRow(save, 1);
        grid.Children.Add(save);
        var body = _screen.BudgetCategory is { } row ? BudgetDetail(row) : BudgetOverview();
        body.Margin = new Thickness(16, 4, 16, _safe.Bottom + 48);
        var scroller = new ScrollViewer { Content = body, Padding = new Thickness(0, 0, 0, _keyboard) };
        Grid.SetRow(scroller, 2);
        grid.Children.Add(scroller);
        BudgetSheet.Child = grid;
    }

    private void BudgetFieldLeft(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        Dispatcher.UIThread.Post(() =>
        {
            if (_budgetWaiting && !IsTypingIn(BudgetSheet))
            {
                _budgetWaiting = false;
                BudgetSheet.RemoveHandler(LostFocusEvent, BudgetFieldLeft);
                BuildBudget();
            }
        });

    private StackPanel BudgetOverview()
    {
        var overview = App.Overview;
        // Niet toegewezen and its figure, and below zero the badge beside it, as in the ring's centre.
        var figure = Ui.Text(overview.UnassignedText, "h2", "figure");
        var right = Ui.Stack(2, Ui.Text(Tekst.Unassigned, "muted"), figure);
        if (overview.IsOverAssigned)
        {
            figure.Classes.Add("danger");
            right.Children.Add(Ui.Badge(Tekst.OverAssigned, "Danger", "OnAccent"));
        }

        right.HorizontalAlignment = HorizontalAlignment.Right;
        foreach (var child in right.Children)
        {
            child.HorizontalAlignment = HorizontalAlignment.Right;
        }

        var body = Ui.Stack(0, Ui.Columns("*,Auto", Ui.Stack(0, Ui.Text(Tekst.Budget, "h1"), Ui.Text(App.PeriodTitle, "muted")), right));
        body.Children[0].Margin = new Thickness(4, 0, 4, 0);

        // Opening a period: the button that takes the latest plan over (arc42 §12).
        if (overview.OfferText is { } offer)
        {
            var take = Ui.Pill(offer, () => Act(() => App.TakeOverPlan()));
            take.HorizontalAlignment = HorizontalAlignment.Stretch;
            take.Margin = new Thickness(0, 18, 0, 0);
            body.Children.Add(take);
        }

        body.Children.Add(Ui.Caption(Tekst.Assign));
        body.Children.Add(Ui.Card(AssignCard(fixedCategory: null), 14));

        body.Children.Add(Ui.Caption(Tekst.Categories));
        var add = Ui.Plain(Ui.Row(12, Ui.Icon(Ui.Plus, "Accent", 18), AccentText(Tekst.AddCategory)), AddCategory, new Thickness(16, 15));
        body.Children.Add(Ui.List(overview.Rows.Select(CategoryRow).Append(add)));

        body.Children.Add(Ui.Caption(Tekst.AtPeriodEnd));
        body.Children.Add(PeriodEnd(overview));
        return body;
    }

    /// <summary>
    /// The sweep, under the list: in the current period and later ones <i>Restant naar</i>; in an
    /// ended one its line, with <i>Tekort</i> beside a shortfall and <i>Restant bijwerken</i> when
    /// the line offers it (arc42 §12, <i>The sweep and Restant</i>).
    /// </summary>
    private Control PeriodEnd(PeriodOverview overview)
    {
        var rows = new List<Control>();
        if (overview.ShowsSweepDestination)
        {
            var chosen = overview.ChosenSweepDestination?.Text ?? Tekst.NoSweepDestination;
            rows.Add(Ui.Plain(Ui.Columns("*,Auto,Auto", Ui.Text(Tekst.SweepDestination, "row"), Ui.Text(chosen, "muted"), Ui.Chevron()),
                () => ChooseSweepDestination(overview), new Thickness(16, 14)));
        }

        if (overview.SweepLineText is { } text)
        {
            var line = Ui.Text(text, "row");
            line.TextWrapping = TextWrapping.Wrap;
            line.TextTrimming = TextTrimming.None;
            var content = Ui.Stack(8, line);
            if (overview.IsSweepLineShort)
            {
                content.Children.Add(Ui.Badge(Tekst.PeriodShortfall, "Danger", "OnAccent"));
                content.Children[^1].HorizontalAlignment = HorizontalAlignment.Left;
            }

            if (overview.CanBringSweepUpToDate)
            {
                var bring = Ui.Pill(Tekst.BringUpToDate, () => Act(() => App.BringSweepUpToDate()), "Ghost");
                bring.HorizontalAlignment = HorizontalAlignment.Left;
                content.Children.Add(bring);
            }

            rows.Add(new Border { Padding = new Thickness(16, 14), Child = content });
        }

        return Ui.List(rows);
    }

    private static TextBlock AccentText(string text) => Ui.Text(text, "row").Res(TextBlock.ForegroundProperty, "Accent");

    /// <summary>
    /// Assigning: a category — typed, or picked from the suggestions — or the category of the page it
    /// is on; an amount; and the period it goes into, which starts as the one on screen and can be
    /// moved on its own, as on the desktop.
    /// </summary>
    private Control AssignCard(string? fixedCategory)
    {
        var form = App.AssignForm;
        var body = Ui.Stack(8);

        if (fixedCategory is null)
        {
            var category = Ui.Linked(Ui.Field(Tekst.ChooseOrTypeCategory), form, nameof(AssignForm.Category), () => form.Category, v => form.Category = v);
            var suggestions = new ContentControl();
            void Suggest() => suggestions.Content = SuggestionChips(form.Category, name => form.Category = name);
            category.TextChanged += (_, _) => Suggest();
            Suggest();
            body.Children.Add(category);
            body.Children.Add(suggestions);
        }

        var amount = Ui.Linked(Ui.Field("0,00"), form, nameof(AssignForm.Amount), () => form.Amount, v => form.Amount = v);
        TextInputOptions.SetContentType(amount, TextInputContentType.Number);
        var go = Ui.Pill(Tekst.Assign, () => Act(() =>
        {
            if (fixedCategory is not null)
            {
                form.Category = fixedCategory;
            }

            form.SubmitCommand.Execute(null);
        }));
        go.Margin = new Thickness(10, 0, 0, 0);
        body.Children.Add(Ui.Columns("*,Auto", amount, go));

        var earlier = Ui.IconButton(Ui.Icon(Ui.ChevronLeft, "Muted", 18), () => form.EarlierPeriodCommand.Execute(null));
        var later = Ui.IconButton(Ui.Icon(Ui.ChevronRight, "Muted", 18), () => form.LaterPeriodCommand.Execute(null));
        var period = Ui.Text(form.PeriodTitle, "muted");
        period.HorizontalAlignment = HorizontalAlignment.Center;
        Ui.WhileShown(period, form, (_, e) =>
        {
            if (e.PropertyName is null or nameof(AssignForm.Period) or nameof(AssignForm.PeriodTitle))
            {
                period.Text = form.PeriodTitle;
            }
        });
        body.Children.Add(Ui.Columns("Auto,*,Auto", earlier, period, later));
        return body;
    }

    private void AddCategory()
    {
        var form = App.CategoryForm;
        var name = Ui.Linked(Ui.Field(Tekst.ExampleCategory), form, nameof(CategoryForm.Name), () => form.Name, v => form.Name = v);
        var add = Ui.Pill(Tekst.AddCategory, () =>
        {
            form.SubmitCommand.Execute(null);
            if (form.Name is null)
            {
                CloseModal();
            }
        });
        add.HorizontalAlignment = HorizontalAlignment.Stretch;
        ShowModal(Card(Tekst.NewCategory, Ui.Stack(14, name, add)), fromBottom: false, onClosed: () => form.Name = null);
        name.Focus();
    }

    private Control CategoryRow(CategoryRow row)
    {
        var name = Ui.Stack(2, Ui.Row(10, Dot(row.SliceIndex), Ui.Text(row.Name, "row")));
        name.VerticalAlignment = VerticalAlignment.Center;
        var under = new List<string>();
        if (row.IsArchived)
        {
            under.Add(Tekst.Archived);
        }

        if (row.BackingAccount is { } account)
        {
            under.Add(Tekst.BackedBy(account.Name));
        }

        if (under.Count > 0)
        {
            var caption = Ui.Text(string.Join(" · ", under), "faint");
            caption.Margin = new Thickness(20, 0, 0, 0);
            name.Children.Add(caption);
        }

        var figure = Ui.Text(row.RemainingText, "row");
        var right = Ui.Row(0, figure);
        if (row.IsOverBudget)
        {
            figure.Classes.Add("danger");
            right.Children.Add(Ui.Marker());
        }

        right.VerticalAlignment = VerticalAlignment.Center;
        var more = Ui.IconButton(Ui.Dots(), () => CategoryMenu(row));
        var fraction = row.Budget.Cents > 0 ? (double)row.Spent.Cents / row.Budget.Cents : row.Spent.Cents > 0 ? 1 : 0;
        var content = Ui.Stack(0,
            Ui.Columns("*,Auto,Auto", name, right, more),
            Ui.Bar(fraction, row.SliceIndex, row.IsOverBudget),
            Ui.Text(Tekst.SpentOf(row.Spent, row.Budget), "faint"));

        // While a plan is offered, the figure the row would take over, in grey (§12).
        if (row.PlanText is { } plan)
        {
            content.Children.Add(Ui.Text(plan, "faint"));
        }

        if (row.AccumulatedText is { } accumulated)
        {
            var line = Ui.Row(6, Ui.Text(accumulated, "faint"));
            if (row.IsAccumulatedBelowZero)
            {
                line.Children.Add(Ui.Badge(Tekst.Overdrawn, "Danger", "OnAccent"));
            }

            content.Children.Add(line);
        }

        return Ui.Plain(content, () => _screen.OpenCategory(row.Name), new Thickness(16, 10, 4, 12));
    }

    /// <summary>A category's own page: everything its row shows, its backing, assigning to it, and its expenses.</summary>
    private StackPanel BudgetDetail(CategoryRow row)
    {
        var back = Ui.IconButton(Ui.Icon(Ui.ChevronLeft), _screen.ShowAllCategories);
        var title = Ui.Row(10, Dot(row.SliceIndex, 12), Ui.Text(row.Name, "h1"));
        title.VerticalAlignment = VerticalAlignment.Center;
        var header = Ui.Columns("Auto,*,Auto", back, title, Ui.IconButton(Ui.Dots(), () => CategoryMenu(row)));
        header.Margin = new Thickness(-8, 0, 0, 0);
        var body = Ui.Stack(0, header);

        if (row.IsArchived)
        {
            var archived = Ui.Text(Tekst.Archived, "muted");
            archived.Margin = new Thickness(4, 4, 0, 0);
            body.Children.Add(archived);
        }

        var figure = Ui.Text(row.RemainingText, "big");
        var amount = Ui.Row(8, figure);
        if (row.IsOverBudget)
        {
            figure.Classes.Add("danger");
            amount.Children.Add(Ui.Badge(Tekst.OverBudget, "Danger", "OnAccent"));
        }

        var big = Ui.Stack(0, Ui.Text(Tekst.Remaining, "muted"), amount);
        big.Margin = new Thickness(4, 14, 0, 0);
        body.Children.Add(big);

        var budget = Ui.Stack(0, Ui.Text(row.BudgetText, "row"));
        if (row.PlanText is { } plan)
        {
            budget.Children.Add(Ui.Text(plan, "faint"));
        }

        foreach (var child in budget.Children)
        {
            child.HorizontalAlignment = HorizontalAlignment.Right;
        }

        var stats = new List<Control>
        {
            Stat(Ui.Text(Tekst.Budget, "row"), budget),
            Stat(Ui.Text(Tekst.Spent, "row"), Ui.Text(row.SpentText, "row")),
            Ui.Plain(Ui.Columns("*,Auto,Auto", Ui.Text(Tekst.BackingAccount, "row"), Ui.Text(row.ChosenBacking?.Text ?? Tekst.NoBacking, "muted"), Ui.Chevron()),
                () => ChooseBacking(row), new Thickness(16, 14)),
        };
        if (row.Accumulated is { } accumulated)
        {
            var value = Ui.Row(6, Ui.Text(Tekst.Euro(accumulated), "row"));
            if (row.AccumulatedOn is { } on)
            {
                value.Children.Add(Ui.Text(Tekst.OnAccount(on), "muted"));
            }

            if (row.IsAccumulatedBelowZero)
            {
                value.Children.Add(Ui.Badge(Tekst.Overdrawn, "Danger", "OnAccent"));
            }

            stats.Add(Stat(Ui.Text(Tekst.Accumulated, "row"), value));
        }

        body.Children.Add(Ui.Caption(Tekst.ThisPeriod));
        body.Children.Add(Ui.List(stats));
        body.Children.Add(Ui.Caption(Tekst.AssignTo(row.Name)));
        body.Children.Add(Ui.Card(AssignCard(row.Name), 14));

        var expenses = App.Overview.ExpensesOn(row.Name);
        if (expenses.Count > 0)
        {
            body.Children.Add(Ui.Caption(Tekst.Expenses));
            body.Children.Add(Ui.List(expenses.Select(e => (Control)Ui.Plain(
                Ui.Columns("*,Auto", Ui.Stack(2, Ui.Text(e.Label ?? e.Category, "row"), Ui.Text(e.DateText, "muted")), Ui.Text(e.AmountText, "row")),
                () => _screen.OpenEntry(e), new Thickness(16, 12)))));
        }

        return body;

        static Control Stat(Control label, Control value)
        {
            value.VerticalAlignment = VerticalAlignment.Center;
            return new Border { Padding = new Thickness(16, 14), Child = Ui.Columns("*,Auto", label, value) };
        }
    }

    // ---- Accounts ------------------------------------------------------------------------------

    private void BuildAccounts()
    {
        var gear = Ui.IconButton(Ui.FilledIcon(Ui.Gear, "Muted", 22), ShowSettings);
        gear.Name = "Gear";
        var figure = Ui.Text(App.NetWorthText, "big");
        var net = Ui.Row(8, figure);
        if (App.IsNetWorthNegative)
        {
            net.Children.Add(Ui.Badge(Tekst.Overdrawn, "Danger", "OnAccent"));
        }

        var worth = Ui.Stack(0, Ui.Text(Tekst.NetWorth, "muted"), net);
        worth.Margin = new Thickness(4, 0, 0, 0);

        var rows = App.Accounts.Select(line =>
        {
            var name = Ui.Row(8, Ui.Text(line.Name, "row"));
            if (line.IsPool)
            {
                name.Children.Add(Ui.Icon(Ui.House, "Accent", 17));
            }

            // Vrij, on every account but the Hoofdrekening, under its name (§12, ruling 1).
            var left = Ui.Stack(2, name);
            if (line.UnclaimedText is { } unclaimed)
            {
                var vrij = Ui.Row(6, Ui.Text(unclaimed, "faint"));
                if (line.IsUnclaimedBelowZero)
                {
                    vrij.Children.Add(Ui.Badge(Tekst.Overdrawn, "Danger", "OnAccent"));
                }

                left.Children.Add(vrij);
            }

            var right = Ui.Row(6, Ui.Text(line.BalanceText, "row"));
            if (line.IsOverdrawn)
            {
                right.Children.Add(Ui.Badge(Tekst.Overdrawn, "Danger", "OnAccent"));
            }

            right.Children.Add(Ui.Chevron());
            right.VerticalAlignment = VerticalAlignment.Center;
            return (Control)Ui.Plain(Ui.Columns("*,Auto", left, right), () => OpenHistory(line.Account), new Thickness(16, 15));
        });

        var actions = Ui.Columns("*,12,*",
            Ui.Pill(Tekst.TransferAct, OpenTransfer, "Ghost"),
            new Border(),
            Ui.Pill(Tekst.AddAccount, AddAccount, "Ghost"));
        actions.Margin = new Thickness(0, 16, 0, 0);
        var reallocate = Ui.Pill(Tekst.Reallocate, () => OpenReallocate(null), "Ghost");
        reallocate.Margin = new Thickness(0, 12, 0, 0);
        reallocate.IsVisible = App.ReallocateForm.HasEnds;
        foreach (var child in actions.Children.Append(reallocate))
        {
            child.HorizontalAlignment = HorizontalAlignment.Stretch;
            if (child is Button button)
            {
                button.Padding = new Thickness(10, 11);
            }
        }

        var body = Ui.Stack(0, worth, Ui.Caption(Tekst.Accounts), Ui.List(rows), actions, reallocate);
        AccountsList.Child = Page(Header(Tekst.Accounts, null, gear), body, footer: Ui.Handle());
    }

    private void OpenHistory(Account account)
    {
        _screen.OpenHistory(account);
        BuildHistory();
    }

    private void BuildHistory()
    {
        if (App.HistoryAccount is not { } account)
        {
            return;
        }

        var back = Ui.IconButton(Ui.Icon(Ui.ChevronLeft), () => MoveTo(PhonePanel.Accounts, PanelStep.First));
        var more = Ui.IconButton(Ui.Dots(), AccountMenu);
        var title = Ui.Text(account.Name, "h1");
        var header = Ui.Columns("Auto,*,Auto", back, title, more);

        var line = App.Accounts.First(a => a.Account == account);
        var balance = Ui.Row(8, Ui.Text(line.BalanceText, "big"));
        if (line.IsOverdrawn)
        {
            balance.Children.Add(Ui.Badge(Tekst.Overdrawn, "Danger", "OnAccent"));
        }

        var big = Ui.Stack(0, Ui.Text(Tekst.BalanceToday, "muted"), balance);
        if (line.UnclaimedText is { } unclaimed)
        {
            var vrij = Ui.Row(6, Ui.Text(unclaimed, "muted"));
            if (line.IsUnclaimedBelowZero)
            {
                vrij.Children.Add(Ui.Badge(Tekst.Overdrawn, "Danger", "OnAccent"));
            }

            big.Children.Add(vrij);
        }

        big.Margin = new Thickness(4, 0, 0, 0);
        var body = Ui.Stack(0, big);

        var history = App.History;
        foreach (var day in history.GroupBy(h => h.Date))
        {
            body.Children.Add(Ui.Caption(Tekst.DayHeading(day.Key, App.Ledger.Today)));
            body.Children.Add(Ui.List(day.Select(HistoryRow)));
        }

        if (history.Count == 0)
        {
            var nothing = Ui.Text(Tekst.NoHistory, "muted");
            nothing.Margin = new Thickness(4, 24, 0, 0);
            body.Children.Add(nothing);
        }

        var page = Page(header, body, footer: Ui.Handle());
        header.Margin = new Thickness(8, _safe.Top + 10, 8, 8);
        AccountHistory.Child = page;
    }

    /// <summary>
    /// A row of an account's history. A transfer opens in the transfer form, to change or remove; a
    /// starting balance or a correction asks to be removed; an income, expense or movement is only
    /// listed there, as on the desktop (arc42 §12).
    /// </summary>
    private Control HistoryRow(HistoryLine line)
    {
        var text = Ui.Text(line.Text, "row");
        text.TextWrapping = TextWrapping.Wrap;
        text.TextTrimming = TextTrimming.None;
        var content = Ui.Columns("*,Auto", text, Ui.Text(line.AmountText ?? "", "row"));
        content.Children[1].Margin = new Thickness(10, 0, 0, 0);

        if (line.CanChange)
        {
            return Ui.Plain(content, () =>
            {
                App.EditTransfer(line);
                ShowTransfer();
            }, new Thickness(16, 13));
        }

        if (line.CanRemove)
        {
            return Ui.Plain(content, () => App.RemoveFromHistory(line), new Thickness(16, 13));
        }

        return new Border { Padding = new Thickness(16, 13), Child = content };
    }
}
