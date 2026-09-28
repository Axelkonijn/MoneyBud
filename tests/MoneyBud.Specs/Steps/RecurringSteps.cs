using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;
using Reqnroll.Bindings;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for features/repeat-an-entry.feature, whose header explains them, and change-a-repeat.feature
/// and keep-data.feature's repeats, which share them: a scenario that begins on a calendar date, the
/// day moving on, starting again on a day, the Herhalen list of a new entry, and what MoneyBud says
/// when it records occurrences by itself. The record steps' ", repeating monthly" ending is on the
/// record steps themselves, and the entry steps' "dated …" and the frequency of an entry opened for
/// changing are in <see cref="CorrectionSteps"/>.
/// </summary>
[Binding]
public sealed class RecurringSteps(SpecContext context, ScenarioContext scenario)
{
    private const string CalendarDate = @"(\d{1,2} [A-Z][a-z]+ \d{4})";

    // What MoneyBud had recorded by itself, and what it had last said, before the last When: "since
    // the step before" is since the last thing done, whatever Thens came after it.
    private HashSet<int> recordedBeforeLastWhen = [];
    private Notice? noticeBeforeLastWhen;

    private Ledger Ledger => context.Ledger;
    private MoneyBudApp App => context.App;

    [BeforeStep]
    public void NoteWhatWasRecordedBeforeEachWhen()
    {
        if (scenario.StepContext.StepInfo.StepDefinitionType != StepDefinitionType.When) return;

        recordedBeforeLastWhen = RecordedByMoneyBud();
        noticeBeforeLastWhen = context.AppIfOpen?.Notice;
    }

    // ------------------------------------------------------------------ Given

    [Given(@"^today is " + CalendarDate + "$")]
    public void GivenTodayIs(string date) => context.BeginOn(Day(date));

    // ------------------------------------------------------------------ When

    // The clock moves on to that day without MoneyBud being closed, and the minute's timer ticks, as
    // "the next budget period begins while MoneyBud is open" does (step-between-periods.feature).
    [When(@"^the day becomes " + CalendarDate + @" while MoneyBud is open$")]
    public void WhenTheDayBecomes(string date)
    {
        _ = App;
        context.SetToday(Day(date));
        context.TimePassed();
        App.Tick();
    }

    [When(@"^I close MoneyBud, and start it again on " + CalendarDate + "$")]
    public void WhenICloseAndStartAgainOn(string date)
    {
        context.TimePassed();
        context.Restart(Day(date));
    }

    // ------------------------------------------------------------------ Then

    [Then(@"^a new (expense|income) should start out as a one-off$")]
    public void ThenANewEntryShouldStartOutAsAOneOff(string what)
    {
        var (editing, shown, canChange) = what == "expense"
            ? (App.ExpenseForm.IsEditing, App.ExpenseForm.ChosenFrequency, App.ExpenseForm.CanChangeFrequency)
            : (App.IncomeForm.IsEditing, App.IncomeForm.ChosenFrequency, App.IncomeForm.CanChangeFrequency);

        Assert.False(editing);
        Assert.True(canChange);
        Assert.Null(shown.Frequency);
        Assert.Equal(Tekst.OneOff, shown.Text);
    }

    [Then(@"^the frequencies offered for a new (expense|income) should be exactly these, in this order:$")]
    public void ThenTheFrequenciesOfferedShouldBe(string what, Table table)
    {
        var offered = what == "expense" ? App.ExpenseForm.FrequencyChoices : App.IncomeForm.FrequencyChoices;

        Assert.Equal(
            table.Rows.Select(row => Tekst.FrequencyName(SpecParsing.Frequency(row["frequency"]))),
            offered.Select(choice => choice.Text));
    }

    // One notice names every occurrence MoneyBud recorded by itself just then, one row each, and no
    // other, in any order. "Just then" is since the last When: a notice left over from an earlier
    // step does not count, even one naming the same entries. What is fixed is what is named, never
    // the wording, so the notice's own list is compared, and its sentence is checked to be there.
    [Then(@"^I should be told, in one notice, that these repeating entries were recorded:$")]
    public void ThenIShouldBeToldTheseRepeatingEntriesWereRecorded(Table table)
    {
        var notice = App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.False(ReferenceEquals(notice, noticeBeforeLastWhen), "Nothing new was said since the last thing done.");

        var expected = table.Rows
            .Select(row => Describe(
                row["entry"], row["category"] is "" ? null : row["category"], row["label"] is "" ? null : row["label"],
                SpecParsing.MoneyAmount(row["amount"])))
            .Order();
        var named = notice.Repeated
            .Select(o => o.Entry switch
            {
                Expense e => Describe("expense", e.Category.Name, e.Label, e.Amount),
                Income i => Describe("income", null, i.Label, i.Amount),
                _ => throw new InvalidOperationException($"Not an income or an expense: {o.Entry}."),
            })
            .Order();

        Assert.Equal(expected, named);
        Assert.Contains("Herhaald: ", notice.Text);
        foreach (var occurrence in notice.Repeated)
        {
            // "Netflix € 13,99 (25 september)", as the sentence names it.
            var sentence = Tekst.Repeated([occurrence]);
            var start = "Herhaald: ".Length;
            Assert.Contains(sentence[start..(sentence.IndexOf(')', start) + 1)], notice.Text);
        }
    }

    // Nothing recorded by itself since the last When, and nothing said about it: the notice, if a
    // new one was said, names no occurrence.
    [Then(@"^MoneyBud should not have recorded any repeating entry$")]
    public void ThenMoneyBudShouldNotHaveRecordedAnyRepeatingEntry()
    {
        Assert.Empty(RecordedByMoneyBud().Except(recordedBeforeLastWhen));

        if (!ReferenceEquals(App.Notice, noticeBeforeLastWhen))
            Assert.Empty(App.Notice?.Repeated ?? []);
    }

    // ----------------------------------------------------------------- Shared

    private static DateOnly Day(string date) =>
        SpecParsing.CalendarDate(date) ?? throw new ArgumentException($"Not a calendar date: \"{date}\".", nameof(date));

    private static string Describe(string entry, string? category, string? label, Money amount) =>
        $"{entry} | {category} | {label} | {amount.Cents}";

    // Every occurrence MoneyBud recorded by itself: all of a repeat's occurrences but the first, which
    // the user typed. After the first is removed, the one that takes its place was recorded before,
    // so it never counts as new.
    private HashSet<int> RecordedByMoneyBud() =>
        Ledger.ToSnapshot().Repeats.SelectMany(r => r.Occurrences.Skip(1)).ToHashSet();
}
