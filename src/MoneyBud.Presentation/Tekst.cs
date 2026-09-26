using System.Globalization;
using MoneyBud.Domain;

namespace MoneyBud.Presentation;

/// <summary>
/// Everything MoneyBud says on screen, in Dutch (arc42 §12, *The UI is in Dutch*).
///
/// <para>Two kinds of text live here, and the difference matters. The <b>display terms</b> are
/// fixed: they are §12's *Dutch display terms* table, approved by the stakeholder, and a unit
/// test holds them to it. The <b>sentences</b> — refusals, confirmations, the notice that says
/// where an entry went — are copy. The domain gives reasons, never messages (§8.1), so wording
/// them is this class's job and nobody else's.</para>
///
/// <para>Every refusal switch below lists each reason and has no fallback arm, so a reason added
/// to the domain without Dutch wording is a compiler warning, and the build is kept at zero.</para>
/// </summary>
public static class Tekst
{
    // ------------------------------------------------------------ display terms (§12)

    public const string Expense = "Uitgave";
    public const string Income = "Inkomst";
    public const string Category = "Categorie";
    public const string DefaultCategories = "Standaardcategorieën";
    public const string BudgetPeriod = "Periode";
    public const string CurrentPeriod = "Huidige periode";
    public const string Budget = "Budget";
    public const string Assign = "Toewijzen";
    public const string Spent = "Uitgegeven";
    public const string Remaining = "Resterend";
    public const string OverBudget = "Over budget";
    public const string Unassigned = "Niet toegewezen";
    public const string OverAssigned = "Te veel toegewezen";
    public const string Label = "Omschrijving";
    public const string Archive = "Archiveren";
    public const string Archived = "Gearchiveerd";
    public const string BroughtBack = "Weer in gebruik";
    public const string Shortfall = "Niet teruggezet";
    public const string Amount = "Bedrag";
    public const string Date = "Datum";
    public const string PreviousPeriod = "Vorige periode";
    public const string NextPeriod = "Volgende periode";
    public const string AddCategory = "Categorie toevoegen";
    public const string RecordExpense = "Uitgave toevoegen";
    public const string RecordIncome = "Inkomst toevoegen";
    public const string Overview = "Overzicht";
    public const string Change = "Wijzigen";
    public const string Remove = "Verwijderen";
    public const string Rename = "Hernoemen";

    // One Dutch word for two English terms, chosen rather than fallen into (§12): removing acts on
    // an entry and deleting on a category, so the word is never ambiguous where it is shown.
    public const string Delete = "Verwijderen";

    // The form's controls and the one question MoneyBud asks. Not terms of the model, so not in
    // the table (§12, *Dutch display terms*).
    public const string Save = "Opslaan";
    public const string Cancel = "Annuleren";
    public const string AreYouSure = "Weet je het zeker?";

    // Words the table does not fix, used as headings and hints.
    public const string Expenses = "Uitgaven";
    public const string Incomes = "Inkomsten";
    public const string Today = "Vandaag";
    public const string EmptyRing = "Nog geen inkomsten in deze periode";
    public const string NoExpenses = "Geen uitgaven in deze periode";
    public const string NoIncomes = "Geen inkomsten in deze periode";

    // ------------------------------------------------------------------ amounts

