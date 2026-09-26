using System.Text;
using CommunityToolkit.Mvvm.Input;
using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using MoneyBud.Storage;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for keeping data between runs — features/keep-data.feature, start-moneybud.feature and
/// carry-on-when-saving-fails.feature.
///
/// <para>Closing and starting go through the same doors the Desktop uses:
/// <see cref="MoneyBudApp.Close"/> and <see cref="MoneyBudStart.Start"/>, over the real
/// <see cref="FileLedgerStore"/> in the scenario's own folder (<see cref="SpecContext"/>). What
/// happens <i>around</i> MoneyBud — saving becoming impossible, kept data being damaged, MoneyBud
/// being cut off mid-save — is done to that folder, the way it would happen on a real disk.</para>
/// </summary>
[Binding]
public sealed class KeepingSteps(SpecContext context)
{
    private Ledger Ledger => context.Ledger;
    private MoneyBudApp App => context.App;

    private byte[]? unreadable;
    private StartResult? secondStart;
    private bool? askingWhenClosed;
    private (string What, string Asked, string Said)? waiting;

    // ------------------------------------------------------------------ Given: what is kept

    [Given(@"^I have never used MoneyBud$")]
    public void GivenIHaveNeverUsedMoneyBud() => Assert.False(File.Exists(context.DataFile));

    [Given(@"^I used MoneyBud, recorded an income of (\S+) euro labelled ""([^""]*)"" in it, and have since deleted the data it kept$")]
    public void GivenIUsedMoneyBudAndDeletedItsData(string amount, string label)
    {
        Assert.True(App.RecordIncome(amount, label)?.WasRecorded);
        context.Close();
        Assert.True(File.Exists(context.DataFile));
        File.Delete(context.DataFile);
    }

    [Given(@"^MoneyBud could not read the data it kept, and I have since deleted it$")]
    public void GivenMoneyBudCouldNotReadItsDataAndIDeletedIt()
    {
        WriteKeptData(Encoding.UTF8.GetBytes(Damaged));
        Assert.IsType<StartResult.Refused>(context.Start());
        File.Delete(context.DataFile);
    }

    [Given(@"^MoneyBud has kept data that (is damaged|is blank, with nothing at all in it|was written by a newer version of MoneyBud|cannot be reached)$")]
    public void GivenMoneyBudHasKeptData(string problem)
    {
        WriteKeptData(problem switch
        {
            "is damaged" => Encoding.UTF8.GetBytes(Damaged),
            "is blank, with nothing at all in it" => [],
            "was written by a newer version of MoneyBud" => Encoding.UTF8.GetBytes(WrittenByANewerVersion()),
            _ => Encoding.UTF8.GetBytes(LedgerJson.Write(new LedgerSnapshot([new(1, "Groceries", false)], [], [], [], 0))),
        });

        // Readable data, out of reach: a folder stands where MoneyBud claims its data, so the
        // claim fails the way a blocked folder does, and the data itself is never got at.
        if (problem == "cannot be reached")
            Directory.CreateDirectory(Path.Combine(context.Folder, FileLedgerStore.LockFileName));
    }

    [Given(@"^MoneyBud is open$")]
    public void GivenMoneyBudIsOpen() => _ = App;

    // A When as well as a Given: it happens around MoneyBud, not on its screen. What the Givens set
    // up before it is kept.
    [Given(@"^saving is not possible$")]
    [When(@"^saving is not possible$")]
    public void SavingIsNotPossible()
    {
        context.KeepGivens();
        context.BlockSaving();
    }

    [When(@"^saving is possible again$")]
    public void WhenSavingIsPossibleAgain() => context.UnblockSaving();

    // ------------------------------------------------------------------ When: closing and starting

    [When(@"^I close MoneyBud and start it again$")]
    public void WhenICloseAndStartAgain() => context.Restart();

    [When(@"^I close MoneyBud, and start it again on the first day of the next budget period$")]
    public void WhenICloseAndStartAgainNextPeriod() => context.Restart(Ledger.Period("next").FirstDay);

