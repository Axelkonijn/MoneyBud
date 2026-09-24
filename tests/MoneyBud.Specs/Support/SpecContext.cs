using MoneyBud.Domain;

namespace MoneyBud.Specs.Support;

/// <summary>
/// One scenario's world: a ledger with a controllable clock, and whatever came of the last
/// attempt to record something. Reqnroll creates one per scenario, so scenarios cannot leak into
/// each other.
/// </summary>
public sealed class SpecContext
{
    /// <summary>
    /// The day every scenario starts on. Mid-month, so that "today" is neither the first nor the
    /// last day of a period, and with February as the previous month, so a period of a length
    /// other than 31 days is exercised by the boundary scenarios.
    /// </summary>
    public static readonly DateOnly Today = new(2026, 3, 15);

    private readonly FixedClock clock = new(Noon(Today));

    public SpecContext() => Ledger = new Ledger(clock);

    public Ledger Ledger { get; }

    /// <summary>What came of the last attempt to record an expense. Null until one was made.</summary>
    public RecordExpenseResult? LastExpenseResult { get; private set; }

    /// <summary>What came of the last attempt to record an income. Null until one was made.</summary>
    public RecordIncomeResult? LastIncomeResult { get; private set; }

    /// <summary>
    /// Whichever of the two came last, as the result it was.
    ///
    /// <para>Needed because a few steps are worded without naming a transaction type, on purpose:
    /// "I should not be warned or asked to confirm" is a claim about MoneyBud rather than about
    /// expenses, and the cent rule is stated by arc42 §8.2 about <i>amounts</i>. Both feature
    /// files use those sentences, so one step definition has to serve both.</para>
    ///
    /// <para><b>Last, rather than whichever is present.</b> Setup steps call the ledger directly
    /// and never come through here, so today only a <c>When</c> ever sets this and presence would
    /// in fact have been enough. Order is the stronger rule and stops being equivalent the moment
    /// one scenario's <c>When</c> steps record an income and an expense — which is also this
    /// design's limit: such a scenario would assert against whichever came last and say nothing
    /// about the other. No feature file does that yet, and a step that needs to name one uses
    /// <see cref="ExpenseResult"/> or <see cref="IncomeResult"/> instead.</para>
    /// </summary>
    public object? LastAttempt { get; private set; }

    public RecordExpenseResult ExpenseResult =>
        LastExpenseResult ?? throw new InvalidOperationException("No expense has been recorded yet.");

    public RecordIncomeResult IncomeResult =>
        LastIncomeResult ?? throw new InvalidOperationException("No income has been recorded yet.");

    public void Record(RecordExpenseResult result)
    {
        LastExpenseResult = result;
        LastAttempt = result;
    }

    public void Record(RecordIncomeResult result)
    {
        LastIncomeResult = result;
        LastAttempt = result;
    }

    /// <summary>Moves the day the ledger considers today. See <see cref="FixedClock"/>.</summary>
    public void SetToday(DateOnly day) => clock.Now = Noon(day);

    /// <summary>Midday, so that no scenario sits within hours of a day boundary.</summary>
    private static DateTimeOffset Noon(DateOnly day) =>
        new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
}
