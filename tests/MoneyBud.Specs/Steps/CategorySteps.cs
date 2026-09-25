using MoneyBud.Domain;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for adding and archiving categories — features/add-category.feature and
/// features/archive-category.feature — and the category observations record-expense.feature
/// shares with them.
///
/// <para>How names are checked follows the key in add-category.feature's header, and the
/// difference between its steps is deliberate. "should include X" is letter for letter, the name
/// exactly as MoneyBud shows it. "should not include X" means no spelling of the name, under the
/// name rule. "should list X once, and no other spelling of it" is both at once: exactly one
/// offered category matches X under the rule, and it is spelled exactly X.</para>
/// </summary>
[Binding]
public sealed class CategorySteps(SpecContext context)
{
    private Ledger Ledger => context.Ledger;

    /// <summary>What the categories were before the <c>When</c>, for "should be unchanged".</summary>
    private (string[] Offered, string[] Archived)? before;

    // ------------------------------------------------------------------ Given

    [Given(@"I have just started using MoneyBud for the first time")]
    public void GivenIHaveJustStartedUsingMoneyBudForTheFirstTime() =>
        context.StartUsingMoneyBudForTheFirstTime();

    [Given(@"I have archived the category ""([^""]*)""")]
    public void GivenIHaveArchivedTheCategory(string category) => Ledger.ArchiveCategory(category);

    [Given(@"I have set no budget for ""([^""]*)"" in the (current|previous|next) budget period")]
    public void GivenIHaveSetNoBudgetFor(string category, string which)
    {
        RecordExpenseSteps.EnsureCategory(Ledger, category);
        Assert.False(
            Ledger.HasBudget(category, Ledger.Period(which)),
            $"{category} should have no budget set in the {which} budget period.");
    }

    // ------------------------------------------------------------------- When

    [When(@"I start using MoneyBud for the first time")]
    public void WhenIStartUsingMoneyBudForTheFirstTime() =>
        context.StartUsingMoneyBudForTheFirstTime();

    // "add" and "try to add" are one step, for the same reason "record" and "try to record" are:
    // they differ only in what the scenario expects next.
    [When(@"I (?:add|try to add) a category ""([^""]*)""")]
    public void WhenIAddACategory(string name)
    {
        before = Snapshot();
        context.Record(Ledger.AddCategory(name));
    }

    [When(@"I archive the category ""([^""]*)""")]
    public void WhenIArchiveTheCategory(string category)
    {
        before = Snapshot();
        context.RecordArchived(Ledger.ArchiveCategory(category));
    }

    // ------------------------------------------------------------------- Then

    [Then(@"I should be told that ""([^""]*)"" was created")]
    public void ThenIShouldBeToldThatWasCreated(string category) =>
        AssertAdded(AddCategoryOutcome.Created, category);

    [Then(@"I should be told that ""([^""]*)"" was already there")]
    public void ThenIShouldBeToldThatWasAlreadyThere(string category) =>
        AssertAdded(AddCategoryOutcome.AlreadyThere, category);

    [Then(@"I should be told that ""([^""]*)"" was archived")]
    public void ThenIShouldBeToldThatWasArchived(string category)
    {
        var archived = context.LastArchived ?? throw new InvalidOperationException("Nothing was archived.");
        Assert.Equal(category, archived.Name);
        Assert.True(Ledger.IsArchived(category), $"{category} should be archived.");
    }

    [Then(@"I should be told that a category needs a name")]
    public void ThenIShouldBeToldThatACategoryNeedsAName()
    {
        var result = LastAdd();
        Assert.True(result.WasRefused, "Expected adding the category to be refused, but it was not.");
        Assert.Equal(CategoryRefusal.NameMissing, result.Refusal);
    }

    [Then(@"my categories should be unchanged")]
    public void ThenMyCategoriesShouldBeUnchanged()
    {
        var (offered, archived) = before ?? throw new InvalidOperationException("Nothing was attempted.");
        var (offeredNow, archivedNow) = Snapshot();

        Assert.Equal(offered, offeredNow);
        Assert.Equal(archived, archivedNow);
    }