    private static readonly NumberFormatInfo DutchNumbers = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NumberGroupSizes = [3],
    };

    /// <summary>
    /// An amount as MoneyBud shows it: "€ 1.832,45", and a negative one as "−€ 20,00" with a
    /// true minus sign, which is how §12 writes the negative figure beside the marker. Built from
    /// fixed separators rather than the machine's nl-NL data, so it reads the same everywhere.
    /// </summary>
    public static string Euro(Money amount)
    {
        var digits = Math.Abs(amount.Euros).ToString("N2", DutchNumbers);
        return amount.IsNegative ? $"−€ {digits}" : $"€ {digits}";
    }

    // ------------------------------------------------------------------ periods

    private static readonly string[] Months =
    [
        "januari", "februari", "maart", "april", "mei", "juni",
        "juli", "augustus", "september", "oktober", "november", "december",
    ];

    /// <summary>
    /// A period's name. With periods starting on the 1st that is the month, "maart 2026"; a
    /// period that does not match a calendar month is named by its first and last day.
    /// </summary>
    public static string PeriodName(BudgetPeriod period)
    {
        var first = period.FirstDay;
        var last = period.LastDay;

        if (first.Day == 1 && last == first.AddMonths(1).AddDays(-1))
            return $"{Months[first.Month - 1]} {first.Year}";

        return $"{DayName(first)} t/m {DayName(last)}";
    }

    public static string DayName(DateOnly day) => $"{day.Day} {Months[day.Month - 1]} {day.Year}";

    // ------------------------------------------------------------------ refusals

    public static string AmbiguousAmount(string typed)
    {
        var (asThousands, asDecimal) = AmountInput.Readings(typed);
        return $"„{typed.Trim()}” is dubbelzinnig: bedoel je {asThousands} of {asDecimal}?";
    }

    public static string NotAnAmount(string? typed) =>
        string.IsNullOrWhiteSpace(typed)
            ? "Vul een bedrag in."
            : $"„{typed.Trim()}” is geen bedrag. Gebruik bijvoorbeeld 12,50.";

    public static string Refusal(ExpenseRefusal refusal, string? category) => refusal switch
    {
        ExpenseRefusal.CategoryMissing => "Een uitgave heeft een categorie nodig.",
        ExpenseRefusal.UnknownCategory => NotACategory(category),
        ExpenseRefusal.AmountNotPositive => "Een uitgave moet meer dan € 0,00 zijn.",
        ExpenseRefusal.AmountFinerThanCent => FinerThanCent,
        ExpenseRefusal.DateInFuture => "Een uitgave kan niet in de toekomst liggen.",
    };

    public static string Refusal(IncomeRefusal refusal) => refusal switch
    {
        IncomeRefusal.LabelMissing => "Een inkomst heeft een omschrijving nodig.",
        IncomeRefusal.AmountNotPositive => "Een inkomst moet meer dan € 0,00 zijn.",
        IncomeRefusal.AmountFinerThanCent => FinerThanCent,
    };

    public static string Refusal(AssignRefusal refusal, string? category) => refusal switch
    {
        AssignRefusal.CategoryMissing => "Toewijzen heeft een categorie nodig.",
        AssignRefusal.UnknownCategory => NotACategory(category),
        AssignRefusal.AmountFinerThanCent => FinerThanCent,
        AssignRefusal.PeriodInPast => "In een voorbije periode kan niets meer worden toegewezen.",
    };

    public static string Refusal(CategoryRefusal refusal) => refusal switch
    {
        CategoryRefusal.NameMissing => NameMissing,
    };

    public static string Refusal(RenameRefusal refusal, string? newName) => refusal switch
    {
        RenameRefusal.NameMissing => NameMissing,
        RenameRefusal.NameTaken => $"„{newName?.Trim()}” is al de naam van een andere categorie.",
    };

    private const string NameMissing = "Een categorie heeft een naam nodig.";

    private const string FinerThanCent = "Een bedrag kan niet kleiner zijn dan een cent.";

    private static string NotACategory(string? category) =>
        $"„{category?.Trim()}” is geen van je categorieën.";

    // ------------------------------------------------------------------ outcomes

    public static string ExpenseRecorded(Expense expense, bool broughtBack) =>
        $"{Expense} van {Euro(expense.Amount)} voor {Quoted(expense.Category.Name)} toegevoegd."
        + (broughtBack ? " " + WasBroughtBack(expense.Category.Name) : "");

    public static string IncomeRecorded(Income income) =>
        $"{Income} van {Euro(income.Amount)} toegevoegd.";

    public static string Assigned(Money amount, AssignResult result)
    {
        var name = Quoted(result.Category!.Name);
        var text = amount.IsNegative
            ? $"{Euro(-amount - result.Shortfall)} teruggezet van {name} naar {Unassigned.ToLowerInvariant()}."
            : $"{Euro(amount)} toegewezen aan {name}.";

        if (result.Shortfall != Money.Zero)
            text += $" {Shortfall}: {Euro(result.Shortfall)}.";

        if (result.CategoryBroughtBack)
            text += " " + WasBroughtBack(result.Category.Name);

        return text;
    }

    public static string CategoryAdded(AddCategoryResult result) => result.Outcome switch
    {
        AddCategoryOutcome.Created => $"{Category} {Quoted(result.Category!.Name)} toegevoegd.",
        AddCategoryOutcome.AlreadyThere => $"{Quoted(result.Category!.Name)} bestond al.",
        AddCategoryOutcome.BroughtBack => WasBroughtBack(result.Category!.Name),
        null => Refusal(result.Refusal!.Value),
    };

    public static string CategoryArchived(Domain.Category category) =>
        $"{Quoted(category.Name)} is {Archived.ToLowerInvariant()}.";

    public static string CategoryRenamed(string oldName, Domain.Category category) =>
        $"{Quoted(oldName)} hernoemd naar {Quoted(category.Name)}.";

    public static string CategoryDeleted(Domain.Category category) => $"{Quoted(category.Name)} is verwijderd.";

    public static string ExpenseChanged(Domain.Expense expense, bool broughtBack) =>
        $"{Expense} gewijzigd: {Euro(expense.Amount)} voor {Quoted(expense.Category.Name)}."
        + (broughtBack ? " " + WasBroughtBack(expense.Category.Name) : "");

    public static string IncomeChanged(Domain.Income income) =>
        $"{Income} gewijzigd: {Quoted(income.Label)}, {Euro(income.Amount)}.";

    // The question names the entry, so it is clear which one goes. It says nothing about what the
    // removal does to the figures: being asked is not being warned (§12, *Removing an entry asks
    // first*).
    public static string AskToRemove(Domain.Expense expense) =>
        $"{Describe(expense)} {Remove.ToLowerInvariant()}? {AreYouSure}";

    public static string AskToRemove(Domain.Income income) =>
        $"{Describe(income)} {Remove.ToLowerInvariant()}? {AreYouSure}";

    public static string ExpenseRemoved(Domain.Expense expense) => $"{Describe(expense)} verwijderd.";

    public static string IncomeRemoved(Domain.Income income) => $"{Describe(income)} verwijderd.";

    private static string Describe(Domain.Expense expense) =>
        $"{Expense} van {Euro(expense.Amount)} voor {Quoted(expense.Category.Name)}";

    private static string Describe(Domain.Income income) =>
        $"{Income} {Quoted(income.Label)} van {Euro(income.Amount)}";

    /// <summary>The notice that an entry landed in a period other than the one on screen.</summary>
    public static string WentInto(BudgetPeriod period) => $"Dit staat in {PeriodName(period)}.";

    private static string WasBroughtBack(string name) => $"{Quoted(name)} is weer in gebruik.";

    private static string Quoted(string text) => $"„{text}”";
}
