# Keeping data: what I entered is still there after I close MoneyBud and start it again (glossary:
# "What MoneyBud keeps", settled by the stakeholder on 2026-09-26). Starting MoneyBud (no kept data
# yet, data it cannot read, a second start while it is open) is in start-moneybud.feature. A save
# that fails is in carry-on-when-saving-fails.feature.
#
# The rules, from arc42 §12:
#   - EVERYTHING IS KEPT, as one continuous history, for as long as I have it. There is no fresh
#     start per year, and nothing is dropped for being old.
#   - "Everything" is THE LEDGER: categories, archived or not, renamed ones with their history,
#     budgets, expenses and incomes. WHAT IS ON SCREEN IS NOT KEPT: the period shown, an entry half
#     typed, an entry being changed, a question waiting for an answer, a rename in progress. So
#     MoneyBud ALWAYS OPENS ON THE CURRENT BUDGET PERIOD.
#   - Kept AUTOMATICALLY AFTER EVERY CHANGE. There is no save button and no act of saving.
#   - Only what went through is kept. A refused entry, a change not saved, a removal not confirmed
#     and a rename not saved change nothing, so there is nothing of them to keep.
#   - One set of data, and no act inside MoneyBud for starting over.
#
# A kept entry, and a kept category, is THE SAME ONE after a restart. That is what makes the
# corrections increment still work across runs: an entry recorded in one run can be changed or
# removed in the next, a renamed category keeps its history, and a category that took its old name
# stays a different category. None of this was put to the stakeholder as a separate question. It is
# what "everything is kept" means once entries can be corrected and categories renamed.
#
# Not specified here: carrying data from one version of MoneyBud to the next. Until the switch to
# real use, and at least up to and including the accounts increment, a new version may be unable to
# read what an older one kept (glossary: "Demo data may not survive a new version"). So no scenario
# here involves more than one version.
#
# Reading the steps:
#   - "I close MoneyBud and start it again" is closing it and starting it again on the same day,
#     with nothing going wrong in between. Everything asserted afterwards is what the new start shows.
#   - "I close MoneyBud, and start it again on the first day of the next budget period" is the same,
#     with the clock moved on to that day while MoneyBud was closed. "... on the first day of the
#     budget period 13 after the current one" moves it on 13 periods. As in step-between-periods.feature
#     ("the next budget period begins while MoneyBud is open"), periods are always named relative to
#     TODAY: so after the clock moves one period, the period the Givens called "current" is
#     "previous". "The budget period 13 before the current one" names a period the other files have
#     no name for, and is used only to show that nothing is dropped for being old.
#   - Every Given describes something done in MoneyBud earlier, and what a Given sets up counts as
#     kept, exactly as if it had been entered on the screen. The Whens act through the screen, as in
#     every other file. Where the point of a scenario is that what I ENTERED is kept, it enters it
#     with Whens.
#   - Every scenario starts from an empty ledger, as in the other files, and that empty ledger counts
#     as data already kept. So no scenario here gets the six default categories by starting again.
#     Starting with no kept data at all is in start-moneybud.feature.
#   - "I type an expense ... without recording it" is filling in the expense form and not pressing
#     the button that records it. "I start changing ... and do not save it" is loading an entry into
#     its form, changing the one field named, and not saving. "I start renaming ... and do not save
#     it" is opening a category's rename box and typing the new name, without saving.
#   - "I ask to remove the expense labelled "X"" is pressing Verwijderen on it, so that MoneyBud asks
#     me to confirm, and not answering yet. "MoneyBud should be asking me to confirm" means that
#     question is waiting. "MoneyBud should not be asking me anything" means no question is waiting.
#   - "the expense form should be empty and ready for a new expense" means nothing is typed in it
#     and it is not changing an entry. "no rename should be in progress" means no category's rename
#     box is open.
#   - "nothing should have been said about saving" means no message mentions saving: when saving
#     works, and has not failed before, it is not announced. "MoneyBud should offer no act for
#     saving (starting over)" means no button or other act for it exists anywhere on the screen.
#   - Every other step is reused unchanged from the file that introduced it, with the meaning that
#     file gives it: the list steps are list-transactions-in-a-period.feature's, the category row
#     steps overview.feature's, and the correcting steps change-an-entry.feature's and
#     remove-an-entry.feature's.
#
# The names, labels and amounts are synthetic test data.

