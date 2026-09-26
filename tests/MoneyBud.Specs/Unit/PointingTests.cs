using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Pointing at the ring, beyond what point-at-a-slice.feature holds: what the ring's centre shows
/// when nothing is pointed at, and that pointing never shows a stale or another period's slice.
/// </summary>
public sealed class PointingTests
{
    private readonly MoneyBudApp app = new(Ledger.StartNew(new FixedClock(new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero))));

    public PointingTests()
    {
        app.RecordIncome("2000", "Salaris");
        app.Assign("400", "Boodschappen");
    }

    [Fact]
    public void With_nothing_pointed_at_the_centre_shows_Unassigned()
    {
        Assert.Null(app.PointedSlice);
        Assert.Null(app.PointedRow);
        Assert.True(app.RingCentreShowsUnassigned);
    }

    [Fact]
    public void An_empty_ring_shows_its_hint_in_the_centre_and_not_Unassigned()
    {
        app.StepForward();

        Assert.True(app.Overview.Ring.IsEmpty);
        Assert.False(app.RingCentreShowsUnassigned);
        Assert.NotNull(app.Overview.RingHint);
    }

    [Fact]
    public void Pointing_off_the_ring_shows_Unassigned_again()
    {
        app.PointAt(0.1);
        Assert.NotNull(app.PointedRow);

        app.PointAt(null);

        Assert.Null(app.PointedRow);
        Assert.True(app.RingCentreShowsUnassigned);
    }

    [Fact]
    public void The_slice_pointed_at_shows_figures_as_they_are_now()
    {
        app.PointAt(0.1);
        app.RecordExpense("25", "Boodschappen", null);

        Assert.Equal(Money.FromCents(2500), app.PointedRow!.Spent);
    }

    [Fact]
    public void Stepping_to_another_period_points_at_nothing()
    {
        app.PointAt(0.1);

        app.StepBack();

        Assert.Null(app.PointedSlice);
    }
}
