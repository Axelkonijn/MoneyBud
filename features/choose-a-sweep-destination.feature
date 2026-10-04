# Choosing the sweep destination: the one backed category a period's leftover goes to when the period
# ends, and what becomes of that choice when its category changes (glossary: "The destination is one
# list, Restant naar", "With no destination, nothing moves", Sweep destination; "The sweep and
# Restant", settled by the stakeholder on 2026-09-27). What moves at a period's end is in
# sweep-at-a-period-end.feature, which explains the steps the sweep files share.
#
# The rules, from arc42 §12:
#   - THE DESTINATION IS CHOSEN IN ONE LIST NEAR Niet toegewezen, on screen "Restant naar", IN THE
#     CURRENT PERIOD AND LATER ONES (ruling 9). An ended period shows its line instead
#     (show-an-ended-period.feature). NO DESTINATION SHOWS "—", as Staat op does (follow-up, over
#     ruling 9's "geen"). Rejected: a toggle on each category row, which would suggest several.
#   - THE DESTINATION MUST ITSELF BE BACKED, so that swept money really lands somewhere. ONLY A BACKED
#     CATEGORY THAT IS NOT ARCHIVED CAN BE CHOSEN, and A FIRST START HAS NONE, since Sparen is unbacked
#     (derived; start-moneybud.feature).
#   - IT CAN BE CHANGED AT ANY TIME, FOR FUTURE SWEEPS (ruling 4). Redirecting a past sweep is deferred
#     until missed.
#   - CHOOSING OR CHANGING IT IS ANNOUNCED AFTERWARDS, NEVER CONFIRMED, with a notice such as "Restant
#     gaat voortaan naar Sparen." (follow-up). Rejected: not announcing it, which would make it the one
#     act on a category that says nothing. CHOOSING THE DESTINATION ALREADY SET DOES NOTHING AT ALL: no
#     notice, and nothing saved, because the list writes back what it shows on every redraw, as Staat
#     op does (follow-up).
#   - WHEN THE DESTINATION LOSES ITS BACKING, OR IS ARCHIVED, THE SETTING IS CLEARED, AND THE NOTICE
#     SAYS SO. The next period end then counts as having no destination (ruling 6). Rejected:
#     unbacking clears it but archiving keeps it, since a sweep every month is the most regular new
#     entry there is, and an archived category is out of new entry.
#   - Choosing a destination DOES NOT SWEEP PERIODS THAT ENDED WITHOUT ONE (ruling 2). Rejected:
#     catching up every unswept period when a destination is first chosen, a surprise movement.
#
# IN THE DOCUMENTATION'S READING, open to contradiction at this gate:
#   - It is ONE SETTING, the same in the current period and every later one, and it applies to every
#     period that ends from then on. Changing it while a later period is on screen changes it
#     everywhere.
#   - RE-POINTING THE DESTINATION KEEPS THE SETTING: the category is still backed.
#   - DELETING THE DESTINATION CLEARS THE SETTING, and the notice says so. A category with no history
#     can be deleted while it is the destination: the setting is not history.
#   - BRINGING AN ARCHIVED FORMER DESTINATION BACK DOES NOT SET IT AGAIN.
#   - UNBACKING THE DESTINATION RETURNS ITS SWEPT MONEY TO THE POOL ACCOUNT TOO, like any money there
#     for it, with no purpose. That is not a redirect. The ended period's line still says where its
#     leftover was swept.
#     (REVISED FOR INCREMENT 15, 2026-10-04, not yet approved: setting Staat op to "—" now returns only
#     this period's money, and swept money is not part of it, even a sweep dated this period's first day.
#     SWEPT MONEY STAYS ON THE ACCOUNT, STILL THE CATEGORY'S ACCUMULATED, and its row says where
#     (glossary: "Vrij, and moving Opgebouwd", ruling 6). The one scenario that asserted the old reading,
#     "Unbacking the destination clears it, says so, returns its swept money to the pool account, and the
#     next period end moves nothing", is revised below. It still clears the destination and says so.)
#
# RULED BY THE STAKEHOLDER AT THE SCENARIO GATE, 2026-09-27, on points raised while writing these
# scenarios:
#   - REMOVING THE DESTINATION, choosing "—", IS ANNOUNCED, like any change of it.
#   - ANY SWEEP INTO A CATEGORY KEEPS IT FROM BEING DELETED, whichever account backs it: the ended
#     period's line names the category, as a history row does. For a sweep from the pool account to
#     itself this overrides the backing rule that lets such a movement go with the category.
#     Rejected: letting it be deleted, after which that period's money shows as still to sweep again.
#   - BACKING A CATEGORY NEVER CHOOSES IT AS THE DESTINATION, even when it is the only backed one.
#   - THE ORDER OF THE LIST: "—" first, then the backed categories alphabetically, compared as the
#     category suggestions are (suggest-categories.feature): case does not count, and a name is shown
#     as it is stored.
#
# Reading the steps: see sweep-at-a-period-end.feature. The backing steps are back-a-category.feature's,
# the category steps add-category.feature's, archive-category.feature's and delete-a-category.feature's,
# and the saving steps carry-on-when-saving-fails.feature's.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature), first started in the current budget
# period (sweep-at-a-period-end.feature). No category in it is backed, and it has no sweep
# destination. The names, labels and amounts are synthetic test data.

