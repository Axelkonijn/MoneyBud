# Correcting a balance: typing the balance the bank shows today, when MoneyBud's figure has drifted
# (glossary: Balance correction; "A balance is worked out from the entries", "A typed balance is what
# the bank said that day"; "Accounts and net worth", settled by the stakeholder on 2026-09-27). The
# steps the accounts files share are explained in show-accounts.feature.
#
# The rules, from arc42 §12:
#   - A balance is WORKED OUT, never overwritten. Typing the real balance records a BALANCE
#     CORRECTION beside the entries. "Correction" alone means changing or removing an entry in the
#     earlier files. This is a different act, so it always has its full name here.
#   - A balance correction is WHAT THE ACCOUNT REALLY HELD THAT DAY. It is DATED TODAY, whichever
#     period is on screen, so the balance to type is today's (derived).
#   - So it ALREADY HAS IN IT every entry dated BEFORE its day, including one recorded afterwards: a
#     late receipt does not knock a checked balance off again. An entry dated AFTER its day moves the
#     balance as normal. An entry dated ON ITS DAY is in it if it was recorded before the balance
#     correction, and moves the balance if it was recorded after it (derived, the documentation's own
#     tie-break, not put to the stakeholder). A date has no time of day, so on the day itself only
#     the order of recording can tell.
#   - A CHANGED ENTRY KEEPS THE MOMENT IT WAS FIRST RECORDED (derived). Whether it counts as recorded
#     before or after a balance correction never depends on when it was changed, even when the change
#     moves its date onto, or off, the balance correction's day.
#   - A typed STARTING BALANCE is the account's first balance correction (add-an-account.feature).
#     An account with no typed balance at all, like the pool account every scenario starts with, or
#     one added with its starting balance left empty, has as its balance the plain sum of what is on
#     it, whatever the dates, until it is first corrected. An income dated in the future still counts
#     only from its date. A starting balance TYPED AS 0 is a typed balance like any other, and has
#     in it every entry dated before it (add-an-account.feature shows the two side by side).
#   - It is NET WORTH ONLY: not income, in no period's Unassigned, and it changes no budget figure.
#   - It may be ZERO or NEGATIVE (derived). It is whole cents, and typed like any amount.
#   - It CAN BE REMOVED, from the account's history, which ASKS FIRST, because a record is lost
#     (derived, by "confirm only where a record is lost"). It CANNOT BE CHANGED: to change one,
#     correct again (derived). Which rows of a history offer which act is shown in one scenario in
#     show-accounts.feature. After a removal the balance is worked out from the account's previous
#     typed balance, or from none.
#   - Its row in the account's history shows THE NEW BALANCE AND THE DIFFERENCE, for example
#     "Correctie — saldo € 1.000,00 (− € 23,40)". The wording is copy. The difference is the ruling:
#     a balance correction is the only trace of something forgotten.
#   - THE DIFFERENCE IS RECOMPUTED, and always shows what is still unexplained. RULED BY THE
#     STAKEHOLDER AFTER THE GLOSSARY SECTION WAS WRITTEN, and superseding its reading that the
#     difference is fixed when typed. The difference is the typed balance minus what the entries,
#     AS THEY ARE NOW, say the account held just before it: the previous typed balance (or none),
#     plus everything since that the balance correction has in it. So recording a forgotten expense
#     dated before it, changing or removing an entry it has in it, all change the difference and
#     never the balance. The stakeholder's example: MoneyBud had 1023.40, the balance was corrected
#     to 1000 and the history showed -23.40. A forgotten 23.40 expense dated before that day is then
#     recorded: the difference shows 0.00, and the balance stays 1000.
#
# What the rules cost, stated plainly in the glossary and shown below rather than hidden: changing,
# re-pointing or removing an entry dated before an account's latest balance correction does not
# change that account's balance, and an expense dated before one lowers neither the balance nor net
# worth. Two consequences of the derived tie-break are shown too, so that they are approved with
# eyes open: an entry dated on a balance correction's day but recorded the NEXT day moves the balance,
# and changing an entry's date to BEFORE the balance correction's day takes it into the correction.
#
# Reading the steps:
#   - "I correct the balance of "X" to N euro" is the act. "I try to correct" is the same act where
#     the scenario expects a refusal. A balance in quotation marks, with no "euro", is the text as
#     typed (type-an-amount.feature). "the balance correction should be recorded (refused)" says
#     whether it went through.
#   - "I have corrected the balance of "X" to N euro" is the same act done earlier today, at that
#     point in the order of the Givens.
#   - "the balance of "X" is N euro", as a Given, SETS NOTHING. It states what the entries above it
#     add up to, so that the figure before the act is on the page, like assign-to-category.feature's
#     "Unassigned in the current budget period is X euro".
#   - "the balance correction of "X" to N euro" names a balance correction by its account and the
#     balance typed. "... should show a difference of D euro" is the DIFFERENCE its history row shows.
#     As a Given, "... shows a difference of D euro" sets nothing either: it states the difference
#     before the act, so that the change the act makes to it is on the page.
#   - "I remove the balance correction of ... and confirm" (or "but decline to confirm") and "I
#     remove the starting balance of "X" and confirm" are remove-an-entry.feature's grammar. "I
#     should be told that the balance correction (starting balance) was removed" is the message
#     afterwards.
#   - Every other step is reused unchanged from the file that introduced it, the account steps from
#     show-accounts.feature.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). Most fix today as the last day of the
# current budget period, so that "yesterday" and "the first day of the current budget period" are in
# the current period and are different days. The names, labels and amounts are synthetic test data.

