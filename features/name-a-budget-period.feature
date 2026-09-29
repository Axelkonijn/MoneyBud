# How MoneyBud names a budget period, now that a period need not be a calendar month (glossary: "A
# configurable period start day", ruling 3 and follow-up 3, settled by the stakeholder on 2026-09-29).
# Changing the start day is in change-the-period-start-day.feature, which explains the steps the three
# start-day files share.
#
# The rules, from arc42 §12:
#   - RULING 3. A CALENDAR MONTH IS NAMED BY ITS MONTH: "september 2026". A PERIOD THAT IS NOT A CALENDAR
#     MONTH IS NAMED BY ITS DAYS, SHORT: "27 sep – 26 okt 2026", with short month names and the year once,
#     at the end. ACROSS A NEW YEAR: "27 dec 2026 – 26 jan 2027". Rejected: the long form "27 september
#     2026 t/m 26 oktober 2026", too long for the header and notices; "oktober 2026", named by the month
#     most of it falls in, which a short one-off period could not be told from a normal one by.
#   - FOLLOW-UP 3. A PERIOD INSIDE ONE MONTH NAMES THE MONTH ONCE: "1 – 26 sep 2026". A ONE-DAY PERIOD IS
#     "27 sep 2026". Rejected: "1 sep – 26 sep 2026", which names the same month twice.
#
# IN THE DOCUMENTATION'S READING, open to contradiction at this gate:
#   - "A calendar month" is a period running from the 1st to its month's last day. So after changing back
#     to the 1st, October is "oktober 2026" again.
#   - EVERY PLACE THAT NAMES A PERIOD USES THIS FORM: the header, the assign form's period, the take-over
#     button, the sweep's line, history rows and notices. Here the header, a notice that says where an
#     entry went, and the take-over button are checked. The rest are held by developer tests.
#   - The rule under all four forms: write each day with its month, and leave out what the end repeats of
#     the start, the year always and the month when it is the same.
#
# RULED BY THE STAKEHOLDER AT THE SCENARIO STAGE, 2026-09-29, on a point raised while writing these
# scenarios: THE SHORT MONTH NAMES ARE jan feb mrt apr mei jun jul aug sep okt nov dec, WITHOUT DOTS, as
# the rulings' own sep, okt, dec and jan are written. Rejected: the same with dots ("mrt."), as Dutch
# date formatting on Windows writes them; always the first three letters ("maa").
#
# Reading the steps. Unlike the other files, THESE STEPS COMPARE THE DUTCH TEXT, because the form of a
# name is what was ruled. The dash is an en dash with a space either side, as the rulings write it.
#   - "the budget periods should be named like this" is a table: PERIOD names a period as the other steps
#     do, and NAME is exactly what the Overview's header shows while that period is on screen.
#   - "that notice should name the period "X"" means the notice just given contains the name X, written
#     exactly so. "the plan offered in the current budget period should name the period "X"" is the same,
#     for the take-over button.
#   - Every other step is reused unchanged from change-the-period-start-day.feature and the files it names.
#
# Every scenario starts from an empty ledger (show-accounts.feature). The names, labels and amounts are
# synthetic test data.

