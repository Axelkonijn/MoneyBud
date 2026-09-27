using MoneyBud.Domain;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for accounts (arc42 §12, <i>Accounts and net worth</i>; ADR 0008): the worked-out
/// balance at the edges the scenarios reach through the screen, and the misuse no user can reach,
/// which throws.
/// </summary>
public sealed class AccountTests
{
    private static readonly DateOnly Today = new(2026, 3, 15);
    private static readonly DateOnly Yesterday = Today.AddDays(-1);

    private readonly FixedClock clock = new(Noon(Today));
    private readonly Ledger ledger;

    public AccountTests()
    {
        ledger = new Ledger(clock, "Bank");
        ledger.AddCategory("Groceries");
    }

    private Account Bank => ledger.PoolAccount;

    private static DateTimeOffset Noon(DateOnly day) => new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

    // ------------------------------------------------------------------ what a typed balance holds

    [Fact]
    public void With_no_typed_balance_the_balance_is_the_sum_of_what_is_on_the_account_whatever_the_dates()
    {
        ledger.RecordIncome(100m, "Salaris", Today.AddYears(-1));
        ledger.RecordExpense(30m, "Groceries", Yesterday);

        Assert.Equal(Money.FromCents(7000), ledger.BalanceOf(Bank));
    }

    [Fact]
    public void A_typed_balance_holds_what_is_dated_before_its_day_whenever_it_was_recorded()
    {
        ledger.CorrectBalance(Bank, 1000m);
        ledger.RecordExpense(50m, "Groceries", Yesterday);

        Assert.Equal(Money.FromCents(100000), ledger.BalanceOf(Bank));
    }

    [Fact]
    public void On_its_own_day_a_typed_balance_holds_only_what_was_recorded_before_it()
    {
        ledger.RecordExpense(10m, "Groceries", Today);
        ledger.CorrectBalance(Bank, 1000m);
        ledger.RecordExpense(30m, "Groceries", Today);

        Assert.Equal(Money.FromCents(97000), ledger.BalanceOf(Bank));
    }

    [Fact]
    public void A_changed_entry_keeps_the_moment_it_was_first_recorded()
    {
        ledger.CorrectBalance(Bank, 1000m);
        var late = ledger.RecordExpense(30m, "Groceries", Yesterday).Expense!;
        Assert.Equal(Money.FromCents(100000), ledger.BalanceOf(Bank));

        // Moved onto the correction's day, it was still recorded after it, so it moves the balance.
        ledger.ChangeExpense(late, 30m, "Groceries", Today, null);
        Assert.Equal(Money.FromCents(97000), ledger.BalanceOf(Bank));
    }

    [Fact]
    public void The_latest_typed_balance_counts_and_removing_it_goes_back_to_the_one_before()
    {
        ledger.CorrectBalance(Bank, 1000m);
        var second = ledger.CorrectBalance(Bank, 800m).Correction!;
        ledger.RecordExpense(20m, "Groceries", Today);

        Assert.Equal(Money.FromCents(78000), ledger.BalanceOf(Bank));

        ledger.RemoveBalanceCorrection(second);
        Assert.Equal(Money.FromCents(98000), ledger.BalanceOf(Bank));
    }

    [Fact]
    public void An_income_dated_ahead_reaches_the_balance_on_its_date_and_Unassigned_at_once()
    {
        var nextMonth = new DateOnly(2026, 4, 1);
        ledger.RecordIncome(500m, "Salaris", nextMonth);

        Assert.Equal(Money.Zero, ledger.BalanceOf(Bank));
        Assert.Equal(Money.FromCents(50000), ledger.UnassignedIn(ledger.Calendar.PeriodContaining(nextMonth)));

        clock.Now = Noon(nextMonth);
        Assert.Equal(Money.FromCents(50000), ledger.BalanceOf(Bank));
    }

    // ------------------------------------------------------------------ the difference

