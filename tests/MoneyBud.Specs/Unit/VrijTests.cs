using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for <i>Vrij</i> and moving <i>Opgebouwd</i> (arc42 §12; ADR 0015; plan for increment
/// 15): the history of stretches across periods, the difference a changed old receipt moves (reading
/// 4), the lists of <i>Verplaatsen</i> (reading 5), and the invariant the rulings were made for —
/// on every account but the pool, <i>Vrij</i> plus the <i>Opgebouwd</i> of its categories is its
/// <i>Saldo</i>.
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

    // A first start has nothing to take from: Verplaatsen is not offered, and a form left open as its
    // last end went away closes rather than failing.
    [Fact]
    public void With_nothing_to_take_from_Verplaatsen_is_not_offered_and_does_nothing()
    {
        var app = new MoneyBudApp(Ledger.StartNew(clock));
        var form = app.ReallocateForm;

        Assert.False(form.HasEnds);
        form.OpenCommand.Execute(null);
        Assert.False(form.IsOpen);

        form.IsOpen = true;
        form.Amount = "10";
        Assert.Null(form.Reallocate());
        Assert.False(form.IsOpen);
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
            ["Vrij op \"Deposit\"", "Vrij op \"Broker\"", "Aandelen", "Savings", "Shares"],
            form.FromChoices.Select(c => c.Text));
        Assert.Equal(
            ["Vrij op \"Deposit\"", "Vrij op \"Broker\"", "Savings", "Shares", Tekst.Unassigned],
            form.ToChoices.Select(c => c.Text));
        Assert.Same(form.FromChoices, form.FromChoices);
        Assert.Equal(form.FromChoices[0], form.ChosenFrom);
        Assert.Equal(form.ToChoices[1], form.ChosenTo);
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

    // ------------------------------------------------------------------ the invariant

    // A long run of mixed acts under today's rules: late receipts dated back, dates changed, re-pointing,
    // "—", moves of purpose, corrections, sweeps across period ends and changes of start day. After each,
    // on every account but the pool, Vrij plus the Opgebouwd of the categories whose money is on it is
    // its balance; and net worth is the sum of the balances.
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
        foreach (var account in ledger.Accounts.Where(a => a != ledger.PoolAccount))
        {
            var claimed = Money.Sum(ledger.CategoriesOffered.Concat(ledger.ArchivedCategories)
                .Where(c => (ledger.BackingOf(c.Name) ?? ledger.LeftOn(c.Name)) == account)
                .Select(c => ledger.AccumulatedFor(c.Name, ledger.CurrentPeriod) ?? Money.Zero));
            Assert.True(
                ledger.UnclaimedOf(account)!.Value + claimed == ledger.BalanceOf(account),
                $"After step {step}, {account.Name}: Vrij {ledger.UnclaimedOf(account)} + Opgebouwd {claimed} is not its balance {ledger.BalanceOf(account)}.");
        }

        Assert.Equal(ledger.NetWorth, Money.Sum(ledger.Accounts.Select(ledger.BalanceOf)));
    }

    private static (Account From, Account To, Money Amount) Moved(Movement movement) =>
        (movement.From, movement.To, movement.Amount);
}
