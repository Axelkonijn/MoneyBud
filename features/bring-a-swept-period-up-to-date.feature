# Bringing a swept period up to date: a period never closes, so its figures can change after its
# leftover was swept. MoneyBud shows the difference, and one click moves exactly that (glossary: "A
# swept period that changes: shown, and one click moves the difference"; "A late expense against a
# leftover that has already been directed"; "Corrections and the sweep"; "The sweep and Restant",
# settled by the stakeholder on 2026-09-27). What moves at a period's end is in
# sweep-at-a-period-end.feature, which explains the steps the sweep files share, and what an ended
# period shows is in show-an-ended-period.feature.
#
# This answers the last open question in the glossary: what becomes of an income back-dated into a
# period that was already swept. It is swept too, on my click, into today's destination.
#
# The rules, from arc42 §12:
#   - WHEN AN ENDED PERIOD THAT WAS ALREADY SWEPT CHANGES (a late expense, a back-dated income, or any
#     correction, changing or removing an entry) MONEYBUD SHOWS IT, AND ONE CLICK MOVES THE
#     DIFFERENCE, to or from the destination. MONEYBUD NEVER ADJUSTS BY ITSELF (ruling 1). Rejected:
#     adjusting automatically, which overturns the late-expense rule; showing only, which leaves a
#     transfer as the only tool and Accumulated wrong; a late income joining the current period's
#     Unassigned, which is an exception to record-income.feature.
#   - THE BUTTON, BY DIRECTION (ruling 5). An amount STILL TO SWEEP goes to TODAY'S destination. An
#     amount SWEPT TOO MUCH comes back FROM THE CATEGORY IT WAS ACTUALLY SWEPT INTO, so a category that
#     never received the money never loses any. Rejected: today's destination both ways.
#   - AN OVER-SWEEP COMES BACK FROM THE CATEGORY'S CURRENT BACKING ACCOUNT, AT MOST WHAT IS THERE FOR
#     IT, like the cap on a negative assignment. IF THE CATEGORY IS NO LONGER BACKED, NOTHING MOVES,
#     because unbacking already returned its money, and THE LINE STOPS ASKING (follow-up). Rejected:
#     only from the account it was first swept into.
#     (REVISED FOR INCREMENT 15, 2026-10-04, not yet approved; glossary: "Vrij, and moving Opgebouwd",
#     follow-up 14. Setting Staat op to "—" no longer returns swept money: it stays on the account, still
#     the category's. So AN OVER-SWEEP FROM A CATEGORY ON "—" COMES BACK OUT OF THE MONEY IT LEFT BEHIND,
#     AT MOST THAT, from the account it is on to the pool account. Nothing moves, and the line stops
#     asking, only when it left nothing behind. The scenario "When the category the money was swept into
#     is no longer backed, nothing comes back, and the line stops asking" is revised below, and one
#     scenario is new: the cap.)
#   - WHEN A PERIOD'S LEFTOVER WENT TO TWO CATEGORIES, AN OVER-SWEEP IS TAKEN BACK LATEST FIRST: the
#     most recent move for that period is undone first, then the one before (follow-up). Rejected: in
#     proportion to what each received.
#   - ONE BUTTON, "Restant bijwerken", FOR BOTH DIRECTIONS (follow-up).
#   - Redirecting a past sweep to another category is DEFERRED UNTIL MISSED (ruling 4).
#
# DERIVED BY THE DOCUMENTATION AND NOT PUT TO THE STAKEHOLDER, marked where a scenario rests on one:
#   - THE DIFFERENCE for a period is what it should sweep now (its period leftover, floored at zero)
#     minus what has been swept for it so far, net. Above zero it is still to sweep; below zero, swept
#     too much.
#   - A PERIOD NEVER SWEPT, for want of a destination or because it ended before the first start, uses
#     the same button: nothing has been swept for it so far.
#   - THE BUTTON IS NOT CONFIRMED, and A NOTICE SAYS WHAT MOVED afterwards.
#   - WITH NO DESTINATION AND MONEY STILL TO SWEEP, the button cannot move anything, and the line says
#     the money is not swept. Ruled at the scenario gate, 2026-09-27: the button is then NOT OFFERED.
#
# IN THE DOCUMENTATION'S READING, open to contradiction at this gate:
#   - What the button moves is A NEW MOVEMENT, DATED THE DAY IT IS PRESSED, from the pool account of
#     that day or back to it. The first sweep is never changed, so the history keeps both.
#   - IT CHANGES NO BUDGET, in any period: an ended period's plan still cannot change.
#   - Taking back MAY OVERDRAW the backing account the money leaves, when that money was spent on
#     something else, as unbacking may. Capped at what is there for the category, it overdraws only as
#     far as that figure does.
#   - WHEN THE CAP LEAVES PART OF AN OVER-SWEEP BEHIND, THE LINE STOPS ASKING FOR THAT PART TOO, as it
#     does after unbacking, with no second shortfall message.
#
# RULED BY THE STAKEHOLDER AT THE SCENARIO GATE, 2026-09-27:
#   - ONCE THE LINE STOPS ASKING, IT STOPS FOR GOOD: money later built up for the category does not
#     make it ask again. Later changes to the period are measured against what really moved.
#   - THE LINE THEN NAMES WHAT REALLY WENT TO THE CATEGORY, not the period leftover as it is now.
#     Rejected: the period leftover now; "te veel weggezet" with no button.
#   - A PERIOD WHOSE LEFTOVER WENT TO TWO CATEGORIES has a line naming each with its amount, and a
#     category whose share has been taken back to nothing drops off the line.
#
# Reading the steps: see sweep-at-a-period-end.feature. The correcting steps are
# change-an-entry.feature's and remove-an-entry.feature's, the backing steps back-a-category.feature's,
# the account steps show-accounts.feature's. "a while passes with nothing done" is keep-data.feature's:
# the once-a-minute refresh, which is when MoneyBud would adjust by itself if it ever did.
#
# Most scenarios start the same way: a salary of 2000 and one 100 euro receipt against a Groceries
# budget of 400 leave Unassigned 1600 and Groceries 300, so 1900 is swept from Bank into Savings,
# backed by Deposit, when the period ends. Bank then holds 0 and Deposit 1900.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature), first started in the current budget
# period (sweep-at-a-period-end.feature). The names, labels and amounts are synthetic test data.

