namespace MoneyBud.Domain;

/// <summary>
/// Everything MoneyBud knows: the categories, what was budgeted for each in each period, the
/// expenses recorded against them, and the income recorded into each period.
///
/// <para>Nothing is stored. State lives here for the lifetime of a run and is gone afterwards —
/// a deliberate deferral, with its reasoning and its trigger in arc42 §8.3.</para>
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
/// <para>Not in this increment: accounts, assigning from the pool, and the end-of-period sweep.
/// A budget is set here directly, standing in for the act of assigning, which has no approved
/// scenarios yet — which is also why <see cref="UnassignedIn"/> has nothing to subtract.</para>
/// </summary>
public sealed class Ledger
{
    private readonly TimeProvider clock;
    private readonly Dictionary<string, Category> categories = new(CategoryName.Comparer);
    private readonly List<Category> categoriesInOrderAdded = [];
    private readonly HashSet<Category> archived = [];
    private readonly Dictionary<(Category Category, DateOnly PeriodStart), Money> budgets = [];
    private readonly List<Expense> expenses = [];
    private readonly List<Income> incomes = [];

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
    /// The categories offered when recording something new: every category that is not archived,
    /// in the order they were added. No order is specified (arc42 §12 has no ordering rule);
    /// this one is merely stable.
    /// </summary>
    public IReadOnlyList<Category> CategoriesOffered =>
        categoriesInOrderAdded.Where(c => !archived.Contains(c)).ToList();

    public IReadOnlyList<Category> ArchivedCategories =>
        categoriesInOrderAdded.Where(archived.Contains).ToList();

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
    /// <para>There is no general "is shown" query beside it yet, because no view needs one. The
    /// full rule is settled (§12): a category is shown in a period where it has history, and a
    /// category <i>in use</i> is also shown in the current period and every later one, where it
    /// can be planned for. The increment that builds a period view implements that rule; until
    /// then this ledger states the fact the rule is built from.</para>
    /// </summary>
    public bool HasHistoryIn(string categoryName, BudgetPeriod period) =>
        BudgetFor(categoryName, period).Cents > 0 || ExpensesFor(categoryName, period).Count > 0;

    /// <summary>
    /// Sets a plan directly. Stands in for the act of assigning, which has no approved scenarios
    /// yet (arc42 §8.1), and so is scaffolding: it works on an archived category too, and does
    /// not bring it back. That is a known gap, not a rule — §12 settles that assigning to an
    /// archived category's name brings it back, as recording an expense does. The assigning
    /// increment builds the real act to that rule.
    /// </summary>
    public void SetBudget(string categoryName, BudgetPeriod period, Money amount)
    {
        var category = Find(categoryName)
            ?? throw new InvalidOperationException($"There is no category called \"{categoryName}\".");

        budgets[(category, period.FirstDay)] = amount;
    }

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
        if (CategoryName.Normalise(categoryName) is null)
            return RecordExpenseResult.Refused(ExpenseRefusal.CategoryMissing);

        if (Find(categoryName) is not { } category)
            return RecordExpenseResult.Refused(ExpenseRefusal.UnknownCategory);

        if (amountInEuros <= 0)
            return RecordExpenseResult.Refused(ExpenseRefusal.AmountNotPositive);

        if (!Money.IsWholeCents(amountInEuros))
            return RecordExpenseResult.Refused(ExpenseRefusal.AmountFinerThanCent);

        if (date > Today)
            return RecordExpenseResult.Refused(ExpenseRefusal.DateInFuture);

        var broughtBack = archived.Remove(category);

        var expense = new Expense(
            Money.FromEuros(amountInEuros), date, category, NormaliseLabel(label));
        expenses.Add(expense);
        return RecordExpenseResult.Recorded(expense, broughtBack);
    }

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
        var trimmed = NormaliseLabel(label);
        if (trimmed is null)
            return RecordIncomeResult.Refused(IncomeRefusal.LabelMissing);

        if (amountInEuros <= 0)
            return RecordIncomeResult.Refused(IncomeRefusal.AmountNotPositive);

        if (!Money.IsWholeCents(amountInEuros))
            return RecordIncomeResult.Refused(IncomeRefusal.AmountFinerThanCent);

        var income = new Income(Money.FromEuros(amountInEuros), date, trimmed);
        incomes.Add(income);
        return RecordIncomeResult.Recorded(income);
    }

    public IReadOnlyList<Income> IncomesIn(BudgetPeriod period) =>
        incomes.Where(i => period.Contains(i.Date)).ToList();

    /// <summary>
    /// <i>Unassigned</i> for a period: its income minus everything assigned to categories in it
    /// (arc42 §12).
    ///
    /// <para>Nothing assigns yet, so the subtraction has nothing to subtract and this is the
    /// period's income. It is named for the figure rather than for today's arithmetic because
    /// the figure is what the scenarios assert and what the user is shown; when assigning
    /// arrives it subtracts from this, and nothing here needs renaming.</para>
    ///
    /// <para>A period's income includes amounts <i>dated</i> later than today — future-dating is
    /// allowed and counts immediately (<see cref="IncomeRefusal"/>). This is where
    /// <i>Unassigned</i> and net worth part company on purpose: net worth is what you have today,
    /// <i>Unassigned</i> covers a whole period.</para>
    /// </summary>
    public Money UnassignedIn(BudgetPeriod period) =>
        Money.Sum(IncomesIn(period).Select(i => i.Amount));

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
