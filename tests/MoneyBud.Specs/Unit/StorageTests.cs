using System.Text;
using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using MoneyBud.Storage;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Keeping the ledger (arc42 §8.3, ADR 0007): the form it is kept in, the rules kept data is read
/// against, and the file it is kept in. The scenarios show that what was entered comes back; these
/// hold the parts no scenario can reach — every field of the format, every way kept data can be
/// broken, and what the file store does on the disk.
///
/// <para>Every file here is in a folder of the test's own under the machine's temporary folder,
/// never the user's profile, and is removed afterwards. All data is synthetic.</para>
/// </summary>
public sealed class StorageTests : IDisposable
{
    private static readonly DateOnly Today = new(2026, 3, 15);

    private readonly FixedClock clock = new(new DateTimeOffset(Today.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero));
    private readonly string folder = Path.Combine(Path.GetTempPath(), "MoneyBud.Specs", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
    }

    // ------------------------------------------------------------------ the format

    /// <summary>
    /// A ledger with one of everything that has to survive: an archived category, a renamed one
    /// and a new one on its old name, a budget taken back to zero, a budget in the next period, an
    /// expense with no label, a future-dated income, and a removed entry above the rest, so the
    /// last id issued is not the largest one kept.
    /// </summary>
    private Ledger Everything()
    {
        var ledger = new Ledger(clock);
        ledger.AddCategory("Groceries");
        ledger.AddCategory("Hobby");
        ledger.AddCategory("Magazines");
        ledger.RecordIncome(1832.45m, "Salaris", Today);
        ledger.RecordIncome(1900m, "Salaris april", Today.AddMonths(1));
        ledger.Assign(400m, "Groceries", ledger.CurrentPeriod);
        ledger.Assign(30m, "Hobby", ledger.CurrentPeriod);
        ledger.Assign(-30m, "Hobby", ledger.CurrentPeriod);
        ledger.Assign(350m, "Groceries", ledger.Calendar.Next(ledger.CurrentPeriod));
        ledger.RecordExpense(32.15m, "Groceries", Today, "Albert Heijn");
        ledger.RecordExpense(0.01m, "Groceries", Today.AddDays(-15));
        ledger.RenameCategory("Groceries", "Food");
        ledger.AddCategory("Groceries");
        ledger.RecordExpense(12.50m, "Groceries", Today, "Kiosk");
        ledger.ArchiveCategory("Magazines");
        var removed = ledger.RecordExpense(3.50m, "Food", Today, "Coffee").Expense!;
        ledger.RemoveExpense(removed);
        return ledger;
    }

    [Fact]
    public void Everything_the_ledger_holds_comes_back_from_the_format_exactly()
    {
        var original = Everything();
        var text = LedgerJson.Write(original.ToSnapshot());

        var restored = Ledger.FromSnapshot(LedgerJson.Read(text)!, clock);

        Assert.Equal(text, LedgerJson.Write(restored.ToSnapshot()));

        var now = restored.CurrentPeriod;
        Assert.Equal(["Food", "Hobby", "Groceries"], restored.CategoriesOffered.Select(c => c.Name));
        Assert.Equal(["Magazines"], restored.ArchivedCategories.Select(c => c.Name));
        Assert.Equal(Money.FromCents(40000), restored.BudgetFor("Food", now));
        Assert.Equal(Money.FromCents(35000), restored.BudgetFor("Food", restored.Calendar.Next(now)));
        Assert.True(restored.HasBudget("Hobby", now));
        Assert.Equal(Money.Zero, restored.BudgetFor("Hobby", now));
        Assert.Equal(Money.FromCents(-1250), restored.RemainingFor("Groceries", now));
        Assert.Equal(
            [(3, "Albert Heijn", 3215L, "Food"), (5, "Kiosk", 1250L, "Groceries")],
            restored.ExpensesIn(now).Select(e => (e.Id, e.Label, e.Amount.Cents, e.Category.Name)));
        Assert.Null(Assert.Single(restored.ExpensesIn(restored.Calendar.Previous(now))).Label);
        Assert.Equal(Money.FromCents(190000), restored.UnassignedIn(restored.Calendar.Next(now)) + Money.FromCents(35000));
    }

