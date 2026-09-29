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

    // ------------------------------------------------------------------ a changed start day (ADR 0012)

    public static TheoryData<int, int> EveryChange
    {
        get
        {
            var changes = new TheoryData<int, int>();
            foreach (var from in new[] { 1, 15, 27, 29, 30, 31 })
                for (var to = 1; to <= 31; to++)
                    if (to != from) changes.Add(from, to);
            return changes;
        }
    }

    // Ruling 1, for a start day changed on every day of a year of periods: every period before the
    // current one is as it was, the current one keeps its first day and ends the day before the new
    // day first comes round after it, and from there periods tile on the new day.
    [Theory]
    [MemberData(nameof(EveryChange))]
    public void A_change_applies_from_the_current_period_on_and_periods_still_tile(int from, int to)
    {
        var before = new BudgetPeriodCalendar(from);

        for (var today = new DateOnly(2027, 1, 1); today < new DateOnly(2028, 1, 1); today = today.AddDays(3))
        {
            var current = before.PeriodContaining(today);
            var after = before.ChangedFrom(current, to);

            var earlier = before.Previous(current);
            for (var i = 0; i < 3; i++, earlier = before.Previous(earlier))
                Assert.Equal(earlier, after.PeriodContaining(earlier.FirstDay));

            var cut = after.PeriodContaining(current.FirstDay);
            Assert.Equal(current.FirstDay, cut.FirstDay);
            var newDay = cut.LastDay.AddDays(1);
            Assert.Equal(FirstAfter(current.FirstDay, to), newDay);

            var period = cut;
            for (var i = 0; i < 14; i++)
            {
                for (var day = period.FirstDay; day <= period.LastDay; day = day.AddDays(1))
                    Assert.Equal(period, after.PeriodContaining(day));
                var next = after.Next(period);
                Assert.Equal(period.LastDay.AddDays(1), next.FirstDay);
                Assert.Equal(period, after.Previous(next));
                if (i > 0) Assert.Equal(new BudgetPeriodCalendar(to).PeriodContaining(period.FirstDay), period);
                period = next;
            }
        }
    }

    // The first day, strictly after the one given, that the day starts a period on, clamped.
    private static DateOnly FirstAfter(DateOnly after, int day)
    {
        for (var d = after.AddDays(1); ; d = d.AddDays(1))
            if (d.Day == Math.Min(day, DateTime.DaysInMonth(d.Year, d.Month))) return d;
    }

    // The worked example. On 29 September the 27th has already come round, so September ends there.
    [Fact]
    public void Changing_to_the_27th_on_29_September_cuts_September_short_at_the_26th()
    {
        var calendar = new BudgetPeriodCalendar().ChangedFrom(new(Day("2026-09-01"), Day("2026-09-30")), 27);

        Assert.Equal(new BudgetPeriod(Day("2026-08-01"), Day("2026-08-31")), calendar.PeriodContaining(Day("2026-08-15")));
        Assert.Equal(new BudgetPeriod(Day("2026-09-01"), Day("2026-09-26")), calendar.PeriodContaining(Day("2026-09-26")));
        Assert.Equal(new BudgetPeriod(Day("2026-09-27"), Day("2026-10-26")), calendar.PeriodContaining(Day("2026-09-29")));
        Assert.Equal(27, calendar.StartDay);
        Assert.Equal([new StartDayChange(Day("2026-09-01"), Day("2026-09-27"), 27)], calendar.Changes);
    }

    // A change still to take effect — September cut short by the 30th, not yet ended — is replaced by
    // a second change made while the same period is current, and choosing the day in force for that
    // period again drops it, leaving the calendar as it was.
    [Fact]
    public void A_second_change_in_the_same_period_replaces_one_still_to_take_effect()
    {
        var september = new BudgetPeriod(Day("2026-09-01"), Day("2026-09-30"));
        var to30 = new BudgetPeriodCalendar().ChangedFrom(september, 30);
        var cut = to30.PeriodContaining(Day("2026-09-29"));
        Assert.Equal(new BudgetPeriod(Day("2026-09-01"), Day("2026-09-29")), cut);

        var to27 = to30.ChangedFrom(cut, 27);
        Assert.Equal([new StartDayChange(Day("2026-09-01"), Day("2026-09-27"), 27)], to27.Changes);

        var back = to30.ChangedFrom(cut, 1);
        Assert.Empty(back.Changes);
        Assert.Equal(1, back.StartDay);
        Assert.Equal(september, back.PeriodContaining(Day("2026-09-29")));
    }

    // Ruled at the build, 2026-09-29: where the new day's clamp in the current period's month is that
    // period's own first day, the next occurrence is a month on, and the period comes out longer.
    [Fact]
    public void A_period_begun_on_a_clamped_day_changed_to_a_later_day_runs_to_the_new_days_next_occurrence()
    {
        var on29th = new BudgetPeriodCalendar(29);
        var current = on29th.PeriodContaining(Day("2027-03-10"));
        Assert.Equal(new BudgetPeriod(Day("2027-02-28"), Day("2027-03-28")), current);

        var changed = on29th.ChangedFrom(current, 31);

        Assert.Equal(new BudgetPeriod(Day("2027-02-28"), Day("2027-03-30")), changed.PeriodContaining(Day("2027-03-10")));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void A_change_to_a_day_that_is_not_a_day_of_the_month_is_refused(int day) =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BudgetPeriodCalendar().ChangedFrom(new(Day("2026-09-01"), Day("2026-09-30")), day));

    [Fact]
    public void A_change_from_a_span_that_is_not_one_of_the_calendars_periods_is_refused() =>
        Assert.Throws<ArgumentException>(() =>
            new BudgetPeriodCalendar().ChangedFrom(new(Day("2026-09-05"), Day("2026-10-04")), 27));

    // What kept data may hold: only a history some series of changes could have made.
    [Fact]
    public void A_history_is_rebuilt_from_its_changes_and_one_no_change_could_make_is_refused()
    {
        var made = new BudgetPeriodCalendar().ChangedFrom(new(Day("2026-09-01"), Day("2026-09-30")), 27);
        var rebuilt = BudgetPeriodCalendar.FromChanges(made.Changes);
        Assert.Equal(made.PeriodContaining(Day("2026-09-10")), rebuilt.PeriodContaining(Day("2026-09-10")));
        Assert.Equal(made.PeriodContaining(Day("2026-10-10")), rebuilt.PeriodContaining(Day("2026-10-10")));

        var september = Day("2026-09-01");
        Assert.Throws<ArgumentException>(() => BudgetPeriodCalendar.FromChanges([new(september, Day("2026-09-20"), 27)]));
        Assert.Throws<ArgumentException>(() => BudgetPeriodCalendar.FromChanges([new(september, Day("2026-10-01"), 1)]));
        Assert.Throws<ArgumentException>(() => BudgetPeriodCalendar.FromChanges([new(Day("2026-09-02"), Day("2026-09-27"), 27)]));
        Assert.Throws<ArgumentException>(() => BudgetPeriodCalendar.FromChanges(
            [new(september, Day("2026-09-27"), 27), new(september, Day("2026-10-01"), 1)]));
    }

    // The two changes a single From cannot tell apart (StartDayChange): made from 28 February and from
    // 29 March under the 29th, both to the 31st, both taking effect on 31 March.
    [Fact]
    public void Two_changes_taking_effect_on_the_same_day_from_different_periods_give_different_periods()
    {
        var on29th = new BudgetPeriodCalendar(29);
        var fromFebruary = on29th.ChangedFrom(on29th.PeriodContaining(Day("2027-03-10")), 31);
        var fromMarch = on29th.ChangedFrom(on29th.PeriodContaining(Day("2027-03-30")), 31);

        Assert.Equal(fromFebruary.Changes[0].From, fromMarch.Changes[0].From);
        Assert.Equal(new BudgetPeriod(Day("2027-02-28"), Day("2027-03-30")), fromFebruary.PeriodContaining(Day("2027-03-29")));
        Assert.Equal(new BudgetPeriod(Day("2027-03-29"), Day("2027-03-30")), fromMarch.PeriodContaining(Day("2027-03-29")));
    }
}
