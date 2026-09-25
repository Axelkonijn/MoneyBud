using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// The entry forms the Desktop binds to. The scenarios act on <see cref="MoneyBudApp"/> directly;
/// these check the thin layer on top — that a form hands over what was typed, clears once the
/// entry went through, and keeps it when it was refused, so it can be corrected in place.
/// </summary>
public sealed class FormTests
{
    private readonly MoneyBudApp app = new(Ledger.StartNew(new FixedClock(new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero))));

    [Fact]
    public void An_expense_typed_the_Dutch_way_is_recorded_today_and_the_form_clears()
    {
        var form = app.ExpenseForm;
        form.Amount = "12,50";
        form.Category = "boodschappen";
        form.Label = "Markt";

        form.SubmitCommand.Execute(null);

        var expense = Assert.Single(app.Overview.Expenses);
        Assert.Equal((new DateOnly(2026, 3, 15), "Boodschappen", "Markt", Money.FromCents(1250)),
            (expense.Date, expense.Category, expense.Label, expense.Amount));
        Assert.Null(form.Amount);
        Assert.Null(form.Label);
        Assert.False(app.Notice!.IsRefusal);
    }

    [Fact]
    public void A_refused_expense_keeps_what_was_typed()
    {
        var form = app.ExpenseForm;
        form.Amount = "12,345";
        form.Category = "Boodschappen";

        form.SubmitCommand.Execute(null);

        Assert.Empty(app.Overview.Expenses);
        Assert.Equal("12,345", form.Amount);
        Assert.True(app.Notice!.IsRefusal);
    }

    [Fact]
    public void Text_that_is_not_an_amount_is_refused_before_the_ledger_is_asked()
    {
        var result = app.RecordIncome("twaalf", "Salaris");

        Assert.Null(result);
        Assert.Empty(app.Overview.Incomes);
        Assert.True(app.Notice!.IsRefusal);
        Assert.Contains("twaalf", app.Notice.Text);
    }

    [Fact]
    public void The_assign_form_follows_the_screen_and_can_be_moved_without_moving_it()
    {
        var current = app.ShownPeriod;
        app.StepForward();
        Assert.Equal(app.ShownPeriod, app.AssignForm.Period);

        app.AssignForm.EarlierPeriodCommand.Execute(null);

        Assert.Equal(current, app.AssignForm.Period);
        Assert.NotEqual(current, app.ShownPeriod);
    }

    [Fact]
    public void Stepping_clears_what_was_said_about_the_last_act()
    {
        app.AddCategory("Vakantie");
        Assert.NotNull(app.Notice);

        app.StepBack();

        Assert.Null(app.Notice);
    }
}
