# Seeing Unclaimed: the money on an account that no category claims (glossary: Unclaimed; "Vrij, and
# moving Opgebouwd", "Vrij: the money on an account that no category claims", "A fall in value makes Vrij
# negative", "Gains come in as a balance correction", "Vrij on the pool account: ruled after the install";
# settled by the stakeholder on 2026-10-04 and 2026-10-05). Giving that money a purpose, and moving it on,
# is in reallocate-an-amount.feature. What the strip shows otherwise is in show-accounts.feature.
#
# WRITTEN FOR INCREMENT 15, 2026-10-04. APPROVED at the scenario gate on 2026-10-04.
# REVISED 2026-10-05 for ruling 5 revised; gates waived by Axel, presented with the plan and the app.
#
# WHAT THE REVISION OF 2026-10-05 CHANGES, AND WHY. Ruling 5 said the pool account shows no Unclaimed,
# because there the period's Unassigned plays that role. Trying the increment on his phone showed it was
# wrong: Unassigned only ever holds income, so money on the pool account that came from anywhere else, a
# starting balance or a balance correction, could never be given a purpose. Now THE POOL ACCOUNT SHOWS
# UNCLAIMED TOO, and it is an end in a reallocation both ways (reallocate-an-amount.feature). His two uses:
# giving money already on the pool account a purpose without transferring it away and back, and covering a
# lower balance correction there. So:
#   - Revised: "Every account but the pool account shows ..." (now every account, the pool account
#     included); "The pool account shows no Unclaimed, and making another account the pool ..." (now what
#     making another account the pool does to the claims, derived f); "A first start's only account ..."
#     (shows 0.00). Every accounts table here now gives the pool account a figure.
#   - New: the pool account's own money from before MoneyBud; an income dated later in the period (b); an
#     income on another account (c); an expense on a category without an account paid from another account
#     (d); a period's end (e), with and without a sweep destination; a late change to a swept period (ruled
#     2026-10-05); taking a backed category's budget back (g).
#
# The rules, from arc42 §12:
#   - EVERY ACCOUNT SHOWS UNCLAIMED, on screen "Vrij": the money on it that no category claims. It is today's
#     balance minus what the categories the account backs have on it (ruling 1), and minus what a category
#     set to "—" left on it. On screen: "Saldo € 5.200,00 · Vrij € 5.000,00". Rejected as names: "Niet
#     toegewezen", his own word in the wish, because it is the period's figure; and "Zonder doel".
#   - ON THE POOL ACCOUNT, THIS PERIOD'S PLAN CLAIMS MONEY TOO (ruling 5, revised 2026-10-04). The pool
#     account's Unclaimed is its balance, minus this period's Unassigned, minus the Remaining of this
#     period's categories without an account, minus the Accumulated of the categories it backs (and what a
#     category on "—" left on it). That money is on the pool account and already has a purpose in the plan.
#     With the salary on the pool account and all of it planned or still unassigned, that is 0.00.
#     (First ruled, 2026-10-04: THE POOL ACCOUNT SHOWS NO UNCLAIMED, the period's Unassigned playing that
#     role. Revised for the reason above.)
#   - WHAT AN ENDED PERIOD'S SWEEP LINE STILL ASKS FOR STAYS CLAIMED ON THE POOL ACCOUNT, NOT UNCLAIMED
#     (ruled 2026-10-05). "Still to sweep" is a claim, and "swept too much", while some move can still be
#     undone, a negative one, until "Restant bijwerken" moves it or lets it go. So a late income or receipt
#     dated in an ended period leaves the pool account's Unclaimed where it was, and so does bringing that
#     period up to date. WITH NO SWEEP DESTINATION, AN ENDED PERIOD'S LEFTOVER STAYS ITS LINE'S, and does not
#     become Unclaimed.
#   - A STARTING BALANCE, A TRANSFER IN AND A BALANCE CORRECTION UPWARDS LAND IN IT BY THEMSELVES
#     (ruling 1). EVERYTHING THAT CHANGES THE BALANCE AND NO CATEGORY'S CLAIM LANDS IN IT: a transfer in or
#     out, a balance correction up or down, an income on the account, and an expense on the account against
#     a category it does not back. AN EXPENSE AGAINST A CATEGORY IT BACKS lowers the balance and that
#     category's claim alike, and leaves Unclaimed as it was (derived). On the pool account, the claims of
#     the period's plan follow the same logic (the derivations below).
#   - A FALL IN VALUE MAKES IT NEGATIVE (ruling 3): a lower balance correction can take an account below
#     what its categories claim. It is then shown with the one marker and the badge "Rood", and IT IS
#     NEVER ADJUSTED BY ITSELF: the categories keep their Accumulated until I move it (his words: "there is
#     no automatic system"). Rejected: Accumulated falling by itself. Exactly zero carries no marker, the
#     line every marker draws.
#   - GAINS COME IN AS A BALANCE CORRECTION, into Unclaimed, and he shares them out from there (ruling 4).
#     That is his usage, not a rule: an income can still be recorded on any account.
#   - Since a backed category's expense is always on its account (spend-against-a-backed-category.feature),
#     UNCLAIMED PLUS THE ACCUMULATED OF THE ACCOUNT'S CATEGORIES IS ITS BALANCE, in the current period, on
#     every account but the pool account, apart from data kept before this increment (keep-data.feature).
#     On the pool account, the period's claims and the sweep lines' claims are added to them.
#
# DERIVED BY THE DOCUMENTATION AND NOT PUT TO THE STAKEHOLDER, marked where a scenario rests on one:
#   - IT IS TODAY'S, like the balance, and THE SAME IN EVERY PERIOD ON SCREEN: it sits in the strip, which
#     does not step with the period. A FUTURE-DATED INCOME on an account other than the pool reaches it on
#     its date, as it reaches the balance.
#   - (a, 2026-10-05) "THIS PERIOD" IN THE POOL ACCOUNT'S CLAIMS IS THE CURRENT PERIOD, whatever period is
#     on screen.
#   - (b, 2026-10-05) AN INCOME DATED LATER IN THE CURRENT PERIOD counts in Unassigned at once, as it always
#     has, but is left out of the pool account's claim until its date, since it is not on the account yet.
#     So the pool account's Unclaimed does not change when it is recorded, when it is assigned, or when its
#     date arrives.
#   - (c, 2026-10-05) AN INCOME RECORDED ON ANOTHER ACCOUNT counts in Unassigned, which is claimed on the
#     pool account. So the pool account's Unclaimed is lower by it, and that account's higher, until a
#     transfer to the pool account squares both. In total nothing is Unclaimed twice.
#   - (d, 2026-10-05) AN EXPENSE ON A CATEGORY WITHOUT AN ACCOUNT, PAID FROM ANOTHER ACCOUNT, lowers its
#     Remaining, and so the claim on the pool account: the pool account's Unclaimed rises by it, and that
#     account's falls. A transfer from the pool account squares both.
#   - (e, 2026-10-05) AT A PERIOD'S END, a leftover that is swept leaves the pool account with its claim, so
#     its Unclaimed does not change. With no destination the leftover stays claimed by its line (ruled
#     above), so it does not change either. A leftover below zero is not swept, and nothing asks for it, so
#     at the period's end the pool account's Unclaimed falls by the shortfall.
#   - (f, 2026-10-05) MAKING ANOTHER ACCOUNT THE POOL ACCOUNT MOVES THE CLAIMS OF UNASSIGNED AND OF THE
#     REMAINING OF THE CATEGORIES WITHOUT AN ACCOUNT WITH IT. The new pool account's Unclaimed may go below
#     zero, until the money is transferred to it; the old one's rises by as much.
#     (First derived, 2026-10-04: the old pool account starts to show Unclaimed, and the new one stops.)
#   - (g, 2026-10-05) A NEGATIVE ASSIGNMENT TO A BACKED CATEGORY still moves back at most what is there for
#     it (assign-to-a-backed-category.feature), but all of it joins Unassigned. The pool account's Unclaimed
#     falls by what could not come back. Shown so that it is approved knowingly.
#   - Money a category set to "—" left on the account counts as claimed, by that category
#     (back-a-category.feature).
#   - Derivations h and i, about moving money out of Unassigned and what the form offers, are in
#     reallocate-an-amount.feature.
#
# THE KNOWN CORNER, NOW RIGHT IN TOTAL (2026-10-05). An income recorded on an account other than the pool
# account counts in its period's Unassigned, as approved. Until this revision it also counted in that
# account's Unclaimed, so the same euros could be given a purpose twice (follow-up of 2026-10-04, left as it
# was). Now Unassigned is claimed on the pool account, so the pool account's Unclaimed falls by what that
# account's rises (derived c): the two together are right, and each is right once the money is transferred
# to the pool account. He does not record income there (ruling 4).
#
# Reading the steps:
#   - The English term "Unclaimed" is the documentation's proposal for "Vrij", approved with it, like
#     "Reallocate" for "Verplaatsen". The steps use it as they use "Accumulated" for "Opgebouwd".
#   - show-accounts.feature's "the accounts should be exactly these, in this order" gains the column
#     UNCLAIMED: the Unclaimed the strip shows for that account. Every account shows one now, so EVERY CELL
#     CARRIES A FIGURE. (Until 2026-10-05 a blank cell meant the strip shows none, which is how the pool
#     account was checked. A blank cell means nothing now, and is not accepted.) UNCLAIMED MARKED "yes" means
#     that figure carries the marker, badge "Rood". A table without these columns does not check them, which
#     is how every earlier file stays as it was.
#   - "the Unclaimed of "X" should be N euro" is the same figure for one account. "the Unclaimed of "X" is N
#     euro", as a Given, SETS NOTHING: it states what the Givens above it come to, like "the balance of "X" is
#     N euro". (""X" should show no Unclaimed" is gone with the revision: every account shows one.)
#   - "the Unclaimed of "X" should (not) be marked below zero, with the marker a category over budget has
#     and the badge "Rood"" is show-accounts.feature's marker step, for this figure.
#   - The other steps are explained in show-accounts.feature, back-a-category.feature,
#     sweep-at-a-period-end.feature, bring-a-swept-period-up-to-date.feature and
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
  # and 200 was assigned to Savings since, which moved there.
  #
  # Revised 2026-10-05 (ruling 5 revised). It said Bank, the pool account, shows none. Now it shows its
  # Unclaimed too: its 1800 is all this period's Unassigned, which the plan claims, so none of it is Unclaimed.
  Scenario: Every account shows what no category claims, beside its balance, the pool account included
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 5000 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    When I assign 200 euro to "Savings" in the current budget period
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 1800.00 | 0.00      |
      | Deposit | 5200.00 | 5000.00   |
    And Unassigned in the current budget period should be 1800 euro
    And Accumulated for "Savings" in the current budget period should be 200 euro

  # Deposit starts with 1000 of its own, and 200 assigned to Savings. Each row is one act. Everything that
  # changes the balance and no category's claim lands in Unclaimed. An expense against Savings, which
  # Deposit backs, comes off Savings' Accumulated and leaves Unclaimed alone. Only Deposit is checked here.
  # (Revised 2026-10-05, comment only: the income row also joins this period's Unassigned, which is claimed
  # on Bank, so Bank's Unclaimed falls by 60; that is derived c, and has its own scenario below.)
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
  # Deposit is not overdrawn. (Revised 2026-10-05: Bank, the pool account, now shows 0.00, where it showed
  # none; its 1800 is this period's Unassigned.)
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
      | Bank    | 1800.00   | no        | 0.00        | no               |
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
  # The pool account's Unclaimed (revised 2026-10-05)
  #
  # Ruling 5 revised. On the pool account, this period's plan claims money too: its Unassigned, and the
  # Remaining of its categories without an account, are on the pool account and already have a purpose.
  # What is left is Unclaimed, as on any account.
  # ----------------------------------------------------------------------------------

  # New 2026-10-05: his first use. Bank held 500 of my own before the salary came in, which the bank's
  # figure shows: I correct the balance to 2500. Unassigned holds only income, 2000, so the 500 is
  # Unclaimed. Assigning to Groceries, which has no account, and spending there, moves money between
  # Unassigned, Groceries' Remaining and the shop, and never into or out of the 500.
  Scenario: Money the pool account held before MoneyBud shows as its Unclaimed, and planning and spending the period's money leave it as it is
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have a category "Groceries"
    When I correct the balance of "Bank" to 2500 euro
    Then the Unclaimed of "Bank" should be 500 euro
    And Unassigned in the current budget period should still be 2000 euro
    When I assign 400 euro to "Groceries" in the current budget period
    And I record an expense of 150 euro for "Groceries" labelled "Markt"
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 2350.00 | 500.00    |
    And Unassigned in the current budget period should be 1600 euro
    And the remaining "Groceries" budget in the current budget period should be 250 euro

  # Revised 2026-10-05 (derived f). It said the pool account shows no Unclaimed, and that making Deposit
  # the pool account makes Bank show some and Deposit none. Now both show one before and after.
  #
  # Savings is backed by Bank, the pool account, so its 300 never left Bank. Groceries has no account: 200
  # assigned, 50 spent from Bank, 150 Remaining. Bank's 1950 is Savings' 300, Unassigned's 1500 and
  # Groceries' 150: 0.00 Unclaimed. Once Deposit is the pool account, the claims of Unassigned and
  # Groceries go with it, and Savings' stays on Bank, which backs it. So Deposit's Unclaimed is 500 less
  # 1650, below zero with the marker, and Bank's rises by as much, to 1650. Transferring the 1650 to Deposit,
  # where the plan now expects it, squares both.
  Scenario: Making another account the pool account moves the claims of Unassigned and of the categories without an account with it
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 500 euro
    And I have a category "Savings"
    And I have a category "Groceries"
    And I have set the backing account of "Savings" to "Bank"
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have a budget of 200 euro for "Groceries" in the current budget period
    And I have already spent 50 euro on "Groceries" in the current budget period
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed | unclaimed marked |
      | Bank    | 1950.00 | 0.00      | no               |
      | Deposit | 500.00  | 500.00    | no               |
    When I make "Deposit" the pool account
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed | unclaimed marked |
      | Deposit | 500.00  | -1150.00  | yes              |
      | Bank    | 1950.00 | 1650.00   | no               |
    When I record a transfer of 1650 euro from "Bank" to "Deposit"
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed | unclaimed marked |
      | Deposit | 2150.00 | 500.00    | no               |
      | Bank    | 300.00  | 0.00      | no               |
    And Accumulated for "Savings" in the current budget period should still be 300 euro

  # Revised 2026-10-05: it said the first start's account shows no Unclaimed. It shows 0.00: nothing is on
  # it, and nothing is planned.
  Scenario: A first start's only account is the pool account, and its Unclaimed is zero
    Given I have just started using MoneyBud for the first time
    Then the accounts should be exactly these, in this order:
      | account        | balance | unclaimed |
      | Betaalrekening | 0.00    | 0.00      |
    And the Unclaimed of "Betaalrekening" should not be marked below zero

  # New 2026-10-05 (derived c): the known corner in the header, made right in total. Interest of 100 is
  # recorded on Deposit. It counts in this period's Unassigned, as every income does, and Unassigned is
  # claimed on Bank, where the 100 is not. So Bank's Unclaimed falls 100 below zero and Deposit's rises by
  # 100: together they are right. A transfer of the 100 to Bank, where Unassigned money is, squares both.
  Scenario: An income on another account counts in Unassigned, so the pool account's Unclaimed falls by it and that account's rises, until a transfer squares both
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    When I record an income of 100 euro labelled "Rente" on the account "Deposit"
    Then Unassigned in the current budget period should be 2100 euro
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed | unclaimed marked |
      | Bank    | 2000.00 | -100.00   | yes              |
      | Deposit | 100.00  | 100.00    | no               |
    When I record a transfer of 100 euro from "Deposit" to "Bank"
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed | unclaimed marked |
      | Bank    | 2100.00 | 0.00      | no               |
      | Deposit | 0.00    | 0.00      | no               |

  # New 2026-10-05 (derived d). Groceries has no account, so its 400 is claimed on Bank. 50 of it is paid
  # at the market from Deposit: Groceries' Remaining falls by 50, but Bank's balance does not, so Bank's
  # Unclaimed rises by 50, and Deposit's falls by 50 with its balance. A transfer of 50 from Bank to
  # Deposit, as I would make at the bank, squares both.
  Scenario: An expense on a category without an account paid from another account raises the pool account's Unclaimed and lowers that account's, until a transfer squares both
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 500 euro
    And I have a category "Groceries"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    When I record an expense of 50 euro for "Groceries" labelled "Markt" on the account "Deposit"
    Then the remaining "Groceries" budget in the current budget period should be 350 euro
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 2000.00 | 50.00     |
      | Deposit | 450.00  | 450.00    |
    When I record a transfer of 50 euro from "Bank" to "Deposit"
    Then the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 1950.00 | 0.00      |
      | Deposit | 500.00  | 500.00    |

  # New 2026-10-05 (derived g), shown so that it is approved knowingly. 300 was assigned to Savings and
  # moved to Deposit, and Savings spent from Deposit. Taking 100 of the budget back always adds 100 to
  # Unassigned, which is claimed on Bank, but moves back only what is still there for Savings, at most 100
  # (assign-to-a-backed-category.feature). What cannot come back was already spent from Deposit, so Bank's
  # Unclaimed falls below zero by that gap, with the marker: Unassigned now plans money Bank does not hold.
  Scenario Outline: Taking a backed category's budget back adds all of it to Unassigned, and what cannot come back lowers the pool account's Unclaimed
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 300 euro for "Savings" in the current budget period
    And I have recorded an expense of <spent> euro for "Savings" labelled "Fiets" dated today on the account "Deposit"
    When I assign -100 euro to "Savings" in the current budget period
    Then Unassigned in the current budget period should be 1800 euro
    And the accounts should be exactly these, in this order:
      | account | balance   | unclaimed   | unclaimed marked |
      | Bank    | <bank>    | <unclaimed> | <marked>         |
      | Deposit | <deposit> | 0.00        | no               |

    Examples:
      | spent  | bank    | deposit | unclaimed | marked |
      | 100.00 | 1800.00 | 100.00  | 0.00      | no     |
      | 250.00 | 1750.00 | 0.00    | -50.00    | yes    |
      | 300.00 | 1700.00 | 0.00    | -100.00   | yes    |

  # ----------------------------------------------------------------------------------
  # Unclaimed is today's
  #
  # Derived: like the balance, it is the same whichever period is on screen, and an income dated ahead
  # on an account other than the pool reaches it on its date. (Revised 2026-10-05, comment only: the
  # income on Deposit is dated in the next period, so it is not in this period's Unassigned, and Bank's
  # Unclaimed is not touched by it.)
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

  # New 2026-10-05 (derived b). The bonus is dated tomorrow, the last day of this period. It counts in
  # Unassigned at once, as any income does, but it is not on Bank until its date, so the plan's claim
  # leaves it out until then. Bank's Unclaimed does not move when it is recorded, when it is assigned, or
  # when its date arrives.
  Scenario: An income dated later in the current period leaves the pool account's Unclaimed as it is, before and on its date
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have a category "Groceries"
    And the Unclaimed of "Bank" is 0.00 euro
    When I record an income of 300 euro labelled "Bonus" dated 30 September 2026
    Then Unassigned in the current budget period should be 2300 euro
    And the balance of "Bank" should still be 2000 euro
    And the Unclaimed of "Bank" should still be 0.00 euro
    When I assign 300 euro to "Groceries" in the current budget period
    Then the Unclaimed of "Bank" should still be 0.00 euro
    When the day becomes 30 September 2026 while MoneyBud is open
    Then the balance of "Bank" should be 2300 euro
    And the Unclaimed of "Bank" should still be 0.00 euro

  # ----------------------------------------------------------------------------------
  # At a period's end, and after it (new 2026-10-05)
  # ----------------------------------------------------------------------------------

  # Derived e. Groceries has no account. Before the end the plan claims the period's leftover on Bank:
  # Unassigned's 600 plus Groceries' Remaining, which is exactly what Bank holds. At the end:
  #   - 100 spent: the leftover, 900, is swept to Savings on Deposit, and takes its claim along. Bank's
  #     Unclaimed stays 0.00.
  #   - 1000 spent: the leftover is exactly zero. Nothing is swept, and nothing was claimed.
  #   - 1000.01 spent: the leftover is one cent below zero, an overspending paid from Bank. It is not swept,
  #     and nothing asks for it any more, so from the end Bank's Unclaimed shows it, with the marker.
  Scenario Outline: At a period's end, a leftover swept leaves the pool account's Unclaimed as it was, and a leftover below zero lowers it by the shortfall
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have a category "Groceries"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent <spent> euro on "Groceries" in the current budget period
    And the Unclaimed of "Bank" is 0.00 euro
    When the next budget period begins while MoneyBud is open
    Then the accounts should be exactly these, in this order:
      | account | balance   | unclaimed   | unclaimed marked |
      | Bank    | <bank>    | <unclaimed> | <marked>         |
      | Deposit | <deposit> | 0.00        | no               |

    Examples:
      | spent   | bank  | deposit | unclaimed | marked |
      | 100.00  | 0.00  | 900.00  | 0.00      | no     |
      | 1000.00 | 0.00  | 0.00    | 0.00      | no     |
      | 1000.01 | -0.01 | 0.00    | -0.01     | yes    |

  # Ruled 2026-10-05. With no sweep destination nothing is swept, and the ended period's line asks for its
  # 900 still to be swept (sweep-at-a-period-end.feature). That 900 stays the line's: claimed on Bank, not
  # Unclaimed. Once a destination is chosen, bringing the period up to date moves it, and Bank's Unclaimed
  # still does not change.
  Scenario: With no sweep destination, an ended period's leftover stays claimed by its line, and does not become the pool account's Unclaimed
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have a category "Groceries"
    And I have set the backing account of "Savings" to "Deposit"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 100 euro on "Groceries" in the current budget period
    When the next budget period begins while MoneyBud is open
    Then the previous budget period should show 900 euro of its period leftover still to sweep
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 900.00  | 0.00      |
      | Deposit | 0.00    | 0.00      |
    When I set the sweep destination to "Savings"
    And I bring the swept amount of the previous budget period up to date
    Then I should be told that 900 euro more of the period leftover of the previous budget period was swept into "Savings"
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | 0.00    | 0.00      |
      | Deposit | 900.00  | 0.00      |

  # Ruled 2026-10-05. The period was swept: 1900 went from Bank to Savings on Deposit. Then a refund, or a
  # receipt, dated in that period turns up. Bank's balance changes, and the period's line asks for the
  # difference: still to sweep, or swept too much (bring-a-swept-period-up-to-date.feature). Until
  # "Restant bijwerken" moves it, that difference is the line's, so Bank's Unclaimed stays 0.00, before and
  # after the button.
  Scenario Outline: A late change to a swept period leaves the pool account's Unclaimed where it was, and so does bringing the period up to date
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated today
    When the next budget period begins while MoneyBud is open
    And <late change>
    Then <line>
    And the accounts should be exactly these, in this order:
      | account | balance | unclaimed |
      | Bank    | <bank>  | 0.00      |
      | Deposit | 1900.00 | 0.00      |
    When I bring the swept amount of the previous budget period up to date
    Then the accounts should be exactly these, in this order:
      | account | balance   | unclaimed |
      | Bank    | 0.00      | 0.00      |
      | Deposit | <deposit> | 0.00      |

    Examples:
      | late change                                                                                                       | line                                                                                  | bank   | deposit |
      | I record an income of 100 euro labelled "Terugbetaling" dated on the last day of the previous budget period       | the previous budget period should show 100 euro of its period leftover still to sweep | 100.00 | 2000.00 |
      | I record an expense of 40 euro for "Groceries" labelled "Bon" dated on the last day of the previous budget period | the previous budget period should show 40 euro of its period leftover swept too much  | -40.00 | 1860.00 |
