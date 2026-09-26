namespace MoneyBud.Domain;

/// <summary>
/// What came of trying to change an expense: it was changed, nothing about it changed, or it was
/// refused for one reason (arc42 §12, *A changed entry is judged as if it were recorded now*).
///
/// <para>The refusals are recording's own, <see cref="ExpenseRefusal"/>, because a change is
/// judged exactly as the changed expense would be if it were recorded now. A refused change
/// leaves the expense exactly as it was.</para>
///
/// <para><see cref="ChangeOutcome.Unchanged"/> is not a refusal and not a warning. Saving an
/// entry with nothing changed is never refused and goes through quietly, so it needs telling
/// apart from a change only because a change is announced and this is not (§12, *Changes and
/// renames are announced*).</para>
///
/// <para>As with <see cref="RecordExpenseResult"/>, <see cref="CategoryBroughtBack"/> is
/// information after the fact. It is true only when the change moved the expense <b>onto</b> an
/// archived category; fixing an expense that was already on one is correcting history, and brings
/// nothing back.</para>
/// </summary>
public sealed record ChangeExpenseResult
{
    private ChangeExpenseResult(
        Expense? expense, ChangeOutcome? outcome, ExpenseRefusal? refusal, bool categoryBroughtBack)
    {
        Expense = expense;
        Outcome = outcome;
        Refusal = refusal;
        CategoryBroughtBack = categoryBroughtBack;
    }

    /// <summary>The expense as it now is. Null when refused.</summary>
    public Expense? Expense { get; }

    public ChangeOutcome? Outcome { get; }

    public ExpenseRefusal? Refusal { get; }

    public bool CategoryBroughtBack { get; }

    public bool WasRefused => Refusal is not null;

    public static ChangeExpenseResult Changed(Expense expense, bool categoryBroughtBack) =>
        new(expense, ChangeOutcome.Changed, null, categoryBroughtBack);

    public static ChangeExpenseResult Unchanged(Expense expense) =>
        new(expense, ChangeOutcome.Unchanged, null, false);

    public static ChangeExpenseResult Refused(ExpenseRefusal refusal) => new(null, null, refusal, false);
}

/// <summary>What changing an entry did, when it was not refused.</summary>
public enum ChangeOutcome
{
    /// <summary>The entry now reads differently. Announced.</summary>
    Changed,

    /// <summary>What was saved is what the entry already was. Nothing changed, nothing is said.</summary>
    Unchanged,
}
