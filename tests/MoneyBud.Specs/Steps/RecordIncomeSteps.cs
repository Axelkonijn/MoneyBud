using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

[Binding]
public sealed class RecordIncomeSteps(SpecContext context)
{
    private Ledger Ledger => context.Ledger;

    // ------------------------------------------------------------------ Given

    // Anchored, where the neighbouring steps are not, and deliberately so: an alternation
    // is this pattern's only regex construct, and without anchors Reqnroll reads the
    // whole thing as a Cucumber Expression, in which "(...)" means optional text rather
    // than a choice. The other steps carry \S or a character class, which settles it.
    //
    // It checks for income records, not for an Unassigned of zero: a period planned before any
    // income arrived has none and is below zero.
    [Given(@"^I have recorded no income in the (current|previous|next) budget period$")]
    public void GivenIHaveRecordedNoIncomeIn(string which) =>
        Assert.Empty(Ledger.IncomesIn(Ledger.Period(which)));

    [Given(@"I have already recorded (\S+) euro of income in the (current|previous|next) budget period")]
    public void GivenIHaveAlreadyRecordedIncomeIn(string amount, string which)
    {
        var period = Ledger.Period(which);

        // Setup goes through the same door as anything else, so a setup income that would be
        // refused fails the scenario here rather than quietly not existing.
        var result = Ledger.RecordIncome(
            SpecParsing.Amount(amount), "Eerder ontvangen", Ledger.ADayInside(period));

        Assert.True(result.WasRecorded, $"Setting up income was refused: {result.Refusal}.");
    }

    // Moving "today" to the start of the period is what makes "the last day of the current
    // budget period" unambiguously still to come. The period itself does not change: its first
    // day is inside it, so the ledger still reports the same current period afterwards.
    [Given(@"today is the first day of the current budget period")]
    public void GivenTodayIsTheFirstDayOfTheCurrentBudgetPeriod() =>
        context.SetToday(Ledger.CurrentPeriod.FirstDay);

    // ------------------------------------------------------------------- When

    // The same grammar as recording an expense, in the variants the feature file uses, and for
    // the same reasons — see the note above the When steps in RecordExpenseSteps.

    // Ending in ", repeating monthly" or "weekly" where a recurring file uses them, as for an expense.
    [When(@"I (?:record|try to record) an income of (\S+) euro labelled ""([^""]*)""" + SpecParsing.RepeatingEnding)]
    public void WhenIRecordAnIncomeLabelled(string amount, string label, string repeating) =>
        Record(amount, label, date: null, repeat: SpecParsing.Repeating(repeating));

    [When(@"I (?:record|try to record) an income of (\S+) euro labelled ""([^""]*)"" dated (?:on )?((?:(?! on the account |, repeating ).)+)" + SpecParsing.RepeatingEnding)]
    public void WhenIRecordAnIncomeLabelledAndDated(string amount, string label, string date, string repeating) =>
        Record(amount, label, date, repeat: SpecParsing.Repeating(repeating));

    // On an account chosen from the form's list (record-on-an-account.feature).
    [When(@"I (?:record|try to record) an income of (\S+) euro labelled ""([^""]*)"" on the account ""([^""]*)""" + SpecParsing.RepeatingEnding)]
    public void WhenIRecordAnIncomeLabelledOnTheAccount(string amount, string label, string account, string repeating) =>
        Record(amount, label, date: null, account, SpecParsing.Repeating(repeating));

    [When(@"I (?:record|try to record) an income of (\S+) euro labelled ""([^""]*)"" dated (?:on )?(.+) on the account ""([^""]*)""" + SpecParsing.RepeatingEnding)]
    public void WhenIRecordAnIncomeLabelledAndDatedOnTheAccount(
        string amount, string label, string date, string account, string repeating) =>
        Record(amount, label, date, account, SpecParsing.Repeating(repeating));

    // Leaving the date as it starts out: today, whatever period is on screen.
    [When(@"I (?:record|try to record) an income of (\S+) euro labelled ""([^""]*)"" without giving a date")]
    public void WhenIRecordAnIncomeLabelledWithoutGivingADate(string amount, string label)
    {
        Assert.Null(context.App.IncomeForm.Date);
        Record(amount, label, date: null);
    }

