using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

[Binding]
public sealed class RecordExpenseSteps(SpecContext context)
{
    private Ledger Ledger => context.Ledger;

    // ------------------------------------------------------------------ Given

    [Given(@"my budget periods are one month long")]
    public void GivenMyBudgetPeriodsAreOneMonthLong()
    {
        var period = Ledger.CurrentPeriod;
        var next = Ledger.Calendar.Next(period);

        // "One month long" means consecutive periods start a month apart and leave no gap. It
        // does not mean the last day is the first day plus a month minus a day: that is false
        // whenever the start day is later than a short month has, where periods still tile but
        // one of them is longer than the calendar month it sits in.
        Assert.Equal(period.LastDay.AddDays(1), next.FirstDay);
        Assert.Equal(period.FirstDay.AddMonths(1).Month, next.FirstDay.Month);
    }

    [Given(@"I have a category ""([^""]*)""")]
    public void GivenIHaveACategory(string category) => EnsureCategory(category);

    // Neither in use nor archived: an archived category is still one of mine.
    [Given(@"I have no category called ""([^""]*)""")]
    public void GivenIHaveNoCategoryCalled(string category) =>
        Assert.False(Ledger.HasCategory(category), $"{category} should not be a category.");

    // A budget is only ever made by assigning, so that is how this makes one: the amount comes
    // out of that period's Unassigned, as assign-to-category.feature's header says every such
    // Given means. A past period refuses assigning, so a budget there is assigned as of a day
    // inside it — back when it was current, which is what the Given describes.
    //
    // Setup goes through the same door as anything else, so a setup assignment that would be
    // refused, clipped or would bring a category back fails the scenario here rather than
    // quietly making some other state.
    [Given(@"I have a budget of (\S+) euro for ""([^""]*)"" in the (current|previous|next) budget period")]
    public void GivenIHaveABudgetFor(string amount, string category, string which)
    {
        EnsureCategory(category);
        var period = Ledger.Period(which);

        var result = period.FirstDay < Ledger.CurrentPeriod.FirstDay
            ? context.AsIfToday(period.FirstDay, () => Ledger.Assign(SpecParsing.Amount(amount), category, period))
            : Ledger.Assign(SpecParsing.Amount(amount), category, period);

        Assert.True(result.WasAssigned, $"Setting up a budget for {category} was refused: {result.Refusal}.");
        Assert.Equal(Money.Zero, result.Shortfall);
        Assert.False(result.CategoryBroughtBack, $"Setting up a budget brought {category} back.");

        // Assigning adds. A second budget Given for the same category and period would set up
        // the sum rather than the figure written, so say so rather than let it pass.
        Assert.Equal(SpecParsing.MoneyAmount(amount), Ledger.BudgetFor(category, period));
    }

    [Given(@"I have never set a budget for ""([^""]*)""")]
    public void GivenIHaveNeverSetABudgetFor(string category) =>
        Assert.False(
            Ledger.HasBudget(category, Ledger.CurrentPeriod),
            $"{category} should have no budget set.");

    [Given(@"I have already spent (\S+) euro on ""([^""]*)"" in the (current|previous|next) budget period")]
    public void GivenIHaveAlreadySpent(string amount, string category, string which)
    {
        var period = Ledger.Period(which);
        EnsureCategory(category);

        // Setup goes through the same door as anything else, so a setup expense that would be
        // refused fails the scenario here rather than quietly not existing.
        var result = Ledger.RecordExpense(
            SpecParsing.Amount(amount), category, Ledger.ADayInside(period), label: null);

        Assert.True(
            result.WasRecorded,
            $"Setting up spending on {category} was refused: {result.Refusal}.");
    }

    [Given(@"I have not yet spent anything on ""([^""]*)"" in the (current|previous|next) budget period")]
    public void GivenIHaveNotYetSpentAnythingOn(string category, string which)
    {
        EnsureCategory(category);
        AssertNothingSpentOn(category, Ledger.Period(which));
    }

    [Given(@"I have not yet spent anything on ""([^""]*)"" in either budget period")]
    public void GivenIHaveNotYetSpentAnythingOnInEitherPeriod(string category)
    {
        EnsureCategory(category);
        AssertNothingSpentOn(category, Ledger.Period("previous"));
        AssertNothingSpentOn(category, Ledger.Period("current"));
    }

