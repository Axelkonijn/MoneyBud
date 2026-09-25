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

    public bool IsOverBudget => Marker == Marker.Over;
    public string BudgetText => Tekst.Euro(Budget);
    public string SpentText => Tekst.Euro(Spent);
    public string RemainingText => Tekst.Euro(Remaining);
}

public sealed record ExpenseLine(DateOnly Date, string Category, string? Label, Money Amount)
{
    public string DateText => Tekst.DayName(Date);
    public string AmountText => Tekst.Euro(Amount);
}

public sealed record IncomeLine(DateOnly Date, string Label, Money Amount)
{
    public string DateText => Tekst.DayName(Date);
    public string AmountText => Tekst.Euro(Amount);
}

/// <summary>
/// One budget period as the Overview shows it: the categories it lists, its ring, its
/// <i>Unassigned</i>, and its expenses and incomes. Worked out afresh from the ledger each time,
/// so it never holds a figure that has since changed.
/// </summary>
public sealed record PeriodOverview(
    BudgetPeriod Period,
    IReadOnlyList<CategoryRow> Rows,
    Ring Ring,
    Money Unassigned,
    IReadOnlyList<ExpenseLine> Expenses,
    IReadOnlyList<IncomeLine> Incomes)
{
    public bool IsOverAssigned => Unassigned.IsNegative;

    /// <summary>The same marker an overspent category carries (§12).</summary>
    public Marker UnassignedMarker => IsOverAssigned ? Marker.Over : Marker.None;

    public string UnassignedText => Tekst.Euro(Unassigned);

    /// <summary>What an empty ring says instead of being blank. Null when there is a ring to draw.</summary>
    public string? RingHint => Ring.IsEmpty ? Tekst.EmptyRing : null;

    public static PeriodOverview Of(Ledger ledger, BudgetPeriod period)
    {
        // The display rule decides which categories are listed, in the order they were added.
        // Sorting by Budget is stable, so equal budgets — every zero among them — keep that
        // order, and a category brought back keeps the place it was first added in (§12).
        var rows = ledger.CategoriesShownIn(period)
            .Select(c => new CategoryRow(
                c.Name,
                ledger.BudgetFor(c.Name, period),
                ledger.SpentOn(c.Name, period),
                ledger.RemainingFor(c.Name, period),
                ledger.IsArchived(c.Name)))
            .OrderByDescending(r => r.Budget.Cents)
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
            NewestFirst(ledger.ExpensesIn(period), e => e.Date)
                .Select(e => new ExpenseLine(e.Date, e.Category.Name, e.Label, e.Amount))
                .ToList(),
            NewestFirst(incomes, i => i.Date)
                .Select(i => new IncomeLine(i.Date, i.Label, i.Amount))
                .ToList());
    }

    /// <summary>
    /// Newest date first, and on the same date newest recorded first (§12). The ledger lists in
    /// the order recorded; reversing that and then sorting stably by date gives both at once.
    /// </summary>
    private static IEnumerable<T> NewestFirst<T>(IReadOnlyList<T> inOrderRecorded, Func<T, DateOnly> date) =>
        inOrderRecorded.Reverse().OrderByDescending(date);
}