    [When(@"I (?:record|try to record) an income of (\S+) euro without a label")]
    public void WhenIRecordAnIncomeWithoutALabel(string amount) =>
        Record(amount, label: null, date: null);

    // ------------------------------------------------------------------- Then

    [Then(@"the income should be recorded")]
    public void ThenTheIncomeShouldBeRecorded() =>
        Assert.True(
            context.IncomeResult.WasRecorded,
            $"Expected it to be recorded, but it was refused: {context.IncomeResult.Refusal}.");

    [Then(@"the income should not be recorded")]
    public void ThenTheIncomeShouldNotBeRecorded()
    {
        if (context.LastAttempt is SpecContext.Unread { What: "income" }) return;

        Assert.False(context.IncomeResult.WasRecorded, "Expected it not to be recorded, but it was.");
    }

    [Then(@"I should be told that an income needs a label")]
    public void ThenIShouldBeToldAnIncomeNeedsALabel() => AssertRefused(IncomeRefusal.LabelMissing);

    [Then(@"I should be told that an income must be more than 0 euro")]
    public void ThenIShouldBeToldAnIncomeMustBeMoreThanZero() =>
        AssertRefused(IncomeRefusal.AmountNotPositive);

    [Then(@"Unassigned in the (current|previous|next) budget period should (?:still )?be (\S+) euro")]
    public void ThenUnassignedShouldBe(string which, string expected) =>
        Assert.Equal(SpecParsing.MoneyAmount(expected), Ledger.UnassignedIn(Ledger.Period(which)));

    [Then(@"my income in the (current|previous|next) budget period should include (\d+) incomes of (\S+) euro")]
    public void ThenMyIncomeShouldIncludeNIncomesOf(string which, int count, string amount)
    {
        var matching = Ledger
            .IncomesIn(Ledger.Period(which))
            .Count(i => i.Amount == SpecParsing.MoneyAmount(amount));

        Assert.Equal(count, matching);
    }

    [Then(@"my income in the (current|previous|next) budget period should include (\S+) euro labelled ""([^""]*)""")]
    public void ThenMyIncomeShouldIncludeLabelled(string which, string amount, string label) =>
        Assert.Contains(
            Ledger.IncomesIn(Ledger.Period(which)),
            i => i.Amount == SpecParsing.MoneyAmount(amount) && i.Label == label);

    // ----------------------------------------------------------------- Shared

    // Through the screen, as RecordExpenseSteps does, and for the same reasons.
    //
    // A frequency is set in the form's Herhalen list, so an income that repeats goes through the form
    // too.
    private void Record(string amount, string? label, string? date, string? account = null, Frequency? repeat = null)
    {
        var day = date is null ? null : (DateOnly?)Ledger.Date(date);
        if (account is null && repeat is null)
        {
            context.Record(context.App.RecordIncome(amount, label, day)
                ?? throw new InvalidOperationException($"\"{amount}\" was not read as an amount."));
            return;
        }

        // Another account is chosen in the form's list, as for an expense.
        var form = context.App.IncomeForm;
        (form.Amount, form.Label) = (amount, label);
        form.Date = day?.ToDateTime(TimeOnly.MinValue);
        if (account is not null) form.ChosenAccount = Ledger.Account(account);
        if (repeat is not null) form.ChosenFrequency = new FrequencyChoice(repeat);
        context.Record(form.Record() ?? throw new InvalidOperationException($"\"{amount}\" was not read as an amount."));
    }

    // Answers for a change too, as RecordExpenseSteps' does.
    private void AssertRefused(IncomeRefusal expected)
    {
        if (context.LastAttempt is ChangeIncomeResult changed)
        {
            Assert.True(changed.WasRefused, "Expected the change to be refused, but it went through.");
            Assert.Equal(expected, changed.Refusal);
            return;
        }

        Assert.False(
            context.IncomeResult.WasRecorded, "Expected the income to be refused, but it was recorded.");
        Assert.Equal(expected, context.IncomeResult.Refusal);
    }
}
