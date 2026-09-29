using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for features/take-over-a-plan.feature. The <c>plan</c> column of the categories table
/// is read by <see cref="ScreenSteps"/>, where that step lives, and a budget further back than the
/// previous period is made by <see cref="RecordExpenseSteps"/>, with the other budget Givens.
///
/// <para>Like the other screen steps, everything here reads <see cref="MoneyBudApp"/>, never the
/// ledger: whether a plan is offered is a claim about what the Overview shows. "Offered in the
/// next budget period" is the Overview that period would draw, read without stepping to it.</para>
/// </summary>
[Binding]
public sealed class TakeOverSteps(SpecContext context)
{
    private Ledger Ledger => context.Ledger;
    private MoneyBudApp App => context.App;

    // ------------------------------------------------------------------ Given

    // Through the form's own buttons, as the Desktop moves it; the screen stays where it is.
    [Given(@"^I have set the period to assign in to the " + SpecParsing.PeriodPhrase + @", and assigned nothing$")]
    public void GivenIHaveSetThePeriodToAssignIn(string period)
    {
        var shown = App.ShownPeriod;
        var target = Ledger.PeriodNamed(period);
        var form = App.AssignForm;

        while (form.Period.FirstDay > target.FirstDay) form.EarlierPeriodCommand.Execute(null);
        while (form.Period.FirstDay < target.FirstDay) form.LaterPeriodCommand.Execute(null);

        Assert.Equal(target, form.Period);
        Assert.Equal(shown, App.ShownPeriod);
    }

    // ------------------------------------------------------------------- When

    // Pressing the button, so it must be there to press.
    [When(@"^I take over the plan offered$")]
    public void WhenITakeOverThePlanOffered()
    {
        Assert.True(App.Overview.HasOffer, "No plan is offered on screen, so there is no button to press.");
        context.Record(App.TakeOverPlan());
    }

    // The button as it was last drawn: after a period boundary the screen may not have caught up,
    // so what is offered now is not asked first. Every budget is noted beforehand, so that a
    // refusal can be shown to have assigned nothing anywhere.
    [When(@"^I try to take over the plan offered$")]
    public void WhenITryToTakeOverThePlanOffered()
    {
        budgetsBefore = Ledger.ToSnapshot().Budgets;
        context.Record(App.TakeOverPlan());
    }

    private IReadOnlyList<BudgetSnapshot>? budgetsBefore;

    // ------------------------------------------------------------------- Then: the offer

    [Then(@"^the plan of the (.+) should be offered in the (current|previous|next) budget period, with a total of (\S+) euro$")]
    public void ThenThePlanShouldBeOffered(string from, string which, string total)
    {
        var overview = App.OverviewFor(Ledger.Period(which));
        var offer = overview.Offer ?? throw new InvalidOperationException($"No plan is offered in the {which} budget period.");
        var source = Ledger.PeriodNamed(from);

        Assert.Equal(source, offer.From);
        Assert.Equal(SpecParsing.MoneyAmount(total), offer.Total);

        // The button names both. Its wording is copy; the period and the amount are what is fixed.
        Assert.True(overview.HasOffer);
        Assert.Contains(Tekst.PeriodName(source), overview.OfferText);
        Assert.Contains(Tekst.Euro(offer.Total), overview.OfferText);
    }

    [Then(@"^no plan should be offered in the (current|previous|next) budget period$")]
    public void ThenNoPlanShouldBeOffered(string which) =>
        AssertNoOffer(App.OverviewFor(Ledger.Period(which)));

    [Then(@"^the period on screen should offer no plan$")]
    public void ThenThePeriodOnScreenShouldOfferNoPlan() => AssertNoOffer(App.Overview);

    [Then(@"^no category row in the (current|previous|next) budget period should show a plan figure$")]
    public void ThenNoCategoryRowShouldShowAPlanFigure(string which) =>
        AssertNoPlanFigures(App.OverviewFor(Ledger.Period(which)));

    [Then(@"^no category row on screen should show a plan figure$")]
    public void ThenNoCategoryRowOnScreenShouldShowAPlanFigure() => AssertNoPlanFigures(App.Overview);

    // ------------------------------------------------------------------- Then: taking it over

    [Then(@"^the take-over should go through$")]
    public void ThenTheTakeOverShouldGoThrough()
    {
        var result = TakeOverResult();
        Assert.True(result.WasTakenOver, $"Expected the take-over to go through, but it was refused: {result.Refusal}.");
    }

    // Refused, and nothing assigned anywhere: every budget in every period is as it was.
    [Then(@"^the take-over should be refused$")]
    public void ThenTheTakeOverShouldBeRefused()
    {
        Assert.False(TakeOverResult().WasTakenOver, "Expected the take-over to be refused, but it went through.");

        var before = budgetsBefore ?? throw new InvalidOperationException("The take-over was not tried by \"I try to take over the plan offered\".");
        Assert.Equal(before, Ledger.ToSnapshot().Budgets);
    }

    // That I am told, and which period is named. The notice is checked against the sentence
    // Tekst makes for this take-over, which is also how it is told apart from any other notice.
    [Then(@"^I should be told that the plan was taken over into the (current|previous|next) budget period$")]
    public void ThenIShouldBeToldThatThePlanWasTakenOverInto(string which)
    {
        var result = TakeOverResult();
        var period = Ledger.Period(which);
        Assert.True(result.WasTakenOver, $"The take-over was refused: {result.Refusal}.");
        Assert.Equal(period, result.Into);

        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.False(notice.IsRefusal);
        Assert.Equal(Tekst.PlanTakenOver(result.Plan!, period), notice.Text);
        Assert.Contains(Tekst.PeriodName(period), notice.Text);
    }

    // ----------------------------------------------------------------- Shared

    private TakeOverPlanResult TakeOverResult() =>
        context.LastAttempt as TakeOverPlanResult
        ?? throw new InvalidOperationException("The last thing done was not taking over a plan.");

    private static void AssertNoOffer(PeriodOverview overview)
    {
        Assert.Null(overview.Offer);
        Assert.False(overview.HasOffer);
        Assert.Null(overview.OfferText);
    }

    // A past period can have no rows at all, when no category has history there; then no row
    // shows a figure either.
    private static void AssertNoPlanFigures(PeriodOverview overview) =>
        Assert.All(overview.Rows, row => ScreenSteps.AssertPlanFigure(null, row));
}
