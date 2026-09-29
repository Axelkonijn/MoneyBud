# Changing the period start day: the day every budget period begins on, set in a drop-down beside the
# period's name, and what a change does to the periods (glossary: "A configurable period start day",
# settled by the stakeholder on 2026-09-29; Budget period; "A start day the month is too short for clamps
# to its last day"). What a change does to plans, backed money and the sweep is in
# carry-plans-and-money-across-a-start-day-change.feature. How a period is named is in
# name-a-budget-period.feature. THIS FILE EXPLAINS THE STEPS THE THREE SHARE.
#
# The stakeholder's reason, 2026-09-28: his salary comes on the 27th, so he wants periods to run from
# payday.
#
# The rules, from arc42 §12:
#   - RULING 1. The start day CAN BE CHANGED AT ANY TIME, and applies FROM THE CURRENT PERIOD ON. Earlier
#     periods keep their boundaries. THE CURRENT PERIOD KEEPS ITS FIRST DAY AND ENDS THE DAY BEFORE THE
#     NEW START DAY'S FIRST OCCURRENCE AFTER THAT FIRST DAY. Every period after it starts on the new day.
#     So the current period can END ON THE SPOT, when the new day has already come round in it. Rejected:
#     from the next period on; once, at the first start only.
#   - THE CLAMP STANDS: a start day a month is too short for falls on that month's last day, for the day
#     the new start day first comes round as for every period after.
#   - RULING 2. A drop-down beside the period's name, captioned "Periode begint op", OFFERING 1 TO 31.
#   - RULING 6. The drop-down SHOWS ON THE CURRENT PERIOD AND LATER ONES ONLY, and is hidden on a past
#     one. Rejected: always visible.
#   - RULING 5. CHANGING IT ASKS FIRST, in the message bar, answered "Wijzigen" or "Annuleren".
#     ANNULEREN PUTS THE LIST BACK. Once confirmed, MONEYBUD SAYS WHAT CHANGED. Rejected: doing it and
#     saying so afterwards, as archiving does.
#   - FOLLOW-UP 4. THE PERIOD CUT SHORT KEEPS ITS PLAN, and loses what is dated after its new end: its
#     salary, in the stakeholder's example, so it reads "Te veel toegewezen" and, below zero, "Tekort", for
#     good. Expenses dated after its new end are in the new current period, over budget until it is
#     planned. Rejected: moving the plan along into the new current period.
#   - FOLLOW-UP 1. AFTER A CHANGE THE OVERVIEW SHOWS THE PERIOD NEAREST TO WHERE IT WAS: made on the
#     current period, the new current period; made on a later period, the period THAT PERIOD'S PLAN WENT
#     TO (the period its old first day falls in, carry-plans-and-money-across-a-start-day-change.feature).
#     Rejected: always the new current period; the period that now holds the shown period's first day.
#
# DERIVED BY THE DOCUMENTATION AND NOT PUT TO THE STAKEHOLDER, marked where a scenario rests on one:
#   - A change NEVER LENGTHENS the current period: the new day always comes round inside it. (Ruled at
#     the build, 2026-09-29, on the recommendation: one corner is an exception, and ruling 1 is followed
#     there. A period begun on a clamped day, such as 28 February under the 29th, changed to a later day,
#     such as the 31st, whose own clamp is that same day, ends the day before the new day's next
#     occurrence: 28 February to 30 March, two days longer. No scenario here reaches it. Found at the
#     build the same day, by the same ruling: going back to the old day while a change is still to take
#     effect gives the current period back its old end, as to the 30th on 29 September, which cuts
#     September to the 29th, and back to the 1st, which makes it 1 to 30 September again.)
#   - The drop-down SHOWS THE DAY NOW SET, the same on every period where it shows, and is gone on the
#     period cut short, which is past.
#   - Data kept before this increment, and A FIRST START, BEGIN ON THE 1ST (start-moneybud.feature). So
#     does every scenario's empty ledger.
#   - CHOOSING THE DAY ALREADY SET IS A COMPLETE NO-OP: no question, no notice, nothing saved. The list
#     writes back what it shows on every redraw, as "Restant naar" does (choose-a-sweep-destination.feature).
#   - CHANGING MORE THAN ONCE LEAVES EACH SHORT PERIOD IN PLACE, and there is no undo beyond changing
#     again. A short period can be A SINGLE DAY.
#   - ENTRIES FOLLOW THEIR DATES: each income and expense is in the period its date falls in under the
#     calendar as it now is, an income dated in the future included.
#   - RECURRING ENTRIES ARE UNAFFECTED: a repeat keeps its day of the month, not a period.
#   - THE PERIOD CUT SHORT IS PAST: assigning in it is refused, and the drop-down is hidden there.
#
# IN THE DOCUMENTATION'S READING, open to contradiction at this gate:
#   - EVERY CHANGE ASKS, including one that ends nothing, such as the 30th on 29 September.
#   - IT IS THE ONE QUESTION, as removing's is: asking it drops a removal question still waiting, and
#     stepping away drops it and puts the list back.
#   - Declining SAYS NOTHING, as declining to remove says nothing (remove-an-entry.feature).
#
# RULED BY THE STAKEHOLDER AT THE SCENARIO STAGE, 2026-09-29, on points raised while writing these
# scenarios:
#   - THE ASSIGN FORM'S OWN PERIOD GOES NEAREST TO WHERE IT WAS, BY THE SAME RULE AS THE SCREEN: to the
#     period its old first day falls in. With the form on the period on screen that is where the screen
#     goes. With the form set apart, say on November while September is on screen, it goes to 27 okt –
#     26 nov, not to the screen's period. Rejected: the form always follows the screen.
#   - A period ended by a change is swept by itself even when its new end falls before MoneyBud was first
#     started, and short month names are jan feb mrt apr mei jun jul aug sep okt nov dec, without dots.
#     Both are in the files they concern.
#
# NOT specified here: the words of the question and of the notice, which are copy; how the drop-down's
# items are written; where the drop-down sits beside the name, which is layout. Money a change moves,
# and the sweep of the period it ends, are in carry-plans-and-money-across-a-start-day-change.feature.
#
# DATES. As in repeat-an-entry.feature, days are named as CALENDAR DATES, since a start day is a day of
# the month. Periods are still named relative to today: "the current budget period", "the previous budget
# period", "the budget period 2 before the current one". AFTER A CHANGE, PERIODS ARE NAMED UNDER THE
# CALENDAR AS CHANGED, as after a period boundary they are named relative to the new today
# (step-between-periods.feature). So a Given before the change that names "the current budget period"
# means 1 to 30 September, and a Then after a change to the 27th that names "the previous budget period"
# means 1 to 26 September. Each scenario says which days a period runs from and to, so that there is no
# guessing. 2027 has a 28 February, and 2028 a 29th.
#
# Reading the steps. These are shared by the three files and by the scenarios about the start day in
# keep-data.feature and start-moneybud.feature:
#   - "today is 29 September 2026" is repeat-an-entry.feature's: the day the scenario begins, and THE
#     EMPTY LEDGER COUNTS AS FIRST STARTED THAT DAY, with its periods beginning on the 1st.
#   - "I change the period start day to the 27th and confirm" is the whole act, with the period on screen:
#     I choose the 27th in the drop-down, MoneyBud asks, and I answer Wijzigen. "... but decline to
#     confirm" is the same, answered Annuleren. "I choose the 27th as the period start day" is only the
#     choosing, with the answer not given yet. "I have changed the period start day to the 27th" is the
#     whole act, confirmed, done earlier TODAY, at that point in the order of the Givens, with everything
#     it did.
#   - "I should have been asked to confirm first" is remove-an-entry.feature's: the question came before
#     anything changed. "MoneyBud should be asking me to confirm" and "MoneyBud should not be asking me
#     anything" are keep-data.feature's: a question is, or is not, waiting.
#   - "I should be told that budget periods now start on the 27th" is the notice after a change. What is
#     fixed is that I am told, and the day. Not the wording, and not whether it also names the new current
#     period.
#   - "the period start day shown in the current (next) budget period should be the 27th" is what the
#     drop-down shows while that period is on screen. "the previous budget period should offer no period
#     start day" means there is no drop-down while that period is on screen. "the choices offered for the
#     period start day should be every day from the 1st to the 31st, in order" is the whole drop-down.
#   - "the current budget period should run from 27 September 2026 to 26 October 2026" names that
#     period's first and last day. "should still run" says the step before did not change it.
#   - "I have set the period to assign in to the next budget period, and assigned nothing" is
#     take-over-a-plan.feature's: the assign form's own period moved away from the period on screen.
#     "the period to assign in should be the next budget period" is where that form's period is now.
#   - "the Overview shows (should show) the budget period 2 after the current one" is step-between-
#     periods.feature's step, for a period further on than the next.
#   - "the day becomes 30 September 2026 while MoneyBud is open" is repeat-an-entry.feature's.
#   - "the budget for "X" in the budget period 2 before the current one" is the budget step of
#     assign-to-category.feature, for a period further back than the previous.
#   - Every other step is reused unchanged from the file that introduced it: the list steps are
#     list-transactions-in-a-period.feature's, the take-over steps take-over-a-plan.feature's, the saving
#     steps carry-on-when-saving-fails.feature's, the period leftover steps sweep-at-a-period-end.feature's.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). No category in it is backed, and it has no
# sweep destination. The names, labels and amounts are synthetic test data.

