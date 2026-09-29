using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace MoneyBud.Prototype.Ring;

/// <summary>
/// Kintsugi: the ring as a porcelain plate lying on the table, seen from above and lit from the
/// top left. Its rim is the band of slices, each a shard; the flat well inside is the hole, where
/// the figures are. A shard is glazed as far as its budget is spent and bare porcelain beyond; Niet
/// toegewezen is raw, unglazed clay. The shards meet in gold-mended breaks, and finer gold veins
/// run through every piece, as in a bowl that broke more than once. An overspent shard's veins are
/// red lacquer, with a lacquered break down its middle. The gold runs right across the bowl, bottom
/// and all, under the figures: the stakeholder's choice over keeping the middle clean. After the
/// ring draws in, the gold runs into the breaks (<see cref="RingFrame.Mend"/>).
/// <para>
/// Drawn in two parts, because one shader doing everything each frame hung the phone's GPU. What
/// never changes for a bowl of a given size — its light, where the veins run, how the breaks
/// wander — is worked out once on the CPU, in the background, into a few textures
/// (<see cref="Bowl"/>). Each frame a small strip describes the slices round the circle
/// (<see cref="Strip"/>), and a light shader only reads the two.
/// </para>
/// </summary>
public sealed class KintsugiRingPainter : RingPainter
{
    /// <summary>Texture pixels per device-independent pixel: the textures are smooth, so this is enough.</summary>
    private const double BakeScale = 2;

    private const int Bins = 4096;

    private static readonly Lazy<SKRuntimeEffect?> Effect = new(() =>
    {
        var effect = SKRuntimeEffect.CreateShader(Sksl, out var errors);
        if (effect is null)
        {
            Console.WriteLine("The kintsugi ring shader does not compile: " + errors);
        }

        return effect;
    });

    private static Bowl? _bowl;
    private static (double Outer, double Inner, bool Dark)? _baking;

    private readonly DefaultRingPainter _fallback = new();

    public override void Paint(DrawingContext context, RingFrame frame)
    {
        // Without the shader — it did not compile on this device — the plain ring, never a blank.
        if (Effect.Value is null)
        {
            _fallback.Paint(context, frame);
            return;
        }

        var body = frame.Named("BowlColor");
        var dark = body.R + body.G + body.B < 3 * 128;
        var key = (Math.Round(frame.Outer, 1), Math.Round(frame.Inner, 1), dark);
        if (_bowl is not { } bowl || bowl.Key != key)
        {
            // The bowl is still being made; the table shows until it is, then the ring draws.
            StartBaking(key, frame.Invalidate);
            return;
        }

        // Everything the render thread needs is read here, on the UI thread: a theme colour cannot be
        // read from the render thread, and on the phone that is where the drawing happens.
        var strip = Strip(frame);
        var reach = bowl.Reach;
        var centre = frame.Centre;
        var (outer, inner, reveal, fill, mend) = (frame.Outer, frame.Inner, frame.Reveal, frame.Fill, frame.Mend);
        var pointing = frame.Emphasis.Count == 0 ? 0 : frame.Emphasis.Max();
        var clay = Colour(frame.Unassigned);
        var seam = Colour(frame.Named("SeamColor"));
        var lacquer = Colour(frame.Named("LacquerColor"));
        context.Custom(new Operation(new Rect(centre.X - reach, centre.Y - reach, 2 * reach, 2 * reach), bowl, strip, uniforms =>
        {
            uniforms["center"] = new[] { (float)centre.X, (float)centre.Y };
            uniforms["outer"] = (float)outer;
            uniforms["inner"] = (float)inner;
            uniforms["reveal"] = (float)reveal;
            uniforms["fillAmount"] = (float)fill;
            uniforms["mend"] = (float)mend;
            uniforms["pointing"] = (float)pointing;
            uniforms["body"] = Colour(body);
            uniforms["clay"] = clay;
            uniforms["gold"] = seam;
            uniforms["lacquer"] = lacquer;
            uniforms["dark"] = dark ? 1f : 0f;
        }));
    }

