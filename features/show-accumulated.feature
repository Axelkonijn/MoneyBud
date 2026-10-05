# Seeing Accumulated: what has been built up for a backed category, on its row and in its slice of
# the ring (glossary: Accumulated; "Backed categories accumulate", "Accumulated covers everything up
# to the period on screen", "On screen: Staat op and Opgebouwd"; "Backing and Accumulated", settled
# by the stakeholder on 2026-09-27). What moves money in and out, and so what Accumulated counts, is
# in back-a-category.feature, assign-to-a-backed-category.feature and
# spend-against-a-backed-category.feature. The steps the backing files share are explained in
# back-a-category.feature.
#
# REVISED FOR INCREMENT 15, 2026-10-04 (glossary: "Vrij, and moving Opgebouwd", settled by the stakeholder
# that day). The scenarios marked "Revised for increment 15" or "New in increment 15" below were
# APPROVED at the scenario gate on 2026-10-04.
#   - ACCUMULATED CAN NOW BE MOVED BY ME, as an amount: in from an account's Unclaimed or from another
#     category, out to Unclaimed, to another category or to this period's Unassigned
#     (reallocate-an-amount.feature). None of that is a budget. And IT NO LONGER STARTS OVER: setting Staat
#     op to "—" returns only this period's money and leaves the rest where it is (back-a-category.feature).
#   - A CATEGORY ON "—" THAT LEFT MONEY ON AN ACCOUNT SHOWS IT, WITH THE ACCOUNT IT IS ON: "Opgebouwd
#     € 5.000,00 op Spaarrekening" (follow-up). Its expenses do not lower it, since they are this period's
#     and counted in Remaining; and assigning to it moves nothing (follow-ups). Two scenarios are new.
#   - An expense against a backed category is always on its backing account now
#     (spend-against-a-backed-category.feature). Two scenarios here paid one from Bank: "Accumulated below
#     zero carries the marker ..." and "Backing an overspent category starts Accumulated below zero ...".
#     Both are revised to pay from Deposit; what they show about Accumulated is unchanged, and Deposit now
#     holds exactly what Accumulated says.
#   - BACKING AN OVERSPENT CATEGORY MOVES THE OVERSPENDING THE OTHER WAY (follow-up 15), which the second of
#     those two now shows too.
#   - "An archived backed category with money built up is shown ... until it is unbacked" is revised: set to
#     "—", it keeps the money it built up in earlier periods, so it stays shown until nothing is left for it.
#
# REVISED 2026-10-05 for ruling 5 revised; gates waived by Axel, presented with the plan and the app. The
# pool account now shows Unclaimed too (show-unclaimed.feature). The three accounts tables with an UNCLAIMED
# column gave Bank, the pool account, a blank cell, meaning it shows none; each now carries its figure, 0.00
# in every one, since Bank holds exactly this period's Unassigned and the Remaining of its categories without
# an account. Nothing else changes.
#
# The rules, from arc42 §12:
#   - ACCUMULATED, on screen "Opgebouwd", IS WHAT HAS MOVED IN ON THE CATEGORY'S BEHALF SINCE IT WAS
#     LAST BACKED, MINUS WHAT HAS BEEN SPENT AGAINST IT SINCE, on any account. "Moved in" is net: a
#     negative assignment moves money back out. The move made when the category is backed counts as
#     moved in.
#   - A BACKED ROW SHOWS "Opgebouwd: € 600,00" UNDER ITS FIGURES, AND SO DO ITS SLICE'S HOVER DETAILS.
#     AN UNBACKED CATEGORY SHOWS NONE, not a zero. (Revised 2026-10-04: except a category set to "—" that
#     left money on an account. Its row shows that money and where it is, "Opgebouwd € 5.000,00 op
#     Spaarrekening", and so, in this file's reading, does its slice, as a slice shows everything its row
#     does. A category that was never backed, or left nothing behind, still shows none.)
#   - IT IS SHOWN UP TO AND INCLUDING THE PERIOD ON SCREEN. Stepping back shows what had been built up
#     by then; stepping forward includes what is planned. The strip's balances are always today's, so
#     in a later period Accumulated and the backing account's balance differ by what is planned, on
#     purpose. Rejected: always as of today, like Vermogen.
#   - IN EARLIER PERIODS, IT FOLLOWS TODAY'S BACKING: a category backed now shows it in every period
#     it is shown in, zero before anything moved; an unbacked category shows it nowhere (follow-up).
#     Rejected: following the backing as it was in the period on screen, because a row would then
#     show it in some periods and not others, for reasons nothing on screen shows.
#   - BELOW ZERO IT CARRIES THE ONE MARKER, WITH THE BADGE "Rood", the same as an overdrawn account.
#     Shown, never blocked. Exactly zero carries none (follow-up).
#   - AN ARCHIVED BACKED CATEGORY KEEPS AND SHOWS IT, wherever its row is shown and in its slice
#     (follow-up). AND ITS ROW IS SHOWN IN THE CURRENT PERIOD AND LATER ONES WHILE ITS ACCUMULATED
#     THERE IS NOT ZERO, even with no budget and no expense there, so that money still there for it is
#     never hidden and it can be unbacked from the period it is in. Past periods are unchanged. Ruled
#     by the stakeholder at the build, 2026-09-27, over leaving the row out.
#   - It is NOT the backing account's balance. They differ by what was in the account before, by
#     spending put on another account, and by the period on screen. (Revised 2026-10-04: what was in the
#     account before now shows as its Unclaimed, and spending can no longer be put on another account
#     except in data kept before increment 15. So in the current period Unclaimed plus the Accumulated of
#     the account's categories is its balance: show-unclaimed.feature.)
#
# Which categories a period shows is otherwise unchanged (show-categories-in-a-period.feature): a past
# period shows only the categories with history there. So a backed category is not shown, and neither
# is its Accumulated, in a past period where it has no budget and no expense.
#
# NOT specified here, and left to developer tests and the markup, as for every figure on a row: where
# Opgebouwd sits under the figures, and how it is written.
#
# Reading the steps: the Accumulated steps and the ACCUMULATED and ACCUMULATED MARKED columns are
# explained in back-a-category.feature. The row table is overview.feature's, the slice steps are
# point-at-a-slice.feature's, and the stepping steps step-between-periods.feature's.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names, labels and amounts are
# synthetic test data.

