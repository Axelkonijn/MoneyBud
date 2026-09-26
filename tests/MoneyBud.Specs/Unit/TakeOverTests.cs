using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for opening a period, for what the scenarios cannot reach or do not pin. The
/// scenarios remain the contract (features/take-over-a-plan.feature) and are not repeated here.
///
/// <para>Most of these start from arc42 §12's own example: August 2026 planned — Boodschappen
/// €400, Huur €900, Hobby €150 — September unplanned, and today 1 October.</para>
/// </summary>
public sealed class TakeOverTests : IDisposable
{
    private readonly SpecContext context = new();
    private Ledger Ledger => context.Ledger;

    private static readonly DateOnly August = new(2026, 8, 1);
    private static readonly DateOnly October = new(2026, 10, 1);

    public TakeOverTests() => context.SetToday(October);

    public void Dispose() => context.Dispose();

    // ------------------------------------------------------------------ where the plan comes from

    // "However far back": a scenario goes two periods back, this goes further than anyone would.
    [Fact]
    public void The_search_back_has_no_limit()
    {
        PlanIn(new DateOnly(2024, 2, 1), ("Boodschappen", 400m));

        var offer = Ledger.PlanOfferedIn(Ledger.CurrentPeriod);

        Assert.NotNull(offer);
        Assert.Equal(new DateOnly(2024, 2, 1), offer.From.FirstDay);
    }

    // The latest earlier period with a plan is the source, whole: not each category's own latest
    // figure, gathered from several periods.
    [Fact]
    public void The_plan_comes_from_one_period_not_from_each_categorys_latest_figure()
    {
        PlanIn(new DateOnly(2026, 7, 1), ("Boodschappen", 400m), ("Huur", 900m));
        PlanIn(August, ("Hobby", 150m));

        var offer = Ledger.PlanOfferedIn(Ledger.CurrentPeriod)!;

        Assert.Equal(August, offer.From.FirstDay);
        Assert.Equal(["Hobby"], offer.Figures.Select(f => f.Category.Name));
    }

    [Fact]
    public void The_figures_are_in_the_order_the_categories_were_added()
    {
        Ledger.AddCategory("Huur");
        PlanIn(August, ("Boodschappen", 400m), ("Huur", 900m), ("Hobby", 150m));

        var offer = Ledger.PlanOfferedIn(Ledger.CurrentPeriod)!;

        Assert.Equal(["Huur", "Boodschappen", "Hobby"], offer.Figures.Select(f => f.Category.Name));
        Assert.Equal(Money.FromEuros(1450m), offer.Total);
    }

    // ------------------------------------------------------------------ non-cases

    [Fact]
    public void Taking_over_where_no_plan_is_offered_is_a_caller_mistake()
    {
        Ledger.AddCategory("Boodschappen");

        Assert.Throws<InvalidOperationException>(() => Ledger.TakeOverPlan(Ledger.CurrentPeriod));
    }

    [Fact]
    public void Taking_over_in_a_period_that_already_has_a_plan_is_a_caller_mistake()
    {
        PlanIn(August, ("Boodschappen", 400m));
        Ledger.Assign(10m, "Boodschappen", Ledger.CurrentPeriod);

        Assert.Throws<InvalidOperationException>(() => Ledger.TakeOverPlan(Ledger.CurrentPeriod));
        Assert.Equal(Money.FromEuros(10m), Ledger.BudgetFor("Boodschappen", Ledger.CurrentPeriod));
    }

    [Fact]
    public void A_date_range_that_is_not_a_budget_period_is_a_caller_mistake()
    {
        PlanIn(August, ("Boodschappen", 400m));
        var current = Ledger.CurrentPeriod;
        var madeUp = new BudgetPeriod(current.FirstDay.AddDays(1), current.LastDay);

        Assert.Throws<ArgumentException>(() => Ledger.PlanOfferedIn(madeUp));
        Assert.Throws<ArgumentException>(() => Ledger.TakeOverPlan(madeUp));
    }

    // ------------------------------------------------------------------ the Dutch on screen

    [Fact]
    public void The_button_names_the_period_and_the_total() =>
        Assert.Equal("Plan van augustus 2026 overnemen (€ 1.450,00)", GlossaryExample().OfferText);

    [Fact]
    public void The_grey_figure_is_labelled_plan()
    {
        var row = GlossaryExample().Rows.Single(r => r.Name == "Boodschappen");

        Assert.Equal("plan: € 400,00", row.PlanText);
    }

    [Fact]
    public void The_notice_names_where_the_plan_came_from_and_where_it_went()
    {
        PlanGlossaryExample();
        var app = context.App;

        app.TakeOverPlan();

        Assert.Equal("Plan van augustus 2026 overgenomen in oktober 2026: € 1.450,00 toegewezen.", app.Notice?.Text);
        Assert.False(app.Notice!.IsRefusal);
    }

    // ------------------------------------------------------------------ the order of the rows

    // Largest plan figure first; equal figures in the order added; a row with no figure after
    // every row with one, in the order added. The ring order's own tie rule, one key further on.
    [Fact]
    public void While_a_plan_is_offered_rows_are_in_plan_order_with_ties_in_order_added()
    {
        Ledger.AddCategory("Zonder plan");
        PlanIn(August, ("Huur", 900m), ("Kleding", 60m), ("Boodschappen", 400m), ("Hobby", 60m));
        Ledger.AddCategory("Ook zonder");

        var rows = context.App.OverviewFor(Ledger.CurrentPeriod).Rows;

        Assert.Equal(
            ["Huur", "Boodschappen", "Kleding", "Hobby", "Zonder plan", "Ook zonder"],
            rows.Select(r => r.Name));
        Assert.Equal(
            [90000, 40000, 6000, 6000, null, null],
            rows.Select(r => r.PlanFigure?.Cents));
    }

    // Once taken over, Budget order is the same order, which is the point of sorting by plan.
    [Fact]
    public void Taking_the_plan_over_moves_no_row()
    {
        PlanIn(August, ("Hobby", 150m), ("Huur", 900m), ("Boodschappen", 400m));
        var app = context.App;
        var before = app.Overview.Rows.Select(r => r.Name).ToList();

        app.TakeOverPlan();

        Assert.Equal(before, app.Overview.Rows.Select(r => r.Name));
        Assert.All(app.Overview.Rows, r => Assert.Null(r.PlanFigure));
    }

    // ------------------------------------------------------------------ setup

    private PeriodOverview GlossaryExample()
    {
        PlanGlossaryExample();
        return context.App.OverviewFor(Ledger.CurrentPeriod);
    }

    private void PlanGlossaryExample() =>
        PlanIn(August, ("Boodschappen", 400m), ("Huur", 900m), ("Hobby", 150m));

    // Assigned back when the period was current, as the scenarios' budget Givens do.
    private void PlanIn(DateOnly firstDay, params (string Category, decimal Euros)[] figures)
    {
        var period = Ledger.Calendar.PeriodContaining(firstDay);
        foreach (var (category, euros) in figures)
        {
            Ledger.AddCategory(category);
            var result = context.AsIfToday(firstDay, () => Ledger.Assign(euros, category, period));
            Assert.True(result.WasAssigned);
        }
    }
}
