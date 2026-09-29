using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MoneyBud.Prototype.Motion;
using MoneyBud.Prototype.Platform;
using MoneyBud.Prototype.Ring;
using MoneyBud.Prototype.Sample;

namespace MoneyBud.Prototype.Views;

public enum Side
{
    None,
    Income,
    Expense,
    Budget,
    Accounts,
}

/// <summary>
/// The shell: where each layer is, and the gestures that move them. Each side has one number —
/// how far it is pulled in — driven by a spring, and everything else (the panels' positions, how
/// much the home screen shrinks and blurs) is worked out from those numbers every frame.
/// </summary>
public sealed partial class MainView : UserControl
{
    /// <summary>How far a finger moves before it counts as a swipe rather than a tap.</summary>
    private const double Slop = 10;

    private readonly SampleLedger _ledger = new(DateOnly.FromDateTime(DateTime.Today));
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

    private int _offset;
    private Side _open;
    private Thickness _safe;
    private Size _laidOut;
    private bool _opened;

    private Track _track;
    private Point _start;
    private Side _dragSide;
    private double _dragBase;
    private object? _pressSource;
    private readonly List<(ulong Time, double Value)> _samples = [];

    public MainView()
    {
        InitializeComponent();
        Focusable = true;

        _income = new Spring(this, _ => ApplyIncome());
        _expense = new Spring(this, _ => ApplyExpense());
        _budget = new Spring(this, _ => ApplyBudget());
        _accounts = new Spring(this, _ => ApplyAccounts());
        foreach (var (spring, side) in new[] { (_income, Side.Income), (_expense, Side.Expense), (_budget, Side.Budget), (_accounts, Side.Accounts) })
        {
            spring.Settled += value =>
            {
                if (value <= 0.5 && _open == side)
                {
                    Closed(side);
                }
            };
        }

        IncomeList.RenderTransform = _incomeListShift;
        IncomeForm.RenderTransform = _incomeFormShift;
        ExpenseList.RenderTransform = _expenseListShift;
        ExpenseForm.RenderTransform = _expenseFormShift;
        BudgetSheet.RenderTransform = _sheetShift;
        AccountsList.RenderTransform = _accountsShift;
        AccountHistory.RenderTransform = _historyShift;

        PreviousPeriod.Content = Ui.Icon(Ui.ChevronLeft, "Muted");
        NextPeriod.Content = Ui.Icon(Ui.ChevronRight, "Muted");
        PreviousPeriod.Click += (_, _) => StepPeriod(-1);
        NextPeriod.Click += (_, _) => StepPeriod(+1);
        PeriodButton.Click += (_, _) => ShowStartDay();

        Ring.Tick = () => Haptics.Tick();
        Ring.SelectedChanged += (_, _) =>
        {
            ShowHole(countUp: false);
            if (_open == Side.Budget)
            {
                BuildBudget();
            }
        };

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
    }

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

    private double SheetHalf => Math.Round(H * 0.52);

