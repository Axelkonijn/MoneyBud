# Spending against a backed category: which account the expense starts out on, and what the expense
# does to Accumulated (glossary: "An expense defaults to the pool account", "What the backing
# increment covers, and what waits", "Spending against a backed category from another account",
# "Backing a category that already has money"; "Backing and Accumulated", settled by the stakeholder
# on 2026-09-27). Recording itself is unchanged (record-expense.feature, record-on-an-account.feature).
# The steps the backing files share are explained in back-a-category.feature.
#
# The rules, from arc42 §12:
#   - AN EXPENSE AGAINST A BACKED CATEGORY IS PRE-FILLED WITH ITS BACKING ACCOUNT, and the account list
#     still lets me pick another, for that one expense.
#   - THE FORM'S ACCOUNT FOLLOWS THE CATEGORY TYPED: the backing account for a backed category, the
#     pool account for an unbacked one. UNTIL I PICK AN ACCOUNT MYSELF: that choice then sticks for
#     that entry, whatever category I type after it. An expense BEING CHANGED KEEPS ITS ACCOUNT.
#     (Follow-up.) Rejected: the account always following the category, because typing the category
#     after choosing the account, which the field order allows, would silently undo the choice.
#   - An expense against a backed category PUT ON ANOTHER ACCOUNT STILL LOWERS ACCUMULATED: the money
#     for that purpose was spent, whichever account paid. The backing account's balance and
#     Accumulated then differ by that amount, which is visible and true (follow-up).
#   - THE EXPENSES THAT LOWER ACCUMULATED are those DATED AFTER THE BACKING DAY, OR ON IT AND RECORDED
#     AFTER THE BACKING: the test a balance correction applies. A forgotten expense dated before the
#     backing, recorded later, does not touch Accumulated, and THE AMOUNT MOVED AT BACKING STAYS WHAT
#     IT WAS. The same holds for an expense from before the backing that is changed or removed
#     (follow-up). THE ACCEPTED COST: the period's Remaining and what moved then disagree, and nothing
#     on screen says so.
#
# DERIVED, NOT ASKED: the category typed is recognised as the glossary compares category names,
# ignoring case. And a changed expense keeps the moment it was first recorded
# (correct-a-balance.feature), so whether it counts against Accumulated never depends on when it was
# changed.
#
# DERIVED, NOT ASKED, at the scenario gate: while what is typed is not yet, or not at all, the name of
# a category, the form shows the POOL ACCOUNT. There is no category to have an opinion, and the pool
# account is the default wherever none has one.
#
# Reading the steps:
#   - "I type "X" as the category of the new expense" types X into the category box of the expense
#     form, on its own, without recording anything. "I choose the account "Y" for the new expense"
#     picks Y in the form's account list, without recording anything.
#   - "the account chosen for the new expense should be "Y"" is what the form's account list shows
#     now. "a new expense should start out on the account "Y"" is show-accounts.feature's: what the
#     list shows before I touch the form.
#   - "... on the account "Y"", at the end of a step that records an expense, means Y was chosen for
#     it. Without it, the expense is on the account the form shows once the category is typed.
#   - The other steps are explained in back-a-category.feature and show-accounts.feature, or reused
#     unchanged from the file that introduced them.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names, labels and amounts are
# synthetic test data.

