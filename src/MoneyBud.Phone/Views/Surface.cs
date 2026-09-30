using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using MoneyBud.Phone.Themes;

namespace MoneyBud.Phone.Views;

/// <summary>
/// A panel's backing: the default theme's glass — a filled rounded rectangle with a hairline on
/// one side — unless the theme draws its panels as something, as kintsugi draws them as porcelain
/// slabs on the table. Then the theme's painter draws it, and the content keeps inside what it
/// draws. It takes Border's properties, so a panel is set up as a border would be.
/// </summary>
public sealed class Surface : Decorator
{
    public static readonly StyledProperty<IBrush?> BackgroundProperty = Border.BackgroundProperty.AddOwner<Surface>();

    public static readonly StyledProperty<IBrush?> BorderBrushProperty = Border.BorderBrushProperty.AddOwner<Surface>();

    public static readonly StyledProperty<Thickness> BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner<Surface>();

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner<Surface>();

    public static readonly StyledProperty<SurfaceSide> SideProperty =
        AvaloniaProperty.Register<Surface, SurfaceSide>(nameof(Side), SurfaceSide.Centre);

    private Thickness _padding;
    private bool _attached;

    /// <summary>
    /// The slab, drawn once into a picture at the screen's own resolution. Drawn as shapes every
    /// frame — an uneven outline, gradients, a clip, hundreds of short gold lines — it held a drag
    /// to about 24 frames a second on the phone; a picture is one cheap draw. It is kept as a plain,
    /// unchanging bitmap, which the GPU can keep; a render target was sent to it again each frame.
    /// </summary>
    private Bitmap? _picture;
    private (Size Size, double Scaling, bool Dark)? _pictureOf;

    static Surface() => AffectsRender<Surface>(BackgroundProperty, BorderBrushProperty, BorderThicknessProperty, CornerRadiusProperty, SideProperty);

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>Which way it is pulled in: the edges on that side's far end are off screen.</summary>
    public SurfaceSide Side
    {
        get => GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!_attached)
        {
            _padding = Padding;
            _attached = true;
        }

        Looks.Changed += Refresh;
        ActualThemeVariantChanged += OnVariantChanged;
        Refresh();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Looks.Changed -= Refresh;
        ActualThemeVariantChanged -= OnVariantChanged;
        Forget();
        base.OnDetachedFromVisualTree(e);
    }

    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);
        if (Looks.Current.Slab is { } slab)
        {
            if (Picture(slab, bounds) is { } picture)
            {
                context.DrawImage(picture, new Rect(picture.Size), bounds);
            }

            return;
        }

        var shape = new RoundedRect(bounds, CornerRadius);
        if (Background is { } background)
        {
            context.DrawRectangle(background, null, shape);
        }

        PaintEdge(context, bounds);
    }

    /// <summary>The hairline: all round, or along the one side that has it, round its corners.</summary>
    private void PaintEdge(DrawingContext context, Rect bounds)
    {
        var edge = BorderThickness;
        var width = Math.Max(Math.Max(edge.Left, edge.Right), Math.Max(edge.Top, edge.Bottom));
        if (BorderBrush is not { } brush || width <= 0)
        {
            return;
        }

        var pen = new Pen(brush, width);
        var inset = new RoundedRect(bounds.Deflate(width / 2), CornerRadius);
        if (edge.Left > 0 && edge.Right > 0 && edge.Top > 0 && edge.Bottom > 0)
        {
            context.DrawRectangle(null, pen, inset);
            return;
        }

        var reach = Math.Max(Math.Max(CornerRadius.TopLeft, CornerRadius.TopRight), Math.Max(CornerRadius.BottomLeft, CornerRadius.BottomRight)) + width;
        var band = edge.Left > 0 ? new Rect(0, 0, reach, bounds.Height)
            : edge.Right > 0 ? new Rect(bounds.Width - reach, 0, reach, bounds.Height)
            : edge.Top > 0 ? new Rect(0, 0, bounds.Width, reach)
            : new Rect(0, bounds.Height - reach, bounds.Width, reach);
        using (context.PushClip(band))
        {
            context.DrawRectangle(null, pen, inset);
        }
    }

    private Bitmap? Picture(SlabPainter slab, Rect bounds)
    {
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var dark = ActualThemeVariant == ThemeVariant.Dark;
        var of = (bounds.Size, scaling, dark);
        if (_picture is not null && _pictureOf == of)
        {
            return _picture;
        }

        Forget();
        var pixels = new PixelSize((int)Math.Ceiling(bounds.Width * scaling), (int)Math.Ceiling(bounds.Height * scaling));
        if (pixels.Width <= 0 || pixels.Height <= 0)
        {
            return null;
        }

        // Made at plain 96 dpi, so its size is its pixels, and scaled by hand to the screen's density:
        // a picture at the screen's own dpi came out magnified on the phone, because one step read
        // its size in points and another in pixels. On the desktop both are the same.
        var dpi = new Vector(96, 96);
        using var drawn = new RenderTargetBitmap(pixels, dpi);
        using (var context = drawn.CreateDrawingContext())
        using (context.PushTransform(Matrix.CreateScale(scaling, scaling)))
        {
            slab.Paint(context, bounds, Side, Seed(), Named("BowlColor"), Named("SeamColor"), dark);
        }

        var format = drawn.Format ?? Avalonia.Platform.PixelFormats.Bgra8888;
        var alpha = drawn.AlphaFormat ?? Avalonia.Platform.AlphaFormat.Premul;
        var stride = pixels.Width * 4;
        var buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(stride * pixels.Height);
        try
        {
            drawn.CopyPixels(new PixelRect(pixels), buffer, stride * pixels.Height, stride);
            _picture = new Bitmap(format, alpha, buffer, pixels, dpi, stride);
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer);
        }

        _pictureOf = of;
        return _picture;
    }

    private void Forget()
    {
        _picture?.Dispose();
        _picture = null;
        _pictureOf = null;
    }

    private void OnVariantChanged(object? sender, EventArgs e) => InvalidateVisual();

    /// <summary>A slab keeps its content inside it; glass lets it reach its hairline.</summary>
    private void Refresh()
    {
        Padding = Looks.Current.Slab is { } slab ? _padding + slab.Inset(Side) : _padding;
        Forget();
        InvalidateVisual();
    }

    /// <summary>The same breaks every time for the same panel.</summary>
    private int Seed()
    {
        var hash = 17;
        foreach (var c in Name ?? Side.ToString())
        {
            hash = hash * 31 + c;
        }

        return hash;
    }

    private Color Named(string key) =>
        this.TryFindResource(key, ActualThemeVariant, out var value) && value is Color colour ? colour : Colors.Gray;
}
