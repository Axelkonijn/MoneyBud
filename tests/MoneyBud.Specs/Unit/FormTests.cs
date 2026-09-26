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
        Assert.Null(form.Category);
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
        Assert.Equal("Boodschappen", form.Category);
        Assert.True(app.Notice!.IsRefusal);
    }

    // The category box empties with the rest of its form, but only once the act went through
    // (arc42 §12, first demo).
    [Fact]
    public void An_assignment_that_goes_through_empties_the_category_box()
    {
        var form = app.AssignForm;
        form.Amount = "50";
        form.Category = "hobby";

        form.SubmitCommand.Execute(null);

        Assert.Equal(Money.FromCents(5000), app.Ledger.BudgetFor("Hobby", app.ShownPeriod));
        Assert.Null(form.Amount);
        Assert.Null(form.Category);
    }

    [Fact]
    public void A_refused_assignment_keeps_its_category_to_be_corrected()
    {
        var form = app.AssignForm;
        form.Amount = "50";
        form.Category = "Hobbie";

        form.SubmitCommand.Execute(null);

        Assert.True(app.Notice!.IsRefusal);
        Assert.Equal("50", form.Amount);
        Assert.Equal("Hobbie", form.Category);
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
    public void Two_thousand_written_the_Dutch_way_is_refused_as_ambiguous_not_recorded_as_two()
    {
        Assert.Null(app.RecordIncome("2.000", "Salaris"));

        Assert.Empty(app.Overview.Incomes);
        Assert.True(app.Notice!.IsRefusal);
        Assert.Contains("2000", app.Notice.Text);
        Assert.Contains("2,00", app.Notice.Text);
    }

    [Fact]
    public void Suggestions_are_alphabetical_with_accented_names_among_their_letters()
    {
        app.AddCategory("Zorg");
        app.AddCategory("Één keer");

        Assert.Equal(
            ["Abonnementen", "Boodschappen", "Één keer", "Hobby", "Huur", "Sparen", "Verzekeringen", "Zorg"],
            app.CategorySuggestions);
    }

    // Narrowing on "contains" (arc42 §12), decided here rather than by the toolkit's own filter.
    [Theory]
    [InlineData("schap", new[] { "Boodschappen" })]
    [InlineData("HU", new[] { "Huur" })]
    [InlineData("  ren ", new[] { "Sparen" })]
    [InlineData("", new[] { "Abonnementen", "Boodschappen", "Hobby", "Huur", "Sparen", "Verzekeringen" })]
    [InlineData("xyz", new string[0])]
    public void Suggestions_narrow_to_the_names_that_contain_what_was_typed(string typed, string[] left) =>
        Assert.Equal(left, app.SuggestionsFor(typed));

    [Fact]
    public void Narrowing_counts_a_run_of_spaces_as_one_like_the_name_rule()
    {
        app.AddCategory("Vaste lasten");

        Assert.Equal(["Vaste lasten"], app.SuggestionsFor("vaste   las"));
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
