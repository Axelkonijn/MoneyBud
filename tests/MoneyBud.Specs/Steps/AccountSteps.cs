using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for the accounts files: add-an-account, record-on-an-account, correct-a-balance,
/// transfer-between-accounts, manage-accounts and show-accounts. show-accounts.feature's header
/// explains the steps they share.
///
/// <para>Every act goes the way the user's does. A balance correction, a rename, deleting and
/// making the pool account are done from the account's history, opened by clicking the account in
/// the strip. A transfer is recorded through the transfer form, and changed or removed from a row
/// of a history. Every figure is read from what the screen shows: the strip, and the rows of a
/// history.</para>
///
/// <para>Givens set up the ledger directly, as everywhere: "I have an account ... dated on a day"
/// moves the clock to that day for the adding, so the starting balance is typed on it.</para>
/// </summary>
[Binding]
public sealed class AccountSteps(SpecContext context)
{
    private Ledger Ledger => context.Ledger;
    private MoneyBudApp App => context.App;

    // ------------------------------------------------------------------ Given

    [Given(@"^I have an account ""([^""]*)"" with a starting balance of (\S+) euro$")]
    public void GivenIHaveAnAccount(string name, string balance) => AddAccount(name, balance, Ledger.Today);

    [Given(@"^I have an account ""([^""]*)"" with a starting balance of (\S+) euro dated (?:on )?(.+)$")]
    public void GivenIHaveAnAccountDated(string name, string balance, string day) =>
        AddAccount(name, balance, Ledger.Date(day));

    [Given(@"^I have corrected the balance of ""([^""]*)"" to (\S+) euro$")]
    public void GivenIHaveCorrectedTheBalance(string account, string balance)
    {
        var result = Ledger.CorrectBalance(Ledger.Account(account), SpecParsing.Amount(balance));
        Assert.True(result.WasRecorded, $"Setting up a balance correction of {account} was refused.");
    }

    [Given(@"^I have deleted the account ""([^""]*)""$")]
    public void GivenIHaveDeletedTheAccount(string account) => Ledger.DeleteAccount(Ledger.Account(account));

    [Given(@"^I have recorded a transfer of (\S+) euro from ""([^""]*)"" to ""([^""]*)"" dated (?:on )?(.+)$")]
    public void GivenIHaveRecordedATransfer(string amount, string from, string to, string day)
    {
        var result = Ledger.RecordTransfer(SpecParsing.Amount(amount), Ledger.Account(from), Ledger.Account(to), Ledger.Date(day));
        Assert.True(result.WasRecorded, $"Setting up a transfer from {from} to {to} was refused: {result.Refusal}.");
    }

    // These set nothing: they state what the Givens above already add up to (show-accounts.feature).
    [Given(@"^the balance of ""([^""]*)"" is (\S+) euro$")]
    public void GivenTheBalanceIs(string account, string balance) =>
        Assert.Equal(SpecParsing.MoneyAmount(balance), Ledger.BalanceOf(Ledger.Account(account)));

    [Given(@"^net worth is (\S+) euro$")]
    public void GivenNetWorthIs(string amount) => Assert.Equal(SpecParsing.MoneyAmount(amount), Ledger.NetWorth);

    [Given(@"^the balance correction of ""([^""]*)"" to (\S+) euro shows a difference of (\S+) euro$")]
    public void GivenTheBalanceCorrectionShowsADifference(string account, string balance, string difference)
    {
        var correction = Assert.Single(
            Ledger.HistoryOf(Ledger.Account(account)).OfType<BalanceCorrection>(),
            c => !c.IsStartingBalance && c.Balance == SpecParsing.MoneyAmount(balance));
        Assert.Equal(SpecParsing.MoneyAmount(difference), Ledger.DifferenceOf(correction));
    }

    // ------------------------------------------------------------------ When: accounts

    [When(@"^I (?:add|try to add) an account ""([^""]*)"" with a starting balance of (\S+) euro$")]
    public void WhenIAddAnAccount(string name, string balance) => Add(name, balance);

