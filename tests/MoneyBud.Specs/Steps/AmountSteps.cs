using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps for features/type-an-amount.feature, where the amount is quoted because it is text as
/// typed rather than a number: "12,50", "2.000", "abc".
///
/// <para>Unlike the unquoted steps elsewhere, these do not fail when the text cannot be read.
/// That is the case under test: the screen refuses it and the ledger is never asked, which is
/// recorded as <see cref="SpecContext.Unread"/> so that "should not be recorded" and "should be
/// refused" can tell a refusal that never reached the ledger from one that did.</para>
/// </summary>
[Binding]
public sealed class AmountSteps(SpecContext context)
{
    // ------------------------------------------------------------------- When

    [When(@"^I (?:record|try to record) an expense of ""([^""]*)"" for ""([^""]*)""$")]
    public void WhenIRecordAnExpenseTyped(string typed, string category) =>
        Keep(context.App.RecordExpense(typed, category, label: null), typed, "expense", context.Record);

    [When(@"^I (?:record|try to record) an income of ""([^""]*)"" labelled ""([^""]*)""$")]
    public void WhenIRecordAnIncomeTyped(string typed, string label) =>
        Keep(context.App.RecordIncome(typed, label), typed, "income", context.Record);

    [When(@"^I (?:assign|try to assign) ""([^""]*)"" to ""([^""]*)"" in the (current|previous|next) budget period$")]
    public void WhenIAssignTyped(string typed, string category, string which) =>
        Keep(context.App.Assign(typed, category, context.Ledger.Period(which)), typed, "assignment", context.Record);

    // ------------------------------------------------------------------- Then

    [Then(@"^I should be told that ""([^""]*)"" is not an amount$")]
    public void ThenIShouldBeToldThatIsNotAnAmount(string typed)
    {
        AssertUnread(typed);
        Assert.Equal(AmountReading.NotAnAmount, AmountInput.Read(typed, out _));
    }

    [Then(@"^I should be told that ""([^""]*)"" is ambiguous: (\S+) or (\S+)$")]
    public void ThenIShouldBeToldThatIsAmbiguous(string typed, string asThousands, string asDecimal)
    {
        var notice = AssertUnread(typed);
        Assert.Equal(AmountReading.Ambiguous, AmountInput.Read(typed, out _));

        // Both readings are named, which is what lets the user retype the one they meant.
        Assert.Equal((asThousands, asDecimal), AmountInput.Readings(typed));
        Assert.Contains(asThousands, notice.Text);
        Assert.Contains(asDecimal, notice.Text);
    }

    // ----------------------------------------------------------------- Shared

    private void Keep<T>(T? result, string typed, string what, Action<T> record) where T : class
    {
        if (result is null) context.RecordUnread(typed, what);
        else record(result);
    }

    private Notice AssertUnread(string typed)
    {
        var unread = Assert.IsType<SpecContext.Unread>(context.LastAttempt);
        Assert.Equal(typed, unread.Typed);

        var notice = context.App.Notice ?? throw new InvalidOperationException("Nothing was said.");
        Assert.True(notice.IsRefusal, "Expected a refusal.");
        return notice;
    }
}
