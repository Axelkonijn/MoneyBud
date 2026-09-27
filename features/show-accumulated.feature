# Seeing Accumulated: what has been built up for a backed category, on its row and in its slice of
# the ring (glossary: Accumulated; "Backed categories accumulate", "Accumulated covers everything up
# to the period on screen", "On screen: Staat op and Opgebouwd"; "Backing and Accumulated", settled
# by the stakeholder on 2026-09-27). What moves money in and out, and so what Accumulated counts, is
# in back-a-category.feature, assign-to-a-backed-category.feature and
# spend-against-a-backed-category.feature. The steps the backing files share are explained in
# back-a-category.feature.
#
# The rules, from arc42 §12:
#   - ACCUMULATED, on screen "Opgebouwd", IS WHAT HAS MOVED IN ON THE CATEGORY'S BEHALF SINCE IT WAS
#     LAST BACKED, MINUS WHAT HAS BEEN SPENT AGAINST IT SINCE, on any account. "Moved in" is net: a
#     negative assignment moves money back out. The move made when the category is backed counts as
#     moved in.
#   - A BACKED ROW SHOWS "Opgebouwd: € 600,00" UNDER ITS FIGURES, AND SO DO ITS SLICE'S HOVER DETAILS.
#     AN UNBACKED CATEGORY SHOWS NONE, not a zero.
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
#     spending put on another account, and by the period on screen.
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
  # money spent down to nothing, and carries no marker, the line every marker draws.
  # ----------------------------------------------------------------------------------

  # Paid from Bank, so Deposit keeps the 100 and is not overdrawn. The two markers are separate:
  # the budget's Remaining is over budget, and Accumulated is below zero.
  Scenario Outline: Accumulated below zero carries the marker with the badge "Rood", and exactly zero does not
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 100 euro for "Savings" in the current budget period
    When I record an expense of <spent> euro for "Savings" labelled "Fiets" on the account "Bank"
    Then I should not be warned or asked to confirm
    And the categories shown in the current budget period should be exactly these, in this order:
      | category | budget | spent   | remaining   | over budget | accumulated   | accumulated marked |
      | Savings  | 100.00 | <spent> | <remaining> | <over>      | <accumulated> | <marked>           |
    And the balance of "Deposit" should still be 100 euro
    And "Deposit" should not be shown as overdrawn

    Examples:
      | spent  | remaining | over | accumulated | marked |
      | 100.00 | 0.00      | no   | 0.00        | no     |
      | 100.01 | -0.01     | yes  | -0.01       | yes    |

  # The glossary's example: 350 spent of 300, so nothing moves at the backing and Accumulated starts
  # at zero. 30 more spent takes it below zero. 100 assigned moves 100 and brings it back to 70, while
  # the budget's Remaining is 20.
  Scenario: Backing an overspent category and spending against it takes Accumulated below zero, and assigning brings it back
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have already spent 350 euro on "Savings" in the current budget period
    And I have set the backing account of "Savings" to "Deposit"
    When I record an expense of 30 euro for "Savings" labelled "Kado" on the account "Bank"
    Then Accumulated for "Savings" in the current budget period should be -30 euro
    And Accumulated for "Savings" in the current budget period should be marked below zero, with the marker a category over budget has and the badge "Rood"
    When I assign 100 euro to "Savings" in the current budget period
    Then Accumulated for "Savings" in the current budget period should be 70 euro
    And Accumulated for "Savings" in the current budget period should not be marked below zero
    And the remaining "Savings" budget in the current budget period should be 20 euro

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
  Scenario: An archived backed category with money built up is shown in the current period and later ones, until it is unbacked
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
    Then I should be told that "Savings" is no longer backed, and that 200 euro moved from "Deposit" to "Bank"
    And "Savings" should not be shown in the current budget period
    And "Savings" should not be shown in the next budget period
