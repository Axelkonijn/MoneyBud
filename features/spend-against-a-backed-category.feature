# Spending against a backed category: which account the expense starts out on, and what the expense
# does to Accumulated (glossary: "An expense defaults to the pool account", "What the backing
# increment covers, and what waits", "Spending against a backed category from another account",
# "Backing a category that already has money"; "Backing and Accumulated", settled by the stakeholder
# on 2026-09-27). Recording itself is unchanged (record-expense.feature, record-on-an-account.feature).
# The steps the backing files share are explained in back-a-category.feature.
#
# REVISED FOR INCREMENT 15, 2026-10-04 (glossary: "An expense on a backed category is on its account", a
# follow-up ruling of the stakeholder that day, and its follow-ups 15 and 16). The scenarios marked
# "Revised for increment 15", "Reversed for increment 15", "Replaced for increment 15" or "New in increment
# 15" below were APPROVED at the scenario gate on 2026-10-04.
#   - AN EXPENSE AGAINST A CATEGORY THAT HAS AN ACCOUNT IS ALWAYS ON THAT ACCOUNT. THE FORM'S ACCOUNT LIST
#     IS LOCKED ON IT. Categories without an account keep the list, for cash or another card. He did not
#     see how a backed category's expense could be on another account at all: "Why can we make expenses
#     from an account that isn't linked to it?" With the new act it need not be: money built up is spent
#     by moving it to Unassigned first, assigning it, and paying from the pool account
#     (reallocate-an-amount.feature). Rejected: no account list at all, which leaves cash nowhere to go;
#     keeping the list changeable, which lets Accumulated and the account drift apart.
#   - THE LOCK APPLIES FROM THE PERIOD THE CATEGORY GOT ITS ACCOUNT (follow-up 16). An expense dated in an
#     earlier period, when the category had none and its money was on the pool account, goes on the pool
#     account with the list open. In his words: "you paid it with the Betaalrekening, so of course it
#     should come off the Betaalrekening". Ruled, and low priority for him.
#   - What it changes here:
#       - "An expense against a backed category put on another account still lowers Accumulated" is
#         REPLACED: it can no longer be put there.
#       - "An account chosen before the category is typed stays chosen" and "An account chosen after the
#         category is typed stays chosen when I type another category" are REWRITTEN. A pick no longer
#         survives a backed category, which locks the list on its account, but it comes back when a
#         category without an account is typed after it (derived), and a pick between categories without
#         an account sticks as before.
#       - "An expense being changed keeps its account when its category is changed to a backed one" is
#         REVERSED (derived): it goes onto that category's account.
#       - The outline of expenses dated in the period of backing put them on Bank. They are now on
#         Deposit, so the balances change; what they do to Accumulated does not.
#       - The first outline gains whether the list is locked, and a row for a category the pool account
#         backs, whose list is locked on the pool account (derived).
#       - New: the lock from the period of backing; an expense changed from a backed category to one
#         without an account; re-pointing leaving earlier expenses where they are; a repeat's next
#         occurrence on the account backing its category on its own day; the list of a category on "—",
#         open, and of an archived backed category, locked (all derived but the first).
#   - RULED AT THE SCENARIO STAGE, 2026-10-04, on the recommendation: an expense from before the backing,
#     dated in the period of backing, that is changed or removed STAYS ON THE ACCOUNT THAT PAID IT, and
#     MONEYBUD MOVES THE DIFFERENCE between the pool account and the backing account, so that what the
#     backing account holds for the category stays equal to its Accumulated. "Changing or removing an
#     expense from before the backing moves Accumulated with Remaining, and moves no money" is revised:
#     its title, its balances, and a third row, raising the expense, which moves money back. RULED AT THE
#     SCENARIO STAGE, 2026-10-04, too: OPENED TO BE CHANGED, SUCH AN EXPENSE KEEPS THE ACCOUNT IT IS ON, AND
#     THE LIST IS LOCKED TO IT; another cannot be chosen. One small scenario is new for it.
#
# REVISED 2026-10-05 for ruling 5 revised; gates waived by Axel, presented with the plan and the app. The
# pool account now shows Unclaimed too (show-unclaimed.feature). The one accounts table here with an
# UNCLAIMED column, in "Changing or removing an expense from before the backing ...", gave Bank, the pool
# account, a blank cell, meaning it shows none. It now carries 0.00 in every row: Bank holds 1700, exactly
# this period's Unassigned. Nothing else changes.
#
# The rules, from arc42 §12:
#   - AN EXPENSE AGAINST A BACKED CATEGORY IS PRE-FILLED WITH ITS BACKING ACCOUNT, and the account list
#     still lets me pick another, for that one expense. (Revised 2026-10-04: IT IS ALWAYS ON ITS BACKING
#     ACCOUNT, and the list is locked. Picking another account holds only for a category without one.)
#   - THE FORM'S ACCOUNT FOLLOWS THE CATEGORY TYPED: the backing account for a backed category, the
#     pool account for an unbacked one. UNTIL I PICK AN ACCOUNT MYSELF: that choice then sticks for
#     that entry, whatever category I type after it. An expense BEING CHANGED KEEPS ITS ACCOUNT.
#     (Follow-up.) Rejected: the account always following the category, because typing the category
#     after choosing the account, which the field order allows, would silently undo the choice.
#     (Revised 2026-10-04: for a backed category the list is locked on its account, so a pick cannot hold
#     there. A pick made for a category without an account survives a backed one typed in between
#     (derived). An expense being changed to a backed category goes onto its account (derived).)
#   - An expense against a backed category PUT ON ANOTHER ACCOUNT STILL LOWERS ACCUMULATED: the money
#     for that purpose was spent, whichever account paid. The backing account's balance and
#     Accumulated then differ by that amount, which is visible and true (follow-up). (Revised 2026-10-04:
#     no new or changed expense can be put there any more. It survives only in data kept before
#     increment 15, keep-data.feature.)
#   - IN THE PERIOD OF BACKING, ACCUMULATED MOVES WITH REMAINING (ruling of 2026-09-28, which replaced
#     the follow-up of 2026-09-27): EVERY EXPENSE DATED IN THAT PERIOD OR LATER LOWERS IT, WHENEVER IT
#     WAS ENTERED, and changing or removing an expense from before the backing moves it just as it
#     moves Remaining. THE AMOUNT MOVED AT BACKING STAYS WHAT IT WAS. An expense dated in a period
#     before the backing's does not count: a late one there is for the sweep. What is there for the
#     category, which unbacking returns, follows the same rule for the expenses its backing account
#     paid. Found by the stakeholder: a weekly expense set up in the past, after the backing, recorded
#     three weeks at once and only the last lowered Accumulated.
#
# DERIVED, NOT ASKED: the category typed is recognised as the glossary compares category names,
# ignoring case.
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
#   - Added for increment 15: "the account chosen for the new expense should be "Y", locked" means the
#     list shows Y and cannot be changed; "..., changeable" means it shows Y and I can choose another.
#     "I type "X" as the category of the new expense, and <date> as its date" types both, without
#     recording anything. "the expense labelled "X" should open on the account "Y", locked (changeable)"
#     is what the form shows when that expense is loaded to be changed, as change-a-repeat.feature's
#     "should open with the frequency ..." is.
#   - Added for increment 15: "... on the account "Y"" at the end of a step that records an expense
#     against a backed category names its backing account, the only one it can be on.
#   - "... on the account "Y"", at the end of a step that records an expense, means Y was chosen for
#     it. Without it, the expense is on the account the form shows once the category is typed.
#   - The other steps are explained in back-a-category.feature and show-accounts.feature, or reused
#     unchanged from the file that introduced them.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names, labels and amounts are
# synthetic test data.

