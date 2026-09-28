using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// The sweep on screen (arc42 §12, <i>The sweep and Restant</i>): the <i>Restant naar</i> list's order
/// and what choosing in it does, the line's wording in each state, and when a sweep is told and kept —
/// after an act, a refused one included, on opening and on the minute's tick.
/// </summary>
public sealed class SweepScreenTests
{
    private static readonly DateOnly Today = new(2026, 3, 15);

    private readonly FixedClock clock = new(Noon(Today));
    private readonly Ledger ledger;
    private readonly CountingStore store = new();

    public SweepScreenTests()
    {
        ledger = new Ledger(clock, "Bank");
        ledger.AddCategory("Groceries");
        ledger.AddCategory("Savings");
        ledger.RecordIncome(2000m, "Salaris", Today);
        ledger.SetBacking("Savings", ledger.AddAccount("Deposit", 0m).Account!);
        ledger.SetSweepDestination("Savings");
    }

    private static DateTimeOffset Noon(DateOnly day) => new(day.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);

    private void MoveTo(DateOnly day) => clock.Now = Noon(day);

    private static readonly DateOnly April = new(2026, 4, 1);

    private static string MarchSwept(decimal euros) =>
        $"Restant van maart 2026: {Tekst.Euro(Money.FromEuros(euros))} naar \"Savings\".";

    // ------------------------------------------------------------------ the list

    // "—" first, then alphabetically as the suggestions are: case not counting, and an accented
    // name among its letter.
    [Fact]
    public void The_list_is_none_first_then_the_backed_categories_alphabetically()
    {
        var pool = ledger.PoolAccount;
        foreach (var name in (string[])["Pension", "emergency", "Één keer"])
        {
            ledger.AddCategory(name);
            ledger.SetBacking(name, pool);
        }

        var app = new MoneyBudApp(ledger);

        Assert.Equal(["—", "Één keer", "emergency", "Pension", "Savings"], app.Overview.SweepChoices.Select(c => c.Text));
    }

    [Fact]
    public void The_list_is_the_same_collection_until_what_it_offers_changes()
    {
        var app = new MoneyBudApp(ledger);
        var first = app.SweepChoices;

        app.Tick();
        Assert.Same(first, app.SweepChoices);

        app.AddCategory("Holiday");
        Assert.Same(first, app.SweepChoices);

        app.SetBacking("Holiday", ledger.PoolAccount);
        Assert.NotSame(first, app.SweepChoices);
    }

    // What the list shows written back, as it is on every redraw: nothing said, nothing kept.
    [Fact]
    public void The_list_writing_back_what_it_shows_does_nothing_at_all()
    {
        var app = new MoneyBudApp(ledger, store);
        app.AddCategory("Holiday");
        var saves = store.Saves;
        var said = app.Notice;

        var overview = app.Overview;
        overview.ChosenSweepDestination = overview.ChosenSweepDestination;
        overview.ChosenSweepDestination = null;

        Assert.Same(said, app.Notice);
        Assert.Equal(saves, store.Saves);
    }

    // The list hands its choice to the screen's act and nothing else, which is what lets the steps
    // make the act there.
    [Fact]
    public void Choosing_in_the_list_is_the_screens_act()
    {
        var app = new MoneyBudApp(ledger, store);

        var overview = app.Overview;
        overview.ChosenSweepDestination = overview.SweepChoices[0];

        Assert.Null(ledger.SweepDestination);
        Assert.Equal("Restant gaat voortaan nergens heen.", app.Notice!.Text);
        Assert.Equal(1, store.Saves);
    }

    [Fact]
    public void An_ended_period_shows_its_line_and_no_list()
    {
        MoveTo(April);
        var app = new MoneyBudApp(ledger);
        app.StepBack();

        var march = app.Overview;
        Assert.False(march.ShowsSweepDestination);
        Assert.Equal("Restant € 2.000,00 naar Savings", march.SweepLineText);

        app.StepForward();
        Assert.True(app.Overview.ShowsSweepDestination);
        Assert.Null(app.Overview.SweepLineText);
    }

    // ------------------------------------------------------------------ the line's wording

    public static TheoryData<string, SweepLineKind, int, bool, string> Lines => new()
    {
        { "swept", SweepLineKind.Swept, 2000, true, "Restant € 2.000,00 naar Sparen" },
        { "still to sweep, nothing swept", SweepLineKind.StillToSweep, 40, false, "Restant: € 40,00 nog niet weggezet" },
        { "still to sweep, some swept", SweepLineKind.StillToSweep, 40, true, "Restant € 2.000,00 naar Sparen · € 40,00 nog niet weggezet" },
        { "swept too much", SweepLineKind.SweptTooMuch, 40, true, "Restant € 2.000,00 naar Sparen · € 40,00 te veel weggezet" },
        { "a shortfall", SweepLineKind.Shortfall, -30, false, "Restant: −€ 30,00" },
    };

    [Theory]
    [MemberData(nameof(Lines))]
    public void The_line_says_what_went_and_what_is_still_to_move(
        string what, SweepLineKind kind, int euros, bool anythingWent, string expected)
    {
        _ = what;
        var sparen = Ledger.StartNew(clock).CategoriesOffered.Single(c => c.Name == "Sparen");
        var parts = anythingWent ? [new SweptPart(sparen, Money.FromEuros(2000m))] : Array.Empty<SweptPart>();

        Assert.Equal(expected, Tekst.SweepLineText(new SweepLine(kind, parts, Money.FromEuros(euros), false)));
    }

