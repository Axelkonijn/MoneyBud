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
///   "version": 2,
///   "lastEntryId": 4,
///   "categories": [ { "key": 1, "name": "Boodschappen", "archived": false } ],
///   "budgets":    [ { "category": 1, "periodStart": "2026-03-01", "cents": 40000 } ],
///   "accounts":   [ { "key": 1, "name": "Betaalrekening" }, { "key": 2, "name": "Contant" } ],
///   "poolAccount": 1,
///   "expenses":   [ { "id": 1, "cents": 3215, "date": "2026-03-15", "category": 1, "label": "Albert Heijn", "account": 1 } ],
///   "incomes":    [ { "id": 2, "cents": 183245, "date": "2026-03-15", "label": "Salaris", "account": 1 } ],
///   "transfers":  [ { "id": 3, "cents": 5000, "date": "2026-03-15", "from": 1, "to": 2 } ],
///   "balanceCorrections": [ { "id": 4, "date": "2026-03-15", "account": 1, "cents": 175000, "starting": false } ]
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
/// <para>Reading is strict: anything that is not a whole version-2 document is not read at all.
/// That includes a blank document — MoneyBud never writes one, so blank means something went
/// wrong — and a newer version, since this MoneyBud cannot know what a newer one meant
/// (§12, <i>When the data cannot be read</i>). <b>Version 1</b>, the form before accounts, is not
/// read either: the stakeholder ruled that data saved before accounts is started afresh rather than
/// carried over (§12, <i>Accounts and net worth</i>; <i>Demo data may not survive a new version</i>).
/// Properties this version does not know are ignored.</para>
/// </summary>
public static class LedgerJson
{
    public const int Version = 2;
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

            json.WriteStartArray("categories");
            foreach (var c in snapshot.Categories)
            {
                json.WriteStartObject();
                json.WriteNumber("key", c.Key);
                json.WriteString("name", c.Name);
                json.WriteBoolean("archived", c.IsArchived);
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

            if (root.ValueKind != JsonValueKind.Object
                || Text(root, "format") != Format
                || Int(root, "version") != Version)
                return null;

            return new LedgerSnapshot(
                Array(root, "categories", c => new CategorySnapshot(Int(c, "key"), Text(c, "name"), Bool(c, "archived"))),
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
                    Int(c, "id"), DateOf(c, "date"), Int(c, "account"), Cents(c), Bool(c, "starting"))));
        }
        catch (Exception e) when (e is JsonException or FormatException or InvalidOperationException
                                      or KeyNotFoundException)
        {
            return null;
        }
    }

    private static string Date(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

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

    private static Money Cents(JsonElement parent) => Money.FromCents(parent.GetProperty("cents").GetInt64());

    private static bool Bool(JsonElement parent, string name) => parent.GetProperty(name).GetBoolean();

    private static string Text(JsonElement parent, string name) =>
        parent.GetProperty(name).GetString() ?? throw new FormatException($"\"{name}\" is missing.");

    private static string? TextOrNull(JsonElement parent, string name) => parent.GetProperty(name).GetString();

    private static DateOnly DateOf(JsonElement parent, string name) =>
        DateOnly.ParseExact(Text(parent, name), "yyyy-MM-dd", CultureInfo.InvariantCulture);
}
