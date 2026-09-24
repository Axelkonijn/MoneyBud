using System.Globalization;
using System.Text.RegularExpressions;
using MoneyBud.Domain;

namespace MoneyBud.Specs.Support;

/// <summary>
/// Turns the words the feature files use into values the domain understands.
/// </summary>
internal static partial class SpecParsing
{
    /// <summary>
    /// Reads an amount as written in a feature file.
    ///
    /// <para>Always invariant culture, never the machine's. The feature files write 3.50 and
    /// -0.01 with a dot, and on a Dutch machine the ambient culture reads that dot as a group
    /// separator — which would pass locally and fail elsewhere, or worse, silently mean
    /// something else.</para>
    ///
    /// <para>Returns a <see cref="decimal"/> rather than a <see cref="Money"/> because some of
    /// these amounts are not valid money at all: 12.345 and -0.01 exist in the scenarios
    /// precisely to be refused.</para>
    /// </summary>
    public static decimal Amount(string text) =>
        decimal.Parse(text, NumberStyles.Number, CultureInfo.InvariantCulture);

    public static Money MoneyAmount(string text) => Money.FromEuros(Amount(text));

    /// <summary>Resolves "current", "previous" or "next" against the ledger's own calendar.</summary>
    public static BudgetPeriod Period(this Ledger ledger, string which) => which switch
    {
        "current" => ledger.CurrentPeriod,
        "previous" => ledger.Calendar.Previous(ledger.CurrentPeriod),
        "next" => ledger.Calendar.Next(ledger.CurrentPeriod),
        _ => throw new ArgumentException($"Unknown budget period \"{which}\".", nameof(which)),
    };

    /// <summary>
    /// Resolves a date as the scenarios phrase it: "today", "tomorrow", or a day of a named
    /// budget period. A null phrase means the step named no date, which means today.
    /// </summary>
    public static DateOnly Date(this Ledger ledger, string? phrase)
    {
        if (phrase is null) return ledger.Today;

        switch (phrase.Trim())
        {
            case "today":
                return ledger.Today;
            case "tomorrow":
                return ledger.Today.AddDays(1);
        }

        var match = DayOfPeriod().Match(phrase.Trim());
        if (!match.Success)
            throw new ArgumentException($"Unknown date phrase \"{phrase}\".", nameof(phrase));

        var period = ledger.Period(match.Groups[2].Value);
        return match.Groups[1].Value == "first" ? period.FirstDay : period.LastDay;
    }

    /// <summary>
    /// A day inside a period that is safely in the past — the period's last day, unless today
    /// falls inside it, in which case today. Setup steps record real expenses through the real
    /// validation, so a setup date in the future would be refused.
    /// </summary>
    public static DateOnly ADayInside(this Ledger ledger, BudgetPeriod period) =>
        period.Contains(ledger.Today) ? ledger.Today : period.LastDay;

    [GeneratedRegex(@"^the (first|last) day of the (current|previous|next) budget period$")]
    private static partial Regex DayOfPeriod();
}
