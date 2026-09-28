using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for the sweep files: sweep-at-a-period-end, show-an-ended-period,
/// bring-a-swept-period-up-to-date and choose-a-sweep-destination. sweep-at-a-period-end.feature's
/// header explains the steps they share.
///
/// <para>Every act goes the way the user's does: the destination is a choice in the <i>Restant
/// naar</i> list, which is only on the current period and later ones, so choosing steps forward to
/// the current period first when an ended one is on screen; and <i>Restant bijwerken</i> is pressed
/// with its period on screen. Every figure is read from what the screen shows: the period's line,
/// the list, and the notice. "The line of the previous budget period" is what the Overview draws with
/// that period on screen, read without stepping to it. Givens set up the ledger directly, as
/// everywhere.</para>
/// </summary>
[Binding]
public sealed class SweepSteps(SpecContext context)
{
    // A period as a step names it in full, without its "the" (SpecParsing.PeriodNamed).
    private const string Period = @"((?:current|previous|next) budget period|budget period \d+ (?:before|after) the current one)";

    private Ledger Ledger => context.Ledger;
    private MoneyBudApp App => context.App;

    // ------------------------------------------------------------------ Given

    [Given(@"^I have set the sweep destination to ""([^""]*)""$")]
    public void GivenIHaveSetTheSweepDestination(string category) =>
        Assert.Equal(SweepDestinationOutcome.Chosen, Ledger.SetSweepDestination(category).Outcome);

    // ------------------------------------------------------------------ When

    [When(@"^I set the sweep destination to ""([^""]*)""$")]
    public void WhenISetTheSweepDestination(string category) => Choose(category);

    [When(@"^I remove the sweep destination$")]
    public void WhenIRemoveTheSweepDestination() => Choose(Tekst.NoSweepDestination);

    // Pressed with that period on screen, as the button is only on its line.
    [When(@"^I bring the swept amount of the " + Period + @" up to date$")]
    public void WhenIBringTheSweptAmountUpToDate(string period)
    {
        Show(Ledger.PeriodNamed(period));
        Assert.True(App.Overview.CanBringSweepUpToDate, "Restant bijwerken is not offered.");
        context.Record(App.BringSweepUpToDate() ?? throw new InvalidOperationException("Restant bijwerken moved nothing."));
    }

    // ------------------------------------------------------------------ Then: the list

    [Then(@"^the sweep destination shown in the (current|next) budget period should (?:still )?be ""([^""]*)""$")]
    public void ThenTheSweepDestinationShownShouldBe(string which, string category) =>
        AssertDestinationShown(which, category);

    [Then(@"^the sweep destination shown in the (current|next) budget period should be none$")]
    public void ThenTheSweepDestinationShownShouldBeNone(string which) =>
        AssertDestinationShown(which, Tekst.NoSweepDestination);

    [Then(@"^the choices offered for the sweep destination should be exactly these, in this order:$")]
    public void ThenTheChoicesOfferedForTheSweepDestinationShouldBe(Table table) =>
        Assert.Equal(
            table.Rows.Select(r => r["choice"] is "none" ? Tekst.NoSweepDestination : r["choice"]),
            App.OverviewFor(Ledger.CurrentPeriod).SweepChoices.Select(c => c.Text));

    [Then(@"^the " + Period + @" should offer no sweep destination$")]
    public void ThenThePeriodShouldOfferNoSweepDestination(string period) =>
        Assert.False(OverviewOf(period).ShowsSweepDestination, "The Restant naar list is shown.");

    // ------------------------------------------------------------------ Then: the line

    [Then(@"^the " + Period + @" should (?:still )?show that (\S+) euro was swept into ""([^""]*)""$")]
    public void ThenThePeriodShouldShowThatWasSweptInto(string period, string amount, string category)
    {
        var line = LineOf(period, SweepLineKind.Swept);
        var part = Assert.Single(line.Parts);
        Assert.Equal((category, SpecParsing.MoneyAmount(amount)), (part.Category.Name, part.Amount));
        Assert.False(line.CanBringUpToDate);
    }

    [Then(@"^the " + Period + @" should show that its period leftover was swept into these categories:$")]
    public void ThenThePeriodShouldShowThatItWasSweptIntoThese(string period, Table table)
    {
        var line = LineOf(period, SweepLineKind.Swept);
        Assert.Equal(
            table.Rows.Select(r => (r["category"], SpecParsing.MoneyAmount(r["amount"]))),
            line.Parts.Select(p => (p.Category.Name, p.Amount)));
    }

