# Managing accounts: renaming one, deleting one added by mistake, and choosing the pool account
# (glossary: "Managing accounts", "The pool account can be any account"; "Accounts and net worth",
# settled by the stakeholder on 2026-09-27). Adding an account is in add-an-account.feature. The
# steps the accounts files share are explained in show-accounts.feature.
#
# The rules, from arc42 §12:
#   - RENAMING follows the category rules (derived, and see rename-a-category.feature): the new name
#     is trimmed, stored otherwise as typed, and refused if it trims to nothing. A name ANOTHER
#     ACCOUNT has is refused, compared with the ends trimmed, a run of inner spaces counted as one
#     and case ignored. The account's OWN name in a new spelling is allowed. Renaming to exactly its
#     own name is quiet. It is the SAME ACCOUNT under a new name: its balance, its history and the
#     entries on it stay with it. An account MAY take a CATEGORY's name.
#   - An account can be DELETED ONLY WHILE UNUSED, which is for an account added by mistake. Unused
#     means NO INCOME, EXPENSE OR TRANSFER ON IT (derived). Its own starting balance and balance
#     corrections do not count, and they go with it. "On it" is read as it is NOW, as for a
#     category (delete-a-category.feature): an account whose only entry was removed, or moved to
#     another account, is unused. That reading is this file's, by analogy, and not a ruling.
#   - Deleting is NEVER CONFIRMED and is ANNOUNCED afterwards, EVEN WITH A STARTING BALANCE OTHER
#     THAN ZERO. Ruled by the stakeholder: the only thing lost is a number just typed. Net worth
#     changes accordingly.
#   - An account with history cannot be deleted. ARCHIVING an account is DEFERRED until missed, so
#     there is nothing else to do with it in this increment.
#   - ANY ACCOUNT CAN BE MADE THE POOL ACCOUNT, on screen with "Maak hoofdrekening", and THERE IS
#     ALWAYS EXACTLY ONE. Making another account the pool changes the account a NEW income or expense
#     starts out on. Existing entries keep their account, and no balance moves.
#   - THE POOL ACCOUNT CANNOT BE DELETED while it is the pool, so there is always at least one
#     account (derived). To delete it, make another account the pool first.
#
# Reading the steps:
#   - "I rename the account "X" to "Y"" names the account by its current name, and "Y" is the new
#     name exactly as typed. "I try to rename" is the same act where the scenario expects a refusal.
#     "the rename should go through (be refused)" is rename-a-category.feature's.
#   - "I should be told that the account "X" was renamed to "Y"" is the announcement, "X" as it was
#     and "Y" as now stored. "I should be told that an account needs a name" and "... that another
#     account already has that name" are add-an-account.feature's refusals, reached by renaming.
#   - "I should (not) be able to delete the account "X"" means the delete act is (not) offered for X.
#     "I delete the account "X"" is the act, and "I should be told that the account "X" was deleted"
#     the message afterwards.
#   - "I make "X" the pool account" is the act "Maak hoofdrekening". "I should be told that "X" is
#     now the pool account" is the message afterwards. The glossary does not rule that it is
#     announced. It follows "every act tells its outcome", and is this file's reading.
#   - Every other step is reused unchanged from the file that introduced it, the account steps from
#     show-accounts.feature.
#
# THE ORDER of the accounts is the pool account first, then the rest in the order added (ruled,
# show-accounts.feature, where a pool change reordering them is shown). That a RENAMED account keeps
# its place, and that an account added again after being deleted goes LAST, are this file's
# reading, from "the same account under a new name" and "nothing is kept of a deleted account", as
# for a category. Neither was put to the stakeholder.
#
# On screen the controls are Hernoemen and Verwijderen, reused from the category rows, and Maak
# hoofdrekening, confirmed by the stakeholder on 2026-09-27. They are held by the glossary's display
# terms and `Tekst`, not by these steps. NOT specified here, and left to the plan: where the
# controls are.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names and amounts are synthetic
# test data.