    // Quoted: the starting balance as typed, "" being the field left empty.
    [When(@"^I (?:add|try to add) an account ""([^""]*)"" with a starting balance of ""([^""]*)""$")]
    public void WhenIAddAnAccountTyped(string name, string typed) => Add(name, typed);

    [When(@"^I (?:rename|try to rename) the account ""([^""]*)"" to ""([^""]*)""$")]
    public void WhenIRenameTheAccount(string account, string newName)
    {
        var chosen = OpenHistoryOf(account);
        App.StartAccountRenameCommand.Execute(null);
        Assert.Equal(chosen.Name, App.NewAccountName);
        App.NewAccountName = newName;
        context.Record(App.RenameAccount(chosen, App.NewAccountName));
    }

    [When(@"^I delete the account ""([^""]*)""$")]
    public void WhenIDeleteTheAccount(string account)
    {
        var chosen = OpenHistoryOf(account);
        Assert.True(App.CanDeleteHistoryAccount, $"{account} offers no delete.");
        context.RecordDeleted(App.DeleteAccount(chosen));
    }

    [When(@"^I make ""([^""]*)"" the pool account$")]
    public void WhenIMakeThePoolAccount(string account)
    {
        var chosen = OpenHistoryOf(account);
        Assert.True(App.CanMakeHistoryAccountPool, $"{account} does not offer to be made the pool account.");
        App.MakePool(chosen);
        context.RecordPoolMade(chosen);
    }

    // ------------------------------------------------------------------ When: balance corrections

    [When(@"^I (?:correct|try to correct) the balance of ""([^""]*)"" to (\S+) euro$")]
    public void WhenICorrectTheBalance(string account, string balance) => Correct(account, balance);

    [When(@"^I (?:correct|try to correct) the balance of ""([^""]*)"" to ""([^""]*)""$")]
    public void WhenICorrectTheBalanceTyped(string account, string typed) => Correct(account, typed);

    [When(@"^I remove the balance correction of ""([^""]*)"" to (\S+) euro (and confirm|but decline to confirm)$")]
    public void WhenIRemoveTheBalanceCorrection(string account, string balance, string answer) =>
        RemoveFromHistory(account, "balance correction", CorrectionLine(account, balance), answer == "and confirm");

    [When(@"^I remove the starting balance of ""([^""]*)"" (and confirm|but decline to confirm)$")]
    public void WhenIRemoveTheStartingBalance(string account, string answer) =>
        RemoveFromHistory(account, "starting balance", StartingBalanceLine(account), answer == "and confirm");

    // ------------------------------------------------------------------ When: transfers

    [When(@"^I (?:record|try to record) a transfer of (\S+) euro from ""([^""]*)"" to ""([^""]*)""$")]
    public void WhenIRecordATransfer(string amount, string from, string to) => RecordTransfer(amount, from, to, day: null);

    [When(@"^I (?:record|try to record) a transfer of (\S+) euro from ""([^""]*)"" to ""([^""]*)"" dated (?:on )?(.+)$")]
    public void WhenIRecordATransferDated(string amount, string from, string to, string day) =>
        RecordTransfer(amount, from, to, day);

    [When(@"^I (?:change|try to change) the transfer of (\S+) euro from ""([^""]*)"" to ""([^""]*)"" into a transfer of (\S+) euro from ""([^""]*)"" to ""([^""]*)"" dated (?:on )?(.+)$")]
    public void WhenIChangeTheTransfer(
        string amount, string from, string to, string newAmount, string newFrom, string newTo, string day)
    {
        var form = LoadTransfer(amount, from, to);
        form.Amount = newAmount;
        form.ChosenFrom = Ledger.Account(newFrom);
        form.ChosenTo = Ledger.Account(newTo);
        form.Date = Ledger.Date(day).ToDateTime(TimeOnly.MinValue);
        SaveTransfer(form);
    }

    [When(@"^I save the transfer of (\S+) euro from ""([^""]*)"" to ""([^""]*)"" without changing anything$")]
    public void WhenISaveTheTransferWithoutChangingAnything(string amount, string from, string to) =>
        SaveTransfer(LoadTransfer(amount, from, to));

