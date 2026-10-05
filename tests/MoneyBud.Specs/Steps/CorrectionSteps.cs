using System.Text.RegularExpressions;
using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for features/change-an-entry.feature and remove-an-entry.feature.
///
/// <para>Every act goes the way the user's does: the entry is picked by clicking its row in the
/// period on screen (<see cref="MoneyBudApp.EditExpense"/>), which loads it into its form; the
/// step then changes the one field it names, as the user would type it, and saves the form. So
/// "saved without changing anything" is exactly what the form was loaded with, and proves that
/// what the form loads can be saved back.</para>
///
/// <para>The refusals, "brought back", "went into" and "not warned" are told in recording's own
/// words and are answered by the steps that answer them for recording, which also know a change's
/// result.</para>
/// </summary>
[Binding]
public sealed partial class CorrectionSteps(SpecContext context)
{
    // How the feature files name an entry: by what its row shows (change-an-entry.feature's header),
    // with its date added where occurrences share a label (repeat-an-entry.feature's header).
    private const string Entry =
        @"(the (?:expense|income) labelled ""[^""]*""(?: dated \d{1,2} [A-Z][a-z]+ \d{4})?|the expense of \S+ euro for ""[^""]*"" without a label)";

    private Ledger Ledger => context.Ledger;
    private MoneyBudApp App => context.App;

    // ------------------------------------------------------------------ Given

    // Recording's grammar, as something that happened earlier. Setup goes through the ledger's
    // own door, so a setup entry that would be refused fails the scenario here; and it never
    // brings a category back, which would mean the Givens are in the wrong order.
    //
    // Ending in ", repeating monthly" or "weekly", it was set to repeat (repeat-an-entry.feature),
    // and no Given sets up a repeat with an occurrence already due, so that fails here too.
    [Given(@"^I have recorded an expense of (\S+) euro for ""([^""]*)"" labelled ""([^""]*)"" dated (?:on )?((?:(?! on the account |, repeating ).)+)" + SpecParsing.RepeatingEnding + "$")]
    public void GivenIHaveRecordedAnExpense(string amount, string category, string label, string date, string repeating) =>
        RecordExpense(amount, category, label, Ledger.Date(date), repeat: SpecParsing.Repeating(repeating));

    // On an account other than the one a new entry starts out on (features/show-accounts.feature).
    [Given(@"^I have recorded an expense of (\S+) euro for ""([^""]*)"" labelled ""([^""]*)"" dated (?:on )?(.+) on the account ""([^""]*)""" + SpecParsing.RepeatingEnding + "$")]
    public void GivenIHaveRecordedAnExpenseOnTheAccount(
        string amount, string category, string label, string date, string account, string repeating) =>
        RecordExpense(amount, category, label, Ledger.Date(date), Ledger.Account(account), SpecParsing.Repeating(repeating));

    [Given(@"^I have recorded an expense of (\S+) euro for ""([^""]*)"" without a label$")]
    public void GivenIHaveRecordedAnExpenseWithoutALabel(string amount, string category) =>
        RecordExpense(amount, category, label: null, Ledger.Today);

    [Given(@"^I have recorded an income of (\S+) euro labelled ""([^""]*)"" dated (?:on )?((?:(?! on the account |, repeating ).)+)" + SpecParsing.RepeatingEnding + "$")]
    public void GivenIHaveRecordedAnIncome(string amount, string label, string date, string repeating) =>
        RecordIncome(amount, label, date, account: null, SpecParsing.Repeating(repeating));

    [Given(@"^I have recorded an income of (\S+) euro labelled ""([^""]*)"" dated (?:on )?(.+) on the account ""([^""]*)""" + SpecParsing.RepeatingEnding + "$")]
    public void GivenIHaveRecordedAnIncomeOnTheAccount(string amount, string label, string date, string account, string repeating) =>
        RecordIncome(amount, label, date, Ledger.Account(account), SpecParsing.Repeating(repeating));

    private void RecordIncome(string amount, string label, string date, Account? account, Frequency? repeat)
    {
        var result = Ledger.RecordIncome(SpecParsing.Amount(amount), label, Ledger.Date(date), account, repeat);
        Assert.True(result.WasRecorded, $"Setting up the income \"{label}\" was refused: {result.Refusal}.");
        AssertNothingWasDue();
    }

    private void AssertNothingWasDue() =>
        Assert.True(Ledger.TakeOccurrencesMade().Count == 0, "A Given set up a repeat with an occurrence already due.");

