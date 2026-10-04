# Seeing Unclaimed: the money on an account that no category claims (glossary: Unclaimed; "Vrij, and
# moving Opgebouwd", "Vrij: the money on an account that no category claims", "A fall in value makes Vrij
# negative", "Gains come in as a balance correction", "No Vrij on the pool account"; settled by the
# stakeholder on 2026-10-04). Giving that money a purpose, and moving it on, is in
# reallocate-an-amount.feature. What the strip shows otherwise is in show-accounts.feature.
#
# WRITTEN FOR INCREMENT 15, 2026-10-04. NOT YET APPROVED at the scenario gate.
#
# The rules, from arc42 §12:
#   - EVERY ACCOUNT EXCEPT THE POOL ACCOUNT SHOWS UNCLAIMED, on screen "Vrij": the money on it that no
#     category claims. It is today's balance minus what the categories the account backs have on it
#     (ruling 1). On screen: "Saldo € 5.200,00 · Vrij € 5.000,00". Rejected as names: "Niet toegewezen",
#     his own word in the wish, because it is the period's figure; and "Zonder doel".
#   - A STARTING BALANCE, A TRANSFER IN AND A BALANCE CORRECTION UPWARDS LAND IN IT BY THEMSELVES
#     (ruling 1). EVERYTHING THAT CHANGES THE BALANCE AND NO CATEGORY'S CLAIM LANDS IN IT: a transfer in or
#     out, a balance correction up or down, an income on the account, and an expense on the account against
#     a category it does not back. AN EXPENSE AGAINST A CATEGORY IT BACKS lowers the balance and that
#     category's claim alike, and leaves Unclaimed as it was (derived).
#   - A FALL IN VALUE MAKES IT NEGATIVE (ruling 3): a lower balance correction can take an account below
#     what its categories claim. It is then shown with the one marker and the badge "Rood", and IT IS
#     NEVER ADJUSTED BY ITSELF: the categories keep their Accumulated until I move it (his words: "there is
#     no automatic system"). Rejected: Accumulated falling by itself. Exactly zero carries no marker, the
#     line every marker draws.
#   - GAINS COME IN AS A BALANCE CORRECTION, into Unclaimed, and he shares them out from there (ruling 4).
#     That is his usage, not a rule: an income can still be recorded on any account.
#   - THE POOL ACCOUNT SHOWS NO UNCLAIMED (ruling 5): there, the period's Unassigned plays that role.
#     Recorded in the glossary as ruled with a weaker confirmation than the others.
#   - Since a backed category's expense is always on its account (spend-against-a-backed-category.feature),
#     UNCLAIMED PLUS THE ACCUMULATED OF THE ACCOUNT'S CATEGORIES IS ITS BALANCE, in the current period,
#     apart from data kept before this increment (keep-data.feature).
#
# DERIVED BY THE DOCUMENTATION AND NOT PUT TO THE STAKEHOLDER, marked where a scenario rests on one, so
# that they are approved or contradicted at this gate:
#   - IT IS TODAY'S, like the balance, and THE SAME IN EVERY PERIOD ON SCREEN: it sits in the strip, which
#     does not step with the period. A FUTURE-DATED INCOME reaches it on its date, as it reaches the balance.
#   - MAKING ANOTHER ACCOUNT THE POOL MOVES UNCLAIMED WITH IT: the old pool account starts to show one,
#     which may be most of its balance, and the new one stops.
#   - Money a category set to "—" left on the account counts as claimed, by that category
#     (back-a-category.feature).
#
# KNOWN, AND LEFT AS IT IS (follow-up of 2026-10-04, on the recommendation): an income recorded on an
# account other than the pool account counts in its period's Unassigned, as approved, AND in that account's
# Unclaimed. So the same euros could be given a purpose twice. He does not record income there (ruling 4).
# One row of the second outline below records such an income; it checks only the strip.
#
# Reading the steps:
#   - The English term "Unclaimed" is the documentation's proposal for "Vrij", open at this gate, like
#     "Reallocate" for "Verplaatsen". The steps use it as they use "Accumulated" for "Opgebouwd".
#   - show-accounts.feature's "the accounts should be exactly these, in this order" gains the column
#     UNCLAIMED: the Unclaimed the strip shows for that account, and a BLANK cell means it shows none, which
#     is how the pool account is checked. UNCLAIMED MARKED "yes" means that figure carries the marker, badge
#     "Rood". A table without these columns does not check them, which is how every earlier file stays as
#     it was.
#   - "the Unclaimed of "X" should be N euro" is the same figure for one account. ""X" should show no
#     Unclaimed" means the strip shows none for X at all, not a zero. "the Unclaimed of "X" is N euro", as
#     a Given, SETS NOTHING: it states what the Givens above it come to, like "the balance of "X" is N euro".
#   - "the Unclaimed of "X" should (not) be marked below zero, with the marker a category over budget has
#     and the badge "Rood"" is show-accounts.feature's marker step, for this figure.
#   - The other steps are explained in show-accounts.feature, back-a-category.feature and
#     reallocate-an-amount.feature, or reused unchanged from the file that introduced them.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature), except the one about a first start. The
# names, labels and amounts are synthetic test data.

