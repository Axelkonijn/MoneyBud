using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoneyBud.Domain;

namespace MoneyBud.Presentation;

// The entry forms the Desktop binds to. Each holds what has been typed and hands it to
// MoneyBudApp, which does the act and says what came of it. A form clears itself once its entry
// has gone through, and keeps what was typed when it has not, so a refusal can be corrected in
// place rather than typed again.
//
// A date left empty means today, and an empty date is what every form starts with and returns
// to: the date does not follow the period on screen (arc42 §12).
//
// The expense and income forms have a second state, Wijzigen: clicking a row in a transaction list
// loads that entry into its form (arc42 §12, *On screen: picking an entry to correct*). The same
// fields are read by the same rules, and the submit button saves the change instead of recording.
// The form empties and returns to recording once the change goes through — saved unchanged
// included — or on Annuleren, or once the entry is removed; after a refusal it keeps what was
// typed and stays in Wijzigen, as a refused recording keeps what was typed.
//
// The expense and income forms have an account list as their last field (arc42 §12, *Accounts and
// net worth*). A list, never free text: an account cannot be made by typing its name. A new entry
// starts out on the pool account, and moves on with it when another account is made the pool; an
// entry being changed keeps its own. What a form holds is always a plain account: a list writes
// back what it shows, and writing back nothing — which it does while its items are replaced — is
// ignored. A headless run of the window found that anything cleverer, such as treating the pool
// written back as "not chosen", turned into choices the user never made.
//
// Since backing, the expense form's account also FOLLOWS THE CATEGORY TYPED: the backing account of
// a backed category, the pool account for anything else, until the user picks an account, which then
// sticks for that entry (arc42 §12, *What the backing increment covers, and what waits*). That needs
// one inference from the list after all, kept as small as it can be: a write of the account the list
// already shows is the list writing back, and changes nothing; a write of any other account is a
// pick. Picking the account already shown is indistinguishable from the list writing back, and leaves
// the account following the category.
//
// Since increment 15 the list is LOCKED for a backed category, on its backing account, from the period
// the category got it (arc42 §12, *An expense on a backed category is on its account*): the domain says
// which account (Ledger.LockedAccountFor), from the category typed and the date. While locked, a write
// to the list is ignored, as the Herhalen list's is; a pick made before is kept for when the list opens
// again, so a backed category typed in between does not erase it (derived). Only categories without an
// account keep a free list, which no longer follows the category: it shows the pool account until an
// account is picked.
//
// Since recurring entries, both forms end with a Herhalen list (arc42 §12, Recurring entries,
// follow-up 6): Eenmalig, the default, then Wekelijks and Maandelijks. It is locked on Eenmalig for an
// earlier occurrence of a repeat, which cannot change it (follow-up 2), and a form in Wijzigen reads
// that afresh on every redraw, so an entry that stops being the latest while it is open locks there
// (plan for increment 12, 4). The list writes back like the account list: a plain value, a write of
// nothing ignored, and a write while locked ignored too.

/// <summary>One item of the <i>Herhalen</i> list: a frequency, or none for <i>Eenmalig</i>.</summary>
public sealed record FrequencyChoice(Frequency? Frequency)
{
    public string Text => Tekst.FrequencyName(Frequency);

    /// <summary>The list, in the ruled order: <i>Eenmalig</i>, <i>Wekelijks</i>, <i>Maandelijks</i> (§12, ruled at the scenario stage, 2).</summary>
    public static IReadOnlyList<FrequencyChoice> All { get; } =
        [new((Frequency?)null), new(Domain.Frequency.Weekly), new(Domain.Frequency.Monthly)];

    public override string ToString() => Text;
}

