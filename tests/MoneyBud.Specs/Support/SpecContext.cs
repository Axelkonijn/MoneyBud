using MoneyBud.Domain;
using MoneyBud.Presentation;

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
/// </summary>
public sealed class SpecContext
{
    /// <summary>
    /// The day every scenario starts on. Mid-month, so that "today" is neither the first nor the
    /// last day of a period, and with February as the previous month, so a period of a length
    /// other than 31 days is exercised by the boundary scenarios.
    /// </summary>
    public static readonly DateOnly Today = new(2026, 3, 15);

    private readonly FixedClock clock = new(Noon(Today));

    private MoneyBudApp? app;

    public SpecContext() => Ledger = new Ledger(clock);

    public Ledger Ledger { get; private set; }

    /// <summary>
    /// MoneyBud as the user meets it: the screen over <see cref="Ledger"/>. Every <c>When</c>
    /// step acts through this, never on the ledger directly, so each scenario — the domain ones
    /// included — goes through the same doors the Desktop uses. <c>Given</c> steps set up the
    /// ledger directly: setting up is not what is under test.
    ///
    /// <para>Made on first use, so that it opens on whatever period is current once the Givens
    /// have fixed today, as MoneyBud opens on the current period when it starts.</para>
    /// </summary>
    public MoneyBudApp App => app ??= new MoneyBudApp(Ledger);

    /// <summary>
    /// A MoneyBud used for the first time, through the same door the app will use.
    ///
    /// <para>This replaces the ledger, so anything an earlier step set up would be lost without a
    /// word. A first start comes first, so an earlier setup means the scenario is written in the
    /// wrong order — and that fails here rather than silently.</para>
    /// </summary>
    public void StartUsingMoneyBudForTheFirstTime()
    {
        if (Ledger.CategoriesOffered.Count > 0 || Ledger.ArchivedCategories.Count > 0)
            throw new InvalidOperationException(
                "Starting MoneyBud for the first time must come before any other setup.");

        Ledger = Ledger.StartNew(clock);
        app = null;
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