    [When(@"^I remove the transfer of (\S+) euro from ""([^""]*)"" to ""([^""]*)"" (and confirm|but decline to confirm)$")]
    public void WhenIRemoveTheTransfer(string amount, string from, string to, string answer) =>
        RemoveFromHistory(from, "transfer", TransferLine(from, amount, from, to), answer == "and confirm");

    // ------------------------------------------------------------------ Then: the strip

    [Then(@"^the balance of ""([^""]*)"" should (?:still )?be (\S+) euro$")]
    public void ThenTheBalanceShouldBe(string account, string balance) =>
        Assert.Equal(SpecParsing.MoneyAmount(balance), LineOf(account).Balance);

    [Then(@"^net worth should (?:still )?be (\S+) euro$")]
    public void ThenNetWorthShouldBe(string amount) => Assert.Equal(SpecParsing.MoneyAmount(amount), App.NetWorth);

    [Then(@"^the accounts should be exactly these, in this order:$")]
    public void ThenTheAccountsShouldBeExactly(Table table)
    {
        var lines = App.Accounts;
        Assert.Equal(table.Rows.Select(r => Name(r["account"])), lines.Select(l => l.Name));

        if (table.ContainsColumn("balance"))
            Assert.Equal(table.Rows.Select(r => SpecParsing.MoneyAmount(r["balance"])), lines.Select(l => l.Balance));

        if (table.ContainsColumn("overdrawn"))
        {
            Assert.Equal(table.Rows.Select(r => YesNo(r["overdrawn"])), lines.Select(l => l.IsOverdrawn));
            Assert.All(lines, l => Assert.Equal(l.IsOverdrawn ? Marker.Over : Marker.None, l.Marker));
        }

        // Vrij, which every account shows, the pool account too since ruling 5 was revised
        // (show-unclaimed.feature).
        if (table.ContainsColumn("unclaimed"))
        {
            Assert.Equal(table.Rows.Select(r => SpecParsing.MoneyAmount(r["unclaimed"])), lines.Select(l => l.Unclaimed));
            Assert.All(lines, l => Assert.Equal(Tekst.UnclaimedFigure(l.Unclaimed), l.UnclaimedText));
        }

        if (table.ContainsColumn("unclaimed marked"))
        {
            Assert.Equal(table.Rows.Select(r => YesNo(r["unclaimed marked"])), lines.Select(l => l.IsUnclaimedBelowZero));
            Assert.All(lines, l => Assert.Equal(l.IsUnclaimedBelowZero ? Marker.Over : Marker.None, l.UnclaimedMarker));
        }
    }

    [Then(@"^the pool account should be ""([^""]*)""$")]
    public void ThenThePoolAccountShouldBe(string account)
    {
        var pool = Assert.Single(App.Accounts, l => l.IsPool);
        Assert.Equal(account, pool.Name);
        Assert.Equal(Tekst.PoolAccount, pool.PoolText);
    }

    [Then(@"^""([^""]*)"" should be shown as overdrawn, with the marker a category over budget has and the badge ""([^""]*)""$")]
    public void ThenShouldBeShownAsOverdrawn(string account, string badge)
    {
        var line = LineOf(account);
        Assert.True(line.IsOverdrawn);
        Assert.Equal(Marker.Over, line.Marker);
        Assert.Equal(Tekst.Overdrawn, badge);
    }

    [Then(@"^""([^""]*)"" should not be shown as overdrawn$")]
    public void ThenShouldNotBeShownAsOverdrawn(string account)
    {
        var line = LineOf(account);
        Assert.False(line.IsOverdrawn);
        Assert.Equal(Marker.None, line.Marker);
    }

    [Then(@"^net worth should be marked below zero, with the marker a category over budget has and the badge ""([^""]*)""$")]
    public void ThenNetWorthShouldBeMarked(string badge)
    {
        Assert.True(App.IsNetWorthNegative);
        Assert.Equal(Marker.Over, App.NetWorthMarker);
        Assert.Equal(Tekst.Overdrawn, badge);
    }

