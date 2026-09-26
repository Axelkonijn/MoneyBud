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
/// <para>An entry — an expense or an income — can be changed or removed in any period, past ones
/// included (arc42 §12, *An entry can be changed or removed*). A change is judged exactly as
/// recording the changed entry now would be, by the same checks, and it <b>overwrites</b> the
/// entry: nothing remembers what it was, and it keeps its place in the order recorded. A category
/// can be renamed, and one with no history in any period can be deleted.</para>
///
/// <para>Not built yet: accounts, and with them backed categories and the pool account; carrying
/// last period's figures into a new one; and the end-of-period sweep (arc42 §12).</para>
/// </summary>
public sealed class Ledger
{
    private readonly TimeProvider clock;
    private readonly Dictionary<string, Category> categories = new(CategoryName.Comparer);
    private readonly List<Category> categoriesInOrderAdded = [];
    private readonly HashSet<Category> archived = [];
    private readonly Dictionary<(Category Category, DateOnly PeriodStart), Money> budgets = [];

    // In the order recorded. A change replaces an entry where it stands, so the order stays the
    // order the entries were first recorded in.
    private readonly List<Expense> expenses = [];
    private readonly List<Income> incomes = [];
    private int lastEntryId;

    public Ledger(TimeProvider clock, BudgetPeriodCalendar? calendar = null)
    {
        this.clock = clock;
        Calendar = calendar ?? new BudgetPeriodCalendar();
    }

    /// <summary>
    /// The categories MoneyBud ships with (arc42 §12, *The default categories*) — the
    /// stakeholder's own starting set, chosen to be tried rather than a claim about what a
    /// household needs. Dutch, because they are content the user reads, not test data.
    /// </summary>
    public static IReadOnlyList<string> DefaultCategoryNames { get; } =
        ["Boodschappen", "Huur", "Hobby", "Sparen", "Verzekeringen", "Abonnementen"];

    /// <summary>
    /// A MoneyBud used for the first time: the default categories and nothing else — nothing
    /// budgeted, nothing spent, no income.
    ///
    /// <para>The constructor, by contrast, gives an empty ledger. That is what the scenarios start
    /// from unless they are about the first start, which is how the suite proves that no other
    /// scenario leans on the defaults being there.</para>
    /// </summary>
    public static Ledger StartNew(TimeProvider clock, BudgetPeriodCalendar? calendar = null)
    {
        var ledger = new Ledger(clock, calendar);
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

        return new LedgerSnapshot(
            categoriesInOrderAdded.Select(c => new CategorySnapshot(keys[c], c.Name, archived.Contains(c))).ToList(),
            budgets.Select(b => new BudgetSnapshot(keys[b.Key.Category], b.Key.PeriodStart, b.Value)).ToList(),
            expenses.Select(e => new ExpenseSnapshot(e.Id, e.Amount, e.Date, keys[e.Category], e.Label)).ToList(),
            incomes.Select(i => new IncomeSnapshot(i.Id, i.Amount, i.Date, i.Label)).ToList(),
            lastEntryId);
    }

    /// <summary>
    /// A ledger holding exactly what a snapshot holds, as it was when the snapshot was taken.
    ///
    /// <para>Kept data is checked against every rule the ledger keeps while it runs, because it
    /// was read from outside MoneyBud and could have been changed there. <b>Throws</b>
    /// <see cref="InvalidDataException"/> for any that is broken — a name the name rule does not
    /// store, two names the rule counts as one, a budget or an expense pointing at no category, a
    /// budget below zero or outside a period's first day, an entry of zero or less, an income
    /// without a label, an id used twice or beyond the last one issued. Data like that cannot be
    /// read (arc42 §12, <i>When the data cannot be read</i>).</para>
    ///
    /// <para>Dates are not checked against today. An expense cannot be <i>recorded</i> in the
    /// future, but one recorded today is still valid kept data if the clock is later turned
    /// back.</para>
    /// </summary>
    public static Ledger FromSnapshot(
        LedgerSnapshot snapshot, TimeProvider clock, BudgetPeriodCalendar? calendar = null)
    {
        var ledger = new Ledger(clock, calendar);
        var byKey = new Dictionary<int, Category>();

        if (snapshot.LastEntryId < 0)
            throw Invalid("the last entry id issued is below zero");

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
            ledger.expenses.Add(new Expense(kept.Id, kept.Amount, kept.Date, CategoryFor(kept.Category), kept.Label));
        }

