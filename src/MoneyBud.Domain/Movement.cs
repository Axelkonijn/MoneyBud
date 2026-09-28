namespace MoneyBud.Domain;

/// <summary>
/// Money MoneyBud moved on a category's behalf, from one account to another (arc42 §12, <i>Backing
/// and Accumulated</i>; ADR 0009). The user's own counterpart is a <see cref="Transfer"/>.
///
/// <para><b>It is written on the day the money moves, and never changed.</b> What moves at backing,
/// unbacking and re-pointing is worked out from the figures at that moment, and must stay what it was
/// when something dated earlier is recorded later (§12, <i>follow-ups</i>), so it cannot be worked
/// out afresh. Money planned for a later period is written by <see cref="Ledger.Settle"/> on that
/// period's first day, to whatever backs the category then.</para>
///
/// <para>Like a transfer it is <b>not income and not an expense</b>: it moves two balances and no
/// budget figure, and is in neither of the Overview's lists. It shows in both accounts' histories,
/// read-only. <see cref="Amount"/> is a positive magnitude, <see cref="From"/> to <see cref="To"/>.
/// When the pool account backs the category the two are the same account: no balance changes and
/// no history shows it, but <i>Accumulated</i> still counts it.</para>
///
/// <para><see cref="Direction"/> is what it does to <i>Accumulated</i>, and <see cref="Reason"/>
/// what caused it, for the history's wording. Neither can be told from the accounts: a movement
/// from the pool account to itself is in or out only by what it was for.</para>
///
/// <para><see cref="SweptFor"/> is the first day of the period a sweep was for (arc42 §12, <i>The
/// sweep and Restant</i>; ADR 0010), set exactly when <see cref="Reason"/> is
/// <see cref="MovementReason.Swept"/>. A sweep is dated the day it moved, which is a day of a later
/// period, so the period it was for has to be kept beside it.</para>
/// </summary>
public sealed record Movement(
    int Id, DateOnly Date, Category Category, Account From, Account To, Money Amount,
    MovementReason Reason, MovementDirection Direction, DateOnly? SweptFor = null) : IEntry;

/// <summary>What made MoneyBud move money for a category.</summary>
public enum MovementReason
{
    /// <summary>Assigning to a backed category: a positive amount in, a negative one back out.</summary>
    Assigned,

    /// <summary>Backing a category: its unspent <i>Remaining</i> for the current period.</summary>
    Backed,

    /// <summary>Unbacking a category: what was there for it goes back to the pool account.</summary>
    Unbacked,

    /// <summary>Pointing the backing at another account: what was there for it goes along.</summary>
    Repointed,

    /// <summary>
    /// A period's leftover, <i>Restant</i>: in to the sweep destination at the period's end or by the
    /// button, and out of a category it was swept into too much, by the button.
    /// </summary>
    Swept,
}

/// <summary>Which way a movement goes, as <i>Accumulated</i> counts it.</summary>
public enum MovementDirection
{
    /// <summary>Towards the backing account, out of the pool account. Adds to <i>Accumulated</i>.</summary>
    In,

    /// <summary>Back to the pool account. Takes away from <i>Accumulated</i>.</summary>
    Out,

    /// <summary>From one backing account to the next. <i>Accumulated</i> carries on unchanged.</summary>
    Along,
}
