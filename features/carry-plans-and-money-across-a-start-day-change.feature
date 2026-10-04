# What a change of the period start day does to plans and money (glossary: "A configurable period start
# day", settled by the stakeholder on 2026-09-29: ruling 4, follow-ups 2 and 5, "What else a change
# meets"). Changing the start day itself, and what it does to the periods, is in
# change-the-period-start-day.feature, which explains the steps the three start-day files share.
#
# The rules, from arc42 §12:
#   - RULING 4. A PLAN MADE AHEAD FOR A PERIOD THAT NO LONGER EXISTS GOES INTO THE PERIOD ITS OLD FIRST DAY
#     FALLS IN. Assigned ahead to October, and the start day then changed to the 27th: October's plan goes
#     to 27 September to 26 October, the period that is mostly October. FOR A BACKED CATEGORY, ITS MONEY
#     THEN MOVES ON THAT PERIOD'S FIRST DAY INSTEAD. Rejected: giving it back to Unassigned.
#   - FOLLOW-UP 2. MONEY A CHANGE MAKES MONEYBUD MOVE IS DATED THE DAY OF THE CHANGE: the sweep of a period
#     the change ended, and a backed category's plan made ahead for a period that has ALREADY BEGUN under
#     the new calendar. Rejected: the new period's first day, which a balance correction dated between the
#     two would take in, so the balance would not rise although MoneyBud says the money moved.
#   - FOLLOW-UP 5, in the stakeholder's own words: A CHANGE NEVER CHANGES OPGEBOUWD. It is Resterend summed
#     over every period since the backing, plus what sweeps brought in, COUNTED FROM THE FIRST DAY THE
#     PERIOD OF BACKING HAD WHEN THE CATEGORY WAS BACKED, the period cut short included. A change only moves
#     expenses between the two halves of the period it cuts, so the sum is unchanged.
#
# DERIVED BY THE DOCUMENTATION AND NOT PUT TO THE STAKEHOLDER, marked where a scenario rests on one:
#   - A PLAN LANDING IN THE NEW CURRENT PERIOD MEANS IT HAS A PLAN, so it is not offered one. With nothing
#     planned ahead it is offered the plan of the period cut short (change-the-period-start-day.feature).
#   - A plan made ahead for a period that begins AFTER the change still moves on that period's first day:
#     nothing moves it early.
#   - A CHANGE THAT ENDS THE CURRENT PERIOD ENDS IT LIKE ANY PERIOD'S END: it is swept, the sweep is
#     announced once and saved straight away, and it is dated the day of the change. The categories counted
#     as unbacked are those unbacked AT THE CHANGE, so a category backed during the period cut short had
#     its Remaining moved at backing and is not swept a second time.
#   - WHAT IS THERE FOR A CATEGORY, which unbacking returns and re-pointing takes along, FOLLOWS FOLLOW-UP 5
#     TOO: a change never changes it.
#   - A change moves NO OTHER MONEY.
#
# REVISED FOR INCREMENT 15, 2026-10-04 (glossary: "Vrij, and moving Opgebouwd", ruling 6; not yet
# approved). Setting Staat op to "—" no longer returns what is there for a category: it returns this
# period's Remaining, and leaves the rest on the account. So "what unbacking returns" now depends on which
# period is the current one, which a change of start day does change. What a change still never changes is
# Accumulated, and what is there for the category, which re-pointing takes along whole. The last scenario,
# "A change never changes Accumulated, nor what unbacking returns", is revised to re-point instead of
# setting "—", and its title follows. What "—" does after a change is not specified here: it follows the
# rules of back-a-category.feature for whichever period is current.
#
# NOT specified here: two plans for one category landing in one period. They would add up, but the
# documentation found no change that makes it happen, since each old first day falls in a different new
# period. (Found at the build, 2026-09-29: two changes in one period do it. To the 30th on 29 September,
# a plan made for the period from 30 September, then back to the 1st: that plan lands in September
# beside its own, they add up, and a backed category's money for it moves at once, dated the day of the
# change. A developer test holds it.) The words of the notices, which are copy.
#
# RULED BY THE STAKEHOLDER AT THE SCENARIO STAGE, 2026-09-29, on a point raised while writing these
# scenarios: A PERIOD ENDED BY A CHANGE IS SWEPT BY ITSELF EVEN WHEN ITS NEW END FALLS BEFORE MONEYBUD WAS
# FIRST STARTED, when it was part of the period current at the first start. The sweep is otherwise
# automatic only for a period that ends after the first start (ruling 7 of the sweep,
# sweep-at-a-period-end.feature). Here the period was current when MoneyBud was first started, and the
# change ends it like any period's end. Rejected: leaving it to the button, as a period that ended before
# the first start is. A period that was over before the first start, and never current, is still not
# swept by itself.
#
# Reading the steps: see change-the-period-start-day.feature. The backing steps are back-a-category.feature's,
# the sweep steps sweep-at-a-period-end.feature's, the account and history steps show-accounts.feature's,
# and "I have a budget of N euro for "X" in the budget period 2 after the current one" is assign-to-
# category.feature's budget Given, for a period further on than the next.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). No category in it is backed, and it has no
# sweep destination unless a scenario sets one. The names, labels and amounts are synthetic test data.

