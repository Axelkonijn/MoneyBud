using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using MoneyBud.Prototype.Motion;
using MoneyBud.Prototype.Platform;
using MoneyBud.Prototype.Sample;
using MoneyBud.Prototype.Themes;

namespace MoneyBud.Prototype.Views;

/// <summary>What lies on top of everything: the message bar, pop-ups, and the first-time hints.</summary>
public sealed partial class MainView
{
    private readonly TranslateTransform _toastShift = new(0, 200);
    private DispatcherTimer? _toastTimer;
    private FrameLoop? _toastMove;
    private FrameLoop? _hintLoop;
    private DispatcherTimer? _hintTimer;
    private Control? _modal;
    private bool _modalFromBottom;

    // ---- The message bar -----------------------------------------------------------------------

    /// <summary>Says something in the bar that slides up from below; a question stays until answered.</summary>
    private void Say(string text, params (string Label, Action Act)[] answers)
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
        Toast.RenderTransform = _toastShift;
        Toast.IsVisible = true;
        _toastMove?.Stop();
        var from = _toastShift.Y;
        _toastMove = Tween.Run(this, 0.42, Ease.OutBack, t => _toastShift.Y = Ease.Lerp(from, 0, t));

        _toastTimer?.Stop();
        if (answers.Length == 0)
        {
            _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.6) };
            _toastTimer.Tick += (_, _) => HideToast();
            _toastTimer.Start();
        }
    }

    private void Ask(string question, string confirm, Action onConfirm) =>
        Say(question, (confirm, () =>
        {
            HideToast();
            onConfirm();
        }), ("Annuleren", HideToast));

    private void HideToast()
    {
        _toastTimer?.Stop();
        _toastMove?.Stop();
        var from = _toastShift.Y;
        _toastMove = Tween.Run(this, 0.25, Ease.InCubic, t => _toastShift.Y = Ease.Lerp(from, 200, t), done: () => Toast.IsVisible = false);
    }

    // ---- Pop-ups -------------------------------------------------------------------------------

    private void ShowModal(Control content, bool fromBottom)
    {
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
        Tween.Run(this, 0.34, Ease.OutCubic, t =>
        {
            scrim.Opacity = 0.5 * t;
            Place(content, fromBottom, t);
        });
    }

    private void CloseModal()
    {
        if (_modal is not { } content || ModalLayer.Children.FirstOrDefault() is not { } scrim)
        {
            ModalLayer.IsVisible = false;
            return;
        }

        _modal = null;
        var fromBottom = _modalFromBottom;
        Tween.Run(this, 0.22, Ease.InCubic, t =>
        {
            scrim.Opacity = 0.5 * (1 - t);
            Place(content, fromBottom, 1 - t);
        }, done: () =>
        {
            ModalLayer.IsVisible = false;
            ModalLayer.Children.Clear();
        });
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
            Side = SurfaceSide.Bottom,
            CornerRadius = new CornerRadius(28, 28, 0, 0),
            Padding = new Thickness(18, 0, 18, _safe.Bottom + 18),
            BorderThickness = new Thickness(0, 1, 0, 0),
            Child = Ui.Stack(12, Ui.Handle(), Ui.Text(title, "h2"), body),
        }.Res(Border.BackgroundProperty, "Card").Res(Border.BorderBrushProperty, "CardEdge");

    private Surface Card(string title, Control body) =>
        new Surface
        {
            CornerRadius = new CornerRadius(26),
            Padding = new Thickness(22),
            Margin = new Thickness(18),
            BorderThickness = new Thickness(1),
            Child = Ui.Stack(14, Ui.Text(title, "h2"), body),
        }.Res(Border.BackgroundProperty, "Card").Res(Border.BorderBrushProperty, "CardEdge");

    /// <summary>A menu that slides up from below, for the things behind a ⋯.</summary>
    private void Choice(string title, IEnumerable<(string Label, bool Danger, Action Act)> options)
    {
        var rows = options.Select(o =>
        {
            var label = Ui.Text(o.Label, "row");
            if (o.Danger)
            {
                label.Classes.Add("danger");
            }

            return (Control)Ui.Plain(label, () =>
            {
                CloseModal();
                o.Act();
            }, new Thickness(16, 15));
        });
        var cancel = Ui.Pill("Annuleren", CloseModal, "Ghost");
        cancel.HorizontalAlignment = HorizontalAlignment.Stretch;
        var list = Ui.List(rows).Res(Border.BackgroundProperty, "Field");
        ShowModal(Sheet(title, Ui.Stack(12, list, cancel)), fromBottom: true);
    }

    private void ActionSheet(string title, params (string Label, bool Danger)[] options) =>
        Choice(title, options.Select(o => (o.Label, o.Danger, (Action)(() => Say($"{o.Label} zit nog niet in het prototype.")))));

    private void CategoryMenu(SampleCategory category) =>
        ActionSheet(category.Name, ("Hernoemen", false), ("Archiveren", false), ("Verwijderen", true));

    private void ChooseSweepDestination()
    {
        var options = _ledger.Categories.Where(c => c.BackedBy is not null).Select(c => c.Name).Append("Nergens");
        Choice("Restant naar", options.Select(name => (name, false, (Action)(() =>
        {
            _ledger.SweepDestination = name == "Nergens" ? null : name;
            BuildBudget();
            Say(name == "Nergens" ? "Het restant blijft staan." : $"Het restant gaat voortaan naar {name}.");
        }))));
    }

    private void ChooseBacking(SampleCategory category)
    {
        var options = _ledger.Accounts.Select(a => a.Name).Append("Nergens");
        Choice($"{category.Name} staat op", options.Select(name => (name, false, (Action)(() =>
        {
            category.BackedBy = name == "Nergens" ? null : name;
            BuildBudget();
            ShowHole(countUp: false);
            Say(name == "Nergens" ? $"{category.Name} staat nergens meer op." : $"{category.Name} staat nu op {name}.");
        }))));
    }

    private void ShowSettings()
    {
        var variant = Application.Current!.RequestedThemeVariant;
        var shown = variant == ThemeVariant.Dark ? "Donker" : variant == ThemeVariant.Light ? "Licht" : "Systeem";
        var look = Ui.Chips(["Systeem", "Donker", "Licht"], shown, picked =>
            Application.Current.RequestedThemeVariant = picked switch
            {
                "Donker" => ThemeVariant.Dark,
                "Licht" => ThemeVariant.Light,
                _ => ThemeVariant.Default,
            });
        var theme = Ui.Chips(Looks.All.Select(l => l.Name), Looks.Current.Name, picked => SwitchLook(Looks.All.First(l => l.Name == picked)));
        var hints = Ui.Pill("Aanwijzingen opnieuw tonen", () =>
        {
            CloseModal();
            _accounts.AnimateTo(0);
            ShowHints(delay: 0.6);
        }, "Ghost");
        hints.HorizontalAlignment = HorizontalAlignment.Stretch;
        var done = Ui.Pill("Klaar", CloseModal);
        done.HorizontalAlignment = HorizontalAlignment.Stretch;

        ShowModal(Card("Instellingen", Ui.Stack(0,
            Ui.Caption("Weergave"), look,
            Ui.Caption("Thema"), theme,
            new Border { Height = 20 }, hints, new Border { Height = 10 }, done)), fromBottom: false);
    }

    /// <summary>Tapping the period's name: on which day a period begins.</summary>
    private void ShowStartDay()
    {
        if (_offset < 0)
        {
            Say("Een voorbije periode houdt haar begin.");
            return;
        }

        var grid = new UniformGrid { Columns = 7 };
        for (var day = 1; day <= 31; day++)
        {
            var chosen = day;
            var chip = new ToggleButton
            {
                Theme = Ui.Theme("Chip"), Content = day.ToString(), IsChecked = day == 1,
                Margin = new Thickness(3), Padding = new Thickness(0), Height = 38,
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            chip.Content = Centered(day.ToString());
            chip.Click += (_, _) =>
            {
                CloseModal();
                if (chosen == 1)
                {
                    Say("Een periode begint al op de 1e.");
                    return;
                }

                Ask($"Vanaf {Dutch.Month(_ledger.FirstDay(_offset))} begint elke periode op de {chosen}e. Wijzigen?", "Wijzigen",
                    () => Say("In het prototype blijven de periodes zoals ze zijn."));
            };
            grid.Children.Add(chip);
        }

        ShowModal(Card("Periode begint op", Ui.Stack(12, Ui.Text("De dag waarop elke nieuwe periode begint.", "muted"), grid)), fromBottom: false);

        static Control Centered(string text)
        {
            var block = Ui.Text(text);
            block.HorizontalAlignment = HorizontalAlignment.Center;
            block.VerticalAlignment = VerticalAlignment.Center;
            block.Width = 34;
            block.TextAlignment = TextAlignment.Center;
            return block;
        }
    }

    private void PickDate(DateOnly current, bool allowFuture, Action<DateOnly> picked)
    {
        var calendar = new Calendar
        {
            SelectedDate = current.ToDateTime(TimeOnly.MinValue),
            DisplayDate = current.ToDateTime(TimeOnly.MinValue),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        if (!allowFuture)
        {
            calendar.DisplayDateEnd = _ledger.Today.ToDateTime(TimeOnly.MinValue);
        }

        calendar.SelectedDatesChanged += (_, _) =>
        {
            if (calendar.SelectedDate is { } date)
            {
                CloseModal();
                picked(DateOnly.FromDateTime(date));
            }
        };
        ShowModal(Card("Datum", calendar), fromBottom: false);
    }

    // ---- First-time hints ----------------------------------------------------------------------

    private void MaybeShowHints()
    {
        if (!Flags.HintsShown)
        {
            ShowHints(delay: 1.7);
        }
    }

    /// <summary>
    /// Light hints on the home screen: where each panel comes from, and that the ring can be
    /// held. They sit in the band under the ring, where a swipe has to start, and fade after ten
    /// seconds or at the first touch.
    /// </summary>
    private void ShowHints(double delay)
    {
        Flags.HintsShown = true;
        Hints.Children.Clear();
        var band = H - _safe.Bottom - 76;
        var centre = Ring.TranslatePoint(new Point(Ring.Bounds.Width / 2, Ring.Bounds.Height / 2), this)?.Y ?? H / 2;

        var arrows = new List<(Control Arrow, double Dx, double Dy)>();
        Place(Hint("Inkomsten", Ui.ChevronRight, before: false, 1, 0), HorizontalAlignment.Left, new Thickness(18, band, 0, 0));
        Place(Hint("Uitgaven", Ui.ChevronLeft, before: true, -1, 0), HorizontalAlignment.Right, new Thickness(0, band, 18, 0));
        Place(Stacked("Budget", Ui.ChevronUp, before: true, 0, -1), HorizontalAlignment.Center, new Thickness(0, band - 8, 0, 0));
        Place(Stacked("Rekeningen", Ui.ChevronDown, before: false, 0, 1), HorizontalAlignment.Center, new Thickness(0, _safe.Top + 2, 0, 0));
        var hold = Ui.Text("Houd je vinger op de ring", "faint");
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
        var from = Hints.Opacity;
        Tween.Run(this, 0.4, Ease.OutCubic, t => Hints.Opacity = from * (1 - t), done: () =>
        {
            Hints.IsVisible = false;
            Hints.Tag = null;
        });
    }
}
