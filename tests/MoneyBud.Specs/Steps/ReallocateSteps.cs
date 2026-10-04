using System.Text.Json.Nodes;
using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using MoneyBud.Storage;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for show-unclaimed.feature and reallocate-an-amount.feature, the expense form's locked list
/// (spend-against-a-backed-category.feature) and keep-data.feature's data kept before <i>Vrij</i>. The
/// two new files' headers explain the steps.
///
/// <para>Every act goes the way the user's does: a reallocation is made in the <i>Verplaatsen</i> form,
/// choosing <i>Van</i> and <i>Naar</i> from its lists by what they offer, with the period whose
/// <i>Niet toegewezen</i> is chosen on screen. <i>Vrij</i> is read from the strip. Givens set up the
/// ledger directly, as everywhere.</para>
/// </summary>
[Binding]
public sealed class ReallocateSteps(SpecContext context)
{
    // An end as the steps write it: Vrij on an account, a category, or a period's Niet toegewezen.
    private const string End = @"(Unclaimed on ""[^""]*""|""[^""]*""|Unassigned in the (?:current|previous|next) budget period)";

    private Ledger Ledger => context.Ledger;
    private MoneyBudApp App => context.App;

    // ------------------------------------------------------------------ Given

    [Given(@"^I have reallocated (\S+) euro from " + End + " to " + End + "$")]
    public void GivenIHaveReallocated(string amount, string from, string to)
    {
        var result = Ledger.Reallocate(SpecParsing.Amount(amount), EndOf(from), EndOf(to), PeriodOf(from, to));
        Assert.False(result.WasRefused, $"Setting up a reallocation was refused: {result.Refusal}.");
    }

    // Sets nothing: it states what the Givens above it come to (show-unclaimed.feature).
    [Given(@"^the Unclaimed of ""([^""]*)"" is (\S+) euro$")]
    public void GivenTheUnclaimedIs(string account, string amount) =>
        Assert.Equal(SpecParsing.MoneyAmount(amount), Ledger.UnclaimedOf(Ledger.Account(account)));

    // ------------------------------------------------------------------ When

    [When(@"^I (?:reallocate|try to reallocate) (\S+) euro from " + End + " to " + End + "$")]
    public void WhenIReallocate(string amount, string from, string to)
    {
        // Niet toegewezen means the period on screen, so that is the period put on screen.
        ShowOnScreen(PeriodOf(from, to));

        var form = App.ReallocateForm;
        form.OpenCommand.Execute(null);
        form.ChosenFrom = Choice(form.FromChoices, from, "From");
        form.ChosenTo = Choice(form.ToChoices, to, "To");
        form.Amount = amount;

        var result = form.Reallocate();
        if (result is null) context.RecordUnread(amount, "reallocation");
        else context.Record(result);
    }

    [When(@"^I start a reallocation from the row of ""([^""]*)""$")]
    public void WhenIStartAReallocationFromTheRow(string category)
    {
        var row = Assert.Single(App.Overview.Rows, r => r.Name == category);
        Assert.True(row.CanReallocate, $"\"{category}\"'s row offers no Verplaatsen.");
        App.ReallocateForm.StartFromCommand.Execute(row.Name);
    }

    [When(@"^I type ""([^""]*)"" as the category of the new expense, and (.+) as its date$")]
    public void WhenITypeTheCategoryAndDate(string category, string date)
    {
        App.ExpenseForm.Category = category;
        App.ExpenseForm.Date = Ledger.Date(date).ToDateTime(TimeOnly.MinValue);
    }

    // ------------------------------------------------------------------ Then: the act

    [Then(@"^the reallocation should go through$")]
    public void ThenTheReallocationShouldGoThrough()
    {
        var result = Assert.IsType<ReallocateResult>(context.LastAttempt);
        Assert.False(result.WasRefused, $"Expected it to go through, but it was refused: {result.Refusal}.");
        Assert.False(App.Notice?.IsRefusal ?? false);
        Assert.False(App.ReallocateForm.IsOpen, "The form should have emptied.");
    }

