using MoneyBud.Domain;
using MoneyBud.Specs.Support;
using Reqnroll;

namespace MoneyBud.Specs.Steps;

/// <summary>
/// Steps whose sentences name no transaction type, because the rules behind them do not either.
///
/// <para>Both feature files use these words, and that is deliberate rather than accidental
/// duplication. "I should not be warned or asked to confirm" is a claim about MoneyBud — it
/// shows, it never blocks (arc42 §12) — not a claim about expenses; and the cent rule is stated
/// by arc42 §8.2 about <i>amounts</i>. One definition serves both files, and it asks whichever
/// recording attempt came last, because an income scenario may have recorded an expense during
/// setup and presence alone would pick the wrong one.</para>
/// </summary>
[Binding]
public sealed class TransactionSteps(SpecContext context)
{
    [Then(@"I should not be warned or asked to confirm")]
    public void ThenIShouldNotBeWarnedOrAskedToConfirm()
    {
        // There is no warning to assert the absence of. Neither result type has anywhere to put
        // one and neither has an outcome between recorded and refused, so asserting that the
        // transaction simply went through is what "not warned or asked to confirm" amounts to.
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
            default:
                throw NothingRecorded();
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
                throw NothingRecorded();
        }
    }

    private static InvalidOperationException NothingRecorded() =>
        new("Nothing has been recorded yet.");
}
