# Adding an account: a place where money really sits, with the balance the bank shows for it today
# (glossary: Account, Starting balance; "Accounts and net worth", settled by the stakeholder on
# 2026-09-27). What the strip then shows is in show-accounts.feature, which also explains the steps
# every accounts file shares. Renaming and deleting an account are in manage-accounts.feature.
#
# The rules, from arc42 §12:
#   - An account is ADDED WITH A NAME AND A STARTING BALANCE. There are no account kinds: current
#     account, savings account, cash, each is just a name and what is on it.
#   - The starting balance MAY BE LEFT EMPTY, and then the account has NO STARTING BALANCE. It is
#     like the first start's Betaalrekening: its balance is the plain sum of what is on it,
#     whatever the dates (an income dated in the future still only from its date), until I first
#     correct it. That is NOT the same as typing 0. A starting balance of 0 is a balance I checked,
#     and has in it every entry dated before it. Ruled by the stakeholder on 2026-09-27. A starting
#     balance of ONLY SPACES is the same as one left empty, as a label that trims to nothing is no
#     label: ruled at the scenario gate, 2026-09-27, and added as one row after it. Text that is not
#     an amount, such as "abc", is still refused.
#   - A STARTING BALANCE IS WHAT THE BANK SAYS TODAY. It is dated the day it is typed, and it is the
#     account's first balance correction, with everything that follows from that
#     (correct-a-balance.feature): every entry dated BEFORE today is already in it, and one dated
#     today moves the balance if it is recorded after the account was added. So the balance to type
#     is today's, not the one on last week's statement.
#   - A starting balance is NET WORTH ONLY. It is money I already had: not income, in no period's
#     Unassigned, and it changes no budget figure. Counting it as income was rejected, because adding
#     a savings account would suddenly give thousands to assign.
#   - It may be ZERO or NEGATIVE (derived), for an account that is empty or already overdrawn. It is
#     whole cents, like every amount, and typed like every amount (type-an-amount.feature).
#   - The NAME follows the category name rules (derived): trimmed at the ends, stored otherwise as
#     typed, compared with the ends trimmed, a run of inner spaces counted as one and case ignored,
#     and refused if it trims to nothing. It is UNIQUE AMONG ACCOUNTS. An account MAY SHARE A NAME
#     WITH A CATEGORY: Savings the account and Savings the category are different dimensions.
#   - Adding an account does not change which account is the pool account (manage-accounts.feature).
#
#   - ADDING A NAME ANOTHER ACCOUNT ALREADY HAS IS REFUSED, and nothing is added. Ruled by the
#     stakeholder on 2026-09-27. Unlike a category, which hands back the one I already have: an
#     account cannot do that without dropping the starting balance I just typed, or overwriting the
#     balance of the account I have. Refusing, as renaming does, loses nothing.
#   - An added account takes its place in the ORDER: after every account added before it, and after
#     the pool account (show-accounts.feature).
#
# ONE THING HERE IS THIS FILE'S CHOICE, NOT A RULING: when both the name and the starting balance
# are wrong, I am TOLD ABOUT THE NAME, as a category's name comes before an amount everywhere else.
#
# Reading the steps:
#   - "I add an account "X" with a starting balance of N euro" is the act. "I try to add" is the
#     same act where the scenario expects a refusal. A starting balance written in quotation marks,
#     with no "euro", is the text as typed: type-an-amount.feature's grammar, borrowed as
#     change-an-entry.feature borrows it. So "with a starting balance of """ is the field left empty.
#   - "I should be told that the account "X" was added" is the message afterwards. "the account
#     should not be added" means no account was added, and the Thens after it show the accounts as
#     they were.
#   - "I should be told that an account needs a name" and "... that another account already has
#     that name" are the two refusals of a name. The amount refusals are type-an-amount.feature's.
#   - The shared account steps are explained in show-accounts.feature, the rest are reused
#     unchanged from the file that introduced them.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names and amounts are synthetic
# test data.

