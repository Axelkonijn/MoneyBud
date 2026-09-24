using System.Globalization;
using MoneyBud.Domain;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for the calendar.
///
/// <para>Most of these assert the property that matters whatever the start day is: periods tile
/// time. No day belongs to two periods, and no day belongs to none — otherwise an expense could
/// count twice or vanish.</para>
///
/// <para>The last test is different in kind. It pins the short-month rule — a start day later
/// than the month has clamps to that month's last day — which is a decision the stakeholder took
/// on 2026-09-24 with its odd-looking consequence visible (arc42 §12). It is asserted here
/// because it is now a rule; before it was decided, a test would have quietly turned an
/// unanswered question into an answer.</para>
/// </summary>
public class BudgetPeriodCalendarTests
{
    public static TheoryData<int> EveryStartDay
    {
        get
        {
            var days = new TheoryData<int>();
            for (var day = 1; day <= 31; day++) days.Add(day);
            return days;
        }
    }

    // Invariant, not the machine's culture: the dates above are written ISO and must read that
    // way wherever the suite runs.
    private static DateOnly Day(string date) =>
        DateOnly.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    [Theory]
    [MemberData(nameof(EveryStartDay))]
    public void Periods_run_back_to_back_without_gaps_or_overlaps(int startDay)
    {
        var calendar = new BudgetPeriodCalendar(startDay);
        var period = calendar.PeriodContaining(new DateOnly(2026, 1, 15));

        for (var i = 0; i < 24; i++)
        {
            var next = calendar.Next(period);

            Assert.True(
                period.LastDay < next.FirstDay,
                $"Period {period} overlaps {next} with start day {startDay}.");
            Assert.Equal(period.LastDay.AddDays(1), next.FirstDay);

            period = next;
        }
    }

    [Theory]
    [MemberData(nameof(EveryStartDay))]
    public void Every_day_of_a_period_resolves_back_to_it(int startDay)
    {
        var calendar = new BudgetPeriodCalendar(startDay);
        var period = calendar.PeriodContaining(new DateOnly(2026, 1, 15));

        for (var i = 0; i < 24; i++)
        {
            for (var day = period.FirstDay; day <= period.LastDay; day = day.AddDays(1))
                Assert.Equal(period, calendar.PeriodContaining(day));

            period = calendar.Next(period);
        }
    }

    [Theory]
    [MemberData(nameof(EveryStartDay))]
    public void A_period_is_one_month_long(int startDay)
    {
        var calendar = new BudgetPeriodCalendar(startDay);
        var period = calendar.PeriodContaining(new DateOnly(2026, 1, 15));

        for (var i = 0; i < 24; i++)
        {
            var next = calendar.Next(period);

            Assert.Equal(period.FirstDay.AddMonths(1).Month, next.FirstDay.Month);

            period = next;
        }
    }

    [Theory]
    [MemberData(nameof(EveryStartDay))]
    public void Previous_and_next_are_each_other(int startDay)
    {
        var calendar = new BudgetPeriodCalendar(startDay);
        var period = calendar.PeriodContaining(new DateOnly(2026, 6, 10));

        Assert.Equal(period, calendar.Previous(calendar.Next(period)));
        Assert.Equal(period, calendar.Next(calendar.Previous(period)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    [InlineData(-1)]
    public void A_start_day_that_is_not_a_day_of_the_month_is_refused(int startDay) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new BudgetPeriodCalendar(startDay));

    [Theory]
    // Start day 31, a date in February: the period began on 31 January, and ends the day before
    // February's own start — which clamped to the 28th.
    [InlineData(31, "2026-02-15", "2026-01-31", "2026-02-27")]
    // The consequence the stakeholder accepted: a period that starts on a day nobody chose and
    // runs 31 days.
    [InlineData(31, "2026-03-15", "2026-02-28", "2026-03-30")]
    [InlineData(30, "2026-02-10", "2026-01-30", "2026-02-27")]
    // A leap year clamps to the 29th, not the 28th.
    [InlineData(31, "2028-02-15", "2028-01-31", "2028-02-28")]
    public void A_start_day_the_month_is_too_short_for_clamps_to_its_last_day(
        int startDay, string date, string expectedFirstDay, string expectedLastDay)
    {
        var period = new BudgetPeriodCalendar(startDay).PeriodContaining(Day(date));

        Assert.Equal(Day(expectedFirstDay), period.FirstDay);
        Assert.Equal(Day(expectedLastDay), period.LastDay);
    }
}
