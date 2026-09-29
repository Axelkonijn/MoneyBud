using MoneyBud.Domain;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for backing (arc42 §12, <i>Backing and Accumulated</i>; ADR 0009): what is there
/// for a category, <i>Accumulated</i> in every kind of period, the cap on a negative assignment, and
/// settling — the edges the scenarios reach only one at a time — and the misuse no user can reach,
/// which throws.
/// </summary>
public sealed class BackingTests
{
    private static readonly DateOnly Today = new(2026, 3, 15);

    private readonly FixedClock clock = new(Noon(Today));
    private readonly Ledger ledger;
    private readonly Account deposit;
    private readonly Account broker;

    public BackingTests()
    {
        ledger = new Ledger(clock, "Bank");
        ledger.AddCategory("Savings");
        ledger.AddCategory("Groceries");
        ledger.RecordIncome(2000m, "Salaris", Today);
        deposit = ledger.AddAccount("Deposit", 0m).Account!;
        broker = ledger.AddAccount("Broker", 0m).Account!;
    }

    private Account Bank => ledger.PoolAccount;

    // Read from the clock, so taken before a test moves it.
    private BudgetPeriod March => ledger.CurrentPeriod;

    private BudgetPeriod April => ledger.Calendar.Next(March);

    private static DateTimeOffset Noon(DateOnly day) => new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

    private void MoveTo(DateOnly day) => clock.Now = Noon(day);

    private static Money Euros(decimal euros) => Money.FromEuros(euros);

    // ------------------------------------------------------------------ what is there for a category

    // The glossary's example: 200 moved in, 50 spent from the pool account. Accumulated is 150, and
    // 200 is there.
    [Fact]
    public void What_is_there_counts_only_expenses_paid_from_the_backing_account()
    {
        ledger.Assign(200m, "Savings", March);
        ledger.SetBacking("Savings", deposit);
        ledger.RecordExpense(50m, "Savings", Today, account: Bank);

        Assert.Equal(Euros(200m), ledger.ThereFor("Savings"));
        Assert.Equal(Euros(150m), ledger.AccumulatedFor("Savings", March));
    }

    [Fact]
    public void An_expense_from_a_third_account_is_not_taken_off_what_is_there()
    {
        ledger.Assign(200m, "Savings", March);
        ledger.SetBacking("Savings", deposit);
        ledger.RecordExpense(50m, "Savings", Today, account: broker);

        Assert.Equal(Euros(200m), ledger.ThereFor("Savings"));
    }

    [Fact]
    public void What_is_there_can_be_below_zero_when_the_backing_account_paid_out_more()
    {
        ledger.Assign(200m, "Savings", March);
        ledger.SetBacking("Savings", deposit);
        ledger.RecordExpense(250m, "Savings", Today, account: deposit);

        Assert.Equal(Euros(-50m), ledger.ThereFor("Savings"));
    }

    // The ruling of 2026-09-28: expenses are counted by period, and what the account had already paid
    // in the period when it became the backing account is remembered, so what is there starts at what
    // moved. After re-pointing, the new account's own earlier expenses are remembered the same way.
    [Fact]
    public void What_the_backing_account_paid_in_the_period_before_it_backed_is_not_taken_off_again()
    {
        ledger.Assign(300m, "Savings", March);
        ledger.RecordExpense(100m, "Savings", Today, account: deposit);
        ledger.RecordExpense(40m, "Savings", Today, account: broker);
        ledger.SetBacking("Savings", deposit);

        Assert.Equal(Euros(160m), ledger.ThereFor("Savings"));

        ledger.SetBacking("Savings", broker);

        Assert.Equal(Euros(160m), ledger.ThereFor("Savings"));
        Assert.Equal(Euros(160m), ledger.AccumulatedFor("Savings", March));
    }

    // Counted by direction: money moved from the pool to the pool is there for the category, so
    // pointing the backing elsewhere takes it along.
    [Fact]
    public void When_the_pool_account_backs_a_category_what_was_assigned_is_there_and_goes_along_on_re_pointing()
    {
        ledger.SetBacking("Savings", Bank);
        ledger.Assign(300m, "Savings", March);

        Assert.Equal(Euros(300m), ledger.ThereFor("Savings"));
        Assert.Equal(Euros(2000m), ledger.BalanceOf(Bank));

        var result = ledger.SetBacking("Savings", deposit);

        Assert.Equal(Euros(300m), result.Moved!.Amount);
        Assert.Equal(Euros(1700m), ledger.BalanceOf(Bank));
        Assert.Equal(Euros(300m), ledger.BalanceOf(deposit));
        Assert.Equal(Euros(300m), ledger.AccumulatedFor("Savings", March));
    }

    [Fact]
    public void An_unbacked_category_has_nothing_there_and_no_Accumulated()
    {
        Assert.Null(ledger.ThereFor("Savings"));
        Assert.Null(ledger.AccumulatedFor("Savings", March));
        Assert.Null(ledger.BackingOf("Savings"));
    }

    // ------------------------------------------------------------------ Accumulated in each period