    [Then(@"^the reallocation should be refused$")]
    public void ThenTheReallocationShouldBeRefused()
    {
        switch (context.LastAttempt)
        {
            case SpecContext.Unread { What: "reallocation" }:
                break;
            case ReallocateResult result:
                Assert.True(result.WasRefused, "Expected the reallocation to be refused, but it went through.");
                break;
            default:
                throw new InvalidOperationException($"The last thing done was not a reallocation: {context.LastAttempt}.");
        }

        Assert.True(App.Notice?.IsRefusal, "Expected a refusal.");
        Assert.True(App.ReallocateForm.IsOpen, "The form should still hold what was typed.");
    }

    [Then(@"^I should be told that a reallocation needs two different ends$")]
    public void ThenToldTwoDifferentEnds() => AssertRefused(ReallocationRefusal.SameEnd);

    [Then(@"^I should be told that moving money out of Unassigned is assigning$")]
    public void ThenToldOutOfUnassigned() => AssertRefused(ReallocationRefusal.OutOfUnassigned);

    [Then(@"^I should be told that money can be moved to Unassigned only in the current budget period$")]
    public void ThenToldOnlyTheCurrentPeriod() => AssertRefused(ReallocationRefusal.UnassignedNotCurrent);

    [Then(@"^I should be told that money cannot be moved into ""([^""]*)""$")]
    public void ThenToldCannotMoveInto(string category)
    {
        var result = AssertRefused(ReallocationRefusal.IntoGivingEnd);
        Assert.Equal(category, result.Into?.Name);
    }

    [Then(@"^I should be told that (\S+) euro was reallocated from " + End + " to " + End + @", and of no money moved$")]
    public void ThenToldReallocatedAndNothingMoved(string amount, string from, string to)
    {
        var made = AssertReallocated(amount, from, to);
        Assert.False(made.MovedMoney, "Money moved between accounts.");
    }

    [Then(@"^I should be told that (\S+) euro was reallocated from " + End + " to " + End + @", and that (\S+) euro moved from ""([^""]*)"" to ""([^""]*)""$")]
    public void ThenToldReallocatedAndMoved(string amount, string from, string to, string moved, string fromAccount, string toAccount)
    {
        var made = AssertReallocated(amount, from, to);
        Assert.True(made.MovedMoney, "No money moved between accounts.");
        Assert.Equal(
            (SpecParsing.MoneyAmount(moved), fromAccount, toAccount),
            (made.Amount, made.FromAccount.Name, made.ToAccount.Name));
    }

    // ------------------------------------------------------------------ Then: the form

    [Then(@"^the choices offered as (From|To) for a reallocation should be exactly these, in any order:$")]
    public void ThenTheChoicesShouldBe(string side, Table table) =>
        Assert.Equal(
            table.Rows.Select(r => r["choice"]).Order(),
            ChoicesOf(side).Select(SpecName).Order());

    [Then(@"^the choices offered as (From|To) for a reallocation should include ""([^""]*)""$")]
    public void ThenTheChoicesShouldInclude(string side, string choice) =>
        Assert.Contains(choice, ChoicesOf(side).Select(SpecName));

    [Then(@"^the choices offered as (From|To) for a reallocation should not include ""([^""]*)""$")]
    public void ThenTheChoicesShouldNotInclude(string side, string choice) =>
        Assert.DoesNotContain(choice, ChoicesOf(side).Select(SpecName));

    [Then(@"^the reallocation should start with ""([^""]*)"" as From$")]
    public void ThenTheReallocationShouldStartWith(string category)
    {
        var form = App.ReallocateForm;
        Assert.True(form.IsOpen, "The form should be open.");
        Assert.Equal(category, form.ChosenFrom?.End.Category?.Name);
    }

    // ------------------------------------------------------------------ Then: Vrij in the strip

    [Then(@"^the Unclaimed of ""([^""]*)"" should (?:still )?be (\S+) euro$")]
    public void ThenTheUnclaimedShouldBe(string account, string amount)
    {
        var line = LineOf(account);
        var expected = SpecParsing.MoneyAmount(amount);
        Assert.Equal(expected, line.Unclaimed);
        Assert.Equal(Tekst.UnclaimedFigure(expected), line.UnclaimedText);
    }

