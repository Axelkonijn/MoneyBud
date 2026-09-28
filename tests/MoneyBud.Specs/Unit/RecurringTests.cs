using MoneyBud.Domain;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for recurring entries (arc42 §12, <i>Recurring entries</i>; ADR 0011): the dates a
/// repeat falls on, the order settling records in, which occurrence sets the next, what removing,
/// stopping and starting again leave, and the frequency a locked entry is handed — the edges the
/// scenarios reach one at a time.
/// </summary>
public sealed class RecurringTests
{
    private static readonly DateOnly Today = new(2026, 8, 25);

    private readonly FixedClock clock = new(Noon(Today));
    private readonly Ledger ledger;

    public RecurringTests()
    {
        ledger = new Ledger(clock, "Bank");
        ledger.AddCategory("Subscriptions");
    }

    private static DateTimeOffset Noon(DateOnly day) => new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

    private void MoveTo(DateOnly day)
    {
        clock.Now = Noon(day);
        ledger.Settle();
    }

    private Expense Netflix(DateOnly? date = null, Frequency? repeat = Frequency.Monthly) =>
        ledger.RecordExpense(13.99m, "Subscriptions", date ?? Today, "Netflix", repeat: repeat).Expense!;

    private List<Expense> Expenses() =>
        ledger.ExpensesIn(new BudgetPeriod(new DateOnly(2000, 1, 1), new DateOnly(2100, 1, 1))).ToList();

    private List<DateOnly> ExpenseDates() => Expenses().Select(e => e.Date).ToList();

    private Expense Latest() => Expenses().MaxBy(e => e.Id)!;

    // ------------------------------------------------------------------ the dates

    [Fact]
    public void A_weekly_repeat_comes_every_seven_days_from_its_date()
    {
        Netflix(repeat: Frequency.Weekly);

        MoveTo(new DateOnly(2026, 9, 15));

        Assert.Equal(
            [Today, new(2026, 9, 1), new(2026, 9, 8), new(2026, 9, 15)],
            ExpenseDates());
    }

    // Ruling 5: the day it was set to, clamped to a short month and back where the month has it.
    [Theory]
    [InlineData("2027-01-31", "2027-02-28 2027-03-31 2027-04-30 2027-05-31")]
    [InlineData("2027-01-30", "2027-02-28 2027-03-30 2027-04-30 2027-05-30")]
    [InlineData("2027-01-29", "2027-02-28 2027-03-29 2027-04-29 2027-05-29")]
    [InlineData("2028-01-31", "2028-02-29 2028-03-31 2028-04-30 2028-05-31")]
    [InlineData("2026-12-31", "2027-01-31 2027-02-28 2027-03-31 2027-04-30")]
    public void A_monthly_repeat_keeps_its_day_through_short_months(string started, string then)
    {
        var start = DateOnly.Parse(started);
        MoveTo(start);
        Netflix(start);

        var dates = then.Split(' ').Select(DateOnly.Parse).ToList();
        MoveTo(dates[^1]);

        Assert.Equal([start, .. dates], ExpenseDates());
    }

    // ------------------------------------------------------------------ settling, day by day

    // Follow-up 5, and the plan's order on a boundary day. The 31 August occurrence is August's, and
    // recorded before August is swept; the 1 September one comes after the boundary.
    [Fact]
    public void Settling_records_a_periods_occurrences_before_its_sweep_and_the_boundary_days_after_it()
    {
        var deposit = ledger.AddAccount("Deposit", 0m).Account!;
        ledger.AddCategory("Savings");
        ledger.SetBacking("Savings", deposit);
        ledger.SetSweepDestination("Savings");
        ledger.RecordIncome(1000m, "Salaris", Today);
        ledger.RecordExpense(45m, "Subscriptions", new DateOnly(2026, 8, 24), "Markt", repeat: Frequency.Weekly);
        ledger.RecordExpense(20m, "Subscriptions", Today, "Bakker", repeat: Frequency.Weekly);

        MoveTo(new DateOnly(2026, 9, 5));

        var sweep = ledger.HistoryOf(deposit).OfType<Movement>().Single(m => m.Reason == MovementReason.Swept);
        Assert.Equal(Money.FromEuros(1000m - 45m - 45m - 20m), sweep.Amount);
        var onTheEnd = Expenses().Single(e => e.Date == new DateOnly(2026, 8, 31));
        var onTheBoundary = Expenses().Single(e => e.Date == new DateOnly(2026, 9, 1));
        Assert.True(onTheEnd.Id < sweep.Id);
        Assert.True(sweep.Id < onTheBoundary.Id);
    }

    [Fact]
    public void Several_due_on_one_day_are_recorded_in_the_order_their_repeats_were_set_up()
    {
        ledger.AddCategory("Rent");
        ledger.RecordExpense(900m, "Rent", Today, "Huur", repeat: Frequency.Monthly);
        Netflix();
        ledger.TakeOccurrencesMade();

        MoveTo(new DateOnly(2026, 9, 25));

        Assert.Equal(
            ["Huur", "Netflix"],
            ledger.TakeOccurrencesMade().Select(o => ((Expense)o.Entry).Label));
    }

