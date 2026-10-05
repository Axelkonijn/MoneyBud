using MoneyBud.Domain;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for the sweep (arc42 §12, <i>The sweep and Restant</i>; ADR 0010): what a period's
/// leftover is, what settling sweeps and in what order, the line in each of its states, what
/// <i>Restant bijwerken</i> moves and lets go, and the destination — the edges the scenarios reach only
/// one at a time — and the misuse no user can reach, which throws.
/// </summary>
public sealed class SweepTests
{
    private static readonly DateOnly Today = new(2026, 3, 15);

    private readonly FixedClock clock = new(Noon(Today));
    private readonly Ledger ledger;
    private readonly Account deposit;
    private readonly Account broker;

    // March is the period the ledger is first made in, so February ended before the first start.
    private readonly BudgetPeriod february;
    private readonly BudgetPeriod march;
    private readonly BudgetPeriod april;

    public SweepTests()
    {
        ledger = new Ledger(clock, "Bank");
        ledger.AddCategory("Savings");
        ledger.AddCategory("Holiday");
        ledger.AddCategory("Groceries");
        ledger.AddCategory("Hobby");
        ledger.RecordIncome(2000m, "Salaris", Today);
        deposit = ledger.AddAccount("Deposit", 0m).Account!;
        broker = ledger.AddAccount("Broker", 0m).Account!;

        march = ledger.CurrentPeriod;
        february = ledger.Calendar.Previous(march);
        april = ledger.Calendar.Next(march);
    }

    private Account Bank => ledger.PoolAccount;

    private static DateTimeOffset Noon(DateOnly day) => new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

    private void MoveTo(DateOnly day) => clock.Now = Noon(day);

    private static Money Euros(decimal euros) => Money.FromEuros(euros);

    // A line's fields, its parts as "name amount" pairs: a record's own equality would compare the
    // parts list by reference.
    private static (SweepLineKind, Money, bool, string)? Read(SweepLine? line) =>
        line is null
            ? null
            : (line.Kind, line.Amount, line.CanBringUpToDate,
               string.Join(", ", line.Parts.Select(p => $"{p.Category.Name} {p.Amount}")));

    private IEnumerable<Movement> Sweeps() =>
        ledger.HistoryOf(deposit).Concat(ledger.HistoryOf(broker)).OfType<Movement>()
            .Where(m => m.Reason == MovementReason.Swept).Distinct().OrderBy(m => m.Id);

