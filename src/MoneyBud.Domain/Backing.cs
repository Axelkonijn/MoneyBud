namespace MoneyBud.Domain;

/// <summary>
/// The account a category's money really sits in (arc42 §12, <i>Backing and Accumulated</i>). A
/// category has one, or none; several per category are deferred until missed.
///
/// <para>Two marks go with it, because two figures count from different moments.
/// <see cref="AccumulatingSince"/> is when the category was last backed after being unbacked:
/// <i>Accumulated</i> counts from there, and re-pointing leaves it alone, so <i>Accumulated</i>
/// carries on (§12, <i>Re-backing starts Accumulated over</i>). <see cref="HereSince"/> is when this
/// account became the backing account, by backing or by re-pointing: what is there for the category
/// in it counts from there (<see cref="Ledger.ThereFor"/>).</para>
///
/// <para>A mark is drawn from the entries' counter without being an entry, so it orders against an
/// expense recorded the same day. It orders movements; expenses are counted by <b>period</b>: every
/// expense dated in the mark's period or later counts, whenever it was entered (§12, <i>ruling of
/// 2026-09-28</i>, under <i>Backing a category that already has money</i>). So a figure is remembered
/// with each mark, for what that period had already done before it.</para>
///
/// <para><see cref="NotMoved"/> goes with <see cref="AccumulatingSince"/>: the part of that period's
/// <i>Budget</i> that did not move at backing, which is what had been spent by then, at most the
/// <i>Budget</i>. Added back, it makes <i>Opgebouwd</i> in the period of backing read what
/// <i>Resterend</i> reads, overspent included. <see cref="PaidHereBefore"/> goes with
/// <see cref="HereSince"/>: what the account had paid for the category in that period before it became
/// the backing account, so what is there for it starts at what moved there.</para>
/// </summary>
public sealed record Backing(
    Account Account, EntryMark AccumulatingSince, EntryMark HereSince, Money NotMoved, Money PaidHereBefore);

/// <summary>What setting a category's backing did.</summary>
public enum BackingOutcome
{
    /// <summary>An unbacked category is now backed.</summary>
    Backed,

    /// <summary>A backed category now points at another account.</summary>
    Repointed,

    /// <summary>A backed category is no longer backed.</summary>
    Unbacked,

    /// <summary>What was chosen was already set. Nothing changed, and nothing is said.</summary>
    Unchanged,
}

/// <summary>
/// What came of setting a category's backing: what it did, the account it was backed by before, and
/// the money that moved, if any. Never refused: backing is chosen from a list, and every choice in
/// it is allowed, including one that overdraws an account (§12).
/// </summary>
public sealed record SetBackingResult(
    Category Category, BackingOutcome Outcome, Account? Before, Account? After, Movement? Moved)
{
    /// <summary>
    /// Whether money visibly moved: a movement between two accounts. A movement from the pool account
    /// to itself changes no balance, so there is nothing to say about it.
    /// </summary>
    public bool MovedMoney => Moved is { } m && m.From != m.To;
}
