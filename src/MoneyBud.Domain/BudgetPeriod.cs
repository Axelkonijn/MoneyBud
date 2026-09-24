using System.Globalization;

namespace MoneyBud.Domain;

/// <summary>
/// The span a budget covers (arc42 §12). Normally a month, but the start day is configurable,
/// so a period is held as a pair of dates rather than as a month.
///
/// <para>A period <i>ends</i>; it is never <i>closed</i>. There is deliberately no state on this
/// type that could stop it accepting an expense — see §12, *Ending versus closing*.</para>
/// </summary>
public sealed record BudgetPeriod(DateOnly FirstDay, DateOnly LastDay)
{
    public bool Contains(DateOnly date) => date >= FirstDay && date <= LastDay;

    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{FirstDay:yyyy-MM-dd}..{LastDay:yyyy-MM-dd}");
}