    // Savings backed by Deposit and the destination; March's 2000 swept into it on 1 April.
    private void SweptIntoSavings()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.SetSweepDestination("Savings");
        MoveTo(april.FirstDay.AddDays(4));
        ledger.Settle();
    }

    // ------------------------------------------------------------------ the period leftover

    [Fact]
    public void The_leftover_is_Unassigned_plus_every_unbacked_Remaining_negatives_included()
    {
        ledger.Assign(400m, "Groceries", march);
        ledger.Assign(100m, "Hobby", march);
        ledger.RecordExpense(350m, "Groceries", Today);
        ledger.RecordExpense(130m, "Hobby", Today);

        Assert.Equal(Euros(1520m), ledger.PeriodLeftover(march));
    }

    [Fact]
    public void A_backed_categorys_Remaining_and_its_overspending_are_not_part_of_it()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.SetBacking("Holiday", deposit);
        ledger.Assign(300m, "Savings", march);
        ledger.Assign(100m, "Holiday", march);
        ledger.RecordExpense(150m, "Holiday", Today, account: deposit);

        Assert.Equal(Euros(1600m), ledger.PeriodLeftover(march));
    }

    // Hobby was unbacked and Savings backed when March ended. Backing the one and unbacking the other
    // afterwards changes nothing about March.
    [Fact]
    public void Backing_is_judged_as_it_was_at_the_periods_end_in_both_directions()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(100m, "Savings", march);
        ledger.Assign(100m, "Hobby", march);
        ledger.RecordExpense(40m, "Hobby", Today);
        Assert.Equal(Euros(1860m), ledger.PeriodLeftover(march));

        MoveTo(april.FirstDay);
        ledger.Settle();
        ledger.SetBacking("Hobby", deposit);
        ledger.SetBacking("Savings", null);

        Assert.Equal(Euros(1860m), ledger.PeriodLeftover(march));
    }

    // Before the period ends it is judged by today's backing, which is what its end will have.
    [Fact]
    public void A_period_not_yet_ended_is_judged_by_todays_backing()
    {
        ledger.Assign(100m, "Hobby", march);
        Assert.Equal(Euros(2000m), ledger.PeriodLeftover(march));

        ledger.SetBacking("Hobby", deposit);

        Assert.Equal(Euros(1900m), ledger.PeriodLeftover(march));
    }

    [Fact]
    public void An_archived_unbacked_category_counts_like_any_other()
    {
        ledger.Assign(100m, "Hobby", march);
        ledger.RecordExpense(40m, "Hobby", Today);
        ledger.ArchiveCategory("Hobby");

        Assert.Equal(Euros(1960m), ledger.PeriodLeftover(march));
    }

    // Savings is backed now, but February ended before the ledger was made: nothing was backed then.
    [Fact]
    public void A_period_that_ended_before_the_first_start_had_nothing_backed()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.RecordExpense(30m, "Savings", february.LastDay);

        Assert.Equal(Euros(-30m), ledger.PeriodLeftover(february));
    }

    // ------------------------------------------------------------------ settling

    [Fact]
    public void Each_period_that_ended_is_swept_at_its_own_end_in_order_and_told_once()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.SetSweepDestination("Savings");
        ledger.RecordIncome(100m, "April", april.FirstDay);
        ledger.RecordIncome(200m, "Mei", ledger.Calendar.Next(april).FirstDay);

        MoveTo(new DateOnly(2026, 6, 5));
        Assert.True(ledger.Settle());

        Assert.Equal(
            [(new DateOnly(2026, 4, 1), new DateOnly(2026, 3, 1), 2000m), (new DateOnly(2026, 5, 1), new DateOnly(2026, 4, 1), 100m),
             (new DateOnly(2026, 6, 1), new DateOnly(2026, 5, 1), 200m)],
            Sweeps().Select(m => (m.Date, m.SweptFor!.Value, m.Amount.Euros)));
        Assert.Equal(
            [march.FirstDay, april.FirstDay, new DateOnly(2026, 5, 1)],
            ledger.TakeSweepsMade().Select(s => s.Period.FirstDay));
        Assert.Empty(ledger.TakeSweepsMade());
        Assert.Equal(Euros(0m), ledger.BalanceOf(Bank));
    }

    [Fact]
    public void Nothing_is_swept_without_a_destination()
    {
        ledger.SetBacking("Savings", deposit);

        MoveTo(april.FirstDay);

        Assert.False(ledger.Settle());
        Assert.Empty(Sweeps());
        Assert.Empty(ledger.TakeSweepsMade());
        Assert.Equal((SweepLineKind.StillToSweep, Euros(2000m), false, ""), Read(ledger.SweepLineFor(march)));
    }

    [Theory]
    [InlineData(2000, 0)]
    [InlineData(2030, -30)]
    public void Nothing_is_swept_at_zero_or_below_and_savings_are_never_drawn_on(int spent, int leftover)
    {
        ledger.SetBacking("Savings", deposit);
        ledger.SetSweepDestination("Savings");
        ledger.Assign(200m, "Savings", march);
        ledger.Assign(200m, "Groceries", march);
        ledger.RecordExpense(spent - 200m, "Groceries", Today);

        MoveTo(april.FirstDay);
        ledger.Settle();

        Assert.Empty(Sweeps());
        Assert.Equal(Euros(leftover), ledger.PeriodLeftover(march));
        Assert.Equal(Euros(200m), ledger.BalanceOf(deposit));
        Assert.Equal(
            leftover < 0 ? (SweepLineKind.Shortfall, Euros(leftover), false, "") : null,
            Read(ledger.SweepLineFor(march)));
    }

    [Fact]
    public void Into_a_category_the_pool_account_backs_the_sweep_moves_no_balance_and_counts_in_Accumulated()
    {
        ledger.SetBacking("Savings", Bank);
        ledger.SetSweepDestination("Savings");

        MoveTo(april.FirstDay);
        ledger.Settle();

        var sweep = Assert.Single(ledger.TakeSweepsMade()).Movement;
        Assert.Equal((Bank, Bank, Euros(2000m)), (sweep.From, sweep.To, sweep.Amount));
        Assert.Equal(Euros(2000m), ledger.BalanceOf(Bank));
        Assert.Empty(ledger.HistoryOf(Bank).OfType<Movement>());
        Assert.Equal(Euros(2000m), ledger.AccumulatedFor("Savings", april));
        Assert.Equal(Euros(0m), ledger.AccumulatedFor("Savings", march));
    }

    // The old period ends before the new one's money moves: the sweep has the lower id.
    [Fact]
    public void The_ended_period_is_swept_before_the_new_periods_planned_money_moves()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.SetSweepDestination("Savings");
        ledger.Assign(150m, "Savings", april);

        MoveTo(april.FirstDay);
        ledger.Settle();

        Assert.Equal(
            [(MovementReason.Swept, 2000m), (MovementReason.Assigned, 150m)],
            ledger.HistoryOf(deposit).OfType<Movement>().OrderBy(m => m.Id).Select(m => (m.Reason, m.Amount.Euros)));
    }

    // ------------------------------------------------------------------ the line

    [Fact]
    public void The_current_period_and_later_ones_have_no_line()
    {
        Assert.Null(ledger.SweepLineFor(march));
        Assert.Null(ledger.SweepLineFor(april));
    }

    [Fact]
    public void A_swept_period_names_what_went_and_offers_no_button()
    {
        SweptIntoSavings();

        var line = ledger.SweepLineFor(march)!;

        Assert.Equal((SweepLineKind.Swept, Euros(2000m), false), (line.Kind, line.Amount, line.CanBringUpToDate));
        Assert.Equal([("Savings", Euros(2000m))], line.Parts.Select(p => (p.Category.Name, p.Amount)));
    }

    [Fact]
    public void A_late_income_shows_money_still_to_sweep_and_a_late_expense_money_swept_too_much()
    {
        SweptIntoSavings();

        var income = ledger.RecordIncome(100m, "Terugbetaling", march.LastDay).Income!;
        Assert.Equal((SweepLineKind.StillToSweep, Euros(100m), true), Kind(ledger.SweepLineFor(march)!));

        ledger.RemoveIncome(income);
        ledger.RecordExpense(40m, "Groceries", march.LastDay);
        Assert.Equal((SweepLineKind.SweptTooMuch, Euros(40m), true), Kind(ledger.SweepLineFor(march)!));

        static (SweepLineKind, Money, bool) Kind(SweepLine line) => (line.Kind, line.Amount, line.CanBringUpToDate);
    }

    // One line at a time: what was swept first, and the shortfall once that is back.
    [Fact]
    public void A_swept_period_that_falls_short_shows_swept_too_much_before_its_shortfall()
    {
        SweptIntoSavings();
        ledger.RecordExpense(2030m, "Groceries", march.LastDay);

        Assert.Equal(SweepLineKind.SweptTooMuch, ledger.SweepLineFor(march)!.Kind);
        Assert.Equal(Euros(2000m), ledger.SweepLineFor(march)!.Amount);

        ledger.BringUpToDate(march);

        Assert.Equal((SweepLineKind.Shortfall, Euros(-30m), false, ""), Read(ledger.SweepLineFor(march)));
    }

    // ------------------------------------------------------------------ bringing up to date

    [Fact]
    public void Money_still_to_sweep_goes_to_todays_destination_dated_today()
    {
        SweptIntoSavings();
        ledger.SetBacking("Holiday", broker);
        ledger.SetSweepDestination("Holiday");
        ledger.RecordIncome(100m, "Terugbetaling", march.LastDay);

        var result = ledger.BringUpToDate(march);

        var move = Assert.Single(result.Moves);
        Assert.Equal(("Holiday", Bank, broker, Euros(100m), ledger.Today, march.FirstDay, MovementDirection.In),
                     (move.Category.Name, move.From, move.To, move.Amount, move.Date, move.SweptFor!.Value, move.Direction));
        Assert.Equal(Money.Zero, result.LetGo);
        Assert.Equal(
            [("Savings", Euros(2000m)), ("Holiday", Euros(100m))],
            ledger.SweepLineFor(march)!.Parts.Select(p => (p.Category.Name, p.Amount)));
    }

    [Fact]
    public void Money_swept_too_much_comes_back_latest_first_each_giving_at_most_what_it_received()
    {
        SweptIntoSavings();
        ledger.SetBacking("Holiday", broker);
        ledger.SetSweepDestination("Holiday");
        ledger.RecordIncome(300m, "Terugbetaling", march.LastDay);
        ledger.BringUpToDate(march);
        ledger.RecordExpense(400m, "Groceries", march.LastDay);

        var result = ledger.BringUpToDate(march);

        Assert.Equal(
            [("Holiday", broker, Euros(300m)), ("Savings", deposit, Euros(100m))],
            result.Moves.Select(m => (m.Category.Name, m.From, m.Amount)));
        Assert.All(result.Moves, m => Assert.Equal((Bank, MovementDirection.Out), (m.To, m.Direction)));
        Assert.Equal([("Savings", Euros(1900m))], ledger.SweepLineFor(march)!.Parts.Select(p => (p.Category.Name, p.Amount)));
    }

    // Holiday is the latest, but only 100 of its 300 is still there: it gives that, and Savings the rest.
    [Fact]
    public void When_the_latest_is_capped_the_one_before_it_gives_the_rest_before_anything_is_let_go()
    {
        SweptIntoSavings();
        ledger.SetBacking("Holiday", broker);
        ledger.SetSweepDestination("Holiday");
        ledger.RecordIncome(300m, "Terugbetaling", march.LastDay);
        ledger.BringUpToDate(march);
        ledger.RecordExpense(200m, "Holiday", ledger.Today, account: broker);
        ledger.RecordExpense(400m, "Groceries", march.LastDay);

        var result = ledger.BringUpToDate(march);

        Assert.Equal([("Holiday", Euros(100m)), ("Savings", Euros(300m))], result.Moves.Select(m => (m.Category.Name, m.Amount)));
        Assert.Equal(Money.Zero, result.LetGo);
    }

    // Only 20 is there for Savings: 20 comes back, 30 is let go, and the line stops asking for good,
    // even once more is built up for Savings.
    [Fact]
    public void What_none_can_give_back_is_let_go_and_the_line_stops_asking_for_good()
    {
        SweptIntoSavings();
        ledger.RecordExpense(1980m, "Savings", ledger.Today, account: deposit);
        ledger.RecordExpense(50m, "Groceries", march.LastDay);

        var result = ledger.BringUpToDate(march);

        Assert.Equal(Euros(20m), Assert.Single(result.Moves).Amount);
        Assert.Equal(Euros(30m), result.LetGo);
        Assert.Equal(new SweepLineKind?(SweepLineKind.Swept), ledger.SweepLineFor(march)?.Kind);
        Assert.Equal(Euros(1980m), ledger.SweepLineFor(march)!.Amount);

        ledger.Assign(100m, "Savings", ledger.CurrentPeriod);

        Assert.Equal(SweepLineKind.Swept, ledger.SweepLineFor(march)!.Kind);

        // Measured against what really moved, 1980: a refund of 20 fills the 30 let go and leaves
        // nothing to sweep; 30 more takes it 20 past what moved, which is still to sweep.
        ledger.RecordIncome(20m, "Terugbetaling", march.LastDay);
        Assert.Equal(SweepLineKind.Swept, ledger.SweepLineFor(march)!.Kind);

        ledger.RecordIncome(30m, "Nog een", march.LastDay);
        Assert.Equal((SweepLineKind.StillToSweep, Euros(20m)), (ledger.SweepLineFor(march)!.Kind, ledger.SweepLineFor(march)!.Amount));
    }

    // Per move, ruled at the build (2026-09-28): 2000 to Savings, then 100 to Holiday, then 50 to
    // Savings. 200 too much undoes the 50, the 100 and 50 of the 2000: Savings gives 100, Holiday 100,
    // one movement each, Savings first because its move was undone first.
    [Fact]
    public void Money_swept_too_much_undoes_the_latest_move_first_whichever_category_it_went_to()
    {
        SweptIntoSavings();
        ledger.SetBacking("Holiday", broker);
        ledger.SetSweepDestination("Holiday");
        ledger.RecordIncome(100m, "Terugbetaling", march.LastDay);
        ledger.BringUpToDate(march);
        ledger.SetSweepDestination("Savings");
        ledger.RecordIncome(50m, "Nog een", march.LastDay);
        ledger.BringUpToDate(march);
        ledger.RecordExpense(200m, "Groceries", march.LastDay);

        var result = ledger.BringUpToDate(march);

        Assert.Equal([("Savings", Euros(100m)), ("Holiday", Euros(100m))], result.Moves.Select(m => (m.Category.Name, m.Amount)));
        Assert.Equal(Money.Zero, result.LetGo);
        Assert.Equal([("Savings", Euros(1950m))], ledger.SweepLineFor(march)!.Parts.Select(p => (p.Category.Name, p.Amount)));
    }

    // A move already partly undone gives back only the rest of it before the one before it is asked.
    [Fact]
    public void A_move_partly_undone_before_gives_back_only_what_is_left_of_it()
    {
        SweptIntoSavings();
        ledger.SetBacking("Holiday", broker);
        ledger.SetSweepDestination("Holiday");
        ledger.RecordIncome(100m, "Terugbetaling", march.LastDay);
        ledger.BringUpToDate(march);
        ledger.RecordExpense(60m, "Groceries", march.LastDay);
        Assert.Equal([("Holiday", Euros(60m))], ledger.BringUpToDate(march).Moves.Select(m => (m.Category.Name, m.Amount)));

        ledger.RecordExpense(100m, "Groceries", march.LastDay);

        Assert.Equal(
            [("Holiday", Euros(40m)), ("Savings", Euros(60m))],
            ledger.BringUpToDate(march).Moves.Select(m => (m.Category.Name, m.Amount)));
    }

    // Nothing is there at all: nothing moves, all of it is let go, and the line stops asking.
    [Fact]
    public void With_nothing_there_the_button_moves_nothing_and_lets_it_all_go()
    {
        SweptIntoSavings();
        ledger.RecordExpense(2000m, "Savings", ledger.Today, account: deposit);
        ledger.RecordExpense(40m, "Groceries", march.LastDay);

        var result = ledger.BringUpToDate(march);

        Assert.Empty(result.Moves);
        Assert.Equal(Euros(40m), result.LetGo);
        Assert.Equal(SweepLineKind.Swept, ledger.SweepLineFor(march)!.Kind);
    }

    // Since increment 15 "—" leaves swept money where it is, still the category's, and money swept
    // too much comes back out of it, from that account (§12, follow-up 14).
    [Fact]
    public void A_category_set_to_none_gives_back_from_the_money_it_left_behind()
    {
        SweptIntoSavings();
        ledger.SetBacking("Savings", null);
        ledger.RecordExpense(40m, "Groceries", march.LastDay);

        var move = Assert.Single(ledger.BringUpToDate(march).Moves);

        Assert.Equal((deposit, Bank, Euros(40m)), (move.From, move.To, move.Amount));
        Assert.Equal(deposit, ledger.LeftOn("Savings"));
    }

    // Backing again carries the history on, so the sweep into it can still come back.
    [Fact]
    public void A_category_set_to_none_and_backed_again_since_still_gives_back()
    {
        SweptIntoSavings();
        ledger.SetBacking("Savings", null);
        ledger.SetBacking("Savings", broker);
        ledger.RecordExpense(40m, "Groceries", march.LastDay);

        var move = Assert.Single(ledger.BringUpToDate(march).Moves);

        Assert.Equal((broker, Bank, Euros(40m)), (move.From, move.To, move.Amount));
    }

    [Fact]
    public void Re_pointing_keeps_what_can_come_back_and_it_comes_from_the_new_account()
    {
        SweptIntoSavings();
        ledger.SetBacking("Savings", broker);
        ledger.RecordExpense(40m, "Groceries", march.LastDay);

        var move = Assert.Single(ledger.BringUpToDate(march).Moves);

        Assert.Equal((broker, Bank, Euros(40m)), (move.From, move.To, move.Amount));
    }

    [Fact]
    public void The_button_throws_where_the_line_offers_none()
    {
        Assert.Throws<InvalidOperationException>(() => ledger.BringUpToDate(march));

        SweptIntoSavings();
        Assert.Throws<InvalidOperationException>(() => ledger.BringUpToDate(march));
    }

    // ------------------------------------------------------------------ the destination

    [Fact]
    public void The_choices_are_the_backed_categories_not_archived_in_the_order_added()
    {
        ledger.SetBacking("Groceries", deposit);
        ledger.SetBacking("Savings", Bank);
        ledger.SetBacking("Holiday", broker);
        ledger.ArchiveCategory("Holiday");

        Assert.Equal(["Savings", "Groceries"], ledger.SweepDestinationChoices.Select(c => c.Name));
    }

    // The list writes back what it shows on every redraw: that must not even settle.
    [Fact]
    public void Choosing_the_destination_already_set_changes_nothing_and_does_not_settle()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.SetSweepDestination("Savings");
        MoveTo(april.FirstDay);

        var result = ledger.SetSweepDestination("savings");

        Assert.Equal(SweepDestinationOutcome.Unchanged, result.Outcome);
        Assert.Empty(ledger.TakeSweepsMade());
        Assert.True(ledger.Settle(), "Choosing what was set settled.");
        Assert.Single(ledger.TakeSweepsMade());
    }

    [Fact]
    public void Changing_the_destination_settles_first_so_the_period_that_ended_goes_where_it_was_set_to_go()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.SetBacking("Holiday", broker);
        ledger.SetSweepDestination("Savings");
        MoveTo(april.FirstDay);

        var result = ledger.SetSweepDestination("Holiday");

        Assert.Equal((SweepDestinationOutcome.Chosen, "Savings", "Holiday"), (result.Outcome, result.Before!.Name, result.After!.Name));
        Assert.Equal("Savings", Assert.Single(ledger.TakeSweepsMade()).Movement.Category.Name);
    }

    [Fact]
    public void Choosing_what_cannot_be_chosen_throws()
    {
        ledger.SetBacking("Holiday", broker);
        ledger.ArchiveCategory("Holiday");

        Assert.Throws<InvalidOperationException>(() => ledger.SetSweepDestination("Nothing"));
        Assert.Throws<InvalidOperationException>(() => ledger.SetSweepDestination("Groceries"));
        Assert.Throws<InvalidOperationException>(() => ledger.SetSweepDestination("Holiday"));
    }

    [Fact]
    public void Unbacking_archiving_or_deleting_the_destination_clears_it_and_re_pointing_keeps_it()
    {
        ledger.SetBacking("Savings", deposit);
        ledger.SetSweepDestination("Savings");
        ledger.SetBacking("Savings", broker);
        Assert.Equal("Savings", ledger.SweepDestination?.Name);

        ledger.SetBacking("Savings", null);
        Assert.Null(ledger.SweepDestination);

        ledger.SetBacking("Savings", deposit);
        ledger.SetSweepDestination("Savings");
        ledger.ArchiveCategory("Savings");
        Assert.Null(ledger.SweepDestination);
        ledger.AddCategory("Savings");
        Assert.Null(ledger.SweepDestination);

        ledger.SetBacking("Hobby", deposit);
        ledger.SetSweepDestination("Hobby");
        ledger.DeleteCategory("Hobby");
        Assert.Null(ledger.SweepDestination);
    }

    // ------------------------------------------------------------------ deleting

    [Fact]
    public void Any_sweep_into_a_category_keeps_it_from_being_deleted_pool_to_pool_included()
    {
        ledger.SetBacking("Savings", Bank);
        ledger.SetSweepDestination("Savings");
        Assert.True(ledger.CanDelete("Savings"));

        MoveTo(april.FirstDay);
        ledger.Settle();

        Assert.False(ledger.CanDelete("Savings"));
    }

    // An account whose only use is a sweep to itself, made while it was the pool account: deleting it
    // would delete the sweep, and the period would show its money as still to sweep again.
    [Fact]
    public void A_sweep_from_an_account_to_itself_keeps_the_account_from_being_deleted()
    {
        var cash = ledger.AddAccount("Cash", null).Account!;
        ledger.MakePool(cash);
        ledger.SetBacking("Savings", cash);
        ledger.SetSweepDestination("Savings");
        MoveTo(april.FirstDay);
        ledger.Settle();
        ledger.SetBacking("Savings", null);
        ledger.MakePool(ledger.AccountNamed("Bank")!);

        Assert.False(ledger.CanDeleteAccount(cash));
    }

    // A category deleted leaves the record of what was backed when a period ended.
    [Fact]
    public void Deleting_a_category_takes_it_out_of_the_periods_ends()
    {
        ledger.SetBacking("Hobby", deposit);
        MoveTo(april.FirstDay);
        ledger.Settle();
        ledger.SetBacking("Hobby", null);

        ledger.DeleteCategory("Hobby");

        var restored = Ledger.FromSnapshot(ledger.ToSnapshot(), clock);
        Assert.Equal(Euros(2000m), restored.PeriodLeftover(march));
    }
}
