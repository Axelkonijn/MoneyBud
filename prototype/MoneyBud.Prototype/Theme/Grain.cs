using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MoneyBud.Prototype.Themes;

/// <summary>
/// A tile of ceramic grain: fine light and dark specks over a soft mottling, repeated across the
/// screen. Neutral on purpose: how strongly it shows is the theme's <c>GrainOpacity</c>.
/// </summary>
public static class Grain
{
    private const int Size = 160;
    private const int Cell = 20;

    private static IBrush? _brush;

    public static IBrush Brush => _brush ??= Make();

    private static IBrush Make()
    {
        var random = new Random(7);

        // The mottling: random values on a coarse grid that wraps, so the tile repeats seamlessly.
        var cells = Size / Cell;
        var coarse = new double[cells, cells];
        for (var y = 0; y < cells; y++)
        {
            for (var x = 0; x < cells; x++)
            {
                coarse[x, y] = random.NextDouble();
            }
        }

        var pixels = new byte[Size * Size * 4];
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var mottle = Smooth(coarse, cells, x / (double)Cell, y / (double)Cell);
                var speck = random.NextDouble() - 0.5;
                var alpha = (byte)Math.Clamp(Math.Pow(Math.Abs(speck) * 2, 2) * 255 * (0.45 + 0.55 * mottle), 0, 255);
                var light = speck > 0 ? alpha : (byte)0; // premultiplied: white at alpha, or black
                var i = (y * Size + x) * 4;
                pixels[i] = light;
                pixels[i + 1] = light;
                pixels[i + 2] = light;
                pixels[i + 3] = alpha;
            }
        }

        var bitmap = new WriteableBitmap(new PixelSize(Size, Size), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
        using (var buffer = bitmap.Lock())
        {
            for (var y = 0; y < Size; y++)
            {
                Marshal.Copy(pixels, y * Size * 4, buffer.Address + y * buffer.RowBytes, Size * 4);
            }
        }

        return new ImageBrush(bitmap)
        {
            TileMode = TileMode.Tile,
            Stretch = Stretch.None,
            DestinationRect = new RelativeRect(0, 0, Size, Size, RelativeUnit.Absolute),
        };
    }

    private static double Smooth(double[,] grid, int cells, double x, double y)
    {
        var x0 = (int)x;
        var y0 = (int)y;
        var fx = x - x0;
        var fy = y - y0;
        fx = fx * fx * (3 - 2 * fx);
        fy = fy * fy * (3 - 2 * fy);
        double At(int gx, int gy) => grid[gx % cells, gy % cells];
        var top = At(x0, y0) + (At(x0 + 1, y0) - At(x0, y0)) * fx;
        var bottom = At(x0, y0 + 1) + (At(x0 + 1, y0 + 1) - At(x0, y0 + 1)) * fx;
        return top + (bottom - top) * fy;
    }
}
