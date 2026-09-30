using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// The phone's own screen rules (plan for increment 14, D2): the slice that stays chosen, what the
/// budget panel is about, the back button, and where an act leaves a panel. Window behaviour of the
/// phone, held here rather than in scenarios, as the desktop's field order is.
///
/// <para>The ring they work on: income 2000; Huur 800 from 0 to 0.4, Boodschappen 400 from 0.4 to 0.6,
/// and Niet toegewezen 800 from 0.6 to 1.</para>
/// </summary>
public sealed class PhoneScreenTests
{
    private readonly MoneyBudApp app = new(Ledger.StartNew(new FixedClock(new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero))));
    private readonly PhoneScreen screen;

    public PhoneScreenTests()
    {
        app.RecordIncome("2000", "Salaris");
        app.Assign("800", "Huur");
        app.Assign("400", "Boodschappen");
        screen = new PhoneScreen(app);
    }

    // ------------------------------------------------------------------ the chosen slice

    [Fact]
    public void A_slice_touched_stays_chosen_when_the_finger_lifts()
    {
        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);

        Assert.Equal("Boodschappen", screen.ChosenSlice?.Category);
        Assert.Equal("Boodschappen", app.PointedRow?.Name);
    }

    [Fact]
    public void Sliding_chooses_each_slice_passed_and_keeps_the_last()
    {
        screen.TouchRing(0.1);
        screen.SlideOnRing(0.5);
        screen.SlideOnRing(0.7);
        screen.LiftFromRing(0.7, slid: true);

        Assert.True(screen.ChosenSlice?.IsUnassigned);
    }

    [Fact]
    public void A_tap_on_the_slice_already_chosen_lets_go()
    {
        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);

        screen.TouchRing(0.55);
        screen.LiftFromRing(0.55, slid: false);

        Assert.Null(screen.ChosenSlice);
        Assert.Null(app.PointedSlice);
        Assert.True(app.RingCentreShowsUnassigned);
    }

    [Fact]
    public void A_tap_on_another_slice_chooses_that_one()
    {
        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);

        screen.TouchRing(0.1);
        screen.LiftFromRing(0.1, slid: false);

        Assert.Equal("Huur", screen.ChosenSlice?.Category);
    }

    [Fact]
    public void Sliding_back_onto_the_slice_already_chosen_does_not_let_go()
    {
        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);

        screen.TouchRing(0.5);
        screen.SlideOnRing(0.1);
        screen.SlideOnRing(0.5);
        screen.LiftFromRing(0.5, slid: true);

        Assert.Equal("Boodschappen", screen.ChosenSlice?.Category);
    }

    [Fact]
    public void A_tap_on_the_centre_lets_go()
    {
        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);

        screen.TouchRing(null);
        screen.LiftFromRing(null, slid: false);

        Assert.Null(screen.ChosenSlice);
    }

    [Fact]
    public void A_tap_elsewhere_on_the_home_screen_lets_go()
    {
        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);

        screen.TapHome();

        Assert.Null(screen.ChosenSlice);
        Assert.Null(app.PointedRow);
    }

    [Fact]
    public void The_choice_stays_on_its_category_when_the_slices_reorder()
    {
        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);

        // Boodschappen becomes the largest, so the first slice: where it was is now Huur's.
        app.Assign("600", "Boodschappen");

        Assert.Equal("Boodschappen", screen.ChosenSlice?.Category);
        Assert.Equal("Boodschappen", app.PointedRow?.Name);
    }

    [Fact]
    public void The_choice_follows_its_category_through_a_rename()
    {
        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);

        app.StartRename("Boodschappen");
        app.NewName = "Eten";
        app.SaveRename();

        Assert.Equal("Eten", screen.ChosenSlice?.Category);
        Assert.Equal("Eten", screen.BudgetCategory?.Name);
    }

    [Fact]
    public void A_slice_that_goes_away_is_let_go_but_the_budget_stays_on_its_category()
    {
        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);
        screen.Pull(PhonePanel.Budget);
        screen.Settled(PhonePanel.Budget, PanelStep.First);

        app.Assign("-400", "Boodschappen");

        Assert.Null(screen.ChosenSlice);
        Assert.Equal("Boodschappen", screen.BudgetCategory?.Name);
    }

    [Fact]
    public void Stepping_to_another_period_lets_go()
    {
        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);

        screen.StepBack();

        Assert.Null(screen.ChosenSlice);
        Assert.Null(screen.BudgetCategory);
    }

    // ------------------------------------------------------------------ the budget panel

    [Fact]
    public void The_budget_opens_on_the_category_chosen_on_the_ring()
    {
        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);

        Assert.Equal("Boodschappen", screen.BudgetCategory?.Name);
    }

    [Fact]
    public void With_Unassigned_chosen_the_budget_shows_the_whole_list()
    {
        screen.TouchRing(0.8);
        screen.LiftFromRing(0.8, slid: false);

        Assert.True(screen.ChosenSlice?.IsUnassigned);
        Assert.Null(screen.BudgetCategory);
    }

    [Fact]
    public void A_row_tapped_opens_the_budget_on_it_and_chooses_its_slice()
    {
        screen.OpenCategory("Huur");

        Assert.Equal(PhonePanel.Budget, screen.Open);
        Assert.Equal("Huur", screen.BudgetCategory?.Name);
        Assert.Equal("Huur", screen.ChosenSlice?.Category);
    }

    [Fact]
    public void A_row_with_no_slice_opens_the_budget_on_it_with_nothing_chosen_on_the_ring()
    {
        screen.OpenCategory("Hobby");

        Assert.Equal("Hobby", screen.BudgetCategory?.Name);
        Assert.Null(screen.ChosenSlice);
        Assert.Null(app.PointedSlice);
    }

    [Fact]
    public void Closing_the_budget_lets_go_of_a_row_it_was_opened_on_but_not_of_a_slice_still_chosen()
    {
        screen.OpenCategory("Hobby");
        screen.Settled(PhonePanel.Budget, PanelStep.First);
        screen.Settled(PhonePanel.Budget, PanelStep.Closed);
        Assert.Null(screen.BudgetCategory);

        screen.TouchRing(0.5);
        screen.LiftFromRing(0.5, slid: false);
        screen.Pull(PhonePanel.Budget);
        screen.Settled(PhonePanel.Budget, PanelStep.First);
        screen.Settled(PhonePanel.Budget, PanelStep.Closed);
        Assert.Equal("Boodschappen", screen.BudgetCategory?.Name);
    }

    // ------------------------------------------------------------------ the back button

    [Fact]
    public void Back_on_the_home_screen_is_left_to_Android()
    {
        Assert.False(screen.Back());
        Assert.Equal(PhonePanel.None, screen.Open);
    }

    [Fact]
    public void Back_first_lets_go_of_the_budgets_category_and_then_closes_the_budget()
    {
        screen.OpenCategory("Huur");
        screen.Settled(PhonePanel.Budget, PanelStep.First);

        Assert.True(screen.Back());
        Assert.Null(screen.BudgetCategory);
        Assert.Null(screen.ChosenSlice);
        Assert.Equal(PhonePanel.Budget, screen.Open);

        Assert.True(screen.Back());
        Assert.Equal(PhonePanel.None, screen.Open);
    }

    [Fact]
    public void Back_goes_from_a_form_to_its_list_and_then_closes_the_panel()
    {
        screen.Pull(PhonePanel.Expenses);
        screen.Settled(PhonePanel.Expenses, PanelStep.First);
        screen.Settled(PhonePanel.Expenses, PanelStep.Second);

        Assert.True(screen.Back());
        Assert.Equal((PhonePanel.Expenses, PanelStep.First), (screen.Open, screen.Step));

        Assert.True(screen.Back());
        Assert.Equal((PhonePanel.None, PanelStep.Closed), (screen.Open, screen.Step));
    }

    [Fact]
    public void Back_says_no_to_a_question_before_anything_else()
    {
        screen.OpenEntry(app.Overview.Incomes.Single());
        screen.RemoveEntry();
        Assert.True(app.IsAsking);

        Assert.True(screen.Back());

        Assert.False(app.IsAsking);
        Assert.Equal((PhonePanel.Income, PanelStep.Second), (screen.Open, screen.Step));
        Assert.Single(app.Overview.Incomes);
    }

    // ------------------------------------------------------------------ panels and acts

    [Fact]
    public void Pulling_one_panel_closes_another()
    {
        screen.Pull(PhonePanel.Accounts);
        screen.Settled(PhonePanel.Accounts, PanelStep.First);

        screen.Pull(PhonePanel.Income);

        Assert.Equal(PhonePanel.Income, screen.Open);
    }

    [Fact]
    public void Pulled_and_let_go_at_once_is_closed_again()
    {
        screen.Pull(PhonePanel.Income);
        screen.Settled(PhonePanel.Income, PanelStep.Closed);

        Assert.Equal(PhonePanel.None, screen.Open);
    }

    [Fact]
    public void Leaving_an_entrys_form_lets_go_of_an_entry_being_changed()
    {
        screen.OpenEntry(app.Overview.Incomes.Single());
        Assert.True(app.IncomeForm.IsEditing);

        screen.Settled(PhonePanel.Income, PanelStep.First);

        Assert.False(app.IncomeForm.IsEditing);
        Assert.Null(app.IncomeForm.Label);
    }

    [Fact]
    public void Leaving_an_entrys_form_keeps_a_new_entry_being_typed()
    {
        screen.Pull(PhonePanel.Expenses);
        screen.Settled(PhonePanel.Expenses, PanelStep.Second);
        app.ExpenseForm.Amount = "12,50";
        app.ExpenseForm.Category = "Boodschappen";

        screen.Settled(PhonePanel.Expenses, PanelStep.Closed);

        Assert.Equal("12,50", app.ExpenseForm.Amount);
        Assert.Equal("Boodschappen", app.ExpenseForm.Category);
    }

    [Fact]
    public void A_new_expense_that_goes_through_closes_its_panel()
    {
        screen.Pull(PhonePanel.Expenses);
        screen.Settled(PhonePanel.Expenses, PanelStep.Second);
        var moved = 0;
        screen.Moved += () => moved++;
        app.ExpenseForm.Amount = "12,50";
        app.ExpenseForm.Category = "Boodschappen";

        screen.SubmitExpense();

        Assert.Equal(PhonePanel.None, screen.Open);
        Assert.Equal(1, moved);
        Assert.Single(app.Overview.Expenses);
    }

    [Fact]
    public void A_refused_entry_stays_on_its_form()
    {
        screen.Pull(PhonePanel.Expenses);
        screen.Settled(PhonePanel.Expenses, PanelStep.Second);
        app.ExpenseForm.Amount = "12,50";

        screen.SubmitExpense();

        Assert.Equal((PhonePanel.Expenses, PanelStep.Second), (screen.Open, screen.Step));
        Assert.True(app.Notice?.IsRefusal);
    }

    [Fact]
    public void A_change_saved_goes_back_to_the_list()
    {
        screen.OpenEntry(app.Overview.Incomes.Single());
        app.IncomeForm.Amount = "2100";

        screen.SubmitIncome();

        Assert.Equal((PhonePanel.Income, PanelStep.First), (screen.Open, screen.Step));
        Assert.Equal(Money.FromCents(210000), app.Overview.Incomes.Single().Amount);
    }

    [Fact]
    public void An_entry_removed_from_its_form_goes_back_to_the_list_once_confirmed()
    {
        screen.OpenEntry(app.Overview.Incomes.Single());

        screen.RemoveEntry();
        Assert.Equal(PanelStep.Second, screen.Step);
        screen.Confirm();

        Assert.Empty(app.Overview.Incomes);
        Assert.Equal((PhonePanel.Income, PanelStep.First), (screen.Open, screen.Step));
    }

    [Fact]
    public void Deleting_the_account_whose_history_is_open_goes_back_to_the_accounts()
    {
        var cash = app.AddAccount("Contant", null)!.Account!;
        screen.OpenHistory(cash);

        screen.DeleteAccount(cash);

        Assert.Null(app.HistoryAccount);
        Assert.Equal((PhonePanel.Accounts, PanelStep.First), (screen.Open, screen.Step));
        Assert.DoesNotContain(app.Accounts, a => a.Name == "Contant");
    }

    // Sliding across the ring chooses at every slice passed: told apart from a change, so the phone
    // redraws only the ring and its centre, not every panel.
    [Fact]
    public void Choosing_on_the_ring_is_told_apart_from_a_change()
    {
        var (changed, chosen) = (0, 0);
        screen.Changed += () => changed++;
        screen.ChoiceChanged += () => chosen++;

        screen.TouchRing(0.5);
        screen.SlideOnRing(0.55);
        screen.SlideOnRing(0.1);
        screen.LiftFromRing(0.1, slid: true);

        Assert.Equal(2, chosen);
        Assert.Equal(0, changed);
    }

    [Fact]
    public void An_account_tapped_opens_its_history_and_leaving_it_closes_it()
    {
        screen.Pull(PhonePanel.Accounts);
        screen.Settled(PhonePanel.Accounts, PanelStep.First);

        screen.OpenHistory(app.Ledger.PoolAccount);
        Assert.Equal(app.Ledger.PoolAccount, app.HistoryAccount);
        Assert.Equal(PanelStep.Second, screen.Step);

        screen.Settled(PhonePanel.Accounts, PanelStep.First);
        Assert.Null(app.HistoryAccount);
    }
}