    [When(@"^I close MoneyBud, and start it again on the first day of the budget period (\d+) after the current one$")]
    public void WhenICloseAndStartAgainPeriodsLater(int count) =>
        context.Restart(Ledger.PeriodsFromCurrent(count).FirstDay);

    [When(@"^I close MoneyBud(?: before it has tried to save again)?$")]
    public void WhenICloseMoneyBud()
    {
        askingWhenClosed = App.IsAsking;
        context.Close();
    }

    [When(@"^I start MoneyBud$")]
    public void WhenIStartMoneyBud() => context.Start();

    [When(@"^I start MoneyBud again while it is open$")]
    public void WhenIStartMoneyBudAgainWhileItIsOpen() => secondStart = context.StartAgainWhileOpen();

    [When(@"^MoneyBud is interrupted while saving it$")]
    public void WhenMoneyBudIsInterruptedWhileSaving() => context.InterruptTheLastSave();

    // The Desktop's once-a-minute timer, which is what "a while" is long enough for.
    [When(@"^a while passes with nothing done$")]
    public void WhenAWhilePassesWithNothingDone() => App.Tick();

    // ------------------------------------------------------------------ When: on the screen, not finished

    [When(@"^I type an expense of (\S+) euro for ""([^""]*)"" labelled ""([^""]*)"" without recording it$")]
    public void WhenITypeAnExpenseWithoutRecordingIt(string amount, string category, string label)
    {
        var form = App.ExpenseForm;
        form.Amount = amount;
        form.Category = category;
        form.Label = label;
    }

    [When(@"^I start changing the amount of the expense labelled ""([^""]*)"" to (\S+) euro, and do not save it$")]
    public void WhenIStartChangingTheAmountWithoutSaving(string label, string amount)
    {
        App.EditExpense(ExpenseLabelled(label));
        App.ExpenseForm.Amount = amount;
    }

    [When(@"^I start renaming the category ""([^""]*)"" to ""([^""]*)"", and do not save it$")]
    public void WhenIStartRenamingWithoutSaving(string name, string newName)
    {
        App.StartRename(name);
        App.NewName = newName;
    }

    // Removing in its two halves (remove-an-entry.feature has it as one): Verwijderen, and later
    // the answer.
    [When(@"^I ask to remove the expense labelled ""([^""]*)""$")]
    public void WhenIAskToRemove(string label)
    {
        var line = ExpenseLabelled(label);
        App.EditExpense(line);
        App.ExpenseForm.Remove();
        waiting = ("expense", App.Question?.Text ?? "", Tekst.ExpenseRemoved(line.Entry));
    }

    [When(@"^I confirm$")]
    public void WhenIConfirm()
    {
        var (what, asked, said) = waiting ?? throw new InvalidOperationException("Nothing was asked.");
        App.Confirm();
        context.RecordRemoved(what, asked, said);
        waiting = null;
    }

    // ------------------------------------------------------------------ Then: starting

    [Then(@"^I should be told that MoneyBud cannot read my data$")]
    public void ThenIShouldBeToldThatMoneyBudCannotReadMyData()
    {
        var refused = Assert.IsType<StartResult.Refused>(context.LastStart);
        Assert.Equal(StartRefusal.CannotRead, refused.Reason);
        Assert.Equal(Tekst.CannotRead, refused.Text);
    }

    // Names no folder or file, and does not send the user to the README (arc42 §12).
    [Then(@"^that message should not say where my data is kept, nor where to find out$")]
    public void ThenThatMessageShouldNotSayWhereMyDataIsKept()
    {
        var text = Assert.IsType<StartResult.Refused>(context.LastStart).Text;

        foreach (var place in (string[])[context.Folder, FileLedgerStore.DefaultFolder, FileLedgerStore.DataFileName,
                                         "README", "AppData", "MoneyBud\\", "/", "\\", ".json"])
            Assert.DoesNotContain(place, text, StringComparison.OrdinalIgnoreCase);
    }

    [Then(@"^MoneyBud should close without opening the Overview$")]
    public void ThenMoneyBudShouldCloseWithoutOpeningTheOverview() =>
        Assert.IsType<StartResult.Refused>(context.LastStart);

