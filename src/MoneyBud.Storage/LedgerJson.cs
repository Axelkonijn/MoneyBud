using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using MoneyBud.Domain;

namespace MoneyBud.Storage;

/// <summary>
/// The form a ledger is kept in: one JSON document, the whole ledger every time (ADR 0007).
///
/// <code>
/// {
///   "format": "MoneyBud",
///   "version": 6,
///   "lastEntryId": 7,
///   "settledThrough": "2026-03-15",
///   "categories": [ { "key": 1, "name": "Boodschappen", "archived": false, "backing": null },
///                  { "key": 2, "name": "Sparen", "archived": false,
///                    "backing": { "account": 2, "accumulatingSince": { "date": "2026-03-15", "id": 5 },
///                                 "hereSince": { "date": "2026-03-15", "id": 5 },
///                                 "notMovedCents": 0, "paidHereBeforeCents": 0 } } ],
///   "budgets":    [ { "category": 1, "periodStart": "2026-03-01", "cents": 40000 } ],
///   "accounts":   [ { "key": 1, "name": "Betaalrekening" }, { "key": 2, "name": "Contant" } ],
///   "poolAccount": 1,
///   "expenses":   [ { "id": 1, "cents": 3215, "date": "2026-03-15", "category": 1, "label": "Albert Heijn", "account": 1 } ],
///   "incomes":    [ { "id": 2, "cents": 183245, "date": "2026-03-15", "label": "Salaris", "account": 1 } ],
///   "transfers":  [ { "id": 3, "cents": 5000, "date": "2026-03-15", "from": 1, "to": 2 } ],
///   "balanceCorrections": [ { "id": 4, "date": "2026-03-15", "account": 1, "cents": 175000, "starting": false } ],
///   "movements":  [ { "id": 6, "date": "2026-03-15", "category": 2, "from": 1, "to": 2, "cents": 20000,
///                     "reason": "backed", "direction": "in", "sweptFor": null },
///                   { "id": 8, "date": "2026-04-01", "category": 2, "from": 1, "to": 2, "cents": 13000,
///                     "reason": "swept", "direction": "in", "sweptFor": "2026-03-01" } ],
///   "sweepDestination": 2,
///   "periodEnds": [ { "periodStart": "2026-03-01", "backed": [ 2 ] } ],
///   "letGo":      [ ],
///   "repeats":    [ { "occurrences": [ 1, 9 ], "frequency": "monthly", "day": 15, "next": "2026-05-15" } ]
/// }
/// </code>
///
/// <para><b>Amounts are whole cents, as JSON integers</b> — exactly what <see cref="Money"/>
/// holds, so nothing is parsed from decimal text and nothing can round (arc42 §8.2). <b>Dates are
/// <c>yyyy-MM-dd</c></b>, with no time and no zone. <b>Categories are referred to by key</b>, never
/// by name (<see cref="LedgerSnapshot"/>), and so are <b>accounts</b>. An expense with no label has
/// <c>"label": null</c>. <b>No balance is written</b>: a balance is worked out from the typed
/// balances and the entries (ADR 0008), so there is no figure in the file that could disagree with
/// them.</para>
///
/// <para><b>Since version 3</b>, the backing increment (ADR 0009): a category's <c>backing</c>, or
/// null, with the two marks it counts from; the <c>movements</c> MoneyBud made on categories' behalf,
/// each with its reason and direction as words; and <c>settledThrough</c>, the day planned money has
/// been moved up to.</para>
///
/// <para><b>Since version 4</b>, the sweep (ADR 0010): a movement's <c>sweptFor</c>, the first day of
/// the period a sweep was for, null on every other movement; <c>sweepDestination</c>, a category key
/// or null; <c>periodEnds</c>, the categories backed when each period ended; and <c>letGo</c>, what a
/// period's line stopped asking for, in cents.</para>
///
/// <para><b>Since version 5</b>, recurring entries (ADR 0011): <c>repeats</c>, each with the ids of
/// its <c>occurrences</c> in the order recorded, its <c>frequency</c> as a word, <c>day</c>, the day of
/// the month a monthly one was last set to, and <c>next</c>, the date of the next occurrence. A stopped
/// repeat has all three null, and a weekly one has no day.</para>
///
/// <para><b>Since version 6</b>, the change to <i>Opgebouwd</i> of 2026-09-28: a backing's
/// <c>notMovedCents</c> and <c>paidHereBeforeCents</c>, the figure remembered with each mark
/// (<see cref="Backing"/>).</para>
///
/// <para>Reading is strict: anything that is not a whole version-6 document is not read at all.
/// That includes a blank document — MoneyBud never writes one, so blank means something went
/// wrong — and a newer version, since this MoneyBud cannot know what a newer one meant
/// (§12, <i>When the data cannot be read</i>). <b>Version 1</b>, the form before accounts, is not
/// read either: the stakeholder ruled that data saved before accounts is started afresh rather than
/// carried over (§12, <i>Accounts and net worth</i>; <i>Demo data may not survive a new version</i>).
/// Nor is <b>version 2</b>, the form with accounts and without backing: the stakeholder did not mind
/// starting over, and reading it was not worth a second way in (plan for increment 10, D2). Nor is
/// <b>version 3</b>, the form with backing and without the sweep: it has no record of what was backed
/// when each period ended, and reading it would mean guessing that, the one thing the sweep's rulings
/// say must not be guessed (plan for increment 11, D2). <b>Version 4</b>, the form with the sweep and
/// without recurring entries, <b>is read</b>, as data with no repeats: nothing could repeat when it was
/// written, so reading it guesses nothing (plan for increment 12, D2). One with a <c>repeats</c> list is
/// not what version 4 wrote, and is not read. <b>Version 5</b>, the form without the two remembered
/// figures, <b>is read</b> too, and so is version 4 through it: the figures are worked out again from
/// the order entries were recorded in, exact unless an expense from before a backing was changed after
/// it (§12, ruling of 2026-09-28). Properties this version does not know are ignored.</para>
/// </summary>
public static class LedgerJson
{
    public const int Version = 6;

