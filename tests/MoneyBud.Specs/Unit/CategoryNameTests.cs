using System.Globalization;
using MoneyBud.Domain;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for the edges of the category name rule and of archiving that the scenarios
/// deliberately do not reach. The scenarios remain the contract (features/README.md): trimming,
/// ignoring case, counting a run of inner spaces as one, keeping the existing spelling and
/// refusing a blank name are all asserted there and are not repeated here.
///
/// <para>What is left is whitespace a feature file cannot legibly write — a tab inside a Gherkin
/// step is invisible to a reader — the machine's culture, which no scenario can vary, and the two
/// archiving non-cases, which have no user-facing behaviour and so no scenario at all.</para>
/// </summary>
public class CategoryNameTests
{
    private readonly Ledger ledger = new SpecContext().Ledger;

    [Theory]
    [InlineData("\tVaste lasten\t")]
    [InlineData("\nVaste lasten\n")]
    [InlineData("Vaste\tlasten")]
    [InlineData("Vaste \t lasten")]
    [InlineData("vaste lasten")]
    public void Any_whitespace_counts_for_the_name_rule_not_only_spaces(string typed)
    {
        ledger.AddCategory("Vaste lasten");

        var result = ledger.AddCategory(typed);

        Assert.Equal(AddCategoryOutcome.AlreadyThere, result.Outcome);
        Assert.Equal("Vaste lasten", result.Category!.Name);
    }

    [Theory]
    [InlineData("\t")]
    [InlineData(" \t\n ")]
    public void A_name_of_nothing_but_whitespace_is_refused(string typed) =>
        Assert.Equal(CategoryRefusal.NameMissing, ledger.AddCategory(typed).Refusal);

    [Fact]
    public void A_new_name_is_stored_with_its_inner_whitespace_exactly_as_typed() =>
        Assert.Equal("Vaste  lasten", ledger.AddCategory("  Vaste  lasten  ").Category!.Name);

    [Fact]
    public void Case_is_ignored_the_same_way_whatever_the_machine_culture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            // Under Turkish casing rules "I" lowercases to a dotless ı, so a culture-sensitive
            // comparison would make "HOBI" and "hobi" two categories on a Turkish machine only.
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            ledger.AddCategory("hobi");

            Assert.Equal(AddCategoryOutcome.AlreadyThere, ledger.AddCategory("HOBI").Outcome);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Archiving_a_name_that_is_not_a_category_is_a_mistake_in_the_caller() =>
        Assert.Throws<InvalidOperationException>(() => ledger.ArchiveCategory("Holiday"));

    [Fact]
    public void Archiving_a_category_that_is_already_archived_is_a_mistake_in_the_caller()
    {
        ledger.AddCategory("Hobby");
        ledger.ArchiveCategory("Hobby");

        Assert.Throws<InvalidOperationException>(() => ledger.ArchiveCategory("hobby"));
    }

    [Fact]
    public void An_empty_ledger_has_no_categories_and_a_new_MoneyBud_has_the_six_defaults()
    {
        Assert.Empty(ledger.CategoriesOffered);
        Assert.Equal(
            Ledger.DefaultCategoryNames,
            Ledger.StartNew(TimeProvider.System).CategoriesOffered.Select(c => c.Name));
    }
}
