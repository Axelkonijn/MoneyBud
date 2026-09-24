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
/// </summary>
public sealed class BudgetPeriodCalendar
{
    public const int DefaultStartDay = 1;

    private readonly int startDay;

    public BudgetPeriodCalendar(int startDayOfMonth = DefaultStartDay)
    {
        if (startDayOfMonth is < 1 or > 31)
            throw new ArgumentOutOfRangeException(
                nameof(startDayOfMonth), startDayOfMonth, "A period starts on a day of the month.");

        startDay = startDayOfMonth;
    }

    public BudgetPeriod PeriodContaining(DateOnly date)
    {
        var startThisMonth = StartIn(date.Year, date.Month);
        var firstDay = date >= startThisMonth ? startThisMonth : StartIn(MonthBefore(date));
        return new BudgetPeriod(firstDay, StartIn(MonthAfter(firstDay)).AddDays(-1));
    }

    public BudgetPeriod Next(BudgetPeriod period) => PeriodContaining(period.LastDay.AddDays(1));

    public BudgetPeriod Previous(BudgetPeriod period) => PeriodContaining(period.FirstDay.AddDays(-1));

    private DateOnly StartIn((int Year, int Month) month) => StartIn(month.Year, month.Month);

    private DateOnly StartIn(int year, int month) =>
        new(year, month, Math.Min(startDay, DateTime.DaysInMonth(year, month)));

    private static (int Year, int Month) MonthBefore(DateOnly date) =>
        date.Month == 1 ? (date.Year - 1, 12) : (date.Year, date.Month - 1);

    private static (int Year, int Month) MonthAfter(DateOnly date) =>
        date.Month == 12 ? (date.Year + 1, 1) : (date.Year, date.Month + 1);
}