    [Then(@"^the " + Period + @" should (?:still )?show (\S+) euro of its period leftover still to sweep$")]
    public void ThenThePeriodShouldShowStillToSweep(string period, string amount) =>
        AssertLineAmount(period, SweepLineKind.StillToSweep, amount, Tekst.StillToSweep);

    [Then(@"^the " + Period + @" should (?:still )?show (\S+) euro of its period leftover swept too much$")]
    public void ThenThePeriodShouldShowSweptTooMuch(string period, string amount) =>
        AssertLineAmount(period, SweepLineKind.SweptTooMuch, amount, Tekst.SweptTooMuch);

    [Then(@"^the " + Period + @" should show a period leftover of (\S+) euro, with the marker a category over budget has and the badge ""([^""]*)""$")]
    public void ThenThePeriodShouldShowAShortfall(string period, string amount, string badge)
    {
        var overview = OverviewOf(period);
        var line = LineOf(period, SweepLineKind.Shortfall);
        Assert.Equal(SpecParsing.MoneyAmount(amount), line.Amount);
        Assert.Contains(Tekst.Euro(line.Amount), overview.SweepLineText);
        Assert.Equal(Marker.Over, overview.SweepLineMarker);
        Assert.True(overview.IsSweepLineShort);
        Assert.Equal(Tekst.PeriodShortfall, badge);
    }

    [Then(@"^the " + Period + @" should show no period leftover line$")]
    public void ThenThePeriodShouldShowNoLine(string period)
    {
        var overview = OverviewOf(period);
        Assert.False(overview.HasSweepLine, $"The line reads: {overview.SweepLineText}");
        Assert.Null(overview.SweepLineText);
        Assert.False(overview.CanBringSweepUpToDate);
    }

    [Then(@"^I should (?:still )?be able to bring the swept amount of the " + Period + @" up to date$")]
    public void ThenIShouldBeAbleToBringUpToDate(string period) =>
        Assert.True(OverviewOf(period).CanBringSweepUpToDate, "Restant bijwerken is not offered.");

    [Then(@"^I should (?:still )?not be able to bring the swept amount of the " + Period + @" up to date$")]
    public void ThenIShouldNotBeAbleToBringUpToDate(string period) =>
        Assert.False(OverviewOf(period).CanBringSweepUpToDate, "Restant bijwerken is offered.");

    // ------------------------------------------------------------------ Then: what I am told

    // One notice can carry several sentences — the sweeps of several periods, or a sweep in front
    // of what an act says — so the notice is checked to contain the one asked about.
    [Then(@"^I should be told that the period leftover of the " + Period + @", (\S+) euro, was swept into ""([^""]*)""$")]
    public void ThenIShouldBeToldThatItWasSwept(string period, string amount, string category)
    {
        var swept = Ledger.PeriodNamed(period);
        var sweep = new SweepMade(swept, new Movement(
            0, Ledger.Calendar.Next(swept).FirstDay, CategoryNamed(category), Ledger.PoolAccount, Ledger.PoolAccount,
            SpecParsing.MoneyAmount(amount), MovementReason.Swept, MovementDirection.In, swept.FirstDay));

        AssertToldContains(Tekst.Swept([sweep]));
    }

    [Then(@"^I should be told that (\S+) euro more of the period leftover of the " + Period + @" was swept into ""([^""]*)""$")]
    public void ThenIShouldBeToldThatMoreWasSwept(string amount, string period, string category) =>
        AssertBroughtUpToDate(period, MovementDirection.In, amount, category);

    [Then(@"^I should be told that (\S+) euro swept too much for the " + Period + @" was taken back from ""([^""]*)""$")]
    public void ThenIShouldBeToldThatItWasTakenBack(string amount, string period, string category) =>
        AssertBroughtUpToDate(period, MovementDirection.Out, amount, category);

    [Then(@"^I should be told that the period leftover will go to ""([^""]*)"" from now on$")]
    public void ThenIShouldBeToldTheLeftoverWillGoTo(string category)
    {
        var result = Assert.IsType<SetSweepDestinationResult>(context.LastAttempt);
        Assert.Equal(SweepDestinationOutcome.Chosen, result.Outcome);
        Assert.Equal(category, result.After!.Name);
        AssertToldContains(Tekst.SweepDestinationSet(result));
    }

