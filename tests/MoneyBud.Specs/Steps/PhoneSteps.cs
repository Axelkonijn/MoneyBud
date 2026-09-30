using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for the phone's own rules — choose-how-moneybud-looks.feature and the last section of
/// carry-on-when-saving-fails.feature (increment 14).
///
/// <para>Going to the background and coming back go through <see cref="MoneyBudApp.GoToBackground"/>
/// and <see cref="MoneyBudApp.ComeBack"/>, the doors the phone uses. The settings go through
/// <see cref="PhoneSettings"/> over the real settings file, in a folder of the scenario's own apart from
/// its data (<see cref="SpecContext.SettingsFolder"/>).</para>
/// </summary>
[Binding]
public sealed class PhoneSteps(SpecContext context)
{
    private MoneyBudApp App => context.App;

    // The data file as it was just before the scenario's first choice of how MoneyBud looks.
    private byte[]? dataBeforeChoosing;

    // ------------------------------------------------------------------ the background

    [When(@"^MoneyBud goes to the background$")]
    public void WhenMoneyBudGoesToTheBackground() => App.GoToBackground();

    // The minute's look is done by coming back itself, never by the step's own Tick.
    [When(@"^I come back to MoneyBud$")]
    public void WhenIComeBackToMoneyBud()
    {
        context.TimePassed();
        App.ComeBack();
    }

    [When(@"^the phone ends MoneyBud while it is in the background$")]
    public void WhenThePhoneEndsMoneyBudInTheBackground() => context.EndInBackground();

    // The phone's timer might still fire in the background; MoneyBud must do nothing there, so the
    // steps straight after this one are about exactly that.
    [When(@"^the next budget period begins while MoneyBud is in the background$")]
    public void WhenTheNextBudgetPeriodBeginsInTheBackground()
    {
        _ = App;
        context.SetToday(context.Ledger.Calendar.Next(context.Ledger.CurrentPeriod).FirstDay);
        context.TimePassed();
        App.Tick();
    }

    // ------------------------------------------------------------------ settings: Given

    // An earlier use, around this one: those two choices, and the hints shown, as at any first start.
    [Given(@"^the last time I used MoneyBud, I chose the theme ""([^""]*)"" and the appearance ""([^""]*)""$")]
    public void GivenTheLastTimeIChose(string theme, string appearance)
    {
        var earlier = context.OpenSettings();
        earlier.HomeScreenOpened();
        earlier.ChooseTheme(Theme(theme));
        earlier.ChooseAppearance(Appearance(appearance));
    }

    [When(@"^the settings MoneyBud kept (become damaged|become blank, with nothing at all in them|are gone)$")]
    public void WhenTheSettingsKeptBecome(string problem)
    {
        Assert.True(File.Exists(context.SettingsPath), "No settings were kept to begin with.");
        switch (problem)
        {
            case "become damaged":
                File.WriteAllText(context.SettingsPath, "{\n  \"theme\": \"Kin");
                break;
            case "become blank, with nothing at all in them":
                File.WriteAllBytes(context.SettingsPath, []);
                break;
            default:
                File.Delete(context.SettingsPath);
                break;
        }
    }

    // ------------------------------------------------------------------ settings: When

    [When(@"^I choose the theme ""([^""]*)""$")]
    public void WhenIChooseTheTheme(string theme)
    {
        NoteTheDataBeforeChoosing();
        context.Settings.ChooseTheme(Theme(theme));
    }

    [When(@"^I choose the appearance ""([^""]*)""$")]
    public void WhenIChooseTheAppearance(string appearance)
    {
        NoteTheDataBeforeChoosing();
        context.Settings.ChooseAppearance(Appearance(appearance));
    }

    [When(@"^I ask for the hints to be shown again$")]
    public void WhenIAskForTheHintsToBeShownAgain() => context.Settings.ShowHintsAgain();

    // ------------------------------------------------------------------ settings: Then

    [Then(@"^the theme should (?:still )?be ""([^""]*)""$")]
    public void ThenTheThemeShouldBe(string theme) => Assert.Equal(Theme(theme), context.Settings.Theme);

    [Then(@"^the appearance should (?:still )?be ""([^""]*)""$")]
    public void ThenTheAppearanceShouldBe(string appearance) =>
        Assert.Equal(Appearance(appearance), context.Settings.Appearance);

    [Then(@"^the themes offered should be exactly these, in this order:$")]
    public void ThenTheThemesOfferedShouldBe(Table table) =>
        Assert.Equal(table.Rows.Select(r => r["theme"]), PhoneSettings.Themes.Select(t => t.Text));

    [Then(@"^the appearances offered should be exactly these, in this order:$")]
    public void ThenTheAppearancesOfferedShouldBe(Table table) =>
        Assert.Equal(table.Rows.Select(r => r["appearance"]), PhoneSettings.Appearances.Select(a => a.Text));

    [Then(@"^the home screen should show the hints$")]
    public void ThenTheHomeScreenShouldShowTheHints() => Assert.True(context.Settings.ShowsHints);

    [Then(@"^the home screen should not show the hints$")]
    public void ThenTheHomeScreenShouldNotShowTheHints() => Assert.False(context.Settings.ShowsHints);

    [Then(@"^the data MoneyBud keeps should be exactly as it was before I chose how MoneyBud looks$")]
    public void ThenTheDataShouldBeAsBeforeChoosing()
    {
        var before = dataBeforeChoosing ?? throw new InvalidOperationException("Nothing was chosen.");
        Assert.Equal(before, File.ReadAllBytes(context.DataFile));
    }

    // ------------------------------------------------------------------

    // MoneyBud is open when anything is chosen, so what the Givens set up is kept by then.
    private void NoteTheDataBeforeChoosing()
    {
        _ = App;
        dataBeforeChoosing ??= File.ReadAllBytes(context.DataFile);
    }

    private static PhoneTheme Theme(string name) =>
        PhoneSettings.Themes.Single(t => t.Text == name).Theme;

    private static Appearance Appearance(string name) =>
        PhoneSettings.Appearances.Single(a => a.Text == name).Appearance;
}
