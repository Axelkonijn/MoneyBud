namespace MoneyBud.Domain;

/// <summary>
/// Why an assignment was refused.
///
/// <para>These are reasons, not messages — the wording belongs to a UI. When one assignment
/// breaks several rules, the reason given is the first of these in declaration order: the
/// category, then the amount, then the period. That is the order recording an expense uses
/// (arc42 §12, *When an assignment is refused*).</para>
///
/// <para>Every reason here is about the <i>target</i>, apart from the cent rule. Nothing refuses
/// an amount for being zero or negative: zero is accepted and moves nothing, and a negative
/// amount moves money back, clipped at what the <i>Budget</i> holds. Those allowances do not
/// rescue an assignment whose target is wrong.</para>
/// </summary>
public enum AssignRefusal
{
    /// <summary>The name trimmed to nothing, which is no name (arc42 §12).</summary>
    CategoryMissing,

    /// <summary>
    /// A category was named, but it is not one of the user's — neither in use nor archived.
    /// Assigning never adds a category.
    /// </summary>
    UnknownCategory,

    /// <summary>The amount was finer than a cent. Refused, never rounded (arc42 §8.2).</summary>
    AmountFinerThanCent,

    /// <summary>
    /// The period has ended. Only the current period and later ones can be planned; a past period
    /// is a record of what happened, including the plan the user actually had (arc42 §12,
    /// *Assigning happens in the current budget period and later ones*).
    /// </summary>
    PeriodInPast,
}
