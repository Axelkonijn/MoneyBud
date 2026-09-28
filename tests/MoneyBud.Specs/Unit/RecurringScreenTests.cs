using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Recurring entries on screen (arc42 §12, <i>Recurring entries</i>; plan for increment 12): the
/// <i>Herhalen</i> list, its lock and its write-backs, the grey label and where it is not, the notice
/// and the order it says things in, and when occurrences are kept.
/// </summary>
public sealed class RecurringScreenTests
{
    private static readonly DateOnly Today = new(2026, 8, 25);

    private readonly FixedClock clock = new(Noon(Today));
    private readonly Ledger ledger;
    private readonly CountingStore store = new();

    public RecurringScreenTests()
    {
        ledger = new Ledger(clock, "Bank");
        ledger.AddCategory("Subscriptions");
    }

    private static DateTimeOffset Noon(DateOnly day) => new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

    private void MoveTo(DateOnly day) => clock.Now = Noon(day);

    private Expense Netflix(DateOnly? date = null) =>
        ledger.RecordExpense(13.99m, "Subscriptions", date ?? Today, "Netflix", repeat: Frequency.Monthly).Expense!;

    // ------------------------------------------------------------------ the list

    [Fact]
    public void The_list_offers_one_off_weekly_and_monthly_in_that_order_and_starts_on_one_off()
    {
        var app = new MoneyBudApp(ledger);

        Assert.Equal(["Eenmalig", "Wekelijks", "Maandelijks"], app.ExpenseForm.FrequencyChoices.Select(c => c.Text));
        Assert.Equal(new FrequencyChoice(null), app.ExpenseForm.ChosenFrequency);
        Assert.Equal(new FrequencyChoice(null), app.IncomeForm.ChosenFrequency);
    }

    // The list writes back what it shows on first show and on every redraw. Written back while
    // loaded, it must not turn an unchanged save into a change; written back as nothing, it is ignored.
    [Fact]
    public void The_list_writing_back_what_it_shows_leaves_an_unchanged_save_unchanged()
    {
        Netflix();
        var app = new MoneyBudApp(ledger, store);
        app.EditExpense(app.Overview.Expenses[0]);
        var form = app.ExpenseForm;

        form.ChosenFrequency = form.ChosenFrequency;
        form.ChosenFrequency = null!;
        var saves = store.Saves;
        var result = form.Save();

        Assert.Equal(ChangeOutcome.Unchanged, result!.Outcome);
        Assert.Null(app.Notice);
        Assert.Equal(saves, store.Saves);
    }

    // Plan for increment 12, 4: the tick records the next while the latest is open in the form. The
    // form, still open, now shows one-off, locked; a frequency chosen then is ignored, and saving
    // changes that entry alone.
    [Fact]
    public void An_entry_open_in_the_form_locks_on_one_off_once_the_next_occurrence_is_recorded()
    {
        Netflix();
        var app = new MoneyBudApp(ledger);
        app.EditExpense(app.Overview.Expenses[0]);
        var form = app.ExpenseForm;
        Assert.Equal((Frequency?)Frequency.Monthly, form.ChosenFrequency.Frequency);
        Assert.True(form.CanChangeFrequency);

        MoveTo(new DateOnly(2026, 9, 25));
        app.Tick();

        Assert.True(form.IsEditing);
        Assert.False(form.CanChangeFrequency);
        Assert.Null(form.ChosenFrequency.Frequency);
        form.ChosenFrequency = new FrequencyChoice(Frequency.Weekly);
        Assert.Null(form.ChosenFrequency.Frequency);

        form.Amount = "15,99";
        form.Save();
        var september = Assert.Single(app.OverviewFor(ledger.CurrentPeriod).Expenses);
        Assert.Equal((Money.FromEuros(13.99m), "maandelijks"), (september.Amount, september.RepeatLabel));
    }

    // ------------------------------------------------------------------ the label

    // Ruled at the scenario stage, 3: only the Overview's lists carry it, never an account's history,
    // where an entry is listed but not changed.
    [Fact]
    public void The_label_is_on_the_latest_row_of_the_Overview_and_nowhere_in_an_accounts_history()
    {
        Netflix();
        MoveTo(new DateOnly(2026, 9, 25));
        var app = new MoneyBudApp(ledger);

        Assert.Equal("maandelijks", Assert.Single(app.OverviewFor(ledger.CurrentPeriod).Expenses).RepeatLabel);
        Assert.Null(Assert.Single(app.OverviewFor(ledger.Calendar.Previous(ledger.CurrentPeriod)).Expenses).RepeatLabel);
        Assert.Null(typeof(HistoryLine).GetProperty(nameof(ExpenseLine.RepeatLabel)));
    }