    [Fact]
    public void An_entry_recorded_after_loading_gets_an_id_never_issued_before()
    {
        var restored = Ledger.FromSnapshot(Everything().ToSnapshot(), clock);

        var next = restored.RecordExpense(1m, "Food", Today).Expense!;

        // Coffee was 6, and removed: 6 is still never issued again.
        Assert.Equal(7, next.Id);
    }

    [Fact]
    public void Amounts_are_kept_as_whole_cents_and_dates_as_days()
    {
        var text = LedgerJson.Write(Everything().ToSnapshot());

        Assert.Contains("\"cents\": 3215", text);
        Assert.Contains("\"cents\": 1", text);
        Assert.Contains("\"date\": \"2026-03-15\"", text);
        Assert.Contains("\"periodStart\": \"2026-03-01\"", text);
        Assert.Contains("\"label\": null", text);
        Assert.DoesNotContain("32.15", text);
    }

    [Fact]
    public void An_empty_budget_is_written_and_read_back_as_no_categories()
    {
        var text = LedgerJson.Write(new Ledger(clock).ToSnapshot());

        var restored = Ledger.FromSnapshot(LedgerJson.Read(text)!, clock);

        Assert.Empty(restored.CategoriesOffered);
        Assert.Empty(restored.ArchivedCategories);
    }

    public static TheoryData<string, string> Unreadable => new()
    {
        { "blank", "" },
        { "only whitespace", "  \n\t " },
        { "cut off", "{ \"format\": \"MoneyBud\", \"version\": 1, \"categ" },
        { "not an object", "[]" },
        { "another format", Valid().Replace("\"MoneyBud\"", "\"SomethingElse\"") },
        { "a newer version", Valid().Replace("\"version\": 1", "\"version\": 2") },
        { "version 0", Valid().Replace("\"version\": 1", "\"version\": 0") },
        { "no version", Valid().Replace("\"version\": 1,", "") },
        { "no categories", Valid().Replace("\"categories\"", "\"kategorien\"") },
        { "categories not a list", Valid().Replace("\"budgets\": [", "\"budgets\": {").Replace("\n  ],\n  \"expenses\"", "\n  },\n  \"expenses\"") },
        { "a fraction of a cent", Valid().Replace("\"cents\": 3215", "\"cents\": 3215.5") },
        { "cents as text", Valid().Replace("\"cents\": 3215", "\"cents\": \"3215\"") },
        { "a date in another form", Valid().Replace("\"2026-03-15\"", "\"15-03-2026\"") },
        { "an income without a label", Valid().Replace("\"label\": \"Salaris\"", "\"label\": null") },
    };

    [Theory]
    [MemberData(nameof(Unreadable))]
    public void Anything_but_a_whole_version_1_document_cannot_be_read(string what, string text)
    {
        _ = what;
        Assert.NotEqual(Valid(), text);
        Assert.Null(LedgerJson.Read(text));
    }

    private static string Valid()
    {
        var ledger = new Ledger(new FixedClock(new DateTimeOffset(Today.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero)));
        ledger.AddCategory("Groceries");
        ledger.RecordIncome(1832.45m, "Salaris", Today);
        ledger.Assign(400m, "Groceries", ledger.CurrentPeriod);
        ledger.RecordExpense(32.15m, "Groceries", Today, "Albert Heijn");
        var text = LedgerJson.Write(ledger.ToSnapshot());
        Assert.NotNull(LedgerJson.Read(text));
        return text;
    }

    // ------------------------------------------------------------------ the rules kept data is read against