    // ------------------------------------------------------------------- When

    // One grammar for recording, in the variants the feature file actually uses. "record" and
    // "try to record" are one step, not two: they differ only in what the scenario expects to
    // happen next, and the domain is asked the same question either way.
    //
    // These are separate bindings rather than one pattern with optional groups because Reqnroll
    // passes only the groups that matched — an unmatched optional group changes the method's
    // arity rather than arriving as null.

    [When(@"I (?:record|try to record) an expense of (\S+) euro for ""([^""]*)""")]
    public void WhenIRecordAnExpenseFor(string amount, string category) =>
        Record(amount, category, label: null, date: null);

    [When(@"I (?:record|try to record) an expense of (\S+) euro for ""([^""]*)"" without a label")]
    public void WhenIRecordAnExpenseWithoutALabel(string amount, string category) =>
        Record(amount, category, label: null, date: null);

    [When(@"I (?:record|try to record) an expense of (\S+) euro for ""([^""]*)"" labelled ""([^""]*)""")]
    public void WhenIRecordAnExpenseLabelled(string amount, string category, string label) =>
        Record(amount, category, label, date: null);

    [When(@"I (?:record|try to record) an expense of (\S+) euro for ""([^""]*)"" labelled ""([^""]*)"" dated (?:on )?(.+)")]
    public void WhenIRecordAnExpenseLabelledAndDated(
        string amount, string category, string label, string date) =>
        Record(amount, category, label, date);

    [When(@"I (?:record|try to record) an expense of (\S+) euro for ""([^""]*)"" dated (?:on )?(.+)")]
    public void WhenIRecordAnExpenseDated(string amount, string category, string date) =>
        Record(amount, category, label: null, date);

    [When(@"I (?:record|try to record) an expense of (\S+) euro without naming a category")]
    public void WhenIRecordAnExpenseWithoutNamingACategory(string amount) =>
        Record(amount, category: null, label: null, date: null);

    // Leaving the date as it starts out, which is today whatever period is on screen
    // (step-between-periods.feature). Every variant above that names no date means the same.
    [When(@"I (?:record|try to record) an expense of (\S+) euro for ""([^""]*)"" labelled ""([^""]*)"" without giving a date")]
    public void WhenIRecordAnExpenseLabelledWithoutGivingADate(string amount, string category, string label)
    {
        // The form's date starts out empty whatever period is shown; empty is what means today.
        Assert.Null(context.App.ExpenseForm.Date);
        Record(amount, category, label, date: null);
    }

    // ------------------------------------------------------------------- Then

    [Then(@"the expense should be recorded")]
    public void ThenTheExpenseShouldBeRecorded() =>
        Assert.True(
            context.ExpenseResult.WasRecorded,
            $"Expected it to be recorded, but it was refused: {context.ExpenseResult.Refusal}.");

    [Then(@"the expense should not be recorded")]
    public void ThenTheExpenseShouldNotBeRecorded() =>
        Assert.False(context.ExpenseResult.WasRecorded, "Expected it not to be recorded, but it was.");

    [Then(@"I should be told that an expense needs a category")]
    public void ThenIShouldBeToldAnExpenseNeedsACategory() =>
        AssertRefused(ExpenseRefusal.CategoryMissing);

    [Then(@"I should be told that an expense must be more than 0 euro")]
    public void ThenIShouldBeToldAnExpenseMustBeMoreThanZero() =>
        AssertRefused(ExpenseRefusal.AmountNotPositive);

    [Then(@"I should be told that an expense cannot be dated in the future")]
    public void ThenIShouldBeToldAnExpenseCannotBeDatedInTheFuture() =>
        AssertRefused(ExpenseRefusal.DateInFuture);

    // Both this and the remaining-budget step below are used by record-income.feature too,
    // which asserts that recording income leaves the plan layer alone.
    [Then(@"the budget for ""([^""]*)"" in the (current|previous|next) budget period should (?:still )?be (\S+) euro")]
    public void ThenTheBudgetForShouldBe(string category, string which, string expected) =>
        Assert.Equal(
            SpecParsing.MoneyAmount(expected),
            Ledger.BudgetFor(category, Ledger.Period(which)));