@backing
Feature: See what has been built up for a backed category
  As someone saving for a purpose over many months
  I want each backed category to show how much has been built up for it, in whichever period I look at
  So that I can see my progress towards the purpose, which a budget that starts again every period cannot show

  # ----------------------------------------------------------------------------------
  # What Accumulated counts
  # ----------------------------------------------------------------------------------

  # The glossary's own example, with synthetic names. 300 budgeted and 100 spent before the backing,
  # so 200 moves and Accumulated starts there. 50 more moves today. 300 for the next period moves on
  # its first day, so today's balances do not change, while stepping forward shows it in
  # Accumulated.
  Scenario: Accumulated starts at what moved on backing, grows with each assignment, and a later period includes what is planned
    Given my budget periods are one month long
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have already spent 100 euro on "Savings" in the current budget period
    When I set the backing account of "Savings" to "Deposit"
    Then Accumulated for "Savings" in the current budget period should be 200 euro
    And the balance of "Deposit" should be 5200 euro
    When I assign 50 euro to "Savings" in the current budget period
    Then Accumulated for "Savings" in the current budget period should be 250 euro
    And the balance of "Deposit" should be 5250 euro
    When I assign 300 euro to "Savings" in the next budget period
    Then Accumulated for "Savings" in the current budget period should still be 250 euro
    And the balance of "Deposit" should still be 5250 euro
    When I step forward one budget period
    Then the categories shown in the next budget period should be exactly these, in this order:
      | category | budget | spent | remaining | over budget | accumulated |
      | Savings  | 300.00 | 0.00  | 300.00    | no          | 550.00      |
    And the balance of "Deposit" should still be 5250 euro

  # 200 moved in the period that is now the previous one, and 100 in the current one. Each period
  # shows what had been built up by its end. The strip is today's in both.
  Scenario: Stepping back shows Accumulated as it stood at the end of that period, while the balance stays today's
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have recorded an income of 1800 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    When the next budget period begins while MoneyBud is open
    And I assign 100 euro to "Savings" in the current budget period
    Then Accumulated for "Savings" in the current budget period should be 300 euro
    And the balance of "Deposit" should be 300 euro
    When I step back one budget period
    Then the categories shown in the previous budget period should be exactly these, in this order:
      | category | budget | accumulated |
      | Savings  | 200.00 | 200.00      |
    And the balance of "Deposit" should still be 300 euro

  # Savings and Groceries both had a budget in the previous period, so both are shown there. Savings
  # is backed now, and nothing had moved for it by the end of that period, so it shows zero there.
  # Groceries is not backed, and shows none in any period.
  Scenario: In earlier periods Accumulated follows today's backing: zero before anything moved, and none for an unbacked category
    Given my budget periods are one month long
    And I have recorded an income of 3000 euro labelled "Salaris" dated on the first day of the previous budget period
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 100 euro for "Savings" in the previous budget period
    And I have a budget of 100 euro for "Groceries" in the previous budget period
    And I have a budget of 200 euro for "Savings" in the current budget period
    When I set the backing account of "Savings" to "Deposit"
    Then the categories shown in the previous budget period should be exactly these, in this order:
      | category  | budget | accumulated |
      | Savings   | 100.00 | 0.00        |
      | Groceries | 100.00 |             |
    And the categories shown in the current budget period should be exactly these, in this order:
      | category  | budget | accumulated |
      | Savings   | 200.00 | 200.00      |
      | Groceries | 0.00   |             |

  # ----------------------------------------------------------------------------------
  # Accumulated below zero carries the marker, badge "Rood"
  #
  # Reached when more has been spent against the category since the backing than has moved in: by
  # spending put on another account, or after backing an overspent category. Exactly zero is the
  # money spent down to nothing, and carries no marker, the line every marker draws. (Since increment
  # 15 spending can no longer be put on another account; overspending the backing account reaches it,
  # and so does moving out more than there is: reallocate-an-amount.feature.)
  # ----------------------------------------------------------------------------------

  # The two markers are separate: the budget's Remaining is over budget, and Accumulated is below zero.
  #
  # Revised for increment 15. Fiets was first paid from Bank, so Deposit kept the 100 and was never
  # overdrawn. A backed category's expense is now always on its backing account, so Fiets comes off
  # Deposit, which then holds exactly Savings' Accumulated: overdrawn by the cent, with its own marker.
  # Deposit's Unclaimed stays zero, since everything on it is Savings'.
  Scenario Outline: Accumulated below zero carries the marker with the badge "Rood", and exactly zero does not
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 100 euro for "Savings" in the current budget period
    When I record an expense of <spent> euro for "Savings" labelled "Fiets"
    Then I should not be warned or asked to confirm
    And the categories shown in the current budget period should be exactly these, in this order:
      | category | budget | spent   | remaining   | over budget | accumulated   | accumulated marked |
      | Savings  | 100.00 | <spent> | <remaining> | <over>      | <accumulated> | <marked>           |
    And the accounts should be exactly these, in this order:
      | account | balance       | overdrawn | unclaimed |
      | Bank    | 1900.00       | no        | 0.00      |
      | Deposit | <accumulated> | <marked>  | 0.00      |

    Examples:
      | spent  | remaining | over | accumulated | marked |
      | 100.00 | 0.00      | no   | 0.00        | no     |
      | 100.01 | -0.01     | yes  | -0.01       | yes    |

  # The glossary's example: 350 spent of 300, and Accumulated starts at the period's Remaining, -50
  # (ruling of 2026-09-28). 30 more spent takes it to -80. 100 assigned moves 100 and brings it back to
  # 20, the budget's Remaining.
  #
  # Revised for increment 15. It first said nothing moves at the backing, and paid Kado from Bank. Now the
  # 50 overspent moves the other way at the backing, from Deposit to Bank, which paid it (follow-up 15), and
  # Kado comes off Deposit, as a backed category's expense always does. So Deposit holds exactly what
  # Accumulated says at every step. What Accumulated says is unchanged.
  Scenario: Backing an overspent category starts Accumulated below zero, and assigning brings it back
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have already spent 350 euro on "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    Then the balance of "Deposit" should be -50 euro
    And the balance of "Bank" should be 1700 euro
    When I record an expense of 30 euro for "Savings" labelled "Kado"
    Then Accumulated for "Savings" in the current budget period should be -80 euro
    And Accumulated for "Savings" in the current budget period should be marked below zero, with the marker a category over budget has and the badge "Rood"
    And the balance of "Deposit" should be -80 euro
    When I assign 100 euro to "Savings" in the current budget period
    Then Accumulated for "Savings" in the current budget period should be 20 euro
    And Accumulated for "Savings" in the current budget period should not be marked below zero
    And the remaining "Savings" budget in the current budget period should be 20 euro
    And the balance of "Deposit" should be 20 euro

  # ----------------------------------------------------------------------------------
  # In the ring: pointing at a slice
  #
  # A backed category's slice shows its Accumulated with its other figures, as its row does. An
  # unbacked category's shows none.
  # ----------------------------------------------------------------------------------

  Scenario: Pointing at a backed category's slice shows its Accumulated, and an unbacked one's shows none
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 50 euro for "Savings" labelled "Fiets" dated today on the account "Deposit"
    When I point at the "Savings" slice in the ring for the current budget period
    Then the slice pointed at should show these figures:
      | category | budget | spent | remaining | over budget | accumulated |
      | Savings  | 300.00 | 50.00 | 250.00    | no          | 250.00      |
    When I point at the "Groceries" slice in the ring for the current budget period
    Then the slice pointed at should show these figures:
      | category  | budget | spent | remaining | over budget | accumulated |
      | Groceries | 400.00 | 0.00  | 400.00    | no          |             |

  # ----------------------------------------------------------------------------------
  # An archived backed category keeps showing it
  # ----------------------------------------------------------------------------------

  Scenario: An archived backed category keeps showing Accumulated, on its row and in its slice
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have recorded an expense of 50 euro for "Savings" labelled "Fiets" dated today on the account "Deposit"
    When I archive the category "Savings"
    Then the categories shown in the current budget period should be exactly these, in this order:
      | category | budget | spent | remaining | over budget | accumulated |
      | Savings  | 200.00 | 50.00 | 150.00    | no          | 150.00      |
    When I point at the "Savings" slice in the ring for the current budget period
    Then the slice pointed at should show these figures:
      | category | budget | spent | remaining | over budget | accumulated |
      | Savings  | 200.00 | 50.00 | 150.00    | no          | 150.00      |
    And the slice pointed at should show that its category is archived

  # Ruled at the build (see the header). Savings is archived with 200 built up for it, and has no
  # budget and no expense in the period that begins. Stepping forward puts that period on screen. Once
  # it is unbacked, nothing is built up for it any more, and it is not shown there.
  #
  # Revised for increment 15 (ruling 6). Setting it to "—" now returns only this period's money, and it
  # has none, so nothing moves. The 200 from the previous period stays on Deposit, still Savings', and the
  # row stays, now saying where the money is. Only once nothing is left for it, here by moving the 200 to
  # Deposit's Unclaimed (reallocate-an-amount.feature), is it no longer shown.
  Scenario: An archived backed category with money built up is shown in the current period and later ones, until nothing is left for it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And I have archived the category "Savings"
    When the next budget period begins while MoneyBud is open
    And I step forward one budget period
    Then the categories shown in the current budget period should be exactly these, in this order:
      | category | budget | accumulated |
      | Savings  | 0.00   | 200.00      |
    And the categories shown in the next budget period should be exactly these, in this order:
      | category | budget | accumulated |
      | Savings  | 0.00   | 200.00      |
    When I remove the backing of "Savings"
    Then I should be told that "Savings" is no longer backed, and of no money moved
    And the categories shown in the current budget period should be exactly these, in this order:
      | category | budget | accumulated | accumulated on |
      | Savings  | 0.00   | 200.00      | Deposit        |
    And the categories shown in the next budget period should be exactly these, in this order:
      | category | budget | accumulated | accumulated on |
      | Savings  | 0.00   | 200.00      | Deposit        |
    When I reallocate 200 euro from "Savings" to Unclaimed on "Deposit"
    Then "Savings" should not be shown in the current budget period
    And "Savings" should not be shown in the next budget period

  # ----------------------------------------------------------------------------------
  # A category on "—" with money left behind (new in increment 15)
  #
  # Setting Staat op to "—" leaves the money from earlier periods, and money given a purpose from
  # Unclaimed, where it is (back-a-category.feature). It is still the category's Accumulated, so its row
  # shows it, with the account it is on, since "—" no longer says where: "Opgebouwd € 5.000,00 op
  # Spaarrekening" (follow-up of 2026-10-04, over counting it only as claimed on the account).
  # ----------------------------------------------------------------------------------

  # The 5000 was given to Savings from Deposit's Unclaimed, so it is not this period's money, and stays.
  # This period's 100 goes back to Bank. That the slice shows it too is this file's reading (header).
  Scenario: A category on none shows the money it left behind, with the account it is on, on its row and in its slice
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have reallocated 5000 euro from Unclaimed on "Deposit" to "Savings"
    And I have a budget of 100 euro for "Savings" in the current budget period
    When I remove the backing of "Savings"
    Then the categories shown in the current budget period should be exactly these, in this order:
      | category | budget | spent | remaining | accumulated | accumulated on |
      | Savings  | 100.00 | 0.00  | 100.00    | 5000.00     | Deposit        |
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 2000.00 | 0.00      |
      | Deposit | 5000.00 | 0.00      |
    When I point at the "Savings" slice in the ring for the current budget period
    Then the slice pointed at should show these figures:
      | category | budget | spent | remaining | over budget | accumulated | accumulated on |
      | Savings  | 100.00 | 0.00  | 100.00    | no          | 5000.00     | Deposit        |

  # Follow-ups of 2026-10-04. Cadeau is this period's, paid from Bank, the pool account, which a category
  # without an account defaults to. It counts in this period's Remaining, and lowering the 5000 as well
  # would count it twice: to spend from the 5000, it is moved to Unassigned first. Assigning to Savings
  # moves nothing, as for any category without an account.
  Scenario: Spending against a category on none, or assigning to it, does not change what it left behind
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have reallocated 5000 euro from Unclaimed on "Deposit" to "Savings"
    And I have a budget of 100 euro for "Savings" in the current budget period
    And I have removed the backing of "Savings"
    When I record an expense of 50 euro for "Savings" labelled "Cadeau"
    And I assign 200 euro to "Savings" in the current budget period
    Then the remaining "Savings" budget in the current budget period should be 250 euro
    And Accumulated for "Savings" in the current budget period should still be 5000 euro, on "Deposit"
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 1950.00 | 0.00      |
      | Deposit | 5000.00 | 0.00      |