@unclaimed
Feature: See the money on an account that no category claims
  As someone who already had savings and shares before I used MoneyBud
  I want each account to show how much of its money has no purpose yet, beside its balance
  So that the purposes I track add up to what is really in the account, and I can see what is still mine to give a purpose

  # ----------------------------------------------------------------------------------
  # What Unclaimed is
  # ----------------------------------------------------------------------------------

  # The glossary's own example, with synthetic names. 5000 was in Deposit before Savings was backed,
  # and 200 was assigned to Savings since, which moved there. Bank is the pool account, and shows none.
  Scenario: Every account but the pool account shows what no category claims, beside its balance
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I assign 200 euro to "Savings" in the current budget period
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 1800.00 |           |
      | Deposit | 5200.00 | 5000.00   |
    And "Bank" should show no Unclaimed
    And Accumulated for "Savings" in the current budget period should be 200 euro

  # Deposit starts with 1000 of its own, and 200 assigned to Savings. Each row is one act. Everything that
  # changes the balance and no category's claim lands in Unclaimed. An expense against Savings, which
  # Deposit backs, comes off Savings' Accumulated and leaves Unclaimed alone. The income row is the known
  # corner in the header: it also joins this period's Unassigned, which is not checked here.
  Scenario Outline: What changes an account's balance and no category's claim lands in Unclaimed, and an expense against a category it backs does not
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 1000 euro
    And I have a category "Groceries"
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    And the Unclaimed of "Deposit" is 1000 euro
    When <act>
    Then the balance of "Deposit" should be <balance> euro
    And the Unclaimed of "Deposit" should be <unclaimed> euro
    And Accumulated for "Savings" in the current budget period should be <accumulated> euro

    Examples:
      | act                                                                       | balance | unclaimed | accumulated |
      | I record a transfer of 100 euro from "Bank" to "Deposit"                  | 1300.00 | 1100.00   | 200.00      |
      | I record a transfer of 100 euro from "Deposit" to "Bank"                  | 1100.00 | 900.00    | 200.00      |
      | I correct the balance of "Deposit" to 1260 euro                           | 1260.00 | 1060.00   | 200.00      |
      | I correct the balance of "Deposit" to 1150 euro                           | 1150.00 | 950.00    | 200.00      |
      | I record an income of 60 euro labelled "Rente" on the account "Deposit"   | 1260.00 | 1060.00   | 200.00      |
      | I record an expense of 30 euro for "Groceries" labelled "Markt" on the account "Deposit" | 1170.00 | 970.00 | 200.00 |
      | I record an expense of 30 euro for "Savings" labelled "Boek"              | 1170.00 | 1000.00   | 170.00      |

  # ----------------------------------------------------------------------------------
  # A fall in value takes Unclaimed below zero
  #
  # Ruling 3. Deposit holds exactly what Savings has built up, 200. The bank says 199.99: one cent
  # less than Savings claims. Unclaimed shows it, with the marker, and Savings' Accumulated is not touched:
  # I share the loss out myself (reallocate-an-amount.feature). The balance itself is not below zero, so
  # Deposit is not overdrawn.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A balance correction below what the categories claim takes Unclaimed below zero, with the marker and "Rood", and nothing adjusts it
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 200 euro for "Savings" in the current budget period
    When I correct the balance of "Deposit" to <balance> euro
    Then I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance   | overdrawn | unclaimed   | unclaimed marked |
      | Bank    | 1800.00   | no        |             | no               |
      | Deposit | <balance> | no        | <unclaimed> | <marked>         |
    And Accumulated for "Savings" in the current budget period should still be 200 euro
    And Accumulated for "Savings" in the current budget period should not be marked below zero

    Examples:
      | balance | unclaimed | marked |
      | 200.00  | 0.00      | no     |
      | 199.99  | -0.01     | yes    |

  Scenario: Unclaimed below zero carries the marker with the badge "Rood"
    Given I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have reallocated 300 euro from Unclaimed on "Deposit" to "Savings"
    Then the Unclaimed of "Deposit" should be -300 euro
    And the Unclaimed of "Deposit" should be marked below zero, with the marker a category over budget has and the badge "Rood"
    And "Deposit" should not be shown as overdrawn

  # ----------------------------------------------------------------------------------
  # The pool account shows none
  # ----------------------------------------------------------------------------------

  # Ruling 5, and, derived, what follows when the pool account changes. Savings is backed by Bank, the
  # pool account, so the 300 assigned to it never left Bank. Once Deposit is the pool account, Bank shows
  # Unclaimed: its balance less the 300 Savings has on it. Deposit, the new pool account, shows none.
  Scenario: The pool account shows no Unclaimed, and making another account the pool moves Unclaimed with it
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 500 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Bank"
    And I have a budget of 300 euro for "Savings" in the current budget period
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 2000.00 |           |
      | Deposit | 500.00  | 500.00    |
    When I make "Deposit" the pool account
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Deposit | 500.00  |           |
      | Bank    | 2000.00 | 1700.00   |
    And "Deposit" should show no Unclaimed

  Scenario: A first start's only account is the pool account, and shows no Unclaimed
    Given I have just started using MoneyBud for the first time
    Then the accounts should be exactly these, in this order:
      | account        | balance | unclaimed |
      | Betaalrekening | 0.00    |           |
    And "Betaalrekening" should show no Unclaimed

  # ----------------------------------------------------------------------------------
  # Unclaimed is today's
  #
  # Derived: like the balance, it is the same whichever period is on screen, and an income dated ahead
  # reaches it on its date. The income on Deposit is the known corner in the header.
  # ----------------------------------------------------------------------------------

  Scenario: Unclaimed is the same in every period on screen, and an income dated ahead reaches it on its date
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have an account "Deposit" with a starting balance of 1000 euro
    And I have recorded an income of 15 euro labelled "Rente" dated 1 October 2026 on the account "Deposit"
    Then the Unclaimed of "Deposit" should be 1000 euro
    When I step forward one budget period
    Then the Unclaimed of "Deposit" should still be 1000 euro
    When I step back one budget period
    And I step back one budget period
    Then the Unclaimed of "Deposit" should still be 1000 euro
    When the day becomes 1 October 2026 while MoneyBud is open
    Then the balance of "Deposit" should be 1015 euro
    And the Unclaimed of "Deposit" should be 1015 euro
