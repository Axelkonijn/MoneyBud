namespace MoneyBud.Domain;

/// <summary>
/// The plan a period is offered while it has none of its own (arc42 §12, <i>Opening a period</i>):
/// the figures of the latest earlier period that has a plan, and which period that is.
///
/// <para>It is worked out afresh each time it is asked for, never stored. The offer is a state,
/// not an event: it is there whenever the conditions hold and gone otherwise, so there is nothing
/// to remember and nothing to open or close at a period boundary.</para>
/// </summary>
/// <param name="From">The period whose plan this is.</param>
/// <param name="Figures">Every category in that plan — a <i>Budget</i> above zero there, and not
/// archived now — in the order the categories were added. A category whose figure there was zero
/// is not in the plan.</param>
public sealed record PlanOffer(BudgetPeriod From, IReadOnlyList<PlanFigure> Figures)
{
    /// <summary>What taking the plan over assigns in all, added to the cent.</summary>
    public Money Total => Money.Sum(Figures.Select(f => f.Amount));

    /// <summary>The figure a category would take over, or null when it is not in the plan.</summary>
    public Money? FigureFor(Category category) =>
        Figures.FirstOrDefault(f => f.Category == category)?.Amount;
}

/// <summary>
/// One category's figure in a plan offered. It follows the category, not its name, so a category
/// renamed since is offered under its new name.
/// </summary>
public sealed record PlanFigure(Category Category, Money Amount);