    // ------------------------------------------------------------------- When: changing

    // "change" and "try to change" are one step, as "record" and "try to record" are.

    [When(@"^I (?:change|try to change) the amount of " + Entry + @" to (\S+) euro$")]
    public void WhenIChangeTheAmount(string entry, string amount) =>
        Change(entry, expense => expense.Amount = amount, income => income.Amount = amount);

    // Quoted: the text as typed (type-an-amount.feature's grammar).
    [When(@"^I (?:change|try to change) the amount of " + Entry + @" to ""([^""]*)""$")]
    public void WhenIChangeTheAmountTyped(string entry, string typed) =>
        Change(entry, expense => expense.Amount = typed, income => income.Amount = typed);

    [When(@"^I (?:change|try to change) the category of " + Entry + @" to ""([^""]*)""$")]
    public void WhenIChangeTheCategory(string entry, string category) =>
        Change(entry, expense => expense.Category = category, incomeEdit: null);

    [When(@"^I (?:change|try to change) the label of " + Entry + @" to ""([^""]*)""$")]
    public void WhenIChangeTheLabel(string entry, string label) =>
        Change(entry, expense => expense.Label = label, income => income.Label = label);

    // Choosing another account from the form's list (record-on-an-account.feature).
    [When(@"^I (?:change|try to change) the account of " + Entry + @" to ""([^""]*)""$")]
    public void WhenIChangeTheAccount(string entry, string account)
    {
        var chosen = Ledger.Account(account);
        Change(entry, expense => expense.ChosenAccount = chosen, income => income.ChosenAccount = chosen);
    }

    [When(@"^I (?:change|try to change) the date of " + Entry + @" to (.+)$")]
    public void WhenIChangeTheDate(string entry, string date)
    {
        var day = Ledger.Date(date).ToDateTime(TimeOnly.MinValue);
        Change(entry, expense => expense.Date = day, income => income.Date = day);
    }

    [When(@"^I try to change " + Entry + @" into an expense of (\S+) euro for ""([^""]*)"" labelled ""([^""]*)"" dated (?:on )?((?:(?! on the account ).)+)$")]
    public void WhenITryToChangeIntoAnExpense(string entry, string amount, string category, string label, string date) =>
        ChangeIntoAnExpense(entry, amount, category, label, date, account: null);

    [When(@"^I try to change " + Entry + @" into an expense of (\S+) euro for ""([^""]*)"" labelled ""([^""]*)"" dated (?:on )?(.+) on the account ""([^""]*)""$")]
    public void WhenITryToChangeIntoAnExpenseOnTheAccount(
        string entry, string amount, string category, string label, string date, string account) =>
        ChangeIntoAnExpense(entry, amount, category, label, date, Ledger.Account(account));

    private void ChangeIntoAnExpense(string entry, string amount, string category, string label, string date, Account? account)
    {
        var day = Ledger.Date(date).ToDateTime(TimeOnly.MinValue);
        Change(entry, expense =>
        {
            expense.Label = label;
            expense.Category = category;
            expense.Amount = amount;
            expense.Date = day;
            if (account is not null) expense.ChosenAccount = account;
        }, incomeEdit: null);
    }

    [When(@"^I try to change " + Entry + @" into an income of (\S+) euro labelled ""([^""]*)"" dated (?:on )?(.+)$")]
    public void WhenITryToChangeIntoAnIncome(string entry, string amount, string label, string date)
    {
        var day = Ledger.Date(date).ToDateTime(TimeOnly.MinValue);
        Change(entry, expenseEdit: null, income =>
        {
            income.Label = label;
            income.Amount = amount;
            income.Date = day;
        });
    }

    [When(@"^I save " + Entry + @" without changing anything$")]
    public void WhenISaveWithoutChangingAnything(string entry) =>
        Change(entry, _ => { }, _ => { });

    // Choosing it in the Herhalen list of the entry opened for changing, and saving
    // (repeat-an-entry.feature's header).
    [When(@"^I change the frequency of " + Entry + @" to (one-off|weekly|monthly)$")]
    public void WhenIChangeTheFrequency(string entry, string frequency)
    {
        var choice = new FrequencyChoice(SpecParsing.Frequency(frequency));
        Change(entry, expense => expense.ChosenFrequency = choice, income => income.ChosenFrequency = choice);
    }

    // ------------------------------------------------------------------- When: removing

