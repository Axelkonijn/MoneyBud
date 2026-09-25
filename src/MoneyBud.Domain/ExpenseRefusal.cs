namespace MoneyBud.Domain;

/// <summary>
/// Why recording an expense was refused.
///
/// <para>These are reasons, not messages. The wording the user sees belongs to whatever is
/// asking — there is no UI yet, and putting copy here would put it in the wrong place.</para>
/// </summary>
public enum ExpenseRefusal
{
    /// <summary>
    /// No category was named — or the name trimmed to nothing, which is no name (arc42 §12). An
    /// expense must name one (§12, *Expense*).
    /// </summary>
    CategoryMissing,

    /// <summary>
    /// A category was named, but it is not one of the user's — neither in use nor archived. An
    /// archived category's name is not refused: recording against it brings it back.
    /// </summary>
    UnknownCategory,

    /// <summary>The amount was zero or negative.</summary>
    AmountNotPositive,

    /// <summary>The amount was finer than a cent. Refused, never rounded (arc42 §8.2).</summary>
    AmountFinerThanCent,

    /// <summary>The date was in the future. An expense records money already spent.</summary>
    DateInFuture,
}
