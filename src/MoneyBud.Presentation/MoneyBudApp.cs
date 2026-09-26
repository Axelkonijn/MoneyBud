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
/// </list>
///
/// <para>The period on screen is held as the period itself, never as "current" or an offset from
/// it. So when a new period begins while MoneyBud is open, the screen stays on the period it
/// showed, which is now past; only whether it is labelled current changes, and nothing is
/// announced (§12, *Staying open across a period boundary*).</para>
/// </summary>
public sealed partial class MoneyBudApp : ObservableObject
{
    public MoneyBudApp(Ledger ledger)
    {
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
    public PeriodOverview OverviewFor(BudgetPeriod period) => PeriodOverview.Of(Ledger, period);

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

    // ------------------------------------------------------------------ stepping

    [RelayCommand]
    public void StepBack() => Show(Ledger.Calendar.Previous(ShownPeriod));

    [RelayCommand]
    public void StepForward() => Show(Ledger.Calendar.Next(ShownPeriod));

    private void Show(BudgetPeriod period)
    {
        ShownPeriod = period;
        AssignForm.Period = period;
        Notice = null;
        pointedAt = null;
        Refresh();
    }

    /// <summary>
    /// Tells whatever is bound to this that every figure may have changed. Called after each act,
    /// and by the Desktop on a timer, so the current-period label moves when a period ends.
    /// Changes nothing itself.
    /// </summary>
    public void Refresh()
    {
        // An empty name means "everything" to most listeners; the named ones are for any that
        // only listen by name.
        OnPropertyChanged(string.Empty);
        foreach (var name in (string[])[nameof(ShownPeriod), nameof(ShowsCurrentPeriod), nameof(PeriodTitle),
                                        nameof(PeriodLabel), nameof(Overview), nameof(CategorySuggestions),
                                        ..PointingNames])
            OnPropertyChanged(name);
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
            Tell(Tekst.Assigned(Money.FromEuros(euros), result), target);
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
            Tell(Tekst.CategoryAdded(result), landedIn: null);

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

    // ------------------------------------------------------------------ telling

    private BudgetPeriod PeriodOf(DateOnly date) => Ledger.Calendar.PeriodContaining(date);

    private void Tell(string text, BudgetPeriod? landedIn)
    {
        Notice = landedIn is { } period && period != ShownPeriod
            ? new Notice($"{text} {Tekst.WentInto(period)}", IsRefusal: false, WentInto: period)
            : new Notice(text, IsRefusal: false);
        Refresh();
    }

    private void Refuse(string text)
    {
        Notice = new Notice(text, IsRefusal: true);
        Refresh();
    }

    private void NotAnAmount(string? typed) =>
        Refuse(AmountInput.Read(typed, out _) == AmountReading.Ambiguous
            ? Tekst.AmbiguousAmount(typed!)
            : Tekst.NotAnAmount(typed));
}
