# MoneyBud opens on the current budget period and steps back and forward from there, one period
# at a time (glossary: "Stepping between periods", settled 2026-09-25). A UI that showed only the
# current period would accept entries it could never show. An expense can be back-dated into any
# earlier period, an income can be dated into any period, and an amount can be assigned in any
# later one. Each lands in its own period, so each needs a way to reach that period.
#
# No limit is settled in either direction: assigning is allowed in future periods without limit,
# and nothing bounds how far back an expense or an income may be dated. The step grammar names
# only one period each way, so "not limited" is asserted as being able to step on again from the
# periods either side of the current one.
#
# Stepping back and forward is the only way to move between periods. There is no one-step way
# back to the current period: the stakeholder considered one and turned it down (2026-09-25).
#
# What the period on screen does and does not decide, settled by the stakeholder on 2026-09-25:
#   - A new expense's or income's date starts out as TODAY, whichever period is shown. Recording
#     one without giving a date lands it in the current period.
#   - A new assignment's period starts out as THE PERIOD SHOWN. Assigning without naming a period
#     assigns in the period on screen.
#   - Assigning is offered whichever period is shown, a past one included. In a past period it is
#     refused, and the reason is shown. That is how the past-period refusal is reached from the
#     screen. The refusal itself, and the order refusals are reported in, are specified in
#     assign-to-category.feature and are not restated here.
#   - When an expense, an income or an assignment lands in a different period from the one on
#     screen, the Overview stays on the period it showed and says which period the entry went to.
#     Both parts are asserted. What is fixed is that I am told which period, not how it is worded.
#   - If MoneyBud stays open while a new period begins, the Overview stays on the period it
#     showed. That period is now the previous one, so assigning in it is refused from then on.
#     Nothing is announced. The only visible change is that the period on screen stops being
#     labelled as the current period.
#
# An expense or income given an explicit date, and an assignment that names its period, still go
# where their date or their period says (record-expense.feature, record-income.feature,
# assign-to-category.feature). The period on screen does not override that.
#
# Reading the steps:
#   - "the Overview shows the ... budget period" (a Given) and "the Overview should show the ...
#     budget period" are about which period is on screen.
#   - Periods are always named relative to TODAY, never relative to what is on screen. In the one
#     scenario where a new period begins, today moves forward, so the period that the Givens call
#     "current" is "previous" in every step after "the next budget period begins".
#   - "I step back one budget period" and "I step forward one budget period" are relative to the
#     period on screen.
#   - "I should be able to step back (forward) one budget period" means stepping that way is
#     available from the period on screen. "I should be able to assign in the budget period shown"
#     means assigning is offered there. It says nothing about whether the assignment is accepted.
#   - "without giving a date" and "without naming a budget period" mean I leave those as they
#     start out.
#   - "I should be told that the expense (income, assignment) went into the ... budget period" is
#     the Overview saying where the entry landed.
#   - "today is the last day of the current budget period" is record-income.feature's "today is
#     the first day of the current budget period", naming a different day.
#   - "the next budget period begins while MoneyBud is open" means today moves on to the first day
#     of the next period without MoneyBud being closed or started again.
#   - "the period on screen should no longer be labelled as the current budget period" is about
#     the label only. The label's wording is a display term, not asserted here.
#   - "I should not have been told anything" means no message of any kind appeared.
#   - The steps that list expenses and incomes are explained in
#     list-transactions-in-a-period.feature, and the ring steps in overview.feature. The refusal
#     steps are assign-to-category.feature's.
#
# Every scenario starts from an empty ledger and names the categories it needs, except the first,
# which is about starting. The names are synthetic test data.

