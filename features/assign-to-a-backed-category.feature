# Assigning to a backed category: for such a category, assigning is a plan AND a real transfer
# (glossary: "Assigning to a backed category moves money", "Assigning may overdraw the pool account",
# "Backing and Accumulated", settled by the stakeholder on 2026-09-27). Everything assigning already
# does is unchanged and is not restated: moving an amount out of Unassigned into a Budget, negative
# and zero amounts, clipping, the refusals and their order (assign-to-category.feature), and taking a
# plan over (take-over-a-plan.feature). This file adds only the money that moves. The steps the
# backing files share are explained in back-a-category.feature.
#
# The rules, from arc42 §12:
#   - ASSIGNING TO A BACKED CATEGORY MOVES THE AMOUNT FROM THE POOL ACCOUNT TO ITS BACKING ACCOUNT.
#     Choosing another account for one assignment is deferred until missed.
#   - THE MONEY MOVES ON THE DAY OF ASSIGNING, OR ON THE PERIOD'S FIRST DAY IF THAT IS LATER. So an
#     assignment in the current period moves today, and one in a later period leaves today's balances
#     alone and moves on that period's first day. Why, as accepted: always on the first day would date
#     a mid-month move before a balance correction taken earlier that month, which would swallow it;
#     always today would show money moved for a plan that has not started.
#   - A NEGATIVE ASSIGNMENT MOVES IT BACK, BUT ONLY WHAT THE CLIP LETS THROUGH, AND NEVER MORE THAN
#     IS THERE FOR THE CATEGORY IN THE BACKING ACCOUNT: what was moved in for it, minus its expenses
#     paid from that account, the test unbacking uses (back-a-category.feature). Ruled by the
#     stakeholder at the scenario gate, 2026-09-27. The clip against the Budget is unchanged: the
#     Budget still comes down by the whole amount the clip lets through. The cap is about money only.
#     So when some of the category's money was spent before the backing, or paid from the backing
#     account, less money moves back than the Budget comes down by. How that difference is reported,
#     if at all, is not fixed here.
#     (Noted for increment 15, 2026-10-04, derived and not put to the stakeholder: "what was moved in for
#     it" now also counts money given to it from an account's Unclaimed or moved in from another
#     category, less money moved out of it (reallocate-an-amount.feature, which has a scenario for it). The
#     clip against the Budget is unchanged, so in practice the Budget still bounds what comes back. No
#     scenario in this file changes: none of them reallocates, and every expense in them against a backed
#     category is already on its backing account, as it now must be.)
#   - It GOES THROUGH WHEN THE POOL ACCOUNT HAS NOT GOT THE MONEY, leaving it overdrawn and marked,
#     never blocked or warned about.
#
# DERIVED BY THE DOCUMENTATION AND NOT PUT TO THE STAKEHOLDER, marked where a scenario rests on one:
#   - Taking a plan over is assigning, so each figure it assigns to a backed category moves money by
#     the same rule: today in the current period, on the period's first day in a later one.
#   - Assigning zero moves nothing, and a clipped negative assignment moves back only what came back.
#   - A movement is between two of my accounts, so it leaves net worth as it was, except where a
#     balance correction has already counted one side, as for a transfer. It changes no budget figure
#     beyond what the assignment itself changes.
#   - A movement meets balance corrections by the same rule as any entry. On its own day it orders by
#     RECORDING ORDER, and a movement for a later period was recorded when it was ASSIGNED. So a
#     balance correction typed on that period's first day already has the movement in it: the bank is
#     taken to show it moved already. Kept after the follow-ups.
#   - Money for a later period comes out of the pool account AS IT IS ON THE DAY IT MOVES, not as it
#     was when it was assigned.
#
# Reading the steps: the assigning steps are assign-to-category.feature's, the take-over steps
# take-over-a-plan.feature's, the account steps show-accounts.feature's, and the backing steps
# back-a-category.feature's. After a period boundary, periods are named relative to the new today, as
# in step-between-periods.feature.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names, labels and amounts are
# synthetic test data.

