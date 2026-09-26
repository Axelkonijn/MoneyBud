using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyBud.Domain;

namespace MoneyBud.Presentation;

/// <summary>
/// What MoneyBud said after the last thing the user did. <see cref="WentInto"/> is set when the
/// entry landed in a period other than the one on screen, and names that period.
/// </summary>
public sealed record Notice(string Text, bool IsRefusal, BudgetPeriod? WentInto = null);

/// <summary>
/// A question MoneyBud is waiting on an answer to. There is only ever one kind — whether to remove
/// an entry — because removing is the one act that asks before it acts (arc42 §12, *Removing an
/// entry asks first*). It is shown where a <see cref="Notice"/> would be, in its place.
/// </summary>
public sealed record Question(string Text);

/// <summary>
/// The whole screen, without a toolkit: the period on screen and stepping between periods, the
/// Overview of that period, the category suggestions, the acts the user can take, and what
/// MoneyBud tells them afterwards.
///
/// <para>Every act comes through here, and each is a thin call on the <see cref="Ledger"/>. What
/// this adds is only what the screen decides (arc42 §12, *The user interface*):</para>
/// <list type="bullet">
/// <item>A date left out means today, whatever period is shown. A period left out for assigning
/// means the period shown.</item>
/// <item>When an entry lands in a period other than the one on screen, the screen stays where it
/// is and says which period the entry went into.</item>
/// <item>Amounts arrive as typed text, read by <see cref="AmountInput"/>.</item>
/// <item>Removing an entry asks first, and waits on the answer (<see cref="Question"/>). Declining
/// says nothing.</item>
/// <item>An entry saved unchanged, or a category renamed to exactly its own name, goes through
/// quietly: nothing is said, and whatever was said before is gone.</item>
/// <item>Stepping to another period drops whatever was in progress — an entry being changed, a
/// category being renamed, a question waiting on an answer — because each was picked from a row
/// of the period that was on screen.</item>
/// </list>
///
/// <para>The period on screen is held as the period itself, never as "current" or an offset from
/// it. So when a new period begins while MoneyBud is open, the screen stays on the period it
/// showed, which is now past; only whether it is labelled current changes, and nothing is
/// announced (§12, *Staying open across a period boundary*).</para>
///
/// <para><b>Keeping the ledger</b> (§12, *What MoneyBud keeps*) is also decided here, because each
/// part of it is something the screen shows or does:</para>
/// <list type="bullet">
/// <item>Every act that goes through saves the whole ledger. A refusal, or an act that changed
/// nothing, does not. A save that works says nothing.</item>
/// <item>A save that fails leaves the act done and says so on the <see cref="SaveLine"/>, which is
/// not the notice's place: it stays there beside any notice and beside the question, and stepping
/// does not clear it, until a save works.</item>
/// <item>Every later act tries again, and so does <see cref="Tick"/>, once a minute. The save that
/// works says once, on the same line, that everything is saved again.</item>
/// <item><see cref="Close"/> tries once more, and asks nothing whatever comes of it.</item>
/// </list>
/// </summary>
public sealed partial class MoneyBudApp : ObservableObject
{
    private readonly ILedgerStore? store;

    /// <param name="store">Where the ledger is kept, already claimed and loaded from. Null keeps
    /// nothing, for a screen made to be looked at rather than used.</param>
    public MoneyBudApp(Ledger ledger, ILedgerStore? store = null)
    {
        this.store = store;
        Ledger = ledger;
        ShownPeriod = ledger.CurrentPeriod;
        ExpenseForm = new ExpenseForm(this);
        IncomeForm = new IncomeForm(this);
        AssignForm = new AssignForm(this);
        CategoryForm = new CategoryForm(this);
    }

    public Ledger Ledger { get; }

    public ExpenseForm ExpenseForm { get; }
    public IncomeForm IncomeForm { get; }
    public AssignForm AssignForm { get; }
    public CategoryForm CategoryForm { get; }

    public BudgetPeriod ShownPeriod { get; private set; }

    /// <summary>Read from the clock each time, so it turns false when the period ends.</summary>
    public bool ShowsCurrentPeriod => ShownPeriod == Ledger.CurrentPeriod;

    public string PeriodTitle => Tekst.PeriodName(ShownPeriod);

    public string? PeriodLabel => ShowsCurrentPeriod ? Tekst.CurrentPeriod : null;

    public PeriodOverview Overview => OverviewFor(ShownPeriod);

    /// <summary>The Overview any period would show if it were on screen.</summary>
    public PeriodOverview OverviewFor(BudgetPeriod period) => PeriodOverview.Of(Ledger, period, Renaming);

