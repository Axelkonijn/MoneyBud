using MoneyBud.Domain;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for the one thing about assigning that the scenarios cannot reach. The
/// scenarios remain the contract (features/assign-to-category.feature) and are not repeated here.
///
/// <para>A scenario names a period as current, previous or next, and every one of those comes
/// from the ledger's own calendar. A caller can still hand <see cref="Ledger.Assign"/> a date
/// range that is no budget period at all — a user cannot, which makes it a non-case with no
/// defined behaviour, and the ledger throws rather than filing a budget under a period that does
/// not exist.</para>
/// </summary>
public class AssignTests
{
    private readonly Ledger ledger = new SpecContext().Ledger;

    public AssignTests() => ledger.AddCategory("Groceries");

    [Fact]
    public void A_date_range_that_is_not_a_budget_period_is_a_caller_mistake()
    {
        var current = ledger.CurrentPeriod;
        var madeUp = new BudgetPeriod(current.FirstDay.AddDays(1), current.LastDay);

        Assert.Throws<ArgumentException>(() => ledger.Assign(50m, "Groceries", madeUp));
        Assert.Equal(Money.Zero, ledger.BudgetFor("Groceries", current));
    }

    // Found in review: the clip wrote a zero Budget even where there had been none, so a
    // category nothing was ever assigned to read as having had a budget set. No scenario reaches
    // it — none asks "never set a budget" after a clip — so it is pinned here.
    [Fact]
    public void Clipping_against_a_category_never_budgeted_leaves_it_never_budgeted()
    {
        var result = ledger.Assign(-20m, "Groceries", ledger.CurrentPeriod);

        Assert.Equal(Money.FromEuros(20m), result.Shortfall);
        Assert.False(ledger.HasBudget("Groceries", ledger.CurrentPeriod));
    }

    [Fact]
    public void A_period_from_a_calendar_with_another_start_day_is_not_one_of_this_ledgers_periods()
    {
        var other = new BudgetPeriodCalendar(startDayOfMonth: 10).PeriodContaining(ledger.Today);

        Assert.Throws<ArgumentException>(() => ledger.Assign(50m, "Groceries", other));
    }
}
