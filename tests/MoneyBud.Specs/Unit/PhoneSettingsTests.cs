using MoneyBud.Presentation;
using MoneyBud.Storage;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// The phone's settings, beyond what choose-how-moneybud-looks.feature holds: how they are read when
/// only part of them can be, and that the file keeping them never throws (plan for increment 14, D3).
/// </summary>
public sealed class PhoneSettingsTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "MoneyBud.Specs", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
    }

    private static PhoneSettings Read(string? text) => PhoneSettings.Open(() => text, _ => true);

    [Fact]
    public void Each_setting_is_read_on_its_own_so_one_that_cannot_be_read_keeps_its_default()
    {
        var settings = Read("""{ "theme": "Porselein", "appearance": "Dark", "hintsShown": true }""");

        Assert.Equal(PhoneTheme.Standaard, settings.Theme);
        Assert.Equal(Appearance.Dark, settings.Appearance);
        Assert.False(settings.ShowsHints);
    }

    [Fact]
    public void A_setting_is_read_by_its_name_never_by_a_number()
    {
        var settings = Read("""{ "theme": 1, "appearance": "2" }""");

        Assert.Equal(PhoneTheme.Standaard, settings.Theme);
        Assert.Equal(Appearance.FollowThePhone, settings.Appearance);
    }

    [Fact]
    public void Text_that_is_not_an_object_is_passed_over()
    {
        var settings = Read("""["Kintsugi"]""");

        Assert.Equal(PhoneTheme.Standaard, settings.Theme);
        Assert.True(settings.ShowsHints);
    }

    [Fact]
    public void A_write_that_fails_says_nothing_and_the_next_choice_tries_again()
    {
        var written = new List<string>();
        var works = false;
        var settings = PhoneSettings.Open(() => null, text =>
        {
            if (works) written.Add(text);
            return works;
        });

        settings.ChooseTheme(PhoneTheme.Kintsugi);
        works = true;
        settings.ChooseAppearance(Appearance.Light);

        var kept = Read(Assert.Single(written));
        Assert.Equal(PhoneTheme.Kintsugi, kept.Theme);
        Assert.Equal(Appearance.Light, kept.Appearance);
    }

    [Fact]
    public void Choosing_what_is_already_chosen_writes_nothing()
    {
        var writes = 0;
        var settings = PhoneSettings.Open(() => """{ "theme": "Kintsugi", "appearance": "Dark", "hintsShown": true }""", _ =>
        {
            writes++;
            return true;
        });

        settings.ChooseTheme(PhoneTheme.Kintsugi);
        settings.ChooseAppearance(Appearance.Dark);

        Assert.Equal(0, writes);
    }

    [Fact]
    public void Opening_the_settings_writes_nothing_until_the_home_screen_opens()
    {
        var writes = new List<string>();
        var settings = PhoneSettings.Open(() => null, text =>
        {
            writes.Add(text);
            return true;
        });
        Assert.Empty(writes);
        Assert.True(settings.ShowsHints);

        settings.HomeScreenOpened();
        settings.HomeScreenOpened();

        Assert.False(Read(Assert.Single(writes)).ShowsHints);
    }

    [Fact]
    public void The_hints_fade_and_are_not_shown_again_until_asked_for()
    {
        var settings = Read(null);
        Assert.True(settings.ShowsHints);

        settings.HintsGone();
        Assert.False(settings.ShowsHints);

        settings.ShowHintsAgain();
        Assert.True(settings.ShowsHints);
    }

    [Fact]
    public void The_settings_file_keeps_text_whole_and_reads_it_back()
    {
        var file = new SettingsFile(folder);
        Assert.Null(file.Read());

        Assert.True(file.TryWrite("eerst"));
        Assert.True(file.TryWrite("daarna"));

        Assert.Equal("daarna", file.Read());
        Assert.Equal(["settings.json"], Directory.GetFiles(folder).Select(Path.GetFileName));
    }

    [Fact]
    public void A_settings_file_that_cannot_be_written_or_read_says_so_without_throwing()
    {
        // A file stands where the folder should be.
        Directory.CreateDirectory(folder);
        var blocked = Path.Combine(folder, "blocked");
        File.WriteAllText(blocked, "");
        var file = new SettingsFile(blocked);

        Assert.False(file.TryWrite("{}"));
        Assert.Null(file.Read());
    }
}
