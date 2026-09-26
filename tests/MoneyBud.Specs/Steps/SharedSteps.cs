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
/// arc42 §8.2 about <i>amounts</i>; a category is "brought back" by adding its name, by
/// recording an expense against it and by assigning to it alike; and an unknown category name is
/// refused by recording and assigning alike. One definition serves every file, and it asks
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
        // through is what "not warned or asked to confirm" amounts to. The one question MoneyBud
        // can ask is whether to remove an entry, and none may be waiting.
        Assert.False(context.App.IsAsking, $"Expected no question, but MoneyBud asked: {context.App.Question?.Text}");

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
            case AssignResult assigned:
                Assert.True(assigned.WasAssigned);
                Assert.Null(assigned.Refusal);
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
            case ChangeExpenseResult changed:
                Assert.False(changed.WasRefused);
                break;
            case ChangeIncomeResult changed:
                Assert.False(changed.WasRefused);
                break;
            case RenameCategoryResult renamed:
                Assert.False(renamed.WasRefused);
                break;
            case SpecContext.Deleted deleted:
                // As archiving: the category it deleted, or a throw for a non-case.
                Assert.NotNull(deleted.Category);
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
            case AssignResult assigned:
                Assert.False(assigned.WasAssigned, "Expected the assignment to be refused, but it went through.");
                Assert.Equal(AssignRefusal.AmountFinerThanCent, assigned.Refusal);
                break;
            case ChangeExpenseResult changed:
                Assert.Equal(ExpenseRefusal.AmountFinerThanCent, changed.Refusal);
                break;
            case ChangeIncomeResult changed:
                Assert.Equal(IncomeRefusal.AmountFinerThanCent, changed.Refusal);
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
            case AssignResult assigned:
                Assert.True(assigned.WasAssigned, $"Expected the assignment to go through, but it was refused: {assigned.Refusal}.");
                Assert.True(assigned.CategoryBroughtBack, "Expected the assignment to bring its category back.");
                Assert.Equal(category, assigned.Category!.Name);
                break;
            case ChangeExpenseResult changed:
                Assert.Equal(ChangeOutcome.Changed, changed.Outcome);
                Assert.True(changed.CategoryBroughtBack, "Expected the change to bring the category back.");
                Assert.Equal(category, changed.Expense!.Category.Name);
                break;
            default:
                throw NothingAttempted();
        }
    }

    // Recording an expense, changing one and assigning all name a category, and all refuse one
    // that is not the user's — in use or archived — without adding it.
    [Then(@"I should be told that ""([^""]*)"" is not one of my categories")]
    public void ThenIShouldBeToldThatIsNotOneOfMyCategories(string category)
    {
        switch (context.LastAttempt)
        {
            case RecordExpenseResult expense:
                Assert.False(expense.WasRecorded, "Expected the expense to be refused, but it was recorded.");
                Assert.Equal(ExpenseRefusal.UnknownCategory, expense.Refusal);
                break;
            case AssignResult assigned:
                Assert.False(assigned.WasAssigned, "Expected the assignment to be refused, but it went through.");
                Assert.Equal(AssignRefusal.UnknownCategory, assigned.Refusal);
                break;
            case ChangeExpenseResult changed:
                Assert.Equal(ExpenseRefusal.UnknownCategory, changed.Refusal);
                break;
            default:
                throw NothingAttempted();
        }

        Assert.False(context.Ledger.HasCategory(category), $"{category} should not be a category.");
    }

    private static InvalidOperationException NothingAttempted() =>
        new("Nothing has been attempted yet.");
}