    [Fact]
    public void Accumulated_is_zero_in_a_period_before_anything_moved_and_includes_what_is_planned_in_a_later_one()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(100m, "Savings", March);
        ledger.Assign(40m, "Savings", April);
        ledger.Assign(60m, "Savings", ledger.Calendar.Next(April));

        Assert.Equal(Money.Zero, ledger.AccumulatedFor("Savings", ledger.Calendar.Previous(March)));
        Assert.Equal(Euros(100m), ledger.AccumulatedFor("Savings", March));
        Assert.Equal(Euros(140m), ledger.AccumulatedFor("Savings", April));
        Assert.Equal(Euros(200m), ledger.AccumulatedFor("Savings", ledger.Calendar.Next(April)));
    }

    // Until the screen settles, a period that has begun still counts its money as planned, so the
    // figure does not dip for the minute before the timer's refresh.
    [Fact]
    public void Money_not_yet_settled_is_counted_as_planned_so_Accumulated_does_not_change_when_it_moves()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(40m, "Savings", April);
        MoveTo(April.FirstDay);

        Assert.Equal(Euros(40m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));

        ledger.Settle();

        Assert.Equal(Euros(40m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
    }

    [Fact]
    public void Backing_again_after_unbacking_starts_Accumulated_over_and_re_pointing_does_not()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(100m, "Savings", March);
        ledger.RecordExpense(30m, "Savings", Today, account: Bank);
        ledger.SetBacking("Savings", broker);

        Assert.Equal(Euros(70m), ledger.AccumulatedFor("Savings", March));

        ledger.SetBacking("Savings", null);
        ledger.SetBacking("Savings", deposit);

        // Budget 100, spent 30: 70 remains and moves, and the 30 spent before this backing no
        // longer counts.
        Assert.Equal(Euros(70m), ledger.AccumulatedFor("Savings", March));
        ledger.RecordExpense(10m, "Savings", Today, account: Bank);
        Assert.Equal(Euros(60m), ledger.AccumulatedFor("Savings", March));
    }

    // ------------------------------------------------------------------ a negative assignment

    [Fact]
    public void A_negative_assignment_moves_back_the_smaller_of_what_the_clip_let_through_and_what_is_there()
    {
        ledger.Assign(300m, "Savings", March);
        ledger.RecordExpense(100m, "Savings", Today, account: Bank);
        ledger.SetBacking("Savings", deposit);

        var result = ledger.Assign(-300m, "Savings", March);

        Assert.Equal(Money.Zero, result.Shortfall);
        Assert.Equal(Money.Zero, ledger.BudgetFor("Savings", March));
        Assert.Equal(Money.Zero, ledger.BalanceOf(deposit));
        var back = Assert.Single(ledger.HistoryOf(deposit).OfType<Movement>(), m => m.Direction == MovementDirection.Out);
        Assert.Equal(Euros(200m), back.Amount);
    }

    [Fact]
    public void A_negative_assignment_moves_nothing_when_nothing_is_there()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(100m, "Savings", March);
        ledger.RecordExpense(100m, "Savings", Today, account: deposit);

        ledger.Assign(-50m, "Savings", March);

        Assert.DoesNotContain(ledger.HistoryOf(deposit).OfType<Movement>(), m => m.Direction == MovementDirection.Out);
        Assert.Equal(Euros(50m), ledger.BudgetFor("Savings", March));
    }

    [Fact]
    public void Assigning_in_a_later_period_moves_nothing_today()
    {
        ledger.SetBacking("Savings", deposit);

        ledger.Assign(100m, "Savings", April);
        ledger.Assign(-40m, "Savings", April);

        Assert.Empty(ledger.HistoryOf(deposit).OfType<Movement>());
        Assert.Equal(Euros(60m), ledger.BudgetFor("Savings", April));
    }

    // ------------------------------------------------------------------ settling

    [Fact]
    public void Settling_moves_each_period_s_money_once_on_its_first_day_to_the_backing_of_that_moment()
    {
        var april = April;
        var may = ledger.Calendar.Next(april);
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(40m, "Savings", april);
        ledger.Assign(60m, "Savings", may);
        ledger.SetBacking("Savings", broker);

        MoveTo(may.FirstDay.AddDays(3));

        Assert.True(ledger.Settle());
        Assert.False(ledger.Settle());
        Assert.Equal(
            [(may.FirstDay, 6000L), (april.FirstDay, 4000L)],
            ledger.HistoryOf(broker).OfType<Movement>().Select(m => (m.Date, m.Amount.Cents)));
        Assert.Empty(ledger.HistoryOf(deposit).OfType<Movement>());
    }

    [Fact]
    public void Money_planned_for_a_category_unbacked_since_moves_nowhere()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(40m, "Savings", April);
        ledger.SetBacking("Savings", null);

        MoveTo(April.FirstDay);

        Assert.False(ledger.Settle());
        Assert.Equal(Euros(2000m), ledger.BalanceOf(Bank));
    }

    [Fact]
    public void Every_act_that_changes_the_ledger_settles_first_so_a_balance_correction_on_the_day_holds_the_movement()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(40m, "Savings", April);
        MoveTo(April.FirstDay);

        var correction = ledger.CorrectBalance(deposit, 40m).Correction!;

        Assert.Equal(Money.Zero, ledger.DifferenceOf(correction));
        Assert.Equal(Euros(40m), ledger.BalanceOf(deposit));
    }

    [Fact]
    public void A_clock_turned_back_settles_nothing_and_does_not_forget_what_was_settled()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(40m, "Savings", April);
        MoveTo(April.FirstDay);
        ledger.Settle();

        MoveTo(Today);
        Assert.False(ledger.Settle());
        MoveTo(April.FirstDay.AddDays(1));
        Assert.False(ledger.Settle());
        Assert.Single(ledger.HistoryOf(deposit).OfType<Movement>());
    }

    // ------------------------------------------------------------------ setting the backing

    [Fact]
    public void Choosing_the_backing_already_set_changes_nothing()
    {
        ledger.Assign(100m, "Savings", March);
        ledger.SetBacking("Savings", deposit);

        var again = ledger.SetBacking("Savings", deposit);
        var none = ledger.SetBacking("Groceries", null);

        Assert.Equal(BackingOutcome.Unchanged, again.Outcome);
        Assert.Null(again.Moved);
        Assert.Equal(BackingOutcome.Unchanged, none.Outcome);
        Assert.Single(ledger.HistoryOf(deposit).OfType<Movement>());
    }

    [Fact]
    public void A_movement_from_the_pool_account_to_itself_is_not_money_moved()
    {
        ledger.Assign(100m, "Savings", March);

        var result = ledger.SetBacking("Savings", Bank);

        Assert.NotNull(result.Moved);
        Assert.False(result.MovedMoney);
        Assert.Empty(ledger.HistoryOf(Bank).OfType<Movement>());
    }

    [Fact]
    public void Backing_an_archived_category_leaves_it_archived()
    {
        ledger.ArchiveCategory("Savings");

        ledger.SetBacking("Savings", deposit);

        Assert.True(ledger.IsArchived("Savings"));
        Assert.Equal(deposit, ledger.BackingOf("Savings"));
    }

    [Fact]
    public void An_account_is_in_use_while_it_backs_a_category_or_has_a_movement_on_it()
    {
        ledger.SetBacking("Groceries", broker);
        Assert.False(ledger.CanDeleteAccount(broker));
        ledger.SetBacking("Groceries", null);
        Assert.True(ledger.CanDeleteAccount(broker));

        ledger.SetBacking("Savings", deposit);
        ledger.Assign(10m, "Savings", March);
        ledger.SetBacking("Savings", null);
        Assert.False(ledger.CanDeleteAccount(deposit));
    }

    // Chosen in the build: the history rows name it, so it has history even with its budgets back
    // at zero.
    [Fact]
    public void A_category_money_was_moved_for_cannot_be_deleted_even_with_no_budget_left()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(10m, "Savings", March);
        ledger.Assign(-10m, "Savings", March);

        Assert.Equal(Money.Zero, ledger.BudgetFor("Savings", March));
        Assert.False(ledger.CanDelete("Savings"));
    }

    // Found by the spec review: money moved from the pool account to itself is in no history, so it
    // does not keep a category from being deleted, and goes with it.
    [Fact]
    public void Money_moved_from_the_pool_account_to_itself_does_not_keep_a_category_from_being_deleted()
    {
        ledger.SetBacking("Savings", Bank);
        ledger.Assign(100m, "Savings", March);
        ledger.Assign(-100m, "Savings", March);
        ledger.SetBacking("Savings", null);

        Assert.True(ledger.CanDelete("Savings"));
        ledger.DeleteCategory("Savings");
        Assert.NotNull(Ledger.FromSnapshot(ledger.ToSnapshot(), clock));
    }

    // The same for an account: one that backed a category while it was the pool account keeps only
    // movements to itself, which nobody can see.
    [Fact]
    public void Money_moved_from_an_account_to_itself_does_not_keep_the_account_from_being_deleted()
    {
        ledger.MakePool(broker);
        ledger.SetBacking("Savings", broker);
        ledger.Assign(100m, "Savings", March);
        ledger.SetBacking("Savings", null);
        ledger.MakePool(ledger.AccountNamed("Bank")!);

        Assert.True(ledger.CanDeleteAccount(broker));
        ledger.DeleteAccount(broker);
        Assert.NotNull(Ledger.FromSnapshot(ledger.ToSnapshot(), clock));
    }

    [Fact]
    public void Deleting_a_category_with_no_history_takes_its_backing_with_it()
    {
        ledger.SetBacking("Groceries", broker);

        ledger.DeleteCategory("Groceries");

        Assert.True(ledger.CanDeleteAccount(broker));
    }

    [Fact]
    public void Backing_a_name_that_is_no_category_or_with_an_account_from_another_ledger_throws()
    {
        var other = new Ledger(clock, "Elsewhere").PoolAccount;

        Assert.Throws<InvalidOperationException>(() => ledger.SetBacking("Holiday", deposit));
        Assert.Throws<InvalidOperationException>(() => ledger.SetBacking("Savings", other));
    }
}
