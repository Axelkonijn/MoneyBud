namespace MoneyBud.Domain;

/// <summary>
/// Why recording an income was refused.
///
/// <para>These are reasons, not messages. The wording the user sees belongs to whatever is
/// asking — there is no UI yet, and putting copy here would put it in the wrong place.</para>
///
/// <para><b>There is deliberately no "date in the future" reason, and its absence is the
/// decision rather than an omission.</b> An income may be dated forward, and it joins its
/// period's <i>Unassigned</i> from the moment it is recorded rather than from its date. That is
/// the one place income and expense differ about dates, and it follows from the plan/actual
/// split: a future expense is already expressible as a <i>Budget</i>, while there is no planned
/// income anywhere in the model, so future-dating is the only way to state an amount that is
/// coming (arc42 §12, *Income may be dated in the future; an expense may not*).</para>
/// </summary>
public enum IncomeRefusal
{
    /// <summary>
    /// No label was given, or one that is nothing but whitespace. The two are one reason
    /// because a label is trimmed before it is judged (arc42 §12, *A label is trimmed*).
    /// </summary>
    LabelMissing,

    /// <summary>The amount was zero or negative.</summary>
    AmountNotPositive,

    /// <summary>The amount was finer than a cent. Refused, never rounded (arc42 §8.2).</summary>
    AmountFinerThanCent,
}
