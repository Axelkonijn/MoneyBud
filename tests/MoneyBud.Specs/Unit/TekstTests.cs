using System.Text.RegularExpressions;
using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// The Dutch on screen. The display terms are held to arc42 §12's *Dutch display terms* table,
/// read from the document itself, so the table and the code cannot drift apart unnoticed. The
/// sentences are copy and are not pinned word for word; what is checked is that every refusal
/// reason has one.
/// </summary>
public sealed partial class TekstTests
{
    // Each row of the §12 table, by its English cell, and the constants that show it.
    private static readonly Dictionary<string, string[]> TermsByRow = new()
    {
        ["Expense"] = [Tekst.Expense],
        ["Income"] = [Tekst.Income],
        ["Category"] = [Tekst.Category],
        ["Default categories"] = [Tekst.DefaultCategories],
        ["Budget period"] = [Tekst.BudgetPeriod],
        ["Current period"] = [Tekst.CurrentPeriod],
        ["Budget"] = [Tekst.Budget],
        ["Assign"] = [Tekst.Assign],
        ["Spent"] = [Tekst.Spent],
        ["Remaining"] = [Tekst.Remaining],
        ["Over budget"] = [Tekst.OverBudget],
        ["Unassigned"] = [Tekst.Unassigned],
        ["Over-assigned"] = [Tekst.OverAssigned],
        ["Label"] = [Tekst.Label],
        ["Archive (the act) / Archived (the state)"] = [Tekst.Archive, Tekst.Archived],
        ["Brought back"] = [Tekst.BroughtBack],
        ["The shortfall of a clipped negative assignment"] = [Tekst.Shortfall],
        ["Amount / Date"] = [Tekst.Amount, Tekst.Date],
        ["Previous / next period"] = [Tekst.PreviousPeriod, Tekst.NextPeriod],
        ["Add category / Record expense / Record income"] = [Tekst.AddCategory, Tekst.RecordExpense, Tekst.RecordIncome],
        ["Overview (the start screen)"] = [Tekst.Overview],
        ["Change (an entry)"] = [Tekst.Change],
        ["Remove (an entry)"] = [Tekst.Remove],
        ["Rename (a category)"] = [Tekst.Rename],
        ["Delete (a category)"] = [Tekst.Delete],
    };

    [Fact]
    public void Every_display_term_is_the_one_the_glossary_fixes()
    {
        var rows = DisplayTermsTable();
        Assert.Equal(TermsByRow.Keys.Order(), rows.Keys.Order());

        foreach (var (english, dutch) in rows)
        {
            // Case is left out: a term starts with a capital on a button and may not mid-cell.
            Assert.Equal(TermsByRow[english], Parts(dutch), StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void The_retired_term_is_not_shown_in_Dutch_either()
    {
        var everything = typeof(Tekst).GetFields()
            .Where(f => f.IsLiteral)
            .Select(f => (string)f.GetRawConstantValue()!);

        Assert.DoesNotContain(everything, text => text.Contains("toe te wijzen", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Every_expense_refusal_has_Dutch_wording() =>
        AssertWorded(Enum.GetValues<ExpenseRefusal>().Select(r => Tekst.Refusal(r, "Hobby")));

    [Fact]
    public void Every_income_refusal_has_Dutch_wording() =>
        AssertWorded(Enum.GetValues<IncomeRefusal>().Select(Tekst.Refusal));

    [Fact]
    public void Every_assign_refusal_has_Dutch_wording() =>
        AssertWorded(Enum.GetValues<AssignRefusal>().Select(r => Tekst.Refusal(r, "Hobby")));

    [Fact]
    public void Every_category_refusal_has_Dutch_wording() =>
        AssertWorded(Enum.GetValues<CategoryRefusal>().Select(Tekst.Refusal));

    [Fact]
    public void Every_rename_refusal_has_Dutch_wording() =>
        AssertWorded(Enum.GetValues<RenameRefusal>().Select(r => Tekst.Refusal(r, "Hobby")));

    [Theory]
    [InlineData(183245, "€ 1.832,45")]
    [InlineData(1, "€ 0,01")]
    [InlineData(0, "€ 0,00")]
    [InlineData(-2000, "−€ 20,00")]
    [InlineData(-123456789, "−€ 1.234.567,89")]
    public void Amounts_are_shown_the_Dutch_way_whatever_the_machine(long cents, string shown) =>
        Assert.Equal(shown, Tekst.Euro(Money.FromCents(cents)));

    [Fact]
    public void A_period_starting_on_the_first_is_named_by_its_month() =>
        Assert.Equal("maart 2026", Tekst.PeriodName(new BudgetPeriodCalendar().PeriodContaining(new(2026, 3, 15))));

    [Fact]
    public void A_period_that_is_not_a_calendar_month_is_named_by_its_days() =>
        Assert.Equal(
            "25 februari 2026 t/m 24 maart 2026",
            Tekst.PeriodName(new BudgetPeriodCalendar(25).PeriodContaining(new(2026, 3, 15))));

    // ----------------------------------------------------------------- reading §12

    private static Dictionary<string, string> DisplayTermsTable()
    {
        var glossary = Repository.ReadText("docs", "arc42", "12-glossary.md");
        var section = glossary[glossary.IndexOf("\n## Dutch display terms", StringComparison.Ordinal)..];
        section = section[..section.IndexOf("\n## ", 5, StringComparison.Ordinal)];

        var table = section[section.IndexOf("| English (this project) | Dutch (on screen) |", StringComparison.Ordinal)..];

        return table.Split('\n')
            .Skip(2)
            .TakeWhile(line => line.StartsWith('|'))
            .Select(line => line.Split('|', StringSplitOptions.TrimEntries))
            .ToDictionary(cells => cells[1], cells => cells[2]);
    }

    /// <summary>
    /// A cell's terms. "Vorige / volgende periode" shares its last word, so a one-word part gets
    /// the last part's final word: "Vorige periode", "volgende periode".
    /// </summary>
    private static string[] Parts(string cell)
    {
        var parts = cell.Split(" / ");
        var sharedEnd = parts[^1].Split(' ')[^1];

        return parts
            .Select((part, i) => i < parts.Length - 1 && !part.Contains(' ') && parts[^1].Contains(' ')
                ? $"{part} {sharedEnd}"
                : part)
            .ToArray();
    }

    private static void AssertWorded(IEnumerable<string> sentences)
    {
        foreach (var sentence in sentences)
        {
            Assert.False(string.IsNullOrWhiteSpace(sentence));
            Assert.DoesNotMatch(Latin(), sentence);
        }
    }

    // A crude guard against an English sentence slipping through: none of these words is Dutch.
    [GeneratedRegex(@"\b(the|must|cannot|needs|is not)\b")]
    private static partial Regex Latin();
}
