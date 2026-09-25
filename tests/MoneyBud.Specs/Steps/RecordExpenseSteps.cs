using MoneyBud.Domain;
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

    [Given(@"I have a budget of (\S+) euro for ""([^""]*)"" in the (current|previous|next) budget period")]
    public void GivenIHaveABudgetFor(string amount, string category, string which)
    {
        EnsureCategory(category);
        Ledger.SetBudget(category, Ledger.Period(which), SpecParsing.MoneyAmount(amount));
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

    [Then(@"I should be told that ""([^""]*)"" is not one of my categories")]
    public void ThenIShouldBeToldThatIsNotOneOfMyCategories(string category)
    {
        AssertRefused(ExpenseRefusal.UnknownCategory);
        Assert.False(Ledger.HasCategory(category), $"{category} should not be a category.");
    }

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

    [Then(@"""([^""]*)"" should be shown as over budget in the (current|previous|next) budget period")]
    public void ThenShouldBeShownAsOverBudgetIn(string category, string which) =>
        Assert.True(
            Ledger.IsOverBudget(category, Ledger.Period(which)),
            $"{category} should be shown as over budget.");

    [Then(@"""([^""]*)"" should not be shown as over budget")]
    public void ThenShouldNotBeShownAsOverBudget(string category) =>
        Assert.False(
            Ledger.IsOverBudget(category, Ledger.CurrentPeriod),
            $"{category} should not be shown as over budget.");

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

    private void Record(string amount, string? category, string? label, string? date) =>
        context.Record(Ledger.RecordExpense(
            SpecParsing.Amount(amount), category, Ledger.Date(date), label));

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
