using MoneyBud.Domain;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps whose sentences do not name what was attempted, because the rules behind them do not
/// either.
///
/// <para>Several feature files use these words, and that is deliberate rather than accidental
/// duplication. "I should not be warned or asked to confirm" is a claim about MoneyBud — it
/// shows, it never blocks (arc42 §12) — not a claim about expenses; the cent rule is stated by
/// arc42 §8.2 about <i>amounts</i>; and a category is "brought back" by adding its name and by
/// recording an expense against it alike. One definition serves every file, and it asks
/// whichever attempt came last, because a scenario may have done something else during setup
/// and presence alone would pick the wrong one.</para>
/// </summary>
[Binding]
public sealed class SharedSteps(SpecContext context)
{
    [Then(@"I should not be warned or asked to confirm")]
    public void ThenIShouldNotBeWarnedOrAskedToConfirm()
    {
        // There is no warning to assert the absence of. None of the result types has anywhere to
        // put one or an outcome between done and refused, so asserting that the act simply went
        // through is what "not warned or asked to confirm" amounts to.
        switch (context.LastAttempt)
        {
            case RecordExpenseResult expense:
                Assert.True(expense.WasRecorded);
                Assert.Null(expense.Refusal);
                break;
            case RecordIncomeResult income:
                Assert.True(income.WasRecorded);
                Assert.Null(income.Refusal);
                break;
            case AddCategoryResult added:
                Assert.False(added.WasRefused);
                Assert.NotNull(added.Category);
                break;
            case SpecContext.Archived archived:
                // Archiving returns the category it put away, or throws for a non-case. Having
                // one is the whole of "it went through".
                Assert.NotNull(archived.Category);
                break;
            default:
                throw NothingAttempted();
        }
    }

    [Then(@"I should be told that an amount cannot be finer than a cent")]
    public void ThenIShouldBeToldAnAmountCannotBeFinerThanACent()
    {
        switch (context.LastAttempt)
        {
            case RecordExpenseResult expense:
                Assert.False(expense.WasRecorded, "Expected the expense to be refused, but it was recorded.");
                Assert.Equal(ExpenseRefusal.AmountFinerThanCent, expense.Refusal);
                break;
            case RecordIncomeResult income:
                Assert.False(income.WasRecorded, "Expected the income to be refused, but it was recorded.");
                Assert.Equal(IncomeRefusal.AmountFinerThanCent, income.Refusal);
                break;
            default:
                throw NothingAttempted();
        }
    }

    // The name is checked letter for letter: being told that "Hobby" was brought back after
    // typing "  hobby " is how the scenarios assert that it came back spelled as it was.
    [Then(@"I should be told that ""([^""]*)"" was brought back")]
    public void ThenIShouldBeToldThatWasBroughtBack(string category)
    {
        switch (context.LastAttempt)
        {
            case AddCategoryResult added:
                Assert.Equal(AddCategoryOutcome.BroughtBack, added.Outcome);
                Assert.Equal(category, added.Category!.Name);
                break;
            case RecordExpenseResult expense:
                Assert.True(expense.WasRecorded, $"Expected the expense to be recorded, but it was refused: {expense.Refusal}.");
                Assert.True(expense.CategoryBroughtBack, "Expected the expense to bring its category back.");
                Assert.Equal(category, expense.Expense!.Category.Name);
                break;
            default:
                throw NothingAttempted();
        }
    }

    private static InvalidOperationException NothingAttempted() =>
        new("Nothing has been attempted yet.");
}
