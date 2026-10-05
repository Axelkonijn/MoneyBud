# The sweep: when a budget period ends, the money of that period that never landed anywhere moves
# into one backed category, the sweep destination (glossary: "The sweep and Restant", settled by the
# stakeholder on 2026-09-27; "Nothing crosses a period boundary without a purpose"; Sweep, Period
# leftover, Sweep destination). Choosing the destination, and what happens to it when its category
# changes, is in choose-a-sweep-destination.feature. What an ended period shows is in
# show-an-ended-period.feature. A swept period whose figures change later is in
# bring-a-swept-period-up-to-date.feature. THIS FILE EXPLAINS THE STEPS THE FOUR SHARE.
#
# The rules, from arc42 §12:
#   - WHEN A BUDGET PERIOD ENDS, ITS PERIOD LEFTOVER, on screen "Restant", MOVES FROM THE POOL ACCOUNT
#     INTO THE SWEEP DESTINATION'S BACKING ACCOUNT. The period leftover is the period's Unassigned
#     PLUS EVERY UNBACKED CATEGORY'S REMAINING, NEGATIVES INCLUDED (ruling 3). In the ordinary case it
#     is exactly what is left on the pool account of that period's income.
#   - IF IT IS ZERO OR LESS, NOTHING MOVES, and the period shows the shortfall. SAVINGS ARE NEVER DRAWN
#     ON. Rejected: taking a negative total back out of the destination; sweeping only the positive
#     figures, which moves more than the pool account has.
#   - BACKED CATEGORIES ARE NOT PART OF IT: their money has landed. So a backed category's overspending
#     does not lower the period leftover either.
#   - WITH NO DESTINATION SET, NOTHING MOVES (ruling 2). The money stays on the pool account with no
#     purpose, in no period's Unassigned: nothing rolls forward.
#   - ONLY PERIODS THAT END AFTER MONEYBUD WAS FIRST STARTED ARE SWEPT BY THEMSELVES (ruling 7). An
#     earlier one, such as last month's salary entered back-dated on the first day, shows its money and
#     offers the button (bring-a-swept-period-up-to-date.feature).
#   - AN AUTOMATIC SWEEP THAT MOVED MONEY IS ANNOUNCED, ONCE (follow-up), by a notice such as "Restant
#     van september: € 130,00 naar Sparen." That is AN EXCEPTION to "nothing is announced when a new
#     period begins" (step-between-periods.feature), which still holds for everything else.
#
# DERIVED BY THE DOCUMENTATION AND NOT PUT TO THE STAKEHOLDER, marked where a scenario rests on one:
#   - THE SWEEP RUNS AT SETTLING: the first time MoneyBud runs on or after the next period's first day,
#     before anything else is done, as money planned for a later period does (show-moved-money.feature).
#     It is a movement DATED THAT FIRST DAY, from the pool account of that moment into the destination's
#     backing account of that moment. IF MONEYBUD WAS NOT OPENED FOR SEVERAL PERIODS, EACH ENDED PERIOD
#     IS SWEPT AT ITS OWN END, IN ORDER. (Since 2026-09-29, one exception: a period ended by a change of
#     the period start day is swept at once, dated the day of the change:
#     carry-plans-and-money-across-a-start-day-change.feature.)
#   - WHEN THE POOL ACCOUNT BACKS THE DESTINATION, the sweep changes no balance and leaves no history
#     row, but Accumulated counts it, as assigning does (back-a-category.feature).
#   - SWEPT MONEY RAISES THE DESTINATION'S ACCUMULATED FROM THE DAY IT MOVES. IT IS NOT A BUDGET in any
#     period.
#   - Which categories count as unbacked is judged by their backing AT THAT PERIOD'S END
#     (show-an-ended-period.feature).
#
# IN THE DOCUMENTATION'S READING, open to contradiction at this gate:
#   - An ARCHIVED unbacked category counts like any other.
#   - The sweep may OVERDRAW THE POOL ACCOUNT when money did not go where MoneyBud assumes, such as an
#     income put on another account. Shown with the marker, badge "Rood", never blocked.
#   - The ended period's own view does not include its sweep in Accumulated, which is dated in the next
#     period; nor does a later period's view include a sweep still to come.
#   - A PERIOD END AT WHICH NOTHING MOVES (no destination, or a period leftover of zero or less)
#     ANNOUNCES NOTHING.
#   - Several periods swept at one settling are announced in one notice or several, as the plan
#     chooses. EITHER WAY, EVERY ONE OF THEM IS TOLD.
#
# RULED BY THE STAKEHOLDER AT THE SCENARIO GATE, 2026-09-27: A SWEEP THAT MOVED MONEY IS SAVED STRAIGHT
# AWAY, so it is announced exactly once, and starting MoneyBud again neither announces it again nor
# sweeps again. This overrides, for such a sweep, the gap accepted for settling, that what settling
# moves is saved with the next change. Rejected: accepting a repeated announcement after a restart
# with no change in between.
#
# NOT in this increment: redirecting a past sweep to another category (deferred until missed), and
# choosing the account a sweep comes from (derived: deferred until missed, like overriding the source
# of any movement).
#
# WHEN MONEYBUD WAS FIRST STARTED. Every scenario's empty ledger counts as FIRST STARTED IN THE
# CURRENT BUDGET PERIOD, on the day the scenario begins. So the previous budget period and every
# earlier one ended before the first start, and are never swept by themselves; the current period
# and every later one end after it. A scenario about a real first start says so
# (start-moneybud.feature).
#
# Reading the steps. These are shared by all four sweep files:
#   - "I set the sweep destination to "X"" chooses X in the list near Niet toegewezen, on screen
#     "Restant naar". "I remove the sweep destination" chooses "—" there. The list is only in the
#     current period and later ones, so when an ended period is on screen these steps first step
#     forward to the current period, as I would. "I have set the sweep destination to "X"" is the same
#     act done earlier TODAY, at that point in the order of the Givens.
#   - "the sweep destination shown in the current (next) budget period should be "X"" is what that list
#     shows while that period is on screen. "... should be none" means it shows "—".
#   - "the choices offered for the sweep destination should be exactly these, in this order" is the
#     whole list, in its order. "none" is the "—" choice. The order was ruled by the stakeholder at the
#     scenario gate, 2026-09-27: "—" first, then the backed categories alphabetically, compared as
#     the category suggestions are, case not counting (choose-a-sweep-destination.feature).
#   - "the previous budget period should offer no sweep destination" means that, with that period on
#     screen, there is no "Restant naar" list: an ended period shows its line instead (ruling 9).
#   - THE LINE on an ended period, near Niet toegewezen, is read by five steps. Each says what the
#     line shows and whether it offers the button, on screen "Restant bijwerken":
#       "... should show that N euro was swept into "X"" is a period whose money has gone where it
#         should, "Restant € N,00 naar X", with NO button.
#       "... should show that its period leftover was swept into these categories" is the same, for a
#         period whose money went to more than one category, each with its amount.
#       "... should show N euro of its period leftover still to sweep" is "€ N nog niet weggezet".
#       "... should show N euro of its period leftover swept too much" is "€ N te veel weggezet".
#       "... should show a period leftover of N euro, with the marker a category over budget has and
#         the badge "Tekort"" is a shortfall: N is the negative period leftover itself, and there is
#         NO button (ruled at the scenario gate, 2026-09-27).
#       "... should show no period leftover line" means there is no line at all. In the current period
#       and later ones it is always so: there is no preview (ruling 10).
#     The two with money to move say nothing about the button: that is the next step's. "should still
#     show" and "should still be" say the act before them did not change it, as everywhere.
#   - "I should (not) be able to bring the swept amount of the ... budget period up to date" means the
#     button is (not) offered on that period's line; "I should still not be able to ..." is the same.
#     "I bring the swept amount of the ... budget period up to date" presses it, with that period on
#     screen.
#   - The announcements. What is fixed is what I am told, never the wording:
#       "I should be told that the period leftover of the ... budget period, N euro, was swept into
#         "X"" is the automatic sweep's notice.
#       "I should be told that N euro more of the period leftover of the ... budget period was swept
#         into "X"" and "I should be told that N euro swept too much for the ... budget period was
#         taken back from "X"" are what the button moved. Where it moved money for two categories,
#         I am told both.
#       "I should be told that the period leftover will go to "X" from now on" and "... will go
#         nowhere from now on" announce choosing or removing the destination.
#       "I should be told that "X" is no longer the sweep destination" is said with whatever cleared it.
#     "I should not have been told anything" is step-between-periods.feature's: no message of any kind.
#   - Periods are named relative to TODAY, and after a period boundary relative to the NEW today, as
#     in step-between-periods.feature. "the budget period N before the current one" names a period the
#     three usual names do not reach, as in keep-data.feature. "the next budget period begins while
#     MoneyBud is open" (step-between-periods.feature) and "I close MoneyBud, and start it again on the
#     first day of ..." (keep-data.feature) are the two ways time passes a boundary.
#   - In an account's history (show-accounts.feature's step), a sweep, and what the button moves, are
#     ENTRY "movement", as all money MoneyBud moves on a category's behalf is (show-moved-money.feature).
#     CATEGORY is the category the money went to or came back from. The row's wording is copy.
#   - Every other step is reused unchanged from the file that introduced it: the backing steps are
#     back-a-category.feature's, the account steps show-accounts.feature's.
#
# The Dutch on screen, ruled by the stakeholder on 2026-09-27: "Restant" (his own word, from round 2),
# "Restant naar", "Restant bijwerken", "nog niet weggezet", "te veel weggezet", and the badge "Tekort".
# No destination shows "—". The steps use this project's English terms instead. "Period leftover" is
# THE DOCUMENTATION'S PROPOSED ENGLISH TERM for Restant, NOT RULED, and first used here: it is open to
# contradiction at this gate.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). No category in it is backed, and it has
# no sweep destination. The names, labels and amounts are synthetic test data.

