using MoneyBud.Domain;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// The ledger's side of correcting things, below what the scenarios can see: that an entry has an
/// identity a change keeps, that a rename re-keys the one category rather than making another, and
/// that deleting leaves nothing of the category behind. And the non-cases, which throw.
/// </summary>
public sealed class CorrectionTests
{
    private static readonly DateOnly Today = new(2026, 3, 15);

    private readonly Ledger ledger = new(new FixedClock(new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero)));

    private BudgetPeriod Current => ledger.CurrentPeriod;

    // ------------------------------------------------------------------ identity

    [Fact]
    public void Two_identical_expenses_are_two_entries_and_removing_one_leaves_the_other()
    {
        ledger.AddCategory("Groceries");
        var first = ledger.RecordExpense(3.50m, "Groceries", Today, "Coffee").Expense!;
        var second = ledger.RecordExpense(3.50m, "Groceries", Today, "Coffee").Expense!;

        Assert.NotEqual(first, second);

        ledger.RemoveExpense(second);

        Assert.Equal(first, Assert.Single(ledger.ExpensesIn(Current)));
    }

    [Fact]
    public void A_changed_expense_keeps_its_id_and_its_place_in_the_order_recorded()
    {
        ledger.AddCategory("Groceries");
        var bakker = ledger.RecordExpense(18m, "Groceries", Today, "Bakker").Expense!;
        ledger.RecordExpense(3.50m, "Groceries", Today, "Coffee");

        var changed = ledger.ChangeExpense(bakker, 20m, "Groceries", Today, "Bakker").Expense!;

        Assert.Equal(bakker.Id, changed.Id);
        Assert.Equal(["Bakker", "Coffee"], ledger.ExpensesIn(Current).Select(e => e.Label));
        Assert.Equal(Money.FromCents(2000), ledger.ExpensesIn(Current)[0].Amount);
    }

    [Fact]
    public void A_change_made_through_a_stale_copy_of_an_entry_still_finds_it_by_its_id()
    {
        var salaris = ledger.RecordIncome(1800m, "Salaris", Today).Income!;
        ledger.ChangeIncome(salaris, 1900m, "Salaris", Today);

        ledger.RemoveIncome(salaris);

        Assert.Empty(ledger.IncomesIn(Current));
    }

    // Recognised before any check runs, so that nothing can refuse it.
    [Fact]
    public void An_expense_saved_unchanged_is_unchanged_even_when_its_category_is_archived()
    {
        ledger.AddCategory("Hobby");
        var verf = ledger.RecordExpense(25m, "Hobby", Today, "Verf").Expense!;
        ledger.ArchiveCategory("Hobby");

        var result = ledger.ChangeExpense(verf, 25.00m, "  hobby ", Today, "  Verf ");

        Assert.Equal(ChangeOutcome.Unchanged, result.Outcome);
        Assert.True(ledger.IsArchived("Hobby"));
    }

    [Fact]
    public void Changing_or_removing_an_entry_that_is_not_in_the_ledger_throws()
    {
        ledger.AddCategory("Groceries");
        var expense = ledger.RecordExpense(10m, "Groceries", Today).Expense!;
        var income = ledger.RecordIncome(10m, "Loon", Today).Income!;
        ledger.RemoveExpense(expense);
        ledger.RemoveIncome(income);

        Assert.Throws<InvalidOperationException>(() => ledger.ChangeExpense(expense, 12m, "Groceries", Today, null));
        Assert.Throws<InvalidOperationException>(() => ledger.RemoveExpense(expense));
        Assert.Throws<InvalidOperationException>(() => ledger.ChangeIncome(income, 12m, "Loon", Today));
        Assert.Throws<InvalidOperationException>(() => ledger.RemoveIncome(income));
    }

    // ------------------------------------------------------------------ renaming

    [Fact]
    public void A_renamed_category_is_found_by_its_new_name_and_no_longer_by_its_old_one()
    {
        ledger.AddCategory("Groceries");
        ledger.Assign(400m, "Groceries", Current);

        ledger.RenameCategory("groceries", "Food");

        Assert.True(ledger.HasCategory("food"));
        Assert.False(ledger.HasCategory("Groceries"));
        Assert.Equal(Money.FromCents(40000), ledger.BudgetFor("FOOD", Current));
    }

    [Fact]
    public void Renaming_hands_back_the_same_category_with_its_old_name_as_it_was_stored()
    {
        var added = ledger.AddCategory("Vaste  lasten").Category!;

        var result = ledger.RenameCategory("vaste lasten", "Vaste lasten");

        Assert.Same(added, result.Category);
        Assert.Equal("Vaste  lasten", result.OldName);
        Assert.Equal("Vaste lasten", added.Name);
    }

    [Fact]
    public void A_renamed_archived_category_stays_archived()
    {
        ledger.AddCategory("Hobby");
        ledger.ArchiveCategory("Hobby");

        ledger.RenameCategory("Hobby", "Crafts");

        Assert.True(ledger.IsArchived("Crafts"));
        Assert.Empty(ledger.CategoriesOffered);
    }

    [Fact]
    public void Renaming_a_category_that_is_not_one_of_mine_throws() =>
        Assert.Throws<InvalidOperationException>(() => ledger.RenameCategory("Holiday", "Vakantie"));

    // ------------------------------------------------------------------ deleting

    // Assigned and taken back leaves a budget of zero behind, which HasBudget can see. It is not
    // history, so it neither stops the delete nor survives it.
    [Fact]
    public void A_budget_taken_back_to_zero_does_not_stop_a_delete_and_goes_with_the_category()
    {
        ledger.RecordIncome(100m, "Loon", Today);
        ledger.AddCategory("Hobby");
        ledger.Assign(60m, "Hobby", Current);
        ledger.Assign(-60m, "Hobby", Current);
        Assert.True(ledger.HasBudget("Hobby", Current));

        Assert.True(ledger.CanDelete("Hobby"));
        ledger.DeleteCategory("Hobby");
        ledger.AddCategory("Hobby");

        Assert.False(ledger.HasBudget("Hobby", Current));
        Assert.Equal(Money.FromCents(10000), ledger.UnassignedIn(Current));
    }

    [Fact]
    public void A_deleted_archived_category_is_gone_from_the_archive_too()
    {
        ledger.AddCategory("Magazines");
        ledger.ArchiveCategory("Magazines");

        ledger.DeleteCategory("Magazines");

        Assert.Empty(ledger.ArchivedCategories);
        Assert.Equal(AddCategoryOutcome.Created, ledger.AddCategory("Magazines").Outcome);
    }

    [Fact]
    public void Deleting_a_category_with_history_or_one_that_is_not_mine_throws()
    {
        ledger.AddCategory("Hobby");
        ledger.RecordExpense(0.01m, "Hobby", Today);

        Assert.False(ledger.CanDelete("Hobby"));
        Assert.Throws<InvalidOperationException>(() => ledger.DeleteCategory("Hobby"));
        Assert.Throws<InvalidOperationException>(() => ledger.DeleteCategory("Holiday"));
    }
}
