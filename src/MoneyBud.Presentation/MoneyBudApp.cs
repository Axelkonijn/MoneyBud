using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyBud.Domain;

namespace MoneyBud.Presentation;

/// <summary>
/// What MoneyBud said after the last thing the user did. <see cref="WentInto"/> is set when the
/// entry landed in a period other than the one on screen, and names that period.
/// <see cref="Repeated"/> is every occurrence the notice names, recorded by MoneyBud itself since it
/// last said anything (arc42 §12, <i>Recurring entries</i>, ruling 7).
/// </summary>
public sealed record Notice(string Text, bool IsRefusal, BudgetPeriod? WentInto = null)
{
    public IReadOnlyList<OccurrenceMade> Repeated { get; init; } = [];
}

/// <summary>
/// A question MoneyBud is waiting on an answer to. There are two kinds, since two acts ask before
/// they act, because neither can be undone: removing something recorded — an entry, a transfer, a
/// balance correction or a starting balance (arc42 §12, *Removing an entry asks first*) — and
/// changing the period start day (§12, ruling 5). It is shown where a <see cref="Notice"/> would be,
/// in its place, and <see cref="ConfirmText"/> is the button that says yes: <i>Verwijderen</i> or
/// <i>Wijzigen</i>. The other is always <i>Annuleren</i>.
/// </summary>
public sealed record Question(string Text, string ConfirmText = Tekst.Remove);

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
/// <item>Taking a plan over always acts on the period shown, wherever the assign form's own period
/// has been moved to.</item>
/// <item>When an entry lands in a period other than the one on screen, the screen stays where it
/// is and says which period the entry went into.</item>
/// <item>Amounts arrive as typed text, read by <see cref="AmountInput"/>.</item>
/// <item>Removing an entry asks first, and waits on the answer (<see cref="Question"/>). Declining
/// says nothing. So does changing the period start day, which then moves the screen, and the assign
/// form's own period, to the period nearest to where each was (<see cref="ChooseStartDay"/>).</item>
/// <item>An entry saved unchanged, or a category renamed to exactly its own name, goes through
/// quietly: nothing is said, and whatever was said before is gone.</item>
/// <item>Stepping to another period drops whatever was picked from the period on screen — an entry
/// being changed, a category being renamed, a question waiting on an answer. What belongs to no
/// period stays: a new entry being typed, an account's history and anything done in it, a transfer
/// being recorded or changed.</item>
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
///
/// <para><b>Settling</b> (§12, <i>Backing and Accumulated</i>; ADR 0009): money planned for a period
/// that has begun moves when MoneyBud opens and on every <see cref="Tick"/>, and what moved is kept.
/// Every act that changes the ledger settles first as well, inside the ledger, and is kept with the
/// act. The one gap — an act that is then refused, straight after a period began — leaves the moved
/// money unsaved until the next change; nothing is lost, since the next start settles the kept data
/// the same way.</para>
///
/// <para><b>Sweeps</b> (§12, <i>The sweep and Restant</i>) are the exception to that gap: settling
/// may sweep an ended period's leftover wherever it runs — on opening, on a <see cref="Tick"/>, or
/// inside any act, refused ones and ones that change nothing included. So after each of those the
/// screen takes the sweeps made, says them first, one sentence per period, and keeps the ledger
/// straight away, so that each sweep is announced exactly once.</para>
///
/// <para><b>Occurrences</b> of a recurring entry (§12, <i>Recurring entries</i>; ADR 0011) are the
/// same: settling records them wherever it runs, and an act that sets up or moves a repeat records
/// what is already due. They are said in one sentence with the sweeps, and kept straight away. The
/// notice says things in the order they happened (ruled at the build, 2026-09-28): what settling did
/// before an act in front of the act's own sentence, and the occurrences the act itself caused after
/// it.</para>
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
        AccountForm = new AccountForm(this);
        TransferForm = new TransferForm(this);
        ReallocateForm = new ReallocateForm(this);

        if (Ledger.Settle()) Keep();
        if (TakeWhatSettlingDid() is { Said: { } said } settled)
            Notice = new Notice(said, IsRefusal: false) { Repeated = settled.Repeated };
    }

    public Ledger Ledger { get; }

    public ExpenseForm ExpenseForm { get; }
    public IncomeForm IncomeForm { get; }
    public AssignForm AssignForm { get; }
    public CategoryForm CategoryForm { get; }
    public AccountForm AccountForm { get; }
    public TransferForm TransferForm { get; }
    public ReallocateForm ReallocateForm { get; }

    public BudgetPeriod ShownPeriod { get; private set; }

    /// <summary>Read from the clock each time, so it turns false when the period ends.</summary>
    public bool ShowsCurrentPeriod => ShownPeriod == Ledger.CurrentPeriod;

    public string PeriodTitle => Tekst.PeriodName(ShownPeriod);

    public string? PeriodLabel => ShowsCurrentPeriod ? Tekst.CurrentPeriod : null;

    public PeriodOverview Overview => OverviewFor(ShownPeriod);

    // ------------------------------------------------------------------ the period start day

    // The start day a waiting question asks about, and the day it was asked on; null when none waits.
    private int? askedStartDay;
    private DateOnly askedOn;

    /// <summary>What the <i>Periode begint op</i> list offers: every day from 1 to 31, in order (ruling 2).</summary>
    public IReadOnlyList<int> StartDayChoices { get; } = Enumerable.Range(1, 31).ToList();

    /// <summary>
    /// What the <i>Periode begint op</i> list shows while <paramref name="period"/> is on screen, or
    /// null where it is not shown (arc42 §12, ruling 6): on the current period and later ones only,
    /// since an earlier one keeps the day it began on. It shows the day now set, the same on every
    /// period — or, while the question waits, the day being asked about (plan for increment 13,
    /// reading 3).
    /// </summary>
    public int? StartDayShownIn(BudgetPeriod period) =>
        period.FirstDay >= Ledger.CurrentPeriod.FirstDay ? askedStartDay ?? Ledger.Calendar.StartDay : null;

    /// <summary>Whether the <i>Periode begint op</i> list is shown beside the period on screen.</summary>
    public bool ShowsStartDay => StartDayShownIn(ShownPeriod) is not null;

    /// <summary>
    /// The <i>Periode begint op</i> list's choice. Choosing a day asks first (<see cref="ChooseStartDay"/>);
    /// null, which a list writes while its items are replaced, is ignored.
    /// </summary>
    public int? StartDayChoice
    {
        get => askedStartDay ?? Ledger.Calendar.StartDay;
        set
        {
            if (value is { } day) ChooseStartDay(day);
        }
    }

    /// <summary>
    /// A day chosen in the <i>Periode begint op</i> list (arc42 §12, <i>A configurable period start
    /// day</i>). A change asks first, in the message bar, since it can end the current period on the
    /// spot and cannot be undone (ruling 5); <see cref="Confirm"/> makes it, and says what changed.
    /// It is the one question: asking it drops any other waiting.
    ///
    /// <para><b>Choosing the day already set does nothing at all</b>: no question, nothing said,
    /// nothing kept, nothing redrawn. The list writes back what it shows on every redraw, as the
    /// <i>Staat op</i> lists do. While the question waits, the list shows the day asked about, and
    /// writing that back does nothing either; choosing the day already set then is declining.</para>
    /// </summary>
    public void ChooseStartDay(int day)
    {
        if (askedStartDay == day) return;

        if (day == Ledger.Calendar.StartDay)
        {
            if (askedStartDay is not null) Decline();
            return;
        }

        Ask(Tekst.AskToChangeStartDay(Ledger.PreviewStartDay(day)), () => ChangeStartDay(day),
            confirmText: Tekst.Change, startDay: day);
    }

    // Confirmed. The screen, and the assign form's own period, go to the period nearest to where each
    // was (§12, follow-up 1; ruled at the scenario stage, 3): from the current period the new current
    // period, from any other the period its first day now falls in.
    private void ChangeStartDay(int day)
    {
        SettleBeforeActing();
        var current = Ledger.CurrentPeriod;
        var shown = ShownPeriod;
        var assignIn = AssignForm.Period;

        var result = Ledger.ChangeStartDay(day);

        ShownPeriod = NearestTo(shown);
        AssignForm.Period = NearestTo(assignIn);
        pointedAt = null;
        Tell(Tekst.StartDayChanged(result), landedIn: null);

        BudgetPeriod NearestTo(BudgetPeriod period) =>
            period == current ? Ledger.CurrentPeriod : Ledger.Calendar.PeriodContaining(period.FirstDay);
    }

    /// <summary>The Overview any period would show if it were on screen.</summary>
    public PeriodOverview OverviewFor(BudgetPeriod period) =>
        PeriodOverview.Of(Ledger, period, Renaming, BackingChoices, (name, account) => SetBacking(name, account),
                          SweepChoices, name => SetSweepDestination(name));

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
    internal static readonly StringComparer Alphabetical =
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
        RefreshFormAccounts();
        foreach (var name in (string[])[nameof(ShownPeriod), nameof(ShowsCurrentPeriod), nameof(PeriodTitle),
                                        nameof(PeriodLabel), nameof(ShowsStartDay), nameof(StartDayChoice),
                                        nameof(Overview), nameof(CategorySuggestions),
                                        nameof(IsUnsaved), nameof(SaveLine), ..PointingNames])
            OnPropertyChanged(name);
    }

    // Also the Herhalen lists: an entry open in its form locks there once it is no longer the one
    // that sets its repeat (plan for increment 12, 4).
    private void RefreshFormAccounts()
    {
        ExpenseForm?.RefreshAccount();
        IncomeForm?.RefreshAccount();
        TransferForm?.RefreshAccounts();
        ReallocateForm?.RefreshChoices();
        ExpenseForm?.RefreshFrequency();
        IncomeForm?.RefreshFrequency();
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
    /// a period ends; moves the money planned for a period that has just begun, and keeps it; records
    /// the occurrences that have come due and sweeps the period that ended, and says so; and tries
    /// again to save when a save has failed — so that once saving works again, the changes are kept
    /// with nothing done (§12).
    ///
    /// <para>That notice replaces a question still waiting, since a question and a notice are never
    /// shown together: MoneyBud did something, and being told of it wins (plan for increment 11, 7;
    /// increment 12, 7). Anything else the tick does says nothing.</para>
    /// </summary>
    public void Tick()
    {
        // On the phone, in the background, nothing is done by itself: a notice said while nobody is
        // looking would never be seen, and a sweep is said only once. Coming back does it instead.
        if (inBackground) return;

        Look();
    }

    private void Look()
    {
        // A start-day question names the current period it would give, which a new day can change,
        // so it is dropped and the list put back (plan for increment 13, reading 4).
        if (askedStartDay is not null && Ledger.Today != askedOn) DropQuestion();

        var changed = Ledger.Settle();
        if (TakeWhatSettlingDid() is { Said: { } said } settled)
        {
            DropQuestion();
            Notice = new Notice(said, IsRefusal: false) { Repeated = settled.Repeated };
            savedAgain = false;
        }

        if (changed | IsUnsaved) Keep();
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

    // On the phone, between going to the background and coming back.
    private bool inBackground;

    /// <summary>
    /// MoneyBud going to the background, on the phone: the last moment it can count on, since
    /// Android may end it there without warning (arc42 §12, <i>Android's lifecycle</i>). Changes not
    /// yet kept get one more try, as closing gives them on the desktop; with everything kept,
    /// nothing is written. Nothing is asked, and — unlike <see cref="Close"/> — MoneyBud stays open,
    /// holding its data, to carry on when it comes back. Until then <see cref="Tick"/> does nothing
    /// (plan for increment 14, D4; carry-on-when-saving-fails.feature).
    /// </summary>
    public void GoToBackground()
    {
        inBackground = true;
        if (IsUnsaved) Keep();
    }

    /// <summary>
    /// MoneyBud back in the foreground, on the phone: it looks again at once, exactly as the minute's
    /// <see cref="Tick"/> does, so that a screen left overnight is right the moment it is seen rather
    /// than up to a minute later — and whatever came due in the background is done and said now,
    /// where it can be seen (plan for increment 14, D4).
    /// </summary>
    public void ComeBack()
    {
        inBackground = false;
        Look();
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
    public RecordExpenseResult? RecordExpense(
        string? amount, string? category, string? label, DateOnly? date = null, Account? account = null,
        Frequency? repeat = null)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        SettleBeforeActing();
        var result = Ledger.RecordExpense(euros, category, date ?? Ledger.Today, label, account, repeat);

        if (result.Expense is { } expense)
            Tell(Tekst.ExpenseRecorded(expense, result.CategoryBroughtBack), PeriodOf(expense.Date));
        else
            Refuse(Tekst.Refusal(result.Refusal!.Value, category));

        return result;
    }

    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public RecordIncomeResult? RecordIncome(
        string? amount, string? label, DateOnly? date = null, Account? account = null, Frequency? repeat = null)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        SettleBeforeActing();
        var result = Ledger.RecordIncome(euros, label, date ?? Ledger.Today, account, repeat);

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

    /// <summary>
    /// The offer's button: takes the plan offered over, in full, into the <b>period on screen</b> —
    /// never the assign form's own period, because the grey figures it takes over are on this
    /// period's rows (arc42 §12, <i>The offer is a button, and a figure on each row</i>). Asks
    /// nothing, and says afterwards which period the plan went into.
    ///
    /// <para>For up to a minute after a period boundary the button can still be on screen over a
    /// period that is now past. Pressed then, it is refused as any assignment there is, and the
    /// redraw after the refusal takes the offer away.</para>
    /// </summary>
    public TakeOverPlanResult TakeOverPlan()
    {
        var result = Ledger.TakeOverPlan(ShownPeriod);

        if (result.WasTakenOver)
            Tell(Tekst.PlanTakenOver(result.Plan!, result.Into!), landedIn: null);
        else
            Refuse(Tekst.Refusal(result.Refusal!.Value, category: null));

        return result;
    }

    /// <summary>The offer's button, shown only while a plan is offered.</summary>
    [RelayCommand]
    private void TakeOver() => TakeOverPlan();

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
        var destination = Ledger.SweepDestination;
        var category = Ledger.ArchiveCategory(name);
        Tell(AndIfNoLongerDestination(Tekst.CategoryArchived(category), destination), landedIn: null);
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
    ///
    /// <para>This keeps the repeat's frequency as it is. The overload that takes a frequency, which the
    /// form uses, changes it too.</para>
    /// </summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeExpenseResult? ChangeExpense(
        Expense expense, string? amount, string? category, string? label, DateOnly? date = null,
        Account? account = null) =>
        ChangeExpense(expense, amount, category, label, date, account, Ledger.FrequencyOf(expense));

    /// <summary>Changes an expense and its repeat, the frequency null being one-off (§12, ruling 3).</summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeExpenseResult? ChangeExpense(
        Expense expense, string? amount, string? category, string? label, DateOnly? date, Account? account,
        Frequency? repeat)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        SettleBeforeActing();
        var result = Ledger.ChangeExpense(expense, euros, category, date ?? Ledger.Today, label, account, repeat);

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

    /// <summary>Changes an income as an expense is changed, keeping its repeat's frequency as it is.</summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeIncomeResult? ChangeIncome(
        Income income, string? amount, string? label, DateOnly? date = null, Account? account = null) =>
        ChangeIncome(income, amount, label, date, account, Ledger.FrequencyOf(income));

    /// <summary>Changes an income and its repeat.</summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeIncomeResult? ChangeIncome(
        Income income, string? amount, string? label, DateOnly? date, Account? account, Frequency? repeat)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        SettleBeforeActing();
        var result = Ledger.ChangeIncome(income, euros, label, date ?? Ledger.Today, account, repeat);

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
    public void AskToRemove(Expense expense) => Ask(Tekst.AskToRemove(expense, Ledger.FrequencyOf(expense) is not null), () =>
    {
        Ledger.RemoveExpense(expense);
        if (ExpenseForm.Editing?.Id == expense.Id) ExpenseForm.Clear();
        Tell(Tekst.ExpenseRemoved(expense), landedIn: null);
    });

    /// <summary>Asks whether to remove an income, as <see cref="AskToRemove(Expense)"/>.</summary>
    public void AskToRemove(Income income) => Ask(Tekst.AskToRemove(income, Ledger.FrequencyOf(income) is not null), () =>
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

        // A start-day question asked before midnight names a current period that may be gone; the
        // tick drops it, and until the tick has run, answering it is answering nothing (plan for
        // increment 13, reading 4).
        if (askedStartDay is not null && Ledger.Today != askedOn)
        {
            SayNothing();
            return;
        }

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

    private void Ask(string text, Action act, string confirmText = Tekst.Remove, int? startDay = null)
    {
        Notice = null;
        savedAgain = false;
        Question = new Question(text, confirmText);
        onConfirm = act;
        askedStartDay = startDay;
        askedOn = Ledger.Today;
        Refresh();
    }

    // The list shows the day set again once no start-day question waits; every caller redraws.
    private void DropQuestion()
    {
        Question = null;
        onConfirm = null;
        askedStartDay = null;
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
        var destination = Ledger.SweepDestination;
        var category = Ledger.DeleteCategory(name);
        Tell(AndIfNoLongerDestination(Tekst.CategoryDeleted(category), destination), landedIn: null);
        return category;
    }

    /// <summary>The delete button on the row of a category with no history anywhere.</summary>
    [RelayCommand]
    private void Delete(string name) => DeleteCategory(name);

    // ------------------------------------------------------------------ accounts

    /// <summary>
    /// The strip across the top: every account with its balance today, the pool account first and
    /// the rest in the order added (arc42 §12, <i>Accounts and net worth</i>), and, since increment 15,
    /// every account but the pool account with its <i>Vrij</i>. The same in every period — a balance
    /// is about today, not the period on screen.
    /// </summary>
    public IReadOnlyList<AccountLine> Accounts =>
        Ledger.Accounts
            .Select(a => new AccountLine(a, Ledger.BalanceOf(a), a == Ledger.PoolAccount, Ledger.UnclaimedOf(a))
                { IsOpen = a == HistoryAccount })
            .ToList();

    private IReadOnlyList<Account> accountChoices = [];
    private IReadOnlyList<string> accountNames = [];

    /// <summary>
    /// The accounts a form's account list offers, in the strip's order. The same list as long as the
    /// accounts, their order and their names stay the same, and a new one when any changes: a list
    /// handed a new collection lets go of what it had selected and takes it up again from its form,
    /// while one handed the same collection on every refresh would do that once a minute.
    /// </summary>
    public IReadOnlyList<Account> AccountChoices
    {
        get
        {
            var wanted = Ledger.Accounts;
            if (!wanted.SequenceEqual(accountChoices) || !wanted.Select(a => a.Name).SequenceEqual(accountNames))
            {
                accountChoices = wanted;
                accountNames = wanted.Select(a => a.Name).ToList();
            }

            return accountChoices;
        }
    }

    private IReadOnlyList<BackingChoice> backingChoices = [];
    private IReadOnlyList<Account>? backingChoicesFor;

    /// <summary>
    /// The <i>Staat op</i> list every category row offers. Made anew only when
    /// <see cref="AccountChoices"/> is, so for as long as the accounts, their order and their names
    /// stay the same a refresh does not hand every row's list a new collection.
    /// </summary>
    public IReadOnlyList<BackingChoice> BackingChoices
    {
        get
        {
            var accounts = AccountChoices;
            if (!ReferenceEquals(accounts, backingChoicesFor))
            {
                backingChoices = PeriodOverview.BackingChoicesOf(Ledger);
                backingChoicesFor = accounts;
            }

            return backingChoices;
        }
    }

    /// <summary>Net worth, <i>Vermogen</i>: every balance today, summed.</summary>
    public Money NetWorth => Ledger.NetWorth;

    public string NetWorthText => Tekst.Euro(NetWorth);

    /// <summary>
    /// Below zero, net worth carries the one marker, badge <i>Rood</i>, as an overdrawn account does.
    /// Exactly zero does not.
    /// </summary>
    public Marker NetWorthMarker => NetWorth.IsNegative ? Marker.Over : Marker.None;

    public bool IsNetWorthNegative => NetWorth.IsNegative;

    /// <summary>
    /// Adds an account with the starting balance typed. Left empty, or only spaces, there is no
    /// starting balance, as a label that trims to nothing is no label; anything else must read as an
    /// amount (arc42 §12).
    /// </summary>
    /// <returns>The ledger's answer, or null when the starting balance could not be read as one.</returns>
    public AddAccountResult? AddAccount(string? name, string? startingBalance)
    {
        decimal? euros = null;
        if (!string.IsNullOrWhiteSpace(startingBalance))
        {
            if (!AmountInput.TryRead(startingBalance, out var typed))
            {
                NotAnAmount(startingBalance);
                return null;
            }

            euros = typed;
        }

        var result = Ledger.AddAccount(name, euros);

        if (result.Account is { } account)
        {
            Tell(Tekst.AccountAdded(account), landedIn: null);
            // After the redraw, so that the lists already hold the new account when the transfer
            // form moves Naar onto it; a list cannot show an account it has not got.
            TransferForm.AccountsChanged(oldPool: null);
        }
        else
            Refuse(Tekst.Refusal(result.Refusal!.Value));

        return result;
    }

    /// <summary>
    /// Renames an account: announced from what to what, quiet when the name is exactly as it was,
    /// refused for a name that trims to nothing or another account has. Every entry on it, and every
    /// form holding it, follows, because they hold the account and not its name.
    /// </summary>
    public RenameAccountResult RenameAccount(Account account, string? newName)
    {
        var result = Ledger.RenameAccount(account, newName);

        if (result.WasRefused)
        {
            Refuse(Tekst.AccountRenameRefusal(result.Refusal!.Value));
            return result;
        }

        RenamingAccount = false;
        NewAccountName = null;

        if (result.Outcome == RenameOutcome.Unchanged)
            SayNothing();
        else
            Tell(Tekst.AccountRenamed(result.OldName!, result.Account!), landedIn: null);

        return result;
    }

    /// <summary>
    /// Deletes an unused account, without asking, and says so. Offered only on such an account
    /// (<see cref="Ledger.CanDeleteAccount"/>); the ledger throws for any other. A form that had it
    /// chosen goes back to the pool account, and its history closes.
    /// </summary>
    public Account DeleteAccount(Account account)
    {
        Ledger.DeleteAccount(account);

        ExpenseForm.Forget(account);
        IncomeForm.Forget(account);
        TransferForm.Forget(account);
        if (HistoryAccount == account) CloseHistory();

        Tell(Tekst.AccountDeleted(account), landedIn: null);
        return account;
    }

    /// <summary>
    /// Makes an account the pool account, and says so. New entries start out on it from now on;
    /// nothing already recorded moves.
    /// </summary>
    public void MakePool(Account account)
    {
        var oldPool = Ledger.PoolAccount;
        Ledger.MakePool(account);
        Tell(Tekst.PoolChanged(account), landedIn: null);

        // After the redraw, as for a new account: the lists are in their new order first.
        ExpenseForm.PoolChanged(oldPool);
        IncomeForm.PoolChanged(oldPool);
        TransferForm.AccountsChanged(oldPool);
    }

    /// <summary>
    /// Records the balance the bank shows today for an account. Dated today whatever period is on
    /// screen. Any amount reads, zero and below zero included; the one refusal is finer than a cent.
    /// </summary>
    /// <returns>The ledger's answer, or null when the balance could not be read as an amount.</returns>
    public CorrectBalanceResult? CorrectBalance(Account account, string? balance)
    {
        if (!AmountInput.TryRead(balance, out var euros))
        {
            NotAnAmount(balance);
            return null;
        }

        var result = Ledger.CorrectBalance(account, euros);

        if (result.Correction is { } correction)
        {
            BalanceInput = null;
            Tell(Tekst.BalanceCorrected(correction), landedIn: null);
        }
        else
            Refuse(Tekst.BalanceFinerThanCent);

        return result;
    }

    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public RecordTransferResult? RecordTransfer(string? amount, Account from, Account to, DateOnly? date = null)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        var result = Ledger.RecordTransfer(euros, from, to, date ?? Ledger.Today);

        if (result.Transfer is { } transfer)
            Tell(Tekst.TransferRecorded(transfer), landedIn: null);
        else
            Refuse(Tekst.Refusal(result.Refusal!.Value));

        return result;
    }

    /// <summary>
    /// Changes a transfer, judged as recording it now would be. Announced; saved unchanged is
    /// quiet. A transfer is in no period's lists, so nothing says where it went.
    /// </summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeTransferResult? ChangeTransfer(
        Transfer transfer, string? amount, Account from, Account to, DateOnly? date = null)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        var result = Ledger.ChangeTransfer(transfer, euros, from, to, date ?? Ledger.Today);

        switch (result.Outcome)
        {
            case ChangeOutcome.Changed:
                Tell(Tekst.TransferChanged(result.Transfer!), landedIn: null);
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

    /// <summary>Asks whether to remove a transfer, as removing an entry asks.</summary>
    public void AskToRemove(Transfer transfer) => Ask(Tekst.AskToRemove(transfer), () =>
    {
        Ledger.RemoveTransfer(transfer);
        if (TransferForm.Editing?.Id == transfer.Id) TransferForm.Clear();
        Tell(Tekst.TransferRemoved(transfer), landedIn: null);
    });

    /// <summary>
    /// Asks whether to remove a balance correction or a starting balance: a record is lost, so it
    /// asks first, as removing an entry does.
    /// </summary>
    public void AskToRemove(BalanceCorrection correction) => Ask(Tekst.AskToRemove(correction), () =>
    {
        Ledger.RemoveBalanceCorrection(correction);
        Tell(Tekst.BalanceCorrectionRemoved(correction), landedIn: null);
    });

    // ------------------------------------------------------------------ backing

    /// <summary>
    /// A choice in a category row's <i>Staat op</i> list: backs the category, points its backing at
    /// another account, or — given null — removes it (arc42 §12, <i>Backing and Accumulated</i>).
    /// Never asks first, and says afterwards what moved, or only the backing when nothing did. Kept,
    /// like every act that goes through.
    ///
    /// <para><b>Choosing what is already set does nothing at all</b>: nothing moves, nothing is said,
    /// what was said stays, and nothing is redrawn. A list writes back what it shows whenever it is
    /// redrawn, so this is called that way after every act, and must leave the screen exactly as the
    /// act left it.</para>
    /// </summary>
    public SetBackingResult SetBacking(string categoryName, Account? account)
    {
        var destination = Ledger.SweepDestination;
        var result = Ledger.SetBacking(categoryName, account);
        if (result.Outcome != BackingOutcome.Unchanged)
            Tell(AndIfNoLongerDestination(Tekst.BackingSet(result), destination), landedIn: null);

        return result;
    }

    // ------------------------------------------------------------------ Vrij, and moving Opgebouwd

    /// <summary>
    /// <i>Verplaatsen</i>: moves the amount typed from one end to the other (arc42 §12, <i>One act moves
    /// an amount of purpose</i>). <i>Niet toegewezen</i> is the period on screen's, which must be the
    /// current one. Never asks first; says afterwards what moved, and which way the money went if it
    /// went between two accounts. Zero is said and changes nothing. Kept, like every act that goes
    /// through.
    /// </summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ReallocateResult? Reallocate(string? amount, ReallocationEnd from, ReallocationEnd to)
    {
        if (!AmountInput.TryRead(amount, out var euros))
        {
            NotAnAmount(amount);
            return null;
        }

        var result = Ledger.Reallocate(euros, from, to, ShownPeriod);

        if (result.Made is { } made)
            Tell(Tekst.Reallocated(made), landedIn: null);
        else if (result.Refusal is { } refusal)
            Refuse(Tekst.Refusal(refusal, result.Into?.Name));
        else
            Tell(Tekst.ReallocatedNothing(from, to), landedIn: null, changed: false);

        return result;
    }

    // ------------------------------------------------------------------ the sweep

    private IReadOnlyList<SweepChoice> sweepChoices = [];

    /// <summary>
    /// The <i>Restant naar</i> list. The same list for as long as the categories it offers and their
    /// names stay the same, and a new one when any changes, for the reason
    /// <see cref="AccountChoices"/> gives: a list handed a new collection on every refresh would let
    /// go of its choice and take it up again once a minute.
    /// </summary>
    public IReadOnlyList<SweepChoice> SweepChoices
    {
        get
        {
            var wanted = PeriodOverview.SweepChoicesOf(Ledger);
            if (!wanted.Select(c => (c.Category, c.Text)).SequenceEqual(sweepChoices.Select(c => (c.Category, c.Text))))
                sweepChoices = wanted;

            return sweepChoices;
        }
    }

    /// <summary>
    /// A choice in the <i>Restant naar</i> list: a backed category's name, or null for none (arc42
    /// §12, <i>The destination is one list</i>). Never asks first; says afterwards where the leftover
    /// goes from now on. Kept, like every act that goes through.
    ///
    /// <para><b>Choosing what is already set does nothing at all</b>: nothing is said, what was said
    /// stays, nothing is kept and nothing is redrawn — the list writes back what it shows on every
    /// redraw, as the <i>Staat op</i> lists do (<see cref="SetBacking"/>).</para>
    /// </summary>
    public SetSweepDestinationResult SetSweepDestination(string? categoryName)
    {
        var result = Ledger.SetSweepDestination(categoryName);
        if (result.Outcome != SweepDestinationOutcome.Unchanged)
            Tell(Tekst.SweepDestinationSet(result), landedIn: null);

        return result;
    }

    /// <summary>
    /// <i>Restant bijwerken</i> on the period on screen: moves exactly the difference its line shows,
    /// never asks, and says afterwards what moved (§12, <i>A swept period that changes</i>). Offered
    /// only where the line offers it.
    ///
    /// <para>For up to a minute after a period boundary the button can be on screen over a line that
    /// settling is about to change: the period that just ended is not swept until the next tick or
    /// act. So this settles first, and when the line then offers no button, nothing is moved: the
    /// sweep settling made is said, and the redraw takes the button away. Null then.</para>
    /// </summary>
    public BringUpToDateResult? BringSweepUpToDate()
    {
        Ledger.Settle();
        if (Ledger.SweepLineFor(ShownPeriod) is not { CanBringUpToDate: true })
        {
            SayNothing();
            return null;
        }

        var result = Ledger.BringUpToDate(ShownPeriod);
        Tell(Tekst.BroughtUpToDate(result), landedIn: null);
        return result;
    }

    [RelayCommand]
    private void BringUpToDate() => BringSweepUpToDate();

    // Unbacking, archiving or deleting the destination clears it, and the act says so (§12, ruling 6).
    private string AndIfNoLongerDestination(string text, Category? destinationBefore) =>
        destinationBefore is not null && Ledger.SweepDestination != destinationBefore
            ? $"{text} {Tekst.NoLongerSweepDestination(destinationBefore.Name)}"
            : text;

    // ------------------------------------------------------------------ an account's history

    /// <summary>The account whose history is open, or null. Clicking an account in the strip opens it.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHistoryOpen), nameof(History))]
    public partial Account? HistoryAccount { get; private set; }

    public bool IsHistoryOpen => HistoryAccount is not null;

    /// <summary>
    /// The open account's history, every period, newest first (<see cref="HistoryLine"/>). Empty
    /// when no history is open.
    /// </summary>
    public IReadOnlyList<HistoryLine> History =>
        HistoryAccount is { } account
            ? Ledger.HistoryOf(account)
                .Select(e => new HistoryLine(
                    e, account, e is BalanceCorrection c ? Ledger.DifferenceOf(c) : null,
                    e is Movement { SweptFor: { } sweptFor } ? Ledger.Calendar.PeriodContaining(sweptFor) : null))
                .ToList()
            : [];

    /// <summary>Whether the open account can be deleted: unused, and not the pool account.</summary>
    public bool CanDeleteHistoryAccount => HistoryAccount is { } account && Ledger.CanDeleteAccount(account);

    /// <summary>Whether the open account can be made the pool account: it is not the pool already.</summary>
    public bool CanMakeHistoryAccountPool => HistoryAccount is { } account && account != Ledger.PoolAccount;

    /// <summary>What the balance box in the open history holds.</summary>
    [ObservableProperty]
    public partial string? BalanceInput { get; set; }

    /// <summary>Whether the open account's name is a text box, being renamed.</summary>
    [ObservableProperty]
    public partial bool RenamingAccount { get; private set; }

    /// <summary>What the account's rename box holds. Starts as the name it already has.</summary>
    [ObservableProperty]
    public partial string? NewAccountName { get; set; }

    /// <summary>Clicking an account in the strip: opens its history. Clicking the open one closes it.</summary>
    [RelayCommand]
    public void OpenHistory(Account account)
    {
        if (HistoryAccount == account)
        {
            CloseHistory();
            return;
        }

        HistoryAccount = account;
        RenamingAccount = false;
        NewAccountName = null;
        BalanceInput = null;
        Refresh();
    }

    [RelayCommand]
    public void CloseHistory()
    {
        HistoryAccount = null;
        RenamingAccount = false;
        NewAccountName = null;
        BalanceInput = null;
        Refresh();
    }

    [RelayCommand]
    private void CorrectOpenBalance()
    {
        if (HistoryAccount is { } account) CorrectBalance(account, BalanceInput);
    }

    [RelayCommand]
    private void StartAccountRename()
    {
        RenamingAccount = true;
        NewAccountName = HistoryAccount?.Name;
    }

    [RelayCommand]
    private void CancelAccountRename()
    {
        RenamingAccount = false;
        NewAccountName = null;
    }

    [RelayCommand]
    private void SaveAccountRename()
    {
        if (HistoryAccount is { } account) RenameAccount(account, NewAccountName);
    }

    [RelayCommand]
    private void DeleteOpenAccount()
    {
        if (HistoryAccount is { } account) DeleteAccount(account);
    }

    [RelayCommand]
    private void MakeOpenAccountPool()
    {
        if (HistoryAccount is { } account) MakePool(account);
    }

    /// <summary>A transfer's row in a history: loads it into the transfer form, to be changed or removed.</summary>
    [RelayCommand]
    public void EditTransfer(HistoryLine line)
    {
        DropQuestion();
        TransferForm.Load((Transfer)line.Entry);
    }

    /// <summary>The remove button on a history row that offers it: a transfer or a typed balance.</summary>
    [RelayCommand]
    public void RemoveFromHistory(HistoryLine line)
    {
        switch (line.Entry)
        {
            case Transfer transfer: AskToRemove(transfer); break;
            case BalanceCorrection correction: AskToRemove(correction); break;
            default: throw new InvalidOperationException("Only a transfer or a typed balance is removed from a history.");
        }
    }

    // ------------------------------------------------------------------ telling

    private BudgetPeriod PeriodOf(DateOnly date) => Ledger.Calendar.PeriodContaining(date);

    // Whatever MoneyBud says after an act replaces a question still waiting: the user has moved
    // on from it, and a question and a notice are never shown together. The save line is not
    // what MoneyBud says after an act, and is left to Keep.

    /// <summary>
    /// An act that went through: said, and then the ledger is kept — unless the act changed
    /// nothing, such as adding a name already there, when there is nothing to keep. Whatever the
    /// act's settling did by itself is said first, and kept whatever the act was.
    /// </summary>
    private void Tell(string text, BudgetPeriod? landedIn, bool changed = true)
    {
        var settled = TakeWhatSettlingDid();
        DropQuestion();
        Notice = landedIn is { } period && period != ShownPeriod
            ? new Notice(settled.Around($"{text} {Tekst.WentInto(period)}"), IsRefusal: false, WentInto: period)
                { Repeated = settled.Repeated }
            : new Notice(settled.Around(text), IsRefusal: false) { Repeated = settled.Repeated };
        savedAgain = false;
        if (changed || settled.Said is not null) Keep();
        Refresh();
    }

    private void Refuse(string text)
    {
        var settled = TakeWhatSettlingDid();
        DropQuestion();
        Notice = new Notice(settled.Around(text), IsRefusal: true) { Repeated = settled.Repeated };
        savedAgain = false;
        if (settled.Said is not null) Keep();
        Refresh();
    }

    /// <summary>
    /// An act with no outcome to tell: nothing is said, and what was said before is gone. Nothing
    /// changed, so there is nothing to keep — unless the act's settling did something by itself,
    /// which is said, and kept.
    /// </summary>
    private void SayNothing()
    {
        var settled = TakeWhatSettlingDid();
        DropQuestion();
        Notice = settled.Said is { } said ? new Notice(said, IsRefusal: false) { Repeated = settled.Repeated } : null;
        savedAgain = false;
        if (settled.Said is not null) Keep();
        Refresh();
    }

    // What settling did before an act that can itself record occurrences, taken apart from what the
    // act caused, so that the notice says each where it happened.
    private Said? saidBeforeAct;

    /// <summary>
    /// Settles before an act that can record occurrences itself — recording or changing an income or
    /// an expense, which may set up or move a repeat — and holds what settling did, so the notice
    /// can say it in front of the act and what the act caused after it: in the order it happened
    /// (ruled at the build, 2026-09-28).
    /// </summary>
    private void SettleBeforeActing()
    {
        Ledger.Settle();
        saidBeforeAct = TakeSaid();
    }

    /// <summary>
    /// What settling did by itself since last asked, to be said once (plan for increment 12, 7): in
    /// front of what the act says, and — after an act that settled first — what the act itself
    /// caused after it. <see cref="Settled.Said"/> is null when it did nothing to say.
    /// </summary>
    private Settled TakeWhatSettlingDid()
    {
        var before = saidBeforeAct;
        saidBeforeAct = null;
        var now = TakeSaid();
        return before is null ? new Settled(now, Said.Nothing) : new Settled(before, now);
    }

    /// <summary>
    /// The occurrences settling recorded, in one sentence with any category they brought back, then
    /// the periods it swept.
    /// </summary>
    private Said TakeSaid()
    {
        var repeated = Ledger.TakeOccurrencesMade();
        var swept = Ledger.TakeSweepsMade();
        string?[] sentences = [repeated.Count > 0 ? Tekst.Repeated(repeated) : null, swept.Count > 0 ? Tekst.Swept(swept) : null];
        var text = string.Join(" ", sentences.OfType<string>());
        return new Said(text.Length > 0 ? text : null, repeated);
    }

    private sealed record Said(string? Text, IReadOnlyList<OccurrenceMade> Repeated)
    {
        public static readonly Said Nothing = new(null, []);
    }

    // What settling did in front of an act's own sentence, and what the act caused after it.
    private sealed record Settled(Said InFront, Said After)
    {
        public string? Said => (InFront.Text, After.Text) switch
        {
            (null, null) => null,
            (var first, null) => first,
            (null, var then) => then,
            var (first, then) => $"{first} {then}",
        };

        public IReadOnlyList<OccurrenceMade> Repeated => [.. InFront.Repeated, .. After.Repeated];

        public string Around(string text) =>
            string.Join(" ", new[] { InFront.Text, text, After.Text }.OfType<string>());
    }

    private void NotAnAmount(string? typed) =>
        Refuse(AmountInput.Read(typed, out _) == AmountReading.Ambiguous
            ? Tekst.AmbiguousAmount(typed!)
            : Tekst.NotAnAmount(typed));
}