    public static TheoryData<string, LedgerSnapshot> Broken
    {
        get
        {
            CategorySnapshot groceries = new(1, "Groceries", false);
            var march = new DateOnly(2026, 3, 1);
            ExpenseSnapshot expense = new(1, Money.FromCents(100), Today, 1, "Kiosk");

            LedgerSnapshot With(
                IReadOnlyList<CategorySnapshot>? categories = null, IReadOnlyList<BudgetSnapshot>? budgets = null,
                IReadOnlyList<ExpenseSnapshot>? expenses = null, IReadOnlyList<IncomeSnapshot>? incomes = null,
                int last = 2) =>
                new(categories ?? [groceries], budgets ?? [], expenses ?? [], incomes ?? [], last);

            return new()
            {
                { "a name with space at its end", With(categories: [new(1, "Groceries ", false)]) },
                { "an empty name", With(categories: [new(1, "", false)]) },
                { "two names the rule counts as one", With(categories: [groceries, new(2, "groceries", true)]) },
                { "a category key used twice", With(categories: [groceries, new(1, "Hobby", false)]) },
                { "a budget for no category", With(budgets: [new(9, march, Money.FromCents(100))]) },
                { "a budget below zero", With(budgets: [new(1, march, Money.FromCents(-1))]) },
                { "a budget starting mid-period", With(budgets: [new(1, march.AddDays(3), Money.FromCents(100))]) },
                { "two budgets for one period", With(budgets: [new(1, march, Money.FromCents(1)), new(1, march, Money.FromCents(2))]) },
                { "an expense for no category", With(expenses: [expense with { Category = 9 }]) },
                { "an expense of zero", With(expenses: [expense with { Amount = Money.Zero }]) },
                { "an expense below zero", With(expenses: [expense with { Amount = Money.FromCents(-100) }]) },
                { "an expense label with space at its end", With(expenses: [expense with { Label = "Kiosk " }]) },
                { "an expense label of only space", With(expenses: [expense with { Label = "  " }]) },
                { "an income without a label", With(incomes: [new(1, Money.FromCents(100), Today, "")]) },
                { "an id used twice", With(expenses: [expense], incomes: [new(1, Money.FromCents(100), Today, "Salaris")]) },
                { "an id never issued", With(expenses: [expense with { Id = 3 }]) },
                { "an id of zero", With(expenses: [expense with { Id = 0 }]) },
                { "a last id below zero", With(last: -1) },
            };
        }
    }

    [Theory]
    [MemberData(nameof(Broken))]
    public void Kept_data_that_breaks_a_rule_the_ledger_keeps_cannot_be_read(string what, LedgerSnapshot snapshot)
    {
        _ = what;
        Assert.Throws<InvalidDataException>(() => Ledger.FromSnapshot(snapshot, clock));
    }

    // The last month the calendar has: a period starting there has no end it can name, so a budget
    // there cannot be read — said as such, rather than failing the start some other way.
    [Fact]
    public void A_budget_in_the_calendars_last_month_cannot_be_read()
    {
        var snapshot = new LedgerSnapshot(
            [new(1, "Groceries", false)], [new(1, new DateOnly(9999, 12, 1), Money.FromCents(100))], [], [], 0);

        Assert.Throws<InvalidDataException>(() => Ledger.FromSnapshot(snapshot, clock));
    }

    // An expense cannot be recorded in the future, but one recorded today is still valid kept data
    // if the machine's clock is later turned back.
    [Fact]
    public void An_expense_dated_after_today_is_still_valid_kept_data()
    {
        var snapshot = new LedgerSnapshot(
            [new(1, "Groceries", false)], [], [new(1, Money.FromCents(100), Today.AddDays(5), 1, null)], [], 1);

        Assert.Single(Ledger.FromSnapshot(snapshot, clock).ExpensesIn(new BudgetPeriod(Today, Today.AddDays(30))));
    }

    // ------------------------------------------------------------------ the file

    private FileLedgerStore Claimed()
    {
        var store = new FileLedgerStore(folder);
        Assert.Equal(Claim.Claimed, store.TryClaim());
        return store;
    }

    private string DataFile => Path.Combine(folder, FileLedgerStore.DataFileName);

    private string TemporaryFile => Path.Combine(folder, FileLedgerStore.TemporaryFileName);