    [Fact]
    public void The_difference_is_worked_out_again_as_forgotten_entries_are_found()
    {
        ledger.RecordIncome(1023.40m, "Salaris", Yesterday);
        var correction = ledger.CorrectBalance(Bank, 1000m).Correction!;
        Assert.Equal(Money.FromCents(-2340), ledger.DifferenceOf(correction));

        ledger.RecordExpense(23.40m, "Groceries", Yesterday);

        Assert.Equal(Money.Zero, ledger.DifferenceOf(correction));
        Assert.Equal(Money.FromCents(100000), ledger.BalanceOf(Bank));
    }

    [Fact]
    public void A_difference_counts_from_the_typed_balance_before_it()
    {
        var first = ledger.CorrectBalance(Bank, 1000m).Correction!;
        ledger.RecordExpense(20m, "Groceries", Today);
        var second = ledger.CorrectBalance(Bank, 975m).Correction!;

        Assert.Equal(Money.FromCents(100000), ledger.DifferenceOf(first));
        Assert.Equal(Money.FromCents(-500), ledger.DifferenceOf(second));

        // With the first gone, the second counts from nothing, and everything before it is in it.
        ledger.RemoveBalanceCorrection(first);
        Assert.Equal(Money.FromCents(99500), ledger.DifferenceOf(second));
    }

    [Fact]
    public void A_starting_balance_has_no_difference()
    {
        var cash = ledger.AddAccount("Cash", 40m).Account!;
        var start = Assert.Single(ledger.HistoryOf(cash).OfType<BalanceCorrection>());

        Assert.True(start.IsStartingBalance);
        Assert.Null(ledger.DifferenceOf(start));
    }

    // ------------------------------------------------------------------ transfers and net worth

    [Fact]
    public void A_transfer_moves_both_balances_and_leaves_net_worth()
    {
        var cash = ledger.AddAccount("Cash", null).Account!;
        ledger.RecordIncome(100m, "Salaris", Today);
        ledger.RecordTransfer(40m, Bank, cash, Today);

        Assert.Equal((Money.FromCents(6000), Money.FromCents(4000)), (ledger.BalanceOf(Bank), ledger.BalanceOf(cash)));
        Assert.Equal(Money.FromCents(10000), ledger.NetWorth);
    }

    [Fact]
    public void A_transfer_one_side_of_a_correction_already_holds_changes_net_worth()
    {
        var cash = ledger.AddAccount("Cash", null).Account!;
        ledger.CorrectBalance(Bank, 950m);
        ledger.RecordTransfer(50m, Bank, cash, Yesterday);

        Assert.Equal(Money.FromCents(95000), ledger.BalanceOf(Bank));
        Assert.Equal(Money.FromCents(100000), ledger.NetWorth);
    }

    [Fact]
    public void Saving_a_transfer_unchanged_is_never_refused_even_once_its_date_is_ahead()
    {
        var cash = ledger.AddAccount("Cash", null).Account!;
        var transfer = ledger.RecordTransfer(40m, Bank, cash, Today).Transfer!;
        clock.Now = Noon(Yesterday);

        Assert.Equal(ChangeOutcome.Unchanged, ledger.ChangeTransfer(transfer, 40m, Bank, cash, Today).Outcome);
    }

    [Fact]
    public void A_changed_transfer_keeps_its_id()
    {
        var cash = ledger.AddAccount("Cash", null).Account!;
        var transfer = ledger.RecordTransfer(40m, Bank, cash, Today).Transfer!;

        var changed = ledger.ChangeTransfer(transfer, 45m, cash, Bank, Yesterday).Transfer!;
        Assert.Equal(transfer.Id, changed.Id);
    }

    // ------------------------------------------------------------------ the order of the accounts

    [Fact]
    public void The_pool_comes_first_and_the_old_pool_goes_back_to_its_place()
    {
        ledger.AddAccount("Cash", null);
        var savings = ledger.AddAccount("Savings", null).Account!;
        ledger.MakePool(savings);

        Assert.Equal(["Savings", "Bank", "Cash"], ledger.Accounts.Select(a => a.Name));
    }

