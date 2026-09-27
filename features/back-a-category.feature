# Backing a category: saying which account a category's money really sits in, pointing that at
# another account, or removing it (glossary: "Backing and Accumulated", settled by the stakeholder on
# 2026-09-27; "Account-backed categories"; "The pool account"). What assigning to a backed category
# moves is in assign-to-a-backed-category.feature, spending against one in
# spend-against-a-backed-category.feature, Accumulated on the Overview in show-accumulated.feature,
# and the rows moved money leaves in an account's history in show-moved-money.feature. THIS FILE
# EXPLAINS THE STEPS THE FIVE SHARE.
#
# The rules, from arc42 §12:
#   - A category is BACKED BY ONE ACCOUNT, OR BY NONE. I set it, point it at another account, or
#     remove it, AT ANY TIME, from the category's row: on screen the list "Staat op", with "—" for
#     none and my accounts after it. Several backing accounts per category are deferred until missed.
#   - EVERY CATEGORY STARTS OUT UNBACKED, and assigning to an unbacked category moves no money, as
#     before (show-accounts.feature).
#   - BACKING A CATEGORY MOVES NOTHING ALREADY IN THE ACCOUNT. Money I already had stays seen by
#     location only. In the stakeholder's words: "nothing happens except that they are now backed".
#   - BUT ON THE DAY OF BACKING, THE CATEGORY'S UNSPENT REMAINING IN THE CURRENT PERIOD MOVES from the
#     pool account to the backing account: a Budget of 300 with 100 spent moves 200, because the 100
#     spent has already left the pool. IF NOTHING REMAINS, OR THE CATEGORY IS OVERSPENT, NOTHING MOVES.
#     Budgets already set for LATER periods move on those periods' first day, like any assignment.
#     ACCUMULATED STARTS AT WHAT MOVED. (Raised by the stakeholder himself, and ruled.)
#   - UNBACKING RETURNS THE MONEY to the pool account on that day, and RE-POINTING TAKES IT ALONG to
#     the new account. Revised by the stakeholder the same day, his own idea: "if you unback a
#     categorie its money return to the default account". "The money" is WHAT IS THERE FOR THE
#     CATEGORY IN THE BACKING ACCOUNT: what MoneyBud moved in for it, minus the category's expenses
#     paid FROM THAT ACCOUNT. That is not Accumulated, which counts the category's expenses on every
#     account. IF THERE IS NONE, NOTHING MOVES. Either act MAY OVERDRAW the account the money leaves,
#     shown with the marker, badge "Rood", and never blocked. All three are follow-ups ruled the same
#     day.
#   - Money built up in EARLIER PERIODS goes back to the pool account too, WITH NO PURPOSE: it joins
#     no period's Unassigned (ruled with the revision).
#   - MONEY PLANNED FOR A LATER PERIOD has not moved yet, so it is not returned or taken along. It
#     moves on that period's first day TO WHATEVER THE CATEGORY IS BACKED BY THEN: the new account
#     after re-pointing, and NOWHERE if it has been unbacked.
#   - RE-POINTING: ACCUMULATED CARRIES ON. BACKING AGAIN AFTER UNBACKING: ACCUMULATED STARTS OVER,
#     from what moves at the new backing.
#   - THE POOL ACCOUNT MAY BACK A CATEGORY. Assigning then moves money from the pool to the pool, so
#     no balance changes, but Accumulated still counts. The same when a backing account is later made
#     the pool (follow-up). SUCH A MOVEMENT LEAVES NO ROW in the history: no balance changed (ruled at
#     the scenario gate, 2026-09-27, over one row netting to zero).
#   - ARCHIVING DOES NOTHING TO BACKING. An archived category stays backed, keeps its Accumulated, and
#     its later budgets still move on their day. It can be unbacked separately, which returns its
#     money (follow-up). AN ARCHIVED CATEGORY CAN BE BACKED AND RE-POINTED, AND IT STAYS ARCHIVED:
#     backing is not one of the three acts that bring a category back (ruled at the scenario gate,
#     2026-09-27, over offering only "none" and over backing bringing it back).
#   - AN ACCOUNT THAT BACKS A CATEGORY COUNTS AS USED, so it cannot be deleted. An account with a
#     movement in its history is used too, even after the category it backed is unbacked (derived,
#     kept after the follow-ups).
#   - SETTING, RE-POINTING AND REMOVING BACKING ARE NEVER CONFIRMED, AND ARE ANNOUNCED AFTERWARDS,
#     NAMING WHAT MOVED (follow-up). The proposed wording, copy and not a ruling: "Sparen staat nu op
#     Spaarrekening: € 200,00 overgeboekt van Hoofdrekening." WHEN NOTHING MOVES, THE ANNOUNCEMENT
#     NAMES ONLY THE BACKING, with no amount (ruled at the scenario gate, 2026-09-27).
#
# DERIVED BY THE DOCUMENTATION AND NOT PUT TO THE STAKEHOLDER, marked where a scenario rests on one,
# so that they are approved or contradicted at this gate:
#   - Which of the category's expenses count for what unbacking moves: those paid from the backing
#     account and dated after the day it became the backing account, or on that day and recorded after.
#   - Re-pointing when there is nothing there for the category moves nothing, and Accumulated carries
#     on at the new account.
#   - An expense against the category paid from a THIRD account (neither the pool nor the backing
#     account) comes back to the pool account on unbacking, not to the account that paid.
#   - An account may back SEVERAL categories. Nothing limits it, and the settled model allowed it.
#   - Deleting a category with no history takes its backing with it, and nothing moves.
#   - A budget left in a PAST period does not move when the category is backed: the ruling names the
#     current period, and a past period's Leftover of an unbacked category is for the sweep.
#   - The accounts in the Staat op list follow the strip's order, pool account first, then the order
#     added, as every account list does.
#   - An account that backed a category and never had money moved for it is unused again once it
#     backs nothing. That follows from the two rules above, read as it is NOW, as manage-accounts.feature
#     reads "unused".
#   - Unbacking an archived category leaves it archived. Only the three acts the glossary names bring
#     an archived category back (adding its name, recording an expense against it, assigning a
#     positive amount to it), and unbacking is none of them.
#
# NOT specified here, and left to the plan: where the Staat op list sits on the row (next to
# Hernoemen, per the glossary), and on which periods' rows it is offered.
#
# Reading the steps. These are shared by all five backing files:
#   - "I set the backing account of "X" to "Y"" chooses Y in category X's Staat op list. It is the
#     act of backing when X is unbacked, and of re-pointing when X is already backed. "I remove the
#     backing of "X"" chooses "—". "I have set ..." and "I have removed ..." are the same acts done
#     earlier TODAY, at that point in the order of the Givens, with everything they move.
#   - Givens are listed in the order things happened, as in the accounts files. So a budget Given
#     ("I have a budget of N euro for "X" in the current (next) budget period") that comes AFTER X is
#     backed is an assignment made earlier today, and moved money exactly as assigning does
#     (assign-to-a-backed-category.feature): today for the current period, on its first day for the
#     next. One that comes BEFORE X is backed moved nothing. No scenario gives a BACKED category a
#     budget in a past period by a Given.
#   - "I have already spent N euro on "X" ..." is an expense on the pool account, and is only used
#     before X is backed. A Given that records an expense against a backed category always names its
#     account.
#   - "the backing account of "X" should be "Y"" means X's Staat op list shows Y. ""X" should not be
#     backed" means it shows "—".
#   - "the choices offered for the backing account of "X" should be exactly these, in this order" is
#     the whole Staat op list, in its order. "none" is the "—" choice.
#   - "Accumulated for "X" in the ... budget period should be N euro" is the figure X's row shows as
#     Opgebouwd while that period is on screen. ""X" should show no Accumulated in the ... budget
#     period" means the row shows no Opgebouwd at all, not a zero. "Accumulated for "X" in the ...
#     budget period is N euro", as a Given, SETS NOTHING: it states what the Givens above it already
#     come to, like show-accounts.feature's "the balance of "X" is N euro".
#   - "Accumulated for "X" in the ... budget period should (not) be marked below zero, with the
#     marker a category over budget has and the badge "Rood"" is show-accounts.feature's marker step,
#     for this figure.
#   - overview.feature's "the categories shown in the ... budget period should be exactly these" gains
#     two columns. ACCUMULATED is the Opgebouwd figure on that row, and a BLANK cell means the row
#     shows none. ACCUMULATED MARKED "yes" means that figure carries the marker, badge "Rood". A table
#     without these columns does not check them, which is how every earlier file stays as it was.
#     point-at-a-slice.feature's "the slice pointed at should show these figures" gains the column
#     ACCUMULATED too, with the same meaning.
#   - "I should be told that "X" is now backed by "Y", and that N euro moved from "A" to "B"" and "I
#     should be told that "X" is no longer backed, and that N euro moved from "A" to "B"" are the
#     announcements. "I should be told that "X" is now backed by "Y"", with nothing after it, checks
#     only that the backing was announced. "I should be told that "X" is now backed by "Y", and of no
#     money moved" is used where NOTHING MOVED: the announcement then names only the backing and no
#     amount (ruled by the stakeholder at the scenario gate, 2026-09-27, over saying that nothing
#     moved). What is fixed is what I am told, never the wording.
#   - In an account's history (show-accounts.feature's step), ENTRY "movement" is money MoneyBud moved
#     on a category's behalf. CATEGORY is the category it moved for, FROM and TO its two accounts, and
#     AMOUNT the amount, without a sign. See show-moved-money.feature.
#   - Every other step is reused unchanged from the file that introduced it, the account steps from
#     show-accounts.feature.
#
# The Dutch on screen, ruled by the stakeholder on 2026-09-27: "Staat op" (his own phrase from round
# 2) and "Opgebouwd", chosen over "Gespaard". A negative Opgebouwd reuses "Rood". As with every
# earlier display term, they are held by the glossary and `Tekst`, not by these steps, which use this
# project's English terms: backing account, Accumulated.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). No category in it is backed. The names,
# labels and amounts are synthetic test data.