    [When(@"^I remove " + Entry + @" and confirm$")]
    public void WhenIRemoveAndConfirm(string entry) => Remove(Pick(entry), confirm: true);

    [When(@"^I remove " + Entry + @" but decline to confirm$")]
    public void WhenIRemoveButDecline(string entry) => Remove(Pick(entry), confirm: false);

    // The two are identical in every respect, so which one goes cannot matter: the first.
    [When(@"^I remove one of the two (expenses|incomes) labelled ""([^""]*)"" and confirm$")]
    public void WhenIRemoveOneOfTheTwo(string what, string label)
    {
        object[] rows = what == "expenses"
            ? App.Overview.Expenses.Where(e => e.Label == label).ToArray()
            : App.Overview.Incomes.Where(i => i.Label == label).ToArray();

        Assert.Equal(2, rows.Length);
        Remove(rows[0], confirm: true);
    }

    // ------------------------------------------------------------------- Then

    // Went through: changed, or saved unchanged. Either way the form returned to recording.
    [Then(@"^the change should go through$")]
    public void ThenTheChangeShouldGoThrough()
    {
        switch (context.LastAttempt)
        {
            case ChangeExpenseResult expense:
                Assert.False(expense.WasRefused, $"Expected the change to go through, but it was refused: {expense.Refusal}.");
                Assert.False(App.ExpenseForm.IsEditing, "The expense form should have returned to recording.");
                break;
            case ChangeIncomeResult income:
                Assert.False(income.WasRefused, $"Expected the change to go through, but it was refused: {income.Refusal}.");
                Assert.False(App.IncomeForm.IsEditing, "The income form should have returned to recording.");
                break;
            case ChangeTransferResult transfer:
                Assert.False(transfer.WasRefused, $"Expected the change to go through, but it was refused: {transfer.Refusal}.");
                Assert.False(App.TransferForm.IsEditing, "The transfer form should have returned to recording.");
                break;
            default:
                throw NothingChanged();
        }
    }

    // Refused, before the ledger was asked or by it. Either way the form keeps what was typed
    // and stays on the entry, to be corrected in place.
    [Then(@"^the change should be refused$")]
    public void ThenTheChangeShouldBeRefused()
    {
        switch (context.LastAttempt)
        {
            case SpecContext.Unread { What: "change" }:
                Assert.True(App.ExpenseForm.IsEditing || App.IncomeForm.IsEditing || App.TransferForm.IsEditing,
                            "The form should still be on the entry.");
                break;
            case ChangeTransferResult transfer:
                Assert.True(transfer.WasRefused, "Expected the change to be refused, but it went through.");
                Assert.True(App.TransferForm.IsEditing, "The transfer form should still be on the transfer.");
                break;
            case ChangeExpenseResult expense:
                Assert.True(expense.WasRefused, "Expected the change to be refused, but it went through.");
                Assert.True(App.ExpenseForm.IsEditing, "The expense form should still be on the expense.");
                break;
            case ChangeIncomeResult income:
                Assert.True(income.WasRefused, "Expected the change to be refused, but it went through.");
                Assert.True(App.IncomeForm.IsEditing, "The income form should still be on the income.");
                break;
            default:
                throw NothingChanged();
        }
    }

    // What is fixed is that the change is announced, not the wording: what was said opens with
    // the announcement, whatever follows it.
    [Then(@"^I should be told that the (expense|income) was changed$")]
    public void ThenIShouldBeToldThatItWasChanged(string what)
    {
        var announcement = (what, context.LastAttempt) switch
        {
            ("expense", ChangeExpenseResult { Outcome: ChangeOutcome.Changed } expense) =>
                Tekst.ExpenseChanged(expense.Expense!, expense.CategoryBroughtBack),
            ("income", ChangeIncomeResult { Outcome: ChangeOutcome.Changed } income) =>
                Tekst.IncomeChanged(income.Income!),
            _ => throw new InvalidOperationException($"The last thing done did not change an {what}: {context.LastAttempt}."),
        };

        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.False(notice.IsRefusal);
        Assert.StartsWith(announcement, notice.Text);
    }

