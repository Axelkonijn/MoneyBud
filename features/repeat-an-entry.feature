# Repeating an entry: an income or an expense set to repeat, weekly or monthly, in one drop-down on its
# form, and recorded again by MoneyBud on every date it falls on (glossary: "Recurring entries", settled
# by the stakeholder on 2026-09-28; Recurring transaction, Occurrence, Frequency). Changing, stopping and
# correcting a repeat, and which row carries its label, are in change-a-repeat.feature. THIS FILE
# EXPLAINS THE STEPS THE TWO SHARE.
#
# The stakeholder's words, 2026-09-28: "Recurring entries on income and spending. This should just be a
# extra drop down where you can choose default one time, or weekly or monthly."
#
# The rules, from arc42 §12:
#   - RULING 1. The income form and the expense form get one more field, on screen "Herhalen", offering
#     "Eenmalig", THE DEFAULT, "wekelijks" and "maandelijks". An entry left at Eenmalig is exactly the
#     one-off entry of every earlier file, so none of them changes. Yearly is deferred until missed.
#   - RULING 2. EACH OCCURRENCE IS RECORDED ON ITS OWN DATE, AS AN ORDINARY ENTRY, THE FIRST TIME
#     MONEYBUD RUNS ON OR AFTER THAT DATE. Occurrences missed while MoneyBud was closed are ALL recorded
#     at the next start, each on its own date. ONE RULE FOR INCOMES AND EXPENSES, so an expense is still
#     never dated in the future and never recorded ahead. Rejected: recording an income ahead when its
#     period begins. THE COST, accepted: a repeated salary counts in its period's Unassigned only from
#     its date.
#   - RULING 5. A monthly repeat keeps its day of the month. A month too short for that day gets it on
#     its last day, and the month after has it back: started on 31 January, it comes on 28 February
#     (29 in a leap year) and 31 March. Rejected: staying on the 28th once there.
#   - RULING 7. What MoneyBud records by itself is SAID ONCE, IN ONE NOTICE, naming what was added, such
#     as "Herhaald: Netflix € 13,99, Salaris € 2.500,00." Several at one start go in the same notice.
#     They are SAVED STRAIGHT AWAY, as a sweep is, so starting again neither records them nor says them
#     again. Rejected: saying nothing.
#   - RULING 8. An occurrence on a category archived since IS RECORDED, BRINGS THE CATEGORY BACK, AND I
#     AM TOLD, as when I record an expense against it myself. Rejected: archiving stops every repeat on it.
#   - FOLLOW-UP 4. A repeat SET UP IN THE PAST, typed back-dated or a one-off changed to repeat, records
#     EVERY OCCURRENCE ALREADY DUE AT ONCE, each on its own date, in one notice. The cost, accepted: a
#     weekly entry dated months back records many entries at once.
#   - FOLLOW-UP 5. SETTLING WORKS THROUGH THE DAYS IN ORDER, as if MoneyBud had been open: the
#     occurrences dated in a period are recorded BEFORE THAT PERIOD IS SWEPT, so its sweep has them.
#     Rejected: sweeping first, which would leave a difference to bring up to date after every month
#     I was away.
#   - FOLLOW-UP 6. Herhalen is THE FORM'S LAST FIELD, after Rekening. That is held by the developer test
#     that reads the window's markup, as the rest of the field order is, and NOT BY A SCENARIO HERE.
#
# DERIVED BY THE DOCUMENTATION AND NOT PUT TO THE STAKEHOLDER, marked where a scenario rests on one:
#   - THE ENTRY THE DROP-DOWN IS SET ON IS THE FIRST OCCURRENCE. WEEKLY IS EVERY 7 DAYS from its date;
#     MONTHLY IS THE SAME DAY EACH MONTH. The 29th and the 30th clamp as the 31st does. An income typed
#     ahead and set to repeat is its own first occurrence, and the next comes one step after its date.
#   - AN OCCURRENCE IS AN ORDINARY INCOME OR EXPENSE. It counts in its period's figures, its account's
#     balance, Unassigned and Remaining exactly as a typed one does, is listed in its period, and is
#     changed and removed like any other. IT IS NEVER REFUSED.
#   - It copies the latest occurrence's amount, label, category and ACCOUNT: THE ACCOUNT IT WAS ON, not
#     the pool account of the day. The category is copied as the category, so A RENAME CARRIES OVER.
#   - "Runs on or after that date" is SETTLING: the first thing MoneyBud does when it opens, at every
#     tick of its minute's timer, and before every act. So an occurrence dated today is recorded before
#     anything I type today, and A BALANCE CORRECTION TYPED THAT DAY HAS IT IN IT. One dated back by a
#     repeat set up late is in an earlier balance correction, like any entry remembered late.
#
# IN THE DOCUMENTATION'S READING, open to contradiction at this gate:
#   - Occurrences are said WHATEVER DAY THEY FALL ON, A PERIOD'S FIRST DAY INCLUDED. That is a second
#     exception, after the sweep, to "nothing is announced when a new period begins"
#     (step-between-periods.feature).
#   - An occurrence recorded while MoneyBud is open is said at that tick, and, by the sweep's precedent,
#     that notice drops a removal question still waiting for an answer.
#   - AN OCCURRENCE LANDING IN A PERIOD OTHER THAN THE ONE ON SCREEN LEAVES THE SCREEN WHERE IT IS, as
#     any entry does.
#   - AN EXPENSE OCCURRENCE WITH NO LABEL is named in the notice by its category.
#
# RULED BY THE STAKEHOLDER AT THE SCENARIO STAGE, 2026-09-28, each on the recommendation:
#   - THE DROP-DOWN OFFERS ITS CHOICES IN THE ORDER Eenmalig, wekelijks, maandelijks, as ruling 1 names
#     them. Rejected: the most-used first.
#   - THE GREY LABEL IS SHOWN ONLY IN THE OVERVIEW'S LISTS, never in an account's history, where entries
#     are listed but not changed. The history step has no column for it, so that is left to a developer
#     test (change-a-repeat.feature).
#   - Removing a stopped repeat's last occurrence LEAVES THE REPEAT STOPPED (change-a-repeat.feature).
#
# NOT in this increment: yearly; repeating a transfer, an assignment or a balance correction (derived);
# a list of repeats to manage them from (rejected, ruling 3). NOT specified here: the clock turned back,
# or set wrongly ahead, both derived in §12 "What else an occurrence meets"; the words of the notice and
# the order it names things in, which are copy; and where Herhalen sits on the form (above).
#
# DATES. For the first time in these files, dates are named as CALENDAR DATES: a monthly repeat keeps a
# day of the month, which no phrase relative to a budget period can name. The period start day is still
# fixed at the 1st, so here a budget period is a calendar month, and periods are still named relative to
# today, as everywhere else. The years are chosen: 2027 has a 28 February, and 2028 a 29th.
#
# Reading the steps. These are shared by repeat-an-entry.feature, change-a-repeat.feature and the
# scenarios about repeats in keep-data.feature:
#   - "today is 25 August 2026" is the day the scenario begins. THE EMPTY LEDGER COUNTS AS FIRST STARTED
#     THAT DAY, so a period is swept by itself only if it ends after it (sweep-at-a-period-end.feature).
#   - "..., repeating monthly" ("weekly"), at the end of a step that records an income or an expense,
#     means the Herhalen drop-down was set so before recording. Without it, the drop-down is left as it
#     starts out, on Eenmalig. As a Given, "I have recorded ... dated ..., repeating monthly" was done
#     earlier today, in the order of the Givens. NO GIVEN SETS UP A REPEAT WITH AN OCCURRENCE ALREADY DUE:
#     where that is the point, a When does it.
#   - The English "one-off", "weekly" and "monthly" stand for Eenmalig, wekelijks and maandelijks.
#   - "a new expense (income) should start out as a one-off" is what the drop-down shows before I touch
#     it. "the frequencies offered for a new expense (income) should be exactly these, in this order" is
#     the whole drop-down, in its order.
#   - "the day becomes 25 September 2026 while MoneyBud is open" moves the clock on to that day without
#     MoneyBud being closed, and the minute's timer ticks, as "the next budget period begins while
#     MoneyBud is open" (step-between-periods.feature) does. "I close MoneyBud, and start it again on
#     29 October 2026" is keep-data.feature's step, on a named day. After either, periods are named
#     relative to the new today.
#   - "I should be told, in one notice, that these repeating entries were recorded" means ONE notice
#     names EVERY occurrence MoneyBud recorded by itself just then, one table row each, and no other, in
#     any order. ENTRY is income or expense; CATEGORY is an expense's, blank for an income; LABEL and
#     AMOUNT are the occurrence's own, and a blank LABEL is an expense with none. What is fixed is what I
#     am told, never the wording, nor whether dates are named. Anything else said at the same time, a
#     sweep, a category brought back or the outcome of my own act, is its own step's.
#   - "MoneyBud should not have recorded any repeating entry" means that since the step before, MoneyBud
#     recorded nothing by itself and said nothing about it. It is used where a notice from my own act may
#     still be showing. After a start, "I should not have been told anything" (step-between-periods.feature)
#     is used instead: no message of any kind.
#   - An entry is named as change-an-entry.feature names it, by what its row shows, with ITS DATE ADDED:
#     "the expense labelled "Netflix" dated 25 September 2026". Occurrences share a label, so the date
#     says which one. As in that file, the entry is picked from the period on screen, and a scenario
#     steps to another period first. change-an-entry.feature's and remove-an-entry.feature's steps take
#     this name unchanged: "I change the amount of ... to ...", "I remove ... and confirm", "I save ...
#     without changing anything".
#   - "I change the frequency of the expense (income) labelled "X" dated D to monthly (weekly, one-off)"
#     is choosing that in the drop-down of the entry opened for changing, and saving it.
#   - "the expense (income) labelled "X" dated D should open with the frequency monthly, changeable" is
#     what the drop-down shows when I click that row, and that I can change it. "..., locked" means it
#     shows that frequency and I cannot change it.
#   - The list steps are list-transactions-in-a-period.feature's, with dates as calendar dates and ONE
#     NEW COLUMN: REPEATS is the small grey label a row carries, "monthly" or "weekly", and a BLANK CELL
#     MEANS THE ROW CARRIES NONE. A table without the column does not check it, which keeps every earlier
#     list as it was. keep-data.feature's "the expenses listed in the budget period 2 before the current
#     one" takes the column too.
#   - "I should be told that "X" was brought back" is record-expense.feature's, said here with the
#     occurrences.
#   - Every other step is reused unchanged from the file that introduced it: the account and history
#     steps are show-accounts.feature's, the balance correction steps correct-a-balance.feature's, the
#     backing steps back-a-category.feature's, and the sweep steps sweep-at-a-period-end.feature's.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). No sweep destination is set unless a
# scenario sets one. The names, labels and amounts are synthetic test data.

