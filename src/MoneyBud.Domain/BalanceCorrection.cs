namespace MoneyBud.Domain;

/// <summary>
/// A balance the user typed for an account: what the bank said it held on <see cref="Date"/>
/// (arc42 §12, <i>Balance correction</i>; on screen <i>Correctie</i>). An account's
/// <b>starting balance</b> is the first of these, typed when the account was added, and is marked
/// by <see cref="IsStartingBalance"/>.
///
/// <para><b>It is a statement, not an adjustment.</b> It already has in it every entry on the
/// account dated before its day, and every entry dated on its day that was recorded before it, so
/// a receipt entered late does not knock a checked balance off again (§12, <i>A typed balance is
/// what the bank said that day</i>). Entries after it move the balance as normal. What it holds is
/// the balance itself; how far it was from what MoneyBud had worked out is not stored but worked
/// out afresh (<see cref="Ledger.DifferenceOf"/>), so that it always shows what is still
/// unexplained.</para>
///
/// <para>It is <b>net worth only</b>: never income, in no period's <i>Unassigned</i>, and it
/// changes no budget figure. It may be zero or negative. It is dated the day it was typed, can be
/// removed, and cannot be changed — to change one, correct again.</para>
/// </summary>
public sealed record BalanceCorrection(
    int Id, DateOnly Date, Account Account, Money Balance, bool IsStartingBalance) : IEntry;