    private double AccountsHeight => Math.Max(0, H - 170);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is not { } top)
        {
            return;
        }

        top.BackRequested += OnBack;
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

        Dispatcher.UIThread.Post(Open, DispatcherPriority.Loaded);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is { } top)
        {
            top.BackRequested -= OnBack;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void ApplySafeArea(Thickness safe)
    {
        _safe = safe;
        HomeGrid.Margin = new Thickness(0, safe.Top, 0, safe.Bottom);
        Toast.Margin = new Thickness(14, 0, 14, safe.Bottom + 14);
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
            // Keep each open side at the same step: a full panel stays full on the new size.
            Rescale(_income, _laidOut.Width, W);
            Rescale(_expense, _laidOut.Width, W);
            Rescale(_accounts, _laidOut.Height - 170, AccountsHeight);
            _budget.Set(Nearest([0, SheetHalf, SheetFull], _budget.Value));
        }

        _laidOut = Bounds.Size;
        ApplyIncome();
        ApplyExpense();
        ApplyBudget();
        ApplyAccounts();
        PaintGlow();

        static void Rescale(Spring spring, double from, double to) =>
            spring.Set(from > 0 ? spring.Value / from * to : 0);
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

    // ---- Where each side is -------------------------------------------------------------------

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
    /// The home screen under whatever is pulled over it: it blurs and darkens, like something seen
    /// through glass. Under a half-open budget sheet it shrinks instead, so the ring and the period
    /// stay readable above the sheet.
    /// </summary>
    private void ApplyHome()
    {
        if (W <= 0 || H <= 0)
        {
            return;
        }

        var over = Math.Max(Math.Max(_income.Value / W, _expense.Value / W), AccountsHeight > 0 ? _accounts.Value / AccountsHeight : 0);
        over = Math.Clamp(over, 0, 1);
        var shrink = Math.Clamp(_budget.Value / SheetHalf, 0, 1);
        var sheetOver = Math.Clamp((_budget.Value - SheetHalf) / Math.Max(1, SheetFull - SheetHalf), 0, 1);
        var blur = Math.Max(over, sheetOver);

        // Only the half-open budget sheet moves the home screen: it shrinks it to fit above. Every
        // other panel lies over a home screen that stays exactly where it is.
        var scale = Ease.Lerp(1, HalfScale(), shrink);
        Home.RenderTransform = scale < 1 ? new MatrixTransform(Matrix.CreateScale(scale, scale)) : null;
        Scrim.Opacity = 0.32 * blur;
        Home.Effect = blur > 0.01 ? new BlurEffect { Radius = 22 * blur } : null;
    }

    /// <summary>How small the home screen gets for the whole ring to fit above a half-open sheet.</summary>
    private double HalfScale()
    {
        var bottom = PeriodBar.TranslatePoint(new Point(0, PeriodBar.Bounds.Height + 6), Home)?.Y ?? H;
        return bottom <= 0 ? 1 : Math.Clamp((H - SheetHalf - 8) / bottom, 0.3, 1);
    }

    private Spring SpringOf(Side side) => side switch
    {
        Side.Income => _income,
        Side.Expense => _expense,
        Side.Budget => _budget,
        _ => _accounts,
    };

    /// <summary>The resting points of a side, in the same units as its spring.</summary>
    private double[] Detents(Side side) => side switch
    {
        Side.Income or Side.Expense => [0, W, 2 * W],
        Side.Budget => [0, SheetHalf, SheetFull],
        _ => _historyAccount is null ? [0, AccountsHeight] : [0, AccountsHeight, 2 * AccountsHeight],
    };

    /// <summary>Moves a side to a resting point, opening it first if it was closed.</summary>
    private void MoveTo(Side side, double target, double velocity = 0)
    {
        if (_open != side && target > 0)
        {
            Opening(side);
        }

        SpringOf(side).AnimateTo(target, velocity);
    }

    private void Opening(Side side)
    {
        if (_open != Side.None && _open != side)
        {
            SpringOf(_open).Set(0);
            Closed(_open);
        }

        _open = side;
        HideHints();
        switch (side)
        {
            case Side.Income:
                BuildEntryList(EntryKind.Income);
                break;
            case Side.Expense:
                BuildEntryList(EntryKind.Expense);
                break;
            case Side.Budget:
                BuildBudget();
                break;
            case Side.Accounts:
                BuildAccounts();
                break;
        }
    }

    private void Closed(Side side)
    {
        if (_open == side)
        {
            _open = Side.None;
        }

        if (side == Side.Accounts)
        {
            _historyAccount = null;
        }

        Unfocus();
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
        var ringVisible = _open == Side.None || (_open == Side.Budget && _budget.Value <= SheetHalf + 1);
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

            if (Decide(Math.Abs(dx) > Math.Abs(dy), dx, dy) is not { } side)
            {
                _track = Track.Ignored;
                return;
            }

            _track = Track.Dragging;
            _dragSide = side;
            var spring = SpringOf(side);
            if (_open != side)
            {
                Opening(side);
            }

            _dragBase = spring.Value;
            spring.Set(_dragBase);
            PrepareSecondStep(side);
            e.Pointer.Capture(this);
        }

        var value = Math.Max(0, _dragBase + Along(_dragSide, dx, dy));
        var top = Detents(_dragSide)[^1];
        if (value > top)
        {
            value = top + (value - top) * 0.25; // Give a little past the end, and resist.
        }

        SpringOf(_dragSide).Set(value);
        _samples.Add((e.Timestamp, Along(_dragSide, point.X, point.Y)));
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
            var spring = SpringOf(_dragSide);
            var velocity = Velocity();
            var detents = Detents(_dragSide);
            // One swipe moves one step — list, then form — except the budget sheet, which a long
            // pull opens all the way.
            var from = Array.IndexOf(detents, Nearest(detents, _dragBase));
            var reachable = _dragSide == Side.Budget ? detents : detents[Math.Max(0, from - 1)..Math.Min(detents.Length, from + 2)];
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
        if (_open == Side.None)
        {
            Ring.Selected = -1;
        }
        else if (_open == Side.Budget && !OnSheet(point))
        {
            MoveTo(Side.Budget, 0);
        }
        else if (_open == Side.Accounts && point.Y > AccountsHeight + 20)
        {
            MoveTo(Side.Accounts, 0);
        }
    }

    private void EndTracking() => _track = Track.None;

    /// <summary>Which side a swipe moves, given where things are; null leaves it to a list's scrolling.</summary>
    private Side? Decide(bool horizontal, double dx, double dy)
    {
        switch (_open)
        {
            case Side.None:
                return horizontal ? (dx > 0 ? Side.Income : Side.Expense) : (dy < 0 ? Side.Budget : Side.Accounts);

            case Side.Income:
            case Side.Expense:
                return horizontal ? _open : null;

            case Side.Budget:
                if (horizontal)
                {
                    return null;
                }

                if (!OnSheet(_start))
                {
                    return Side.Budget;
                }

                // Up grows the sheet until it is full, then scrolls. Down scrolls back to the
                // top first, and only then pulls the sheet down.
                return dy < 0
                    ? (_budget.Value < SheetFull - 1 ? Side.Budget : null)
                    : (ScrolledTo(top: true) ? Side.Budget : null);

            case Side.Accounts:
                if (horizontal)
                {
                    return null;
                }

                return dy < 0 && ScrolledTo(top: false) ? Side.Accounts : null;
        }

        return null;
    }

    /// <summary>How far a movement pulls a side in: each side opens in its own direction.</summary>
    private static double Along(Side side, double dx, double dy) => side switch
    {
        Side.Income => dx,
        Side.Expense => -dx,
        Side.Budget => -dy,
        _ => dy,
    };

    private bool OnSheet(Point point) => _open == Side.Budget && point.Y >= H - _budget.Value;

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

    /// <summary>The fling speed along the side's direction, in pixels a second.</summary>
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

    private void OnBack(object? sender, RoutedEventArgs e)
    {
        if (ModalLayer.IsVisible)
        {
            CloseModal();
            e.Handled = true;
            return;
        }

        if (_open == Side.None)
        {
            return;
        }

        if (_open == Side.Budget && FocusedCategory is not null)
        {
            ShowAllCategories();
            e.Handled = true;
            return;
        }

        var spring = SpringOf(_open);
        var detents = Detents(_open);
        var at = Array.IndexOf(detents, Nearest(detents, spring.Target));
        var back = _open == Side.Budget ? 0 : detents[Math.Max(0, at - 1)];
        spring.AnimateTo(back);
        e.Handled = true;
    }

    private void KeyboardChanged(double height)
    {
        foreach (var form in new[] { IncomeForm, ExpenseForm })
        {
            if (form.Child is Grid { Children: [_, ScrollViewer scroller, ..] })
            {
                scroller.Padding = new Thickness(0, 0, 0, height);
            }
        }
    }
}