    [Then(@"^net worth should not be marked below zero$")]
    public void ThenNetWorthShouldNotBeMarked()
    {
        Assert.False(App.IsNetWorthNegative);
        Assert.Equal(Marker.None, App.NetWorthMarker);
    }

    [Then(@"^a new (expense|income) should start out on the account ""([^""]*)""$")]
    public void ThenANewEntryShouldStartOutOn(string what, string account)
    {
        var chosen = what == "expense" ? App.ExpenseForm.ChosenAccount : App.IncomeForm.ChosenAccount;
        Assert.Equal(account, chosen.Name);
    }

    // One list serves both forms, in the strip's order.
    [Then(@"^the accounts offered for a new (?:expense|income) should be exactly these, in this order:$")]
    public void ThenTheAccountsOfferedShouldBeExactly(Table table) =>
        Assert.Equal(table.Rows.Select(r => Name(r["account"])), App.AccountChoices.Select(a => a.Name));

    [Then(@"^I should be able to delete the account ""([^""]*)""$")]
    public void ThenIShouldBeAbleToDelete(string account)
    {
        OpenHistoryOf(account);
        Assert.True(App.CanDeleteHistoryAccount, $"{account} should offer a delete.");
    }

    [Then(@"^I should not be able to delete the account ""([^""]*)""$")]
    public void ThenIShouldNotBeAbleToDelete(string account)
    {
        OpenHistoryOf(account);
        Assert.False(App.CanDeleteHistoryAccount, $"{account} should offer no delete.");
    }

    // ------------------------------------------------------------------ Then: a history

    [Then(@"^the history of ""([^""]*)"" should be exactly these, newest first:$")]
    public void ThenTheHistoryShouldBeExactly(string account, Table table)
    {
        OpenHistoryOf(account);
        var lines = App.History;
        Assert.Equal(table.RowCount, lines.Count);

        foreach (var (row, line) in table.Rows.Zip(lines))
        {
            Assert.Equal(Ledger.Date(row["date"]), line.Date);
            Assert.Equal(row["entry"], KindName(line.Kind));
            Check("category", line.Entry switch { Expense e => e.Category.Name, Movement m => m.Category.Name, _ => null });
            Check("label", line.Entry switch { Expense e => e.Label, Income i => i.Label, _ => null });
            Check("from", line.Entry switch
            {
                Transfer t => t.From.Name, Movement m => m.From.Name, Reallocation r => EndName(r.From), _ => null,
            });
            Check("to", line.Entry switch
            {
                Transfer t => t.To.Name, Movement m => m.To.Name, Reallocation r => EndName(r.To), _ => null,
            });
            CheckMoney("amount", line.Amount);
            CheckMoney("balance", line.Balance);
            CheckMoney("difference", line.Difference);

            void Check(string column, string? actual)
            {
                if (table.ContainsColumn(column)) Assert.Equal(row[column] is "" ? null : row[column], actual);
            }

            void CheckMoney(string column, Money? actual)
            {
                if (table.ContainsColumn(column))
                    Assert.Equal(row[column] is "" ? null : SpecParsing.MoneyAmount(row[column]), actual);
            }
        }
    }

    // A reallocation's row, by its two ends as the history table writes them (reallocate-an-amount.feature).
    [Then(@"^in the history of ""([^""]*)"" I should not be able to change or remove the reallocation of (\S+) euro from (\S+|""[^""]*"") to (\S+|""[^""]*"")$")]
    public void ThenICannotChangeOrRemoveTheReallocation(string account, string amount, string from, string to)
    {
        OpenHistoryOf(account);
        var money = SpecParsing.MoneyAmount(amount);
        var line = Assert.Single(App.History, l =>
            l.Entry is Reallocation r && r.Amount == money && EndName(r.From) == Name(from) && EndName(r.To) == Name(to));
        Assert.Equal(HistoryKind.Reallocation, line.Kind);
        Assert.False(line.CanChange);
        Assert.False(line.CanRemove);
    }

