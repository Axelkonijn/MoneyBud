using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Storage;

namespace MoneyBud.Specs.Support;

/// <summary>
/// One scenario's world: a ledger with a controllable clock, and whatever came of the last thing
/// the scenario's <c>When</c> steps asked MoneyBud to do. Reqnroll creates one per scenario, so
/// scenarios cannot leak into each other.
///
/// <para>The ledger starts <b>empty</b> — no default categories. Every scenario names the
/// categories it needs with an explicit <c>Given</c> (features/add-category.feature), and
/// starting empty is what makes that claim checked rather than hoped: a scenario that leaned on
/// a default would fail. The scenarios that are about the first start say so, and
/// <see cref="StartUsingMoneyBudForTheFirstTime"/> replaces the ledger with one that has
/// them.</para>
///
/// <para><b>Every scenario keeps its data, for real.</b> The screen saves through the real
/// <see cref="FileLedgerStore"/>, in a folder of the scenario's own under the machine's temporary
/// folder — never the user's profile, and never the repository — removed when the scenario ends.
/// What the <c>Given</c> steps set up counts as kept (features/keep-data.feature): it is saved
/// when the screen first opens, and again whenever a step needs it kept before something
/// happens, such as saving becoming impossible.</para>
/// </summary>
public sealed class SpecContext : IDisposable
{
    /// <summary>
    /// The day every scenario starts on. Mid-month, so that "today" is neither the first nor the
    /// last day of a period, and with February as the previous month, so a period of a length
    /// other than 31 days is exercised by the boundary scenarios.
    /// </summary>
    public static readonly DateOnly Today = new(2026, 3, 15);

    private readonly FixedClock clock = new(Noon(Today));

    private MoneyBudApp? app;
    private SpecStore? store;
    private bool closed;

    public SpecContext() => Ledger = new Ledger(clock);

    public Ledger Ledger { get; private set; }

    /// <summary>
    /// MoneyBud as the user meets it: the screen over <see cref="Ledger"/>. Every <c>When</c>
    /// step acts through this, never on the ledger directly, so each scenario — the domain ones
    /// included — goes through the same doors the Desktop uses. <c>Given</c> steps set up the
    /// ledger directly: setting up is not what is under test.
    ///
    /// <para>Made on first use, so that it opens on whatever period is current once the Givens
    /// have fixed today, as MoneyBud opens on the current period when it starts. What the Givens
    /// set up is kept at that moment, as data MoneyBud already had.</para>
    /// </summary>
    public MoneyBudApp App => app ??= OpenOverGivens();

    private MoneyBudApp OpenOverGivens()
    {
        if (closed)
            throw new InvalidOperationException("MoneyBud was closed; a step has to start it again.");

        store = new SpecStore(new FileLedgerStore(Folder));
        Assert.Equal(Claim.Claimed, store.TryClaim());
        Assert.True(store.TrySave(Ledger.ToSnapshot()), "Keeping what the Givens set up failed.");
        return new MoneyBudApp(Ledger, store);
    }

    /// <summary>
    /// A MoneyBud used for the first time, through the same door the app will use.
    ///
    /// <para>This replaces the ledger, so anything an earlier step set up would be lost without a
    /// word. A first start comes first, so an earlier setup means the scenario is written in the
    /// wrong order — and that fails here rather than silently.</para>
    /// </summary>
    public void StartUsingMoneyBudForTheFirstTime()
    {
        if (Ledger.CategoriesOffered.Count > 0 || Ledger.ArchivedCategories.Count > 0 || app is not null)
            throw new InvalidOperationException(
                "Starting MoneyBud for the first time must come before any other setup.");

        Ledger = Ledger.StartNew(clock);
    }

    // ------------------------------------------------------------------ keeping

    /// <summary>
    /// The scenario's own data folder: under the machine's temporary folder, so that no scenario
    /// ever reads or writes the user's real data, and removed with the scenario.
    /// </summary>
    public string Folder { get; } = Path.Combine(Path.GetTempPath(), "MoneyBud.Specs", Guid.NewGuid().ToString("N"));

    public string DataFile => Path.Combine(Folder, FileLedgerStore.DataFileName);

    private string TemporaryFile => Path.Combine(Folder, FileLedgerStore.TemporaryFileName);

    /// <summary>What came of the last start, by <see cref="Start"/>. Null until one was made.</summary>
    public StartResult? LastStart { get; private set; }

    /// <summary>
    /// Keeps whatever the Givens have set up by now, with the screen open — the setup of a
    /// scenario whose next step is about what is kept from here on.
    /// </summary>
    public void KeepGivens()
    {
        _ = App;
        Assert.True(store!.TrySave(Ledger.ToSnapshot()), "Keeping what the Givens set up failed.");
    }

    /// <summary>
    /// Makes every save fail from now on, for real: a folder stands where the save writes its
    /// temporary file, so opening that file fails the way a full disk or a missing profile would.
    /// Nothing in MoneyBud knows it is being tested.
    /// </summary>
    public void BlockSaving() => Directory.CreateDirectory(TemporaryFile);

