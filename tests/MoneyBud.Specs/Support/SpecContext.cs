using MoneyBud.Domain;

namespace MoneyBud.Specs.Support;

/// <summary>
/// One scenario's world: a ledger with a frozen clock, and whatever came of the last attempt to
/// record an expense. Reqnroll creates one per scenario, so scenarios cannot leak into each other.
/// </summary>
public sealed class SpecContext
{
    /// <summary>
    /// The day every scenario is run on. Mid-month, so that "today" is neither the first nor the
    /// last day of a period, and with February as the previous month, so a period of a length
    /// other than 31 days is exercised by the boundary scenarios.
    /// </summary>
    public static readonly DateOnly Today = new(2026, 3, 15);

    public SpecContext()
    {
        Clock = new FixedClock(new DateTimeOffset(Today.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero));
        Ledger = new Ledger(Clock);
    }

    public TimeProvider Clock { get; }

    public Ledger Ledger { get; }

    /// <summary>What came of the last recording attempt. Null until one has been made.</summary>
    public RecordExpenseResult? LastResult { get; set; }

    public RecordExpenseResult Result =>
        LastResult ?? throw new InvalidOperationException("No expense has been recorded yet.");
}
