using MoneyBud.Domain;

namespace MoneyBud.Presentation;

/// <summary>
/// Whether a figure carries the over marker. One marker serves both states it is used for, a
/// category over budget and a period over-assigned (arc42 §12, *One marker for over budget and
/// over-assigned*). It is information, never a warning: it sits beside the negative figure and
/// replaces nothing.
/// </summary>
public enum Marker
{
    None,
    Over,
}

/// <summary>A category's row on the Overview: its <i>Budget</i>, what was spent, and <i>Remaining</i>.</summary>
public sealed record CategoryRow(string Name, Money Budget, Money Spent, Money Remaining, bool IsArchived)
{
    public Marker Marker => Remaining.IsNegative ? Marker.Over : Marker.None;

    /// <summary>
    /// Which slice of the ring is this category's, counting from the first clockwise; null when
    /// it has none. Rows and slices share one order and the budgeted rows come first, so this is
    /// simply the row's place — it lets a list show each row in its slice's colour.
    /// </summary>
    public int? SliceIndex { get; init; }

    /// <summary>
    /// Whether the row carries the delete button: the category has no history in any period
    /// (arc42 §12, *Deleting a category that has no history anywhere*). The archive button is a
    /// separate act and stays, so nothing is decided for the user.
    /// </summary>
    public bool CanDelete { get; init; }

    /// <summary>Whether this row's name is being renamed, and so shows a text box in its place.</summary>
    public bool IsRenaming { get; init; }

    /// <summary>
    /// The figure this category would take over from the plan offered, shown in grey under its
    /// <i>Budget</i> (arc42 §12, <i>The offer is a button, and a figure on each row</i>). Null while
    /// no plan is offered, and for a category not in the plan — which shows no figure, not a zero.
    /// </summary>
    public Money? PlanFigure { get; init; }

    /// <summary>"plan: € 400,00", or null when the row shows no plan figure.</summary>
    public string? PlanText => PlanFigure is { } figure ? Tekst.PlanFigure(figure) : null;

    public bool IsOverBudget => Marker == Marker.Over;
    public string BudgetText => Tekst.Euro(Budget);
    public string SpentText => Tekst.Euro(Spent);
    public string RemainingText => Tekst.Euro(Remaining);
}

/// <summary>
/// An expense's row in the period's list. It carries the expense itself, because clicking the row
/// is how that expense is picked to be changed or removed (arc42 §12, *On screen: picking an entry
/// to correct*), and two identical rows are still two expenses.
/// </summary>
public sealed record ExpenseLine(Expense Entry)
{
    public DateOnly Date => Entry.Date;
    public string Category => Entry.Category.Name;
    public string? Label => Entry.Label;
    public Money Amount => Entry.Amount;
    public string DateText => Tekst.DayName(Date);
    public string AmountText => Tekst.Euro(Amount);
}

/// <summary>An income's row in the period's list, carrying the income for the reason <see cref="ExpenseLine"/> does.</summary>
public sealed record IncomeLine(Income Entry)
{
    public DateOnly Date => Entry.Date;
    public string Label => Entry.Label;
    public Money Amount => Entry.Amount;
    public string DateText => Tekst.DayName(Date);
    public string AmountText => Tekst.Euro(Amount);
}

/// <summary>
/// One budget period as the Overview shows it: the categories it lists, its ring, its
/// <i>Unassigned</i>, its expenses and incomes, and the plan it is offered, if any. Worked out
/// afresh from the ledger each time, so it never holds a figure that has since changed — and an
/// offer that has gone, because the period has passed or has a plan now, is simply not there.
/// </summary>
public sealed record PeriodOverview(
    BudgetPeriod Period,
    IReadOnlyList<CategoryRow> Rows,
    Ring Ring,
    Money Unassigned,
    IReadOnlyList<ExpenseLine> Expenses,
    IReadOnlyList<IncomeLine> Incomes,
    PlanOffer? Offer = null)
{
    /// <summary>Whether the button that takes the plan over is shown.</summary>
    public bool HasOffer => Offer is not null;

    /// <summary>
    /// The button's text, naming the period whose plan is offered and its total: "Plan van
    /// augustus 2026 overnemen (€ 1.450,00)". Null while no plan is offered.
    /// </summary>
    public string? OfferText => Offer is { } offer ? Tekst.TakeOverPlanButton(offer) : null;

    public bool IsOverAssigned => Unassigned.IsNegative;

    /// <summary>The same marker an overspent category carries (§12).</summary>
    public Marker UnassignedMarker => IsOverAssigned ? Marker.Over : Marker.None;

    public string UnassignedText => Tekst.Euro(Unassigned);

    /// <summary>What an empty ring says instead of being blank. Null when there is a ring to draw.</summary>
    public string? RingHint => Ring.IsEmpty ? Tekst.EmptyRing : null;

    /// <param name="renaming">The name of the category being renamed, if any, whose row shows a text box.</param>
    public static PeriodOverview Of(Ledger ledger, BudgetPeriod period, string? renaming = null)
    {
        var offer = ledger.PlanOfferedIn(period);

        // The display rule decides which categories are listed, in the order they were added.
        // Sorting by Budget is stable, so equal budgets — every zero among them — keep that
        // order, and a category brought back keeps the place it was first added in (§12).
        //
        // While a plan is offered every Budget is zero, so the plan figure decides instead, a row
        // with none counting as zero: that is the order the rows will have once the plan is taken
        // over, so nothing jumps (§12, *The figure on each row goes with the offer*). With no
        // offer every plan figure is null and the second key changes nothing.
        var rows = ledger.CategoriesShownIn(period)
            .Select(c => new CategoryRow(
                c.Name,
                ledger.BudgetFor(c.Name, period),
                ledger.SpentOn(c.Name, period),
                ledger.RemainingFor(c.Name, period),
                ledger.IsArchived(c.Name))
            {
                CanDelete = ledger.CanDelete(c.Name),
                IsRenaming = c.Name == renaming,
                PlanFigure = offer?.FigureFor(c),
            })
            .OrderByDescending(r => r.Budget.Cents)
            .ThenByDescending(r => r.PlanFigure?.Cents ?? 0)
            .ToList();

        var incomes = ledger.IncomesIn(period);
        var unassigned = ledger.UnassignedIn(period);
        var ring = Ring.Of(rows, unassigned, hasIncome: incomes.Count > 0);

        rows = rows
            .Select((row, i) => row with { SliceIndex = row.Budget.Cents > 0 && !ring.IsEmpty ? i : null })
            .ToList();

        return new PeriodOverview(
            period,
            rows,
            ring,
            unassigned,
            NewestFirst(ledger.ExpensesIn(period), e => e.Date).Select(e => new ExpenseLine(e)).ToList(),
            NewestFirst(incomes, i => i.Date).Select(i => new IncomeLine(i)).ToList(),
            offer);
    }

    /// <summary>
    /// Newest date first, and on the same date newest recorded first (§12). The ledger lists in
    /// the order recorded; reversing that and then sorting stably by date gives both at once.
    /// </summary>
    private static IEnumerable<T> NewestFirst<T>(IReadOnlyList<T> inOrderRecorded, Func<T, DateOnly> date) =>
        inOrderRecorded.Reverse().OrderByDescending(date);
}