@backing
Feature: Spend against a backed category
  As someone who pays for a purpose from the account I save for it in
  I want an expense against that purpose to start out on that account, and still to count against the purpose if I pay it from somewhere else
  So that I choose the account only when it is not the usual one, and what I have built up for a purpose goes down whenever I spend on it

  # ----------------------------------------------------------------------------------
  # The account follows the category typed
  # ----------------------------------------------------------------------------------

  # "savings" is Savings: a category name is compared ignoring case (derived).
  Scenario Outline: A new expense's account follows the category typed: its backing account if it is backed, the pool account if not
    Given I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Groceries"
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    Then a new expense should start out on the account "Bank"
    When I type <typed> as the category of the new expense
    Then the account chosen for the new expense should be "<account>"

    Examples:
      | typed       | account |
      | "Savings"   | Deposit |
      | "savings"   | Deposit |
      | "Groceries" | Bank    |

  # Derived (see the header): a name half typed, or one no category has, is no category, so the
  # account is the pool account, even straight after a backed category was typed.
  Scenario Outline: While what is typed is not a category, a new expense's account is the pool account
    Given I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I type "Savings" as the category of the new expense
    Then the account chosen for the new expense should be "Deposit"
    When I type <typed> as the category of the new expense
    Then the account chosen for the new expense should be "Bank"

    Examples:
      | typed       |
      | "Sav"       |
      | "Savingsx"  |
      | "Holiday"   |

  Scenario: The account follows the category again when I type another category
    Given I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Groceries"
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I type "Savings" as the category of the new expense
    Then the account chosen for the new expense should be "Deposit"
    When I type "Groceries" as the category of the new expense
    Then the account chosen for the new expense should be "Bank"

  # ----------------------------------------------------------------------------------
  # An account I choose myself sticks, for that entry
  # ----------------------------------------------------------------------------------

  # The field order puts the category before the account, but nothing stops me choosing the account
  # first. Typing the category afterwards must not undo that choice.
  Scenario: An account chosen before the category is typed stays chosen
    Given I have an account "Deposit" with a starting balance of 5000 euro
    And I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I choose the account "Cash" for the new expense
    And I type "Savings" as the category of the new expense
    Then the account chosen for the new expense should be "Cash"

  Scenario: An account chosen after the category is typed stays chosen when I type another category
    Given I have an account "Deposit" with a starting balance of 5000 euro
    And I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I type "Savings" as the category of the new expense
    And I choose the account "Cash" for the new expense
    And I type "Groceries" as the category of the new expense
    Then the account chosen for the new expense should be "Cash"

  # ----------------------------------------------------------------------------------
  # What an expense against a backed category does
  # ----------------------------------------------------------------------------------

  Scenario: An expense against a backed category comes off its backing account unless I choose another, and lowers Accumulated
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    When I record an expense of 120 euro for "Savings" labelled "Fiets"
    Then the expense should be recorded
    And the balance of "Deposit" should be 5180 euro
    And the balance of "Bank" should still be 1700 euro
    And the remaining "Savings" budget in the current budget period should be 180 euro
    And Accumulated for "Savings" in the current budget period should be 180 euro
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category | label | amount | account |
      | today | Savings  | Fiets | 120.00 | Deposit |

  # The follow-up's case: Savings money was spent, whichever account paid. Deposit still holds the
  # 300 moved in, and Accumulated says 180: the two differ by the 120 Bank paid. The choice was for
  # that one expense, so the next one starts out on Deposit again.
  Scenario: An expense against a backed category put on another account still lowers Accumulated
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    When I record an expense of 120 euro for "Savings" labelled "Fiets" on the account "Bank"
    Then the expense should be recorded
    And the balance of "Bank" should be 1580 euro
    And the balance of "Deposit" should still be 5300 euro
    And the remaining "Savings" budget in the current budget period should be 180 euro
    And Accumulated for "Savings" in the current budget period should be 180 euro
    When I type "Savings" as the category of the new expense
    Then the account chosen for the new expense should be "Deposit"

  # Markt was on Bank as a Groceries expense. Changed to Savings, it keeps its account, and it now
  # counts against Savings: it was recorded after the backing.
  Scenario: An expense being changed keeps its account when its category is changed to a backed one
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Groceries"
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 100 euro for "Savings" in the current budget period
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated today on the account "Bank"
    When I change the category of the expense labelled "Markt" to "Savings"
    Then the change should go through
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category | label | amount | account |
      | today | Savings  | Markt | 25.00  |         |
    And the balance of "Bank" should still be 1875 euro
    And the balance of "Deposit" should still be 5100 euro
    And the remaining "Savings" budget in the current budget period should be 75 euro
    And Accumulated for "Savings" in the current budget period should be 75 euro

  # ----------------------------------------------------------------------------------
  # Which expenses lower Accumulated
  #
  # Those dated after the backing day, or on it and recorded after the backing. Savings was backed
  # today, with its whole 300 moved. A receipt from yesterday, entered now, still counts against the
  # budget, but not against Accumulated: it was paid before the money moved. Bank pays in both rows,
  # so the balances are the same.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An expense dated before the day of backing does not lower Accumulated, and one after the backing does
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    When I record an expense of 50 euro for "Savings" labelled "Bon" dated <day> on the account "Bank"
    Then the expense should be recorded
    And the remaining "Savings" budget in the current budget period should be 250 euro
    And Accumulated for "Savings" in the current budget period should be <accumulated> euro
    And the balance of "Deposit" should still be 300 euro
    And the balance of "Bank" should be 1650 euro

    Examples:
      | day       | accumulated |
      | yesterday | 300.00      |
      | today     | 250.00      |

  # Voorschot was recorded before the backing, so 200 moved, not 300. Removing or lowering it
  # afterwards changes the budget's Remaining and Bank, and leaves Accumulated and what moved as they
  # were. That is the accepted cost: Remaining and what moved now disagree, and nothing says so.
  Scenario Outline: Changing or removing an expense from before the backing leaves Accumulated, and the money moved, as they were
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have recorded an expense of 100 euro for "Savings" labelled "Voorschot" dated today on the account "Bank"
    And I have set the backing account of "Savings" to "Deposit"
    And Accumulated for "Savings" in the current budget period is 200 euro
    When <act>
    Then Accumulated for "Savings" in the current budget period should still be 200 euro
    And the balance of "Deposit" should still be 200 euro
    And the balance of "Bank" should be <bank> euro
    And the remaining "Savings" budget in the current budget period should be <remaining> euro

    Examples:
      | act                                                                | bank    | remaining |
      | I remove the expense labelled "Voorschot" and confirm              | 1800.00 | 300.00    |
      | I change the amount of the expense labelled "Voorschot" to 40 euro | 1760.00 | 260.00    |
