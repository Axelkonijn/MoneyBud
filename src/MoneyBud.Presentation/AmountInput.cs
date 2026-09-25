using System.Globalization;
using System.Text.RegularExpressions;

namespace MoneyBud.Presentation;

/// <summary>What came of reading typed text as an amount.</summary>
public enum AmountReading
{
    Read,
    NotAnAmount,

    /// <summary>
    /// A point or comma followed by exactly three digits ending in 0, such as "2.000": it could be
    /// two thousand written the Dutch way, or two euros with a stray zero, and both readings are
    /// whole cents, so nothing further down would catch the wrong one.
    /// </summary>
    Ambiguous,
}

/// <summary>
/// Reads an amount as the user types it (arc42 §12, settled with the stakeholder 2026-09-25):
/// a comma or a point is the decimal mark, so "12,50" and "12.50" are both twelve fifty, and no
/// thousands separator is accepted.
///
/// <para>A mark followed by exactly three digits is the one place those rules can mislead.
/// "1.832" is read as one euro and 832 thousandths, which the domain refuses as finer than a
/// cent — a refusal the user sees. But "2.000" would be read as two euros, which <i>is</i> whole
/// cents and would be recorded, while the user very likely meant two thousand: MoneyBud itself
/// shows thousands with a point. So when three digits end in 0 the text is refused as ambiguous
/// rather than guessed at (settled with the stakeholder after review, 2026-09-25).</para>
///
/// <para>Four or more decimals ending in nothing but zeros ("2.0000") are not an amount, for the
/// same reason one step removed; and a mark needs a digit before it (",50" is not an amount).</para>
///
/// <para>Nothing is rounded here: the decimal passed on is exactly what was typed, and the cent
/// rule stays the domain's. A leading minus is accepted, because assigning a negative amount is
/// how money goes back to <i>Unassigned</i>; whether a negative is allowed elsewhere is again the
/// domain's to say. A euro sign and spaces around the number are tolerated.</para>
/// </summary>
public static partial class AmountInput
{
    public static bool TryRead(string? typed, out decimal euros) => Read(typed, out euros) == AmountReading.Read;

    public static AmountReading Read(string? typed, out decimal euros)
    {
        euros = 0;
        if (typed is null) return AmountReading.NotAnAmount;

        var match = Shape().Match(typed);
        if (!match.Success) return AmountReading.NotAnAmount;

        var whole = match.Groups["whole"].Value;
        var fraction = match.Groups["fraction"];

        if (fraction.Success && fraction.Value.Length == 3 && fraction.Value[^1] == '0')
            return AmountReading.Ambiguous;

        // Four or more decimals that are still whole cents, such as "2.0000": the tail is all
        // zeros, so the domain would take it as 2,00 without a word. MoneyBud never shows more
        // than two decimals, so this is not how anyone writes an amount (stakeholder, 2026-09-26).
        // A tail that is not all zeros is left to the cent rule, as "1.832" is.
        if (fraction.Success && fraction.Value.Length > 3 && fraction.Value[2..].All(d => d == '0'))
            return AmountReading.NotAnAmount;

        var number = fraction.Success ? $"{whole}.{fraction.Value}" : whole;
        if (!decimal.TryParse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out euros))
            return AmountReading.NotAnAmount;

        if (match.Groups["minus"].Success) euros = -euros;
        return AmountReading.Read;
    }

    /// <summary>For an ambiguous amount, the two things it could have meant, as MoneyBud would show them.</summary>
    public static (string AsThousands, string AsDecimal) Readings(string typed)
    {
        var match = Shape().Match(typed);
        var whole = match.Groups["whole"].Value;
        var fraction = match.Groups["fraction"].Value;
        var minus = match.Groups["minus"].Success ? "-" : "";

        var thousands = (whole + fraction).TrimStart('0');

        return ($"{minus}{(thousands.Length > 0 ? thousands : "0")}", $"{minus}{whole},{fraction[..2]}");
    }

    // Up to thirteen whole digits: more than any household needs, and far enough inside what a
    // Money's whole cents in a long can hold that no amount read here can overflow it. Up to ten
    // decimals, so that the number always fits a decimal exactly: a longer tail would be rounded
    // by the parse, and 1.00000000000000000000000000001 would quietly become 1.
    [GeneratedRegex(@"^\s*(?<minus>[-−])?\s*(?:€\s*)?(?<whole>\d{1,13})(?:[.,](?<fraction>\d{1,10}))?\s*$")]
    private static partial Regex Shape();
}