    [Then(@"^nothing should be in the history of ""([^""]*)""$")]
    public void ThenNothingShouldBeInTheHistory(string account)
    {
        OpenHistoryOf(account);
        Assert.Empty(App.History);
    }

    [Then(@"^the balance correction of ""([^""]*)"" to (\S+) euro should show a difference of (\S+) euro$")]
    public void ThenTheBalanceCorrectionShouldShowADifference(string account, string balance, string difference)
    {
        var line = CorrectionLine(account, balance);
        Assert.Equal(SpecParsing.MoneyAmount(difference), line.Difference);
        Assert.Contains(Tekst.Signed(line.Difference!.Value), line.Text);
    }

    [Then(@"^in the history of ""([^""]*)"" I should be able to change the transfer of (\S+) euro from ""([^""]*)"" to ""([^""]*)""$")]
    public void ThenICanChangeTheTransfer(string account, string amount, string from, string to) =>
        Assert.True(TransferLine(account, amount, from, to).CanChange);

    [Then(@"^in the history of ""([^""]*)"" I should be able to remove the transfer of (\S+) euro from ""([^""]*)"" to ""([^""]*)""$")]
    public void ThenICanRemoveTheTransfer(string account, string amount, string from, string to) =>
        Assert.True(TransferLine(account, amount, from, to).CanRemove);

    [Then(@"^in the history of ""([^""]*)"" I should be able to remove the balance correction of ""([^""]*)"" to (\S+) euro$")]
    public void ThenICanRemoveTheBalanceCorrection(string account, string of, string balance)
    {
        Assert.Equal(account, of);
        Assert.True(CorrectionLine(account, balance).CanRemove);
    }

    [Then(@"^in the history of ""([^""]*)"" I should not be able to change the balance correction of ""([^""]*)"" to (\S+) euro$")]
    public void ThenICannotChangeTheBalanceCorrection(string account, string of, string balance)
    {
        Assert.Equal(account, of);
        Assert.False(CorrectionLine(account, balance).CanChange);
    }

    [Then(@"^in the history of ""([^""]*)"" I should be able to remove the starting balance of ""([^""]*)""$")]
    public void ThenICanRemoveTheStartingBalance(string account, string of)
    {
        Assert.Equal(account, of);
        Assert.True(StartingBalanceLine(account).CanRemove);
    }

    [Then(@"^in the history of ""([^""]*)"" I should not be able to change the starting balance of ""([^""]*)""$")]
    public void ThenICannotChangeTheStartingBalance(string account, string of)
    {
        Assert.Equal(account, of);
        Assert.False(StartingBalanceLine(account).CanChange);
    }

    [Then(@"^in the history of ""([^""]*)"" I should not be able to change or remove the (expense|income) labelled ""([^""]*)""$")]
    public void ThenICannotChangeOrRemoveTheEntry(string account, string what, string label)
    {
        OpenHistoryOf(account);
        var line = Assert.Single(App.History, l => l.Entry switch
        {
            Expense e => what == "expense" && e.Label == label,
            Income i => what == "income" && i.Label == label,
            _ => false,
        });
        Assert.False(line.CanChange);
        Assert.False(line.CanRemove);
    }

    // ------------------------------------------------------------------ Then: what came of it

    [Then(@"^the account should not be added$")]
    public void ThenTheAccountShouldNotBeAdded()
    {
        switch (context.LastAttempt)
        {
            case SpecContext.Unread { What: "account" }:
                break;
            case AddAccountResult result:
                Assert.True(result.WasRefused, "Expected the account to be refused, but it was added.");
                break;
            default:
                throw new InvalidOperationException($"The last thing done was not adding an account: {context.LastAttempt}.");
        }

        Assert.True(App.AccountForm.IsOpen, "The account form should still hold what was typed.");
    }

