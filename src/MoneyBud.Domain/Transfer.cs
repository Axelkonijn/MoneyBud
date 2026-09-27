namespace MoneyBud.Domain;

/// <summary>
/// Money moved from one account to another by the user: an ATM withdrawal, a top-up of savings
/// (arc42 §12, <i>Transfer</i>; on screen <i>Overboeking</i>).
///
/// <para>It is <b>not income and not an expense</b>. It moves two balances and no budget figure: it
/// names no category, is in no period's <i>Unassigned</i>, and has no label. Net worth is unchanged
/// by it, except where a balance correction on one side already has it in and the other side does
/// not — which is then the true figure (§12).</para>
///
/// <para><see cref="Amount"/> is a positive magnitude; the direction is <see cref="From"/> to
/// <see cref="To"/>, which are always two different accounts. It may not be dated in the future:
/// it reports money that has moved.</para>
///
/// <para>Backing makes moves of the same shape on MoneyBud's own initiative, as a
/// <see cref="Movement"/>, and the sweep will too. This is the user making one.</para>
/// </summary>
public sealed record Transfer(int Id, Money Amount, DateOnly Date, Account From, Account To) : IEntry;
