using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for the period start day: change-the-period-start-day.feature, whose header explains them,
/// carry-plans-and-money-across-a-start-day-change.feature and name-a-budget-period.feature, and the
/// scenarios about the start day in keep-data.feature and start-moneybud.feature.
///
/// <para>Every act goes the way the user's does: a day is chosen in the <i>Periode begint op</i>
/// list, by setting what the list sets, and the question is answered with its buttons. What the list
/// shows is read for a period without stepping to it, as the sweep steps read <i>Restant naar</i>.
/// Where a period runs from and to is a claim about the calendar, so it is read from the ledger; how a
/// period is named is read from what the screen shows. The Given sets up the ledger directly, as
/// everywhere.</para>
/// </summary>
[Binding]
public sealed class StartDaySteps(SpecContext context)
{
    private const string CalendarDate = @"(\d{1,2} [A-Z][a-z]+ \d{4})";

    private Ledger Ledger => context.Ledger;
    private MoneyBudApp App => context.App;

    // ------------------------------------------------------------------ Given

    // The whole act, done earlier today, with everything it did. What it swept or recorded was said
    // then, so it is not said again when the screen opens.
    [Given(@"^I have changed the period start day to the " + SpecParsing.DayOfMonth + "$")]
    public void GivenIHaveChangedThePeriodStartDay(int day)
    {
        if (context.AppIfOpen is not null)
            throw new InvalidOperationException("\"I have changed the period start day\" must come before MoneyBud is on screen.");

        Assert.True(Ledger.ChangeStartDay(day).WasChanged, $"The start day was already the {day}.");
        Ledger.TakeSweepsMade();
        Ledger.TakeOccurrencesMade();
    }

    // ------------------------------------------------------------------ When

    [When(@"^I change the period start day to the " + SpecParsing.DayOfMonth + " and confirm$")]
    public void WhenIChangeAndConfirm(int day) => Change(day, confirm: true);

    [When(@"^I change the period start day to the " + SpecParsing.DayOfMonth + " but decline to confirm$")]
    public void WhenIChangeButDecline(int day) => Change(day, confirm: false);

    // Only the choosing: the question, if any, is left waiting.
    [When(@"^I choose the " + SpecParsing.DayOfMonth + " as the period start day$")]
    public void WhenIChoose(int day) => App.StartDayChoice = day;

    // ------------------------------------------------------------------ Then: the drop-down

    [Then(@"^the period start day shown in the " + SpecParsing.PeriodPhrase + " should (?:still )?be the " + SpecParsing.DayOfMonth + "$")]
    public void ThenThePeriodStartDayShownShouldBe(string period, int day)
    {
        var shown = App.StartDayShownIn(Ledger.PeriodNamed(period));
        Assert.True(shown is not null, $"The {period} offers no period start day.");
        Assert.Equal(day, shown);
        Assert.Contains(day, App.StartDayChoices);
    }

    [Then(@"^the " + SpecParsing.PeriodPhrase + " should offer no period start day$")]
    public void ThenThePeriodShouldOfferNoStartDay(string period) =>
        Assert.Null(App.StartDayShownIn(Ledger.PeriodNamed(period)));

    [Then(@"^the choices offered for the period start day should be every day from the 1st to the 31st, in order$")]
    public void ThenTheChoicesShouldBeEveryDay()
    {
        Assert.Equal(Enumerable.Range(1, 31), App.StartDayChoices);
        Assert.Equal(Enumerable.Range(1, 31).Select(d => d.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                     App.StartDayChoices.Select(Tekst.StartDayName));
    }

    // ------------------------------------------------------------------ Then: the periods

    [Then(@"^the " + SpecParsing.PeriodPhrase + " should (?:still )?run from " + CalendarDate + " to " + CalendarDate + "$")]
    public void ThenThePeriodShouldRunFrom(string period, string from, string to) =>
        Assert.Equal(
            new BudgetPeriod(SpecParsing.CalendarDate(from)!.Value, SpecParsing.CalendarDate(to)!.Value),
            Ledger.PeriodNamed(period));

    [Then(@"^the period to assign in should be the " + SpecParsing.PeriodPhrase + "$")]
    public void ThenThePeriodToAssignInShouldBe(string period) =>
        Assert.Equal(Ledger.PeriodNamed(period), App.AssignForm.Period);

    // ------------------------------------------------------------------ Then: what I am told

    // The day and that I am told are fixed, not the wording: the notice is checked to hold the
    // sentence MoneyBud says for a change to that day, which may be followed by a sweep.
    [Then(@"^I should be told that budget periods now start on the " + SpecParsing.DayOfMonth + "$")]
    public void ThenIShouldBeToldThatPeriodsNowStartOn(int day)
    {
        Assert.IsType<SpecContext.StartDayChanged>(context.LastAttempt);
        Assert.Equal(day, Ledger.Calendar.StartDay);

        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.False(notice.IsRefusal);
        Assert.Contains(Tekst.StartDayChanged(new ChangeStartDayResult(day, true, Ledger.CurrentPeriod, null)), notice.Text);
    }

    // ------------------------------------------------------------------ Then: names (name-a-budget-period.feature)

    // What the header shows while each period is on screen. The name is the screen's own, from the
    // period itself; the period on screen is also read from the header.
    [Then(@"^the budget periods should be named like this:$")]
    public void ThenTheBudgetPeriodsShouldBeNamed(Table table)
    {
        foreach (var row in table.Rows)
        {
            var period = Ledger.PeriodNamed(WithoutThe(row["period"]));
            Assert.Equal(row["name"], Tekst.PeriodName(period));
            if (period == App.ShownPeriod) Assert.Equal(row["name"], App.PeriodTitle);
        }
    }

    [Then(@"^that notice should name the period ""([^""]*)""$")]
    public void ThenThatNoticeShouldNameThePeriod(string name)
    {
        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.Contains(name, notice.Text);
    }

    [Then(@"^the plan offered in the " + SpecParsing.PeriodPhrase + @" should name the period ""([^""]*)""$")]
    public void ThenThePlanOfferedShouldNameThePeriod(string period, string name)
    {
        var overview = App.OverviewFor(Ledger.PeriodNamed(period));
        Assert.True(overview.HasOffer, $"No plan is offered in the {period}.");
        Assert.Contains(name, overview.OfferText);
    }

    // ------------------------------------------------------------------ Shared

    // Chosen in the list, as the user chooses; then the answer. A question must come, since the day
    // is not the one set, and nothing may have changed while it waits.
    private void Change(int day, bool confirm)
    {
        var periodBefore = Ledger.CurrentPeriod;
        var dayBefore = Ledger.Calendar.StartDay;

        App.StartDayChoice = day;
        var asked = App.Question?.Text
            ?? throw new InvalidOperationException($"Choosing the {day} asked nothing.");
        context.AskedFirst = App.Question.ConfirmText == Tekst.Change
                             && Ledger.Calendar.StartDay == dayBefore
                             && Ledger.CurrentPeriod == periodBefore;

        if (confirm)
        {
            App.Confirm();
            context.Record(new SpecContext.StartDayChanged(asked, App.Notice?.Text ?? ""));
        }
        else
        {
            App.Decline();
            context.RecordDeclined("start day");
        }
    }

    private static string WithoutThe(string phrase) =>
        phrase.StartsWith("the ", StringComparison.Ordinal) ? phrase[4..] : phrase;
}
