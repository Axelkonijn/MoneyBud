namespace MoneyBud.Specs.Support;

/// <summary>
/// A clock that does not move, so that "today", "tomorrow" and every period-relative date in the
/// scenarios mean the same thing on every run and on every machine.
///
/// <para>The time zone is fixed to UTC as well. The domain asks for local time, and leaving that
/// to the machine would make the suite depend on where it is run — which is the shape of bug
/// arc42 §8.2's open "period boundaries and timezones" question is about.</para>
/// </summary>
internal sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
