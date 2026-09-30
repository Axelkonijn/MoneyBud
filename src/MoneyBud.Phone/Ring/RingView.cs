using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering;
using MoneyBud.Phone.Motion;

namespace MoneyBud.Phone.Ring;

/// <summary>
/// The ring on the home screen. It draws the slices it is handed, where it is told they are, and
/// reports where the finger is as a share of the ring read clockwise from the top: which slice that
/// is, and what a touch there means, are <c>PhoneScreen</c>'s to decide. The hit area is a pizza, not
/// the coloured band (arc42 §12, <i>Touching the ring</i>): the whole disc and a margin round it, so
/// the far side of a big screen is reached by moving round the centre rather than across it.
/// </summary>
public sealed class RingView : Control, ICustomHitTest
{
    /// <summary>How far outside the band a touch still counts as the ring's.</summary>
    public const double TouchMargin = 20;

    private IReadOnlyList<DrawnSlice> _slices = [];
    private (double Start, double Sweep)[] _angles = [];
    private (double Start, double Sweep)[] _fromAngles = [];
    private double[] _filled = [];
    private double[] _fromFilled = [];
    private double _change = 1;
    private double[] _emphasis = [];
    private FrameLoop? _emphasisLoop;
    private FrameLoop? _changeLoop;
    private double _reveal = 1;
    private double _fill = 1;
    private double _mend = 1;
    private int _selected = -1;

    private bool _touching;
    private bool _slid;
    private int _underFinger = -1;
    private Point _pressedAt;

    /// <summary>The finger went down on the ring: the share there, or null at its centre.</summary>
    public event Action<double?>? Pressed;

    /// <summary>The finger slid over the ring.</summary>
    public event Action<double?>? Slid;

    /// <summary>The finger lifted: the share it lifted at, and whether it slid on the way.</summary>
    public event Action<double?, bool>? Lifted;

    /// <summary>Called when the finger crosses into another slice, for a tick in the hand.</summary>
    public Action? Tick { get; set; }

    public RingPainter Painter { get; set; } = new DefaultRingPainter();

    public double Reveal
    {
        get => _reveal;
        set
        {
            _reveal = value;
            InvalidateVisual();
        }
    }

    public double Fill
    {
        get => _fill;
        set
        {
            _fill = value;
            InvalidateVisual();
        }
    }

    public double Mend
    {
        get => _mend;
        set
        {
            _mend = value;
            InvalidateVisual();
        }
    }

    /// <summary>Which slice is shown as chosen, or -1. Set from outside; it lifts smoothly.</summary>
    public int Selected
    {
        get => _selected;
        set
        {
            if (value == _selected)
            {
                return;
            }

            _selected = value;
            AnimateEmphasis();
        }
    }

    public bool IsTouching => _touching;

    public Point Centre => new(Bounds.Width / 2, Bounds.Height / 2);

    public double Outer => Math.Max(0, Math.Min(Bounds.Width, Bounds.Height) / 2 - 16);

    public double Inner => Outer * 0.74;

    /// <summary>
    /// Shows new slices, at the shares of the ring they are handed. When the same slices are shown in
    /// the same order, it moves from the old sizes to the new ones, so an entry visibly grows its
    /// slice; otherwise it just redraws.
    /// </summary>
    public void Show(IReadOnlyList<DrawnSlice> slices, IReadOnlyList<(double Start, double Sweep)> shares, bool animate)
    {
        var same = animate && slices.Count == _slices.Count && slices.Select(s => (s.Name, s.IsUnassigned)).SequenceEqual(_slices.Select(s => (s.Name, s.IsUnassigned)));
        _fromAngles = same ? ShownAngles().ToArray() : [];
        _fromFilled = same ? ShownFilled() : [];

        _slices = slices;
        _angles = shares.Select(s => (s.Start * 2 * Math.PI, s.Sweep * 2 * Math.PI)).ToArray();
        _filled = slices.Select(s => s.Filled).ToArray();
        _emphasis = new double[slices.Count];
        if (_selected >= slices.Count)
        {
            _selected = -1;
        }

        if (_selected >= 0)
        {
            _emphasis[_selected] = 1;
        }

        _changeLoop?.Stop();
        if (same)
        {
            _changeLoop = Tween.Run(this, 0.7, Ease.OutCubic, t =>
            {
                _change = t;
                InvalidateVisual();
            });
        }
        else
        {
            _change = 1;
            InvalidateVisual();
        }
    }

    /// <summary>Whether a point, in the ring's own coordinates, belongs to the ring's touch zone.</summary>
    public bool IsInTouchZone(Point point) => Outer > 0 && Distance(point) <= Outer + TouchMargin;

    /// <summary>The whole pizza is touchable, not only where colour is drawn.</summary>
    public bool HitTest(Point point) => IsInTouchZone(point);