    [Then(@"^""([^""]*)"" should show no Unclaimed$")]
    public void ThenShouldShowNoUnclaimed(string account)
    {
        var line = LineOf(account);
        Assert.Null(line.Unclaimed);
        Assert.Null(line.UnclaimedText);
    }

    [Then(@"^the Unclaimed of ""([^""]*)"" should be marked below zero, with the marker a category over budget has and the badge ""([^""]*)""$")]
    public void ThenTheUnclaimedShouldBeMarked(string account, string badge)
    {
        var line = LineOf(account);
        Assert.True(line.IsUnclaimedBelowZero);
        Assert.Equal(Marker.Over, line.UnclaimedMarker);
        Assert.Equal(Tekst.Overdrawn, badge);
    }

    [Then(@"^the Unclaimed of ""([^""]*)"" should not be marked below zero$")]
    public void ThenTheUnclaimedShouldNotBeMarked(string account)
    {
        var line = LineOf(account);
        Assert.False(line.IsUnclaimedBelowZero);
        Assert.Equal(Marker.None, line.UnclaimedMarker);
    }

    // ------------------------------------------------------------------ Then: the expense form's list

    // "locked" is tried, by choosing another account, which must leave the list as it was.
    [Then(@"^the account chosen for the new expense should be ""([^""]*)"", (locked|changeable)$")]
    public void ThenTheAccountChosenShouldBe(string account, string state)
    {
        var form = App.ExpenseForm;
        Assert.False(form.IsEditing);
        Assert.Equal(account, form.ChosenAccount.Name);
        Assert.Equal(state == "locked", form.IsAccountLocked);

        if (state == "locked")
        {
            form.ChosenAccount = Ledger.Accounts.First(a => a.Name != account);
            Assert.Equal(account, form.ChosenAccount.Name);
        }
    }

    // ------------------------------------------------------------------ keep-data: two versions

    /// <summary>
    /// The Givens after this describe what the version before <i>Vrij</i> kept, done under its rules
    /// (keep-data.feature). They run against today's ledger, and the one thing today's rules cannot
    /// record, a backed category's expense on another account, is recorded on its backing account
    /// and moved to the account named when the file is written (<see cref="RecordAsKeptBefore"/>).
    /// </summary>
    [Given(@"^what follows was kept by the version of MoneyBud from before Unclaimed$")]
    public void GivenWhatFollowsWasKeptBeforeUnclaimed() => context.KeptBeforeUnclaimed = [];

    /// <summary>
    /// Writes what the Givens set up as the version before <i>Vrij</i> would have kept it — version 7,
    /// without what version 8 added — and starts this MoneyBud on it, on the same day. A simulation of
    /// old data, like an interrupted save (plan for increment 15, Tests): balances are worked out from
    /// the entries, so the file is exactly what version 7 would have written.
    /// </summary>
    [When(@"^I start MoneyBud, updated to the version with Unclaimed$")]
    public void WhenIStartMoneyBudUpdated()
    {
        var moved = context.KeptBeforeUnclaimed
            ?? throw new InvalidOperationException("Nothing was kept by the version from before Unclaimed.");

        var json = JsonNode.Parse(LedgerJson.Write(Ledger.ToSnapshot()))!.AsObject();
        json["version"] = 7;
        json.Remove("reallocations");

        var keys = json["accounts"]!.AsArray().ToDictionary(a => (string)a!["name"]!, a => (int)a!["key"]!);
        foreach (var category in json["categories"]!.AsArray())
        {
            Assert.Null(category!["leftBehind"]);
            category.AsObject().Remove("leftBehind");
            if (category["backing"] is JsonObject backing)
            {
                Assert.Null(backing["earlier"]);
                backing.Remove("earlier");
            }
        }

        foreach (var expense in json["expenses"]!.AsArray())
            if (moved.TryGetValue((int)expense!["id"]!, out var account)) expense["account"] = keys[account];

        Directory.CreateDirectory(context.Folder);
        File.WriteAllText(context.DataFile, json.ToJsonString(new() { WriteIndented = true }));
        Assert.IsType<StartResult.Opened>(context.Start());
    }