@accounts
Feature: Correct a balance
  As someone whose accounts are kept by hand, so that now and then something gets forgotten
  I want to type the balance my bank shows today when MoneyBud's figure has drifted, and see how much was unaccounted for
  So that each balance is right again from today, a receipt I enter late does not knock it off again, and I can still see what was missing

  # ----------------------------------------------------------------------------------
  # Correcting a balance
  # ----------------------------------------------------------------------------------

  Scenario: Correcting a balance sets it to what I type, and the history shows how far off MoneyBud was
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Rent"
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And the balance of "Bank" is 1023.40 euro
    When I correct the balance of "Bank" to 1000 euro
    Then the balance correction should be recorded
    And I should not be warned or asked to confirm
    And the balance of "Bank" should be 1000 euro
    And net worth should be 1000 euro
    And the history of "Bank" should be exactly these, newest first:
      | date      | entry              | category | label   | amount  | balance | difference |
      | today     | balance correction |          |         |         | 1000.00 | -23.40     |
      | yesterday | expense            | Rent     | Huur    | 809.05  |         |            |
      | yesterday | income             |          | Salaris | 1832.45 |         |            |

  # Money I already had, or had not, is not income or spending: up or down, the purpose side does
  # not move.
  Scenario Outline: A balance correction changes the balance and net worth, and no budget figure
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have a budget of 900 euro for "Rent" in the current budget period
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And the balance of "Bank" is 1023.40 euro
    When I correct the balance of "Bank" to <typed> euro
    Then the balance of "Bank" should be <typed> euro
    And net worth should be <typed> euro
    And the balance correction of "Bank" to <typed> euro should show a difference of <difference> euro
    And Unassigned in the current budget period should still be 932.45 euro
    And the budget for "Rent" in the current budget period should still be 900 euro
    And the remaining "Rent" budget in the current budget period should still be 90.95 euro

    Examples:
      | typed   | difference |
      | 1100.00 | 76.60      |
      | 900.00  | -123.40    |

  # Zero is an empty account and below zero an overdrawn one. Typing the figure MoneyBud already has
  # is a check that it is right: it is recorded like any other, with a difference of nothing, and
  # from then on it has in it every entry dated before today. One cent over is one cent unexplained.
  Scenario Outline: A balance may be corrected to zero, to below zero, or to the figure MoneyBud already has
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Rent"
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And the balance of "Bank" is 1023.40 euro
    When I correct the balance of "Bank" to <typed> euro
    Then the balance correction should be recorded
    And the accounts should be exactly these, in this order:
      | account | balance | overdrawn   |
      | Bank    | <typed> | <overdrawn> |
    And net worth should be <typed> euro
    And the balance correction of "Bank" to <typed> euro should show a difference of <difference> euro

    Examples:
      | typed   | difference | overdrawn |
      | 0.00    | -1023.40   | no        |
      | -250.00 | -1273.40   | yes       |
      | 1023.40 | 0.00       | no        |
      | 1023.41 | 0.01       | no        |

  # The date is the day it is typed, not the period on screen. The balances in the strip are the
  # same in every period (show-accounts.feature).
  Scenario: A balance correction is dated today, whichever period is on screen
    Given my budget periods are one month long
    And I have recorded an income of 100 euro labelled "Salaris" dated on the last day of the previous budget period
    And the Overview shows the previous budget period
    When I correct the balance of "Bank" to 90 euro
    Then the balance correction should be recorded
    And the Overview should show the previous budget period
    And the history of "Bank" should be exactly these, newest first:
      | date                                       | entry              | label   | amount | balance | difference |
      | today                                      | balance correction |         |        | 90.00   | -10.00     |
      | the last day of the previous budget period | income             | Salaris | 100.00 |         |            |

  # ----------------------------------------------------------------------------------
  # What a balance correction already has in it
  #
  # Everything dated before its day. So an entry recorded, changed or removed afterwards, but dated
  # before that day, leaves the balance where I put it. It does change the DIFFERENCE, which always
  # shows what is still unexplained (the stakeholder's ruling of 2026-09-27, above).
  # ----------------------------------------------------------------------------------

  # The first row is the stakeholder's own example: the forgotten 23.40 was the whole difference.
  # A forgotten income makes MoneyBud's figure larger, so more is unexplained. Removing Huur says
  # the bank should have held 832.45 more than it did.
  Scenario Outline: An entry dated before the balance correction's day leaves the balance as I typed it, and the difference shows what is still unexplained
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Rent"
    And I have a category "Groceries"
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And I have corrected the balance of "Bank" to 1000 euro
    And the balance correction of "Bank" to 1000 euro shows a difference of -23.40 euro
    When <act>
    Then the balance of "Bank" should still be 1000 euro
    And net worth should still be 1000 euro
    And the balance correction of "Bank" to 1000 euro should show a difference of <difference> euro

    Examples:
      | act                                                                                          | difference |
      | I record an expense of 23.40 euro for "Groceries" labelled "Vergeten bon" dated yesterday    | 0.00       |
      | I change the amount of the expense labelled "Huur" to 832.45 euro                            | 0.00       |
      | I record an income of 23.40 euro labelled "Teruggave" dated yesterday                        | -46.80     |
      | I remove the expense labelled "Huur" and confirm                                             | -832.45    |

  # The expense still counts against its category in its period, as any expense does. Only the
  # location side has already seen it.
  Scenario: A receipt entered late still counts against its category, though the balance already had it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have corrected the balance of "Bank" to 1832.45 euro
    When I record an expense of 23.40 euro for "Groceries" labelled "Vergeten bon" dated yesterday
    Then the expense should be recorded
    And the balance of "Bank" should still be 1832.45 euro
    And the remaining "Groceries" budget in the current budget period should be 376.60 euro
    And the balance correction of "Bank" to 1832.45 euro should show a difference of 23.40 euro

  # ----------------------------------------------------------------------------------
  # The balance correction's own day
  #
  # The documentation's tie-break, not put to the stakeholder: on the day itself, an entry recorded
  # before the balance correction is in it, and one recorded after it moves the balance. The
  # history lists entries on one date newest recorded first, so it shows the order.
  # ----------------------------------------------------------------------------------

  Scenario: On the balance correction's day, an entry recorded before it is in it, and one recorded after it moves the balance
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Rent"
    And I have a category "Groceries"
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And I have recorded an expense of 12.50 euro for "Groceries" labelled "Lunch" dated today
    And the balance of "Bank" is 1010.90 euro
    And I have corrected the balance of "Bank" to 1000 euro
    When I record an expense of 30 euro for "Groceries" labelled "Diner" dated today
    Then the balance of "Bank" should be 970 euro
    And net worth should be 970 euro
    And the history of "Bank" should be exactly these, newest first:
      | date      | entry              | category  | label   | amount  | balance | difference |
      | today     | expense            | Groceries | Diner   | 30.00   |         |            |
      | today     | balance correction |           |         |         | 1000.00 | -10.90     |
      | today     | expense            | Groceries | Lunch   | 12.50   |         |            |
      | yesterday | expense            | Rent      | Huur    | 809.05  |         |            |
      | yesterday | income             |           | Salaris | 1832.45 |         |            |

  # A consequence of the tie-break, shown so it is approved knowingly. The balance was corrected on
  # the last day of what is now the previous period, and a lunch from that day is entered the next
  # morning. It is dated on the balance correction's day and recorded after it, so it moves the
  # balance, whether or not the bank had it in the figure that was typed.
  Scenario: An entry dated on the balance correction's day but recorded the next day moves the balance
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Groceries"
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have corrected the balance of "Bank" to 1000 euro
    When the next budget period begins while MoneyBud is open
    And I record an expense of 12.50 euro for "Groceries" labelled "Lunch" dated on the last day of the previous budget period
    Then the expense should be recorded
    And the balance of "Bank" should be 987.50 euro
    And the balance correction of "Bank" to 1000 euro should show a difference of 0.00 euro

  # A balance correction cannot be changed. Correcting again is how it is put right, and the second
  # one counts from its own moment: Diner came between the two, so the second has it in.
  Scenario: Correcting the balance again on the same day counts from the latest balance correction
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Rent"
    And I have a category "Groceries"
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And I have corrected the balance of "Bank" to 1000 euro
    And I have recorded an expense of 30 euro for "Groceries" labelled "Diner" dated today
    And the balance of "Bank" is 970 euro
    When I correct the balance of "Bank" to 975 euro
    Then the balance of "Bank" should be 975 euro
    And the history of "Bank" should be exactly these, newest first:
      | date      | entry              | category  | label   | amount  | balance | difference |
      | today     | balance correction |           |         |         | 975.00  | 5.00       |
      | today     | expense            | Groceries | Diner   | 30.00   |         |            |
      | today     | balance correction |           |         |         | 1000.00 | -23.40     |
      | yesterday | expense            | Rent      | Huur    | 809.05  |         |            |
      | yesterday | income             |           | Salaris | 1832.45 |         |            |

  # An income dated in the future reaches its account on its date (show-accounts.feature). A
  # balance typed before then does not have it in, because it is dated after the balance
  # correction's day, so the difference does not count it either.
  Scenario: An income dated after the balance correction's day reaches the balance on its date
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1800 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    And the balance of "Bank" is 0.00 euro
    When I correct the balance of "Bank" to 250 euro
    Then the balance of "Bank" should be 250 euro
    And the balance correction of "Bank" to 250 euro should show a difference of 250.00 euro
    When the next budget period begins while MoneyBud is open
    Then the balance of "Bank" should be 2050 euro
    And net worth should be 2050 euro

  # ----------------------------------------------------------------------------------
  # A changed entry keeps the moment it was first recorded
  #
  # Derived by the documentation, not put to the stakeholder: a change overwrites an entry in place,
  # and it keeps its place in the lists. So whether it is before or after a balance correction is
  # decided by when it was first recorded, and its DATE is whatever it is now.
  # ----------------------------------------------------------------------------------

  # Koffie was recorded before the balance correction. Dated today now, it is on the balance
  # correction's day and recorded before it, so it is still in it.
  Scenario: An entry recorded before the balance correction, changed onto its day, is still in it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Rent"
    And I have a category "Groceries"
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And I have recorded an expense of 5 euro for "Groceries" labelled "Koffie" dated yesterday
    And I have corrected the balance of "Bank" to 1000 euro
    And the balance correction of "Bank" to 1000 euro shows a difference of -18.40 euro
    When I change the date of the expense labelled "Koffie" to today
    Then the change should go through
    And the balance of "Bank" should still be 1000 euro
    And the balance correction of "Bank" to 1000 euro should show a difference of -18.40 euro

  # Koffie was recorded after the balance correction, dated yesterday, so it was in it. Dated today
  # now, it is on the balance correction's day and was recorded after it, so it moves the balance.
  # Changing it did not make it "recorded" at a new moment.
  Scenario: An entry recorded after the balance correction, changed onto its day, moves the balance
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Rent"
    And I have a category "Groceries"
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And I have corrected the balance of "Bank" to 1000 euro
    And I have recorded an expense of 5 euro for "Groceries" labelled "Koffie" dated yesterday
    And the balance correction of "Bank" to 1000 euro shows a difference of -18.40 euro
    When I change the date of the expense labelled "Koffie" to today
    Then the change should go through
    And the balance of "Bank" should be 995 euro
    And the balance correction of "Bank" to 1000 euro should show a difference of -23.40 euro

  # The other way: a consequence of the rules, shown so it is approved knowingly. Diner, dated
  # yesterday now, is before the balance correction's day, so the balance correction has it in, and
  # the 30 euro comes back onto the balance. What the entries said Bank held before the balance
  # correction is now 993.40, so 6.60 is unexplained.
  Scenario: Changing an entry's date to before the balance correction's day takes it into the balance correction
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Rent"
    And I have a category "Groceries"
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And I have corrected the balance of "Bank" to 1000 euro
    And I have recorded an expense of 30 euro for "Groceries" labelled "Diner" dated today
    And the balance of "Bank" is 970 euro
    When I change the date of the expense labelled "Diner" to yesterday
    Then the change should go through
    And the balance of "Bank" should be 1000 euro
    And the balance correction of "Bank" to 1000 euro should show a difference of 6.60 euro

  # ----------------------------------------------------------------------------------
  # Moving an entry to another account across a balance correction
  #
  # The glossary's stated cost: re-pointing an entry dated before an account's latest balance
  # correction does not change that account's balance. It still moves the balance of the account it
  # goes to, unless that one also has a typed balance dated after the entry. In the first row Cash's
  # starting balance is older than Markt, so Cash pays. In the second it was typed today, so it
  # already had Markt in it and nothing moves: only the difference on Bank shows that 25 euro more
  # is unexplained there.
  # ----------------------------------------------------------------------------------

  Scenario Outline: Moving an entry dated before a balance correction to another account leaves the corrected balance as it is
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Groceries"
    And I have an account "Cash" with a starting balance of 40 euro dated on <cash added>
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated yesterday
    And I have corrected the balance of "Bank" to 1800 euro
    And the balance correction of "Bank" to 1800 euro shows a difference of -7.45 euro
    When I change the account of the expense labelled "Markt" to "Cash"
    Then the change should go through
    And the balance of "Bank" should still be 1800 euro
    And the balance correction of "Bank" to 1800 euro should show a difference of -32.45 euro
    And the balance of "Cash" should be <cash> euro
    And net worth should be <net worth> euro

    Examples:
      | cash added                                 | cash  | net worth |
      | the first day of the current budget period | 15.00 | 1815.00   |
      | today                                      | 40.00 | 1840.00   |

  # ----------------------------------------------------------------------------------
  # Removing a balance correction
  #
  # Removing one loses a record, so it asks first, as removing an entry does. After declining,
  # nothing is said. Afterwards the balance is worked out as if it had never been typed: from the
  # account's previous typed balance, or, with none, as the plain sum. That it can be removed and
  # not changed, in the account's history, is in show-accounts.feature.
  # ----------------------------------------------------------------------------------

  Scenario: Removing a balance correction asks first, and the balance goes back to what the entries give
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Rent"
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And I have corrected the balance of "Bank" to 1000 euro
    When I remove the balance correction of "Bank" to 1000 euro and confirm
    Then I should have been asked to confirm first
    And I should be told that the balance correction was removed
    And the balance of "Bank" should be 1023.40 euro
    And net worth should be 1023.40 euro
    And the history of "Bank" should be exactly these, newest first:
      | date      | entry   | label   | amount  |
      | yesterday | expense | Huur    | 809.05  |
      | yesterday | income  | Salaris | 1832.45 |

  Scenario: Declining to confirm removes no balance correction
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Rent"
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And I have corrected the balance of "Bank" to 1000 euro
    When I remove the balance correction of "Bank" to 1000 euro but decline to confirm
    Then I should have been asked to confirm first
    And I should not have been told anything
    And the balance of "Bank" should still be 1000 euro

  # Two balance corrections on one day, with Diner between them. Removing the later one leaves the
  # earlier one as the latest, with Diner after it. Removing the earlier one leaves the later one as
  # it was typed, and its difference is worked out again from the entries alone: 975 against 993.40.
  Scenario Outline: Removing one of two balance corrections leaves the other, with its difference worked out again
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Rent"
    And I have a category "Groceries"
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated yesterday
    And I have recorded an expense of 809.05 euro for "Rent" labelled "Huur" dated yesterday
    And I have corrected the balance of "Bank" to 1000 euro
    And I have recorded an expense of 30 euro for "Groceries" labelled "Diner" dated today
    And I have corrected the balance of "Bank" to 975 euro
    When I remove the balance correction of "Bank" to <removed> euro and confirm
    Then I should be told that the balance correction was removed
    And the balance of "Bank" should be <balance> euro
    And the balance correction of "Bank" to <kept> euro should show a difference of <difference> euro

    Examples:
      | removed | balance | kept | difference |
      | 975     | 970.00  | 1000 | -23.40     |
      | 1000    | 975.00  | 975  | -18.40     |

  # A starting balance is the account's first balance correction, so it can be removed the same way.
  # Cash is then an account with no typed balance, whose balance is the plain sum of what is on it,
  # and Markt, which the starting balance had in it, now counts.
  Scenario: Removing a starting balance leaves the account with none, and its balance is the plain sum of what is on it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have an account "Cash" with a starting balance of 40 euro
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated yesterday on the account "Cash"
    And the balance of "Cash" is 40 euro
    When I remove the starting balance of "Cash" and confirm
    Then I should have been asked to confirm first
    And I should be told that the starting balance was removed
    And the accounts should be exactly these, in this order:
      | account | balance | overdrawn |
      | Bank    | 0.00    | no        |
      | Cash    | -25.00  | yes       |
    And the history of "Cash" should be exactly these, newest first:
      | date      | entry   | category  | label | amount |
      | yesterday | expense | Groceries | Markt | 25.00  |
    And the remaining "Groceries" budget in the current budget period should still be 375 euro

  # ----------------------------------------------------------------------------------
  # What is refused. Nothing is recorded and the balance does not move.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A corrected balance cannot be finer than a cent
    Given I have recorded an income of 100 euro labelled "Salaris" dated today
    When I try to correct the balance of "Bank" to <typed> euro
    Then the balance correction should be refused
    And I should be told that an amount cannot be finer than a cent
    And the balance of "Bank" should still be 100 euro

    Examples:
      | typed    |
      | 99.995   |
      | -0.001   |

  Scenario Outline: A corrected balance that cannot be read as an amount is refused
    Given I have recorded an income of 100 euro labelled "Salaris" dated today
    When I try to correct the balance of "Bank" to <typed>
    Then the balance correction should be refused
    And I should be told that <reason>
    And the balance of "Bank" should still be 100 euro
    And the history of "Bank" should be exactly these, newest first:
      | date  | entry  | label   | amount |
      | today | income | Salaris | 100.00 |

    Examples:
      | typed   | reason                             |
      | ""      | "" is not an amount                |
      | "abc"   | "abc" is not an amount             |
      | "1.000" | "1.000" is ambiguous: 1000 or 1,00 |
