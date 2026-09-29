namespace MoneyBud.Domain;

/// <summary>
/// A moment in the ledger's record: a day, and an id from the counter entries share. An entry is
/// <b>after</b> a mark when it is dated later, or dated that day and recorded later — its id is
/// higher (arc42 §12, <i>A typed balance is what the bank said that day</i>). A date has no time of
/// day, so on the day itself only the order of recording can tell.
///
/// <para>A balance correction is a mark, and holds every entry that is not after it. A backing is a
/// mark too, and the movements that count for it are those after it; its expenses are counted by
/// period instead (<see cref="Backing"/>; §12, <i>Backing a category that already has money</i>).</para>
/// </summary>
public readonly record struct EntryMark(DateOnly Date, int Id)
{
    public bool IsBefore(IEntry entry) => entry.Date > Date || (entry.Date == Date && entry.Id > Id);

    public static EntryMark Of(IEntry entry) => new(entry.Date, entry.Id);
}
