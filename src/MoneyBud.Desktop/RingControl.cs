using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using MoneyBud.Presentation;

namespace MoneyBud.Desktop;

/// <summary>
/// Draws the ring from <see cref="Presentation.Ring"/>'s shares, clockwise from the top, and works
/// nothing out for itself (arc42 §12, *The overview, and its ring*).
///
/// <para>Each slice is drawn as a pale band the size of its budget, with the part that has been
/// spent laid over it in full colour, so what is left of the plan is the pale part. An overspent
/// slice is full and gets an outer edge in the marker's colour. <i>Unassigned</i> is the grey last
/// slice. An empty ring is a grey outline; the hint is text laid over it by the window.</para>
///
/// <para>Pointing: the control turns where the pointer is into a share of the ring, or nothing
/// when it is off the band, and hands it to <see cref="MoneyBudApp.PointAt"/>, which decides which
/// slice that is. The slice it names comes back as <see cref="Pointed"/> and is drawn
/// highlighted.</para>
/// </summary>
public sealed class RingControl : Control
{
    public static readonly StyledProperty<Ring?> RingProperty =
        AvaloniaProperty.Register<RingControl, Ring?>(nameof(Ring));

    public static readonly StyledProperty<RingSlice?> PointedProperty =
        AvaloniaProperty.Register<RingControl, RingSlice?>(nameof(Pointed));

    public static readonly StyledProperty<MoneyBudApp?> AppProperty =
        AvaloniaProperty.Register<RingControl, MoneyBudApp?>(nameof(App));

    static RingControl() => AffectsRender<RingControl>(RingProperty, PointedProperty);

    public Ring? Ring
    {
        get => GetValue(RingProperty);
        set => SetValue(RingProperty, value);
    }

    public RingSlice? Pointed
    {
        get => GetValue(PointedProperty);
        set => SetValue(PointedProperty, value);
    }

    public MoneyBudApp? App
    {
        get => GetValue(AppProperty);
        set => SetValue(AppProperty, value);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        App?.PointAt(ShareAt(e.GetPosition(this)));
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        App?.PointAt(null);
    }

    /// <summary>The share of the ring, clockwise from the top, under a point on the band; null off it.</summary>
    private double? ShareAt(Point point)
    {
        var (centre, outer, inner) = Measures();
        var dx = point.X - centre.X;
        var dy = point.Y - centre.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (outer <= 0 || distance < inner || distance > outer + HighlightEdge) return null;

        var share = (Math.Atan2(dy, dx) + Math.PI / 2) / (2 * Math.PI);
        return share < 0 ? share + 1 : share;
    }

    private (Point Centre, double Outer, double Inner) Measures()
    {
        var size = Math.Min(Bounds.Width, Bounds.Height);
        var outer = size / 2 - 10;
        return (new Point(Bounds.Width / 2, Bounds.Height / 2), outer, outer * (1 - Thickness));
    }

    private const double Thickness = 0.24;

    // How far the slice pointed at stands out past the ring; still part of the slice to point at.
    private const double HighlightEdge = 4;
    private const double GapShare = 0.004;

    public override void Render(DrawingContext context)
    {
        // Transparent, but drawn, so the whole control answers the pointer: the gaps and the
        // centre as well as the band, which is how pointing at nothing is noticed.
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));

        var (centre, outer, inner) = Measures();
        if (outer <= 0) return;

        if (Ring is not { IsEmpty: false } ring)
        {
            var pen = new Pen(new SolidColorBrush(SliceColours.Empty), 2);
            context.DrawEllipse(null, pen, centre, outer, outer);
            context.DrawEllipse(null, pen, centre, inner, inner);
            return;
        }

        var gap = ring.Slices.Count > 1 ? GapShare : 0;

        for (var i = 0; i < ring.Slices.Count; i++)
        {
            var slice = ring.Slices[i];
            var colour = slice.IsUnassigned ? SliceColours.Unassigned : SliceColours.ForSlice(i);
            var start = slice.Start + gap / 2;
            var sweep = Math.Max(slice.Sweep - gap, 0);
            var pointed = slice == Pointed;
            var edge = pointed ? HighlightEdge : 0;

            context.DrawGeometry(new SolidColorBrush(colour, pointed ? 0.45 : 0.28), null,
                Band(centre, outer + edge, inner, start, sweep));

            if (slice.FilledShare > 0)
                context.DrawGeometry(new SolidColorBrush(colour), null,
                    Band(centre, outer + edge, inner, start, sweep * slice.FilledShare));

            if (slice.Marker == Marker.Over)
                context.DrawGeometry(SliceColours.OverBrush, null,
                    Band(centre, outer + edge + 5, outer + edge + 1, start, sweep));
        }
    }

    /// <summary>A band of the ring between two radii, from one share of the circle to another.</summary>
    private static Geometry Band(Point centre, double outer, double inner, double start, double sweep)
    {
        if (sweep >= 0.9999)
        {
            return new CombinedGeometry(GeometryCombineMode.Exclude,
                new EllipseGeometry(new Rect(centre.X - outer, centre.Y - outer, outer * 2, outer * 2)),
                new EllipseGeometry(new Rect(centre.X - inner, centre.Y - inner, inner * 2, inner * 2)));
        }

        var from = Angle(start);
        var to = Angle(start + sweep);
        var large = sweep > 0.5;

        var geometry = new StreamGeometry();
        using (var figure = geometry.Open())
        {
            figure.BeginFigure(On(centre, outer, from), isFilled: true);
            figure.ArcTo(On(centre, outer, to), new Size(outer, outer), 0, large, SweepDirection.Clockwise);
            figure.LineTo(On(centre, inner, to));
            figure.ArcTo(On(centre, inner, from), new Size(inner, inner), 0, large, SweepDirection.CounterClockwise);
            figure.EndFigure(isClosed: true);
        }

        return geometry;
    }

    // Zero is the top; screen y points down, so increasing angles run clockwise.
    private static double Angle(double share) => -Math.PI / 2 + share * 2 * Math.PI;

    private static Point On(Point centre, double radius, double angle) =>
        new(centre.X + radius * Math.Cos(angle), centre.Y + radius * Math.Sin(angle));
}
