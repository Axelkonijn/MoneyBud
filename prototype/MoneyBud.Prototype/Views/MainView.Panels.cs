using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.TextInput;
using Avalonia.Layout;
using Avalonia.Media;
using MoneyBud.Prototype.Motion;
using MoneyBud.Prototype.Sample;

namespace MoneyBud.Prototype.Views;

/// <summary>What is on each panel: the lists, the forms, the budget and the accounts.</summary>
public sealed partial class MainView
{
    private string? _historyAccount;
    private string? _sheetCategory;

    private void RebuildAll()
    {
        if (!_opened)
        {
            return;
        }

        BuildEntryList(EntryKind.Income);
        BuildEntryList(EntryKind.Expense);
        BuildBudget();
        BuildAccounts();
        if (_historyAccount is not null)
        {
            BuildHistory();
        }
    }

    /// <summary>A panel's page: a heading that stays, and everything under it scrolling.</summary>
    private Grid Page(Control header, Control body, bool topInset = true, Control? footer = null)
    {
        var grid = new Grid { RowDefinitions = RowDefinitions.Parse("Auto,*,Auto") };
        header.Margin = new Thickness(20, (topInset ? _safe.Top : 0) + 14, 10, 8);
        grid.Children.Add(header);

        body.Margin = new Thickness(16, 0, 16, (footer is null ? _safe.Bottom : 0) + 36);
        var scroller = new ScrollViewer { Content = body };
        Grid.SetRow(scroller, 1);
        grid.Children.Add(scroller);

        if (footer is not null)
        {
            Grid.SetRow(footer, 2);
            grid.Children.Add(footer);
        }

        return grid;
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

    // ---- Income and expenses ------------------------------------------------------------------

    private void BuildEntryList(EntryKind kind)
    {
        var income = kind == EntryKind.Income;
        var entries = (income ? _ledger.Incomes(_offset) : _ledger.Expenses(_offset)).ToList();
        var body = new StackPanel();

        var total = Ui.Stack(0,
            Ui.Text(Dutch.Euro(entries.Sum(e => e.Amount)), "big"),
            Ui.Text(income ? "binnengekomen of verwacht" : "uitgegeven", "muted"));
        total.Margin = new Thickness(4, 4, 0, 4);
        body.Children.Add(total);

        foreach (var day in entries.GroupBy(e => e.Date))
        {
            body.Children.Add(Ui.Caption(Dutch.DayHeading(day.Key, _ledger.Today)));
            body.Children.Add(Ui.List(day.Select(EntryRow)));
        }

        if (entries.Count == 0)
        {
            var nothing = Ui.Text("Nog niets in deze periode.", "muted");
            nothing.Margin = new Thickness(4, 24, 0, 0);
            body.Children.Add(nothing);
        }

        var hint = Ui.Text(income ? "Veeg nog eens naar rechts voor een nieuwe inkomst  ›" : "‹  Veeg nog eens naar links voor een nieuwe uitgave", "faint");
        hint.HorizontalAlignment = HorizontalAlignment.Center;
        hint.Margin = new Thickness(0, 28, 0, 0);
        body.Children.Add(hint);

        (income ? IncomeList : ExpenseList).Child =
            Page(Header(income ? "Inkomsten" : "Uitgaven", Dutch.Month(_ledger.FirstDay(_offset))), body);
    }

    private Control EntryRow(SampleEntry entry)
    {
        var expense = entry.Kind == EntryKind.Expense;
        var category = expense ? _ledger.Find(entry.Category ?? "") : null;
        Control dot = category is not null ? Ui.Dot(category.Colour) : Ui.ColouredDot("Positive");
        dot.Margin = new Thickness(0, 0, 14, 0);

        var primary = string.IsNullOrEmpty(entry.Label) ? entry.Category ?? "" : entry.Label;
        var secondary = expense
            ? entry.Account == _ledger.PoolAccount ? entry.Category ?? "" : $"{entry.Category} · {entry.Account}"
            : entry.Account;

        var amount = Ui.Text(Dutch.Euro(entry.Amount), "row");
        amount.HorizontalAlignment = HorizontalAlignment.Right;
        var right = Ui.Stack(2, amount);
        if (entry.Repeat is { } repeat)
        {
            var repeats = Ui.Text(repeat, "faint");
            repeats.HorizontalAlignment = HorizontalAlignment.Right;
            right.Children.Add(repeats);
        }

        var texts = Ui.Stack(2, Ui.Text(primary, "row"), Ui.Text(secondary, "muted"));
        return Ui.Plain(Ui.Columns("Auto,*,Auto", dot, texts, right), () => EditEntry(entry), new Thickness(16, 13));
    }

    private void EditEntry(SampleEntry entry)
    {
        var side = entry.Kind == EntryKind.Income ? Side.Income : Side.Expense;
        BuildEntryForm(entry.Kind, entry);
        MoveTo(side, 2 * W);
    }

    /// <summary>A second swipe on a list shows an empty form for a new entry.</summary>
    private void PrepareSecondStep(Side side)
    {
        if (side is Side.Income or Side.Expense && _dragBase <= W + 1)
        {
            BuildEntryForm(side == Side.Income ? EntryKind.Income : EntryKind.Expense, null);
        }
    }

    private void BuildEntryForm(EntryKind kind, SampleEntry? entry)
    {
        var income = kind == EntryKind.Income;
        var side = income ? Side.Income : Side.Expense;
        var date = entry?.Date ?? _ledger.Today;
        var account = entry?.Account ?? _ledger.PoolAccount;
        var repeat = entry?.Repeat ?? "Eenmalig";

        var label = Ui.Field(income ? "Bijvoorbeeld Salaris" : "Bijvoorbeeld Albert Heijn", entry?.Label ?? "");
        var category = Ui.Field("Kies of typ een categorie", entry?.Category ?? "");
        var suggestions = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
        category.TextChanged += (_, _) => Suggest();
        Suggest();

        var amount = Ui.Field("0,00", entry is null ? "" : Dutch.Typed(entry.Amount));
        amount.Classes.Add("amount");
        var euro = Ui.Text("€", "h2");
        euro.Margin = new Thickness(16, 0, 0, 0);
        euro.Res(TextBlock.ForegroundProperty, "Muted");
        amount.InnerLeftContent = euro;
        TextInputOptions.SetContentType(amount, TextInputContentType.Number);

        var body = Ui.Stack(0, Ui.Caption("Omschrijving"), label);
        WrapPanel dates = null!;
        dates = DateChips();
        if (!income)
        {
            body.Children.Add(Ui.Caption("Categorie"));
            body.Children.Add(category);
            body.Children.Add(suggestions);
        }

        body.Children.Add(Ui.Caption("Bedrag"));
        body.Children.Add(amount);
        body.Children.Add(Ui.Caption("Datum"));
        body.Children.Add(dates);
        body.Children.Add(Ui.Caption("Rekening"));
        body.Children.Add(Ui.Chips(_ledger.Accounts.Select(a => a.Name), account, picked => account = picked));
        body.Children.Add(Ui.Caption("Herhalen"));
        body.Children.Add(Ui.Chips(["Eenmalig", "wekelijks", "maandelijks"], repeat, picked => repeat = picked));

        if (entry is not null)
        {
            var remove = Ui.Pill("Verwijderen", () => Ask(
                $"{(income ? "Inkomst" : "Uitgave")} \"{Shown(entry)}\" verwijderen?", "Verwijderen", () =>
                {
                    _ledger.Entries.Remove(entry);
                    MoveTo(side, W);
                    BuildEntryList(kind);
                    RefreshRing();
                    Say($"\"{Shown(entry)}\" is verwijderd.");
                }), "DangerGhost");
            remove.HorizontalAlignment = HorizontalAlignment.Center;
            remove.Margin = new Thickness(0, 28, 0, 0);
            body.Children.Add(remove);
        }

        var title = entry is null ? income ? "Nieuwe inkomst" : "Nieuwe uitgave" : income ? "Inkomst wijzigen" : "Uitgave wijzigen";
        var save = Ui.Round(entry is null ? Ui.Plus : Ui.Tick, Save);
        (income ? IncomeForm : ExpenseForm).Child = Page(Header(title, null, save), body);

        void Suggest()
        {
            suggestions.Children.Clear();
            var typed = category.Text?.Trim() ?? "";
            foreach (var c in _ledger.Categories.Where(c => c.Name.Contains(typed, StringComparison.OrdinalIgnoreCase)))
            {
                if (string.Equals(c.Name, typed, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                suggestions.Children.Add(Ui.Suggestion(c.Name, c.Colour, () =>
                {
                    category.Text = c.Name;
                    category.CaretIndex = c.Name.Length;
                }));
            }
        }

        WrapPanel DateChips()
        {
            var today = _ledger.Today;
            var other = date != today && date != today.AddDays(-1) ? Dutch.Day(date) : "Andere datum…";
            var chosen = date == today ? "Vandaag" : date == today.AddDays(-1) ? "Gisteren" : other;
            return Ui.Chips(["Vandaag", "Gisteren", other], chosen, picked =>
            {
                if (picked == "Vandaag")
                {
                    date = today;
                }
                else if (picked == "Gisteren")
                {
                    date = today.AddDays(-1);
                }
                else
                {
                    PickDate(date, allowFuture: income, picked =>
                    {
                        date = picked;
                        var index = body.Children.IndexOf(dates);
                        dates = DateChips();
                        body.Children[index] = dates;
                    });
                }
            });
        }

        void Save()
        {
            if (!Dutch.TryReadAmount(amount.Text, out var value) || value <= 0)
            {
                Say("Vul een bedrag in, bijvoorbeeld 12,50.");
                return;
            }

            var chosen = income ? null : _ledger.Find(category.Text ?? "");
            if (!income && chosen is null)
            {
                Say(string.IsNullOrWhiteSpace(category.Text) ? "Kies een categorie." : $"Er is geen categorie \"{category.Text!.Trim()}\".");
                return;
            }

            if (!income && date > _ledger.Today)
            {
                Say("Een uitgave kan niet in de toekomst liggen.");
                return;
            }

            if (income && string.IsNullOrWhiteSpace(label.Text))
            {
                Say("Een inkomst heeft een omschrijving nodig.");
                return;
            }

            var shownRepeat = repeat == "Eenmalig" ? null : repeat;
            if (entry is null)
            {
                _ledger.Add(kind, label.Text ?? "", chosen?.Name, value, date, account, shownRepeat);
                Unfocus();
                MoveTo(side, 0);
                BuildEntryList(kind);

                // Once the panel is out of the way, the slice grows where it can be seen.
                Tween.Run(this, 0.28, t => t, _ => { }, done: () =>
                {
                    RefreshRing();
                    Say(income
                        ? $"{Dutch.Euro(value)} inkomen toegevoegd."
                        : $"{Dutch.Euro(value)} uitgegeven aan {chosen!.Name}.");
                });
                return;
            }

            entry.Label = label.Text?.Trim() ?? "";
            entry.Category = chosen?.Name;
            entry.Amount = value;
            entry.Date = date;
            entry.Account = account;
            entry.Repeat = shownRepeat;
            Unfocus();
            MoveTo(side, W);
            BuildEntryList(kind);
            RefreshRing();
            Say($"\"{Shown(entry)}\" is gewijzigd.");
        }

    }

    private static string Shown(SampleEntry entry) =>
        string.IsNullOrEmpty(entry.Label) ? entry.Category ?? "" : entry.Label;

    // ---- Budget --------------------------------------------------------------------------------

    /// <summary>The category the budget sheet is about: the slice pointed at, or a row tapped.</summary>
    private SampleCategory? FocusedCategory =>
        Ring.SelectedName is { } name && name != UnassignedName ? _ledger.Find(name)
        : _sheetCategory is { } tapped ? _ledger.Find(tapped)
        : null;

    private void BuildBudget()
    {
        if (!_opened)
        {
            return;
        }

        var grid = new Grid { RowDefinitions = RowDefinitions.Parse("Auto,*") };
        grid.Children.Add(Ui.Handle());
        var body = FocusedCategory is { } category ? BudgetDetail(category) : BudgetOverview();
        body.Margin = new Thickness(16, 4, 16, _safe.Bottom + 48);
        var scroller = new ScrollViewer { Content = body };
        Grid.SetRow(scroller, 1);
        grid.Children.Add(scroller);
        BudgetSheet.Child = grid;
    }

    private void ShowAllCategories()
    {
        _sheetCategory = null;
        Ring.Selected = -1;
        BuildBudget();
    }

    private StackPanel BudgetOverview()
    {
        var unassigned = _ledger.Unassigned(_offset);
        var figure = Ui.Text(Dutch.Euro(unassigned), "h2");
        if (unassigned < 0)
        {
            figure.Classes.Add("danger");
        }

        var right = Ui.Stack(0, Ui.Text(unassigned < 0 ? "Te veel toegewezen" : "Niet toegewezen", "muted"), figure);
        right.HorizontalAlignment = HorizontalAlignment.Right;
        foreach (var child in right.Children)
        {
            child.HorizontalAlignment = HorizontalAlignment.Right;
        }

        var body = Ui.Stack(0, Ui.Columns("*,Auto", Ui.Stack(0, Ui.Text("Budget", "h1"), Ui.Text(Dutch.Month(_ledger.FirstDay(_offset)), "muted")), right));
        body.Children[0].Margin = new Thickness(4, 0, 4, 0);

        if (_ledger.OfferFor(_offset) is { } offer)
        {
            var take = Ui.Pill("Overnemen", () =>
            {
                _ledger.TakeOverPlan(_offset);
                BuildBudget();
                RefreshRing();
                Say($"Plan van {Dutch.Month(_ledger.FirstDay(offer.FromOffset))} overgenomen in {Dutch.Month(_ledger.FirstDay(_offset))}: {Dutch.Euro(offer.Total)} toegewezen.");
            });
            var card = Ui.Card(Ui.Columns("*,Auto",
                Ui.Stack(2, Ui.Text($"Plan van {Dutch.Month(_ledger.FirstDay(offer.FromOffset))}", "row"), Ui.Text($"{Dutch.Euro(offer.Total)} voor {offer.Figures.Count} categorieën", "muted")),
                take), 16);
            card.Margin = new Thickness(0, 18, 0, 0);
            body.Children.Add(card);
        }

        var target = _ledger.Categories.FirstOrDefault()?.Name;
        var chips = Ui.Chips(_ledger.Categories.Select(c => c.Name), target, picked => target = picked);
        body.Children.Add(Ui.Caption("Toewijzen"));
        body.Children.Add(Ui.Card(Ui.Stack(6, chips, AssignRow(() => target is null ? null : _ledger.Find(target))), 14));

        body.Children.Add(Ui.Caption("Categorieën"));
        var add = Ui.Plain(Ui.Row(12, Ui.Icon(Ui.Plus, "Accent", 18), AccentText("Categorie toevoegen")), AddCategory, new Thickness(16, 15));
        body.Children.Add(Ui.List(_ledger.Categories
            .OrderByDescending(c => _ledger.BudgetOf(c, _offset))
            .Select(CategoryRow)
            .Append(add)));

        var destination = Ui.Columns("*,Auto,Auto",
            Ui.Text("Restant naar", "row"),
            Ui.Text(_ledger.SweepDestination ?? "Nergens", "muted"),
            Ui.Chevron());
        body.Children.Add(Ui.Caption("Aan het eind van de periode"));
        body.Children.Add(Ui.List([Ui.Plain(destination, ChooseSweepDestination, new Thickness(16, 14))]));
        return body;
    }

    private static TextBlock AccentText(string text) => Ui.Text(text, "row").Res(TextBlock.ForegroundProperty, "Accent");

    private void AddCategory()
    {
        var name = Ui.Field("Bijvoorbeeld Uit eten");
        var add = Ui.Pill("Toevoegen", () =>
        {
            var typed = name.Text?.Trim() ?? "";
            if (typed.Length == 0)
            {
                Say("Een categorie heeft een naam nodig.");
                return;
            }

            if (_ledger.Find(typed) is { } existing)
            {
                CloseModal();
                Say($"\"{existing.Name}\" is er al.");
                return;
            }

            _ledger.Categories.Add(new SampleCategory(typed, _ledger.Categories.Count));
            CloseModal();
            BuildBudget();
            Say($"Categorie \"{typed}\" toegevoegd.");
        });
        add.HorizontalAlignment = HorizontalAlignment.Stretch;
        ShowModal(Card("Nieuwe categorie", Ui.Stack(14, name, add)), fromBottom: false);
        name.Focus();
    }

    private Control CategoryRow(SampleCategory category)
    {
        var budget = _ledger.BudgetOf(category, _offset);
        var spent = _ledger.SpentOn(category, _offset);
        var remaining = budget - spent;

        var name = Ui.Stack(2, Ui.Row(10, Ui.Dot(category.Colour), Ui.Text(category.Name, "row")));
        name.VerticalAlignment = VerticalAlignment.Center;
        if (category.BackedBy is { } account)
        {
            var backed = Ui.Text($"op {account}", "faint");
            backed.Margin = new Thickness(20, 0, 0, 0);
            name.Children.Add(backed);
        }

        var figure = Ui.Text(Dutch.Euro(remaining), "row");
        var right = Ui.Row(0, figure);
        if (remaining < 0)
        {
            figure.Classes.Add("danger");
            right.Children.Add(Ui.Marker());
        }

        right.VerticalAlignment = VerticalAlignment.Center;
        var more = Ui.IconButton(Ui.Dots(), () => CategoryMenu(category));
        var fraction = budget > 0 ? (double)(spent / budget) : spent > 0 ? 1 : 0;
        var content = Ui.Stack(0,
            Ui.Columns("*,Auto,Auto", name, right, more),
            Ui.Bar(fraction, category.Colour, remaining < 0),
            Ui.Text($"{Dutch.Euro(spent)} van {Dutch.Euro(budget)}", "faint"));
        if (budget == 0 && spent == 0)
        {
            content.Opacity = 0.55;
        }

        return Ui.Plain(content, () =>
        {
            _sheetCategory = category.Name;
            Ring.Selected = Ring.IndexOf(category.Name);
            BuildBudget();
            if (_budget.Target < SheetHalf)
            {
                MoveTo(Side.Budget, SheetHalf);
            }
        }, new Thickness(16, 10, 4, 12));
    }

    private StackPanel BudgetDetail(SampleCategory category)
    {
        var budget = _ledger.BudgetOf(category, _offset);
        var spent = _ledger.SpentOn(category, _offset);
        var remaining = budget - spent;

        var back = Ui.IconButton(Ui.Icon(Ui.ChevronLeft), ShowAllCategories);
        var title = Ui.Row(10, Ui.Dot(category.Colour, 12), Ui.Text(category.Name, "h1"));
        title.VerticalAlignment = VerticalAlignment.Center;
        var header = Ui.Columns("Auto,*,Auto", back, title, Ui.IconButton(Ui.Dots(), () => CategoryMenu(category)));
        header.Margin = new Thickness(-8, 0, 0, 0);

        var figure = Ui.Text(Dutch.Euro(remaining), "big");
        var amount = Ui.Row(0, figure);
        if (remaining < 0)
        {
            figure.Classes.Add("danger");
            amount.Children.Add(Ui.Marker());
        }

        var big = Ui.Stack(0, Ui.Text("Resterend", "muted"), amount);
        big.Margin = new Thickness(4, 14, 0, 0);

        var stats = new List<Control>
        {
            Stat("Budget", Dutch.Euro(budget)),
            Stat("Uitgegeven", Dutch.Euro(spent)),
            Ui.Plain(Ui.Columns("*,Auto,Auto", Ui.Text("Staat op", "row"), Ui.Text(category.BackedBy ?? "Nergens", "muted"), Ui.Chevron()),
                () => ChooseBacking(category), new Thickness(16, 14)),
        };
        if (category.BackedBy is not null)
        {
            stats.Add(Stat("Opgebouwd", Dutch.Euro(_ledger.AccumulatedOf(category, _offset))));
        }

        var body = Ui.Stack(0, header, big, Ui.Caption("Deze periode"), Ui.List(stats));
        body.Children.Add(Ui.Caption($"Toewijzen aan {category.Name}"));
        body.Children.Add(Ui.Card(AssignRow(() => category), 14));

        var expenses = _ledger.Expenses(_offset).Where(e => e.Category == category.Name).ToList();
        if (expenses.Count > 0)
        {
            body.Children.Add(Ui.Caption("Uitgaven"));
            body.Children.Add(Ui.List(expenses.Select(e => Ui.Plain(
                Ui.Columns("*,Auto", Ui.Stack(2, Ui.Text(Shown(e), "row"), Ui.Text(Dutch.Day(e.Date), "muted")), Ui.Text(Dutch.Euro(e.Amount), "row")),
                () =>
                {
                    // From the budget straight to that expense's form.
                    MoveTo(Side.Budget, 0);
                    Opening(Side.Expense);
                    BuildEntryForm(EntryKind.Expense, e);
                    MoveTo(Side.Expense, 2 * W);
                }, new Thickness(16, 12)))));
        }

        return body;

        static Control Stat(string label, string value) =>
            new Border { Padding = new Thickness(16, 14), Child = Ui.Columns("*,Auto", Ui.Text(label, "row"), Ui.Text(value, "row")) };
    }

    /// <summary>An amount and a button: assigning moves it out of Niet toegewezen onto the Budget.</summary>
    private Control AssignRow(Func<SampleCategory?> target)
    {
        var amount = Ui.Field("0,00");
        TextInputOptions.SetContentType(amount, TextInputContentType.Number);
        var go = Ui.Pill("Toewijzen", () =>
        {
            if (target() is not { } category)
            {
                Say("Kies een categorie.");
                return;
            }

            if (!Dutch.TryReadAmount(amount.Text, out var value))
            {
                Say("Vul een bedrag in, bijvoorbeeld 50 of -50.");
                return;
            }

            if (_offset < 0)
            {
                Say("Een voorbije periode krijgt geen budget meer.");
                return;
            }

            _ledger.Assign(category, _offset, value);
            Unfocus();
            BuildBudget();
            RefreshRing();
            Say(value >= 0
                ? $"{Dutch.Euro(value)} toegewezen aan {category.Name}."
                : $"{Dutch.Euro(-value)} van {category.Name} terug naar Niet toegewezen.");
        });
        go.Margin = new Thickness(10, 0, 0, 0);
        return Ui.Columns("*,Auto", amount, go);
    }

    // ---- Accounts ------------------------------------------------------------------------------

    private void BuildAccounts()
    {
        var gear = Ui.IconButton(Ui.FilledIcon(Ui.Gear, "Muted", 22), ShowSettings);
        var net = _ledger.NetWorth;
        var figure = Ui.Text(Dutch.Euro(net), "big");
        var worth = Ui.Stack(0, Ui.Text("Vermogen", "muted"), Ui.Row(0, figure));
        worth.Margin = new Thickness(4, 0, 0, 0);

        var rows = _ledger.Accounts.Select(account =>
        {
            var balance = _ledger.BalanceOf(account.Name);
            var name = Ui.Row(8, Ui.Text(account.Name, "row"));
            if (account.Name == _ledger.PoolAccount)
            {
                name.Children.Add(Ui.Icon(Ui.House, "Accent", 17));
            }

            var right = Ui.Row(6, Ui.Text(Dutch.Euro(balance), "row"));
            if (balance < 0)
            {
                right.Children.Add(Ui.Badge("Rood", "Danger", "OnAccent"));
            }

            right.Children.Add(Ui.Chevron());
            return (Control)Ui.Plain(Ui.Columns("*,Auto", name, right), () => OpenHistory(account.Name), new Thickness(16, 15));
        });

        var actions = Ui.Columns("*,12,*",
            Ui.Pill("Overboeken", () => Say("Overboeken zit nog niet in het prototype."), "Ghost"),
            new Border(),
            Ui.Pill("Rekening toevoegen", () => Say("Een rekening toevoegen zit nog niet in het prototype."), "Ghost"));
        actions.Margin = new Thickness(0, 16, 0, 0);
        foreach (var child in actions.Children)
        {
            child.HorizontalAlignment = HorizontalAlignment.Stretch;
            if (child is Button button)
            {
                button.Padding = new Thickness(10, 11);
            }
        }

        var body = Ui.Stack(0, worth, Ui.Caption("Rekeningen"), Ui.List(rows), actions);
        AccountsList.Child = Page(Header("Rekeningen", null, gear), body, footer: Ui.Handle());
    }

    private void OpenHistory(string account)
    {
        _historyAccount = account;
        BuildHistory();
        MoveTo(Side.Accounts, 2 * AccountsHeight);
    }

    private void BuildHistory()
    {
        var account = _historyAccount!;
        var back = Ui.IconButton(Ui.Icon(Ui.ChevronLeft), () => MoveTo(Side.Accounts, AccountsHeight));
        var more = Ui.IconButton(Ui.Dots(), () => ActionSheet(account,
            ("Hernoemen", false), ("Hoofdrekening maken", false), ("Saldo corrigeren", false), ("Verwijderen", true)));
        var title = Ui.Text(account, "h1");
        var header = Ui.Columns("Auto,*,Auto", back, title, more);

        var balance = _ledger.BalanceOf(account);
        var big = Ui.Stack(0, Ui.Text("Saldo vandaag", "muted"), Ui.Text(Dutch.Euro(balance), "big"));
        big.Margin = new Thickness(4, 0, 0, 0);
        var body = Ui.Stack(0, big);

        foreach (var day in _ledger.HistoryOf(account).GroupBy(e => e.Date).Take(40))
        {
            body.Children.Add(Ui.Caption(Dutch.DayHeading(day.Key, _ledger.Today)));
            body.Children.Add(Ui.List(day.Select(entry =>
            {
                var effect = SampleLedger.Effect(entry, account);
                var kind = entry.Kind switch
                {
                    EntryKind.Expense => $"Uitgave · {entry.Category}",
                    EntryKind.Income => "Inkomst",
                    _ when entry.Account == account => $"Naar {entry.ToAccount} · voor {entry.Category}",
                    _ => $"Van {entry.Account} · voor {entry.Category}",
                };
                var figure = Ui.Text(Dutch.Signed(effect), "row");
                if (effect > 0)
                {
                    figure.Res(TextBlock.ForegroundProperty, "Positive");
                }

                return (Control)new Border
                {
                    Padding = new Thickness(16, 13),
                    Child = Ui.Columns("*,Auto", Ui.Stack(2, Ui.Text(Shown(entry), "row"), Ui.Text(kind, "muted")), figure),
                };
            })));
        }

        AccountHistory.Child = Page(header, body, footer: Ui.Handle());
        header.Margin = new Thickness(8, _safe.Top + 10, 8, 8);
    }
}
