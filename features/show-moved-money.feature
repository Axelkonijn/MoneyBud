# Seeing moved money in an account's history: every amount MoneyBud moves on a category's behalf is
# a row in both accounts' histories (glossary: "Moved money in the account's history"; "Backing and
# Accumulated", settled by the stakeholder on 2026-09-27). What moves, and when, is in
# back-a-category.feature and assign-to-a-backed-category.feature. An account's history itself is
# show-accounts.feature's. The steps the backing files share are explained in
# back-a-category.feature.
#
# The rules, from arc42 §12:
#   - MOVED MONEY SHOWS IN BOTH ACCOUNTS' HISTORIES, ONE ROW PER MOVEMENT, EACH ON ITS OWN DAY: each
#     assignment to a backed category, the move made on backing, and, since the revision of unbacking,
#     the moves made on unbacking and on re-pointing. Rejected: one row per period, because whether a
#     balance correction holds a movement depends on its day. Since the sweep increment, each sweep
#     and each difference moved by "Restant bijwerken" is a movement row too
#     (sweep-at-a-period-end.feature, bring-a-swept-period-up-to-date.feature).
#   - THE ROWS ARE READ-ONLY. Moved money is changed by assigning again, not from the history.
#   - Derived, not asked: a movement row cannot be removed from the history, and A NEGATIVE ASSIGNMENT
#     ADDS A ROW GOING THE OTHER WAY rather than changing the earlier one.
#   - Derived, kept after the follow-ups: MONEY PLANNED FOR A LATER PERIOD IS IN NO HISTORY BEFORE ITS
#     DAY, because which account it lands in is decided only on that day.
#   - Derived: assigning zero moves nothing, so it adds no row, and a clipped negative assignment adds
#     a row for only what came back.
#   - Moved money is neither an income nor an expense, so it is not in the Overview's lists, which
#     stay incomes and expenses only (show-accounts.feature).
#
# The proposed row wording, copy and not a ruling, is "Toegewezen aan Sparen — € 200,00". What is
# specified is what each row is and which way the money went, not how the row reads, and not whether
# it says what caused the movement.
#
# Money "moved" from the pool account to itself, when the pool account backs a category, is NO ROW:
# no balance changed (ruled at the scenario gate, 2026-09-27, over one row netting to zero). Its
# scenarios are in back-a-category.feature, under "The pool account may back a category".
#
# Reading the steps: in the history table, ENTRY "movement" is a row of moved money, CATEGORY the
# category it moved for, FROM and TO its two accounts, and AMOUNT the amount without a sign
# (back-a-category.feature). Rows are newest first, and rows on one date newest recorded first
# (show-accounts.feature). "in the history of "X" I should not be able to change or remove the
# movement of N euro for "Y"" means neither act is offered on that row.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names, labels and amounts are
# synthetic test data.

@backing
Feature: See money moved for a category in an account's history
  As someone who checks each balance against my bank
  I want every amount MoneyBud moves for a category to show in both accounts' histories, on the day it moved
  So that I can see why a balance changed, and make the same transfer at my bank

  # ----------------------------------------------------------------------------------
  # One row per movement, in both accounts
  # ----------------------------------------------------------------------------------

  # Backing moves 300, assigning moves 50 in, assigning -20 moves 20 back out, and unbacking returns
  # the 330 there for Savings. Four movements, four rows in each account, all today, newest first.
  Scenario: Every movement is a row of its own in both accounts' histories, and in neither period list
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    When I set the backing account of "Savings" to "Deposit"
    And I assign 50 euro to "Savings" in the current budget period
    And I assign -20 euro to "Savings" in the current budget period
    And I remove the backing of "Savings"
    Then the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | category | from    | to      | amount | balance |
      | today | movement         | Savings  | Deposit | Bank    | 330.00 |         |
      | today | movement         | Savings  | Deposit | Bank    | 20.00  |         |
      | today | movement         | Savings  | Bank    | Deposit | 50.00  |         |
      | today | movement         | Savings  | Bank    | Deposit | 300.00 |         |
      | today | starting balance |          |         |         |        | 5000.00 |
    And the history of "Bank" should be exactly these, newest first:
      | date  | entry    | category | label   | from    | to      | amount  |
      | today | movement | Savings  |         | Deposit | Bank    | 330.00  |
      | today | movement | Savings  |         | Deposit | Bank    | 20.00   |
      | today | movement | Savings  |         | Bank    | Deposit | 50.00   |
      | today | movement | Savings  |         | Bank    | Deposit | 300.00  |
      | today | income   |          | Salaris |         |         | 2000.00 |
    And the balance of "Bank" should be 2000 euro
    And the balance of "Deposit" should be 5000 euro
    And no expenses should be listed in the current budget period
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | today | Salaris | 2000.00 |

  Scenario: A movement cannot be changed or removed from either account's history
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    Then in the history of "Deposit" I should not be able to change or remove the movement of 300 euro for "Savings"
    And in the history of "Bank" I should not be able to change or remove the movement of 300 euro for "Savings"

  # Re-pointing moves the money from the old account to the new one: a row in each of those two. The
  # pool account is not part of it.
  Scenario: Re-pointing is a row in the old and the new backing account, and none in the pool account's
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    When I set the backing account of "Savings" to "Broker"
    Then the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | category | from    | to      | amount | balance |
      | today | movement         | Savings  | Deposit | Broker  | 200.00 |         |
      | today | movement         | Savings  | Bank    | Deposit | 200.00 |         |
      | today | starting balance |          |         |         |        | 0.00    |
    And the history of "Broker" should be exactly these, newest first:
      | date  | entry            | category | from    | to     | amount | balance |
      | today | movement         | Savings  | Deposit | Broker | 200.00 |         |
      | today | starting balance |          |         |        |        | 0.00    |
    And the history of "Bank" should be exactly these, newest first:
      | date  | entry    | category | label   | from | to      | amount  |
      | today | movement | Savings  |         | Bank | Deposit | 200.00  |
      | today | income   |          | Salaris |      |         | 2000.00 |

  # ----------------------------------------------------------------------------------
  # Planned money is in no history until its day
  # ----------------------------------------------------------------------------------

  # Derived, kept after the follow-ups. The starting balance was typed on what is now the last day
  # of the previous period.
  Scenario: Money assigned for a later period is in no history until that period's first day, and is dated on it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I assign 300 euro to "Savings" in the next budget period
    Then the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | balance |
      | today | starting balance | 0.00    |
    When the next budget period begins while MoneyBud is open
    Then the history of "Deposit" should be exactly these, newest first:
      | date                                       | entry            | category | from | to      | amount | balance |
      | the first day of the current budget period | movement         | Savings  | Bank | Deposit | 300.00 |         |
      | the last day of the previous budget period | starting balance |          |      |         |        | 0.00    |

  # ----------------------------------------------------------------------------------
  # Zero and clipped amounts
  #
  # Assigning zero moves nothing, so it adds no row. -50 against a budget of 30 is clipped: 30 comes
  # back, and that is the row.
  # ----------------------------------------------------------------------------------

  Scenario: Assigning zero adds no row, and a clipped negative assignment adds a row for what came back
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 30 euro for "Savings" in the current budget period
    When I assign 0 euro to "Savings" in the current budget period
    And I assign -50 euro to "Savings" in the current budget period
    Then I should be told of a shortfall of 20 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | category | from    | to      | amount | balance |
      | today | movement         | Savings  | Deposit | Bank    | 30.00  |         |
      | today | movement         | Savings  | Bank    | Deposit | 30.00  |         |
      | today | starting balance |          |         |         |        | 0.00    |