public sealed partial class ExpenseForm(MoneyBudApp app) : ObservableObject
{
    [ObservableProperty] public partial string? Amount { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChosenAccount), nameof(IsAccountLocked))]
    public partial string? Category { get; set; }

    [ObservableProperty] public partial string? Label { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChosenAccount), nameof(IsAccountLocked))]
    public partial DateTime? Date { get; set; }

    /// <summary>
    /// The account the user picked, or the one the entry being changed is on. Null until then, when
    /// the entry is on the pool account, or on the account its category's list is locked on.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChosenAccount))]
    public partial Account? Account { get; set; }

    /// <summary>
    /// The account the list is locked on, or null while it is open: the backing account of the
    /// category typed, from the period it got it, or for an entry being changed that was recorded
    /// before that, the account it is on (plan for increment 15, reading 3).
    /// </summary>
    private Account? Locked =>
        app.Ledger.LockedAccountFor(Category, DateOf(Date) ?? app.Ledger.Today, Editing);

    /// <summary>Whether the account list is locked, so cannot be changed.</summary>
    public bool IsAccountLocked => Locked is not null;

    /// <summary>
    /// What the account list shows and sets: the account it is locked on; otherwise the account
    /// picked, and the pool account until one is. Setting nothing, the account already shown, or
    /// anything while locked, changes nothing; setting another account picks it.
    /// </summary>
    public Account ChosenAccount
    {
        get => Locked ?? Account ?? app.Ledger.PoolAccount;
        set
        {
            if (value is not null && !IsAccountLocked && value != ChosenAccount) Account = value;
        }
    }

    internal void RefreshAccount()
    {
        OnPropertyChanged(nameof(IsAccountLocked));
        OnPropertyChanged(nameof(ChosenAccount));
    }

    /// <summary>
    /// Another account was made the pool. A new entry whose account was picked as the old pool moves
    /// on to the new one, as a new entry starts out on the pool account; one following its category
    /// follows on by itself; an entry being changed keeps its own.
    /// </summary>
    internal void PoolChanged(Account oldPool)
    {
        if (!IsEditing && Account == oldPool) Account = null;
    }

    /// <summary>An account deleted while chosen here: the form goes back to following its category.</summary>
    internal void Forget(Account deleted)
    {
        if (Account == deleted) Account = null;
    }

    /// <summary>What the <i>Herhalen</i> list is set to: null, <i>Eenmalig</i>, until another is chosen or loaded.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChosenFrequency))]
    public partial Frequency? Frequency { get; set; }

    public IReadOnlyList<FrequencyChoice> FrequencyChoices => FrequencyChoice.All;

    /// <summary>Whether the list can be changed: not on an earlier occurrence of a repeat (§12, follow-up 2).</summary>
    public bool CanChangeFrequency => Editing is not { } expense || app.Ledger.SetsTheRepeat(expense);

    /// <summary>What the list shows and sets: <i>Eenmalig</i> while locked. Setting nothing, or anything while locked, changes nothing.</summary>
    public FrequencyChoice ChosenFrequency
    {
        get => new(CanChangeFrequency ? Frequency : null);
        set
        {
            if (value is not null && CanChangeFrequency) Frequency = value.Frequency;
        }
    }

    internal void RefreshFrequency()
    {
        OnPropertyChanged(nameof(CanChangeFrequency));
        OnPropertyChanged(nameof(ChosenFrequency));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing), nameof(SubmitText), nameof(CanChangeFrequency), nameof(ChosenFrequency),
        nameof(ChosenAccount), nameof(IsAccountLocked))]
    public partial Expense? Editing { get; private set; }

    public bool IsEditing => Editing is not null;

    public string SubmitText => IsEditing ? Tekst.Save : Tekst.RecordExpense;

    /// <summary>
    /// Loads an expense to be changed or removed, each field as it would be typed: the amount in
    /// the form the amount box reads back (<see cref="AmountInput.Format"/>), so that saving it
    /// unchanged cannot be refused.
    /// </summary>
    public void Load(Expense expense)
    {
        Editing = expense;
        Label = expense.Label;
        Category = expense.Category.Name;
        Amount = AmountInput.Format(expense.Amount);
        Date = expense.Date.ToDateTime(TimeOnly.MinValue);
        Account = expense.Account;
        Frequency = app.Ledger.FrequencyOf(expense);
    }

    [RelayCommand]
    private void Submit()
    {
        if (IsEditing)
        {
            Save();
            return;
        }

        Record();
    }

    /// <returns>What came of recording, or null when the amount could not be read.</returns>
    public RecordExpenseResult? Record()
    {
        var result = app.RecordExpense(Amount, Category, Label, DateOf(Date), ChosenAccount, Frequency);
        if (result is { WasRecorded: true }) Clear();
        return result;
    }

    /// <summary>Saves the change to the expense being edited.</summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeExpenseResult? Save()
    {
        var expense = Editing ?? throw new InvalidOperationException("No expense is being changed.");

        var result = app.ChangeExpense(expense, Amount, Category, Label, DateOf(Date), ChosenAccount, ChosenFrequency.Frequency);
        if (result is { WasRefused: false }) Clear();
        return result;
    }

    /// <summary>Verwijderen: asks first, and removes only once the user confirms.</summary>
    [RelayCommand]
    public void Remove() =>
        app.AskToRemove(Editing ?? throw new InvalidOperationException("No expense is being changed."));

    [RelayCommand]
    public void Cancel()
    {
        app.Decline();
        Clear();
    }

    /// <summary>Empties the form and returns it to recording a new expense.</summary>
    public void Clear()
    {
        Editing = null;
        Amount = null;
        Category = null;
        Label = null;
        Date = null;
        Account = null;
        Frequency = null;
    }

    internal static DateOnly? DateOf(DateTime? date) => date is { } d ? DateOnly.FromDateTime(d) : null;
}

