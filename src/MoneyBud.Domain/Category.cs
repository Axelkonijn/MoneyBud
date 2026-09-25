namespace MoneyBud.Domain;

/// <summary>
/// What money is earmarked for (arc42 §12). A label, which exists independently of any amount
/// assigned to it — a category with no budget is a category, not an absence.
///
/// <para><see cref="Name"/> is the name as stored: trimmed, otherwise exactly as the user first
/// typed it. Two names that differ only in case or spacing are one category, but that is
/// decided by <see cref="CategoryName.Comparer"/>, never by comparing this string — the
/// <see cref="Ledger"/> only ever holds one category per name under that rule, so two instances
/// with equal names are the same category.</para>
///
/// <para>Whether a category is <i>archived</i> is not on this type. It is a fact about the
/// ledger's use of the category rather than about the category, and an expense recorded against
/// it last year should not change because the category was put away today.</para>
///
/// <para>In this increment a category is only its name. Backing accounts and a default backing
/// account (§12, *Account-backed categories*) arrive with the location dimension.</para>
/// </summary>
public sealed record Category(string Name);