    [Then(@"^the data MoneyBud could not read should be exactly as it was$")]
    public void ThenTheDataShouldBeExactlyAsItWas()
    {
        Assert.Equal(unreadable, File.ReadAllBytes(context.DataFile));
        Assert.False(Path.Exists(Path.Combine(context.Folder, FileLedgerStore.TemporaryFileName)));
    }

    [Then(@"^the second start should tell me that MoneyBud is already open$")]
    public void ThenTheSecondStartShouldTellMeMoneyBudIsAlreadyOpen()
    {
        var refused = Assert.IsType<StartResult.Refused>(secondStart);
        Assert.Equal(StartRefusal.AlreadyOpen, refused.Reason);
        Assert.Equal(Tekst.AlreadyOpen, refused.Text);
    }

    [Then(@"^the second start should close without opening the Overview$")]
    public void ThenTheSecondStartShouldCloseWithoutOpeningTheOverview() =>
        Assert.IsType<StartResult.Refused>(secondStart);

    [Then(@"^the MoneyBud that was already open should still show the previous budget period$")]
    public void ThenTheOpenMoneyBudShouldStillShowThePreviousPeriod() =>
        Assert.Equal(Ledger.Period("previous"), App.ShownPeriod);

    [Then(@"^no categories should be offered for a new expense$")]
    public void ThenNoCategoriesShouldBeOffered() => Assert.Empty(App.CategorySuggestions);

    // ------------------------------------------------------------------ Then: what is on screen

    [Then(@"^the expenses listed in the budget period (\d+) before the current one should be exactly these, in this order:$")]
    public void ThenTheExpensesListedPeriodsBackShouldBeExactly(int count, Table table)
    {
        var expected = table.Rows.Select(row => (
            Ledger.Date(row["date"]),
            row["category"],
            row["label"] is "" ? null : row["label"],
            SpecParsing.MoneyAmount(row["amount"])));

        var actual = App.OverviewFor(Ledger.PeriodsFromCurrent(-count)).Expenses
            .Select(e => (e.Date, e.Category, e.Label, e.Amount));

        Assert.Equal(expected, actual);
    }

    [Then(@"^the expense form should be empty and ready for a new expense$")]
    public void ThenTheExpenseFormShouldBeEmpty()
    {
        var form = App.ExpenseForm;
        Assert.False(form.IsEditing);
        Assert.True(string.IsNullOrEmpty(form.Amount));
        Assert.True(string.IsNullOrEmpty(form.Category));
        Assert.True(string.IsNullOrEmpty(form.Label));
        Assert.Null(form.Date);
    }

    [Then(@"^no rename should be in progress$")]
    public void ThenNoRenameShouldBeInProgress() => Assert.Null(App.Renaming);

    [Then(@"^MoneyBud should be asking me to confirm$")]
    public void ThenMoneyBudShouldBeAskingMeToConfirm() => Assert.True(App.IsAsking);

    [Then(@"^MoneyBud should not be asking me anything$")]
    public void ThenMoneyBudShouldNotBeAskingMeAnything() => Assert.False(App.IsAsking);

    // ------------------------------------------------------------------ Then: saving

    [Then(@"^MoneyBud should (?:still )?show that my changes are not saved$")]
    public void ThenMoneyBudShouldShowNotSaved()
    {
        Assert.True(App.IsUnsaved);
        Assert.Equal(Tekst.NotSaved, App.SaveLine);
    }

    [Then(@"^MoneyBud should (?:no longer|not) show that my changes are not saved$")]
    public void ThenMoneyBudShouldNotShowNotSaved()
    {
        Assert.False(App.IsUnsaved);
        Assert.NotEqual(Tekst.NotSaved, App.SaveLine);
    }

    [Then(@"^I should be told that everything is saved again$")]
    public void ThenIShouldBeToldThatEverythingIsSavedAgain() => Assert.Equal(Tekst.SavedAgain, App.SaveLine);