public sealed partial class IncomeForm(MoneyBudApp app) : ObservableObject
{
    [ObservableProperty] public partial string? Amount { get; set; }
    [ObservableProperty] public partial string? Label { get; set; }
    [ObservableProperty] public partial DateTime? Date { get; set; }

    /// <summary>The account the entry is on. Null until one is chosen or loaded, which is the pool account.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChosenAccount))]
    public partial Account? Account { get; set; }

    /// <summary>What the account list shows and sets. Setting nothing changes nothing.</summary>
    public Account ChosenAccount
    {
        get => Account ?? app.Ledger.PoolAccount;
        set
        {
            if (value is not null) Account = value;
        }
    }

    internal void RefreshAccount() => OnPropertyChanged(nameof(ChosenAccount));

    /// <summary>
    /// Another account was made the pool. A new entry still on the old one moves on to the new one,
    /// as a new entry starts out on the pool account; an entry being changed keeps its own.
    /// </summary>
    internal void PoolChanged(Account oldPool)
    {
        if (!IsEditing && ChosenAccount == oldPool) Account = null;
    }

    /// <summary>An account deleted while chosen here: the form goes back to the pool account.</summary>
    internal void Forget(Account deleted)
    {
        if (Account == deleted) Account = null;
    }

    /// <summary>What the <i>Herhalen</i> list is set to, as on the expense form.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChosenFrequency))]
    public partial Frequency? Frequency { get; set; }

    public IReadOnlyList<FrequencyChoice> FrequencyChoices => FrequencyChoice.All;

    /// <summary>Whether the list can be changed, as on the expense form.</summary>
    public bool CanChangeFrequency => Editing is not { } income || app.Ledger.SetsTheRepeat(income);

    /// <summary>What the list shows and sets, as on the expense form.</summary>
    public FrequencyChoice ChosenFrequency
    {
        get => new(CanChangeFrequency ? Frequency : null);
        set
        {
            if (value is not null && CanChangeFrequency) Frequency = value.Frequency;
        }
    }

    internal void RefreshFrequency()
    {
        OnPropertyChanged(nameof(CanChangeFrequency));
        OnPropertyChanged(nameof(ChosenFrequency));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing), nameof(SubmitText), nameof(CanChangeFrequency), nameof(ChosenFrequency))]
    public partial Income? Editing { get; private set; }

    public bool IsEditing => Editing is not null;

    public string SubmitText => IsEditing ? Tekst.Save : Tekst.RecordIncome;

    /// <summary>Loads an income to be changed or removed, as <see cref="ExpenseForm.Load"/> does.</summary>
    public void Load(Income income)
    {
        Editing = income;
        Label = income.Label;
        Amount = AmountInput.Format(income.Amount);
        Date = income.Date.ToDateTime(TimeOnly.MinValue);
        Account = income.Account;
        Frequency = app.Ledger.FrequencyOf(income);
    }

    [RelayCommand]
    private void Submit()
    {
        if (IsEditing)
        {
            Save();
            return;
        }

        Record();
    }

    /// <returns>What came of recording, or null when the amount could not be read.</returns>
    public RecordIncomeResult? Record()
    {
        var result = app.RecordIncome(Amount, Label, ExpenseForm.DateOf(Date), ChosenAccount, Frequency);
        if (result is { WasRecorded: true }) Clear();
        return result;
    }

    /// <summary>Saves the change to the income being edited.</summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeIncomeResult? Save()
    {
        var income = Editing ?? throw new InvalidOperationException("No income is being changed.");

        var result = app.ChangeIncome(income, Amount, Label, ExpenseForm.DateOf(Date), ChosenAccount, ChosenFrequency.Frequency);
        if (result is { WasRefused: false }) Clear();
        return result;
    }

    /// <summary>Verwijderen: asks first, and removes only once the user confirms.</summary>
    [RelayCommand]
    public void Remove() =>
        app.AskToRemove(Editing ?? throw new InvalidOperationException("No income is being changed."));

    [RelayCommand]
    public void Cancel()
    {
        app.Decline();
        Clear();
    }

    /// <summary>Empties the form and returns it to recording a new income.</summary>
    public void Clear()
    {
        Editing = null;
        Amount = null;
        Label = null;
        Date = null;
        Account = null;
        Frequency = null;
    }
}