    public void UnblockSaving() => Directory.Delete(TemporaryFile);

    /// <summary>The user closes MoneyBud's window. Closing asks nothing, so this returns.</summary>
    public void Close()
    {
        (app ?? throw new InvalidOperationException("MoneyBud is not open.")).Close();
        app = null;
        store = null;
        closed = true;
    }

    /// <summary>
    /// Starts MoneyBud as the Desktop does, on this scenario's folder. When it opens, it becomes
    /// the screen every later step uses, over the ledger it loaded.
    /// </summary>
    public StartResult Start()
    {
        var started = MoneyBudStart.Start(new SpecStore(new FileLedgerStore(Folder)), clock);
        LastStart = started;

        if (started is StartResult.Opened opened)
        {
            if (app is not null)
                throw new InvalidOperationException("A second MoneyBud opened while one was open.");

            app = opened.App;
            Ledger = app.Ledger;
            closed = false;
        }

        return started;
    }

    /// <summary>
    /// A second start while this MoneyBud is open, which is expected to be refused. Leaves the
    /// open one as it was.
    /// </summary>
    public StartResult StartAgainWhileOpen()
    {
        _ = App;
        return MoneyBudStart.Start(new FileLedgerStore(Folder), clock);
    }

    /// <summary>Closes MoneyBud, moves the clock if asked, and starts it again. It must open.</summary>
    public void Restart(DateOnly? on = null)
    {
        Close();
        if (on is { } day) SetToday(day);
        Assert.IsType<StartResult.Opened>(Start());
    }

    /// <summary>
    /// MoneyBud stops in the middle of saving the change just made, without closing.
    ///
    /// <para>The save of that change has already run, so this puts the folder in the state a
    /// save cut off part-way leaves behind, by <see cref="FileLedgerStore"/>'s own design: the
    /// kept file exactly as it was before the save began, since the save never writes it in place,
    /// and half of the new one in the temporary file, which is where the cut-off save was writing.
    /// Then MoneyBud is gone without closing: no last try at saving, and the hold on the data let go
    /// as the operating system lets go of a process that has stopped. The guarantee itself — that
    /// a save never writes the kept file in place — is <see cref="FileLedgerStore"/>'s, and its unit
    /// tests hold it.</para>
    /// </summary>
    public void InterruptTheLastSave()
    {
        var kept = store ?? throw new InvalidOperationException("MoneyBud is not open.");
        var written = File.ReadAllBytes(DataFile);

        if (kept.BeforeLastSave is { } before) File.WriteAllBytes(DataFile, before);
        else File.Delete(DataFile);
        File.WriteAllBytes(TemporaryFile, written[..(written.Length / 2)]);

        kept.Dispose();
        app = null;
        store = null;
        closed = true;
    }

    public void Dispose()
    {
        app?.Close();
        try
        {
            if (Directory.Exists(Folder)) Directory.Delete(Folder, recursive: true);
        }
        catch (IOException)
        {
            // A folder left in the machine's temporary folder is harmless; a failing scenario is not.
        }
    }

    /// <summary>
    /// The real store, noting what the kept file held before each save, so that
    /// <see cref="InterruptTheLastSave"/> can put it back.
    /// </summary>
    private sealed class SpecStore(FileLedgerStore inner) : ILedgerStore
    {
        public byte[]? BeforeLastSave { get; private set; }

        public Claim TryClaim() => inner.TryClaim();

        public LoadResult Load() => inner.Load();

        public bool TrySave(LedgerSnapshot snapshot)
        {
            var before = File.Exists(inner.DataFile) ? File.ReadAllBytes(inner.DataFile) : null;
            var saved = inner.TrySave(snapshot);
            if (saved) BeforeLastSave = before;
            return saved;
        }

        public void Dispose() => inner.Dispose();
    }

    /// <summary>What came of the last attempt to record an expense. Null until one was made.</summary>
    public RecordExpenseResult? LastExpenseResult { get; private set; }

    /// <summary>What came of the last attempt to record an income. Null until one was made.</summary>
    public RecordIncomeResult? LastIncomeResult { get; private set; }

    /// <summary>
    /// Whichever attempt came last — an expense, an income, an assignment, adding a category or
    /// archiving one — as the result it was.
    ///
    /// <para>Needed because a few steps are worded without naming what was attempted, on purpose:
    /// "I should not be warned or asked to confirm" is a claim about MoneyBud rather than about
    /// expenses, the cent rule is stated by arc42 §8.2 about <i>amounts</i>, and "I should be
    /// told that X was brought back" follows adding a name and recording an expense alike.
    /// Several feature files use those sentences, so one step definition has to serve them
    /// all.</para>
    ///
    /// <para><b>Last, rather than whichever is present.</b> Setup steps call the ledger directly
    /// and never come through here, so today only a <c>When</c> ever sets this and presence would
    /// in fact have been enough. Order is the stronger rule and stops being equivalent the moment
    /// one scenario's <c>When</c> steps record an income and an expense — which is also this
    /// design's limit: such a scenario would assert against whichever came last and say nothing
    /// about the other. No feature file does that yet, and a step that needs to name one uses
    /// <see cref="ExpenseResult"/> or <see cref="IncomeResult"/> instead.</para>
    /// </summary>
    public object? LastAttempt { get; private set; }

