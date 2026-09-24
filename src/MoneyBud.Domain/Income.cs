namespace MoneyBud.Domain;

/// <summary>
/// A transaction that increases the total (arc42 §12).
///
/// <para><see cref="Amount"/> is a positive magnitude; that this is income rather than an expense
/// is what makes it an increase (ADR 0003, sign convention).</para>
///
/// <para><see cref="Label"/> is <b>required</b> and never empty, where an expense's is optional.
/// An income names no category, so the label is the only thing on the record that says what the
/// money is (arc42 §12, *Income carries a label, and it is required*). It arrives here already
/// trimmed; <see cref="Ledger.RecordIncome"/> is what refuses one that trims away to nothing.</para>
///
/// <para>Income has <b>no category</b>. It lands <i>Unassigned</i> and is given a purpose later by
/// assigning, which does not exist yet. It has no account either — the location dimension is not
/// built (arc42 §11), the same gap an expense has.</para>
/// </summary>
public sealed record Income(Money Amount, DateOnly Date, string Label);