    protected override Size MeasureOverride(Size availableSize)
    {
        var side = Math.Min(availableSize.Width, availableSize.Height);
        return double.IsInfinity(side) ? new Size(320, 320) : new Size(side, side);
    }

    public override void Render(DrawingContext context)
    {
        if (Outer <= 0)
        {
            return;
        }

        // Every theme colour is read here, on the UI thread: on the phone a painter's own drawing
        // may run on the render thread, where reading one throws (arc42 §8.5).
        Painter.Paint(context, new RingFrame
        {
            Centre = Centre,
            Outer = Outer,
            Inner = Inner,
            Slices = _change >= 1 ? _slices : _slices.Select((s, i) => s with { Filled = ShownFilled()[i] }).ToArray(),
            Angles = ShownAngles(),
            Emphasis = _emphasis,
            Reveal = _reveal,
            Fill = _fill,
            Mend = _mend,
            SliceColour = i => Colour("Slice" + i % 8),
            Unassigned = Colour("UnassignedColor"),
            Danger = Colour("DangerColor"),
            Line = Colour("LineColor"),
            Named = Colour,
            Invalidate = InvalidateVisual,
        });
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var point = e.GetPosition(this);
        if (!IsInTouchZone(point))
        {
            return;
        }

        e.Pointer.Capture(this);
        e.Handled = true;
        _touching = true;
        _slid = false;
        _pressedAt = point;
        _underFinger = IndexAt(point);
        if (_underFinger >= 0)
        {
            Tick?.Invoke();
        }

        Pressed?.Invoke(ShareAt(point));
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_touching)
        {
            return;
        }

        var point = e.GetPosition(this);
        if (Math.Abs(point.X - _pressedAt.X) + Math.Abs(point.Y - _pressedAt.Y) > 10)
        {
            _slid = true;
        }

        var index = IndexAt(point);
        if (index >= 0 && index != _underFinger)
        {
            _underFinger = index;
            Tick?.Invoke();
        }

        Slid?.Invoke(ShareAt(point));
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!_touching)
        {
            return;
        }

        _touching = false;
        e.Pointer.Capture(null);
        e.Handled = true;
        Lifted?.Invoke(ShareAt(e.GetPosition(this)), _slid);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e) => _touching = false;

    private void AnimateEmphasis()
    {
        if (_emphasisLoop?.IsRunning == true)
        {
            return;
        }

        _emphasisLoop = FrameLoop.Start(this, dt =>
        {
            var k = 1 - Math.Exp(-dt * 16);
            var busy = false;
            for (var i = 0; i < _emphasis.Length; i++)
            {
                var target = i == _selected ? 1.0 : 0.0;
                _emphasis[i] += (target - _emphasis[i]) * k;
                if (Math.Abs(target - _emphasis[i]) > 0.002)
                {
                    busy = true;
                }
                else
                {
                    _emphasis[i] = target;
                }
            }

            InvalidateVisual();
            return busy;
        });
    }

    /// <summary>The share of the ring in the direction of a point, or null right at the centre.</summary>
    private double? ShareAt(Point point)
    {
        if (Distance(point) < Inner * 0.22)
        {
            return null;
        }

        var angle = Math.Atan2(point.X - Centre.X, -(point.Y - Centre.Y));
        if (angle < 0)
        {
            angle += 2 * Math.PI;
        }

        return angle / (2 * Math.PI);
    }

    // Only for the tick in the hand: which drawn slice the finger is over.
    private int IndexAt(Point point)
    {
        if (ShareAt(point) is not { } share || _angles.Length == 0)
        {
            return -1;
        }

        var angle = share * 2 * Math.PI;
        for (var i = 0; i < _angles.Length; i++)
        {
            if (angle < _angles[i].Start + _angles[i].Sweep)
            {
                return i;
            }
        }

        return _angles.Length - 1;
    }

    private double Distance(Point point) =>
        Math.Sqrt(Math.Pow(point.X - Centre.X, 2) + Math.Pow(point.Y - Centre.Y, 2));

    private IReadOnlyList<(double Start, double Sweep)> ShownAngles()
    {
        if (_change >= 1 || _fromAngles.Length != _angles.Length)
        {
            return _angles;
        }

        return _angles.Select((a, i) => (Ease.Lerp(_fromAngles[i].Start, a.Start, _change), Ease.Lerp(_fromAngles[i].Sweep, a.Sweep, _change))).ToArray();
    }

    private double[] ShownFilled() =>
        _change >= 1 || _fromFilled.Length != _filled.Length
            ? _filled
            : _filled.Select((f, i) => Ease.Lerp(_fromFilled[i], f, _change)).ToArray();

    private Color Colour(string key) =>
        this.TryFindResource(key, ActualThemeVariant, out var value) && value is Color colour ? colour : Colors.Gray;
}
