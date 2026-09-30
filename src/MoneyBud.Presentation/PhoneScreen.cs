using MoneyBud.Domain;

namespace MoneyBud.Presentation;

/// <summary>What is pulled over the phone's home screen, the ring (arc42 §12, <i>Panels pulled over it</i>).</summary>
public enum PhonePanel
{
    None,

    /// <summary>From the left.</summary>
    Income,

    /// <summary>From the right.</summary>
    Expenses,

    /// <summary>From below.</summary>
    Budget,

    /// <summary>From above.</summary>
    Accounts,
}

/// <summary>
/// How far a panel is pulled in. Income and expenses: the list, then the form. Accounts: the list,
/// then an account's history. The budget: in full, in one pull — it once stopped half-way first,
/// which Axel had removed at the review (2026-09-30).
/// </summary>
public enum PanelStep
{
    Closed,
    First,
    Second,
}

/// <summary>
/// The phone's own screen rules, over <see cref="MoneyBudApp"/>, without a toolkit (plan for
/// increment 14, D2). The phone head lays out, draws and animates; everything it would otherwise
/// decide about where the user is and what a touch means is decided here, where the tests reach it
/// (ADR 0013, decision 4). The desktop has no counterpart, since nothing on it stays chosen or is
/// pulled in.
///
/// <list type="bullet">
/// <item><b>Which panel is open, and how far</b> (<see cref="Open"/>, <see cref="Step"/>). The view's
/// springs follow the finger and report where a panel came to rest (<see cref="Settled"/>); an act or
/// the back button moves a panel here, and the view animates to it (<see cref="Moved"/>).</item>
/// <item><b>The chosen slice.</b> Touching the ring points at the slice in the finger's direction; it
/// stays chosen when the finger lifts; a tap on the slice already chosen, or on the ring's centre,
/// lets go, and so does a tap anywhere else on the home screen (§12, <i>Touching the ring</i>). It is
/// held as the category itself, not as a place on the ring, so a change that reorders the slices, or
/// renames the category, never moves the choice onto another slice.</item>
/// <item><b>What the budget panel is about</b> (<see cref="BudgetCategory"/>): the chosen category, or
/// a row tapped in its list; otherwise the whole list (§12, <i>The budget panel</i>).</item>
/// <item><b>The back button</b> (<see cref="Back"/>): first lets go of the budget's category, then goes
/// back a step, then closes the panel (§12, <i>Panels pulled over it</i>).</item>
/// <item><b>Where an act leaves the panel</b>: a new entry that goes through closes its panel, so its
/// slice grows where it can be seen; a change saved or an entry removed goes back to the list; a
/// refusal stays where it is (plan D2).</item>
/// </list>
///
/// <para>Pointing goes through <see cref="MoneyBudApp.PointAt"/>, as the desktop's hover does, so what
/// the ring's centre shows is <see cref="MoneyBudApp.PointedRow"/> on both heads, and the pointing
/// scenarios cover both.</para>
/// </summary>
public sealed class PhoneScreen
{
    // The slice chosen on the ring: a category's, or Unassigned's.
    private Category? chosen;
    private bool unassignedChosen;

    // What the budget panel is about. Follows the chosen slice, and a row tapped; survives the slice
    // going away while the panel is open (a budget taken back to zero), so the page does not vanish.
    private Category? subject;

    // What was chosen when the finger went down on the ring, for telling a tap that lets go.
    private (Category? Category, bool Unassigned) chosenAtPress;

    // Whether the question waiting was asked from an entry's form, which goes back to its list once
    // the entry is removed.
    private bool removingFromForm;

    public PhoneScreen(MoneyBudApp app)
    {
        App = app;
        app.PropertyChanged += (_, e) =>
        {
            // A refresh says everything may have changed: the slices may have reordered, so the
            // chosen slice is pointed at again where it is now. Everything else — a notice, text typed
            // into a bound box — changes nothing on the ring.
            if (string.IsNullOrEmpty(e.PropertyName)) Repoint();
        };
    }

    public MoneyBudApp App { get; }

