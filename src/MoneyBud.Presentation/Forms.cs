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

public sealed partial class ExpenseForm(MoneyBudApp app) : ObservableObject
{
    [ObservableProperty] public partial string? Amount { get; set; }
    [ObservableProperty] public partial string? Category { get; set; }
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing), nameof(SubmitText))]
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
        var result = app.RecordExpense(Amount, Category, Label, DateOf(Date), ChosenAccount);
        if (result is { WasRecorded: true }) Clear();
        return result;
    }

    /// <summary>Saves the change to the expense being edited.</summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeExpenseResult? Save()
    {
        var expense = Editing ?? throw new InvalidOperationException("No expense is being changed.");

        var result = app.ChangeExpense(expense, Amount, Category, Label, DateOf(Date), ChosenAccount);
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing), nameof(SubmitText))]
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
        var result = app.RecordIncome(Amount, Label, ExpenseForm.DateOf(Date), ChosenAccount);
        if (result is { WasRecorded: true }) Clear();
        return result;
    }

    /// <summary>Saves the change to the income being edited.</summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeIncomeResult? Save()
    {
        var income = Editing ?? throw new InvalidOperationException("No income is being changed.");

        var result = app.ChangeIncome(income, Amount, Label, ExpenseForm.DateOf(Date), ChosenAccount);
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
