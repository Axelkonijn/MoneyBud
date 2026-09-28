# Changing, stopping and correcting a repeat: the latest occurrence sets what follows, and every
# occurrence is still an entry of its own (glossary: "Recurring entries", settled by the stakeholder on
# 2026-09-28; Occurrence). Setting a repeat up, and what MoneyBud records by itself, are in
# repeat-an-entry.feature, WHICH EXPLAINS THE STEPS THE TWO SHARE. Changing and removing any entry are in
# change-an-entry.feature and remove-an-entry.feature, and everything they say holds for an occurrence:
# a change is judged as if recorded now, is announced, and is never confirmed; removing asks first.
#
# The rules, from arc42 §12:
#   - RULING 3. THE LATEST OCCURRENCE SETS THE NEXT. Each new occurrence copies its amount, label,
#     category (for an expense), account and frequency. CHANGING IT CHANGES WHAT FOLLOWS. SETTING IT TO
#     ONE-OFF STOPS THE REPEAT. EARLIER OCCURRENCES ARE NEVER TOUCHED: a new price is not back-dated.
#     This is the first interview's wish, a changed price "zonder dat ik een hele nieuwe reeks moet
#     starten". Rejected: a separate list of repeats to edit, pause and stop them.
#   - RULING 4. REMOVING AN OCCURRENCE REMOVES ONLY THAT ONE. The repeat carries on, and the next still
#     comes. Rejected: removing the latest stops the repeat, which would quietly stop a subscription meant
#     to be kept.
#   - RULING 6. ONLY THE LATEST OCCURRENCE'S ROW CARRIES A SMALL GREY LABEL, "maandelijks" or
#     "wekelijks". Earlier occurrences are plain rows. The label is in the period the latest's date falls
#     in, so between two occurrences the current period may have none, and the row to change or stop the
#     repeat from is in the period before. Rejected: the label on every occurrence.
#   - FOLLOW-UP 1. When the latest occurrence is removed, THE MOST RECENTLY RECORDED OCCURRENCE LEFT
#     BECOMES THE LATEST: it carries the label, and the next copies it. THE NEXT DATE DOES NOT MOVE.
#     REMOVING THE ONLY OCCURRENCE ENDS THE REPEAT.
#   - FOLLOW-UP 2. AN EARLIER OCCURRENCE OPENS WITH ITS DROP-DOWN ON ONE-OFF, AND IT CANNOT BE CHANGED
#     THERE, so it cannot start a second repeat beside the running one. A one-off entry that never
#     repeated stays changeable.
#   - FOLLOW-UP 3, CHOSEN BY THE STAKEHOLDER AGAINST THE RECOMMENDATION. CHANGING THE LATEST OCCURRENCE'S
#     DATE MOVES THE DAY FOR EVERY LATER ONE: the next comes one step after the new date. CHANGING AN
#     EARLIER OCCURRENCE'S DATE CHANGES ONLY THAT ONE. The consequence, stated plainly and accepted: a
#     salary that comes on the 25th once, because the 27th is a Saturday, moves every later salary to the
#     25th, UNLESS THE DATE IS CHANGED BACK BEFORE THE NEXT IS RECORDED. After that, the way back is to
#     change the new latest occurrence's date.
#
# DERIVED BY THE DOCUMENTATION AND NOT PUT TO THE STAKEHOLDER, marked where a scenario rests on one:
#   - The date is not copied: the next date comes from the day the repeat is on and its frequency.
#   - CHANGING THE LATEST'S FREQUENCY starts the new frequency from that occurrence's date.
#   - A change to the latest is judged as if recorded now, so A REFUSED CHANGE LEAVES WHAT FOLLOWS AS IT
#     WAS. CHANGING ONLY THE DROP-DOWN IS A CHANGE, announced as one. SAVING WITH NOTHING CHANGED STAYS
#     QUIET, the drop-down included.
#   - A STOPPED REPEAT STARTS AGAIN by setting its last occurrence back to weekly or monthly. It is then a
#     one-off set to repeat, so what is due since its date is recorded at once (follow-up 4).
#   - Removing an occurrence does not move the schedule, so a removed occurrence is not recorded again.
#
# IN THE DOCUMENTATION'S READING, open to contradiction at this gate:
#   - ABOVE ALL, HOW FOLLOW-UP 3 MEETS RULING 5. THE DAY A MONTHLY REPEAT COMES ON IS THE DAY IT WAS LAST
#     SET TO, by setting the repeat up or by changing the latest occurrence's date, AND NEVER A DATE
#     MONEYBUD CLAMPED. So: a repeat on the 31st, clamped to 28 February, returns to 31 March; changing
#     that 28 February occurrence's amount leaves the day on the 31st; changing its date to the 27th
#     makes the 27th the day; and changing a latest occurrence's date to the 31st makes it a month-end
#     repeat from then on. A weekly repeat moves to the new date's weekday, every 7 days from it.
#   - A NEW DATE FAR ENOUGH BACK records at once what is then already due, as a repeat set up in the
#     past does. A new date ahead delays the next: for an income it may be in the future, for an expense
#     it may not, as when typed.
#   - "THE LATEST OCCURRENCE" IS THE ONE RECORDED MOST RECENTLY, not the one with the latest date, so
#     correcting an older occurrence's date cannot hand the repeat to it.
#   - "AN EARLIER OCCURRENCE" INCLUDES THOSE OF A STOPPED REPEAT: they open on one-off, locked, too. The
#     stopped repeat's last occurrence, the one set to one-off, stays changeable, which is how the repeat
#     is started again.
#   - When the latest is removed, A DAY MOVED ON IT STAYS MOVED, since removing is not changing a date,
#     and WHAT ELSE WAS CHANGED ON IT GOES WITH IT: the next copies the occurrence before it.
#
# RULED BY THE STAKEHOLDER AT THE SCENARIO STAGE, 2026-09-28, each on the recommendation:
#   - REMOVING A STOPPED REPEAT'S LAST OCCURRENCE, the one set to one-off, LEAVES THE REPEAT STOPPED. The
#     occurrence before it becomes its last one: it opens on one-off, CHANGEABLE, so the repeat can be
#     started again from there, and NOTHING IS RECORDED BY ITSELF. Rejected: the occurrence before
#     becoming the latest of a running repeat again, with whatever is due recorded at once; and the
#     repeat staying stopped with every occurrence left locked.
#   - THE GREY LABEL IS SHOWN ONLY IN THE OVERVIEW'S LISTS, NOT IN AN ACCOUNT'S HISTORY. The history step
#     (show-accounts.feature) checks the columns its table has and has none for the label, so this is
#     held by a developer test, not by a scenario.
#   - The drop-down's order, Eenmalig, wekelijks, maandelijks (repeat-an-entry.feature).
#
# NOT specified here: what saving an occurrence does when MoneyBud records the next one while it is
# open in the form, which §12 leaves to the plan; what the removal question says, which is copy; and a
# latest occurrence whose frequency was changed and which was then removed, which follows from the last
# reading above and is not shown separately.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names, labels and amounts are synthetic
# test data.

