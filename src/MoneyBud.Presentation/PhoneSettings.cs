using System.Text.Json;

namespace MoneyBud.Presentation;

/// <summary>The phone's two themes (arc42 §12, <i>Themes</i>): a world each, not a palette.</summary>
public enum PhoneTheme
{
    Standaard,
    Kintsugi,
}

/// <summary><i>Weergave</i>: light or dark, or whatever the phone is set to. One setting for every theme.</summary>
public enum Appearance
{
    FollowThePhone,
    Dark,
    Light,
}

/// <summary>
/// What the phone remembers about itself between starts: the theme, <i>Weergave</i>, and whether the
/// home screen's first-time hints have been shown (arc42 §12, <i>Where the data lives on the phone</i>
/// and <i>Themes</i>; plan for increment 14, D3). Phone-only, by ruling: themes are the phone's alone.
///
/// <list type="bullet">
/// <item><b>Kept apart from the data.</b> The theme is about the device in the hand, not about the
/// money, and the data file stays the same for desktop and phone. So nothing here touches the ledger
/// or its store, and choosing never writes the data file.</item>
/// <item><b>Never in the way.</b> Settings that cannot be read — none yet, damaged, blank, an unknown
/// name — are passed over without a word: <i>Standaard</i>, <i>Systeem</i>, and the hints shown, as on
/// a first start. A failed write says nothing either, and is tried again with the next choice; the
/// worst it costs is the theme on the next start.</item>
/// <item><b>Written on every choice</b>, as a few lines of JSON text, through the two functions it is
/// handed: the file itself is <c>MoneyBud.Storage</c>'s, so neither project depends on the other.</item>
/// </list>
/// </summary>
public sealed class PhoneSettings
{
    private readonly Func<string, bool> write;
    private bool hintsSeen;

    private PhoneSettings(Func<string, bool> write) => this.write = write;

    /// <summary>
    /// Reads what was kept, passing over anything it cannot read.
    /// </summary>
    /// <param name="read">The text kept, or null when there is none or it cannot be read.</param>
    /// <param name="write">Keeps the text given, whole; false when that failed.</param>
    public static PhoneSettings Open(Func<string?> read, Func<string, bool> write)
    {
        var settings = new PhoneSettings(write);
        settings.ReadFrom(read());
        settings.ShowsHints = !settings.hintsSeen;
        return settings;
    }

    /// <summary>
    /// MoneyBud opened on its home screen, which shows the hints if they are due: they count as shown
    /// from this moment (choose-how-moneybud-looks.feature, as the approved prototype did). Not when
    /// MoneyBud refuses to open, since then there is no home screen to show them on.
    /// </summary>
    public void HomeScreenOpened()
    {
        if (hintsSeen) return;

        hintsSeen = true;
        write(ToText());
    }

    public PhoneTheme Theme { get; private set; } = PhoneTheme.Standaard;

    public Appearance Appearance { get; private set; } = Appearance.FollowThePhone;

    /// <summary>Whether the home screen shows its hints: on a first start, and after <see cref="ShowHintsAgain"/>.</summary>
    public bool ShowsHints { get; private set; }

    /// <summary>Raised after every choice, so whatever shows a setting can look again.</summary>
    public event Action? Changed;

    /// <summary>The choices <i>Thema</i> offers, in order, with their names on screen.</summary>
    public static IReadOnlyList<(PhoneTheme Theme, string Text)> Themes { get; } =
        [(PhoneTheme.Standaard, Tekst.StandardTheme), (PhoneTheme.Kintsugi, Tekst.KintsugiTheme)];

    /// <summary>The choices <i>Weergave</i> offers, in order, with their names on screen.</summary>
    public static IReadOnlyList<(Appearance Appearance, string Text)> Appearances { get; } =
        [(Appearance.FollowThePhone, Tekst.FollowThePhone), (Appearance.Dark, Tekst.Dark), (Appearance.Light, Tekst.Light)];

    public void ChooseTheme(PhoneTheme theme)
    {
        if (theme == Theme) return;

        Theme = theme;
        Keep();
    }

    /// <summary>Choosing <i>Weergave</i>; the theme stays as it is, since one <i>Weergave</i> serves every theme.</summary>
    public void ChooseAppearance(Appearance appearance)
    {
        if (appearance == Appearance) return;

        Appearance = appearance;
        Keep();
    }

    /// <summary>The hints have faded from the home screen, after about ten seconds or at the first touch.</summary>
    public void HintsGone() => ShowsHints = false;

    /// <summary>
    /// <i>Aanwijzingen opnieuw tonen</i>: the home screen shows its hints again, now. They were shown
    /// before, so they are not shown again at the next start.
    /// </summary>
    public void ShowHintsAgain()
    {
        ShowsHints = true;
        Changed?.Invoke();
    }

    private void Keep()
    {
        write(ToText());
        Changed?.Invoke();
    }

    // Written and read by hand, as the data file is (LedgerJson): nothing here leans on reflection,
    // which Android's trimmed build may take away.
    private string ToText()
    {
        using var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = true }))
        {
            json.WriteStartObject();
            json.WriteString("theme", Theme.ToString());
            json.WriteString("appearance", Appearance.ToString());
            json.WriteBoolean("hintsShown", hintsSeen);
            json.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(buffer.ToArray());
    }

    // Each setting on its own: one that cannot be read keeps its default, and the rest still count.
    private void ReadFrom(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        try
        {
            using var document = JsonDocument.Parse(text);
            var kept = document.RootElement;
            if (kept.ValueKind != JsonValueKind.Object) return;

            if (Named<PhoneTheme>(kept, "theme") is { } theme) Theme = theme;
            if (Named<Appearance>(kept, "appearance") is { } appearance) Appearance = appearance;
            if (kept.TryGetProperty("hintsShown", out var shown) && shown.ValueKind is JsonValueKind.True or JsonValueKind.False)
                hintsSeen = shown.GetBoolean();
        }
        catch (JsonException)
        {
            // Passed over: whatever was read before the fault still counts.
        }
    }

    // Names only, never numbers: "1" is not a theme.
    private static T? Named<T>(JsonElement kept, string property) where T : struct, Enum =>
        kept.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
        && value.GetString() is { } name && Enum.GetNames<T>().Contains(name, StringComparer.Ordinal)
            ? Enum.Parse<T>(name)
            : null;
}
