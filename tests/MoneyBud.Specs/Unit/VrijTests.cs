using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for <i>Vrij</i> and moving <i>Opgebouwd</i> (arc42 §12; ADR 0015; plan for increment
/// 15): the history of stretches across periods, the difference a changed old receipt moves (reading
/// 4), the lists of <i>Verplaatsen</i> (reading 5), and the invariant the rulings were made for —
/// on every account, <i>Vrij</i> plus the <i>Opgebouwd</i> of its categories is its <i>Saldo</i>, and on
/// the pool account also what the current period claims there. And <i>Vrij</i> on the pool account
/// (ruling 5, revised 2026-10-04), with <i>Niet toegewezen</i> giving to it (ruled 2026-10-05).
/// </summary>
public sealed class VrijTests
{
    private static readonly DateOnly Today = new(2026, 3, 15);

    private readonly FixedClock clock = new(Noon(Today));
    private readonly Ledger ledger;
    private readonly Account deposit;
    private readonly Account broker;
    private readonly Category savings;
    private readonly Category shares;

    public VrijTests()
    {
        ledger = new Ledger(clock, "Bank");
        savings = ledger.AddCategory("Savings").Category!;
        shares = ledger.AddCategory("Shares").Category!;
        ledger.AddCategory("Groceries");
        ledger.RecordIncome(2000m, "Salaris", Today);
        deposit = ledger.AddAccount("Deposit", 0m).Account!;
        broker = ledger.AddAccount("Broker", 0m).Account!;
    }

    private Account Bank => ledger.PoolAccount;

    private static DateTimeOffset Noon(DateOnly day) => new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

    private void MoveTo(DateOnly day) => clock.Now = Noon(day);

    private static Money Euros(decimal euros) => Money.FromEuros(euros);

    private BudgetPeriod Period(DateOnly day) => ledger.Calendar.PeriodContaining(day);

    // ------------------------------------------------------------------ the history of stretches

    // Backed in March, 200 built; set to "—" in April with this period's 100 returned; backed again with
    // Broker in May. Each period reads what it had built then, and Opgebouwd carries on throughout.
    [Fact]
    public void Each_period_reads_what_was_built_by_then_across_none_and_backing_again()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(200m, "Savings", Period(Today));

        MoveTo(new DateOnly(2026, 4, 10));
        ledger.RecordIncome(1000m, "Salaris", ledger.Today);
        ledger.Assign(100m, "Savings", ledger.CurrentPeriod);
        var none = ledger.SetBacking("Savings", null);
        Assert.Equal((deposit, Bank, Euros(100m)), Moved(Assert.Single(none.Moves)));
        ledger.RecordExpense(30m, "Savings", ledger.Today);

        MoveTo(new DateOnly(2026, 5, 10));
        var again = ledger.SetBacking("Savings", broker);
        Assert.Equal([(deposit, broker, Euros(200m))], again.MovedBetweenAccounts.Select(Moved));

        Assert.Equal(Euros(200m), ledger.AccumulatedFor("Savings", Period(new(2026, 3, 1))));
        Assert.Equal(Euros(200m), ledger.AccumulatedFor("Savings", Period(new(2026, 4, 1))));
        Assert.Equal(Euros(200m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
        Assert.Equal(Euros(200m), ledger.BalanceOf(broker));
        Assert.Equal(Money.Zero, ledger.BalanceOf(deposit));
    }

    // A "—" with nothing left behind shows nothing, and is otherwise simply unbacked (reading 2).
    [Fact]
    public void A_category_set_to_none_with_nothing_left_shows_nothing()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(200m, "Savings", ledger.CurrentPeriod);
        ledger.SetBacking("Savings", null);

        Assert.Null(ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
        Assert.Same(deposit, ledger.LeftOn("Savings"));
        Assert.True(ledger.CanDeleteAccount(deposit) is false, "Deposit has a movement on it.");
        Assert.DoesNotContain(ledger.ReallocationSources, e => e.Category == savings);
    }

    // An account that only backed a category, with nothing ever moved, is unused again once the
    // category is set to "—": deleting it forgets that stretch, and the snapshot still reads.
    [Fact]
    public void Deleting_an_account_a_none_left_nothing_on_forgets_that_stretch()
    {
        ledger.SetBacking("Savings", broker);
        ledger.SetBacking("Savings", null);

        Assert.True(ledger.CanDeleteAccount(broker));
        ledger.DeleteAccount(broker);

        Assert.Null(ledger.LeftOn("Savings"));
        var restored = Ledger.FromSnapshot(ledger.ToSnapshot(), clock);
        Assert.Null(restored.LeftOn("Savings"));
    }

    // While on "—", the money left behind is fixed: its expenses do not lower it, wherever they are
    // paid from, and only moves out of it do (reading 2).
    [Fact]
    public void Money_left_behind_is_lowered_only_by_moving_it_out()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.CorrectBalance(deposit, 500m);
        ledger.Reallocate(500m, ReallocationEnd.UnclaimedOn(deposit), ReallocationEnd.For(savings), ledger.CurrentPeriod);
        ledger.SetBacking("Savings", null);

        ledger.RecordExpense(20m, "Savings", Today);
        ledger.RecordExpense(10m, "Savings", Today, account: deposit);
        Assert.Equal(Euros(500m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
        Assert.Equal(Euros(-10m), ledger.UnclaimedOf(deposit));

        ledger.Reallocate(100m, ReallocationEnd.For(savings), ReallocationEnd.Unassigned, ledger.CurrentPeriod);
        Assert.Equal(Euros(400m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
    }

    // ------------------------------------------------------------------ the difference a changed old receipt moves

    // Scenario-stage ruling 2, and reading 4: the difference moves between the account the receipt is
    // on and the backing account. From the pool account it is exactly the ruling; from Contant, the
    // money goes between Contant and the backing account.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_changed_receipt_from_before_the_backing_moves_its_difference_from_the_account_it_is_on(bool fromCash)
    {
        var cash = ledger.AddAccount("Contant", 200m).Account!;
        var paidFrom = fromCash ? cash : Bank;
        ledger.Assign(300m, "Savings", ledger.CurrentPeriod);
        var receipt = ledger.RecordExpense(100m, "Savings", Today, "Voorschot", paidFrom).Expense!;
        ledger.SetBacking("Savings", deposit);
        var before = ledger.BalanceOf(paidFrom);

        ledger.ChangeExpense(receipt, 40m, "Savings", Today, "Voorschot", paidFrom);

        Assert.Equal(Euros(260m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
        Assert.Equal(Euros(260m), ledger.ThereFor("Savings"));
        Assert.Equal(Euros(260m), ledger.BalanceOf(deposit));
        Assert.Equal(before, ledger.BalanceOf(paidFrom));
        Assert.Same(paidFrom, ledger.ExpensesIn(ledger.CurrentPeriod).Single(e => e.Label == "Voorschot").Account);
    }

    // Reading 3: one recorded before a re-pointing keeps its account too, and its difference moves to
    // the new backing account.
    [Fact]
    public void A_receipt_from_before_a_re_pointing_keeps_its_account_and_its_difference_moves_to_the_new_one()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(300m, "Savings", ledger.CurrentPeriod);
        var receipt = ledger.RecordExpense(50m, "Savings", Today, "Boek").Expense!;
        ledger.SetBacking("Savings", broker);

        Assert.Same(deposit, ledger.LockedAccountFor("Savings", Today, receipt));
        ledger.ChangeExpense(receipt, 30m, "Savings", Today, "Boek");

        Assert.Equal(Euros(270m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
        Assert.Equal(Euros(270m), ledger.BalanceOf(broker));
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(deposit));
    }

    // ------------------------------------------------------------------ found by review at the build

    // A late receipt dated in an earlier backed period, entered after re-pointing, is locked on the new
    // account, and counts in what that account holds as it does in Opgebouwd. So does an expense whose
    // date is changed back into that period.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void After_re_pointing_a_receipt_dated_in_an_earlier_period_counts_in_what_is_there(bool byChangingTheDate)
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(300m, "Savings", ledger.CurrentPeriod);
        MoveTo(new DateOnly(2026, 4, 10));
        ledger.SetBacking("Savings", broker);

        var march20 = new DateOnly(2026, 3, 20);
        if (byChangingTheDate)
        {
            var receipt = ledger.RecordExpense(100m, "Savings", ledger.Today, "Kado").Expense!;
            ledger.ChangeExpense(receipt, 100m, "Savings", march20, "Kado");
        }
        else
        {
            Assert.Same(broker, ledger.RecordExpense(100m, "Savings", march20, "Kado").Expense!.Account);
        }

        Assert.Equal(Euros(200m), ledger.ThereFor("Savings"));
        Assert.Equal(Euros(200m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(broker));
        Assert.Empty(ledger.SetBacking("Savings", null).Moves);
    }

    // A category pointed from one account to another and back: an old receipt on the first, changed
    // later, moves what is there with Opgebouwd.
    [Fact]
    public void Pointed_away_and_back_an_old_receipt_on_the_account_still_counts()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(300m, "Savings", ledger.CurrentPeriod);
        var receipt = ledger.RecordExpense(50m, "Savings", Today, "Boek").Expense!;
        MoveTo(new DateOnly(2026, 4, 10));
        ledger.SetBacking("Savings", broker);
        ledger.SetBacking("Savings", deposit);

        ledger.ChangeExpense(receipt, 20m, "Savings", Today, "Boek");

        Assert.Equal(Euros(280m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
        Assert.Equal(Euros(280m), ledger.ThereFor("Savings"));
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(deposit));
    }

    // Deleting an unused account named deep in one category's history cuts only that stretch: the money
    // another stretch left behind stays.
    [Fact]
    public void Deleting_an_account_named_deep_in_a_history_keeps_the_money_left_behind_since()
    {
        var old = ledger.AddAccount("Old", null).Account!;
        ledger.SetBacking("Savings", old);
        ledger.SetBacking("Savings", null);
        ledger.SetBacking("Savings", deposit);
        ledger.CorrectBalance(deposit, 1000m);
        ledger.Reallocate(1000m, ReallocationEnd.UnclaimedOn(deposit), ReallocationEnd.For(savings), ledger.CurrentPeriod);
        ledger.SetBacking("Savings", null);

        Assert.True(ledger.CanDeleteAccount(old));
        ledger.DeleteAccount(old);

        Assert.Equal(Euros(1000m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
        Assert.Same(deposit, ledger.LeftOn("Savings"));
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(deposit));
        Assert.NotNull(Ledger.FromSnapshot(ledger.ToSnapshot(), clock).LeftOn("Savings"));
    }

    // Ruled at the build, 2026-10-04: Vrij to Vrij on another account gives nothing a purpose, and is
    // Overboeken. Refused, after the same end on both sides.
    [Fact]
    public void Vrij_to_Vrij_on_another_account_is_refused()
    {
        var result = ledger.Reallocate(100m, ReallocationEnd.UnclaimedOn(deposit), ReallocationEnd.UnclaimedOn(broker), ledger.CurrentPeriod);

        Assert.Equal(ReallocationRefusal.UnclaimedToUnclaimed, result.Refusal);
        Assert.Equal(ReallocationRefusal.UnclaimedToUnclaimed,
            ledger.Reallocate(0.005m, ReallocationEnd.UnclaimedOn(deposit), ReallocationEnd.UnclaimedOn(broker), ledger.CurrentPeriod).Refusal);
        Assert.Equal(Money.Zero, ledger.BalanceOf(broker));
    }

    // A first start has two ends, Vrij on the pool account and Niet toegewezen, so Verplaatsen opens and
    // starts from one to the other (ruling 5 revised; ruled 2026-10-05).
    [Fact]
    public void A_first_start_offers_Vrij_on_the_pool_account_and_Niet_toegewezen()
    {
        var app = new MoneyBudApp(Ledger.StartNew(clock));
        var form = app.ReallocateForm;
        string[] both = ["Vrij op \"Betaalrekening\"", Tekst.Unassigned];

        form.OpenCommand.Execute(null);

        Assert.True(form.IsOpen);
        Assert.Equal(both, form.FromChoices.Select(c => c.Text));
        Assert.Equal(both, form.ToChoices.Select(c => c.Text));
        Assert.Equal(form.FromChoices[0], form.ChosenFrom);
        Assert.Equal(form.ToChoices[1], form.ChosenTo);
    }

    // ------------------------------------------------------------------ the lists of Verplaatsen

    [Fact]
    public void The_lists_offer_Vrij_in_the_strip_s_order_then_categories_alphabetically_and_Niet_toegewezen_last()
    {
        var app = new MoneyBudApp(ledger);
        ledger.AddCategory("Aandelen");
        ledger.SetBacking("Shares", broker);
        ledger.SetBacking("Savings", deposit);
        ledger.SetBacking("Aandelen", deposit);
        ledger.ArchiveCategory("Aandelen");

        var form = app.ReallocateForm;
        Assert.Equal(
            ["Vrij op \"Bank\"", "Vrij op \"Deposit\"", "Vrij op \"Broker\"", "Aandelen", "Savings", "Shares", Tekst.Unassigned],
            form.FromChoices.Select(c => c.Text));
        Assert.Equal(
            ["Vrij op \"Bank\"", "Vrij op \"Deposit\"", "Vrij op \"Broker\"", "Savings", "Shares", Tekst.Unassigned],
            form.ToChoices.Select(c => c.Text));
        Assert.Same(form.FromChoices, form.FromChoices);
        Assert.Equal(form.FromChoices[0], form.ChosenFrom);
        Assert.Equal("Savings", form.ChosenTo!.Text);

        // From a category, Vrij is a fine first Naar.
        form.ChosenFrom = form.FromChoices.Single(c => c.Text == "Savings");
        form.Clear();
        form.ChosenFrom = form.FromChoices.Single(c => c.Text == "Savings");
        Assert.Equal("Vrij op \"Bank\"", form.ChosenTo!.Text);
    }

    // A list writing back nothing changes nothing, as every list on screen.
    [Fact]
    public void A_list_writing_back_nothing_is_not_a_choice()
    {
        var app = new MoneyBudApp(ledger);
        ledger.SetBacking("Savings", deposit);
        var form = app.ReallocateForm;
        form.ChosenTo = form.ToChoices.Single(c => c.End.Category == savings);

        form.ChosenFrom = null;
        form.ChosenTo = null;

        Assert.Equal(savings, form.ChosenTo!.End.Category);
    }

    // ------------------------------------------------------------------ Vrij on the pool account

    // Ruling 5 revised: the pool account's Vrij is its balance less what this period claims there —
    // Niet toegewezen and the Resterend of every category without an account — and less what the
    // categories it backs have there. 500 of my own, given no purpose, stays 500 through a plan.
    [Fact]
    public void The_pool_account_s_Vrij_is_its_balance_less_this_period_s_claim_and_its_categories()
    {
        ledger.CorrectBalance(Bank, 2500m);
        Assert.Equal(Euros(500m), ledger.UnclaimedOf(Bank));

        ledger.Assign(300m, "Groceries", ledger.CurrentPeriod);
        ledger.RecordExpense(40m, "Groceries", Today);
        ledger.SetBacking("Shares", Bank);
        ledger.Assign(200m, "Shares", ledger.CurrentPeriod);
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(100m, "Savings", ledger.CurrentPeriod);

        Assert.Equal(Euros(2360m), ledger.BalanceOf(Bank));
        Assert.Equal(Euros(1400m), ledger.UnassignedIn(ledger.CurrentPeriod));
        Assert.Equal(Euros(500m), ledger.UnclaimedOf(Bank));
    }

    // Derived: an income dated later in the period counts in Niet toegewezen at once and reaches the
    // balance on its date, so the pool account's Vrij does not dip in between, even once it is assigned.
    [Fact]
    public void An_income_dated_later_in_the_period_leaves_the_pool_account_s_Vrij_as_it_was()
    {
        ledger.RecordIncome(800m, "Bonus", new DateOnly(2026, 3, 25));
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(Bank));

        ledger.Assign(800m, "Groceries", ledger.CurrentPeriod);
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(Bank));

        MoveTo(new DateOnly(2026, 3, 25));
        Assert.Equal(Euros(2800m), ledger.BalanceOf(Bank));
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(Bank));
    }

    // Derived: what changes another account's balance against a claim on the pool account shows on both,
    // and a transfer squares both. An income on Deposit counts in Niet toegewezen; cash spent on
    // Groceries lowers its Resterend.
    [Fact]
    public void Income_or_spending_on_another_account_against_the_pool_s_claim_shows_on_both_until_transferred()
    {
        var cash = ledger.AddAccount("Contant", 50m).Account!;
        ledger.RecordIncome(100m, "Rente", Today, deposit);
        ledger.Assign(100m, "Groceries", ledger.CurrentPeriod);
        ledger.RecordExpense(20m, "Groceries", Today, "Markt", cash);

        Assert.Equal(Euros(-80m), ledger.UnclaimedOf(Bank));
        Assert.Equal(Euros(100m), ledger.UnclaimedOf(deposit));
        Assert.Equal(Euros(30m), ledger.UnclaimedOf(cash));

        ledger.RecordTransfer(100m, deposit, Bank, Today);
        ledger.RecordTransfer(20m, Bank, cash, Today);

        Assert.Equal(Money.Zero, ledger.UnclaimedOf(Bank));
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(deposit));
        Assert.Equal(Euros(50m), ledger.UnclaimedOf(cash));
    }

    // Derived: at a period's end what is swept leaves the pool account with its claim. With no destination
    // the leftover stays claimed, by the period's line, which still asks for it (ruled 2026-10-05). A
    // leftover below zero is not swept and nothing asks for it: it was paid from the pool account's Vrij.
    [Theory]
    [InlineData(false, 100, 0)]
    [InlineData(true, 100, 0)]
    [InlineData(true, 2400, -400)]
    [InlineData(false, 2400, -400)]
    public void At_a_period_s_end_what_is_not_swept_is_the_pool_account_s_Vrij(bool destination, decimal spent, decimal vrij)
    {
        ledger.SetBacking("Savings", deposit);
        if (destination) ledger.SetSweepDestination("Savings");
        ledger.Assign(300m, "Groceries", ledger.CurrentPeriod);
        ledger.RecordExpense(spent, "Groceries", Today);
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(Bank));

        MoveTo(new DateOnly(2026, 4, 2));
        ledger.Settle();

        Assert.Equal(Euros(vrij), ledger.UnclaimedOf(Bank));
    }

    // Ruled 2026-10-05: Niet toegewezen gives to Vrij on the pool account, the account that holds it, in
    // the current period. No money moves, and the row is in the pool account's history. A minus sign the
    // other way is the same move. To anything else it is still refused.
    [Fact]
    public void Niet_toegewezen_gives_to_Vrij_on_the_pool_account_and_to_nothing_else()
    {
        ledger.SetBacking("Savings", deposit);
        var unassigned = ReallocationEnd.Unassigned;
        var poolVrij = ReallocationEnd.UnclaimedOn(Bank);

        var made = ledger.Reallocate(30m, unassigned, poolVrij, ledger.CurrentPeriod).Made!;
        Assert.False(made.MovedMoney);
        Assert.Contains(made, ledger.HistoryOf(Bank));
        Assert.Equal(Euros(1970m), ledger.UnassignedIn(ledger.CurrentPeriod));
        Assert.Equal(Euros(30m), ledger.UnclaimedOf(Bank));
        Assert.Equal(Euros(2000m), ledger.BalanceOf(Bank));

        var back = ledger.Reallocate(-20m, poolVrij, unassigned, ledger.CurrentPeriod).Made!;
        Assert.Equal((unassigned, poolVrij), (back.From, back.To));
        Assert.Equal(Euros(1950m), ledger.UnassignedIn(ledger.CurrentPeriod));

        Assert.Equal(ReallocationRefusal.OutOfUnassigned,
            ledger.Reallocate(10m, unassigned, ReallocationEnd.UnclaimedOn(deposit), ledger.CurrentPeriod).Refusal);
        Assert.Equal(ReallocationRefusal.OutOfUnassigned,
            ledger.Reallocate(10m, unassigned, ReallocationEnd.For(savings), ledger.CurrentPeriod).Refusal);
        Assert.Equal(ReallocationRefusal.UnassignedNotCurrent,
            ledger.Reallocate(10m, unassigned, poolVrij, ledger.Calendar.Next(ledger.CurrentPeriod)).Refusal);
        Assert.Equal(Euros(1950m), ledger.UnassignedIn(ledger.CurrentPeriod));
    }

    // Ruled 2026-10-05: what an ended period's line still asks for is the line's, not Vrij. A late refund
    // in a swept period, and a late receipt, leave the pool account's Vrij where it was; Restant
    // bijwerken moves the money and the claim together.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void A_late_change_to_a_swept_period_is_its_line_s_and_leaves_the_pool_account_s_Vrij(bool refund)
    {
        ledger.SetBacking("Savings", deposit);
        ledger.SetSweepDestination("Savings");
        ledger.Assign(300m, "Groceries", ledger.CurrentPeriod);
        ledger.RecordExpense(100m, "Groceries", Today);
        MoveTo(new DateOnly(2026, 4, 10));
        ledger.Settle();
        var before = ledger.UnclaimedOf(Bank);

        var march = Period(Today);
        if (refund) ledger.RecordIncome(40m, "Terugbetaling", Today);
        else ledger.RecordExpense(40m, "Groceries", Today, "Bon");

        Assert.Equal(refund ? SweepLineKind.StillToSweep : SweepLineKind.SweptTooMuch, ledger.SweepLineFor(march)!.Kind);
        Assert.Equal(before, ledger.UnclaimedOf(Bank));

        ledger.BringUpToDate(march);

        Assert.Equal(before, ledger.UnclaimedOf(Bank));
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(deposit));
    }

    // Ruled 2026-10-05, and what letting go means for it: an amount the line lets go is no longer
    // asked for, so it stops being claimed, for good, and the pool account's Vrij falls by what could
    // not come back. Savings spent everything the sweep gave it.
    [Fact]
    public void What_a_line_lets_go_stops_being_claimed()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.SetSweepDestination("Savings");
        ledger.Assign(300m, "Groceries", ledger.CurrentPeriod);
        ledger.RecordExpense(100m, "Groceries", Today);
        MoveTo(new DateOnly(2026, 4, 10));
        ledger.Settle();
        ledger.RecordExpense(1900m, "Savings", ledger.Today);
        var before = ledger.UnclaimedOf(Bank);

        ledger.RecordExpense(40m, "Groceries", Today, "Bon");
        Assert.Equal(before, ledger.UnclaimedOf(Bank));

        var result = ledger.BringUpToDate(Period(Today));

        Assert.Equal(Euros(40m), result.LetGo);
        Assert.Equal(before - Euros(40m), ledger.UnclaimedOf(Bank));
    }

    // Derived (f), and found by review: making another account the pool takes every claim only the
    // pool account has along, an ended period's line included, since Restant bijwerken moves to and from
    // whichever account is the pool then. Until the money is transferred, the new one shows it short.
    [Fact]
    public void Making_another_account_the_pool_takes_the_period_s_and_the_lines_claims_along()
    {
        ledger.Assign(400m, "Groceries", ledger.CurrentPeriod);
        ledger.RecordExpense(100m, "Groceries", Today);
        MoveTo(new DateOnly(2026, 4, 2));
        ledger.RecordIncome(500m, "Salaris", ledger.Today);
        Assert.Equal(SweepLineKind.StillToSweep, ledger.SweepLineFor(Period(Today))!.Kind);
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(Bank));

        var bank = Bank;
        ledger.MakePool(deposit);

        Assert.Equal(Euros(-2400m), ledger.UnclaimedOf(deposit));
        Assert.Equal(Euros(2400m), ledger.UnclaimedOf(bank));

        ledger.RecordTransfer(2400m, bank, deposit, ledger.Today);
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(deposit));
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(bank));
    }

    // Derived (g): Vrij on the pool account to a category the pool account backs moves no money, and is a
    // row in its history.
    [Fact]
    public void Vrij_on_the_pool_account_to_a_category_it_backs_moves_no_money()
    {
        ledger.CorrectBalance(Bank, 2500m);
        ledger.SetBacking("Savings", Bank);

        var made = ledger.Reallocate(500m, ReallocationEnd.UnclaimedOn(Bank), ReallocationEnd.For(savings), ledger.CurrentPeriod).Made!;

        Assert.False(made.MovedMoney);
        Assert.Contains(made, ledger.HistoryOf(Bank));
        Assert.Equal(Euros(2500m), ledger.BalanceOf(Bank));
        Assert.Equal(Money.Zero, ledger.UnclaimedOf(Bank));
        Assert.Equal(Euros(500m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
    }

    // Ruling 5 revised: the pool account's Vrij moves only by what changes its balance and no claim, or a
    // claim and not its balance. A long run of everything else — planning, spending, backing, "—",
    // re-pointing, moves of purpose that do not touch it, income dated now or later in the period, days
    // passing — leaves it exactly where it was, but for the gap a capped negative assignment leaves.
    [Fact]
    public void The_pool_account_s_Vrij_stays_put_through_planning_spending_backing_and_moving_purpose()
    {
        var random = new Random(5);
        var names = new[] { "Savings", "Shares", "Groceries" };
        var backings = new[] { null, Bank, deposit, broker };
        ledger.CorrectBalance(Bank, 2600m);
        ledger.CorrectBalance(deposit, 1000m);
        var vrij = ledger.UnclaimedOf(Bank);
        Assert.Equal(Euros(600m), vrij);

        for (var step = 0; step < 1500; step++)
        {
            var name = names[random.Next(names.Length)];
            var amount = random.Next(1, 300);
            var act = random.Next(10);
            switch (act)
            {
                case 0: ledger.Assign(amount, name, ledger.CurrentPeriod); break;
                case 1:
                    // Derived: a negative assignment to a backed category moves back at most what is there
                    // for it, and what it cannot bring back still joins Niet toegewezen: the pool account's
                    // Vrij shows that gap.
                    var budget = ledger.BudgetFor(name, ledger.CurrentPeriod);
                    var there = ledger.ThereFor(name);
                    ledger.Assign(-amount, name, ledger.CurrentPeriod);
                    if (there is { } before)
                        vrij -= budget - ledger.BudgetFor(name, ledger.CurrentPeriod) - (before - ledger.ThereFor(name)!.Value);
                    break;
                case 2: ledger.RecordExpense(amount, name, ledger.Today, account: ledger.LockedAccountFor(name, ledger.Today) ?? Bank); break;
                case 3: ledger.SetBacking(name, backings[random.Next(backings.Length)]); break;
                case 4: ReallocateWithoutThePoolsVrij(amount, random); break;
                case 5: ledger.RecordIncome(amount, "Extra", ledger.Today); break;
                case 6: ledger.RecordIncome(amount, "Later", ledger.CurrentPeriod.LastDay); break;
                case 7: ledger.RecordTransfer(amount, deposit, broker, ledger.Today); break;
                case 8:
                    // One paid from where its category's money is: one paid from elsewhere moves Vrij, as recording it did.
                    if (ledger.ExpensesIn(ledger.CurrentPeriod)
                            .FirstOrDefault(e => e.Account == (ledger.BackingOf(e.Category.Name) ?? Bank)) is { } expense)
                        ledger.RemoveExpense(expense);
                    break;
                case 9:
                    if (ledger.Today < new DateOnly(2026, 3, 30)) MoveTo(ledger.Today.AddDays(1));
                    break;
            }

            Assert.True(vrij == ledger.UnclaimedOf(Bank),
                $"After step {step}, act {act} for {name} with {amount}: the pool account's Vrij went from {vrij} to {ledger.UnclaimedOf(Bank)}.");
        }
    }

    private void ReallocateWithoutThePoolsVrij(decimal amount, Random random)
    {
        var poolVrij = ReallocationEnd.UnclaimedOn(Bank);
        var from = ledger.ReallocationSources.Where(e => e != poolVrij && e.Kind != ReallocationEndKind.Unassigned).ToList();
        var to = ledger.ReallocationDestinations.Where(e => e != poolVrij).ToList();
        var end = from[random.Next(from.Count)];
        var other = to[random.Next(to.Count)];
        if (end != other) ledger.Reallocate(amount, end, other, ledger.CurrentPeriod);
    }

    // ------------------------------------------------------------------ the invariant

    // A long run of mixed acts under today's rules: late receipts dated back, dates changed, re-pointing,
    // "—", moves of purpose, corrections, sweeps across period ends and changes of start day. After each,
    // on every account, Vrij plus the Opgebouwd of the categories whose money is on it is its balance —
    // on the pool account plus what the current period claims there; and net worth is the sum of the
    // balances.
    [Fact]
    public void Vrij_plus_Opgebouwd_is_the_balance_on_every_account_after_every_act()
    {
        var random = new Random(15);
        var accounts = new[] { Bank, deposit, broker };
        var names = new[] { "Savings", "Shares", "Groceries" };
        var days = 0;

        for (var step = 0; step < 1500; step++)
        {
            var name = names[random.Next(names.Length)];
            var amount = random.Next(1, 400);
            switch (random.Next(14))
            {
                case 0: ledger.Assign(amount, name, ledger.CurrentPeriod); break;
                case 1: ledger.Assign(-amount, name, ledger.CurrentPeriod); break;
                case 2: ledger.RecordExpense(amount, name, ledger.Today); break;
                case 11: ledger.RecordExpense(amount, name, ledger.Today.AddDays(-random.Next(1, 70))); break;
                case 12: ChangeSomeDate(random.Next(0, 70)); break;
                case 13:
                    if (random.Next(4) == 0) ledger.ChangeStartDay(random.Next(1, 32));
                    break;
                case 3: ledger.SetBacking(name, accounts[random.Next(accounts.Length)]); break;
                case 4: ledger.SetBacking(name, null); break;
                case 5: ReallocateSomething(amount); break;
                case 6: ledger.CorrectBalance(accounts[random.Next(1, 3)], random.Next(0, 3000)); break;
                case 7: ledger.RecordTransfer(amount, Bank, accounts[random.Next(1, 3)], ledger.Today); break;
                case 8: ChangeSomeExpense(amount); break;
                case 9:
                    days += random.Next(1, 20);
                    MoveTo(Today.AddDays(days));
                    ledger.RecordIncome(1000m, "Salaris", ledger.Today);
                    break;
                case 10:
                    if (ledger.SweepDestinationChoices.Count > 0)
                        ledger.SetSweepDestination(ledger.SweepDestinationChoices[0].Name);
                    break;
            }

            AssertTheInvariant(step);
        }
    }

    private void ReallocateSomething(decimal amount)
    {
        var from = ledger.ReallocationSources;
        var to = ledger.ReallocationDestinations;
        if (from.Count == 0) return;
        var random = new Random((int)amount);
        var end = from[random.Next(from.Count)];
        var other = to[random.Next(to.Count)];
        if (end != other) ledger.Reallocate(amount, end, other, ledger.CurrentPeriod);
    }

    private void ChangeSomeDate(int daysBack)
    {
        var all = Enumerable.Range(0, 4).SelectMany(i => ledger.ExpensesIn(ledger.Calendar.PeriodContaining(ledger.Today.AddDays(-31 * i)))).ToList();
        if (all.Count == 0) return;
        var expense = all[daysBack % all.Count];
        var date = ledger.Today.AddDays(-daysBack);
        var account = ledger.LockedAccountFor(expense.Category.Name, date, expense) ?? expense.Account;
        ledger.ChangeExpense(expense, expense.Amount.Cents / 100m, expense.Category.Name, date, expense.Label, account);
    }

    private void ChangeSomeExpense(decimal amount)
    {
        var expense = ledger.ExpensesIn(ledger.CurrentPeriod).FirstOrDefault();
        if (expense is null) return;
        var account = ledger.LockedAccountFor(expense.Category.Name, expense.Date, expense) ?? expense.Account;
        ledger.ChangeExpense(expense, amount, expense.Category.Name, expense.Date, expense.Label, account);
    }

    private void AssertTheInvariant(int step)
    {
        var period = ledger.CurrentPeriod;
        var linesClaim = Money.Zero;
        for (var ended = Period(new DateOnly(2025, 1, 1)); ended.FirstDay < period.FirstDay; ended = ledger.Calendar.Next(ended))
        {
            linesClaim += ledger.SweepLineFor(ended) switch
            {
                { Kind: SweepLineKind.StillToSweep } line => line.Amount,
                { Kind: SweepLineKind.SweptTooMuch } line => -line.Amount,
                _ => Money.Zero,
            };
        }

        var periodClaim = ledger.PeriodLeftover(period) + linesClaim
                          - Money.Sum(ledger.IncomesIn(period).Where(i => i.Date > ledger.Today).Select(i => i.Amount));
        foreach (var account in ledger.Accounts)
        {
            var claimed = Money.Sum(ledger.CategoriesOffered.Concat(ledger.ArchivedCategories)
                .Where(c => (ledger.BackingOf(c.Name) ?? ledger.LeftOn(c.Name)) == account)
                .Select(c => ledger.AccumulatedFor(c.Name, period) ?? Money.Zero))
                + (account == ledger.PoolAccount ? periodClaim : Money.Zero);
            Assert.True(
                ledger.UnclaimedOf(account) + claimed == ledger.BalanceOf(account),
                $"After step {step}, {account.Name}: Vrij {ledger.UnclaimedOf(account)} + Opgebouwd {claimed} is not its balance {ledger.BalanceOf(account)}.");
        }

        Assert.Equal(ledger.NetWorth, Money.Sum(ledger.Accounts.Select(ledger.BalanceOf)));
    }

    private static (Account From, Account To, Money Amount) Moved(Movement movement) =>
        (movement.From, movement.To, movement.Amount);
}
