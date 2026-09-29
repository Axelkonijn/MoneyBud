using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// The period start day (arc42 §12, <i>A configurable period start day</i>; ADR 0012), below what the
/// scenarios reach: plans moved by a change and where they add up, the money that moves for them, the
/// figures a change must leave alone, and the screen's list and question — its write-back, the tick at
/// midnight, and the order of what is said.
/// </summary>
public sealed class StartDayTests
{
    private static readonly DateOnly Today = new(2026, 9, 29);

    private readonly FixedClock clock = new(Noon(Today));
    private readonly Ledger ledger;
    private readonly Account deposit;

    public StartDayTests()
    {
        ledger = new Ledger(clock, "Bank");
        deposit = ledger.AddAccount("Deposit", 0m).Account!;
        ledger.RecordIncome(3000m, "Salaris", new(2026, 9, 1));
    }

    private static DateTimeOffset Noon(DateOnly day) => new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

    private static Money Euros(decimal euros) => Money.FromEuros(euros);

    private BudgetPeriod Next => ledger.Calendar.Next(ledger.CurrentPeriod);

    // ------------------------------------------------------------------ plans made ahead (ruling 4)

    // No single change makes two plans land in one period, but two in one period do: to the 30th cuts
    // September short, to 29 September, and plans can then be made for 30 September on; back to the
    // 1st, and that plan lands in September again, beside September's own. They add up (plan reading
    // 7), and since September has begun, a backed category's money for it moves now, dated today
    // (follow-up 2), where settling would never move it.
    [Fact]
    public void A_plan_made_ahead_that_lands_in_the_current_period_adds_to_its_plan_and_its_money_moves_now()
    {
        ledger.AddCategory("Savings");
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(100m, "Savings", ledger.CurrentPeriod);
        ledger.ChangeStartDay(30);
        Assert.Equal(new BudgetPeriod(new(2026, 9, 30), new(2026, 10, 29)), Next);
        ledger.Assign(40m, "Savings", Next);
        Assert.Equal(Euros(100m), ledger.BalanceOf(deposit));

        var result = ledger.ChangeStartDay(1);

        Assert.Null(result.Ended);
        Assert.Equal(new BudgetPeriod(new(2026, 9, 1), new(2026, 9, 30)), ledger.CurrentPeriod);
        Assert.Equal(Euros(140m), ledger.BudgetFor("Savings", ledger.CurrentPeriod));
        Assert.Equal(Euros(140m), ledger.BalanceOf(deposit));
        Assert.Equal(Euros(140m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
        var moved = ledger.HistoryOf(deposit).OfType<Movement>().First();
        Assert.Equal((Today, Euros(40m)), (moved.Date, moved.Amount));

        clock.Now = Noon(new(2026, 10, 1));
        ledger.Settle();
        Assert.Equal(Euros(140m), ledger.BalanceOf(deposit));
    }

    // Found in review: with the clock once ahead, settling moved October's plan on 1 October; turned
    // back to 29 September, a change to the 27th moves that plan into the new current period, but its
    // money has moved already and does not move again.
    [Fact]
    public void A_plan_settling_already_moved_is_not_moved_again_by_a_change_after_the_clock_went_back()
    {
        ledger.AddCategory("Savings");
        ledger.SetBacking("Savings", deposit);
        ledger.Assign(200m, "Savings", Next);
        clock.Now = Noon(new(2026, 10, 2));
        ledger.Settle();
        clock.Now = Noon(Today);

        ledger.ChangeStartDay(27);

        Assert.Equal(Euros(200m), ledger.BudgetFor("Savings", ledger.CurrentPeriod));
        Assert.Single(ledger.ToSnapshot().Movements, m => m.Reason == MovementReason.Assigned);
        Assert.Equal(Euros(200m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
    }

    // Plans made ahead move into the period their old first day falls in, a budget of zero included,
    // so "never assigned" and "assigned, then taken back" stay as they were (HasBudget).
    [Fact]
    public void Every_plan_made_ahead_moves_with_its_first_day_a_zero_one_included()
    {
        ledger.AddCategory("Groceries");
        var october = Next;
        var november = ledger.Calendar.Next(october);
        ledger.Assign(400m, "Groceries", october);
        ledger.Assign(50m, "Groceries", november);
        ledger.Assign(-50m, "Groceries", november);

        ledger.ChangeStartDay(27);

        Assert.Equal(Euros(400m), ledger.BudgetFor("Groceries", ledger.CurrentPeriod));
        Assert.True(ledger.HasBudget("Groceries", Next));
        Assert.Equal(Money.Zero, ledger.BudgetFor("Groceries", Next));
        Assert.DoesNotContain(ledger.ToSnapshot().Budgets, b => b.PeriodStart.Day == 1 && b.PeriodStart > Today);
    }

    // ------------------------------------------------------------------ what a change leaves alone

    // Follow-up 5, and derived for what is there for a category: counted from the first day the period
    // had at backing or re-pointing, so the cut changes neither. Re-pointed on the 28th with Deposit
    // having paid 20 on the 10th: that 20 was before, and is not counted again after the cut.
    [Fact]
    public void A_change_moves_neither_Accumulated_nor_what_is_there_for_a_category()
    {
        var other = ledger.AddAccount("Other", 0m).Account!;
        ledger.AddCategory("Savings");
        ledger.Assign(300m, "Savings", ledger.CurrentPeriod);
        ledger.RecordExpense(20m, "Savings", new(2026, 9, 10), "Boek", other);
        ledger.SetBacking("Savings", deposit);
        ledger.SetBacking("Savings", other);
        ledger.RecordExpense(15m, "Savings", Today, "Kaart", other);
        var accumulated = ledger.AccumulatedFor("Savings", ledger.CurrentPeriod);
        var there = ledger.ThereFor("Savings");

        ledger.ChangeStartDay(27);

        Assert.Equal(accumulated, ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
        Assert.Equal(there, ledger.ThereFor("Savings"));
        Assert.Equal(Euros(265m), ledger.AccumulatedFor("Savings", ledger.CurrentPeriod));
    }

    [Fact]
    public void Choosing_the_day_already_set_changes_nothing_and_settles_nothing()
    {
        var before = LedgerJsonOf(ledger);
        clock.Now = Noon(new(2026, 10, 2));

        var result = ledger.ChangeStartDay(1);

        Assert.False(result.WasChanged);
        Assert.Equal(before, LedgerJsonOf(ledger));
    }

    private static string LedgerJsonOf(Ledger l) => Storage.LedgerJson.Write(l.ToSnapshot());

    // ------------------------------------------------------------------ on screen

    [Fact]
    public void The_list_writing_back_the_day_set_asks_nothing_says_nothing_and_keeps_nothing()
    {
        var store = new CountingStore();
        var app = new MoneyBudApp(ledger, store);
        var saves = store.Saves;

        app.StartDayChoice = 1;
        app.StartDayChoice = null;

        Assert.False(app.IsAsking);
        Assert.Null(app.Notice);
        Assert.Equal(saves, store.Saves);
    }

    // While the question waits the list shows the day asked about, and writing that back does nothing;
    // choosing the day set then is declining, which puts the list back and says nothing.
    [Fact]
    public void While_the_question_waits_the_list_shows_the_day_asked_about()
    {
        var app = new MoneyBudApp(ledger);

        app.StartDayChoice = 27;
        Assert.True(app.IsAsking);
        Assert.Equal(Tekst.Change, app.Question!.ConfirmText);
        Assert.Equal(27, app.StartDayChoice);
        Assert.Equal(27, app.StartDayShownIn(app.ShownPeriod));

        app.StartDayChoice = 27;
        Assert.True(app.IsAsking);

        app.StartDayChoice = 1;
        Assert.False(app.IsAsking);
        Assert.Null(app.Notice);
        Assert.Equal(1, app.StartDayChoice);
        Assert.Equal(1, ledger.Calendar.StartDay);
    }

    // Plan reading 4: the question names the current period it would give, which a new day can
    // change, so at midnight it goes, and the list is put back, with nothing said.
    [Fact]
    public void A_new_day_drops_the_start_day_question()
    {
        var app = new MoneyBudApp(ledger);
        app.StartDayChoice = 27;

        app.Tick();
        Assert.True(app.IsAsking);

        clock.Now = Noon(Today.AddDays(1));
        app.Tick();

        Assert.False(app.IsAsking);
        Assert.Null(app.Notice);
        Assert.Equal(1, app.StartDayChoice);
    }

    // Found in review: Wijzigen pressed after midnight, before the tick, answers a question about a
    // current period that may be gone, so it changes nothing, says nothing and drops the question.
    [Fact]
    public void Wijzigen_after_midnight_before_the_tick_changes_nothing()
    {
        var store = new CountingStore();
        var app = new MoneyBudApp(ledger, store);
        app.StartDayChoice = 27;
        var saves = store.Saves;

        clock.Now = Noon(Today.AddDays(1));
        app.Confirm();

        Assert.False(app.IsAsking);
        Assert.Null(app.Notice);
        Assert.Equal(1, ledger.Calendar.StartDay);
        Assert.Equal(1, app.StartDayChoice);
        Assert.Equal(saves, store.Saves);
    }

    // A removal question waiting over midnight is not the tick's to drop.
    [Fact]
    public void A_new_day_leaves_a_removal_question_waiting()
    {
        ledger.AddCategory("Groceries");
        var expense = ledger.RecordExpense(10m, "Groceries", Today).Expense!;
        var app = new MoneyBudApp(ledger);
        app.AskToRemove(expense);

        clock.Now = Noon(Today.AddDays(1));
        app.Tick();

        Assert.True(app.IsAsking);
        Assert.Equal(Tekst.Remove, app.Question!.ConfirmText);
    }

    // Reading 2: the change is said first, then the sweep it caused, in the order they happened. The
    // sweep's row in the account's history names the period by the ledger's calendar, not a new one.
    [Fact]
    public void The_change_is_said_before_the_sweep_it_caused_and_the_history_names_the_period_it_swept()
    {
        ledger.AddCategory("Savings");
        ledger.SetBacking("Savings", deposit);
        ledger.SetSweepDestination("Savings");
        var app = new MoneyBudApp(ledger);

        app.StartDayChoice = 27;
        Assert.Equal(
            "Perioden laten beginnen op de 27e? De huidige periode wordt dan 27 sep – 26 okt 2026. 1 – 26 sep 2026 is dan afgelopen.",
            app.Question!.Text);
        app.Confirm();

        var text = app.Notice!.Text;
        Assert.StartsWith("Perioden beginnen nu op de 27e.", text);
        Assert.EndsWith("Restant van 1 – 26 sep 2026: € 3.000,00 naar \"Savings\".", text);

        app.OpenHistory(deposit);
        Assert.Contains(app.History, line => line.Text.StartsWith("Restant van 1 – 26 sep 2026 naar \"Savings\"", StringComparison.Ordinal));
    }

    // Reading 5 for a period not on screen and not current: August stays August, and the drop-down is
    // not offered there.
    [Fact]
    public void A_change_leaves_an_earlier_period_on_the_assign_form_where_it_was()
    {
        var app = new MoneyBudApp(ledger);
        var august = ledger.Calendar.Previous(ledger.CurrentPeriod);
        app.AssignForm.EarlierPeriodCommand.Execute(null);
        Assert.Null(app.StartDayShownIn(august));

        app.StartDayChoice = 27;
        app.Confirm();

        Assert.Equal(august, app.AssignForm.Period);
        Assert.Equal(ledger.CurrentPeriod, app.ShownPeriod);
    }

    private sealed class CountingStore : ILedgerStore
    {
        public int Saves { get; private set; }

        public Claim TryClaim() => Claim.Claimed;

        public LoadResult Load() => new LoadResult.NoData();

        public bool TrySave(LedgerSnapshot snapshot)
        {
            Saves++;
            return true;
        }

        public void Dispose() { }
    }
}
