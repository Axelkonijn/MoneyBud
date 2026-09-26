namespace MoneyBud.Domain;

/// <summary>
/// What money is earmarked for (arc42 §12). A label, which exists independently of any amount
/// assigned to it — a category with no budget is a category, not an absence.
///
/// <para><see cref="Name"/> is the name as stored: trimmed, otherwise exactly as the user typed
/// it when adding or last renaming it. Two names that differ only in case or spacing are one
/// category, but that is decided by <see cref="CategoryName.Comparer"/>, never by comparing this
/// string.</para>
///
/// <para><b>A category is one thing with a name, not a name.</b> It has identity: the
/// <see cref="Ledger"/> makes exactly one instance per category and hands out that instance
/// everywhere, so its budgets, its expenses and whether it is archived all hang off the instance
/// rather than off the name. That is what lets a rename be one assignment — every period, past
/// ones included, then shows the new name, and nothing has to remember the old one (arc42 §12,
/// *Renaming a category*). Only the ledger can set the name, because only the ledger can keep
/// its index of names in step with it.</para>
///
/// <para>Whether a category is <i>archived</i> is not on this type. It is a fact about the
/// ledger's use of the category rather than about the category, and an expense recorded against
/// it last year should not change because the category was put away today.</para>
///
/// <para>In this increment a category is only its name. Backing accounts and a default backing
/// account (§12, *Account-backed categories*) arrive with the location dimension.</para>
/// </summary>
public sealed class Category
{
    internal Category(string name) => Name = name;

    public string Name { get; internal set; }

    public override string ToString() => Name;
}
