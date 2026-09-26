using MoneyBud.Domain;
using MoneyBud.Presentation;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// The shares the ring is drawn at. The scenarios fix what is in the ring and in what order;
/// these check that the Desktop is handed angles that tile the circle, since it works nothing out
/// for itself.
/// </summary>
public sealed class RingTests
{
    private static CategoryRow Row(string name, long budget, long spent) =>
        new(name, Money.FromCents(budget), Money.FromCents(spent), Money.FromCents(budget - spent), IsArchived: false);

    [Fact]
    public void Slices_follow_on_from_each_other_and_close_the_circle()
    {
        var ring = Ring.Of([Row("Huur", 90000, 0), Row("Boodschappen", 40000, 15000)], Money.FromCents(70000), hasIncome: true);

        var start = 0.0;
        foreach (var slice in ring.Slices)
        {
            Assert.Equal(start, slice.Start, precision: 12);
            start += slice.Sweep;
        }

        Assert.Equal(1.0, start, precision: 12);
        Assert.Equal([0.45, 0.2, 0.35], ring.Slices.Select(s => Math.Round(s.Sweep, 12)));
    }

    [Fact]
    public void A_slice_is_filled_as_far_as_spent_and_never_past_full()
    {
        var ring = Ring.Of([Row("Boodschappen", 40000, 10000), Row("Hobby", 6000, 18000)], Money.Zero, hasIncome: true);

        Assert.Equal([0.25, 1.0], ring.Slices.Select(s => s.FilledShare));
        Assert.Equal([Marker.None, Marker.Over], ring.Slices.Select(s => s.Marker));
    }

    [Fact]
    public void Unassigned_is_never_filled() =>
        Assert.Equal(0, Ring.Of([], Money.FromCents(100), hasIncome: true).UnassignedSlice!.FilledShare);

    [Fact]
    public void An_over_assigned_ring_still_closes_the_circle_with_the_budgets_alone()
    {
        var ring = Ring.Of([Row("Boodschappen", 120000, 0), Row("Hobby", 32000, 0)], Money.FromCents(-2000), hasIncome: true);

        Assert.Null(ring.UnassignedSlice);
        Assert.Equal(1.0, ring.Slices.Sum(s => s.Sweep), precision: 12);
    }

    // ----------------------------------------------------------------- the minimum slice width

    [Fact]
    public void Slices_already_wider_than_the_minimum_are_drawn_exactly_in_proportion() =>
        Assert.Equal([0.5, 0.3, 0.2], Ring.Sweeps([500, 300, 200]).Select(s => Math.Round(s, 12)));

    // Huur 900, Boodschappen 400, Hobby 10, Unassigned 690: the plan's own example. Hobby's 0.5%
    // is drawn at 2%, and the other three give up the 1.5% between them, in proportion.
    [Fact]
    public void A_slice_below_the_minimum_is_drawn_at_it_and_the_others_give_way_in_proportion()
    {
        var sweeps = Ring.Sweeps([90000, 40000, 1000, 69000]);

        Assert.Equal(Ring.MinimumSweep, sweeps[2], precision: 12);
        var scale = (1 - Ring.MinimumSweep) / 199000;
        Assert.Equal(90000 * scale, sweeps[0], precision: 12);
        Assert.Equal(40000 * scale, sweeps[1], precision: 12);
        Assert.Equal(69000 * scale, sweeps[3], precision: 12);
        Assert.Equal(1.0, sweeps.Sum(), precision: 12);
    }