/// <summary>
/// Assigning names its period, which starts out as the period on screen and follows it when the
/// screen steps (§12). It can be moved to another period here without moving the screen, which is
/// how an assignment lands somewhere other than the period shown.
/// </summary>
public sealed partial class AssignForm : ObservableObject
{
    private readonly MoneyBudApp app;

    public AssignForm(MoneyBudApp app)
    {
        this.app = app;
        Period = app.ShownPeriod;
    }

    [ObservableProperty] public partial string? Amount { get; set; }
    [ObservableProperty] public partial string? Category { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PeriodTitle))]
    public partial BudgetPeriod Period { get; set; }

    public string PeriodTitle => Tekst.PeriodName(Period);

    [RelayCommand]
    private void EarlierPeriod() => Period = app.Ledger.Calendar.Previous(Period);

    [RelayCommand]
    private void LaterPeriod() => Period = app.Ledger.Calendar.Next(Period);

    [RelayCommand]
    private void Submit()
    {
        var result = app.Assign(Amount, Category, Period);
        if (result is not { WasAssigned: true }) return;

        Amount = null;
        Category = null;
    }
}

public sealed partial class CategoryForm(MoneyBudApp app) : ObservableObject
{
    [ObservableProperty] public partial string? Name { get; set; }

    [RelayCommand]
    private void Submit()
    {
        if (!app.AddCategory(Name).WasRefused) Name = null;
    }
}

/// <summary>
/// Adding an account: a name and a starting balance (arc42 §12, <i>Accounts and net worth</i>). The
/// starting balance may be left empty, which is no starting balance.
/// </summary>
public sealed partial class AccountForm(MoneyBudApp app) : ObservableObject
{
    [ObservableProperty] public partial string? Name { get; set; }
    [ObservableProperty] public partial string? StartingBalance { get; set; }

    /// <summary>Whether the form is open in the strip. <i>Rekening toevoegen</i> opens it.</summary>
    [ObservableProperty] public partial bool IsOpen { get; set; }

    [RelayCommand]
    private void Open() => IsOpen = true;

    [RelayCommand]
    public void Cancel()
    {
        IsOpen = false;
        Name = null;
        StartingBalance = null;
    }

    [RelayCommand]
    public void Submit()
    {
        if (app.AddAccount(Name, StartingBalance) is { WasRefused: false }) Cancel();
    }
}

