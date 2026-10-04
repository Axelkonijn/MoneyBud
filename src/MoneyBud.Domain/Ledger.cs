namespace MoneyBud.Domain;

/// <summary>
/// Everything MoneyBud knows: the categories, what was budgeted for each in each period, the
/// expenses recorded against them, and the income recorded into each period.
///
/// <para>The ledger knows nothing of files. What it holds goes out as a <see cref="LedgerSnapshot"/>
/// (<see cref="ToSnapshot"/>) and comes back in as one (<see cref="FromSnapshot"/>); keeping the
/// snapshot between runs is an <see cref="ILedgerStore"/>'s job (arc42 §8.3, ADR 0007).</para>
///
/// <para>The two layers of §12 meet in exactly one place, <see cref="RemainingFor"/>. Budgets are
/// the plan; expenses are the actual; recording an expense never touches a budget.</para>
///
/// <para>Categories are found by name under the category name rule — trimmed, inner whitespace
/// runs counted as one, case ignored (<see cref="CategoryName"/>) — wherever a name is asked
/// about, so every method here that takes a name accepts any spelling the rule matches. A
/// category that is <i>archived</i> is still one of the user's categories: it keeps its budgets and
/// its expenses and every figure below still answers for it. What archiving changes is only
/// whether it is offered for new entry (arc42 §12, *A category is taken out of use, not
/// deleted*).</para>
///
/// <para>A budget is made in exactly one way, by <see cref="Assign"/>, which moves an amount out
/// of a period's <i>Unassigned</i> and into a category's <i>Budget</i>. There is no other way to
/// write a plan.</para>
///
/// <para>An entry — an expense or an income, and likewise a transfer — can be changed or removed in
/// any period, past ones included (arc42 §12, *An entry can be changed or removed*). A change is
/// judged exactly as
/// recording the changed entry now would be, by the same checks, and it <b>overwrites</b> the
/// entry: nothing remembers what it was, and it keeps its place in the order recorded. A category
/// can be renamed, and one with no history in any period can be deleted.</para>
///
/// <para>A period with no plan is offered the latest earlier one (<see cref="PlanOfferedIn"/>),
/// and taking it over assigns it in full (<see cref="TakeOverPlan"/>). The figures are
/// remembered; nothing is assigned until the user takes them over.</para>
///
/// <para><b>Accounts</b> are the location dimension (arc42 §12, <i>Accounts and net worth</i>).
/// Every income and expense is on one, the <i>pool account</i> unless another was chosen, and
/// money moves between them by <see cref="RecordTransfer"/>. A balance is <b>worked out</b>, never
/// stored (<see cref="BalanceOf"/>, ADR 0008), from the account's latest typed balance and whatever
/// is on it that the typed balance does not already have in it. Accounts touch no budget figure:
/// <i>Unassigned</i>, every <i>Budget</i> and every <i>Remaining</i> answer exactly as they did
/// before accounts existed.</para>
///
/// <para><b>Backing</b> (arc42 §12, <i>Backing and Accumulated</i>; ADR 0009) is where the two
/// dimensions meet. A category may be backed by one account (<see cref="SetBacking"/>), and money
/// then really moves for it: assigning moves the amount from the pool account to the backing
/// account, and backing, unbacking and re-pointing move what belongs to the category. Every such
/// move is a stored <see cref="Movement"/>, written on the day the money moves, so a balance is
/// still the sum of what is on the account. Money assigned for a later period moves on that
/// period's first day, written by <see cref="Settle"/>, which every act that changes the ledger
/// calls first. A backed category shows <i>Accumulated</i> (<see cref="AccumulatedFor"/>).</para>
///
/// <para><b>The sweep</b> (arc42 §12, <i>The sweep and Restant</i>; ADR 0010): when a period ends,
/// settling moves its leftover, <i>Unassigned</i> plus every unbacked category's <i>Remaining</i>,
/// from the pool account to the one backed category chosen as the destination
/// (<see cref="SetSweepDestination"/>), as a <see cref="Movement"/> that says which period it was
/// for. It records which categories were backed at that moment, so the leftover can be worked out
/// again, the same way, after every late entry (<see cref="PeriodLeftover"/>). An ended period is
/// never adjusted by itself: its line shows the difference (<see cref="SweepLineFor"/>), and
/// <see cref="BringUpToDate"/> moves it.</para>
///
/// <para><b>Recurring entries</b> (arc42 §12, <i>Recurring entries</i>; ADR 0011): an income or an
/// expense can be set to repeat, weekly or monthly. Each occurrence is an ordinary entry, recorded by
/// settling on its own date, the first time MoneyBud runs on or after it, and kept to be announced
/// (<see cref="TakeOccurrencesMade"/>). The latest occurrence — the one recorded most recently — sets
/// the next; changing it changes what follows, and one-off stops it (<see cref="FrequencyOf"/>,
/// <see cref="SetsTheRepeat"/>).</para>
///
/// <para><b>The period start day</b> (arc42 §12, <i>A configurable period start day</i>; ADR 0012)
/// can be changed at any time, from the current period on (<see cref="ChangeStartDay"/>). The
/// <see cref="Calendar"/> is then a history of changes, and is kept. Everything keyed by a period's
/// first day stays true of the periods that existed when it was written, since no period before the
/// current one moves; only plans made ahead are moved, into the period their old first day falls
/// in.</para>
///
/// <para><b><i>Vrij</i>, and moving <i>Opgebouwd</i></b> (arc42 §12; ADR 0015): every account but the
/// pool account shows the money on it that no category claims (<see cref="UnclaimedOf"/>), worked out
/// like a balance. The user moves an amount of purpose between those, categories' <i>Opgebouwd</i> and
/// the current period's <i>Niet toegewezen</i> (<see cref="Reallocate"/>), as a stored
/// <see cref="Reallocation"/>. Setting a category to "—" sends only the current period's money back and
/// leaves the rest where it is, still the category's (<see cref="LeftBehind"/>); setting an account
/// again carries on. And an expense against a backed category is on its backing account, from the
/// period it got it (<see cref="LockedAccountFor"/>).</para>
/// </summary>
public sealed class Ledger
{
    private readonly TimeProvider clock;
    private readonly Dictionary<string, Category> categories = new(CategoryName.Comparer);
    private readonly List<Category> categoriesInOrderAdded = [];
    private readonly HashSet<Category> archived = [];
    private readonly Dictionary<(Category Category, DateOnly PeriodStart), Money> budgets = [];

    private readonly Dictionary<string, Account> accounts = new(NameRule.Comparer);
    private readonly List<Account> accountsInOrderAdded = [];
    private Account pool = null!;

    // In the order recorded. A change replaces an entry where it stands, so the order stays the
    // order the entries were first recorded in. All five kinds draw their ids from one counter, and
    // so does every backing change, so an id also says which of two was recorded first — across
    // kinds, which is what a balance correction on the day of an entry needs to know (IEntry). Since
    // increment 15 reallocations draw from it too: six kinds.
    private readonly List<Expense> expenses = [];
    private readonly List<Income> incomes = [];
    private readonly List<Transfer> transfers = [];
    private readonly List<BalanceCorrection> balanceCorrections = [];
    private readonly List<Movement> movements = [];
    private readonly List<Reallocation> reallocations = [];
    private int lastEntryId;

    private readonly Dictionary<Category, Backing> backings = [];

    // The categories set to "—" that have a backing in their history, with what they left behind.
    // A category is in at most one of the two.
    private readonly Dictionary<Category, LeftBehind> leftBehind = [];

    // The day money planned for later periods has been moved up to (Settle).
    private DateOnly settledThrough;

    // The sweep (ADR 0010). The categories backed when each period ended, by the period's first
    // day: written by Settle as it passes the end, so a period with none ended before the first
    // start. What each period's line stopped asking for, for good. And the one destination.
    private readonly Dictionary<DateOnly, HashSet<Category>> periodEnds = [];
    private readonly Dictionary<DateOnly, Money> letGo = [];
    private Category? sweepDestination;

    // The sweeps settling made since the screen last asked, to be announced once.
    private readonly List<SweepMade> sweepsMade = [];

    // Recurring entries (ADR 0011), in the order set up, and the occurrences recorded since the
    // screen last asked, to be announced once.
    private readonly List<RecurringEntry> repeats = [];
    private readonly List<OccurrenceMade> occurrencesMade = [];

    /// <summary>
    /// An empty ledger — no categories, nothing recorded — with one account, the pool account, named
    /// <paramref name="poolAccount"/>, with no starting balance. There is always exactly one pool
    /// account, so an empty ledger cannot have none.
    /// </summary>
    public Ledger(TimeProvider clock, string poolAccount)
        : this(clock, new BudgetPeriodCalendar())
    {
        var stored = NameRule.Normalise(poolAccount)
            ?? throw new ArgumentException("The pool account needs a name.", nameof(poolAccount));
        pool = NewAccount(stored);
        settledThrough = Today;
    }

    private Ledger(TimeProvider clock, BudgetPeriodCalendar calendar)
    {
        this.clock = clock;
        Calendar = calendar;
    }

    /// <summary>
    /// The categories MoneyBud ships with (arc42 §12, *The default categories*) — the
    /// stakeholder's own starting set, chosen to be tried rather than a claim about what a
    /// household needs. Dutch, because they are content the user reads, not test data.
    /// </summary>
    public static IReadOnlyList<string> DefaultCategoryNames { get; } =
        ["Boodschappen", "Huur", "Hobby", "Sparen", "Verzekeringen", "Abonnementen"];

    /// <summary>
    /// The account a first start comes with (arc42 §12, <i>Accounts and net worth</i>): one account,
    /// the pool account, named for a current account but of no kind, with no starting balance, so its balance is the plain sum of what
    /// is on it until it is first corrected. Dutch, as content, like the default categories.
    /// </summary>
    public const string DefaultAccountName = "Betaalrekening";

    /// <summary>
    /// A MoneyBud used for the first time: the default categories, the default account, and nothing
    /// else — nothing budgeted, nothing spent, no income.
    ///
    /// <para>The constructor, by contrast, gives an empty ledger with a pool account of the
    /// caller's naming. That is what the scenarios start from unless they are about the first start,
    /// which is how the suite proves that no other scenario leans on the defaults being there.</para>
    /// </summary>
    public static Ledger StartNew(TimeProvider clock)
    {
        var ledger = new Ledger(clock, DefaultAccountName);
        foreach (var name in DefaultCategoryNames) ledger.AddCategory(name);
        return ledger;
    }

    /// <summary>
    /// Everything this ledger holds, as plain data to be kept. Categories are keyed by their
    /// place in the order added, so budgets and expenses point at a category rather than at a
    /// name (<see cref="LedgerSnapshot"/>).
    /// </summary>
    public LedgerSnapshot ToSnapshot()
    {
        var keys = new Dictionary<Category, int>();
        foreach (var category in categoriesInOrderAdded) keys.Add(category, keys.Count + 1);

        var accountKeys = new Dictionary<Account, int>();
        foreach (var account in accountsInOrderAdded) accountKeys.Add(account, accountKeys.Count + 1);

        BackingSnapshot BackingKept(Backing b) => new(
            accountKeys[b.Account], b.AccumulatingSince, b.HereSince, b.NotMoved, b.PaidHereBefore,
            b.AccumulatingFrom, b.HereFrom, b.Earlier is { } earlier ? LeftKept(earlier) : null);

        LeftBehindSnapshot LeftKept(LeftBehind l) =>
            new(accountKeys[l.Account], l.Since, l.From, l.Amount, BackingKept(l.Before));

        ReallocationEndSnapshot EndKept(ReallocationEnd end) => new(
            end.Kind, end.Account is { } a ? accountKeys[a] : null, end.Category is { } c ? keys[c] : null);

        return new LedgerSnapshot(
            categoriesInOrderAdded.Select(c => new CategorySnapshot(
                keys[c], c.Name, archived.Contains(c),
                backings.TryGetValue(c, out var b) ? BackingKept(b) : null,
                leftBehind.TryGetValue(c, out var l) ? LeftKept(l) : null)).ToList(),
            budgets.Select(b => new BudgetSnapshot(keys[b.Key.Category], b.Key.PeriodStart, b.Value)).ToList(),
            expenses.Select(e => new ExpenseSnapshot(
                e.Id, e.Amount, e.Date, keys[e.Category], e.Label, accountKeys[e.Account])).ToList(),
            incomes.Select(i => new IncomeSnapshot(i.Id, i.Amount, i.Date, i.Label, accountKeys[i.Account])).ToList(),
            lastEntryId,
            accountsInOrderAdded.Select(a => new AccountSnapshot(accountKeys[a], a.Name)).ToList(),
            accountKeys[pool],
            transfers.Select(t => new TransferSnapshot(
                t.Id, t.Amount, t.Date, accountKeys[t.From], accountKeys[t.To])).ToList(),
            balanceCorrections.Select(c => new BalanceCorrectionSnapshot(
                c.Id, c.Date, accountKeys[c.Account], c.Balance, c.IsStartingBalance)).ToList(),
            movements.Select(m => new MovementSnapshot(
                m.Id, m.Date, keys[m.Category], accountKeys[m.From], accountKeys[m.To], m.Amount,
                m.Reason, m.Direction, m.SweptFor)).ToList(),
            settledThrough,
            sweepDestination is { } destination ? keys[destination] : null,
            periodEnds.OrderBy(p => p.Key)
                .Select(p => new PeriodEndSnapshot(
                    p.Key, categoriesInOrderAdded.Where(p.Value.Contains).Select(c => keys[c]).ToList()))
                .ToList(),
            letGo.OrderBy(l => l.Key).Select(l => new LetGoSnapshot(l.Key, l.Value)).ToList(),
            repeats.Select(r => new RepeatSnapshot(r.Occurrences.ToList(), r.Frequency, r.Day, r.Next)).ToList(),
            Calendar.Changes.ToList(),
            reallocations.Select(r => new ReallocationSnapshot(
                r.Id, r.Date, EndKept(r.From), EndKept(r.To), accountKeys[r.FromAccount], accountKeys[r.ToAccount],
                r.Amount)).ToList());
    }

