namespace MoneyBud.Domain;

/// <summary>
/// What came of trying to record an expense: it was recorded, or it was refused for one reason.
///
/// <para>There is deliberately no third outcome. MoneyBud shows, it never blocks (arc42 §12), so
/// nothing here can mean "recorded with a warning" or "needs confirming" — going over budget
/// produces a plain <see cref="Recorded"/> like any other expense. That is what the scenario
/// *I should not be warned or asked to confirm* asserts, and it holds because the shape of this
/// type gives a warning nowhere to live.</para>
/// </summary>
public sealed record RecordExpenseResult
{
    private RecordExpenseResult(Expense? expense, ExpenseRefusal? refusal)
    {
        Expense = expense;
        Refusal = refusal;
    }

    public Expense? Expense { get; }

    public ExpenseRefusal? Refusal { get; }

    public bool WasRecorded => Expense is not null;

    public static RecordExpenseResult Recorded(Expense expense) => new(expense, null);

    public static RecordExpenseResult Refused(ExpenseRefusal refusal) => new(null, refusal);
}