@accounts
Feature: Add an account
  As someone keeping track of where my money actually is
  I want to add each account I have, with the balance my bank shows for it today
  So that MoneyBud knows where my money sits and what I have in total, starting from a figure I have checked

  # ----------------------------------------------------------------------------------
  # Adding an account
  # ----------------------------------------------------------------------------------

  Scenario: An added account is in the strip with its starting balance, net worth counts it, and it can be chosen for new entries
    When I add an account "Savings" with a starting balance of 5000 euro
    Then I should be told that the account "Savings" was added
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Savings | 5000.00 |
    And net worth should be 5000 euro
    And the accounts offered for a new expense should be exactly these, in this order:
      | account |
      | Bank    |
      | Savings |
    And the accounts offered for a new income should be exactly these, in this order:
      | account |
      | Bank    |
      | Savings |
    And the pool account should be "Bank"
    And a new expense should start out on the account "Bank"

  # Zero and below zero are real balances: an empty account, and one already overdrawn. A negative
  # starting balance is shown with the overdrawn marker from the start (show-accounts.feature).
  Scenario Outline: A starting balance is taken exactly as typed, zero and below zero included
    When I add an account "Credit card" with a starting balance of <starting balance> euro
    Then I should be told that the account "Credit card" was added
    And the accounts should be exactly these, in this order:
      | account     | balance            | overdrawn   |
      | Bank        | 0.00               | no          |
      | Credit card | <starting balance> | <overdrawn> |
    And net worth should be <starting balance> euro

    Examples:
      | starting balance | overdrawn |
      | 5000.00          | no        |
      | 0.01             | no        |
      | 0.00             | no        |
      | -0.01            | yes       |
      | -250.00          | yes       |

  Scenario: An account name loses the whitespace around it and keeps everything inside it
    When I add an account "  Credit  card  " with a starting balance of 0 euro
    Then I should be told that the account "Credit  card" was added
    And the accounts offered for a new expense should be exactly these, in this order:
      | account      |
      | Bank         |
      | Credit  card |

  # Different dimensions: a category says what money is for, an account where it is.
  Scenario: An account may have the same name as a category
    Given I have a category "Savings"
    When I add an account "Savings" with a starting balance of 5000 euro
    Then I should be told that the account "Savings" was added
    And the balance of "Savings" should be 5000 euro
    And the categories offered for a new expense should include "Savings"

  # ----------------------------------------------------------------------------------
  # A starting balance is money I already had
  #
  # It changes the balance and net worth and nothing on the purpose side: it is not listed as an
  # income, it is in no period's Unassigned, and no budget moves.
  # ----------------------------------------------------------------------------------

  Scenario: A starting balance is not income, and changes no budget figure
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have a budget of 400 euro for "Groceries" in the current budget period
    When I add an account "Savings" with a starting balance of 5000 euro
    Then Unassigned in the current budget period should still be 1600 euro
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | today | Salaris | 2000.00 |
    And net worth should be 7000 euro

  # ----------------------------------------------------------------------------------
  # A starting balance is what the bank says today
  #
  # The stakeholder's own example: add an account at 1000 euro today, then record last week's
  # groceries on it, and the balance stays 1000 euro, because the bank's 1000 euro already has that
  # money taken off. The groceries still count against their category, as any expense does. An
  # expense dated today and recorded after adding the account is not in the bank's figure yet, so it
  # moves the balance. Today is fixed as the last day of the period so that the three dates differ.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An entry dated before the day an account was added is already in its starting balance
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    When I add an account "Joint account" with a starting balance of 1000 euro
    And I record an expense of 50 euro for "Groceries" labelled "Markt" dated on <day> on the account "Joint account"
    Then the expense should be recorded
    And the balance of "Joint account" should be <balance> euro
    And net worth should be <balance> euro
    And the remaining "Groceries" budget in the current budget period should be 350 euro

    Examples:
      | day                                        | balance |
      | the first day of the current budget period | 1000.00 |
      | yesterday                                  | 1000.00 |
      | today                                      | 950.00  |

  # ----------------------------------------------------------------------------------
  # A starting balance left empty: no starting balance
  #
  # Ruled by the stakeholder on 2026-09-27. The account then works like the first start's
  # Betaalrekening (start-moneybud.feature): nobody checked a figure, so nothing dated earlier is
  # taken to be in one, and every entry on it counts, whatever its date. An income dated in the
  # future still counts only from its date (show-accounts.feature). Typing 0 is different: 0 is a
  # balance I checked, and it has in it everything dated before today.
  # ----------------------------------------------------------------------------------

  Scenario: An account added with its starting balance left empty has no starting balance
    When I add an account "Cash" with a starting balance of ""
    Then I should be told that the account "Cash" was added
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Cash    | 0.00    |
    And net worth should be 0.00 euro
    And nothing should be in the history of "Cash"

  # Markt is dated yesterday. Left empty, the account has no typed balance for Markt to be in, so it
  # counts. Typed as 0, the starting balance has Markt in it already. Verkocht is dated today and
  # recorded after the account was added, so it counts either way. The income dated next period
  # counts in neither, until its date.
  Scenario Outline: Left empty, a starting balance has nothing in it, and typed as 0 it has in it everything dated before today
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    When I add an account "Cash" with a starting balance of <typed>
    And I record an expense of 50 euro for "Groceries" labelled "Markt" dated yesterday on the account "Cash"
    And I record an income of 20 euro labelled "Verkocht" dated today on the account "Cash"
    And I record an income of 100 euro labelled "Terugbetaling" dated on the first day of the next budget period on the account "Cash"
    Then the balance of "Cash" should be <balance> euro
    And net worth should be <balance> euro
    And the remaining "Groceries" budget in the current budget period should be 350 euro

    Examples:
      | typed | balance |
      | ""    | -30.00  |
      | "   " | -30.00  |
      | "0"   | 20.00   |

  # ----------------------------------------------------------------------------------
  # What is refused. In every case nothing is added and net worth does not move.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An account name that trims to nothing is refused
    Given I have an account "Cash" with a starting balance of 40 euro
    When I try to add an account <name> with a starting balance of 100 euro
    Then the account should not be added
    And I should be told that an account needs a name
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Cash    | 40.00   |
    And net worth should still be 40 euro

    Examples:
      | name  |
      | ""    |
      | " "   |
      | "   " |

  # Ruled by the stakeholder on 2026-09-27 (see the header). The 300 euro typed is not applied to the
  # account that has the name.
  Scenario Outline: A name another account already has is refused, however I capitalise or space it
    Given I have an account "Credit card" with a starting balance of -120 euro
    When I try to add an account <typed> with a starting balance of 300 euro
    Then the account should not be added
    And I should be told that another account already has that name
    And the accounts should be exactly these, in this order:
      | account     | balance |
      | Bank        | 0.00    |
      | Credit card | -120.00 |
    And net worth should still be -120 euro

    Examples:
      | typed             |
      | "Credit card"     |
      | "credit card"     |
      | "  CREDIT CARD  " |
      | "Credit   card"   |

  Scenario: The pool account's name is taken like any other
    When I try to add an account "bank" with a starting balance of 300 euro
    Then the account should not be added
    And I should be told that another account already has that name
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |

  Scenario Outline: A starting balance cannot be finer than a cent
    When I try to add an account "Cash" with a starting balance of <amount> euro
    Then the account should not be added
    And I should be told that an amount cannot be finer than a cent
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |

    Examples:
      | amount  |
      | 0.001   |
      | 40.005  |
      | -12.345 |

  # Typed as any amount is typed (type-an-amount.feature). Left empty is not refused: it means no
  # starting balance (below).
  Scenario Outline: A starting balance that cannot be read as an amount is refused
    When I try to add an account "Cash" with a starting balance of <typed>
    Then the account should not be added
    And I should be told that <reason>
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |

    Examples:
      | typed   | reason                             |
      | "abc"   | "abc" is not an amount             |
      | "2.000" | "2.000" is ambiguous: 2000 or 2,00 |

  Scenario Outline: A starting balance typed with a comma, a point or a minus is read as usual
    When I add an account "Cash" with a starting balance of <typed>
    Then I should be told that the account "Cash" was added
    And the balance of "Cash" should be <balance> euro

    Examples:
      | typed     | balance |
      | "40,50"   | 40.50   |
      | "40.50"   | 40.50   |
      | "-40,50"  | -40.50  |
      | "€ 0,00"  | 0.00    |

  # This file's choice, not a ruling (see the header): the name is checked first.
  Scenario: An account that breaks two rules at once is refused for its name
    When I try to add an account "   " with a starting balance of 12.345 euro
    Then the account should not be added
    And I should be told that an account needs a name
