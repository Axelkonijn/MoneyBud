using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for features/assign-to-category.feature.
///
/// <para>The Givens that make a budget are in <see cref="RecordExpenseSteps"/>, where they have
/// lived since the first increment; they now assign, as every such Given means. The steps that
/// several files share — not warned, the cent rule, brought back, not one of my categories — are
/// in <see cref="SharedSteps"/>.</para>
/// </summary>
[Binding]
public sealed class AssignSteps(SpecContext context)
{
    private Ledger Ledger => context.Ledger;

    // ------------------------------------------------------------------ Given

    // Checks, never sets: the feature file's header says so. It states what the income and
    // budgets above it already add up to, so the figure before the act is on the page.
    [Given(@"Unassigned in the (current|previous|next) budget period is (\S+) euro")]
    public void GivenUnassignedIs(string which, string expected) =>
        Assert.Equal(SpecParsing.MoneyAmount(expected), Ledger.UnassignedIn(Ledger.Period(which)));

    // ------------------------------------------------------------------- When

    // "assign" and "try to assign" are one step, for the same reason "record" and "try to
    // record" are: they differ only in what the scenario expects next.
    [When(@"I (?:assign|try to assign) (\S+) euro to ""([^""]*)"" in the (current|previous|next) budget period")]
    public void WhenIAssign(string amount, string category, string which) =>
        Assign(amount, category, Ledger.Period(which));

    // Leaving the period as it starts out, which is the period on screen (step-between-periods.feature).
    [When(@"I (?:assign|try to assign) (\S+) euro to ""([^""]*)"" without naming a budget period")]
    //
    // Read off the assign form, as the Desktop does: it always submits the form's own period, so
    // this proves the form starts out on the period shown and stays with it, midnight included.
    public void WhenIAssignWithoutNamingAPeriod(string amount, string category) =>
        Assign(amount, category, context.App.AssignForm.Period);

    // ------------------------------------------------------------------- Then

    [Then(@"the assignment should go through")]
    public void ThenTheAssignmentShouldGoThrough() =>
        Assert.True(
            context.AssignResult.WasAssigned,
            $"Expected it to go through, but it was refused: {context.AssignResult.Refusal}.");

    [Then(@"the assignment should be refused")]
    public void ThenTheAssignmentShouldBeRefused()
    {
        if (context.LastAttempt is SpecContext.Unread { What: "assignment" }) return;

        Assert.False(context.AssignResult.WasAssigned, "Expected it to be refused, but it went through.");
    }

    [Then(@"I should be told of a shortfall of (\S+) euro")]
    public void ThenIShouldBeToldOfAShortfallOf(string amount) =>
        Assert.Equal(SpecParsing.MoneyAmount(amount), context.AssignResult.Shortfall);

    [Then(@"I should not be told of any shortfall")]
    public void ThenIShouldNotBeToldOfAnyShortfall() =>
        Assert.Equal(Money.Zero, context.AssignResult.Shortfall);

    [Then(@"I should be told that nothing can be assigned in a past budget period")]
    public void ThenIShouldBeToldNothingCanBeAssignedInAPastPeriod() =>
        AssertRefused(AssignRefusal.PeriodInPast);

    [Then(@"I should be told that an assignment needs a category")]
    public void ThenIShouldBeToldAnAssignmentNeedsACategory() =>
        AssertRefused(AssignRefusal.CategoryMissing);

    // Anchored for the reason given above "I have recorded no income" in RecordIncomeSteps: the
    // alternation is the only regex construct here.
    [Then(@"^the (current|previous|next) budget period should be shown as over-assigned$")]
    public void ThenThePeriodShouldBeShownAsOverAssigned(string which) =>
        Assert.Equal(Marker.Over, context.App.OverviewFor(Ledger.Period(which)).UnassignedMarker);

    [Then(@"^the (current|previous|next) budget period should not be shown as over-assigned$")]
    public void ThenThePeriodShouldNotBeShownAsOverAssigned(string which) =>
        Assert.Equal(Marker.None, context.App.OverviewFor(Ledger.Period(which)).UnassignedMarker);

    // ----------------------------------------------------------------- Shared

    // Through the screen, with the amount as typed.
    private void Assign(string amount, string category, BudgetPeriod? period) =>
        context.Record(context.App.Assign(amount, category, period)
            ?? throw new InvalidOperationException($"\"{amount}\" was not read as an amount."));

    private void AssertRefused(AssignRefusal expected)
    {
        Assert.False(context.AssignResult.WasAssigned, "Expected the assignment to be refused, but it went through.");
        Assert.Equal(expected, context.AssignResult.Refusal);
    }
}
