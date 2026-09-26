using MoneyBud.Domain;

namespace MoneyBud.Presentation;

/// <summary>
/// One slice of the ring. <see cref="Category"/> and <see cref="Row"/> are null for the
/// <i>Unassigned</i> slice.
///
/// <para><see cref="Start"/> and <see cref="Sweep"/> are shares of the whole ring, from 0 to 1,
/// read clockwise; <see cref="FilledShare"/> is how much of this slice is filled in. The Desktop
/// draws from these and works nothing out itself.</para>
/// </summary>
public sealed record RingSlice(
    string? Category, Money Size, Money Filled, Marker Marker, double Start, double Sweep)
{
    public bool IsUnassigned => Category is null;

    /// <summary>
    /// A category slice's row, which is what pointing at the slice shows: everything the row
    /// shows, the marker and <i>Gearchiveerd</i> included (arc42 §12, <i>Hovering a slice shows
    /// its figures</i>).
    /// </summary>
    public CategoryRow? Row { get; init; }

    public double FilledShare => Size.Cents == 0 ? 0 : (double)Filled.Cents / Size.Cents;

    public double End => Start + Sweep;
}

/// <summary>
/// The radial diagram at the head of the Overview (arc42 §12, *The overview, and its ring*).
///
/// <list type="bullet">
/// <item>One slice per category with a <i>Budget</i> above zero, sized to it and filled in as far
/// as it has been spent. An overspent slice does not grow: it is filled completely and marked.</item>
/// <item>A category with a <i>Budget</i> of zero has no slice, however the zero came about.</item>
/// <item><i>Unassigned</i> above zero is a slice of its own, always the last one clockwise. So
/// unless the period is over-assigned, the slices' sizes add up to the period's income.</item>
/// <item>Over-assigned, there is nothing to draw for <i>Unassigned</i>, and the ring is the
/// budgets only.</item>
/// <item>With neither income nor any <i>Budget</i>, the ring is empty — spending alone does not
/// fill it.</item>
/// <item>Every slice is drawn at least <see cref="MinimumSweep"/> of the ring, so the ring is not
/// drawn exactly in proportion (<see cref="Sweeps"/>).</item>
/// </list>
/// </summary>
public sealed record Ring(IReadOnlyList<RingSlice> Slices)
{
    /// <summary>
    /// The least share of the ring any slice is drawn at: 2%, or 7.2°. Approved at the plan gate,
    /// 2026-09-26, so that a small budget, and room to see its fill, stays visible.
    /// </summary>
    public const double MinimumSweep = 0.02;

    public bool IsEmpty => Slices.Count == 0;

    public IEnumerable<RingSlice> CategorySlices => Slices.Where(s => !s.IsUnassigned);

    public RingSlice? UnassignedSlice => Slices.FirstOrDefault(s => s.IsUnassigned);

    public Money Total => Money.Sum(Slices.Select(s => s.Size));

    /// <summary>The slice at a share of the ring, read clockwise from the top; null for an empty ring.</summary>
    public RingSlice? SliceAt(double share)
    {
        if (IsEmpty) return null;

        share -= Math.Floor(share);
        return Slices.FirstOrDefault(s => share < s.End) ?? Slices[^1];
    }

    /// <param name="rows">The period's rows, already in the Overview's order, which the slices share.</param>
    public static Ring Of(IReadOnlyList<CategoryRow> rows, Money unassigned, bool hasIncome)
    {
        var budgeted = rows.Where(r => r.Budget.Cents > 0).ToList();
        if (!hasIncome && budgeted.Count == 0) return new Ring([]);

        var parts = budgeted
            .Select(r => (Category: (string?)r.Name, Size: r.Budget,
                          Filled: r.Spent.Cents < r.Budget.Cents ? r.Spent : r.Budget, r.Marker, Row: (CategoryRow?)r))
            .ToList();

        if (unassigned.Cents > 0)
            parts.Add((null, unassigned, Money.Zero, Marker.None, null));

        var sweeps = Sweeps(parts.Select(p => p.Size.Cents).ToList());
        var slices = new List<RingSlice>();
        var start = 0.0;

        for (var i = 0; i < parts.Count; i++)
        {
            var (category, size, filled, marker, row) = parts[i];
            slices.Add(new RingSlice(category, size, filled, marker, start, sweeps[i]) { Row = row });
            start += sweeps[i];
        }

        return new Ring(slices);
    }

    /// <summary>
    /// The share of the ring each size is drawn at. A slice whose own share would fall below
    /// <see cref="MinimumSweep"/> is drawn at the minimum, and the others share what is left in
    /// proportion to their sizes; repeated, since giving way can push another below it. So a
    /// larger size is never drawn narrower than a smaller one, equal sizes are drawn equal, and
    /// the shares still close the circle. When the minimum cannot fit every slice, every slice is
    /// drawn equal. <i>Unassigned</i> is a slice like any other here.
    /// </summary>
    public static double[] Sweeps(IReadOnlyList<long> sizes)
    {
        var n = sizes.Count;
        if (n == 0) return [];
        if (n * MinimumSweep >= 1) return Enumerable.Repeat(1.0 / n, n).ToArray();

        var atMinimum = new bool[n];
        while (true)
        {
            var clamped = atMinimum.Count(c => c);
            var rest = (double)Enumerable.Range(0, n).Where(i => !atMinimum[i]).Sum(i => sizes[i]);
            var scale = (1 - clamped * MinimumSweep) / rest;

            var newlyBelow = Enumerable.Range(0, n)
                .Where(i => !atMinimum[i] && sizes[i] * scale < MinimumSweep)
                .ToList();

            if (newlyBelow.Count == 0)
                return Enumerable.Range(0, n)
                    .Select(i => atMinimum[i] ? MinimumSweep : sizes[i] * scale)
                    .ToArray();

            foreach (var i in newlyBelow) atMinimum[i] = true;
        }
    }
}