    [Fact]
    public void Two_categories_are_both_named()
    {
        var categories = Ledger.StartNew(clock).CategoriesOffered;
        var parts = new[]
        {
            new SweptPart(categories.Single(c => c.Name == "Sparen"), Money.FromEuros(1900m)),
            new SweptPart(categories.Single(c => c.Name == "Hobby"), Money.FromEuros(100m)),
        };

        Assert.Equal(
            "Restant € 1.900,00 naar Sparen en € 100,00 naar Hobby",
            Tekst.SweepLineText(new SweepLine(SweepLineKind.Swept, parts, Money.FromEuros(2000m), false)));
    }

    [Fact]
    public void A_shortfall_carries_the_marker()
    {
        ledger.RecordExpense(30m, "Groceries", new DateOnly(2026, 2, 20));
        var app = new MoneyBudApp(ledger);

        var february = app.OverviewFor(ledger.Calendar.Previous(ledger.CurrentPeriod));

        Assert.Equal((Marker.Over, true, false), (february.SweepLineMarker, february.IsSweepLineShort, february.CanBringSweepUpToDate));
    }

    // ------------------------------------------------------------------ told once, and kept

    [Fact]
    public void Several_periods_swept_at_one_start_are_told_in_one_notice_oldest_first()
    {
        ledger.RecordIncome(100m, "April", April);
        MoveTo(new DateOnly(2026, 5, 3));

        var app = new MoneyBudApp(ledger, store);

        Assert.Equal(
            $"{MarchSwept(2000m)} Restant van april 2026: {Tekst.Euro(Money.FromEuros(100m))} naar \"Savings\".",
            app.Notice!.Text);
        Assert.Equal(1, store.Saves);
    }

    // The act's own sentence comes after the sweep its settling made.
    [Fact]
    public void A_sweep_made_by_an_act_is_said_before_what_the_act_says()
    {
        var app = new MoneyBudApp(ledger, store);
        MoveTo(April);

        app.RecordIncome("50", "Rente");

        Assert.StartsWith($"{MarchSwept(2000m)} Inkomst van € 50,00 toegevoegd.", app.Notice!.Text);
    }

    [Fact]
    public void A_sweep_made_by_a_refused_act_is_said_and_kept()
    {
        var app = new MoneyBudApp(ledger, store);
        MoveTo(April);

        app.RecordExpense("10", "Nothing", null);

        Assert.True(app.Notice!.IsRefusal);
        Assert.StartsWith(MarchSwept(2000m), app.Notice.Text);
        Assert.Equal(1, store.Saves);
    }

    [Fact]
    public void A_sweep_made_by_an_act_that_changed_nothing_is_said_and_kept()
    {
        var app = new MoneyBudApp(ledger, store);
        MoveTo(April);

        app.AddCategory("Groceries");

        Assert.StartsWith(MarchSwept(2000m), app.Notice!.Text);
        Assert.Equal(1, store.Saves);
    }

    // A question and a notice are never shown together, and money moved: the notice wins.
    [Fact]
    public void A_sweep_on_the_tick_drops_a_waiting_question()
    {
        var app = new MoneyBudApp(ledger, store);
        app.AskToRemove(ledger.IncomesIn(ledger.CurrentPeriod).Single());
        MoveTo(April);

        app.Tick();

        Assert.Null(app.Question);
        Assert.Equal(MarchSwept(2000m), app.Notice!.Text);
        Assert.Equal(1, store.Saves);
    }

    // In the minute after a boundary, before the tick, the period that just ended is not swept yet,
    // and its line offers the button. Pressing it settles first: the sweep is made and said, the
    // button goes, and nothing more moves.
    [Fact]
    public void The_button_pressed_before_the_tick_after_a_boundary_moves_nothing_more()
    {
        var app = new MoneyBudApp(ledger, store);
        MoveTo(April);
        app.StepBack();
        app.StepForward();
        Assert.True(app.Overview.CanBringSweepUpToDate);

        var result = app.BringSweepUpToDate();

        Assert.Null(result);
        Assert.Equal(MarchSwept(2000m), app.Notice!.Text);
        Assert.False(app.Overview.CanBringSweepUpToDate);
        Assert.Equal(Money.Zero, ledger.BalanceOf(ledger.PoolAccount));
        Assert.Equal(1, store.Saves);
    }

    [Fact]
    public void A_tick_that_sweeps_nothing_says_nothing()
    {
        ledger.SetSweepDestination(null);
        var app = new MoneyBudApp(ledger, store);
        MoveTo(April);

        app.Tick();

        Assert.Null(app.Notice);
    }

    /// <summary>A store that keeps nothing and counts the saves asked of it.</summary>
    private sealed class CountingStore : ILedgerStore
    {
        public int Saves { get; private set; }

        public Claim TryClaim() => Claim.Claimed;

        public LoadResult Load() => new LoadResult.NoData();

        public bool TrySave(LedgerSnapshot snapshot)
        {
            Saves++;
            return true;
        }

        public void Dispose() { }
    }
}
