using MoneyBud.Domain;

namespace MoneyBud.Presentation;

/// <summary>
/// One slice of the ring. <see cref="Category"/> is null for the <i>Unassigned</i> slice.
///
/// <para><see cref="Start"/> and <see cref="Sweep"/> are shares of the whole ring, from 0 to 1,
/// read clockwise; <see cref="FilledShare"/> is how much of this slice is filled in. The Desktop
/// draws from these and works nothing out itself.</para>
/// </summary>
public sealed record RingSlice(
    string? Category, Money Size, Money Filled, Marker Marker, double Start, double Sweep)
{
    public bool IsUnassigned => Category is null;

    public double FilledShare => Size.Cents == 0 ? 0 : (double)Filled.Cents / Size.Cents;
}

/// <summary>
/// The radial diagram at the head of the Overview (arc42 §12, *The overview, and its ring*).
///
/// <list type="bullet">
/// <item>One slice per category with a <i>Budget</i> above zero, sized to it and filled in as far
/// as it has been spent. An overspent slice does not grow: it is filled completely and marked.</item>
/// <item>A category with a <i>Budget</i> of zero has no slice, however the zero came about.</item>
/// <item><i>Unassigned</i> above zero is a slice of its own, always the last one clockwise. So
/// unless the period is over-assigned, the whole ring is the period's income.</item>
/// <item>Over-assigned, there is nothing to draw for <i>Unassigned</i>, and the ring is the
/// budgets only.</item>
/// <item>With neither income nor any <i>Budget</i>, the ring is empty — spending alone does not
/// fill it.</item>
/// </list>
/// </summary>
public sealed record Ring(IReadOnlyList<RingSlice> Slices)
{
    public bool IsEmpty => Slices.Count == 0;

    public IEnumerable<RingSlice> CategorySlices => Slices.Where(s => !s.IsUnassigned);

    public RingSlice? UnassignedSlice => Slices.FirstOrDefault(s => s.IsUnassigned);

    public Money Total => Money.Sum(Slices.Select(s => s.Size));

    /// <param name="rows">The period's rows, already in the Overview's order, which the slices share.</param>
    public static Ring Of(IReadOnlyList<CategoryRow> rows, Money unassigned, bool hasIncome)
    {
        var budgeted = rows.Where(r => r.Budget.Cents > 0).ToList();
        if (!hasIncome && budgeted.Count == 0) return new Ring([]);

        var parts = budgeted
            .Select(r => (Category: (string?)r.Name, Size: r.Budget,
                          Filled: r.Spent.Cents < r.Budget.Cents ? r.Spent : r.Budget, r.Marker))
            .ToList();

        if (unassigned.Cents > 0)
            parts.Add((null, unassigned, Money.Zero, Marker.None));

        var total = (double)Money.Sum(parts.Select(p => p.Size)).Cents;
        var slices = new List<RingSlice>();
        var start = 0.0;

        foreach (var (category, size, filled, marker) in parts)
        {
            var sweep = size.Cents / total;
            slices.Add(new RingSlice(category, size, filled, marker, start, sweep));
            start += sweep;
        }

        return new Ring(slices);
    }
}
