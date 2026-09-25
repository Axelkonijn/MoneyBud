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

public sealed partial class ExpenseForm(MoneyBudApp app) : ObservableObject
{
    [ObservableProperty] public partial string? Amount { get; set; }
    [ObservableProperty] public partial string? Category { get; set; }
    [ObservableProperty] public partial string? Label { get; set; }
    [ObservableProperty] public partial DateTime? Date { get; set; }

    [RelayCommand]
    private void Submit()
    {
        var result = app.RecordExpense(Amount, Category, Label, DateOf(Date));
        if (result is not { WasRecorded: true }) return;

        Amount = null;
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

    [RelayCommand]
    private void Submit()
    {
        var result = app.RecordIncome(Amount, Label, ExpenseForm.DateOf(Date));
        if (result is not { WasRecorded: true }) return;

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
