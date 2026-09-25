using System.Text;

namespace MoneyBud.Domain;

/// <summary>
/// The category name rule (arc42 §12, *A category name is compared case-insensitively and stored
/// as typed, trimmed*). Storing and comparing are two different things, and this type keeps them
/// apart:
///
/// <list type="bullet">
/// <item><b>Stored</b> — <see cref="Normalise"/>: whitespace at either end is stripped and
/// discarded; everything inside is kept exactly as typed, capitalisation and spacing both.</item>
/// <item><b>Compared</b> — <see cref="Comparer"/>: the ends trimmed, any run of whitespace inside
/// the name counted as one space, and case ignored. "Vaste  lasten" is "vaste lasten".</item>
/// </list>
///
/// <para>Case is ignored ordinally, never by the machine's culture. A culture-sensitive
/// comparison would let the same two names be one category on one machine and two on another —
/// the Turkish dotless i is the classic case — and the rule exists precisely so that nobody ends
/// up with two categories they meant as one.</para>
///
/// <para>"Whitespace" means what <see cref="char.IsWhiteSpace(char)"/> means, not only the space
/// character: the same reading the label rule takes, where §12 also says whitespace.</para>
/// </summary>
public static class CategoryName
{
    /// <summary>Compares two names the way MoneyBud does wherever it compares names.</summary>
    public static IEqualityComparer<string> Comparer { get; } = new NameComparer();

    /// <summary>
    /// A name as it is stored: trimmed at the ends, left alone inside, and null when nothing
    /// survives — a name that trims to nothing is no name, which adding refuses and recording
    /// treats as naming no category.
    /// </summary>
    public static string? Normalise(string? name)
    {
        var trimmed = name?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    /// <summary>
    /// The form two names are compared in, before case is ignored: trimmed, with every run of
    /// inner whitespace reduced to a single space.
    /// </summary>
    private static string ComparisonForm(string name)
    {
        var builder = new StringBuilder(name.Length);
        var inWhitespace = false;

        foreach (var c in name.Trim())
        {
            if (char.IsWhiteSpace(c))
            {
                if (!inWhitespace) builder.Append(' ');
                inWhitespace = true;
            }
            else
            {
                builder.Append(c);
                inWhitespace = false;
            }
        }

        return builder.ToString();
    }

    private sealed class NameComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y) =>
            x is null || y is null
                ? x is null && y is null
                : StringComparer.OrdinalIgnoreCase.Equals(ComparisonForm(x), ComparisonForm(y));

        public int GetHashCode(string name) =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(ComparisonForm(name));
    }
}