@sweep
Feature: Choose where a period's leftover money goes
  As someone who saves in an account kept for it
  I want to choose, once, which savings category a period's leftover money goes to, and change that when my plans change
  So that leftover money lands in savings every month without my deciding it again each time

  # ----------------------------------------------------------------------------------
  # What can be chosen
  # ----------------------------------------------------------------------------------

  # Savings and "emergency" are backed by Deposit, and Pension by the pool account itself, which may
  # back a category. Holiday is backed, but archived. Groceries is not backed. The order is ruled
  # (2026-09-27): none first, then alphabetically, like the category suggestions
  # (suggest-categories.feature), case not counting. The categories were added in an order that is
  # not alphabetical, and "emergency" is stored in lower case, so the list cannot pass by keeping
  # the order added, nor by a case-sensitive sort, which would put it last.
  Scenario: Only a backed category that is not archived can be chosen as the sweep destination, none first and the rest alphabetically
    Given I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Groceries"
    And I have a category "Savings"
    And I have a category "Holiday"
    And I have a category "Pension"
    And I have a category "emergency"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Holiday" to "Broker"
    And I have set the backing account of "Pension" to "Bank"
    And I have set the backing account of "emergency" to "Deposit"
    And I have archived the category "Holiday"
    Then the sweep destination shown in the current budget period should be none
    And the choices offered for the sweep destination should be exactly these, in this order:
      | choice    |
      | none      |
      | emergency |
      | Pension   |
      | Savings   |

  # Backing a category makes it a choice. It does not choose it (ruled at the scenario gate,
  # 2026-09-27).
  Scenario: With no backed category, the only choice is none, and backing a category offers it without choosing it
    Given I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    Then the choices offered for the sweep destination should be exactly these, in this order:
      | choice |
      | none   |
    When I set the backing account of "Savings" to "Deposit"
    Then the choices offered for the sweep destination should be exactly these, in this order:
      | choice  |
      | none    |
      | Savings |
    And the sweep destination shown in the current budget period should be none

  # ----------------------------------------------------------------------------------
  # Choosing, changing and removing it
  # ----------------------------------------------------------------------------------

  Scenario: Choosing the sweep destination is announced, never confirmed, and it is shown the same in every period to come
    Given I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I set the sweep destination to "Savings"
    Then I should be told that the period leftover will go to "Savings" from now on
    And I should not be warned or asked to confirm
    And the sweep destination shown in the current budget period should be "Savings"
    And the sweep destination shown in the next budget period should be "Savings"

  # The documentation's reading (see the header): one setting, whichever period it is changed from,
  # and the next period to end uses it.
  Scenario: Changing the sweep destination while a later period is on screen changes it everywhere, and the next sweep follows it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have a category "Holiday"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Holiday" to "Broker"
    And I have set the sweep destination to "Savings"
    And the Overview shows the next budget period
    When I set the sweep destination to "Holiday"
    Then I should be told that the period leftover will go to "Holiday" from now on
    And the sweep destination shown in the next budget period should be "Holiday"
    And the sweep destination shown in the current budget period should be "Holiday"
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 1000 euro, was swept into "Holiday"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 0.00    |
      | Broker  | 1000.00 |

  # The follow-up. The list writes back what it shows whenever the screen redraws, so choosing what is
  # already chosen must not count as an act. Were it saved, a disk that cannot be written to would
  # show "not saved" after nothing at all.
  Scenario: Choosing the sweep destination already set does nothing at all: nothing is said, and nothing is saved
    Given I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And saving is not possible
    When I set the sweep destination to "Savings"
    Then I should not have been told anything
    And MoneyBud should not show that my changes are not saved
    And the sweep destination shown in the current budget period should still be "Savings"

  # That removing it is announced was ruled at the scenario gate on 2026-09-27. With no destination, the
  # period that ends next moves nothing, and shows its money still to sweep.
  Scenario: Removing the sweep destination is announced, and the next period end moves nothing
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    When I remove the sweep destination
    Then I should be told that the period leftover will go nowhere from now on
    And I should not be warned or asked to confirm
    And the sweep destination shown in the current budget period should be none
    When the next budget period begins while MoneyBud is open
    Then the balance of "Bank" should still be 1000 euro
    And the balance of "Deposit" should still be 0.00 euro
    And the previous budget period should show 1000 euro of its period leftover still to sweep

  # Ruling 2, over catching up: a destination chosen now applies to periods that end from now on. The
  # previous period ended before the first start, and its salary is still to sweep, by the button
  # (bring-a-swept-period-up-to-date.feature).
  Scenario: Choosing a sweep destination does not sweep a period that ended without one
    Given my budget periods are one month long
    And I have recorded an income of 1800 euro labelled "Salaris" dated on the first day of the previous budget period
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I set the sweep destination to "Savings"
    Then I should be told that the period leftover will go to "Savings" from now on
    And the previous budget period should still show 1800 euro of its period leftover still to sweep
    And the balance of "Deposit" should still be 0.00 euro
    And I should be able to bring the swept amount of the previous budget period up to date

  # ----------------------------------------------------------------------------------
  # When the destination's category changes
  # ----------------------------------------------------------------------------------

  # Ruling 6. The ended period still says its leftover went to Savings. The next period to end has no
  # destination, so its 500 stays on Bank and shows as still to sweep.
  #
  # Revised for increment 15. It first said, in the documentation's reading, that everything there for
  # Savings went back to Bank, the 1000 swept included, with no purpose. Now "—" returns only this period's
  # money, and Savings has no budget this period, so nothing moves: the swept 1000 stays on Deposit, still
  # Savings', and its row says where.
  Scenario: Unbacking the destination clears it and says so, its swept money stays where it is, still the category's, and the next period end moves nothing
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have recorded an income of 500 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 1000 euro, was swept into "Savings"
    When I remove the backing of "Savings"
    Then I should be told that "Savings" is no longer backed, and of no money moved
    And I should be told that "Savings" is no longer the sweep destination
    And the sweep destination shown in the current budget period should be none
    And the choices offered for the sweep destination should be exactly these, in this order:
      | choice |
      | none   |
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 500.00  |
      | Deposit | 1000.00 |
    And Accumulated for "Savings" in the current budget period should be 1000 euro, on "Deposit"
    And the previous budget period should still show that 1000 euro was swept into "Savings"
    When the next budget period begins while MoneyBud is open
    Then the balance of "Bank" should still be 500 euro
    And the previous budget period should show 500 euro of its period leftover still to sweep
    And the budget period 2 before the current one should still show that 1000 euro was swept into "Savings"

  # Ruling 6. Archiving does nothing to backing (back-a-category.feature), so the 1000 stays built up
  # for Savings, and its row is still shown (show-accumulated.feature). Bringing it back by adding its
  # name makes it a choice again, but does not choose it again: the documentation's reading.
  Scenario: Archiving the destination clears it and says so, and bringing the category back does not choose it again
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 1000 euro, was swept into "Savings"
    When I archive the category "Savings"
    Then I should be told that "Savings" was archived
    And I should be told that "Savings" is no longer the sweep destination
    And the sweep destination shown in the current budget period should be none
    And the choices offered for the sweep destination should be exactly these, in this order:
      | choice |
      | none   |
    And the balance of "Deposit" should still be 1000 euro
    And Accumulated for "Savings" in the current budget period should still be 1000 euro
    When I add a category "Savings"
    Then I should be told that "Savings" was brought back
    And the sweep destination shown in the current budget period should be none
    And the choices offered for the sweep destination should be exactly these, in this order:
      | choice  |
      | none    |
      | Savings |

  # The documentation's reading: Savings is still backed, so it stays the destination. Re-pointing
  # takes the swept money along (back-a-category.feature), and the next sweep goes to Broker.
  Scenario: Pointing the destination's backing at another account keeps it the destination, and the next sweep goes to the new account
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have recorded an income of 1200 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 1000 euro, was swept into "Savings"
    When I set the backing account of "Savings" to "Broker"
    Then I should be told that "Savings" is now backed by "Broker", and that 1000 euro moved from "Deposit" to "Broker"
    And the sweep destination shown in the current budget period should still be "Savings"
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 1200 euro, was swept into "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 0.00    |
      | Broker  | 2200.00 |

  # The documentation's reading. Savings has no budget, no expense and nothing moved for it anywhere,
  # so it can be deleted (delete-a-category.feature), destination or not. The setting goes with it.
  Scenario: Deleting the destination clears it, and says so
    Given I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    Then I should be able to delete the category "Savings"
    When I delete the category "Savings"
    Then I should be told that "Savings" was deleted
    And I should be told that "Savings" is no longer the sweep destination
    And the sweep destination shown in the current budget period should be none
    And the choices offered for the sweep destination should be exactly these, in this order:
      | choice |
      | none   |

  # Once a period's leftover was swept into a category, the ended period's line names it, as a
  # history row would. From Bank to Deposit that follows from the backing rule for movements between
  # two accounts. From Bank to itself it was ruled at the scenario gate on 2026-09-27, over the
  # backing rule that lets such a movement go with the category (see the header).
  Scenario Outline: A category a period's leftover was swept into cannot be deleted, whichever account backs it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "<account>"
    And I have set the sweep destination to "Savings"
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 1000 euro, was swept into "Savings"
    And I should not be able to delete the category "Savings"

    Examples:
      | account |
      | Deposit |
      | Bank    |