    // What the Herhalen list shows once the row is clicked, and whether it can be changed: "locked"
    // is tried, by choosing another frequency, and must still show the same one. The form is then
    // left with Annuleren, as the user would.
    [Then(@"^" + Entry + @" should open with the frequency (one-off|weekly|monthly), (changeable|locked)$")]
    public void ThenItShouldOpenWithTheFrequency(string entry, string frequency, string state)
    {
        var expected = SpecParsing.Frequency(frequency);
        var other = new FrequencyChoice(expected is null ? Frequency.Monthly : null);

        switch (Pick(entry))
        {
            case ExpenseLine expense:
            {
                App.EditExpense(expense);
                var form = App.ExpenseForm;
                Assert.Equal(new FrequencyChoice(expected), form.ChosenFrequency);
                Assert.Equal(state == "changeable", form.CanChangeFrequency);
                if (state == "locked")
                {
                    form.ChosenFrequency = other;
                    Assert.Equal(new FrequencyChoice(expected), form.ChosenFrequency);
                }
                form.Cancel();
                break;
            }
            case IncomeLine income:
            {
                App.EditIncome(income);
                var form = App.IncomeForm;
                Assert.Equal(new FrequencyChoice(expected), form.ChosenFrequency);
                Assert.Equal(state == "changeable", form.CanChangeFrequency);
                if (state == "locked")
                {
                    form.ChosenFrequency = other;
                    Assert.Equal(new FrequencyChoice(expected), form.ChosenFrequency);
                }
                form.Cancel();
                break;
            }
        }
    }

    // What the account list shows once the row is clicked, and whether it can be changed: "locked" is
    // tried, by choosing another account, and must still show the same one. Left with Annuleren.
    [Then(@"^" + Entry + @" should open on the account ""([^""]*)"", (changeable|locked)$")]
    public void ThenItShouldOpenOnTheAccount(string entry, string account, string state)
    {
        var expense = Assert.IsType<ExpenseLine>(Pick(entry));
        App.EditExpense(expense);
        var form = App.ExpenseForm;
        Assert.Equal(account, form.ChosenAccount.Name);
        Assert.Equal(state == "locked", form.IsAccountLocked);
        if (state == "locked")
        {
            form.ChosenAccount = Ledger.Accounts.First(a => a.Name != account);
            Assert.Equal(account, form.ChosenAccount.Name);
        }
        form.Cancel();
    }

    [Then(@"^I should have been asked to confirm first$")]
    public void ThenIShouldHaveBeenAskedToConfirmFirst() =>
        Assert.True(context.AskedFirst, "Expected a question while the entry was still there.");

    [Then(@"^I should be told that the (expense|income) was removed$")]
    public void ThenIShouldBeToldThatItWasRemoved(string what)
    {
        var removed = Assert.IsType<SpecContext.Removed>(context.LastAttempt);
        Assert.Equal(what, removed.What);

        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.False(notice.IsRefusal);
        Assert.Equal(removed.Said, notice.Text);
    }

