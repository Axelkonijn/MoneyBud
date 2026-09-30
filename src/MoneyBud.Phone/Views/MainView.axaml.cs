using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MoneyBud.Phone.Motion;
using MoneyBud.Phone.Themes;
using MoneyBud.Presentation;

namespace MoneyBud.Phone.Views;

/// <summary>
/// The shell: where each layer is, and the gestures that move them. Each panel has one number — how
/// far it is pulled in — driven by a spring, and everything else (the panels' positions, how much
/// the home screen shrinks and dims) is worked out from those numbers every frame.
///
/// <para>It decides nothing (ADR 0013, decision 4). Where the user is, what a touch on the ring
/// means, what the back button does and where an act leaves a panel are <see cref="PhoneScreen"/>'s;
/// every figure and word is <see cref="MoneyBudApp"/>'s. A drag moves a spring and reports where it
/// came to rest; an act moves <see cref="PhoneScreen"/>, and the springs follow.</para>
/// </summary>
public sealed partial class MainView : UserControl
{
    /// <summary>How far a finger moves before it counts as a swipe rather than a tap.</summary>
    private const double Slop = 10;

    private readonly PhoneScreen _screen;
    private readonly PhoneSettings _settings;
    private readonly PhoneHost _host;
    private readonly DispatcherTimer _clock;

    private readonly Spring _income;
    private readonly Spring _expense;
    private readonly Spring _budget;
    private readonly Spring _accounts;

    private readonly TranslateTransform _incomeListShift = new();
    private readonly TranslateTransform _incomeFormShift = new();
    private readonly TranslateTransform _expenseListShift = new();
    private readonly TranslateTransform _expenseFormShift = new();
    private readonly TranslateTransform _sheetShift = new();
    private readonly TranslateTransform _accountsShift = new();
    private readonly TranslateTransform _historyShift = new();

    /// <summary>What a panel does to the home screen under it, from the theme: glass blurs it, a slab does not.</summary>
    private double _homeBlur = 22;
    private double _homeDim = 0.32;

    private Thickness _safe;
    private Size _laidOut;
    private bool _opened;
    private double _keyboard;

    private Track _track;
    private Point _start;
    private PhonePanel _dragPanel;
    private double _dragBase;
    private object? _pressSource;
    private readonly List<(ulong Time, double Value)> _samples = [];

    public MainView(PhoneScreen screen, PhoneSettings settings, PhoneHost host)
    {
        InitializeComponent();
        Focusable = true;
        _screen = screen;
        _settings = settings;
        _host = host;

        _income = new Spring(this, _ => ApplyIncome());
        _expense = new Spring(this, _ => ApplyExpense());
        _budget = new Spring(this, _ => ApplyBudget());
        _accounts = new Spring(this, _ => ApplyAccounts());
        foreach (var panel in (PhonePanel[])[PhonePanel.Income, PhonePanel.Expenses, PhonePanel.Budget, PhonePanel.Accounts])
        {
            SpringOf(panel).Settled += value =>
            {
                _screen.Settled(panel, StepAt(panel, value));
                DrawInIfWaiting();
            };
        }

        IncomeList.RenderTransform = _incomeListShift;
        IncomeFormPanel.RenderTransform = _incomeFormShift;
        ExpenseList.RenderTransform = _expenseListShift;
        ExpenseFormPanel.RenderTransform = _expenseFormShift;
        BudgetSheet.RenderTransform = _sheetShift;
        AccountsList.RenderTransform = _accountsShift;
        AccountHistory.RenderTransform = _historyShift;

        PreviousPeriod.Content = Ui.Icon(Ui.ChevronLeft, "Muted");
        NextPeriod.Content = Ui.Icon(Ui.ChevronRight, "Muted");
        PreviousPeriod.Click += (_, _) => StepPeriod(-1);
        NextPeriod.Click += (_, _) => StepPeriod(+1);
        PeriodButton.Click += (_, _) => ShowStartDay();

        Ring.Painter = Looks.Current.Painter();
        Texture.Background = Grain.Brush;
        Texture.Bind(OpacityProperty, Texture.GetResourceObservable("GrainOpacity"));
        Hole.Bind(EffectProperty, Hole.GetResourceObservable("HoleEffect"));
        HoleBackdrop.Bind(OpacityProperty, Hole.GetObservable(OpacityProperty));

        Ring.Tick = () => _host.Tick();

        AddHandler(PointerPressedEvent, OnPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerMovedEvent, OnMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerCaptureLostEvent, (_, _) => EndTracking(), RoutingStrategies.Direct);

        SizeChanged += (_, _) => LayOut();
        ActualThemeVariantChanged += (_, _) =>
        {
            PaintGlow();
            Ring.InvalidateVisual();
        };

        // Once a minute, while MoneyBud is in the foreground, the screen looks again: the
        // current-period label, a save that failed, what came due. What that does is MoneyBudApp's.
        _clock = new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.Background, (_, _) => App.Tick());

