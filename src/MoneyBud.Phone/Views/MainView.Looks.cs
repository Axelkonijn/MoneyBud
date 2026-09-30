using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MoneyBud.Phone.Motion;
using MoneyBud.Phone.Themes;
using MoneyBud.Presentation;

namespace MoneyBud.Phone.Views;

/// <summary>
/// Switching themes: the screen fades from the old look to the new, and the ring is drawn in again
/// (arc42 §12, <i>Themes</i>). A theme changes only how MoneyBud looks and moves, never what it shows.
/// </summary>
public sealed partial class MainView
{
    /// <summary>The theme changed while a panel hid the ring: it draws itself in once the panel is gone.</summary>
    private bool _drawInWaiting;

    private void SwitchLook(Look look)
    {
        if (look == Looks.Current)
        {
            return;
        }

        // A picture of the screen as it is lies over the new look and fades away.
        if (Picture() is { } picture)
        {
            var cover = new Image { Source = picture, Stretch = Stretch.Fill, Width = W, Height = H, IsHitTestVisible = false };
            Root.Children.Add(cover);
            Tween.Run(this, 0.55, Ease.InOutCubic, t => cover.Opacity = 1 - t, done: () =>
            {
                Root.Children.Remove(cover);
                picture.Dispose();
            });
        }

        Looks.Use(look);

        // The ring is drawn in again in the new look, where it can be seen.
        Ring.Reveal = 0;
        Hole.Opacity = 0;
        if (_screen.Open == PhonePanel.None)
        {
            DrawRingIn(delay: 0.3);
        }
        else
        {
            _drawInWaiting = true;
        }
    }

    /// <summary>A panel came to rest: once none is left over the ring, a draw-in that waited for it runs.</summary>
    private void DrawInIfWaiting()
    {
        if (_drawInWaiting && _screen.Open == PhonePanel.None)
        {
            _drawInWaiting = false;
            DrawRingIn(delay: 0);
        }
    }

    private void LookChanged()
    {
        _homeBlur = this.TryFindResource("HomeBlur", out var blur) && blur is double b ? b : 22;
        _homeDim = this.TryFindResource("HomeDim", out var dim) && dim is double d ? d : 0.32;
        ApplyHome();
        Ring.Painter = Looks.Current.Painter();
        PaintGlow();
        Ring.InvalidateVisual();
    }

    /// <summary>The screen as it is now.</summary>
    private RenderTargetBitmap? Picture()
    {
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var size = new PixelSize((int)Math.Ceiling(W * scaling), (int)Math.Ceiling(H * scaling));
        if (size.Width <= 0 || size.Height <= 0)
        {
            return null;
        }

        // The picture is drawn on the CPU, where kintsugi's plate shader takes seconds. The plate is
        // left out: the settings over it hide it, and it draws itself in again after the switch.
        var picture = new RenderTargetBitmap(size, new Vector(96 * scaling, 96 * scaling));
        var ring = Ring.IsVisible;
        Ring.IsVisible = false;
        picture.Render(this);
        Ring.IsVisible = ring;
        return picture;
    }
}
