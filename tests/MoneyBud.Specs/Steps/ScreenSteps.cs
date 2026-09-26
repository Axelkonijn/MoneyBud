using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for what the screen shows — features/overview.feature,
/// step-between-periods.feature, show-categories-in-a-period.feature and
/// list-transactions-in-a-period.feature. The suggestion steps are with the other "offered"
/// steps in <see cref="CategorySteps"/>.
///
/// <para>Everything here reads <see cref="MoneyBudApp"/>, never the ledger, because each step is
/// a claim about what MoneyBud shows. "The ring for the next budget period" is the ring the
/// Overview draws when that period is on screen, read without stepping to it, so that asking
/// about one period never moves the screen a later step asserts on.</para>
/// </summary>
[Binding]
public sealed class ScreenSteps(SpecContext context)
{
    private Ledger Ledger => context.Ledger;
    private MoneyBudApp App => context.App;

    // ------------------------------------------------------------------ Given

    // Through the screen's own stepping: there is no other way to put a period on screen.
    [Given(@"^the Overview shows the (current|previous|next) budget period$")]
    public void GivenTheOverviewShows(string which)
    {
        var target = Ledger.Period(which);

        while (App.ShownPeriod.FirstDay > target.FirstDay) App.StepBack();
        while (App.ShownPeriod.FirstDay < target.FirstDay) App.StepForward();

        Assert.Equal(target, App.ShownPeriod);
    }

    // The period stays the same; only the day inside it moves.
    [Given(@"today is the last day of the current budget period")]
    public void GivenTodayIsTheLastDayOfTheCurrentBudgetPeriod() =>
        context.SetToday(Ledger.CurrentPeriod.LastDay);

    // ------------------------------------------------------------------- When

    [When(@"^I step (back|forward) one budget period$")]
    public void WhenIStep(string direction)
    {
        if (direction == "back") App.StepBack();
        else App.StepForward();
    }

    // The clock moves on and MoneyBud is not restarted. Refresh is what the Desktop's timer calls;
    // it only tells the screen to look again, so the step does exactly what the running app does.
    [When(@"the next budget period begins while MoneyBud is open")]
    public void WhenTheNextBudgetPeriodBeginsWhileMoneyBudIsOpen()
    {
        _ = App;
        context.SetToday(Ledger.Calendar.Next(Ledger.CurrentPeriod).FirstDay);
        App.Refresh();
    }

    // ------------------------------------------------------------------- Then: stepping

    [Then(@"^the Overview should show the (current|previous|next) budget period$")]
    public void ThenTheOverviewShouldShow(string which) =>
        Assert.Equal(Ledger.Period(which), App.ShownPeriod);

    // Available from here, and it really goes one period that way.
    [Then(@"^I should be able to step (back|forward) one budget period$")]
    public void ThenIShouldBeAbleToStep(string direction)
    {
        var from = App.ShownPeriod;
        var command = direction == "back" ? App.StepBackCommand : App.StepForwardCommand;

        Assert.True(command.CanExecute(null));
        command.Execute(null);

        var expected = direction == "back" ? Ledger.Calendar.Previous(from) : Ledger.Calendar.Next(from);
        Assert.Equal(expected, App.ShownPeriod);
    }

    // Offered means the assign form is on the period shown and can be submitted. Whether the
    // assignment would then be accepted is another step's business.
    [Then(@"I should be able to assign in the budget period shown")]
    public void ThenIShouldBeAbleToAssignInThePeriodShown()
    {
        Assert.Equal(App.ShownPeriod, App.AssignForm.Period);
        Assert.True(App.AssignForm.SubmitCommand.CanExecute(null));
    }

    [Then(@"^I should be told that the (expense|income|assignment) went into the (current|previous|next) budget period$")]
    public void ThenIShouldBeToldThatItWentInto(string what, string which)
    {
        switch (what, context.LastAttempt)
        {
            case ("expense", RecordExpenseResult { WasRecorded: true }):
            case ("income", RecordIncomeResult { WasRecorded: true }):
            case ("assignment", AssignResult { WasAssigned: true }):
            case ("expense", ChangeExpenseResult { Outcome: ChangeOutcome.Changed }):
            case ("income", ChangeIncomeResult { Outcome: ChangeOutcome.Changed }):
                break;
            default:
                throw new InvalidOperationException($"The last thing done was not a successful {what}.");
        }

        var period = Ledger.Period(which);
        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.Equal(period, notice.WentInto);
        Assert.Contains(Tekst.PeriodName(period), notice.Text);
    }

