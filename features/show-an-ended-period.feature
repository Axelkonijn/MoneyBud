# What an ended period shows once the sweep exists: its own figures as they were, and one line saying
# where its leftover went (glossary: "What an ended period shows", "The destination is one list,
# Restant naar", "What a period sweeps: netted, never below zero"; "The sweep and Restant", settled by
# the stakeholder on 2026-09-27). What moves at a period's end is in sweep-at-a-period-end.feature,
# which explains the steps the sweep files share. What the button does is in
# bring-a-swept-period-up-to-date.feature.
#
# The rules, from arc42 §12:
#   - AN ENDED PERIOD'S OWN FIGURES ARE UNCHANGED AFTER ITS SWEEP, PLUS ONE LINE (ruling 11). Niet
#     toegewezen and each Resterend stay what they were. Rejected: showing the swept figures as zero,
#     which breaks Remaining = Budget - spent and hides how the month went.
#   - IN AN ENDED PERIOD THE "Restant naar" LIST IS REPLACED BY WHAT HAPPENED (ruling 9): "Restant
#     € 130,00 naar Sparen", "€ 40,00 nog niet weggezet" or "€ 40,00 te veel weggezet", the last two
#     with the one button "Restant bijwerken" (follow-up).
#   - NO PREVIEW (ruling 10): the current period shows only where its leftover will go, not how much.
#     Niet toegewezen and each Resterend already show it.
#   - A PERIOD LEFTOVER OF EXACTLY ZERO WITH NOTHING SWEPT SHOWS NO LINE (follow-up).
#   - A SHORTFALL SHOWS ITS LINE IN EVERY ENDED PERIOD, those that ended before the first start
#     included: a back-dated expense in a period with no income shows as a shortfall (follow-up). IT
#     CARRIES THE ONE MARKER, WITH A NEW BADGE, "Tekort" (follow-up). Rejected: reusing "Te veel
#     toegewezen", since a shortfall can come from overspending; and no marker.
#   - WHETHER A CATEGORY COUNTS AS UNBACKED IS JUDGED BY ITS BACKING AT THAT PERIOD'S END (derived), so
#     backing a category afterwards does not make a swept period look over-swept.
#   - THAT STANDS BESIDE ACCUMULATED FOLLOWING TODAY'S BACKING, ACCEPTED AS IT IS (follow-up): a past
#     row may show "Opgebouwd € 0,00", because the category is backed today, while that period's
#     Resterend was swept, because it was unbacked when the period ended. Both are true, and the line
#     says where the money went.
#
# IN THE DOCUMENTATION'S READING, open to contradiction at this gate:
#   - "nog niet weggezet" also covers a period never swept, for want of a destination or because it
#     ended before the first start.
#   - The shortfall line and its marker are shown INSTEAD OF a "nog niet weggezet" line and button: at
#     zero or less there is nothing to sweep.
#   - If a period was swept and has since fallen to a shortfall, what was swept is shown as "te veel
#     weggezet", with the button.
#
# RULED BY THE STAKEHOLDER AT THE SCENARIO GATE, 2026-09-27:
#   - ONE LINE AT A TIME: a period that was swept and has since fallen short shows "te veel weggezet"
#     with the button first, and its shortfall, with the marker and "Tekort", only once that is taken
#     back. Rejected: both at once.
#   - WITH NO DESTINATION AND MONEY STILL TO SWEEP, THE BUTTON IS NOT OFFERED, and the line reads "nog
#     niet weggezet". Rejected: offering it and saying, when pressed, that a destination is needed.
#   - THE SHORTFALL LINE SHOWS THE NEGATIVE PERIOD LEFTOVER ITSELF (-30, say), with the marker and the
#     badge "Tekort". Its wording is copy.
#
# NOT specified here, and left to the plan: where the list and the line sit near Niet toegewezen,
# which is in the ring's hole, and how the line is worded beyond the phrases ruled.
#
# REVISED FOR INCREMENT 15, 2026-10-04 (glossary: "Vrij, and moving Opgebouwd", ruling 6; not yet
# approved). One scenario, "Unbacking a category after a period ended does not make that period look as if
# it had more to sweep", said that setting Holiday to "—" returned its money to Bank. Setting "—" now
# returns only this period's money, and Holiday has none this period, so its 100 stays on Deposit, still
# Holiday's. Its point, that the ended period does not look as if it had more to sweep, is unchanged.
#
# Reading the steps: the line's steps and the destination steps are explained in
# sweep-at-a-period-end.feature. The row table is overview.feature's, with back-a-category.feature's
# ACCUMULATED column. The backing steps are back-a-category.feature's.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature), first started in the current budget
# period (sweep-at-a-period-end.feature). The names, labels and amounts are synthetic test data.