    /// <summary>
    /// The categories suggested when recording an expense or assigning: those offered for new
    /// entry, so never an archived one, in alphabetical order with case ignored (§12).
    /// </summary>
    public IReadOnlyList<string> CategorySuggestions =>
        Ledger.CategoriesOffered.Select(c => c.Name).Order(Alphabetical).ToList();

    /// <summary>
    /// Whether a suggestion stays in the list while something is typed: it contains what was
    /// typed, case ignored, with runs of whitespace counted as one as the name rule counts them
    /// (arc42 §12, *Category entry is free text with suggestions*: narrowing on "contains").
    /// Nothing typed keeps every suggestion. Ordinal, so the machine's language plays no part.
    /// </summary>
    public static bool SuggestionMatches(string? typed, string suggestion) =>
        CategoryName.Normalise(typed) is not { } search
        || OneSpace(suggestion).Contains(OneSpace(search), StringComparison.OrdinalIgnoreCase);

    /// <summary>The suggestions left once something has been typed, still alphabetical.</summary>
    public IReadOnlyList<string> SuggestionsFor(string? typed) =>
        CategorySuggestions.Where(s => SuggestionMatches(typed, s)).ToList();

    private static string OneSpace(string text) =>
        System.Text.RegularExpressions.Regex.Replace(text.Trim(), @"\s+", " ");

    // The invariant culture rather than ordinal, so that "Één keer" sorts among the E's instead
    // of after Z; still fixed, so the machine's language cannot change the order.
    private static readonly StringComparer Alphabetical =
        StringComparer.Create(System.Globalization.CultureInfo.InvariantCulture, ignoreCase: true);

    // ------------------------------------------------------------------ pointing at the ring

    private double? pointedAt;

    /// <summary>
    /// Points at the ring on screen, at a share of it read clockwise from the top, or at nothing
    /// (arc42 §12, <i>Hovering a slice shows its figures</i>). The Desktop only turns where the
    /// pointer is into that share.
    /// </summary>
    public void PointAt(double? share)
    {
        pointedAt = share;
        foreach (var name in PointingNames) OnPropertyChanged(name);
    }

    /// <summary>
    /// The slice pointed at in the ring on screen, or null. Worked out afresh from the Overview,
    /// so it never shows a figure that has since changed.
    /// </summary>
    public RingSlice? PointedSlice => pointedAt is { } share ? Overview.Ring.SliceAt(share) : null;

    /// <summary>
    /// The row of the category slice pointed at, which the ring's centre shows: everything the
    /// row shows. Null when nothing is pointed at, or the <i>Unassigned</i> slice is.
    /// </summary>
    public CategoryRow? PointedRow => PointedSlice?.Row;

    /// <summary>
    /// Whether the ring's centre shows <i>Niet toegewezen</i> and its figure: while no category
    /// slice is pointed at, which is also what pointing at the <i>Unassigned</i> slice shows. An
    /// empty ring shows its hint there instead.
    /// </summary>
    public bool RingCentreShowsUnassigned => PointedRow is null && !Overview.Ring.IsEmpty;

    private static readonly string[] PointingNames =
        [nameof(PointedSlice), nameof(PointedRow), nameof(RingCentreShowsUnassigned)];

    [ObservableProperty]
    public partial Notice? Notice { get; private set; }

    /// <summary>The question waiting on an answer, shown in the notice's place. Null when none is.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAsking))]
    public partial Question? Question { get; private set; }

    public bool IsAsking => Question is not null;

    private Action? onConfirm;

    // ------------------------------------------------------------------ stepping

    [RelayCommand]
    public void StepBack() => Show(Ledger.Calendar.Previous(ShownPeriod));

    [RelayCommand]
    public void StepForward() => Show(Ledger.Calendar.Next(ShownPeriod));

    private void Show(BudgetPeriod period)
    {
        ShownPeriod = period;
        AssignForm.Period = period;
        // Only an entry being changed was picked from this period's rows. A new entry being typed
        // belongs to no period until it is recorded, and stays.
        if (ExpenseForm.IsEditing) ExpenseForm.Clear();
        if (IncomeForm.IsEditing) IncomeForm.Clear();
        Renaming = null;
        DropQuestion();
        Notice = null;
        savedAgain = false;
        pointedAt = null;
        Refresh();
    }

