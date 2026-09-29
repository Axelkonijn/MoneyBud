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
        ["Take over (a plan)"] = [Tekst.TakeOverPlan],
        ["Remembered figure"] = [Tekst.Plan],
        ["Account / Accounts"] = [Tekst.Account, Tekst.Accounts],
        ["Balance"] = [Tekst.Balance],
        ["Net worth"] = [Tekst.NetWorth],
        ["Pool account"] = [Tekst.PoolAccount],
        ["Make (an account) the pool account"] = [Tekst.MakePool],
        ["Transfer (the record) / Transfer (the act)"] = [Tekst.Transfer, Tekst.TransferAct],
        ["A transfer's two accounts"] = [Tekst.From, Tekst.To],
        ["Starting balance"] = [Tekst.StartingBalance],
        ["Balance correction (the record)"] = [Tekst.BalanceCorrection],
        ["Correct a balance (the act)"] = [Tekst.CorrectBalance],
        ["Add account"] = [Tekst.AddAccount],
        ["Overdrawn, a negative net worth, and a negative Accumulated (the marker's badge)"] = [Tekst.Overdrawn],
        ["Backing account (the list on a category row that sets it)"] = [Tekst.BackingAccount],
        ["Accumulated"] = [Tekst.Accumulated],
        ["Period leftover"] = [Tekst.PeriodLeftover],
        ["Sweep destination (the list that sets it)"] = [Tekst.SweepDestination],
        ["Bring the swept amount up to date (the button)"] = [Tekst.BringUpToDate],
        ["Still to sweep (an ended period's line)"] = [Tekst.StillToSweep],
        ["Swept too much (an ended period's line)"] = [Tekst.SweptTooMuch],
        ["A shortfall (the marker's badge)"] = [Tekst.PeriodShortfall],
        ["Frequency (the list on the income and expense forms that sets it)"] = [Tekst.Frequency],
        ["One-off / Weekly / Monthly (the frequencies, and a latest occurrence's label)"] = [Tekst.OneOff, Tekst.Weekly, Tekst.Monthly],
        ["Period start day (the list beside the period's name that sets it)"] = [Tekst.PeriodStartDay],
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

    [Fact]
    public void Every_account_refusal_has_Dutch_wording() =>
        AssertWorded(Enum.GetValues<AccountRefusal>().Select(Tekst.Refusal)
            .Concat(Enum.GetValues<RenameRefusal>().Select(Tekst.AccountRenameRefusal)));

    [Fact]
    public void Every_transfer_refusal_has_Dutch_wording() =>
        AssertWorded(Enum.GetValues<TransferRefusal>().Select(Tekst.Refusal));

    // The ruling is that a balance correction's row shows the balance typed and the difference; a
    // starting balance's shows no difference (arc42 §12). The words around them are copy.
    [Fact]
    public void A_balance_correction_row_shows_the_balance_and_the_signed_difference()
    {
        var ledger = new Ledger(new FixedClock(new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero)), "Bank");
        var bank = ledger.PoolAccount;
        ledger.RecordIncome(1023.40m, "Salaris", new DateOnly(2026, 3, 14));
        var correction = ledger.CorrectBalance(bank, 1000m).Correction!;
        var start = ledger.AddAccount("Cash", 40m).Account!;

        var corrected = new HistoryLine(correction, bank, ledger.DifferenceOf(correction));
        Assert.Equal("Correctie — saldo € 1.000,00 (−€ 23,40)", corrected.Text);

        var starting = ledger.HistoryOf(start).OfType<BalanceCorrection>().Single();
        Assert.Equal("Startsaldo — € 40,00", new HistoryLine(starting, start, ledger.DifferenceOf(starting)).Text);
    }

    // Backing is announced naming what moved, and naming only the backing when nothing did: the
    // shape is the ruling (arc42 §12), the sentences are copy.
    [Fact]
    public void A_backing_notice_names_the_money_moved_or_only_the_backing()
    {
        var ledger = new Ledger(new FixedClock(new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero)), "Bank");
        ledger.AddCategory("Sparen");
        ledger.RecordIncome(1000m, "Salaris", new DateOnly(2026, 3, 15));
        var deposit = ledger.AddAccount("Spaarrekening", 0m).Account!;

        Assert.Equal("\"Sparen\" staat nu op \"Spaarrekening\".", Tekst.BackingSet(ledger.SetBacking("Sparen", deposit)));

        ledger.Assign(200m, "Sparen", ledger.CurrentPeriod);
        Assert.Equal(
            "\"Sparen\" staat niet meer op \"Spaarrekening\": € 200,00 teruggeboekt naar \"Bank\".",
            Tekst.BackingSet(ledger.SetBacking("Sparen", null)));
        Assert.Equal(
            "\"Sparen\" staat nu op \"Spaarrekening\": € 200,00 overgeboekt van \"Bank\".",
            Tekst.BackingSet(ledger.SetBacking("Sparen", deposit)));

        // From the pool account to itself nothing visibly moved, so no money is named.
        ledger.SetBacking("Sparen", null);
        Assert.Equal("\"Sparen\" staat nu op \"Bank\".", Tekst.BackingSet(ledger.SetBacking("Sparen", ledger.PoolAccount)));
    }

    [Fact]
    public void A_movement_row_names_its_category_and_its_two_accounts()
    {
        var ledger = new Ledger(new FixedClock(new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero)), "Bank");
        ledger.AddCategory("Sparen");
        var deposit = ledger.AddAccount("Spaarrekening", 0m).Account!;
        ledger.SetBacking("Sparen", deposit);
        ledger.Assign(200m, "Sparen", ledger.CurrentPeriod);

        var line = new HistoryLine(ledger.HistoryOf(deposit).OfType<Movement>().Single(), deposit, null);

        Assert.Equal("Toegewezen aan \"Sparen\" — van \"Bank\" naar \"Spaarrekening\"", line.Text);
        Assert.Equal("€ 200,00", line.AmountText);
    }

    [Fact]
    public void Accumulated_is_shown_as_Opgebouwd_with_its_figure() =>
        Assert.Equal("Opgebouwd: € 600,00", Tekst.AccumulatedFigure(Money.FromCents(60000)));

    [Theory]
    [InlineData(7660, "+€ 76,60")]
    [InlineData(0, "+€ 0,00")]
    [InlineData(-2340, "−€ 23,40")]
    public void A_difference_always_shows_its_sign(long cents, string shown) =>
        Assert.Equal(shown, Tekst.Signed(Money.FromCents(cents)));

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

    // §12, How a period is named: ruling 3, follow-up 3, and the short month names of the scenario
    // stage. Each day with its month, leaving out what the end repeats of the start.
    [Theory]
    [InlineData("2026-09-27", "2026-10-26", "27 sep – 26 okt 2026")]
    [InlineData("2026-09-01", "2026-09-26", "1 – 26 sep 2026")]
    [InlineData("2026-09-27", "2026-09-27", "27 sep 2026")]
    [InlineData("2026-12-27", "2027-01-26", "27 dec 2026 – 26 jan 2027")]
    [InlineData("2026-02-25", "2026-03-24", "25 feb – 24 mrt 2026")]
    [InlineData("2026-09-01", "2026-09-30", "september 2026")]
    [InlineData("2027-02-01", "2027-02-27", "1 – 27 feb 2027")]
    [InlineData("2028-02-01", "2028-02-29", "februari 2028")]
    public void A_period_is_named_by_its_month_when_it_is_one_and_otherwise_by_its_days(string first, string last, string name) =>
        Assert.Equal(name, Tekst.PeriodName(new BudgetPeriod(DateOnly.Parse(first, System.Globalization.CultureInfo.InvariantCulture),
                                                             DateOnly.Parse(last, System.Globalization.CultureInfo.InvariantCulture))));

    [Fact]
    public void Every_month_has_its_own_short_name_and_none_has_a_dot()
    {
        var names = Enumerable.Range(1, 12)
            .Select(m => Tekst.PeriodName(new BudgetPeriod(new(2026, m, 2), new(2026, m, 3))))
            .Select(n => n.Split(' ')[3])
            .ToList();

        Assert.Equal(["jan", "feb", "mrt", "apr", "mei", "jun", "jul", "aug", "sep", "okt", "nov", "dec"], names);
    }

    // The question and the notice name the day as Dutch writes it, and the periods by their names
    // (plan for increment 13, readings 1 and 2). Copy, so only its parts are held.
    [Fact]
    public void The_start_day_question_names_the_new_current_period_and_the_one_it_ends()
    {
        var current = new BudgetPeriod(new(2026, 9, 27), new(2026, 10, 26));
        var ended = new BudgetPeriod(new(2026, 9, 1), new(2026, 9, 26));

        var asked = Tekst.AskToChangeStartDay(new ChangeStartDayResult(27, true, current, ended));
        var said = Tekst.StartDayChanged(new ChangeStartDayResult(27, true, current, ended));

        Assert.Equal("Perioden laten beginnen op de 27e? De huidige periode wordt dan 27 sep – 26 okt 2026. 1 – 26 sep 2026 is dan afgelopen.", asked);
        Assert.DoesNotContain("afgelopen", Tekst.AskToChangeStartDay(new ChangeStartDayResult(30, true, ended, null)));
        Assert.Equal("Perioden beginnen nu op de 27e. De huidige periode is 27 sep – 26 okt 2026.", said);
    }

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