        Ring.Pressed += share => _screen.TouchRing(share);
        Ring.Slid += share => _screen.SlideOnRing(share);
        Ring.Lifted += (share, slid) => _screen.LiftFromRing(share, slid);
        _screen.Changed += ScheduleRedraw;
        _screen.ChoiceChanged += ChoiceChanged;
        _screen.Moved += FollowScreen;
        _settings.Changed += () => ScheduleRedraw();

        BuildForms();
    }

    private MoneyBudApp App => _screen.App;

    private enum Track
    {
        None,
        Undecided,
        Ignored,
        Dragging,
    }

    private double W => Bounds.Width;

    private double H => Bounds.Height;

    private double SheetFull => Math.Max(0, H - _safe.Top - 14);

    private double AccountsHeight => Math.Max(0, H - 170);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is not { } top)
        {
            return;
        }

        top.BackRequested += OnBack;
        Looks.Changed += LookChanged;
        _host.WentToBackground += WentToBackground;
        _host.CameBack += CameBack;
        LookChanged();
        if (top.InsetsManager is { } insets)
        {
            insets.DisplayEdgeToEdgePreference = true;
            insets.SafeAreaChanged += (_, _) => ApplySafeArea(insets.SafeAreaPadding);
            ApplySafeArea(insets.SafeAreaPadding);
        }

        if (top.InputPane is { } pane)
        {
            pane.StateChanged += (_, args) => KeyboardChanged(args.EndRect.Height);
        }

        _clock.Start();
        Dispatcher.UIThread.Post(Open, DispatcherPriority.Loaded);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is { } top)
        {
            top.BackRequested -= OnBack;
        }

        Looks.Changed -= LookChanged;
        _host.WentToBackground -= WentToBackground;
        _host.CameBack -= CameBack;
        _clock.Stop();

        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>The PC's phone window closing: MoneyBud's last attempt to save, and it lets go (the desktop's rule).</summary>
    public void Closing()
    {
        _clock.Stop();
        App.Close();
    }

    // ---- Android's lifecycle (arc42 §12; plan for increment 14, D4) ----------------------------

    private void WentToBackground()
    {
        _clock.Stop();
        Unfocus();
        App.GoToBackground();
    }

    private void CameBack()
    {
        App.ComeBack();
        _clock.Start();
    }

    private void ApplySafeArea(Thickness safe)
    {
        _safe = safe;
        HomeGrid.Margin = new Thickness(0, safe.Top, 0, safe.Bottom);
        PlaceToast();
        LayOut();
        RebuildAll();
    }

    /// <summary>Sizes the panels and puts every layer where its spring says, after a resize.</summary>
    private void LayOut()
    {
        if (W <= 0 || H <= 0)
        {
            return;
        }

        BudgetSheet.Height = SheetFull;
        AccountsList.Height = AccountsHeight;
        AccountHistory.Height = AccountsHeight;
        Hole.MaxWidth = Math.Max(120, Ring.Inner * 1.55);

        if (_laidOut != default && _laidOut != Bounds.Size)
        {
            // Keep each open panel at the same step: a full panel stays full on the new size.
            foreach (var panel in (PhonePanel[])[PhonePanel.Income, PhonePanel.Expenses, PhonePanel.Budget, PhonePanel.Accounts])
            {
                var spring = SpringOf(panel);
                if (spring.Value > 0.5)
                {
                    spring.Set(Detents(panel)[(int)StepAt(panel, spring.Value, _laidOut)]);
                }
            }
        }

        _laidOut = Bounds.Size;
        ApplyIncome();
        ApplyExpense();
        ApplyBudget();
        ApplyAccounts();
        PaintGlow();
    }

    private void PaintGlow()
    {
        if (H <= 0 || !this.TryFindResource("GlowColor", ActualThemeVariant, out var value) || value is not Color glow)
        {
            return;
        }

        var centre = Ring.TranslatePoint(new Point(Ring.Bounds.Width / 2, Ring.Bounds.Height / 2), this) ?? new Point(W / 2, H * 0.45);
        Glow.Background = new RadialGradientBrush
        {
            Center = new RelativePoint(centre.X / W, centre.Y / H, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(centre.X / W, centre.Y / H, RelativeUnit.Relative),
            RadiusX = new RelativeScalar(0.85, RelativeUnit.Relative),
            RadiusY = new RelativeScalar(0.85 * W / H, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(glow, 0),
                new GradientStop(Color.FromArgb(0, glow.R, glow.G, glow.B), 1),
            },
        };
    }

    // ---- Where each panel is -------------------------------------------------------------------

    private void ApplyIncome()
    {
        var v = _income.Value;
        var list = Math.Clamp(v, 0, W);
        var form = Math.Clamp(v - W, 0, W);
        _incomeListShift.X = -W + list + 0.3 * form;
        _incomeFormShift.X = -W + form;
        IncomeList.Opacity = W > 0 ? 1 - form / W : 1;
        IncomeSide.IsVisible = v > 0.5;
        ApplyHome();
    }

    private void ApplyExpense()
    {
        var v = _expense.Value;
        var list = Math.Clamp(v, 0, W);
        var form = Math.Clamp(v - W, 0, W);
        _expenseListShift.X = W - list - 0.3 * form;
        _expenseFormShift.X = W - form;
        ExpenseList.Opacity = W > 0 ? 1 - form / W : 1;
        ExpenseSide.IsVisible = v > 0.5;
        ApplyHome();
    }

    private void ApplyBudget()
    {
        var v = _budget.Value;
        _sheetShift.Y = SheetFull - v;
        BudgetSheet.IsVisible = v > 0.5;
        ApplyHome();
    }

    private void ApplyAccounts()
    {
        var v = _accounts.Value;
        var height = AccountsHeight;
        var list = Math.Clamp(v, 0, height);
        var history = Math.Clamp(v - height, 0, height);
        _accountsShift.Y = -height + list + 0.2 * history;
        _historyShift.Y = -height + history;
        AccountsList.Opacity = height > 0 ? 1 - history / height : 1;
        AccountsSide.IsVisible = v > 0.5;
        ApplyHome();
    }

    /// <summary>
    /// The home screen under whatever is pulled over it: it dims, and in the default theme blurs,
    /// like something seen through glass; it stays exactly where it is (§12, round 1) — under the
    /// budget too, since the budget opens in one pull and no longer stops half-way (Axel, 2026-09-30).
    /// </summary>
    private void ApplyHome()
    {
        if (W <= 0 || H <= 0)
        {
            return;
        }

        var over = Math.Max(Math.Max(_income.Value / W, _expense.Value / W),
            Math.Max(AccountsHeight > 0 ? _accounts.Value / AccountsHeight : 0, SheetFull > 0 ? _budget.Value / SheetFull : 0));
        over = Math.Clamp(over, 0, 1);
        Scrim.Opacity = _homeDim * over;
        Home.Effect = over > 0.01 && _homeBlur > 0 ? new BlurEffect { Radius = _homeBlur * over } : null;
    }

    private Spring SpringOf(PhonePanel panel) => panel switch
    {
        PhonePanel.Income => _income,
        PhonePanel.Expenses => _expense,
        PhonePanel.Budget => _budget,
        _ => _accounts,
    };

    /// <summary>The resting points of a panel, in the same units as its spring: closed, first step, second.</summary>
    private double[] Detents(PhonePanel panel) => Detents(panel, Bounds.Size);

    private double[] Detents(PhonePanel panel, Size size)
    {
        var height = Math.Max(0, size.Height - 170);
        return panel switch
        {
            PhonePanel.Income or PhonePanel.Expenses => [0, size.Width, 2 * size.Width],
            PhonePanel.Budget => [0, Math.Max(0, size.Height - _safe.Top - 14)],
            _ => [0, height, 2 * height],
        };
    }

    /// <summary>Which step a panel's spring is nearest to.</summary>
    private PanelStep StepAt(PhonePanel panel, double value) => StepAt(panel, value, Bounds.Size);

    private PanelStep StepAt(PhonePanel panel, double value, Size size)
    {
        var detents = Detents(panel, size);
        return (PanelStep)Array.IndexOf(detents, Nearest(detents, value));
    }

    /// <summary>The panels follow <see cref="PhoneScreen"/> after an act or the back button moved it.</summary>
    private void FollowScreen()
    {
        var open = _screen.Open;
        foreach (var panel in (PhonePanel[])[PhonePanel.Income, PhonePanel.Expenses, PhonePanel.Budget, PhonePanel.Accounts])
        {
            if (panel != open && SpringOf(panel).Value > 0.5)
            {
                SpringOf(panel).AnimateTo(0);
            }
        }

        if (open == PhonePanel.None)
        {
            Unfocus();
            return;
        }

        BuildPanel(open);
        SpringOf(open).AnimateTo(Detents(open)[(int)_screen.Step]);
        if (_screen.Step != PanelStep.Second)
        {
            Unfocus();
        }
    }

    /// <summary>Moves a panel to a step by itself, as the back arrows in a panel's header do.</summary>
    private void MoveTo(PhonePanel panel, PanelStep step)
    {
        if (step != PanelStep.Closed)
        {
            _screen.Pull(panel);
            BuildPanel(panel);
        }

        SpringOf(panel).AnimateTo(Detents(panel)[(int)step]);
    }

    /// <summary>Takes focus off any field, which also puts the keyboard away.</summary>
    private void Unfocus() => Focus();

    // ---- Gestures ------------------------------------------------------------------------------

    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ModalLayer.IsVisible)
        {
            _track = Track.Ignored;
            return;
        }

        _start = e.GetPosition(this);
        _pressSource = e.Source;
        _samples.Clear();
        _track = Track.Undecided;
        HideHints();

        // The ring is for holding and sliding, never for swiping: the whole pizza, and a margin.
        var open = _screen.Open;
        var ringVisible = open == PhonePanel.None;
        if (ringVisible && !OnSheet(_start) && Ring.IsInTouchZone(e.GetPosition(Ring)))
        {
            _track = Track.Ignored;
        }
    }

    private void OnMoved(object? sender, PointerEventArgs e)
    {
        if (_track is Track.None or Track.Ignored)
        {
            return;
        }

        var point = e.GetPosition(this);
        var dx = point.X - _start.X;
        var dy = point.Y - _start.Y;

        if (_track == Track.Undecided)
        {
            if (Math.Abs(dx) < Slop && Math.Abs(dy) < Slop)
            {
                return;
            }

            if (Decide(Math.Abs(dx) > Math.Abs(dy), dx, dy) is not { } panel)
            {
                _track = Track.Ignored;
                return;
            }

            _track = Track.Dragging;
            _dragPanel = panel;
            var spring = SpringOf(panel);
            if (_screen.Open != panel)
            {
                // Another panel still showing is put away at once, as pulling this one closes it.
                foreach (var other in (PhonePanel[])[PhonePanel.Income, PhonePanel.Expenses, PhonePanel.Budget, PhonePanel.Accounts])
                {
                    if (other != panel)
                    {
                        SpringOf(other).Set(0);
                    }
                }

                _screen.Pull(panel);
                BuildPanel(panel);
            }

            _dragBase = spring.Value;
            spring.Set(_dragBase);
            e.Pointer.Capture(this);
        }

        var value = Math.Max(0, _dragBase + Along(_dragPanel, dx, dy));
        var top = Detents(_dragPanel)[^1];
        if (_dragPanel == PhonePanel.Accounts && !App.IsHistoryOpen)
        {
            top = Detents(_dragPanel)[1];
        }

        if (value > top)
        {
            value = top + (value - top) * 0.25; // Give a little past the end, and resist.
        }

        SpringOf(_dragPanel).Set(value);
        _samples.Add((e.Timestamp, Along(_dragPanel, point.X, point.Y)));
        if (_samples.Count > 12)
        {
            _samples.RemoveAt(0);
        }

        e.Handled = true;
    }

    private void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        var track = _track;
        EndTracking();

        if (track == Track.Dragging)
        {
            e.Pointer.Capture(null);
            e.Handled = true;
            var spring = SpringOf(_dragPanel);
            var velocity = Velocity();
            var detents = Detents(_dragPanel);
            if (_dragPanel == PhonePanel.Accounts && !App.IsHistoryOpen)
            {
                detents = detents[..2];
            }

            // One swipe moves one step — list, then form; the budget has only the one.
            var from = Array.IndexOf(detents, Nearest(detents, _dragBase));
            var reachable = _dragPanel == PhonePanel.Budget ? detents : detents[Math.Max(0, from - 1)..Math.Min(detents.Length, from + 2)];
            var target = Nearest(reachable, spring.Value + velocity * 0.18);
            spring.AnimateTo(target, velocity);
            return;
        }

        if (track != Track.Undecided || IsInButton(_pressSource))
        {
            return;
        }

        // A tap, not a swipe.
        var point = e.GetPosition(this);
        switch (_screen.Open)
        {
            case PhonePanel.None:
                _screen.TapHome();
                break;
            case PhonePanel.Budget when !OnSheet(point):
                MoveTo(PhonePanel.Budget, PanelStep.Closed);
                break;
            case PhonePanel.Accounts when point.Y > AccountsHeight + 20:
                MoveTo(PhonePanel.Accounts, PanelStep.Closed);
                break;
        }
    }

    private void EndTracking() => _track = Track.None;

    /// <summary>Which panel a swipe moves, given where things are; null leaves it to a list's scrolling.</summary>
    private PhonePanel? Decide(bool horizontal, double dx, double dy)
    {
        switch (_screen.Open)
        {
            case PhonePanel.None:
                return horizontal ? (dx > 0 ? PhonePanel.Income : PhonePanel.Expenses) : (dy < 0 ? PhonePanel.Budget : PhonePanel.Accounts);

            case PhonePanel.Income:
            case PhonePanel.Expenses:
                return horizontal ? _screen.Open : null;

            case PhonePanel.Budget:
                if (horizontal)
                {
                    return null;
                }

                if (!OnSheet(_start))
                {
                    return PhonePanel.Budget;
                }

                // Up grows the sheet until it is full, then scrolls. Down scrolls back to the top
                // first, and only then pulls the sheet down (§12: a list scrolls first).
                return dy < 0
                    ? (_budget.Value < SheetFull - 1 ? PhonePanel.Budget : null)
                    : (ScrolledTo(top: true) ? PhonePanel.Budget : null);

            case PhonePanel.Accounts:
                if (horizontal)
                {
                    return null;
                }

                return dy < 0 && ScrolledTo(top: false) ? PhonePanel.Accounts : null;
        }

        return null;
    }

    /// <summary>How far a movement pulls a panel in: each panel opens in its own direction.</summary>
    private static double Along(PhonePanel panel, double dx, double dy) => panel switch
    {
        PhonePanel.Income => dx,
        PhonePanel.Expenses => -dx,
        PhonePanel.Budget => -dy,
        _ => dy,
    };

    private bool OnSheet(Point point) => _screen.Open == PhonePanel.Budget && point.Y >= H - _budget.Value;

    private bool ScrolledTo(bool top)
    {
        var scroller = (_pressSource as Visual)?.FindAncestorOfType<ScrollViewer>(includeSelf: true);
        if (scroller is null)
        {
            return true;
        }

        return top
            ? scroller.Offset.Y <= 0.5
            : scroller.Offset.Y >= scroller.Extent.Height - scroller.Viewport.Height - 0.5;
    }

    private static bool IsInButton(object? source) =>
        source is Visual visual && (visual is Button || visual.FindAncestorOfType<Button>() is not null
            || visual is TextBox || visual.FindAncestorOfType<TextBox>() is not null);

    /// <summary>The fling speed along the panel's direction, in pixels a second.</summary>
    private double Velocity()
    {
        if (_samples.Count < 2)
        {
            return 0;
        }

        var last = _samples[^1];
        var first = _samples.FirstOrDefault(s => last.Time - s.Time <= 100, last);
        var ms = (double)(last.Time - first.Time);
        return ms <= 0 ? 0 : (last.Value - first.Value) / ms * 1000;
    }

    private static double Nearest(IEnumerable<double> values, double to) => values.MinBy(v => Math.Abs(v - to));

    /// <summary>
    /// Android's back button: a pop-up first, which is the view's own; then whatever
    /// <see cref="PhoneScreen.Back"/> decides — the question, the budget's category, a step, the panel.
    /// </summary>
    private void OnBack(object? sender, RoutedEventArgs e)
    {
        if (ModalLayer.IsVisible)
        {
            CloseModal();
            e.Handled = true;
            return;
        }

        if (_screen.Back())
        {
            e.Handled = true;
        }
    }

    private void KeyboardChanged(double height)
    {
        _keyboard = height;
        foreach (var form in new[] { IncomeFormPanel, ExpenseFormPanel, BudgetSheet, AccountHistory })
        {
            if (form.Child is Grid grid && grid.Children.OfType<ScrollViewer>().FirstOrDefault() is { } scroller)
            {
                scroller.Padding = new Thickness(0, 0, 0, height);
            }
        }

        if (_modal is { } modal && _modalFromBottom)
        {
            modal.Margin = new Thickness(0, 0, 0, height);
        }

        PlaceToast();
    }
}