@recurring
Feature: Change, stop or correct a repeating entry
  As someone whose subscriptions change price and whose salary now and then comes on another day
  I want to change what follows by changing the latest entry, stop a repeat the same way, and correct any single entry without touching the rest
  So that a new price or a cancelled subscription is one change in the form I already use, and what already happened stays as it happened

  # ----------------------------------------------------------------------------------
  # Which row carries the label, and what the drop-down shows (ruling 6, follow-up 2)
  # ----------------------------------------------------------------------------------

  # On 3 October, with the next Netflix due on the 25th, October has no Netflix row yet. The label is
  # on September's row, which is where I would stop the repeat from.
  Scenario: Only the latest occurrence's row carries the label, in the period its date falls in
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 3 October 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And no expenses should be listed in the current budget period
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  | monthly |
    And the expenses listed in the budget period 2 before the current one should be exactly these, in this order:
      | date           | category      | label   | amount | repeats |
      | 25 August 2026 | Subscriptions | Netflix | 13.99  |         |

  # Follow-up 2. Bakker never repeated, so it opens changeable, as every one-off entry does.
  Scenario: The latest occurrence opens with its frequency, changeable; an earlier one opens as a one-off, locked
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Groceries"
    And I have a category "Subscriptions"
    And I have recorded an expense of 20 euro for "Groceries" labelled "Bakker" dated 25 August 2026
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When the day becomes 25 September 2026 while MoneyBud is open
    Then the Overview should show the previous budget period
    And the expense labelled "Netflix" dated 25 August 2026 should open with the frequency one-off, locked
    And the expense labelled "Bakker" dated 25 August 2026 should open with the frequency one-off, changeable
    When I step forward one budget period
    Then the expense labelled "Netflix" dated 25 September 2026 should open with the frequency monthly, changeable

  # ----------------------------------------------------------------------------------
  # The latest occurrence sets the next (ruling 3)
  # ----------------------------------------------------------------------------------

  # The glossary's own example. In November the salary goes up to 2600. I change November's, the row with
  # the label, and December's comes at 2600. August to October stay at 2500: 3 x 2500 + 2 x 2600 = 12700.
  Scenario: A new amount on the latest occurrence is what follows, and the earlier occurrences keep theirs
    Given my budget periods are one month long
    And today is 27 August 2026
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 27 November 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount  |
      | income |          | Salaris | 2500.00 |
      | income |          | Salaris | 2500.00 |
      | income |          | Salaris | 2500.00 |
    When I change the amount of the income labelled "Salaris" dated 27 November 2026 to 2600 euro
    Then the change should go through
    And I should be told that the income was changed
    When the day becomes 27 December 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount  |
      | income |          | Salaris | 2600.00 |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date             | label   | amount  | repeats |
      | 27 December 2026 | Salaris | 2600.00 | monthly |
    And the incomes listed in the previous budget period should be exactly these, in this order:
      | date             | label   | amount  | repeats |
      | 27 November 2026 | Salaris | 2600.00 |         |
    And the balance of "Bank" should be 12700 euro

  # Ruling 3: amount, label, category and account are all copied from the latest occurrence as it is
  # now. Nothing of it is left on Subscriptions or on Bank.
  Scenario: The next occurrence copies the latest's amount, label, category and account, as changed
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have an account "Creditcard" with a starting balance of 0 euro
    And I have a category "Subscriptions"
    And I have a category "Entertainment"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I change the amount of the expense labelled "Netflix" dated 25 August 2026 to 17.99 euro
    And I change the category of the expense labelled "Netflix" dated 25 August 2026 to "Entertainment"
    And I change the account of the expense labelled "Netflix" dated 25 August 2026 to "Creditcard"
    And I change the label of the expense labelled "Netflix" dated 25 August 2026 to "Netflix Premium"
    And I close MoneyBud, and start it again on 25 September 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label           | amount |
      | expense | Entertainment | Netflix Premium | 17.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category      | label           | amount | account    | repeats |
      | 25 September 2026 | Entertainment | Netflix Premium | 17.99  | Creditcard | monthly |
    And the remaining "Subscriptions" budget in the current budget period should be 0.00 euro
    And the balance of "Bank" should be 0.00 euro

  # Derived: changing only the drop-down is a change, announced, and the new frequency counts from the
  # latest occurrence's date. Monthly to weekly: the next comes a week after 3 September.
  Scenario: Changing the latest occurrence from monthly to weekly is a change, and the next comes a week after it
    Given my budget periods are one month long
    And today is 3 September 2026
    And I have a category "Household"
    When I record an expense of 40 euro for "Household" labelled "Schoonmaak", repeating monthly
    And I change the frequency of the expense labelled "Schoonmaak" dated 3 September 2026 to weekly
    Then the change should go through
    And I should be told that the expense was changed
    When the day becomes 9 September 2026 while MoneyBud is open
    Then MoneyBud should not have recorded any repeating entry
    When the day becomes 10 September 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category  | label      | amount |
      | expense | Household | Schoonmaak | 40.00  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category  | label      | amount | repeats |
      | 10 September 2026 | Household | Schoonmaak | 40.00  | weekly  |
      | 3 September 2026  | Household | Schoonmaak | 40.00  |         |

  # The same the other way: weekly to monthly, on the occurrence of 10 September, so the next comes on
  # 10 October, and nothing on the 17th in between.
  Scenario: Changing the latest occurrence from weekly to monthly is a change, and the next comes a month after it
    Given my budget periods are one month long
    And today is 3 September 2026
    And I have a category "Household"
    When I record an expense of 40 euro for "Household" labelled "Schoonmaak", repeating weekly
    And the day becomes 10 September 2026 while MoneyBud is open
    And I change the frequency of the expense labelled "Schoonmaak" dated 10 September 2026 to monthly
    Then the change should go through
    And I should be told that the expense was changed
    When the day becomes 9 October 2026 while MoneyBud is open
    Then MoneyBud should not have recorded any repeating entry
    When the day becomes 10 October 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category  | label      | amount |
      | expense | Household | Schoonmaak | 40.00  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date            | category  | label      | amount | repeats |
      | 10 October 2026 | Household | Schoonmaak | 40.00  | monthly |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date              | category  | label      | amount | repeats |
      | 10 September 2026 | Household | Schoonmaak | 40.00  |         |
      | 3 September 2026  | Household | Schoonmaak | 40.00  |         |

  # Derived: saving with nothing changed stays quiet, the drop-down included, and what follows is as it
  # was.
  Scenario: Saving the latest occurrence without changing anything says nothing, and what follows is as it was
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I save the expense labelled "Netflix" dated 25 August 2026 without changing anything
    Then I should not have been told anything
    When I close MoneyBud, and start it again on 25 September 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  | monthly |

  # Derived: a change to the latest is judged as if recorded now, and a refused one changes nothing, so
  # the next comes as it would have. The refusals are told in recording's own words.
  Scenario Outline: A refused change to the latest occurrence leaves what follows as it was
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I try to change the <field> of the expense labelled "Netflix" dated 25 August 2026 to <new value>
    Then the change should be refused
    And I should be told that <reason>
    When I close MoneyBud, and start it again on 25 September 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  | monthly |

    Examples:
      | field    | new value   | reason                                   |
      | amount   | 0 euro      | an expense must be more than 0 euro      |
      | amount   | 12.345 euro | an amount cannot be finer than a cent    |
      | date     | tomorrow    | an expense cannot be dated in the future |
      | category | "Holiday"   | "Holiday" is not one of my categories    |

  # ----------------------------------------------------------------------------------
  # Stopping a repeat, and starting it again
  # ----------------------------------------------------------------------------------

  # Ruling 3: one-off on the latest occurrence stops the repeat. Its row loses the label, and it stays
  # changeable. The earlier occurrence opens as a one-off, locked (the documentation's reading). Nothing
  # comes in the three months after.
  Scenario: Setting the latest occurrence to one-off stops the repeat, and nothing comes after it
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When the day becomes 25 September 2026 while MoneyBud is open
    And I step forward one budget period
    And I change the frequency of the expense labelled "Netflix" dated 25 September 2026 to one-off
    Then the change should go through
    And I should be told that the expense was changed
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  |         |
    And the expense labelled "Netflix" dated 25 September 2026 should open with the frequency one-off, changeable
    When I step back one budget period
    Then the expense labelled "Netflix" dated 25 August 2026 should open with the frequency one-off, locked
    When I close MoneyBud, and start it again on 26 December 2026
    Then I should not have been told anything
    And no expenses should be listed in the current budget period
    And no expenses should be listed in the previous budget period
    And the balance of "Bank" should be -27.98 euro

  # Derived: the stopped repeat's last occurrence set back to monthly starts it again from its date, and
  # 25 October, due since, is recorded at once (follow-up 4).
  Scenario: A stopped repeat starts again from its last occurrence, and what is due since is recorded at once
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When the day becomes 25 September 2026 while MoneyBud is open
    And I step forward one budget period
    And I change the frequency of the expense labelled "Netflix" dated 25 September 2026 to one-off
    And I close MoneyBud, and start it again on 30 October 2026
    Then I should not have been told anything
    When I step back one budget period
    And I change the frequency of the expense labelled "Netflix" dated 25 September 2026 to monthly
    Then the change should go through
    And I should be told that the expense was changed
    And I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date            | category      | label   | amount | repeats |
      | 25 October 2026 | Subscriptions | Netflix | 13.99  | monthly |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  |         |

  # Ruled at the scenario stage, 2026-09-28 (see the header). September's Netflix stopped the repeat and
  # is then removed. The repeat stays stopped: August's becomes its last occurrence, opens on one-off and
  # changeable, and nothing comes of it, neither at once nor at the next period boundary nor at a later
  # start. Read the other way, 25 September and 25 October would have been recorded again.
  Scenario: Removing a stopped repeat's last occurrence leaves the repeat stopped, and the one before it can start it again
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When the day becomes 25 September 2026 while MoneyBud is open
    And I step forward one budget period
    And I change the frequency of the expense labelled "Netflix" dated 25 September 2026 to one-off
    And I remove the expense labelled "Netflix" dated 25 September 2026 and confirm
    Then I should have been asked to confirm first
    And I should be told that the expense was removed
    And MoneyBud should not have recorded any repeating entry
    And no expenses should be listed in the current budget period
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date           | category      | label   | amount | repeats |
      | 25 August 2026 | Subscriptions | Netflix | 13.99  |         |
    When I step back one budget period
    Then the expense labelled "Netflix" dated 25 August 2026 should open with the frequency one-off, changeable
    When the next budget period begins while MoneyBud is open
    Then MoneyBud should not have recorded any repeating entry
    When I close MoneyBud, and start it again on 26 October 2026
    Then I should not have been told anything
    And no expenses should be listed in the current budget period
    And no expenses should be listed in the previous budget period

  # ----------------------------------------------------------------------------------
  # Correcting an earlier occurrence changes only that one
  # ----------------------------------------------------------------------------------

  # September's Netflix was charged at 15.99 once, a day early. Correcting it is correcting history
  # (follow-up 3): November's comes on the 25th, at 13.99, copying October's.
  Scenario: Correcting an earlier occurrence's amount and date changes only that entry, and the repeat goes on from the latest
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 25 October 2026
    And I step back one budget period
    And I change the amount of the expense labelled "Netflix" dated 25 September 2026 to 15.99 euro
    And I change the date of the expense labelled "Netflix" dated 25 September 2026 to 24 September 2026
    Then the change should go through
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 24 September 2026 | Subscriptions | Netflix | 15.99  |         |
    When I close MoneyBud, and start it again on 25 November 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date             | category      | label   | amount | repeats |
      | 25 November 2026 | Subscriptions | Netflix | 13.99  | monthly |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date            | category      | label   | amount | repeats |
      | 25 October 2026 | Subscriptions | Netflix | 13.99  |         |

  # The documentation's reading (see the header): the latest is the one recorded most recently. August's
  # Netflix, moved to 28 September, now has the latest date, but the label stays on 25 September's row
  # and the repeat still goes on from it.
  Scenario: Moving an earlier occurrence's date past the latest's does not make it the latest
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 30 September 2026
    And I step back one budget period
    And I change the date of the expense labelled "Netflix" dated 25 August 2026 to 28 September 2026
    Then the change should go through
    And I should be told that the expense went into the current budget period
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 28 September 2026 | Subscriptions | Netflix | 13.99  |         |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  | monthly |
    When I close MoneyBud, and start it again on 25 October 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date            | category      | label   | amount | repeats |
      | 25 October 2026 | Subscriptions | Netflix | 13.99  | monthly |

  # ----------------------------------------------------------------------------------
  # Changing the latest occurrence's date moves the day (follow-up 3)
  # ----------------------------------------------------------------------------------

  # The stakeholder's own case, and the consequence he accepted. October's salary came on the 25th, as
  # the 27th was a Saturday, and I correct its date. Being the latest, that moves the day: November's and
  # December's come on the 25th too.
  Scenario: Changing the latest occurrence's date moves the day for every later one, even for a date that was a one-off
    Given my budget periods are one month long
    And today is 27 August 2026
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 27 October 2026
    And I change the date of the income labelled "Salaris" dated 27 October 2026 to 25 October 2026
    Then the change should go through
    And I should be told that the income was changed
    When I close MoneyBud, and start it again on 25 November 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount  |
      | income |          | Salaris | 2500.00 |
    When I close MoneyBud, and start it again on 25 December 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount  |
      | income |          | Salaris | 2500.00 |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date             | label   | amount  | repeats |
      | 25 December 2026 | Salaris | 2500.00 | monthly |
    And the incomes listed in the previous budget period should be exactly these, in this order:
      | date             | label   | amount  | repeats |
      | 25 November 2026 | Salaris | 2500.00 |         |

  # Follow-up 3's escape: the date changed back before the next is recorded puts the day back.
  Scenario: A latest occurrence's date changed back before the next is recorded leaves the day as it was
    Given my budget periods are one month long
    And today is 27 August 2026
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 27 October 2026
    And I change the date of the income labelled "Salaris" dated 27 October 2026 to 25 October 2026
    And I change the date of the income labelled "Salaris" dated 25 October 2026 to 27 October 2026
    And the day becomes 25 November 2026 while MoneyBud is open
    Then MoneyBud should not have recorded any repeating entry
    When the day becomes 27 November 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount  |
      | income |          | Salaris | 2500.00 |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date             | label   | amount  | repeats |
      | 27 November 2026 | Salaris | 2500.00 | monthly |

  # Once November's has been recorded on the 25th, putting October's back to the 27th corrects October
  # only, and December's still comes on the 25th. The way back is on the new latest: December's changed
  # to the 27th (an income may be dated ahead) puts January's on the 27th.
  Scenario: Once the next has been recorded, the day is moved back on the new latest occurrence, not on the one before it
    Given my budget periods are one month long
    And today is 27 August 2026
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 27 October 2026
    And I change the date of the income labelled "Salaris" dated 27 October 2026 to 25 October 2026
    And I close MoneyBud, and start it again on 25 November 2026
    And I step back one budget period
    And I change the date of the income labelled "Salaris" dated 25 October 2026 to 27 October 2026
    And I close MoneyBud, and start it again on 25 December 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount  |
      | income |          | Salaris | 2500.00 |
    When I change the date of the income labelled "Salaris" dated 25 December 2026 to 27 December 2026
    And I close MoneyBud, and start it again on 25 January 2027
    Then I should not have been told anything
    When the day becomes 27 January 2027 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount  |
      | income |          | Salaris | 2500.00 |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date            | label   | amount  | repeats |
      | 27 January 2027 | Salaris | 2500.00 | monthly |
    And the incomes listed in the previous budget period should be exactly these, in this order:
      | date             | label   | amount  | repeats |
      | 27 December 2026 | Salaris | 2500.00 |         |

  # The documentation's reading: weekly moves to the new date's weekday, every 7 days from it.
  Scenario: Changing a weekly repeat's latest date moves it to that weekday, every 7 days from it
    Given my budget periods are one month long
    And today is 3 September 2026
    And I have a category "Groceries"
    And I have recorded an expense of 45 euro for "Groceries" labelled "Markt" dated 3 September 2026, repeating weekly
    When the day becomes 10 September 2026 while MoneyBud is open
    And I change the date of the expense labelled "Markt" dated 10 September 2026 to 9 September 2026
    Then the change should go through
    When the day becomes 15 September 2026 while MoneyBud is open
    Then MoneyBud should not have recorded any repeating entry
    When the day becomes 16 September 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category  | label | amount |
      | expense | Groceries | Markt | 45.00  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category  | label | amount | repeats |
      | 16 September 2026 | Groceries | Markt | 45.00  | weekly  |
      | 9 September 2026  | Groceries | Markt | 45.00  |         |
      | 3 September 2026  | Groceries | Markt | 45.00  |         |

  # The documentation's reading: a typo in the first entry's date is fixed on that entry while it is
  # still the latest. Huur was set up today, meant as 1 September: moved back, 1 October is due at once.
  Scenario: A latest occurrence's date moved far enough back records at once what is then already due
    Given my budget periods are one month long
    And today is 5 October 2026
    And I have a category "Rent"
    When I record an expense of 900 euro for "Rent" labelled "Huur", repeating monthly
    And I change the date of the expense labelled "Huur" dated 5 October 2026 to 1 September 2026
    Then the change should go through
    And I should be told that the expense was changed
    And I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category | label | amount |
      | expense | Rent     | Huur  | 900.00 |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date           | category | label | amount | repeats |
      | 1 October 2026 | Rent     | Huur  | 900.00 | monthly |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date             | category | label | amount | repeats |
      | 1 September 2026 | Rent     | Huur  | 900.00 |         |

  # ----------------------------------------------------------------------------------
  # The day of a month-end repeat (follow-up 3 read with ruling 5)
  #
  # The documentation's reading, open to contradiction at this gate: the day is the one it was last set
  # to, by me, and never a date MoneyBud clamped.
  # ----------------------------------------------------------------------------------

  # 28 February is the latest occurrence, but its date was clamped, not changed. Changing its amount is
  # not changing its date, so March's comes on the 31st, at the new amount.
  Scenario: Changing a clamped occurrence's amount leaves a month-end repeat on the 31st
    Given my budget periods are one month long
    And today is 31 January 2027
    And I have a category "Rent"
    And I have recorded an expense of 900 euro for "Rent" labelled "Huur" dated 31 January 2027, repeating monthly
    When I close MoneyBud, and start it again on 28 February 2027
    And I change the amount of the expense labelled "Huur" dated 28 February 2027 to 950 euro
    And I close MoneyBud, and start it again on 30 March 2027
    Then I should not have been told anything
    When the day becomes 31 March 2027 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category | label | amount |
      | expense | Rent     | Huur  | 950.00 |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date          | category | label | amount | repeats |
      | 31 March 2027 | Rent     | Huur  | 950.00 | monthly |

  # Changing the clamped occurrence's date is changing the latest's date: the 27th is the day from then on.
  Scenario: Changing a clamped occurrence's date makes the new date's day the day
    Given my budget periods are one month long
    And today is 31 January 2027
    And I have a category "Rent"
    And I have recorded an expense of 900 euro for "Rent" labelled "Huur" dated 31 January 2027, repeating monthly
    When I close MoneyBud, and start it again on 28 February 2027
    And I change the date of the expense labelled "Huur" dated 28 February 2027 to 27 February 2027
    And I close MoneyBud, and start it again on 27 March 2027
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category | label | amount |
      | expense | Rent     | Huur  | 900.00 |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date          | category | label | amount | repeats |
      | 27 March 2027 | Rent     | Huur  | 900.00 | monthly |

  # Set up on the 30th, then corrected to the 31st while still the latest: a month-end repeat from then
  # on. April has no 31st, so April's comes on the 30th, and May's returns to the 31st, not the 30th.
  Scenario: Changing the latest occurrence's date to the 31st makes it a month-end repeat from then on
    Given my budget periods are one month long
    And today is 31 March 2027
    And I have a category "Rent"
    When I record an expense of 900 euro for "Rent" labelled "Huur" dated 30 March 2027, repeating monthly
    And I change the date of the expense labelled "Huur" dated 30 March 2027 to 31 March 2027
    And I close MoneyBud, and start it again on 30 April 2027
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category | label | amount |
      | expense | Rent     | Huur  | 900.00 |
    When I close MoneyBud, and start it again on 30 May 2027
    Then I should not have been told anything
    When the day becomes 31 May 2027 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category | label | amount |
      | expense | Rent     | Huur  | 900.00 |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date        | category | label | amount | repeats |
      | 31 May 2027 | Rent     | Huur  | 900.00 | monthly |

  # ----------------------------------------------------------------------------------
  # Removing an occurrence removes only that one (ruling 4, follow-up 1)
  # ----------------------------------------------------------------------------------

  # September's Netflix was never charged. Removing it asks first, removes it alone, and the repeat
  # carries on from October's.
  Scenario: Removing an earlier occurrence asks first and removes only that one, and the repeat carries on
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 25 October 2026
    And I step back one budget period
    And I remove the expense labelled "Netflix" dated 25 September 2026 and confirm
    Then I should have been asked to confirm first
    And I should be told that the expense was removed
    And no expenses should be listed in the previous budget period
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date            | category      | label   | amount | repeats |
      | 25 October 2026 | Subscriptions | Netflix | 13.99  | monthly |
    When I close MoneyBud, and start it again on 25 November 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |

  # Follow-up 1. With October's removed, September's is the latest: it carries the label and opens with
  # the frequency. The next date does not move, so November's comes on the 25th, and October's is not
  # recorded again.
  Scenario: Removing the latest occurrence makes the one recorded before it the latest, and the next comes when it would have
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 26 October 2026
    And I remove the expense labelled "Netflix" dated 25 October 2026 and confirm
    Then I should have been asked to confirm first
    And I should be told that the expense was removed
    And no expenses should be listed in the current budget period
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  | monthly |
    When I step back one budget period
    Then the expense labelled "Netflix" dated 25 September 2026 should open with the frequency monthly, changeable
    When I close MoneyBud, and start it again on 25 November 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date             | category      | label   | amount | repeats |
      | 25 November 2026 | Subscriptions | Netflix | 13.99  | monthly |
    And no expenses should be listed in the previous budget period

  # The documentation's reading (see the header). October's was given a new price and moved to the 20th,
  # then removed. The new price goes with it: November's copies September's 13.99. The moved day stays:
  # November's comes on the 20th.
  Scenario: When the latest occurrence is removed, what was changed on it goes with it, but a day moved on it stays moved
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 26 October 2026
    And I change the amount of the expense labelled "Netflix" dated 25 October 2026 to 15.99 euro
    And I change the date of the expense labelled "Netflix" dated 25 October 2026 to 20 October 2026
    And I remove the expense labelled "Netflix" dated 20 October 2026 and confirm
    And I close MoneyBud, and start it again on 20 November 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date             | category      | label   | amount | repeats |
      | 20 November 2026 | Subscriptions | Netflix | 13.99  | monthly |

  # Follow-up 1: removing the only occurrence is almost always undoing a mistake, and ends the repeat.
  # Nothing is left of it, so the category has no history and can be deleted.
  Scenario: Removing the only occurrence ends the repeat
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I remove the expense labelled "Netflix" dated 25 August 2026 and confirm
    Then I should have been asked to confirm first
    And I should be told that the expense was removed
    And no expenses should be listed in the current budget period
    When I close MoneyBud, and start it again on 25 October 2026
    Then I should not have been told anything
    And no expenses should be listed in the current budget period
    And no expenses should be listed in the previous budget period
    And I should be able to delete the category "Subscriptions"