@backing
Feature: Back a category with an account
  As someone who saves or invests for a purpose, in an account kept for it
  I want to say which account a category's money really sits in, and change or remove that whenever my accounts change
  So that money I budget for that purpose really moves there, my balances show where it is, and nothing is left stranded when I change my mind

  # ----------------------------------------------------------------------------------
  # The Staat op list
  # ----------------------------------------------------------------------------------

  # The order is the documentation's reading: every account list is in the strip's order.
  Scenario: A category starts out unbacked, and any of my accounts can back it, the pool account included
    Given I have an account "Deposit" with a starting balance of 5000 euro
    And I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Savings"
    Then "Savings" should not be backed
    And "Savings" should show no Accumulated in the current budget period
    And the choices offered for the backing account of "Savings" should be exactly these, in this order:
      | choice  |
      | none    |
      | Bank    |
      | Deposit |
      | Cash    |

  # ----------------------------------------------------------------------------------
  # Backing a category
  #
  # Nothing already in the account moves: the 5000 is money I already had, and keeps no purpose.
  # What moves is the category's own money for this period, which was on the pool account.
  # ----------------------------------------------------------------------------------

  Scenario: Backing a category with no budget moves nothing, and its Accumulated starts at zero
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    When I set the backing account of "Savings" to "Deposit"
    Then the backing account of "Savings" should be "Deposit"
    And I should be told that "Savings" is now backed by "Deposit", and of no money moved
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 2000.00 |
      | Deposit | 5000.00 |
    And net worth should still be 7000 euro
    And Accumulated for "Savings" in the current budget period should be 0.00 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | balance |
      | today | starting balance | 5000.00 |

  Scenario: Backing a category moves its unspent budget for the current period off the pool account
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    When I set the backing account of "Savings" to "Deposit"
    Then I should be told that "Savings" is now backed by "Deposit", and that 300 euro moved from "Bank" to "Deposit"
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1700.00 |
      | Deposit | 5300.00 |
    And net worth should still be 7000 euro
    And Accumulated for "Savings" in the current budget period should be 300 euro
    And the budget for "Savings" in the current budget period should still be 300 euro
    And Unassigned in the current budget period should still be 1700 euro

  # What was spent has already left the pool account, so only what remains moves. Spent down to
  # exactly nothing, or overspent, nothing moves, and Accumulated starts at zero: the expenses were
  # recorded before the backing, so they do not count against it (spend-against-a-backed-category.feature).
  Scenario Outline: Only what remains of the current period's budget moves, and nothing when it is all spent or overspent
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a budget of <budget> euro for "Savings" in the current budget period
    And I have already spent <spent> euro on "Savings" in the current budget period
    When I set the backing account of "Savings" to "Deposit"
    Then I should be told that "Savings" is now backed by "Deposit"
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance   |
      | Bank    | <bank>    |
      | Deposit | <deposit> |
    And Accumulated for "Savings" in the current budget period should be <moved> euro
    And the remaining "Savings" budget in the current budget period should still be <remaining> euro

    Examples:
      | budget | spent  | remaining | moved  | bank    | deposit |
      | 300.00 | 100.00 | 200.00    | 200.00 | 1700.00 | 5200.00 |
      | 300.00 | 299.99 | 0.01      | 0.01   | 1700.00 | 5000.01 |
      | 300.00 | 300.00 | 0.00      | 0.00   | 1700.00 | 5000.00 |
      | 300.00 | 350.00 | -50.00    | 0.00   | 1650.00 | 5000.00 |

  # "The current period" is the ruling's own wording: the period on screen does not change what moves.
  Scenario: Backing moves the current period's unspent budget, whichever period is on screen
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have a budget of 250 euro for "Savings" in the next budget period
    And the Overview shows the next budget period
    When I set the backing account of "Savings" to "Deposit"
    Then the balance of "Bank" should be 1700 euro
    And the balance of "Deposit" should be 5300 euro

  # The documentation's reading, not put to the stakeholder (see the header): the 400 left in the
  # previous period is that period's Leftover, which is for the sweep, not for backing.
  Scenario: A budget left in a past period does not move when the category is backed
    Given my budget periods are one month long
    And I have recorded an income of 3000 euro labelled "Salaris" dated on the first day of the previous budget period
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a budget of 400 euro for "Savings" in the previous budget period
    And I have a budget of 300 euro for "Savings" in the current budget period
    When I set the backing account of "Savings" to "Deposit"
    Then the balance of "Bank" should be 2700 euro
    And the balance of "Deposit" should be 5300 euro
    And Accumulated for "Savings" in the current budget period should be 300 euro
    And Accumulated for "Savings" in the previous budget period should be 0.00 euro

  Scenario: A budget already set for a later period moves on that period's first day
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a budget of 250 euro for "Savings" in the next budget period
    When I set the backing account of "Savings" to "Deposit"
    Then I should be told that "Savings" is now backed by "Deposit", and of no money moved
    And the balance of "Bank" should still be 2000 euro
    And the balance of "Deposit" should still be 5000 euro
    When the next budget period begins while MoneyBud is open
    Then the balance of "Bank" should be 1750 euro
    And the balance of "Deposit" should be 5250 euro
    And Accumulated for "Savings" in the current budget period should be 250 euro

  # The budget is more than the pool account holds, which assigning allows (Over-assigned). The move
  # goes through, and the pool account is shown overdrawn.
  Scenario: Backing may overdraw the pool account, and nothing stops it
    Given I have recorded an income of 100 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    When I set the backing account of "Savings" to "Deposit"
    Then I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance | overdrawn |
      | Bank    | -200.00 | yes       |
      | Deposit | 5300.00 | no        |
    And "Bank" should be shown as overdrawn, with the marker a category over budget has and the badge "Rood"

  # ----------------------------------------------------------------------------------
  # Unbacking a category returns its money to the pool account
  #
  # What moves is what is there for the category in the backing account: what was moved in for it,
  # minus its expenses paid from that account. Not Accumulated, which counts its expenses on every
  # account. So nothing is left in the backing account with no purpose, and the pool account ends up
  # exactly where MoneyBud assumes it is.
  # ----------------------------------------------------------------------------------

  # The glossary's own example. 200 moved in, 50 spent against Savings from Bank, so Accumulated is
  # 150, and 200 comes back. Bank ends up exactly 50 down, for the 50 spent.
  Scenario: Unbacking returns what was moved in for the category, even when some of it was spent from the pool account
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    And I have recorded an expense of 50 euro for "Savings" labelled "Cadeau" dated today on the account "Bank"
    And Accumulated for "Savings" in the current budget period is 150 euro
    When I remove the backing of "Savings"
    Then I should be told that "Savings" is no longer backed, and that 200 euro moved from "Deposit" to "Bank"
    And I should not be warned or asked to confirm
    And "Savings" should not be backed
    And "Savings" should show no Accumulated in the current budget period
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1950.00 |
      | Deposit | 5000.00 |
    And net worth should still be 6950 euro
    And the remaining "Savings" budget in the current budget period should still be 150 euro
    And Unassigned in the current budget period should still be 1800 euro

  # Deposit starts at 0, and 200 was moved in for Savings. Then:
  #   - 50 against Savings paid from Deposit: 150 is there for it, and 150 moves.
  #   - 150 against Groceries paid from Deposit: that is not Savings money, so 200 is still there for
  #     Savings and 200 moves, which leaves Deposit short: the money moved there was spent on
  #     something else. Ruled true.
  #   - 200 against Savings paid from Deposit: nothing is there, and nothing moves.
  #   - 250 against Savings paid from Deposit: less than nothing is there, and nothing moves.
  Scenario Outline: Unbacking moves what is there for the category in the backing account, and nothing when there is none
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Groceries"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    And <spending>
    When I remove the backing of "Savings"
    Then I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance   | overdrawn   |
      | Bank    | <bank>    | no          |
      | Deposit | <deposit> | <overdrawn> |

    Examples:
      | spending                                                                                                     | bank    | deposit | overdrawn |
      | I have recorded an expense of 50 euro for "Savings" labelled "Cadeau" dated today on the account "Deposit"   | 1950.00 | 0.00    | no        |
      | I have recorded an expense of 150 euro for "Groceries" labelled "Markt" dated today on the account "Deposit" | 2000.00 | -150.00 | yes       |
      | I have recorded an expense of 200 euro for "Savings" labelled "Cadeau" dated today on the account "Deposit"  | 1800.00 | 0.00    | no        |
      | I have recorded an expense of 250 euro for "Savings" labelled "Cadeau" dated today on the account "Deposit"  | 1800.00 | -50.00  | yes       |

  # Ruled with the revision: money built up in an earlier period goes back too. It was assigned in
  # that period long ago, so it joins no period's Unassigned: it is money with no purpose, like a
  # starting balance.
  Scenario: Money built up in earlier periods goes back to the pool account too, and joins no period's Unassigned
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have recorded an income of 1800 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    When the next budget period begins while MoneyBud is open
    And I assign 100 euro to "Savings" in the current budget period
    And I remove the backing of "Savings"
    Then I should be told that "Savings" is no longer backed, and that 300 euro moved from "Deposit" to "Bank"
    And the balance of "Bank" should be 3800 euro
    And the balance of "Deposit" should be 0.00 euro
    And Unassigned in the current budget period should still be 1700 euro
    And Unassigned in the previous budget period should still be 1800 euro
    And "Savings" should show no Accumulated in the current budget period
    And "Savings" should show no Accumulated in the previous budget period

  # A category's expense paid from a third account, Cash, neither the pool nor the backing account.
  # Derived by the documentation and not put to the stakeholder: what came into Deposit for Savings
  # all goes back to the pool account, so nothing is stranded, but the 50 lands on Bank rather than
  # on Cash, which paid it.
  Scenario: An expense against the category paid from a third account does not change what unbacking returns
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Cash" with a starting balance of 100 euro
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    And I have recorded an expense of 50 euro for "Savings" labelled "Cadeau" dated today on the account "Cash"
    When I remove the backing of "Savings"
    Then I should be told that "Savings" is no longer backed, and that 200 euro moved from "Deposit" to "Bank"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 2000.00 |
      | Deposit | 0.00    |
      | Cash    | 50.00   |

  # ----------------------------------------------------------------------------------
  # Pointing the backing at another account takes the money along
  # ----------------------------------------------------------------------------------

  Scenario: Pointing the backing at another account takes the money along, and Accumulated carries on
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    And I have recorded an expense of 50 euro for "Savings" labelled "Cadeau" dated today on the account "Bank"
    When I set the backing account of "Savings" to "Broker"
    Then I should be told that "Savings" is now backed by "Broker", and that 200 euro moved from "Deposit" to "Broker"
    And I should not be warned or asked to confirm
    And the backing account of "Savings" should be "Broker"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1750.00 |
      | Deposit | 0.00    |
      | Broker  | 200.00  |
    And net worth should still be 1950 euro
    And Accumulated for "Savings" in the current budget period should still be 150 euro

  # Derived by the documentation, not put to the stakeholder: the same "nothing when there is none"
  # as unbacking. Deposit has paid out more for Savings than it received, which stays true of Deposit.
  Scenario: Re-pointing when nothing is there for the category moves nothing, and Accumulated carries on
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    And I have recorded an expense of 250 euro for "Savings" labelled "Fiets" dated today on the account "Deposit"
    When I set the backing account of "Savings" to "Broker"
    Then the backing account of "Savings" should be "Broker"
    And I should be told that "Savings" is now backed by "Broker", and of no money moved
    And the accounts should be exactly these, in this order:
      | account | balance | overdrawn |
      | Bank    | 1800.00 | no        |
      | Deposit | -50.00  | yes       |
      | Broker  | 0.00    | no        |
    And Accumulated for "Savings" in the current budget period should still be -50 euro

  # Settled by the revision: the money is where the category is backed now, so pulling budget back
  # draws from there, and Deposit is not touched.
  Scenario: After re-pointing, taking money back out of the budget draws from the new backing account
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Savings" to "Broker"
    When I assign -50 euro to "Savings" in the current budget period
    Then the assignment should go through
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1850.00 |
      | Deposit | 0.00    |
      | Broker  | 150.00  |
    And Accumulated for "Savings" in the current budget period should be 150 euro

  # ----------------------------------------------------------------------------------
  # Money planned for a later period follows the backing on the day it moves
  #
  # It has not moved yet, so neither act touches it today. On the period's first day it goes to
  # whatever backs the category then: the new account, or nowhere.
  # ----------------------------------------------------------------------------------

  Scenario Outline: Money planned for a later period goes to whatever backs the category on that period's first day
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the next budget period
    When <act>
    Then the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 2000.00 |
      | Deposit | 0.00    |
      | Broker  | 0.00    |
    When the next budget period begins while MoneyBud is open
    Then the accounts should be exactly these, in this order:
      | account | balance   |
      | Bank    | <bank>    |
      | Deposit | 0.00      |
      | Broker  | <broker>  |
    And the budget for "Savings" in the current budget period should be 300 euro

    Examples:
      | act                                                | bank    | broker |
      | I set the backing account of "Savings" to "Broker" | 1700.00 | 300.00 |
      | I remove the backing of "Savings"                  | 2000.00 | 0.00   |

  # ----------------------------------------------------------------------------------
  # Backing again after unbacking starts Accumulated over
  #
  # Unbacking returned the old 300 to the pool account. Backing again moves what remains of this
  # period's budget, 100, and Accumulated counts from there. Before today nothing had moved since
  # this backing, so the previous period shows zero.
  # ----------------------------------------------------------------------------------

  Scenario: Backing a category again after unbacking it starts Accumulated over
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have recorded an income of 1800 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    When the next budget period begins while MoneyBud is open
    And I assign 100 euro to "Savings" in the current budget period
    Then Accumulated for "Savings" in the current budget period should be 300 euro
    When I remove the backing of "Savings"
    And I set the backing account of "Savings" to "Deposit"
    Then I should be told that "Savings" is now backed by "Deposit", and that 100 euro moved from "Bank" to "Deposit"
    And Accumulated for "Savings" in the current budget period should be 100 euro
    And Accumulated for "Savings" in the previous budget period should be 0.00 euro
    And the balance of "Bank" should be 3700 euro
    And the balance of "Deposit" should be 100 euro

  # ----------------------------------------------------------------------------------
  # The pool account may back a category
  #
  # Money moves from the pool to the pool, so no balance changes, but what has been set aside for
  # the category is still counted. Nothing changed a balance, so the history shows no movement row
  # (ruled at the scenario gate).
  # ----------------------------------------------------------------------------------

  Scenario: When the pool account backs a category, assigning to it changes no balance, and Accumulated still counts
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have a category "Savings"
    When I set the backing account of "Savings" to "Bank"
    And I assign 300 euro to "Savings" in the current budget period
    Then the backing account of "Savings" should be "Bank"
    And the balance of "Bank" should still be 2000 euro
    And Accumulated for "Savings" in the current budget period should be 300 euro
    And Unassigned in the current budget period should be 1700 euro
    And the history of "Bank" should be exactly these, newest first:
      | date  | entry  | category | label   | from | to | amount  |
      | today | income |          | Salaris |      |    | 2000.00 |

  Scenario: When a backing account is made the pool account, assigning to its category changes no balance either
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I make "Deposit" the pool account
    And I assign 300 euro to "Savings" in the current budget period
    Then the balance of "Bank" should still be 2000 euro
    And the balance of "Deposit" should still be 5000 euro
    And Accumulated for "Savings" in the current budget period should be 300 euro
    And the history of "Deposit" should be exactly these, newest first:
      | date  | entry            | balance |
      | today | starting balance | 5000.00 |

  # ----------------------------------------------------------------------------------
  # One account may back several categories
  #
  # The documentation's reading, not asked: nothing limits it, and the model allowed it. Each
  # category has its own Accumulated, and unbacking one returns what is there for that one only.
  # ----------------------------------------------------------------------------------

  Scenario: One account may back several categories, and unbacking one returns only that category's money
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have a category "Holiday"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the backing account of "Holiday" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have a budget of 150 euro for "Holiday" in the current budget period
    And the balance of "Deposit" is 450 euro
    When I remove the backing of "Holiday"
    Then I should be told that "Holiday" is no longer backed, and that 150 euro moved from "Deposit" to "Bank"
    And the balance of "Deposit" should be 300 euro
    And the balance of "Bank" should be 1700 euro
    And the backing account of "Savings" should be "Deposit"
    And Accumulated for "Savings" in the current budget period should be 300 euro
    And "Holiday" should show no Accumulated in the current budget period

  # ----------------------------------------------------------------------------------
  # Archiving does nothing to backing
  # ----------------------------------------------------------------------------------

  Scenario: An archived category stays backed, and its later budgets still move on their day
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have a budget of 300 euro for "Savings" in the next budget period
    When I archive the category "Savings"
    Then I should be told that "Savings" was archived
    And the backing account of "Savings" should be "Deposit"
    And the balance of "Bank" should still be 1800 euro
    And the balance of "Deposit" should still be 200 euro
    When the next budget period begins while MoneyBud is open
    Then the balance of "Bank" should be 1500 euro
    And the balance of "Deposit" should be 500 euro
    And Accumulated for "Savings" in the current budget period should be 500 euro

  # That it stays archived is the documentation's reading (see the header).
  Scenario: Unbacking an archived category returns its money, and it stays archived
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have archived the category "Savings"
    When I remove the backing of "Savings"
    Then I should be told that "Savings" is no longer backed, and that 200 euro moved from "Deposit" to "Bank"
    And the balance of "Bank" should be 2000 euro
    And the balance of "Deposit" should be 0.00 euro
    And the categories offered for a new expense should not include "Savings"

  # Ruled at the scenario gate: the Staat op list works on an archived row too, and backing is not one
  # of the three acts that bring a category back. Setting the backing moves this period's unspent
  # budget, as for any category; pointing it elsewhere takes the money along.
  Scenario: An archived category can be backed and re-pointed, and it stays archived
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have an account "Broker" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have archived the category "Savings"
    When I set the backing account of "Savings" to "Deposit"
    Then I should be told that "Savings" is now backed by "Deposit", and that 200 euro moved from "Bank" to "Deposit"
    And the backing account of "Savings" should be "Deposit"
    And the categories offered for a new expense should not include "Savings"
    When I set the backing account of "Savings" to "Broker"
    Then I should be told that "Savings" is now backed by "Broker", and that 200 euro moved from "Deposit" to "Broker"
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1800.00 |
      | Deposit | 0.00    |
      | Broker  | 200.00  |
    And Accumulated for "Savings" in the current budget period should be 200 euro
    And the categories offered for a new expense should not include "Savings"

  # ----------------------------------------------------------------------------------
  # What backing makes of the account
  #
  # An account that backs a category is in use, so it cannot be deleted (manage-accounts.feature). So
  # is one with a movement in its history, for good: a movement is on the account, as a transfer is.
  # ----------------------------------------------------------------------------------

  # Nothing ever moved: there was no budget. Once Deposit backs nothing, it is unused again. That
  # last step is derived (see the header).
  Scenario: An account that backs a category cannot be deleted, even with nothing on it, and can once it backs nothing
    Given I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    Then I should not be able to delete the account "Deposit"
    When I remove the backing of "Savings"
    Then I should be able to delete the account "Deposit"

  Scenario: An account that money was moved into or out of stays in use after the category is unbacked
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    When I remove the backing of "Savings"
    Then "Savings" should not be backed
    And the balance of "Deposit" should be 0.00 euro
    And I should not be able to delete the account "Deposit"

  # Derived, not asked: a category with no history has no budget and no expense anywhere, so nothing
  # was ever moved for it, and its backing goes with it.
  Scenario: Deleting a category with no history takes its backing with it, and nothing moves
    Given I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    Then I should be able to delete the category "Savings"
    When I delete the category "Savings"
    Then I should be told that "Savings" was deleted
    And the balance of "Deposit" should still be 0.00 euro
    And I should be able to delete the account "Deposit"