/// <summary>
/// Recording a transfer, <i>Overboeken</i>: Van, Naar, Bedrag, Datum. Van starts out on the pool
/// account and Naar on the first other account, which with only one account is the same one — and a
/// transfer from an account to itself is refused. A date left empty is today.
///
/// <para>It has a <i>Wijzigen</i> state too, as the entry forms do: a transfer's row in an account's
/// history loads it here, to be changed or removed.</para>
/// </summary>
public sealed partial class TransferForm(MoneyBudApp app) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChosenFrom), nameof(ChosenTo))]
    public partial Account? From { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChosenTo))]
    public partial Account? To { get; private set; }

    [ObservableProperty] public partial string? Amount { get; set; }
    [ObservableProperty] public partial DateTime? Date { get; set; }

    /// <summary>Whether the form is open. <i>Overboeken</i> opens it, and so does loading a transfer.</summary>
    [ObservableProperty] public partial bool IsOpen { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing), nameof(SubmitText))]
    public partial Transfer? Editing { get; private set; }

    public bool IsEditing => Editing is not null;

    public string SubmitText => IsEditing ? Tekst.Save : Tekst.TransferAct;

    /// <summary>
    /// Van, which starts out on the pool account. Like an entry form's account it is always a plain
    /// account once set, and setting nothing changes nothing.
    /// </summary>
    public Account ChosenFrom
    {
        get => From ?? app.Ledger.PoolAccount;
        set
        {
            if (value is not null) From = value;
        }
    }

    /// <summary>Naar, which starts out on the first account that is not Van.</summary>
    public Account ChosenTo
    {
        get => To ?? app.Ledger.Accounts.FirstOrDefault(a => a != ChosenFrom) ?? ChosenFrom;
        set
        {
            if (value is not null) To = value;
        }
    }

    internal void RefreshAccounts()
    {
        OnPropertyChanged(nameof(ChosenFrom));
        OnPropertyChanged(nameof(ChosenTo));
    }

    /// <summary>
    /// The accounts changed: one was added, or another made the pool. A new transfer whose Van is
    /// still the old pool account moves on with it, and one whose Naar is its Van — as it must be
    /// while there is only one account — starts out on the first other account again. A transfer
    /// being changed keeps its own two.
    /// </summary>
    internal void AccountsChanged(Account? oldPool)
    {
        if (IsEditing) return;
        if (oldPool is not null && ChosenFrom == oldPool) From = null;
        if (ChosenTo == ChosenFrom) To = null;
    }

    internal void Forget(Account deleted)
    {
        if (From == deleted) From = null;
        if (To == deleted) To = null;
    }

    [RelayCommand]
    private void Open() => IsOpen = true;

    public void Load(Transfer transfer)
    {
        Editing = transfer;
        From = transfer.From;
        To = transfer.To;
        Amount = AmountInput.Format(transfer.Amount);
        Date = transfer.Date.ToDateTime(TimeOnly.MinValue);
        IsOpen = true;
    }

    [RelayCommand]
    private void Submit()
    {
        if (IsEditing) Save();
        else Record();
    }

    /// <returns>What came of recording, or null when the amount could not be read.</returns>
    public RecordTransferResult? Record()
    {
        var result = app.RecordTransfer(Amount, ChosenFrom, ChosenTo, ExpenseForm.DateOf(Date));
        if (result is { WasRecorded: true }) Clear();
        return result;
    }

    /// <returns>What came of saving the change, or null when the amount could not be read.</returns>
    public ChangeTransferResult? Save()
    {
        var transfer = Editing ?? throw new InvalidOperationException("No transfer is being changed.");

        var result = app.ChangeTransfer(transfer, Amount, ChosenFrom, ChosenTo, ExpenseForm.DateOf(Date));
        if (result is { WasRefused: false }) Clear();
        return result;
    }

    [RelayCommand]
    public void Remove() =>
        app.AskToRemove(Editing ?? throw new InvalidOperationException("No transfer is being changed."));

    [RelayCommand]
    public void Cancel()
    {
        app.Decline();
        Clear();
    }

    public void Clear()
    {
        Editing = null;
        From = null;
        To = null;
        Amount = null;
        Date = null;
        IsOpen = false;
    }
}

/// <summary>One item of the <i>Van</i> or <i>Naar</i> list of <i>Verplaatsen</i>: an end, by the name it shows.</summary>
public sealed record ReallocationChoice(ReallocationEnd End)
{
    public string Text => Tekst.EndName(End);

    public override string ToString() => Text;
}

