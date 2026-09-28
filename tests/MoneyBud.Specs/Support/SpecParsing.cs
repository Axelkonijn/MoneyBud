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

    /// <summary>The account a scenario names, which must exist (features/show-accounts.feature).</summary>
    public static Account Account(this Ledger ledger, string name) =>
        ledger.AccountNamed(name) ?? throw new ArgumentException($"There is no account \"{name}\".", nameof(name));

    /// <summary>An account named in a step, or null when the step named none.</summary>
    public static Account? AccountOrNull(this Ledger ledger, string? name) =>
        name is null ? null : ledger.Account(name);

    /// <summary>Resolves "current", "previous" or "next" against the ledger's own calendar.</summary>
    public static BudgetPeriod Period(this Ledger ledger, string which) => which switch
    {
        "current" => ledger.CurrentPeriod,
        "previous" => ledger.Calendar.Previous(ledger.CurrentPeriod),
        "next" => ledger.Calendar.Next(ledger.CurrentPeriod),
        _ => throw new ArgumentException($"Unknown budget period \"{which}\".", nameof(which)),
    };

    /// <summary>
    /// The period <paramref name="count"/> periods after the current one, or before it when
    /// negative — "the budget period 13 before the current one" (features/keep-data.feature).
    /// </summary>
    public static BudgetPeriod PeriodsFromCurrent(this Ledger ledger, int count)
    {
        var period = ledger.CurrentPeriod;
        for (var i = 0; i < Math.Abs(count); i++)
            period = count > 0 ? ledger.Calendar.Next(period) : ledger.Calendar.Previous(period);
        return period;
    }

    /// <summary>
    /// Resolves a period as a step names it in full: "the previous budget period" without its
    /// "the", or "budget period 2 before the current one" (features/take-over-a-plan.feature).
    /// </summary>
    public static BudgetPeriod PeriodNamed(this Ledger ledger, string phrase)
    {
        var match = NamedPeriod().Match(phrase.Trim());
        if (!match.Success)
            throw new ArgumentException($"Unknown budget period \"{phrase}\".", nameof(phrase));

        return match.Groups[1].Success
            ? ledger.Period(match.Groups[1].Value)
            : ledger.PeriodsFromCurrent(int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture)
                                        * (match.Groups[3].Value == "before" ? -1 : 1));
    }

    [GeneratedRegex(@"^(?:(current|previous|next) budget period|budget period (\d+) (before|after) the current one)$")]
    private static partial Regex NamedPeriod();

    /// <summary>
    /// Resolves a date as the scenarios phrase it: "today", "yesterday", "tomorrow", a day of a
    /// named budget period, or a calendar date, "25 August 2026" (repeat-an-entry.feature). A null
    /// phrase means the step named no date, which means today.
    /// </summary>
    public static DateOnly Date(this Ledger ledger, string? phrase)
    {
        if (phrase is null) return ledger.Today;
        if (CalendarDate(phrase) is { } day) return day;

        switch (phrase.Trim())
        {
            case "today":
                return ledger.Today;
            case "tomorrow":
                return ledger.Today.AddDays(1);
            case "yesterday":
                return ledger.Today.AddDays(-1);
        }

        var match = DayOfPeriod().Match(phrase.Trim());
        if (!match.Success)
            throw new ArgumentException($"Unknown date phrase \"{phrase}\".", nameof(phrase));

        var period = match.Groups[2].Success
            ? ledger.Period(match.Groups[2].Value)
            : ledger.PeriodsFromCurrent(int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture)
                                        * (match.Groups[4].Value == "before" ? -1 : 1));
        return match.Groups[1].Value == "first" ? period.FirstDay : period.LastDay;
    }

    /// <summary>
    /// A day inside a period that is safely in the past — the period's last day, unless today
    /// falls inside it, in which case today. Setup steps record real expenses through the real
    /// validation, so a setup date in the future would be refused.
    /// </summary>
    public static DateOnly ADayInside(this Ledger ledger, BudgetPeriod period) =>
        period.Contains(ledger.Today) ? ledger.Today : period.LastDay;

    /// <summary>A calendar date as the recurring files write it, "25 August 2026", or null for any other phrase.</summary>
    public static DateOnly? CalendarDate(string phrase) =>
        DateOnly.TryParseExact(phrase.Trim(), "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day)
            ? day
            : null;

    /// <summary>"one-off", "weekly" or "monthly": a frequency as the recurring files name it, one-off being none.</summary>
    public static Frequency? Frequency(string word) => word switch
    {
        "one-off" => null,
        "weekly" => Domain.Frequency.Weekly,
        "monthly" => Domain.Frequency.Monthly,
        _ => throw new ArgumentException($"Unknown frequency \"{word}\".", nameof(word)),
    };

    /// <summary>
    /// The ending a record step may carry, ", repeating monthly" or ", repeating weekly", or none:
    /// the Herhalen list set so, or left on one-off (repeat-an-entry.feature).
    /// </summary>
    public static Frequency? Repeating(string ending) =>
        ending.Length == 0 ? null : Frequency(ending.Replace(", repeating ", "", StringComparison.Ordinal));

    /// <summary>
    /// What a record step's pattern ends with to take that ending: a group that always matches,
    /// empty or not, since a group that may not match changes the step method's arity.
    /// </summary>
    public const string RepeatingEnding = @"((?:, repeating (?:weekly|monthly))?)";

    [GeneratedRegex(@"^the (first|last) day of the (?:(current|previous|next) budget period|budget period (\d+) (before|after) the current one)$")]
    private static partial Regex DayOfPeriod();
}