@sweep
Feature: Bring a swept period up to date
  As someone who finds receipts and refunds after the month is over
  I want a period whose money was already swept to show how far off the sweep now is, and to put it right with one click
  So that my savings hold what each month really left over, without MoneyBud moving money behind my back

  # ----------------------------------------------------------------------------------
  # Swept too much, and more to sweep
  # ----------------------------------------------------------------------------------

  # The glossary's example, with synthetic figures. A 40 euro receipt from the last day of the ended
  # period, found afterwards. Nothing moves until I press the button, not even when the screen
  # refreshes. Then exactly 40 comes back, dated today, beside the sweep rather than instead of it,
  # and no budget changes.
  Scenario: A late expense in a swept period shows what was swept too much, and one click takes exactly that back
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 1900 euro, was swept into "Savings"
    When I record an expense of 40 euro for "Groceries" labelled "Bon" dated on the last day of the previous budget period
    Then the previous budget period should show 40 euro of its period leftover swept too much
    And I should be able to bring the swept amount of the previous budget period up to date
    And the balance of "Bank" should be -40 euro
    And the balance of "Deposit" should still be 1900 euro
    When a while passes with nothing done
    Then the previous budget period should still show 40 euro of its period leftover swept too much
    And the balance of "Deposit" should still be 1900 euro
    When I bring the swept amount of the previous budget period up to date
    Then I should be told that 40 euro swept too much for the previous budget period was taken back from "Savings"
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 1860.00 |
    And the previous budget period should show that 1860 euro was swept into "Savings"
    And I should not be able to bring the swept amount of the previous budget period up to date
    And Accumulated for "Savings" in the current budget period should be 1860 euro
    And the budget for "Groceries" in the previous budget period should still be 400 euro
    And Unassigned in the previous budget period should still be 1600 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date                                       | entry            | category | from    | to      | amount  | balance |
      | today                                      | movement         | Savings  | Deposit | Bank    | 40.00   |         |
      | today                                      | movement         | Savings  | Bank    | Deposit | 1900.00 |         |
      | the last day of the previous budget period | starting balance |          |         |         |         | 0.00    |

  # The open question, answered: a refund back-dated into a swept period is swept too, on my click,
  # into the destination. MoneyBud is started again a period later, so the day I press the button is
  # not the day of the sweep, and the history shows each on its own day. The period in between had
  # no income, so nothing was swept for it, and nothing was said.
  Scenario: A back-dated income in a swept period shows more to sweep, and one click sweeps it, dated the day I press it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    And I close MoneyBud, and start it again on the first day of the next budget period
    Then I should not have been told anything
    When I record an income of 100 euro labelled "Terugbetaling" dated on the last day of the budget period 2 before the current one
    Then the budget period 2 before the current one should show 100 euro of its period leftover still to sweep
    And I should be able to bring the swept amount of the budget period 2 before the current one up to date
    When I bring the swept amount of the budget period 2 before the current one up to date
    Then I should be told that 100 euro more of the period leftover of the budget period 2 before the current one was swept into "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 2000.00 |
    And the budget period 2 before the current one should show that 2000 euro was swept into "Savings"
    And the history of "Deposit" should be exactly these, newest first:
      | date                                                       | entry            | category | from | to      | amount  | balance |
      | today                                                      | movement         | Savings  | Bank | Deposit | 100.00  |         |
      | the first day of the previous budget period                | movement         | Savings  | Bank | Deposit | 1900.00 |         |
      | the last day of the budget period 2 before the current one | starting balance |          |      |         |         | 0.00    |

  # ----------------------------------------------------------------------------------
  # Every correction meets the same rule
  #
  # The corrections table of the glossary, all four rows. The base adds a deposit refund, Statiegeld,
  # so that one income can be removed: 1925 is swept. Kiosk is an expense in the new period, which
  # one row moves into the swept one. Whichever way the swept figure moves, the period shows the
  # difference, and the button moves exactly that.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A correction that raises a swept period's spending or lowers its income shows what was swept too much, and the button takes it back
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have recorded an income of 25 euro labelled "Statiegeld" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 1925 euro, was swept into "Savings"
    When I record an expense of 30 euro for "Groceries" labelled "Kiosk"
    And <correction>
    Then the previous budget period should show <difference> euro of its period leftover swept too much
    When I bring the swept amount of the previous budget period up to date
    Then I should be told that <difference> euro swept too much for the previous budget period was taken back from "Savings"
    And the balance of "Deposit" should be <swept> euro
    And the previous budget period should show that <swept> euro was swept into "Savings"

    Examples:
      | correction                                                                                    | difference | swept   |
      | I change the amount of the expense labelled "Markt" to 140 euro                               | 40.00      | 1885.00 |
      | I change the date of the expense labelled "Kiosk" to the last day of the previous budget period | 30.00      | 1895.00 |
      | I change the amount of the income labelled "Salaris" to 1950 euro                             | 50.00      | 1875.00 |
      | I remove the income labelled "Statiegeld" and confirm                                         | 25.00      | 1900.00 |
      | I change the date of the income labelled "Statiegeld" to today                                | 25.00      | 1900.00 |

  Scenario Outline: A correction that lowers a swept period's spending or raises its income shows more to sweep, and the button sweeps it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have recorded an income of 25 euro labelled "Statiegeld" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 1925 euro, was swept into "Savings"
    When <correction>
    Then the previous budget period should show <difference> euro of its period leftover still to sweep
    When I bring the swept amount of the previous budget period up to date
    Then I should be told that <difference> euro more of the period leftover of the previous budget period was swept into "Savings"
    And the balance of "Deposit" should be <swept> euro
    And the previous budget period should show that <swept> euro was swept into "Savings"

    Examples:
      | correction                                                       | difference | swept   |
      | I change the amount of the expense labelled "Markt" to 60 euro   | 40.00      | 1965.00 |
      | I remove the expense labelled "Markt" and confirm                | 100.00     | 2025.00 |
      | I change the date of the expense labelled "Markt" to today       | 100.00     | 2025.00 |
      | I change the amount of the income labelled "Salaris" to 2050 euro | 50.00      | 1975.00 |

  # ----------------------------------------------------------------------------------
  # Which category: more goes to today's destination, too much comes back from where it went
  # ----------------------------------------------------------------------------------

  # Ruling 5. The destination is now Holiday, so the refund goes there. The period's leftover has now
  # gone to two categories, and its line names both (ruled at the scenario gate, 2026-09-27).
  Scenario: Money still to sweep goes to today's destination, even when the period was first swept into another
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have a category "Holiday"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Holiday" to "Broker"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    And I set the sweep destination to "Holiday"
    And I record an income of 100 euro labelled "Terugbetaling" dated on the last day of the previous budget period
    And I bring the swept amount of the previous budget period up to date
    Then I should be told that 100 euro more of the period leftover of the previous budget period was swept into "Holiday"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 1900.00 |
      | Broker  | 100.00  |
    And Accumulated for "Holiday" in the current budget period should be 100 euro
    And the previous budget period should show that its period leftover was swept into these categories:
      | category | amount  |
      | Savings  | 1900.00 |
      | Holiday  | 100.00  |

  # Ruling 5: Holiday never received this period's money, so it loses none of it.
  Scenario: Money swept too much comes back from the category it was swept into, not from today's destination
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have a category "Holiday"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Holiday" to "Broker"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    And I set the sweep destination to "Holiday"
    And I record an expense of 40 euro for "Groceries" labelled "Bon" dated on the last day of the previous budget period
    And I bring the swept amount of the previous budget period up to date
    Then I should be told that 40 euro swept too much for the previous budget period was taken back from "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 1860.00 |
      | Broker  | 0.00    |
    And the sweep destination shown in the current budget period should still be "Holiday"

  # The follow-up. Re-pointing Savings took its money, the swept 1900 included, along to Broker, so
  # that is where the 40 is now, and where it comes back from.
  Scenario: After re-pointing, money swept too much comes back from the category's current backing account
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    And I set the backing account of "Savings" to "Broker"
    Then I should be told that "Savings" is now backed by "Broker", and that 1900 euro moved from "Deposit" to "Broker"
    When I record an expense of 40 euro for "Groceries" labelled "Bon" dated on the last day of the previous budget period
    And I bring the swept amount of the previous budget period up to date
    Then I should be told that 40 euro swept too much for the previous budget period was taken back from "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 0.00    |
      | Broker  | 1860.00 |

  # The follow-up. The 300 refund went to Holiday last, so it is the first to be undone: all 300
  # from Holiday, then the other 100 from Savings. Each category loses at most what it received for
  # this period. Holiday's share is then nothing, and the line names Savings alone (ruled at the
  # scenario gate, 2026-09-27).
  Scenario: When a period's leftover went to two categories, money swept too much is taken back latest first
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have a category "Holiday"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Holiday" to "Broker"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    And I set the sweep destination to "Holiday"
    And I record an income of 300 euro labelled "Terugbetaling" dated on the last day of the previous budget period
    And I bring the swept amount of the previous budget period up to date
    And I record an expense of 400 euro for "Groceries" labelled "Bon" dated on the last day of the previous budget period
    Then the previous budget period should show 400 euro of its period leftover swept too much
    When I bring the swept amount of the previous budget period up to date
    Then I should be told that 300 euro swept too much for the previous budget period was taken back from "Holiday"
    And I should be told that 100 euro swept too much for the previous budget period was taken back from "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 1800.00 |
      | Broker  | 0.00    |
    And the previous budget period should show that 1800 euro was swept into "Savings"

  # ----------------------------------------------------------------------------------
  # What is there for the category limits what comes back
  # ----------------------------------------------------------------------------------

  # The follow-up and the documentation's reading. 1880 of Savings' money was spent from Deposit, so
  # only 20 is there for it. The 50 swept too much comes back as far as that goes, 20, and the line
  # stops asking for the other 30, with no second message. Bank is left 30 short, which is true: the
  # receipt was paid and the money is not there. Ruled at the scenario gate on 2026-09-27: it stops
  # for good, so assigning more to Savings later does not make the line ask again, and the line then
  # names what really went to Savings, 1880.
  Scenario: Money swept too much comes back only as far as what is there for the category, and the line then stops asking
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    And I record an expense of 1880 euro for "Savings" labelled "Auto" on the account "Deposit"
    And I record an expense of 50 euro for "Groceries" labelled "Bon" dated on the last day of the previous budget period
    Then the previous budget period should show 50 euro of its period leftover swept too much
    When I bring the swept amount of the previous budget period up to date
    Then I should be told that 20 euro swept too much for the previous budget period was taken back from "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance | overdrawn |
      | Bank    | -30.00  | yes       |
      | Deposit | 0.00    | no        |
    And Accumulated for "Savings" in the current budget period should be 0.00 euro
    And the previous budget period should show that 1880 euro was swept into "Savings"
    And I should not be able to bring the swept amount of the previous budget period up to date
    When I assign 100 euro to "Savings" in the current budget period
    Then I should still not be able to bring the swept amount of the previous budget period up to date

  # The follow-up, as first ruled: unbacking Savings had already returned everything there for it, the
  # swept 1900 included, to Bank, so nothing moved for the 40, and the line stopped asking. That the
  # line names what really went to Savings was ruled at the scenario gate on 2026-09-27.
  #
  # Revised for increment 15 (follow-up 14). "—" now returns only this period's money, and Savings has
  # none this period, so nothing moves: the swept 1900 stays on Deposit, still Savings'. The 40 swept too
  # much comes back out of it, as it would from a backing account, and the line names what really went.
  Scenario: When the category the money was swept into is set to none, money swept too much comes back out of the money it left behind
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    And I remove the backing of "Savings"
    Then I should be told that "Savings" is no longer backed, and of no money moved
    When I record an expense of 40 euro for "Groceries" labelled "Bon" dated on the last day of the previous budget period
    Then the previous budget period should show 40 euro of its period leftover swept too much
    And I should be able to bring the swept amount of the previous budget period up to date
    When I bring the swept amount of the previous budget period up to date
    Then I should be told that 40 euro swept too much for the previous budget period was taken back from "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 1860.00 |
    And Accumulated for "Savings" in the current budget period should be 1860 euro, on "Deposit"
    And the previous budget period should show that 1860 euro was swept into "Savings"
    And I should not be able to bring the swept amount of the previous budget period up to date

  # New in increment 15 (follow-up 14): at most what it left behind. 1880 of the 1900 was moved to
  # Unassigned (reallocate-an-amount.feature), so 20 is left on Deposit for Savings. The 50 swept too
  # much comes back as far as that goes, 20, and the line stops asking for the other 30, for good, as for
  # any capped take-back. Nothing is left for Savings, so it shows no Accumulated.
  Scenario: Money swept too much comes back out of what a category on none left behind only as far as that goes
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    And I remove the backing of "Savings"
    And I reallocate 1880 euro from "Savings" to Unassigned in the current budget period
    And I record an expense of 50 euro for "Groceries" labelled "Bon" dated on the last day of the previous budget period
    Then the previous budget period should show 50 euro of its period leftover swept too much
    When I bring the swept amount of the previous budget period up to date
    Then I should be told that 20 euro swept too much for the previous budget period was taken back from "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1850.00 |
      | Deposit | 0.00    |
    And "Savings" should show no Accumulated in the current budget period
    And the previous budget period should show that 1880 euro was swept into "Savings"
    And I should not be able to bring the swept amount of the previous budget period up to date

  # The documentation's reading. Deposit paid a 1900 Groceries bill, which is not Savings money, so
  # 1900 is still there for Savings and the 40 comes back in full. Deposit goes into the red: the
  # money moved there for Savings was spent on something else. Shown, never blocked.
  Scenario: Taking money back may overdraw the backing account, when the money there was spent on something else
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    And I record an expense of 1900 euro for "Groceries" labelled "Verbouwing" on the account "Deposit"
    And I record an expense of 40 euro for "Groceries" labelled "Bon" dated on the last day of the previous budget period
    And I bring the swept amount of the previous budget period up to date
    Then I should be told that 40 euro swept too much for the previous budget period was taken back from "Savings"
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance | overdrawn |
      | Bank    | 0.00    | no        |
      | Deposit | -40.00  | yes       |
    And "Deposit" should be shown as overdrawn, with the marker a category over budget has and the badge "Rood"

  # ----------------------------------------------------------------------------------
  # Periods never swept, and periods that ended short
  # ----------------------------------------------------------------------------------

  # Derived: a period never swept for want of a destination uses the same button, once there is one.
  # Choosing the destination does not sweep it by itself (choose-a-sweep-destination.feature).
  Scenario: A period that ended with no destination is swept with the same button once there is one
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    Then the previous budget period should show 1900 euro of its period leftover still to sweep
    And I should not be able to bring the swept amount of the previous budget period up to date
    When I set the sweep destination to "Savings"
    Then the previous budget period should still show 1900 euro of its period leftover still to sweep
    And the balance of "Deposit" should still be 0.00 euro
    When I bring the swept amount of the previous budget period up to date
    Then I should be told that 1900 euro more of the period leftover of the previous budget period was swept into "Savings"
    And the balance of "Bank" should be 0.00 euro
    And the balance of "Deposit" should be 1900 euro
    And the previous budget period should show that 1900 euro was swept into "Savings"

  # The period ended 30 euro short, so nothing was swept and nothing said. A refund dated into it
  # lifts its period leftover. Only once that is above zero is there anything to sweep: what it
  # should sweep is floored at zero (derived).
  Scenario Outline: A late income in a period that ended short gives it something to sweep only once its leftover is above zero
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 1030 euro on "Groceries" in the current budget period
    When the next budget period begins while MoneyBud is open
    Then I should not have been told anything
    When I record an income of <income> euro labelled "Terugbetaling" dated on the last day of the previous budget period
    Then <shown>
    And <button>

    Examples:
      | income | shown                                                                                                                                      | button                                                                                  |
      | 20.00  | the previous budget period should show a period leftover of -10 euro, with the marker a category over budget has and the badge "Tekort"    | I should not be able to bring the swept amount of the previous budget period up to date |
      | 30.00  | the previous budget period should show no period leftover line                                                                             | I should not be able to bring the swept amount of the previous budget period up to date |
      | 30.01  | the previous budget period should show 0.01 euro of its period leftover still to sweep                                                     | I should be able to bring the swept amount of the previous budget period up to date     |