    /// <summary>Raised whenever what the screen shows may have changed: after every act, every tick, and every panel come to rest.</summary>
    public event Action? Changed;

    /// <summary>
    /// Raised when the slice chosen on the ring changes, and with it what the ring's centre and the
    /// budget panel are about. Apart from <see cref="Changed"/>, since it comes at every slice a finger
    /// slides across, where redrawing everything would not keep up.
    /// </summary>
    public event Action? ChoiceChanged;

    /// <summary>
    /// Raised when a panel is moved by something other than the finger — an act, a row tapped, the
    /// back button — so the view animates to <see cref="Open"/> and <see cref="Step"/>. A drag is
    /// reported through <see cref="Pull"/> and <see cref="Settled"/>, and raises nothing.
    /// </summary>
    public event Action? Moved;

    public PhonePanel Open { get; private set; }

    public PanelStep Step { get; private set; }

    // ------------------------------------------------------------------ panels

    /// <summary>
    /// The finger starts pulling a panel. Another open panel closes, as it would if it were pushed
    /// away first.
    /// </summary>
    public void Pull(PhonePanel panel)
    {
        if (panel == PhonePanel.None || Open == panel) return;

        if (Open != PhonePanel.None) Leave(Open, PanelStep.Closed);
        Open = panel;
        Step = PanelStep.Closed;
    }

    /// <summary>
    /// A panel came to rest, after a drag or an animation. Leaving an entry's form lets go of an entry
    /// being changed — a new entry being typed stays, for the next time the form is pulled in, as it
    /// does on the desktop. Leaving an account's history closes it. Closing the budget lets go of a
    /// row it was opened on, while a slice still chosen on the ring stays chosen.
    /// </summary>
    public void Settled(PhonePanel panel, PanelStep step)
    {
        if (panel == PhonePanel.None || panel != Open) return;
        // Pulled and let go at once is closed from where it started: still a closing.
        if (step == Step && step != PanelStep.Closed) return;

        Leave(panel, step);
        Step = step;
        if (step == PanelStep.Closed) Open = PhonePanel.None;
        Changed?.Invoke();
    }

    private void Leave(PhonePanel panel, PanelStep to)
    {
        switch (panel)
        {
            case PhonePanel.Income when to != PanelStep.Second && App.IncomeForm.IsEditing:
                App.IncomeForm.Cancel();
                break;
            case PhonePanel.Expenses when to != PanelStep.Second && App.ExpenseForm.IsEditing:
                App.ExpenseForm.Cancel();
                break;
            case PhonePanel.Accounts when to != PanelStep.Second && App.IsHistoryOpen:
                App.CloseHistory();
                break;
            case PhonePanel.Budget when to == PanelStep.Closed && chosen is null:
                subject = null;
                break;
        }
    }

    /// <summary>Moves a panel by itself, and has the view follow.</summary>
    private void GoTo(PhonePanel panel, PanelStep step)
    {
        if (panel != Open)
        {
            if (Open != PhonePanel.None) Leave(Open, PanelStep.Closed);
            Open = panel;
            Step = PanelStep.Closed;
        }

        if (panel != PhonePanel.None && step != Step) Leave(panel, step);
        Step = panel == PhonePanel.None ? PanelStep.Closed : step;
        if (Step == PanelStep.Closed) Open = PhonePanel.None;
        Moved?.Invoke();
        Changed?.Invoke();
    }

    /// <summary>
    /// Android's back button (§12): lets go of the question waiting, then of the budget's category,
    /// then goes back one step, then closes the panel. False when there was nothing to go back from,
    /// so Android does what it does on the home screen.
    /// </summary>
    public bool Back()
    {
        if (App.IsAsking)
        {
            App.Decline();
            removingFromForm = false;
            return true;
        }

        if (Open == PhonePanel.Budget && subject is not null)
        {
            ShowAllCategories();
            return true;
        }

        switch (Open)
        {
            case PhonePanel.None:
                return false;
            case PhonePanel.Budget:
                GoTo(PhonePanel.None, PanelStep.Closed);
                return true;
            default:
                GoTo(Step == PanelStep.Second ? Open : PhonePanel.None, PanelStep.First);
                return true;
        }
    }

