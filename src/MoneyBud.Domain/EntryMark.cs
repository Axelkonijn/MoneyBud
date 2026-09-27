namespace MoneyBud.Domain;

/// <summary>
/// A moment in the ledger's record: a day, and an id from the counter entries share. An entry is
/// <b>after</b> a mark when it is dated later, or dated that day and recorded later — its id is
/// higher (arc42 §12, <i>A typed balance is what the bank said that day</i>). A date has no time of
/// day, so on the day itself only the order of recording can tell.
///
/// <para>One rule, used twice: a balance correction is a mark, and holds every entry that is not
/// after it; a backing is a mark, and the expenses that lower <i>Accumulated</i> are those after it
/// (§12, <i>Backing a category that already has money</i>).</para>
/// </summary>
public readonly record struct EntryMark(DateOnly Date, int Id)
{
    public bool IsBefore(IEntry entry) => entry.Date > Date || (entry.Date == Date && entry.Id > Id);

    public static EntryMark Of(IEntry entry) => new(entry.Date, entry.Id);
}