    [Then(@"^I should be told that the account ""([^""]*)"" was added$")]
    public void ThenIShouldBeToldTheAccountWasAdded(string account)
    {
        var added = Assert.IsType<AddAccountResult>(context.LastAttempt);
        Assert.Equal(account, added.Account!.Name);
        AssertTold(Tekst.AccountAdded(added.Account));
    }

    [Then(@"^I should be told that the account ""([^""]*)"" was renamed to ""([^""]*)""$")]
    public void ThenIShouldBeToldTheAccountWasRenamed(string oldName, string newName)
    {
        var renamed = Assert.IsType<RenameAccountResult>(context.LastAttempt);
        Assert.Equal(RenameOutcome.Renamed, renamed.Outcome);
        Assert.Equal((oldName, newName), (renamed.OldName, renamed.Account!.Name));
        AssertTold(Tekst.AccountRenamed(oldName, renamed.Account));
    }

    [Then(@"^I should be told that the account ""([^""]*)"" was deleted$")]
    public void ThenIShouldBeToldTheAccountWasDeleted(string account)
    {
        var deleted = Assert.IsType<SpecContext.AccountDeleted>(context.LastAttempt);
        Assert.Equal(account, deleted.Account.Name);
        AssertTold(Tekst.AccountDeleted(deleted.Account));
    }

    [Then(@"^I should be told that ""([^""]*)"" is now the pool account$")]
    public void ThenIShouldBeToldIsNowThePoolAccount(string account)
    {
        var made = Assert.IsType<SpecContext.PoolMade>(context.LastAttempt);
        Assert.Equal(account, made.Account.Name);
        AssertTold(Tekst.PoolChanged(made.Account));
    }

    [Then(@"^I should be told that an account needs a name$")]
    public void ThenIShouldBeToldAnAccountNeedsAName() => AssertAccountRefused(AccountRefusal.NameMissing, RenameRefusal.NameMissing);

    [Then(@"^I should be told that another account already has that name$")]
    public void ThenIShouldBeToldTheNameIsTaken() => AssertAccountRefused(AccountRefusal.NameTaken, RenameRefusal.NameTaken);

    [Then(@"^the balance correction should be recorded$")]
    public void ThenTheBalanceCorrectionShouldBeRecorded() =>
        Assert.True(Assert.IsType<CorrectBalanceResult>(context.LastAttempt).WasRecorded, "Expected it to be recorded, but it was refused.");

    [Then(@"^the balance correction should be refused$")]
    public void ThenTheBalanceCorrectionShouldBeRefused()
    {
        switch (context.LastAttempt)
        {
            case SpecContext.Unread { What: "balance correction" }:
                break;
            case CorrectBalanceResult result:
                Assert.False(result.WasRecorded, "Expected the balance correction to be refused, but it was recorded.");
                break;
            default:
                throw new InvalidOperationException($"The last thing done was not a balance correction: {context.LastAttempt}.");
        }

        Assert.True(App.Notice?.IsRefusal, "Expected a refusal.");
    }

    [Then(@"^the transfer should be recorded$")]
    public void ThenTheTransferShouldBeRecorded()
    {
        var result = Assert.IsType<RecordTransferResult>(context.LastAttempt);
        Assert.True(result.WasRecorded, $"Expected it to be recorded, but it was refused: {result.Refusal}.");
        Assert.False(App.TransferForm.IsOpen, "The transfer form should have emptied.");
    }

    [Then(@"^the transfer should not be recorded$")]
    public void ThenTheTransferShouldNotBeRecorded()
    {
        switch (context.LastAttempt)
        {
            case SpecContext.Unread { What: "transfer" }:
                break;
            case RecordTransferResult result:
                Assert.False(result.WasRecorded, "Expected the transfer to be refused, but it was recorded.");
                break;
            default:
                throw new InvalidOperationException($"The last thing done was not recording a transfer: {context.LastAttempt}.");
        }
    }

    [Then(@"^I should be told that a transfer needs two different accounts$")]
    public void ThenIShouldBeToldATransferNeedsTwoAccounts() => AssertTransferRefused(TransferRefusal.SameAccount);

