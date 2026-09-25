namespace MoneyBud.Domain;

/// <summary>
/// What came of trying to record an expense: it was recorded, or it was refused for one reason.
///
/// <para>There is deliberately no third outcome. MoneyBud shows, it never blocks (arc42 §12), so
/// nothing here can mean "recorded with a warning" or "needs confirming" — going over budget
/// produces a plain <see cref="Recorded"/> like any other expense. That is what the scenario
/// *I should not be warned or asked to confirm* asserts, and it holds because the shape of this
/// type gives a warning nowhere to live.</para>
///
/// <para><see cref="CategoryBroughtBack"/> is not a third outcome and not a warning. It is
/// information after the fact: the expense was recorded against an archived category, and that
/// brought the category back into use (arc42 §12, *Recording an expense against an archived
/// category brings it back*). The user is told, because nothing else about recording an expense
/// would show them that a category they put away is back in their list.</para>
/// </summary>
public sealed record RecordExpenseResult
{
    private RecordExpenseResult(Expense? expense, ExpenseRefusal? refusal, bool categoryBroughtBack)
    {
        Expense = expense;
        Refusal = refusal;
        CategoryBroughtBack = categoryBroughtBack;
    }

    public Expense? Expense { get; }

    public ExpenseRefusal? Refusal { get; }

    /// <summary>
    /// Whether recording this expense brought its archived category back. Never true for a
    /// refused expense: bringing back is a side-effect of recording, so an expense that was not
    /// recorded brought nothing back.
    /// </summary>
    public bool CategoryBroughtBack { get; }

    public bool WasRecorded => Expense is not null;

    public static RecordExpenseResult Recorded(Expense expense, bool categoryBroughtBack = false) =>
        new(expense, null, categoryBroughtBack);

    public static RecordExpenseResult Refused(ExpenseRefusal refusal) => new(null, refusal, false);
}