    private static void StartBaking((double Outer, double Inner, bool Dark) key, Action redraw)
    {
        if (_baking == key)
        {
            return;
        }

        _baking = key;
        Task.Run(() =>
        {
            var bowl = Bowl.Bake(key);
            Dispatcher.UIThread.Post(() =>
            {
                if (_baking == key)
                {
                    // The old bowl is not disposed: the render thread may still be drawing with it.
                    // Its textures go with the garbage collector.
                    _bowl = bowl;
                    _baking = null;
                }
                else
                {
                    bowl.Dispose();
                }

                redraw();
            });
        });
    }

    private static float[] Colour(Color colour) => [colour.R / 255f, colour.G / 255f, colour.B / 255f, 1];

    /// <summary>
    /// What the slices are at each angle round the circle, one column per angle: row 0 the glaze;
    /// row 1 the kind (glaze, clay, none), overspent and pointed at; row 2 how far to the nearest
    /// break and to the middle of the shard; row 3 whether that angle is spent, and how far to
    /// where the glaze stops.
    /// </summary>
    private static SKImage Strip(RingFrame frame)
    {
        var pixels = new byte[Bins * 4 * 4];
        var count = frame.Slices.Count;
        var slice = 0;
        for (var k = 0; k < Bins; k++)
        {
            var angle = (k + 0.5) / Bins * 2 * Math.PI;
            while (slice < count - 1 && angle >= frame.Angles[slice].Start + frame.Angles[slice].Sweep)
            {
                slice++;
            }

            if (count == 0)
            {
                Set(pixels, 0, k, 0, 0, 0);
                Set(pixels, 1, k, 1, 0, 0);
                Set(pixels, 2, k, 1, 1, 0);
                Set(pixels, 3, k, 0, 1, 0);
                continue;
            }

            var s = frame.Slices[slice];
            var (start, sweep) = frame.Angles[slice];
            var glaze = frame.SliceColour(s.Colour);
            Set(pixels, 0, k, glaze.R / 255.0, glaze.G / 255.0, glaze.B / 255.0);
            Set(pixels, 1, k, s.IsUnassigned ? 0.5 : 0, s.IsOver ? 1 : 0, frame.Emphasis[slice]);

            var from = angle - start;
            var toBreak = count > 1 ? Math.Min(from, sweep - from) : 0.25;
            Set(pixels, 2, k, toBreak / 0.25, Math.Abs(from - sweep / 2) / 0.25, 0);

            var spentTo = s.IsUnassigned ? 0 : sweep * s.Filled * frame.Fill;
            Set(pixels, 3, k, from < spentTo ? 1 : 0, spentTo > 0 ? Math.Abs(from - spentTo) / 0.05 : 1, 0);
        }

        return SKImage.FromPixelCopy(new SKImageInfo(Bins, 4, SKColorType.Rgba8888, SKAlphaType.Opaque), pixels);
    }

    private static void Set(byte[] pixels, int row, int column, double r, double g, double b)
    {
        var i = (row * Bins + column) * 4;
        pixels[i] = Byte(r);
        pixels[i + 1] = Byte(g);
        pixels[i + 2] = Byte(b);
        pixels[i + 3] = 255;
    }

    private static byte Byte(double value) => (byte)Math.Clamp(Math.Round(value * 255), 0, 255);

    /// <summary>Hands Skia's canvas to the shader for the ring's square, with its textures and uniforms.</summary>
    private sealed class Operation(Rect bounds, Bowl bowl, SKImage strip, Action<SKRuntimeEffectUniforms> fill) : ICustomDrawOperation
    {
        public Rect Bounds => bounds;

        public bool HitTest(Point p) => false;

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Dispose() => strip.Dispose();

