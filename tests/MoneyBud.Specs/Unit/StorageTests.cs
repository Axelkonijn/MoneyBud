using System.Text;
using System.Text.RegularExpressions;
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
    /// last id issued is not the largest one kept. Since backing: a category backed, re-pointed, and
    /// one backed by the pool account, with money moved every way, and money planned for the next
    /// period that has not moved yet.
    /// </summary>
    private Ledger Everything()
    {
        var ledger = new Ledger(clock, "Bank");
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

        ledger.AddAccount("Deposit", 0m);
        ledger.AddAccount("Broker", null);
        ledger.AddCategory("Savings");
        ledger.AddCategory("Holiday");
        ledger.SetBacking("Savings", ledger.AccountNamed("Deposit"));
        ledger.Assign(300m, "Savings", ledger.CurrentPeriod);
        ledger.Assign(-20m, "Savings", ledger.CurrentPeriod);
        ledger.Assign(200m, "Savings", ledger.Calendar.Next(ledger.CurrentPeriod));
        ledger.SetBacking("Savings", ledger.AccountNamed("Broker"));
        ledger.SetBacking("Holiday", ledger.PoolAccount);
        ledger.Assign(50m, "Holiday", ledger.CurrentPeriod);

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
        Assert.Equal(["Food", "Hobby", "Groceries", "Savings", "Holiday"], restored.CategoriesOffered.Select(c => c.Name));
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
        Assert.Equal(Money.FromCents(190000), restored.UnassignedIn(restored.Calendar.Next(now)) + Money.FromCents(55000));

        var broker = restored.AccountNamed("Broker")!;
        var deposit = restored.AccountNamed("Deposit")!;
        Assert.Equal(broker, restored.BackingOf("Savings"));
        Assert.Equal(restored.PoolAccount, restored.BackingOf("Holiday"));
        Assert.Null(restored.BackingOf("Food"));
        Assert.Equal(Money.FromCents(28000), restored.BalanceOf(broker));
        Assert.Equal(Money.Zero, restored.BalanceOf(deposit));
        Assert.Equal(Money.FromCents(28000), restored.AccumulatedFor("Savings", now));
        Assert.Equal(Money.FromCents(48000), restored.AccumulatedFor("Savings", restored.Calendar.Next(now)));
        Assert.Equal(Money.FromCents(5000), restored.AccumulatedFor("Holiday", now));
        Assert.Equal(
            [MovementReason.Repointed, MovementReason.Assigned, MovementReason.Assigned],
            restored.HistoryOf(deposit).OfType<Movement>().Select(m => m.Reason));
    }

    [Fact]
    public void Money_planned_for_a_later_period_still_moves_on_its_day_after_starting_again()
    {
        var restored = Ledger.FromSnapshot(LedgerJson.Read(LedgerJson.Write(Everything().ToSnapshot()))!, clock);
        var april = restored.Calendar.Next(restored.CurrentPeriod);

        clock.Now = new DateTimeOffset(april.FirstDay.AddDays(4).ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

        Assert.True(restored.Settle());
        var moved = Assert.Single(restored.HistoryOf(restored.AccountNamed("Broker")!).OfType<Movement>(), m => m.Date == april.FirstDay);
        Assert.Equal(Money.FromCents(20000), moved.Amount);
        Assert.False(restored.Settle());
    }

    /// <summary>
    /// Since the sweep: a destination, a period that ended with one category backed and was swept,
    /// a period that ended before the ledger was made, a late expense that made a sweep too large,
    /// and a take-back capped by what was there, so that part of it was let go.
    /// </summary>
    [Fact]
    public void Everything_the_sweep_keeps_comes_back_from_the_format_exactly()
    {
        var ledger = new Ledger(clock, "Bank");
        ledger.AddAccount("Deposit", 0m);
        ledger.AddCategory("Groceries");
        ledger.AddCategory("Savings");
        ledger.SetBacking("Savings", ledger.AccountNamed("Deposit"));
        ledger.SetSweepDestination("Savings");
        ledger.RecordIncome(1000m, "Salaris", Today);
        var march = ledger.CurrentPeriod;

        clock.Now = clock.Now.AddMonths(1);
        ledger.Settle();
        ledger.RecordExpense(990m, "Savings", ledger.Today, "Fiets", ledger.AccountNamed("Deposit"));
        ledger.RecordExpense(40m, "Groceries", march.LastDay, "Bon");
        var brought = ledger.BringUpToDate(march);
        Assert.Equal(Money.FromCents(3000), brought.LetGo);

        var text = LedgerJson.Write(ledger.ToSnapshot());
        Assert.Contains("\"sweptFor\": \"2026-03-01\"", text);
        Assert.Contains("\"reason\": \"swept\"", text);
        var restored = Ledger.FromSnapshot(LedgerJson.Read(text)!, clock);

        Assert.Equal(text, LedgerJson.Write(restored.ToSnapshot()));
        Assert.Equal("Savings", restored.SweepDestination?.Name);
        var line = restored.SweepLineFor(march)!;
        Assert.Equal(SweepLineKind.Swept, line.Kind);
        Assert.Equal(Money.FromCents(99000), Assert.Single(line.Parts).Amount);
        Assert.Equal(Money.FromCents(96000), restored.PeriodLeftover(march));
    }

    // Holiday was backed when March ended, with 100 of its Budget unspent, so it was not swept. Were
    // the period-end record not kept, March would count Holiday as unbacked after starting again,
    // show its 100 as still to sweep, and sweep it a second time.
    [Fact]
    public void Which_categories_were_backed_when_a_period_ended_comes_back_from_the_format()
    {
        var ledger = new Ledger(clock, "Bank");
        var deposit = ledger.AddAccount("Deposit", 0m).Account!;
        ledger.AddCategory("Savings");
        ledger.AddCategory("Holiday");
        ledger.SetBacking("Savings", deposit);
        ledger.SetBacking("Holiday", deposit);
        ledger.SetSweepDestination("Savings");
        ledger.RecordIncome(1000m, "Salaris", Today);
        ledger.Assign(100m, "Holiday", ledger.CurrentPeriod);
        var march = ledger.CurrentPeriod;
        clock.Now = clock.Now.AddMonths(1);
        ledger.Settle();

        var text = LedgerJson.Write(ledger.ToSnapshot());
        Assert.Contains("\"periodEnds\"", text);
        var restored = Ledger.FromSnapshot(LedgerJson.Read(text)!, clock);

        var line = restored.SweepLineFor(march)!;
        Assert.Equal((SweepLineKind.Swept, Money.FromCents(90000), false), (line.Kind, line.Amount, line.CanBringUpToDate));
        Assert.Equal(Money.FromCents(90000), restored.PeriodLeftover(march));
    }

    [Fact]
    public void An_entry_recorded_after_loading_gets_an_id_never_issued_before()
    {
        var restored = Ledger.FromSnapshot(Everything().ToSnapshot(), clock);

        var next = restored.RecordExpense(1m, "Food", Today).Expense!;

        // Coffee was 14, and removed: 14 is still never issued again.
        Assert.Equal(15, next.Id);
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
        var text = LedgerJson.Write(new Ledger(clock, "Bank").ToSnapshot());

        var restored = Ledger.FromSnapshot(LedgerJson.Read(text)!, clock);

        Assert.Empty(restored.CategoriesOffered);
        Assert.Empty(restored.ArchivedCategories);
    }

    public static TheoryData<string, string> Unreadable => new()
    {
        { "blank", "" },
        { "only whitespace", "  \n\t " },
        { "cut off", "{ \"format\": \"MoneyBud\", \"version\": 5, \"categ" },
        { "not an object", "[]" },
        { "another format", Valid().Replace("\"MoneyBud\"", "\"SomethingElse\"") },
        { "a newer version", Valid().Replace("\"version\": 5", "\"version\": 6") },
        { "version 4 with repeats, which version 4 never wrote", Valid().Replace("\"version\": 5", "\"version\": 4") },
        { "version 3, from before the sweep", Valid().Replace("\"version\": 5", "\"version\": 3") },
        { "version 2, from before backing", Valid().Replace("\"version\": 5", "\"version\": 2") },
        { "version 1, from before accounts", Valid().Replace("\"version\": 5", "\"version\": 1") },
        { "version 0", Valid().Replace("\"version\": 5", "\"version\": 0") },
        { "no version", Valid().Replace("\"version\": 5,", "") },
        { "no repeats", Valid().Replace("\"repeats\"", "\"herhalingen\"") },
        { "a repeat without its next date", Valid().Replace("\"next\"", "\"volgende\"") },
        { "a frequency this version does not know", Valid().Replace("\"frequency\": \"monthly\"", "\"frequency\": \"jaarlijks\"") },
        { "no sweep destination", Valid().Replace("\"sweepDestination\"", "\"restantNaar\"") },
        { "no period ends", Valid().Replace("\"periodEnds\"", "\"periodeEindes\"") },
        { "nothing let go", Valid().Replace("\"letGo\"", "\"losgelaten\"") },
        { "a movement without the period it was swept for", Valid().Replace("\"sweptFor\"", "\"geveegdVoor\"") },
        { "no movements", Valid().Replace("\"movements\"", "\"bewegingen\"") },
        { "no day settled through", Valid().Replace("\"settledThrough\"", "\"verrekendTot\"") },
        { "a category without its backing", Valid().Replace("\"backing\": null", "\"steun\": null") },
        { "a backing without its marks", Valid().Replace("\"hereSince\"", "\"hierSinds\"") },
        { "a movement for a reason this version does not know", Valid().Replace("\"reason\": \"backed\"", "\"reason\": \"geveegd\"") },
        { "a movement in a direction written as a number", Valid().Replace("\"direction\": \"in\"", "\"direction\": 0") },
        { "no accounts", Valid().Replace("\"accounts\"", "\"rekeningen\"") },
        { "no pool account", Valid().Replace("\"poolAccount\"", "\"hoofdrekening\"") },
        { "entries naming no account", Valid().Replace("\"account\":", "\"rekening\":") },
        { "no transfers", Valid().Replace("\"transfers\"", "\"overboekingen\"") },
        { "no balance corrections", Valid().Replace("\"balanceCorrections\"", "\"correcties\"") },
        { "no categories", Valid().Replace("\"categories\"", "\"kategorien\"") },
        { "categories not a list", Valid().Replace("\"budgets\": [", "\"budgets\": {").Replace("\n  ],\n  \"expenses\"", "\n  },\n  \"expenses\"") },
        { "a fraction of a cent", Valid().Replace("\"cents\": 3215", "\"cents\": 3215.5") },
        { "cents as text", Valid().Replace("\"cents\": 3215", "\"cents\": \"3215\"") },
        { "a date in another form", Valid().Replace("\"2026-03-15\"", "\"15-03-2026\"") },
        { "an income without a label", Valid().Replace("\"label\": \"Salaris\"", "\"label\": null") },
    };

    [Theory]
    [MemberData(nameof(Unreadable))]
    public void Anything_but_a_whole_version_5_document_or_a_version_4_one_cannot_be_read(string what, string text)
    {
        _ = what;
        Assert.NotEqual(Valid(), text);
        Assert.Null(LedgerJson.Read(text));
    }

    private static string Valid()
    {
        var ledger = new Ledger(new FixedClock(new DateTimeOffset(Today.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero)), "Bank");
        ledger.AddCategory("Groceries");
        ledger.RecordIncome(1832.45m, "Salaris", Today);
        ledger.Assign(400m, "Groceries", ledger.CurrentPeriod);
        ledger.RecordExpense(32.15m, "Groceries", Today, "Albert Heijn");
        ledger.RecordExpense(9.99m, "Groceries", Today, "Abonnement", repeat: Frequency.Monthly);
        ledger.AddAccount("Deposit", 0m);
        ledger.AddCategory("Savings");
        ledger.Assign(100m, "Savings", ledger.CurrentPeriod);
        ledger.SetBacking("Savings", ledger.AccountNamed("Deposit"));
        var text = LedgerJson.Write(ledger.ToSnapshot());
        Assert.NotNull(LedgerJson.Read(text));
        Assert.Contains("\"backing\": null", text);
        Assert.Contains("\"reason\": \"backed\"", text);
        Assert.Contains("\"direction\": \"in\"", text);
        Assert.Contains("\"frequency\": \"monthly\"", text);
        return text;
    }

    // ------------------------------------------------------------------ repeats (ADR 0011)

    // One running on the 31st and clamped to 30 April, one weekly, one stopped. After 30 April no
    // date says the 31st, so only the kept day brings May's back to it.
    [Fact]
    public void Repeats_come_back_from_the_format_with_their_day_their_next_date_and_whether_they_were_stopped()
    {
        var march31 = new DateOnly(2026, 3, 31);
        var clock31 = new FixedClock(new DateTimeOffset(march31.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero));
        var ledger = new Ledger(clock31, "Bank");
        ledger.AddCategory("Rent");
        ledger.RecordExpense(900m, "Rent", march31, "Huur", repeat: Frequency.Monthly);
        ledger.RecordIncome(85m, "Bijbaan", march31, repeat: Frequency.Weekly);
        var netflix = ledger.RecordExpense(13.99m, "Rent", march31, "Netflix", repeat: Frequency.Monthly).Expense!;
        ledger.ChangeExpense(netflix, 13.99m, "Rent", march31, "Netflix", null, null);
        clock31.Now = new DateTimeOffset(new DateTime(2026, 4, 30, 12, 0, 0), TimeSpan.Zero);
        ledger.Settle();

        var text = LedgerJson.Write(ledger.ToSnapshot());
        Assert.Contains("\"day\": 31", text);
        Assert.Contains("\"next\": \"2026-05-31\"", text);
        Assert.Contains("\"frequency\": \"weekly\"", text);
        Assert.Contains("\"frequency\": null", text);
        var restored = Ledger.FromSnapshot(LedgerJson.Read(text)!, clock31);

        Assert.Equal(text, LedgerJson.Write(restored.ToSnapshot()));
        clock31.Now = new DateTimeOffset(new DateTime(2026, 5, 31, 12, 0, 0), TimeSpan.Zero);
        restored.Settle();
        var may = restored.Calendar.PeriodContaining(new DateOnly(2026, 5, 1));
        Assert.Equal([new DateOnly(2026, 5, 31)], restored.ExpensesIn(may).Select(e => e.Date));
        Assert.Equal(Frequency.Monthly, restored.FrequencyOf(restored.ExpensesIn(may)[0]));
    }

    // Version 4 had no repeats, so reading it guesses nothing: every entry is a one-off (plan for
    // increment 12, D2).
    [Fact]
    public void A_version_4_document_is_read_as_data_with_no_repeats()
    {
        var ledger = new Ledger(clock, "Bank");
        ledger.AddCategory("Groceries");
        ledger.RecordExpense(32.15m, "Groceries", Today, "Albert Heijn");
        var written = LedgerJson.Write(ledger.ToSnapshot());
        var version4 = Regex.Replace(written, @",\s*""repeats"": \[\]", "").Replace("\"version\": 5", "\"version\": 4");
        Assert.DoesNotContain("repeats", version4);

        var read = LedgerJson.Read(version4);

        Assert.NotNull(read);
        Assert.Empty(read.Repeats);
        var restored = Ledger.FromSnapshot(read, clock);
        Assert.Null(restored.FrequencyOf(restored.ExpensesIn(restored.CurrentPeriod)[0]));
        Assert.Contains("\"version\": 5", LedgerJson.Write(restored.ToSnapshot()));
    }

    // ------------------------------------------------------------------ the rules kept data is read against

    public static TheoryData<string, LedgerSnapshot> Broken
    {
        get
        {
            CategorySnapshot groceries = new(1, "Groceries", false);
            AccountSnapshot bank = new(1, "Bank");
            AccountSnapshot cash = new(2, "Cash");
            var march = new DateOnly(2026, 3, 1);
            ExpenseSnapshot expense = new(1, Money.FromCents(100), Today, 1, "Kiosk", 1);
            IncomeSnapshot income = new(2, Money.FromCents(100), Today, "Salaris", 1);

            LedgerSnapshot With(
                IReadOnlyList<CategorySnapshot>? categories = null, IReadOnlyList<BudgetSnapshot>? budgets = null,
                IReadOnlyList<ExpenseSnapshot>? expenses = null, IReadOnlyList<IncomeSnapshot>? incomes = null,
                int last = 2, IReadOnlyList<AccountSnapshot>? accounts = null, int pool = 1,
                IReadOnlyList<TransferSnapshot>? transfers = null,
                IReadOnlyList<BalanceCorrectionSnapshot>? corrections = null,
                IReadOnlyList<MovementSnapshot>? movements = null, int? destination = null,
                IReadOnlyList<PeriodEndSnapshot>? periodEnds = null, IReadOnlyList<LetGoSnapshot>? letGo = null,
                IReadOnlyList<RepeatSnapshot>? repeats = null) =>
                new(categories ?? [groceries], budgets ?? [], expenses ?? [], incomes ?? [], last,
                    accounts ?? [bank, cash], pool, transfers ?? [], corrections ?? [], movements ?? [], Today,
                    destination, periodEnds ?? [], letGo ?? [], repeats ?? []);

            MovementSnapshot movement = new(1, Today, 1, 1, 2, Money.FromCents(100), MovementReason.Assigned, MovementDirection.In);
            BackingSnapshot backing = new(2, new EntryMark(Today, 1), new EntryMark(Today, 1));
            var february = new DateOnly(2026, 2, 1);
            MovementSnapshot sweep = movement with { Reason = MovementReason.Swept, SweptFor = february };
            RepeatSnapshot monthly = new([1], Frequency.Monthly, 15, Today.AddMonths(1));

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
                { "an income without a label", With(incomes: [income with { Label = "" }]) },
                { "an id used twice", With(expenses: [expense], incomes: [income with { Id = 1 }]) },
                { "an id used by a transfer and an expense", With(expenses: [expense], transfers: [new(1, Money.FromCents(100), Today, 1, 2)]) },
                { "an id used by a balance correction and an income", With(incomes: [income], corrections: [new(2, Today, 1, Money.Zero, false)]) },
                { "an account name with space at its end", With(accounts: [new(1, "Bank ")]) },
                { "an empty account name", With(accounts: [new(1, "")]) },
                { "two account names the rule counts as one", With(accounts: [bank, new(2, "BANK")]) },
                { "an account key used twice", With(accounts: [bank, new(1, "Cash")]) },
                { "no pool account", With(accounts: []) },
                { "a pool account that is no account", With(pool: 9) },
                { "an expense on no account", With(expenses: [expense with { Account = 9 }]) },
                { "an income on no account", With(incomes: [income with { Account = 9 }]) },
                { "a transfer from an account to itself", With(transfers: [new(1, Money.FromCents(100), Today, 1, 1)]) },
                { "a transfer to no account", With(transfers: [new(1, Money.FromCents(100), Today, 1, 9)]) },
                { "a transfer of zero", With(transfers: [new(1, Money.Zero, Today, 1, 2)]) },
                { "a balance correction on no account", With(corrections: [new(1, Today, 9, Money.Zero, false)]) },
                { "two starting balances for one account", With(corrections: [new(1, Today, 1, Money.Zero, true), new(2, Today, 1, Money.Zero, true)]) },
                { "a balance correction id never issued", With(corrections: [new(3, Today, 1, Money.Zero, false)]) },
                { "an id never issued", With(expenses: [expense with { Id = 3 }]) },
                { "an id of zero", With(expenses: [expense with { Id = 0 }]) },
                { "a last id below zero", With(last: -1) },
                { "a movement for no category", With(movements: [movement with { Category = 9 }]) },
                { "a movement from no account", With(movements: [movement with { From = 9 }]) },
                { "a movement of zero", With(movements: [movement with { Amount = Money.Zero }]) },
                { "an id used by a movement and an expense", With(expenses: [expense], movements: [movement]) },
                { "a backing movement going out", With(movements: [movement with { Reason = MovementReason.Backed, Direction = MovementDirection.Out }]) },
                { "an unbacking movement going in", With(movements: [movement with { Reason = MovementReason.Unbacked }]) },
                { "an assignment moved along", With(movements: [movement with { Direction = MovementDirection.Along }]) },
                { "a re-pointing from an account to itself", With(movements: [movement with { To = 1, Reason = MovementReason.Repointed, Direction = MovementDirection.Along }]) },
                { "a backing by no account", With(categories: [groceries with { Backing = backing with { Account = 9 } }], last: 3) },
                { "a backing marked with an entry's id", With(categories: [groceries with { Backing = backing }], expenses: [expense]) },
                { "a backing marked with an id never issued", With(categories: [groceries with { Backing = backing with { HereSince = new EntryMark(Today, 5) } }]) },
                { "a sweep for no period", With(movements: [sweep with { SweptFor = null }]) },
                { "an assignment for a period", With(movements: [movement with { SweptFor = february }]) },
                { "a sweep moved along", With(movements: [sweep with { Direction = MovementDirection.Along }]) },
                { "a sweep for a day that starts no period", With(movements: [sweep with { SweptFor = february.AddDays(3) }]) },
                { "a sweep destination that is no category", With(destination: 9) },
                { "a sweep destination that is not backed", With(destination: 1) },
                { "an archived sweep destination", With(categories: [groceries with { IsArchived = true, Backing = backing with { AccumulatingSince = new EntryMark(Today, 3), HereSince = new EntryMark(Today, 3) } }], last: 3, destination: 1) },
                { "a period end for a day that starts no period", With(periodEnds: [new(february.AddDays(3), [])]) },
                { "a period end for a period not yet ended", With(periodEnds: [new(new DateOnly(2026, 3, 1), [])]) },
                { "a period that ended twice", With(periodEnds: [new(february, []), new(february, [])]) },
                { "a period end naming no category", With(periodEnds: [new(february, [9])]) },
                { "an amount let go of zero", With(movements: [sweep], letGo: [new(february, Money.Zero)]) },
                { "an amount let go for a period nothing was swept for", With(letGo: [new(february, Money.FromCents(100))]) },
                { "a repeat with no occurrences", With(expenses: [expense], repeats: [monthly with { Occurrences = [] }]) },
                { "a repeat of no entry", With(expenses: [expense], repeats: [monthly with { Occurrences = [9] }]) },
                { "a repeat of a transfer", With(transfers: [new(1, Money.FromCents(100), Today, 1, 2)], repeats: [monthly]) },
                { "a repeat of an expense and an income", With(expenses: [expense], incomes: [income], repeats: [monthly with { Occurrences = [1, 2] }]) },
                { "an entry in two repeats", With(expenses: [expense], repeats: [monthly, monthly]) },
                { "an entry twice in one repeat", With(expenses: [expense], repeats: [monthly with { Occurrences = [1, 1] }]) },
                { "a running repeat with no next date", With(expenses: [expense], repeats: [monthly with { Next = null }]) },
                { "a stopped repeat with a next date", With(expenses: [expense], repeats: [monthly with { Frequency = null, Day = null }]) },
                { "a monthly repeat with no day", With(expenses: [expense], repeats: [monthly with { Day = null }]) },
                { "a monthly repeat on the 32nd", With(expenses: [expense], repeats: [monthly with { Day = 32 }]) },
                { "a weekly repeat with a day", With(expenses: [expense], repeats: [monthly with { Frequency = Frequency.Weekly }]) },
                { "a stopped repeat with a day", With(expenses: [expense], repeats: [new([1], null, 15, null)]) },
                { "a frequency MoneyBud does not know", With(expenses: [expense], repeats: [monthly with { Frequency = (Frequency)7 }]) },
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
            [new(1, "Groceries", false)], [new(1, new DateOnly(9999, 12, 1), Money.FromCents(100))], [], [], 0,
            [new(1, "Bank")], 1, [], [], [], Today, null, [], [], []);

        Assert.Throws<InvalidDataException>(() => Ledger.FromSnapshot(snapshot, clock));
    }

    // An expense cannot be recorded in the future, but one recorded today is still valid kept data
    // if the machine's clock is later turned back.
    [Fact]
    public void An_expense_dated_after_today_is_still_valid_kept_data()
    {
        var snapshot = new LedgerSnapshot(
            [new(1, "Groceries", false)], [], [new(1, Money.FromCents(100), Today.AddDays(5), 1, null, 1)], [], 1,
            [new(1, "Bank")], 1, [], [], [], Today, null, [], [], []);

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
        Assert.Throws<InvalidOperationException>(() => store.TrySave(new Ledger(clock, "Bank").ToSnapshot()));
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
        var ledger = new Ledger(clock, "Bank");
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

        Assert.False(store.TrySave(new Ledger(clock, "Bank").ToSnapshot()));

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

        Assert.True(store.TrySave(new Ledger(clock, "Bank").ToSnapshot()));
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
        var ledger = new Ledger(clock, "Bank");
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
        var ledger = new Ledger(clock, "Bank");
        var app = new MoneyBudApp(ledger, store);
        app.AddCategory("Groceries");

        app.Close();

        Assert.Equal(1, store.Saves);
        Assert.True(store.Disposed);
    }

    // Backing (ADR 0009): money planned for a period that has begun moves when MoneyBud opens and on
    // the minute's tick, and what moved is kept; a tick with nothing to move keeps nothing.
    [Fact]
    public void Money_moved_on_opening_or_on_a_tick_is_kept_and_a_tick_with_nothing_to_move_keeps_nothing()
    {
        var store = new CountingStore();
        var ledger = new Ledger(clock, "Bank");
        ledger.AddCategory("Savings");
        var deposit = ledger.AddAccount("Deposit", 0m).Account!;
        ledger.SetBacking("Savings", deposit);
        var april = ledger.Calendar.Next(ledger.CurrentPeriod);
        var may = ledger.Calendar.Next(april);
        ledger.Assign(40m, "Savings", april);
        ledger.Assign(60m, "Savings", may);

        clock.Now = Noon(april.FirstDay);
        var app = new MoneyBudApp(ledger, store);
        Assert.Equal(1, store.Saves);

        app.Tick();
        Assert.Equal(1, store.Saves);

        clock.Now = Noon(may.FirstDay);
        app.Tick();
        Assert.Equal(2, store.Saves);
        Assert.Equal(Money.FromCents(10000), ledger.BalanceOf(deposit));
    }

    // A list on screen writes back what it shows after every redraw. Choosing the backing already
    // set must leave the screen as the act before it left it: nothing said, nothing kept.
    [Fact]
    public void Choosing_the_backing_already_set_says_nothing_keeps_nothing_and_leaves_what_was_said()
    {
        var store = new CountingStore();
        var ledger = new Ledger(clock, "Bank");
        ledger.AddCategory("Savings");
        var deposit = ledger.AddAccount("Deposit", 0m).Account!;
        var app = new MoneyBudApp(ledger, store);
        app.SetBacking("Savings", deposit);
        var said = app.Notice;
        Assert.Equal(1, store.Saves);

        var row = Assert.Single(app.Overview.Rows);
        row.ChosenBacking = row.ChosenBacking;
        row.ChosenBacking = null;
        app.SetBacking("Savings", deposit);

        Assert.Same(said, app.Notice);
        Assert.Equal(1, store.Saves);
    }

    private static DateTimeOffset Noon(DateOnly day) => new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

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
