using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using MoneyBud.Phone.Ring;
using MoneyBud.Presentation;

namespace MoneyBud.Phone.Themes;

/// <summary>
/// A theme's looks: its named colours and resources, the painter that draws its ring, and — if its
/// panels are more than glass — the painter that draws them. Which theme is chosen is
/// <see cref="PhoneSettings"/>'s; this is only how each looks (ADR 0013, decision 5).
/// </summary>
public sealed record Look(PhoneTheme Theme, string Source, Func<RingPainter> Painter, SlabPainter? Slab = null);

/// <summary>
/// The looks there are, and which is in use. Switching swaps the app's one theme dictionary, so
/// everything that follows a named resource repaints by itself. The ring's painter is not a
/// resource, so whoever shows a ring listens for <see cref="Changed"/>.
/// </summary>
public static class Looks
{
    public static readonly Look Standaard = new(PhoneTheme.Standaard, "avares://MoneyBud.Phone/Theme/Default.axaml", () => new DefaultRingPainter());

    public static readonly Look Kintsugi = new(PhoneTheme.Kintsugi, "avares://MoneyBud.Phone/Theme/Kintsugi.axaml", () => new KintsugiRingPainter(), new SlabPainter());

    public static IReadOnlyList<Look> All { get; } = [Standaard, Kintsugi];

    public static Look Current { get; private set; } = Standaard;

    public static event Action? Changed;

    public static Look Of(PhoneTheme theme) => All.Single(l => l.Theme == theme);

    /// <summary>The theme and <i>Weergave</i> the settings hold, at once and without a transition: at the start.</summary>
    public static void Apply(PhoneSettings settings)
    {
        Use(Of(settings.Theme));
        UseAppearance(settings.Appearance);
    }

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

    /// <summary><i>Weergave</i>: dark, light, or whatever the phone is set to.</summary>
    public static void UseAppearance(Appearance appearance) =>
        Application.Current!.RequestedThemeVariant = appearance switch
        {
            Appearance.Dark => ThemeVariant.Dark,
            Appearance.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default,
        };
}
