using Avalonia;
using Avalonia.Controls;
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
/// </summary>
public sealed class RingControl : Control
{
    public static readonly StyledProperty<Ring?> RingProperty =
        AvaloniaProperty.Register<RingControl, Ring?>(nameof(Ring));

    static RingControl() => AffectsRender<RingControl>(RingProperty);

    public Ring? Ring
    {
        get => GetValue(RingProperty);
        set => SetValue(RingProperty, value);
    }

    private const double Thickness = 0.28;
    private const double GapShare = 0.004;

    public override void Render(DrawingContext context)
    {
        var size = Math.Min(Bounds.Width, Bounds.Height);
        if (size <= 0) return;

        var centre = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var outer = size / 2 - 6;
        var inner = outer * (1 - Thickness);

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
            var sweep = Math.Max(slice.Sweep - gap, 0.0005);

            context.DrawGeometry(new SolidColorBrush(colour, 0.28), null, Band(centre, outer, inner, start, sweep));

            if (slice.FilledShare > 0)
                context.DrawGeometry(new SolidColorBrush(colour), null,
                    Band(centre, outer, inner, start, sweep * slice.FilledShare));

            if (slice.Marker == Marker.Over)
                context.DrawGeometry(SliceColours.OverBrush, null,
                    Band(centre, outer + 5, outer + 1, start, sweep));
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