    /// <summary>
    /// For a Given under "kept by the version from before Unclaimed": an expense on another account
    /// than its backed category's own. Recorded on that account's place, and put on
    /// <paramref name="account"/> when the file is written. False when today's rules can record it.
    /// </summary>
    internal static bool RecordAsKeptBefore(SpecContext context, Expense recorded, Account account)
    {
        if (context.KeptBeforeUnclaimed is not { } moved || recorded.Account == account) return false;
        moved[recorded.Id] = account.Name;
        return true;
    }

    // ------------------------------------------------------------------ finding

    private ReallocationEnd EndOf(string phrase)
    {
        if (phrase.StartsWith("Unclaimed on ", StringComparison.Ordinal))
            return ReallocationEnd.UnclaimedOn(Ledger.Account(Unquote(phrase["Unclaimed on ".Length..])));
        if (phrase.StartsWith("Unassigned", StringComparison.Ordinal))
            return ReallocationEnd.Unassigned;

        var name = Unquote(phrase);
        return ReallocationEnd.For(Ledger.CategoriesOffered.Concat(Ledger.ArchivedCategories).Single(c => c.Name == name));
    }

    // The period whose Niet toegewezen an end names, or the current one when neither end does.
    private BudgetPeriod PeriodOf(string from, string to)
    {
        var which = new[] { from, to }
            .Select(e => System.Text.RegularExpressions.Regex.Match(e, "^Unassigned in the (current|previous|next) budget period$"))
            .FirstOrDefault(m => m.Success)?.Groups[1].Value;
        return Ledger.Period(which ?? "current");
    }

    private void ShowOnScreen(BudgetPeriod period)
    {
        while (App.ShownPeriod.FirstDay > period.FirstDay) App.StepBack();
        while (App.ShownPeriod.FirstDay < period.FirstDay) App.StepForward();
    }

    private ReallocationChoice Choice(IReadOnlyList<ReallocationChoice> choices, string phrase, string side)
    {
        var end = EndOf(phrase);
        return choices.SingleOrDefault(c => c.End == end)
            ?? throw new InvalidOperationException($"{side} offers no {phrase}.");
    }

    private IReadOnlyList<ReallocationChoice> ChoicesOf(string side) =>
        side == "From" ? App.ReallocateForm.FromChoices : App.ReallocateForm.ToChoices;

    // A choice as the steps write it: "Unclaimed on Deposit", a category's name, or "Unassigned".
    private static string SpecName(ReallocationChoice choice) => choice.End.Kind switch
    {
        ReallocationEndKind.Unclaimed => $"Unclaimed on {choice.End.Account!.Name}",
        ReallocationEndKind.Category => choice.End.Category!.Name,
        _ => "Unassigned",
    };

    private Reallocation AssertReallocated(string amount, string from, string to)
    {
        var result = Assert.IsType<ReallocateResult>(context.LastAttempt);
        var made = result.Made ?? throw new InvalidOperationException("Nothing was reallocated.");
        Assert.Equal((SpecParsing.MoneyAmount(amount), EndOf(from), EndOf(to)), (made.Amount, made.From, made.To));

        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.False(notice.IsRefusal);
        Assert.Contains(Tekst.Reallocated(made), notice.Text);
        return made;
    }

    private ReallocateResult AssertRefused(ReallocationRefusal expected)
    {
        var result = Assert.IsType<ReallocateResult>(context.LastAttempt);
        Assert.Equal(expected, result.Refusal);

        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.True(notice.IsRefusal);
        Assert.Equal(Tekst.Refusal(expected, result.Into?.Name), notice.Text);
        return result;
    }

    private AccountLine LineOf(string name) =>
        App.Accounts.SingleOrDefault(l => l.Name == name)
        ?? throw new InvalidOperationException($"The strip shows no account \"{name}\".");

    private static string Unquote(string text) => text.Trim('"');
}