    [Fact]
    public void A_store_is_not_loaded_or_saved_before_it_is_claimed()
    {
        using var store = new FileLedgerStore(folder);

        Assert.Throws<InvalidOperationException>(() => store.Load());
        Assert.Throws<InvalidOperationException>(() => store.TrySave(new Ledger(clock).ToSnapshot()));
    }

    [Fact]
    public void Nothing_kept_yet_is_no_data_and_what_is_saved_loads_back()
    {
        using var store = Claimed();
        Assert.IsType<LoadResult.NoData>(store.Load());

        var snapshot = Everything().ToSnapshot();
        Assert.True(store.TrySave(snapshot));

        var loaded = Assert.IsType<LoadResult.Loaded>(store.Load());
        Assert.Equal(LedgerJson.Write(snapshot), LedgerJson.Write(loaded.Snapshot));
        Assert.False(File.Exists(TemporaryFile));
    }

    [Fact]
    public void The_file_is_written_as_utf8_without_a_byte_order_mark()
    {
        using var store = Claimed();
        var ledger = new Ledger(clock);
        ledger.AddCategory("Één keer");
        store.TrySave(ledger.ToSnapshot());

        var bytes = File.ReadAllBytes(DataFile);
        Assert.NotEqual(0xEF, bytes[0]);
        Assert.Contains("Één keer", Encoding.UTF8.GetString(bytes));
    }

    // A save that cannot even begin — here because a folder stands where the temporary file goes,
    // as the scenarios make saving impossible — reports it and leaves the kept file as it was.
    [Fact]
    public void A_save_that_fails_leaves_what_was_kept_byte_for_byte()
    {
        using var store = Claimed();
        store.TrySave(Everything().ToSnapshot());
        var before = File.ReadAllBytes(DataFile);
        Directory.CreateDirectory(TemporaryFile);

        Assert.False(store.TrySave(new Ledger(clock).ToSnapshot()));

        Assert.Equal(before, File.ReadAllBytes(DataFile));
    }

    // What a save cut off part-way leaves behind: the kept file as it was, and half of the new one
    // in the temporary file. The half is never read, and the next save writes over it.
    [Fact]
    public void Half_a_save_left_behind_is_never_read_and_the_next_save_replaces_it()
    {
        using (var first = Claimed())
            first.TrySave(Everything().ToSnapshot());
        var kept = File.ReadAllText(DataFile);
        File.WriteAllText(TemporaryFile, kept[..(kept.Length / 2)]);

        using var store = Claimed();
        var loaded = Assert.IsType<LoadResult.Loaded>(store.Load());
        Assert.Equal(kept, LedgerJson.Write(loaded.Snapshot));

        Assert.True(store.TrySave(new Ledger(clock).ToSnapshot()));
        Assert.False(File.Exists(TemporaryFile));
        Assert.Empty(Assert.IsType<LoadResult.Loaded>(store.Load()).Snapshot.Categories);
    }

    [Fact]
    public void Kept_data_that_cannot_be_read_is_reported_and_left_alone()
    {
        Directory.CreateDirectory(folder);
        byte[] notUtf8 = [0x7B, 0xFF, 0xFE, 0x7D];
        File.WriteAllBytes(DataFile, notUtf8);

        using var store = Claimed();

        Assert.IsType<LoadResult.Unreadable>(store.Load());
        Assert.Equal(notUtf8, File.ReadAllBytes(DataFile));
    }

    [Fact]
    public void Only_one_store_holds_the_data_until_it_lets_go()
    {
        var first = Claimed();
        using var second = new FileLedgerStore(folder);

        Assert.Equal(Claim.HeldElsewhere, second.TryClaim());

        first.Dispose();
        Assert.Equal(Claim.Claimed, second.TryClaim());
    }