@keeping
Feature: Keep my data between runs
  As someone budgeting my money over months, by hand
  I want MoneyBud to keep everything I enter, without my having to save it
  So that I never have to enter it twice, and every period's history is there whenever I come back to it

  # ----------------------------------------------------------------------------------
  # What went through is kept
  # ----------------------------------------------------------------------------------

  # One of each kind of thing the ledger holds. The expense without a label is kept as having no
  # label, shown as an empty cell, and not as a label made of nothing.
  Scenario: Categories, budgets, incomes and expenses are all there after closing MoneyBud and starting it again
    When I add a category "Groceries"
    And I add a category "Hobby"
    And I record an income of 1832.45 euro labelled "Salaris"
    And I assign 400 euro to "Groceries" in the current budget period
    And I assign 60 euro to "Hobby" in the current budget period
    And I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn"
    And I record an expense of 18 euro for "Groceries" without a label
    And I record an expense of 25 euro for "Hobby" labelled "Verf"
    And I close MoneyBud and start it again
    Then the categories offered for a new expense should be exactly these, in any order:
      | category  |
      | Groceries |
      | Hobby     |
    And the categories shown in the current budget period should be exactly these, in this order:
      | category  | budget | spent | remaining | over budget |
      | Groceries | 400.00 | 50.15 | 349.85    | no          |
      | Hobby     | 60.00  | 25.00 | 35.00     | no          |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | today | Salaris | 1832.45 |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Hobby     | Verf         | 25.00  |
      | today | Groceries |              | 18.00  |
      | today | Groceries | Albert Heijn | 32.15  |
    And Unassigned in the current budget period should be 1372.45 euro

  # Money bugs live in the cents, and a figure that is read back even one cent out changes whether
  # a category is over budget or a period over-assigned. So the rows are the awkward ones: single
  # cents, a trailing zero (0.10), exactly on budget, over budget by a cent, over-assigned by a
  # cent, and an amount far larger than a household budget.
  Scenario Outline: Amounts are kept exactly, to the cent
    Given I have a category "Groceries"
    When I record an income of <income> euro labelled "Salaris"
    And I assign <budget> euro to "Groceries" in the current budget period
    And I record an expense of <spent> euro for "Groceries" labelled "Markt"
    And I close MoneyBud and start it again
    Then the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount   |
      | today | Salaris | <income> |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount  |
      | today | Groceries | Markt | <spent> |
    And the budget for "Groceries" in the current budget period should be <budget> euro
    And the remaining "Groceries" budget in the current budget period should be <remaining> euro
    And Unassigned in the current budget period should be <unassigned> euro

    Examples: cents
      | income  | budget | spent | remaining | unassigned |
      | 0.03    | 0.02   | 0.01  | 0.01      | 0.01       |
      | 2000.00 | 400.00 | 0.10  | 399.90    | 1600.00    |

    Examples: exactly on budget, and one cent over
      | income  | budget | spent  | remaining | unassigned |
      | 1832.45 | 400.10 | 400.10 | 0.00      | 1432.35    |
      | 1832.45 | 400.10 | 400.11 | -0.01     | 1432.35    |

    Examples: over-assigned by one cent
      | income  | budget  | spent | remaining | unassigned |
      | 2000.00 | 2000.01 | 0.10  | 1999.91   | -0.01      |

    Examples: a large amount
      | income    | budget    | spent | remaining | unassigned |
      | 987654.32 | 123456.78 | 0.99  | 123455.79 | 864197.54  |

  # A date read back one day out would move an entry across a period boundary, so the first and
  # last days of periods are the rows that matter. An expense cannot be dated in the future, so its
  # rows stop at today.
  Scenario Outline: An expense is kept on its own date, in its own period
    Given my budget periods are one month long
    And I have a category "Groceries"
    When I record an expense of 12.50 euro for "Groceries" labelled "Kiosk" dated on <day>
    And I close MoneyBud and start it again
    Then the expenses listed in the <period> budget period should be exactly these, in this order:
      | date  | category  | label | amount |
      | <day> | Groceries | Kiosk | 12.50  |
    And no expenses should be listed in the <other> budget period

    Examples:
      | day                                         | period   | other    |
      | the first day of the previous budget period | previous | current  |
      | the last day of the previous budget period  | previous | current  |
      | the first day of the current budget period  | current  | previous |
      | today                                       | current  | previous |

  # An income may be dated in the future, and counts towards its period's Unassigned from the
  # moment it is recorded (record-income.feature). That is still so after starting again.
  Scenario Outline: An income is kept on its own date, in its own period, a future date included
    Given my budget periods are one month long
    When I record an income of 1832.45 euro labelled "Salaris" dated on <day>
    And I close MoneyBud and start it again
    Then the incomes listed in the <period> budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | <day> | Salaris | 1832.45 |
    And Unassigned in the <period> budget period should be 1832.45 euro
    And no incomes should be listed in the <other> budget period

    Examples:
      | day                                         | period   | other    |
      | the first day of the previous budget period | previous | current  |
      | the last day of the previous budget period  | previous | current  |
      | today                                       | current  | next     |
      | the last day of the current budget period   | current  | next     |
      | the first day of the next budget period     | next     | current  |
      | the last day of the next budget period      | next     | current  |

  # A budget is kept as the figure the assignments came to, in every period it was assigned in.
  # Hobby's -50 is clipped to the 30 it had (assign-to-category.feature), so it is kept at zero.
  Scenario: Budgets are kept as the figures they came to, in every period they were assigned in
    Given my budget periods are one month long
    And I have a category "Groceries"
    And I have a category "Hobby"
    And I have already recorded 2000 euro of income in the current budget period
    And I have already recorded 1800 euro of income in the next budget period
    When I assign 400 euro to "Groceries" in the current budget period
    And I assign -150 euro to "Groceries" in the current budget period
    And I assign 30 euro to "Hobby" in the current budget period
    And I assign -50 euro to "Hobby" in the current budget period
    And I assign 350 euro to "Groceries" in the next budget period
    And I close MoneyBud and start it again
    Then the budget for "Groceries" in the current budget period should be 250 euro
    And the budget for "Hobby" in the current budget period should be 0.00 euro
    And the budget for "Groceries" in the next budget period should be 350 euro
    And Unassigned in the current budget period should be 1750 euro
    And Unassigned in the next budget period should be 1450 euro

  # ----------------------------------------------------------------------------------
  # What happened to a category is kept
  # ----------------------------------------------------------------------------------

  # Still archived, still shown where it has history and nowhere else, and its name is still its
  # own: adding it brings the same category back rather than creating another.
  Scenario: An archived category is still archived after starting again, and adding its name still brings it back
    Given my budget periods are one month long
    And I have a category "Groceries"
    And I have a budget of 60 euro for "Hobby" in the previous budget period
    And I have already spent 45 euro on "Hobby" in the previous budget period
    When I archive the category "Hobby"
    And I close MoneyBud and start it again
    Then the categories offered for a new expense should not include "Hobby"
    And "Hobby" should be shown in the previous budget period
    And the remaining "Hobby" budget in the previous budget period should still be 15 euro
    And "Hobby" should not be shown in the current budget period
    When I add a category "hobby"
    Then I should be told that "Hobby" was brought back
    And the categories offered for a new expense should list "Hobby" once, and no other spelling of it

  # Deleted is not archived. Were it kept as archived, adding the name would say "brought back".
  Scenario: A deleted category is still gone after starting again, and adding its name creates a new one
    Given I have a category "Groceries"
    And I have a category "Magazines"
    When I delete the category "Magazines"
    And I close MoneyBud and start it again
    Then the categories offered for a new expense should not include "Magazines"
    And the categories offered for a new expense should include "Groceries"
    When I add a category "Magazines"
    Then I should be told that "Magazines" was created

  Scenario: A renamed category keeps its new name and all its history after starting again, in past periods too
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have recorded an expense of 380 euro for "Groceries" labelled "Markt" dated on the last day of the previous budget period
    And I have a budget of 300 euro for "Groceries" in the current budget period
    When I rename the category "Groceries" to "Food"
    And I close MoneyBud and start it again
    Then the categories offered for a new expense should list "Food" once, and no other spelling of it
    And the categories offered for a new expense should not include "Groceries"
    And "Food" should be shown in the previous budget period
    And "Groceries" should not be shown in the previous budget period
    And the remaining "Food" budget in the previous budget period should be 20 euro
    And the budget for "Food" in the current budget period should be 300 euro
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date                                       | category | label | amount |
      | the last day of the previous budget period | Food     | Markt | 380.00 |

  # Once Groceries is Food, its old name is free, and adding it makes a new, empty category
  # (rename-a-category.feature). After starting again the two must still be two: Markt belongs to
  # Food, and Kiosk to the new Groceries. Were categories told apart by name alone, Markt would
  # come back under Groceries, or Food's budget and spending would.
  Scenario: A category that took a renamed category's old name is still a separate category after starting again
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 150 euro for "Groceries" labelled "Markt" dated today
    When I rename the category "Groceries" to "Food"
    And I add a category "Groceries"
    And I record an expense of 12.50 euro for "Groceries" labelled "Kiosk"
    And I close MoneyBud and start it again
    Then the categories shown in the current budget period should be exactly these, in this order:
      | category  | budget | spent  | remaining | over budget |
      | Food      | 400.00 | 150.00 | 250.00    | no          |
      | Groceries | 0.00   | 12.50  | -12.50    | yes         |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount |
      | today | Groceries | Kiosk | 12.50  |
      | today | Food      | Markt | 150.00 |

  # With every budget at zero, categories are listed in the order they were added
  # (overview.feature). That order is kept: a category added after starting again comes last, and
  # an archived one brought back after starting again keeps its original place.
  Scenario: Categories keep the order they were added in, across starting again
    Given I have a category "Magazines"
    And I have a category "Groceries"
    And I have a category "Hobby"
    When I archive the category "Magazines"
    And I close MoneyBud and start it again
    And I add a category "Gifts"
    And I add a category "magazines"
    Then I should be told that "Magazines" was brought back
    And the categories shown in the current budget period should be exactly these, in this order:
      | category  |
      | Magazines |
      | Groceries |
      | Hobby     |
      | Gifts     |

  # ----------------------------------------------------------------------------------
  # A kept entry is the same entry
  #
  # An entry recorded in one run can be corrected in the next, and the correction is kept in its
  # turn. A change after starting again changes that entry: it does not leave the old one and add a
  # new one beside it.
  # ----------------------------------------------------------------------------------

  # 412.50 typed for 41.25, noticed the next time MoneyBud is opened.
  Scenario: An expense recorded before closing can be changed after starting again, and the change is kept
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    When I record an expense of 412.50 euro for "Groceries" labelled "Markt"
    And I close MoneyBud and start it again
    And I change the amount of the expense labelled "Markt" to 41.25 euro
    Then the change should go through
    And I should be told that the expense was changed
    When I close MoneyBud and start it again
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount |
      | today | Groceries | Markt | 41.25  |
    And the remaining "Groceries" budget in the current budget period should be 358.75 euro

  Scenario: An income recorded before closing can be removed after starting again, and stays removed
    When I record an income of 1832.45 euro labelled "Salaris"
    And I record an income of 25 euro labelled "Statiegeld"
    And I close MoneyBud and start it again
    And I remove the income labelled "Statiegeld" and confirm
    Then I should be told that the income was removed
    When I close MoneyBud and start it again
    Then the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | today | Salaris | 1832.45 |
    And Unassigned in the current budget period should be 1832.45 euro

  # Nothing is deduplicated (record-expense.feature), and keeping must not do it either: two
  # identical expenses are still two, and removing one still leaves the other.
  Scenario: Two identical expenses are still two after starting again, and removing one leaves the other
    Given I have a category "Groceries"
    When I record an expense of 3.50 euro for "Groceries" labelled "Coffee" dated today
    And I record an expense of 3.50 euro for "Groceries" labelled "Coffee" dated today
    And I close MoneyBud and start it again
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label  | amount |
      | today | Groceries | Coffee | 3.50   |
      | today | Groceries | Coffee | 3.50   |
    When I remove one of the two expenses labelled "Coffee" and confirm
    And I close MoneyBud and start it again
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label  | amount |
      | today | Groceries | Coffee | 3.50   |

  # Entries on the same date are listed newest recorded first (list-transactions-in-a-period.feature),
  # and a changed entry keeps its place (change-an-entry.feature). Both hold across starting again:
  # Krant, recorded after starting again, is newer than anything recorded before, and Bakker, changed
  # after starting again, stays where it was.
  Scenario: Entries on the same date keep their order across starting again, and one recorded afterwards is the newest
    Given I have a category "Groceries"
    When I record an expense of 18 euro for "Groceries" labelled "Bakker"
    And I record an expense of 3.50 euro for "Groceries" labelled "Coffee"
    And I close MoneyBud and start it again
    And I record an expense of 2.40 euro for "Groceries" labelled "Krant"
    And I change the amount of the expense labelled "Bakker" to 20 euro
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label  | amount |
      | today | Groceries | Krant  | 2.40   |
      | today | Groceries | Coffee | 3.50   |
      | today | Groceries | Bakker | 20.00  |
    When I close MoneyBud and start it again
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label  | amount |
      | today | Groceries | Krant  | 2.40   |
      | today | Groceries | Coffee | 3.50   |
      | today | Groceries | Bakker | 20.00  |

  # ----------------------------------------------------------------------------------
  # Only what went through is kept
  # ----------------------------------------------------------------------------------

  Scenario: Entries that were refused leave nothing to keep
    Given my budget periods are one month long
    And I have a category "Groceries"
    When I try to record an expense of 0.00 euro for "Groceries"
    Then the expense should not be recorded
    When I try to record an income of 250 euro without a label
    Then the income should not be recorded
    When I try to assign 40 euro to "Groceries" in the previous budget period
    Then the assignment should be refused
    When I close MoneyBud and start it again
    Then no expenses should be listed in the current budget period
    And no incomes should be listed in the current budget period
    And the budget for "Groceries" in the previous budget period should be 0.00 euro

  # ----------------------------------------------------------------------------------
  # What is on screen is not kept
  #
  # Only the ledger is kept. Whatever was on screen when MoneyBud closed, it opens on the current
  # budget period with nothing typed, nothing being changed and nothing asked.
  # ----------------------------------------------------------------------------------

  Scenario Outline: MoneyBud opens on the current budget period, whichever period was on screen when it closed
    Given my budget periods are one month long
    And the Overview shows the <shown> budget period
    When I close MoneyBud and start it again
    Then the Overview should show the current budget period

    Examples:
      | shown    |
      | previous |
      | next     |

  Scenario: An expense typed but not recorded is not kept
    Given I have a category "Groceries"
    When I type an expense of 12.50 euro for "Groceries" labelled "Kiosk" without recording it
    And I close MoneyBud and start it again
    Then the expense form should be empty and ready for a new expense
    And no expenses should be listed in the current budget period

  Scenario: A change typed but not saved is not kept
    Given I have a category "Groceries"
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I start changing the amount of the expense labelled "Albert Heijn" to 23.15 euro, and do not save it
    And I close MoneyBud and start it again
    Then the expense form should be empty and ready for a new expense
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |

  # Closing is not an answer. Nothing is removed until I confirm (remove-an-entry.feature), and I
  # never did.
  Scenario: An entry whose removal was asked for but not confirmed is still there after starting again
    Given I have a category "Groceries"
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I ask to remove the expense labelled "Albert Heijn"
    Then MoneyBud should be asking me to confirm
    When I close MoneyBud and start it again
    Then MoneyBud should not be asking me anything
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |

  Scenario: A rename typed but not saved is not kept
    Given I have a category "Groceries"
    When I start renaming the category "Groceries" to "Food", and do not save it
    And I close MoneyBud and start it again
    Then no rename should be in progress
    And the categories offered for a new expense should list "Groceries" once, and no other spelling of it
    And the categories offered for a new expense should not include "Food"

  # ----------------------------------------------------------------------------------
  # Time passes between runs
  #
  # A budget period ends but never closes, so a period that ended while MoneyBud was closed is
  # still there, with everything in it, when I step back to it. MoneyBud opens on the period that
  # is current now. From "start it again on the first day of the next budget period" on, periods
  # are named relative to the new today: the period the Givens called current is the previous one.
  # ----------------------------------------------------------------------------------

  # Salaris volgende maand was dated in the future when it was recorded, and its period has
  # since become the current one.
  Scenario: A period that ended while MoneyBud was closed is still there, whole, when I step back to it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Groceries"
    When I record an income of 1800 euro labelled "Salaris"
    And I assign 400 euro to "Groceries" in the current budget period
    And I record an expense of 412.50 euro for "Groceries" labelled "Markt"
    And I record an income of 1900 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    And I close MoneyBud, and start it again on the first day of the next budget period
    Then the Overview should show the current budget period
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date                                       | label                  | amount  |
      | the first day of the current budget period | Salaris volgende maand | 1900.00 |
    And Unassigned in the current budget period should be 1900 euro
    When I step back one budget period
    Then the Overview should show the previous budget period
    And the incomes listed in the previous budget period should be exactly these, in this order:
      | date                                       | label   | amount  |
      | the last day of the previous budget period | Salaris | 1800.00 |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date                                       | category  | label | amount |
      | the last day of the previous budget period | Groceries | Markt | 412.50 |
    And the remaining "Groceries" budget in the previous budget period should be -12.50 euro
    And "Groceries" should be shown as over budget in the previous budget period
    And Unassigned in the previous budget period should be 1400 euro

  # One continuous history, with no cut at the end of a year. With periods a month long, 13 periods
  # always cross the turn of a calendar year, whatever month today is in.
  Scenario: Nothing is dropped for being old, across the turn of a year
    Given my budget periods are one month long
    And I have a category "Groceries"
    When I record an expense of 20 euro for "Groceries" labelled "Markt" dated on the first day of the current budget period
    And I close MoneyBud, and start it again on the first day of the budget period 13 after the current one
    Then the Overview should show the current budget period
    And the expenses listed in the budget period 13 before the current one should be exactly these, in this order:
      | date                                                         | category  | label | amount |
      | the first day of the budget period 13 before the current one | Groceries | Markt | 20.00  |

  # ----------------------------------------------------------------------------------
  # Keeping asks nothing of me
  #
  # Saving happens by itself after every change (glossary: "Saved by itself, after every change").
  # There is nothing to press, and one set of data with no way to start over inside MoneyBud
  # (glossary: "One set of data, and no way to reset it in MoneyBud"). Saving that works is NOT
  # ANNOUNCED: confirmed by the stakeholder on 2026-09-26. The one exception is a save that
  # succeeds after one has failed, which says once that everything is saved again
  # (carry-on-when-saving-fails.feature).
  # ----------------------------------------------------------------------------------

  Scenario: Keeping data needs no act of mine, and says nothing when it works
    Given I have a category "Groceries"
    When I record an expense of 12.50 euro for "Groceries" labelled "Kiosk"
    Then the expense should be recorded
    And nothing should have been said about saving
    And MoneyBud should offer no act for saving
    And MoneyBud should offer no act for starting over
    When I close MoneyBud and start it again
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount |
      | today | Groceries | Kiosk | 12.50  |
