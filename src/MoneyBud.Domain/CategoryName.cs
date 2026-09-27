namespace MoneyBud.Domain;

/// <summary>
/// The category name rule: the <see cref="NameRule"/>, which an account's name follows too. Kept
/// under this name because categories had the rule first, and everything that compares a category
/// name says so.
/// </summary>
public static class CategoryName
{
    /// <summary>Compares two names the way MoneyBud does wherever it compares names.</summary>
    public static IEqualityComparer<string> Comparer => NameRule.Comparer;

    /// <inheritdoc cref="NameRule.Normalise"/>
    public static string? Normalise(string? name) => NameRule.Normalise(name);
}
