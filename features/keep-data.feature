# Keeping data: what I entered is still there after I close MoneyBud and start it again (glossary:
# "What MoneyBud keeps", settled by the stakeholder on 2026-09-26). Starting MoneyBud (no kept data
# yet, data it cannot read, a second start while it is open) is in start-moneybud.feature. A save
# that fails is in carry-on-when-saving-fails.feature.
#
# The rules, from arc42 §12:
#   - EVERYTHING IS KEPT, as one continuous history, for as long as I have it. There is no fresh
#     start per year, and nothing is dropped for being old.
#   - "Everything" is THE LEDGER: categories, archived or not, renamed ones with their history,
#     budgets, expenses and incomes, and since the accounts increment ACCOUNTS too: each with its
#     starting balance and balance corrections, the account every income and expense is on, the
#     transfers, and which account is the pool account. Since the backing increment, which account
#     backs each category, and the money moved on a category's behalf, including money assigned for
#     a later period that has not moved yet. Since the sweep increment, the sweep destination, every
#     sweep and every difference moved since, with the period each was for and the category it went
#     into, which categories were backed when each period ended, and when MoneyBud was first
#     started (sweep-at-a-period-end.feature, start-moneybud.feature). Since the recurring increment,
#     every repeat: which occurrence is its latest, how often it repeats, the day it repeats on, its
#     next date, and whether it has been stopped (repeat-an-entry.feature). Since the period start day
#     increment, the day budget periods start on, AS A HISTORY OF CHANGES, since every earlier period
#     keeps the boundaries it had (change-the-period-start-day.feature). So is the order things were
#     recorded in, which decides whether an entry on a balance correction's day is in it
#     (correct-a-balance.feature), and
#     whether an expense on the day of backing counts against Accumulated
#     (spend-against-a-backed-category.feature). WHAT IS ON SCREEN IS NOT KEPT: the period shown, an
#     entry half typed, an entry being changed, a question waiting for an answer, a rename in
#     progress. So MoneyBud ALWAYS OPENS ON THE CURRENT BUDGET PERIOD.
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
# here involves more than one version. The version with accounts does NOT read data kept by a
# version without them: it says so and closes, which is a row of start-moneybud.feature's outline
# for data MoneyBud cannot read (glossary: "Saved data from before accounts").
#
# ADDED AND REVISED FOR INCREMENT 15, 2026-10-04 (glossary: "Vrij, and moving Opgebouwd"; not yet approved
# at the scenario gate):
#   - THE DATA PROMISE IS KEPT (ruling 7, left by the stakeholder to the documentation: "Make the choice
#     yourself, based on what is efficient"). Data kept by the version before this increment, the first
#     version the promise covers (ADR 0014), IS READ, AS DATA IN WHICH NOTHING WAS GIVEN A PURPOSE YET.
#     EVERY FIGURE SHOWN BEFORE THE UPDATE READS THE SAME AFTER IT; Unclaimed is new, and shows at once
#     what an account held that no category claims. So, for the first time, scenarios here DO involve two
#     versions: the section "Data kept before Unclaimed" (derived, open at this gate). He added that he will
#     most likely start over anyway; that stays his choice.
#   - DERIVED, AND SHOWN SO THAT IT IS APPROVED KNOWINGLY: that version allowed an expense against a backed
#     category on another account. Such an expense IS READ AS IT IS, on the account it was put on, and the
#     figures follow the general rules. Opening it and saving it puts it on the backing account, since a
#     change is judged as if recorded now, and that is a change.
#   - New sections: what was reallocated is kept, and so is the money a category on "—" left behind and
#     the account it is on.
#   - One scenario is revised: "Whether an expense was recorded before or after the backing, on the day of
#     backing, is kept" put Fiets, a Savings expense recorded after Savings was backed, on Bank. It is now on
#     Deposit, as it must be. What the scenario keeps, Accumulated, reads the same.
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
#   - Added for increment 15: "what follows was kept by the version of MoneyBud from before Unclaimed",
#     as a Given, means the Givens after it describe what that version kept, done under its rules:
#     there, an expense against a backed category could be put on another account. "I start MoneyBud,
#     updated to the version with Unclaimed" starts the version this increment builds on that data, on the
#     same day.
#   - "nothing should have been said about saving" means no message mentions saving: when saving
#     works, and has not failed before, it is not announced. "MoneyBud should offer no act for
#     saving (starting over)" means no button or other act for it exists anywhere on the screen.
#   - Every other step is reused unchanged from the file that introduced it, with the meaning that
#     file gives it: the list steps are list-transactions-in-a-period.feature's, the category row
#     steps overview.feature's, the correcting steps change-an-entry.feature's and
#     remove-an-entry.feature's, the account steps show-accounts.feature's, the backing steps
#     back-a-category.feature's, the Unclaimed steps show-unclaimed.feature's, the reallocating steps
#     reallocate-an-amount.feature's, the sweep steps sweep-at-a-period-end.feature's, the steps about
#     repeats repeat-an-entry.feature's, which names dates as calendar dates, and the steps about the
#     period start day change-the-period-start-day.feature's.
#
# The empty ledger every scenario starts from holds one account, "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names, labels and amounts are
# synthetic test data.

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
  # Accounts are kept
  # ----------------------------------------------------------------------------------

  # One of each thing the accounts increment added. Bank: the salary, less the transfer, is
  # 1782.45, corrected to 1780. Cash: 40, less Markt, plus the transfer. Savings: as added.
  Scenario: Accounts, their starting balances, transfers and balance corrections are all there after starting again
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Groceries"
    When I record an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I add an account "Cash" with a starting balance of 40 euro
    And I add an account "Savings" with a starting balance of 5000 euro
    And I record an expense of 25 euro for "Groceries" labelled "Markt" on the account "Cash"
    And I record a transfer of 50 euro from "Bank" to "Cash"
    And I correct the balance of "Bank" to 1780 euro
    And I close MoneyBud and start it again
    Then the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1780.00 |
      | Cash    | 65.00   |
      | Savings | 5000.00 |
    And net worth should be 6845 euro
    And the pool account should be "Bank"
    And the history of "Bank" should be exactly these, newest first:
      | date      | entry              | label   | from | to   | amount  | balance | difference |
      | today     | balance correction |         |      |      |         | 1780.00 | -2.45      |
      | today     | transfer           |         | Bank | Cash | 50.00   |         |            |
      | yesterday | income             | Salaris |      |      | 1832.45 |         |            |
    And the history of "Cash" should be exactly these, newest first:
      | date  | entry            | category  | label | from | to   | amount | balance |
      | today | transfer         |           |       | Bank | Cash | 50.00  |         |
      | today | expense          | Groceries | Markt |      |      | 25.00  |         |
      | today | starting balance |           |       |      |      |        | 40.00   |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount | account |
      | today | Groceries | Markt | 25.00  | Cash    |

  # Which entries a balance correction has in it depends, on its own day, on the order things were
  # recorded in. Were that order lost, Diner could land before the balance correction, and Bank would
  # read 990 after starting again instead of 960.
  Scenario: Whether an entry was recorded before or after a balance correction on the same day is kept
    Given I have a category "Groceries"
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have recorded an expense of 12.50 euro for "Groceries" labelled "Lunch" dated today
    When I correct the balance of "Bank" to 990 euro
    And I record an expense of 30 euro for "Groceries" labelled "Diner" dated today
    And I close MoneyBud and start it again
    Then the balance of "Bank" should be 960 euro
    And the balance correction of "Bank" to 990 euro should show a difference of 2.50 euro

  # Savings is the pool account, so it comes first. ING was the first account added, and keeps
  # that place under its new name.
  Scenario: A renamed account, the pool account and a deleted account are all as I left them after starting again
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have an account "Savings" with a starting balance of 5000 euro
    When I rename the account "Bank" to "ING"
    And I make "Savings" the pool account
    And I delete the account "Cash"
    And I close MoneyBud and start it again
    Then the accounts should be exactly these, in this order:
      | account | balance |
      | Savings | 5000.00 |
      | ING     | 0.00    |
    And the pool account should be "Savings"
    And a new expense should start out on the account "Savings"
    When I add an account "Cash" with a starting balance of 10 euro
    Then I should be told that the account "Cash" was added

  # ----------------------------------------------------------------------------------
  # Backing is kept
  # ----------------------------------------------------------------------------------

  # One of each thing the backing increment added. 300 moved today, 50 was spent from Deposit, and
  # 200 is planned for the next period, not moved yet. Starting again on that period's first day,
  # the planned 200 moves then, and the rest is as it was.
  Scenario: Backing, the money moved for a category and Accumulated are all there after starting again, and planned money still moves on its day
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Savings"
    When I record an income of 2000 euro labelled "Salaris"
    And I add an account "Deposit" with a starting balance of 0 euro
    And I set the backing account of "Savings" to "Deposit"
    And I assign 300 euro to "Savings" in the current budget period
    And I assign 200 euro to "Savings" in the next budget period
    And I record an expense of 50 euro for "Savings" labelled "Fiets" on the account "Deposit"
    And I close MoneyBud and start it again
    Then the backing account of "Savings" should be "Deposit"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1700.00 |
      | Deposit | 250.00  |
    And Accumulated for "Savings" in the current budget period should be 250 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | category | label | from | to      | amount | balance |
      | today | expense          | Savings  | Fiets |      |         | 50.00  |         |
      | today | movement         | Savings  |       | Bank | Deposit | 300.00 |         |
      | today | starting balance |          |       |      |         |        | 0.00    |
    When I close MoneyBud, and start it again on the first day of the next budget period
    Then the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1500.00 |
      | Deposit | 450.00  |
    And Accumulated for "Savings" in the current budget period should be 450 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date                                       | entry            | category | label | from | to      | amount | balance |
      | the first day of the current budget period | movement         | Savings  |       | Bank | Deposit | 200.00 |         |
      | the last day of the previous budget period | expense          | Savings  | Fiets |      |         | 50.00  |         |
      | the last day of the previous budget period | movement         | Savings  |       | Bank | Deposit | 300.00 |         |
      | the last day of the previous budget period | starting balance |          |       |      |         |        | 0.00    |

  # Which expenses lower Accumulated depends, on the day of backing, on the order things were
  # recorded in (spend-against-a-backed-category.feature). Voorschot came before the backing, and
  # Fiets after it. 200 moved at the backing, and only Fiets counts against it. Were that order lost,
  # Accumulated would read 70 or 200 instead of 170.
  #
  # Revised for increment 15: Fiets was put on Bank, which an expense against a backed category can no
  # longer be. It is now on Deposit, the only account it can be on. Accumulated reads 170 as before.
  Scenario: Whether an expense was recorded before or after the backing, on the day of backing, is kept
    Given I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have recorded an expense of 100 euro for "Savings" labelled "Voorschot" dated today on the account "Bank"
    When I set the backing account of "Savings" to "Deposit"
    And I record an expense of 30 euro for "Savings" labelled "Fiets"
    And I close MoneyBud and start it again
    Then Accumulated for "Savings" in the current budget period should be 170 euro

  # ----------------------------------------------------------------------------------
  # What was reallocated is kept (new in increment 15)
  #
  # A move of purpose within one account moves no balance, yet it must survive starting again: Unclaimed
  # and Accumulated depend on it, and so does its row (reallocate-an-amount.feature). So must the money a
  # category on "—" left behind, and which account it is on, since "—" itself no longer says.
  # ----------------------------------------------------------------------------------

  # 3000 given to Savings on Deposit, then 500 of it moved to Unassigned, which moved it to Bank. After
  # starting again every figure and both rows are as they were.
  Scenario: Unclaimed, what was reallocated, and its rows are all there after starting again
    Given I have a category "Savings"
    When I record an income of 2000 euro labelled "Salaris"
    And I add an account "Deposit" with a starting balance of 5000 euro
    And I set the backing account of "Savings" to "Deposit"
    And I reallocate 3000 euro from Unclaimed on "Deposit" to "Savings"
    And I reallocate 500 euro from "Savings" to Unassigned in the current budget period
    And I close MoneyBud and start it again
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 2500.00 |           |
      | Deposit | 4500.00 | 2000.00   |
    And Accumulated for "Savings" in the current budget period should be 2500 euro
    And Unassigned in the current budget period should be 2500 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | category | from      | to         | amount  | balance |
      | today | reallocation     |          | Savings   | Unassigned | 500.00  |         |
      | today | reallocation     |          | Unclaimed | Savings    | 3000.00 |         |
      | today | starting balance |          |           |            |         | 5000.00 |

  # Savings is set to "—" with 300 given from Unclaimed, which stays on Deposit. After starting again it
  # is still there, still Savings', still on Deposit; and backing Savings with Broker takes it along.
  Scenario: The money a category on none left behind, and the account it is on, are there after starting again
    Given I have a category "Savings"
    When I add an account "Deposit" with a starting balance of 300 euro
    And I add an account "Broker" with a starting balance of 0 euro
    And I set the backing account of "Savings" to "Deposit"
    And I reallocate 300 euro from Unclaimed on "Deposit" to "Savings"
    And I remove the backing of "Savings"
    And I close MoneyBud and start it again
    Then "Savings" should not be backed
    And Accumulated for "Savings" in the current budget period should be 300 euro, on "Deposit"
    And the Unclaimed of "Deposit" should be 0.00 euro
    When I set the backing account of "Savings" to "Broker"
    Then I should be told that "Savings" is now backed by "Broker", and that 300 euro moved from "Deposit" to "Broker"
    And Accumulated for "Savings" in the current budget period should be 300 euro

  # ----------------------------------------------------------------------------------
  # Data kept before Unclaimed (new in increment 15)
  #
  # Ruling 7: the data promise is kept. What the version before this increment kept is read as data in
  # which nothing was given a purpose yet, and every figure it showed reads the same. Derived, open at this
  # gate (see the header).
  # ----------------------------------------------------------------------------------

  # Deposit was added with 5000, and Savings, backed by it, was given 300. The update shows every figure
  # as it was, and Unclaimed, new, shows the 5000 that no category claims. Starting the update says
  # nothing (this file's reading: nothing about it needs me).
  Scenario: Data kept before Unclaimed opens with every figure as it was, and shows what no category claims
    Given what follows was kept by the version of MoneyBud from before Unclaimed
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    When I start MoneyBud, updated to the version with Unclaimed
    Then I should not have been told anything
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 1700.00 |           |
      | Deposit | 5300.00 | 5000.00   |
    And net worth should be 7000 euro
    And the backing account of "Savings" should be "Deposit"
    And Accumulated for "Savings" in the current budget period should be 300 euro
    And Unassigned in the current budget period should be 1700 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | category | from | to      | amount | balance |
      | today | movement         | Savings  | Bank | Deposit | 300.00 |         |
      | today | starting balance |          |      |         |        | 5000.00 |

  # Derived, and shown so that it is approved knowingly. The old version let Fiets, a Savings expense, be
  # put on Bank while Deposit backed Savings. It is read as it was: on Bank. Accumulated counts it, as it
  # always did, but what Deposit holds for Savings does not, so here, and only in such kept data,
  # Unclaimed plus Accumulated falls short of Deposit's balance by the 120. Opened and saved, Fiets goes onto
  # Deposit, since its list is now locked there, and the figures add up again. That is a change, and is
  # announced as one.
  Scenario: An expense kept on another account than its category's backing account stays there until it is changed
    Given what follows was kept by the version of MoneyBud from before Unclaimed
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have recorded an expense of 120 euro for "Savings" labelled "Fiets" dated today on the account "Bank"
    When I start MoneyBud, updated to the version with Unclaimed
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 1580.00 |           |
      | Deposit | 300.00  | 0.00      |
    And Accumulated for "Savings" in the current budget period should be 180 euro
    And the expense labelled "Fiets" should open on the account "Deposit", locked
    When I save the expense labelled "Fiets" without changing anything
    Then the change should go through
    And I should be told that the expense was changed
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 1700.00 |           |
      | Deposit | 180.00  | 0.00      |
    And Accumulated for "Savings" in the current budget period should still be 180 euro

  # ----------------------------------------------------------------------------------
  # The sweep is kept
  # ----------------------------------------------------------------------------------

  # One of each thing the sweep increment added. The 2000 swept into Savings, with the period it was
  # for, is still there after starting again, and so is the destination changed to Holiday since. A
  # 40 euro receipt found afterwards shows as swept too much, and comes back from Savings, the
  # category the money went into, not from Holiday. Were the category of the sweep not kept, it could
  # not. What the button moved is kept in its turn, and the period asks nothing more.
  Scenario: The sweep destination, what was swept for each period, and into which category, are all there after starting again
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Groceries"
    And I have a category "Savings"
    And I have a category "Holiday"
    When I record an income of 2000 euro labelled "Salaris"
    And I add an account "Deposit" with a starting balance of 0 euro
    And I add an account "Broker" with a starting balance of 0 euro
    And I set the backing account of "Savings" to "Deposit"
    And I set the backing account of "Holiday" to "Broker"
    And I set the sweep destination to "Savings"
    And I close MoneyBud, and start it again on the first day of the next budget period
    Then I should be told that the period leftover of the previous budget period, 2000 euro, was swept into "Savings"
    When I set the sweep destination to "Holiday"
    And I record an expense of 40 euro for "Groceries" labelled "Bon" dated on the last day of the previous budget period
    And I close MoneyBud and start it again
    Then the sweep destination shown in the current budget period should be "Holiday"
    And the previous budget period should show 40 euro of its period leftover swept too much
    When I bring the swept amount of the previous budget period up to date
    Then I should be told that 40 euro swept too much for the previous budget period was taken back from "Savings"
    When I close MoneyBud and start it again
    Then the previous budget period should show that 1960 euro was swept into "Savings"
    And I should not be able to bring the swept amount of the previous budget period up to date
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 1960.00 |
      | Broker  | 0.00    |

  # Whether a category counts in a period's leftover is judged by its backing when the period ended
  # (show-an-ended-period.feature). Hobby was unbacked then, so its 60 was swept. Backed since, it
  # must still count as unbacked for that period after starting again. Were the backing at the
  # period's end not kept, the period would show 60 swept too much.
  Scenario: Which categories were backed when a period ended is there after starting again
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Hobby"
    And I have a category "Savings"
    When I record an income of 1000 euro labelled "Salaris"
    And I add an account "Deposit" with a starting balance of 0 euro
    And I set the backing account of "Savings" to "Deposit"
    And I set the sweep destination to "Savings"
    And I assign 100 euro to "Hobby" in the current budget period
    And I record an expense of 40 euro for "Hobby" labelled "Verf"
    And I close MoneyBud, and start it again on the first day of the next budget period
    Then I should be told that the period leftover of the previous budget period, 960 euro, was swept into "Savings"
    When I set the backing account of "Hobby" to "Deposit"
    And I close MoneyBud and start it again
    Then the previous budget period should show that 960 euro was swept into "Savings"
    And I should not be able to bring the swept amount of the previous budget period up to date

  # ----------------------------------------------------------------------------------
  # Repeats are kept
  #
  # A repeat is more than its entries (repeat-an-entry.feature, change-a-repeat.feature): after a short
  # month no occurrence's date says which day it repeats on, and after its latest occurrence is removed
  # no entry says when the next comes. Both have to survive starting again.
  # ----------------------------------------------------------------------------------

  # One weekly and one monthly repeat. Starting again on the same day records nothing and says nothing.
  # The label is still on each latest row, Netflix still opens as monthly, and the next occurrences
  # still come on their dates: Bijbaan on 1, 8, 15 and 22 September, Netflix on 25 September.
  Scenario: A repeat is there after starting again: its latest occurrence carries the label and opens with its frequency, and the next still come on their dates
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    When I record an expense of 13.99 euro for "Subscriptions" labelled "Netflix", repeating monthly
    And I record an income of 85 euro labelled "Bijbaan", repeating weekly
    And I close MoneyBud and start it again
    Then I should not have been told anything
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date           | category      | label   | amount | repeats |
      | 25 August 2026 | Subscriptions | Netflix | 13.99  | monthly |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date           | label   | amount | repeats |
      | 25 August 2026 | Bijbaan | 85.00  | weekly  |
    And the expense labelled "Netflix" dated 25 August 2026 should open with the frequency monthly, changeable
    When I close MoneyBud, and start it again on 1 September 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount |
      | income |          | Bijbaan | 85.00  |
    When I close MoneyBud, and start it again on 25 September 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | income  |               | Bijbaan | 85.00  |
      | income  |               | Bijbaan | 85.00  |
      | income  |               | Bijbaan | 85.00  |
      | expense | Subscriptions | Netflix | 13.99  |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date              | label   | amount | repeats |
      | 22 September 2026 | Bijbaan | 85.00  | weekly  |
      | 15 September 2026 | Bijbaan | 85.00  |         |
      | 8 September 2026  | Bijbaan | 85.00  |         |
      | 1 September 2026  | Bijbaan | 85.00  |         |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  | monthly |

  # After 28 February the only date on the page says the 28th. Were the day it repeats on not kept,
  # March's would come on the 28th after starting again, instead of on the 31st.
  Scenario: The day a month-end repeat comes on is kept, though no occurrence's date shows it
    Given my budget periods are one month long
    And today is 31 January 2027
    And I have a category "Rent"
    When I record an expense of 900 euro for "Rent" labelled "Huur", repeating monthly
    And I close MoneyBud, and start it again on 28 February 2027
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category | label | amount |
      | expense | Rent     | Huur  | 900.00 |
    When I close MoneyBud, and start it again on 30 March 2027
    Then I should not have been told anything
    When I close MoneyBud, and start it again on 31 March 2027
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category | label | amount |
      | expense | Rent     | Huur  | 900.00 |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date          | category | label | amount | repeats |
      | 31 March 2027 | Rent     | Huur  | 900.00 | monthly |

  # Stopped stays stopped, and the earlier occurrence stays locked, while the stopped repeat's last
  # occurrence stays changeable (change-a-repeat.feature).
  Scenario: A stopped repeat is still stopped after starting again
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    When I record an expense of 13.99 euro for "Subscriptions" labelled "Netflix", repeating monthly
    And the day becomes 25 September 2026 while MoneyBud is open
    And I step forward one budget period
    And I change the frequency of the expense labelled "Netflix" dated 25 September 2026 to one-off
    And I close MoneyBud, and start it again on 26 October 2026
    Then I should not have been told anything
    And no expenses should be listed in the current budget period
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  |         |
    When I step back one budget period
    Then the expense labelled "Netflix" dated 25 September 2026 should open with the frequency one-off, changeable
    When I step back one budget period
    Then the expense labelled "Netflix" dated 25 August 2026 should open with the frequency one-off, locked

  # Follow-up 1 across a restart. October's Netflix was moved to the 20th and then removed. No entry left
  # says the 20th: September's is dated the 25th. Were the next date not kept, November's would come on
  # the 25th, or October's would be recorded again.
  Scenario: After the latest occurrence is removed, its next date is kept, a day moved on the removed occurrence included
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    When I record an expense of 13.99 euro for "Subscriptions" labelled "Netflix", repeating monthly
    And I close MoneyBud, and start it again on 26 October 2026
    And I change the date of the expense labelled "Netflix" dated 25 October 2026 to 20 October 2026
    And I remove the expense labelled "Netflix" dated 20 October 2026 and confirm
    And I close MoneyBud and start it again
    Then I should not have been told anything
    And no expenses should be listed in the current budget period
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  | monthly |
    When I close MoneyBud, and start it again on 20 November 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date             | category      | label   | amount | repeats |
      | 20 November 2026 | Subscriptions | Netflix | 13.99  | monthly |

  # ----------------------------------------------------------------------------------
  # The period start day is kept
  #
  # A changed start day must survive starting again, and so must every period an earlier change left
  # behind: the calendar is a history of start days, not one (glossary: "A configurable period start day",
  # "What else a change meets"). The steps are change-the-period-start-day.feature's.
  # ----------------------------------------------------------------------------------

  # The start day, the periods it made, and October's plan, which went into the new current period, are
  # all as they were. Starting again says nothing and asks nothing.
  Scenario: A changed start day, the periods it made, and a plan it moved are all there after starting again
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have a category "Groceries"
    When I assign 400 euro to "Groceries" in the next budget period
    And I change the period start day to the 27th and confirm
    And I close MoneyBud and start it again
    Then I should not have been told anything
    And the Overview should show the current budget period
    And the period start day shown in the current budget period should be the 27th
    And the current budget period should run from 27 September 2026 to 26 October 2026
    And the previous budget period should run from 1 September 2026 to 26 September 2026
    And the budget period 2 before the current one should run from 1 August 2026 to 31 August 2026
    And the budget for "Groceries" in the current budget period should be 400 euro

  # Two changes. On 5 October, back to the 1st, which leaves 27 to 30 September as a short period. Were
  # only the latest start day kept, September would be read as one whole month again after starting
  # again, and the two short periods would be gone.
  Scenario: Every change is kept, so the short periods between changes are still there after starting again
    Given my budget periods are one month long
    And today is 29 September 2026
    When I change the period start day to the 27th and confirm
    And the day becomes 5 October 2026 while MoneyBud is open
    And I change the period start day to the 1st and confirm
    And I close MoneyBud, and start it again on 2 November 2026
    Then the period start day shown in the current budget period should be the 1st
    And the current budget period should run from 1 November 2026 to 30 November 2026
    And the previous budget period should run from 1 October 2026 to 31 October 2026
    And the budget period 2 before the current one should run from 27 September 2026 to 30 September 2026
    And the budget period 3 before the current one should run from 1 September 2026 to 26 September 2026
    And the budget period 4 before the current one should run from 1 August 2026 to 31 August 2026

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