        public void Render(ImmediateDrawingContext context)
        {
            if (context.TryGetFeature<ISkiaSharpApiLeaseFeature>() is not { } feature)
            {
                return;
            }

            using var lease = feature.Lease();
            var canvas = lease.SkCanvas;
            var effect = Effect.Value!;
            var uniforms = new SKRuntimeEffectUniforms(effect);
            fill(uniforms);
            uniforms["px"] = 1f / Math.Max(0.5f, canvas.TotalMatrix.ScaleX);
            uniforms["bins"] = (float)Bins;

            var place = SKMatrix.CreateScaleTranslation((float)(1 / BakeScale), (float)(1 / BakeScale), (float)bounds.Left, (float)bounds.Top);
            var smooth = new SKSamplingOptions(SKFilterMode.Linear);
            using var light = bowl.Light.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, smooth, place);
            using var paths = bowl.Paths.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, smooth, place);
            using var gold = bowl.Gold.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, smooth, place);
            using var surface = bowl.Surface.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, smooth, place);
            using var slices = strip.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Clamp, smooth, SKMatrix.Identity);
            var children = new SKRuntimeEffectChildren(effect)
            {
                ["lightMap"] = light,
                ["pathMap"] = paths,
                ["goldMap"] = gold,
                ["surfaceMap"] = surface,
                ["strip"] = slices,
            };
            using var shader = effect.ToShader(uniforms, children);
            using var paint = new SKPaint { Shader = shader };
            canvas.DrawRect(new SKRect((float)bounds.Left, (float)bounds.Top, (float)bounds.Right, (float)bounds.Bottom), paint);
        }
    }

    /// <summary>
    /// Everything about a bowl that does not change from frame to frame, worked out once per size
    /// and per dark or light. Four textures of three channels each, all smooth enough to be read
    /// between their pixels — a vein is kept as its distance, not as its pixels:
    /// light — how much light falls (with the rim's shadow inside), the glaze's highlight, the
    /// bowl's shadow on the table; paths — how far the breaks and the glaze's edge wander off the
    /// angle, and the distance to the nearest vein; gold — when the gold reaches each vein, how
    /// wide the breaks are there, the clay's grain; surface — the glaze's mottling, and how far the
    /// band's inner edge wanders.
    /// </summary>
    private sealed class Bowl : IDisposable
    {
        private Bowl((double, double, bool) key, double reach, SKImage light, SKImage paths, SKImage gold, SKImage surface)
        {
            Key = key;
            Reach = reach;
            Light = light;
            Paths = paths;
            Gold = gold;
            Surface = surface;
        }

        public (double Outer, double Inner, bool Dark) Key { get; }

        public double Reach { get; }

        public SKImage Light { get; }

        public SKImage Paths { get; }

        public SKImage Gold { get; }

        public SKImage Surface { get; }

        public void Dispose()
        {
            Light.Dispose();
            Paths.Dispose();
            Gold.Dispose();
            Surface.Dispose();
        }

        public static Bowl Bake((double Outer, double Inner, bool Dark) key)
        {
            var (outer, inner, dark) = key;
            var reach = outer * 1.3;
            var size = (int)Math.Ceiling(2 * reach * BakeScale);
            var light = new byte[size * size * 4];
            var paths = new byte[size * size * 4];
            var gold = new byte[size * size * 4];
            var surface = new byte[size * size * 4];

            var l = Normalize(-0.5, -0.72, 0.9);
            var h = Normalize(l.X, l.Y, l.Z + 1);
            var lipWidth = outer * 0.045;
            var rimStart = outer - lipWidth;
            // A plate: a flat well, a soft rise where the well meets the rim, a gently sloping
            // rim that carries the shards, and a rounded lip.
            var wellEdge = inner - 8;
            var rimFoot = inner + 10;
            const double rimSlope = 0.2;
            var rimHeight = outer * 0.07;
            var (ambient, reach2) = dark ? (0.55, 0.55) : (0.52, 0.5);
            const double veinCell = 56;

            Parallel.For(0, size, y =>
            {
                for (var x = 0; x < size; x++)
                {
                    var i = (y * size + x) * 4;
                    // Everything is measured from the centre, so the bowl is the same wherever it stands.
                    var dx = (x + 0.5) / BakeScale - reach;
                    var dy = (y + 0.5) / BakeScale - reach;
                    var r = Math.Sqrt(dx * dx + dy * dy);
                    var theta = Math.Atan2(dx, -dy);
                    if (theta < 0)
                    {
                        theta += 2 * Math.PI;
                    }

                    // ---- The plate's shadow on the table: low, so short and soft.
                    var sx = dx - 0.035 * outer;
                    var sy = dy - 0.05 * outer;
                    var thrown = 1 - SmoothStep(outer * 0.94, outer * 1.16, Math.Sqrt(sx * sx + sy * sy));
                    var contact = 1 - SmoothStep(outer, outer + 5, r);
                    var shadow = Math.Clamp(0.45 * thrown + 0.35 * contact, 0, 1) * (dark ? 0.85 : 0.45);

                    // ---- Its shape, as the slope of the surface at this distance from the centre:
                    // how far it tilts toward the middle (positive) or away from it (negative).
                    double tilt;
                    if (r < wellEdge)
                    {
                        tilt = Math.Atan(0.03 * r / wellEdge);
                    }
                    else if (r < rimFoot)
                    {
                        var t = (r - wellEdge) / (rimFoot - wellEdge);
                        tilt = Math.Atan(Lerp(0.03, rimSlope, t) + 0.55 * Math.Sin(Math.PI * t));
                    }
                    else if (r < rimStart)
                    {
                        tilt = Math.Atan(rimSlope);
                    }
                    else
                    {
                        var t = Math.Min((r - rimStart) / lipWidth, 1.4);
                        tilt = Lerp(Math.Atan(rimSlope), -1.35, t);
                    }

                    var ux = r > 0 ? dx / r : 0;
                    var uy = r > 0 ? dy / r : 0;
                    var n = Normalize(-ux * Math.Sin(tilt), -uy * Math.Sin(tilt), Math.Cos(tilt));

                    // A hand-made surface is never quite even.
                    n = Normalize(n.X + (Noise(dx * 0.06, dy * 0.06) - 0.5) * 0.05, n.Y + (Noise(dx * 0.06 + 31, dy * 0.06 + 31) - 0.5) * 0.05, n.Z);

                    // The low rim throws a thin, soft shadow into the well on the side of the light.
                    var depth = rimHeight * (1 - SmoothStep(wellEdge, rimStart, r));
                    var tx = dx + l.X / l.Z * depth;
                    var ty = dy + l.Y / l.Z * depth;
                    var selfShadow = r < rimStart ? SmoothStep(rimFoot - 6, rimFoot + 10, Math.Sqrt(tx * tx + ty * ty)) * 0.6 : 0;

                    var diffuse = Math.Max(Dot(n, l), 0) * (1 - 0.55 * selfShadow);
                    var nh = Math.Max(Dot(n, h), 0);
                    // A sharp highlight on the rim and the lip; on the well, where the figures are, a
                    // broad soft sheen, so black porcelain still shows it is glazed.
                    var onRim = SmoothStep(inner * 0.9, inner * 1.02, r);
                    var spec = (Math.Pow(nh, 110) * 1.1 * onRim + Math.Pow(nh, dark ? 5 : 8) * (dark ? 0.08 : 0.04)) * (1 - 0.7 * selfShadow);
                    var fresnel = Math.Pow(1 - Math.Max(n.Z, 0), 3) * 0.18;
                    var lit = ambient + reach2 * diffuse;
                    var shine = spec + fresnel;
                    Put(light, i, lit / 2, shine / 2, shadow);

                    // ---- How the breaks and the glaze's edge wander.
                    var wander = (Fbm(dx * 0.03, dy * 0.03) - 0.5) * 9 + (Noise(dx * 0.16, dy * 0.16) - 0.5) * 2.6;
                    var fillWander = (Fbm(dx * 0.045 + 7, dy * 0.045 + 7) - 0.5) * 14;

                    // ---- The veins: some of the borders of a wandering cell pattern, thinning to tips.
                    var qx = dx / veinCell + (Fbm(dx * 0.01, dy * 0.01) - 0.5) * 1.3;
                    var qy = dy / veinCell + (Fbm(dx * 0.01 + 11, dy * 0.01 + 11) - 0.5) * 1.3;
                    var (edge, pair) = Cells(qx, qy);
                    var veinDistance = edge * veinCell;
                    var taper = SmoothStep(0.25, 0.55, Noise(dx * 0.022 + pair * 17, dy * 0.022 + pair * 17));
                    var veinWidth = (0.3 + 0.6 * Noise(dx * 0.05, dy * 0.05)) * taper;
                    var toVein = pair > 0.48 && veinWidth > 0.05 ? veinDistance - veinWidth : 6;
                    Put(paths, i, wander / 16 + 0.5, fillWander / 24 + 0.5, (Math.Clamp(toVein, -2, 6) + 2) / 8);

                    // ---- When the gold reaches each vein: going round the bowl, a little out of turn.
                    var arrive = 0.35 + 0.6 * (0.6 * theta / (2 * Math.PI) + 0.4 * pair);
                    var breakWidth = 1.5 + Noise(dx * 0.04 + 5, dy * 0.04 + 5);
                    var speck = Hash(Math.Floor(dx * 1.3), Math.Floor(dy * 1.3)) > 0.975 ? 0.78 : 1;
                    var clay = (0.9 + 0.2 * Fbm(dx * 0.05, dy * 0.05)) * speck;
                    Put(gold, i, arrive, (breakWidth - 1) / 2, clay / 1.2);

                    var mottle = 0.86 + 0.28 * Fbm(dx * 0.018, dy * 0.018);
                    var bandEdge = (Noise(theta * 4, 3) - 0.5) * 3;
                    Put(surface, i, mottle / 1.2, bandEdge / 8 + 0.5, 0);
                }
            });

            var info = new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Opaque);
            return new Bowl(key, reach, SKImage.FromPixelCopy(info, light), SKImage.FromPixelCopy(info, paths), SKImage.FromPixelCopy(info, gold), SKImage.FromPixelCopy(info, surface));
        }

        private static void Put(byte[] into, int i, double r, double g, double b)
        {
            into[i] = Byte(r);
            into[i + 1] = Byte(g);
            into[i + 2] = Byte(b);
            into[i + 3] = 255;
        }

        /// <summary>
        /// The true distance to the nearest cell border, in cells, and a number for that border.
        /// First the nearest cell point, then the nearest of the borders between it and its
        /// neighbours; a quicker estimate filled whole thin cells with gold.
        /// </summary>
        private static (double Edge, double Pair) Cells(double x, double y)
        {
            var cx = Math.Floor(x);
            var cy = Math.Floor(y);
            var fx = x - cx;
            var fy = y - cy;

            double nearest = 8, mx = 0, my = 0, gx = 0, gy = 0;
            for (var j = -1; j <= 1; j++)
            {
                for (var i = -1; i <= 1; i++)
                {
                    var (hx, hy) = Hash2(cx + i, cy + j);
                    var rx = i + hx - fx;
                    var ry = j + hy - fy;
                    var d = rx * rx + ry * ry;
                    if (d < nearest)
                    {
                        (nearest, mx, my, gx, gy) = (d, rx, ry, i, j);
                    }
                }
            }

            double border = 8, nx = 0, ny = 0;
            for (var j = -2; j <= 2; j++)
            {
                for (var i = -2; i <= 2; i++)
                {
                    var ox = gx + i;
                    var oy = gy + j;
                    var (hx, hy) = Hash2(cx + ox, cy + oy);
                    var rx = ox + hx - fx;
                    var ry = oy + hy - fy;
                    var ex = rx - mx;
                    var ey = ry - my;
                    var length = Math.Sqrt(ex * ex + ey * ey);
                    if (length < 1e-5)
                    {
                        continue;
                    }

                    var d = (0.5 * (mx + rx) * ex + 0.5 * (my + ry) * ey) / length;
                    if (d < border)
                    {
                        (border, nx, ny) = (d, cx + ox, cy + oy);
                    }
                }
            }

            return (border, Hash(cx + gx + nx, cy + gy + ny));
        }

        private static double Hash(double x, double y) => Fract(Math.Sin(x * 127.1 + y * 311.7) * 43758.5453);

        private static (double, double) Hash2(double x, double y) =>
            (Fract(Math.Sin(x * 127.1 + y * 311.7) * 43758.5453), Fract(Math.Sin(x * 269.5 + y * 183.3) * 43758.5453));

        private static double Noise(double x, double y)
        {
            var ix = Math.Floor(x);
            var iy = Math.Floor(y);
            var fx = x - ix;
            var fy = y - iy;
            var ux = fx * fx * (3 - 2 * fx);
            var uy = fy * fy * (3 - 2 * fy);
            var top = Lerp(Hash(ix, iy), Hash(ix + 1, iy), ux);
            var bottom = Lerp(Hash(ix, iy + 1), Hash(ix + 1, iy + 1), ux);
            return Lerp(top, bottom, uy);
        }

        private static double Fbm(double x, double y)
        {
            double v = 0, a = 0.5;
            for (var i = 0; i < 4; i++)
            {
                v += a * Noise(x, y);
                x = x * 2.03 + 17;
                y = y * 2.03 + 9;
                a *= 0.5;
            }

            return v;
        }

        private static double Fract(double v) => v - Math.Floor(v);

        private static double Lerp(double a, double b, double t) => a + (b - a) * t;

        private static double SmoothStep(double from, double to, double v)
        {
            var t = Math.Clamp((v - from) / (to - from), 0, 1);
            return t * t * (3 - 2 * t);
        }

        private static (double X, double Y, double Z) Normalize(double x, double y, double z)
        {
            var length = Math.Sqrt(x * x + y * y + z * z);
            return (x / length, y / length, z / length);
        }

        private static double Dot((double X, double Y, double Z) a, (double X, double Y, double Z) b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    private const string Sksl = """
        uniform shader lightMap;
        uniform shader pathMap;
        uniform shader goldMap;
        uniform shader surfaceMap;
        uniform shader strip;
        uniform float2 center;
        uniform float outer;
        uniform float inner;
        uniform float px;
        uniform float bins;
        uniform float reveal;
        uniform float fillAmount;
        uniform float mend;
        uniform float pointing;
        uniform float4 body;
        uniform float4 clay;
        uniform float4 gold;
        uniform float4 lacquer;
        uniform float dark;

        const float TAU = 6.2831853;

        half4 main(float2 p) {
            float2 d = p - center;
            float r = length(d);
            float3 lightAt = float3(lightMap.eval(p).rgb);
            float lit = lightAt.r * 2.0;
            float shine = lightAt.g * 2.0;
            float shadow = lightAt.b;

            float inside = 1.0 - smoothstep(outer - px, outer + px, r);
            if (inside <= 0.0) { return half4(0.0, 0.0, 0.0, half(shadow)); }

            float3 pathAt = float3(pathMap.eval(p).rgb);
            float3 goldAt = float3(goldMap.eval(p).rgb);
            float3 surfaceAt = float3(surfaceMap.eval(p).rgb);

            float theta = atan(d.x, -d.y);
            if (theta < 0.0) { theta += TAU; }
            float shown = reveal >= 0.999 ? 1.0 : 1.0 - smoothstep(reveal * TAU - 0.02, reveal * TAU + 0.02, theta);

            float rr = max(r, 1.0);
            float warped = mod(theta + (pathAt.r - 0.5) * 16.0 / rr, TAU);
            float filled = mod(theta + (pathAt.g - 0.5) * 24.0 / rr, TAU);
            float x = warped / TAU * bins;
            float3 glaze = float3(strip.eval(float2(x, 0.5)).rgb);
            float3 kind = float3(strip.eval(float2(x, 1.5)).rgb);
            float3 toward = float3(strip.eval(float2(x, 2.5)).rgb);
            float3 spentAt = float3(strip.eval(float2(filled / TAU * bins, 3.5)).rgb);

            float isClay = step(0.25, kind.r) * (1.0 - step(0.75, kind.r));
            float isGlaze = 1.0 - step(0.25, kind.r);
            float emphasis = kind.b;

            float rimStart = outer * 0.955;
            float bandInner = inner + (surfaceAt.g - 0.5) * 8.0;
            float inBand = smoothstep(bandInner - px, bandInner + px, r) * (1.0 - smoothstep(rimStart - 2.0, rimStart + 3.0, r));

            // ---- Surface: porcelain, raw clay, or glaze as far as it is spent.
            float3 colour = body.rgb;
            float gloss = 1.0;
            if (isClay > 0.5) {
                colour = mix(body.rgb, clay.rgb * goldAt.b * 1.2, inBand * shown);
                gloss = mix(1.0, 0.12, inBand * shown);
            } else if (isGlaze > 0.5) {
                float3 glazed = glaze * surfaceAt.r * 1.2;
                glazed *= mix(0.72, 1.06, smoothstep(inner, rimStart, r));
                glazed = mix(glazed, body.rgb, smoothstep(rimStart - 9.0, rimStart + 1.0, r) * 0.55);
                float edge = 1.0 - smoothstep(0.0, 2.2, spentAt.g * 0.05 * rr);
                glazed *= 1.0 - 0.22 * edge;
                colour = mix(body.rgb, glazed, spentAt.r * inBand * shown);
            }

            float3 result = colour * lit + float3(shine * gloss);

            // ---- The gold: veins through the shards, breaks between them.
            float diffuse = clamp(dark > 0.5 ? (lit - 0.55) / 0.55 : (lit - 0.52) / 0.5, 0.0, 1.0);
            float3 metal = gold.rgb * (0.5 + 0.75 * diffuse) + mix(gold.rgb, float3(1.0), 0.45) * min(shine, 1.0) * 1.2;
            float3 red = lacquer.rgb * (0.65 + 0.45 * diffuse) + float3(min(shine, 1.0) * 0.3);
            float3 crack = body.rgb * 0.35;

            float toVein = pathAt.b * 8.0 - 2.0;
            float vein = (1.0 - smoothstep(-px, px, toVein)) * shown;
            float over = kind.g * smoothstep(0.9, 1.0, fillAmount) * inBand;
            float veinArrive = goldAt.r;
            float veinGold = smoothstep(veinArrive - 0.015, veinArrive + 0.015, mend);
            result = mix(result, mix(crack, mix(metal, red, over), veinGold), vein * mix(0.65, 1.0, veinGold));

            // A break carries on into the bowl and ends in a tip: it narrows, it does not fade.
            float breakWidth = (1.0 + goldAt.g * 2.0) * 0.6 * smoothstep(inner * 0.45, inner * 0.97, r);
            float toBreak = toward.r * 0.25 * rr;
            float mended = (1.0 - smoothstep(breakWidth - px, breakWidth + px, toBreak)) * step(0.05, breakWidth) * shown;
            float breakArrive = 0.45 * (warped / TAU);
            float breakGold = smoothstep(breakArrive - 0.015, breakArrive + 0.015, mend);
            result = mix(result, mix(crack, metal, breakGold), mended * mix(0.75, 1.0, breakGold));

            // An overspent shard always shows it: a lacquered break down its middle.
            float overBreak = over * (1.0 - smoothstep(1.0 - px, 1.0 + px, toward.g * 0.25 * rr)) * shown;
            result = mix(result, red, overBreak);

            float glint = max(1.0 - abs(mend - breakArrive) / 0.03, 0.0) * mended + max(1.0 - abs(mend - veinArrive) / 0.03, 0.0) * vein;
            result += mix(gold.rgb, float3(1.0), 0.6) * glint * 0.8 * step(mend, 0.999);

            // ---- Pointing at a shard: the others fade toward bare porcelain, the chosen one lifts.
            float back = 0.6 * pointing * (1.0 - emphasis) * inBand;
            float luma = dot(result, float3(0.299, 0.587, 0.114));
            result = mix(result, mix(float3(luma), body.rgb, 0.45), back);
            result *= 1.0 + 0.08 * emphasis * inBand;

            float3 shownColour = clamp(result, 0.0, 1.0) * inside;
            return half4(half3(shownColour), half(inside + shadow * (1.0 - inside)));
        }
        """;
}