    // The older versions still read. Recurring entries', which differs only by not remembering a
    // backing's two figures, and the sweep's, which also has no repeats.
    private const int VersionWithoutFigures = 5;
    private const int VersionWithoutRepeats = 4;
    private const string Format = "MoneyBud";

    public static string Write(LedgerSnapshot snapshot)
    {
        using var buffer = new MemoryStream();
        // Relaxed escaping writes "Één keer" as itself rather than as Één, so the file
        // stays readable to someone looking inside a backup. Its "unsafe" is about pasting JSON
        // into HTML, which this file never is.
        var options = new JsonWriterOptions { Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        using (var json = new Utf8JsonWriter(buffer, options))
        {
            json.WriteStartObject();
            json.WriteString("format", Format);
            json.WriteNumber("version", Version);
            json.WriteNumber("lastEntryId", snapshot.LastEntryId);
            json.WriteString("settledThrough", Date(snapshot.SettledThrough));

            json.WriteStartArray("categories");
            foreach (var c in snapshot.Categories)
            {
                json.WriteStartObject();
                json.WriteNumber("key", c.Key);
                json.WriteString("name", c.Name);
                json.WriteBoolean("archived", c.IsArchived);
                if (c.Backing is { } backing)
                {
                    json.WriteStartObject("backing");
                    json.WriteNumber("account", backing.Account);
                    WriteMark(json, "accumulatingSince", backing.AccumulatingSince);
                    WriteMark(json, "hereSince", backing.HereSince);
                    json.WriteNumber("notMovedCents", backing.NotMoved!.Value.Cents);
                    json.WriteNumber("paidHereBeforeCents", backing.PaidHereBefore!.Value.Cents);
                    json.WriteEndObject();
                }
                else
                {
                    json.WriteNull("backing");
                }
                json.WriteEndObject();
            }
            json.WriteEndArray();

            json.WriteStartArray("budgets");
            foreach (var b in snapshot.Budgets)
            {
                json.WriteStartObject();
                json.WriteNumber("category", b.Category);
                json.WriteString("periodStart", Date(b.PeriodStart));
                json.WriteNumber("cents", b.Amount.Cents);
                json.WriteEndObject();
            }
            json.WriteEndArray();

            json.WriteStartArray("accounts");
            foreach (var a in snapshot.Accounts)
            {
                json.WriteStartObject();
                json.WriteNumber("key", a.Key);
                json.WriteString("name", a.Name);
                json.WriteEndObject();
            }
            json.WriteEndArray();

            json.WriteNumber("poolAccount", snapshot.PoolAccount);

            json.WriteStartArray("expenses");
            foreach (var e in snapshot.Expenses)
            {
                json.WriteStartObject();
                json.WriteNumber("id", e.Id);
                json.WriteNumber("cents", e.Amount.Cents);
                json.WriteString("date", Date(e.Date));
                json.WriteNumber("category", e.Category);
                json.WriteString("label", e.Label);
                json.WriteNumber("account", e.Account);
                json.WriteEndObject();
            }
            json.WriteEndArray();

            json.WriteStartArray("incomes");
            foreach (var i in snapshot.Incomes)
            {
                json.WriteStartObject();
                json.WriteNumber("id", i.Id);
                json.WriteNumber("cents", i.Amount.Cents);
                json.WriteString("date", Date(i.Date));
                json.WriteString("label", i.Label);
                json.WriteNumber("account", i.Account);
                json.WriteEndObject();
            }
            json.WriteEndArray();

            json.WriteStartArray("transfers");
            foreach (var t in snapshot.Transfers)
            {
                json.WriteStartObject();
                json.WriteNumber("id", t.Id);
                json.WriteNumber("cents", t.Amount.Cents);
                json.WriteString("date", Date(t.Date));
                json.WriteNumber("from", t.From);
                json.WriteNumber("to", t.To);
                json.WriteEndObject();
            }
            json.WriteEndArray();

            json.WriteStartArray("balanceCorrections");
            foreach (var c in snapshot.BalanceCorrections)
            {
                json.WriteStartObject();
                json.WriteNumber("id", c.Id);
                json.WriteString("date", Date(c.Date));
                json.WriteNumber("account", c.Account);
                json.WriteNumber("cents", c.Balance.Cents);
                json.WriteBoolean("starting", c.IsStartingBalance);
                json.WriteEndObject();
            }
            json.WriteEndArray();

            json.WriteStartArray("movements");
            foreach (var m in snapshot.Movements)
            {
                json.WriteStartObject();
                json.WriteNumber("id", m.Id);
                json.WriteString("date", Date(m.Date));
                json.WriteNumber("category", m.Category);
                json.WriteNumber("from", m.From);
                json.WriteNumber("to", m.To);
                json.WriteNumber("cents", m.Amount.Cents);
                json.WriteString("reason", Word(m.Reason));
                json.WriteString("direction", Word(m.Direction));
                if (m.SweptFor is { } sweptFor) json.WriteString("sweptFor", Date(sweptFor));
                else json.WriteNull("sweptFor");
                json.WriteEndObject();
            }
            json.WriteEndArray();

            if (snapshot.SweepDestination is { } destination) json.WriteNumber("sweepDestination", destination);
            else json.WriteNull("sweepDestination");

            json.WriteStartArray("periodEnds");
            foreach (var p in snapshot.PeriodEnds)
            {
                json.WriteStartObject();
                json.WriteString("periodStart", Date(p.PeriodStart));
                json.WriteStartArray("backed");
                foreach (var key in p.Backed) json.WriteNumberValue(key);
                json.WriteEndArray();
                json.WriteEndObject();
            }
            json.WriteEndArray();

            json.WriteStartArray("letGo");
            foreach (var l in snapshot.LetGo)
            {
                json.WriteStartObject();
                json.WriteString("periodStart", Date(l.PeriodStart));
                json.WriteNumber("cents", l.Amount.Cents);
                json.WriteEndObject();
            }
            json.WriteEndArray();

            json.WriteStartArray("repeats");
            foreach (var r in snapshot.Repeats)
            {
                json.WriteStartObject();
                json.WriteStartArray("occurrences");
                foreach (var id in r.Occurrences) json.WriteNumberValue(id);
                json.WriteEndArray();
                if (r.Frequency is { } frequency) json.WriteString("frequency", Word(frequency));
                else json.WriteNull("frequency");
                if (r.Day is { } day) json.WriteNumber("day", day);
                else json.WriteNull("day");
                if (r.Next is { } next) json.WriteString("next", Date(next));
                else json.WriteNull("next");
                json.WriteEndObject();
            }
            json.WriteEndArray();

            json.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    /// <summary>The snapshot a document holds, or null when it cannot be read.</summary>
    public static LedgerSnapshot? Read(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        try
        {
            using var document = JsonDocument.Parse(text);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object || Text(root, "format") != Format)
                return null;

            var version = Int(root, "version");
            var hasRepeats = root.TryGetProperty("repeats", out _);
            if (!(version is Version or VersionWithoutFigures || (version == VersionWithoutRepeats && !hasRepeats)))
                return null;
            var hasFigures = version == Version;

            return new LedgerSnapshot(
                Array(root, "categories", c => new CategorySnapshot(
                    Int(c, "key"), Text(c, "name"), Bool(c, "archived"), BackingOf(c, hasFigures))),
                Array(root, "budgets", b => new BudgetSnapshot(Int(b, "category"), DateOf(b, "periodStart"), Cents(b))),
                Array(root, "expenses", e => new ExpenseSnapshot(
                    Int(e, "id"), Cents(e), DateOf(e, "date"), Int(e, "category"), TextOrNull(e, "label"), Int(e, "account"))),
                Array(root, "incomes", i => new IncomeSnapshot(
                    Int(i, "id"), Cents(i), DateOf(i, "date"), Text(i, "label"), Int(i, "account"))),
                Int(root, "lastEntryId"),
                Array(root, "accounts", a => new AccountSnapshot(Int(a, "key"), Text(a, "name"))),
                Int(root, "poolAccount"),
                Array(root, "transfers", t => new TransferSnapshot(
                    Int(t, "id"), Cents(t), DateOf(t, "date"), Int(t, "from"), Int(t, "to"))),
                Array(root, "balanceCorrections", c => new BalanceCorrectionSnapshot(
                    Int(c, "id"), DateOf(c, "date"), Int(c, "account"), Cents(c), Bool(c, "starting"))),
                Array(root, "movements", m => new MovementSnapshot(
                    Int(m, "id"), DateOf(m, "date"), Int(m, "category"), Int(m, "from"), Int(m, "to"), Cents(m),
                    WordAs<MovementReason>(m, "reason"), WordAs<MovementDirection>(m, "direction"),
                    DateOrNull(m, "sweptFor"))),
                DateOf(root, "settledThrough"),
                IntOrNull(root, "sweepDestination"),
                Array(root, "periodEnds", p => new PeriodEndSnapshot(
                    DateOf(p, "periodStart"), Array(p, "backed", k => k.GetInt32()))),
                Array(root, "letGo", l => new LetGoSnapshot(DateOf(l, "periodStart"), Cents(l))),
                version == VersionWithoutRepeats
                    ? []
                    : Array(root, "repeats", r => new RepeatSnapshot(
                        Array(r, "occurrences", o => o.GetInt32()),
                        WordOrNull<Frequency>(r, "frequency"), IntOrNull(r, "day"), DateOrNull(r, "next"))));
        }
        catch (Exception e) when (e is JsonException or FormatException or InvalidOperationException
                                      or KeyNotFoundException)
        {
            return null;
        }
    }

    private static string Date(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static void WriteMark(Utf8JsonWriter json, string name, EntryMark mark)
    {
        json.WriteStartObject(name);
        json.WriteString("date", Date(mark.Date));
        json.WriteNumber("id", mark.Id);
        json.WriteEndObject();
    }

    private static BackingSnapshot? BackingOf(JsonElement category, bool hasFigures)
    {
        var backing = category.GetProperty("backing");
        if (backing.ValueKind == JsonValueKind.Null) return null;

        return new BackingSnapshot(
            Int(backing, "account"), MarkOf(backing, "accumulatingSince"), MarkOf(backing, "hereSince"),
            hasFigures ? Cents(backing, "notMovedCents") : null,
            hasFigures ? Cents(backing, "paidHereBeforeCents") : null);
    }

    private static EntryMark MarkOf(JsonElement parent, string name)
    {
        var mark = parent.GetProperty(name);
        return new EntryMark(DateOf(mark, "date"), Int(mark, "id"));
    }

    // Enum values as lower-case words, so the file reads plainly and a renumbered enum cannot
    // change what a kept movement meant. A word this version does not know is not read.
    private static string Word<T>(T value) where T : struct, Enum => value.ToString().ToLowerInvariant();

    private static T WordAs<T>(JsonElement parent, string name) where T : struct, Enum
    {
        var word = Text(parent, name);
        foreach (var value in Enum.GetValues<T>())
            if (Word(value) == word) return value;
        throw new FormatException($"\"{word}\" is not a {typeof(T).Name}.");
    }

    private static T? WordOrNull<T>(JsonElement parent, string name) where T : struct, Enum =>
        parent.GetProperty(name) is { ValueKind: JsonValueKind.Null } ? null : WordAs<T>(parent, name);

    private static List<T> Array<T>(JsonElement parent, string name, Func<JsonElement, T> read)
    {
        var array = parent.GetProperty(name);
        if (array.ValueKind != JsonValueKind.Array) throw new FormatException($"\"{name}\" is not a list.");
        return array.EnumerateArray().Select(read).ToList();
    }

    // GetInt32 and GetInt64 throw FormatException for 12.5 or a number out of range, and
    // InvalidOperationException for anything that is not a number; Read turns both into "cannot
    // be read". So a fraction of a cent can never be read in and rounded.
    private static int Int(JsonElement parent, string name) => parent.GetProperty(name).GetInt32();

    private static int? IntOrNull(JsonElement parent, string name) =>
        parent.GetProperty(name) is { ValueKind: JsonValueKind.Null } ? null : Int(parent, name);

    private static Money Cents(JsonElement parent, string name = "cents") =>
        Money.FromCents(parent.GetProperty(name).GetInt64());

    private static bool Bool(JsonElement parent, string name) => parent.GetProperty(name).GetBoolean();

    private static string Text(JsonElement parent, string name) =>
        parent.GetProperty(name).GetString() ?? throw new FormatException($"\"{name}\" is missing.");

    private static string? TextOrNull(JsonElement parent, string name) => parent.GetProperty(name).GetString();

    private static DateOnly DateOf(JsonElement parent, string name) =>
        DateOnly.ParseExact(Text(parent, name), "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static DateOnly? DateOrNull(JsonElement parent, string name) =>
        parent.GetProperty(name) is { ValueKind: JsonValueKind.Null } ? null : DateOf(parent, name);
}