    [Then(@"the period on screen should no longer be labelled as the current budget period")]
    public void ThenThePeriodOnScreenShouldNoLongerBeLabelledCurrent()
    {
        Assert.False(App.ShowsCurrentPeriod);
        Assert.Null(App.PeriodLabel);
    }

    // Nothing new: no notice, no question, and nothing on the save line. A lasting "not saved"
    // is not something new, so it does not count (carry-on-when-saving-fails.feature).
    [Then(@"I should not have been told anything")]
    public void ThenIShouldNotHaveBeenToldAnything()
    {
        Assert.Null(App.Notice);
        Assert.Null(App.Question);
        if (!App.IsUnsaved) Assert.Null(App.SaveLine);
    }

    // ------------------------------------------------------------------- Then: the ring

    [Then(@"^the ring for the (current|previous|next) budget period should have exactly these category slices, in this order:$")]
    public void ThenTheRingShouldHaveExactlyTheseCategorySlices(string which, Table table)
    {
        var expected = table.Rows.Select(row => (
            row["category"],
            SpecParsing.MoneyAmount(row["size"]),
            SpecParsing.MoneyAmount(row["filled"]),
            YesNo(row["over budget"]) ? Marker.Over : Marker.None));

        var actual = RingOf(which).CategorySlices.Select(s => (s.Category!, s.Size, s.Filled, s.Marker));

        Assert.Equal(expected, actual);
    }

    [Then(@"^the ring for the (current|previous|next) budget period should have no category slices$")]
    public void ThenTheRingShouldHaveNoCategorySlices(string which) =>
        Assert.Empty(RingOf(which).CategorySlices);

    [Then(@"^the ring for the (current|previous|next) budget period should have an Unassigned slice of (\S+) euro$")]
    public void ThenTheRingShouldHaveAnUnassignedSliceOf(string which, string amount)
    {
        var slice = RingOf(which).UnassignedSlice;
        Assert.True(slice is not null, "The ring should have an Unassigned slice.");
        Assert.Equal(SpecParsing.MoneyAmount(amount), slice.Size);
    }

    [Then(@"^the ring for the (current|previous|next) budget period should have no Unassigned slice$")]
    public void ThenTheRingShouldHaveNoUnassignedSlice(string which) =>
        Assert.Null(RingOf(which).UnassignedSlice);

    [Then(@"^the Unassigned slice should be the last slice in the ring for the (current|previous|next) budget period$")]
    public void ThenTheUnassignedSliceShouldBeLast(string which) =>
        Assert.True(RingOf(which).Slices[^1].IsUnassigned, "The last slice should be Unassigned.");

    // The slices' sizes, and the shares they are drawn at, which must close the circle exactly.
    [Then(@"^the ring for the (current|previous|next) budget period should add up to (\S+) euro$")]
    public void ThenTheRingShouldAddUpTo(string which, string amount)
    {
        var ring = RingOf(which);
        Assert.Equal(SpecParsing.MoneyAmount(amount), ring.Total);

        var last = ring.Slices[^1];
        Assert.Equal(1.0, last.Start + last.Sweep, precision: 9);
    }

    [Then(@"^the ring for the (current|previous|next) budget period should be empty, with a hint that there is no income in it$")]
    public void ThenTheRingShouldBeEmpty(string which)
    {
        var overview = OverviewOf(which);
        Assert.True(overview.Ring.IsEmpty, "The ring should be empty.");

        // The hint's wording is copy, not a term (arc42 §12); what is fixed is that there is one.
        Assert.Equal(Tekst.EmptyRing, overview.RingHint);
    }

    [Then(@"^the ring for the (current|previous|next) budget period should not be empty$")]
    public void ThenTheRingShouldNotBeEmpty(string which) =>
        Assert.False(RingOf(which).IsEmpty, "The ring should not be empty.");

    [Then(@"^""([^""]*)"" should have no slice in the ring for the (current|previous|next) budget period$")]
    public void ThenShouldHaveNoSlice(string category, string which) =>
        Assert.DoesNotContain(RingOf(which).CategorySlices, s => s.Category == category);

    // ------------------------------------------------------------------- Then: how slices are drawn

