namespace MoneyBud.Domain;

/// <summary>
/// Turns dates into budget periods. Periods are one month long and start on a configurable day
/// of the month (arc42 §12, *Budget period*), so they need not line up with a calendar month.
///
/// <para>A start day later than a short month has — the 31st in February — <b>clamps to that
/// month's last day</b>. Decided by the stakeholder on 2026-09-24; see arc42 §12. It keeps
/// "configurable" true for every day of the month rather than true with an exception, and it is
/// what billing cycles conventionally do.</para>
///
/// <para>The cost was visible when the decision was taken and is not hidden here: configure the
/// 31st and the period containing 15 March 2026 runs 28 February to 30 March — starting on a day
/// the user did not pick, and 31 days long. What it buys is that periods always tile, with no
/// gaps and no overlaps, so an expense always falls in exactly one period.</para>
///
/// <para><b>The start day can be changed, from the current period on</b> (arc42 §12, <i>A
/// configurable period start day</i>; ADR 0012). Every earlier period keeps the boundaries it had,
/// so the calendar is a <b>history of changes</b>: a start day since always, and a list of
/// <see cref="StartDayChange"/>s. Each says which period was current when it was made, from when
/// the new day took effect, and the day. That period runs to the day before the new day took
/// effect, and from there periods start on the new day, until the next change. So periods still
/// tile, and a change never moves a period before the one it was made in. The calendar is
/// immutable: a change makes a new one (<see cref="ChangedFrom"/>).</para>
/// </summary>
public sealed class BudgetPeriodCalendar
{
    public const int DefaultStartDay = 1;

    private readonly int firstStartDay;

    public BudgetPeriodCalendar(int startDayOfMonth = DefaultStartDay)
        : this(startDayOfMonth, [])
    {
    }

    private BudgetPeriodCalendar(int firstStartDay, IReadOnlyList<StartDayChange> changes)
    {
        CheckDay(firstStartDay, nameof(firstStartDay));
        this.firstStartDay = firstStartDay;
        Changes = changes;
    }

    /// <summary>
    /// A calendar that started on the 1st and has been changed as <paramref name="changes"/> say, in
    /// the order they were made — as kept data holds it. Each is made again, so a list that no series
    /// of changes could have made <b>throws</b> <see cref="ArgumentException"/>: out of order, a
    /// period that was never current, a day outside 1 to 31 or the one already in force, or a day
    /// the new start day does not first come round on.
    /// </summary>
    public static BudgetPeriodCalendar FromChanges(IEnumerable<StartDayChange> changes)
    {
        var calendar = new BudgetPeriodCalendar();
        foreach (var change in changes)
        {
            CheckDay(change.StartDay, nameof(changes));
            if (calendar.Changes.Count > 0 && change.PeriodFrom < calendar.Changes[^1].From)
                throw new ArgumentException($"A change made in the period from {change.PeriodFrom} is out of order.", nameof(changes));

            var current = calendar.PeriodContaining(change.PeriodFrom);
            if (current.FirstDay != change.PeriodFrom)
                throw new ArgumentException($"No period starts on {change.PeriodFrom} for a change to be made in.", nameof(changes));

            var changed = calendar.ChangedFrom(current, change.StartDay);
            if (changed.Changes.Count != calendar.Changes.Count + 1 || changed.Changes[^1] != change)
                throw new ArgumentException($"A change to the {change.StartDay} made in the period from {change.PeriodFrom} is not one MoneyBud makes.", nameof(changes));

            calendar = changed;
        }

        return calendar;
    }

    /// <summary>Every change, in the order made, which is the order they take effect in. Empty for a calendar never changed.</summary>
    public IReadOnlyList<StartDayChange> Changes { get; }

    /// <summary>
    /// The day now set: the start day of the latest change, or the one since always. What the
    /// drop-down shows. A change can be set and still to take effect, when the current period was cut
    /// short but has not yet ended.
    /// </summary>
    public int StartDay => Changes.Count > 0 ? Changes[^1].StartDay : firstStartDay;

    public BudgetPeriod PeriodContaining(DateOnly date)
    {
        // The latest change made in a period that began on or before the date governs it: a date
        // before that period's first day is in a period the change left as it was.
        var index = Changes.Count - 1;
        while (index >= 0 && Changes[index].PeriodFrom > date) index--;

        if (index < 0) return Regular(firstStartDay, date);

        var change = Changes[index];
        return date < change.From
            ? new BudgetPeriod(change.PeriodFrom, change.From.AddDays(-1))
            : Regular(change.StartDay, date);
    }

