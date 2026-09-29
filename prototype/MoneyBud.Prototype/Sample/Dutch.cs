using System.Globalization;

namespace MoneyBud.Prototype.Sample;

/// <summary>
/// Dutch amounts and dates, spelled out by hand so the phone's own language settings cannot change
/// them. Amounts are <see cref="decimal"/>: never a floating point for money, even in a prototype.
/// </summary>
public static class Dutch
{
    private static readonly NumberFormatInfo Numbers = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NumberDecimalDigits = 2,
    };

    private static readonly string[] Months =
        ["januari", "februari", "maart", "april", "mei", "juni", "juli", "augustus", "september", "oktober", "november", "december"];

    private static readonly string[] ShortMonths =
        ["jan", "feb", "mrt", "apr", "mei", "jun", "jul", "aug", "sep", "okt", "nov", "dec"];

    private static readonly string[] Days = ["zo", "ma", "di", "wo", "do", "vr", "za"];

    public static string Euro(decimal amount) =>
        amount < 0 ? "€ -" + (-amount).ToString("N2", Numbers) : "€ " + amount.ToString("N2", Numbers);

    /// <summary>With a sign, for an account's history: "+ € 2.450,00", "- € 64,20".</summary>
    public static string Signed(decimal amount) =>
        amount < 0 ? "- " + Euro(-amount) : "+ " + Euro(amount);

    public static string Month(DateOnly date) => $"{Months[date.Month - 1]} {date.Year}";

    public static string MonthOnly(DateOnly date) => Months[date.Month - 1];

    public static string Day(DateOnly date) => $"{date.Day} {ShortMonths[date.Month - 1]}";

    /// <summary>A heading for a day in a list: "vandaag", "gisteren", or "zo 27 sep".</summary>
    public static string DayHeading(DateOnly date, DateOnly today) =>
        date == today ? "Vandaag"
        : date == today.AddDays(-1) ? "Gisteren"
        : date > today ? $"{Days[(int)date.DayOfWeek]} {Day(date)} · gepland"
        : $"{Days[(int)date.DayOfWeek]} {Day(date)}";

    /// <summary>
    /// Reads a typed amount the lenient way the prototype needs: a comma or a point as the decimal
    /// mark, no thousands separator, at most two decimals. The real rules are MoneyBud's own.
    /// </summary>
    public static bool TryReadAmount(string? text, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normal = text.Trim().Replace("€", "").Trim().Replace(',', '.');
        if (!decimal.TryParse(normal, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out amount))
        {
            return false;
        }

        return decimal.Round(amount, 2) == amount;
    }

    public static string Typed(decimal amount) => amount.ToString("0.00", Numbers);
}