@sweep
Feature: See what became of an ended period's leftover money
  As someone looking back at how a month went
  I want an ended period to keep its own figures and say in one line where its leftover money went, or that some is still to go
  So that the month still reads as it happened, and I can see at a glance whether its money has found a purpose

  # ----------------------------------------------------------------------------------
  # The period's own figures stay; one line says where its leftover went
  # ----------------------------------------------------------------------------------

  # Unassigned 1500, Groceries 50 left, Hobby 30 over: 1520 swept. The period still shows all of
  # that, Hobby's marker included. Savings has no history in that period, so it has no row there.
  Scenario: After its sweep, an ended period's own figures are as they were, and one line says where its leftover went
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a budget of 100 euro for "Hobby" in the current budget period
    And I have already spent 350 euro on "Groceries" in the current budget period
    And I have already spent 130 euro on "Hobby" in the current budget period
    When the next budget period begins while MoneyBud is open
    Then the categories shown in the previous budget period should be exactly these, in this order:
      | category  | budget | spent  | remaining | over budget |
      | Groceries | 400.00 | 350.00 | 50.00     | no          |
      | Hobby     | 100.00 | 130.00 | -30.00    | yes         |
    And Unassigned in the previous budget period should still be 1500 euro
    And the ring for the previous budget period should have an Unassigned slice of 1500 euro
    And the previous budget period should show that 1520 euro was swept into "Savings"

  # Ruling 9 and ruling 10. The current period has the list and no line, before the boundary and
  # after it; the period that ended has the line and no list.
  Scenario: The current period shows where its leftover will go and not how much, and an ended period shows its line instead of the list
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 100 euro on "Groceries" in the current budget period
    Then the sweep destination shown in the current budget period should be "Savings"
    And the current budget period should show no period leftover line
    When the next budget period begins while MoneyBud is open
    Then the previous budget period should offer no sweep destination
    And the previous budget period should show that 1900 euro was swept into "Savings"
    And the sweep destination shown in the current budget period should be "Savings"
    And the current budget period should show no period leftover line

  # ----------------------------------------------------------------------------------
  # Below zero, at zero, above zero
  #
  # The previous period ended before the first start, so nothing is ever swept for it by itself:
  # what it shows depends on its period leftover alone. A 30 euro receipt against an income of just
  # under, exactly, and just over 30 euro.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An ended period shows a shortfall below zero, nothing at exactly zero, and money still to sweep above it
    Given my budget periods are one month long
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a category "Groceries"
    And I have recorded an income of <income> euro labelled "Terugbetaling" dated on the first day of the previous budget period
    And I have recorded an expense of 30 euro for "Groceries" labelled "Markt" dated on the last day of the previous budget period
    Then <shown>
    And <button>

    Examples:
      | income | shown                                                                                                                                            | button                                                                                  |
      | 29.99  | the previous budget period should show a period leftover of -0.01 euro, with the marker a category over budget has and the badge "Tekort"        | I should not be able to bring the swept amount of the previous budget period up to date |
      | 30.00  | the previous budget period should show no period leftover line                                                                                   | I should not be able to bring the swept amount of the previous budget period up to date |
      | 30.01  | the previous budget period should show 0.01 euro of its period leftover still to sweep                                                           | I should be able to bring the swept amount of the previous budget period up to date     |

  # The ruling's own example: a receipt back-dated into a period that had no income. It ended before
  # the first start, and the shortfall shows all the same.
  Scenario: A back-dated expense in a period with no income shows as a shortfall, in a period that ended before the first start too
    Given my budget periods are one month long
    And I have a category "Groceries"
    And I have recorded an expense of 30 euro for "Groceries" labelled "Markt" dated on the last day of the previous budget period
    Then the previous budget period should show a period leftover of -30 euro, with the marker a category over budget has and the badge "Tekort"
    And I should not be able to bring the swept amount of the previous budget period up to date

  # Derived, and ruled at the scenario gate on 2026-09-27: with no destination, the button is not
  # offered (see the header). There is no backed category, so there can be no destination.
  Scenario: With no sweep destination, an ended period shows its money still to sweep, and offers no button
    Given my budget periods are one month long
    And I have recorded an income of 1800 euro labelled "Salaris" dated on the first day of the previous budget period
    Then the previous budget period should show 1800 euro of its period leftover still to sweep
    And I should not be able to bring the swept amount of the previous budget period up to date
    And the sweep destination shown in the current budget period should be none

  # The documentation's reading, and one line at a time as ruled at the scenario gate on 2026-09-27
  # (see the header): 20 was swept, and a late receipt of 50 takes the period to -30. What was swept
  # shows as swept too much, with the button. Once that is taken back, the shortfall shows, with its
  # marker.
  Scenario: A swept period that has since fallen short shows what was swept as swept too much, and the shortfall once that is taken back
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 980 euro on "Groceries" in the current budget period
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 20 euro, was swept into "Savings"
    When I record an expense of 50 euro for "Groceries" labelled "Bon" dated on the last day of the previous budget period
    Then the previous budget period should show 20 euro of its period leftover swept too much
    And I should be able to bring the swept amount of the previous budget period up to date
    When I bring the swept amount of the previous budget period up to date
    Then I should be told that 20 euro swept too much for the previous budget period was taken back from "Savings"
    And the previous budget period should show a period leftover of -30 euro, with the marker a category over budget has and the badge "Tekort"
    And I should not be able to bring the swept amount of the previous budget period up to date
    And the accounts should be exactly these, in this order:
      | account | balance | overdrawn |
      | Bank    | -30.00  | yes       |
      | Deposit | 0.00    | no        |

  # ----------------------------------------------------------------------------------
  # Backing is judged as it was when the period ended
  # ----------------------------------------------------------------------------------

  # Derived, and the accepted tension. Hobby was unbacked when the period ended, so its 60 was swept
  # with Unassigned's 900. Backing Hobby now does not make the period look over-swept. Its row there
  # shows Opgebouwd 0.00, because Accumulated follows today's backing, and nothing had moved for it by
  # then. Both are true; the line says where the money went.
  Scenario: Backing a category after a period ended does not make that period look over-swept, and its row there shows Accumulated of zero
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 100 euro for "Hobby" in the current budget period
    And I have already spent 40 euro on "Hobby" in the current budget period
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 960 euro, was swept into "Savings"
    When I set the backing account of "Hobby" to "Deposit"
    Then I should be told that "Hobby" is now backed by "Deposit", and of no money moved
    And the previous budget period should still show that 960 euro was swept into "Savings"
    And I should not be able to bring the swept amount of the previous budget period up to date
    And the categories shown in the previous budget period should be exactly these, in this order:
      | category | budget | spent | remaining | accumulated |
      | Hobby    | 100.00 | 40.00 | 60.00     | 0.00        |

  # The same rule the other way round. Holiday was backed when the period ended, so its unspent 100
  # was not part of the leftover. Unbacking it now does not make the period look as if it had more to
  # sweep.
  #
  # Revised for increment 15. It first said that unbacking returns Holiday's money to Bank, with no
  # purpose. Now "—" returns only this period's money, and Holiday has none this period, so nothing moves:
  # the 100 stays on Deposit, still Holiday's, and its row says where (back-a-category.feature).
  Scenario: Unbacking a category after a period ended does not make that period look as if it had more to sweep
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have a category "Holiday"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Holiday" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 100 euro for "Holiday" in the current budget period
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 900 euro, was swept into "Savings"
    When I remove the backing of "Holiday"
    Then I should be told that "Holiday" is no longer backed, and of no money moved
    And Accumulated for "Holiday" in the current budget period should be 100 euro, on "Deposit"
    And the previous budget period should still show that 900 euro was swept into "Savings"
    And I should not be able to bring the swept amount of the previous budget period up to date
