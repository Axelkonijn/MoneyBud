namespace MoneyBud.Domain;

/// <summary>
/// What came of trying to record an income: it was recorded, or it was refused for one reason.
///
/// <para>There is deliberately no third outcome, for the same reason
/// <see cref="RecordExpenseResult"/> has none. MoneyBud shows, it never blocks (arc42 §12), so
/// nothing here can mean "recorded with a warning" or "needs confirming" — an income dated a year
/// ahead produces a plain <see cref="Recorded"/> like any other. That is what the scenario
/// *I should not be warned or asked to confirm* asserts, and it holds because the shape of this
/// type gives a warning nowhere to live.</para>
/// </summary>
public sealed record RecordIncomeResult
{
    private RecordIncomeResult(Income? income, IncomeRefusal? refusal)
    {
        Income = income;
        Refusal = refusal;
    }

    public Income? Income { get; }

    public IncomeRefusal? Refusal { get; }

    public bool WasRecorded => Income is not null;

    public static RecordIncomeResult Recorded(Income income) => new(income, null);

    public static RecordIncomeResult Refused(IncomeRefusal refusal) => new(null, refusal);
}