    /// <summary>
    /// Tells whatever is bound to this that every figure may have changed. Called after each act,
    /// and by <see cref="Tick"/>, so the current-period label moves when a period ends. Changes
    /// nothing itself.
    /// </summary>
    public void Refresh()
    {
        // An empty name means "everything" to most listeners; the named ones are for any that
        // only listen by name.
        OnPropertyChanged(string.Empty);
        foreach (var name in (string[])[nameof(ShownPeriod), nameof(ShowsCurrentPeriod), nameof(PeriodTitle),
                                        nameof(PeriodLabel), nameof(Overview), nameof(CategorySuggestions),
                                        nameof(IsUnsaved), nameof(SaveLine), ..PointingNames])
            OnPropertyChanged(name);
    }

    // ------------------------------------------------------------------ keeping

    private bool savedAgain;

    /// <summary>
    /// Whether a change has not been kept: the last save failed, and none has worked since (§12,
    /// *When a save fails, MoneyBud says so and keeps going*).
    /// </summary>
    public bool IsUnsaved { get; private set; }

    /// <summary>
    /// What the line for saving says: that changes are not saved, for as long as that is true; that
    /// everything is saved again, from the save that ends it until the next thing done; otherwise
    /// nothing. A line of its own, so that it shows beside a notice and beside the question alike.
    /// </summary>
    public string? SaveLine => IsUnsaved ? Tekst.NotSaved : savedAgain ? Tekst.SavedAgain : null;

    /// <summary>
    /// Once a minute, from the Desktop's timer: looks again, so the current-period label moves when
    /// a period ends, and tries again to save when a save has failed — so that once saving works
    /// again, the changes are kept with nothing done (§12).
    /// </summary>
    public void Tick()
    {
        if (IsUnsaved) Keep();
        Refresh();
    }

    /// <summary>
    /// MoneyBud closing. Changes not yet kept get one last try; whatever comes of it, nothing is
    /// asked and MoneyBud closes (§12, *Closing makes one last attempt*). Lets go of the store, so
    /// nothing may be done here afterwards.
    /// </summary>
    public void Close()
    {
        if (IsUnsaved) store?.TrySave(Ledger.ToSnapshot());
        store?.Dispose();
    }

    /// <summary>Saves the whole ledger, and notes whether that worked for the line to say.</summary>
    private void Keep()
    {
        if (store is null) return;

        var kept = store.TrySave(Ledger.ToSnapshot());
        savedAgain = kept && IsUnsaved;
        IsUnsaved = !kept;
    }

    // ------------------------------------------------------------------ acts

    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public RecordExpenseResult? RecordExpense(string? amount, string? category, string? label, DateOnly? date = null)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        var result = Ledger.RecordExpense(euros, category, date ?? Ledger.Today, label);

        if (result.Expense is { } expense)
            Tell(Tekst.ExpenseRecorded(expense, result.CategoryBroughtBack), PeriodOf(expense.Date));
        else
            Refuse(Tekst.Refusal(result.Refusal!.Value, category));

