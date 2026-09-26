using MoneyBud.Domain;

namespace MoneyBud.Presentation;

/// <summary>Why MoneyBud did not start.</summary>
public enum StartRefusal
{
    /// <summary>
    /// What is kept cannot be read — damaged, blank, written by a newer MoneyBud, or not reachable
    /// at all. Nothing was written over it (arc42 §12, <i>When the data cannot be read</i>).
    /// </summary>
    CannotRead,

    /// <summary>Another MoneyBud is open and holds the data (§12, <i>Only one MoneyBud at a time</i>).</summary>
    AlreadyOpen,
}

/// <summary>What came of starting MoneyBud: the screen, or the one thing to say before closing.</summary>
public abstract record StartResult
{
    private StartResult() { }

    public sealed record Opened(MoneyBudApp App) : StartResult;

    public sealed record Refused(StartRefusal Reason) : StartResult
    {
        public string Text => Reason switch
        {
            StartRefusal.CannotRead => Tekst.CannotRead,
            StartRefusal.AlreadyOpen => Tekst.AlreadyOpen,
        };
    }
}

/// <summary>
/// Starting MoneyBud (arc42 §12, <i>What MoneyBud keeps</i>). One of four things happens:
///
/// <list type="bullet">
/// <item><b>Another MoneyBud holds the data:</b> this one says so and does not start.</item>
/// <item><b>Nothing is kept yet:</b> a first start — the default categories and nothing else.
/// Nothing is saved until the first change, since closing straight away would leave the next start
/// exactly where this one began.</item>
/// <item><b>Kept data that can be read</b>, against every rule the ledger keeps: MoneyBud opens it,
/// as it was left, on the current period. The default categories are not added back.</item>
/// <item><b>Kept data that cannot be read:</b> MoneyBud says so, and does not start, so nothing
/// can be entered and nothing is saved over it.</item>
/// </list>
///
/// <para>The data is claimed before it is read, so that a second MoneyBud never reads a file the
/// first is part-way through writing.</para>
/// </summary>
public static class MoneyBudStart
{
    /// <param name="store">Unclaimed. Handed on to the screen when MoneyBud opens, and let go of
    /// here when it does not.</param>
    public static StartResult Start(ILedgerStore store, TimeProvider clock)
    {
        switch (store.TryClaim())
        {
            case Claim.HeldElsewhere:
                return Refuse(StartRefusal.AlreadyOpen);
            case Claim.Unreachable:
                return Refuse(StartRefusal.CannotRead);
        }

        Ledger ledger;
        switch (store.Load())
        {
            case LoadResult.NoData:
                ledger = Ledger.StartNew(clock);
                break;
            case LoadResult.Loaded loaded:
                try
                {
                    ledger = Ledger.FromSnapshot(loaded.Snapshot, clock);
                }
                catch (InvalidDataException)
                {
                    return Refuse(StartRefusal.CannotRead);
                }
                break;
            default:
                return Refuse(StartRefusal.CannotRead);
        }

        return new StartResult.Opened(new MoneyBudApp(ledger, store));

        StartResult Refuse(StartRefusal reason)
        {
            store.Dispose();
            return new StartResult.Refused(reason);
        }
    }
}