/// <summary>
/// <i>Verplaatsen</i> (arc42 §12, <i>One act moves an amount of purpose</i>, follow-up 12): <i>Van</i>,
/// <i>Naar</i> and <i>Bedrag</i>. Opened beside <i>Overboeken</i>, or from a category's row, which
/// starts it with that category as <i>Van</i> (derived). Nothing is confirmed first; the form empties
/// once the move goes through, and keeps what was typed after a refusal.
///
/// <para>The lists, in the order of the plan for increment 15, reading 5: <i>Vrij</i> on each account
/// but the pool account, in the strip's order, then the categories alphabetically, then — in
/// <i>Naar</i> only — <i>Niet toegewezen</i>, which means the period on screen. Like every list here, a
/// choice is a plain value once set, and a list writing back nothing changes nothing. The lists are
/// the same collections for as long as what they offer stays the same, so a redraw does not make a
/// list let go of its choice.</para>
/// </summary>
public sealed partial class ReallocateForm(MoneyBudApp app) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChosenFrom), nameof(ChosenTo))]
    public partial ReallocationEnd? From { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChosenTo))]
    public partial ReallocationEnd? To { get; private set; }

    [ObservableProperty] public partial string? Amount { get; set; }

    /// <summary>Whether the form is open: <i>Verplaatsen</i> opens it, and so does a row's button.</summary>
    [ObservableProperty] public partial bool IsOpen { get; set; }

    private IReadOnlyList<ReallocationChoice> fromChoices = [];
    private IReadOnlyList<ReallocationChoice> toChoices = [];

    /// <summary>What <i>Van</i> offers.</summary>
    public IReadOnlyList<ReallocationChoice> FromChoices => fromChoices = Same(fromChoices, Ordered(app.Ledger.ReallocationSources));

    /// <summary>What <i>Naar</i> offers.</summary>
    public IReadOnlyList<ReallocationChoice> ToChoices => toChoices = Same(toChoices, Ordered(app.Ledger.ReallocationDestinations));

    /// <summary><i>Van</i>, which starts out on the first choice.</summary>
    public ReallocationChoice? ChosenFrom
    {
        get => FromChoices.FirstOrDefault(c => c.End == From) ?? FromChoices.FirstOrDefault();
        set
        {
            if (value is not null) From = value.End;
        }
    }

    /// <summary><i>Naar</i>, which starts out on the first choice that is not <i>Van</i>.</summary>
    public ReallocationChoice? ChosenTo
    {
        get => ToChoices.FirstOrDefault(c => c.End == To) ?? ToChoices.FirstOrDefault(c => c.End != ChosenFrom?.End);
        set
        {
            if (value is not null) To = value.End;
        }
    }

    internal void RefreshChoices()
    {
        OnPropertyChanged(nameof(FromChoices));
        OnPropertyChanged(nameof(ToChoices));
        OnPropertyChanged(nameof(ChosenFrom));
        OnPropertyChanged(nameof(ChosenTo));
        OnPropertyChanged(nameof(HasEnds));
    }

    /// <summary>
    /// Whether <i>Verplaatsen</i> is offered: there is something to take from — an account besides the
    /// pool, or a category with an account or money left behind. A first start has neither.
    /// </summary>
    public bool HasEnds => FromChoices.Count > 0;

    [RelayCommand]
    private void Open() => IsOpen = HasEnds;

    /// <summary>Opened from a category's row: <i>Van</i> is that category (derived).</summary>
    [RelayCommand]
    public void StartFrom(string category)
    {
        app.Decline();
        Clear();
        From = ReallocationEnd.For(app.Ledger.CategoriesOffered.Concat(app.Ledger.ArchivedCategories)
            .Single(c => c.Name == category));
        IsOpen = true;
    }

    [RelayCommand]
    private void Submit() => Reallocate();

    /// <returns>What came of it, or null when the amount could not be read or, with the form open
    /// as its last end went away, there was nothing to move between, which closes it.</returns>
    public ReallocateResult? Reallocate()
    {
        if (ChosenFrom is not { } from || ChosenTo is not { } to)
        {
            Clear();
            return null;
        }

        var result = app.Reallocate(Amount, from.End, to.End);
        if (result is { WasRefused: false }) Clear();
        return result;
    }

    [RelayCommand]
    public void Cancel()
    {
        app.Decline();
        Clear();
    }

    public void Clear()
    {
        From = null;
        To = null;
        Amount = null;
        IsOpen = false;
    }

    private static List<ReallocationChoice> Ordered(IReadOnlyList<ReallocationEnd> ends) =>
    [
        .. ends.Where(e => e.Kind == ReallocationEndKind.Unclaimed).Select(e => new ReallocationChoice(e)),
        .. ends.Where(e => e.Kind == ReallocationEndKind.Category)
            .OrderBy(e => e.Category!.Name, MoneyBudApp.Alphabetical).Select(e => new ReallocationChoice(e)),
        .. ends.Where(e => e.Kind == ReallocationEndKind.Unassigned).Select(e => new ReallocationChoice(e)),
    ];

    // The list already held, while it offers the same ends under the same names.
    private static IReadOnlyList<ReallocationChoice> Same(IReadOnlyList<ReallocationChoice> held, List<ReallocationChoice> wanted) =>
        held.Select(c => (c.End, c.Text)).SequenceEqual(wanted.Select(c => (c.End, c.Text))) ? held : wanted;
}
