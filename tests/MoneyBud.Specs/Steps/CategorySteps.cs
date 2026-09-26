using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for adding, archiving, renaming and deleting categories — features/add-category.feature,
/// archive-category.feature, rename-a-category.feature and delete-a-category.feature — and the
/// category observations record-expense.feature shares with them.
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

    [Given(@"^I have deleted the category ""([^""]*)""$")]
    public void GivenIHaveDeletedTheCategory(string category) => Ledger.DeleteCategory(category);

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
        context.Record(context.App.AddCategory(name));
    }

    [When(@"I archive the category ""([^""]*)""")]
    public void WhenIArchiveTheCategory(string category)
    {
        before = Snapshot();
        context.RecordArchived(context.App.ArchiveCategory(category));
    }

    // From the category's row on screen, as the user renames it: the rename button turns the
    // name into a text box, which starts out holding the name, and what is typed replaces it.
    [When(@"^I (?:rename|try to rename) the category ""([^""]*)"" to ""([^""]*)""$")]
    public void WhenIRenameTheCategory(string category, string newName)
    {
        before = Snapshot();
        var app = context.App;
        Assert.Contains(category, app.Overview.Rows.Select(r => r.Name));

        app.StartRename(category);
        Assert.True(app.Overview.Rows.Single(r => r.Name == category).IsRenaming);
        Assert.Equal(category, app.NewName);

        app.NewName = newName;
        context.Record(app.SaveRename());
    }

    // With the delete button on its row on screen, which is the only way to reach it.
    [When(@"^I delete the category ""([^""]*)""$")]
    public void WhenIDeleteTheCategory(string category)
    {
        before = Snapshot();
        var row = context.App.Overview.Rows.SingleOrDefault(r => r.Name == category);
        Assert.True(row is { CanDelete: true }, $"The row of {category} should carry the delete button.");

        context.RecordDeleted(context.App.DeleteCategory(category));
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

    // Adding's refusal, reached by renaming as well (rename-a-category.feature).
    [Then(@"I should be told that a category needs a name")]
    public void ThenIShouldBeToldThatACategoryNeedsAName()
    {
        if (context.LastAttempt is RenameCategoryResult renamed)
        {
            Assert.Equal(RenameRefusal.NameMissing, renamed.Refusal);
            return;
        }

        var result = LastAdd();
        Assert.True(result.WasRefused, "Expected adding the category to be refused, but it was not.");
        Assert.Equal(CategoryRefusal.NameMissing, result.Refusal);
    }

    // Went through: renamed, or given exactly its own name. Either way the box has closed.
    [Then(@"^the rename should go through$")]
    public void ThenTheRenameShouldGoThrough()
    {
        var result = LastRename();
        Assert.False(result.WasRefused, $"Expected the rename to go through, but it was refused: {result.Refusal}.");
        Assert.Null(context.App.Renaming);
    }

    // Refused, the box stays open on what was typed, to be corrected in place.
    [Then(@"^the rename should be refused$")]
    public void ThenTheRenameShouldBeRefused()
    {
        Assert.True(LastRename().WasRefused, "Expected the rename to be refused, but it went through.");
        Assert.NotNull(context.App.Renaming);
    }

    // From what to what, letter for letter: the old name as it was stored and the new one as
    // now stored, trimmed. The wording is not fixed; that it was said is.
    [Then(@"^I should be told that ""([^""]*)"" was renamed to ""([^""]*)""$")]
    public void ThenIShouldBeToldThatWasRenamedTo(string oldName, string newName)
    {
        var result = LastRename();
        Assert.Equal(RenameOutcome.Renamed, result.Outcome);
        Assert.Equal((oldName, newName), (result.OldName, result.Category!.Name));

        var notice = context.App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.False(notice.IsRefusal);
        Assert.Equal(Tekst.CategoryRenamed(oldName, result.Category), notice.Text);
    }

    [Then(@"^I should be told that the new name is already taken$")]
    public void ThenIShouldBeToldThatTheNewNameIsAlreadyTaken()
    {
        Assert.Equal(RenameRefusal.NameTaken, LastRename().Refusal);
        Assert.True(context.App.Notice?.IsRefusal, "Expected to be told of a refusal.");
    }

    [Then(@"^I should be told that ""([^""]*)"" was deleted$")]
    public void ThenIShouldBeToldThatWasDeleted(string category)
    {
        var deleted = Assert.IsType<SpecContext.Deleted>(context.LastAttempt);
        Assert.Equal(category, deleted.Category.Name);
        Assert.False(Ledger.HasCategory(category), $"{category} should be gone, not archived.");

        var notice = context.App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.False(notice.IsRefusal);
        Assert.Equal(Tekst.CategoryDeleted(deleted.Category), notice.Text);
    }

    // Offered means a row of the category carries the delete button. The rows looked at are
    // those of every period a scenario can name: a category with history anywhere is shown in at
    // least one of them, and one shown in none has no row to carry the button.
    [Then(@"^I should be able to delete the category ""([^""]*)""$")]
    public void ThenIShouldBeAbleToDelete(string category) =>
        Assert.Contains(RowsOf(category), r => r.Name == category && r.CanDelete);

    [Then(@"^I should not be able to delete the category ""([^""]*)""$")]
    public void ThenIShouldNotBeAbleToDelete(string category) =>
        Assert.DoesNotContain(RowsOf(category), r => r.CanDelete);

    [Then(@"my categories should be unchanged")]
    public void ThenMyCategoriesShouldBeUnchanged()
    {
        var (offered, archived) = before ?? throw new InvalidOperationException("Nothing was attempted.");
        var (offeredNow, archivedNow) = Snapshot();

        Assert.Equal(offered, offeredNow);
        Assert.Equal(archived, archivedNow);
    }

    // "Offered" is what the screen suggests (suggest-categories.feature). Recording an expense
    // and assigning are suggested the same list, so both wordings read the one list.

    [Then(@"the categories offered for (?:a new expense|assigning) should include ""([^""]*)""")]
    public void ThenTheCategoriesOfferedShouldInclude(string category) =>
        Assert.Contains(category, Suggested());

    [Then(@"the categories offered for (?:a new expense|assigning) should not include ""([^""]*)""")]
    public void ThenTheCategoriesOfferedShouldNotInclude(string category) =>
        Assert.DoesNotContain(Suggested(), name => CategoryName.Comparer.Equals(name, category));

    [Then(@"the categories offered for (?:a new expense|assigning) should list ""([^""]*)"" once, and no other spelling of it")]
    public void ThenTheCategoriesOfferedShouldListOnce(string category)
    {
        var matching = Suggested().Where(name => CategoryName.Comparer.Equals(name, category));

        Assert.Equal([category], matching);
    }

    [Then(@"^the categories offered for (?:a new expense|assigning) should be exactly these, in any order:$")]
    public void ThenTheCategoriesOfferedShouldBeExactly(Table table)
    {
        var expected = table.Rows.Select(row => row["category"]).Order(StringComparer.Ordinal);

        Assert.Equal(expected, Suggested().Order(StringComparer.Ordinal));
    }

    [Then(@"^the categories offered for (?:a new expense|assigning) should be exactly these, in this order:$")]
    public void ThenTheCategoriesOfferedShouldBeExactlyInOrder(Table table) =>
        Assert.Equal(table.Rows.Select(row => row["category"]), Suggested());

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

    // Whether the period's Overview lists the category — the full display rule (arc42 §12), as
    // the screen applies it. Letter for letter: the name as MoneyBud shows it.
    [Then(@"""([^""]*)"" should be shown in the (current|previous|next) budget period")]
    public void ThenShouldBeShownIn(string category, string which) =>
        Assert.Contains(category, ShownNames(which));

    [Then(@"""([^""]*)"" should not be shown in the (current|previous|next) budget period")]
    public void ThenShouldNotBeShownIn(string category, string which) =>
        Assert.DoesNotContain(ShownNames(which), name => CategoryName.Comparer.Equals(name, category));

    // ----------------------------------------------------------------- Shared

    private IReadOnlyList<string> Suggested() => context.App.CategorySuggestions;

    private IEnumerable<string> ShownNames(string which) =>
        context.App.OverviewFor(Ledger.Period(which)).Rows.Select(r => r.Name);

    private string[] OfferedNames() => Ledger.CategoriesOffered.Select(c => c.Name).ToArray();

    private (string[] Offered, string[] Archived) Snapshot() =>
        (OfferedNames(), Ledger.ArchivedCategories.Select(c => c.Name).ToArray());

    private AddCategoryResult LastAdd() =>
        context.LastAddResult ?? throw new InvalidOperationException("No category has been added yet.");

    private RenameCategoryResult LastRename() =>
        context.LastAttempt as RenameCategoryResult
        ?? throw new InvalidOperationException($"The last thing done was not a rename: {context.LastAttempt}.");

    private IEnumerable<CategoryRow> RowsOf(string category) =>
        new[] { "previous", "current", "next" }
            .SelectMany(which => context.App.OverviewFor(Ledger.Period(which)).Rows)
            .Where(r => CategoryName.Comparer.Equals(r.Name, category));

    private void AssertAdded(AddCategoryOutcome expected, string category)
    {
        var result = LastAdd();
        Assert.False(result.WasRefused, $"Expected the category to be added, but it was refused: {result.Refusal}.");
        Assert.Equal(expected, result.Outcome);
        Assert.Equal(category, result.Category!.Name);
    }
}