    // Neither the question nor what was said afterwards mentions it; the figure and its marker
    // show it (remove-an-entry.feature's header).
    [Then(@"^I should not be warned about the budget period becoming over-assigned$")]
    public void ThenIShouldNotBeWarnedAboutOverAssigned()
    {
        // Removing an entry, or changing the period start day, which cuts a period short
        // (change-the-period-start-day.feature).
        var (asked, said) = context.LastAttempt switch
        {
            SpecContext.Removed removed => (removed.Asked, removed.Said),
            SpecContext.StartDayChanged changed => (changed.Asked, changed.Said),
            var other => throw new InvalidOperationException($"The last thing done was not a removal or a change of start day: {other}."),
        };

        foreach (var text in new[] { asked, said })
        {
            Assert.DoesNotContain(Tekst.OverAssigned, text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("toegewezen", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ----------------------------------------------------------------- Shared

    /// <summary>
    /// Clicks the entry's row, lets the step change the fields it names, and saves the form. The
    /// form holds text as typed, so a step changes a field exactly as the user would type it.
    /// </summary>
    private void Change(string entry, Action<ExpenseForm>? expenseEdit, Action<IncomeForm>? incomeEdit)
    {
        switch (Pick(entry))
        {
            case ExpenseLine expense:
            {
                App.EditExpense(expense);
                var form = App.ExpenseForm;
                (expenseEdit ?? throw new InvalidOperationException($"This step does not change an expense: {entry}."))(form);
                Keep(form.Save(), form.Amount, context.Record);
                break;
            }
            case IncomeLine income:
            {
                App.EditIncome(income);
                var form = App.IncomeForm;
                (incomeEdit ?? throw new InvalidOperationException($"This step does not change an income: {entry}."))(form);
                Keep(form.Save(), form.Amount, context.Record);
                break;
            }
        }
    }

    private void Keep<T>(T? result, string? typed, Action<T> record) where T : class
    {
        if (result is null) context.RecordUnread(typed ?? "", "change");
        else record(result);
    }

    /// <summary>
    /// Clicks the row, presses Verwijderen, notes whether a question is waiting while the entry is
    /// still listed, and answers it.
    /// </summary>
    private void Remove(object row, bool confirm)
    {
        var (what, id, said) = row switch
        {
            ExpenseLine expense => ("expense", expense.Entry.Id, Tekst.ExpenseRemoved(expense.Entry)),
            IncomeLine income => ("income", income.Entry.Id, Tekst.IncomeRemoved(income.Entry)),
            _ => throw new ArgumentException($"Not a row: {row}.", nameof(row)),
        };

        if (row is ExpenseLine e)
        {
            App.EditExpense(e);
            App.ExpenseForm.Remove();
        }
        else
        {
            App.EditIncome((IncomeLine)row);
            App.IncomeForm.Remove();
        }

        var stillListed = what == "expense"
            ? App.Overview.Expenses.Any(x => x.Entry.Id == id)
            : App.Overview.Incomes.Any(x => x.Entry.Id == id);
        var asked = App.Question?.Text;
        context.AskedFirst = asked is not null && stillListed;

        if (confirm)
        {
            App.Confirm();
            context.RecordRemoved(what, asked ?? "", said);
        }
        else
        {
            App.Decline();
            context.RecordDeclined(what);
        }
    }

    /// <summary>
    /// The row an entry phrase names, in the period on screen. An entry recorded today is not on an
    /// ended period left on screen after a period boundary (the sweep files), so when the period on
    /// screen has no such row, the user steps forward to the current period, and so does this.
    /// </summary>
    private object Pick(string entry)
    {
        if (!RowsNamed(entry).Any())
            while (App.ShownPeriod.FirstDay < Ledger.CurrentPeriod.FirstDay) App.StepForward();

        return Assert.Single(RowsNamed(entry));
    }

    private IEnumerable<object> RowsNamed(string entry)
    {
        var overview = App.Overview;

        if (Labelled().Match(entry) is { Success: true } labelled)
        {
            var label = labelled.Groups[2].Value;
            DateOnly? date = labelled.Groups[3].Success ? SpecParsing.CalendarDate(labelled.Groups[3].Value) : null;
            return labelled.Groups[1].Value == "expense"
                ? overview.Expenses.Where(e => e.Label == label && (date is null || e.Date == date))
                : overview.Incomes.Where(i => i.Label == label && (date is null || i.Date == date));
        }

        var unlabelled = Unlabelled().Match(entry);
        if (!unlabelled.Success)
            throw new ArgumentException($"Unknown entry phrase \"{entry}\".", nameof(entry));

        var amount = SpecParsing.MoneyAmount(unlabelled.Groups[1].Value);
        var category = unlabelled.Groups[2].Value;
        return overview.Expenses.Where(e => e.Label is null && e.Amount == amount && e.Category == category);
    }

    private void RecordExpense(
        string amount, string category, string? label, DateOnly date, Account? account = null, Frequency? repeat = null)
    {
        // Data kept before Vrij could put a backed category's expense on another account, which
        // today's rules cannot: it is recorded on the backing account, and written to the file on
        // the account named (keep-data.feature; ReallocateSteps.RecordAsKeptBefore).
        var keptBefore = context.KeptBeforeUnclaimed is not null && account is not null
                         && Ledger.LockedAccountFor(category, date) is { } locked && locked != account;
        var result = Ledger.RecordExpense(SpecParsing.Amount(amount), category, date, label, keptBefore ? null : account, repeat);
        if (keptBefore && result.Expense is { } kept) ReallocateSteps.RecordAsKeptBefore(context, kept, account!);
        Assert.True(result.WasRecorded, $"Setting up an expense for {category} was refused: {result.Refusal}.");
        Assert.False(result.CategoryBroughtBack, $"Setting up an expense brought {category} back.");
        AssertNothingWasDue();
    }

    private static InvalidOperationException NothingChanged() => new("No entry has been changed yet.");

    [GeneratedRegex(@"^the (expense|income) labelled ""([^""]*)""(?: dated (.+))?$")]
    private static partial Regex Labelled();

    [GeneratedRegex(@"^the expense of (\S+) euro for ""([^""]*)"" without a label$")]
    private static partial Regex Unlabelled();
}
