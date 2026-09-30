using Avalonia;
using Avalonia.Media;

namespace MoneyBud.Prototype.Themes;

/// <summary>Which way a surface is pulled in, so the painter knows which of its edges are off screen.</summary>
public enum SurfaceSide
{
    Left,
    Right,
    Top,
    Bottom,
    Centre,
}

/// <summary>
/// Draws a panel as a porcelain slab lying on the table: a little smaller than the screen, its
/// edges slightly uneven and chipped, lit from the top left like the plate, with a soft shadow,
/// and two or three gold-mended breaks across it with finer branches. Drawn as plain shapes rather
/// than a shader: a slab is mostly flat, and lines stay sharp at any size.
/// </summary>
public sealed class SlabPainter
{
    /// <summary>How far the slab stands in from the panel's edges; the panel's content keeps inside it.</summary>
    private const double Margin = 8;

    private const double Radius = 22;

    /// <summary>Room kept between the panel's edge and its content, so nothing lies on the table.</summary>
    public Thickness Inset(SurfaceSide side) => side switch
    {
        SurfaceSide.Top => new Thickness(Margin + 2, 0, Margin + 2, Margin + 2),
        SurfaceSide.Bottom => new Thickness(Margin + 2, Margin + 2, Margin + 2, 0),
        SurfaceSide.Centre => new Thickness(0),
        _ => new Thickness(Margin + 2),
    };