@sweep
Feature: Sweep a period's leftover money when the period ends
  As someone who wants every euro to have a purpose
  I want what is left of a period's money, once the period is over, to move into savings by itself
  So that money I did not plan or did not spend is put aside, instead of sitting in my current account with no purpose

  # ----------------------------------------------------------------------------------
  # What moves: the period leftover, netted, from the pool account into the destination
  # ----------------------------------------------------------------------------------

  # The glossary's own example, with synthetic names. Unassigned 800, and Remaining 50, 0 and -30
  # for the three unbacked categories: 820. That is exactly what is left on Bank of the period's
  # income: 2500, less the 300 moved for Savings, less the 1380 spent.
  Scenario: When a period ends, its period leftover moves from the pool account into the sweep destination
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2500 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a budget of 900 euro for "Rent" in the current budget period
    And I have a budget of 100 euro for "Hobby" in the current budget period
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have already spent 350 euro on "Groceries" in the current budget period
    And I have already spent 900 euro on "Rent" in the current budget period
    And I have already spent 130 euro on "Hobby" in the current budget period
    And Unassigned in the current budget period is 800 euro
    And the balance of "Bank" is 820 euro
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 820 euro, was swept into "Savings"
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 1120.00 |
    And net worth should still be 1120 euro
    And the previous budget period should show that 820 euro was swept into "Savings"
    And Unassigned in the previous budget period should still be 800 euro
    And the remaining "Hobby" budget in the previous budget period should still be -30 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date                                       | entry            | category | from | to      | amount | balance |
      | the first day of the current budget period | movement         | Savings  | Bank | Deposit | 820.00 |         |
      | the last day of the previous budget period | movement         | Savings  | Bank | Deposit | 300.00 |         |
      | the last day of the previous budget period | starting balance |          |      |         |        | 0.00    |

  # Negatives are netted in. In the second row Groceries is overspent by 599.99, which takes the
  # period leftover down to a single cent, and that cent moves. In the third the period is
  # over-assigned, Unassigned is -200, and Groceries' unspent 500 still leaves 300 to move. In every
  # row the period leftover is exactly what Bank holds, so Bank ends at zero.
  Scenario Outline: The period leftover is Unassigned plus every unbacked category's Remaining, negatives included, and exactly that moves
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of <budget> euro for "Groceries" in the current budget period
    And I have already spent <spent> euro on "Groceries" in the current budget period
    And Unassigned in the current budget period is <unassigned> euro
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, <leftover> euro, was swept into "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance    |
      | Bank    | 0.00       |
      | Deposit | <leftover> |
    And the previous budget period should show that <leftover> euro was swept into "Savings"

    Examples:
      | budget  | spent  | unassigned | leftover |
      | 400.00  | 180.00 | 600.00     | 820.00   |
      | 400.00  | 999.99 | 600.00     | 0.01     |
      | 1200.00 | 700.00 | -200.00    | 300.00   |

  # Exactly zero is nothing to sweep and nothing to say: no movement, no notice, no line. That no
  # notice is given is the documentation's reading (see the header).
  Scenario: A period leftover of exactly zero moves nothing, announces nothing, and leaves no line
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 1000 euro on "Groceries" in the current budget period
    When the next budget period begins while MoneyBud is open
    Then I should not have been told anything
    And the balance of "Bank" should still be 0.00 euro
    And the balance of "Deposit" should still be 0.00 euro
    And the previous budget period should show no period leftover line
    And I should not be able to bring the swept amount of the previous budget period up to date

  # Savings has 200 built up. The period overspent: its leftover is below zero, and the overspending
  # was paid from Bank, so Bank is overdrawn. Nothing is taken out of Savings to cover it. That
  # nothing is announced is the documentation's reading.
  Scenario Outline: A period leftover below zero moves nothing, never draws on the destination, and the period shows the shortfall
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent <spent> euro on "Groceries" in the current budget period
    When the next budget period begins while MoneyBud is open
    Then I should not have been told anything
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance | overdrawn |
      | Bank    | <bank>  | yes       |
      | Deposit | 200.00  | no        |
    And Accumulated for "Savings" in the current budget period should be 200 euro
    And the previous budget period should show a period leftover of <leftover> euro, with the marker a category over budget has and the badge "Tekort"
    And I should not be able to bring the swept amount of the previous budget period up to date

    Examples:
      | spent  | bank   | leftover |
      | 800.01 | -0.01  | -0.01    |
      | 830.00 | -30.00 | -30.00   |

  # ----------------------------------------------------------------------------------
  # Which categories count
  # ----------------------------------------------------------------------------------

  # Savings has 200 of its budget left and Holiday is 50 over, both paid from Deposit. Their money has
  # landed, so neither counts: the period leftover is Unassigned alone, 600, which is what Bank holds.
  Scenario: A backed category's Remaining is not part of the period leftover, and neither is its overspending
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have a category "Holiday"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Holiday" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have a budget of 100 euro for "Holiday" in the current budget period
    And I have recorded an expense of 100 euro for "Savings" labelled "Cadeau" dated today on the account "Deposit"
    And I have recorded an expense of 150 euro for "Holiday" labelled "Hotel" dated today on the account "Deposit"
    And Unassigned in the current budget period is 600 euro
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 600 euro, was swept into "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 750.00  |
    And the remaining "Savings" budget in the previous budget period should still be 200 euro
    And the remaining "Holiday" budget in the previous budget period should still be -50 euro

  # The documentation's reading (see the header): "every unbacked category" makes no exception for
  # an archived one. Hobby's unspent 60 is swept with Unassigned's 900.
  Scenario: An archived unbacked category's Remaining is part of the period leftover like any other
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 100 euro for "Hobby" in the current budget period
    And I have already spent 40 euro on "Hobby" in the current budget period
    And I have archived the category "Hobby"
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 960 euro, was swept into "Savings"
    And the balance of "Bank" should be 0.00 euro
    And the balance of "Deposit" should be 960 euro

  # ----------------------------------------------------------------------------------
  # With no destination, nothing moves
  # ----------------------------------------------------------------------------------

  # The next period's pool is its own income and nothing else: the 900 does not roll into it. It
  # stays on Bank with no purpose, and the ended period says it was not swept. There is nowhere for
  # the button to move it (bring-a-swept-period-up-to-date.feature).
  # Note, 2026-10-05 (ruling 5 revised; gates waived by Axel): the 900 is not Bank's Unclaimed. It is
  # claimed by the ended period's line, which still asks for it, until a destination is set and the
  # line is brought up to date (show-unclaimed.feature). This comment's "no purpose" predates that.
  Scenario: With no sweep destination, nothing moves at a period's end, nothing is announced, and nothing rolls into the next period
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have recorded an income of 1500 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 100 euro on "Groceries" in the current budget period
    When the next budget period begins while MoneyBud is open
    Then I should not have been told anything
    And the balance of "Bank" should be 2400 euro
    And the balance of "Deposit" should still be 0.00 euro
    And Unassigned in the current budget period should be 1500 euro
    And the previous budget period should show 900 euro of its period leftover still to sweep
    And I should not be able to bring the swept amount of the previous budget period up to date

  # ----------------------------------------------------------------------------------
  # Where the money comes from and goes to
  # ----------------------------------------------------------------------------------

  # Derived (see the header): from the pool to the pool changes no balance and leaves no row, as
  # assigning does, but Accumulated counts it, and it is announced and shown like any sweep.
  Scenario: When the pool account backs the destination, the sweep changes no balance and leaves no history row, but Accumulated counts it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Bank"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 900 euro, was swept into "Savings"
    And the balance of "Bank" should still be 900 euro
    And Accumulated for "Savings" in the current budget period should be 900 euro
    And the previous budget period should show that 900 euro was swept into "Savings"
    And the history of "Bank" should be exactly these, newest first:
      | date                                       | entry   | category  | label   | amount  |
      | the last day of the previous budget period | expense | Groceries | Markt   | 100.00  |
      | the last day of the previous budget period | income  |           | Salaris | 1000.00 |

  # The documentation's reading (see the header). The salary was put on Cash, not on the pool
  # account, so Bank never had it. The sweep still moves the period leftover, and Bank goes into the
  # red, shown and not blocked. Net worth is unchanged: the money moved between two of my accounts.
  Scenario: When the period's income was put on another account, the sweep may overdraw the pool account, and nothing stops it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Cash" with a starting balance of 0 euro
    And I have recorded an income of 1000 euro labelled "Salaris" dated today on the account "Cash"
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 1000 euro, was swept into "Savings"
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance  | overdrawn |
      | Bank    | -1000.00 | yes       |
      | Deposit | 1000.00  | no        |
      | Cash    | 1000.00  | no        |
    And "Bank" should be shown as overdrawn, with the marker a category over budget has and the badge "Rood"
    And net worth should still be 1000 euro

  # ----------------------------------------------------------------------------------
  # When it runs: at each period's end, in order, and only after the first start
  # ----------------------------------------------------------------------------------

  # Derived (see the header). MoneyBud was closed on the last day of one period and started again
  # three periods later. Each of the three periods that ended meanwhile is swept at its own end, and
  # each movement is dated the first day of the period after it. Their incomes were recorded ahead,
  # dated in their own periods. Whether that is one notice or three is the plan's choice; either
  # way I am told of all three.
  Scenario: When MoneyBud was not opened for several periods, each period that ended is swept at its own end, in order
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris 1" dated today
    And I have recorded an income of 1100 euro labelled "Salaris 2" dated on the first day of the next budget period
    And I have recorded an income of 1200 euro labelled "Salaris 3" dated on the first day of the budget period 2 after the current one
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    When I close MoneyBud, and start it again on the first day of the budget period 3 after the current one
    Then I should be told that the period leftover of the budget period 3 before the current one, 1000 euro, was swept into "Savings"
    And I should be told that the period leftover of the budget period 2 before the current one, 1100 euro, was swept into "Savings"
    And I should be told that the period leftover of the previous budget period, 1200 euro, was swept into "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 3300.00 |
    And the history of "Deposit" should be exactly these, newest first:
      | date                                                         | entry            | category | from | to      | amount  | balance |
      | the first day of the current budget period                   | movement         | Savings  | Bank | Deposit | 1200.00 |         |
      | the first day of the previous budget period                  | movement         | Savings  | Bank | Deposit | 1100.00 |         |
      | the first day of the budget period 2 before the current one  | movement         | Savings  | Bank | Deposit | 1000.00 |         |
      | the last day of the budget period 3 before the current one   | starting balance |          |      |         |         | 0.00    |
    And the budget period 3 before the current one should show that 1000 euro was swept into "Savings"
    And the budget period 2 before the current one should show that 1100 euro was swept into "Savings"
    And the previous budget period should show that 1200 euro was swept into "Savings"

  # Ruling 7. The previous period ended before MoneyBud was first started (see the header), so the
  # salary entered back-dated into it is never swept by itself, not even when the next boundary
  # passes. It shows as still to sweep, and the button moves it, dated the day I press it.
  Scenario: A period that ended before MoneyBud was first started is not swept by itself, and the button sweeps it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1800 euro labelled "Salaris vorige maand" dated on the first day of the previous budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    Then the previous budget period should show 1800 euro of its period leftover still to sweep
    And I should be able to bring the swept amount of the previous budget period up to date
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 2000 euro, was swept into "Savings"
    And the budget period 2 before the current one should still show 1800 euro of its period leftover still to sweep
    And the balance of "Deposit" should be 2000 euro
    When I bring the swept amount of the budget period 2 before the current one up to date
    Then I should be told that 1800 euro more of the period leftover of the budget period 2 before the current one was swept into "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 3800.00 |
    And the budget period 2 before the current one should show that 1800 euro was swept into "Savings"
    And the history of "Deposit" should be exactly these, newest first:
      | date                                       | entry            | category | from | to      | amount  | balance |
      | today                                      | movement         | Savings  | Bank | Deposit | 1800.00 |         |
      | today                                      | movement         | Savings  | Bank | Deposit | 2000.00 |         |
      | the last day of the previous budget period | starting balance |          |      |         |         | 0.00    |

  # The follow-up: announced once. Ruled at the scenario gate on 2026-09-27 that the sweep is saved
  # straight away, so starting MoneyBud again on the same day, with no change in between, neither
  # says it again nor sweeps again. The history holds one movement, not two.
  Scenario: A sweep is announced once, and not again when MoneyBud is started again
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    When I close MoneyBud, and start it again on the first day of the next budget period
    Then I should be told that the period leftover of the previous budget period, 1000 euro, was swept into "Savings"
    When I close MoneyBud and start it again
    Then I should not have been told anything
    And the balance of "Deposit" should be 1000 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date                                       | entry            | category | from | to      | amount  | balance |
      | the first day of the current budget period | movement         | Savings  | Bank | Deposit | 1000.00 |         |
      | the last day of the previous budget period | starting balance |          |      |         |         | 0.00    |

  # ----------------------------------------------------------------------------------
  # Accumulated and balance corrections
  # ----------------------------------------------------------------------------------

  # 200 moved for Savings today, and 150 is planned for the next period. Before the boundary, the
  # next period's view includes the planned 150 but not the 800 still to be swept: swept money is not
  # a Budget, and there is no preview. After it, the ended period still shows 200, and the new current
  # period shows 200 + 800 + 150. The 800 is not a Budget anywhere. (Derived, and the documentation's
  # reading: see the header.)
  Scenario: Swept money counts in the destination's Accumulated from the day it moves, and is not a budget in any period
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have recorded an income of 1500 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have a budget of 150 euro for "Savings" in the next budget period
    Then Accumulated for "Savings" in the current budget period should be 200 euro
    And Accumulated for "Savings" in the next budget period should be 350 euro
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 800 euro, was swept into "Savings"
    And Accumulated for "Savings" in the previous budget period should still be 200 euro
    And Accumulated for "Savings" in the current budget period should be 1150 euro
    And the budget for "Savings" in the current budget period should be 150 euro
    And Unassigned in the current budget period should be 1350 euro

  # Settling writes the sweep and the planned money on the first day, before anything I type that
  # day. So a balance correction typed that day, with what the bank shows, already has both in it.
  Scenario: A balance correction typed on a period's first day already has that day's sweep and planned money in it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have recorded an income of 1500 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 150 euro for "Savings" in the next budget period
    When the next budget period begins while MoneyBud is open
    Then the balance of "Deposit" should be 6150 euro
    When I correct the balance of "Deposit" to 6150 euro
    Then the balance of "Deposit" should be 6150 euro
    And the balance correction of "Deposit" to 6150 euro should show a difference of 0.00 euro
