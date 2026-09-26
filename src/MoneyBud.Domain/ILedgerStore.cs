namespace MoneyBud.Domain;

/// <summary>
/// Where a <see cref="Ledger"/> is kept between runs (arc42 §8.3, ADR 0007). Defined here so that
/// the screen can use a store without knowing what one is made of, and the store can be built
/// without knowing anything about the screen.
///
/// <para>A store is held by one MoneyBud at a time. <see cref="TryClaim"/> comes first, and a
/// store that could not be claimed is not loaded from or saved to. Disposing it lets go.</para>
/// </summary>
public interface ILedgerStore : IDisposable
{
    /// <summary>
    /// Takes the kept data for this MoneyBud alone, unless another MoneyBud already holds it
    /// (§12, <i>Only one MoneyBud at a time</i>) or it cannot be reached at all.
    /// </summary>
    Claim TryClaim();

    /// <summary>What is kept, if anything, and whether it can be read. Changes nothing.</summary>
    LoadResult Load();

    /// <summary>
    /// Keeps the whole ledger, in place of what was kept before. False when that could not be
    /// done, in which case what was kept before is still there, whole (§12, <i>An interrupted save
    /// never damages the previous one</i>).
    /// </summary>
    bool TrySave(LedgerSnapshot snapshot);
}

/// <summary>Whether a store could be claimed.</summary>
public enum Claim
{
    Claimed,

    /// <summary>Another MoneyBud holds it.</summary>
    HeldElsewhere,

    /// <summary>
    /// It cannot be reached — a profile that is not there, a folder that cannot be made. Treated
    /// as data that cannot be read, since nothing can be read from it.
    /// </summary>
    Unreachable,
}

/// <summary>What a store found when it was loaded.</summary>
public abstract record LoadResult
{
    private LoadResult() { }

    /// <summary>Nothing is kept yet: a first start.</summary>
    public sealed record NoData : LoadResult;

    /// <summary>Kept data, read.</summary>
    public sealed record Loaded(LedgerSnapshot Snapshot) : LoadResult;

    /// <summary>
    /// Kept data that cannot be read: damaged, blank, or written by a newer MoneyBud. Nothing may
    /// be written over it (§12, <i>When the data cannot be read</i>).
    /// </summary>
    public sealed record Unreadable : LoadResult;
}