    /// <summary>What came of the last attempt to add a category. Null until one was made.</summary>
    public AddCategoryResult? LastAddResult { get; private set; }

    /// <summary>What came of the last attempt to assign. Null until one was made.</summary>
    public AssignResult? LastAssignResult { get; private set; }

    /// <summary>The category the last archive put away. Null until one was archived.</summary>
    public Category? LastArchived { get; private set; }

    public RecordExpenseResult ExpenseResult =>
        LastExpenseResult ?? throw new InvalidOperationException("No expense has been recorded yet.");

    public RecordIncomeResult IncomeResult =>
        LastIncomeResult ?? throw new InvalidOperationException("No income has been recorded yet.");

    public AssignResult AssignResult =>
        LastAssignResult ?? throw new InvalidOperationException("Nothing has been assigned yet.");

    public void Record(RecordExpenseResult result)
    {
        LastExpenseResult = result;
        LastAttempt = result;
    }

    public void Record(RecordIncomeResult result)
    {
        LastIncomeResult = result;
        LastAttempt = result;
    }

    public void Record(AddCategoryResult result)
    {
        LastAddResult = result;
        LastAttempt = result;
    }

    public void Record(AssignResult result)
    {
        LastAssignResult = result;
        LastAttempt = result;
    }

    public void RecordArchived(Category category)
    {
        LastArchived = category;
        LastAttempt = new Archived(category);
    }

    public void Record(ChangeExpenseResult result) => LastAttempt = result;

    public void Record(ChangeIncomeResult result) => LastAttempt = result;

    public void Record(RenameCategoryResult result) => LastAttempt = result;

    public void RecordDeleted(Category category) => LastAttempt = new Deleted(category);

    /// <summary>
    /// An entry was removed, after the user confirmed. <paramref name="What"/> is "expense" or
    /// "income"; <paramref name="Asked"/> and <paramref name="Said"/> are the question put first
    /// and what MoneyBud said afterwards.
    /// </summary>
    public sealed record Removed(string What, string Asked, string Said);

    /// <summary>The user was asked whether to remove an entry, and said no.</summary>
    public sealed record Declined(string What);

    public void RecordRemoved(string what, string asked, string said) => LastAttempt = new Removed(what, asked, said);

    public void RecordDeclined(string what) => LastAttempt = new Declined(what);

    /// <summary>
    /// Whether the last removal asked first: a question was waiting while the entry was still
    /// there. Null until something was removed or declined.
    /// </summary>
    public bool? AskedFirst { get; set; }

    /// <summary>Deleting answers with the category it deleted, so this wraps it, as <see cref="Archived"/> does.</summary>
    public sealed record Deleted(Category Category);

    /// <summary>
    /// An entry whose typed amount the screen could not read, so the ledger was never asked
    /// (features/type-an-amount.feature). <paramref name="What"/> is "expense", "income",
    /// "assignment" or "change".
    /// </summary>
    public sealed record Unread(string Typed, string What);

    public void RecordUnread(string typed, string what) => LastAttempt = new Unread(typed, what);

    /// <summary>Moves the day the ledger considers today. See <see cref="FixedClock"/>.</summary>
    public void SetToday(DateOnly day) => clock.Now = Noon(day);

    /// <summary>
    /// Does something as it would have been done on another day, then puts the clock back
    /// exactly where it was.
    ///
    /// <para>This is how a scenario gets a budget in a past period. Assigning in a past period is
    /// refused (arc42 §12), and a Given that says "I have a budget of 400 euro in the previous
    /// budget period" means it was assigned back when that period was current — so that is when
    /// the setup assigns it, through the same door as every other assignment, rather than through
    /// a setter that could make states the rules forbid.</para>
    /// </summary>
    public T AsIfToday<T>(DateOnly day, Func<T> action)
    {
        var now = clock.Now;
        clock.Now = Noon(day);
        try
        {
            return action();
        }
        finally
        {
            clock.Now = now;
        }
    }

    /// <summary>
    /// Archiving answers with the category it put away rather than with a result type — it has
    /// one outcome — so this wraps it, to give <see cref="LastAttempt"/> something to tell apart
    /// from a <see cref="Category"/> handed back for some other reason.
    /// </summary>
    public sealed record Archived(Category Category);

    /// <summary>Midday, so that no scenario sits within hours of a day boundary.</summary>
    private static DateTimeOffset Noon(DateOnly day) =>
        new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
}