    public BudgetPeriod Next(BudgetPeriod period) => PeriodContaining(period.LastDay.AddDays(1));

    public BudgetPeriod Previous(BudgetPeriod period) => PeriodContaining(period.FirstDay.AddDays(-1));

    /// <summary>
    /// This calendar with the start day changed to <paramref name="day"/> while
    /// <paramref name="current"/> is the current period (arc42 §12, ruling 1). The current period
    /// keeps its first day and ends the day before <paramref name="day"/> first comes round after it,
    /// clamped as ever; every period after starts on the new day, and every period before is as it
    /// was. A change still to take effect after the current period's first day — one made earlier the
    /// same period, which cut it short without ending it — is replaced. Choosing the day in force for
    /// the current period drops such a change and adds none.
    ///
    /// <para>The current period can end on the spot, when the new day has already come round in it,
    /// and can come out longer than it was when the new day comes round later than the old one would
    /// have: a period begun on a clamped 28 February under the 29th, changed to the 31st, runs to 30
    /// March. Both are ruling 1 as written, the second ruled at the build, 2026-09-29.</para>
    ///
    /// <para><b>Throws</b> for a period that is not one of this calendar's own.</para>
    /// </summary>
    public BudgetPeriodCalendar ChangedFrom(BudgetPeriod current, int day)
    {
        CheckDay(day, nameof(day));
        if (PeriodContaining(current.FirstDay) != current)
            throw new ArgumentException($"{current} is not a budget period.", nameof(current));

        var kept = Changes.Where(c => c.From <= current.FirstDay).ToList();
        var inForce = kept.Count > 0 ? kept[^1].StartDay : firstStartDay;
        if (day != inForce) kept.Add(new StartDayChange(current.FirstDay, FirstAfter(current.FirstDay, day), day));

        return new BudgetPeriodCalendar(firstStartDay, kept);
    }

    // The first day after the given one on which the day starts a period, clamped.
    private static DateOnly FirstAfter(DateOnly after, int day)
    {
        var thisMonth = StartIn(day, after.Year, after.Month);
        return thisMonth > after ? thisMonth : StartIn(day, MonthAfter(after));
    }

    // The period a date is in when every period starts on the day: the calendar without changes.
    private static BudgetPeriod Regular(int day, DateOnly date)
    {
        var startThisMonth = StartIn(day, date.Year, date.Month);
        var firstDay = date >= startThisMonth ? startThisMonth : StartIn(day, MonthBefore(date));
        return new BudgetPeriod(firstDay, StartIn(day, MonthAfter(firstDay)).AddDays(-1));
    }

    private static DateOnly StartIn(int day, (int Year, int Month) month) => StartIn(day, month.Year, month.Month);

    private static DateOnly StartIn(int day, int year, int month) =>
        new(year, month, Math.Min(day, DateTime.DaysInMonth(year, month)));

    private static (int Year, int Month) MonthBefore(DateOnly date) =>
        date.Month == 1 ? (date.Year - 1, 12) : (date.Year, date.Month - 1);

    private static (int Year, int Month) MonthAfter(DateOnly date) =>
        date.Month == 12 ? (date.Year + 1, 1) : (date.Year, date.Month + 1);

    private static void CheckDay(int day, string name)
    {
        if (day is < 1 or > 31)
            throw new ArgumentOutOfRangeException(name, day, "A period starts on a day of the month.");
    }
}

/// <summary>
/// A change of the period start day (arc42 §12, <i>A configurable period start day</i>): made while
/// the period from <see cref="PeriodFrom"/> was current, which it cut to end the day before
/// <see cref="From"/>; from <see cref="From"/> on, every period starts on <see cref="StartDay"/>,
/// clamped where a month is too short for it. <see cref="From"/> is the first day the new start day
/// came round after <see cref="PeriodFrom"/>.
///
/// <para>Both days are kept because <see cref="From"/> alone does not say which period was cut: under
/// the 29th, a change to the 31st made in the period from a clamped 28 February and one made in the
/// period from 29 March both take effect on 31 March.</para>
/// </summary>
public sealed record StartDayChange(DateOnly PeriodFrom, DateOnly From, int StartDay);
