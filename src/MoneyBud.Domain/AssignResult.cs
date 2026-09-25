namespace MoneyBud.Domain;

/// <summary>
/// What came of trying to assign an amount to a category: it was assigned, or it was refused for
/// one reason.
///
/// <para>As with <see cref="RecordExpenseResult"/>, there is no outcome between the two. Going
/// <i>Over-assigned</i> is allowed, unwarned and unblocked (arc42 §12), so it produces a plain
/// assignment, and the type gives a warning nowhere to live.</para>
///
/// <para>Two things are reported after the fact, and neither is a warning:</para>
/// <list type="bullet">
/// <item><see cref="Shortfall"/> — how much of a negative amount could not come back, because
/// the <i>Budget</i> did not hold it. The <i>Budget</i> floors at zero, so −50 against 30 moves 30
/// and reports 20. Never silently absorbed (§12, *An over-large negative assignment is
/// clipped*).</item>
/// <item><see cref="CategoryBroughtBack"/> — a positive amount assigned to an archived category
/// brought it back into use (§12, *Only a positive assignment brings it back*).</item>
/// </list>
/// </summary>
public sealed record AssignResult
{
    private AssignResult(
        Category? category, AssignRefusal? refusal, Money shortfall, bool categoryBroughtBack)
    {
        Category = category;
        Refusal = refusal;
        Shortfall = shortfall;
        CategoryBroughtBack = categoryBroughtBack;
    }

    /// <summary>The category assigned to, spelled as MoneyBud has it. Null when refused.</summary>
    public Category? Category { get; }

    public AssignRefusal? Refusal { get; }

    /// <summary>
    /// How much of a negative amount could not come back. Zero when all of it did, for every
    /// positive or zero amount, and for a refused assignment — which moved nothing, so there was
    /// nothing to clip.
    /// </summary>
    public Money Shortfall { get; }

    /// <summary>
    /// Whether this assignment brought its archived category back. Only ever true for a positive
    /// amount that was assigned.
    /// </summary>
    public bool CategoryBroughtBack { get; }

    public bool WasAssigned => Refusal is null;

    public static AssignResult Assigned(Category category, Money shortfall, bool categoryBroughtBack) =>
        new(category, null, shortfall, categoryBroughtBack);

    public static AssignResult Refused(AssignRefusal refusal) => new(null, refusal, Money.Zero, false);
}
