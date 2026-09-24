using MoneyBud.Domain;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for the edges of the label rule that the scenarios deliberately do not reach.
/// The scenarios remain the contract (features/README.md): they pin down that a label is trimmed,
/// that a blank one is refused on an income and becomes no label on an expense, and that the
/// label is checked before the amount. All of that is asserted there and is not repeated here.
///
/// <para>What is left to cover is whitespace the feature files cannot legibly write — a tab or a
/// newline inside a Gherkin step is invisible to a reader, so putting it there would make the
/// specification worse to read in exchange for a case nobody disputes. arc42 §12 says whitespace,
/// not spaces, so these check that the code agrees.</para>
/// </summary>
public class LabelTests
{
    private readonly Ledger ledger = new SpecContext().Ledger;

    [Theory]
    [InlineData("\tSalaris\t")]
    [InlineData("\nSalaris\n")]
    [InlineData(" \t Salaris \n ")]
    public void An_income_label_is_trimmed_of_any_whitespace_not_only_spaces(string label)
    {
        var result = ledger.RecordIncome(100m, label, ledger.Today);

        Assert.True(result.WasRecorded);
        Assert.Equal("Salaris", result.Income!.Label);
    }

    [Theory]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData(" \t\n ")]
    public void An_income_label_of_nothing_but_whitespace_is_no_label(string label) =>
        Assert.Equal(
            IncomeRefusal.LabelMissing,
            ledger.RecordIncome(100m, label, ledger.Today).Refusal);

    [Fact]
    public void An_income_label_keeps_the_whitespace_inside_it_exactly()
    {
        var result = ledger.RecordIncome(100m, "  Salaris  september  ", ledger.Today);

        Assert.Equal("Salaris  september", result.Income!.Label);
    }

    [Fact]
    public void An_expense_label_of_nothing_but_whitespace_becomes_no_label()
    {
        ledger.AddCategory("Groceries");

        var result = ledger.RecordExpense(10m, "Groceries", ledger.Today, "\t\n ");

        Assert.True(result.WasRecorded);
        Assert.Null(result.Expense!.Label);
    }

    [Fact]
    public void An_expense_label_is_trimmed_of_any_whitespace_not_only_spaces()
    {
        ledger.AddCategory("Groceries");

        var result = ledger.RecordExpense(10m, "Groceries", ledger.Today, "\tAlbert Heijn\n");

        Assert.Equal("Albert Heijn", result.Expense!.Label);
    }

    /// <summary>
    /// The scenarios pin the order for a blank label against a negative amount. This is the other
    /// pairing — a blank label against an amount finer than a cent — so that the order is fixed
    /// against every amount rule rather than against the one the feature file happened to use.
    /// </summary>
    [Fact]
    public void A_missing_label_is_reported_before_an_amount_finer_than_a_cent() =>
        Assert.Equal(
            IncomeRefusal.LabelMissing,
            ledger.RecordIncome(12.345m, "   ", ledger.Today).Refusal);
}
