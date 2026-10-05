# Reallocating an amount: giving money already on an account a purpose, moving what is built up for one
# purpose to another, and taking it out to spend it (glossary: Reallocate; "Vrij, and moving Opgebouwd",
# "One act moves an amount of purpose", "A fall in value makes Vrij negative", settled by the stakeholder on
# 2026-10-04). What Unclaimed is, on screen "Vrij", is in show-unclaimed.feature. What "—" leaves behind is
# in back-a-category.feature.
#
# WRITTEN FOR INCREMENT 15, 2026-10-04. APPROVED at the scenario gate on 2026-10-04.
# REVISED 2026-10-05 for ruling 5 revised; gates waived by Axel, presented with the plan and the app.
#
# WHAT THE REVISION OF 2026-10-05 CHANGES, AND WHY. The pool account now shows Unclaimed too
# (show-unclaimed.feature, ruling 5 revised), because Unassigned only ever holds income, so money on the
# pool account from a starting balance or a balance correction could never get a purpose. THE POOL
# ACCOUNT'S UNCLAIMED IS AN END LIKE ANY ACCOUNT'S, both ways. And, ruled 2026-10-05, MONEY CAN MOVE FROM
# UNASSIGNED TO UNCLAIMED ON THE POOL ACCOUNT, the account that holds Unassigned. His two uses: giving money
# already on the pool account a purpose without transferring it away and back (Unclaimed on the pool account
# to Unassigned), and covering a lower balance correction there (Unassigned to Unclaimed on the pool account).
#   - Revised: "The form offers every end there is ..." (From gains Unclaimed on Bank and Unassigned, To
#     gains Unclaimed on Bank). Every accounts table here gives Bank, the pool account, a figure: 0.00 in
#     each, since its money is this period's Unassigned. "A move to Unassigned is undone by assigning" gains a
#     note.
#   - New: giving the pool account's own money a purpose; Unclaimed on the pool account to a category another
#     account backs; covering a lower balance correction on the pool account, both ways of writing it; what
#     the form offers at a first start; five rows of the refusal outline.
#
# Where it comes from, in his words, translated: he cannot change Opgebouwd, nor move it between
# categories, and "I already have savings and shares, so Opgebouwd doesn't really match what is in there".
# And: once months of Opgebouwd have built up, can it be used? Until now there was no clean way.
#
# The rules, from arc42 §12:
#   - ONE ACT, on screen "Verplaatsen", MOVES AN AMOUNT OF PURPOSE, as assigning moves an amount (ruling 2).
#     ONE FORM, with "Van", "Naar" and "Bedrag" (follow-up). Rejected: typing a new Opgebouwd, which sets a
#     figure where assigning moves an amount.
#   - ITS ENDS (ruling 2 and follow-up 8):
#       - an account's Unclaimed and a category that account backs, either way;
#       - one backed category's Accumulated and another's;
#       - an account's Unclaimed and a category another account backs;
#       - a backed category's Accumulated, or an account's Unclaimed, to the current period's Unassigned.
#         THIS IS HOW MONEY BUILT UP IS USED: moved to Unassigned, then assigned to what it is spent on,
#         which then spends within its budget. Rejected: spending straight from a category without the
#         over-budget marker, because a month's "over budget" would then depend on earlier months.
#       - (2026-10-05, ruling 5 revised) THE POOL ACCOUNT'S UNCLAIMED IS ONE OF "an account's Unclaimed" in
#         every line above, and the current period's Unassigned can go TO IT (ruled 2026-10-05).
#   - ONE RULE FOR MONEY: IT MOVES BETWEEN ACCOUNTS ONLY WHEN THE TWO ENDS ARE ON DIFFERENT ACCOUNTS
#     (follow-up 8). Unassigned is on the pool account. He makes the same transfer at the bank. So between
#     the pool account's Unclaimed and Unassigned no money moves.
#   - NO BUDGET OR REMAINING CHANGES, IN ANY PERIOD. Moving to Unassigned raises that period's Unassigned,
#     and moving out of it lowers it.
#   - A NEGATIVE AMOUNT MOVES BACK (ruling 2). BUT UNASSIGNED GIVES ONLY TO UNCLAIMED ON THE POOL ACCOUNT
#     (ruled 2026-10-05). Undoing a move to Unassigned from a category is ASSIGNING, which raises the
#     category's budget and moves the money (follow-up 7). Undoing one from the pool account's Unclaimed is
#     the move the other way. Money out of Unassigned to a category, or to Unclaimed on another account, is
#     refused: "moving money out of Unassigned is assigning". A minus sign the other way is the same move and
#     is judged the same: -30 from Unclaimed on the pool account to Unassigned goes through, -30 from
#     Unclaimed on another account to Unassigned is refused.
#     (First ruled, 2026-10-04: UNASSIGNED IS ONLY EVER A DESTINATION.)
#   - MOVING TO OR FROM UNASSIGNED IS POSSIBLE IN THE CURRENT PERIOD ONLY, like assigning; another period
#     is refused (follow-up 6), with the same message either way. Rejected: later periods too.
#   - (Derived h, 2026-10-05.) A MOVE OUT OF UNASSIGNED LOWERS IT, and may take it below zero, Over-assigned,
#     as assigning may. Never blocked.
#   - MOVING MORE THAN THERE IS GOES THROUGH: 500 out of an Accumulated of 300 leaves -200, "Rood"; the same
#     for Unclaimed. Never blocked (follow-up 11). Rejected: clipping it.
#   - EVERY MOVE LEAVES A READ-ONLY ROW in the history of each account it touches, A MOVE WITHIN ONE ACCOUNT
#     INCLUDED, though no balance changes (follow-up 9): Unclaimed is shown in the strip, so what changes it
#     is explained. That is an exception to "a movement from an account to itself leaves no row".
#   - A CATEGORY SET TO "—" CAN BE MOVED OUT OF, NOT INTO (follow-up 5). Into it, only once an account is
#     set again.
#
# DERIVED BY THE DOCUMENTATION AND NOT PUT TO THE STAKEHOLDER, marked where a scenario rests on one, so
# that they are approved or contradicted at this gate:
#   - A MOVE IS DATED TODAY, and counts in Accumulated from the period it falls in, up to the period on
#     screen. A move to Unassigned belongs to the period its date falls in, like an income.
#   - IT IS ANNOUNCED AFTERWARDS, NEVER CONFIRMED, naming what moved, as backing is. The wording is copy.
#   - THE CATEGORY ENDS ARE BACKED CATEGORIES (and one on "—" as a source). A category with no account is
#     not offered: its money is on the pool account, where Unassigned and assigning already reach it.
#     (Revised 2026-10-05. It went on: "The pool account has no Unclaimed, so it is no end." It has one now,
#     and is an end like any account.)
#   - (Derived i, 2026-10-05.) SO "Verplaatsen" IS ALWAYS OFFERED: there is always the pool account's
#     Unclaimed and Unassigned. At a first start those two are all the form offers, on both sides.
#   - AN ARCHIVED BACKED CATEGORY IS OFFERED AS A SOURCE, NOT AS A DESTINATION: taking its money out is
#     tidying up, as a negative assignment to it is. Moving out of it does not bring it back.
#   - THE AMOUNT FOLLOWS THE CENT RULES, and MOVING ZERO IS ACCEPTED AND CHANGES NOTHING, as assigning zero.
#   - A NEGATIVE AMOUNT WITH UNASSIGNED AS "Naar" IS REFUSED: the ruling says assigning does that.
#     (Narrowed 2026-10-05: unless "Van" is the pool account's Unclaimed, which Unassigned may give to.)
#   - "Verplaatsen" OPENED FROM A CATEGORY'S ROW STARTS WITH THAT CATEGORY AS "Van".
#   - A NEGATIVE ASSIGNMENT TO A BACKED CATEGORY STILL MOVES BACK AT MOST WHAT IS THERE FOR IT, which now
#     includes money given from Unclaimed (assign-to-a-backed-category.feature).
#   - Moving to Unassigned changes what Unassigned is: a period's income, minus what was assigned in it,
#     PLUS what was moved into it (and, since 2026-10-05, MINUS what was moved out of it to the pool
#     account's Unclaimed). Left unassigned, that money is swept at the period's end with the rest.
#
# READINGS MADE AT THIS SCENARIO STAGE, which the glossary leaves to it or does not say, each marked
# where a scenario rests on it:
#   - WHICH REFUSAL COMES FIRST (the glossary leaves it to this stage). One reason is reported, the first
#     of: the two ends are the same; the amount is finer than a cent; the amount would move money INTO an
#     end that only gives (out of Unassigned to anything but the pool account's Unclaimed, or into an
#     archived category or one on "—"); Unassigned in a period other than the current one. (Revised
#     2026-10-05: "out of Unassigned" was any move out of it. Unassigned of another period moved to the pool
#     account's Unclaimed passes the third and is refused by the fourth, with its message, now worded
#     "Unassigned can be used only in the current budget period", since it is said for a move out of it
#     too.) Ends first, as a transfer reports its accounts first; then the
#     cent rule; then the period, as assigning reports a past period last. A zero amount with such an end
#     is still refused, as assigning zero to a wrong target is.
#   - THE SAME END ON BOTH SIDES IS REFUSED, as a transfer between one account and itself is.
#   - A NEGATIVE AMOUNT INTO AN END THAT ONLY GIVES IS REFUSED: -100 "from" an archived category, or from
#     one on "—", would move 100 into it, which the rulings rule out.
#   - "Unassigned in the ... budget period" names the period whose Unassigned is chosen. That is the period
#     on screen, as assigning acts on the period shown. With another period on screen, it is refused.
#   - A negative amount's row reads as a move the other way, without a sign, as a negative assignment
#     adds a row going the other way. A zero move leaves no row, as assigning zero does.
#
# RULED BY THE STAKEHOLDER AT THE BUILD, 2026-10-04, on the recommendation: Unclaimed on one account to
# Unclaimed on another is REFUSED, second in the order above, after the same end on both sides. It gives
# nothing a purpose; moving money between accounts is a transfer. No scenario was added; a unit test holds
# it (VrijTests).
#
# Reading the steps:
#   - The English term "Reallocate" is the documentation's proposal for "Verplaatsen", open at this gate,
#     like "Unclaimed" for "Vrij".
#   - "I reallocate N euro from <end> to <end>" fills in Van, Naar and Bedrag and presses the button.
#     An END is written as: Unclaimed on "X", the Unclaimed of account X, the pool account included; "Y",
#     category Y's Accumulated; or Unassigned in the ... budget period, that period's Unassigned, on either
#     side since 2026-10-05 (it was only ever a "to"). "I try to
#     reallocate" is the same, for one that is refused. "I have reallocated ..." is the same act done
#     earlier today, as a Given, at that point in the order of the Givens.
#   - "the reallocation should go through" and "the reallocation should be refused". A refused one changes
#     nothing at all.
#   - "I should be told that N euro was reallocated from <end> to <end>" checks that the move was
#     announced. "..., and of no money moved" is used where no money moved between accounts: the notice
#     then names no money moving. "..., and that N euro moved from "A" to "B"" is used where it did. What
#     is fixed is what I am told, never the wording.
#   - "the choices offered as From (To) for a reallocation should be exactly these, in any order" is the
#     whole Van (Naar) list. A CHOICE is written "Unclaimed on X", a category's name, or "Unassigned". The
#     order of the lists is not specified here, and is left to the plan. "... should (not) include "Y""
#     checks one choice.
#   - "I start a reallocation from the row of "X"" opens the form from category X's row (its ⋯ menu on the
#     phone). "the reallocation should start with "X" as From" is what the form's Van shows then.
#   - In an account's history (show-accounts.feature's step), ENTRY "reallocation" is a row for a move,
#     in the history of each account it touches. For that row, FROM and TO are its two ENDS, as the
#     choices write them ("Unclaimed", a category, "Unassigned"), not accounts, and CATEGORY is blank.
#     "in the history of "X" I should not be able to change or remove the reallocation of N euro from
#     <end> to <end>" means neither act is offered on that row.
#   - The Unclaimed steps and the UNCLAIMED column are explained in show-unclaimed.feature; the backing
#     and Accumulated steps, and the ACCUMULATED ON column, in back-a-category.feature; the sweep steps in
#     sweep-at-a-period-end.feature. Every other step is reused unchanged from the file that introduced it.
#
# Most scenarios start the same way: a salary of 2000 on Bank, the pool account; Deposit, added with 5000
# of my own; Savings and Shares both backed by Deposit; and 200 assigned to Savings, which moved to
# Deposit. So Bank holds 1800 and Deposit 5200, of which 5000 is Unclaimed. Unassigned is 1800, all of
# Bank's money, so Bank's Unclaimed is 0.00.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names, labels and amounts are synthetic
# test data.

