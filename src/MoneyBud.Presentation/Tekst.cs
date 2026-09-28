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
    public const string TakeOverPlan = "Plan overnemen";
    public const string Plan = "plan";
    public const string Account = "Rekening";
    public const string Accounts = "Rekeningen";
    public const string Balance = "Saldo";
    public const string NetWorth = "Vermogen";
    public const string PoolAccount = "Hoofdrekening";
    public const string MakePool = "Maak hoofdrekening";
    public const string Transfer = "Overboeking";
    public const string TransferAct = "Overboeken";
    public const string From = "Van";
    public const string To = "Naar";
    public const string StartingBalance = "Startsaldo";
    public const string BalanceCorrection = "Correctie";
    public const string CorrectBalance = "Saldo corrigeren";
    public const string AddAccount = "Rekening toevoegen";
    public const string Overdrawn = "Rood";
    public const string BackingAccount = "Staat op";
    public const string Accumulated = "Opgebouwd";
    public const string PeriodLeftover = "Restant";
    public const string SweepDestination = "Restant naar";
    public const string BringUpToDate = "Restant bijwerken";
    public const string StillToSweep = "nog niet weggezet";
    public const string SweptTooMuch = "te veel weggezet";
    public const string PeriodShortfall = "Tekort";
    public const string Frequency = "Herhalen";
    public const string OneOff = "Eenmalig";
    public const string Weekly = "Wekelijks";
    public const string Monthly = "Maandelijks";

    // One Dutch word for two English terms, chosen rather than fallen into (§12): removing acts on
    // an entry and deleting on a category, so the word is never ambiguous where it is shown.
    public const string Delete = "Verwijderen";

    // The form's controls and the one question MoneyBud asks. Not terms of the model, so not in
    // the table (§12, *Dutch display terms*).
    public const string Save = "Opslaan";
    public const string Cancel = "Annuleren";
    public const string Close = "Sluiten";
    public const string AreYouSure = "Weet je het zeker?";

    // The Staat op list's choice for no backing: a symbol, not a term (§12, *Proposed display terms
    // for backing*).
    public const string NoBacking = "—";

    // The Restant naar list's choice for no destination: the same symbol, as ruled (§12, *The
    // destination is one list*, follow-up).
    public const string NoSweepDestination = NoBacking;

    // Words the table does not fix, used as headings and hints.
    public const string Expenses = "Uitgaven";
    public const string Incomes = "Inkomsten";
    public const string Today = "Vandaag";
    public const string EmptyRing = "Nog geen inkomsten in deze periode";
    public const string NoExpenses = "Geen uitgaven in deze periode";
    public const string NoIncomes = "Geen inkomsten in deze periode";

    // ------------------------------------------------------------------ keeping (§12, *What MoneyBud keeps*)

    // The line for saving. A save that works says nothing; only one that fails, and the one that
    // ends a failure, are said.
    public const string NotSaved = "Je wijzigingen zijn niet opgeslagen. MoneyBud probeert het opnieuw.";
    public const string SavedAgain = "Alles is weer opgeslagen.";

    // Said when MoneyBud does not start. The first names no place and points nowhere (§12): it says
    // the data cannot be opened, the reasons that can be, and that it was not changed. MoneyBud
    // cannot tell those reasons apart, so it names them all. Reworded on 2026-09-27 after the
    // stakeholder found the first wording confusing.
    public const string CannotRead =
        "MoneyBud kan je opgeslagen gegevens niet openen. Het bestand is beschadigd, niet bereikbaar of gemaakt door een andere versie van MoneyBud. MoneyBud heeft het niet gewijzigd.";
    public const string AlreadyOpen = "MoneyBud is al geopend.";
    public const string Ok = "OK";

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
        return $"{Quoted(typed.Trim())} is dubbelzinnig: bedoel je {asThousands} of {asDecimal}?";
    }

    public static string NotAnAmount(string? typed) =>
        string.IsNullOrWhiteSpace(typed)
            ? "Vul een bedrag in."
            : $"{Quoted(typed.Trim())} is geen bedrag. Gebruik bijvoorbeeld 12,50.";

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
        RenameRefusal.NameTaken => $"{Quoted(newName?.Trim() ?? "")} is al de naam van een andere categorie.",
    };

    public static string Refusal(AccountRefusal refusal) => refusal switch
    {
        AccountRefusal.NameMissing => AccountNameMissing,
        AccountRefusal.NameTaken => AccountNameTaken,
        AccountRefusal.AmountFinerThanCent => FinerThanCent,
    };

    /// <summary>Renaming an account is refused for the reasons renaming a category is, worded for an account.</summary>
    public static string AccountRenameRefusal(RenameRefusal refusal) => refusal switch
    {
        RenameRefusal.NameMissing => AccountNameMissing,
        RenameRefusal.NameTaken => AccountNameTaken,
    };

    public static string Refusal(TransferRefusal refusal) => refusal switch
    {
        TransferRefusal.SameAccount => "Een overboeking heeft twee verschillende rekeningen nodig.",
        TransferRefusal.AmountNotPositive => "Een overboeking moet meer dan € 0,00 zijn.",
        TransferRefusal.AmountFinerThanCent => FinerThanCent,
        TransferRefusal.DateInFuture => "Een overboeking kan niet in de toekomst liggen.",
    };

    /// <summary>The one refusal a balance correction has.</summary>
    public const string BalanceFinerThanCent = FinerThanCent;

    private const string AccountNameMissing = "Een rekening heeft een naam nodig.";
    private const string AccountNameTaken = "Er is al een rekening met die naam.";

    private const string NameMissing = "Een categorie heeft een naam nodig.";

    private const string FinerThanCent = "Een bedrag kan niet kleiner zijn dan een cent.";

    private static string NotACategory(string? category) =>
        $"{Quoted(category?.Trim() ?? "")} is geen van je categorieën.";

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
    //
    // On the latest occurrence of a running repeat it adds that the repeat goes on, since removing
    // could be taken for stopping it (plan for increment 12, 9). That is copy.
    public static string AskToRemove(Domain.Expense expense, bool repeatGoesOn = false) =>
        $"{Describe(expense)} {Remove.ToLowerInvariant()}? {AreYouSure}" + (repeatGoesOn ? " " + RepeatGoesOn : "");

    public static string AskToRemove(Domain.Income income, bool repeatGoesOn = false) =>
        $"{Describe(income)} {Remove.ToLowerInvariant()}? {AreYouSure}" + (repeatGoesOn ? " " + RepeatGoesOn : "");

    private static readonly string RepeatGoesOn = $"De herhaling gaat door; zet hem op {OneOff} om te stoppen.";

    public static string ExpenseRemoved(Domain.Expense expense) => $"{Describe(expense)} verwijderd.";

    public static string IncomeRemoved(Domain.Income income) => $"{Describe(income)} verwijderd.";

    private static string Describe(Domain.Expense expense) =>
        $"{Expense} van {Euro(expense.Amount)} voor {Quoted(expense.Category.Name)}";

    private static string Describe(Domain.Income income) =>
        $"{Income} {Quoted(income.Label)} van {Euro(income.Amount)}";

    // ------------------------------------------------------------------ opening a period

    /// <summary>
    /// The button that takes a plan over: the term <see cref="TakeOverPlan"/> with the period the
    /// plan comes from between its words, and the total after — "Plan van augustus 2026 overnemen
    /// (€ 1.450,00)". The sentence around the term is copy (§12, *Dutch display terms*).
    /// </summary>
    public static string TakeOverPlanButton(PlanOffer offer)
    {
        // Built from the term itself, so that the words the glossary's table fixes are the words
        // on the button.
        var term = TakeOverPlan.Split(' ', 2);
        return $"{term[0]} van {PeriodName(offer.From)} {term[1]} ({Euro(offer.Total)})";
    }

    /// <summary>The grey figure on a category row while a plan is offered: "plan: € 400,00".</summary>
    public static string PlanFigure(Money figure) => $"{Plan}: {Euro(figure)}";

    /// <summary>
    /// Said once a plan is taken over. It always names the period the plan went into, even when
    /// that is the period on screen: a plan taken over in the wrong period shows by that name
    /// (§12, *Taking the plan over assigns it in full*).
    /// </summary>
    public static string PlanTakenOver(PlanOffer plan, BudgetPeriod into) =>
        $"Plan van {PeriodName(plan.From)} overgenomen in {PeriodName(into)}: {Euro(plan.Total)} toegewezen.";

    // ------------------------------------------------------------------ accounts

    public static string AccountAdded(Domain.Account account) => $"{Account} {Quoted(account.Name)} toegevoegd.";

    public static string AccountRenamed(string oldName, Domain.Account account) =>
        $"{Quoted(oldName)} hernoemd naar {Quoted(account.Name)}.";

    public static string AccountDeleted(Domain.Account account) => $"{Account} {Quoted(account.Name)} is verwijderd.";

    public static string PoolChanged(Domain.Account account) =>
        $"{Quoted(account.Name)} is nu de {PoolAccount.ToLowerInvariant()}.";

    public static string TransferRecorded(Domain.Transfer transfer) => $"{Describe(transfer)} toegevoegd.";

    public static string TransferChanged(Domain.Transfer transfer) => $"{Transfer} gewijzigd: {Describe(transfer)}.";

    public static string TransferRemoved(Domain.Transfer transfer) => $"{Describe(transfer)} verwijderd.";

    public static string AskToRemove(Domain.Transfer transfer) =>
        $"{Describe(transfer)} {Remove.ToLowerInvariant()}? {AreYouSure}";

    public static string BalanceCorrected(Domain.BalanceCorrection correction) =>
        $"{Balance} van {Quoted(correction.Account.Name)} gecorrigeerd naar {Euro(correction.Balance)}.";

    public static string AskToRemove(Domain.BalanceCorrection correction) =>
        $"{Describe(correction)} {Remove.ToLowerInvariant()}? {AreYouSure}";

    public static string BalanceCorrectionRemoved(Domain.BalanceCorrection correction) =>
        $"{Describe(correction)} verwijderd.";

    /// <summary>
    /// What a history row says it is: "Correctie — saldo € 1.000,00 (−€ 23,40)" for a balance
    /// correction, "Startsaldo — € 1.000,00" for a starting balance, the two accounts for a transfer,
    /// and the category and label for an expense or income. The wording is copy; the difference is
    /// the ruling (§12, <i>Accounts and net worth</i>).
    /// </summary>
    public static string HistoryText(HistoryLine line) => line.Entry switch
    {
        Domain.BalanceCorrection { IsStartingBalance: true } c => $"{StartingBalance} — {Euro(c.Balance)}",
        Domain.BalanceCorrection c =>
            $"{BalanceCorrection} — {Balance.ToLowerInvariant()} {Euro(c.Balance)} ({Signed(line.Difference ?? Money.Zero)})",
        Domain.Transfer t => $"{Transfer} {From.ToLowerInvariant()} {Quoted(t.From.Name)} {To.ToLowerInvariant()} {Quoted(t.To.Name)}",
        Domain.Income i => $"{Income} {Quoted(i.Label)}",
        Domain.Expense e => $"{Expense} {Quoted(e.Category.Name)}" + (e.Label is { } label ? $", {label}" : ""),
        Domain.Movement m => MovementText(m),
        _ => throw new InvalidOperationException($"{line.Entry.GetType().Name} is not in a history."),
    };

    /// <summary>An amount with its sign always shown, as a difference is: "+€ 76,60", "−€ 23,40".</summary>
    public static string Signed(Money amount) => amount.IsNegative ? Euro(amount) : $"+{Euro(amount)}";

    private static string Describe(Domain.Transfer transfer) =>
        $"{Transfer} van {Euro(transfer.Amount)} van {Quoted(transfer.From.Name)} naar {Quoted(transfer.To.Name)}";

    private static string Describe(Domain.BalanceCorrection correction) =>
        correction.IsStartingBalance
            ? $"{StartingBalance} van {Quoted(correction.Account.Name)}"
            : $"{BalanceCorrection} van {Quoted(correction.Account.Name)} naar {Euro(correction.Balance)}";

    // ------------------------------------------------------------------ backing (§12, *Backing and Accumulated*)

    /// <summary>What a backed category's row shows under its figures: "Opgebouwd: € 600,00".</summary>
    public static string AccumulatedFigure(Money accumulated) => $"{Accumulated}: {Euro(accumulated)}";

    /// <summary>
    /// Said once a backing is set, pointed elsewhere or removed. It names the money that moved, and
    /// when none moved names only the backing, saying nothing about money (§12, <i>Backing,
    /// re-pointing and unbacking are announced, never confirmed</i>). The sentences are copy; the
    /// shape is the ruling. Money moved from the pool account to itself moved nothing anyone can
    /// see, so it is not named either.
    /// </summary>
    public static string BackingSet(SetBackingResult result)
    {
        var name = Quoted(result.Category.Name);
        var moved = result.MovedMoney ? result.Moved : null;

        return result.Outcome switch
        {
            BackingOutcome.Backed or BackingOutcome.Repointed => moved is null
                ? $"{name} staat nu op {Quoted(result.After!.Name)}."
                : $"{name} staat nu op {Quoted(result.After!.Name)}: {Euro(moved.Amount)} overgeboekt van {Quoted(moved.From.Name)}.",
            BackingOutcome.Unbacked => moved is null
                ? $"{name} staat niet meer op {Quoted(result.Before!.Name)}."
                : $"{name} staat niet meer op {Quoted(result.Before!.Name)}: {Euro(moved.Amount)} teruggeboekt naar {Quoted(moved.To.Name)}.",
            BackingOutcome.Unchanged => throw new InvalidOperationException("Nothing changed, so nothing is said."),
        };
    }

    /// <summary>
    /// What a movement's row in an account's history says: what caused it, the category, and the two
    /// accounts — "Toegewezen aan "Sparen" — van "Betaalrekening" naar "Spaarrekening"". The amount
    /// is the row's own column. Copy (§12, <i>Moved money in the account's history</i>).
    /// </summary>
    private static string MovementText(Domain.Movement movement)
    {
        var name = Quoted(movement.Category.Name);
        var what = (movement.Reason, movement.Direction) switch
        {
            (MovementReason.Assigned, MovementDirection.Out) => $"Teruggezet van {name}",
            (MovementReason.Assigned, _) => $"Toegewezen aan {name}",
            (MovementReason.Backed, _) => $"{name} staat op {Quoted(movement.To.Name)}",
            (MovementReason.Unbacked, _) => $"{name} staat niet meer op {Quoted(movement.From.Name)}",
            (MovementReason.Repointed, _) => $"{name} staat nu op {Quoted(movement.To.Name)}",
            (MovementReason.Swept, MovementDirection.Out) => $"{PeriodLeftover} van {PeriodName(SweptPeriod(movement))} teruggehaald van {name}",
            (MovementReason.Swept, _) => $"{PeriodLeftover} van {PeriodName(SweptPeriod(movement))} naar {name}",
        };

        return $"{what} — {From.ToLowerInvariant()} {Quoted(movement.From.Name)} {To.ToLowerInvariant()} {Quoted(movement.To.Name)}";
    }

    // ------------------------------------------------------------------ the sweep (§12, *The sweep and Restant*)

    // A sweep keeps the first day of the period it was for. The start day is fixed at the 1st for
    // now (§12, *The period start day stays at the 1st*), so the default calendar names the period.
    private static BudgetPeriod SweptPeriod(Domain.Movement sweep) =>
        new BudgetPeriodCalendar().PeriodContaining(sweep.SweptFor!.Value);

    /// <summary>
    /// The automatic sweep's notice, one sentence per period swept, oldest first: "Restant van
    /// september 2026: € 130,00 naar "Sparen"." The sentence is copy; that it is said once is the
    /// ruling (§12, <i>When the sweep runs</i>, follow-up).
    /// </summary>
    public static string Swept(IEnumerable<SweepMade> sweeps) =>
        string.Join(" ", sweeps.Select(s =>
            $"{PeriodLeftover} van {PeriodName(s.Period)}: {Euro(s.Movement.Amount)} naar {Quoted(s.Movement.Category.Name)}."));

    /// <summary>"Restant gaat voortaan naar "Sparen".", or "... nergens heen." for none (§12, ruled at the scenario stage, 6).</summary>
    public static string SweepDestinationSet(SetSweepDestinationResult result) => result.Outcome switch
    {
        SweepDestinationOutcome.Chosen => $"{PeriodLeftover} gaat voortaan naar {Quoted(result.After!.Name)}.",
        SweepDestinationOutcome.Removed => $"{PeriodLeftover} gaat voortaan nergens heen.",
        SweepDestinationOutcome.Unchanged => throw new InvalidOperationException("Nothing changed, so nothing is said."),
    };

    /// <summary>Said with whatever cleared the destination: unbacking, archiving or deleting it (§12, ruling 6).</summary>
    public static string NoLongerSweepDestination(string category) =>
        $"{PeriodLeftover} gaat niet meer naar {Quoted(category)}.";

    /// <summary>
    /// What <i>Restant bijwerken</i> moved, one sentence per move: "€ 100,00 extra restant van
    /// september 2026 naar "Sparen"." or "€ 40,00 te veel weggezet van september 2026 teruggehaald
    /// van "Sparen"." What it let go is not said again (§12, <i>A swept period that changes</i>).
    /// </summary>
    public static string BroughtUpToDate(BringUpToDateResult result)
    {
        var period = PeriodName(result.Period);
        if (result.Moves.Count == 0)
            return $"{PeriodLeftover} van {period} bijgewerkt: er stond niets meer om terug te halen.";

        return string.Join(" ", result.Moves.Select(m => m.Direction == MovementDirection.Out
            ? $"{Euro(m.Amount)} {SweptTooMuch} van {period} teruggehaald van {Quoted(m.Category.Name)}."
            : $"{Euro(m.Amount)} extra {PeriodLeftover.ToLowerInvariant()} van {period} naar {Quoted(m.Category.Name)}."));
    }

    /// <summary>
    /// An ended period's line, near <i>Niet toegewezen</i>: "Restant € 130,00 naar Sparen", with
    /// "€ 40,00 nog niet weggezet" or "€ 40,00 te veel weggezet" after it when that is the line, and
    /// for a shortfall "Restant: −€ 30,00", beside the marker and <i>Tekort</i>. The phrases are the
    /// ruled terms; the rest is copy. Names are not quoted: this is a line, not a message.
    /// </summary>
    public static string SweepLineText(SweepLine line)
    {
        var went = line.Parts.Count == 0
            ? null
            : $"{PeriodLeftover} " + string.Join(" en ", line.Parts.Select(p => $"{Euro(p.Amount)} naar {p.Category.Name}"));

        var more = line.Kind switch
        {
            SweepLineKind.StillToSweep => $"{Euro(line.Amount)} {StillToSweep}",
            SweepLineKind.SweptTooMuch => $"{Euro(line.Amount)} {SweptTooMuch}",
            SweepLineKind.Shortfall => $"{PeriodLeftover}: {Euro(line.Amount)}",
            SweepLineKind.Swept => null,
        };

        return (went, more) switch
        {
            (null, null) => throw new InvalidOperationException("A line says something."),
            (null, _) when line.Kind == SweepLineKind.Shortfall => more,
            (null, _) => $"{PeriodLeftover}: {more}",
            (_, null) => went,
            _ => $"{went} · {more}",
        };
    }

    // ------------------------------------------------------------------ recurring entries

    /// <summary>What the <i>Herhalen</i> list shows for a frequency, one-off being none (§12, ruling 1).</summary>
    public static string FrequencyName(Domain.Frequency? frequency) => frequency is { } repeats
        ? repeats switch
        {
            Domain.Frequency.Weekly => Weekly,
            Domain.Frequency.Monthly => Monthly,
        }
        : OneOff;

    /// <summary>The small grey label on a latest occurrence's row: "maandelijks", lower-case, as ruling 6 writes it.</summary>
    public static string RepeatLabel(Domain.Frequency frequency) => FrequencyName(frequency).ToLowerInvariant();

    /// <summary>
    /// The notice for what MoneyBud recorded by itself, in one sentence, in the order recorded:
    /// "Herhaald: Netflix € 13,99 (25 september), Salaris € 2.500,00 (27 september)." An expense with
    /// no label is named by its category. Then a sentence for each category an occurrence brought
    /// back. Copy; that it is one notice, said once, is the ruling (§12, rulings 7 and 8).
    /// </summary>
    public static string Repeated(IReadOnlyList<OccurrenceMade> occurrences)
    {
        var named = occurrences.Select(o => o.Entry switch
        {
            Domain.Expense e => $"{e.Label ?? e.Category.Name} {Euro(e.Amount)} ({DayOfMonth(e.Date)})",
            Domain.Income i => $"{i.Label} {Euro(i.Amount)} ({DayOfMonth(i.Date)})",
            _ => throw new InvalidOperationException("Only incomes and expenses repeat."),
        });
        var broughtBack = occurrences.Where(o => o.BroughtBack is not null).Select(o => o.BroughtBack!.Name).Distinct();

        return string.Join(" ", [$"Herhaald: {string.Join(", ", named)}.", .. broughtBack.Select(WasBroughtBack)]);
    }

    private static string DayOfMonth(DateOnly day) => $"{day.Day} {Months[day.Month - 1]}";

    /// <summary>The notice that an entry landed in a period other than the one on screen.</summary>
    public static string WentInto(BudgetPeriod period) => $"Dit staat in {PeriodName(period)}.";

    private static string WasBroughtBack(string name) => $"{Quoted(name)} is weer in gebruik.";

    // Plain double quotes around a name or what was typed. The Dutch low-high pair was used
    // first; the stakeholder found it odd on screen and asked for ordinary quotes (2026-09-27).
    private static string Quoted(string text) => $"\"{text}\"";
}