    [Then(@"the remaining ""([^""]*)"" budget in the (current|previous|next) budget period should (?:still )?be (\S+) euro")]
    public void ThenTheRemainingBudgetShouldBe(string category, string which, string expected) =>
        Assert.Equal(
            SpecParsing.MoneyAmount(expected),
            Ledger.RemainingFor(category, Ledger.Period(which)));

    [Then(@"""([^""]*)"" should be shown as over budget")]
    public void ThenShouldBeShownAsOverBudget(string category) =>
        ThenShouldBeShownAsOverBudgetIn(category, "current");

    // Shown means on the period's Overview: the category's row is there, and carries the marker.
    [Then(@"""([^""]*)"" should be shown as over budget in the (current|previous|next) budget period")]
    public void ThenShouldBeShownAsOverBudgetIn(string category, string which)
    {
        var row = RowOf(category, which);
        Assert.True(row is not null, $"{category} should be shown in the {which} budget period.");
        Assert.Equal(Marker.Over, row.Marker);
    }

    // Holds as well when the category is not listed at all: nothing is shown as over budget then.
    [Then(@"""([^""]*)"" should not be shown as over budget")]
    public void ThenShouldNotBeShownAsOverBudget(string category) =>
        Assert.NotEqual(Marker.Over, RowOf(category, "current")?.Marker);

    [Then(@"my ""([^""]*)"" spending in the (current|previous|next) budget period should include (\d+) expenses of (\S+) euro")]
    public void ThenMySpendingShouldIncludeNExpensesOf(
        string category, string which, int count, string amount)
    {
        var matching = Ledger
            .ExpensesFor(category, Ledger.Period(which))
            .Count(e => e.Amount == SpecParsing.MoneyAmount(amount));

        Assert.Equal(count, matching);
    }

    [Then(@"my ""([^""]*)"" spending in the (current|previous|next) budget period should include (\S+) euro without a label")]
    public void ThenMySpendingShouldIncludeUnlabelled(string category, string which, string amount) =>
        Assert.Contains(
            Ledger.ExpensesFor(category, Ledger.Period(which)),
            e => e.Amount == SpecParsing.MoneyAmount(amount) && e.Label is null);

    [Then(@"my ""([^""]*)"" spending in the (current|previous|next) budget period should include (\S+) euro labelled ""([^""]*)""")]
    public void ThenMySpendingShouldIncludeLabelled(
        string category, string which, string amount, string label) =>
        Assert.Contains(
            Ledger.ExpensesFor(category, Ledger.Period(which)),
            e => e.Amount == SpecParsing.MoneyAmount(amount) && e.Label == label);

    // ----------------------------------------------------------------- Shared

    private CategoryRow? RowOf(string category, string which) =>
        context.App.OverviewFor(Ledger.Period(which)).Rows.SingleOrDefault(r => r.Name == category);

    // Through the screen, with the amount as typed. A date the step does not name is left out
    // rather than filled in here, so that the screen's own default is what decides it.
    private void Record(string amount, string? category, string? label, string? date) =>
        context.Record(context.App.RecordExpense(
            amount, category, label, date is null ? null : Ledger.Date(date))
            ?? throw new InvalidOperationException($"\"{amount}\" was not read as an amount."));

    private void AssertRefused(ExpenseRefusal expected)
    {
        Assert.False(
            context.ExpenseResult.WasRecorded, "Expected the expense to be refused, but it was recorded.");
        Assert.Equal(expected, context.ExpenseResult.Refusal);
    }

    /// <summary>
    /// Makes sure a category exists, for setup. Never brings an archived one back: every scenario
    /// gives a category its history before archiving it, so a setup step that finds the category
    /// archived means the Givens are in the wrong order — and quietly bringing it back would make
    /// the scenario pass for the wrong reason.
    /// </summary>
    internal static void EnsureCategory(Ledger ledger, string category)
    {
        var result = ledger.AddCategory(category);

        Assert.False(result.WasRefused, $"Setting up the category \"{category}\" was refused: {result.Refusal}.");
        Assert.NotEqual(AddCategoryOutcome.BroughtBack, result.Outcome);
    }

    private void EnsureCategory(string category) => EnsureCategory(Ledger, category);

    private void AssertNothingSpentOn(string category, BudgetPeriod period) =>
        Assert.Equal(Money.Zero, Ledger.SpentOn(category, period));
}