@periods
Feature: Name a budget period
  As someone whose budget periods may run from the 27th to the 26th
  I want every period named by exactly the days it covers, unless it is a whole calendar month
  So that I can always tell which days a period holds, and a short period left by a change is never mistaken for a month

  # Ruling 3: while the start day is the 1st, every period is a calendar month, named as before.
  Scenario: A period that is a calendar month is named by its month
    Given my budget periods are one month long
    And today is 29 September 2026
    Then the budget periods should be named like this:
      | period                     | name           |
      | the previous budget period | augustus 2026  |
      | the current budget period  | september 2026 |
      | the next budget period     | oktober 2026   |

  # The stakeholder's worked example. August is still a calendar month. The period cut short lies inside
  # September, so the month is named once (follow-up 3). The new current period runs across two months,
  # and the one from 27 December runs across a new year, so it names both years.
  Scenario: Periods that are not a calendar month are named by their days
    Given my budget periods are one month long
    And today is 29 September 2026
    When I change the period start day to the 27th and confirm
    Then the budget periods should be named like this:
      | period                                     | name                      |
      | the budget period 2 before the current one | augustus 2026             |
      | the previous budget period                 | 1 – 26 sep 2026           |
      | the current budget period                  | 27 sep – 26 okt 2026      |
      | the budget period 3 after the current one  | 27 dec 2026 – 26 jan 2027 |

  # Follow-up 3. From the 27th to the 28th on 29 September leaves 27 September a period of its own, a
  # single day, which is named by that day (derived: "a short period can be very short").
  Scenario: A one-day period is named by its day
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have changed the period start day to the 27th
    When I change the period start day to the 28th and confirm
    Then the budget periods should be named like this:
      | period                                     | name                 |
      | the budget period 2 before the current one | 1 – 26 sep 2026      |
      | the previous budget period                 | 27 sep 2026          |
      | the current budget period                  | 28 sep – 27 okt 2026 |

  # Ruled at the scenario stage (see the header): every month's short name, over a year of periods from
  # the 27th. On 10 January the 27th is still to come, so January is cut short and stays current.
  Scenario: Every month has its short name, without a dot
    Given my budget periods are one month long
    And today is 10 January 2027
    When I change the period start day to the 27th and confirm
    Then the budget periods should be named like this:
      | period                                     | name                      |
      | the current budget period                  | 1 – 26 jan 2027           |
      | the next budget period                     | 27 jan – 26 feb 2027      |
      | the budget period 2 after the current one  | 27 feb – 26 mrt 2027      |
      | the budget period 3 after the current one  | 27 mrt – 26 apr 2027      |
      | the budget period 4 after the current one  | 27 apr – 26 mei 2027      |
      | the budget period 5 after the current one  | 27 mei – 26 jun 2027      |
      | the budget period 6 after the current one  | 27 jun – 26 jul 2027      |
      | the budget period 7 after the current one  | 27 jul – 26 aug 2027      |
      | the budget period 8 after the current one  | 27 aug – 26 sep 2027      |
      | the budget period 9 after the current one  | 27 sep – 26 okt 2027      |
      | the budget period 10 after the current one | 27 okt – 26 nov 2027      |
      | the budget period 11 after the current one | 27 nov – 26 dec 2027      |
      | the budget period 12 after the current one | 27 dec 2027 – 26 jan 2028 |

  # The documentation's reading (see the header): back on the 1st, a period that runs from the 1st to its
  # month's last day is a calendar month again, and is named so. The short periods stay as they are.
  Scenario: Back on the 1st, a whole month is named by its month again
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have changed the period start day to the 27th
    When I change the period start day to the 1st and confirm
    Then the budget periods should be named like this:
      | period                     | name             |
      | the previous budget period | 1 – 26 sep 2026  |
      | the current budget period  | 27 – 30 sep 2026 |
      | the next budget period     | oktober 2026     |

  # The documentation's reading (see the header): a notice and the take-over button name a period as the
  # header does. Terugbetaling, dated the 26th, lands in the period cut short, and the plan offered in
  # the new current period is that period's.
  Scenario: A notice and the plan offered name a period the same way the header does
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have changed the period start day to the 27th
    And the Overview shows the current budget period
    When I record an income of 45 euro labelled "Terugbetaling" dated 26 September 2026
    Then I should be told that the income went into the previous budget period
    And that notice should name the period "1 – 26 sep 2026"
    And the plan offered in the current budget period should name the period "1 – 26 sep 2026"
    When I take over the plan offered
    Then I should be told that the plan was taken over into the current budget period
    And that notice should name the period "27 sep – 26 okt 2026"
