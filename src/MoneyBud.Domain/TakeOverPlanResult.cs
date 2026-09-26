namespace MoneyBud.Domain;

/// <summary>
/// What came of taking over the plan offered in a period: taken over in full, or refused.
///
/// <para>The only refusal is <see cref="AssignRefusal.PeriodInPast"/>. A take-over is assigning,
/// and a past period cannot be assigned in; that is reachable for up to a minute after a period
/// boundary, before the screen has caught up (arc42 §12, <i>Offered in the current period and
/// later ones</i>). None of assigning's other reasons can arise, because every figure in a plan
/// names a category the user has, in whole cents.</para>
///
/// <para>As with <see cref="AssignResult"/>, there is no outcome between the two. Going
/// <i>Over-assigned</i> is allowed and unwarned, and the plan is assigned in full whatever
/// <i>Unassigned</i> holds.</para>
/// </summary>
public sealed record TakeOverPlanResult
{
    private TakeOverPlanResult(PlanOffer? plan, BudgetPeriod? into, AssignRefusal? refusal)
    {
        Plan = plan;
        Into = into;
        Refusal = refusal;
    }

    /// <summary>The plan that was taken over. Null when refused.</summary>
    public PlanOffer? Plan { get; }

    /// <summary>The period the plan went into. Null when refused.</summary>
    public BudgetPeriod? Into { get; }

    public AssignRefusal? Refusal { get; }

    public bool WasTakenOver => Refusal is null;

    public static TakeOverPlanResult TakenOver(PlanOffer plan, BudgetPeriod into) => new(plan, into, null);

    public static TakeOverPlanResult Refused(AssignRefusal refusal) => new(null, null, refusal);
}