@periods
Feature: Carry plans and money across a change of the period start day
  As someone who plans ahead and saves in accounts kept for it
  I want my plans made ahead, and money built up for a purpose, to come through a change of the day my periods start on
  So that changing to payday costs me no work done ahead and puts no figure of mine wrong

  # ----------------------------------------------------------------------------------
  # A plan made ahead (ruling 4)
  # ----------------------------------------------------------------------------------

  # On 29 September the next period is October and the one after it November. Both were planned. After
  # the change to the 27th, October's 1st is in 27 September to 26 October, which is now current, and
  # November's 1st is in 27 October to 26 November, now the next. The current period has a plan, so it is
  # not offered one, and the salary of the 27th is its Unassigned, less October's plan.
  Scenario: A plan made ahead goes into the period its old first day falls in
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 September 2026
    And I have a budget of 400 euro for "Groceries" in the next budget period
    And I have a budget of 900 euro for "Rent" in the next budget period
    And I have a budget of 380 euro for "Groceries" in the budget period 2 after the current one
    When I change the period start day to the 27th and confirm
    Then the budget for "Groceries" in the current budget period should be 400 euro
    And the budget for "Rent" in the current budget period should be 900 euro
    And the budget for "Groceries" in the next budget period should be 380 euro
    And the budget for "Rent" in the next budget period should be 0.00 euro
    And the budget for "Groceries" in the previous budget period should be 0.00 euro
    And Unassigned in the current budget period should be 1200 euro
    And no plan should be offered in the current budget period

  # ----------------------------------------------------------------------------------
  # A backed category's plan made ahead (ruling 4, follow-up 2)
  # ----------------------------------------------------------------------------------

  # Savings was backed by Deposit, and planned 200 for October and 150 for November, neither moved yet.
  # October's plan lands in 27 September to 26 October, which began two days before the change, so its
  # 200 moves at once, dated the day of the change. It does not move again on 1 October. November's
  # lands in 27 October to 26 November, which has not begun, so its 150 moves on 27 October, that period's
  # first day, not on 1 November and not early.
  Scenario: A backed category's plan made ahead moves on the day of the change if its new period has begun, and otherwise on that period's first day
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have recorded an income of 3000 euro labelled "Salaris" dated 1 September 2026
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the next budget period
    And I have a budget of 150 euro for "Savings" in the budget period 2 after the current one
    And the balance of "Deposit" is 0.00 euro
    When I change the period start day to the 27th and confirm
    Then the budget for "Savings" in the current budget period should be 200 euro
    And the budget for "Savings" in the next budget period should be 150 euro
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 2800.00 |
      | Deposit | 200.00  |
    And Accumulated for "Savings" in the current budget period should be 200 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date              | entry            | category | from | to      | amount | balance |
      | 29 September 2026 | movement         | Savings  | Bank | Deposit | 200.00 |         |
      | 29 September 2026 | starting balance |          |      |         |        | 0.00    |
    When the day becomes 26 October 2026 while MoneyBud is open
    Then the balance of "Deposit" should still be 200 euro
    When the day becomes 27 October 2026 while MoneyBud is open
    Then the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 2650.00 |
      | Deposit | 350.00  |
    And the history of "Deposit" should be exactly these, newest first:
      | date              | entry            | category | from | to      | amount | balance |
      | 27 October 2026   | movement         | Savings  | Bank | Deposit | 150.00 |         |
      | 29 September 2026 | movement         | Savings  | Bank | Deposit | 200.00 |         |
      | 29 September 2026 | starting balance |          |      |         |        | 0.00    |

  # ----------------------------------------------------------------------------------
  # The sweep of the period a change ends (follow-up 2, and derived)
  # ----------------------------------------------------------------------------------

  # MoneyBud was first started on 1 September, with Savings as the sweep destination. Hobby was backed on
  # the 28th, which moved its unspent 100 then. On the 29th the change ends 1 to 26 September on the spot,
  # and it is swept like any period's end: its Unassigned, 1000 less the 500 planned, is 500, and
  # Groceries' Remaining, 400 less Markt's 100, is 300. Hobby was backed at the change, so it is not swept
  # again: the period leftover is 800, not 900. The sweep is dated the day of the change, announced with
  # the change, and saved, so starting again says nothing more. Bank ends with what belongs to the new
  # current period: the salary of the 27th less Bakker of the 28th.
  Scenario: The period a change ends is swept at once, dated the day of the change, announced once, and not swept for a category backed by then
    Given my budget periods are one month long
    And today is 1 September 2026
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a budget of 100 euro for "Hobby" in the current budget period
    And I have recorded an income of 1000 euro labelled "Voorschot" dated 10 September 2026
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 September 2026
    When the day becomes 28 September 2026 while MoneyBud is open
    And I set the backing account of "Hobby" to "Deposit"
    And the day becomes 29 September 2026 while MoneyBud is open
    And I record an expense of 100 euro for "Groceries" labelled "Markt" dated 20 September 2026
    And I record an expense of 30 euro for "Groceries" labelled "Bakker" dated 28 September 2026
    And I change the period start day to the 27th and confirm
    Then I should be told that budget periods now start on the 27th
    And I should be told that the period leftover of the previous budget period, 800 euro, was swept into "Savings"
    And the previous budget period should show that 800 euro was swept into "Savings"
    And I should not be able to bring the swept amount of the previous budget period up to date
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 2470.00 |
      | Deposit | 900.00  |
    And the history of "Deposit" should be exactly these, newest first:
      | date              | entry            | category | from | to      | amount | balance |
      | 29 September 2026 | movement         | Savings  | Bank | Deposit | 800.00 |         |
      | 28 September 2026 | movement         | Hobby    | Bank | Deposit | 100.00 |         |
      | 1 September 2026  | starting balance |          |      |         |        | 0.00    |
    And Accumulated for "Savings" in the current budget period should be 800 euro
    When I close MoneyBud and start it again
    Then I should not have been told anything
    And the balance of "Deposit" should still be 900 euro

  # Ruled at the scenario stage (see the header). MoneyBud was first started on 28 September, in September.
  # On the 29th the change ends 1 to 26 September, which now ends before that first start, and it is swept
  # anyway: it was part of the period current at the first start. August was over before the first start
  # and never current, so its 800 is still not swept by itself, and still waits for the button.
  Scenario: A period ended by a change is swept by itself even when its new end is before the first start, if it was part of the period current then
    Given my budget periods are one month long
    And today is 28 September 2026
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have recorded an income of 800 euro labelled "Salaris augustus" dated 10 August 2026
    And I have recorded an income of 1000 euro labelled "Salaris" dated 10 September 2026
    When the day becomes 29 September 2026 while MoneyBud is open
    And I change the period start day to the 27th and confirm
    Then I should be told that the period leftover of the previous budget period, 1000 euro, was swept into "Savings"
    And the previous budget period should show that 1000 euro was swept into "Savings"
    And the budget period 2 before the current one should still show 800 euro of its period leftover still to sweep
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 800.00  |
      | Deposit | 1000.00 |
    And the history of "Deposit" should be exactly these, newest first:
      | date              | entry            | category | from | to      | amount  | balance |
      | 29 September 2026 | movement         | Savings  | Bank | Deposit | 1000.00 |         |
      | 28 September 2026 | starting balance |          |      |         |         | 0.00    |
    When I close MoneyBud and start it again
    Then I should not have been told anything
    And the balance of "Deposit" should still be 1000 euro

  # ----------------------------------------------------------------------------------
  # A change never changes Opgebouwd (follow-up 5)
  # ----------------------------------------------------------------------------------

  # The glossary's example. Savings was planned 300 in September, 50 spent on the 10th from Bank, and
  # backed on the 28th, which moved the 250 left. Boek, dated the 28th, was paid from Deposit, so
  # Opgebouwd is 230, what September's Resterend was. The change splits September in two: 1 to 26
  # September keeps the plan and Cadeau, Resterend 250, and 27 September to 26 October has Boek and no
  # plan, Resterend -20. Opgebouwd is still their sum, 230, and no money moves. What is there for Savings
  # in Deposit is unchanged too (derived), so unbacking returns the same 230 it would have before.
  #
  # Revised for increment 15 (see the header). The last step first set Savings to "—", which returned all
  # 230. "—" now returns only the current period's Remaining, which the change has made -20, so it no
  # longer shows what is there for Savings. Pointing the backing at Broker does: it takes all of it along,
  # the same 230 as before the change.
  Scenario: A change never changes Accumulated, nor what pointing the backing elsewhere takes along
    Given my budget periods are one month long
    And today is 28 September 2026
    And I have recorded an income of 2000 euro labelled "Salaris" dated 1 September 2026
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have recorded an expense of 50 euro for "Savings" labelled "Cadeau" dated 10 September 2026
    And I have set the backing account of "Savings" to "Deposit"
    And Accumulated for "Savings" in the current budget period is 250 euro
    When the day becomes 29 September 2026 while MoneyBud is open
    And I record an expense of 20 euro for "Savings" labelled "Boek" dated 28 September 2026 on the account "Deposit"
    Then Accumulated for "Savings" in the current budget period should be 230 euro
    When I change the period start day to the 27th and confirm
    Then Accumulated for "Savings" in the current budget period should still be 230 euro
    And the remaining "Savings" budget in the previous budget period should be 250 euro
    And the remaining "Savings" budget in the current budget period should be -20 euro
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1700.00 |
      | Deposit | 230.00  |
      | Broker  | 0.00    |
    When I set the backing account of "Savings" to "Broker"
    Then I should be told that "Savings" is now backed by "Broker", and that 230 euro moved from "Deposit" to "Broker"
