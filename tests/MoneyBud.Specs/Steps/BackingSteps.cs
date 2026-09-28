using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for the backing files: back-a-category, assign-to-a-backed-category,
/// spend-against-a-backed-category, show-accumulated and show-moved-money.
/// back-a-category.feature's header explains the steps they share.
///
/// <para>Every act goes the way the user's does: backing, re-pointing and unbacking are a choice in
/// the category row's <i>Staat op</i> list, and typing a category or choosing an account is done on
/// the expense form. Every figure is read from what the screen shows: the row's <i>Opgebouwd</i>,
/// its list, and the notice. Givens set up the ledger directly, as everywhere, at their place in the
/// order of the Givens, so what they move is moved then.</para>
/// </summary>
[Binding]
public sealed class BackingSteps(SpecContext context)
{
    private Ledger Ledger => context.Ledger;
    private MoneyBudApp App => context.App;

    // ------------------------------------------------------------------ Given

    [Given(@"^I have set the backing account of ""([^""]*)"" to ""([^""]*)""$")]
    public void GivenIHaveSetTheBackingAccount(string category, string account) =>
        Assert.NotEqual(BackingOutcome.Unchanged, Ledger.SetBacking(category, Ledger.Account(account)).Outcome);

    [Given(@"^I have removed the backing of ""([^""]*)""$")]
    public void GivenIHaveRemovedTheBacking(string category) =>
        Assert.Equal(BackingOutcome.Unbacked, Ledger.SetBacking(category, null).Outcome);

    // Sets nothing: it states what the Givens above it already come to (back-a-category.feature).
    [Given(@"^Accumulated for ""([^""]*)"" in the (current|previous|next) budget period is (\S+) euro$")]
    public void GivenAccumulatedIs(string category, string which, string amount) =>
        Assert.Equal(SpecParsing.MoneyAmount(amount), Ledger.AccumulatedFor(category, Ledger.Period(which)));

    // ------------------------------------------------------------------ When: the Staat op list

    [When(@"^I set the backing account of ""([^""]*)"" to ""([^""]*)""$")]
    public void WhenISetTheBackingAccount(string category, string account) =>
        Choose(category, Ledger.Account(account).Name);

    [When(@"^I remove the backing of ""([^""]*)""$")]
    public void WhenIRemoveTheBacking(string category) => Choose(category, Tekst.NoBacking);

    // ------------------------------------------------------------------ When: the expense form

    [When(@"^I type ""([^""]*)"" as the category of the new expense$")]
    public void WhenITypeTheCategory(string typed) => App.ExpenseForm.Category = typed;

    // Picked in the form's list, as the list would write it: the account itself.
    [When(@"^I choose the account ""([^""]*)"" for the new expense$")]
    public void WhenIChooseTheAccount(string account) => App.ExpenseForm.ChosenAccount = Ledger.Account(account);

    // ------------------------------------------------------------------ Then: the row

    [Then(@"^the backing account of ""([^""]*)"" should be ""([^""]*)""$")]
    public void ThenTheBackingAccountShouldBe(string category, string account)
    {
        var row = RowOf(category);
        Assert.Equal(account, row.ChosenBacking?.Text);
        Assert.Same(Ledger.Account(account), row.BackingAccount);
    }

    [Then(@"^""([^""]*)"" should not be backed$")]
    public void ThenShouldNotBeBacked(string category)
    {
        var row = RowOf(category);
        Assert.Null(row.BackingAccount);
        Assert.Equal(Tekst.NoBacking, row.ChosenBacking?.Text);
    }

    [Then(@"^the choices offered for the backing account of ""([^""]*)"" should be exactly these, in this order:$")]
    public void ThenTheChoicesOfferedShouldBe(string category, Table table) =>
        Assert.Equal(
            table.Rows.Select(r => r["choice"] is "none" ? Tekst.NoBacking : r["choice"]),
            RowOf(category).BackingChoices.Select(c => c.Text));

    [Then(@"^Accumulated for ""([^""]*)"" in the (current|previous|next) budget period should (?:still )?be (\S+) euro$")]
    public void ThenAccumulatedShouldBe(string category, string which, string amount) =>
        AssertAccumulated(SpecParsing.MoneyAmount(amount), RowOf(category, which));

    [Then(@"^""([^""]*)"" should show no Accumulated in the (current|previous|next) budget period$")]
    public void ThenShouldShowNoAccumulated(string category, string which) =>
        AssertAccumulated(null, RowOf(category, which));

    [Then(@"^Accumulated for ""([^""]*)"" in the (current|previous|next) budget period should be marked below zero, with the marker a category over budget has and the badge ""([^""]*)""$")]
    public void ThenAccumulatedShouldBeMarked(string category, string which, string badge)
    {
        var row = RowOf(category, which);
        Assert.True(row.IsAccumulatedBelowZero);
        Assert.Equal(Marker.Over, row.AccumulatedMarker);
        Assert.Equal(Tekst.Overdrawn, badge);
    }

    [Then(@"^Accumulated for ""([^""]*)"" in the (current|previous|next) budget period should not be marked below zero$")]
    public void ThenAccumulatedShouldNotBeMarked(string category, string which)
    {
        var row = RowOf(category, which);
        Assert.False(row.IsAccumulatedBelowZero);
        Assert.Equal(Marker.None, row.AccumulatedMarker);
    }

    /// <summary>A row's <i>Opgebouwd</i>, both as a figure and as what the row shows.</summary>
    internal static void AssertAccumulated(Money? expected, CategoryRow row)
    {
        Assert.Equal(expected, row.Accumulated);
        Assert.Equal(expected is { } figure ? Tekst.AccumulatedFigure(figure) : null, row.AccumulatedText);
    }

