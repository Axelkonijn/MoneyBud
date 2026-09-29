using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering;
using MoneyBud.Prototype.Motion;

namespace MoneyBud.Prototype.Ring;

/// <summary>
/// The ring on the home screen. Holding a finger on it and sliding picks the slice in that
/// direction: the hit area is a pizza, not the coloured band, so the far side of a big screen is
/// reached by moving around the centre rather than across it.
/// </summary>
public sealed class RingView : Control, ICustomHitTest
{
    /// <summary>The smallest share of the circle any slice gets, as on the desktop.</summary>
    private const double MinimumShare = 0.02;

    /// <summary>How far outside the band a touch still counts as the ring's.</summary>
    public const double TouchMargin = 20;

    private IReadOnlyList<RingSlice> _slices = [];
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
    private int _selected = -1;

    private bool _scrubbing;
    private bool _moved;
    private int _selectedAtPress;
    private Point _pressedAt;

    /// <summary>Raised whenever the pointed-at slice changes; -1 is none.</summary>
    public event EventHandler<int>? SelectedChanged;

    /// <summary>Called when the finger crosses into another slice, for a tick in the hand.</summary>
    public Action? Tick { get; set; }

    public RingPainter Painter { get; set; } = new DefaultRingPainter();

    public IReadOnlyList<RingSlice> Slices => _slices;

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

    public int Selected
    {
        get => _selected;
        set => Select(value, tick: false);
    }

    public string? SelectedName => _selected >= 0 && _selected < _slices.Count ? _slices[_selected].Name : null;

    public Point Centre => new(Bounds.Width / 2, Bounds.Height / 2);

    public double Outer => Math.Max(0, Math.Min(Bounds.Width, Bounds.Height) / 2 - 16);

    public double Inner => Outer * 0.74;

    /// <summary>
    /// Shows new slices. When the same slices are shown in the same order, it moves from the old
    /// sizes to the new ones, so an entry visibly grows its slice; otherwise it just redraws.
    /// </summary>
    public void Show(IReadOnlyList<RingSlice> slices, bool animate)
    {
        var keep = SelectedName;
        var same = animate && slices.Count == _slices.Count && slices.Select(s => s.Name).SequenceEqual(_slices.Select(s => s.Name));
        _fromAngles = same ? ShownAngles().ToArray() : [];
        _fromFilled = same ? ShownFilled() : [];

        _slices = slices;
        _angles = Angles(slices);
        _filled = slices.Select(s => s.Filled).ToArray();
        _emphasis = new double[slices.Count];
        _selected = keep is null ? -1 : IndexOf(keep);
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

        if (keep is not null && _selected < 0)
        {
            SelectedChanged?.Invoke(this, -1);
        }
    }

    public int IndexOf(string name)
    {
        for (var i = 0; i < _slices.Count; i++)
        {
            if (_slices[i].Name == name)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Whether a point, in the ring's own coordinates, belongs to the ring's touch zone.</summary>
    public bool IsInTouchZone(Point point) => _slices.Count > 0 && Distance(point) <= Outer + TouchMargin;

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
            SliceColour = i => Colour("Slice" + i % 8),
            Unassigned = Colour("UnassignedColor"),
            Danger = Colour("DangerColor"),
            Line = Colour("LineColor"),
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
        _scrubbing = true;
        _moved = false;
        _pressedAt = point;
        _selectedAtPress = _selected;
        if (SliceAt(point) is { } index)
        {
            Select(index, tick: true);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_scrubbing)
        {
            return;
        }

        var point = e.GetPosition(this);
        if (Math.Abs(point.X - _pressedAt.X) + Math.Abs(point.Y - _pressedAt.Y) > 10)
        {
            _moved = true;
        }

        if (SliceAt(point) is { } index)
        {
            Select(index, tick: true);
        }

        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!_scrubbing)
        {
            return;
        }

        _scrubbing = false;
        e.Pointer.Capture(null);
        e.Handled = true;

        // A tap on the slice already shown, or on the centre, goes back to Niet toegewezen.
        var point = e.GetPosition(this);
        if (!_moved && (SliceAt(point) is null || _selectedAtPress == _selected))
        {
            Select(-1, tick: false);
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e) => _scrubbing = false;

    private void Select(int index, bool tick)
    {
        if (index == _selected)
        {
            return;
        }

        _selected = index;
        if (tick && index >= 0)
        {
            Tick?.Invoke();
        }

        SelectedChanged?.Invoke(this, index);
        AnimateEmphasis();
    }

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

    /// <summary>The slice in the direction of a point, or null right at the centre.</summary>
    private int? SliceAt(Point point)
    {
        if (_slices.Count == 0 || Distance(point) < Inner * 0.22)
        {
            return null;
        }

        var dx = point.X - Centre.X;
        var dy = point.Y - Centre.Y;
        var angle = Math.Atan2(dx, -dy);
        if (angle < 0)
        {
            angle += 2 * Math.PI;
        }

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

    /// <summary>
    /// Each slice's share, sized by its figure, with the smallest widened to the minimum and the
    /// rest giving way in proportion.
    /// </summary>
    private static (double Start, double Sweep)[] Angles(IReadOnlyList<RingSlice> slices)
    {
        var count = slices.Count;
        if (count == 0)
        {
            return [];
        }

        var weights = slices.Select(s => (double)Math.Max(0, s.Size)).ToArray();
        var shares = new double[count];
        var total = weights.Sum();
        if (total <= 0 || count * MinimumShare >= 1)
        {
            Array.Fill(shares, 1.0 / count);
        }
        else
        {
            var widened = new bool[count];
            while (true)
            {
                var free = 1 - MinimumShare * widened.Count(w => w);
                var freeWeight = weights.Where((_, i) => !widened[i]).Sum();
                var changed = false;
                for (var i = 0; i < count; i++)
                {
                    if (widened[i])
                    {
                        shares[i] = MinimumShare;
                        continue;
                    }

                    shares[i] = weights[i] / freeWeight * free;
                    if (shares[i] < MinimumShare)
                    {
                        widened[i] = true;
                        changed = true;
                    }
                }

                if (!changed)
                {
                    break;
                }
            }
        }

        var result = new (double, double)[count];
        var start = 0.0;
        for (var i = 0; i < count; i++)
        {
            var sweep = shares[i] * 2 * Math.PI;
            result[i] = (start, sweep);
            start += sweep;
        }

        return result;
    }

    private Color Colour(string key) =>
        this.TryFindResource(key, ActualThemeVariant, out var value) && value is Color colour ? colour : Colors.Gray;
}