    [Fact]
    public void A_folder_that_cannot_be_reached_is_unreachable()
    {
        Directory.CreateDirectory(folder);
        var aFile = Path.Combine(folder, "a file");
        File.WriteAllText(aFile, "");

        using var underAFile = new FileLedgerStore(Path.Combine(aFile, "MoneyBud"));
        using var relative = new FileLedgerStore("MoneyBud");
        using var none = new FileLedgerStore("");

        Assert.Equal(Claim.Unreachable, underAFile.TryClaim());
        Assert.Equal(Claim.Unreachable, relative.TryClaim());
        Assert.Equal(Claim.Unreachable, none.TryClaim());
    }

    // Never the working directory: MoneyBud runs from inside a working copy of a public repository.
    [Fact]
    public void The_data_lives_in_the_users_local_application_data_and_never_in_the_working_directory()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var kept = FileLedgerStore.DefaultFolder;

        Assert.Equal(Path.Combine(local, "MoneyBud"), kept);
        Assert.True(Path.IsPathFullyQualified(kept));
        Assert.False(Path.GetFullPath(kept).StartsWith(Path.GetFullPath(Repository.Root), StringComparison.OrdinalIgnoreCase));
    }

    // ------------------------------------------------------------------ starting and saving

    [Fact]
    public void Kept_data_that_breaks_a_rule_does_not_start_and_lets_go_of_the_store()
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(DataFile, Valid().Replace("\"cents\": 3215", "\"cents\": -3215"));

        var started = MoneyBudStart.Start(new FileLedgerStore(folder), clock);

        Assert.Equal(StartRefusal.CannotRead, Assert.IsType<StartResult.Refused>(started).Reason);
        using var next = new FileLedgerStore(folder);
        Assert.Equal(Claim.Claimed, next.TryClaim());
    }

    [Fact]
    public void A_first_start_saves_nothing_until_the_first_change()
    {
        var app = Assert.IsType<StartResult.Opened>(MoneyBudStart.Start(new FileLedgerStore(folder), clock)).App;
        Assert.False(File.Exists(DataFile));

        app.AddCategory("Groceries");
        Assert.True(File.Exists(DataFile));
        app.Close();
    }

    // Only an act that changed the ledger saves. A refusal, an unchanged save, a declined question,
    // adding a name already there, assigning zero and a negative amount clipped in full against a
    // Budget of zero change nothing, so they neither save nor retry — and a tick with nothing
    // unsaved does neither.
    [Fact]
    public void Only_an_act_that_went_through_saves()
    {
        var store = new CountingStore();
        var ledger = new Ledger(clock);
        ledger.AddCategory("Groceries");
        var expense = ledger.RecordExpense(12.50m, "Groceries", Today, "Kiosk").Expense!;
        var app = new MoneyBudApp(ledger, store);

        app.RecordExpense("0", "Groceries", "Markt");
        app.RecordExpense("twelve", "Groceries", "Markt");
        app.ChangeExpense(expense, "12,50", "Groceries", "Kiosk", Today);
        app.AskToRemove(expense);
        app.Decline();
        app.AddCategory("  groceries ");
        app.Assign("0", "Groceries");
        app.Assign("-5", "Groceries");
        app.Tick();
        Assert.Equal(0, store.Saves);
        Assert.Null(app.SaveLine);

        app.RecordExpense("3,50", "Groceries", "Coffee");
        Assert.Equal(1, store.Saves);
    }

    [Fact]
    public void Closing_tries_to_save_only_what_is_not_yet_saved_and_lets_go()
    {
        var store = new CountingStore();
        var ledger = new Ledger(clock);
        var app = new MoneyBudApp(ledger, store);
        app.AddCategory("Groceries");

        app.Close();

        Assert.Equal(1, store.Saves);
        Assert.True(store.Disposed);
    }

    private sealed class CountingStore : ILedgerStore
    {
        public int Saves { get; private set; }
        public bool Disposed { get; private set; }

        public Claim TryClaim() => Claim.Claimed;

        public LoadResult Load() => new LoadResult.NoData();

        public bool TrySave(LedgerSnapshot snapshot)
        {
            Saves++;
            return true;
        }

        public void Dispose() => Disposed = true;
    }
}
