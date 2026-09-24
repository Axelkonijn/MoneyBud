namespace MoneyBud.Domain;

/// <summary>
/// What money is earmarked for (arc42 §12). A label, which exists independently of any amount
/// assigned to it — a category with no budget is a category, not an absence.
///
/// <para>In this increment a category is only its name. Backing accounts and a default backing
/// account (§12, *Account-backed categories*) arrive with the location dimension, which is not
/// in the first increment.</para>
/// </summary>
public sealed record Category(string Name);