        return result;
    }

    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public RecordIncomeResult? RecordIncome(string? amount, string? label, DateOnly? date = null)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        var result = Ledger.RecordIncome(euros, label, date ?? Ledger.Today);

        if (result.Income is { } income)
            Tell(Tekst.IncomeRecorded(income), PeriodOf(income.Date));
        else
            Refuse(Tekst.Refusal(result.Refusal!.Value));

        return result;
    }

    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public AssignResult? Assign(string? amount, string? category, BudgetPeriod? period = null)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        var target = period ?? ShownPeriod;
        var result = Ledger.Assign(euros, category, target);

        if (result.WasAssigned)
        {
            // Zero moves nothing, and nor does a negative amount clipped in full against a Budget
            // of zero: the act goes through and is said, but there is nothing to keep.
            var assigned = Money.FromEuros(euros);
            var moved = assigned != Money.Zero && result.Shortfall != -assigned;
            Tell(Tekst.Assigned(assigned, result), target, changed: moved);
        }
        else
            Refuse(Tekst.Refusal(result.Refusal!.Value, category));

        return result;
    }

    public AddCategoryResult AddCategory(string? name)
    {
        var result = Ledger.AddCategory(name);

        if (result.WasRefused)
            Refuse(Tekst.CategoryAdded(result));
        else
            Tell(Tekst.CategoryAdded(result), landedIn: null,
                 changed: result.Outcome != AddCategoryOutcome.AlreadyThere);

        return result;
    }

    /// <summary>
    /// Archives a category in use. Offered only on the row of one, since archiving a name you do
    /// not have, or archiving twice, are not things a user can do (§12) — the ledger throws for
    /// both.
    /// </summary>
    public Category ArchiveCategory(string name)
    {
        var category = Ledger.ArchiveCategory(name);
        Tell(Tekst.CategoryArchived(category), landedIn: null);
        return category;
    }

    /// <summary>The archive button on a row of a category in use.</summary>
    [RelayCommand]
    private void Archive(string name) => ArchiveCategory(name);

    // ------------------------------------------------------------------ correcting an entry

    /// <summary>
    /// Clicking an expense's row: loads it into the expense form to be changed or removed. A
    /// question about another entry is dropped, since the user has moved on from it; clicking
    /// another row while one is loaded simply loads that one.
    /// </summary>
    [RelayCommand]
    public void EditExpense(ExpenseLine line)
    {
        DropQuestion();
        ExpenseForm.Load(line.Entry);
    }

    /// <summary>Clicking an income's row, as <see cref="EditExpense"/>.</summary>
    [RelayCommand]
    public void EditIncome(IncomeLine line)
    {
        DropQuestion();
        IncomeForm.Load(line.Entry);
    }

    /// <summary>
    /// Changes an expense, judged as recording it now would be (arc42 §12). A change that goes
    /// through is announced, and says which period the expense went into when that is not the
    /// period on screen; one that changes nothing is quiet.
    /// </summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeExpenseResult? ChangeExpense(
        Expense expense, string? amount, string? category, string? label, DateOnly? date = null)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        var result = Ledger.ChangeExpense(expense, euros, category, date ?? Ledger.Today, label);

        switch (result.Outcome)
        {
            case ChangeOutcome.Changed:
                Tell(Tekst.ExpenseChanged(result.Expense!, result.CategoryBroughtBack), PeriodOf(result.Expense!.Date));
                break;
            case ChangeOutcome.Unchanged:
                SayNothing();
                break;
            default:
                Refuse(Tekst.Refusal(result.Refusal!.Value, category));
                break;
        }

        return result;
    }

    /// <summary>Changes an income, as <see cref="ChangeExpense"/> changes an expense.</summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeIncomeResult? ChangeIncome(Income income, string? amount, string? label, DateOnly? date = null)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        var result = Ledger.ChangeIncome(income, euros, label, date ?? Ledger.Today);

        switch (result.Outcome)
        {
            case ChangeOutcome.Changed:
                Tell(Tekst.IncomeChanged(result.Income!), PeriodOf(result.Income!.Date));
                break;
            case ChangeOutcome.Unchanged:
                SayNothing();
                break;
            default:
                Refuse(Tekst.Refusal(result.Refusal!.Value));
                break;
        }

        return result;
    }

    /// <summary>
    /// Asks whether to remove an expense, and removes nothing until <see cref="Confirm"/>. The
    /// question replaces whatever was said before; the only thing asked is the act, never the
    /// state of the money (§12, *Being asked is not being warned*).
    /// </summary>
    public void AskToRemove(Expense expense) => Ask(Tekst.AskToRemove(expense), () =>
    {
        Ledger.RemoveExpense(expense);
        if (ExpenseForm.Editing?.Id == expense.Id) ExpenseForm.Clear();
        Tell(Tekst.ExpenseRemoved(expense), landedIn: null);
    });

    /// <summary>Asks whether to remove an income, as <see cref="AskToRemove(Expense)"/>.</summary>
    public void AskToRemove(Income income) => Ask(Tekst.AskToRemove(income), () =>
    {
        Ledger.RemoveIncome(income);
        if (IncomeForm.Editing?.Id == income.Id) IncomeForm.Clear();
        Tell(Tekst.IncomeRemoved(income), landedIn: null);
    });

    /// <summary>Yes to the question: the act it asked about is done, and said.</summary>
    [RelayCommand]
    public void Confirm()
    {
        var act = onConfirm ?? throw new InvalidOperationException("Nothing is waiting to be confirmed.");
        DropQuestion();
        act();
    }

    /// <summary>
    /// No to the question, or <i>Annuleren</i> on a form: the entry stays, and nothing is said —
    /// declining is choosing not to act, so there is no outcome to tell (§12). Does nothing when
    /// no question is waiting.
    /// </summary>
    [RelayCommand]
    public void Decline()
    {
        if (Question is null) return;

        SayNothing();
    }

    private void Ask(string text, Action act)
    {
        Notice = null;
        savedAgain = false;
        Question = new Question(text);
        onConfirm = act;
        Refresh();
    }

    private void DropQuestion()
    {
        Question = null;
        onConfirm = null;
    }

    // ------------------------------------------------------------------ renaming and deleting

    /// <summary>
    /// The name of the category being renamed, whose row shows a text box in place of its name.
    /// Null when none is.
    /// </summary>
    [ObservableProperty]
    public partial string? Renaming { get; private set; }

    /// <summary>What the rename box holds. Starts as the name the category already has.</summary>
    [ObservableProperty]
    public partial string? NewName { get; set; }

    /// <summary>The rename button on a category's row: its name becomes a text box.</summary>
    [RelayCommand]
    public void StartRename(string name)
    {
        Renaming = name;
        NewName = name;
        Refresh();
    }

    [RelayCommand]
    public void CancelRename()
    {
        Renaming = null;
        NewName = null;
        Refresh();
    }

    /// <summary>
    /// Renames the category being renamed to what the box holds (arc42 §12, *Renaming a
    /// category*). A rename is announced from what to what; the name spelled exactly as it was
    /// is quiet. Refused, the box keeps what was typed.
    ///
    /// <para>A form that names the category by its old name is made to name it by its new one.
    /// The old name is free once the category is renamed, so otherwise an entry loaded before the
    /// rename and saved unchanged afterwards would be refused, or would land on whatever category
    /// took the old name since — and saving an unchanged entry is never refused (§12).</para>
    /// </summary>
    public RenameCategoryResult SaveRename()
    {
        var name = Renaming ?? throw new InvalidOperationException("No category is being renamed.");
        var result = Ledger.RenameCategory(name, NewName);

        if (result.WasRefused)
        {
            Refuse(Tekst.Refusal(result.Refusal!.Value, NewName));
            return result;
        }

        Renaming = null;
        NewName = null;

        if (result.Outcome == RenameOutcome.Unchanged)
        {
            SayNothing();
            return result;
        }

        var renamed = result.Category!;
        if (CategoryName.Comparer.Equals(ExpenseForm.Category, result.OldName)) ExpenseForm.Category = renamed.Name;
        if (CategoryName.Comparer.Equals(AssignForm.Category, result.OldName)) AssignForm.Category = renamed.Name;

        Tell(Tekst.CategoryRenamed(result.OldName!, renamed), landedIn: null);
        return result;
    }

    [RelayCommand]
    private void Rename() => SaveRename();

    /// <summary>
    /// Deletes a category with no history anywhere, without asking, and says so afterwards
    /// (arc42 §12). Offered only on the row of such a category (<see cref="CategoryRow.CanDelete"/>);
    /// the ledger throws for any other.
    /// </summary>
    public Category DeleteCategory(string name)
    {
        var category = Ledger.DeleteCategory(name);
        Tell(Tekst.CategoryDeleted(category), landedIn: null);
        return category;
    }

    /// <summary>The delete button on the row of a category with no history anywhere.</summary>
    [RelayCommand]
    private void Delete(string name) => DeleteCategory(name);

    // ------------------------------------------------------------------ telling

    private BudgetPeriod PeriodOf(DateOnly date) => Ledger.Calendar.PeriodContaining(date);

    // Whatever MoneyBud says after an act replaces a question still waiting: the user has moved
    // on from it, and a question and a notice are never shown together. The save line is not
    // what MoneyBud says after an act, and is left to Keep.

    /// <summary>
    /// An act that went through: said, and then the ledger is kept — unless the act changed
    /// nothing, such as adding a name already there, when there is nothing to keep.
    /// </summary>
    private void Tell(string text, BudgetPeriod? landedIn, bool changed = true)
    {
        DropQuestion();
        Notice = landedIn is { } period && period != ShownPeriod
            ? new Notice($"{text} {Tekst.WentInto(period)}", IsRefusal: false, WentInto: period)
            : new Notice(text, IsRefusal: false);
        savedAgain = false;
        if (changed) Keep();
        Refresh();
    }

    private void Refuse(string text)
    {
        DropQuestion();
        Notice = new Notice(text, IsRefusal: true);
        savedAgain = false;
        Refresh();
    }

    /// <summary>
    /// An act with no outcome to tell: nothing is said, and what was said before is gone. Nothing
    /// changed, so there is nothing to keep.
    /// </summary>
    private void SayNothing()
    {
        DropQuestion();
        Notice = null;
        savedAgain = false;
        Refresh();
    }

    private void NotAnAmount(string? typed) =>
        Refuse(AmountInput.Read(typed, out _) == AmountReading.Ambiguous
            ? Tekst.AmbiguousAmount(typed!)
            : Tekst.NotAnAmount(typed));
}