@unclaimed
Feature: Reallocate an amount of purpose
  As someone with savings and shares built up before and alongside MoneyBud
  I want to give money already on an account a purpose, move what I built up for one purpose to another, and take it out when I want to spend it
  So that what each purpose shows matches what is really set aside for it, and money built up over months can be used without my budget showing an overspend

  # ----------------------------------------------------------------------------------
  # Giving money already on an account a purpose
  # ----------------------------------------------------------------------------------

  # The glossary's own example, with synthetic names. The 5000 is already on Deposit, so no money moves:
  # only its purpose. No budget, Remaining or Unassigned changes. Each move is a read-only row in
  # Deposit's history, although Deposit's balance did not change (follow-up 9).
  Scenario: Giving Unclaimed money a purpose on its own account moves no money, and changes no budget
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have a category "Shares"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Shares" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    When I reallocate 3000 euro from Unclaimed on "Deposit" to "Savings"
    Then the reallocation should go through
    And I should be told that 3000 euro was reallocated from Unclaimed on "Deposit" to "Savings", and of no money moved
    And I should not be warned or asked to confirm
    When I reallocate 2000 euro from Unclaimed on "Deposit" to "Shares"
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 1800.00 | 0.00      |
      | Deposit | 5200.00 | 0.00      |
    And net worth should still be 7000 euro
    And Accumulated for "Savings" in the current budget period should be 3200 euro
    And Accumulated for "Shares" in the current budget period should be 2000 euro
    And the budget for "Savings" in the current budget period should still be 200 euro
    And the remaining "Savings" budget in the current budget period should still be 200 euro
    And the budget for "Shares" in the current budget period should still be 0.00 euro
    And Unassigned in the current budget period should still be 1800 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | category | from      | to      | amount  | balance |
      | today | reallocation     |          | Unclaimed | Shares  | 2000.00 |         |
      | today | reallocation     |          | Unclaimed | Savings | 3000.00 |         |
      | today | movement         | Savings  | Bank      | Deposit | 200.00  |         |
      | today | starting balance |          |           |         |         | 5000.00 |
    And in the history of "Deposit" I should not be able to change or remove the reallocation of 3000 euro from Unclaimed to "Savings"

  # Ruling 2: a negative amount moves back. Both acts below do the same, and leave the same row: 500 from
  # Savings to Unclaimed. That the row reads as a move the other way is this stage's reading (header).
  Scenario Outline: A negative amount moves back, the same as moving the other way
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have a category "Shares"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Shares" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have reallocated 3000 euro from Unclaimed on "Deposit" to "Savings"
    When <act>
    Then the reallocation should go through
    And the Unclaimed of "Deposit" should be 2500 euro
    And Accumulated for "Savings" in the current budget period should be 2700 euro
    And the balance of "Deposit" should still be 5200 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | category | from      | to        | amount  | balance |
      | today | reallocation     |          | Savings   | Unclaimed | 500.00  |         |
      | today | reallocation     |          | Unclaimed | Savings   | 3000.00 |         |
      | today | movement         | Savings  | Bank      | Deposit   | 200.00  |         |
      | today | starting balance |          |           |           |         | 5000.00 |

    Examples:
      | act                                                              |
      | I reallocate 500 euro from "Savings" to Unclaimed on "Deposit"   |
      | I reallocate -500 euro from Unclaimed on "Deposit" to "Savings"  |

  # The glossary's example of a fall in value (ruling 3). Savings and Shares claim 5200 between them. The
  # bank says 4900, so Unclaimed is -300 (show-unclaimed.feature). I share the loss out myself: 300 from
  # Shares back to Unclaimed. No money moves; the balance is what the bank said.
  Scenario: After a fall in value, moving the loss out of a category brings Unclaimed back to zero
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have a category "Shares"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Shares" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have reallocated 3000 euro from Unclaimed on "Deposit" to "Savings"
    And I have reallocated 2000 euro from Unclaimed on "Deposit" to "Shares"
    And I have corrected the balance of "Deposit" to 4900 euro
    And the Unclaimed of "Deposit" is -300 euro
    When I reallocate 300 euro from "Shares" to Unclaimed on "Deposit"
    Then the Unclaimed of "Deposit" should be 0.00 euro
    And the Unclaimed of "Deposit" should not be marked below zero
    And Accumulated for "Shares" in the current budget period should be 1700 euro
    And Accumulated for "Savings" in the current budget period should still be 3200 euro
    And the balance of "Deposit" should still be 4900 euro

  # ----------------------------------------------------------------------------------
  # Moving what is built up from one purpose to another
  # ----------------------------------------------------------------------------------

  Scenario: Moving between two categories on the same account moves no money
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have a category "Shares"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Shares" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have reallocated 3000 euro from Unclaimed on "Deposit" to "Savings"
    When I reallocate 1000 euro from "Savings" to "Shares"
    Then the reallocation should go through
    And I should be told that 1000 euro was reallocated from "Savings" to "Shares", and of no money moved
    And Accumulated for "Savings" in the current budget period should be 2200 euro
    And Accumulated for "Shares" in the current budget period should be 1000 euro
    And the balance of "Deposit" should still be 5200 euro
    And the Unclaimed of "Deposit" should still be 2000 euro
    And the budget for "Shares" in the current budget period should still be 0.00 euro

  # Ruling 2: the money moves along, a row in both accounts' histories, and he makes the same transfer at
  # the bank. Neither account's Unclaimed changes: the money left with its purpose and arrived with it.
  Scenario: Moving between two categories on different accounts moves the money along
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have a category "Pension"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Pension" to "Broker"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have reallocated 3000 euro from Unclaimed on "Deposit" to "Savings"
    When I reallocate 1000 euro from "Savings" to "Pension"
    Then the reallocation should go through
    And I should be told that 1000 euro was reallocated from "Savings" to "Pension", and that 1000 euro moved from "Deposit" to "Broker"
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 1800.00 | 0.00      |
      | Deposit | 4200.00 | 2000.00   |
      | Broker  | 1000.00 | 0.00      |
    And net worth should still be 7000 euro
    And Accumulated for "Savings" in the current budget period should be 2200 euro
    And Accumulated for "Pension" in the current budget period should be 1000 euro
    And the history of "Broker" should be exactly these, newest first:
      | date  | entry            | category | from    | to      | amount  | balance |
      | today | reallocation     |          | Savings | Pension | 1000.00 |         |
      | today | starting balance |          |         |         |         | 0.00    |
    And the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | category | from      | to      | amount  | balance |
      | today | reallocation     |          | Savings   | Pension | 1000.00 |         |
      | today | reallocation     |          | Unclaimed | Savings | 3000.00 |         |
      | today | movement         | Savings  | Bank      | Deposit | 200.00  |         |
      | today | starting balance |          |           |         |         | 5000.00 |

  # Follow-up 8: one rule for money. Unclaimed on Deposit can go to a category Broker backs, and the
  # money goes along.
  Scenario: Giving Unclaimed money a purpose on another account moves the money along
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Pension"
    And I have set the backing account of "Pension" to "Broker"
    When I reallocate 1000 euro from Unclaimed on "Deposit" to "Pension"
    Then I should be told that 1000 euro was reallocated from Unclaimed on "Deposit" to "Pension", and that 1000 euro moved from "Deposit" to "Broker"
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 2000.00 | 0.00      |
      | Deposit | 4000.00 | 4000.00   |
      | Broker  | 1000.00 | 0.00      |
    And Accumulated for "Pension" in the current budget period should be 1000 euro

  # ----------------------------------------------------------------------------------
  # Using money built up: moving it to Unassigned
  #
  # The money moves to the pool account, and joins this period's Unassigned. From there it is planned
  # like any other: assigned to what I spend it on, which spends within its budget. No budget changes by
  # the move itself.
  # ----------------------------------------------------------------------------------

  # The glossary's example, for a holiday. 500 of Savings moves today from Deposit to Bank, and this
  # period's Unassigned rises by 500. Assigned to Holiday, which has no account, and spent there, it is
  # within Holiday's budget: no over-budget marker anywhere.
  Scenario: Money built up is used by moving it to Unassigned, assigning it, and spending it within that budget
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have a category "Holiday"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have reallocated 3000 euro from Unclaimed on "Deposit" to "Savings"
    When I reallocate 500 euro from "Savings" to Unassigned in the current budget period
    Then the reallocation should go through
    And I should be told that 500 euro was reallocated from "Savings" to Unassigned in the current budget period, and that 500 euro moved from "Deposit" to "Bank"
    And Unassigned in the current budget period should be 2300 euro
    And Accumulated for "Savings" in the current budget period should be 2700 euro
    And the budget for "Savings" in the current budget period should still be 200 euro
    And the remaining "Savings" budget in the current budget period should still be 200 euro
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 2300.00 | 0.00      |
      | Deposit | 4700.00 | 2000.00   |
    And the history of "Bank" should be exactly these, newest first:
      | date  | entry        | category | label   | from    | to         | amount  |
      | today | reallocation |          |         | Savings | Unassigned | 500.00  |
      | today | movement     | Savings  |         | Bank    | Deposit    | 200.00  |
      | today | income       |          | Salaris |         |            | 2000.00 |
    When I assign 500 euro to "Holiday" in the current budget period
    And I record an expense of 450 euro for "Holiday" labelled "Camping"
    Then the remaining "Holiday" budget in the current budget period should be 50 euro
    And "Holiday" should not be shown as over budget in the current budget period
    And Unassigned in the current budget period should be 1800 euro

  # Follow-up 8: Unclaimed money can go straight to Unassigned too, the money moving to the pool account.
  Scenario: Unclaimed money can be moved straight to Unassigned
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    When I reallocate 1000 euro from Unclaimed on "Deposit" to Unassigned in the current budget period
    Then I should be told that 1000 euro was reallocated from Unclaimed on "Deposit" to Unassigned in the current budget period, and that 1000 euro moved from "Deposit" to "Bank"
    And Unassigned in the current budget period should be 3000 euro
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 3000.00 | 0.00      |
      | Deposit | 4000.00 | 4000.00   |

  # Derived: money moved to Unassigned and left there is part of the period's leftover, and is swept at
  # the period's end with the rest. Here the destination is Savings itself, so it goes back where it came
  # from: a round trip that is true. Savings is backed, so its own Remaining is not swept.
  Scenario: Money moved to Unassigned and not assigned is swept at the period's end with the rest
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 1000 euro for "Savings" in the current budget period
    When I reallocate 500 euro from "Savings" to Unassigned in the current budget period
    Then Unassigned in the current budget period should be 1500 euro
    When the next budget period begins while MoneyBud is open
    Then I should be told that the period leftover of the previous budget period, 1500 euro, was swept into "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 0.00    |
      | Deposit | 2000.00 |
    And Accumulated for "Savings" in the current budget period should be 2000 euro

  # Follow-up 7: Unassigned is only ever a destination. 500 moved from Savings by mistake goes back by
  # assigning 500 to Savings, which raises its budget and moves the money back to Deposit. A minus sign
  # towards Unassigned is refused, as a non-case (derived).
  #
  # Note of 2026-10-05 (ruling 5 revised, and the ruling that Unassigned can give to the pool account's
  # Unclaimed): this stays true for a move from a category, and from another account's Unclaimed.
  # Unassigned is no longer only ever a destination, though: a move to it from the POOL ACCOUNT'S Unclaimed
  # is undone the other way, from Unassigned to Unclaimed on the pool account, or with a minus sign
  # ("Covering a lower balance correction ..." below). The scenario itself is unchanged.
  Scenario: A move to Unassigned is undone by assigning, not by a negative amount
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have reallocated 3000 euro from Unclaimed on "Deposit" to "Savings"
    And I have reallocated 500 euro from "Savings" to Unassigned in the current budget period
    When I try to reallocate -500 euro from "Savings" to Unassigned in the current budget period
    Then the reallocation should be refused
    And I should be told that moving money out of Unassigned is assigning
    And Unassigned in the current budget period should still be 2300 euro
    When I assign 500 euro to "Savings" in the current budget period
    Then the budget for "Savings" in the current budget period should be 700 euro
    And Unassigned in the current budget period should be 1800 euro
    And Accumulated for "Savings" in the current budget period should be 3200 euro
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1800.00 |
      | Deposit | 5200.00 |

  # Follow-up 6, over later periods too. The period named is the one whose Unassigned I chose, which is
  # the period on screen (this stage's reading, header). Nothing moves.
  Scenario Outline: Money can be moved to Unassigned only in the current period
    Given my budget periods are one month long
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    When I try to reallocate 100 euro from "Savings" to Unassigned in the <period> budget period
    Then the reallocation should be refused
    And I should be told that Unassigned can be used only in the current budget period
    And Unassigned in the <period> budget period should still be 0.00 euro
    And Accumulated for "Savings" in the current budget period should still be 200 euro
    And the balance of "Deposit" should still be 5200 euro

    Examples:
      | period   |
      | previous |
      | next     |

  # ----------------------------------------------------------------------------------
  # The pool account's Unclaimed (new 2026-10-05)
  #
  # Ruling 5 revised: the pool account shows Unclaimed, and it is an end like any account's. Ruled the
  # next day: Unassigned, which is on the pool account too, can give to it. Between the two no money moves.
  # ----------------------------------------------------------------------------------

  # His first use. Bank held 500 of my own before the salary, which the balance correction shows, and
  # Unassigned holds only income. Moving the 500 from Bank's Unclaimed to Unassigned gives it to the plan
  # without a transfer away and back: both ends are on Bank, so no money moves. It leaves a read-only row in
  # Bank's history, as every move does. Then it is assigned like any other money.
  Scenario: Money already on the pool account is given a purpose by moving it from its Unclaimed to Unassigned, and no money moves
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have a category "Holiday"
    And I have corrected the balance of "Bank" to 2500 euro
    And the Unclaimed of "Bank" is 500 euro
    When I reallocate 500 euro from Unclaimed on "Bank" to Unassigned in the current budget period
    Then the reallocation should go through
    And I should be told that 500 euro was reallocated from Unclaimed on "Bank" to Unassigned in the current budget period, and of no money moved
    And I should not be warned or asked to confirm
    And Unassigned in the current budget period should be 2500 euro
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 2500.00 | 0.00      |
    And the history of "Bank" should be exactly these, newest first:
      | date  | entry              | label   | from      | to         | amount  | balance |
      | today | reallocation       |         | Unclaimed | Unassigned | 500.00  |         |
      | today | balance correction |         |           |            |         | 2500.00 |
      | today | income             | Salaris |           |            | 2000.00 |         |
    And in the history of "Bank" I should not be able to change or remove the reallocation of 500 euro from Unclaimed to Unassigned
    When I assign 2500 euro to "Holiday" in the current budget period
    Then Unassigned in the current budget period should be 0.00 euro
    And the Unclaimed of "Bank" should still be 0.00 euro

  # The pool account's Unclaimed can go to a category another account backs, as any account's can, and the
  # money goes along: Savings is on Deposit, so 300 moves from Bank to Deposit. Unassigned does not change.
  Scenario: Unclaimed on the pool account can be given to a category another account backs, and the money moves along
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have corrected the balance of "Bank" to 2500 euro
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I reallocate 300 euro from Unclaimed on "Bank" to "Savings"
    Then the reallocation should go through
    And I should be told that 300 euro was reallocated from Unclaimed on "Bank" to "Savings", and that 300 euro moved from "Bank" to "Deposit"
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 2200.00 | 200.00    |
      | Deposit | 300.00  | 0.00      |
    And Accumulated for "Savings" in the current budget period should be 300 euro
    And the budget for "Savings" in the current budget period should still be 0.00 euro
    And Unassigned in the current budget period should still be 2000 euro

  # Ruled 2026-10-05, his second use. The bank says 1950: 50 of costs I never recorded. Unassigned still
  # plans all 2000 of the salary, so Bank's Unclaimed is -50, "Rood". I cover it from this period's
  # Unassigned: 50 of the plan's money becomes Unclaimed again, on the same account, so no money moves.
  # A minus sign the other way is the same move, and leaves the same row (this file's reading of a negative
  # amount, header).
  Scenario Outline: A lower balance correction on the pool account is covered by moving money from Unassigned to its Unclaimed
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have corrected the balance of "Bank" to 1950 euro
    And the Unclaimed of "Bank" is -50 euro
    When <act>
    Then the reallocation should go through
    And I should not be warned or asked to confirm
    And the Unclaimed of "Bank" should be 0.00 euro
    And the Unclaimed of "Bank" should not be marked below zero
    And Unassigned in the current budget period should be 1950 euro
    And the balance of "Bank" should still be 1950 euro
    And the history of "Bank" should be exactly these, newest first:
      | date  | entry              | label   | from       | to        | amount  | balance |
      | today | reallocation       |         | Unassigned | Unclaimed | 50.00   |         |
      | today | balance correction |         |            |           |         | 1950.00 |
      | today | income             | Salaris |            |           | 2000.00 |         |

    Examples:
      | act                                                                                       |
      | I reallocate 50 euro from Unassigned in the current budget period to Unclaimed on "Bank"  |
      | I reallocate -50 euro from Unclaimed on "Bank" to Unassigned in the current budget period |

  # Derived h. Moving out of Unassigned is never blocked, as assigning is not. All 2000 is planned for
  # Groceries, and 50 more is moved to Bank's Unclaimed anyway. Unassigned goes to -50, and the period is
  # Over-assigned: shown, not prevented. Bank's Unclaimed is 50, since the plan now claims 1950 of its 2000.
  Scenario: Moving more out of Unassigned than it holds goes through, and the period is shown as over-assigned
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have a category "Groceries"
    And I have a budget of 2000 euro for "Groceries" in the current budget period
    When I reallocate 50 euro from Unassigned in the current budget period to Unclaimed on "Bank"
    Then the reallocation should go through
    And I should not be warned or asked to confirm
    And Unassigned in the current budget period should be -50 euro
    And the current budget period should be shown as over-assigned
    And the Unclaimed of "Bank" should be 50 euro

  # ----------------------------------------------------------------------------------
  # More than there is, zero, and refusals
  # ----------------------------------------------------------------------------------

  # Follow-up 11: never blocked. Savings has 200 and gives 500; Deposit's Unclaimed is 5000 and gives
  # 6000. What falls below zero carries the marker, badge "Rood". Deposit's balance does not move.
  Scenario Outline: Moving more than there is goes through, and what falls below zero carries the marker with "Rood"
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have a category "Shares"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Shares" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    When I reallocate <amount> euro from <from> to "Shares"
    Then the reallocation should go through
    And I should not be warned or asked to confirm
    And the categories shown in the current budget period should be exactly these, in this order:
      | category | accumulated | accumulated marked |
      | Savings  | <savings>   | <savings marked>   |
      | Shares   | <shares>    | no                 |
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed   | unclaimed marked   |
      | Bank    | 1800.00 | 0.00        | no                 |
      | Deposit | 5200.00 | <unclaimed> | <unclaimed marked> |

    Examples:
      | amount  | from                   | savings | savings marked | shares  | unclaimed | unclaimed marked |
      | 500.00  | "Savings"              | -300.00 | yes            | 500.00  | 5000.00   | no               |
      | 6000.00 | Unclaimed on "Deposit" | 200.00  | no             | 6000.00 | -1000.00  | yes              |

  # Moving out of Savings to Unassigned more than Deposit holds: Deposit goes into the red, which is
  # true, and nothing stops it (follow-up 11; as unbacking may overdraw, derived). Deposit's Unclaimed is
  # zero: everything on it, and more, is Savings' shortfall.
  Scenario: Moving money out may overdraw the account it leaves
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    When I reallocate 500 euro from "Savings" to Unassigned in the current budget period
    Then the reallocation should go through
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance | overdrawn | unclaimed |
      | Bank    | 2300.00 | no        | 0.00      |
      | Deposit | -300.00 | yes       | 0.00      |
    And "Deposit" should be shown as overdrawn, with the marker a category over budget has and the badge "Rood"
    And Accumulated for "Savings" in the current budget period should be -300 euro

  # Derived: zero is accepted and changes nothing, as assigning zero is. It leaves no row (this stage's
  # reading).
  Scenario: Moving zero is accepted and changes nothing
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    When I reallocate 0 euro from Unclaimed on "Deposit" to "Savings"
    Then the reallocation should go through
    And the Unclaimed of "Deposit" should still be 5000 euro
    And Accumulated for "Savings" in the current budget period should still be 200 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | category | from | to      | amount | balance |
      | today | movement         | Savings  | Bank | Deposit | 200.00 |         |
      | today | starting balance |          |      |         |        | 5000.00 |

  # The order of the refusals is this stage's reading (header). The first six rows break one rule each;
  # the next three break two, and the first broken is the one reported. A zero amount with a wrong end is
  # still refused. Nothing changes.
  #
  # Five rows added 2026-10-05, for the ruling that Unassigned gives only to the pool account's Unclaimed:
  # out of Unassigned to Unclaimed on another account, or to a category, is still assigning, and so is a
  # minus sign from another account's Unclaimed to Unassigned. Unassigned of the previous period to Bank's
  # Unclaimed passes the ends and is refused for its period. Unassigned of the previous period to a category
  # breaks two rules, and the end is reported first.
  Scenario Outline: A reallocation that breaks a rule is refused, the first rule broken is reported, and nothing changes
    Given my budget periods are one month long
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    When I try to reallocate <amount> euro from <from> to <to>
    Then the reallocation should be refused
    And I should be told that <message>
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 1800.00 | 0.00      |
      | Deposit | 5200.00 | 5000.00   |
    And Accumulated for "Savings" in the current budget period should still be 200 euro
    And Unassigned in the current budget period should still be 1800 euro

    Examples:
      | amount | from                                     | to                                       | message                                                            |
      | 100    | "Savings"                                | "Savings"                                | a reallocation needs two different ends                            |
      | 100    | Unclaimed on "Deposit"                   | Unclaimed on "Deposit"                   | a reallocation needs two different ends                            |
      | 0.005  | Unclaimed on "Deposit"                   | "Savings"                                | an amount cannot be finer than a cent                              |
      | -100   | "Savings"                                | Unassigned in the current budget period  | moving money out of Unassigned is assigning                        |
      | 100    | "Savings"                                | Unassigned in the previous budget period | Unassigned can be used only in the current budget period           |
      | 0      | "Savings"                                | Unassigned in the previous budget period | Unassigned can be used only in the current budget period           |
      | 0.005  | "Savings"                                | "Savings"                                | a reallocation needs two different ends                            |
      | -0.005 | "Savings"                                | Unassigned in the previous budget period | an amount cannot be finer than a cent                              |
      | -100   | "Savings"                                | Unassigned in the previous budget period | moving money out of Unassigned is assigning                        |
      | 100    | Unassigned in the current budget period  | Unclaimed on "Deposit"                   | moving money out of Unassigned is assigning                        |
      | 100    | Unassigned in the current budget period  | "Savings"                                | moving money out of Unassigned is assigning                        |
      | -100   | Unclaimed on "Deposit"                   | Unassigned in the current budget period  | moving money out of Unassigned is assigning                        |
      | 100    | Unassigned in the previous budget period | Unclaimed on "Bank"                      | Unassigned can be used only in the current budget period           |
      | 100    | Unassigned in the previous budget period | "Savings"                                | moving money out of Unassigned is assigning                        |

  # ----------------------------------------------------------------------------------
  # Archived categories, and categories on "—"
  # ----------------------------------------------------------------------------------

  # Derived: an archived category is a source, not a destination. Taking its money out is tidying up,
  # and does not bring it back. With nothing left for it and no history in this period, it is not shown.
  Scenario: Money can be moved out of an archived category, which stays archived, but not into it
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Shares"
    And I have set the backing account of "Shares" to "Deposit"
    And I have reallocated 1000 euro from Unclaimed on "Deposit" to "Shares"
    And I have archived the category "Shares"
    Then the choices offered as From for a reallocation should include "Shares"
    And the choices offered as To for a reallocation should not include "Shares"
    When I reallocate 1000 euro from "Shares" to Unclaimed on "Deposit"
    Then the reallocation should go through
    And the Unclaimed of "Deposit" should be 5000 euro
    And the categories offered for a new expense should not include "Shares"
    And "Shares" should not be shown in the current budget period

  # Follow-up 5 for a category on "—": out yes, into no. Shares gave up its account with 1000 given from
  # Unclaimed, which is not this period's money, so it stayed on Deposit (back-a-category.feature). 400
  # of it goes to Unassigned, from Deposit to Bank, and 600 stays, still on Deposit.
  Scenario: Money can be moved out of what a category on none left behind, but not into it
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Shares"
    And I have set the backing account of "Shares" to "Deposit"
    And I have reallocated 1000 euro from Unclaimed on "Deposit" to "Shares"
    And I have removed the backing of "Shares"
    Then the choices offered as From for a reallocation should include "Shares"
    And the choices offered as To for a reallocation should not include "Shares"
    When I reallocate 400 euro from "Shares" to Unassigned in the current budget period
    Then I should be told that 400 euro was reallocated from "Shares" to Unassigned in the current budget period, and that 400 euro moved from "Deposit" to "Bank"
    And Accumulated for "Shares" in the current budget period should be 600 euro, on "Deposit"
    And Unassigned in the current budget period should be 2400 euro
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 2400.00 | 0.00      |
      | Deposit | 4600.00 | 4000.00   |

  # This stage's reading (header): a minus sign must not move money into an end that only gives.
  Scenario Outline: A negative amount cannot move money into an archived category, nor into one on none
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Shares"
    And I have set the backing account of "Shares" to "Deposit"
    And I have reallocated 1000 euro from Unclaimed on "Deposit" to "Shares"
    And <setup>
    When I try to reallocate -100 euro from "Shares" to Unclaimed on "Deposit"
    Then the reallocation should be refused
    And I should be told that money cannot be moved into "Shares"
    And Accumulated for "Shares" in the current budget period should still be 1000 euro
    And the Unclaimed of "Deposit" should still be 4000 euro

    Examples:
      | setup                                  |
      | I have archived the category "Shares"  |
      | I have removed the backing of "Shares" |

  # ----------------------------------------------------------------------------------
  # What the form offers
  # ----------------------------------------------------------------------------------

  # Derived (header). Savings, Pension and Emergency are backed, Emergency by Bank, the pool account
  # itself. Holiday is backed but archived: From only. Old is on "—" with 50 left on Deposit: From only.
  # Groceries never had an account: its money is on the pool account, where Unassigned and assigning reach
  # it.
  #
  # Revised 2026-10-05 (ruling 5 revised, and the ruling that Unassigned can give to the pool account's
  # Unclaimed). It said Bank, the pool account, has no Unclaimed, and that Unassigned is To only. Now
  # Unclaimed on Bank is offered on both sides, as every account's is, and Unassigned on both sides too:
  # as From it can only go to Unclaimed on Bank, which the form refuses otherwise (the refusal outline above).
  Scenario: The form offers every end there is, and each only on the side it can be
    Given I have an account "Deposit" with a starting balance of 1000 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Groceries"
    And I have a category "Savings"
    And I have a category "Pension"
    And I have a category "Emergency"
    And I have a category "Holiday"
    And I have a category "Old"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Pension" to "Broker"
    And I have set the backing account of "Emergency" to "Bank"
    And I have set the backing account of "Holiday" to "Deposit"
    And I have set the backing account of "Old" to "Deposit"
    And I have reallocated 100 euro from Unclaimed on "Deposit" to "Holiday"
    And I have archived the category "Holiday"
    And I have reallocated 50 euro from Unclaimed on "Deposit" to "Old"
    And I have removed the backing of "Old"
    Then the choices offered as From for a reallocation should be exactly these, in any order:
      | choice               |
      | Unclaimed on Bank    |
      | Unclaimed on Deposit |
      | Unclaimed on Broker  |
      | Savings              |
      | Pension              |
      | Emergency            |
      | Holiday              |
      | Old                  |
      | Unassigned           |
    And the choices offered as To for a reallocation should be exactly these, in any order:
      | choice               |
      | Unclaimed on Bank    |
      | Unclaimed on Deposit |
      | Unclaimed on Broker  |
      | Savings              |
      | Pension              |
      | Emergency            |
      | Unassigned           |

  # New 2026-10-05 (derived i). The first start's six categories have no account, and Betaalrekening is the
  # pool account, so the form offers its Unclaimed and Unassigned, on both sides, and nothing else. There is
  # always something to offer, so "Verplaatsen" is always there.
  Scenario: At a first start the form offers the pool account's Unclaimed and Unassigned, and nothing else
    Given I have just started using MoneyBud for the first time
    Then the choices offered as From for a reallocation should be exactly these, in any order:
      | choice                      |
      | Unclaimed on Betaalrekening |
      | Unassigned                  |
    And the choices offered as To for a reallocation should be exactly these, in any order:
      | choice                      |
      | Unclaimed on Betaalrekening |
      | Unassigned                  |

  # Follow-up 12, and derived: opened from a backed category's row, the form starts from that category.
  Scenario: A reallocation started from a category's row starts with that category as From
    Given I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I start a reallocation from the row of "Savings"
    Then the reallocation should start with "Savings" as From

  # ----------------------------------------------------------------------------------
  # Dated today
  #
  # Derived. The move counts in Accumulated from the period it is made in, so stepping back to the
  # previous period does not show it. The 200 assigned there does.
  # ----------------------------------------------------------------------------------

  Scenario: A reallocation is dated the day it is made, and earlier periods do not count it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    When the next budget period begins while MoneyBud is open
    And I reallocate 1000 euro from Unclaimed on "Deposit" to "Savings"
    Then Accumulated for "Savings" in the current budget period should be 1200 euro
    And Accumulated for "Savings" in the previous budget period should be 200 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date                                       | entry            | category | from      | to      | amount  | balance |
      | today                                      | reallocation     |          | Unclaimed | Savings | 1000.00 |         |
      | the last day of the previous budget period | movement         | Savings  | Bank      | Deposit | 200.00  |         |
      | the last day of the previous budget period | starting balance |          |           |         |         | 5000.00 |

  # ----------------------------------------------------------------------------------
  # What it does to taking a budget back
  #
  # Derived, and shown so that it is approved knowingly. A negative assignment to a backed category moves
  # back at most what is there for it (assign-to-a-backed-category.feature), and that now includes money
  # given from Unclaimed. Savings has 200 of this period's budget, 1000 given from Unclaimed, and 400 spent
  # from Deposit: 800 is there. Taking the 200 budget back moves 200 to Bank, matching the 200 that joins
  # Unassigned. Before this increment nothing was there beyond the budget, -200, and nothing would move.
  # ----------------------------------------------------------------------------------

  Scenario: Taking a backed category's budget back can draw on money given to it from Unclaimed
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have reallocated 1000 euro from Unclaimed on "Deposit" to "Savings"
    And I have recorded an expense of 400 euro for "Savings" labelled "Fiets" dated today on the account "Deposit"
    And Accumulated for "Savings" in the current budget period is 800 euro
    When I assign -200 euro to "Savings" in the current budget period
    Then the assignment should go through
    And the budget for "Savings" in the current budget period should be 0.00 euro
    And Unassigned in the current budget period should be 2000 euro
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 2000.00 | 0.00      |
      | Deposit | 4600.00 | 4000.00   |
    And Accumulated for "Savings" in the current budget period should be 600 euro