    [Then(@"^I should be told that a transfer must be more than 0 euro$")]
    public void ThenIShouldBeToldATransferMustBeMoreThanZero() => AssertTransferRefused(TransferRefusal.AmountNotPositive);

    [Then(@"^I should be told that a transfer cannot be dated in the future$")]
    public void ThenIShouldBeToldATransferCannotBeInTheFuture() => AssertTransferRefused(TransferRefusal.DateInFuture);

    [Then(@"^I should be told that the transfer was changed$")]
    public void ThenIShouldBeToldTheTransferWasChanged()
    {
        var changed = Assert.IsType<ChangeTransferResult>(context.LastAttempt);
        Assert.Equal(ChangeOutcome.Changed, changed.Outcome);
        AssertTold(Tekst.TransferChanged(changed.Transfer!));
    }

    [Then(@"^I should be told that the (transfer|balance correction|starting balance) was removed$")]
    public void ThenIShouldBeToldItWasRemoved(string what)
    {
        var removed = Assert.IsType<SpecContext.Removed>(context.LastAttempt);
        Assert.Equal(what, removed.What);
        AssertTold(removed.Said);
    }

    // ------------------------------------------------------------------ doing

    private void AddAccount(string name, string balance, DateOnly day)
    {
        var result = context.AsIfToday(day, () => Ledger.AddAccount(name, SpecParsing.Amount(balance)));
        Assert.False(result.WasRefused, $"Setting up the account {name} was refused: {result.Refusal}.");
    }

    // Through the form in the strip, as the user adds one: opened, filled in, submitted.
    private void Add(string name, string typed)
    {
        App.AccountForm.OpenCommand.Execute(null);
        App.AccountForm.Name = name;
        App.AccountForm.StartingBalance = typed;

        var result = App.AddAccount(App.AccountForm.Name, App.AccountForm.StartingBalance);
        if (result is null)
        {
            context.RecordUnread(typed, "account");
            return;
        }

        if (!result.WasRefused) App.AccountForm.Cancel();
        context.Record(result);
    }

    private void Correct(string account, string typed)
    {
        var chosen = OpenHistoryOf(account);
        App.BalanceInput = typed;

        var result = App.CorrectBalance(chosen, App.BalanceInput);
        if (result is null) context.RecordUnread(typed, "balance correction");
        else context.Record(result);
    }

    private void RecordTransfer(string amount, string from, string to, string? day)
    {
        var form = App.TransferForm;
        form.OpenCommand.Execute(null);
        form.ChosenFrom = Ledger.Account(from);
        form.ChosenTo = Ledger.Account(to);
        form.Amount = amount;
        if (day is not null) form.Date = Ledger.Date(day).ToDateTime(TimeOnly.MinValue);

        var result = form.Record();
        if (result is null) context.RecordUnread(amount, "transfer");
        else context.Record(result);
    }

    // Clicking the transfer's row in the history of the account it came from.
    private TransferForm LoadTransfer(string amount, string from, string to)
    {
        App.EditTransfer(TransferLine(from, amount, from, to));
        Assert.True(App.TransferForm.IsEditing);
        return App.TransferForm;
    }

    private void SaveTransfer(TransferForm form)
    {
        var typed = form.Amount;
        var result = form.Save();
        if (result is null) context.RecordUnread(typed ?? "", "change");
        else context.Record(result);
    }

    private void RemoveFromHistory(string account, string what, HistoryLine line, bool confirm)
    {
        OpenHistoryOf(account);
        App.RemoveFromHistory(line);

        var asked = App.Question?.Text;
        context.AskedFirst = asked is not null && App.History.Any(l => l.Entry.Id == line.Entry.Id);

        if (confirm)
        {
            var said = line.Entry switch
            {
                Transfer t => Tekst.TransferRemoved(t),
                BalanceCorrection c => Tekst.BalanceCorrectionRemoved(c),
                _ => throw new InvalidOperationException($"Nothing like {line.Entry} is removed from a history."),
            };
            App.Confirm();
            context.RecordRemoved(what, asked ?? "", said);
        }
        else
        {
            App.Decline();
            context.RecordDeclined(what);
        }
    }