    /// <summary>
    /// A ledger holding exactly what a snapshot holds, as it was when the snapshot was taken.
    ///
    /// <para>Kept data is checked against every rule the ledger keeps while it runs, because it
    /// was read from outside MoneyBud and could have been changed there. <b>Throws</b>
    /// <see cref="InvalidDataException"/> for any that is broken — a name the name rule does not
    /// store, two names the rule counts as one, a budget or an expense pointing at no category, a
    /// budget below zero or outside a period's first day, an entry of zero or less, an income
    /// without a label, an id used twice or beyond the last one issued — across all five kinds of
    /// entry, since they share one counter — an account name the rule does not store or two the
    /// rule counts as one, a pool account or an entry pointing at no account, a transfer between one
    /// account and itself, an account with two starting balances, a movement going a way its reason
    /// does not, or a backing marked with an id that is an entry's or was never issued. And, since
    /// the sweep: a sweep that names no period or another movement that names one, a sweep
    /// destination that is not backed or is archived, a period end for a day that starts no period,
    /// twice, or for a period not yet ended, and an amount let go that is not above zero or is for a
    /// period nothing was swept for. And, since recurring entries: a repeat with no occurrences, with
    /// one that is not an expense or an income or mixes the two, an entry in two repeats, a running
    /// repeat with no next date or a stopped one with one, and a day of the month on anything but a
    /// monthly repeat, or none on one. And, since <i>Vrij</i>: a reallocation with an end MoneyBud does
    /// not know, from an end to itself or out of <i>Niet toegewezen</i>, or with <i>Vrij</i> on another
    /// account than its own; a category both backed and set to "—"; and a stretch marked out of order.
    /// Data like that cannot be read (arc42 §12, <i>When the data cannot be read</i>).</para>
    ///
    /// <para>Dates are not checked against today. An expense cannot be <i>recorded</i> in the
    /// future, but one recorded today is still valid kept data if the clock is later turned
    /// back.</para>
    /// </summary>
    public static Ledger FromSnapshot(LedgerSnapshot snapshot, TimeProvider clock)
    {
        BudgetPeriodCalendar calendar;
        try
        {
            calendar = BudgetPeriodCalendar.FromChanges(snapshot.StartDayChanges ?? []);
        }
        catch (ArgumentException e)
        {
            throw Invalid($"the period start day's history is not one MoneyBud could have made ({e.Message})");
        }

        // The calendar first: every day a period starts on below is checked against it.
        var ledger = new Ledger(clock, calendar);
        var byKey = new Dictionary<int, Category>();
        var accountByKey = new Dictionary<int, Account>();

        if (snapshot.LastEntryId < 0)
            throw Invalid("the last entry id issued is below zero");

        foreach (var kept in snapshot.Accounts)
        {
            if (kept.Name is null || NameRule.Normalise(kept.Name) != kept.Name)
                throw Invalid($"account {kept.Key} has a name that is not stored as the name rule stores it");
            if (accountByKey.ContainsKey(kept.Key))
                throw Invalid($"account key {kept.Key} is used twice");
            if (ledger.accounts.ContainsKey(kept.Name))
                throw Invalid($"\"{kept.Name}\" is two accounts under the name rule");

            accountByKey.Add(kept.Key, ledger.NewAccount(kept.Name));
        }

        ledger.pool = AccountFor(snapshot.PoolAccount);

        foreach (var kept in snapshot.Categories)
        {
            if (kept.Name is null || CategoryName.Normalise(kept.Name) != kept.Name)
                throw Invalid($"category {kept.Key} has a name that is not stored as the name rule stores it");
            if (byKey.ContainsKey(kept.Key))
                throw Invalid($"category key {kept.Key} is used twice");
            if (ledger.categories.ContainsKey(kept.Name))
                throw Invalid($"\"{kept.Name}\" is two categories under the name rule");

            var category = new Category(kept.Name);
            byKey.Add(kept.Key, category);
            ledger.categories.Add(kept.Name, category);
            ledger.categoriesInOrderAdded.Add(category);
            if (kept.IsArchived) ledger.archived.Add(category);
        }

        foreach (var kept in snapshot.Budgets)
        {
            var category = CategoryFor(kept.Category);
            if (kept.Amount.IsNegative)
                throw Invalid($"a budget for \"{category.Name}\" is below zero");
            if (!StartsAPeriod(kept.PeriodStart))
                throw Invalid($"a budget for \"{category.Name}\" starts on {kept.PeriodStart}, which starts no period");
            if (!ledger.budgets.TryAdd((category, kept.PeriodStart), kept.Amount))
                throw Invalid($"\"{category.Name}\" has two budgets for the period starting {kept.PeriodStart}");
        }

        var ids = new HashSet<int>();

        foreach (var kept in snapshot.Expenses)
        {
            CheckEntry(kept.Id, kept.Amount);
            if (kept.Label is not null && NormaliseLabel(kept.Label) != kept.Label)
                throw Invalid($"expense {kept.Id} has a label that is not stored as labels are");
            ledger.expenses.Add(new Expense(
                kept.Id, kept.Amount, kept.Date, CategoryFor(kept.Category), kept.Label, AccountFor(kept.Account)));
        }

        foreach (var kept in snapshot.Incomes)
        {
            CheckEntry(kept.Id, kept.Amount);
            if (kept.Label is null || NormaliseLabel(kept.Label) != kept.Label)
                throw Invalid($"income {kept.Id} has no label, or one not stored as labels are");
            ledger.incomes.Add(new Income(kept.Id, kept.Amount, kept.Date, kept.Label, AccountFor(kept.Account)));
        }

        foreach (var kept in snapshot.Transfers)
        {
            CheckEntry(kept.Id, kept.Amount);
            if (kept.From == kept.To)
                throw Invalid($"transfer {kept.Id} is from an account to itself");
            ledger.transfers.Add(new Transfer(kept.Id, kept.Amount, kept.Date, AccountFor(kept.From), AccountFor(kept.To)));
        }

        var started = new HashSet<Account>();
        foreach (var kept in snapshot.BalanceCorrections)
        {
            CheckId(kept.Id);
            var account = AccountFor(kept.Account);
            if (kept.IsStartingBalance && !started.Add(account))
                throw Invalid($"\"{account.Name}\" has two starting balances");
            ledger.balanceCorrections.Add(new BalanceCorrection(
                kept.Id, kept.Date, account, kept.Balance, kept.IsStartingBalance));
        }

        foreach (var kept in snapshot.Movements)
        {
            CheckEntry(kept.Id, kept.Amount);
            var fits = (kept.Reason, kept.Direction) switch
            {
                (MovementReason.Assigned, MovementDirection.In or MovementDirection.Out) => true,
                (MovementReason.Backed, MovementDirection.In or MovementDirection.Out) => true,
                (MovementReason.Unbacked, MovementDirection.In or MovementDirection.Out) => true,
                (MovementReason.Repointed, MovementDirection.Along) => kept.From != kept.To,
                (MovementReason.Swept, MovementDirection.In or MovementDirection.Out) => true,
                (MovementReason.Rebacked, MovementDirection.In or MovementDirection.Out) => true,
                (MovementReason.Adjusted, MovementDirection.Along) => kept.From != kept.To,
                _ => false,
            };
            if (!fits)
                throw Invalid($"movement {kept.Id} goes {kept.Direction} for a reason that does not go that way");
            if ((kept.Reason == MovementReason.Swept) != kept.SweptFor.HasValue)
                throw Invalid($"movement {kept.Id} is a sweep with no period, or names a period and is not a sweep");
            if (kept.SweptFor is { } sweptFor && !StartsAPeriod(sweptFor))
                throw Invalid($"movement {kept.Id} is a sweep for {sweptFor}, which starts no period");
            ledger.movements.Add(new Movement(
                kept.Id, kept.Date, CategoryFor(kept.Category), AccountFor(kept.From), AccountFor(kept.To),
                kept.Amount, kept.Reason, kept.Direction, kept.SweptFor));
        }

        // Data from before version 8 has no list of reallocations at all; data since has one, if empty.
        var fromBeforeVersion8 = snapshot.Reallocations is null;
        foreach (var kept in snapshot.Reallocations ?? [])
        {
            CheckEntry(kept.Id, kept.Amount);
            var from = EndFor(kept.From, kept.Id);
            var to = EndFor(kept.To, kept.Id);
            if (from == to || from.Kind == ReallocationEndKind.Unassigned)
                throw Invalid($"reallocation {kept.Id} is from an end to itself, or out of Niet toegewezen");
            var fromAccount = AccountFor(kept.FromAccount);
            var toAccount = AccountFor(kept.ToAccount);
            if ((from.Account is { } a && a != fromAccount) || (to.Account is { } b && b != toAccount))
                throw Invalid($"reallocation {kept.Id} moves Vrij on one account as if it were on another");
            ledger.reallocations.Add(new Reallocation(kept.Id, kept.Date, from, to, fromAccount, toAccount, kept.Amount));
        }

        // A backing's marks were drawn from the entries' counter, after every entry recorded
        // before them, so each is an id issued and no entry's. So were the marks of every stretch
        // before it, and of a "—".
        foreach (var kept in snapshot.Categories)
        {
            if (kept.Backing is not null && kept.LeftBehind is not null)
                throw Invalid($"\"{byKey[kept.Key].Name}\" is both backed and set to none");
            var category = byKey[kept.Key];
            if (kept.Backing is { } backing) ledger.backings.Add(category, BackingFor(category, backing));
            if (kept.LeftBehind is { } left) ledger.leftBehind.Add(category, LeftFor(category, left));
        }

        if (snapshot.SweepDestination is { } destinationKey)
        {
            var destination = CategoryFor(destinationKey);
            if (!ledger.backings.ContainsKey(destination) || ledger.archived.Contains(destination))
                throw Invalid($"the sweep destination \"{destination.Name}\" is not backed, or is archived");
            ledger.sweepDestination = destination;
        }

        foreach (var kept in snapshot.PeriodEnds)
        {
            if (!StartsAPeriod(kept.PeriodStart))
                throw Invalid($"a period end is recorded for {kept.PeriodStart}, which starts no period");
            if (ledger.Calendar.PeriodContaining(kept.PeriodStart).LastDay >= snapshot.SettledThrough)
                throw Invalid($"a period end is recorded for the period starting {kept.PeriodStart}, which has not ended");
            if (!ledger.periodEnds.TryAdd(kept.PeriodStart, kept.Backed.Select(CategoryFor).ToHashSet()))
                throw Invalid($"the period starting {kept.PeriodStart} ended twice");
        }

        foreach (var kept in snapshot.LetGo)
        {
            if (kept.Amount.Cents <= 0)
                throw Invalid($"an amount let go for {kept.PeriodStart} is not more than zero");
            if (!ledger.movements.Any(m => m.SweptFor == kept.PeriodStart))
                throw Invalid($"an amount is let go for {kept.PeriodStart}, which nothing was swept for");
            if (!ledger.letGo.TryAdd(kept.PeriodStart, kept.Amount))
                throw Invalid($"two amounts are let go for {kept.PeriodStart}");
        }

        var expenseIds = ledger.expenses.Select(e => e.Id).ToHashSet();
        var incomeIds = ledger.incomes.Select(i => i.Id).ToHashSet();
        var repeated = new HashSet<int>();
        foreach (var kept in snapshot.Repeats)
        {
            if (kept.Occurrences.Count == 0)
                throw Invalid("a repeat has no occurrences");
            if (!kept.Occurrences.All(expenseIds.Contains) && !kept.Occurrences.All(incomeIds.Contains))
                throw Invalid("a repeat's occurrences are not all expenses or all incomes");
            foreach (var id in kept.Occurrences)
            {
                if (!repeated.Add(id))
                    throw Invalid($"entry {id} is an occurrence of two repeats, or twice of one");
            }
            if (kept.Frequency is { } frequency && !Enum.IsDefined(frequency))
                throw Invalid("a repeat has a frequency MoneyBud does not know");
            if ((kept.Frequency is null) != (kept.Next is null))
                throw Invalid("a repeat is running with no next date, or stopped with one");
            var dayFits = kept.Frequency == Frequency.Monthly ? kept.Day is >= 1 and <= 31 : kept.Day is null;
            if (!dayFits)
                throw Invalid("a monthly repeat has no day of the month, or another repeat has one");
            ledger.repeats.Add(RecurringEntry.Kept(kept.Occurrences, kept.Frequency, kept.Day, kept.Next));
        }

        ledger.lastEntryId = snapshot.LastEntryId;
        ledger.settledThrough = snapshot.SettledThrough;
        return ledger;

        Backing BackingFor(Category category, BackingSnapshot backing)
        {
            CheckMark(backing.AccumulatingSince);
            CheckMark(backing.HereSince);
            if (backing.PaidHereBefore is { Cents: < 0 } || backing.NotMoved is { Cents: < 0 })
                throw Invalid($"the backing of \"{category.Name}\" remembers less than nothing");
            var account = AccountFor(backing.Account);
            var earlier = backing.Earlier is { } e ? LeftFor(category, e) : null;
            if (earlier is not null && earlier.Since.Id >= backing.AccumulatingSince.Id)
                throw Invalid($"a backing of \"{category.Name}\" is older than the none before it");
            var accumulatingFrom = PeriodFrom(backing.AccumulatingFrom, backing.AccumulatingSince);
            var hereFrom = PeriodFrom(backing.HereFrom, backing.HereSince);
            var paidHereBefore = backing.PaidHereBefore ?? ledger.PaidBefore(category, account, backing.HereSince);
            // Before version 8 what is there counted the account's expenses from the period of the
            // latest re-pointing. It counts them from the period of backing now, so the ones dated in
            // between, as they stand, are remembered with the mark: every figure reads the same.
            if (fromBeforeVersion8 && hereFrom > accumulatingFrom)
                paidHereBefore += Money.Sum(ledger.expenses
                    .Where(e => e.Category == category && e.Account == account
                                && e.Date >= accumulatingFrom && e.Date < hereFrom)
                    .Select(e => e.Amount));
            return new Backing(
                account, backing.AccumulatingSince, backing.HereSince,
                backing.NotMoved ?? ledger.NotMovedBefore(category, backing.AccumulatingSince),
                paidHereBefore, accumulatingFrom, hereFrom, earlier);
        }

        LeftBehind LeftFor(Category category, LeftBehindSnapshot left)
        {
            CheckMark(left.Since);
            if (!StartsAPeriod(left.From) || left.From > left.Since.Date)
                throw Invalid($"\"{category.Name}\" was set to none in a period starting {left.From}, which starts no period on or before it");
            var before = BackingFor(category, left.Before);
            if (left.Since.Id <= before.HereSince.Id || left.Since.Id <= before.AccumulatingSince.Id)
                throw Invalid($"\"{category.Name}\" was set to none before the backing it ended");
            return new LeftBehind(AccountFor(left.Account), left.Since, left.From, left.Amount, before);
        }

        ReallocationEnd EndFor(ReallocationEndSnapshot end, int id) => end.Kind switch
        {
            ReallocationEndKind.Unclaimed when end.Account is { } key && end.Category is null =>
                ReallocationEnd.UnclaimedOn(AccountFor(key)),
            ReallocationEndKind.Category when end.Category is { } key && end.Account is null =>
                ReallocationEnd.For(CategoryFor(key)),
            ReallocationEndKind.Unassigned when end.Account is null && end.Category is null => ReallocationEnd.Unassigned,
            _ => throw Invalid($"reallocation {id} has an end MoneyBud does not know"),
        };

        // Data from before version 7 knew only periods starting on the 1st, so the mark's period is
        // the calendar's. A day kept is the first day of a period on or before the mark.
        DateOnly PeriodFrom(DateOnly? kept, EntryMark mark)
        {
            if (kept is not { } from) return ledger.Calendar.PeriodContaining(mark.Date).FirstDay;
            if (from > mark.Date || !StartsAPeriod(from))
                throw Invalid($"a backing counts from {from}, which starts no period on or before its mark");
            return from;
        }

        void CheckMark(EntryMark mark)
        {
            if (mark.Id < 1 || mark.Id > snapshot.LastEntryId || ids.Contains(mark.Id))
                throw Invalid($"a backing is marked with id {mark.Id}, which is not one drawn for it");
        }

        // A period in the last month of the calendar has no end the calendar can name, so a day
        // there starts no period — rather than failing the whole start with something other than
        // "cannot be read".
        bool StartsAPeriod(DateOnly day)
        {
            try
            {
                return ledger.Calendar.PeriodContaining(day).FirstDay == day;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        Category CategoryFor(int key) =>
            byKey.TryGetValue(key, out var category) ? category : throw Invalid($"there is no category {key}");

        Account AccountFor(int key) =>
            accountByKey.TryGetValue(key, out var account) ? account : throw Invalid($"there is no account {key}");

        void CheckEntry(int id, Money amount)
        {
            CheckId(id);
            if (amount.Cents <= 0)
                throw Invalid($"entry {id} is not more than zero");
        }

        void CheckId(int id)
        {
            if (id < 1 || id > snapshot.LastEntryId)
                throw Invalid($"entry id {id} was never issued");
            if (!ids.Add(id))
                throw Invalid($"entry id {id} is used twice");
        }

        static InvalidDataException Invalid(string what) => new($"The kept ledger cannot be read: {what}.");
    }

    /// <summary>
    /// How dates fall into periods. Replaced, never changed, by <see cref="ChangeStartDay"/>: every
    /// period before the current one keeps its boundaries.
    /// </summary>
    public BudgetPeriodCalendar Calendar { get; private set; }

    /// <summary>
    /// The day MoneyBud considers today, in the user's own time. How a configurable period start
    /// day should interact with time zones is still open (arc42 §8.2); on a single-machine
    /// desktop app local time is the reading that matches what the user sees on the wall.
    /// </summary>
    public DateOnly Today => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

    public BudgetPeriod CurrentPeriod => Calendar.PeriodContaining(Today);

    /// <summary>
    /// Adds a category name, with exactly one of four outcomes (arc42 §12): the category is
    /// created; a category in use already had the name and is handed back; an archived category
    /// had it and is brought back, history and all; or the name trimmed to nothing and is refused.
    ///
    /// <para>In the two cases that hand back an existing category, it keeps the spelling it
    /// already had. Taking the one typed now would be a rename by the back door.</para>
    /// </summary>
    public AddCategoryResult AddCategory(string? name)
    {
        Settle();
        var stored = CategoryName.Normalise(name);
        if (stored is null)
            return AddCategoryResult.Refused(CategoryRefusal.NameMissing);

        if (categories.TryGetValue(stored, out var existing))
        {
            return archived.Remove(existing)
                ? AddCategoryResult.BroughtBack(existing)
                : AddCategoryResult.AlreadyThere(existing);
        }

        var category = new Category(stored);
        categories.Add(stored, category);
        categoriesInOrderAdded.Add(category);
        return AddCategoryResult.Created(category);
    }

    /// <summary>
    /// Takes a category out of new entry. Nothing it owns changes: its budgets and its expenses
    /// stay, and every figure for it answers exactly as before (arc42 §12). Never asks for
    /// confirmation — nothing is lost, and adding the name undoes it. Returns the category, so
    /// that what was archived can be said, spelled as MoneyBud has it.
    ///
    /// <para><b>Throws</b> for a name that is not one of the user's categories, and for a
    /// category that is already archived. Neither is something the user can do — archiving
    /// applies to a category you have and that is in use, and archived is a yes-or-no state — so
    /// no user-facing behaviour is defined for them (§12, *What the state fixes*). Reaching
    /// either is a mistake in whatever called this, not a situation to report to the user.</para>
    /// </summary>
    public Category ArchiveCategory(string name)
    {
        Settle();
        var category = Find(name)
            ?? throw new InvalidOperationException($"There is no category called \"{name}\" to archive.");

        if (!archived.Add(category))
            throw new InvalidOperationException($"\"{category.Name}\" is already archived.");

        // An archived category is out of new entry, and a sweep into it every month would be the
        // most regular new entry there is (§12, ruling 6). Bringing it back does not set it again.
        if (sweepDestination == category) sweepDestination = null;

        return category;
    }

    /// <summary>
    /// Gives a category a new name (arc42 §12, *Renaming a category*). The new name follows the
    /// rules for adding one: trimmed, stored otherwise as typed, and refused when it trims to
    /// nothing. A name another category has — archived ones included — is refused as taken.
    ///
    /// <para>A new spelling of the category's own name is not taken: under the name rule it is
    /// the same name, so no other category can hold it. The name spelled exactly as it already
    /// is changes nothing.</para>
    ///
    /// <para>It stays the same category. Its budgets, its expenses, its place in the order added
    /// and whether it is archived are untouched, and every period, past ones included, shows the
    /// new name. Renaming is not new entry, so it brings nothing back. The old name is free
    /// afterwards: nothing remembers it.</para>
    ///
    /// <para><b>Throws</b> for a name that is not one of the user's categories. A category is
    /// renamed from its row, so this is not something the user can do.</para>
    /// </summary>
    public RenameCategoryResult RenameCategory(string name, string? newName)
    {
        Settle();
        var category = Find(name)
            ?? throw new InvalidOperationException($"There is no category called \"{name}\" to rename.");

        var stored = CategoryName.Normalise(newName);
        if (stored is null)
            return RenameCategoryResult.Refused(RenameRefusal.NameMissing);

        if (stored == category.Name)
            return RenameCategoryResult.Unchanged(category);

        if (categories.TryGetValue(stored, out var holder) && holder != category)
            return RenameCategoryResult.Refused(RenameRefusal.NameTaken);

        // The index is keyed by name, so the category is taken out under its old name and put
        // back under its new one. Everything else holds the category itself and follows.
        var oldName = category.Name;
        categories.Remove(oldName);
        category.Name = stored;
        categories.Add(stored, category);

        return RenameCategoryResult.Renamed(oldName, category);
    }

    /// <summary>
    /// Whether a category has no history in any period — no expense, and no budget of more than
    /// zero — and so can be deleted (arc42 §12, *Deleting a category that has no history
    /// anywhere*).
    ///
    /// <para>Decided by the figures as they are now. A budget assigned and taken back to zero is
    /// no history: that is deliberately not <see cref="HasBudget"/>, which can tell it apart from
    /// never assigned, a difference §12 says does not exist. And an expense that was removed, or
    /// moved to another category, leaves no trace to count.</para>
    ///
    /// <para>Money moved on its behalf between two accounts is history too: those rows stay in both
    /// accounts' histories and name it, so such a category cannot be deleted, even once its budgets
    /// are back at zero (§12, <i>Backing: ruled after the build</i>). A movement from the pool account
    /// to itself is in no history and moves no balance, so it does not count — <b>except a
    /// sweep</b>, which an ended period's line names whichever accounts it went between (§12, ruled
    /// at the scenario stage, 7).</para>
    ///
    /// <para>Since increment 15, two more (§12, follow-up 10 and ruled at the scenario stage, 3): a
    /// category whose <i>Opgebouwd</i> is not zero, however it got there, money a "—" left behind
    /// included; and a category any reallocation names, even once its <i>Opgebouwd</i> is back at
    /// zero, since those rows stay in an account's history and explain its <i>Vrij</i>. Archiving
    /// stays open.</para>
    /// </summary>
    public bool CanDelete(string name) =>
        Find(name) is { } category
        && !expenses.Any(e => e.Category == category)
        && !budgets.Any(b => b.Key.Category == category && b.Value.Cents > 0)
        && !movements.Any(m => m.Category == category && (m.From != m.To || m.Reason == MovementReason.Swept))
        && !reallocations.Any(r => r.From.Category == category || r.To.Category == category)
        && AccumulatedFor(category.Name, CurrentPeriod) is not { Cents: not 0 };

    /// <summary>
    /// Deletes a category with no history anywhere. It is gone: not archived, brought back by
    /// nothing, and its name is free, so adding the name afterwards creates a new category that
    /// goes last in the order added. Never asks first — nothing of value is lost (arc42 §12).
    /// Budgets of zero it still had go with it, since they are not history, and so does its
    /// backing, and any money moved for it from the pool account to itself, which moved no balance:
    /// nothing moves. It leaves the record of what was backed when each period ended, and when it
    /// is the sweep destination there is none afterwards: the setting is not history (§12, <i>The
    /// destination is one list</i>).
    ///
    /// <para><b>Throws</b> for a name that is not one of the user's categories, and for a category
    /// with history. The delete act is offered only on a category that <see cref="CanDelete"/>, so
    /// neither is something the user can do.</para>
    /// </summary>
    public Category DeleteCategory(string name)
    {
        Settle();
        var category = Find(name)
            ?? throw new InvalidOperationException($"There is no category called \"{name}\" to delete.");

        if (!CanDelete(name))
            throw new InvalidOperationException($"\"{category.Name}\" has history, so it cannot be deleted.");

        categories.Remove(category.Name);
        categoriesInOrderAdded.Remove(category);
        archived.Remove(category);
        backings.Remove(category);
        leftBehind.Remove(category);
        movements.RemoveAll(m => m.Category == category);
        foreach (var backed in periodEnds.Values) backed.Remove(category);
        if (sweepDestination == category) sweepDestination = null;
        foreach (var key in budgets.Keys.Where(k => k.Category == category).ToList())
            budgets.Remove(key);

        return category;
    }

    /// <summary>
    /// The categories offered when recording something new: every category that is not archived,
    /// in the order they were added. The UI suggests them alphabetically (arc42 §12, *Category
    /// entry is free text with suggestions*); that is how they are shown, so it is the UI's to do.
    /// </summary>
    public IReadOnlyList<Category> CategoriesOffered =>
        categoriesInOrderAdded.Where(c => !archived.Contains(c)).ToList();

    public IReadOnlyList<Category> ArchivedCategories =>
        categoriesInOrderAdded.Where(archived.Contains).ToList();

    /// <summary>
    /// The categories a period shows, in the order they were added — the Overview's tie order, and
    /// a category brought back keeps its first place because bringing back does not add it again
    /// (arc42 §12, *When any category is shown in a period: the full rule*): every category with history there, and every
    /// category <i>in use</i> in the current period and every later one, where it can be planned
    /// for. A past period shows only what has history in it, and an archived category is shown
    /// only where it has history — and, since backing, in the current period and every later one
    /// while it is backed and its <i>Accumulated</i> there is not zero, so that money still there
    /// for it is never hidden, and it can be unbacked from the period it is in (§12, ruled at the
    /// build, 2026-09-27).
    ///
    /// <para>"Current" is read from the clock on every call, so a period that was current becomes
    /// past the moment the next one begins, with nothing rebuilt around it.</para>
    /// </summary>
    public IReadOnlyList<Category> CategoriesShownIn(BudgetPeriod period)
    {
        var plannable = period.FirstDay >= CurrentPeriod.FirstDay;

        return categoriesInOrderAdded
            .Where(c => (plannable && !archived.Contains(c))
                        || HasHistoryIn(c.Name, period)
                        || (plannable && AccumulatedFor(c.Name, period) is { Cents: not 0 }))
            .ToList();
    }

    /// <summary>Whether the name is one of the user's categories — in use or archived.</summary>
    public bool HasCategory(string name) => Find(name) is not null;

    public bool IsArchived(string name) => Find(name) is { } category && archived.Contains(category);

    /// <summary>
    /// Whether a category has history in a period: a budget of more than zero, or an expense.
    ///
    /// <para>This is what decides where an <i>archived</i> category is still shown — in every
    /// period where it has history, the current one included, and nowhere else (arc42 §12,
    /// *Where an archived category is still shown*). A budget of zero is not history: a category
    /// with no budget set behaves exactly as one budgeted at zero, so the two cannot differ
    /// here either.</para>
    ///
    /// <para>The whole rule, of which this is one half, is <see cref="CategoriesShownIn"/>.</para>
    /// </summary>
    public bool HasHistoryIn(string categoryName, BudgetPeriod period) =>
        BudgetFor(categoryName, period).Cents > 0 || ExpensesFor(categoryName, period).Count > 0;

    /// <summary>
    /// Assigns an amount to a category in a period, or refuses it for exactly one reason
    /// (arc42 §12, *Assign*).
    ///
    /// <para>Assigning <b>moves</b> an amount; it does not set a figure. A positive amount goes
    /// out of the period's <i>Unassigned</i> and onto the category's <i>Budget</i>. A negative one
    /// comes back, but the <i>Budget</i> floors at zero, so at most what it holds comes back and
    /// the rest is reported as the <see cref="AssignResult.Shortfall"/>. Zero is accepted and
    /// moves nothing. Nothing is spent either way, and nothing about <i>Unassigned</i> is checked:
    /// going <i>Over-assigned</i> is allowed and unwarned.</para>
    ///
    /// <para>The checks run in a fixed order, the same as <see cref="RecordExpense"/>'s: the
    /// category, then the amount, then the period. See <see cref="AssignRefusal"/>. The amount
    /// arrives as a <see cref="decimal"/> of euros so that one finer than a cent can be refused
    /// rather than rounded.</para>
    ///
    /// <para>An <b>archived</b> category is not refused. Only a <b>positive</b> amount brings it
    /// back, and only once every check has passed: a negative amount is tidying up after putting
    /// it away, zero plans nothing, and a refused assignment did nothing at all (§12, *Only a
    /// positive assignment brings it back*).</para>
    ///
    /// <para>For a <b>backed</b> category, assigning in the current period also moves money today
    /// (§12, <i>Assigning to a backed category moves money</i>): a positive amount from the pool
    /// account to the backing account, a negative one back, but only what the clip let through and
    /// at most what is there for the category (<see cref="ThereFor"/>). In a later period only the
    /// figure changes; the money moves on that period's first day (<see cref="Settle"/>). Nothing
    /// about balances is checked: a move may overdraw the pool account.</para>
    ///
    /// <para><b>Throws</b> for a period that is not one of <see cref="Calendar"/>'s own, such as a
    /// hand-made date range. A user picks a period from the calendar and cannot reach this, so no
    /// behaviour is defined for it; reaching it is a mistake in whatever called this.</para>
    /// </summary>
    public AssignResult Assign(decimal amountInEuros, string? categoryName, BudgetPeriod period)
    {
        Settle();
        CheckIsAPeriod(period);

        if (CategoryName.Normalise(categoryName) is null)
            return AssignResult.Refused(AssignRefusal.CategoryMissing);

        if (Find(categoryName) is not { } category)
            return AssignResult.Refused(AssignRefusal.UnknownCategory);

        if (!Money.IsWholeCents(amountInEuros))
            return AssignResult.Refused(AssignRefusal.AmountFinerThanCent);

        if (period.FirstDay < CurrentPeriod.FirstDay)
            return AssignResult.Refused(AssignRefusal.PeriodInPast);

        var amount = Money.FromEuros(amountInEuros);
        if (amount == Money.Zero)
            return AssignResult.Assigned(category, Money.Zero, categoryBroughtBack: false);

        var budget = BudgetFor(category.Name, period);
        var shortfall = amount.IsNegative && (-amount).Cents > budget.Cents
            ? -amount - budget
            : Money.Zero;

        // A clipped negative takes the Budget to exactly zero; everything else moves in full.
        // Written only when it changes, so that clipping against a Budget that was already zero
        // leaves no mark, just as assigning zero does not.
        var newBudget = shortfall == Money.Zero ? budget + amount : Money.Zero;
        if (newBudget != budget)
            budgets[(category, period.FirstDay)] = newBudget;

        // For a backed category the money moves as well, today, but only in the current period:
        // a later period's moves on its first day (Settle). A negative amount moves back what the
        // clip let through, and never more than is there for the category.
        if (period == CurrentPeriod && backings.TryGetValue(category, out var backing))
        {
            if (!amount.IsNegative)
            {
                Move(category, pool, backing.Account, amount, MovementReason.Assigned, MovementDirection.In);
            }
            else
            {
                var cameBack = budget - newBudget;
                var there = ThereFor(category, backing);
                var back = there.Cents < cameBack.Cents ? there : cameBack;
                if (back.Cents > 0)
                    Move(category, backing.Account, pool, back, MovementReason.Assigned, MovementDirection.Out);
            }
        }

        var broughtBack = !amount.IsNegative && archived.Remove(category);
        return AssignResult.Assigned(category, shortfall, broughtBack);
    }

    /// <summary>
    /// The plan a period is offered, or null when it is offered none (arc42 §12, <i>Opening a
    /// period</i>).
    ///
    /// <para>A period is offered a plan when it is the current period or a later one, every
    /// <i>Budget</i> in it is zero — an archived category's included, since that is still assigned
    /// money — and some earlier period has a plan. The plan offered is the <b>latest</b> earlier
    /// one, however far back, and "earlier" is earlier than <paramref name="period"/>, not than
    /// today. A period has a plan when it has a <i>Budget</i> above zero for a category that is
    /// not archived now; spending is not a plan.</para>
    ///
    /// <para>Decided by the figures alone, never by <see cref="HasBudget"/>: a <i>Budget</i> taken
    /// back to zero is the same as one never made, so taking every <i>Budget</i> back brings the
    /// offer back (§8.1). And read afresh on every call — the clock for "current", the archive for
    /// "archived now" — so the offer comes and goes with no event to mark it.</para>
    ///
    /// <para><b>Throws</b> for a period that is not one of <see cref="Calendar"/>'s own, as
    /// <see cref="Assign"/> does.</para>
    /// </summary>
    public PlanOffer? PlanOfferedIn(BudgetPeriod period)
    {
        CheckIsAPeriod(period);

        if (period.FirstDay < CurrentPeriod.FirstDay)
            return null;

        if (budgets.Any(b => b.Key.PeriodStart == period.FirstDay && b.Value.Cents > 0))
            return null;

        // Budgets are kept by their period's first day, so the latest earlier plan is simply the
        // latest first day that carries one: no walking back period by period, and no limit.
        var planned = budgets
            .Where(b => b.Key.PeriodStart < period.FirstDay && b.Value.Cents > 0 && !archived.Contains(b.Key.Category))
            .Select(b => b.Key.PeriodStart)
            .ToList();

        if (planned.Count == 0)
            return null;

        var from = planned.Max();
        var figures = categoriesInOrderAdded
            .Where(c => !archived.Contains(c)
                        && budgets.TryGetValue((c, from), out var amount) && amount.Cents > 0)
            .Select(c => new PlanFigure(c, budgets[(c, from)]))
            .ToList();

        return new PlanOffer(Calendar.PeriodContaining(from), figures);
    }

    /// <summary>
    /// Takes over the plan offered in a period: every figure in it is assigned in full, into
    /// <paramref name="period"/>, as one act (arc42 §12, <i>Taking the plan over assigns it in
    /// full</i>). Even past what <i>Unassigned</i> holds, so the period may go <i>Over-assigned</i>,
    /// allowed and unwarned. It touches no expense, and brings no archived category back, since no
    /// archived category's figure is in a plan.
    ///
    /// <para>Each figure goes through <see cref="Assign"/>, which stays the only way a budget is
    /// written. Refused, like any assignment, for a past period; that is checked first, since a
    /// past period is offered nothing.</para>
    ///
    /// <para><b>Throws</b> when the period is offered no plan. The act is offered only while a plan
    /// is, and every act redraws the screen, so the user cannot reach it.</para>
    /// </summary>
    public TakeOverPlanResult TakeOverPlan(BudgetPeriod period)
    {
        Settle();
        CheckIsAPeriod(period);

        if (period.FirstDay < CurrentPeriod.FirstDay)
            return TakeOverPlanResult.Refused(AssignRefusal.PeriodInPast);

        var plan = PlanOfferedIn(period)
            ?? throw new InvalidOperationException($"No plan is offered in {period}.");

        foreach (var figure in plan.Figures)
        {
            var result = Assign(figure.Amount.Euros, figure.Category.Name, period);
            if (!result.WasAssigned || result.Shortfall != Money.Zero || result.CategoryBroughtBack)
                throw new InvalidOperationException(
                    $"Taking over {figure.Category.Name}'s figure did not simply assign it: {result}.");
        }

        return TakeOverPlanResult.TakenOver(plan, period);
    }

    private void CheckIsAPeriod(BudgetPeriod period)
    {
        if (Calendar.PeriodContaining(period.FirstDay) != period)
            throw new ArgumentException($"{period} is not a budget period.", nameof(period));
    }

    /// <summary>
    /// Whether anything was ever assigned to a category in a period — including amounts since
    /// taken back out again. Assigning zero leaves no mark, because it changes nothing.
    /// </summary>
    public bool HasBudget(string categoryName, BudgetPeriod period) =>
        Find(categoryName) is { } category && budgets.ContainsKey((category, period.FirstDay));

    /// <summary>
    /// The plan for a category in a period. A category with no budget set behaves exactly as one
    /// budgeted at zero — there is no separate "unbudgeted" state (arc42 §12, *Budget*).
    /// </summary>
    public Money BudgetFor(string categoryName, BudgetPeriod period) =>
        Find(categoryName) is { } category
        && budgets.TryGetValue((category, period.FirstDay), out var amount)
            ? amount
            : Money.Zero;

    /// <summary>
    /// Records an expense, or refuses it for exactly one reason.
    ///
    /// <para>The amount arrives as a <see cref="decimal"/> of euros rather than as
    /// <see cref="Money"/>, and the order of the checks below is fixed rather than incidental.
    /// -12.345 breaks two rules at once; the sign is checked first, so the user is told the
    /// amount must be more than zero. See the comment above the refusal scenarios in
    /// features/record-expense.feature, and ADR 0003.</para>
    ///
    /// <para>Nothing about being over budget is checked. Going over is allowed, unwarned and
    /// unblocked (arc42 §12), so there is no check to make.</para>
    ///
    /// <para>The label is trimmed, and one that is nothing but whitespace becomes no label at
    /// all — an expense is allowed to have none. See <see cref="NormaliseLabel"/>.</para>
    ///
    /// <para>The category is found by the name rule, so "  groceries " records against
    /// Groceries, and a name that trims to nothing names no category. An <b>archived</b> category
    /// is not refused: the expense is recorded and brings it back, and the result says so
    /// (arc42 §12). That happens only once every other check has passed — bringing back is a
    /// side-effect of recording, so a refused expense leaves the category archived.</para>
    ///
    /// <para>The expense is on <paramref name="account"/>, or on the <b>pool account</b> when none
    /// is given (arc42 §12, *An expense defaults to the pool account*). An account is picked from a
    /// list, never typed, so it is never refused: one that is not in the ledger <b>throws</b>.</para>
    ///
    /// <para>Given a <paramref name="repeat"/>, the expense is the first occurrence of a recurring
    /// entry, which repeats from its date (arc42 §12, <i>Recurring entries</i>). Occurrences already
    /// due — the expense was dated back — are recorded at once (follow-up 4), to be announced
    /// (<see cref="TakeOccurrencesMade"/>). A refused expense sets nothing up.</para>
    ///
    /// <para>Since increment 15, <b>an expense against a backed category is on its backing account</b>,
    /// from the period the category got it (<see cref="LockedAccountFor"/>): with no account given it
    /// goes there, and another account given for it <b>throws</b>, since the list is locked on screen
    /// and the user cannot choose one.</para>
    /// </summary>
    public RecordExpenseResult RecordExpense(
        decimal amountInEuros, string? categoryName, DateOnly date, string? label = null, Account? account = null,
        Frequency? repeat = null)
    {
        Settle();
        if (account is not null) CheckIsMine(account);
        var (refusal, category) = CheckExpense(amountInEuros, categoryName, date);
        if (refusal is { } reason)
            return RecordExpenseResult.Refused(reason);

        var on = OnLockedAccount(LockedAccountFor(category!.Name, date), account) ?? account ?? pool;

        var broughtBack = archived.Remove(category!);

        var expense = new Expense(
            ++lastEntryId, Money.FromEuros(amountInEuros), date, category!, NormaliseLabel(label), on);
        expenses.Add(expense);
        StartRepeating(expense, repeat);
        return RecordExpenseResult.Recorded(expense, broughtBack);
    }

    /// <summary>
    /// Changes an expense to the amount, category, date and label given, or refuses the change
    /// for exactly one reason (arc42 §12, *A changed entry is judged as if it were recorded now*).
    ///
    /// <para>The checks are recording's, in recording's order, so a change passes or fails exactly
    /// as the changed expense would if it were recorded now — past periods included, where a
    /// correction changes the period's figures, and that is its purpose. A refused change leaves
    /// the expense exactly as it was.</para>
    ///
    /// <para><b>An expense saved with nothing changed is never refused</b>, and is told apart so
    /// that it can go through quietly. It is recognised before any check runs, so that it cannot
    /// fail one: what was accepted once is accepted again as it is.</para>
    ///
    /// <para>A change <b>overwrites</b> the expense: same id, same place in the order recorded,
    /// and no record kept of what it was. Moving it <b>onto</b> an archived category brings that
    /// category back, as recording against it would; fixing an expense already on an archived
    /// category does not, because that is correcting history, not using the category
    /// again.</para>
    ///
    /// <para><paramref name="account"/> moves the expense to another account; left out, it stays on
    /// the one it is on. Only the balances move: the id, and so the moment it was first recorded,
    /// stay as they were.</para>
    ///
    /// <para><b>Throws</b> for an expense that is not in the ledger, such as one already removed.
    /// A change is made from the expense's row, so that is not something the user can do.</para>
    ///
    /// <para>This keeps the repeat's frequency as it is; a changed date on the latest occurrence still
    /// moves its day (§12, follow-up 3). The overload that takes a frequency can change that too.</para>
    /// </summary>
    public ChangeExpenseResult ChangeExpense(
        Expense expense, decimal amountInEuros, string? categoryName, DateOnly date, string? label,
        Account? account = null) =>
        ChangeExpense(expense, amountInEuros, categoryName, date, label, account, FrequencyOf(expense));

    /// <summary>
    /// Changes an expense as the overload without a frequency does, and its repeat to
    /// <paramref name="repeat"/>, null being one-off (arc42 §12, <i>The latest occurrence sets the
    /// next</i>). Only an expense that <see cref="SetsTheRepeat"/> can change it; for an earlier
    /// occurrence the frequency is <b>ignored</b>, since its drop-down is locked, and the change is to
    /// that expense alone (plan for increment 12, 3 and 4).
    ///
    /// <para>On one that sets the repeat: a one-off given a frequency starts repeating from its date;
    /// on the latest occurrence, a new frequency, or a new date, sets the day and the next date from
    /// that date, and one-off stops the repeat; a stopped repeat's last occurrence given a frequency
    /// starts it again. A changed frequency alone is a change. Whatever is due by then is recorded at
    /// once (follow-ups 3 and 4).</para>
    /// </summary>
    public ChangeExpenseResult ChangeExpense(
        Expense expense, decimal amountInEuros, string? categoryName, DateOnly date, string? label,
        Account? account, Frequency? repeat)
    {
        Settle();
        var index = IndexOf(expense);
        var current = expenses[index];
        if (account is not null) CheckIsMine(account);
        var on = OnLockedAccount(LockedAccountFor(categoryName, date, current), account) ?? account ?? current.Account;
        var repeatChanged = SetsTheRepeat(current) && repeat != FrequencyOf(current);

        if (Money.IsWholeCents(amountInEuros)
            && Money.FromEuros(amountInEuros) == current.Amount
            && Find(categoryName) == current.Category
            && date == current.Date
            && NormaliseLabel(label) == current.Label
            && on == current.Account
            && !repeatChanged)
            return ChangeExpenseResult.Unchanged(current);

        var (refusal, category) = CheckExpense(amountInEuros, categoryName, date);
        if (refusal is { } reason)
            return ChangeExpenseResult.Refused(reason);

        var broughtBack = category != current.Category && archived.Remove(category!);

        var changed = current with
        {
            Amount = Money.FromEuros(amountInEuros),
            Date = date,
            Category = category!,
            Label = NormaliseLabel(label),
            Account = on,
        };
        var before = GapsBefore(current, changed);
        expenses[index] = changed;
        Repeat(current, changed, repeat);
        CloseGaps(before, changed);
        return ChangeExpenseResult.Changed(changed, broughtBack);
    }

    /// <summary>
    /// Removes an expense. It is gone, and nothing keeps a copy. Asking first is the screen's to
    /// do (arc42 §12, *Removing an entry asks first*); by the time this is called, the user has
    /// confirmed. Removing is not new entry, so it never brings a category back.
    ///
    /// <para>An occurrence removed is that one alone: the repeat carries on, and its next date does
    /// not move. The one recorded before it becomes the latest; removing the only one ends the repeat
    /// (arc42 §12, ruling 4, follow-up 1).</para>
    ///
    /// <para><b>Throws</b> for an expense that is not in the ledger.</para>
    /// </summary>
    public void RemoveExpense(Expense expense)
    {
        Settle();
        var index = IndexOf(expense);
        var current = expenses[index];
        var before = GapsBefore(current, null);
        expenses.RemoveAt(index);
        NoLongerRepeats(expense);
        CloseGaps(before, current);
    }

    // The account an expense goes on when the list is locked: that account, or none when it is open.
    // Another account given for a locked list is something the screen cannot do.
    private static Account? OnLockedAccount(Account? locked, Account? given) =>
        locked is null || given is null || given == locked
            ? locked
            : throw new InvalidOperationException(
                $"The expense's account is locked on \"{locked.Name}\", so it cannot go on \"{given.Name}\".");

    // An expense recorded before a backed category's account became its backing account, changed or
    // removed (§12, ruled at the scenario stage, 2; plan for increment 15, reading 4). Opgebouwd follows
    // the change, by the ruling of 2026-09-28, but what is there for the category on its account does
    // not, since the expense is on another account or was paid before it held anything. So, for each
    // backed category the expense was or becomes one of, before such a change: Opgebouwd less what is
    // there, which the change may move.
    private List<(Category Category, Money Gap)> GapsBefore(Expense current, Expense? changed) =>
        new[] { current.Category, changed?.Category }
            .OfType<Category>()
            .Distinct()
            .Where(c => backings.TryGetValue(c, out var b) && current.Id < b.HereSince.Id)
            .Select(c => (c, Gap(c)))
            .ToList();

    // After the change: whatever the gap moved by goes between the account the expense is on and the
    // backing account, so the money there follows Opgebouwd again. The adjustment changes what is
    // there, never Opgebouwd.
    private void CloseGaps(List<(Category Category, Money Gap)> before, Expense paidFrom)
    {
        foreach (var (category, gap) in before)
        {
            var backing = backings[category];
            var widened = Gap(category) - gap;
            if (widened == Money.Zero || paidFrom.Account == backing.Account) continue;

            if (widened.Cents > 0)
                Move(category, paidFrom.Account, backing.Account, widened, MovementReason.Adjusted, MovementDirection.Along);
            else
                Move(category, backing.Account, paidFrom.Account, -widened, MovementReason.Adjusted, MovementDirection.Along);
        }
    }

    private Money Gap(Category category) =>
        AccumulatedIn(category, backings[category], CurrentPeriod) - ThereFor(category, backings[category]);

    /// <summary>
    /// The one check an expense passes or fails, for recording and changing alike, in the fixed
    /// order <see cref="RecordExpense"/> describes: the category, then the amount, then the date.
    /// On success, the category the name refers to.
    /// </summary>
    private (ExpenseRefusal? Refusal, Category? Category) CheckExpense(
        decimal amountInEuros, string? categoryName, DateOnly date)
    {
        if (CategoryName.Normalise(categoryName) is null)
            return (ExpenseRefusal.CategoryMissing, null);

        if (Find(categoryName) is not { } category)
            return (ExpenseRefusal.UnknownCategory, null);

        if (amountInEuros <= 0)
            return (ExpenseRefusal.AmountNotPositive, null);

        if (!Money.IsWholeCents(amountInEuros))
            return (ExpenseRefusal.AmountFinerThanCent, null);

        if (date > Today)
            return (ExpenseRefusal.DateInFuture, null);

        return (null, category);
    }

    private int IndexOf(Expense expense)
    {
        var index = expenses.FindIndex(e => e.Id == expense.Id);
        return index >= 0
            ? index
            : throw new InvalidOperationException($"Expense {expense.Id} is not in the ledger.");
    }

    /// <summary>Every expense dated in a period, whatever its category, in the order recorded.</summary>
    public IReadOnlyList<Expense> ExpensesIn(BudgetPeriod period) =>
        expenses.Where(e => period.Contains(e.Date)).ToList();

    public IReadOnlyList<Expense> ExpensesFor(string categoryName, BudgetPeriod period) =>
        Find(categoryName) is { } category
            ? expenses.Where(e => e.Category == category && period.Contains(e.Date)).ToList()
            : [];

    public Money SpentOn(string categoryName, BudgetPeriod period) =>
        Money.Sum(ExpensesFor(categoryName, period).Select(e => e.Amount));

    /// <summary>
    /// The category's <i>Budget</i> minus everything spent against it — the one figure where the
    /// plan and the actual meet (arc42 §12). Goes negative when a category is overspent.
    /// </summary>
    public Money RemainingFor(string categoryName, BudgetPeriod period) =>
        BudgetFor(categoryName, period) - SpentOn(categoryName, period);

    /// <summary>
    /// Records an income, or refuses it for exactly one reason.
    ///
    /// <para>The checks mirror <see cref="RecordExpense"/>, including that their order is fixed
    /// rather than incidental: an income of -20 labelled "   " breaks two rules at once, and the
    /// label is checked first, so that is what the user is told.</para>
    ///
    /// <para><b>The date is not checked at all.</b> An income may be dated in the future and
    /// joins its period's <i>Unassigned</i> from the moment it is recorded — see
    /// <see cref="IncomeRefusal"/> for why that differs from an expense.</para>
    ///
    /// <para>The income is on <paramref name="account"/>, or on the pool account when none is given,
    /// as for an expense. A future-dated income reaches that account's balance only on its
    /// date.</para>
    ///
    /// <para>Given a <paramref name="repeat"/>, the income repeats from its date, as an expense does
    /// (<see cref="RecordExpense"/>). One dated ahead is its own first occurrence, and the next comes
    /// one step after its date.</para>
    /// </summary>
    public RecordIncomeResult RecordIncome(
        decimal amountInEuros, string? label, DateOnly date, Account? account = null, Frequency? repeat = null)
    {
        Settle();
        var on = CheckIsMine(account ?? pool);
        if (CheckIncome(amountInEuros, label) is { } refusal)
            return RecordIncomeResult.Refused(refusal);

        var income = new Income(++lastEntryId, Money.FromEuros(amountInEuros), date, NormaliseLabel(label)!, on);
        incomes.Add(income);
        StartRepeating(income, repeat);
        return RecordIncomeResult.Recorded(income);
    }

    /// <summary>
    /// Changes an income to the amount, label and date given, or refuses the change for exactly
    /// one of recording's reasons. Everything <see cref="ChangeExpense"/> says holds here too:
    /// judged as recording now, never refused when nothing changed, an overwrite that keeps the
    /// income's place, and an account left out stays as it is. A future date is allowed, as it is
    /// when recording.
    ///
    /// <para>Lowering an income, or moving it out of its period, may leave that period
    /// <i>Over-assigned</i>. Allowed, shown with the marker, and nothing more is said — in a past
    /// period for good, since nothing can be assigned there to balance it (arc42 §12).</para>
    ///
    /// <para><b>Throws</b> for an income that is not in the ledger.</para>
    ///
    /// <para>This keeps the repeat's frequency as it is; a changed date on the latest occurrence still
    /// moves its day (§12, follow-up 3). The overload that takes a frequency can change that too.</para>
    /// </summary>
    public ChangeIncomeResult ChangeIncome(
        Income income, decimal amountInEuros, string? label, DateOnly date, Account? account = null) =>
        ChangeIncome(income, amountInEuros, label, date, account, FrequencyOf(income));

    /// <summary>
    /// Changes an income and its repeat, as the frequency overload of <see cref="ChangeExpense"/>
    /// does for an expense.
    /// </summary>
    public ChangeIncomeResult ChangeIncome(
        Income income, decimal amountInEuros, string? label, DateOnly date, Account? account, Frequency? repeat)
    {
        Settle();
        var index = IndexOf(income);
        var current = incomes[index];
        var on = CheckIsMine(account ?? current.Account);
        var repeatChanged = SetsTheRepeat(current) && repeat != FrequencyOf(current);

        if (Money.IsWholeCents(amountInEuros)
            && Money.FromEuros(amountInEuros) == current.Amount
            && NormaliseLabel(label) == current.Label
            && date == current.Date
            && on == current.Account
            && !repeatChanged)
            return ChangeIncomeResult.Unchanged(current);

        if (CheckIncome(amountInEuros, label) is { } refusal)
            return ChangeIncomeResult.Refused(refusal);

        var changed = current with
        {
            Amount = Money.FromEuros(amountInEuros),
            Date = date,
            Label = NormaliseLabel(label)!,
            Account = on,
        };
        incomes[index] = changed;
        Repeat(current, changed, repeat);
        return ChangeIncomeResult.Changed(changed);
    }

    /// <summary>
    /// Removes an income, once the user has confirmed. It may leave its period
    /// <i>Over-assigned</i>, which is allowed and shown, never refused. An occurrence removed is that
    /// one alone, as for an expense (<see cref="RemoveExpense"/>). <b>Throws</b> for an income that is
    /// not in the ledger.
    /// </summary>
    public void RemoveIncome(Income income)
    {
        Settle();
        incomes.RemoveAt(IndexOf(income));
        NoLongerRepeats(income);
    }

    /// <summary>
    /// The one check an income passes or fails, for recording and changing alike, in the fixed
    /// order <see cref="RecordIncome"/> describes: the label, then the amount. The date is not
    /// checked.
    /// </summary>
    private static IncomeRefusal? CheckIncome(decimal amountInEuros, string? label)
    {
        if (NormaliseLabel(label) is null)
            return IncomeRefusal.LabelMissing;

        if (amountInEuros <= 0)
            return IncomeRefusal.AmountNotPositive;

        if (!Money.IsWholeCents(amountInEuros))
            return IncomeRefusal.AmountFinerThanCent;

        return null;
    }

    private int IndexOf(Income income)
    {
        var index = incomes.FindIndex(i => i.Id == income.Id);
        return index >= 0
            ? index
            : throw new InvalidOperationException($"Income {income.Id} is not in the ledger.");
    }

    public IReadOnlyList<Income> IncomesIn(BudgetPeriod period) =>
        incomes.Where(i => period.Contains(i.Date)).ToList();

    /// <summary>
    /// <i>Unassigned</i> for a period: its income minus everything assigned to categories in it
    /// (arc42 §12).
    ///
    /// <para>Everything assigned counts, including to a category since archived: archiving says
    /// nothing about money, so that category's <i>Budget</i> is still assigned money until it is
    /// taken back out. The figure goes negative when more is assigned than came in, which is
    /// <see cref="IsOverAssigned"/>.</para>
    ///
    /// <para>A period's income includes amounts <i>dated</i> later than today — future-dating is
    /// allowed and counts immediately (<see cref="IncomeRefusal"/>). This is where
    /// <i>Unassigned</i> and net worth part company on purpose: net worth is what you have today,
    /// <i>Unassigned</i> covers a whole period.</para>
    ///
    /// <para>Since increment 15 it also counts what was moved into it from <i>Opgebouwd</i> or
    /// <i>Vrij</i> (<see cref="Reallocate"/>), in the period each move is dated in, like an income
    /// (§12, <i>One act moves an amount of purpose</i>, derived).</para>
    /// </summary>
    public Money UnassignedIn(BudgetPeriod period) =>
        Money.Sum(IncomesIn(period).Select(i => i.Amount))
        + Money.Sum(reallocations
            .Where(r => r.To.Kind == ReallocationEndKind.Unassigned && period.Contains(r.Date))
            .Select(r => r.Amount))
        - Money.Sum(budgets.Where(b => b.Key.PeriodStart == period.FirstDay).Select(b => b.Value));

    /// <summary>
    /// Whether more has been assigned in a period than its income: a negative <i>Unassigned</i>.
    /// Exactly zero is every euro having a job, not over-assigned. Shown, never blocked and never
    /// warned about — the period's counterpart of <see cref="IsOverBudget"/> (arc42 §12,
    /// *Over-assigned*).
    /// </summary>
    public bool IsOverAssigned(BudgetPeriod period) => UnassignedIn(period).IsNegative;

    /// <summary>
    /// A label as it is stored: trimmed at the ends, left alone inside, and null when nothing
    /// survives. One rule for every label (arc42 §12, *A label is trimmed, and that is what makes
    /// "blank" mean anything*) — something has to trim a label in order to judge it blank, so
    /// trimming and requiring are one rule rather than two that could disagree.
    ///
    /// <para>What differs between the two transactions is only what each does with a null
    /// result: an income refuses it, an expense accepts it as having no label.</para>
    /// </summary>
    private static string? NormaliseLabel(string? label)
    {
        var trimmed = label?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    /// <summary>
    /// The category a name refers to under the name rule, in use or archived, or null when the
    /// name refers to none — including a name that trims to nothing.
    /// </summary>
    private Category? Find(string? name) =>
        CategoryName.Normalise(name) is { } stored && categories.TryGetValue(stored, out var category)
            ? category
            : null;

    /// <summary>
    /// Whether more has been spent against a category than was budgeted for it in this period.
    /// Exactly zero <i>Remaining</i> is not over budget — spending a category down to nothing is
    /// the plan working (arc42 §12, *Over budget*).
    /// </summary>
    public bool IsOverBudget(string categoryName, BudgetPeriod period) =>
        RemainingFor(categoryName, period).IsNegative;

    // ================================================================== accounts

    /// <summary>
    /// Every account, <b>the pool account first</b>, then the rest in the order added — the order
    /// of the strip and of every form's account list (arc42 §12, <i>Accounts and net worth</i>).
    /// Making another account the pool moves it to the front, and the old pool back to its place
    /// in the order added. A renamed account keeps its place.
    /// </summary>
    public IReadOnlyList<Account> Accounts =>
        [pool, .. accountsInOrderAdded.Where(a => a != pool)];

    /// <summary>
    /// The one account a new income or expense starts out on, when nothing else is chosen (arc42
    /// §12, <i>The pool account</i>). There is always exactly one.
    /// </summary>
    public Account PoolAccount => pool;

    /// <summary>The account a name refers to under the name rule, or null.</summary>
    public Account? AccountNamed(string? name) =>
        NameRule.Normalise(name) is { } stored && accounts.TryGetValue(stored, out var account) ? account : null;

    /// <summary>
    /// Adds an account, or refuses it for exactly one reason: a name that trims to nothing, then a
    /// name another account has, then a starting balance finer than a cent (arc42 §12).
    ///
    /// <para>A name another account has is <b>refused</b>, where a category's is handed back: handing
    /// the account back would drop the starting balance just typed. A category's name is free, since
    /// an account and a category are different dimensions.</para>
    ///
    /// <para>A <paramref name="startingBalance"/> is recorded as the account's first
    /// <see cref="BalanceCorrection"/>, dated today: what the bank says the account holds now. It
    /// may be zero or negative. <b>Null</b> — the field left empty — is <b>no starting balance</b>:
    /// the account's balance is then the plain sum of what is on it until it is first corrected,
    /// like the account a first start comes with. That is not the same as 0, which is a balance
    /// checked and has in it everything dated before today.</para>
    /// </summary>
    public AddAccountResult AddAccount(string? name, decimal? startingBalance)
    {
        Settle();
        var stored = NameRule.Normalise(name);
        if (stored is null)
            return AddAccountResult.Refused(AccountRefusal.NameMissing);

        if (accounts.ContainsKey(stored))
            return AddAccountResult.Refused(AccountRefusal.NameTaken);

        if (startingBalance is { } typed && !Money.IsWholeCents(typed))
            return AddAccountResult.Refused(AccountRefusal.AmountFinerThanCent);

        var account = NewAccount(stored);
        if (startingBalance is { } balance)
            balanceCorrections.Add(new BalanceCorrection(
                ++lastEntryId, Today, account, Money.FromEuros(balance), IsStartingBalance: true));

        return AddAccountResult.Added(account);
    }

    /// <summary>
    /// Gives an account a new name, under the rules for adding one: refused when it trims to
    /// nothing or another account has it; a new spelling of its own name is a rename; the name
    /// exactly as it is changes nothing. It stays the same account, in the same place, with
    /// everything on it. <b>Throws</b> for an account that is not in the ledger.
    /// </summary>
    public RenameAccountResult RenameAccount(Account account, string? newName)
    {
        Settle();
        CheckIsMine(account);

        var stored = NameRule.Normalise(newName);
        if (stored is null)
            return RenameAccountResult.Refused(RenameRefusal.NameMissing);

        if (stored == account.Name)
            return RenameAccountResult.Unchanged(account);

        if (accounts.TryGetValue(stored, out var holder) && holder != account)
            return RenameAccountResult.Refused(RenameRefusal.NameTaken);

        var oldName = account.Name;
        accounts.Remove(oldName);
        account.Name = stored;
        accounts.Add(stored, account);

        return RenameAccountResult.Renamed(oldName, account);
    }

    /// <summary>
    /// Whether an account can be deleted: it is <b>unused</b> — no income, expense, transfer or
    /// movement is on it now, whatever was once, and it backs no category — and it is not the pool
    /// account (arc42 §12). Its own starting balance and balance corrections do not count as use:
    /// deleting is for an account added by mistake, and those are the mistake. A movement stays on an
    /// account for good, as a transfer does, so an account money was moved through stays used after
    /// the category is unbacked; one that backed a category and never had money moved is unused
    /// again once it backs nothing. A movement from an account to itself — made while it was the
    /// pool account and backed a category — is in no history and moved no balance, so it does not
    /// count, as it does not for deleting a category (§12, <i>Backing: ruled after the build</i>).
    /// A sweep to itself does count, as it does for a category: an ended period's line stands on it.
    /// Since increment 15 a reallocation on it counts too, a move within the account included, and so
    /// does money a category set to "—" left on it (plan for increment 15, reading 9). A "—" that left
    /// nothing there does not, as an account that backed a category and never had money moved is
    /// unused again once it backs nothing.
    /// </summary>
    public bool CanDeleteAccount(Account account) =>
        account != pool
        && accountsInOrderAdded.Contains(account)
        && !expenses.Any(e => e.Account == account)
        && !incomes.Any(i => i.Account == account)
        && !transfers.Any(t => t.From == account || t.To == account)
        && !movements.Any(m => (m.From != m.To || m.Reason == MovementReason.Swept)
                               && (m.From == account || m.To == account))
        && !backings.Values.Any(b => b.Account == account)
        && !reallocations.Any(r => r.FromAccount == account || r.ToAccount == account)
        && !leftBehind.Any(l => l.Value.Account == account && LeftFigure(l.Key, l.Value, CurrentPeriod) != Money.Zero);

    /// <summary>
    /// Deletes an unused account, with its starting balance and balance corrections. Never asks
    /// first, and is announced afterwards, as deleting a category is (arc42 §12). Its name is free,
    /// and adding it again makes a new account, last in the order. <b>Throws</b> unless
    /// <see cref="CanDeleteAccount"/>: the act is offered only on such an account.
    /// </summary>
    public Account DeleteAccount(Account account)
    {
        Settle();
        if (!CanDeleteAccount(account))
            throw new InvalidOperationException($"\"{account.Name}\" cannot be deleted.");

        accounts.Remove(account.Name);
        accountsInOrderAdded.Remove(account);
        balanceCorrections.RemoveAll(c => c.Account == account);
        movements.RemoveAll(m => m.From == account && m.To == account);
        ForgetStretchesOn(account);
        return account;
    }

    // An unused account can still be named by a category's history of stretches: one it backed and
    // that was set to "—" with nothing left behind, before or since. Nothing ever moved there, so
    // that history is cut where it names the account, and what came before it is forgotten, as a
    // category backed again under version 7 started over: a "—" with nothing left is simply unbacked.
    private void ForgetStretchesOn(Account account)
    {
        // A "—" whose money was left on the account left nothing there, or the account would be in use.
        foreach (var (category, left) in leftBehind.ToList())
        {
            if (left.Account == account) leftBehind.Remove(category);
            else leftBehind[category] = left with { Before = Cut(left.Before) };
        }

        foreach (var (category, backing) in backings.ToList())
            backings[category] = Cut(backing);

        // The history from the stretch that names the account back is forgotten; everything after it,
        // and the money anything after it left behind, stays.
        Backing Cut(Backing backing) => backing.Earlier switch
        {
            null => backing,
            { } earlier when earlier.Account == account => backing with { Earlier = null },
            { } earlier => backing with { Earlier = earlier with { Before = Cut(earlier.Before) } },
        };
    }

    /// <summary>
    /// Makes an account the pool account. That changes which account a <b>new</b> entry starts out
    /// on and nothing else: every entry already recorded keeps its account (arc42 §12). <b>Throws</b>
    /// for an account that is not in the ledger, or that is already the pool — the act is offered
    /// only on the others.
    /// </summary>
    public void MakePool(Account account)
    {
        Settle();
        CheckIsMine(account);
        if (account == pool)
            throw new InvalidOperationException($"\"{account.Name}\" is already the pool account.");

        pool = account;
    }

    // ------------------------------------------------------------------ transfers

    /// <summary>
    /// Records a transfer, or refuses it for exactly one reason, in this order: the same account at
    /// both ends, an amount not above zero, an amount finer than a cent, a date after today (arc42
    /// §12). It moves two balances and no budget figure. <b>Throws</b> for an account that is not in
    /// the ledger.
    /// </summary>
    public RecordTransferResult RecordTransfer(decimal amountInEuros, Account from, Account to, DateOnly date)
    {
        Settle();
        if (CheckTransfer(amountInEuros, from, to, date) is { } refusal)
            return RecordTransferResult.Refused(refusal);

        var transfer = new Transfer(++lastEntryId, Money.FromEuros(amountInEuros), date, from, to);
        transfers.Add(transfer);
        return RecordTransferResult.Recorded(transfer);
    }

    /// <summary>
    /// Changes a transfer, judged as recording it now would be, and never refused when nothing
    /// changed — as <see cref="ChangeExpense"/>. It keeps its id, and so the moment it was first
    /// recorded. <b>Throws</b> for a transfer that is not in the ledger.
    /// </summary>
    public ChangeTransferResult ChangeTransfer(
        Transfer transfer, decimal amountInEuros, Account from, Account to, DateOnly date)
    {
        Settle();
        var index = IndexOf(transfer);
        var current = transfers[index];

        if (Money.IsWholeCents(amountInEuros)
            && Money.FromEuros(amountInEuros) == current.Amount
            && from == current.From
            && to == current.To
            && date == current.Date)
            return ChangeTransferResult.Unchanged(current);

        if (CheckTransfer(amountInEuros, from, to, date) is { } refusal)
            return ChangeTransferResult.Refused(refusal);

        var changed = current with { Amount = Money.FromEuros(amountInEuros), From = from, To = to, Date = date };
        transfers[index] = changed;
        return ChangeTransferResult.Changed(changed);
    }

    /// <summary>Removes a transfer, once the user has confirmed. <b>Throws</b> for one not in the ledger.</summary>
    public void RemoveTransfer(Transfer transfer)
    {
        Settle();
        transfers.RemoveAt(IndexOf(transfer));
    }

    private TransferRefusal? CheckTransfer(decimal amountInEuros, Account from, Account to, DateOnly date)
    {
        CheckIsMine(from);
        CheckIsMine(to);

        if (from == to)
            return TransferRefusal.SameAccount;

        if (amountInEuros <= 0)
            return TransferRefusal.AmountNotPositive;

        if (!Money.IsWholeCents(amountInEuros))
            return TransferRefusal.AmountFinerThanCent;

        if (date > Today)
            return TransferRefusal.DateInFuture;

        return null;
    }

    private int IndexOf(Transfer transfer)
    {
        var index = transfers.FindIndex(t => t.Id == transfer.Id);
        return index >= 0
            ? index
            : throw new InvalidOperationException($"Transfer {transfer.Id} is not in the ledger.");
    }

    // ------------------------------------------------------------------ balance corrections

    /// <summary>
    /// Records the balance the bank shows today for an account, as a <see cref="BalanceCorrection"/>
    /// dated today (arc42 §12). Zero and below zero are balances; the only refusal is a balance
    /// finer than a cent. Typing the figure MoneyBud already has is recorded too, with a difference
    /// of nothing: it is a check that the balance is right. <b>Throws</b> for an account not in the
    /// ledger.
    /// </summary>
    public CorrectBalanceResult CorrectBalance(Account account, decimal balanceInEuros)
    {
        Settle();
        CheckIsMine(account);
        if (!Money.IsWholeCents(balanceInEuros))
            return CorrectBalanceResult.RefusedFinerThanCent();

        var correction = new BalanceCorrection(
            ++lastEntryId, Today, account, Money.FromEuros(balanceInEuros), IsStartingBalance: false);
        balanceCorrections.Add(correction);
        return CorrectBalanceResult.Recorded(correction);
    }

    /// <summary>
    /// Removes a balance correction or a starting balance, once the user has confirmed. The balance
    /// is then worked out from the account's previous typed balance, or from none. There is no
    /// changing one: to change it, correct again. <b>Throws</b> for one not in the ledger.
    /// </summary>
    public void RemoveBalanceCorrection(BalanceCorrection correction)
    {
        Settle();
        var index = balanceCorrections.FindIndex(c => c.Id == correction.Id);
        if (index < 0)
            throw new InvalidOperationException($"Balance correction {correction.Id} is not in the ledger.");
        balanceCorrections.RemoveAt(index);
    }

    // ------------------------------------------------------------------ balances

    /// <summary>
    /// An account's <i>Balance</i> today, worked out and never stored (arc42 §12, ADR 0008): its
    /// latest typed balance, plus every income, expense and transfer on it dated today or earlier
    /// that the typed balance does not already have in it (<see cref="Holds"/>). With no typed
    /// balance at all, it is the plain sum of those, whatever their dates.
    ///
    /// <para>"Today or earlier" is what keeps a future-dated income out until its date: net worth is
    /// what you have today. Nothing about budgets enters into it.</para>
    /// </summary>
    public Money BalanceOf(Account account)
    {
        var latest = TypedBalancesOf(account).LastOrDefault();
        var moved = MovementsOn(account)
            .Where(m => m.Entry.Date <= Today && (latest is null || !Holds(latest, m.Entry)))
            .Select(m => m.Amount);

        return (latest?.Balance ?? Money.Zero) + Money.Sum(moved);
    }

    /// <summary>Net worth: the sum of every account's balance today (arc42 §12).</summary>
    public Money NetWorth => Money.Sum(accountsInOrderAdded.Select(BalanceOf));

    /// <summary>
    /// Whether an account's balance is below zero. Exactly zero is not overdrawn. Shown with the
    /// marker, never blocked or warned about (arc42 §12).
    /// </summary>
    public bool IsOverdrawn(Account account) => BalanceOf(account).IsNegative;

    /// <summary>
    /// How far a balance correction was from what MoneyBud had worked out just before it: what was
    /// typed, minus the previous typed balance (or nothing) plus everything this one has in it that
    /// the previous one did not. Negative when MoneyBud had more. <b>Worked out afresh</b>, so it
    /// always shows what is <i>still unexplained</i>: record a forgotten receipt dated before it and
    /// the difference shrinks, while the balance stays what was typed (arc42 §12).
    ///
    /// <para>Null for a starting balance, which corrected nothing.</para>
    /// </summary>
    public Money? DifferenceOf(BalanceCorrection correction)
    {
        if (correction.IsStartingBalance) return null;

        var typed = TypedBalancesOf(correction.Account);
        var index = typed.FindIndex(c => c.Id == correction.Id);
        if (index < 0)
            throw new InvalidOperationException($"Balance correction {correction.Id} is not in the ledger.");

        var previous = index > 0 ? typed[index - 1] : null;
        var since = MovementsOn(correction.Account)
            .Where(m => Holds(correction, m.Entry) && (previous is null || !Holds(previous, m.Entry)))
            .Select(m => m.Amount);

        return correction.Balance - ((previous?.Balance ?? Money.Zero) + Money.Sum(since));
    }

    /// <summary>
    /// Everything on an account, in every period: its starting balance and balance corrections, its
    /// transfers and movements either way, and the incomes and expenses on it. <b>Newest first</b>:
    /// by date, and on one date newest recorded first, as the Overview's lists are. Money planned for
    /// a later period is in no history until its day, because it is written only then. Every
    /// reallocation that touches the account is in it, one within the account included, although that
    /// moves no balance (§12, follow-up 9).
    /// </summary>
    public IReadOnlyList<IEntry> HistoryOf(Account account) =>
        MovementsOn(account).Select(m => m.Entry)
            .Concat(balanceCorrections.Where(c => c.Account == account))
            .Concat(reallocations.Where(r => !r.MovedMoney && r.FromAccount == account))
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.Id)
            .ToList();

    /// <summary>
    /// Whether a typed balance already has an entry in it: the entry is dated before the typed
    /// balance's day, or on that day and recorded before it (arc42 §12, <i>A typed balance is what
    /// the bank said that day</i>) — anything not after it, as <see cref="EntryMark"/> reads "after".
    /// A changed entry keeps its id, and so its first recording.
    /// </summary>
    private static bool Holds(BalanceCorrection typed, IEntry entry) =>
        entry.Id != typed.Id && !EntryMark.Of(typed).IsBefore(entry);

    private List<BalanceCorrection> TypedBalancesOf(Account account) =>
        balanceCorrections.Where(c => c.Account == account).OrderBy(c => c.Date).ThenBy(c => c.Id).ToList();

    /// <summary>
    /// What each income, expense, transfer, movement and reallocation on an account does to its
    /// balance: an income adds, an expense takes away, and a transfer, a movement or a reallocation
    /// between two accounts takes away from where it came from and adds where it went. A movement from an account to itself — the pool account backing a
    /// category — does nothing to it, so it is not here, and so it is in no history either (§12,
    /// <i>The pool account may back a category</i>).
    /// </summary>
    private IEnumerable<(IEntry Entry, Money Amount)> MovementsOn(Account account)
    {
        foreach (var income in incomes.Where(i => i.Account == account)) yield return (income, income.Amount);
        foreach (var expense in expenses.Where(e => e.Account == account)) yield return (expense, -expense.Amount);
        foreach (var transfer in transfers)
        {
            if (transfer.From == account) yield return (transfer, -transfer.Amount);
            if (transfer.To == account) yield return (transfer, transfer.Amount);
        }

        foreach (var movement in movements.Where(m => m.From != m.To))
        {
            if (movement.From == account) yield return (movement, -movement.Amount);
            if (movement.To == account) yield return (movement, movement.Amount);
        }

        foreach (var reallocation in reallocations.Where(r => r.MovedMoney))
        {
            if (reallocation.FromAccount == account) yield return (reallocation, -reallocation.Amount);
            if (reallocation.ToAccount == account) yield return (reallocation, reallocation.Amount);
        }
    }

    // ================================================================== backing

    /// <summary>The account backing a category, or null when it is unbacked or not one of the user's.</summary>
    public Account? BackingOf(string? categoryName) =>
        Find(categoryName) is { } category && backings.TryGetValue(category, out var backing) ? backing.Account : null;

    /// <summary>
    /// The account a category set to "—" left its older money on (arc42 §12, <i>Setting Staat op to
    /// "—"</i>), or null when it is backed, was never backed, or is not one of the user's. Its row shows
    /// it beside <i>Opgebouwd</i>: <i>"Opgebouwd € 5.000,00 op Spaarrekening"</i>.
    /// </summary>
    public Account? LeftOn(string? categoryName) =>
        Find(categoryName) is { } category && leftBehind.TryGetValue(category, out var left) ? left.Account : null;

    /// <summary>
    /// Backs a category with an account, points its backing at another account, or — given null —
    /// sets it to "—" (arc42 §12, <i>Backing can be set, changed or removed at any time</i>, and since
    /// increment 15 <i>Setting Staat op to "—"</i>). Never refused and never confirmed; the result says
    /// what moved, to be announced.
    ///
    /// <list type="bullet">
    /// <item><b>Backing</b> moves nothing already in the account. It moves the category's
    /// <i>Remaining</i> for the <b>current</b> period from the pool account to the backing account;
    /// an overspending moves the other way, from the backing account to the pool account, which paid it
    /// (follow-up 15). <i>Accumulated</i> starts at that <i>Remaining</i>. <b>Backed again after
    /// "—"</b>, it also takes the money "—" left behind along to the new account, and
    /// <i>Accumulated</i> carries on (ruling 6).</item>
    /// <item><b>Re-pointing</b> takes what is there for the category along, from the old account to
    /// the new one, or a shortfall the other way (ruled at the scenario stage, 1), and
    /// <i>Accumulated</i> carries on.</item>
    /// <item><b>"—"</b> sends only the current period's money back to the pool account, or an
    /// overspending of the current period back from it (ruling 6, follow-up 15): what is there for the
    /// category, less its <i>Accumulated</i> from before this period (plan for increment 15, reading
    /// 1). Everything older stays where it is, still the category's (<see cref="LeftBehind"/>). It is no
    /// longer the sweep destination.</item>
    /// </list>
    ///
    /// <para>"What is there for it" is <see cref="ThereFor"/>; when there is none, nothing moves. Every
    /// move may overdraw the account the money leaves. Money planned for a later period has not moved
    /// yet, so none of these touches it: it moves on its day to whatever backs the category then. An
    /// archived category can be backed like any other and stays archived.</para>
    ///
    /// <para><b>Choosing what is already set changes nothing</b> and moves nothing, and the result
    /// says so. A list on screen writes back what it shows, so this is the case it must be safe
    /// in.</para>
    ///
    /// <para><b>Throws</b> for a name that is not one of the user's categories, and for an account
    /// not in the ledger. Both are picked on screen, so neither is something the user can do.</para>
    /// </summary>
    public SetBackingResult SetBacking(string categoryName, Account? account)
    {
        var category = Find(categoryName)
            ?? throw new InvalidOperationException($"There is no category called \"{categoryName}\" to back.");
        if (account is not null) CheckIsMine(account);

        // Before settling: choosing what is set changes nothing, and so settles nothing either.
        if (BackingOf(category.Name) == account)
            return new SetBackingResult(category, BackingOutcome.Unchanged, account, account, []);

        Settle();
        backings.TryGetValue(category, out var current);
        var moves = new List<Movement>();

        if (current is null)
        {
            var remaining = RemainingFor(category.Name, CurrentPeriod);
            leftBehind.Remove(category, out var left);
            // Worked out before anything is written: the moves below come after the "—" mark.
            var carried = left is null ? Money.Zero : LeftFigure(category, left, CurrentPeriod);

            var mark = NewMark();
            backings[category] = new Backing(
                account!, mark, mark,
                NotMoved: BudgetFor(category.Name, CurrentPeriod) - remaining,
                PaidHereBefore: PaidSince(category, account!, CurrentPeriod.FirstDay),
                AccumulatingFrom: CurrentPeriod.FirstDay,
                HereFrom: CurrentPeriod.FirstDay,
                Earlier: left);

            if (remaining.Cents > 0)
                moves.Add(Move(category, pool, account!, remaining, MovementReason.Backed, MovementDirection.In));
            else if (remaining.IsNegative)
                moves.Add(Move(category, account!, pool, -remaining, MovementReason.Backed, MovementDirection.Out));

            if (carried.Cents > 0)
                moves.Add(Move(category, left!.Account, account!, carried, MovementReason.Rebacked, MovementDirection.In));
            else if (carried.IsNegative)
                moves.Add(Move(category, account!, left!.Account, -carried, MovementReason.Rebacked, MovementDirection.Out));

            return new SetBackingResult(category, BackingOutcome.Backed, null, account, moves);
        }

        // Worked out under the backing being left, before anything is written.
        var there = ThereFor(category, current);

        if (account is null)
        {
            var older = AccumulatedIn(category, current, CurrentPeriod) - RemainingFor(category.Name, CurrentPeriod);
            var returned = there - older;

            backings.Remove(category);
            // Only a backed category can be the destination (§12, ruling 6). Re-pointing keeps it.
            if (sweepDestination == category) sweepDestination = null;
            if (returned.Cents > 0)
                moves.Add(Move(category, current.Account, pool, returned, MovementReason.Unbacked, MovementDirection.Out));
            else if (returned.IsNegative)
                moves.Add(Move(category, pool, current.Account, -returned, MovementReason.Unbacked, MovementDirection.In));

            leftBehind[category] = new LeftBehind(current.Account, NewMark(), CurrentPeriod.FirstDay, older, current);
            return new SetBackingResult(category, BackingOutcome.Unbacked, current.Account, null, moves);
        }

        backings[category] = current with
        {
            Account = account, HereSince = NewMark(), PaidHereBefore = PaidSince(category, account, current.AccumulatingFrom),
            HereFrom = CurrentPeriod.FirstDay,
        };
        if (there.Cents > 0)
            moves.Add(Move(category, current.Account, account, there, MovementReason.Repointed, MovementDirection.Along));
        else if (there.IsNegative)
            moves.Add(Move(category, account, current.Account, -there, MovementReason.Repointed, MovementDirection.Along));
        return new SetBackingResult(category, BackingOutcome.Repointed, current.Account, account, moves);
    }

    /// <summary>
    /// What is there for a backed category in its backing account (arc42 §12, <i>follow-up</i> to
    /// <i>Backing can be set, changed or removed at any time</i>): what moved into that account on its
    /// behalf since the account became its backing account, minus what moved back out, minus the
    /// category's expenses paid <b>from that account</b> since. Since increment 15 "moved" includes
    /// what was reallocated into it or out of it (<see cref="Reallocate"/>), and a move between two
    /// accounts counts by which way it went: a shortfall taken along on re-pointing comes off. Null for
    /// a category that is not backed.
    ///
    /// <para>"Since" counts expenses by period, as <see cref="AccumulatedFor"/> does (§12, <i>ruling of
    /// 2026-09-28</i>): every expense from that account dated in the period the category was backed in,
    /// or later, whenever it was entered, less what the account had already paid for the category from
    /// that period on by the time it became the backing account (<see cref="Backing.PaidHereBefore"/>).
    /// That period is counted from the first day it had then (<see cref="Backing.AccumulatingFrom"/>),
    /// so a change of the period start day never changes this figure (§12, <i>follow-up 5</i>). Counting
    /// from there rather than from the period of the latest re-pointing is what keeps a late receipt,
    /// dated in an earlier period and locked on today's account, in this figure as it is in
    /// <i>Opgebouwd</i> (found by review at the build, increment 15).</para>
    ///
    /// <para>It is what "—" returns from, what re-pointing takes along, the most a negative assignment
    /// moves back, and the account's claim that <see cref="UnclaimedOf"/> takes off. Since every expense
    /// of a backed category is on its account (<see cref="LockedAccountFor"/>), it is the category's
    /// <i>Accumulated</i> in the current period, except in data kept before increment 15 with an
    /// expense on another account. Money planned for a later period has not moved yet, so it is not in
    /// here.</para>
    /// </summary>
    public Money? ThereFor(string categoryName) =>
        Find(categoryName) is { } category && backings.TryGetValue(category, out var backing)
            ? ThereFor(category, backing)
            : null;

    // In and out are counted by direction, not by the accounts' names: when the pool account backs the
    // category, money assigned moves from the pool to the pool and is still there for it. A move
    // between two accounts for the same purpose counts by where it went.
    private Money ThereFor(Category category, Backing backing)
    {
        var moved = Money.Sum(movements
            .Where(m => m.Category == category && backing.HereSince.IsBefore(m))
            .Select(m => m.Direction switch
            {
                MovementDirection.In => m.Amount,
                MovementDirection.Out => -m.Amount,
                _ => m.To == backing.Account ? m.Amount : -m.Amount,
            }));
        var reallocated = Money.Sum(reallocations
            .Where(r => backing.HereSince.IsBefore(r))
            .Select(r => r.For(category)));
        var spent = PaidSince(category, backing.Account, backing.AccumulatingFrom);
        return moved + reallocated - spent + backing.PaidHereBefore;
    }

    // What an account has paid for a category in expenses dated on a day or later, entered when they may be.
    private Money PaidSince(Category category, Account account, DateOnly from) =>
        Money.Sum(expenses
            .Where(e => e.Category == category && e.Account == account && e.Date >= from)
            .Select(e => e.Amount));

    // For data from before version 6, which did not remember the two figures (Backing): worked out
    // again on loading. What the account had paid before the mark is taken from the order entries
    // were recorded in, so it is exact unless such an expense was changed afterwards.
    private Money PaidBefore(Category category, Account account, EntryMark mark) =>
        Money.Sum(expenses
            .Where(e => e.Category == category && e.Account == account && e.Id < mark.Id
                        && Calendar.PeriodContaining(mark.Date).Contains(e.Date))
            .Select(e => e.Amount));

    // What did not move at backing is the period's Budget then, less what moved. The Budget then is
    // today's, less what was assigned to the period since: an assignment to a backed category moves
    // money on its day, which in the period of backing is a day in it. What moved at backing is the
    // movement drawn right after the mark.
    private Money NotMovedBefore(Category category, EntryMark mark)
    {
        var period = Calendar.PeriodContaining(mark.Date);
        var assignedSince = Money.Sum(movements
            .Where(m => m.Category == category && m.Reason == MovementReason.Assigned && m.Id > mark.Id
                        && period.Contains(m.Date))
            .Select(m => m.Direction == MovementDirection.Out ? -m.Amount : m.Amount));
        // Signed: since increment 15 an overspending moves out at backing. Data of version 5 never has
        // one, but the sign is what the figure means.
        var moved = movements
            .FirstOrDefault(m => m.Category == category && m.Reason == MovementReason.Backed && m.Id == mark.Id + 1)
            is { } backed ? Purpose(backed) : Money.Zero;
        var budget = budgets.GetValueOrDefault((category, period.FirstDay));
        var notMoved = budget - assignedSince - moved;
        return notMoved.Cents > 0 ? notMoved : Money.Zero;
    }

    /// <summary>
    /// A category's <i>Accumulated</i>, on screen <i>Opgebouwd</i>, as the period shown sees it (arc42
    /// §12, <i>Accumulated covers everything up to the period on screen</i>): what has moved in on its
    /// behalf since it was backed, minus what moved back out, minus every expense against it since — on
    /// any account — all up to the period's last day. For a period whose money has not moved yet, what
    /// is planned there and before it is added, since it will move on those periods' first days.
    ///
    /// <para>"Since it was backed" is <see cref="Backing.AccumulatingSince"/> for movements. For
    /// expenses it is the <b>period</b> of backing (§12, <i>ruling of 2026-09-28</i>): every expense
    /// dated in it or later counts, whenever it was entered, and <see cref="Backing.NotMoved"/> is
    /// added back, so in that period <i>Opgebouwd</i> moves with <i>Resterend</i>, a change to an
    /// earlier expense included. An expense dated before that period does not count: it is for the
    /// sweep. Below zero is allowed and shown. The period of backing is counted from the first day it
    /// had when the category was backed (<see cref="Backing.AccumulatingFrom"/>), so a change of the
    /// period start day never changes this figure (§12, follow-up 5).</para>
    ///
    /// <para><b>Since increment 15</b> (ADR 0015) "moved in" includes what was reallocated into it, and
    /// "out" what was reallocated out. And it no longer starts over: a category set to "—" keeps the
    /// money it left behind as its <i>Accumulated</i> (<see cref="LeftBehind"/>), shown while it is not
    /// zero, and setting an account again carries on from there. A period before the latest stretch is
    /// worked out by the stretch it falls in, so stepping back shows what each period had built.
    /// <b>Null</b> for a category never backed, or set to "—" with nothing left behind.</para>
    /// </summary>
    public Money? AccumulatedFor(string categoryName, BudgetPeriod period)
    {
        if (Find(categoryName) is not { } category) return null;
        if (backings.TryGetValue(category, out var backing)) return AccumulatedIn(category, backing, period);
        if (leftBehind.TryGetValue(category, out var left) && LeftFigure(category, left, period) is { Cents: not 0 } figure)
            return figure;
        return null;
    }

    private Money AccumulatedIn(Category category, Backing backing, BudgetPeriod period)
    {
        if (period.LastDay < backing.AccumulatingFrom && backing.Earlier is { } earlier)
            return LeftFigure(category, earlier, period);

        var since = backing.AccumulatingSince;
        var moved = Money.Sum(movements
            .Where(m => m.Category == category && since.IsBefore(m) && m.Date <= period.LastDay)
            .Select(Purpose));
        var reallocated = Money.Sum(reallocations
            .Where(r => since.IsBefore(r) && r.Date <= period.LastDay)
            .Select(r => r.For(category)));
        var from = backing.AccumulatingFrom;
        var spent = Money.Sum(expenses
            .Where(e => e.Category == category && e.Date >= from && e.Date <= period.LastDay)
            .Select(e => e.Amount));
        var notMoved = from <= period.LastDay ? backing.NotMoved : Money.Zero;
        var planned = Money.Sum(budgets
            .Where(b => b.Key.Category == category && b.Key.PeriodStart > settledThrough
                        && b.Key.PeriodStart <= period.FirstDay)
            .Select(b => b.Value));

        return moved + reallocated - spent + notMoved + planned;
    }

    // What a category set to "—" has, as a period sees it: what was left behind, less what was moved
    // out of it since, up to the period's last day. Its expenses do not count (§12, follow-up). A
    // period before the "—" is worked out by the backing it ended.
    private Money LeftFigure(Category category, LeftBehind left, BudgetPeriod period)
    {
        if (period.LastDay < left.From) return AccumulatedIn(category, left.Before, period);

        var moved = Money.Sum(movements
            .Where(m => m.Category == category && left.Since.IsBefore(m) && m.Date <= period.LastDay)
            .Select(Purpose));
        var reallocated = Money.Sum(reallocations
            .Where(r => left.Since.IsBefore(r) && r.Date <= period.LastDay)
            .Select(r => r.For(category)));
        return left.Amount + moved + reallocated;
    }

    // What a movement does to Accumulated: in adds, out takes away, along carries on.
    private static Money Purpose(Movement movement) => movement.Direction switch
    {
        MovementDirection.In => movement.Amount,
        MovementDirection.Out => -movement.Amount,
        _ => Money.Zero,
    };

    // ================================================================== Vrij, and moving Opgebouwd

    /// <summary>
    /// An account's <i>Vrij</i>, in English <i>Unclaimed</i>: the money on it that no category claims
    /// (arc42 §12, ruling 1). Its <see cref="BalanceOf">balance</see> today, less what is there for each
    /// category it backs (<see cref="ThereFor"/>), and less what each category set to "—" left on it.
    /// Worked out, never stored, like the balance (ADR 0008, ADR 0015). It is today's, the same in every
    /// period. It may be below zero — a fall in value the categories have not been told about — and is
    /// never adjusted by itself (ruling 3). <b>Null for the pool account</b>, which shows none: there,
    /// the period's <i>Niet toegewezen</i> plays that role (ruling 5).
    ///
    /// <para><b>Throws</b> for an account not in the ledger.</para>
    /// </summary>
    public Money? UnclaimedOf(Account account)
    {
        CheckIsMine(account);
        if (account == pool) return null;

        var claimed = Money.Sum(backings.Where(b => b.Value.Account == account).Select(b => ThereFor(b.Key, b.Value)))
                      + Money.Sum(leftBehind.Where(l => l.Value.Account == account)
                          .Select(l => LeftFigure(l.Key, l.Value, CurrentPeriod)));
        return BalanceOf(account) - claimed;
    }

    /// <summary>
    /// The account an expense against a category is locked on, or null when the expense form's list is
    /// open (arc42 §12, <i>An expense on a backed category is on its account</i>). A backed category's
    /// expense is on its backing account, <b>from the period the category got it</b> (follow-up 16): one
    /// dated earlier, when the category had no account, is open, as for any category without one. So
    /// is every category without an account, one set to "—" included.
    ///
    /// <para>For an expense being changed, <paramref name="editing"/>: one against the same category,
    /// <b>recorded before the category's account became its account</b> — by backing, or by the
    /// latest re-pointing — keeps the account it is on, locked there (§12, ruled at the scenario stage,
    /// 4; plan for increment 15, reading 3). Any difference a change of it makes is moved for it.</para>
    /// </summary>
    public Account? LockedAccountFor(string? categoryName, DateOnly date, Expense? editing = null)
    {
        if (Find(categoryName) is not { } category || !backings.TryGetValue(category, out var backing))
            return null;
        if (date < backing.AccumulatingFrom) return null;

        if (editing is not null && expenses.Find(e => e.Id == editing.Id) is { } kept
            && kept.Category == category && kept.Id < backing.HereSince.Id)
            return kept.Account;

        return backing.Account;
    }

    /// <summary>
    /// What a reallocation can take from, in the order added: <i>Vrij</i> on every account but the pool
    /// account, then every backed category, archived ones included, and every category set to "—"
    /// that has money left behind (arc42 §12, <i>One act moves an amount of purpose</i>, derived).
    /// </summary>
    public IReadOnlyList<ReallocationEnd> ReallocationSources =>
        accountsInOrderAdded.Where(a => a != pool).Select(ReallocationEnd.UnclaimedOn)
            .Concat(categoriesInOrderAdded
                .Where(c => backings.ContainsKey(c)
                            || (leftBehind.TryGetValue(c, out var left) && LeftFigure(c, left, CurrentPeriod) != Money.Zero))
                .Select(ReallocationEnd.For))
            .ToList();

    /// <summary>
    /// What a reallocation can go to, in the order added: <i>Vrij</i> on every account but the pool
    /// account, every backed category that is not archived, and <i>Niet toegewezen</i>.
    /// </summary>
    public IReadOnlyList<ReallocationEnd> ReallocationDestinations =>
        accountsInOrderAdded.Where(a => a != pool).Select(ReallocationEnd.UnclaimedOn)
            .Concat(categoriesInOrderAdded.Where(CanReceive).Select(ReallocationEnd.For))
            .Append(ReallocationEnd.Unassigned)
            .ToList();

    /// <summary>
    /// Moves an amount of purpose, on screen <i>Verplaatsen</i> (arc42 §12, ruling 2 and its follow-ups;
    /// ADR 0015): between an account's <i>Vrij</i>, a category's <i>Opgebouwd</i> and the current
    /// period's <i>Niet toegewezen</i>, which <paramref name="period"/> names when it is an end — the
    /// period on screen. A negative amount moves back, and is kept as the move the other way. Dated
    /// today, never confirmed, and it changes no <i>Budget</i> or <i>Remaining</i> in any period.
    ///
    /// <para><b>Money moves between accounts only when the two ends are on different accounts</b>
    /// (follow-up 8): the reallocation itself moves it. More than there is goes through, and may take
    /// <i>Vrij</i>, <i>Opgebouwd</i> or a balance below zero (follow-up 11). Zero is accepted and changes
    /// nothing. Taking from an archived category does not bring it back.</para>
    ///
    /// <para>Refused, for the first that applies (§12, readings of the scenario stage): the same end on
    /// both sides; <i>Vrij</i> on one account to <i>Vrij</i> on another, which gives nothing a purpose
    /// and is <i>Overboeken</i> (ruled at the build, 2026-10-04); an amount finer than a cent; money that would come <b>out of</b> <i>Niet
    /// toegewezen</i>, which is assigning (follow-up 7); money that would go <b>into</b> an archived
    /// category or one set to "—", which only give (follow-up 5); <i>Niet toegewezen</i> of a period
    /// other than the current one (follow-up 6). A refused reallocation changes nothing.</para>
    ///
    /// <para><b>Throws</b> for an end no list offers (plan for increment 15, reading 8): <i>Vrij</i> on
    /// the pool account or on an account not in the ledger, a category with no account and nothing left
    /// behind, and <i>Niet toegewezen</i> as <paramref name="from"/>. And for a period that is not one of
    /// <see cref="Calendar"/>'s own.</para>
    /// </summary>
    public ReallocateResult Reallocate(decimal amountInEuros, ReallocationEnd from, ReallocationEnd to, BudgetPeriod period)
    {
        Settle();
        CheckIsAPeriod(period);
        CheckIsAnEnd(from);
        CheckIsAnEnd(to);
        if (from.Kind == ReallocationEndKind.Unassigned)
            throw new InvalidOperationException("Niet toegewezen is only ever a destination.");

        if (from == to)
            return ReallocateResult.Refused(ReallocationRefusal.SameEnd);

        if (from.Kind == ReallocationEndKind.Unclaimed && to.Kind == ReallocationEndKind.Unclaimed)
            return ReallocateResult.Refused(ReallocationRefusal.UnclaimedToUnclaimed);

        if (!Money.IsWholeCents(amountInEuros))
            return ReallocateResult.Refused(ReallocationRefusal.AmountFinerThanCent);

        var amount = Money.FromEuros(amountInEuros);
        var (giver, receiver) = amount.IsNegative ? (to, from) : (from, to);
        if (giver.Kind == ReallocationEndKind.Unassigned)
            return ReallocateResult.Refused(ReallocationRefusal.OutOfUnassigned);
        if (receiver.Category is { } into && !CanReceive(into))
            return ReallocateResult.Refused(ReallocationRefusal.IntoGivingEnd, into);

        if ((from.Kind == ReallocationEndKind.Unassigned || to.Kind == ReallocationEndKind.Unassigned)
            && period != CurrentPeriod)
            return ReallocateResult.Refused(ReallocationRefusal.UnassignedNotCurrent);

        if (amount == Money.Zero)
            return ReallocateResult.Nothing;

        var reallocation = new Reallocation(
            ++lastEntryId, Today, giver, receiver, AccountOf(giver), AccountOf(receiver),
            amount.IsNegative ? -amount : amount);
        reallocations.Add(reallocation);
        return new ReallocateResult(reallocation, null);
    }

    // Money can be moved into a category that is backed and not archived (§12, follow-up 5; derived).
    private bool CanReceive(Category category) => backings.ContainsKey(category) && !archived.Contains(category);

    private void CheckIsAnEnd(ReallocationEnd end)
    {
        var offered = end.Kind switch
        {
            ReallocationEndKind.Unclaimed => end.Account is { } account && accountsInOrderAdded.Contains(account) && account != pool,
            ReallocationEndKind.Category => end.Category is { } category
                                            && categoriesInOrderAdded.Contains(category)
                                            && (backings.ContainsKey(category) || leftBehind.ContainsKey(category)),
            _ => true,
        };
        if (!offered)
            throw new InvalidOperationException($"{end} is not an end a reallocation can have.");
    }

    // The account an end is on now: Vrij its own, a category its backing account or the account it left
    // money on, and Niet toegewezen the pool account.
    private Account AccountOf(ReallocationEnd end) => end.Kind switch
    {
        ReallocationEndKind.Unclaimed => end.Account!,
        ReallocationEndKind.Category => backings.TryGetValue(end.Category!, out var backing)
            ? backing.Account
            : leftBehind[end.Category!].Account,
        _ => pool,
    };

    /// <summary>
    /// Ends every period that has ended, and moves the money planned for every period that has begun,
    /// since the last time this ran (arc42 §12, <i>Planned money follows the backing on the day it
    /// moves</i>, <i>When the sweep runs</i>; ADR 0009, ADR 0010). At each period boundary passed, in
    /// order:
    /// <list type="number">
    /// <item>The period that ended is recorded with the categories backed at that moment, which is
    /// what its leftover is worked out under from then on (<see cref="PeriodLeftover"/>).</item>
    /// <item>With a destination set and a leftover above zero, the leftover is <b>swept</b>: moved
    /// from the pool account to the destination's backing account, dated the new period's first day,
    /// and kept to be announced (<see cref="TakeSweepsMade"/>). At zero or below, or with no
    /// destination, nothing moves.</item>
    /// <item>Each backed category with a <i>Budget</i> above zero in the new period has it moved from
    /// the pool account to its backing account, dated that first day.</item>
    /// </list>
    /// The old period ends before the new one's money moves.
    ///
    /// <para><b>Since recurring entries it works through the days in order</b> (§12, follow-up 5;
    /// ADR 0011), from event to event: whichever comes first, a recurring entry's next occurrence that
    /// is due, or the next period boundary. An occurrence dated in a period is so recorded <b>before
    /// that period is swept</b>, as if MoneyBud had been open. On a boundary day the boundary comes
    /// first and that day's occurrences after it; several due on one day are recorded in the order
    /// their repeats were set up. An occurrence dated on or before the day already settled, as when a
    /// repeat is set up in the past, is recorded at once. Each is kept to be announced
    /// (<see cref="TakeOccurrencesMade"/>). Returns whether anything was moved or recorded.</para>
    ///
    /// <para>Every act that changes the ledger calls this first, and the screen calls it when it
    /// starts and once a minute. That is what makes the date right: nothing can change while
    /// MoneyBud is closed, so settling on 5 November for 1 November sees exactly the backing and the
    /// pool account 1 November had. And it gives each movement and occurrence the next id, lower than
    /// anything typed after it, so a balance correction typed on 1 November already has it in.</para>
    ///
    /// <para>Never goes backwards: a clock turned back passes no boundary and finds no occurrence
    /// due. A period that ended before the ledger was first made is passed by no settling, so it has
    /// no record and is never swept by itself (§12, ruling 7).</para>
    /// </summary>
    public bool Settle()
    {
        var today = Today;
        var changed = false;
        while (true)
        {
            var boundary = Calendar.Next(Calendar.PeriodContaining(settledThrough));
            var due = repeats.Where(r => r.Next <= today).MinBy(r => r.Next!.Value);

            if (due is not null && due.Next!.Value < boundary.FirstDay)
            {
                RecordOccurrence(due);
                changed = true;
            }
            else if (boundary.FirstDay <= today)
            {
                changed |= PassInto(boundary, boundary.FirstDay);
                settledThrough = boundary.FirstDay;
            }
            else
            {
                break;
            }
        }

        if (today > settledThrough) settledThrough = today;
        return changed;
    }

    // A period boundary: the period that ended, its sweep, and the new period's planned money, all
    // dated the day given — the new period's first day, unless a change of the start day passed it.
    // A change also says what of the new period's plan settling had already moved, by category.
    private bool PassInto(BudgetPeriod period, DateOnly on, IReadOnlyDictionary<Category, Money>? settledAlready = null)
    {
        var moved = false;
        var ended = Calendar.Previous(period);
        periodEnds[ended.FirstDay] = backings.Keys.ToHashSet();

        if (sweepDestination is { } destination && DifferenceFor(ended) is { Cents: > 0 } leftover)
        {
            var sweep = Move(destination, pool, backings[destination].Account, leftover,
                             MovementReason.Swept, MovementDirection.In, on, ended);
            sweepsMade.Add(new SweepMade(ended, sweep));
            moved = true;
        }

        foreach (var category in categoriesInOrderAdded)
        {
            if (backings.TryGetValue(category, out var backing)
                && budgets.TryGetValue((category, period.FirstDay), out var planned)
                && planned - (settledAlready?.GetValueOrDefault(category) ?? Money.Zero) is { Cents: > 0 } budget)
            {
                Move(category, pool, backing.Account, budget, MovementReason.Assigned, MovementDirection.In, on);
                moved = true;
            }
        }

        return moved;
    }

    // ================================================================== the period start day

    /// <summary>
    /// What changing the period start day to <paramref name="day"/> would do now, without doing it:
    /// the current period it would give, and the period it would end on the spot, if any. What the
    /// question asks about (arc42 §12, ruling 5). <see cref="ChangeStartDayResult.WasChanged"/> is
    /// false for the day already set.
    /// </summary>
    public ChangeStartDayResult PreviewStartDay(int day)
    {
        if (day == Calendar.StartDay)
            return new ChangeStartDayResult(day, WasChanged: false, CurrentPeriod, Ended: null);

        var current = CurrentPeriod;
        var changed = Calendar.ChangedFrom(current, day);
        var now = changed.PeriodContaining(Today);
        return new ChangeStartDayResult(
            day, WasChanged: true, now, now.FirstDay != current.FirstDay ? changed.Previous(now) : null);
    }

    /// <summary>
    /// Changes the day budget periods start on, from the current period on (arc42 §12, <i>A
    /// configurable period start day</i>; ADR 0012). Asking first is the screen's to do; by the time
    /// this is called, the user has confirmed. Never refused.
    /// <list type="bullet">
    /// <item>The current period keeps its first day and ends the day before <paramref name="day"/>
    /// first comes round after it; every later period starts on the new day, and every earlier one is
    /// as it was (<see cref="BudgetPeriodCalendar.ChangedFrom"/>).</item>
    /// <item>A plan made ahead for a period that no longer exists goes into the period its old first
    /// day falls in, adding to any plan already there (ruling 4). The current period's own plan stays
    /// with it, cut short or not (follow-up 4). Where the period it lands in has already begun, a
    /// backed category's money for it moves at once, dated today (follow-up 2): the new current
    /// period's, below, and the current period's own when it grows back over a period a change made
    /// earlier the same period had cut off — changed to the 30th on the 29th, planned ahead, and
    /// changed back to the 1st.</item>
    /// <item>When the new day has already come round, the current period <b>ends on the spot</b>, and
    /// the new current period is passed into at once, as settling passes any boundary, but <b>dated
    /// today</b> (follow-up 2): the ended period is recorded with what is backed now and swept, and the
    /// money planned for the new period moves. Settling itself cannot do it, since it looks for the
    /// next boundary after the day already settled, and this one lies before it. So the ended period is
    /// swept even when its new end falls before the first start (ruled at the scenario stage, 1).</item>
    /// </list>
    ///
    /// <para>Nothing else moves: no other money, no <i>Opgebouwd</i>, which counts from the first day
    /// the period of backing had (<see cref="Backing.AccumulatingFrom"/>), and no repeat, which keeps
    /// its day of the month.</para>
    ///
    /// <para><b>Choosing the day already set changes nothing</b>, and does not even settle: the
    /// drop-down writes back what it shows on every redraw, as <see cref="SetBacking"/>'s list does.</para>
    /// </summary>
    public ChangeStartDayResult ChangeStartDay(int day)
    {
        if (day is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(day), day, "A period starts on a day of the month.");

        if (day == Calendar.StartDay)
            return new ChangeStartDayResult(day, WasChanged: false, CurrentPeriod, Ended: null);

        Settle();
        var current = CurrentPeriod;
        var changed = Calendar.ChangedFrom(current, day);

        // A plan whose old first day settling has already passed — only when the clock was once ahead
        // and has been turned back — had its chance to move then, so the change moves none of it:
        // what it moves is only what is noted here as not yet settled.
        var intoCurrent = new List<(Category Category, Money Amount)>();
        var settledAlready = new Dictionary<Category, Money>();
        foreach (var ((category, start), amount) in budgets.Where(b => b.Key.PeriodStart > current.FirstDay).ToList())
        {
            var into = changed.PeriodContaining(start).FirstDay;
            if (into == start) continue;

            budgets.Remove((category, start));
            budgets[(category, into)] = budgets.GetValueOrDefault((category, into)) + amount;
            if (start <= settledThrough)
                settledAlready[category] = settledAlready.GetValueOrDefault(category) + amount;
            else if (into == current.FirstDay)
                intoCurrent.Add((category, amount));
        }

        Calendar = changed;
        var now = CurrentPeriod;
        if (now.FirstDay != current.FirstDay)
        {
            PassInto(now, Today, settledAlready);
            return new ChangeStartDayResult(day, WasChanged: true, now, Calendar.Previous(now));
        }

        // The current period goes on, and may have grown back over a period a change made earlier the
        // same period had begun to cut off: a plan made ahead for that period lands in this one, which
        // has begun, so a backed category's money for it moves now, dated today (follow-up 2).
        foreach (var (category, amount) in intoCurrent)
        {
            if (amount.Cents > 0 && backings.TryGetValue(category, out var backing))
                Move(category, pool, backing.Account, amount, MovementReason.Assigned, MovementDirection.In);
        }

        return new ChangeStartDayResult(day, WasChanged: true, now, Ended: null);
    }

    // ================================================================== recurring entries

    /// <summary>
    /// What the entry's <i>Herhalen</i> drop-down shows, and what its row's grey label says (arc42 §12,
    /// <i>Recurring entries</i>, ruling 6, follow-up 2): the frequency of the repeat whose latest
    /// occurrence it is, while that repeat runs. Null — one-off — for every other entry: a one-off, an
    /// earlier occurrence, and a stopped repeat's last one.
    /// </summary>
    public Frequency? FrequencyOf(IEntry entry) =>
        RepeatOf(entry) is { IsRunning: true } repeat && repeat.Latest == entry.Id ? repeat.Frequency : null;

    /// <summary>
    /// Whether the entry's drop-down can be changed: a one-off, the latest occurrence of a running
    /// repeat, or the last one of a stopped repeat, which is how it is started again (§12, follow-up
    /// 2; ruled at the scenario stage, 1). An earlier occurrence cannot: it would start a second
    /// repeat beside the running one.
    /// </summary>
    public bool SetsTheRepeat(IEntry entry) => RepeatOf(entry) is not { } repeat || repeat.Latest == entry.Id;

    /// <summary>
    /// The occurrences recorded by themselves since this was last asked, oldest first, and forgets
    /// them: each is announced once (§12, ruling 7), as <see cref="TakeSweepsMade"/>.
    /// </summary>
    public IReadOnlyList<OccurrenceMade> TakeOccurrencesMade()
    {
        var made = occurrencesMade.ToList();
        occurrencesMade.Clear();
        return made;
    }

    private RecurringEntry? RepeatOf(IEntry entry) => repeats.Find(r => r.Has(entry.Id));

    // A new entry set to repeat: what is already due is recorded at once (follow-up 4).
    private void StartRepeating(IEntry entry, Frequency? repeat)
    {
        if (repeat is not { } frequency) return;

        repeats.Add(RecurringEntry.StartingFrom(entry, frequency));
        Settle();
    }

    // A change that went through, to an entry that may set its repeat. For an earlier occurrence the
    // frequency is ignored: its drop-down is locked, and the change is to it alone.
    private void Repeat(IEntry before, IEntry after, Frequency? repeat)
    {
        if (!SetsTheRepeat(after)) return;

        if (RepeatOf(after) is not { } running)
        {
            StartRepeating(after, repeat);
            return;
        }

        if (repeat != FrequencyOf(after) || (repeat is not null && after.Date != before.Date))
            running.SetFrom(after.Date, repeat);
        Settle();
    }

    // A removed entry leaves its repeat; a repeat with nothing left ends.
    private void NoLongerRepeats(IEntry entry)
    {
        if (RepeatOf(entry) is { } repeat && !repeat.Removed(entry.Id))
            repeats.Remove(repeat);
    }

    // The next occurrence: a copy of the latest, on the repeat's next date, never checked and never
    // refused. One on an archived category brings it back (ruling 8). Since increment 15 an expense's
    // goes on the account its category's list is locked on that day, if it is.
    private void RecordOccurrence(RecurringEntry repeat)
    {
        var date = repeat.Next!.Value;
        Category? broughtBack = null;
        IEntry occurrence;
        if (expenses.Find(e => e.Id == repeat.Latest) is { } expense)
        {
            if (archived.Remove(expense.Category)) broughtBack = expense.Category;
            // On a backed category, the account backing it on its day (plan for increment 15, reading 10).
            var copy = expense with
            {
                Id = ++lastEntryId, Date = date,
                Account = LockedAccountFor(expense.Category.Name, date) ?? expense.Account,
            };
            expenses.Add(copy);
            occurrence = copy;
        }
        else
        {
            var income = incomes.Find(i => i.Id == repeat.Latest)
                ?? throw new InvalidOperationException($"A repeat's latest occurrence, {repeat.Latest}, is not in the ledger.");
            var copy = income with { Id = ++lastEntryId, Date = date };
            incomes.Add(copy);
            occurrence = copy;
        }

        repeat.Recorded(occurrence);
        occurrencesMade.Add(new OccurrenceMade(occurrence, broughtBack));
    }

    // ================================================================== the sweep

    /// <summary>The category a period's leftover goes to when the period ends, or null for none.</summary>
    public Category? SweepDestination => sweepDestination;

    /// <summary>
    /// What can be chosen as the sweep destination: every backed category that is not archived, in
    /// the order added (arc42 §12, <i>The destination is one list</i>). The screen shows them
    /// alphabetically, after "—" for none; that is how they are shown, so it is the screen's to do.
    /// </summary>
    public IReadOnlyList<Category> SweepDestinationChoices =>
        categoriesInOrderAdded.Where(c => backings.ContainsKey(c) && !archived.Contains(c)).ToList();

    /// <summary>
    /// Chooses where a period's leftover goes when the period ends, or — given null — that it goes
    /// nowhere (arc42 §12, <i>The destination is one list</i>). One setting for every period that
    /// ends from now on; a period that already ended is not swept by it (ruling 2). Never refused and
    /// never confirmed; the result says what changed, to be announced.
    ///
    /// <para><b>Choosing what is already set changes nothing</b>, and does not even settle: the list
    /// on screen writes back what it shows on every redraw, so this is the case it must be safe
    /// in, as <see cref="SetBacking"/> is.</para>
    ///
    /// <para><b>Throws</b> for a name that is not one of the user's categories, or names one that is
    /// not backed or is archived. The list offers only <see cref="SweepDestinationChoices"/>, so
    /// none of these is something the user can do.</para>
    /// </summary>
    public SetSweepDestinationResult SetSweepDestination(string? categoryName)
    {
        var category = categoryName is null
            ? null
            : Find(categoryName)
              ?? throw new InvalidOperationException($"There is no category called \"{categoryName}\" to sweep into.");

        if (category == sweepDestination)
            return new SetSweepDestinationResult(SweepDestinationOutcome.Unchanged, category, category);

        if (category is not null && (!backings.ContainsKey(category) || archived.Contains(category)))
            throw new InvalidOperationException($"\"{category.Name}\" is not backed, or is archived, so nothing can be swept into it.");

        Settle();
        var before = sweepDestination;
        sweepDestination = category;
        return new SetSweepDestinationResult(
            category is null ? SweepDestinationOutcome.Removed : SweepDestinationOutcome.Chosen, before, category);
    }

    /// <summary>
    /// A period's leftover, on screen <i>Restant</i> (arc42 §12, <i>What a period sweeps</i>): its
    /// <i>Unassigned</i> plus the <i>Remaining</i> of every category that was <b>not backed when the
    /// period ended</b>, negatives included, archived categories included. Backed categories are not
    /// part of it: their money has landed.
    ///
    /// <para>"Backed when the period ended" is what settling recorded as it passed the end, so it is
    /// the same answer months later, whatever was backed or unbacked since. A period that ended before
    /// the ledger was first made had nothing backed. A period that has not ended — the current one,
    /// a later one, or one whose end settling has not yet passed — is judged by today's backing,
    /// which is what its end will have unless something changes first.</para>
    /// </summary>
    public Money PeriodLeftover(BudgetPeriod period)
    {
        CheckIsAPeriod(period);
        var backedAtEnd = BackedAtEndOf(period);
        return UnassignedIn(period)
               + Money.Sum(categoriesInOrderAdded
                   .Where(c => !backedAtEnd.Contains(c))
                   .Select(c => RemainingFor(c.Name, period)));
    }

    private IReadOnlySet<Category> BackedAtEndOf(BudgetPeriod period) =>
        periodEnds.TryGetValue(period.FirstDay, out var recorded) ? recorded
        : period.LastDay >= settledThrough ? backings.Keys.ToHashSet()
        : new HashSet<Category>();

    /// <summary>
    /// What an ended period's line says, or null when it says nothing: in the current period and
    /// later ones, which show where their leftover will go and never how much (§12, ruling 10), and
    /// in an ended period with nothing to say. One line at a time, the first that applies:
    /// <list type="number">
    /// <item><b>Still to sweep</b>, when the difference is above zero. <i>Restant bijwerken</i> is
    /// offered only with a destination set (§12, ruled at the scenario stage, 3).</item>
    /// <item><b>Swept too much</b>, when it is below zero and some move into a category can still be
    /// undone, with the button.</item>
    /// <item><b>Swept</b>, when anything went: what really went to each category.</item>
    /// <item><b>A shortfall</b>, when the leftover is below zero: the negative figure.</item>
    /// </list>
    ///
    /// <para><b>The difference</b> is measured against what really moved for the period, net (§12,
    /// ruled at the scenario stage, 4; ruled at the build, 2026-09-28). What the period should sweep
    /// now, its leftover floored at zero, above what moved is still to sweep. Below what moved, it is
    /// swept too much only beyond what the line already let go: an amount let go stays let go, and a
    /// later rise in the leftover fills it before anything is still to sweep. A move into a category
    /// can be undone only while the category is still backed as it was when the money went in:
    /// backed now, and not unbacked and backed again since, since unbacking already returned the
    /// money. When none can, the line stops asking by itself and names what went; the backing mark
    /// never moves back, so that is for good, like what <see cref="BringUpToDate"/> lets go.</para>
    ///
    /// <para><b>Throws</b> for a period that is not one of <see cref="Calendar"/>'s own.</para>
    /// </summary>
    public SweepLine? SweepLineFor(BudgetPeriod period)
    {
        CheckIsAPeriod(period);
        if (period.FirstDay >= CurrentPeriod.FirstDay) return null;

        var leftover = PeriodLeftover(period);
        var difference = DifferenceFor(period);
        var parts = PartsFor(period);

        if (difference.Cents > 0)
            return new SweepLine(SweepLineKind.StillToSweep, parts, difference, sweepDestination is not null);

        if (difference.IsNegative && UndoableSweepsFor(period).Count > 0)
            return new SweepLine(SweepLineKind.SweptTooMuch, parts, -difference, CanBringUpToDate: true);

        if (parts.Count > 0)
            return new SweepLine(SweepLineKind.Swept, parts, Money.Sum(parts.Select(p => p.Amount)), CanBringUpToDate: false);

        if (leftover.IsNegative)
            return new SweepLine(SweepLineKind.Shortfall, [], leftover, CanBringUpToDate: false);

        return null;
    }

    /// <summary>
    /// <i>Restant bijwerken</i> on an ended period: moves exactly the difference its line shows,
    /// dated today, never confirmed (arc42 §12, <i>A swept period that changes</i>). It changes no
    /// <i>Budget</i> in any period, and the sweep it corrects stays as it was, beside it.
    /// <list type="bullet">
    /// <item><b>Still to sweep</b> goes to <b>today's</b> destination, from the pool account to its
    /// backing account.</item>
    /// <item><b>Swept too much</b> comes back to the pool account from the categories it went into,
    /// the <b>latest move for the period undone first</b>, then the one before (§12, follow-up; per
    /// move, ruled at the build, 2026-09-28). Each move gives back at most what is left of it, and a
    /// category at most what is there for it (<see cref="ThereFor"/>), from its <b>current</b>
    /// backing account — or, for a category set to "—", at most the money it left behind, from where
    /// it is (§12, follow-up 14). What comes back from one category is one movement. What none can give is
    /// <b>let go</b>: the line stops asking for it, for good.</item>
    /// </list>
    ///
    /// <para><b>Throws</b> when the line offers no button: it is not on screen then.</para>
    /// </summary>
    public BringUpToDateResult BringUpToDate(BudgetPeriod period)
    {
        Settle();
        var line = SweepLineFor(period);
        if (line is not { CanBringUpToDate: true })
            throw new InvalidOperationException($"{period} has nothing to bring up to date.");

        if (line.Kind == SweepLineKind.StillToSweep)
        {
            var destination = sweepDestination!;
            var sweep = Move(destination, pool, backings[destination].Account, line.Amount,
                             MovementReason.Swept, MovementDirection.In, sweptFor: period);
            return new BringUpToDateResult(period, [sweep], Money.Zero);
        }

        // Worked out move by move, latest first, then written as one movement per category, in the
        // order each category was first reached.
        var owed = line.Amount;
        var given = new Dictionary<Category, Money>();
        var order = new List<Category>();
        foreach (var (sweep, left) in UndoableSweepsFor(period))
        {
            var category = sweep.Category;
            var there = HeldFor(category) - given.GetValueOrDefault(category);
            var give = Smallest(owed, left, there);
            if (give.Cents <= 0) continue;

            if (!given.ContainsKey(category)) order.Add(category);
            given[category] = given.GetValueOrDefault(category) + give;
            owed -= give;
            if (owed == Money.Zero) break;
        }

        var moves = order
            .Select(c => Move(c, HeldOn(c), pool, given[c], MovementReason.Swept, MovementDirection.Out, sweptFor: period))
            .ToList();

        if (owed.Cents > 0)
            letGo[period.FirstDay] = LetGoFor(period) + owed;

        return new BringUpToDateResult(period, moves, owed);

        static Money Smallest(params Money[] amounts) => amounts.MinBy(a => a.Cents);
    }

    /// <summary>
    /// The sweeps settling made since this was last asked, oldest first, and forgets them: each is
    /// announced once (§12, <i>When the sweep runs</i>, follow-up).
    /// </summary>
    public IReadOnlyList<SweepMade> TakeSweepsMade()
    {
        var made = sweepsMade.ToList();
        sweepsMade.Clear();
        return made;
    }

    private IEnumerable<Movement> SweepsFor(BudgetPeriod period) =>
        movements.Where(m => m.Reason == MovementReason.Swept && m.SweptFor == period.FirstDay);

    private static Money Signed(Movement sweep) => sweep.Direction == MovementDirection.Out ? -sweep.Amount : sweep.Amount;

    private Money LetGoFor(BudgetPeriod period) => letGo.GetValueOrDefault(period.FirstDay);

    // Above zero, still to sweep; below zero, swept too much. Measured against what really moved,
    // and an amount let go is never asked for again: a rise in the leftover fills it first.
    private Money DifferenceFor(BudgetPeriod period)
    {
        var leftover = PeriodLeftover(period);
        var owed = leftover.IsNegative ? Money.Zero : leftover;
        var difference = owed - Money.Sum(SweepsFor(period).Select(Signed));
        if (!difference.IsNegative) return difference;

        var beyondLetGo = difference + LetGoFor(period);
        return beyondLetGo.IsNegative ? beyondLetGo : Money.Zero;
    }

    // What really went to each category for the period, in the order each first received money,
    // leaving off a category whose share was all taken back.
    private List<SweptPart> PartsFor(BudgetPeriod period) =>
        SweepsFor(period)
            .GroupBy(m => m.Category)
            .Select(g => new SweptPart(g.Key, Money.Sum(g.Select(Signed))))
            .Where(p => p.Amount.Cents > 0)
            .ToList();

    // The moves into categories for the period that can still be undone, latest first, each with
    // what is left of it. Only a category backed now, or set to "—" with its money left behind, counts,
    // and only its moves since its history of stretches began: before increment 15, unbacking
    // returned everything, and a category backed again started over (§12, follow-up 14). What was
    // taken back from a category already is counted against its latest moves first, as it was undone.
    private List<(Movement Sweep, Money Left)> UndoableSweepsFor(BudgetPeriod period)
    {
        var undoable = new List<(Movement, Money)>();
        foreach (var byCategory in SweepsFor(period).GroupBy(m => m.Category))
        {
            EntryMark first;
            if (backings.TryGetValue(byCategory.Key, out var backing)) first = backing.FirstSince;
            else if (leftBehind.TryGetValue(byCategory.Key, out var left)) first = left.Before.FirstSince;
            else continue;

            var since = byCategory.Where(m => first.IsBefore(m)).ToList();
            var takenBack = Money.Sum(since.Where(m => m.Direction == MovementDirection.Out).Select(m => m.Amount));
            foreach (var sweep in since.Where(m => m.Direction == MovementDirection.In).OrderByDescending(m => m.Id))
            {
                var undone = takenBack.Cents < sweep.Amount.Cents ? takenBack : sweep.Amount;
                takenBack -= undone;
                if (sweep.Amount != undone) undoable.Add((sweep, sweep.Amount - undone));
            }
        }

        return undoable.OrderByDescending(u => u.Item1.Id).ToList();
    }

    // What a category's money on an account comes to, and that account: a backed category's
    // backing account and what is there for it, or what a "—" left behind and where.
    private Money HeldFor(Category category) =>
        backings.TryGetValue(category, out var backing)
            ? ThereFor(category, backing)
            : LeftFigure(category, leftBehind[category], CurrentPeriod);

    private Account HeldOn(Category category) =>
        backings.TryGetValue(category, out var backing) ? backing.Account : leftBehind[category].Account;

    private Movement Move(
        Category category, Account from, Account to, Money amount, MovementReason reason,
        MovementDirection direction, DateOnly? date = null, BudgetPeriod? sweptFor = null)
    {
        var movement = new Movement(
            ++lastEntryId, date ?? Today, category, from, to, amount, reason, direction, sweptFor?.FirstDay);
        movements.Add(movement);
        return movement;
    }

    private EntryMark NewMark() => new(Today, ++lastEntryId);

    private Account NewAccount(string name)
    {
        var account = new Account(name);
        accounts.Add(name, account);
        accountsInOrderAdded.Add(account);
        return account;
    }

    /// <summary>
    /// The account itself, when it is one of this ledger's. An account is picked from a list, never
    /// typed, so one that is not here is a mistake in the caller: <b>throws</b>.
    /// </summary>
    private Account CheckIsMine(Account account) =>
        accountsInOrderAdded.Contains(account)
            ? account
            : throw new InvalidOperationException($"\"{account.Name}\" is not an account in this ledger.");
}