    // ------------------------------------------------------------------ the ring

    /// <summary>The finger goes down on the ring, at a share of it read clockwise from the top, or at its centre (null).</summary>
    public void TouchRing(double? share)
    {
        chosenAtPress = (chosen, unassignedChosen);
        if (share is { } at) Choose(App.Overview.Ring.SliceAt(at));
    }

    /// <summary>The finger slides over the ring: the slice in its direction is chosen as it passes.</summary>
    public void SlideOnRing(double? share)
    {
        if (share is { } at) Choose(App.Overview.Ring.SliceAt(at));
    }

    /// <summary>
    /// The finger lifts from the ring. What it chose stays chosen, unless it was a tap — no slide —
    /// on the centre, or on the slice that was already chosen when it went down: that lets go.
    /// </summary>
    public void LiftFromRing(double? share, bool slid)
    {
        if (slid) return;
        if (share is null || (chosen, unassignedChosen) == chosenAtPress) Release();
    }

    /// <summary>A tap on the home screen that is not on the ring lets go of the chosen slice.</summary>
    public void TapHome() => Release();

    /// <summary>The slice chosen on the ring, as the Overview on screen has it now; null when none is.</summary>
    public RingSlice? ChosenSlice => SliceOf(App.Overview.Ring);

    private RingSlice? SliceOf(Ring ring) =>
        unassignedChosen ? ring.UnassignedSlice
        : chosen is { } category ? ring.CategorySlices.FirstOrDefault(s => s.Category == category.Name)
        : null;

    private void Choose(RingSlice? slice)
    {
        if (slice is null) return;

        var category = slice.Category is { } name ? Find(name) : null;
        if (category == chosen && slice.IsUnassigned == unassignedChosen) return;

        chosen = category;
        unassignedChosen = slice.IsUnassigned;
        subject = category;
        App.PointAt(slice.Start + slice.Sweep / 2);
        ChoiceChanged?.Invoke();
    }

    private void Release()
    {
        if (chosen is null && !unassignedChosen && subject is null) return;

        chosen = null;
        unassignedChosen = false;
        subject = null;
        App.PointAt(null);
        ChoiceChanged?.Invoke();
    }

    // After every refresh: the chosen slice may have moved, or gone. It is pointed at where it is now;
    // gone, it is let go. The budget's category goes when it is no longer listed at all.
    private void Repoint()
    {
        var overview = App.Overview;
        var slice = SliceOf(overview.Ring);
        if (slice is null)
        {
            chosen = null;
            unassignedChosen = false;
        }

        if (subject is { } about && overview.Rows.All(r => r.Name != about.Name)) subject = null;

        var pointed = App.PointedSlice;
        if (pointed?.Category != slice?.Category || pointed?.IsUnassigned != slice?.IsUnassigned)
            App.PointAt(slice is null ? null : slice.Start + slice.Sweep / 2);

        Changed?.Invoke();
    }

    // ------------------------------------------------------------------ the budget panel

    /// <summary>The category the budget panel is about, as its row on screen; null for the whole list.</summary>
    public CategoryRow? BudgetCategory =>
        subject is { } about ? App.Overview.Rows.FirstOrDefault(r => r.Name == about.Name) : null;

    /// <summary>A row tapped in the budget's list: the panel opens on it, and its slice, if it has one, is chosen.</summary>
    public void OpenCategory(string name)
    {
        var category = Find(name) ?? throw new InvalidOperationException($"No category is named {name}.");
        var slice = App.Overview.Ring.CategorySlices.FirstOrDefault(s => s.Category == category.Name);

        chosen = slice is null ? null : category;
        unassignedChosen = false;
        subject = category;
        App.PointAt(slice is null ? null : slice.Start + slice.Sweep / 2);
        if (Open != PhonePanel.Budget || Step == PanelStep.Closed) GoTo(PhonePanel.Budget, PanelStep.First);
        else Changed?.Invoke();
    }

