using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MoneyBud.Phone.Motion;
using MoneyBud.Phone.Themes;
using MoneyBud.Presentation;

namespace MoneyBud.Phone.Views;

/// <summary>What lies on top of everything: the message bar, pop-ups, settings, and the first-time hints.</summary>
public sealed partial class MainView
{
    private readonly TranslateTransform _toastShift = new(0, 240);
    private DispatcherTimer? _toastTimer;
    private FrameLoop? _toastMove;
    private FrameLoop? _hintLoop;
    private DispatcherTimer? _hintTimer;
    private Control? _modal;
    private bool _modalFromBottom;
    private Action? _modalClosed;
    private Func<bool>? _modalDone;

    // ---- The message bar -----------------------------------------------------------------------

    /// <summary>
    /// What MoneyBud said, in the bar from below: a notice for a few seconds, a refusal edged in the
    /// warning colour, and the question until it is answered (arc42 §12, <i>The home screen is the
    /// ring</i>). Worked out afresh after every change, like everything else on screen; a pop-up
    /// whose act is done closes here too.
    /// </summary>
    private void ShowMessages()
    {
        if (_modalDone?.Invoke() == true)
        {
            CloseModal();
        }

        if (App.Question is { } question)
        {
            if (!ReferenceEquals(question, _questionShown))
            {
                _questionShown = question;
                _noticeShown = null;
                Say(question.Text, refusal: false,
                    (question.ConfirmText, () => Act(_screen.Confirm)),
                    (Tekst.Cancel, () => Act(_screen.Decline)));
            }

            return;
        }

        var notice = App.Notice;
        if (_questionShown is not null)
        {
            _questionShown = null;
            if (notice is null)
            {
                HideToast();
            }
        }

        if (notice is not null && !ReferenceEquals(notice, _noticeShown))
        {
            Say(notice.Text, notice.IsRefusal);
        }

        _noticeShown = notice;
    }

    private void Say(string text, bool refusal, params (string Label, Action Act)[] answers)
    {
        var message = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, FontSize = 15, Foreground = Brushes.White };
        Control content = message;
        if (answers.Length > 0)
        {
            var buttons = Ui.Row(8, answers.Select(a =>
            {
                var label = new TextBlock { Text = a.Label, FontWeight = FontWeight.SemiBold, Foreground = Brushes.White };
                var button = Ui.Plain(label, a.Act, new Thickness(16, 9));
                button.Background = new SolidColorBrush(Color.FromArgb(0x2E, 0xFF, 0xFF, 0xFF));
                button.CornerRadius = new CornerRadius(16);
                return (Control)button;
            }).ToArray());
            buttons.HorizontalAlignment = HorizontalAlignment.Right;
            content = Ui.Stack(12, message, buttons);
        }

        Toast.Child = content;
        Toast.BorderThickness = new Thickness(refusal ? 1.5 : 0);
        Toast.RenderTransform = _toastShift;
        Toast.IsVisible = true;
        _toastMove?.Stop();
        var from = _toastShift.Y;
        _toastMove = Tween.Run(this, 0.42, Ease.OutBack, t => _toastShift.Y = Ease.Lerp(from, 0, t));

