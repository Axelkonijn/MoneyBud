# Recording on an account: every income and expense is on an account, the pool account unless I
# choose another (glossary: "Every income and expense is on an account", "An expense defaults to the
# pool account"; "Accounts and net worth", settled by the stakeholder on 2026-09-27). What the strip
# and the lists then show is in show-accounts.feature, which also explains the shared steps.
# Recording itself, and changing and removing an entry, are unchanged, and are specified in
# record-expense.feature, record-income.feature, change-an-entry.feature and remove-an-entry.feature.
#
# The rules, from arc42 §12:
#   - EVERY INCOME AND EXPENSE IS ON AN ACCOUNT. On both forms the account is chosen from a LIST OF
#     MY ACCOUNTS, not typed, and the list STARTS OUT ON THE POOL ACCOUNT. The list is in the
#     strip's order: the pool account first, then the rest in the order added (show-accounts.feature).
#     An expense against a BACKED category starts out on its backing account instead, once the
#     category is typed (spend-against-a-backed-category.feature). No category below is backed, so
#     every expense here starts out on the pool account.
#     (Revised for increment 15, 2026-10-04, approved at the scenario gate on 2026-10-04: an expense against a backed category IS
#     ALWAYS ON ITS BACKING ACCOUNT, and the list is locked, from the period the category got its account.
#     Everything below is about categories without an account, so no scenario here changes.)
#   - Choosing another account is FOR THAT ONE ENTRY. It changes nothing about the next.
#   - An expense lowers its account's Balance and an income raises it, and each STILL COUNTS ON THE
#     PURPOSE SIDE EXACTLY AS BEFORE, whichever account it is on: an expense against its category's
#     Remaining, an income in its period's Unassigned.
#   - An entry's ACCOUNT CAN BE CHANGED like anything else about it, which moves the amount from one
#     balance to the other. Changing its amount moves its balance by the difference. Removing it
#     moves its balance back. Each of these is subject to the account's balance corrections: an
#     entry dated before an account's latest typed balance is already in that balance, so correcting
#     it moves nothing there (correct-a-balance.feature). Every scenario below avoids that on
#     purpose, so that what is shown here is the plain rule.
#   - A change is judged and announced exactly as before (change-an-entry.feature). The account is a
#     choice from a list, so no account I have can be refused, and there is no refusal to specify.
#
# NOT specified here, and left to developer tests and the markup, as the first demo round left the
# field order: that the account is the LAST field of each form, after Datum; that it is a list and
# not free text; and what the form shows after an entry goes through.
#
# Reading the steps:
#   - "... on the account "X"" chooses X for the entry. Without it, the entry is on the account the
#     form starts out on.
#   - "I change the account of the expense (income) labelled "X" to "Y"" is change-an-entry.feature's
#     grammar, for the one new thing that can be changed.
#   - The shared account steps are explained in show-accounts.feature. The rest are reused unchanged
#     from the file that introduced them.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names, labels and amounts are
# synthetic test data.