    // The minimum is read from where the screen keeps it, so these hold for the width approved at
    // the plan gate; "wider than its share" is the guard that a minimum of zero cannot pass.
    [Then(@"^every slice in the ring for the (current|previous|next) budget period should be drawn at least the minimum slice width$")]
    public void ThenEverySliceShouldBeDrawnAtLeastTheMinimum(string which)
    {
        var ring = RingOf(which);
        Assert.NotEmpty(ring.Slices);
        Assert.All(ring.Slices, s => Assert.True(s.Sweep >= Ring.MinimumSweep - 1e-12,
            $"{s.Category ?? "Unassigned"} is drawn at {s.Sweep:P3}, under the minimum of {Ring.MinimumSweep:P0}."));
    }

    [Then(@"^the ""([^""]*)"" slice in the ring for the (current|previous|next) budget period should be drawn wider than its share of the ring's total$")]
    public void ThenTheSliceShouldBeDrawnWiderThanItsShare(string category, string which) =>
        AssertDrawnWiderThanItsShare(RingOf(which), CategorySlice(which, category));

    [Then(@"^the Unassigned slice in the ring for the (current|previous|next) budget period should be drawn wider than its share of the ring's total$")]
    public void ThenTheUnassignedSliceShouldBeDrawnWiderThanItsShare(string which)
    {
        var ring = RingOf(which);
        AssertDrawnWiderThanItsShare(ring, ring.UnassignedSlice ?? throw new InvalidOperationException("There is no Unassigned slice."));
    }

    [Then(@"^the ""([^""]*)"" slice in the ring for the (current|previous|next) budget period should be drawn (\d+)% filled$")]
    public void ThenTheSliceShouldBeDrawnFilled(string category, string which, int percent) =>
        Assert.Equal(percent / 100.0, CategorySlice(which, category).FilledShare, precision: 9);

    private static void AssertDrawnWiderThanItsShare(Ring ring, RingSlice slice)
    {
        var share = (double)slice.Size.Cents / ring.Total.Cents;
        Assert.True(slice.Sweep > share,
            $"{slice.Category ?? "Unassigned"} is drawn at {slice.Sweep:P3}, no wider than its share of {share:P3}.");
    }

    private RingSlice CategorySlice(string which, string category) =>
        RingOf(which).CategorySlices.SingleOrDefault(s => s.Category == category)
        ?? throw new InvalidOperationException($"\"{category}\" has no slice.");

    // ------------------------------------------------------------------- pointing at a slice

    // Points through MoneyBudApp, at the middle of the slice's share of the ring on screen, so the
    // app's own SliceAt decides which slice that is.
    [When(@"^I point at the ""([^""]*)"" slice in the ring for the (current|previous|next) budget period$")]
    public void WhenIPointAtTheSlice(string category, string which)
    {
        ShowOnScreen(which);
        var slice = App.Overview.Ring.CategorySlices.SingleOrDefault(s => s.Category == category)
            ?? throw new InvalidOperationException($"\"{category}\" has no slice to point at.");
        App.PointAt(slice.Start + slice.Sweep / 2);
    }

    [When(@"^I point at the Unassigned slice in the ring for the (current|previous|next) budget period$")]
    public void WhenIPointAtTheUnassignedSlice(string which)
    {
        ShowOnScreen(which);
        var slice = App.Overview.Ring.UnassignedSlice
            ?? throw new InvalidOperationException("There is no Unassigned slice to point at.");
        App.PointAt(slice.Start + slice.Sweep / 2);
    }

    [Then(@"^the slice pointed at should show these figures:$")]
    public void ThenTheSlicePointedAtShouldShowTheseFigures(Table table)
    {
        var expected = table.Rows.Single();
        var row = App.PointedRow ?? throw new InvalidOperationException("No category slice is pointed at.");

        Assert.False(App.RingCentreShowsUnassigned);
        Assert.Equal(
            (expected["category"], SpecParsing.MoneyAmount(expected["budget"]), SpecParsing.MoneyAmount(expected["spent"]),
             SpecParsing.MoneyAmount(expected["remaining"]), YesNo(expected["over budget"]) ? Marker.Over : Marker.None),
            (row.Name, row.Budget, row.Spent, row.Remaining, row.Marker));
    }

    [Then(@"^the slice pointed at should show Unassigned of (\S+) euro$")]
    public void ThenTheSlicePointedAtShouldShowUnassigned(string amount)
    {
        var slice = App.PointedSlice ?? throw new InvalidOperationException("Nothing is pointed at.");

        Assert.True(slice.IsUnassigned, "The Unassigned slice should be pointed at.");
        Assert.True(App.RingCentreShowsUnassigned, "The ring's centre should show Unassigned.");
        Assert.Equal(SpecParsing.MoneyAmount(amount), App.Overview.Unassigned);
    }