        _toastTimer?.Stop();
        if (answers.Length == 0)
        {
            // Long enough to read a sentence or two; a longer notice stays a little longer.
            _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Math.Clamp(2.4 + text.Length / 30.0, 3.6, 8)) };
            _toastTimer.Tick += (_, _) => HideToast();
            _toastTimer.Start();
        }
    }

    private void HideToast()
    {
        _toastTimer?.Stop();
        _toastMove?.Stop();
        var from = _toastShift.Y;
        _toastMove = Tween.Run(this, 0.25, Ease.InCubic, t => _toastShift.Y = Ease.Lerp(from, 240, t), done: () => Toast.IsVisible = false);
    }

    /// <summary>Above the navigation bar, and above the keyboard while it is up.</summary>
    private void PlaceToast() => Toast.Margin = new Thickness(14, 0, 14, _safe.Bottom + 14 + _keyboard);

    // ---- Pop-ups -------------------------------------------------------------------------------

    /// <param name="onClosed">Done however it closes: the act it was for let go of, if it is still open.</param>
    /// <param name="closeWhen">Asked after every change: true once the act it was for has gone through.</param>
    private void ShowModal(Control content, bool fromBottom, Action? onClosed = null, Func<bool>? closeWhen = null)
    {
        if (_modal is not null)
        {
            CloseModal();
        }

        Unfocus();
        ModalLayer.Children.Clear();
        var scrim = new Border { Background = Brushes.Black, Opacity = 0 };
        scrim.PointerPressed += (_, _) => CloseModal();
        content.VerticalAlignment = fromBottom ? VerticalAlignment.Bottom : VerticalAlignment.Center;
        ModalLayer.Children.Add(scrim);
        ModalLayer.Children.Add(content);
        ModalLayer.IsVisible = true;
        _modal = content;
        _modalFromBottom = fromBottom;
        _modalClosed = onClosed;
        _modalDone = closeWhen;
        Tween.Run(this, 0.34, Ease.OutCubic, t =>
        {
            scrim.Opacity = 0.5 * t;
            Place(content, fromBottom, t);
        });
    }

    private void CloseModal()
    {
        var closed = _modalClosed;
        _modalClosed = null;
        _modalDone = null;
        if (_modal is not { } content || ModalLayer.Children.FirstOrDefault() is not { } scrim)
        {
            ModalLayer.IsVisible = false;
            closed?.Invoke();
            return;
        }

        _modal = null;
        Unfocus();
        var fromBottom = _modalFromBottom;
        Tween.Run(this, 0.22, Ease.InCubic, t =>
        {
            scrim.Opacity = 0.5 * (1 - t);
            Place(content, fromBottom, 1 - t);
        }, done: () =>
        {
            if (_modal is null)
            {
                ModalLayer.IsVisible = false;
                ModalLayer.Children.Clear();
            }
        });
        closed?.Invoke();
    }

    private static void Place(Control content, bool fromBottom, double t)
    {
        if (fromBottom)
        {
            content.RenderTransform = new TranslateTransform(0, (1 - t) * Math.Max(360, content.Bounds.Height));
        }
        else
        {
            content.Opacity = t;
            content.RenderTransform = new ScaleTransform(0.92 + 0.08 * t, 0.92 + 0.08 * t);
        }
    }

    private Surface Sheet(string title, Control body) =>
        new Surface
        {
            Side = Themes.SurfaceSide.Bottom,
            CornerRadius = new CornerRadius(28, 28, 0, 0),
            Padding = new Thickness(18, 0, 18, _safe.Bottom + 18),
            BorderThickness = new Thickness(0, 1, 0, 0),
            Child = Ui.Stack(12, Ui.Handle(), Ui.Text(title, "h2"), body),
        }.Res(Surface.BackgroundProperty, "Card").Res(Surface.BorderBrushProperty, "CardEdge");

    private static Surface Card(string title, Control body) =>
        new Surface
        {
            CornerRadius = new CornerRadius(26),
            Padding = new Thickness(22),
            Margin = new Thickness(18),
            BorderThickness = new Thickness(1),
            Child = Ui.Stack(14, Ui.Text(title, "h2"), body),
        }.Res(Surface.BackgroundProperty, "Card").Res(Surface.BorderBrushProperty, "CardEdge");

    /// <summary>A list that slides up from below, to choose one thing or do one act; the one chosen now carries a tick.</summary>
    private void Choice(string title, IEnumerable<(string Label, bool Danger, Action Act)> options, string? chosen = null)
    {
        var rows = options.Select(o =>
        {
            var label = Ui.Text(o.Label, "row");
            if (o.Danger)
            {
                label.Classes.Add("danger");
            }

            Control content = o.Label == chosen ? Ui.Columns("*,Auto", label, Ui.Icon(Ui.Tick, "Accent", 18)) : label;
            return (Control)Ui.Plain(content, () =>
            {
                CloseModal();
                o.Act();
            }, new Thickness(16, 15));
        });
        var cancel = Ui.Pill(Tekst.Cancel, CloseModal, "Ghost");
        cancel.HorizontalAlignment = HorizontalAlignment.Stretch;
        var list = Ui.List(rows).Res(Border.BackgroundProperty, "Field");
        var scroller = new ScrollViewer { Content = list, MaxHeight = Math.Max(200, H * 0.55) };
        ShowModal(Sheet(title, Ui.Stack(12, scroller, cancel)), fromBottom: true);
    }

    // ---- A category's acts ---------------------------------------------------------------------

    /// <summary>Hernoemen always; Archiveren while in use; Verwijderen only with no history anywhere (§12).</summary>
    private void CategoryMenu(CategoryRow row)
    {
        var options = new List<(string, bool, Action)> { (Tekst.Rename, false, () => RenameCategory(row.Name)) };
        if (!row.IsArchived)
        {
            options.Add((Tekst.Archive, false, () => App.ArchiveCategory(row.Name)));
        }

        if (row.CanDelete)
        {
            options.Add((Tekst.Delete, true, () => App.DeleteCategory(row.Name)));
        }

        Choice(row.Name, options);
    }

    private void RenameCategory(string name)
    {
        App.StartRename(name);
        var box = Ui.Linked(Ui.Field(name), App, nameof(MoneyBudApp.NewName), () => App.NewName, v => App.NewName = v);
        var save = Ui.Pill(Tekst.Save, () => Act(() => App.SaveRename()));
        save.HorizontalAlignment = HorizontalAlignment.Stretch;
        ShowModal(Card(Tekst.RenameCategoryTitle, Ui.Stack(14, box, save)), fromBottom: false,
            onClosed: () =>
            {
                if (App.Renaming is not null)
                {
                    App.CancelRename();
                }
            },
            closeWhen: () => App.Renaming is null);
        box.Focus();
    }

    private void ChooseBacking(CategoryRow row) =>
        Choice(Tekst.BackingOf(row.Name),
            row.BackingChoices.Select(c => (c.Text, false, (Action)(() => App.SetBacking(row.Name, c.Account)))),
            row.ChosenBacking?.Text);

    private void ChooseSweepDestination(PeriodOverview overview) =>
        Choice(Tekst.SweepDestination,
            overview.SweepChoices.Select(c => (c.Text, false, (Action)(() => App.SetSweepDestination(c.Category?.Name)))),
            overview.ChosenSweepDestination?.Text);

    // ---- Accounts' acts ------------------------------------------------------------------------

    private void AddAccount()
    {
        var form = App.AccountForm;
        form.OpenCommand.Execute(null);
        var name = Ui.Linked(Ui.Field(Tekst.ExampleAccount), form, nameof(AccountForm.Name), () => form.Name, v => form.Name = v);
        var balance = AmountField(form, nameof(AccountForm.StartingBalance), () => form.StartingBalance, v => form.StartingBalance = v);
        balance.PlaceholderText = Tekst.StartingBalanceHint;
        var add = Ui.Pill(Tekst.AddAccount, () => Act(form.Submit));
        add.HorizontalAlignment = HorizontalAlignment.Stretch;
        ShowModal(Card(Tekst.NewAccount, Ui.Stack(0,
                Ui.Caption(Tekst.Account), name, Ui.Caption(Tekst.StartingBalance), balance, new Border { Height = 18 }, add)),
            fromBottom: false,
            onClosed: () =>
            {
                if (form.IsOpen)
                {
                    form.Cancel();
                }
            },
            closeWhen: () => !form.IsOpen);
        name.Focus();
    }

    private void OpenTransfer()
    {
        App.TransferForm.OpenCommand.Execute(null);
        ShowTransfer();
    }

    /// <summary><i>Overboeken</i>, or a transfer tapped in a history to change or remove: Van, Naar, Bedrag, Datum.</summary>
    private void ShowTransfer()
    {
        var form = App.TransferForm;
        var from = new ContentControl();
        var to = new ContentControl();
        var dates = new ContentControl();
        var amount = AmountField(form, nameof(TransferForm.Amount), () => form.Amount, v => form.Amount = v);
        var submit = Ui.Pill(form.SubmitText, () => Act(() => form.SubmitCommand.Execute(null)));
        submit.HorizontalAlignment = HorizontalAlignment.Stretch;
        var body = Ui.Stack(0,
            Ui.Caption(Tekst.From), from, Ui.Caption(Tekst.To), to,
            Ui.Caption(Tekst.Amount), amount, Ui.Caption(Tekst.Date), dates,
            new Border { Height = 18 }, submit);
        if (form.IsEditing)
        {
            var remove = Ui.Pill(Tekst.Remove, () => Act(form.Remove), "DangerGhost");
            remove.HorizontalAlignment = HorizontalAlignment.Center;
            remove.Margin = new Thickness(0, 10, 0, 0);
            body.Children.Add(remove);
        }

        void Refresh()
        {
            from.Content = AccountChips(form.ChosenFrom, a => form.ChosenFrom = a);
            to.Content = AccountChips(form.ChosenTo, a => form.ChosenTo = a);
            dates.Content = DateChips(form.Date, allowFuture: false, d => form.Date = d);
        }

        void Changed(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TransferForm.Amount))
            {
                Refresh();
            }
        }

        form.PropertyChanged += Changed;
        Refresh();
        var scroller = new ScrollViewer { Content = body, MaxHeight = Math.Max(300, H * 0.7) };
        ShowModal(Card(form.IsEditing ? Tekst.ChangeTransfer : Tekst.TransferAct, scroller), fromBottom: false,
            onClosed: () =>
            {
                form.PropertyChanged -= Changed;
                if (form.IsOpen)
                {
                    form.Cancel();
                }
            },
            closeWhen: () => !form.IsOpen);
    }

    /// <summary>Hernoemen; Maak hoofdrekening, unless it is; Saldo corrigeren; Verwijderen, only while unused (§12).</summary>
    private void AccountMenu()
    {
        if (App.HistoryAccount is not { } account)
        {
            return;
        }

        var options = new List<(string, bool, Action)> { (Tekst.Rename, false, RenameAccount) };
        if (App.CanMakeHistoryAccountPool)
        {
            options.Add((Tekst.MakePool, false, () => App.MakePool(account)));
        }

        options.Add((Tekst.CorrectBalance, false, CorrectBalance));
        if (App.CanDeleteHistoryAccount)
        {
            options.Add((Tekst.Delete, true, () => _screen.DeleteAccount(account)));
        }

        Choice(account.Name, options);
    }

    private void RenameAccount()
    {
        App.StartAccountRenameCommand.Execute(null);
        var box = Ui.Linked(Ui.Field(App.HistoryAccount?.Name ?? ""), App, nameof(MoneyBudApp.NewAccountName),
            () => App.NewAccountName, v => App.NewAccountName = v);
        var save = Ui.Pill(Tekst.Save, () => Act(() => App.SaveAccountRenameCommand.Execute(null)));
        save.HorizontalAlignment = HorizontalAlignment.Stretch;
        ShowModal(Card(Tekst.RenameAccountTitle, Ui.Stack(14, box, save)), fromBottom: false,
            onClosed: () =>
            {
                if (App.RenamingAccount)
                {
                    App.CancelAccountRenameCommand.Execute(null);
                }
            },
            closeWhen: () => !App.RenamingAccount);
        box.Focus();
    }

    /// <summary><i>Saldo corrigeren</i>: the balance the bank shows today. Any amount reads, zero and below included.</summary>
    private void CorrectBalance()
    {
        var box = AmountField(App, nameof(MoneyBudApp.BalanceInput), () => App.BalanceInput, v => App.BalanceInput = v);
        var correct = Ui.Pill(Tekst.CorrectBalance, () => Act(() =>
        {
            App.CorrectOpenBalanceCommand.Execute(null);
            if (App.Notice is { IsRefusal: false })
            {
                CloseModal();
            }
        }));
        correct.HorizontalAlignment = HorizontalAlignment.Stretch;
        ShowModal(Card(Tekst.CorrectBalance, Ui.Stack(0, Ui.Caption(Tekst.Balance), box, new Border { Height = 18 }, correct)),
            fromBottom: false, onClosed: () => App.BalanceInput = null);
        box.Focus();
    }

    // ---- Settings and the period start day -----------------------------------------------------

    /// <summary><i>Instellingen</i>: <i>Weergave</i>, <i>Thema</i>, the hints again, and <i>Klaar</i>.</summary>
    private void ShowSettings()
    {
        var appearance = Ui.Chips(PhoneSettings.Appearances.Select(a => a.Text),
            PhoneSettings.Appearances.First(a => a.Appearance == _settings.Appearance).Text, picked =>
            {
                var chosen = PhoneSettings.Appearances.First(a => a.Text == picked).Appearance;
                _settings.ChooseAppearance(chosen);
                Looks.UseAppearance(chosen);
            });
        var theme = Ui.Chips(PhoneSettings.Themes.Select(t => t.Text),
            PhoneSettings.Themes.First(t => t.Theme == _settings.Theme).Text, picked =>
            {
                var chosen = PhoneSettings.Themes.First(t => t.Text == picked).Theme;
                _settings.ChooseTheme(chosen);
                SwitchLook(Looks.Of(chosen));
            });
        var hints = Ui.Pill(Tekst.ShowHintsAgain, () =>
        {
            CloseModal();
            MoveTo(PhonePanel.Accounts, PanelStep.Closed);
            _settings.ShowHintsAgain();
            ShowHints(delay: 0.6);
        }, "Ghost");
        hints.HorizontalAlignment = HorizontalAlignment.Stretch;
        var done = Ui.Pill(Tekst.Done, CloseModal);
        done.HorizontalAlignment = HorizontalAlignment.Stretch;

        ShowModal(Card(Tekst.Settings, Ui.Stack(0,
            Ui.Caption(Tekst.Appearance), appearance,
            Ui.Caption(Tekst.Theme), theme,
            new Border { Height = 20 }, hints, new Border { Height = 10 }, done)), fromBottom: false);
    }

    /// <summary>
    /// Tapping the period's name: <i>Periode begint op</i>, on the current period and later ones, as
    /// the desktop shows its list only there. Choosing a day asks first, in the message bar.
    /// </summary>
    private void ShowStartDay()
    {
        if (!App.ShowsStartDay)
        {
            return;
        }

        var grid = new UniformGrid { Columns = 7 };
        foreach (var day in App.StartDayChoices)
        {
            var text = Ui.Text(Tekst.StartDayName(day));
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.TextAlignment = TextAlignment.Center;
            text.Width = 34;
            var chip = new ToggleButton
            {
                Theme = Ui.Theme("Chip"), Content = text, IsChecked = day == App.StartDayChoice,
                Margin = new Thickness(3), Padding = new Thickness(0), Height = 38,
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            chip.Click += (_, _) =>
            {
                CloseModal();
                App.ChooseStartDay(day);
            };
            grid.Children.Add(chip);
        }

        ShowModal(Card(Tekst.PeriodStartDay, Ui.Stack(12, Ui.Text(Tekst.StartDayExplained, "muted"), grid)), fromBottom: false);
    }

    private void PickDate(DateTime current, bool allowFuture, Action<DateTime> picked)
    {
        var calendar = new Calendar
        {
            SelectedDate = current,
            DisplayDate = current,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        if (!allowFuture)
        {
            calendar.DisplayDateEnd = App.Ledger.Today.ToDateTime(TimeOnly.MinValue);
        }

        calendar.SelectedDatesChanged += (_, _) =>
        {
            if (calendar.SelectedDate is { } date)
            {
                CloseModal();
                picked(date.Date);
            }
        };

        // Over a pop-up (a transfer's date), the calendar takes its place and gives it back.
        var under = _modal;
        if (under is not null)
        {
            var (closed, done, fromBottom) = (_modalClosed, _modalDone, _modalFromBottom);
            _modalClosed = null;
            ShowModal(Card(Tekst.Date, calendar), fromBottom: false, onClosed: () => Dispatcher.UIThread.Post(() =>
                ShowModal(under, fromBottom, closed, done)));
            return;
        }

        ShowModal(Card(Tekst.Date, calendar), fromBottom: false);
    }

    // ---- First-time hints ----------------------------------------------------------------------

    /// <summary>
    /// Light hints on the home screen: where each panel comes from, and that the ring can be held.
    /// They sit in the band under the ring, where a swipe has to start, and fade after ten seconds or
    /// at the first touch. That they were shown is the settings' to remember.
    /// </summary>
    private void ShowHints(double delay)
    {
        Hints.Children.Clear();
        var band = H - _safe.Bottom - 76;
        var centre = Ring.TranslatePoint(new Point(Ring.Bounds.Width / 2, Ring.Bounds.Height / 2), this)?.Y ?? H / 2;

        var arrows = new List<(Control Arrow, double Dx, double Dy)>();
        Place(Hint(Tekst.Incomes, Ui.ChevronRight, before: false, 1, 0), HorizontalAlignment.Left, new Thickness(18, band, 0, 0));
        Place(Hint(Tekst.Expenses, Ui.ChevronLeft, before: true, -1, 0), HorizontalAlignment.Right, new Thickness(0, band, 18, 0));
        Place(Stacked(Tekst.Budget, Ui.ChevronUp, before: true, 0, -1), HorizontalAlignment.Center, new Thickness(0, band - 8, 0, 0));
        Place(Stacked(Tekst.Accounts, Ui.ChevronDown, before: false, 0, 1), HorizontalAlignment.Center, new Thickness(0, _safe.Top + 2, 0, 0));
        var hold = Ui.Text(Tekst.HoldTheRing, "faint");
        Place(hold, HorizontalAlignment.Center, new Thickness(0, centre + Ring.Inner * 0.4, 0, 0));

        Hints.Opacity = 0;
        Hints.IsVisible = true;
        _hintLoop?.Stop();
        var time = -delay;
        _hintLoop = FrameLoop.Start(this, dt =>
        {
            time += dt;
            Hints.Opacity = Math.Clamp(time / 0.6, 0, 1) * (Hints.Tag is "leaving" ? 0 : 1);
            var nudge = 5 * (0.5 - 0.5 * Math.Cos(time * 2 * Math.PI / 1.5));
            foreach (var (arrow, x, y) in arrows)
            {
                arrow.RenderTransform = new TranslateTransform(x * nudge, y * nudge);
            }

            return Hints.IsVisible;
        });

        Hints.Tag = null;
        _hintTimer?.Stop();
        _hintTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(delay + 10) };
        _hintTimer.Tick += (_, _) => HideHints();
        _hintTimer.Start();

        void Place(Control control, HorizontalAlignment alignment, Thickness margin)
        {
            control.HorizontalAlignment = alignment;
            control.VerticalAlignment = VerticalAlignment.Top;
            control.Margin = margin;
            Hints.Children.Add(control);
        }

        Control Hint(string text, string chevron, bool before, double dx, double dy)
        {
            var arrow = Ui.Icon(chevron, "Muted", 16);
            arrows.Add((arrow, dx, dy));
            var label = Ui.Text(text, "muted");
            return before ? Ui.Row(4, arrow, label) : Ui.Row(4, label, arrow);
        }

        Control Stacked(string text, string chevron, bool before, double dx, double dy)
        {
            var arrow = Ui.Icon(chevron, "Muted", 16);
            arrow.HorizontalAlignment = HorizontalAlignment.Center;
            arrows.Add((arrow, dx, dy));
            var label = Ui.Text(text, "muted");
            label.HorizontalAlignment = HorizontalAlignment.Center;
            return before ? Ui.Stack(2, arrow, label) : Ui.Stack(2, label, arrow);
        }
    }

    private void HideHints()
    {
        if (!Hints.IsVisible || Hints.Tag is "leaving")
        {
            return;
        }

        _hintTimer?.Stop();
        _hintLoop?.Stop();
        Hints.Tag = "leaving";
        _settings.HintsGone();
        var from = Hints.Opacity;
        Tween.Run(this, 0.4, Ease.OutCubic, t => Hints.Opacity = from * (1 - t), done: () =>
        {
            Hints.IsVisible = false;
            Hints.Tag = null;
        });
    }
}
