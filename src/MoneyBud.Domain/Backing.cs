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
/// <para>Since increment 15, <see cref="PaidHereBefore"/> is what the account had paid for the
/// category in expenses dated from <see cref="AccumulatingFrom"/> on, by the time it became the backing
/// account; before, only those in the period of <see cref="HereFrom"/>. Data kept before version 8 is
/// converted on reading (<see cref="Ledger.FromSnapshot"/>).</para>
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
///
/// <para>And with each mark, the <b>first day its period had</b> at that moment:
/// <see cref="AccumulatingFrom"/> and <see cref="HereFrom"/>. Expenses are counted from there. It is
/// remembered, not worked out again from the mark's date, because a change of the period start day
/// can cut that period short, and <i>Opgebouwd</i> must not move because of it (§12, <i>A change never
/// changes Opgebouwd</i>, follow-up 5; ADR 0012).</para>
///
/// <para><see cref="NotMoved"/> is the period's <i>Budget</i> less what moved at backing, which since
/// increment 15 may be below zero: an overspent category moves its overspending the other way (§12,
/// follow-up 15). So it is what had been spent by then.</para>
///
/// <para><b>Since increment 15 a backing is one stretch of a short history</b> (ADR 0015).
/// <see cref="Earlier"/> is the stretch before it: the "—" the category was set to, which may have left
/// money behind, and through that the backing before. A period before <see cref="AccumulatingFrom"/> is
/// worked out by that history, so <i>Opgebouwd</i> carries on across "—" and stepping back still shows
/// what each period had built. Null for a category backed for the first time, or backed again under a
/// version that started <i>Opgebouwd</i> over.</para>
/// </summary>
public sealed record Backing(
    Account Account, EntryMark AccumulatingSince, EntryMark HereSince, Money NotMoved, Money PaidHereBefore,
    DateOnly AccumulatingFrom, DateOnly HereFrom, LeftBehind? Earlier = null)
{
    /// <summary>The mark the whole history counts from: this stretch's, or the oldest before it.</summary>
    public EntryMark FirstSince => Earlier?.Before.FirstSince ?? AccumulatingSince;
}

/// <summary>
/// A category set to "—", and what it left behind (arc42 §12, <i>Setting Staat op to "—"</i>; ADR 0015).
/// "—" sends only the current period's money back to the pool account; everything older stays on the
/// account where it was, <see cref="Account"/>, still the category's <i>Opgebouwd</i>.
///
/// <para><see cref="Amount"/> is what was left there at <see cref="Since"/>, the mark drawn when it was
/// set, in the period starting <see cref="From"/>. It is a fixed figure: the category's expenses do not
/// lower it (§12, follow-up), and only moves out of it do — a reallocation, or <i>Restant bijwerken</i>
/// taking an over-sweep back. It may be zero, and then nothing shows; the record is kept, so that setting
/// an account again carries on. It may be below zero, when the older periods were overspent.</para>
///
/// <para><see cref="Before"/> is the backing it ended, which works out every period before
/// <see cref="From"/>.</para>
/// </summary>
public sealed record LeftBehind(Account Account, EntryMark Since, DateOnly From, Money Amount, Backing Before);

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
/// the movements made, in the order made. Never refused: backing is chosen from a list, and every
/// choice in it is allowed, including one that overdraws an account (§12).
///
/// <para>Since increment 15 an act can make two: backing a category again after "—" takes the money it
/// left behind along, and moves this period's <i>Resterend</i> off the pool account as well.</para>
/// </summary>
public sealed record SetBackingResult(
    Category Category, BackingOutcome Outcome, Account? Before, Account? After, IReadOnlyList<Movement> Moves)
{
    /// <summary>
    /// The movements between two accounts, which are what is said. A movement from an account to itself
    /// changes no balance, so there is nothing to say about it.
    /// </summary>
    public IReadOnlyList<Movement> MovedBetweenAccounts => Moves.Where(m => m.From != m.To).ToList();

    /// <summary>Whether money visibly moved.</summary>
    public bool MovedMoney => MovedBetweenAccounts.Count > 0;
}