@accounts
Feature: Record on an account
  As someone who pays from more than one account, cash included
  I want each income and expense to say which account it went into or came out of, the pool account unless I choose another
  So that every account's balance follows what I record, and money paid in cash is not taken off my bank balance

  # ----------------------------------------------------------------------------------
  # Which account a new entry is on
  # ----------------------------------------------------------------------------------

  Scenario: A new expense and a new income start out on the pool account, and any of my accounts can be chosen
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have an account "Savings" with a starting balance of 5000 euro
    Then a new expense should start out on the account "Bank"
    And a new income should start out on the account "Bank"
    And the accounts offered for a new expense should be exactly these, in this order:
      | account |
      | Bank    |
      | Cash    |
      | Savings |
    And the accounts offered for a new income should be exactly these, in this order:
      | account |
      | Bank    |
      | Cash    |
      | Savings |

  Scenario: An expense comes off the pool account unless I choose another
    Given I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 40 euro
    And I have a budget of 400 euro for "Groceries" in the current budget period
    When I record an expense of 25 euro for "Groceries" labelled "Markt"
    Then the expense should be recorded
    And the balance of "Bank" should be 1807.45 euro
    And the balance of "Cash" should still be 40 euro
    And net worth should be 1847.45 euro
    And the remaining "Groceries" budget in the current budget period should be 375 euro

  # The cash at the market: the same expense, the same category, the same Remaining. Only where the
  # money came from differs.
  Scenario: An expense on the account I choose comes off that account, and counts against its category just the same
    Given I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 40 euro
    And I have a budget of 400 euro for "Groceries" in the current budget period
    When I record an expense of 25 euro for "Groceries" labelled "Markt" on the account "Cash"
    Then the expense should be recorded
    And the balance of "Cash" should be 15 euro
    And the balance of "Bank" should still be 1832.45 euro
    And net worth should be 1847.45 euro
    And the remaining "Groceries" budget in the current budget period should be 375 euro

  # Interest recorded as an income, so that it can be budgeted: the glossary's usage note. It joins
  # Unassigned like any income, whichever account it went into.
  Scenario: An income goes into the pool account unless I choose another, and joins Unassigned either way
    Given I have an account "Savings" with a starting balance of 5000 euro
    When I record an income of 1832.45 euro labelled "Salaris"
    And I record an income of 12.50 euro labelled "Rente" on the account "Savings"
    Then the balance of "Bank" should be 1832.45 euro
    And the balance of "Savings" should be 5012.50 euro
    And net worth should be 6844.95 euro
    And Unassigned in the current budget period should be 1844.95 euro

  # The override is per entry (glossary: "An expense defaults to the pool account").
  Scenario: Choosing an account is for that one entry, and the next one starts out on the pool account again
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    When I record an expense of 25 euro for "Groceries" labelled "Markt" on the account "Cash"
    Then a new expense should start out on the account "Bank"
    When I record an income of 10 euro labelled "Verkocht" on the account "Cash"
    Then a new income should start out on the account "Bank"

  # ----------------------------------------------------------------------------------
  # Changing an entry's account, or its amount
  #
  # A change is judged and announced exactly as in change-an-entry.feature. Only the balances are
  # new: the amount leaves one account and arrives in the other, and nothing on the purpose side
  # moves.
  # ----------------------------------------------------------------------------------

  Scenario: Changing an expense's account moves it from one balance to the other, and nothing else
    Given I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 40 euro
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated today
    When I change the account of the expense labelled "Markt" to "Cash"
    Then the change should go through
    And I should be told that the expense was changed
    And I should not be warned or asked to confirm
    And the balance of "Bank" should be 1832.45 euro
    And the balance of "Cash" should be 15 euro
    And net worth should still be 1847.45 euro
    And the remaining "Groceries" budget in the current budget period should still be 375 euro
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount | account |
      | today | Groceries | Markt | 25.00  | Cash    |

  Scenario: Changing an income's account moves it from one balance to the other, and Unassigned stays as it was
    Given I have an account "Savings" with a starting balance of 5000 euro
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    When I change the account of the income labelled "Salaris" to "Savings"
    Then the change should go through
    And I should be told that the income was changed
    And the balance of "Bank" should be 0.00 euro
    And the balance of "Savings" should be 6832.45 euro
    And net worth should still be 6832.45 euro
    And Unassigned in the current budget period should still be 1832.45 euro

  Scenario Outline: Changing an expense's amount moves its account's balance by the difference
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated today on the account "Cash"
    When I change the amount of the expense labelled "Markt" to <new amount> euro
    Then the change should go through
    And the balance of "Cash" should be <cash> euro
    And the balance of "Bank" should still be 0.00 euro
    And the remaining "Groceries" budget in the current budget period should be <remaining> euro

    Examples:
      | new amount | cash  | remaining |
      | 32.15      | 7.85  | 367.85    |
      | 24.99      | 15.01 | 375.01    |
      | 40.00      | 0.00  | 360.00    |
      | 40.01      | -0.01 | 359.99    |

  # A refused change leaves the entry exactly as it was (change-an-entry.feature), and so leaves
  # both balances as they were.
  Scenario: A change that is refused leaves the entry on its account, and every balance as it was
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated today
    When I try to change the expense labelled "Markt" into an expense of 0.00 euro for "Groceries" labelled "Markt" dated on today on the account "Cash"
    Then the change should be refused
    And I should be told that an expense must be more than 0 euro
    And the balance of "Bank" should still be -25 euro
    And the balance of "Cash" should still be 40 euro
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount | account |
      | today | Groceries | Markt | 25.00  |         |

  # ----------------------------------------------------------------------------------
  # Removing an entry moves its account's balance back
  #
  # Removing asks first, exactly as in remove-an-entry.feature.
  # ----------------------------------------------------------------------------------

  Scenario Outline: Removing an entry moves its account's balance back
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have an account "Savings" with a starting balance of 5000 euro
    And I have a category "Groceries"
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated today on the account "Cash"
    And I have recorded an income of 12.50 euro labelled "Rente" dated today on the account "Savings"
    When I remove the <entry> labelled "<label>" and confirm
    Then I should have been asked to confirm first
    And I should be told that the <entry> was removed
    And the accounts should be exactly these, in this order:
      | account | balance   |
      | Bank    | 0.00      |
      | Cash    | <cash>    |
      | Savings | <savings> |
    And net worth should be <net worth> euro

    Examples:
      | entry   | label | cash  | savings | net worth |
      | expense | Markt | 40.00 | 5012.50 | 5052.50   |
      | income  | Rente | 15.00 | 5000.00 | 5015.00   |