@accounts
Feature: Manage my accounts
  As someone whose accounts change now and then
  I want to rename an account, delete one I added by mistake, and choose which account is my pool account
  So that the accounts in MoneyBud match the ones I really have, and new entries start on the account my income lands in

  # ----------------------------------------------------------------------------------
  # Renaming an account
  # ----------------------------------------------------------------------------------

  Scenario: Renaming an account gives it the new name, and its balance, its history and its entries stay with it
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated today on the account "Cash"
    When I rename the account "Cash" to "Wallet"
    Then the rename should go through
    And I should be told that the account "Cash" was renamed to "Wallet"
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Wallet  | 15.00   |
    And the accounts offered for a new expense should be exactly these, in this order:
      | account |
      | Bank    |
      | Wallet  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount | account |
      | today | Groceries | Markt | 25.00  | Wallet  |
    And the history of "Wallet" should be exactly these, newest first:
      | date  | entry            | label | amount | balance |
      | today | expense          | Markt | 25.00  |         |
      | today | starting balance |       |        | 40.00   |

  Scenario: A new account name loses the whitespace around it and keeps everything inside it
    Given I have an account "Cash" with a starting balance of 40 euro
    When I rename the account "Cash" to "  Credit  card  "
    Then the rename should go through
    And I should be told that the account "Cash" was renamed to "Credit  card"
    And the accounts offered for a new expense should be exactly these, in this order:
      | account      |
      | Bank         |
      | Credit  card |

  Scenario Outline: A new account name that trims to nothing is refused
    Given I have an account "Cash" with a starting balance of 40 euro
    When I try to rename the account "Cash" to <name>
    Then the rename should be refused
    And I should be told that an account needs a name
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Cash    | 40.00   |

    Examples:
      | name  |
      | ""    |
      | " "   |
      | "   " |

  Scenario Outline: A name another account already has is refused, however I capitalise or space it
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have an account "Credit card" with a starting balance of -120 euro
    When I try to rename the account "Cash" to <typed>
    Then the rename should be refused
    And I should be told that another account already has that name
    And the accounts should be exactly these, in this order:
      | account     | balance |
      | Bank        | 0.00    |
      | Cash        | 40.00   |
      | Credit card | -120.00 |

    Examples:
      | typed             |
      | "Credit card"     |
      | "credit card"     |
      | "  CREDIT CARD  " |
      | "Credit   card"   |
      | "BANK"            |

  Scenario Outline: An account's own name can be respelled
    Given I have an account <name> with a starting balance of 40 euro
    When I rename the account <name> to <new spelling>
    Then the rename should go through
    And I should be told that the account <name> was renamed to <new spelling>
    And the accounts offered for a new expense should be exactly these, in this order:
      | account        |
      | Bank           |
      | <new spelling> |

    Examples:
      | name           | new spelling  |
      | "cash"         | "Cash"        |
      | "Credit  card" | "Credit card" |
      | "Wallet"       | "WALLET"      |

  Scenario: Renaming an account to exactly its own name goes through quietly
    Given I have an account "Cash" with a starting balance of 40 euro
    When I rename the account "Cash" to "Cash"
    Then the rename should go through
    And I should not have been told anything
    And the balance of "Cash" should still be 40 euro

  # Different dimensions, as when adding (add-an-account.feature).
  Scenario: An account may be renamed to a category's name
    Given I have a category "Savings"
    And I have an account "Spaarpot" with a starting balance of 5000 euro
    When I rename the account "Spaarpot" to "Savings"
    Then the rename should go through
    And I should be told that the account "Spaarpot" was renamed to "Savings"
    And the balance of "Savings" should be 5000 euro
    And the categories offered for a new expense should include "Savings"

  Scenario: The pool account keeps being the pool account under its new name
    When I rename the account "Bank" to "ING"
    Then the rename should go through
    And the pool account should be "ING"
    And a new expense should start out on the account "ING"
    And a new income should start out on the account "ING"

  # ----------------------------------------------------------------------------------
  # Deleting an account I added by mistake
  #
  # Only while nothing is on it. Never asked about, and said afterwards, even when it was added with
  # a starting balance: the stakeholder's ruling. Its starting balance goes with it, so net worth
  # changes by that much. "Savngs" is the typo the act exists for.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An account with nothing on it can be deleted, without being asked, whatever its starting balance
    Given I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Savngs" with a starting balance of <starting balance> euro
    And net worth is <net worth before> euro
    Then I should be able to delete the account "Savngs"
    When I delete the account "Savngs"
    Then I should be told that the account "Savngs" was deleted
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1000.00 |
    And net worth should be 1000 euro
    And the accounts offered for a new expense should be exactly these, in this order:
      | account |
      | Bank    |

    Examples:
      | starting balance | net worth before |
      | 0.00             | 1000.00          |
      | 5000.00          | 6000.00          |
      | -25.00           | 975.00           |

  # Balance corrections are not entries on the account, so they do not make it used. They go with
  # it.
  Scenario: An account whose only history is balance corrections can still be deleted, and they go with it
    Given I have an account "Savngs" with a starting balance of 5000 euro
    And I have corrected the balance of "Savngs" to 5100 euro
    Then I should be able to delete the account "Savngs"
    When I delete the account "Savngs"
    Then I should be told that the account "Savngs" was deleted
    And net worth should be 0.00 euro

  # One cent is the smallest use there is. An entry counts as on the account whatever its date: a
  # salary still to come, and an expense dated before the starting balance, which the balance
  # already has in it. Both sides of a transfer count.
  Scenario Outline: An account with an income, an expense or a transfer on it cannot be deleted
    Given my budget periods are one month long
    And I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    And <history>
    Then I should not be able to delete the account "Cash"

    Examples:
      | history                                                                                                                                        |
      | I have recorded an expense of 0.01 euro for "Groceries" labelled "Snoep" dated today on the account "Cash"                                      |
      | I have recorded an income of 0.01 euro labelled "Gevonden" dated today on the account "Cash"                                                    |
      | I have recorded a transfer of 0.01 euro from "Cash" to "Bank" dated today                                                                        |
      | I have recorded a transfer of 0.01 euro from "Bank" to "Cash" dated today                                                                        |
      | I have recorded an income of 1800 euro labelled "Salaris volgende maand" dated on the first day of the next budget period on the account "Cash"  |
      | I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated on the last day of the previous budget period on the account "Cash" |

  # "Unused" is read as it is now, as it is for a category (delete-a-category.feature): MoneyBud
  # keeps no record of where an entry used to be. This file's reading, by analogy, not a ruling.
  # Two When/Then pairs: first the act that takes the use away, then the delete it makes possible.
  Scenario Outline: An account whose only entry was removed or moved elsewhere can be deleted
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have an account "Savings" with a starting balance of 5000 euro
    And I have a category "Groceries"
    And <history>
    When <act>
    Then I should be able to delete the account "Cash"
    When I delete the account "Cash"
    Then I should be told that the account "Cash" was deleted

    Examples:
      | history                                                                                                    | act                                                                                                                         |
      | I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated today on the account "Cash" | I remove the expense labelled "Markt" and confirm                                                                           |
      | I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated today on the account "Cash" | I change the account of the expense labelled "Markt" to "Bank"                                                              |
      | I have recorded a transfer of 50 euro from "Bank" to "Cash" dated today                                   | I remove the transfer of 50 euro from "Bank" to "Cash" and confirm                                                          |
      | I have recorded a transfer of 50 euro from "Bank" to "Cash" dated today                                   | I change the transfer of 50 euro from "Bank" to "Cash" into a transfer of 50 euro from "Bank" to "Savings" dated on today |

  # Nothing is kept of a deleted account, so its name is free and adding it makes a new one.
  Scenario: Adding a deleted account's name adds a new account
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have deleted the account "Cash"
    When I add an account "Cash" with a starting balance of 10 euro
    Then I should be told that the account "Cash" was added
    And the history of "Cash" should be exactly these, newest first:
      | date  | entry            | balance |
      | today | starting balance | 10.00   |

  # This file's reading, not a ruling (see the header). Cash was added before Gifts and Savings, and
  # as Wallet it keeps that place. Gifts, deleted and added again, is a new account, so it comes
  # last.
  Scenario: A renamed account keeps its place in the order, and an account added again after being deleted comes last
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have an account "Gifts" with a starting balance of 0 euro
    And I have an account "Savings" with a starting balance of 5000 euro
    When I rename the account "Cash" to "Wallet"
    And I delete the account "Gifts"
    And I add an account "Gifts" with a starting balance of 0 euro
    Then the accounts should be exactly these, in this order:
      | account |
      | Bank    |
      | Wallet  |
      | Savings |
      | Gifts   |

  # ----------------------------------------------------------------------------------
  # The pool account
  # ----------------------------------------------------------------------------------

  # Bank has nothing on it, and still cannot go while it is the pool: there is always exactly one
  # pool account, so there is always at least one account.
  Scenario: The pool account cannot be deleted, even with nothing on it
    Given I have an account "Cash" with a starting balance of 40 euro
    Then I should not be able to delete the account "Bank"
    And I should be able to delete the account "Cash"

  # The salary lands in another account than the one MoneyBud started with. Making that one the pool
  # changes where a NEW entry starts out. Markt stays on Bank, and no balance moves.
  Scenario: Making another account the pool account changes where new entries start out, and nothing else
    Given I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    And I have an account "ING" with a starting balance of 40 euro
    And I have a category "Groceries"
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated today
    When I make "ING" the pool account
    Then I should be told that "ING" is now the pool account
    And I should not be warned or asked to confirm
    And the pool account should be "ING"
    And a new expense should start out on the account "ING"
    And a new income should start out on the account "ING"
    And the balance of "Bank" should still be 1807.45 euro
    And the balance of "ING" should still be 40 euro
    And net worth should still be 1847.45 euro
    When I record an expense of 3.50 euro for "Groceries" labelled "Kiosk"
    Then the balance of "ING" should be 36.50 euro
    And the balance of "Bank" should still be 1807.45 euro

  # Once it is no longer the pool, the account MoneyBud started with is an account like any other,
  # and can go if nothing is on it.
  Scenario: Once another account is the pool, the old pool account can be deleted if nothing is on it
    Given I have an account "ING" with a starting balance of 40 euro
    When I make "ING" the pool account
    Then I should be able to delete the account "Bank"
    When I delete the account "Bank"
    Then I should be told that the account "Bank" was deleted
    And the accounts should be exactly these, in this order:
      | account | balance |
      | ING     | 40.00   |
    And the pool account should be "ING"