    // A save that works is not announced: not on the save line, and not in the notice either.
    [Then(@"^nothing should have been said about saving$")]
    public void ThenNothingShouldHaveBeenSaidAboutSaving()
    {
        Assert.Null(App.SaveLine);
        Assert.DoesNotContain("opgeslagen", App.Notice?.Text ?? "", StringComparison.OrdinalIgnoreCase);
    }

    // The acts the screen offers are the commands of the screen and its forms, and the window's
    // buttons are bound to those and nothing else. Both are listed in full, so a new act — one to
    // save, or to start over — fails here until it is looked at. None of those listed saves data or
    // starts over: the forms' Submit records or saves an entry, Remove asks to remove one.
    [Then(@"^MoneyBud should offer no act for (saving|starting over)$")]
    public void ThenMoneyBudShouldOfferNoActFor(string _)
    {
        string[] screen =
        [
            "ArchiveCommand", "CancelRenameCommand", "ConfirmCommand", "DeclineCommand", "DeleteCommand",
            "EditExpenseCommand", "EditIncomeCommand", "RenameCommand", "StartRenameCommand",
            "StepBackCommand", "StepForwardCommand",
        ];
        Assert.Equal(screen, CommandsOf(App));

        var forms = new Dictionary<string, object>
        {
            ["ExpenseForm"] = App.ExpenseForm, ["IncomeForm"] = App.IncomeForm,
            ["AssignForm"] = App.AssignForm, ["CategoryForm"] = App.CategoryForm,
        };
        var offered = screen.ToHashSet();
        foreach (var (name, form) in forms)
        {
            var commands = CommandsOf(form);
            Assert.Subset(new HashSet<string>(["SubmitCommand", "CancelCommand", "RemoveCommand",
                                               "EarlierPeriodCommand", "LaterPeriodCommand"]), commands.ToHashSet());
            offered.UnionWith(commands);
        }

        // Every button in the window is bound to one of those, and to nothing else.
        var bound = WindowCommands().Select(c => c[(c.LastIndexOf('.') + 1)..]).ToHashSet();
        Assert.Subset(offered, bound);
    }

    [Then(@"^MoneyBud should close without asking me anything$")]
    public void ThenMoneyBudShouldCloseWithoutAskingMeAnything()
    {
        Assert.False(askingWhenClosed, "A question was waiting when MoneyBud closed.");

        // Closed, and let go of its data: another start could take it now.
        using var free = new FileLedgerStore(context.Folder);
        Assert.Equal(Claim.Claimed, free.TryClaim());
    }

    // ------------------------------------------------------------------ Shared

    // A file cut off in the middle, as a damaged disk or an editor might leave it.
    private const string Damaged = "{\n  \"format\": \"MoneyBud\",\n  \"version\": 1,\n  \"lastEntryId\": 3,\n  \"categ";

    private static string WrittenByANewerVersion()
    {
        var snapshot = new LedgerSnapshot([new CategorySnapshot(1, "Groceries", false)], [], [], [], 0);
        var text = LedgerJson.Write(snapshot);
        Assert.Contains("\"version\": 1", text);
        return text.Replace("\"version\": 1", $"\"version\": {LedgerJson.Version + 1}");
    }

    private void WriteKeptData(byte[] bytes)
    {
        Directory.CreateDirectory(context.Folder);
        File.WriteAllBytes(context.DataFile, bytes);
        unreadable = bytes;
    }

    private ExpenseLine ExpenseLabelled(string label) => Assert.Single(App.Overview.Expenses, e => e.Label == label);

    // Every Command binding in the window's markup, as written.
    private static IEnumerable<string> WindowCommands() =>
        System.Text.RegularExpressions.Regex
            .Matches(Repository.ReadText("src", "MoneyBud.Desktop", "MainWindow.axaml"), @"Command=""\{Binding ([^}]*)\}""")
            .Select(m => m.Groups[1].Value);

    private static List<string> CommandsOf(object target) =>
        target.GetType().GetProperties()
            .Where(p => typeof(IRelayCommand).IsAssignableFrom(p.PropertyType))
            .Select(p => p.Name)
            .Order(StringComparer.Ordinal)
            .ToList();
}
