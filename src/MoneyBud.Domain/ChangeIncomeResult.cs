namespace MoneyBud.Domain;

/// <summary>
/// What came of trying to change an income: it was changed, nothing about it changed, or it was
/// refused for one of recording's own reasons. See <see cref="ChangeExpenseResult"/>, which this
/// mirrors; an income has no category, so there is nothing for a change to bring back.
///
/// <para>Lowering an income, or moving it out of its period, may leave that period
/// <i>Over-assigned</i>. That is a plain <see cref="ChangeOutcome.Changed"/>: allowed, shown with
/// the marker, and nothing more said (arc42 §12, *Removing or lowering an income may leave its
/// period over-assigned*).</para>
/// </summary>
public sealed record ChangeIncomeResult
{
    private ChangeIncomeResult(Income? income, ChangeOutcome? outcome, IncomeRefusal? refusal)
    {
        Income = income;
        Outcome = outcome;
        Refusal = refusal;
    }

    /// <summary>The income as it now is. Null when refused.</summary>
    public Income? Income { get; }

    public ChangeOutcome? Outcome { get; }

    public IncomeRefusal? Refusal { get; }

    public bool WasRefused => Refusal is not null;

    public static ChangeIncomeResult Changed(Income income) => new(income, ChangeOutcome.Changed, null);

    public static ChangeIncomeResult Unchanged(Income income) => new(income, ChangeOutcome.Unchanged, null);

    public static ChangeIncomeResult Refused(IncomeRefusal refusal) => new(null, null, refusal);
}