@backing
Feature: Assign to a backed category
  As someone saving for a purpose in an account kept for it
  I want the money I assign to that purpose to move into that account, out of the account my income lands in
  So that my balances show where the money for each purpose really is, without my recording the transfer twice

  # ----------------------------------------------------------------------------------
  # Assigning moves the money today, in the current period
  # ----------------------------------------------------------------------------------

  Scenario: Assigning to a backed category moves the amount from the pool account to its backing account
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I assign 300 euro to "Savings" in the current budget period
    Then the assignment should go through
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1700.00 |
      | Deposit | 5300.00 |
    And net worth should still be 7000 euro
    And the budget for "Savings" in the current budget period should be 300 euro
    And the remaining "Savings" budget in the current budget period should be 300 euro
    And Unassigned in the current budget period should be 1700 euro
    And Accumulated for "Savings" in the current budget period should be 300 euro

  # All of Bank's balance is not an overdraft. One cent more is, and it goes through without a word.
  Scenario Outline: Exactly the amount assigned moves, even past what the pool account holds
    Given I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I assign <amount> euro to "Savings" in the current budget period
    Then the assignment should go through
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance  | overdrawn   |
      | Bank    | <bank>   | <overdrawn> |
      | Deposit | <amount> | no          |
    And net worth should still be 1000 euro

    Examples:
      | amount  | bank   | overdrawn |
      | 0.01    | 999.99 | no        |
      | 1000.00 | 0.00   | no        |
      | 1000.01 | -0.01  | yes       |

  # ----------------------------------------------------------------------------------
  # Assigning in a later period moves the money on that period's first day
  # ----------------------------------------------------------------------------------

  Scenario: Assigning in a later period leaves today's balances alone, and moves the money on that period's first day
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I assign 300 euro to "Savings" in the next budget period
    Then the assignment should go through
    And the balance of "Bank" should still be 2000 euro
    And the balance of "Deposit" should still be 0.00 euro
    When the next budget period begins while MoneyBud is open
    Then the balance of "Bank" should be 1700 euro
    And the balance of "Deposit" should be 300 euro
    And net worth should still be 2000 euro

  # Derived, not asked (see the header): the pool account on the day the money moves is the source.
  Scenario: Money for a later period comes out of whichever account is the pool account on the day it moves
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 1000 euro
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the next budget period
    When I make "Cash" the pool account
    And the next budget period begins while MoneyBud is open
    Then the balance of "Cash" should be 700 euro
    And the balance of "Bank" should still be 2000 euro
    And the balance of "Deposit" should be 300 euro

  # Refused as any assignment in a past period is (assign-to-category.feature), so nothing moves.
  Scenario: An assignment in a past period is refused, and moves no money
    Given my budget periods are one month long
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I try to assign 40 euro to "Savings" in the previous budget period
    Then the assignment should be refused
    And I should be told that nothing can be assigned in a past budget period
    And the balance of "Bank" should still be 2000 euro
    And the balance of "Deposit" should still be 0.00 euro

  # ----------------------------------------------------------------------------------
  # Negative and zero amounts
  #
  # A negative amount moves money back from the backing account to the pool account, as much as the
  # clip lets through, and never more than is there for the category. Zero moves nothing. In the
  # first scenarios below the whole budget moved in and none of it was spent, so all of it is there.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A negative amount moves money back to the pool account, and zero moves nothing
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    And the balance of "Deposit" is 300 euro
    When I assign <amount> euro to "Savings" in the current budget period
    Then the assignment should go through
    And I should not be told of any shortfall
    And the budget for "Savings" in the current budget period should be <budget> euro
    And the accounts should be exactly these, in this order:
      | account | balance   |
      | Bank    | <bank>    |
      | Deposit | <deposit> |
    And Accumulated for "Savings" in the current budget period should be <deposit> euro

    Examples:
      | amount  | budget | bank    | deposit |
      | -50.00  | 250.00 | 1750.00 | 250.00  |
      | -0.01   | 299.99 | 1700.01 | 299.99  |
      | -300.00 | 0.00   | 2000.00 | 0.00    |
      | 0.00    | 300.00 | 1700.00 | 300.00  |

  Scenario: A negative amount larger than the budget moves back only what the budget held
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    When I assign -350 euro to "Savings" in the current budget period
    Then the assignment should go through
    And I should be told of a shortfall of 50 euro
    And the budget for "Savings" in the current budget period should be 0.00 euro
    And the balance of "Bank" should be 2000 euro
    And the balance of "Deposit" should be 0.00 euro
    And Accumulated for "Savings" in the current budget period should be 0.00 euro

  # The stakeholder's ruling at the scenario gate, in the example it was put to him with. 100 was
  # spent from Bank before the backing, so the backing moved 200, not 300. Assigning -300 takes the
  # Budget to zero, as the clip allows, but only the 200 there for Savings moves back. Bank ends 100
  # down, for the 100 spent, and Accumulated reads zero. How the 100 difference is worded is not
  # fixed, so nothing is asserted about what I am told.
  Scenario: A negative amount moves back no more money than is there for the category, though the budget comes down by all of it
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have already spent 100 euro on "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    And the balance of "Deposit" is 200 euro
    And Accumulated for "Savings" in the current budget period is 200 euro
    When I assign -300 euro to "Savings" in the current budget period
    Then the assignment should go through
    And I should not be warned or asked to confirm
    And the budget for "Savings" in the current budget period should be 0.00 euro
    And Unassigned in the current budget period should be 2000 euro
    And the remaining "Savings" budget in the current budget period should be -100 euro
    And "Savings" should be shown as over budget in the current budget period
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1900.00 |
      | Deposit | 0.00    |
    And Accumulated for "Savings" in the current budget period should be 0.00 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | category | from    | to      | amount | balance |
      | today | movement         | Savings  | Deposit | Bank    | 200.00 |         |
      | today | movement         | Savings  | Bank    | Deposit | 200.00 |         |
      | today | starting balance |          |         |         |        | 0.00    |

  # The same cap, when the category's expenses were paid from the backing account itself. 300 moved
  # in, and Savings spent from Deposit. -100 always takes the Budget to 200. What moves back is what
  # is there for Savings, at most 100:
  #   - 100 spent: 200 is there, so all 100 moves.
  #   - 250 spent: 50 is there, so 50 moves.
  #   - 300 spent: nothing is there, and nothing moves.
  #   - 350 spent: less than nothing is there, and nothing moves. Deposit stays overdrawn.
  Scenario Outline: A negative amount moves back at most what is left for the category after its expenses paid from the backing account
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have recorded an expense of <spent> euro for "Savings" labelled "Fiets" dated today on the account "Deposit"
    When I assign -100 euro to "Savings" in the current budget period
    Then the assignment should go through
    And the budget for "Savings" in the current budget period should be 200 euro
    And the accounts should be exactly these, in this order:
      | account | balance   | overdrawn   |
      | Bank    | <bank>    | no          |
      | Deposit | <deposit> | <overdrawn> |
    And Accumulated for "Savings" in the current budget period should be <accumulated> euro

    Examples:
      | spent  | bank    | deposit | overdrawn | accumulated |
      | 100.00 | 1800.00 | 100.00  | no        | 100.00      |
      | 250.00 | 1750.00 | 0.00    | no        | 0.00        |
      | 300.00 | 1700.00 | 0.00    | no        | 0.00        |
      | 350.00 | 1700.00 | -50.00  | yes       | -50.00      |

  # Archiving does nothing to backing (back-a-category.feature). Pulling an archived category's
  # money back is tidying up: it moves the money back, and a negative amount brings nothing back
  # from archived (assign-to-category.feature). With its budget at zero and nothing built up for it,
  # the archived category has nothing left to show in the period, so it is not shown. The last line
  # was amended with the stakeholder's approval at the build, 2026-09-27: it first asked for the
  # Accumulated of a row that no screen shows.
  Scenario: A negative amount assigned to an archived backed category moves its money back, and it stays archived
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have archived the category "Savings"
    When I assign -200 euro to "Savings" in the current budget period
    Then the assignment should go through
    And the balance of "Bank" should be 2000 euro
    And the balance of "Deposit" should be 0.00 euro
    And the categories offered for a new expense should not include "Savings"
    And "Savings" should not be shown in the current budget period

  # ----------------------------------------------------------------------------------
  # Taking a plan over is assigning
  #
  # Derived, not asked (see the header): each figure for a backed category moves money by the same
  # date rule. Groceries is unbacked, so its 400 moves nothing.
  # ----------------------------------------------------------------------------------

  # Savings was unbacked in the previous period, so nothing moved then. It is backed now, with
  # nothing in the current period to move.
  Scenario: Taking a plan over in the current period moves each backed category's figure today
    Given my budget periods are one month long
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have a budget of 200 euro for "Savings" in the previous budget period
    And I have set the backing account of "Savings" to "Deposit"
    Then the plan of the previous budget period should be offered in the current budget period, with a total of 600 euro
    When I take over the plan offered
    Then the take-over should go through
    And I should be told that the plan was taken over into the current budget period
    And the balance of "Bank" should be 1800 euro
    And the balance of "Deposit" should be 200 euro
    And Unassigned in the current budget period should be 1400 euro
    And Accumulated for "Savings" in the current budget period should be 200 euro

  Scenario: Taking a plan over into a later period moves each backed category's figure on that period's first day
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a budget of 200 euro for "Savings" in the current budget period
    And the Overview shows the next budget period
    When I take over the plan offered
    Then the take-over should go through
    And I should be told that the plan was taken over into the next budget period
    And the balance of "Bank" should still be 1800 euro
    And the balance of "Deposit" should still be 200 euro
    And Accumulated for "Savings" in the next budget period should be 400 euro
    When the next budget period begins while MoneyBud is open
    Then the balance of "Bank" should be 1600 euro
    And the balance of "Deposit" should be 400 euro

  # ----------------------------------------------------------------------------------
  # Movements and balance corrections
  #
  # A movement meets a balance correction as any entry does (correct-a-balance.feature).
  # ----------------------------------------------------------------------------------

  # Bank was corrected first, so the move recorded after it, on the same day, moves Bank's balance.
  Scenario: Money moved after a balance correction on the same day moves the balance
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have corrected the balance of "Bank" to 2000 euro
    When I assign 300 euro to "Savings" in the current budget period
    Then the balance of "Bank" should be 1700 euro
    And the balance of "Deposit" should be 5300 euro

  # Derived, kept after the follow-ups, and shown so that it is approved knowingly: the movement for
  # the next period was recorded when it was assigned, the day before, so a balance correction typed
  # on its day already has it in. Typing the 5300 the bank shows leaves the balance at 5300. Had the
  # movement counted as recorded after the balance correction, the balance would read 5600.
  Scenario: A balance correction typed on the day planned money moves already has that movement in it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the next budget period
    When the next budget period begins while MoneyBud is open
    Then the balance of "Deposit" should be 5300 euro
    When I correct the balance of "Deposit" to 5300 euro
    Then the balance of "Deposit" should be 5300 euro
    And the balance correction of "Deposit" to 5300 euro should show a difference of 0.00 euro
