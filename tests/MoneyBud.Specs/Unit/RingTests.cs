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
}