    /// <summary>Back to the whole list: lets go of the budget's category, and of its slice.</summary>
    public void ShowAllCategories() => Release();

    // ------------------------------------------------------------------ periods

    /// <summary>Stepping lets go of the chosen slice, as the desktop's stepping lets go of what was pointed at.</summary>
    public void StepBack()
    {
        Release();
        App.StepBack();
    }

    public void StepForward()
    {
        Release();
        App.StepForward();
    }

    // ------------------------------------------------------------------ entries

    /// <summary>An expense tapped in the list: its form opens, filled in, to change or remove it.</summary>
    public void OpenEntry(ExpenseLine line)
    {
        App.EditExpense(line);
        GoTo(PhonePanel.Expenses, PanelStep.Second);
    }

    /// <summary>An income tapped in the list, as an expense.</summary>
    public void OpenEntry(IncomeLine line)
    {
        App.EditIncome(line);
        GoTo(PhonePanel.Income, PanelStep.Second);
    }

    /// <summary>
    /// The expense form's ＋ or ✓. A new expense that goes through closes the panel, so the ring is
    /// seen to change; a change that goes through, saved unchanged included, goes back to the list; a
    /// refusal keeps the form as it is.
    /// </summary>
    public void SubmitExpense()
    {
        var form = App.ExpenseForm;
        if (form.IsEditing)
        {
            if (form.Save() is { WasRefused: false }) GoTo(PhonePanel.Expenses, PanelStep.First);
        }
        else if (form.Record() is { WasRecorded: true })
            GoTo(PhonePanel.None, PanelStep.Closed);
    }

    /// <summary>The income form's ＋ or ✓, as <see cref="SubmitExpense"/>.</summary>
    public void SubmitIncome()
    {
        var form = App.IncomeForm;
        if (form.IsEditing)
        {
            if (form.Save() is { WasRefused: false }) GoTo(PhonePanel.Income, PanelStep.First);
        }
        else if (form.Record() is { WasRecorded: true })
            GoTo(PhonePanel.None, PanelStep.Closed);
    }

    /// <summary><i>Verwijderen</i> on an entry's form: asks first; once confirmed, the form goes back to its list.</summary>
    public void RemoveEntry()
    {
        switch (Open)
        {
            case PhonePanel.Expenses: App.ExpenseForm.Remove(); break;
            case PhonePanel.Income: App.IncomeForm.Remove(); break;
            default: throw new InvalidOperationException("Only an entry's form removes an entry.");
        }

        removingFromForm = true;
    }

    /// <summary>Yes to the question waiting.</summary>
    public void Confirm()
    {
        var fromForm = removingFromForm;
        removingFromForm = false;
        App.Confirm();

        var stillEditing = Open switch
        {
            PhonePanel.Expenses => App.ExpenseForm.IsEditing,
            PhonePanel.Income => App.IncomeForm.IsEditing,
            _ => true,
        };
        if (fromForm && Step == PanelStep.Second && !stillEditing) GoTo(Open, PanelStep.First);
    }

    /// <summary>No to the question waiting.</summary>
    public void Decline()
    {
        removingFromForm = false;
        App.Decline();
    }

    // ------------------------------------------------------------------ accounts

    /// <summary>An account tapped in the accounts panel: its history opens as the panel's second step.</summary>
    public void OpenHistory(Account account)
    {
        if (App.HistoryAccount != account) App.OpenHistory(account);
        GoTo(PhonePanel.Accounts, PanelStep.Second);
    }

    /// <summary>Deleting the account whose history is open: its history goes with it, back to the accounts.</summary>
    public void DeleteAccount(Account account)
    {
        App.DeleteAccount(account);
        GoTo(PhonePanel.Accounts, PanelStep.First);
    }

    // ------------------------------------------------------------------

    private Category? Find(string name) =>
        App.Ledger.CategoriesOffered.Concat(App.Ledger.ArchivedCategories).FirstOrDefault(c => c.Name == name);
}