        foreach (var kept in snapshot.Incomes)
        {
            CheckEntry(kept.Id, kept.Amount);
            if (kept.Label is null || NormaliseLabel(kept.Label) != kept.Label)
                throw Invalid($"income {kept.Id} has no label, or one not stored as labels are");
            ledger.incomes.Add(new Income(kept.Id, kept.Amount, kept.Date, kept.Label));
        }

        ledger.lastEntryId = snapshot.LastEntryId;
        return ledger;

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

        void CheckEntry(int id, Money amount)
        {
            if (id < 1 || id > snapshot.LastEntryId)
                throw Invalid($"entry id {id} was never issued");
            if (!ids.Add(id))
                throw Invalid($"entry id {id} is used twice");
            if (amount.Cents <= 0)
                throw Invalid($"entry {id} is not more than zero");
        }

        static InvalidDataException Invalid(string what) => new($"The kept ledger cannot be read: {what}.");
    }

    public BudgetPeriodCalendar Calendar { get; }

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
        var category = Find(name)
            ?? throw new InvalidOperationException($"There is no category called \"{name}\" to archive.");

        if (!archived.Add(category))
            throw new InvalidOperationException($"\"{category.Name}\" is already archived.");

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
    /// </summary>
    public bool CanDelete(string name) =>
        Find(name) is { } category
        && !expenses.Any(e => e.Category == category)
        && !budgets.Any(b => b.Key.Category == category && b.Value.Cents > 0);

    /// <summary>
    /// Deletes a category with no history anywhere. It is gone: not archived, brought back by
    /// nothing, and its name is free, so adding the name afterwards creates a new category that
    /// goes last in the order added. Never asks first — nothing of value is lost (arc42 §12).
    /// Budgets of zero it still had go with it, since they are not history.
    ///
    /// <para><b>Throws</b> for a name that is not one of the user's categories, and for a category
    /// with history. The delete act is offered only on a category that <see cref="CanDelete"/>, so
    /// neither is something the user can do.</para>
    /// </summary>
    public Category DeleteCategory(string name)
    {
        var category = Find(name)
            ?? throw new InvalidOperationException($"There is no category called \"{name}\" to delete.");

        if (!CanDelete(name))
            throw new InvalidOperationException($"\"{category.Name}\" has history, so it cannot be deleted.");

        categories.Remove(category.Name);
        categoriesInOrderAdded.Remove(category);
        archived.Remove(category);
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
    /// only where it has history.
    ///
    /// <para>"Current" is read from the clock on every call, so a period that was current becomes
    /// past the moment the next one begins, with nothing rebuilt around it.</para>
    /// </summary>
    public IReadOnlyList<Category> CategoriesShownIn(BudgetPeriod period)
    {
        var plannable = period.FirstDay >= CurrentPeriod.FirstDay;

        return categoriesInOrderAdded
            .Where(c => (plannable && !archived.Contains(c)) || HasHistoryIn(c.Name, period))
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
    /// <para><b>Throws</b> for a period that is not one of <see cref="Calendar"/>'s own, such as a
    /// hand-made date range. A user picks a period from the calendar and cannot reach this, so no
    /// behaviour is defined for it; reaching it is a mistake in whatever called this.</para>
    /// </summary>
    public AssignResult Assign(decimal amountInEuros, string? categoryName, BudgetPeriod period)
    {
        if (Calendar.PeriodContaining(period.FirstDay) != period)
            throw new ArgumentException($"{period} is not a budget period.", nameof(period));

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

        var broughtBack = !amount.IsNegative && archived.Remove(category);
        return AssignResult.Assigned(category, shortfall, broughtBack);
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
    /// </summary>
    public RecordExpenseResult RecordExpense(
        decimal amountInEuros, string? categoryName, DateOnly date, string? label = null)
    {
        var (refusal, category) = CheckExpense(amountInEuros, categoryName, date);
        if (refusal is { } reason)
            return RecordExpenseResult.Refused(reason);

        var broughtBack = archived.Remove(category!);

        var expense = new Expense(
            ++lastEntryId, Money.FromEuros(amountInEuros), date, category!, NormaliseLabel(label));
        expenses.Add(expense);
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
    /// <para><b>Throws</b> for an expense that is not in the ledger, such as one already removed.
    /// A change is made from the expense's row, so that is not something the user can do.</para>
    /// </summary>
    public ChangeExpenseResult ChangeExpense(
        Expense expense, decimal amountInEuros, string? categoryName, DateOnly date, string? label)
    {
        var index = IndexOf(expense);
        var current = expenses[index];

        if (Money.IsWholeCents(amountInEuros)
            && Money.FromEuros(amountInEuros) == current.Amount
            && Find(categoryName) == current.Category
            && date == current.Date
            && NormaliseLabel(label) == current.Label)
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
        };
        expenses[index] = changed;
        return ChangeExpenseResult.Changed(changed, broughtBack);
    }

    /// <summary>
    /// Removes an expense. It is gone, and nothing keeps a copy. Asking first is the screen's to
    /// do (arc42 §12, *Removing an entry asks first*); by the time this is called, the user has
    /// confirmed. Removing is not new entry, so it never brings a category back.
    ///
    /// <para><b>Throws</b> for an expense that is not in the ledger.</para>
    /// </summary>
    public void RemoveExpense(Expense expense) => expenses.RemoveAt(IndexOf(expense));

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
    /// </summary>
    public RecordIncomeResult RecordIncome(decimal amountInEuros, string? label, DateOnly date)
    {
        if (CheckIncome(amountInEuros, label) is { } refusal)
            return RecordIncomeResult.Refused(refusal);

        var income = new Income(++lastEntryId, Money.FromEuros(amountInEuros), date, NormaliseLabel(label)!);
        incomes.Add(income);
        return RecordIncomeResult.Recorded(income);
    }

    /// <summary>
    /// Changes an income to the amount, label and date given, or refuses the change for exactly
    /// one of recording's reasons. Everything <see cref="ChangeExpense"/> says holds here too:
    /// judged as recording now, never refused when nothing changed, and an overwrite that keeps the
    /// income's place. A future date is allowed, as it is when recording.
    ///
    /// <para>Lowering an income, or moving it out of its period, may leave that period
    /// <i>Over-assigned</i>. Allowed, shown with the marker, and nothing more is said — in a past
    /// period for good, since nothing can be assigned there to balance it (arc42 §12).</para>
    ///
    /// <para><b>Throws</b> for an income that is not in the ledger.</para>
    /// </summary>
    public ChangeIncomeResult ChangeIncome(Income income, decimal amountInEuros, string? label, DateOnly date)
    {
        var index = IndexOf(income);
        var current = incomes[index];

        if (Money.IsWholeCents(amountInEuros)
            && Money.FromEuros(amountInEuros) == current.Amount
            && NormaliseLabel(label) == current.Label
            && date == current.Date)
            return ChangeIncomeResult.Unchanged(current);

        if (CheckIncome(amountInEuros, label) is { } refusal)
            return ChangeIncomeResult.Refused(refusal);

        var changed = current with
        {
            Amount = Money.FromEuros(amountInEuros),
            Date = date,
            Label = NormaliseLabel(label)!,
        };
        incomes[index] = changed;
        return ChangeIncomeResult.Changed(changed);
    }

    /// <summary>
    /// Removes an income, once the user has confirmed. It may leave its period
    /// <i>Over-assigned</i>, which is allowed and shown, never refused. <b>Throws</b> for an income
    /// that is not in the ledger.
    /// </summary>
    public void RemoveIncome(Income income) => incomes.RemoveAt(IndexOf(income));

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
    /// </summary>
    public Money UnassignedIn(BudgetPeriod period) =>
        Money.Sum(IncomesIn(period).Select(i => i.Amount))
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
}