    [Then(@"^I should be told that the period leftover will go nowhere from now on$")]
    public void ThenIShouldBeToldTheLeftoverWillGoNowhere()
    {
        var result = Assert.IsType<SetSweepDestinationResult>(context.LastAttempt);
        Assert.Equal(SweepDestinationOutcome.Removed, result.Outcome);
        AssertToldContains(Tekst.SweepDestinationSet(result));
    }

    [Then(@"^I should be told that ""([^""]*)"" is no longer the sweep destination$")]
    public void ThenIShouldBeToldItIsNoLongerTheDestination(string category)
    {
        Assert.Null(Ledger.SweepDestination);
        AssertToldContains(Tekst.NoLongerSweepDestination(category));
    }

    // ------------------------------------------------------------------ doing

    // Chooses in the Restant naar list by what the list shows, as the user does. The list is only on
    // the current period and later ones, so an ended period on screen is stepped away from first.
    // The list hands the choice to MoneyBudApp.SetSweepDestination and nothing else (a unit test
    // holds that), so the act is made there, where its result can be kept for the Thens.
    private void Choose(string choice)
    {
        while (!App.Overview.ShowsSweepDestination) App.StepForward();

        var chosen = App.Overview.SweepChoices.SingleOrDefault(c => c.Text == choice)
            ?? throw new InvalidOperationException($"The Restant naar list offers no \"{choice}\".");

        context.Record(App.SetSweepDestination(chosen.Category?.Name));
    }

    private void Show(BudgetPeriod period)
    {
        while (App.ShownPeriod.FirstDay > period.FirstDay) App.StepBack();
        while (App.ShownPeriod.FirstDay < period.FirstDay) App.StepForward();
    }

    private PeriodOverview OverviewOf(string period) => App.OverviewFor(Ledger.PeriodNamed(period));

    private SweepLine LineOf(string period, SweepLineKind kind)
    {
        var overview = OverviewOf(period);
        var line = overview.SweepLine ?? throw new InvalidOperationException($"The {period} shows no line.");
        Assert.Equal(kind, line.Kind);
        Assert.Equal(Tekst.SweepLineText(line), overview.SweepLineText);
        return line;
    }

    private void AssertLineAmount(string period, SweepLineKind kind, string amount, string phrase)
    {
        var line = LineOf(period, kind);
        Assert.Equal(SpecParsing.MoneyAmount(amount), line.Amount);
        Assert.Contains($"{Tekst.Euro(line.Amount)} {phrase}", OverviewOf(period).SweepLineText);
    }

    private void AssertDestinationShown(string which, string text)
    {
        var overview = App.OverviewFor(Ledger.Period(which));
        Assert.True(overview.ShowsSweepDestination, "The Restant naar list is not shown.");
        Assert.Equal(text, overview.ChosenSweepDestination?.Text);
    }

    private void AssertBroughtUpToDate(string period, MovementDirection direction, string amount, string category)
    {
        var result = Assert.IsType<BringUpToDateResult>(context.LastAttempt);
        Assert.Equal(Ledger.PeriodNamed(period), result.Period);
        Assert.Contains(result.Moves, m => m.Direction == direction && m.Category.Name == category
                                           && m.Amount == SpecParsing.MoneyAmount(amount));
        AssertToldContains(Tekst.BroughtUpToDate(result));

        var sentence = direction == MovementDirection.In
            ? $"{Tekst.Euro(SpecParsing.MoneyAmount(amount))} extra"
            : $"{Tekst.Euro(SpecParsing.MoneyAmount(amount))} {Tekst.SweptTooMuch}";
        Assert.Contains(sentence, App.Notice!.Text);
        Assert.Contains($"\"{category}\"", App.Notice.Text);
    }

    private void AssertToldContains(string text)
    {
        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.False(notice.IsRefusal);
        Assert.Contains(text, notice.Text);
    }

    private Category CategoryNamed(string name) =>
        Ledger.CategoriesOffered.Concat(Ledger.ArchivedCategories).SingleOrDefault(c => c.Name == name)
        ?? throw new InvalidOperationException($"There is no category \"{name}\".");
}
