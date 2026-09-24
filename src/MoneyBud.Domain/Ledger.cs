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
/// <para>Not in this increment: accounts, assigning from the pool, and the end-of-period sweep.
/// A budget is set here directly, standing in for the act of assigning, which has no approved
/// scenarios yet — which is also why <see cref="UnassignedIn"/> has nothing to subtract.</para>
/// </summary>
public sealed class Ledger
{
    private readonly TimeProvider clock;
    private readonly Dictionary<string, Category> categories = new(StringComparer.Ordinal);
    private readonly Dictionary<(string Category, DateOnly PeriodStart), Money> budgets = [];
    private readonly List<Expense> expenses = [];
    private readonly List<Income> incomes = [];

    public Ledger(TimeProvider clock, BudgetPeriodCalendar? calendar = null)
    {
        this.clock = clock;
        Calendar = calendar ?? new BudgetPeriodCalendar();
    }

    public BudgetPeriodCalendar Calendar { get; }

    /// <summary>
    /// The day MoneyBud considers today, in the user's own time. How a configurable period start
    /// day should interact with time zones is still open (arc42 §8.2); on a single-machine
    /// desktop app local time is the reading that matches what the user sees on the wall.
    /// </summary>
    public DateOnly Today => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

    public BudgetPeriod CurrentPeriod => Calendar.PeriodContaining(Today);

    /// <summary>Adds a category, or does nothing if it is already there.</summary>
    public Category AddCategory(string name)
    {
        if (categories.TryGetValue(name, out var existing)) return existing;

        var category = new Category(name);
        categories.Add(name, category);
        return category;
    }

    public bool HasCategory(string name) => categories.ContainsKey(name);

    public void SetBudget(string categoryName, BudgetPeriod period, Money amount)
    {
        if (!HasCategory(categoryName))
            throw new InvalidOperationException($"There is no category called \"{categoryName}\".");

        budgets[(categoryName, period.FirstDay)] = amount;
    }

    public bool HasBudget(string categoryName, BudgetPeriod period) =>
        budgets.ContainsKey((categoryName, period.FirstDay));

    /// <summary>
    /// The plan for a category in a period. A category with no budget set behaves exactly as one
    /// budgeted at zero — there is no separate "unbudgeted" state (arc42 §12, *Budget*).
    /// </summary>
    public Money BudgetFor(string categoryName, BudgetPeriod period) =>
        budgets.TryGetValue((categoryName, period.FirstDay), out var amount) ? amount : Money.Zero;

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
    /// </summary>
    public RecordExpenseResult RecordExpense(
        decimal amountInEuros, string? categoryName, DateOnly date, string? label = null)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return RecordExpenseResult.Refused(ExpenseRefusal.CategoryMissing);

        if (!categories.TryGetValue(categoryName, out var category))
            return RecordExpenseResult.Refused(ExpenseRefusal.UnknownCategory);

        if (amountInEuros <= 0)
            return RecordExpenseResult.Refused(ExpenseRefusal.AmountNotPositive);

        if (!Money.IsWholeCents(amountInEuros))
            return RecordExpenseResult.Refused(ExpenseRefusal.AmountFinerThanCent);

        if (date > Today)
            return RecordExpenseResult.Refused(ExpenseRefusal.DateInFuture);

        var expense = new Expense(
            Money.FromEuros(amountInEuros), date, category, NormaliseLabel(label));
        expenses.Add(expense);
        return RecordExpenseResult.Recorded(expense);
    }

    public IReadOnlyList<Expense> ExpensesFor(string categoryName, BudgetPeriod period) =>
        expenses
            .Where(e => e.Category.Name == categoryName && period.Contains(e.Date))
            .ToList();

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
    /// Whether more has been spent against a category than was budgeted for it in this period.
    /// Exactly zero <i>Remaining</i> is not over budget — spending a category down to nothing is
    /// the plan working (arc42 §12, *Over budget*).
    /// </summary>
    public bool IsOverBudget(string categoryName, BudgetPeriod period) =>
        RemainingFor(categoryName, period).IsNegative;
}