# The user story below was revised for increment 15. It read: "I want an expense against that purpose to
# start out on that account, and still to count against the purpose if I pay it from somewhere else / So
# that I choose the account only when it is not the usual one, and what I have built up for a purpose goes
# down whenever I spend on it."

@backing
Feature: Spend against a backed category
  As someone who pays for a purpose from the account I save for it in
  I want every expense against that purpose to come off that account, without my choosing it
  So that what I have built up for a purpose goes down whenever I spend on it, and always matches what is on its account

  # ----------------------------------------------------------------------------------
  # The account follows the category typed
  # ----------------------------------------------------------------------------------

  # "savings" is Savings: a category name is compared ignoring case (derived). Revised for increment 15:
  # a backed category's account is locked, the pool account backing one included (derived), and a
  # category without an account's is not.
  Scenario Outline: A new expense's account follows the category typed: its backing account, locked, if it is backed, the pool account if not
    Given I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Groceries"
    And I have a category "Savings"
    And I have a category "Pension"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Pension" to "Bank"
    Then a new expense should start out on the account "Bank"
    When I type <typed> as the category of the new expense
    Then the account chosen for the new expense should be "<account>", <list>

    Examples:
      | typed       | account | list       |
      | "Savings"   | Deposit | locked     |
      | "savings"   | Deposit | locked     |
      | "Pension"   | Bank    | locked     |
      | "Groceries" | Bank    | changeable |

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
  # An account I choose myself sticks, for that entry, while the category has no account
  # ----------------------------------------------------------------------------------

  # Revised for increment 15. The field order puts the category before the account, but nothing stops
  # me choosing the account first. It first said that typing the category afterwards must not undo that
  # choice, even for a backed category. Now a backed category's list is locked on its account, so it shows
  # Deposit. My pick is not forgotten: typing a category without an account afterwards brings it back
  # (derived).
  Scenario: An account chosen before a backed category is typed gives way to its backing account, and comes back with a category without one
    Given I have an account "Deposit" with a starting balance of 5000 euro
    And I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I choose the account "Cash" for the new expense
    And I type "Savings" as the category of the new expense
    Then the account chosen for the new expense should be "Deposit", locked
    When I type "Groceries" as the category of the new expense
    Then the account chosen for the new expense should be "Cash", changeable

  # Revised for increment 15. It first typed Savings, chose Cash, and showed Cash surviving Groceries
  # typed after it. Choosing is no longer possible while Savings is typed. Between categories without an
  # account a choice sticks, as before.
  Scenario: An account chosen for a category without one stays chosen when I type another category without one
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    And I have a category "Hobby"
    When I type "Groceries" as the category of the new expense
    And I choose the account "Cash" for the new expense
    And I type "Hobby" as the category of the new expense
    Then the account chosen for the new expense should be "Cash", changeable

  # New in increment 15 (derived). A category set to "—" has no account, so its list is open, on the pool
  # account, even though it left money on Deposit. An archived category that still has an account is
  # locked on it; recording against it brings it back, as before.
  Scenario Outline: A category on none has an open list, and an archived category with an account a locked one
    Given I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have reallocated 1000 euro from Unclaimed on "Deposit" to "Savings"
    And <setup>
    When I type "Savings" as the category of the new expense
    Then the account chosen for the new expense should be "<account>", <list>

    Examples:
      | setup                                   | account | list       |
      | I have removed the backing of "Savings" | Bank    | changeable |
      | I have archived the category "Savings"  | Deposit | locked     |

  # ----------------------------------------------------------------------------------
  # What an expense against a backed category does
  # ----------------------------------------------------------------------------------

  # Revised for increment 15: its title ended "unless I choose another". Nothing else changed.
  Scenario: An expense against a backed category comes off its backing account, and lowers Accumulated
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

  # Replaced for increment 15. It was "An expense against a backed category put on another account still
  # lowers Accumulated": Fiets went on Bank, and Deposit and Accumulated then differed by the 120 Bank paid.
  # That can no longer be done. Choosing Bank first makes no difference: once Savings is typed, the list is
  # locked on Deposit, and Fiets comes off Deposit. Deposit's Unclaimed, the 5000 that was mine, is
  # untouched.
  Scenario: An expense against a backed category cannot be put on another account
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    When I choose the account "Bank" for the new expense
    And I record an expense of 120 euro for "Savings" labelled "Fiets"
    Then the expense should be recorded
    And the balance of "Deposit" should be 5180 euro
    And the balance of "Bank" should still be 1700 euro
    And Accumulated for "Savings" in the current budget period should be 180 euro
    And the Unclaimed of "Deposit" should still be 5000 euro
    And the expense labelled "Fiets" should open on the account "Deposit", locked

  # Reversed for increment 15 (derived from the follow-up ruling). It first said that Markt keeps Bank. A
  # change is judged as if it were recorded now, and Savings' list is locked, so Markt goes onto Deposit:
  # the 25 comes back to Bank and off Deposit. It counts against Savings, as before.
  Scenario: An expense being changed to a backed category goes onto that category's account
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
      | today | Savings  | Markt | 25.00  | Deposit |
    And the balance of "Bank" should be 1900 euro
    And the balance of "Deposit" should be 5075 euro
    And the remaining "Savings" budget in the current budget period should be 75 euro
    And Accumulated for "Savings" in the current budget period should be 75 euro
    And the expense labelled "Markt" should open on the account "Deposit", locked

  # New in increment 15 (derived). Taken off a backed category, an expense keeps the account it was on, as
  # an expense being changed always has, and the list opens for me to change it. As a Groceries expense
  # on Deposit it now lowers Deposit's Unclaimed, where as a Savings expense it lowered Savings'.
  Scenario: An expense changed from a backed category to one without an account keeps its account, and the list opens
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Groceries"
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have recorded an expense of 25 euro for "Savings" labelled "Markt" dated today on the account "Deposit"
    When I change the category of the expense labelled "Markt" to "Groceries"
    Then the change should go through
    And the balance of "Deposit" should still be 4975 euro
    And the Unclaimed of "Deposit" should be 4975 euro
    And the expense labelled "Markt" should open on the account "Deposit", changeable

  # New in increment 15 (derived). Re-pointing takes along what is there for Savings, 250, and leaves Boek,
  # paid before, on Deposit, as every entry keeps its account. The next expense goes on Broker.
  Scenario: Pointing a category's backing elsewhere leaves its earlier expenses where they were paid, and the next goes on the new account
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have recorded an expense of 50 euro for "Savings" labelled "Boek" dated today on the account "Deposit"
    When I set the backing account of "Savings" to "Broker"
    And I record an expense of 20 euro for "Savings" labelled "Pen"
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category | label | amount | account |
      | today | Savings  | Pen   | 20.00  | Broker  |
      | today | Savings  | Boek  | 50.00  | Deposit |
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1700.00 |
      | Deposit | 0.00    |
      | Broker  | 230.00  |
    And Accumulated for "Savings" in the current budget period should be 230 euro

  # New in increment 15 (derived). Until now each occurrence copied the account of the one before it. A
  # backed category's expense is always on its account, so the occurrence goes on the account backing
  # the category on the day it comes: Holiday was pointed at Broker in between.
  Scenario: A repeating expense on a backed category comes on the account backing it on the day it comes
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Holiday"
    And I have set the backing account of "Holiday" to "Deposit"
    And I have a budget of 100 euro for "Holiday" in the current budget period
    When I record an expense of 40 euro for "Holiday" labelled "Campingfonds", repeating monthly
    And I set the backing account of "Holiday" to "Broker"
    And the day becomes 25 September 2026 while MoneyBud is open
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category | label        | amount | account | repeats |
      | 25 September 2026 | Holiday  | Campingfonds | 40.00  | Broker  | monthly |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date           | category | label        | amount | account | repeats |
      | 25 August 2026 | Holiday  | Campingfonds | 40.00  | Deposit |         |

  # ----------------------------------------------------------------------------------
  # Which expenses lower Accumulated
  #
  # Every one dated in the period of backing or later, whenever it was entered. Savings was backed
  # today, with its whole 300 moved. A receipt from yesterday, entered now, counts against Accumulated
  # as it does against the budget. Bank pays in both rows, so the balances are the same.
  #
  # Revised for increment 15: Deposit now pays in both rows, not Bank. The list is locked from the period
  # the category got its account, so the receipt from yesterday, before the day of backing, is on Deposit
  # too (follow-up 16, derived). The balances change; Accumulated does not.
  #
  # Corrected at the build, 2026-10-04, with the stakeholder: Deposit's starting balance is dated the
  # period's first day, not today. Dated today it would already hold the receipt from yesterday (a typed
  # balance holds every entry dated before its day, show-accounts.feature), and Deposit would stay at
  # 300. What the scenario is about is unchanged.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An expense dated in the period of backing lowers Accumulated, dated before the backing or after it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro dated the first day of the current budget period
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    When I record an expense of 50 euro for "Savings" labelled "Bon" dated <day>
    Then the expense should be recorded
    And the remaining "Savings" budget in the current budget period should be 250 euro
    And Accumulated for "Savings" in the current budget period should be <accumulated> euro
    And the balance of "Deposit" should be 250 euro
    And the balance of "Bank" should still be 1700 euro

    Examples:
      | day       | accumulated |
      | yesterday | 250.00      |
      | today     | 250.00      |

  # New in increment 15 (follow-up 16, his example with synthetic names). Savings got Deposit today. A
  # receipt from the previous period, when Savings had no account and its money was on Bank, starts out on
  # Bank with the list open. A receipt from this period is locked on Deposit, whether it is dated before
  # the day of backing or on it.
  Scenario Outline: The list is locked from the period the category got its account, and open for an expense dated before that period
    Given my budget periods are one month long
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I type "Savings" as the category of the new expense, and <date> as its date
    Then the account chosen for the new expense should be "<account>", <list>

    Examples:
      | date                                       | account | list       |
      | the last day of the previous budget period | Bank    | changeable |
      | yesterday                                  | Deposit | locked     |
      | today                                      | Deposit | locked     |

  # A late receipt from a period before the backing's is for the sweep, not for Accumulated. It still
  # counts against that period's budget. (Since increment 15 this is also the one kind of expense against
  # a backed category that can be put on another account: follow-up 16, above.)
  Scenario: An expense dated in a period before the backing's does not lower Accumulated
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    When I record an expense of 50 euro for "Savings" labelled "Bon" dated on the last day of the previous budget period on the account "Bank"
    Then the expense should be recorded
    And Accumulated for "Savings" in the current budget period should still be 300 euro
    And the remaining "Savings" budget in the current budget period should still be 300 euro
    And the balance of "Deposit" should still be 300 euro

  # The stakeholder's own case, 2026-09-28, which changed the rule. A weekly expense set up in the
  # past, after the backing, records three weeks at once, two of them dated before the day of backing.
  # All three lower Accumulated, as they lower Remaining, and unbacking returns what is left.
  Scenario: A weekly expense set up in the past after the backing lowers Accumulated by every week in the period
    Given my budget periods are one month long
    And today is 28 September 2026
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 300 euro for "Groceries" in the current budget period
    And I have set the backing account of "Groceries" to "Deposit"
    When I record an expense of 4 euro for "Groceries" labelled "Broodje kip" dated 14 September 2026, repeating weekly
    Then the expense should be recorded
    And the remaining "Groceries" budget in the current budget period should be 288 euro
    And Accumulated for "Groceries" in the current budget period should be 288 euro
    When I remove the backing of "Groceries"
    Then I should be told that "Groceries" is no longer backed, and that 288 euro moved from "Deposit" to "Bank"

  # Voorschot was recorded before the backing, so 200 moved, not 300. Removing or lowering it
  # afterwards changes the budget's Remaining and Bank, and Accumulated with Remaining. What moved stays
  # what it was, so Deposit does not change.
  #
  # Revised for increment 15. RULED AT THE SCENARIO STAGE, 2026-10-04, on the recommendation: WHEN AN
  # EXPENSE FROM BEFORE THE BACKING, DATED IN THE PERIOD OF BACKING, IS CHANGED OR REMOVED, IT STAYS ON THE
  # ACCOUNT THAT PAID IT, AND MONEYBUD MOVES THE DIFFERENCE BETWEEN THE POOL ACCOUNT AND THE BACKING
  # ACCOUNT, so that what the backing account holds for the category stays equal to its Accumulated. It
  # first said that no money moves: Accumulated then followed Remaining while Deposit stayed at 200, and
  # since Unclaimed is shown, that would show. Now Voorschot stays on Bank, and:
  #   - removed, 100 moves from Bank to Deposit, as backing would have moved it had Voorschot not been there;
  #   - lowered to 40, 60 moves from Bank to Deposit;
  #   - raised to 150 (a new row), 50 moves back from Deposit to Bank.
  # Bank ends at 1700 in every row: it pays exactly what Savings' budget plans, 300. Deposit's Unclaimed
  # stays zero. Rejected: moving Voorschot itself onto Deposit, which the lock from the period of backing
  # would have done; and leaving it as it was.
  Scenario Outline: Changing or removing an expense from before the backing moves Accumulated with Remaining, and the money with it
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have recorded an expense of 100 euro for "Savings" labelled "Voorschot" dated today on the account "Bank"
    And I have set the backing account of "Savings" to "Deposit"
    And Accumulated for "Savings" in the current budget period is 200 euro
    When <act>
    Then Accumulated for "Savings" in the current budget period should be <remaining> euro
    And the remaining "Savings" budget in the current budget period should be <remaining> euro
    And the accounts should be exactly these, in this order:
      | account | balance     | unclaimed |
      | Bank    | 1700.00     | 0.00      |
      | Deposit | <remaining> | 0.00      |

    Examples:
      | act                                                                 | remaining |
      | I remove the expense labelled "Voorschot" and confirm               | 300.00    |
      | I change the amount of the expense labelled "Voorschot" to 40 euro  | 260.00    |
      | I change the amount of the expense labelled "Voorschot" to 150 euro | 150.00    |

  # New in increment 15, ruled at the scenario stage, 2026-10-04: Voorschot, paid from Bank before Savings
  # got Deposit, opens on Bank, and its list is locked there. It is neither moved to Deposit nor put on any
  # other account.
  Scenario: An expense from before the backing opens on the account that paid it, locked
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have recorded an expense of 100 euro for "Savings" labelled "Voorschot" dated today on the account "Bank"
    And I have set the backing account of "Savings" to "Deposit"
    Then the expense labelled "Voorschot" should open on the account "Bank", locked
