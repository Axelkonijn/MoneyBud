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

public sealed partial class ExpenseForm(MoneyBudApp app) : ObservableObject
{
    [ObservableProperty] public partial string? Amount { get; set; }
    [ObservableProperty] public partial string? Category { get; set; }
    [ObservableProperty] public partial string? Label { get; set; }
    [ObservableProperty] public partial DateTime? Date { get; set; }

    /// <summary>The expense being changed, or null while the form records a new one.</summary>
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
    }

    [RelayCommand]
    private void Submit()
    {
        if (IsEditing)
        {
            Save();
            return;
        }

        var result = app.RecordExpense(Amount, Category, Label, DateOf(Date));
        if (result is { WasRecorded: true }) Clear();
    }

    /// <summary>Saves the change to the expense being edited.</summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeExpenseResult? Save()
    {
        var expense = Editing ?? throw new InvalidOperationException("No expense is being changed.");

        var result = app.ChangeExpense(expense, Amount, Category, Label, DateOf(Date));
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
    }

    internal static DateOnly? DateOf(DateTime? date) => date is { } d ? DateOnly.FromDateTime(d) : null;
}

public sealed partial class IncomeForm(MoneyBudApp app) : ObservableObject
{
    [ObservableProperty] public partial string? Amount { get; set; }
    [ObservableProperty] public partial string? Label { get; set; }
    [ObservableProperty] public partial DateTime? Date { get; set; }

    /// <summary>The income being changed, or null while the form records a new one.</summary>
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
    }

    [RelayCommand]
    private void Submit()
    {
        if (IsEditing)
        {
            Save();
            return;
        }

        var result = app.RecordIncome(Amount, Label, ExpenseForm.DateOf(Date));
        if (result is { WasRecorded: true }) Clear();
    }

    /// <summary>Saves the change to the income being edited.</summary>
    /// <returns>The ledger's answer, or null when the amount could not be read as one.</returns>
    public ChangeIncomeResult? Save()
    {
        var income = Editing ?? throw new InvalidOperationException("No income is being changed.");

        var result = app.ChangeIncome(income, Amount, Label, ExpenseForm.DateOf(Date));
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