    [Fact]
    public void A_renamed_account_keeps_its_place_and_a_deleted_one_added_again_goes_last()
    {
        var cash = ledger.AddAccount("Cash", null).Account!;
        ledger.AddAccount("Savings", null);

        ledger.RenameAccount(cash, "Wallet");
        Assert.Equal(["Bank", "Wallet", "Savings"], ledger.Accounts.Select(a => a.Name));

        ledger.DeleteAccount(cash);
        ledger.AddAccount("Wallet", null);
        Assert.Equal(["Bank", "Savings", "Wallet"], ledger.Accounts.Select(a => a.Name));
    }

    [Fact]
    public void An_account_may_share_a_category_name_but_not_another_accounts()
    {
        Assert.False(ledger.AddAccount("groceries", null).WasRefused);
        Assert.Equal(AccountRefusal.NameTaken, ledger.AddAccount("  BANK ", null).Refusal);
    }

    [Fact]
    public void The_history_is_newest_first_and_on_one_day_newest_recorded_first()
    {
        var first = ledger.RecordExpense(10m, "Groceries", Today).Expense!;
        var older = ledger.RecordExpense(20m, "Groceries", Yesterday).Expense!;
        var second = ledger.CorrectBalance(Bank, 5m).Correction!;

        Assert.Equal([second.Id, first.Id, older.Id], ledger.HistoryOf(Bank).Select(e => e.Id));
    }

    // ------------------------------------------------------------------ kept

    [Fact]
    public void Kept_and_read_back_every_balance_and_difference_is_the_same()
    {
        var cash = ledger.AddAccount("Cash", 40m).Account!;
        ledger.RecordIncome(1023.40m, "Salaris", Yesterday);
        ledger.RecordExpense(12.5m, "Groceries", Today, "Markt", cash);
        ledger.CorrectBalance(Bank, 1000m);
        ledger.RecordTransfer(20m, Bank, cash, Today);
        ledger.MakePool(cash);

        var restored = Ledger.FromSnapshot(ledger.ToSnapshot(), clock);

        Assert.Equal(ledger.Accounts.Select(a => (a.Name, ledger.BalanceOf(a))),
                     restored.Accounts.Select(a => (a.Name, restored.BalanceOf(a))));
        Assert.Equal("Cash", restored.PoolAccount.Name);
        var correction = restored.HistoryOf(restored.AccountNamed("Bank")!).OfType<BalanceCorrection>().Single();
        Assert.Equal(Money.FromCents(-2340), restored.DifferenceOf(correction));
    }

    // ------------------------------------------------------------------ misuse no user can reach

    [Fact]
    public void The_pool_account_cannot_be_deleted() =>
        Assert.Throws<InvalidOperationException>(() => ledger.DeleteAccount(Bank));

    [Fact]
    public void An_account_in_use_cannot_be_deleted()
    {
        var cash = ledger.AddAccount("Cash", null).Account!;
        ledger.RecordExpense(5m, "Groceries", Today, account: cash);

        Assert.False(ledger.CanDeleteAccount(cash));
        Assert.Throws<InvalidOperationException>(() => ledger.DeleteAccount(cash));
    }

    [Fact]
    public void The_pool_account_cannot_be_made_the_pool_again() =>
        Assert.Throws<InvalidOperationException>(() => ledger.MakePool(Bank));

    [Fact]
    public void Another_ledgers_account_is_refused_as_a_mistake()
    {
        var elsewhere = new Ledger(clock, "Elders").PoolAccount;

        Assert.Throws<InvalidOperationException>(() => ledger.RecordExpense(5m, "Groceries", Today, account: elsewhere));
        Assert.Throws<InvalidOperationException>(() => ledger.CorrectBalance(elsewhere, 5m));
    }

    [Fact]
    public void A_transfer_removed_twice_throws()
    {
        var cash = ledger.AddAccount("Cash", null).Account!;
        var transfer = ledger.RecordTransfer(5m, Bank, cash, Today).Transfer!;
        ledger.RemoveTransfer(transfer);

        Assert.Throws<InvalidOperationException>(() => ledger.RemoveTransfer(transfer));
    }
}
