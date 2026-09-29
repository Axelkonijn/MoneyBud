using Avalonia;
using Avalonia.Media;

namespace MoneyBud.Prototype.Ring;

/// <summary>One slice as the ring shows it: its share of the circle is worked out by the ring.</summary>
public sealed record RingSlice(string Name, decimal Size, double Filled, bool IsOver, int Colour, bool IsUnassigned);

/// <summary>Everything a painter needs for one frame, already laid out.</summary>
public sealed class RingFrame
{
    public required Point Centre { get; init; }

    public required double Outer { get; init; }

    public required double Inner { get; init; }

    public required IReadOnlyList<RingSlice> Slices { get; init; }

    /// <summary>Where each slice starts and how far it reaches, in radians clockwise from the top.</summary>
    public required IReadOnlyList<(double Start, double Sweep)> Angles { get; init; }

    /// <summary>Per slice, 0 to 1: how much it is the one being pointed at, animated.</summary>
    public required IReadOnlyList<double> Emphasis { get; init; }

    /// <summary>How much of the circle is drawn yet, 0 to 1, for drawing in and draining away.</summary>
    public required double Reveal { get; init; }

    /// <summary>How far the spent part is filled in yet, 0 to 1.</summary>
    public required double Fill { get; init; }

    public required Func<int, Color> SliceColour { get; init; }

    public required Color Unassigned { get; init; }

    public required Color Danger { get; init; }

    public required Color Line { get; init; }
}

/// <summary>
/// Draws the ring. The theme decides which painter is used, so a theme can change how the ring
/// looks — a brush stroke, a glazed bowl — without touching what the ring means.
/// </summary>
public abstract class RingPainter
{
    public abstract void Paint(DrawingContext context, RingFrame frame);

    protected static Point At(Point centre, double radius, double angle) =>
        new(centre.X + radius * Math.Sin(angle), centre.Y - radius * Math.Cos(angle));

    /// <summary>A band between two radii, from one angle to another, clockwise.</summary>
    protected static Geometry Band(Point centre, double outer, double inner, double from, double to)
    {
        var geometry = new StreamGeometry();
        using var context = geometry.Open();
        var large = to - from > Math.PI;
        context.BeginFigure(At(centre, outer, from), true);
        context.ArcTo(At(centre, outer, to), new Size(outer, outer), 0, large, SweepDirection.Clockwise);
        context.LineTo(At(centre, inner, to));
        context.ArcTo(At(centre, inner, from), new Size(inner, inner), 0, large, SweepDirection.CounterClockwise);
        context.EndFigure(true);
        return geometry;
    }

    protected static Color Faded(Color colour, double opacity) =>
        Color.FromArgb((byte)Math.Clamp(colour.A * opacity, 0, 255), colour.R, colour.G, colour.B);
}

/// <summary>The default look: clean bands with small gaps, the spent part solid over a dim track.</summary>
public sealed class DefaultRingPainter : RingPainter
{
    private const double GapPixels = 3.5;
    private const double Grow = 9;

    public override void Paint(DrawingContext context, RingFrame frame)
    {
        var pointing = frame.Emphasis.Count == 0 ? 0 : frame.Emphasis.Max();
        var revealEnd = frame.Reveal * 2 * Math.PI;
        var middle = (frame.Outer + frame.Inner) / 2;
        var gap = frame.Slices.Count > 1 ? GapPixels / middle : 0;

        // A hairline around the hole, so an empty ring still reads as a ring.
        context.DrawEllipse(null, new Pen(new SolidColorBrush(Faded(frame.Line, frame.Reveal)), 1), frame.Centre, frame.Inner - 14, frame.Inner - 14);

        if (frame.Slices.Count == 0)
        {
            context.DrawEllipse(null, new Pen(new SolidColorBrush(Faded(frame.Unassigned, frame.Reveal)), frame.Outer - frame.Inner), frame.Centre, middle, middle);
            return;
        }

        for (var i = 0; i < frame.Slices.Count; i++)
        {
            var slice = frame.Slices[i];
            var (start, sweep) = frame.Angles[i];
            var from = start + gap / 2;
            var to = start + sweep - gap / 2;
            if (from >= revealEnd || to <= from)
            {
                continue;
            }

            var emphasis = frame.Emphasis[i];
            var outer = frame.Outer + Grow * emphasis;
            var inner = frame.Inner - 2 * emphasis;
            var opacity = 1 - 0.62 * pointing * (1 - emphasis);
            var colour = slice.IsUnassigned ? frame.Unassigned : frame.SliceColour(slice.Colour);
            var shownTo = Math.Min(to, revealEnd);

            if (slice.IsUnassigned)
            {
                Draw(context, frame.Centre, outer, inner, from, shownTo, Faded(colour, opacity));
                continue;
            }

            Draw(context, frame.Centre, outer, inner, from, shownTo, Faded(colour, 0.26 * opacity));
            var filledTo = Math.Min(from + (to - from) * slice.Filled * frame.Fill, shownTo);
            if (filledTo > from)
            {
                Draw(context, frame.Centre, outer, inner, from, filledTo, Faded(colour, opacity));
            }

            if (slice.IsOver && frame.Fill > 0.98)
            {
                // Over budget: a thin rim just outside the slice, in the warning colour.
                Draw(context, frame.Centre, outer + 7, outer + 3.5, from, shownTo, Faded(frame.Danger, opacity));
            }
        }
    }

    private static void Draw(DrawingContext context, Point centre, double outer, double inner, double from, double to, Color colour)
    {
        var brush = new SolidColorBrush(colour);
        if (to - from > 2 * Math.PI - 0.0001)
        {
            var middle = (outer + inner) / 2;
            context.DrawEllipse(null, new Pen(brush, outer - inner), centre, middle, middle);
            return;
        }

        context.DrawGeometry(brush, null, Band(centre, outer, inner, from, to));
    }
}
