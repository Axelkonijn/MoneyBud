using MoneyBud.Domain;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

[Binding]
public sealed class RecordIncomeSteps(SpecContext context)
{
    private Ledger Ledger => context.Ledger;

    // ------------------------------------------------------------------ Given

    // Anchored, where the neighbouring steps are not, and deliberately so: an alternation
    // is this pattern's only regex construct, and without anchors Reqnroll reads the
    // whole thing as a Cucumber Expression, in which "(...)" means optional text rather
    // than a choice. The other steps carry \S or a character class, which settles it.
    [Given(@"^I have recorded no income in the (current|previous|next) budget period$")]
    public void GivenIHaveRecordedNoIncomeIn(string which) =>
        Assert.Equal(Money.Zero, Ledger.UnassignedIn(Ledger.Period(which)));

    [Given(@"I have already recorded (\S+) euro of income in the (current|previous|next) budget period")]
    public void GivenIHaveAlreadyRecordedIncomeIn(string amount, string which)
    {
        var period = Ledger.Period(which);

        // Setup goes through the same door as anything else, so a setup income that would be
        // refused fails the scenario here rather than quietly not existing.
        var result = Ledger.RecordIncome(
            SpecParsing.Amount(amount), "Eerder ontvangen", Ledger.ADayInside(period));

        Assert.True(result.WasRecorded, $"Setting up income was refused: {result.Refusal}.");
    }

    // Moving "today" to the start of the period is what makes "the last day of the current
    // budget period" unambiguously still to come. The period itself does not change: its first
    // day is inside it, so the ledger still reports the same current period afterwards.
    [Given(@"today is the first day of the current budget period")]
    public void GivenTodayIsTheFirstDayOfTheCurrentBudgetPeriod() =>
        context.SetToday(Ledger.CurrentPeriod.FirstDay);

    // ------------------------------------------------------------------- When

    // The same grammar as recording an expense, in the variants the feature file uses, and for
    // the same reasons — see the note above the When steps in RecordExpenseSteps.

    [When(@"I (?:record|try to record) an income of (\S+) euro labelled ""([^""]*)""")]
    public void WhenIRecordAnIncomeLabelled(string amount, string label) =>
        Record(amount, label, date: null);

    [When(@"I (?:record|try to record) an income of (\S+) euro labelled ""([^""]*)"" dated (?:on )?(.+)")]
    public void WhenIRecordAnIncomeLabelledAndDated(string amount, string label, string date) =>
        Record(amount, label, date);

    [When(@"I (?:record|try to record) an income of (\S+) euro without a label")]
    public void WhenIRecordAnIncomeWithoutALabel(string amount) =>
        Record(amount, label: null, date: null);

    // ------------------------------------------------------------------- Then

    [Then(@"the income should be recorded")]
    public void ThenTheIncomeShouldBeRecorded() =>
        Assert.True(
            context.IncomeResult.WasRecorded,
            $"Expected it to be recorded, but it was refused: {context.IncomeResult.Refusal}.");

    [Then(@"the income should not be recorded")]
    public void ThenTheIncomeShouldNotBeRecorded() =>
        Assert.False(context.IncomeResult.WasRecorded, "Expected it not to be recorded, but it was.");

    [Then(@"I should be told that an income needs a label")]
    public void ThenIShouldBeToldAnIncomeNeedsALabel() => AssertRefused(IncomeRefusal.LabelMissing);

    [Then(@"I should be told that an income must be more than 0 euro")]
    public void ThenIShouldBeToldAnIncomeMustBeMoreThanZero() =>
        AssertRefused(IncomeRefusal.AmountNotPositive);

    [Then(@"Unassigned in the (current|previous|next) budget period should (?:still )?be (\S+) euro")]
    public void ThenUnassignedShouldBe(string which, string expected) =>
        Assert.Equal(SpecParsing.MoneyAmount(expected), Ledger.UnassignedIn(Ledger.Period(which)));

    [Then(@"my income in the (current|previous|next) budget period should include (\d+) incomes of (\S+) euro")]
    public void ThenMyIncomeShouldIncludeNIncomesOf(string which, int count, string amount)
    {
        var matching = Ledger
            .IncomesIn(Ledger.Period(which))
            .Count(i => i.Amount == SpecParsing.MoneyAmount(amount));

        Assert.Equal(count, matching);
    }

    [Then(@"my income in the (current|previous|next) budget period should include (\S+) euro labelled ""([^""]*)""")]
    public void ThenMyIncomeShouldIncludeLabelled(string which, string amount, string label) =>
        Assert.Contains(
            Ledger.IncomesIn(Ledger.Period(which)),
            i => i.Amount == SpecParsing.MoneyAmount(amount) && i.Label == label);

    // ----------------------------------------------------------------- Shared

    private void Record(string amount, string? label, string? date) =>
        context.Record(Ledger.RecordIncome(SpecParsing.Amount(amount), label, Ledger.Date(date)));

    private void AssertRefused(IncomeRefusal expected)
    {
        Assert.False(
            context.IncomeResult.WasRecorded, "Expected the income to be refused, but it was recorded.");
        Assert.Equal(expected, context.IncomeResult.Refusal);
    }
}