    // Follow-up 4: set up in the past, what is due comes at once, each on its own date.
    [Fact]
    public void A_repeat_set_up_in_the_past_records_what_is_due_at_once()
    {
        Netflix(new DateOnly(2026, 6, 25));

        Assert.Equal([new(2026, 6, 25), new(2026, 7, 25), Today], ExpenseDates());
        Assert.Equal(2, ledger.TakeOccurrencesMade().Count);
    }

    [Fact]
    public void Occurrences_made_are_given_once()
    {
        Netflix();
        MoveTo(new DateOnly(2026, 9, 25));

        Assert.Single(ledger.TakeOccurrencesMade());
        Assert.Empty(ledger.TakeOccurrencesMade());
    }

    // A clock turned back finds nothing due, and undoes nothing.
    [Fact]
    public void A_clock_turned_back_records_nothing()
    {
        Netflix();
        MoveTo(new DateOnly(2026, 9, 25));
        MoveTo(new DateOnly(2026, 8, 20));
        MoveTo(new DateOnly(2026, 9, 25));

        Assert.Equal([Today, new(2026, 9, 25)], ExpenseDates());
    }

    // Ruling 8.
    [Fact]
    public void An_occurrence_on_an_archived_category_brings_it_back_and_says_so()
    {
        Netflix();
        ledger.ArchiveCategory("Subscriptions");

        MoveTo(new DateOnly(2026, 9, 25));

        Assert.False(ledger.IsArchived("Subscriptions"));
        Assert.Equal("Subscriptions", Assert.Single(ledger.TakeOccurrencesMade()).BroughtBack?.Name);
    }

    [Fact]
    public void An_occurrence_copies_the_latest_including_its_account_and_is_the_new_latest()
    {
        var card = ledger.AddAccount("Card", 0m).Account!;
        var first = ledger.RecordExpense(13.99m, "Subscriptions", Today, "Netflix", card, Frequency.Monthly).Expense!;

        MoveTo(new DateOnly(2026, 9, 25));

        var next = Latest();
        Assert.Equal((first.Amount, first.Label, first.Category, card), (next.Amount, next.Label, next.Category, next.Account));
        Assert.Null(ledger.FrequencyOf(first));
        Assert.False(ledger.SetsTheRepeat(first));
        Assert.Equal(Frequency.Monthly, ledger.FrequencyOf(next));
    }

    // ------------------------------------------------------------------ the latest

    // The latest is the one recorded most recently, not the one with the latest date.
    [Fact]
    public void An_earlier_occurrence_moved_past_the_latests_date_does_not_become_the_latest()
    {
        var first = Netflix();
        MoveTo(new DateOnly(2026, 9, 30));
        var september = Latest();

        ledger.ChangeExpense(first, 13.99m, "Subscriptions", new DateOnly(2026, 9, 28), "Netflix");

        Assert.Equal(Frequency.Monthly, ledger.FrequencyOf(september));
        Assert.Null(ledger.FrequencyOf(first));
    }

    // A frequency handed in for a locked entry is ignored: the change is to that entry alone.
    [Fact]
    public void A_locked_occurrence_ignores_the_frequency_it_is_given()
    {
        var first = Netflix();
        MoveTo(new DateOnly(2026, 9, 25));

        var result = ledger.ChangeExpense(first, 13.99m, "Subscriptions", Today, "Netflix", null, Frequency.Weekly);

        Assert.Equal(ChangeOutcome.Unchanged, result.Outcome);
        MoveTo(new DateOnly(2026, 10, 2));
        Assert.Equal([Today, new(2026, 9, 25)], ExpenseDates());
    }

    [Fact]
    public void Saving_the_latest_with_its_own_frequency_is_unchanged_and_with_another_is_a_change()
    {
        var netflix = Netflix();

        Assert.Equal(ChangeOutcome.Unchanged,
            ledger.ChangeExpense(netflix, 13.99m, "Subscriptions", Today, "Netflix", null, Frequency.Monthly).Outcome);
        Assert.Equal(ChangeOutcome.Changed,
            ledger.ChangeExpense(netflix, 13.99m, "Subscriptions", Today, "Netflix", null, Frequency.Weekly).Outcome);
        Assert.Equal(Frequency.Weekly, ledger.FrequencyOf(netflix));
    }

    // The overload without a frequency keeps the repeat's frequency.
    [Fact]
    public void A_change_that_names_no_frequency_keeps_the_frequency()
    {
        var netflix = Netflix();

        ledger.ChangeExpense(netflix, 17.99m, "Subscriptions", Today, "Netflix");

        Assert.Equal(Frequency.Monthly, ledger.FrequencyOf(netflix));
    }

