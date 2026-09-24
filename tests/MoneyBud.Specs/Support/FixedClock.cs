namespace MoneyBud.Specs.Support;

/// <summary>
/// A clock that moves only when a scenario tells it to, so that "today", "tomorrow" and every
/// period-relative date in the scenarios mean the same thing on every run and on every machine.
///
/// <para>Most scenarios never move it. The one that does is the future-dating scenario in
/// <c>record-income.feature</c>, which has to fix today at the first day of a period in order for
/// "the last day of the current budget period" to be unambiguously still to come. Moving it is
/// enough on its own: <see cref="MoneyBud.Domain.Ledger.Today"/> reads the clock on every call,
/// so nothing has to be rebuilt around it.</para>
///
/// <para>The time zone is fixed to UTC as well. The domain asks for local time, and leaving that
/// to the machine would make the suite depend on where it is run — which is the shape of bug
/// arc42 §8.2's open "period boundaries and timezones" question is about.</para>
/// </summary>
internal sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