    public void Paint(DrawingContext context, Rect bounds, SurfaceSide side, int seed, Color porcelain, Color gold, bool dark)
    {
        // An edge that is off screen is pushed further out, so it never shows.
        var slab = side switch
        {
            SurfaceSide.Top => new Rect(bounds.X + Margin, bounds.Y - 60, bounds.Width - 2 * Margin, bounds.Height + 60 - Margin),
            SurfaceSide.Bottom => new Rect(bounds.X + Margin, bounds.Y + Margin, bounds.Width - 2 * Margin, bounds.Height + 60 - Margin),
            SurfaceSide.Centre => bounds,
            _ => bounds.Deflate(Margin),
        };
        if (slab.Width < 4 * Radius || slab.Height < 4 * Radius)
        {
            return;
        }

        var random = new Random(seed);
        var outline = Outline(slab, random);
        var shape = Path(outline, closed: true);

        // A soft shadow on the table, cast away from the light.
        context.DrawRectangle(Brushes.Transparent, null, new RoundedRect(slab, Radius),
            new BoxShadows(new BoxShadow { OffsetX = 4, OffsetY = 9, Blur = 26, Color = Color.FromArgb((byte)(dark ? 170 : 90), 0, 0, 0) }));

        // Porcelain, lit from the top left, with a faint sheen across it.
        var lightSide = Mix(porcelain, Colors.White, dark ? 0.07 : 0.4);
        var shadowSide = Mix(porcelain, Colors.Black, dark ? 0.3 : 0.05);
        context.DrawGeometry(new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops = { new GradientStop(lightSide, 0), new GradientStop(shadowSide, 1) },
        }, null, shape);
        context.DrawGeometry(new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.1, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.9, 0.55, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.FromArgb(0, 255, 255, 255), 0.35),
                new GradientStop(Color.FromArgb((byte)(dark ? 16 : 40), 255, 255, 255), 0.5),
                new GradientStop(Color.FromArgb(0, 255, 255, 255), 0.65),
            },
        }, null, shape);

        // The gold: breaks right across the slab, and branches off them.
        using (context.PushGeometryClip(shape))
        {
            // Two or three across a panel; a small slab, a pop-up, gets one or two.
            var breaks = slab.Width * Math.Min(slab.Height, 900) < 250_000 ? 1 + random.Next(2) : 2 + random.Next(2);
            for (var i = 0; i < breaks; i++)
            {
                PaintBreak(context, outline, random, gold);
            }
        }

        // The rounded edge catches the light on the top left and falls into shadow on the bottom right.
        context.DrawGeometry(null, new Pen(new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.FromArgb((byte)(dark ? 90 : 230), 255, 255, 255), 0),
                new GradientStop(Color.FromArgb((byte)(dark ? 140 : 70), 0, 0, 0), 1),
            },
        }, 2.2), shape);
    }

    /// <summary>
    /// The slab's outline: a rounded rectangle walked round in small steps, each point nudged a
    /// little in or out, with here and there a chip taken out of the edge.
    /// </summary>
    private static List<Point> Outline(Rect slab, Random random)
    {
        var points = new List<Point>();
        var corners = new[]
        {
            (Centre: new Point(slab.Right - Radius, slab.Top + Radius), From: -Math.PI / 2),
            (Centre: new Point(slab.Right - Radius, slab.Bottom - Radius), From: 0.0),
            (Centre: new Point(slab.Left + Radius, slab.Bottom - Radius), From: Math.PI / 2),
            (Centre: new Point(slab.Left + Radius, slab.Top + Radius), From: Math.PI),
        };
        var starts = new[]
        {
            new Point(slab.Left + Radius, slab.Top),
            new Point(slab.Right, slab.Top + Radius),
            new Point(slab.Right - Radius, slab.Bottom),
            new Point(slab.Left, slab.Bottom - Radius),
        };

        for (var side = 0; side < 4; side++)
        {
            var from = starts[side];
            var corner = corners[side];
            var to = new Point(corner.Centre.X + Radius * Math.Cos(corner.From), corner.Centre.Y + Radius * Math.Sin(corner.From));
            var length = Distance(from, to);
            var steps = Math.Max(1, (int)(length / 12));
            for (var k = 0; k < steps; k++)
            {
                var t = k / (double)steps;
                points.Add(new Point(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t));
            }

            for (var k = 0; k < 6; k++)
            {
                var a = corner.From + Math.PI / 2 * k / 6;
                points.Add(new Point(corner.Centre.X + Radius * Math.Cos(a), corner.Centre.Y + Radius * Math.Sin(a)));
            }
        }

        // Nudge each point along the outward direction: a slow wander, a little grain, and chips.
        var count = points.Count;
        var wander = Enumerable.Range(0, count / 6 + 2).Select(_ => random.NextDouble() - 0.5).ToArray();
        var chips = Enumerable.Range(0, 3 + random.Next(3)).Select(_ => (At: random.Next(count), Width: 2 + random.Next(3), Depth: 2.5 + random.NextDouble() * 3)).ToArray();
        var nudged = new List<Point>(count);
        for (var k = 0; k < count; k++)
        {
            var before = points[(k - 1 + count) % count];
            var after = points[(k + 1) % count];
            var along = after - before;
            var length = Math.Sqrt(along.X * along.X + along.Y * along.Y);
            var outward = length > 0 ? new Vector(along.Y / length, -along.X / length) : default;

            var w = k / 6.0;
            var i = (int)w;
            var f = w - i;
            f = f * f * (3 - 2 * f);
            var offset = (wander[i] + (wander[i + 1] - wander[i]) * f) * 3.2 + (random.NextDouble() - 0.5) * 0.8;
            foreach (var chip in chips)
            {
                var d = Math.Abs(k - chip.At);
                if (d <= chip.Width)
                {
                    offset -= chip.Depth * (1 - d / (double)(chip.Width + 1));
                }
            }

            nudged.Add(points[k] + outward * offset);
        }

        return nudged;
    }

    /// <summary>A gold-mended break from one edge of the slab to another, wandering, with branches.</summary>
    private static void PaintBreak(DrawingContext context, List<Point> outline, Random random, Color gold)
    {
        var count = outline.Count;
        var start = random.Next(count);
        var end = (start + count * (35 + random.Next(30)) / 100) % count;
        var from = outline[start];
        var to = outline[end];

        var path = Wander(from, to, random, 14, 16);
        var width = 1.2 + random.NextDouble() * 0.7;
        PaintGold(context, path, gold, k => width * (0.85 + 0.3 * Math.Sin(k * 0.7)));

        // Branches off the break, thinning to a tip.
        for (var b = 0; b < 1 + random.Next(3); b++)
        {
            var at = 2 + random.Next(Math.Max(1, path.Count - 4));
            var direction = path[Math.Min(at + 1, path.Count - 1)] - path[at - 1];
            var angle = Math.Atan2(direction.Y, direction.X) + (random.Next(2) == 0 ? 1 : -1) * (0.6 + random.NextDouble() * 0.5);
            var length = 25 + random.NextDouble() * 60;
            var tip = path[at] + new Vector(Math.Cos(angle), Math.Sin(angle)) * length;
            var branch = Wander(path[at], tip, random, 10, 6);
            PaintGold(context, branch, gold, k => width * 0.8 * (1 - k / (double)branch.Count) + 0.15);
        }
    }

    /// <summary>Points from one place to another every few pixels, drifting sideways as a crack does.</summary>
    private static List<Point> Wander(Point from, Point to, Random random, double step, double drift)
    {
        var length = Distance(from, to);
        var steps = Math.Max(2, (int)(length / step));
        var across = length > 0 ? new Vector(-(to.Y - from.Y) / length, (to.X - from.X) / length) : default;
        var points = new List<Point>(steps + 1);
        var side = 0.0;
        for (var k = 0; k <= steps; k++)
        {
            var t = k / (double)steps;
            side = Math.Clamp(side + (random.NextDouble() - 0.5) * drift * 0.5, -drift, drift);
            var pull = Math.Sin(Math.PI * t);
            var jitter = (random.NextDouble() - 0.5) * 2.4;
            points.Add(new Point(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t) + across * ((side + jitter) * pull));
        }

        return points;
    }

    private static void PaintGold(DrawingContext context, List<Point> points, Color gold, Func<int, double> width)
    {
        var shine = Mix(gold, Colors.White, 0.5);
        for (var k = 1; k < points.Count; k++)
        {
            var w = width(k);
            context.DrawLine(new Pen(new SolidColorBrush(gold), w, lineCap: PenLineCap.Round), points[k - 1], points[k]);
            context.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(170, shine.R, shine.G, shine.B)), w * 0.35, lineCap: PenLineCap.Round),
                points[k - 1] + new Vector(-0.3, -0.3), points[k] + new Vector(-0.3, -0.3));
        }
    }

    private static StreamGeometry Path(List<Point> points, bool closed)
    {
        var geometry = new StreamGeometry();
        using var context = geometry.Open();
        context.BeginFigure(points[0], closed);
        for (var k = 1; k < points.Count; k++)
        {
            context.LineTo(points[k]);
        }

        context.EndFigure(closed);
        return geometry;
    }

    private static double Distance(Point a, Point b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

    private static Color Mix(Color from, Color to, double amount) =>
        Color.FromRgb(
            (byte)Math.Round(from.R + (to.R - from.R) * amount),
            (byte)Math.Round(from.G + (to.G - from.G) * amount),
            (byte)Math.Round(from.B + (to.B - from.B) * amount));
}