    [Fact]
    public void A_refused_change_leaves_the_repeat_as_it_was()
    {
        var netflix = Netflix();

        var result = ledger.ChangeExpense(netflix, 0m, "Subscriptions", Today, "Netflix", null, null);

        Assert.True(result.WasRefused);
        Assert.Equal(Frequency.Monthly, ledger.FrequencyOf(netflix));
    }

    // Follow-up 3 with ruling 5: only a changed date sets the day, never a clamped one.
    [Fact]
    public void Only_a_changed_date_on_the_latest_moves_the_day()
    {
        MoveTo(new DateOnly(2027, 1, 31));
        Netflix(new DateOnly(2027, 1, 31));
        MoveTo(new DateOnly(2027, 2, 28));
        ledger.ChangeExpense(Latest(), 15.99m, "Subscriptions", new DateOnly(2027, 2, 28), "Netflix");
        MoveTo(new DateOnly(2027, 3, 31));
        Assert.Equal(new DateOnly(2027, 3, 31), Latest().Date);

        ledger.ChangeExpense(Latest(), 15.99m, "Subscriptions", new DateOnly(2027, 3, 27), "Netflix");
        MoveTo(new DateOnly(2027, 4, 30));

        Assert.Equal(new DateOnly(2027, 4, 27), Latest().Date);
    }

    // ------------------------------------------------------------------ removing, stopping, starting again

    // Follow-up 1: the one recorded before takes over, and the next date does not move.
    [Fact]
    public void Removing_the_latest_hands_the_repeat_to_the_one_before_and_keeps_the_next_date()
    {
        Netflix();
        MoveTo(new DateOnly(2026, 10, 26));
        var october = Latest();
        ledger.ChangeExpense(october, 15.99m, "Subscriptions", new DateOnly(2026, 10, 20), "Netflix");
        ledger.RemoveExpense(Latest());

        var september = Latest();
        Assert.Equal(Frequency.Monthly, ledger.FrequencyOf(september));
        MoveTo(new DateOnly(2026, 11, 20));
        Assert.Equal((new DateOnly(2026, 11, 20), Money.FromEuros(13.99m)), (Latest().Date, Latest().Amount));
    }

    [Fact]
    public void Removing_the_only_occurrence_ends_the_repeat()
    {
        var netflix = Netflix();

        ledger.RemoveExpense(netflix);
        MoveTo(new DateOnly(2026, 12, 1));

        Assert.Empty(Expenses());
        Assert.True(ledger.CanDelete("Subscriptions"));
    }

    [Fact]
    public void One_off_stops_the_repeat_and_the_last_occurrence_starts_it_again_with_what_is_due()
    {
        Netflix();
        MoveTo(new DateOnly(2026, 9, 25));
        var september = Latest();
        ledger.ChangeExpense(september, 13.99m, "Subscriptions", september.Date, "Netflix", null, null);
        MoveTo(new DateOnly(2026, 10, 30));
        Assert.Equal(2, Expenses().Count);
        Assert.True(ledger.SetsTheRepeat(september));
        Assert.Null(ledger.FrequencyOf(september));

        ledger.ChangeExpense(september, 13.99m, "Subscriptions", september.Date, "Netflix", null, Frequency.Monthly);

        Assert.Equal(new DateOnly(2026, 10, 25), Latest().Date);
        Assert.Equal(Frequency.Monthly, ledger.FrequencyOf(Latest()));
    }

    // Ruled at the scenario stage, 1: the one before becomes the stopped repeat's last, changeable,
    // and nothing is recorded.
    [Fact]
    public void Removing_a_stopped_repeats_last_occurrence_leaves_it_stopped()
    {
        var first = Netflix();
        MoveTo(new DateOnly(2026, 9, 25));
        var september = Latest();
        ledger.ChangeExpense(september, 13.99m, "Subscriptions", september.Date, "Netflix", null, null);

        ledger.RemoveExpense(september);
        MoveTo(new DateOnly(2026, 11, 26));

        Assert.Equal([first.Id], Expenses().Select(e => e.Id));
        Assert.True(ledger.SetsTheRepeat(first));
        Assert.Null(ledger.FrequencyOf(first));
    }

    [Fact]
    public void An_income_repeats_as_an_expense_does()
    {
        var salaris = ledger.RecordIncome(2500m, "Salaris", new DateOnly(2026, 8, 27), repeat: Frequency.Monthly).Income!;

        MoveTo(new DateOnly(2026, 9, 27));

        var incomes = ledger.IncomesIn(ledger.CurrentPeriod);
        var september = Assert.Single(incomes);
        Assert.Equal((new DateOnly(2026, 9, 27), "Salaris"), (september.Date, september.Label));
        Assert.Equal(Frequency.Monthly, ledger.FrequencyOf(september));
        Assert.False(ledger.SetsTheRepeat(salaris));
    }
}
