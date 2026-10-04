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

    // ------------------------------------------------------------------ Wijzigen
    //
    // What a form does around a change is form behaviour, left by §12 for the plan to propose and
    // approved with it: these hold that proposal.

    private ExpenseLine RecordAndPick(string amount, string label)
    {
        app.RecordExpense(amount, "Boodschappen", label);
        return app.Overview.Expenses.Single(e => e.Label == label);
    }

    [Fact]
    public void Clicking_a_row_loads_the_entry_as_it_would_be_typed_and_switches_the_form_to_Wijzigen()
    {
        var line = RecordAndPick("2000", "Markt");

        app.EditExpense(line);

        var form = app.ExpenseForm;
        Assert.True(form.IsEditing);
        Assert.Equal(Tekst.Save, form.SubmitText);
        Assert.Equal(("Markt", "Boodschappen", "2000,00", new DateTime(2026, 3, 15)),
            (form.Label, form.Category, form.Amount, form.Date));
    }

    [Fact]
    public void A_change_that_goes_through_empties_the_form_and_returns_it_to_recording()
    {
        app.EditExpense(RecordAndPick("12,50", "Markt"));
        app.ExpenseForm.Amount = "13,50";

        app.ExpenseForm.SubmitCommand.Execute(null);

        Assert.False(app.ExpenseForm.IsEditing);
        Assert.Equal(Tekst.RecordExpense, app.ExpenseForm.SubmitText);
        Assert.Null(app.ExpenseForm.Amount);
        Assert.Equal(Money.FromCents(1350), Assert.Single(app.Overview.Expenses).Amount);
        Assert.False(app.Notice!.IsRefusal);
    }

    [Fact]
    public void A_refused_change_keeps_what_was_typed_and_stays_on_the_entry()
    {
        app.EditExpense(RecordAndPick("12,50", "Markt"));
        app.ExpenseForm.Amount = "12,505";

        app.ExpenseForm.SubmitCommand.Execute(null);

        Assert.True(app.ExpenseForm.IsEditing);
        Assert.Equal("12,505", app.ExpenseForm.Amount);
        Assert.True(app.Notice!.IsRefusal);
    }

    [Fact]
    public void Saving_unchanged_empties_the_form_and_says_nothing_even_after_something_was_said()
    {
        app.EditExpense(RecordAndPick("12,50", "Markt"));
        Assert.NotNull(app.Notice);

        app.ExpenseForm.SubmitCommand.Execute(null);

        Assert.False(app.ExpenseForm.IsEditing);
        Assert.Null(app.Notice);
    }

    [Fact]
    public void Annuleren_empties_the_form_and_changes_nothing()
    {
        app.EditExpense(RecordAndPick("12,50", "Markt"));
        app.ExpenseForm.Amount = "99";

        app.ExpenseForm.CancelCommand.Execute(null);

        Assert.False(app.ExpenseForm.IsEditing);
        Assert.Null(app.ExpenseForm.Amount);
        Assert.Equal(Money.FromCents(1250), Assert.Single(app.Overview.Expenses).Amount);
    }

    [Fact]
    public void Verwijderen_asks_first_and_the_form_empties_once_the_removal_is_confirmed()
    {
        app.EditExpense(RecordAndPick("12,50", "Markt"));

        app.ExpenseForm.RemoveCommand.Execute(null);

        Assert.True(app.IsAsking);
        Assert.Contains(Tekst.AreYouSure, app.Question!.Text);
        Assert.Null(app.Notice);
        Assert.Single(app.Overview.Expenses);

        app.ConfirmCommand.Execute(null);

        Assert.False(app.IsAsking);
        Assert.Empty(app.Overview.Expenses);
        Assert.False(app.ExpenseForm.IsEditing);
        Assert.False(app.Notice!.IsRefusal);
    }

    [Fact]
    public void Declining_says_nothing_and_leaves_the_entry_loaded()
    {
        app.EditExpense(RecordAndPick("12,50", "Markt"));
        app.ExpenseForm.RemoveCommand.Execute(null);

        app.DeclineCommand.Execute(null);

        Assert.False(app.IsAsking);
        Assert.Null(app.Notice);
        Assert.Single(app.Overview.Expenses);
        Assert.True(app.ExpenseForm.IsEditing);
    }

    [Fact]
    public void Stepping_to_another_period_drops_an_edit_in_progress_and_the_question_waiting()
    {
        app.EditExpense(RecordAndPick("12,50", "Markt"));
        app.ExpenseForm.Amount = "99";
        app.ExpenseForm.RemoveCommand.Execute(null);

        app.StepForward();

        Assert.False(app.IsAsking);
        Assert.False(app.ExpenseForm.IsEditing);
        Assert.Null(app.ExpenseForm.Amount);
        app.StepBack();
        Assert.Equal(Money.FromCents(1250), Assert.Single(app.Overview.Expenses).Amount);
    }

    // A new entry belongs to no period until it is recorded, so stepping to look at another
    // period, or to enter an income there, must not lose what was typed.
    [Fact]
    public void Stepping_keeps_a_new_entry_that_is_being_typed()
    {
        app.ExpenseForm.Label = "Markt";
        app.ExpenseForm.Amount = "45";
        app.IncomeForm.Label = "Salaris";

        app.StepBack();
        app.StepForward();

        Assert.Equal(("Markt", "45"), (app.ExpenseForm.Label, app.ExpenseForm.Amount));
        Assert.Equal("Salaris", app.IncomeForm.Label);
    }

    [Fact]
    public void Clicking_another_row_loads_that_one_and_drops_the_question_about_the_first()
    {
        var markt = RecordAndPick("12,50", "Markt");
        var bakker = RecordAndPick("3", "Bakker");
        app.EditExpense(markt);
        app.ExpenseForm.RemoveCommand.Execute(null);

        app.EditExpense(bakker);

        Assert.False(app.IsAsking);
        Assert.Equal(("Bakker", "3,00"), (app.ExpenseForm.Label, app.ExpenseForm.Amount));
    }

    // A question and a notice are never shown together: whatever is said next replaces it.
    [Fact]
    public void Doing_something_else_while_a_question_waits_drops_the_question()
    {
        app.EditExpense(RecordAndPick("12,50", "Markt"));
        app.ExpenseForm.RemoveCommand.Execute(null);

        app.RecordIncome("100", "Statiegeld");

        Assert.False(app.IsAsking);
        Assert.NotNull(app.Notice);
        Assert.Single(app.Overview.Expenses);
    }

    [Fact]
    public void An_income_row_loads_into_the_income_form()
    {
        app.RecordIncome("1832,45", "Salaris");

        app.EditIncome(Assert.Single(app.Overview.Incomes));

        Assert.True(app.IncomeForm.IsEditing);
        Assert.Equal(Tekst.Save, app.IncomeForm.SubmitText);
        Assert.Equal(("Salaris", "1832,45"), (app.IncomeForm.Label, app.IncomeForm.Amount));
    }

    // ------------------------------------------------------------------ Hernoemen

    [Fact]
    public void The_rename_button_turns_that_row_into_a_box_holding_its_name()
    {
        app.StartRename("Hobby");

        Assert.Equal("Hobby", app.NewName);
        Assert.Equal(["Hobby"], app.Overview.Rows.Where(r => r.IsRenaming).Select(r => r.Name));
    }

    [Fact]
    public void A_refused_rename_keeps_the_box_open_on_what_was_typed()
    {
        app.StartRename("Hobby");
        app.NewName = "huur";

        app.RenameCommand.Execute(null);

        Assert.Equal("Hobby", app.Renaming);
        Assert.Equal("huur", app.NewName);
        Assert.True(app.Notice!.IsRefusal);
    }

    // The old name is free once the category is renamed, so a form still naming it by the old one
    // would otherwise be left pointing at nothing, or at a new category that took the name.
    [Fact]
    public void A_rename_follows_into_a_form_that_names_the_category_so_saving_it_unchanged_still_holds()
    {
        app.EditExpense(RecordAndPick("12,50", "Markt"));
        app.StartRename("Boodschappen");
        app.NewName = "Eten";
        app.RenameCommand.Execute(null);
        app.AddCategory("Boodschappen");

        Assert.Equal("Eten", app.ExpenseForm.Category);
        Assert.Equal(ChangeOutcome.Unchanged, app.ExpenseForm.Save()!.Outcome);
    }

    [Fact]
    public void Only_a_category_with_no_history_anywhere_carries_the_delete_button()
    {
        app.RecordExpense("5", "Hobby", "Verf");

        Assert.Equal(
            ["Boodschappen", "Huur", "Sparen", "Verzekeringen", "Abonnementen"],
            app.Overview.Rows.Where(r => r.CanDelete).Select(r => r.Name));
    }

    // ------------------------------------------------------------------ accounts (arc42 §12, Accounts and net worth)

    [Fact]
    public void A_first_start_has_Betaalrekening_as_the_pool_with_nothing_typed()
    {
        var line = Assert.Single(app.Accounts);
        Assert.Equal(("Betaalrekening", true, Money.Zero), (line.Name, line.IsPool, line.Balance));
        Assert.Empty(app.Ledger.HistoryOf(line.Account));
    }

    [Fact]
    public void A_new_entry_starts_out_on_whichever_account_is_the_pool_now()
    {
        var cash = app.Ledger.AddAccount("Contant", null).Account!;
        Assert.Equal("Betaalrekening", app.ExpenseForm.ChosenAccount.Name);

        var told = new List<string?>();
        app.IncomeForm.PropertyChanged += (_, e) => told.Add(e.PropertyName);
        app.MakePool(cash);

        Assert.Same(cash, app.ExpenseForm.ChosenAccount);
        Assert.Same(cash, app.IncomeForm.ChosenAccount);
        Assert.Contains(nameof(IncomeForm.ChosenAccount), told);
    }

    [Fact]
    public void An_entry_is_recorded_on_the_account_chosen_and_the_form_goes_back_to_the_pool()
    {
        var cash = app.Ledger.AddAccount("Contant", null).Account!;
        var form = app.ExpenseForm;
        (form.Amount, form.Category, form.ChosenAccount) = ("5", "Boodschappen", cash);

        form.SubmitCommand.Execute(null);

        var line = Assert.Single(app.Overview.Expenses);
        Assert.Same(cash, line.Entry.Account);
        Assert.Equal("Contant", line.AccountName);
        Assert.Same(app.Ledger.PoolAccount, form.ChosenAccount);
    }

    [Fact]
    public void Loading_an_entry_loads_its_account()
    {
        var cash = app.Ledger.AddAccount("Contant", null).Account!;
        app.RecordIncome("20", "Verkocht", account: cash);

        app.EditIncome(Assert.Single(app.Overview.Incomes));

        Assert.Same(cash, app.IncomeForm.ChosenAccount);
    }

    [Fact]
    public void A_form_holding_a_deleted_account_goes_back_to_the_pool()
    {
        var cash = app.Ledger.AddAccount("Contant", null).Account!;
        app.ExpenseForm.ChosenAccount = cash;
        app.TransferForm.ChosenTo = cash;
        app.OpenHistory(cash);

        app.DeleteAccount(cash);

        Assert.Same(app.Ledger.PoolAccount, app.ExpenseForm.ChosenAccount);
        Assert.NotSame(cash, app.TransferForm.ChosenTo);
        Assert.False(app.IsHistoryOpen);
    }

    [Fact]
    public void A_transfer_starts_out_from_the_pool_to_the_first_other_account()
    {
        var form = app.TransferForm;
        Assert.Same(form.ChosenFrom, form.ChosenTo);

        app.Ledger.AddAccount("Contant", null);
        app.Ledger.AddAccount("Spaarrekening", null);

        Assert.Equal(("Betaalrekening", "Contant"), (form.ChosenFrom.Name, form.ChosenTo.Name));
    }

    // A list writes back what it shows, and nothing while its items are replaced. Found in a
    // headless run of the window: Naar stayed on the only account there was, and a form stayed on
    // the old pool account.
    [Fact]
    public void A_list_writing_back_what_it_shows_does_not_hold_a_form_on_an_old_default()
    {
        var transfer = app.TransferForm;
        var expense = app.ExpenseForm;
        transfer.ChosenFrom = transfer.ChosenFrom;
        transfer.ChosenTo = transfer.ChosenTo;
        expense.ChosenAccount = expense.ChosenAccount;
        expense.ChosenAccount = null!;

        var cash = app.AddAccount("Contant", "")!.Account!;
        Assert.Same(cash, transfer.ChosenTo);

        app.MakePool(cash);
        Assert.Same(cash, expense.ChosenAccount);
        Assert.Same(cash, transfer.ChosenFrom);
        Assert.Equal("Betaalrekening", transfer.ChosenTo.Name);
    }

    // Found by the spec review: Naar that happened to be the default moved when Van was chosen after it.
    [Fact]
    public void Naar_chosen_before_Van_stays_what_was_chosen()
    {
        var cash = app.AddAccount("Contant", "")!.Account!;
        var savings = app.AddAccount("Spaarrekening", "")!.Account!;
        var form = app.TransferForm;

        form.ChosenTo = cash;
        form.ChosenFrom = savings;

        Assert.Equal((savings, cash), (form.ChosenFrom, form.ChosenTo));
    }

    // Found by the spec review: an entry moved from another account onto the pool account, and saved.
    [Fact]
    public void An_entry_can_be_moved_onto_the_pool_account()
    {
        var cash = app.AddAccount("Contant", "")!.Account!;
        app.RecordExpense("5", "Boodschappen", label: "Markt", account: cash);
        app.EditExpense(Assert.Single(app.Overview.Expenses));

        app.ExpenseForm.ChosenAccount = app.Ledger.PoolAccount;
        var result = app.ExpenseForm.Save();

        Assert.Equal(ChangeOutcome.Changed, result!.Outcome);
        Assert.Same(app.Ledger.PoolAccount, Assert.Single(app.Overview.Expenses).Entry.Account);
    }

    // Found by the spec review: nothing held that the income form, too, goes back to the pool.
    [Fact]
    public void After_an_income_on_another_account_the_form_starts_out_on_the_pool_again()
    {
        var cash = app.AddAccount("Contant", "")!.Account!;
        var form = app.IncomeForm;
        (form.Amount, form.Label, form.ChosenAccount) = ("20", "Verkocht", cash);

        form.SubmitCommand.Execute(null);

        Assert.Same(cash, Assert.Single(app.Overview.Incomes).Entry.Account);
        Assert.Same(app.Ledger.PoolAccount, form.ChosenAccount);
    }

    // Also found in the headless run: a transfer being changed had its Van written back as the pool
    // account, and followed the pool when it changed, turning the transfer round.
    [Fact]
    public void An_entry_being_changed_keeps_its_own_accounts_whatever_the_pool()
    {
        var bank = app.Ledger.PoolAccount;
        var cash = app.Ledger.AddAccount("Contant", null).Account!;
        app.RecordIncome("100", "Salaris");
        var transfer = app.Ledger.RecordTransfer(20m, bank, cash, new DateOnly(2026, 3, 15)).Transfer!;
        app.EditIncome(Assert.Single(app.Overview.Incomes));
        app.TransferForm.Load(transfer);

        app.IncomeForm.ChosenAccount = bank;
        app.TransferForm.ChosenFrom = bank;
        app.TransferForm.ChosenTo = cash;
        app.MakePool(cash);

        Assert.Same(bank, app.IncomeForm.ChosenAccount);
        Assert.Equal((bank, cash), (app.TransferForm.ChosenFrom, app.TransferForm.ChosenTo));
    }

    // ------------------------------------------------------------------ backing (arc42 §12, Backing and Accumulated)

    private Account BackSparen()
    {
        var savings = app.AddAccount("Spaarrekening", "")!.Account!;
        app.SetBacking("Sparen", savings);
        return savings;
    }

    [Theory]
    [InlineData("Sparen", "Spaarrekening")]
    [InlineData("  sparen ", "Spaarrekening")]
    [InlineData("Boodschappen", "Betaalrekening")]
    [InlineData("Spa", "Betaalrekening")]
    [InlineData("Vakantie", "Betaalrekening")]
    [InlineData(null, "Betaalrekening")]
    public void A_new_expense_s_account_follows_the_category_typed(string? typed, string account)
    {
        BackSparen();
        var told = new List<string?>();
        app.ExpenseForm.PropertyChanged += (_, e) => told.Add(e.PropertyName);

        app.ExpenseForm.Category = typed;

        Assert.Equal(account, app.ExpenseForm.ChosenAccount.Name);
        if (typed is not null) Assert.Contains(nameof(ExpenseForm.ChosenAccount), told);
    }

    // The list writes back the account it shows, and nothing while its items are replaced. Neither
    // is a choice, so the account still follows what is typed next.
    [Fact]
    public void The_list_writing_back_what_it_shows_is_not_a_choice()
    {
        var savings = BackSparen();
        var form = app.ExpenseForm;

        form.ChosenAccount = form.ChosenAccount;
        form.ChosenAccount = null!;
        form.Category = "Sparen";
        Assert.Same(savings, form.ChosenAccount);

        form.ChosenAccount = form.ChosenAccount;
        form.Category = "Boodschappen";
        Assert.Same(app.Ledger.PoolAccount, form.ChosenAccount);
    }

    // Since increment 15 a backed category's list is locked on its account, and a write to it is
    // ignored. A pick made before is kept for a category without an account typed after it, and is
    // for that one expense only (arc42 §12, *An expense on a backed category is on its account*).
    [Fact]
    public void An_account_picked_gives_way_to_a_locked_list_comes_back_after_it_and_is_for_that_expense_only()
    {
        var savings = BackSparen();
        var cash = app.AddAccount("Contant", "")!.Account!;
        var form = app.ExpenseForm;

        form.ChosenAccount = cash;
        form.Category = "Sparen";
        Assert.Same(savings, form.ChosenAccount);
        Assert.True(form.IsAccountLocked);

        form.ChosenAccount = app.Ledger.PoolAccount;
        Assert.Same(savings, form.ChosenAccount);

        form.Category = "Boodschappen";
        Assert.Same(cash, form.ChosenAccount);
        Assert.False(form.IsAccountLocked);

        (form.Amount, form.Label) = ("5", "Markt");
        form.SubmitCommand.Execute(null);
        Assert.Same(cash, Assert.Single(app.Overview.Expenses).Entry.Account);

        form.Category = "Boodschappen";
        Assert.Same(app.Ledger.PoolAccount, form.ChosenAccount);
    }

    // Reversed in increment 15 (derived): a change is judged as if recorded now, and the list is locked.
    [Fact]
    public void An_expense_being_changed_goes_onto_the_account_of_a_backed_category_it_is_changed_to()
    {
        var savings = BackSparen();
        app.RecordExpense("5", "Boodschappen", "Markt");
        app.EditExpense(Assert.Single(app.Overview.Expenses));

        app.ExpenseForm.Category = "Sparen";

        Assert.Same(savings, app.ExpenseForm.ChosenAccount);
        Assert.True(app.ExpenseForm.IsAccountLocked);
    }

    // The lock follows the date as well as the category (follow-up 16).
    [Fact]
    public void The_list_opens_for_a_date_before_the_period_the_category_got_its_account()
    {
        var savings = BackSparen();
        var form = app.ExpenseForm;
        var told = new List<string?>();
        form.PropertyChanged += (_, e) => told.Add(e.PropertyName);
        form.Category = "Sparen";

        form.Date = app.Ledger.Calendar.Previous(app.Ledger.CurrentPeriod).LastDay.ToDateTime(TimeOnly.MinValue);

        Assert.False(form.IsAccountLocked);
        Assert.Same(app.Ledger.PoolAccount, form.ChosenAccount);
        Assert.Contains(nameof(ExpenseForm.IsAccountLocked), told);

        form.Date = null;
        Assert.Same(savings, form.ChosenAccount);
    }

    [Fact]
    public void Choosing_in_a_row_s_Staat_op_list_backs_the_category_and_shows_it()
    {
        var savings = app.AddAccount("Spaarrekening", "")!.Account!;
        var row = app.Overview.Rows.Single(r => r.Name == "Sparen");
        Assert.Equal(["—", "Betaalrekening", "Spaarrekening"], row.BackingChoices.Select(c => c.Text));
        Assert.Equal("—", row.ChosenBacking!.Text);
        Assert.Null(row.AccumulatedText);

        row.ChosenBacking = row.BackingChoices.Single(c => c.Account == savings);

        var backed = app.Overview.Rows.Single(r => r.Name == "Sparen");
        Assert.Same(savings, backed.BackingAccount);
        Assert.Equal("Spaarrekening", backed.ChosenBacking!.Text);
        Assert.Equal("Opgebouwd: € 0,00", backed.AccumulatedText);
        Assert.False(app.Notice!.IsRefusal);
    }

    // The same list for every row and every refresh, until the accounts change.
    [Fact]
    public void The_Staat_op_list_is_the_same_collection_until_the_accounts_change()
    {
        var before = app.BackingChoices;
        app.Refresh();
        Assert.Same(before, app.BackingChoices);
        Assert.All(app.Overview.Rows, r => Assert.Same(before, r.BackingChoices));

        app.AddAccount("Contant", "");
        Assert.NotSame(before, app.BackingChoices);
    }

    [Fact]
    public void Accumulated_below_zero_carries_the_marker_and_exactly_zero_does_not()
    {
        BackSparen();
        app.Assign("10", "Sparen");
        app.RecordExpense("10", "Sparen", "Fiets");
        Assert.Equal(Marker.None, app.Overview.Rows.Single(r => r.Name == "Sparen").AccumulatedMarker);

        app.RecordExpense("0,01", "Sparen", "Bel");
        var row = app.Overview.Rows.Single(r => r.Name == "Sparen");
        Assert.Equal((Marker.Over, true, "Opgebouwd: −€ 0,01"), (row.AccumulatedMarker, row.IsAccumulatedBelowZero, row.AccumulatedText));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void A_starting_balance_left_empty_is_none(string? typed)
    {
        var form = app.AccountForm;
        form.OpenCommand.Execute(null);
        (form.Name, form.StartingBalance) = ("Contant", typed);

        form.SubmitCommand.Execute(null);

        var cash = app.Ledger.AccountNamed("Contant")!;
        Assert.Empty(app.Ledger.HistoryOf(cash));
        Assert.False(form.IsOpen);
    }

    [Fact]
    public void A_refused_account_keeps_the_form_open_on_what_was_typed()
    {
        var form = app.AccountForm;
        form.OpenCommand.Execute(null);
        (form.Name, form.StartingBalance) = ("betaalrekening", "10");

        form.SubmitCommand.Execute(null);

        Assert.True(form.IsOpen);
        Assert.Equal(("betaalrekening", "10"), (form.Name, form.StartingBalance));
        Assert.True(app.Notice!.IsRefusal);
    }

    [Fact]
    public void Clicking_the_open_account_again_closes_its_history()
    {
        var bank = app.Ledger.PoolAccount;
        app.OpenHistory(bank);
        Assert.True(Assert.Single(app.Accounts).IsOpen);

        app.OpenHistory(bank);
        Assert.False(app.IsHistoryOpen);
    }
}