    [Then(@"^the slice pointed at should show that its category is archived$")]
    public void ThenTheSlicePointedAtShouldShowArchived() =>
        Assert.True(App.PointedRow?.IsArchived, "The slice pointed at should show that its category is archived.");

    [Then(@"^the slice pointed at should not show that its category is archived$")]
    public void ThenTheSlicePointedAtShouldNotShowArchived() =>
        Assert.False(App.PointedRow?.IsArchived ?? throw new InvalidOperationException("No category slice is pointed at."));

    private void ShowOnScreen(string which)
    {
        var period = Ledger.Period(which);
        while (App.ShownPeriod.FirstDay > period.FirstDay) App.StepBack();
        while (App.ShownPeriod.FirstDay < period.FirstDay) App.StepForward();
    }

    [Then(@"^Unassigned in the (current|previous|next) budget period should be marked the same way as a category that is over budget$")]
    public void ThenUnassignedShouldBeMarkedLikeOverBudget(string which)
    {
        var overview = OverviewOf(which);
        var overspent = new CategoryRow("any", Money.Zero, Money.FromCents(1), Money.FromCents(-1), IsArchived: false);

        Assert.Equal(overspent.Marker, overview.UnassignedMarker);
    }

    // ------------------------------------------------------------------- Then: rows

    // Checks the columns the table has: only the names and their order, or the figures and the
    // marker as well.
    [Then(@"^the categories shown in the (current|previous|next) budget period should be exactly these, in this order:$")]
    public void ThenTheCategoriesShownShouldBeExactly(string which, Table table)
    {
        var rows = OverviewOf(which).Rows;
        Assert.Equal(table.Rows.Select(r => r["category"]), rows.Select(r => r.Name));

        if (!table.ContainsColumn("budget")) return;

        foreach (var (expected, actual) in table.Rows.Zip(rows))
        {
            Assert.Equal(SpecParsing.MoneyAmount(expected["budget"]), actual.Budget);
            Assert.Equal(SpecParsing.MoneyAmount(expected["spent"]), actual.Spent);
            Assert.Equal(SpecParsing.MoneyAmount(expected["remaining"]), actual.Remaining);
            Assert.Equal(YesNo(expected["over budget"]) ? Marker.Over : Marker.None, actual.Marker);
        }
    }

    // ------------------------------------------------------------------- Then: lists

    [Then(@"^the expenses listed in the (current|previous|next) budget period should be exactly these, in this order:$")]
    public void ThenTheExpensesListedShouldBeExactly(string which, Table table)
    {
        var expected = table.Rows.Select(row => (
            Ledger.Date(row["date"]),
            row["category"],
            row["label"] is "" ? null : row["label"],
            SpecParsing.MoneyAmount(row["amount"])));

        var actual = OverviewOf(which).Expenses.Select(e => (e.Date, e.Category, e.Label, e.Amount));

        Assert.Equal(expected, actual);
    }

    [Then(@"^the incomes listed in the (current|previous|next) budget period should be exactly these, in this order:$")]
    public void ThenTheIncomesListedShouldBeExactly(string which, Table table)
    {
        var expected = table.Rows.Select(row => (
            Ledger.Date(row["date"]), row["label"], SpecParsing.MoneyAmount(row["amount"])));

        var actual = OverviewOf(which).Incomes.Select(i => (i.Date, i.Label, i.Amount));

        Assert.Equal(expected, actual);
    }

    [Then(@"^no expenses should be listed in the (current|previous|next) budget period$")]
    public void ThenNoExpensesShouldBeListed(string which) => Assert.Empty(OverviewOf(which).Expenses);

    [Then(@"^no incomes should be listed in the (current|previous|next) budget period$")]
    public void ThenNoIncomesShouldBeListed(string which) => Assert.Empty(OverviewOf(which).Incomes);

    // ----------------------------------------------------------------- Shared

    private PeriodOverview OverviewOf(string which) => App.OverviewFor(Ledger.Period(which));

    private Ring RingOf(string which) => OverviewOf(which).Ring;

    private static bool YesNo(string text) => text switch
    {
        "yes" => true,
        "no" => false,
        _ => throw new ArgumentException($"Expected yes or no, not \"{text}\".", nameof(text)),
    };
}