    // ------------------------------------------------------------------ finding

    // Clicking the account in the strip, unless its history is already open: clicking it again
    // would close it.
    private Account OpenHistoryOf(string name)
    {
        var account = LineOf(name).Account;
        if (App.HistoryAccount != account) App.OpenHistory(account);
        return account;
    }

    private AccountLine LineOf(string name) =>
        App.Accounts.SingleOrDefault(l => l.Name == name)
        ?? throw new InvalidOperationException($"The strip shows no account \"{name}\".");

    private HistoryLine TransferLine(string account, string amount, string from, string to)
    {
        OpenHistoryOf(account);
        var money = SpecParsing.MoneyAmount(amount);
        return Assert.Single(App.History, l =>
            l.Entry is Transfer t && t.Amount == money && t.From.Name == from && t.To.Name == to);
    }

    private HistoryLine CorrectionLine(string account, string balance)
    {
        OpenHistoryOf(account);
        var money = SpecParsing.MoneyAmount(balance);
        return Assert.Single(App.History, l => l.Kind == HistoryKind.BalanceCorrection && l.Balance == money);
    }

    private HistoryLine StartingBalanceLine(string account)
    {
        OpenHistoryOf(account);
        return Assert.Single(App.History, l => l.Kind == HistoryKind.StartingBalance);
    }

    private static string KindName(HistoryKind kind) => kind switch
    {
        HistoryKind.StartingBalance => "starting balance",
        HistoryKind.BalanceCorrection => "balance correction",
        HistoryKind.Transfer => "transfer",
        HistoryKind.Income => "income",
        HistoryKind.Expense => "expense",
        HistoryKind.Movement => "movement",
        HistoryKind.Reallocation => "reallocation",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    // A reallocation's end as the history table writes it: "Unclaimed", a category, or "Unassigned".
    private static string EndName(ReallocationEnd end) => end.Kind switch
    {
        ReallocationEndKind.Unclaimed => "Unclaimed",
        ReallocationEndKind.Category => end.Category!.Name,
        _ => "Unassigned",
    };

    // ------------------------------------------------------------------ checking

    private void AssertTold(string text)
    {
        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.False(notice.IsRefusal);
        Assert.Equal(text, notice.Text);
    }

    private void AssertRefusedWith(string text)
    {
        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.True(notice.IsRefusal, "Expected a refusal.");
        Assert.Equal(text, notice.Text);
    }

    private void AssertAccountRefused(AccountRefusal adding, RenameRefusal renaming)
    {
        switch (context.LastAttempt)
        {
            case AddAccountResult added:
                Assert.Equal(adding, added.Refusal);
                AssertRefusedWith(Tekst.Refusal(adding));
                break;
            case RenameAccountResult renamed:
                Assert.Equal(renaming, renamed.Refusal);
                AssertRefusedWith(Tekst.AccountRenameRefusal(renaming));
                break;
            default:
                throw new InvalidOperationException($"The last thing done was not adding or renaming an account: {context.LastAttempt}.");
        }
    }

    private void AssertTransferRefused(TransferRefusal expected)
    {
        var refusal = context.LastAttempt switch
        {
            RecordTransferResult recorded => recorded.Refusal,
            ChangeTransferResult changed => changed.Refusal,
            _ => throw new InvalidOperationException($"The last thing done was not a transfer: {context.LastAttempt}."),
        };
        Assert.Equal(expected, refusal);
        AssertRefusedWith(Tekst.Refusal(expected));
    }

    // A name in a table cell, which an outline may fill in with its quotation marks still on.
    private static string Name(string cell) =>
        cell.Length >= 2 && cell[0] == '"' && cell[^1] == '"' ? cell[1..^1] : cell;

    private static bool YesNo(string text) => text switch
    {
        "yes" => true,
        "no" => false,
        _ => throw new ArgumentException($"Expected yes or no, not \"{text}\".", nameof(text)),
    };
}