@periods
Feature: Change the day budget periods start on
  As someone whose salary comes on the 27th of the month
  I want to choose the day my budget periods start on, and change it when my payday changes
  So that each period runs from one payday to the next, and the salary that has just come in is in the period it pays for

  # ----------------------------------------------------------------------------------
  # The drop-down
  # ----------------------------------------------------------------------------------

  # Derived: a ledger begins on the 1st, so every period is a calendar month until I change it.
  Scenario: Budget periods start on the 1st until I change it, and every day from the 1st to the 31st is offered
    Given my budget periods are one month long
    And today is 29 September 2026
    Then the period start day shown in the current budget period should be the 1st
    And the choices offered for the period start day should be every day from the 1st to the 31st, in order
    And the current budget period should run from 1 September 2026 to 30 September 2026
    And the previous budget period should run from 1 August 2026 to 31 August 2026

  # Ruling 6: August must not seem to say it began on the 27th when it began on the 1st. After the change,
  # 1 to 26 September is past too, and has no drop-down either (derived).
  Scenario: The start day is offered on the current period and later ones, shows the day set, and is not offered on a past one
    Given my budget periods are one month long
    And today is 29 September 2026
    Then the period start day shown in the current budget period should be the 1st
    And the period start day shown in the next budget period should be the 1st
    And the previous budget period should offer no period start day
    When I change the period start day to the 27th and confirm
    Then the period start day shown in the current budget period should be the 27th
    And the period start day shown in the next budget period should be the 27th
    And the previous budget period should offer no period start day
    And the budget period 2 before the current one should offer no period start day

  # ----------------------------------------------------------------------------------
  # Changing it asks first (ruling 5)
  # ----------------------------------------------------------------------------------

  # A change can end the current period on the spot, and that cannot be undone. So nothing changes
  # until I answer: the salary of the 27th is still in September while the question waits.
  Scenario: Choosing a new start day asks first, and nothing changes until I answer
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 September 2026
    When I choose the 27th as the period start day
    Then MoneyBud should be asking me to confirm
    And the current budget period should still run from 1 September 2026 to 30 September 2026
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date              | label   | amount  |
      | 27 September 2026 | Salaris | 2500.00 |

  # Annuleren puts the list back and changes nothing, so there is nothing to save, and nothing is said:
  # the documentation's reading, as declining to remove says nothing (see the header).
  Scenario: Declining puts the start day back and changes nothing
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 September 2026
    And saving is not possible
    When I change the period start day to the 27th but decline to confirm
    Then I should have been asked to confirm first
    And I should not have been told anything
    And MoneyBud should not show that my changes are not saved
    And the period start day shown in the current budget period should still be the 1st
    And the current budget period should still run from 1 September 2026 to 30 September 2026
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date              | label   | amount  |
      | 27 September 2026 | Salaris | 2500.00 |
    And the Overview should show the current budget period

  # The documentation's reading (see the header): every change asks, one that ends nothing included.
  # The 30th has not yet come round on the 29th, so September is cut short to end TODAY, not on the
  # spot. It ends at midnight like any period, and the screen stays where it was (step-between-
  # periods.feature).
  Scenario: A change that ends nothing still asks, and the current period ends when the new day comes
    Given my budget periods are one month long
    And today is 29 September 2026
    When I change the period start day to the 30th and confirm
    Then I should have been asked to confirm first
    And I should be told that budget periods now start on the 30th
    And the Overview should show the current budget period
    And the current budget period should run from 1 September 2026 to 29 September 2026
    And the next budget period should run from 30 September 2026 to 29 October 2026
    When the day becomes 30 September 2026 while MoneyBud is open
    Then the Overview should show the previous budget period
    And the current budget period should run from 30 September 2026 to 29 October 2026

  # Derived: the drop-down writes back what it shows on every redraw, so choosing what is already chosen
  # must not count as an act. Were it saved, a disk that cannot be written to would show "not saved"
  # after nothing at all.
  Scenario: Choosing the start day already set does nothing at all: no question, nothing said, nothing saved
    Given my budget periods are one month long
    And today is 29 September 2026
    And saving is not possible
    When I choose the 1st as the period start day
    Then MoneyBud should not be asking me anything
    And I should not have been told anything
    And MoneyBud should not show that my changes are not saved
    And the current budget period should still run from 1 September 2026 to 30 September 2026

  Scenario: Choosing again the start day I changed to does nothing at all
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have changed the period start day to the 27th
    And saving is not possible
    When I choose the 27th as the period start day
    Then MoneyBud should not be asking me anything
    And I should not have been told anything
    And MoneyBud should not show that my changes are not saved
    And the current budget period should still run from 27 September 2026 to 26 October 2026

  # The documentation's reading (see the header): it is the one question, so asking it drops the removal
  # question that was waiting. Nothing is removed, since that question was never answered. Bakker, dated
  # the 29th, is now in the new current period.
  Scenario: Choosing a start day while a removal question waits drops that question, and nothing is removed
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have a category "Groceries"
    And I have recorded an expense of 20 euro for "Groceries" labelled "Bakker" dated 29 September 2026
    When I ask to remove the expense labelled "Bakker"
    And I change the period start day to the 27th and confirm
    Then MoneyBud should not be asking me anything
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category  | label  | amount |
      | 29 September 2026 | Groceries | Bakker | 20.00  |

  # The documentation's reading (see the header): stepping away drops the question and puts the list
  # back, as it drops a removal question. Nothing changed, and nothing is said.
  Scenario: Stepping to another period while the question waits drops it, and the start day stays as it was
    Given my budget periods are one month long
    And today is 29 September 2026
    When I choose the 27th as the period start day
    And I step forward one budget period
    Then MoneyBud should not be asking me anything
    And I should not have been told anything
    And the Overview should show the next budget period
    And the period start day shown in the next budget period should be the 1st
    And the current budget period should still run from 1 September 2026 to 30 September 2026

  # ----------------------------------------------------------------------------------
  # A change applies from the current period on (ruling 1)
  # ----------------------------------------------------------------------------------

  # The stakeholder's worked example, the periods only. On 29 September, with September current, the
  # 27th has already come round, so 1 to 26 September ends on the spot and 27 September begins the new
  # current period, which the Overview shows. August keeps its boundaries.
  Scenario: The current period keeps its first day and ends the day before the new start day, and the Overview shows the new current period
    Given my budget periods are one month long
    And today is 29 September 2026
    And the Overview shows the current budget period
    When I change the period start day to the 27th and confirm
    Then I should have been asked to confirm first
    And I should be told that budget periods now start on the 27th
    And MoneyBud should not be asking me anything
    And the Overview should show the current budget period
    And the current budget period should run from 27 September 2026 to 26 October 2026
    And the previous budget period should run from 1 September 2026 to 26 September 2026
    And the budget period 2 before the current one should run from 1 August 2026 to 31 August 2026
    And the next budget period should run from 27 October 2026 to 26 November 2026

  # Ruling 1, the new end for several days, all on 29 September with September current. In the first
  # table the new day has already come round in September, so the current period ends on the spot: on
  # the 2nd it becomes a single day, and on the 29th it ended yesterday. In the second it has not, so
  # September is cut short and stays current. The 31st falls on 30 September, which September is too
  # short for, and the next period then runs to the 30th of October, the day before the 31st. No row
  # makes the current period longer (derived).
  Scenario Outline: The current period ends the day before the new start day first comes round
    Given my budget periods are one month long
    And today is 29 September 2026
    When I change the period start day to the <day> and confirm
    Then the previous budget period should run from <previous from> to <previous to>
    And the current budget period should run from <current from> to <current to>
    And the next budget period should run from <next from> to <next to>

    Examples: the new day has come round, so the current period ends on the spot
      | day  | previous from    | previous to       | current from      | current to        | next from         | next to          |
      | 27th | 1 September 2026 | 26 September 2026 | 27 September 2026 | 26 October 2026   | 27 October 2026   | 26 November 2026 |
      | 5th  | 1 September 2026 | 4 September 2026  | 5 September 2026  | 4 October 2026    | 5 October 2026    | 4 November 2026  |
      | 2nd  | 1 September 2026 | 1 September 2026  | 2 September 2026  | 1 October 2026    | 2 October 2026    | 1 November 2026  |
      | 29th | 1 September 2026 | 28 September 2026 | 29 September 2026 | 28 October 2026   | 29 October 2026   | 28 November 2026 |

    Examples: the new day is still to come, so the current period is cut short and stays current
      | day  | previous from    | previous to       | current from      | current to        | next from         | next to          |
      | 30th | 1 August 2026    | 31 August 2026    | 1 September 2026  | 29 September 2026 | 30 September 2026 | 29 October 2026  |
      | 31st | 1 August 2026    | 31 August 2026    | 1 September 2026  | 29 September 2026 | 30 September 2026 | 30 October 2026  |

  # The clamp stands. On 10 February the 29th, 30th and 31st first come round on February's last day,
  # a day nobody picked, and the period that begins there is longer than its neighbours. The period after
  # it is back on the day chosen. That cost was accepted with the clamp ("A start day the month is too
  # short for clamps to its last day"). 2028 is a leap year, so there the 29th needs no clamp. April has
  # no 31st either, so for the 31st the period from 31 March ends on 29 April (two cells corrected from
  # 30 April at the build, 2026-09-29, with the stakeholder's approval: the clamp as ruled).
  Scenario Outline: A start day February is too short for comes round on its last day, and the periods after it are back on the day chosen
    Given my budget periods are one month long
    And today is <today>
    When I change the period start day to the <day> and confirm
    Then the current budget period should run from <current from> to <current to>
    And the next budget period should run from <next from> to <next to>
    And the budget period 2 after the current one should run from <then from> to <then to>

    Examples:
      | today            | day  | current from    | current to       | next from        | next to       | then from     | then to       |
      | 10 February 2027 | 31st | 1 February 2027 | 27 February 2027 | 28 February 2027 | 30 March 2027 | 31 March 2027 | 29 April 2027 |
      | 10 February 2027 | 30th | 1 February 2027 | 27 February 2027 | 28 February 2027 | 29 March 2027 | 30 March 2027 | 29 April 2027 |
      | 10 February 2027 | 29th | 1 February 2027 | 27 February 2027 | 28 February 2027 | 28 March 2027 | 29 March 2027 | 28 April 2027 |
      | 10 February 2028 | 31st | 1 February 2028 | 28 February 2028 | 29 February 2028 | 30 March 2028 | 31 March 2028 | 29 April 2028 |
      | 10 February 2028 | 29th | 1 February 2028 | 28 February 2028 | 29 February 2028 | 28 March 2028 | 29 March 2028 | 28 April 2028 |

  # Derived: changing again applies from the current period on as well, so each short period stays. Back
  # to the 1st, 1 to 26 September does not come back: 27 to 30 September becomes a short current period,
  # then October (the stakeholder was shown this with ruling 5). To the 28th, 27 September becomes a
  # single day that has ended. To the 26th, the current period loses a day and ends nothing: it is never
  # made longer.
  Scenario Outline: A second change also applies from the current period on, keeps each short period, and never lengthens the current one
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have changed the period start day to the 27th
    When I change the period start day to the <day> and confirm
    Then I should have been asked to confirm first
    And the budget period 2 before the current one should run from <earlier from> to <earlier to>
    And the previous budget period should run from <previous from> to <previous to>
    And the current budget period should run from <current from> to <current to>
    And the next budget period should run from <next from> to <next to>

    Examples:
      | day  | earlier from     | earlier to        | previous from     | previous to       | current from      | current to        | next from       | next to          |
      | 1st  | 1 August 2026    | 31 August 2026    | 1 September 2026  | 26 September 2026 | 27 September 2026 | 30 September 2026 | 1 October 2026  | 31 October 2026  |
      | 28th | 1 September 2026 | 26 September 2026 | 27 September 2026 | 27 September 2026 | 28 September 2026 | 27 October 2026   | 28 October 2026 | 27 November 2026 |
      | 26th | 1 August 2026    | 31 August 2026    | 1 September 2026  | 26 September 2026 | 27 September 2026 | 25 October 2026   | 26 October 2026 | 25 November 2026 |

  # ----------------------------------------------------------------------------------
  # Entries follow their dates
  # ----------------------------------------------------------------------------------

  # Derived. Every entry falls in the period its date is in under the calendar as changed. The rows sit
  # on the boundaries: the 26th is the last day of the period cut short, the 27th the first of the new
  # current one, and 30 October is in the period after. Bonus, dated in the future, moves from October
  # into the current period and its Unassigned.
  Scenario: Every income and expense is in the period its date falls in once the start day has changed, one dated in the future included
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have a category "Groceries"
    And I have recorded an income of 1000 euro labelled "Voorschot" dated 20 September 2026
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 September 2026
    And I have recorded an income of 1800 euro labelled "Bonus" dated 10 October 2026
    And I have recorded an income of 90 euro labelled "Teruggave" dated 30 October 2026
    And I have recorded an expense of 12.50 euro for "Groceries" labelled "Kiosk" dated 26 September 2026
    And I have recorded an expense of 30 euro for "Groceries" labelled "Bakker" dated 27 September 2026
    When I change the period start day to the 27th and confirm
    Then the incomes listed in the previous budget period should be exactly these, in this order:
      | date              | label     | amount  |
      | 20 September 2026 | Voorschot | 1000.00 |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date              | label   | amount  |
      | 10 October 2026   | Bonus   | 1800.00 |
      | 27 September 2026 | Salaris | 2500.00 |
    And the incomes listed in the next budget period should be exactly these, in this order:
      | date            | label     | amount |
      | 30 October 2026 | Teruggave | 90.00  |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date              | category  | label | amount |
      | 26 September 2026 | Groceries | Kiosk | 12.50  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category  | label  | amount |
      | 27 September 2026 | Groceries | Bakker | 30.00  |
    And Unassigned in the previous budget period should be 1000 euro
    And Unassigned in the current budget period should be 4300 euro
    And Unassigned in the next budget period should be 90 euro

  # Derived: a repeat keeps its day of the month, not a period. Netflix still comes on the 25th, not on
  # the new start day, and October's falls in the period that runs to 26 October. The change records
  # nothing by itself.
  Scenario: A monthly repeat keeps its own day after the start day changes
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have a category "Subscriptions"
    When I record an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 September 2026, repeating monthly
    And I change the period start day to the 27th and confirm
    Then MoneyBud should not have recorded any repeating entry
    When the day becomes 25 October 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date            | category      | label   | amount | repeats |
      | 25 October 2026 | Subscriptions | Netflix | 13.99  | monthly |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  |         |

  # ----------------------------------------------------------------------------------
  # The period cut short keeps its plan (follow-up 4)
  # ----------------------------------------------------------------------------------

  # The cost the stakeholder was shown and took. September's plan stays with 1 to 26 September, but the
  # salary of the 27th does not: that period now has a plan of 400 and no income, so it is over-assigned
  # by 400, and with Groceries' unspent 300 its period leftover is -100, a shortfall. Bakker, dated the
  # 28th, is in a period with no plan yet, so Groceries is over budget there. Nothing warns about either.
  # The new current period has no plan, so it is offered the one of the period cut short. And that period
  # is past, so the shortfall stays for good: nothing can be assigned in it.
  Scenario: The period cut short keeps its plan and loses what is dated after its new end, and is past from then on
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 100 euro for "Groceries" labelled "Markt" dated 20 September 2026
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 September 2026
    And I have recorded an expense of 30 euro for "Groceries" labelled "Bakker" dated 28 September 2026
    And Unassigned in the current budget period is 2100 euro
    When I change the period start day to the 27th and confirm
    Then I should not be warned about the budget period becoming over-assigned
    And the budget for "Groceries" in the previous budget period should still be 400 euro
    And the remaining "Groceries" budget in the previous budget period should be 300 euro
    And Unassigned in the previous budget period should be -400 euro
    And the previous budget period should be shown as over-assigned
    And the previous budget period should show a period leftover of -100 euro, with the marker a category over budget has and the badge "Tekort"
    And the budget for "Groceries" in the current budget period should be 0.00 euro
    And the remaining "Groceries" budget in the current budget period should be -30 euro
    And "Groceries" should be shown as over budget in the current budget period
    And Unassigned in the current budget period should be 2500 euro
    And the plan of the previous budget period should be offered in the current budget period, with a total of 400 euro
    When I try to assign -100 euro to "Groceries" in the previous budget period
    Then the assignment should be refused
    And I should be told that nothing can be assigned in a past budget period
    And Unassigned in the previous budget period should still be -400 euro

  # Ruling 1: earlier periods keep their boundaries, and so do their plans.
  Scenario: A period before the current one keeps its boundaries and its plan
    Given my budget periods are one month long
    And today is 29 September 2026
    And I have a budget of 350 euro for "Groceries" in the previous budget period
    When I change the period start day to the 27th and confirm
    Then the budget period 2 before the current one should run from 1 August 2026 to 31 August 2026
    And the budget for "Groceries" in the budget period 2 before the current one should still be 350 euro

  # ----------------------------------------------------------------------------------
  # The period shown after a change (follow-up 1)
  # ----------------------------------------------------------------------------------

  # From the current period, the new current period. From a later period, the period that period's plan
  # went to, the one its old first day falls in: October's 1st is in 27 September to 26 October, and
  # November's in 27 October to 26 November. Either way the change applies from the current period on,
  # not from the period on screen. A change that leaves the period on screen in place shows it as it now
  # is: to the 30th, September is still current, cut short, and October's 1st is in the period after it.
  # The assign form was on the period on screen, so it goes where the screen goes (ruled at the scenario
  # stage, see the header).
  Scenario Outline: After a change the Overview shows the period nearest to where it was
    Given my budget periods are one month long
    And today is 29 September 2026
    And the Overview shows <shown>
    When I change the period start day to the <day> and confirm
    Then the Overview should show <shown now>
    And <shown now> should run from <from> to <to>
    And I should be able to assign in the budget period shown

    Examples:
      | day  | shown                                     | shown now                 | from              | to                |
      | 27th | the current budget period                 | the current budget period | 27 September 2026 | 26 October 2026   |
      | 27th | the next budget period                    | the current budget period | 27 September 2026 | 26 October 2026   |
      | 27th | the budget period 2 after the current one | the next budget period    | 27 October 2026   | 26 November 2026  |
      | 30th | the current budget period                 | the current budget period | 1 September 2026  | 29 September 2026 |
      | 30th | the next budget period                    | the next budget period    | 30 September 2026 | 29 October 2026   |

  # Ruled at the scenario stage (see the header). September is on screen, but the assign form's own period
  # was set elsewhere. It goes to the period its old first day falls in, as the screen would, and not to
  # the screen's period: November's 1st is in 27 October to 26 November, October's in 27 September to
  # 26 October. August is before the current period and keeps its boundaries, so the form stays on it.
  Scenario Outline: An assign form set to another period than the one on screen goes to the period nearest to where it was
    Given my budget periods are one month long
    And today is 29 September 2026
    And the Overview shows the current budget period
    And I have set the period to assign in to <form before>, and assigned nothing
    When I change the period start day to the 27th and confirm
    Then the Overview should show the current budget period
    And the period to assign in should be <form now>
    And <form now> should run from <from> to <to>

    Examples:
      | form before                               | form now                                   | from              | to               |
      | the budget period 2 after the current one | the next budget period                     | 27 October 2026   | 26 November 2026 |
      | the next budget period                    | the current budget period                  | 27 September 2026 | 26 October 2026  |
      | the previous budget period                | the budget period 2 before the current one | 1 August 2026     | 31 August 2026   |