    // ------------------------------------------------------------------ Then: what I am told

    [Then(@"^I should be told that ""([^""]*)"" is now backed by ""([^""]*)"", and that (\S+) euro moved from ""([^""]*)"" to ""([^""]*)""$")]
    public void ThenIShouldBeToldBackedAndMoved(string category, string account, string amount, string from, string to)
    {
        var result = LastBacking(category);
        Assert.True(result.Outcome is BackingOutcome.Backed or BackingOutcome.Repointed, $"Expected it backed, but it was {result.Outcome}.");
        Assert.Equal(account, result.After!.Name);
        AssertMoved(result, amount, from, to);
    }

    [Then(@"^I should be told that ""([^""]*)"" is no longer backed, and that (\S+) euro moved from ""([^""]*)"" to ""([^""]*)""$")]
    public void ThenIShouldBeToldUnbackedAndMoved(string category, string amount, string from, string to)
    {
        var result = LastBacking(category);
        Assert.Equal(BackingOutcome.Unbacked, result.Outcome);
        AssertMoved(result, amount, from, to);
    }

    // Only that the backing was announced: whether money moved is not checked.
    [Then(@"^I should be told that ""([^""]*)"" is now backed by ""([^""]*)""$")]
    public void ThenIShouldBeToldBacked(string category, string account)
    {
        var result = LastBacking(category);
        Assert.True(result.Outcome is BackingOutcome.Backed or BackingOutcome.Repointed, $"Expected it backed, but it was {result.Outcome}.");
        Assert.Equal(account, result.After!.Name);
        AssertTold(result);
    }

    [Then(@"^I should be told that ""([^""]*)"" is now backed by ""([^""]*)"", and of no money moved$")]
    public void ThenIShouldBeToldBackedAndNothingMoved(string category, string account)
    {
        var result = LastBacking(category);
        Assert.True(result.Outcome is BackingOutcome.Backed or BackingOutcome.Repointed, $"Expected it backed, but it was {result.Outcome}.");
        Assert.Equal(account, result.After!.Name);
        Assert.False(result.MovedMoney, "Money moved.");

        // The notice names the backing and no amount.
        var text = AssertTold(result);
        Assert.DoesNotContain("€", text);
    }

    // ------------------------------------------------------------------ Then: the expense form

    [Then(@"^the account chosen for the new expense should be ""([^""]*)""$")]
    public void ThenTheAccountChosenShouldBe(string account)
    {
        Assert.False(App.ExpenseForm.IsEditing);
        Assert.Equal(account, App.ExpenseForm.ChosenAccount.Name);
    }

    // ------------------------------------------------------------------ Then: a history

    [Then(@"^in the history of ""([^""]*)"" I should not be able to change or remove the movement of (\S+) euro for ""([^""]*)""$")]
    public void ThenICannotChangeOrRemoveTheMovement(string account, string amount, string category)
    {
        var chosen = Ledger.Account(account);
        if (App.HistoryAccount != chosen) App.OpenHistory(chosen);

        var money = SpecParsing.MoneyAmount(amount);
        var line = Assert.Single(App.History, l => l.Entry is Movement m && m.Amount == money && m.Category.Name == category);
        Assert.Equal(HistoryKind.Movement, line.Kind);
        Assert.False(line.CanChange);
        Assert.False(line.CanRemove);
    }

    // ------------------------------------------------------------------ doing

    // Chooses in the row's Staat op list by what the list shows, as the user does, from the row on
    // screen. The row hands the choice to MoneyBudApp.SetBacking and nothing else (a unit test holds
    // that), so the act is made there, where its result can be kept for the Thens.
    //
    // An ended period on screen may have no row for the category, when it has no history there, as
    // after a period boundary (the sweep files). Then the user steps forward to the current period,
    // where every category in use has a row, and so does this.
    private void Choose(string category, string choice)
    {
        if (App.Overview.Rows.All(r => r.Name != category))
            while (App.ShownPeriod.FirstDay < Ledger.CurrentPeriod.FirstDay) App.StepForward();

        var row = App.Overview.Rows.SingleOrDefault(r => r.Name == category)
            ?? throw new InvalidOperationException($"\"{category}\" has no row on screen.");
        var chosen = row.BackingChoices.SingleOrDefault(c => c.Text == choice)
            ?? throw new InvalidOperationException($"\"{category}\"'s Staat op list offers no \"{choice}\".");

        context.Record(App.SetBacking(row.Name, chosen.Account));
    }

    private SetBackingResult LastBacking(string category)
    {
        var result = Assert.IsType<SetBackingResult>(context.LastAttempt);
        Assert.Equal(category, result.Category.Name);
        return result;
    }

    private void AssertMoved(SetBackingResult result, string amount, string from, string to)
    {
        Assert.True(result.MovedMoney, "No money moved.");
        Assert.Equal(
            (SpecParsing.MoneyAmount(amount), from, to),
            (result.Moved!.Amount, result.Moved.From.Name, result.Moved.To.Name));

        var text = AssertTold(result);
        Assert.Contains(Tekst.Euro(result.Moved.Amount), text);
    }

    // Contains rather than equals: unbacking the sweep destination also says that it no longer is
    // one (choose-a-sweep-destination.feature).
    private string AssertTold(SetBackingResult result)
    {
        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.False(notice.IsRefusal);
        Assert.Contains(Tekst.BackingSet(result), notice.Text);
        return Tekst.BackingSet(result);
    }

    private CategoryRow RowOf(string category, string which = "current") =>
        App.OverviewFor(Ledger.Period(which)).Rows.SingleOrDefault(r => r.Name == category)
        ?? throw new InvalidOperationException($"\"{category}\" is not shown in the {which} budget period.");
}
