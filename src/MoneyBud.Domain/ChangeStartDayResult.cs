namespace MoneyBud.Domain;

/// <summary>
/// What changing the period start day does, or did (arc42 §12, <i>A configurable period start
/// day</i>): the day, the current period under the calendar as changed, and the period the change
/// ended on the spot, if it ended one. Never refused: the day is picked from a list of 1 to 31, and
/// every day in it is allowed.
/// </summary>
/// <param name="Day">The start day chosen.</param>
/// <param name="WasChanged">False when the day chosen was already set, which changes nothing and is
/// said by nobody: the drop-down writes back what it shows on every redraw.</param>
/// <param name="Current">The current period once the change is made. It keeps the old current
/// period's first day unless the change ended that period.</param>
/// <param name="Ended">The period the change ended on the spot — the old current period, cut short
/// — or null when the current period goes on, perhaps cut short to end on a later day.</param>
public sealed record ChangeStartDayResult(int Day, bool WasChanged, BudgetPeriod Current, BudgetPeriod? Ended);
