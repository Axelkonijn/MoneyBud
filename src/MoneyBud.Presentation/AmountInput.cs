using System.Globalization;
using System.Text.RegularExpressions;

namespace MoneyBud.Presentation;

/// <summary>
/// Reads an amount as the user types it (arc42 §12, settled with the stakeholder 2026-09-25):
/// a comma or a point is the decimal mark, so "12,50" and "12.50" are both twelve fifty, and no
/// thousands separator is accepted.
///
/// <para>That makes "1.832" one euro and 832 thousandths. It is read that way, not guessed at, and
/// the domain then refuses it as finer than a cent — a refusal the user can see, where reading it
/// as 1832 would be a wrong amount nobody noticed. For the same reason nothing is rounded here:
/// the decimal passed on is exactly what was typed, and the cent rule stays the domain's.</para>
///
/// <para>A leading minus is accepted, because assigning a negative amount is how money goes back
/// to <i>Unassigned</i>. Whether a negative is allowed for an expense or an income is again the
/// domain's to say. A euro sign and spaces around the number are tolerated.</para>
/// </summary>
public static partial class AmountInput
{
    public static bool TryRead(string? typed, out decimal euros)
    {
        euros = 0;
        if (typed is null) return false;

        var match = Shape().Match(typed);
        if (!match.Success) return false;

        var number = match.Groups["whole"].Value;
        if (match.Groups["fraction"].Success) number += "." + match.Groups["fraction"].Value;

        if (!decimal.TryParse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out euros))
            return false;

        if (match.Groups["minus"].Success) euros = -euros;
        return true;
    }

    // Up to thirteen whole digits: more than any household needs, and far enough inside what a
    // Money's whole cents in a long can hold that no amount read here can overflow it. Up to ten
    // decimals, so that the number always fits a decimal exactly: a longer tail would be rounded
    // by the parse, and 1.00000000000000000000000000001 would quietly become 1.
    [GeneratedRegex(@"^\s*(?<minus>[-−])?\s*(?:€\s*)?(?<whole>\d{1,13})(?:[.,](?<fraction>\d{1,10}))?\s*$")]
    private static partial Regex Shape();
}