@recurring
Feature: Repeat an income or an expense
  As someone whose salary, rent and subscriptions come back every week or every month
  I want to set an income or an expense to repeat, once, when I enter it
  So that MoneyBud records each one on its own date by itself, and I never type the same entry twice

  # ----------------------------------------------------------------------------------
  # The drop-down
  # ----------------------------------------------------------------------------------

  # Ruling 1. The order of the three was ruled at the scenario stage, 2026-09-28 (see the header).
  Scenario: A new income and a new expense start out as a one-off, with weekly and monthly on offer
    Given MoneyBud is open
    Then a new expense should start out as a one-off
    And the frequencies offered for a new expense should be exactly these, in this order:
      | frequency |
      | one-off   |
      | weekly    |
      | monthly   |
    And a new income should start out as a one-off
    And the frequencies offered for a new income should be exactly these, in this order:
      | frequency |
      | one-off   |
      | weekly    |
      | monthly   |

  # Left at one-off, an entry is the one-off entry of every earlier file: nothing comes of it later.
  Scenario: An income or an expense left at one-off is recorded once, and never again
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    When I record an expense of 13.99 euro for "Subscriptions" labelled "Netflix"
    And I record an income of 2500 euro labelled "Salaris"
    And I close MoneyBud, and start it again on 26 September 2026
    Then I should not have been told anything
    And no expenses should be listed in the current budget period
    And no incomes should be listed in the current budget period
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date           | category      | label   | amount | repeats |
      | 25 August 2026 | Subscriptions | Netflix | 13.99  |         |
    And the incomes listed in the previous budget period should be exactly these, in this order:
      | date           | label   | amount  | repeats |
      | 25 August 2026 | Salaris | 2500.00 |         |

  # ----------------------------------------------------------------------------------
  # An occurrence is recorded on its own date, as an ordinary entry
  # ----------------------------------------------------------------------------------

  # Ruling 2, weekly and monthly. Both rows cross a period boundary while MoneyBud stays open. The day
  # before, nothing has been recorded, although the new period has begun. On the day, the occurrence is
  # an ordinary expense: listed in its period, against its category's Remaining, off the balance. The
  # screen stays on the period it showed (the documentation's reading), and only the new row carries
  # the label (change-a-repeat.feature).
  Scenario Outline: An expense set to repeat is recorded again on its next date, and not a day before, as an ordinary expense, and I am told
    Given my budget periods are one month long
    And today is <first>
    And I have a category "Sport"
    When I record an expense of 12.50 euro for "Sport" labelled "Contributie", repeating <frequency>
    And the day becomes <day before> while MoneyBud is open
    Then MoneyBud should not have recorded any repeating entry
    And no expenses should be listed in the current budget period
    When the day becomes <next> while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category | label       | amount |
      | expense | Sport    | Contributie | 12.50  |
    And I should not be warned or asked to confirm
    And the Overview should show the previous budget period
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date   | category | label       | amount | repeats     |
      | <next> | Sport    | Contributie | 12.50  | <frequency> |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date    | category | label       | amount | repeats |
      | <first> | Sport    | Contributie | 12.50  |         |
    And the remaining "Sport" budget in the current budget period should be -12.50 euro
    And the balance of "Bank" should be -25.00 euro

    Examples:
      | frequency | first          | day before        | next              |
      | weekly    | 27 August 2026 | 2 September 2026  | 3 September 2026  |
      | monthly   | 25 August 2026 | 24 September 2026 | 25 September 2026 |

  # The same rule for an income. Its cost is on the page: the new period's Unassigned has none of it
  # until its date, even though the period has begun (ruling 2, "What that costs").
  Scenario Outline: An income set to repeat is recorded again on its next date, and counts in Unassigned only from then
    Given my budget periods are one month long
    And today is <first>
    When I record an income of 85 euro labelled "Bijbaan", repeating <frequency>
    And the day becomes <day before> while MoneyBud is open
    Then MoneyBud should not have recorded any repeating entry
    And no incomes should be listed in the current budget period
    And Unassigned in the current budget period should be 0 euro
    When the day becomes <next> while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount |
      | income |          | Bijbaan | 85.00  |
    And the Overview should show the previous budget period
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date   | label   | amount | repeats     |
      | <next> | Bijbaan | 85.00  | <frequency> |
    And the incomes listed in the previous budget period should be exactly these, in this order:
      | date    | label   | amount | repeats |
      | <first> | Bijbaan | 85.00  |         |
    And Unassigned in the current budget period should be 85 euro
    And the balance of "Bank" should be 170 euro

    Examples:
      | frequency | first          | day before        | next              |
      | weekly    | 27 August 2026 | 2 September 2026  | 3 September 2026  |
      | monthly   | 25 August 2026 | 24 September 2026 | 25 September 2026 |

  # The documentation's reading (see the header): an occurrence on a period's first day is said at the
  # boundary, although nothing else about a new period is.
  Scenario: An occurrence on a period's first day is said when the period begins
    Given my budget periods are one month long
    And today is 1 September 2026
    And I have a category "Rent"
    When I record an expense of 900 euro for "Rent" labelled "Huur", repeating monthly
    And the next budget period begins while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category | label | amount |
      | expense | Rent     | Huur  | 900.00 |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date           | category | label | amount | repeats |
      | 1 October 2026 | Rent     | Huur  | 900.00 | monthly |

  # The glossary's own example, with synthetic names. MoneyBud was last open on 27 August and is not
  # opened again until 29 October. September's and October's Netflix and Salaris are all recorded at
  # that start, each on its own date and in its own period, and all four are said in one notice.
  Scenario: After MoneyBud was closed for weeks, every occurrence missed is recorded on its own date, and all are said in one notice
    Given my budget periods are one month long
    And today is 27 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 29 October 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount  |
      | expense | Subscriptions | Netflix | 13.99   |
      | income  |               | Salaris | 2500.00 |
      | expense | Subscriptions | Netflix | 13.99   |
      | income  |               | Salaris | 2500.00 |
    And the Overview should show the current budget period
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date            | label   | amount  | repeats |
      | 27 October 2026 | Salaris | 2500.00 | monthly |
    And the incomes listed in the previous budget period should be exactly these, in this order:
      | date              | label   | amount  | repeats |
      | 27 September 2026 | Salaris | 2500.00 |         |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date            | category      | label   | amount | repeats |
      | 25 October 2026 | Subscriptions | Netflix | 13.99  | monthly |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  |         |
    And the expenses listed in the budget period 2 before the current one should be exactly these, in this order:
      | date           | category      | label   | amount | repeats |
      | 25 August 2026 | Subscriptions | Netflix | 13.99  |         |
    And Unassigned in the previous budget period should be 2500 euro
    And the balance of "Bank" should be 7458.03 euro

  # Ruling 7: saved straight away, so said exactly once, whether it was recorded while MoneyBud was open
  # or when it started. Starting again on the same day, with nothing done in between, neither says it
  # again nor records it twice.
  Scenario: An occurrence is said once, and starting MoneyBud again neither says it again nor records it twice
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When the day becomes 25 September 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    When I close MoneyBud and start it again
    Then I should not have been told anything
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  | monthly |
    When I close MoneyBud, and start it again on 25 October 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    When I close MoneyBud and start it again
    Then I should not have been told anything
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date            | category      | label   | amount | repeats |
      | 25 October 2026 | Subscriptions | Netflix | 13.99  | monthly |
    And the balance of "Bank" should be -41.97 euro

  # Ruling 2: nothing is recorded ahead, for incomes as for expenses, not even once the period it falls
  # in has begun. So the new period's Unassigned has no salary in it until the 27th: the cost the
  # stakeholder accepted, until the start day can be set to payday.
  Scenario: Nothing is recorded ahead of its date, so a repeated salary is in its period's Unassigned only from its date
    Given my budget periods are one month long
    And today is 27 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 1 September 2026
    Then I should not have been told anything
    And no expenses should be listed in the current budget period
    And no incomes should be listed in the current budget period
    And Unassigned in the current budget period should be 0 euro
    When the day becomes 25 September 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And Unassigned in the current budget period should still be 0 euro
    When the day becomes 27 September 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount  |
      | income |          | Salaris | 2500.00 |
    And Unassigned in the current budget period should be 2500 euro

  # Derived. Typing an income ahead is unchanged: it is its own first occurrence, counted in Unassigned
  # from the moment it is typed and in the balance from its date. Its date arriving records nothing new.
  # The next comes a month after it.
  Scenario: An income typed ahead and set to repeat is its own first occurrence, and the next comes a month after its date
    Given my budget periods are one month long
    And today is 20 August 2026
    When I record an income of 2500 euro labelled "Salaris" dated 27 August 2026, repeating monthly
    Then the income should be recorded
    And Unassigned in the current budget period should be 2500 euro
    And the balance of "Bank" should be 0.00 euro
    When the day becomes 27 August 2026 while MoneyBud is open
    Then MoneyBud should not have recorded any repeating entry
    And the balance of "Bank" should be 2500 euro
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date           | label   | amount  | repeats |
      | 27 August 2026 | Salaris | 2500.00 | monthly |
    When the day becomes 27 September 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount  |
      | income |          | Salaris | 2500.00 |
    And the balance of "Bank" should be 5000 euro
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date              | label   | amount  | repeats |
      | 27 September 2026 | Salaris | 2500.00 | monthly |
    And the incomes listed in the previous budget period should be exactly these, in this order:
      | date           | label   | amount  | repeats |
      | 27 August 2026 | Salaris | 2500.00 |         |

  # Ruling 2: the refusal of a future-dated expense stands, repeating or not. Refused, nothing is set up,
  # so nothing comes of it later either.
  Scenario: An expense dated in the future is refused, set to repeat or not, and nothing comes of it later
    Given my budget periods are one month long
    And today is 24 August 2026
    And I have a category "Subscriptions"
    When I try to record an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    Then the expense should not be recorded
    And I should be told that an expense cannot be dated in the future
    When I close MoneyBud, and start it again on 26 September 2026
    Then I should not have been told anything
    And no expenses should be listed in the current budget period
    And no expenses should be listed in the previous budget period

  # ----------------------------------------------------------------------------------
  # A repeat set up in the past records what is already due, at once (follow-up 4)
  # ----------------------------------------------------------------------------------

  # Last months' salaries entered on a day of first use. 27 August and 27 September are due and
  # recorded at once, each in its own period; 27 October is not yet.
  Scenario: A monthly income typed back-dated records every occurrence already due at once, each on its own date, in one notice
    Given my budget periods are one month long
    And today is 10 October 2026
    When I record an income of 2500 euro labelled "Salaris" dated 27 July 2026, repeating monthly
    Then the income should be recorded
    And I should be told, in one notice, that these repeating entries were recorded:
      | entry  | category | label   | amount  |
      | income |          | Salaris | 2500.00 |
      | income |          | Salaris | 2500.00 |
    And no incomes should be listed in the current budget period
    And the incomes listed in the previous budget period should be exactly these, in this order:
      | date              | label   | amount  | repeats |
      | 27 September 2026 | Salaris | 2500.00 | monthly |
    And the balance of "Bank" should be 7500 euro

  # The accepted cost of follow-up 4, on a small scale: four weeks at once, each named. The last is
  # dated today, and today counts as "on or after".
  Scenario: A weekly expense typed back-dated records every week already due at once, today's included
    Given my budget periods are one month long
    And today is 1 October 2026
    And I have a category "Groceries"
    When I record an expense of 45 euro for "Groceries" labelled "Markt" dated 3 September 2026, repeating weekly
    Then the expense should be recorded
    And I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category  | label | amount |
      | expense | Groceries | Markt | 45.00  |
      | expense | Groceries | Markt | 45.00  |
      | expense | Groceries | Markt | 45.00  |
      | expense | Groceries | Markt | 45.00  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date           | category  | label | amount | repeats |
      | 1 October 2026 | Groceries | Markt | 45.00  | weekly  |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date              | category  | label | amount | repeats |
      | 24 September 2026 | Groceries | Markt | 45.00  |         |
      | 17 September 2026 | Groceries | Markt | 45.00  |         |
      | 10 September 2026 | Groceries | Markt | 45.00  |         |
      | 3 September 2026  | Groceries | Markt | 45.00  |         |
    And the balance of "Bank" should be -225.00 euro

  # Follow-up 4, the other way of setting up in the past: a one-off already recorded is changed to
  # repeat. It starts from its own date, as if set so when recorded (derived).
  Scenario: A one-off entry changed to repeat starts from its own date, and records at once what is already due
    Given my budget periods are one month long
    And today is 30 September 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026
    And the Overview shows the previous budget period
    When I change the frequency of the expense labelled "Netflix" dated 25 August 2026 to monthly
    Then the change should go through
    And I should be told that the expense was changed
    And I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  | monthly |
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date           | category      | label   | amount | repeats |
      | 25 August 2026 | Subscriptions | Netflix | 13.99  |         |

  # ----------------------------------------------------------------------------------
  # A monthly repeat on a day a month is too short for (ruling 5)
  # ----------------------------------------------------------------------------------

  # The short month gets it on its last day, and the month after has it back on its own day, not on
  # the clamped one: on "not yet on" nothing comes. The 29th and the 30th behave as the 31st (derived).
  # 29 January 2028 is the row with nothing to clamp: 2028 is a leap year.
  Scenario Outline: A monthly repeat falls on a short month's last day, and returns to its own day the month after
    Given my budget periods are one month long
    And today is <started on>
    And I have a category "Rent"
    When I record an expense of 900 euro for "Rent" labelled "Huur", repeating monthly
    And I close MoneyBud, and start it again on <short month>
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category | label | amount |
      | expense | Rent     | Huur  | 900.00 |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date          | category | label | amount | repeats |
      | <short month> | Rent     | Huur  | 900.00 | monthly |
    When the day becomes <not yet on> while MoneyBud is open
    Then MoneyBud should not have recorded any repeating entry
    When the day becomes <then on> while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category | label | amount |
      | expense | Rent     | Huur  | 900.00 |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date      | category | label | amount | repeats |
      | <then on> | Rent     | Huur  | 900.00 | monthly |

    Examples:
      | started on      | short month      | not yet on    | then on       |
      | 31 January 2027 | 28 February 2027 | 30 March 2027 | 31 March 2027 |
      | 30 January 2027 | 28 February 2027 | 29 March 2027 | 30 March 2027 |
      | 29 January 2027 | 28 February 2027 | 28 March 2027 | 29 March 2027 |
      | 31 January 2028 | 29 February 2028 | 30 March 2028 | 31 March 2028 |
      | 29 January 2028 | 29 February 2028 | 28 March 2028 | 29 March 2028 |
      | 31 March 2027   | 30 April 2027    | 30 May 2027   | 31 May 2027   |

  # ----------------------------------------------------------------------------------
  # What an occurrence copies
  # ----------------------------------------------------------------------------------

  # Derived: the account is copied, not worked out afresh. The salary is paid into Bank and Netflix is
  # charged to Creditcard, and that does not change because Joint became the pool account. Both rows now
  # name their account, since neither is on the pool, and carry the frequency beside it.
  Scenario: An occurrence stays on the account the one before it was on, whichever account is the pool account by then
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have an account "Creditcard" with a starting balance of 0 euro
    And I have an account "Joint" with a starting balance of 0 euro
    And I have a category "Subscriptions"
    And I have recorded an income of 2500 euro labelled "Salaris" dated 27 August 2026, repeating monthly
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026 on the account "Creditcard", repeating monthly
    When I make "Joint" the pool account
    And I close MoneyBud, and start it again on 27 September 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount  |
      | expense | Subscriptions | Netflix | 13.99   |
      | income  |               | Salaris | 2500.00 |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date              | label   | amount  | account | repeats |
      | 27 September 2026 | Salaris | 2500.00 | Bank    | monthly |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category      | label   | amount | account    | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  | Creditcard | monthly |
    And the accounts should be exactly these, in this order:
      | account    | balance |
      | Joint      | 0.00    |
      | Bank       | 5000.00 |
      | Creditcard | -27.98  |

  # Derived: the category is copied as the category, not as its name, so a rename carries over.
  Scenario: An occurrence is on the category the one before it was on, under the name it has now
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I rename the category "Subscriptions" to "Streaming"
    And I close MoneyBud, and start it again on 25 September 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category  | label   | amount |
      | expense | Streaming | Netflix | 13.99  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category  | label   | amount | repeats |
      | 25 September 2026 | Streaming | Netflix | 13.99  | monthly |
    And the remaining "Streaming" budget in the current budget period should be -13.99 euro

  # An expense's label is optional, and an occurrence of one without a label has none either. The
  # notice names it by its category: the documentation's reading (see the header).
  Scenario: An expense without a label repeats without one, and I am told of it by its category
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    When I record an expense of 9.99 euro for "Subscriptions" without a label, repeating monthly
    And I close MoneyBud, and start it again on 25 September 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label | amount |
      | expense | Subscriptions |       | 9.99   |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category      | label | amount | repeats |
      | 25 September 2026 | Subscriptions |       | 9.99   | monthly |

  # ----------------------------------------------------------------------------------
  # An archived category comes back (ruling 8)
  # ----------------------------------------------------------------------------------

  # A subscription still paid goes on being tracked. The category comes back as it does when I record
  # against it myself, and I am told, without being asked anything first. This is the first way back
  # not taken by me: to keep it archived, I stop the repeat (change-a-repeat.feature).
  Scenario: An occurrence on an archived category is recorded, brings the category back, and I am told
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    And I have archived the category "Subscriptions"
    When the day becomes 25 September 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And I should be told that "Subscriptions" was brought back
    And I should not be warned or asked to confirm
    And the categories offered for a new expense should include "Subscriptions"
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date              | category      | label   | amount | repeats |
      | 25 September 2026 | Subscriptions | Netflix | 13.99  | monthly |

  # ----------------------------------------------------------------------------------
  # Settling day by day, the sweep, and balance corrections
  # ----------------------------------------------------------------------------------

  # Follow-up 5. MoneyBud was closed from 5 September to 3 October. The Markt of 12, 19 and 26 September
  # are recorded before September is swept, so the sweep has them: Unassigned 2500, less Groceries' 180
  # overspent, is 2320. Had the sweep come first, it would have moved 2455 and September would now show
  # 135 swept too much, with a button to press. The Markt of 3 October is October's.
  Scenario: When MoneyBud was closed across a period's end, that period's occurrences are recorded before it is swept, and the sweep has them
    Given my budget periods are one month long
    And today is 5 September 2026
    And I have an account "Deposit" with a starting balance of 0 euro
    And I have a category "Savings"
    And I have set the backing account of "Savings" to "Deposit"
    And I have set the sweep destination to "Savings"
    And I have a category "Groceries"
    And I have recorded an income of 2500 euro labelled "Salaris" dated 5 September 2026
    And I have recorded an income of 2500 euro labelled "Salaris oktober" dated 1 October 2026
    And I have recorded an expense of 45 euro for "Groceries" labelled "Markt" dated 5 September 2026, repeating weekly
    When I close MoneyBud, and start it again on 3 October 2026
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category  | label | amount |
      | expense | Groceries | Markt | 45.00  |
      | expense | Groceries | Markt | 45.00  |
      | expense | Groceries | Markt | 45.00  |
      | expense | Groceries | Markt | 45.00  |
    And I should be told that the period leftover of the previous budget period, 2320 euro, was swept into "Savings"
    And the remaining "Groceries" budget in the previous budget period should be -180 euro
    And the previous budget period should show that 2320 euro was swept into "Savings"
    And I should not be able to bring the swept amount of the previous budget period up to date
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 2455.00 |
      | Deposit | 2320.00 |

  # Derived. Settling comes first when MoneyBud starts, so Netflix is recorded before the balance
  # correction I type that morning, and the correction has it in it: the bank's 972.02 matches, the
  # difference is 0.00, and the history shows Netflix below the correction on their shared day.
  Scenario: A balance correction typed on an occurrence's day already has the occurrence in it
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Subscriptions"
    And I have recorded an income of 1000 euro labelled "Salaris" dated 25 August 2026
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I close MoneyBud, and start it again on 25 September 2026
    And I correct the balance of "Bank" to 972.02 euro
    Then the balance of "Bank" should be 972.02 euro
    And the balance correction of "Bank" to 972.02 euro should show a difference of 0.00 euro
    And the history of "Bank" should be exactly these, newest first:
      | date              | entry              | category      | label   | amount  | balance | difference |
      | 25 September 2026 | balance correction |               |         |         | 972.02  | 0.00       |
      | 25 September 2026 | expense            | Subscriptions | Netflix | 13.99   |         |            |
      | 25 August 2026    | expense            | Subscriptions | Netflix | 13.99   |         |            |
      | 25 August 2026    | income             |               | Salaris | 1000.00 |         |            |

  # Derived. The bank said 972.02, and MoneyBud had 1000: 27.98 unexplained. Netflix is then set up late,
  # dated 25 August, and brings 25 September with it. Both are dated before the balance correction's day,
  # so they are in it like any entry remembered late: the balance stays as typed, and the difference is
  # explained.
  Scenario: Occurrences dated back by a repeat set up late are in an earlier balance correction, like any entry remembered late
    Given my budget periods are one month long
    And today is 30 September 2026
    And I have a category "Subscriptions"
    And I have recorded an income of 1000 euro labelled "Salaris" dated 1 September 2026
    And I have corrected the balance of "Bank" to 972.02 euro
    And the balance correction of "Bank" to 972.02 euro shows a difference of -27.98 euro
    When I record an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And the balance of "Bank" should still be 972.02 euro
    And the balance correction of "Bank" to 972.02 euro should show a difference of 0.00 euro

  # ----------------------------------------------------------------------------------
  # Said while I am doing something else
  # ----------------------------------------------------------------------------------

  # The documentation's reading, by the sweep's precedent: a question and a notice are never shown
  # together, and being told of what MoneyBud recorded wins. Nothing is removed, since I never
  # confirmed. Netflix was recorded after Bakker, so it is listed above it on their shared day.
  Scenario: An occurrence said while a removal question is waiting drops the question, and nothing is removed
    Given my budget periods are one month long
    And today is 25 August 2026
    And I have a category "Groceries"
    And I have a category "Subscriptions"
    And I have recorded an expense of 20 euro for "Groceries" labelled "Bakker" dated 25 August 2026
    And I have recorded an expense of 13.99 euro for "Subscriptions" labelled "Netflix" dated 25 August 2026, repeating monthly
    When I ask to remove the expense labelled "Bakker"
    And the day becomes 25 September 2026 while MoneyBud is open
    Then I should be told, in one notice, that these repeating entries were recorded:
      | entry   | category      | label   | amount |
      | expense | Subscriptions | Netflix | 13.99  |
    And MoneyBud should not be asking me anything
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date           | category      | label   | amount | repeats |
      | 25 August 2026 | Subscriptions | Netflix | 13.99  |         |
      | 25 August 2026 | Groceries     | Bakker  | 20.00  |         |