    [Fact]
    public void A_weekly_income_carries_wekelijks()
    {
        ledger.RecordIncome(85m, "Bijbaan", Today, repeat: Frequency.Weekly);

        var app = new MoneyBudApp(ledger);

        Assert.Equal("wekelijks", Assert.Single(app.Overview.Incomes).RepeatLabel);
    }

    // ------------------------------------------------------------------ the notice

    [Fact]
    public void Occurrences_recorded_at_a_start_are_said_in_one_sentence_with_their_days_and_kept()
    {
        Netflix();
        ledger.RecordExpense(9.99m, "Subscriptions", Today, repeat: Frequency.Monthly);
        MoveTo(new DateOnly(2026, 10, 1));

        var app = new MoneyBudApp(ledger, store);

        Assert.Equal(
            "Herhaald: Netflix € 13,99 (25 september), Subscriptions € 9,99 (25 september).",
            app.Notice!.Text);
        Assert.Equal(2, app.Notice.Repeated.Count);
        Assert.Equal(1, store.Saves);
    }

    // Ruled at the build, 2026-09-28: in the order it happened. The act's own sentence first, and
    // what it caused after it.
    [Fact]
    public void Occurrences_an_act_caused_are_said_after_what_the_act_says()
    {
        var app = new MoneyBudApp(ledger, store);

        var june = new DateOnly(2026, 6, 27);
        app.RecordIncome("2500", "Salaris", june, repeat: Frequency.Monthly);

        var typed = ledger.IncomesIn(ledger.Calendar.PeriodContaining(june))[0];
        Assert.StartsWith(Tekst.IncomeRecorded(typed), app.Notice!.Text);
        Assert.EndsWith("Herhaald: Salaris € 2.500,00 (27 juli).", app.Notice.Text);
    }

    // What settling did before the act is said in front of it, as a sweep is.
    [Fact]
    public void Occurrences_settling_recorded_before_an_act_are_said_in_front_of_it()
    {
        Netflix();
        var app = new MoneyBudApp(ledger, store);
        MoveTo(new DateOnly(2026, 9, 25));

        app.RecordExpense("4", "Subscriptions", "Koffie");

        Assert.StartsWith("Herhaald: Netflix € 13,99 (25 september).", app.Notice!.Text);
        Assert.Single(app.Notice.Repeated);
    }

    [Fact]
    public void Occurrences_recorded_before_a_refused_act_are_said_and_kept()
    {
        Netflix();
        var app = new MoneyBudApp(ledger, store);
        MoveTo(new DateOnly(2026, 9, 25));
        var saves = store.Saves;

        app.RecordExpense("0", "Subscriptions", "Niets");

        Assert.True(app.Notice!.IsRefusal);
        Assert.StartsWith("Herhaald: Netflix", app.Notice.Text);
        Assert.Equal(saves + 1, store.Saves);
    }

    [Fact]
    public void An_occurrence_on_an_archived_category_says_it_is_back_in_the_same_notice()
    {
        Netflix();
        ledger.ArchiveCategory("Subscriptions");
        MoveTo(new DateOnly(2026, 9, 25));

        var app = new MoneyBudApp(ledger);

        Assert.Equal("Herhaald: Netflix € 13,99 (25 september). \"Subscriptions\" is weer in gebruik.", app.Notice!.Text);
    }

    [Fact]
    public void The_tick_that_records_an_occurrence_drops_a_waiting_question_and_one_that_records_none_says_nothing()
    {
        Netflix();
        var app = new MoneyBudApp(ledger, store);
        app.AskToRemove(ledger.ExpensesIn(ledger.CurrentPeriod)[0]);

        MoveTo(new DateOnly(2026, 9, 24));
        app.Tick();
        Assert.True(app.IsAsking);

        MoveTo(new DateOnly(2026, 9, 25));
        app.Tick();
        Assert.False(app.IsAsking);
        Assert.StartsWith("Herhaald: ", app.Notice!.Text);
    }

    // Plan for increment 12, 9: removing the latest of a running repeat says it goes on; any other
    // removal asks as before.
    [Fact]
    public void Removing_the_latest_occurrence_says_the_repeat_goes_on()
    {
        var netflix = Netflix();
        var bakker = ledger.RecordExpense(20m, "Subscriptions", Today, "Bakker").Expense!;
        var app = new MoneyBudApp(ledger);

        app.AskToRemove(netflix);
        Assert.EndsWith("De herhaling gaat door; zet hem op Eenmalig om te stoppen.", app.Question!.Text);

        app.AskToRemove(bakker);
        Assert.DoesNotContain("herhaling", app.Question!.Text);
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