    [Then(@"the categories offered for a new expense should include ""([^""]*)""")]
    public void ThenTheCategoriesOfferedShouldInclude(string category) =>
        Assert.Contains(category, OfferedNames());

    [Then(@"the categories offered for a new expense should not include ""([^""]*)""")]
    public void ThenTheCategoriesOfferedShouldNotInclude(string category) =>
        Assert.DoesNotContain(OfferedNames(), name => CategoryName.Comparer.Equals(name, category));

    [Then(@"the categories offered for a new expense should list ""([^""]*)"" once, and no other spelling of it")]
    public void ThenTheCategoriesOfferedShouldListOnce(string category)
    {
        var matching = OfferedNames().Where(name => CategoryName.Comparer.Equals(name, category));

        Assert.Equal([category], matching);
    }

    [Then(@"the categories offered for a new expense should be exactly these, in any order:")]
    public void ThenTheCategoriesOfferedShouldBeExactly(Table table)
    {
        var expected = table.Rows.Select(row => row["category"]).Order(StringComparer.Ordinal);

        Assert.Equal(expected, OfferedNames().Order(StringComparer.Ordinal));
    }

    [Then(@"no category should have a budget or any spending in the current budget period")]
    public void ThenNoCategoryShouldHaveABudgetOrAnySpending()
    {
        var period = Ledger.CurrentPeriod;

        foreach (var category in Ledger.CategoriesOffered.Concat(Ledger.ArchivedCategories))
        {
            Assert.False(Ledger.HasBudget(category.Name, period), $"{category.Name} should have no budget.");
            Assert.Equal(Money.Zero, Ledger.SpentOn(category.Name, period));
        }
    }

    // The full rule (arc42 §12): a category is shown in a period where it has history, and one in
    // use is also shown in the current period and every later one. No view implements it yet, so
    // these steps are bound to the ledger's facts and cover what the approved scenarios ask:
    // history is enough for any category to be shown, and an ARCHIVED category without history is
    // not. "Should not be shown" fails loudly if asked of a category in use rather than
    // re-implement the rule here — when a period view exists, rebind both steps to it (§11).
    [Then(@"""([^""]*)"" should be shown in the (current|previous|next) budget period")]
    public void ThenShouldBeShownIn(string category, string which)
    {
        Assert.True(Ledger.HasCategory(category), $"{category} should be one of my categories.");
        Assert.True(
            Ledger.HasHistoryIn(category, Ledger.Period(which)),
            $"{category} should be shown in the {which} budget period, so it needs history there.");
    }

    [Then(@"""([^""]*)"" should not be shown in the (current|previous|next) budget period")]
    public void ThenShouldNotBeShownIn(string category, string which)
    {
        Assert.True(
            Ledger.IsArchived(category),
            $"Until a period view exists this step answers only for an archived category " +
            $"(see the comment above), and {category} is not archived.");
        Assert.False(
            Ledger.HasHistoryIn(category, Ledger.Period(which)),
            $"{category} has history in the {which} budget period, so it is still shown there.");
    }

    // ----------------------------------------------------------------- Shared

    private string[] OfferedNames() => Ledger.CategoriesOffered.Select(c => c.Name).ToArray();

    private (string[] Offered, string[] Archived) Snapshot() =>
        (OfferedNames(), Ledger.ArchivedCategories.Select(c => c.Name).ToArray());

    private AddCategoryResult LastAdd() =>
        context.LastAddResult ?? throw new InvalidOperationException("No category has been added yet.");

    private void AssertAdded(AddCategoryOutcome expected, string category)
    {
        var result = LastAdd();
        Assert.False(result.WasRefused, $"Expected the category to be added, but it was refused: {result.Refusal}.");
        Assert.Equal(expected, result.Outcome);
        Assert.Equal(category, result.Category!.Name);
    }
}