@budget
Feature: Step between budget periods
  As someone budgeting my money
  I want to step back and forward from the current budget period to the ones before and after it
  So that I can see what belongs to other periods: a receipt remembered late, a salary that is still to come, and the plan I have made for next month

  # ----------------------------------------------------------------------------------
  # Opening and stepping
  # ----------------------------------------------------------------------------------

  # Nothing is kept when MoneyBud closes, so for now every start is a first start (glossary: "What
  # the UI starts with, and what it keeps").
  Scenario: MoneyBud opens on the current budget period
    When I start using MoneyBud for the first time
    Then the Overview should show the current budget period

  Scenario Outline: Stepping moves one budget period at a time
    Given my budget periods are one month long
    And the Overview shows the <from> budget period
    When I step <direction> one budget period
    Then the Overview should show the <to> budget period

    Examples:
      | from     | direction | to       |
      | current  | back      | previous |
      | current  | forward   | next     |
      | previous | forward   | current  |
      | next     | back      | current  |

  # Nothing is recorded anywhere. An empty period does not stop the stepping either.
  Scenario Outline: Stepping is not limited to the periods either side of the current one
    Given my budget periods are one month long
    And the Overview shows the <period> budget period
    Then I should be able to step back one budget period
    And I should be able to step forward one budget period

    Examples:
      | period   |
      | previous |
      | current  |
      | next     |

  # ----------------------------------------------------------------------------------
  # A new expense or income is dated today, whichever period is shown
  #
  # The date does not follow the screen. Where that lands the entry in a period other than the
  # one shown, the Overview stays put and says where it went.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An expense recorded without a date while another period is shown is dated today
    Given my budget periods are one month long
    And I have a category "Groceries"
    And the Overview shows the <shown> budget period
    When I record an expense of 25 euro for "Groceries" labelled "Albert Heijn" without giving a date
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 25.00  |
    And no expenses should be listed in the <shown> budget period
    And the Overview should show the <shown> budget period
    And I should be told that the expense went into the current budget period

    Examples:
      | shown    |
      | previous |
      | next     |

  Scenario Outline: An income recorded without a date while another period is shown is dated today
    Given my budget periods are one month long
    And the Overview shows the <shown> budget period
    When I record an income of 1832.45 euro labelled "Salaris" without giving a date
    Then the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | today | Salaris | 1832.45 |
    And no incomes should be listed in the <shown> budget period
    And Unassigned in the current budget period should be 1832.45 euro
    And the Overview should show the <shown> budget period
    And I should be told that the income went into the current budget period

    Examples:
      | shown    |
      | previous |
      | next     |

  # ----------------------------------------------------------------------------------
  # A new assignment is in the period shown
  #
  # Unlike a date, an assignment's period follows the screen.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An assignment that names no period goes to the period shown
    Given my budget periods are one month long
    And I have already recorded 2000 euro of income in the current budget period
    And I have already recorded 1800 euro of income in the next budget period
    And I have a category "Groceries"
    And the Overview shows the <shown> budget period
    When I assign 400 euro to "Groceries" without naming a budget period
    Then the budget for "Groceries" in the <shown> budget period should be 400 euro
    And the budget for "Groceries" in the <other> budget period should be 0.00 euro
    And Unassigned in the <shown> budget period should be <unassigned> euro
    And the Overview should show the <shown> budget period

    Examples:
      | shown   | other   | unassigned |
      | current | next    | 1600.00    |
      | next    | current | 1400.00    |

  # Assigning is not taken away from a past period's screen. It is offered, and refused there.
  Scenario Outline: Assigning is offered whichever period is shown
    Given my budget periods are one month long
    And I have a category "Groceries"
    And the Overview shows the <period> budget period
    Then I should be able to assign in the budget period shown

    Examples:
      | period   |
      | previous |
      | current  |
      | next     |

  # The refusal and its reason are assign-to-category.feature's ("Assigning in the previous budget
  # period is refused, whatever the amount"). What is new is only that the period shown is the one
  # the assignment is tried in, and that it does not quietly go to the current period instead.
  Scenario: Assigning while a past period is shown is refused, and says why
    Given my budget periods are one month long
    And I have already recorded 1800 euro of income in the previous budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have already recorded 2000 euro of income in the current budget period
    And the Overview shows the previous budget period
    When I try to assign 40 euro to "Groceries" without naming a budget period
    Then the assignment should be refused
    And I should be told that nothing can be assigned in a past budget period
    And the budget for "Groceries" in the previous budget period should still be 400 euro
    And the budget for "Groceries" in the current budget period should be 0.00 euro
    And Unassigned in the current budget period should still be 2000 euro
    And the Overview should show the previous budget period

  # ----------------------------------------------------------------------------------
  # An entry that lands in another period
  #
  # The date decides where an expense or income lands, and the period named decides where an
  # assignment lands. When that is not the period on screen, the Overview stays on the period it
  # showed and says where the entry went.
  # ----------------------------------------------------------------------------------

  Scenario: A back-dated expense lands in its own period, and the Overview stays and says where it went
    Given my budget periods are one month long
    And I have a category "Groceries"
    And the Overview shows the current budget period
    When I record an expense of 12.50 euro for "Groceries" labelled "Kiosk" dated on the last day of the previous budget period
    Then no expenses should be listed in the current budget period
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date                                       | category  | label | amount |
      | the last day of the previous budget period | Groceries | Kiosk | 12.50  |
    And "Groceries" should be shown in the previous budget period
    And the Overview should show the current budget period
    And I should be told that the expense went into the previous budget period

  Scenario: An income dated in the next period lands there, and the Overview stays and says where it went
    Given my budget periods are one month long
    And I have recorded no income in the current budget period
    And the Overview shows the current budget period
    When I record an income of 1800 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    Then no incomes should be listed in the current budget period
    And the incomes listed in the next budget period should be exactly these, in this order:
      | date                                    | label                  | amount  |
      | the first day of the next budget period | Salaris volgende maand | 1800.00 |
    And the ring for the next budget period should have an Unassigned slice of 1800 euro
    And the ring for the current budget period should be empty, with a hint that there is no income in it
    And the Overview should show the current budget period
    And I should be told that the income went into the next budget period

  # The way to enter a receipt remembered late: step back to its period, and give its date, since
  # the date starts out as today.
  Scenario: A back-dated expense recorded while its own period is shown lands in that period
    Given my budget periods are one month long
    And I have a category "Groceries"
    And the Overview shows the previous budget period
    When I record an expense of 12.50 euro for "Groceries" labelled "Kiosk" dated on the last day of the previous budget period
    Then the expenses listed in the previous budget period should be exactly these, in this order:
      | date                                       | category  | label | amount |
      | the last day of the previous budget period | Groceries | Kiosk | 12.50  |
    And no expenses should be listed in the current budget period
    And the Overview should show the previous budget period

  Scenario: An assignment that names another period lands there, and the Overview stays and says where it went
    Given my budget periods are one month long
    And I have already recorded 1500 euro of income in the current budget period
    And I have already recorded 1800 euro of income in the next budget period
    And I have a category "Groceries"
    And the Overview shows the current budget period
    When I assign 400 euro to "Groceries" in the next budget period
    Then the ring for the next budget period should have exactly these category slices, in this order:
      | category  | size   | filled | over budget |
      | Groceries | 400.00 | 0.00   | no          |
    And the ring for the next budget period should have an Unassigned slice of 1400 euro
    And the ring for the current budget period should have no category slices
    And the ring for the current budget period should have an Unassigned slice of 1500 euro
    And the Overview should show the current budget period
    And I should be told that the assignment went into the next budget period

  # ----------------------------------------------------------------------------------
  # Staying open into a new period
  #
  # The screen does not jump when the clock passes a period boundary, and nothing is announced.
  # The period it showed is still on screen, no longer labelled as the current one, and a past
  # period's plan cannot be changed. From "the next budget period begins" on, periods are named
  # relative to the new today.
  #
  # Two When/Then pairs, so that "not told anything" is checked before the refused assignment
  # tells me something.
  # ----------------------------------------------------------------------------------

  Scenario: When a new period begins while MoneyBud is open, the Overview stays on the period it showed, and that period is now past
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And the Overview shows the current budget period
    When the next budget period begins while MoneyBud is open
    Then the Overview should show the previous budget period
    And the period on screen should no longer be labelled as the current budget period
    And I should not have been told anything
    When I try to assign 40 euro to "Groceries" without naming a budget period
    Then the assignment should be refused
    And I should be told that nothing can be assigned in a past budget period
    And the budget for "Groceries" in the previous budget period should still be 400 euro
    And the budget for "Groceries" in the current budget period should be 0.00 euro
