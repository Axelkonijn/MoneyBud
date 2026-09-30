using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MoneyBud.Prototype.Ring;

namespace MoneyBud.Prototype.Themes;

/// <summary>
/// A theme: its named colours and resources, the painter that draws its ring, and — if its panels
/// are more than glass — the painter that draws them.
/// </summary>
public sealed record Look(string Name, string Source, Func<RingPainter> Painter, SlabPainter? Slab = null);

/// <summary>
/// The themes there are, and which one is in use. Switching swaps the app's one theme dictionary,
/// so everything that follows a named resource repaints by itself. The ring's painter is not a
/// resource, so whoever shows a ring listens for <see cref="Changed"/>. Not remembered between
/// starts: the prototype saves nothing.
/// </summary>
public static class Looks
{
    public static readonly Look Standaard = new("Standaard", "avares://MoneyBud.Prototype/Theme/Default.axaml", () => new DefaultRingPainter());

    public static readonly Look Kintsugi = new("Kintsugi", "avares://MoneyBud.Prototype/Theme/Kintsugi.axaml", () => new KintsugiRingPainter(), new SlabPainter());

    public static IReadOnlyList<Look> All { get; } = [Standaard, Kintsugi];

    public static Look Current { get; private set; } = Standaard;

    public static event Action? Changed;

    public static void Use(Look look)
    {
        if (look == Current)
        {
            return;
        }

        var dictionaries = Application.Current!.Resources.MergedDictionaries;
        dictionaries[0] = (IResourceProvider)AvaloniaXamlLoader.Load(new Uri(look.Source));
        Current = look;
        Changed?.Invoke();
    }
}