    // At its own share, 2050 is just over 2% of the ring. But widening the four one-cent slices
    // takes 8% of it, and giving way pushes the 2050 under the minimum, so a second round widens
    // it too.
    [Fact]
    public void Giving_way_can_push_another_slice_below_the_minimum_and_it_is_widened_too()
    {
        long[] sizes = [94700, 2050, 1, 1, 1, 1];
        var once = (1 - 4 * Ring.MinimumSweep) / (94700 + 2050);
        Assert.True(2050 * once < Ring.MinimumSweep, "The example should need a second round.");
        Assert.True((double)2050 / sizes.Sum() >= Ring.MinimumSweep, "At its own share it should be wide enough.");

        var sweeps = Ring.Sweeps(sizes);

        Assert.All(sweeps.Skip(1), s => Assert.Equal(Ring.MinimumSweep, s, precision: 12));
        Assert.Equal(1 - 5 * Ring.MinimumSweep, sweeps[0], precision: 12);
    }

    [Theory]
    [InlineData(new long[] { 100000, 40000, 3000, 2500, 2500, 10, 1 })]
    [InlineData(new long[] { 5, 4, 3, 2, 1 })]
    [InlineData(new long[] { 1_000_000, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 })]
    [InlineData(new long[] { 2000, 1999, 1998, 1, 30000 })]
    public void Every_slice_is_at_least_the_minimum_a_larger_one_is_never_narrower_and_the_circle_closes(long[] sizes)
    {
        var sweeps = Ring.Sweeps(sizes);

        Assert.Equal(1.0, sweeps.Sum(), precision: 12);
        Assert.All(sweeps, s => Assert.True(s >= Ring.MinimumSweep - 1e-12));
        for (var i = 0; i < sizes.Length; i++)
        for (var j = 0; j < sizes.Length; j++)
        {
            if (sizes[i] > sizes[j]) Assert.True(sweeps[i] >= sweeps[j] - 1e-12);
            if (sizes[i] == sizes[j]) Assert.Equal(sweeps[i], sweeps[j], precision: 12);
        }
    }

    [Fact]
    public void When_the_minimum_cannot_fit_every_slice_every_slice_is_drawn_equal()
    {
        var sizes = Enumerable.Range(1, 60).Select(i => (long)i * 100).ToArray();

        Assert.All(Ring.Sweeps(sizes), s => Assert.Equal(1.0 / 60, s, precision: 12));
    }

    [Fact]
    public void A_ring_of_one_slice_is_all_of_it() =>
        Assert.Equal([1.0], Ring.Sweeps([1]));

    [Fact]
    public void The_minimum_widens_Unassigned_like_any_other_slice()
    {
        var ring = Ring.Of([Row("Boodschappen", 199999, 0)], Money.FromCents(1), hasIncome: true);

        Assert.Equal(Ring.MinimumSweep, ring.UnassignedSlice!.Sweep, precision: 12);
        Assert.Equal(1.0, ring.Slices.Sum(s => s.Sweep), precision: 12);
    }

    // ----------------------------------------------------------------- pointing

    [Fact]
    public void A_category_slice_carries_its_row_and_the_Unassigned_slice_none()
    {
        var hobby = Row("Hobby", 6000, 2500) with { IsArchived = true };
        var ring = Ring.Of([hobby], Money.FromCents(4000), hasIncome: true);

        Assert.Equal(hobby, ring.Slices[0].Row);
        Assert.Null(ring.UnassignedSlice!.Row);
    }

    [Theory]
    [InlineData(0.0, "Huur")]
    [InlineData(0.4499, "Huur")]
    [InlineData(0.45, "Boodschappen")]
    [InlineData(0.6499, "Boodschappen")]
    [InlineData(0.65, null)]
    [InlineData(0.9999, null)]
    [InlineData(1.0, "Huur")]
    [InlineData(-0.1, null)]
    public void The_slice_at_a_share_of_the_ring_is_found_clockwise_from_the_top(double share, string? category)
    {
        var ring = Ring.Of([Row("Huur", 90000, 0), Row("Boodschappen", 40000, 0)], Money.FromCents(70000), hasIncome: true);

        Assert.Equal(category, ring.SliceAt(share)!.Category);
    }

    [Fact]
    public void An_empty_ring_has_nothing_to_point_at() =>
        Assert.Null(Ring.Of([], Money.Zero, hasIncome: false).SliceAt(0.5));
}
