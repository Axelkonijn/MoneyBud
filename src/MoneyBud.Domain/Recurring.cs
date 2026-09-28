namespace MoneyBud.Domain;

/// <summary>
/// How often a recurring entry repeats (arc42 §12, <i>Recurring entries</i>, ruling 1): every 7 days,
/// or the same day each month. <b>One-off is not a frequency</b>: it is the absence of a repeat, so it
/// is <c>null</c> wherever a <c>Frequency?</c> is taken, and an entry left at it is exactly the one-off
/// entry of every earlier increment. Yearly is deferred until missed.
/// </summary>
public enum Frequency
{
    Weekly,
    Monthly,
}

/// <summary>
/// An occurrence MoneyBud recorded by itself, to be announced once (§12, ruling 7): the entry, and the
/// archived category it brought back, if it did (ruling 8).
/// </summary>
public sealed record OccurrenceMade(IEntry Entry, Category? BroughtBack);

/// <summary>
/// A recurring entry (ADR 0011): the state a repeat keeps beside its occurrences, which are ordinary
/// expenses or incomes. It holds what no single entry can: the <b>day</b> a monthly repeat was last set
/// to, which no occurrence's date says after a short month (ruling 5, follow-up 3); the <b>next date</b>,
/// which outlives the removal of the latest occurrence (follow-up 1); and <b>which entries are its
/// occurrences</b>, so that an earlier one opens locked (follow-up 2).
///
/// <para><b>The latest occurrence is the one with the highest id</b>, and is not stored: the id is the
/// recording order, so the highest is the one recorded most recently, by definition (§12, the latest
/// is "the one recorded most recently"). When it is removed, the one before takes over by itself.</para>
///
/// <para><b>A stopped repeat is still one</b>: its frequency, day and next date are gone, but its
/// occurrences stay its own, so its earlier ones stay locked and its last one can start it again
/// (§12, ruled at the scenario stage, 1).</para>
/// </summary>
internal sealed class RecurringEntry
{
    private readonly List<int> occurrences = [];

    private RecurringEntry() { }

    /// <summary>A repeat set up on one entry, from that entry's date.</summary>
    public static RecurringEntry StartingFrom(IEntry entry, Frequency frequency)
    {
        var repeat = new RecurringEntry();
        repeat.occurrences.Add(entry.Id);
        repeat.SetFrom(entry.Date, frequency);
        return repeat;
    }

    /// <summary>A repeat as it was kept, already checked by the caller.</summary>
    public static RecurringEntry Kept(IEnumerable<int> occurrences, Frequency? frequency, int? day, DateOnly? next)
    {
        var repeat = new RecurringEntry { Frequency = frequency, Day = day, Next = next };
        repeat.occurrences.AddRange(occurrences.Order());
        return repeat;
    }

    /// <summary>The ids of its occurrences, in the order recorded.</summary>
    public IReadOnlyList<int> Occurrences => occurrences;

    /// <summary>The id of the occurrence that sets the next: the one recorded most recently.</summary>
    public int Latest => occurrences[^1];

    /// <summary>Null once stopped.</summary>
    public Frequency? Frequency { get; private set; }

    /// <summary>The day of the month a monthly repeat falls on, as last set. Null for weekly, and once stopped.</summary>
    public int? Day { get; private set; }

    /// <summary>The date the next occurrence falls on. Null once stopped.</summary>
    public DateOnly? Next { get; private set; }

    public bool IsRunning => Frequency is not null;

    public bool Has(int id) => occurrences.Contains(id);

    /// <summary>
    /// Sets how often it repeats, and from which date: the day becomes that date's day, and the next
    /// date is one step after it. Given null, stops it. This is the only thing that sets the day, so a
    /// date MoneyBud clamped never does (§12, follow-up 3 read with ruling 5).
    /// </summary>
    public void SetFrom(DateOnly date, Frequency? frequency)
    {
        Frequency = frequency;
        Day = frequency == Domain.Frequency.Monthly ? date.Day : null;
        Next = frequency is null ? null : After(date);
    }

    /// <summary>A new occurrence was recorded on <see cref="Next"/>: it joins, and the next date moves on a step.</summary>
    public void Recorded(IEntry occurrence)
    {
        occurrences.Add(occurrence.Id);
        Next = After(Next!.Value);
    }

    /// <summary>An occurrence was removed. The next date does not move (ruling 4). Returns whether any are left.</summary>
    public bool Removed(int id)
    {
        occurrences.Remove(id);
        return occurrences.Count > 0;
    }

    private DateOnly After(DateOnly date) => Frequency switch
    {
        Domain.Frequency.Weekly => date.AddDays(7),
        Domain.Frequency.Monthly => InTheMonthAfter(date, Day!.Value),
        _ => throw new InvalidOperationException("A stopped repeat has no next date."),
    };

    /// <summary>
    /// The day in the month after <paramref name="date"/>'s, or that month's last day when it is too
    /// short: the clamp a period start day uses (§12, ruling 5).
    /// </summary>
    internal static DateOnly InTheMonthAfter(DateOnly date, int day)
    {
        var month = new DateOnly(date.Year, date.Month, 1).AddMonths(1);
        return new DateOnly(month.Year, month.Month, Math.Min(day, DateTime.DaysInMonth(month.Year, month.Month)));
    }
}
