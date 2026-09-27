# Seeing my accounts: every account's Balance and my net worth, in a strip across the top of the
# Overview, and each account's history a click away (glossary: "Accounts and net worth", settled by
# the stakeholder on 2026-09-27). Adding an account is in add-an-account.feature, the account an
# income or expense is on in record-on-an-account.feature, balance corrections in
# correct-a-balance.feature, transfers in transfer-between-accounts.feature, and renaming,
# deleting and the pool account in manage-accounts.feature. THIS FILE EXPLAINS THE STEPS THE OTHER
# FIVE SHARE.
#
# The rules, from arc42 §12:
#   - A Balance is WORKED OUT from what is on the account, never stored as a number of its own. It
#     starts from the account's latest typed balance (its starting balance, or a later balance
#     correction), and every income, expense and transfer on it dated after that typed balance's day,
#     or on that day and first recorded after it, moves it. Entries dated before are already in it
#     (correct-a-balance.feature). An account with NO typed balance, such as the pool account every
#     scenario starts with, or one added with its starting balance left empty
#     (add-an-account.feature), has as its balance the plain sum of what is on it.
#   - NET WORTH, on screen "Vermogen", is the sum of every account's Balance TODAY. So the strip is
#     THE SAME IN EVERY PERIOD: it is not about the period on screen.
#   - THE ORDER of the accounts, in the strip and in the forms' account list alike: THE POOL ACCOUNT
#     FIRST, then every other account IN THE ORDER IT WAS ADDED. Making another account the pool
#     reorders them. Ruled by the stakeholder on 2026-09-27. That a renamed account keeps its place
#     is the documentation's reading, from "the same account under a new name", as for a category.
#   - An income dated in the future counts in its period's Unassigned from the moment it is recorded,
#     as before, but reaches its account's Balance, and so net worth, only on its date. That is where
#     net worth and Unassigned disagree by design (glossary: "The central distinction").
#   - Assigning to an UNBACKED category is planning only and moves no money, so it changes no
#     Balance, even to a category that shares its name with an account. Every category starts out
#     unbacked. Assigning to a BACKED category moves money from the pool account to its backing
#     account: that came with the backing increment (back-a-category.feature and the files it names).
#   - An account whose Balance is BELOW ZERO is OVERDRAWN. It is shown with the same marker as Over
#     budget and Over-assigned, with a badge of its own that reads "Rood". It is never blocked and
#     never warned about. Exactly zero is not overdrawn. The badge was ruled by the stakeholder on
#     2026-09-27.
#   - NET WORTH BELOW ZERO carries THE SAME MARKER, badge "Rood" too. Exactly zero does not. Ruled by
#     the stakeholder on 2026-09-27.
#   - Clicking an account opens its HISTORY, NEWEST FIRST: its starting balance, its balance
#     corrections, its transfers, and the incomes and expenses on it, and, since the backing
#     increment, the money MoneyBud moved on a category's behalf (show-moved-money.feature). It
#     covers EVERY PERIOD, not the one on screen. In the history, TRANSFERS CAN BE CHANGED AND
#     REMOVED, and A BALANCE CORRECTION (a starting balance included) CAN BE REMOVED. INCOMES AND
#     EXPENSES ARE LISTED THERE BUT NOT CHANGED OR REMOVED THERE: that is done from the Overview's
#     lists only, as before. Ruled by the stakeholder on 2026-09-27. The Overview's income and expense
#     lists stay incomes and expenses only.
#   - An income or expense row on the Overview NAMES ITS ACCOUNT ONLY WHEN THAT IS NOT THE POOL
#     ACCOUNT. A row reads the pool account as it is when shown, so after another account is made
#     the pool, rows on the old pool gain its name and rows on the new one lose theirs (derived).
#
# Every scenario starts from an empty ledger, as in every other file. SINCE THIS INCREMENT THAT
# LEDGER HOLDS ONE ACCOUNT, "Bank", which is the pool account, has no starting balance and has
# nothing on it, so its Balance is 0.00. There is always exactly one pool account, so an empty
# ledger cannot have none. "Bank" is synthetic test data, like "Groceries": the account a FIRST
# START comes with is "Betaalrekening" (start-moneybud.feature), and no scenario outside the
# first-start ones depends on it. Every income and expense a scenario records without naming an
# account is on the pool account, which is how the scenarios of earlier increments stay as they
# were. The one exception is an expense against a BACKED category, which starts out on its backing
# account (spend-against-a-backed-category.feature). No scenario outside the backing files backs a
# category.
#
# Reading the steps. These are shared by all six accounts files:
#   - "I have an account "X" with a starting balance of N euro" means X was added earlier, with
#     that starting balance typed TODAY, at that point in the order of the Givens. "... dated on
#     <day>" means it was added, and its starting balance typed, on that earlier day.
#   - "I have corrected the balance of "X" to N euro" is a balance correction typed earlier today,
#     at that point in the order of the Givens (correct-a-balance.feature).
#   - Givens are listed in the order things happened, so the order of the Givens IS the order
#     things were recorded in. That order matters on the day of a balance correction.
#   - "the balance of "X" is N euro" and "net worth is N euro", as Givens, SET NOTHING. They state
#     what the Givens above them already add up to, so that the figure before the act is on the
#     page, like assign-to-category.feature's "Unassigned in the current budget period is X euro".
#   - "I have deleted the account "X"" is manage-accounts.feature's act, done earlier, as a Given.
#   - "... on the account "X"", at the end of a step that records an income or an expense, means
#     that account was chosen for it. Without it, the entry is on the account a new entry starts out
#     on, which is the pool account, or, for an expense against a backed category, its backing
#     account.
#   - "yesterday" is the day before today. Every scenario that uses it fixes today as the last day
#     of the current budget period, so yesterday is in the current budget period too.
#   - "the balance of "X" should be N euro" is the Balance the strip shows for X. "net worth should
#     be N euro" is the figure the strip shows as Vermogen. "should still be" says the act did not
#     change it.
#   - "the accounts should be exactly these, in this order" lists every account in the strip, in
#     the strip's order, and no other. It checks the columns its table has: ACCOUNT alone, or
#     BALANCE and OVERDRAWN as well. OVERDRAWN "yes" means the account carries the marker, with the
#     badge "Rood".
#   - ""X" should be shown as overdrawn, with the marker a category over budget has and the badge
#     "Rood"" asserts that the marker is the same one Over budget and Over-assigned use, and that
#     its badge reads "Rood". ""X" should not be shown as overdrawn" means neither. "net worth
#     should (not) be marked below zero, ..." is the same for net worth.
#   - "the pool account should be "X"" means X is the one account the strip marks as the pool
#     account, on screen "Hoofdrekening", and no other account is marked so.
#   - "a new expense (income) should start out on the account "X"" is what the form's account list
#     is filled in with before I touch it. "the accounts offered for a new expense (income) should
#     be exactly these, in this order" is the whole list I can choose from, in the list's order.
#   - "the history of "X" should be exactly these, newest first" lists every row of X's history, in
#     order, and no other. Rows are in date order, newest first, and rows on the same date are
#     newest recorded first, as in the Overview's lists. It checks the columns its table has:
#       DATE is the row's date. ENTRY is what the row is: "starting balance", "balance correction",
#       "transfer", "income" or "expense". CATEGORY and LABEL are an expense's or income's own.
#       FROM and TO are a transfer's two accounts. AMOUNT is the entry's amount, as it is recorded,
#       without a sign: an expense, an income and a transfer each say by what they are which way
#       the money went. BALANCE is the balance a starting balance or a balance correction typed.
#       DIFFERENCE is how far that typed balance is from what MoneyBud had worked out just before
#       it, negative when MoneyBud had more (correct-a-balance.feature).
#       A BLANK CELL means the row shows nothing there.
#     "nothing should be in the history of "X"" means the history has no rows.
#   - "in the history of "X" I should be able to change (remove) ..." and "... I should not be able
#     to change or remove ..." mean that act is, or is not, offered on that row of X's history.
#   - The Overview's list steps are list-transactions-in-a-period.feature's, with one new column:
#     ACCOUNT is the account the row names, and a blank ACCOUNT cell means the row names none,
#     because the entry is on the pool account. A table without the column does not check it, which
#     is how every list in the earlier files stays as it was.
#   - What MoneyBud tells me is a fact, not a sentence, as everywhere else. What is fixed is that I
#     am told, and what about, never the wording.
#   - The Dutch on screen, confirmed by the stakeholder on 2026-09-27: Rekening / Rekeningen, Saldo,
#     Vermogen, Hoofdrekening, Maak hoofdrekening, Overboeking / Overboeken, Van / Naar, Startsaldo,
#     Correctie / Saldo corrigeren, Rekening toevoegen, and Hernoemen / Verwijderen reused for
#     accounts. As with every earlier display term, they are held by the glossary's table of Dutch
#     display terms and the developer test that reads it, not by these steps. The steps use this
#     project's English terms, and assert on-screen Dutch only where a badge is the point: "Rood".
#   - Every other step is reused unchanged from the file that introduced it.
#
# NOT specified here, and left to developer tests and the markup, as the first demo round left the
# field order: where the strip sits (above the three columns), that Vermogen comes at the end of it,
# that a non-pool account's name on a row is small and grey, and where the history opens.
#
# The names, labels and amounts are synthetic test data.

@accounts
Feature: See my accounts and my net worth
  As someone who wants to know how I am doing, not only where this month's money goes
  I want every account's balance and my net worth in view on the Overview, and each account's history a click away
  So that I can check each balance against my bank, and see what I have today in whichever period I am looking at

  # ----------------------------------------------------------------------------------
  # The strip: every account's balance, and net worth
  # ----------------------------------------------------------------------------------

  Scenario: The strip shows every account's balance, and net worth is their sum
    Given I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 40 euro
    And I have an account "Savings" with a starting balance of 5000 euro
    And I have a category "Groceries"
    When I record an expense of 25 euro for "Groceries" labelled "Markt" on the account "Cash"
    Then the accounts should be exactly these, in this order:
      | account | balance | overdrawn |
      | Bank    | 1832.45 | no        |
      | Cash    | 15.00   | no        |
      | Savings | 5000.00 | no        |
    And net worth should be 6847.45 euro
    And the pool account should be "Bank"

  # ----------------------------------------------------------------------------------
  # The order: the pool account first, then the others in the order they were added
  #
  # Ruled by the stakeholder on 2026-09-27, for the strip and the forms' account list alike. The
  # names are chosen so that alphabetical order would give a different answer: Savings was added
  # before Cash.
  # ----------------------------------------------------------------------------------

  Scenario: The pool account comes first, then every other account in the order it was added
    Given I have an account "Savings" with a starting balance of 5000 euro
    And I have an account "Cash" with a starting balance of 40 euro
    Then the accounts should be exactly these, in this order:
      | account |
      | Bank    |
      | Savings |
      | Cash    |
    And the accounts offered for a new expense should be exactly these, in this order:
      | account |
      | Bank    |
      | Savings |
      | Cash    |
    And the accounts offered for a new income should be exactly these, in this order:
      | account |
      | Bank    |
      | Savings |
      | Cash    |

  # The new pool account moves to the front. The old one goes back to its place in the order
  # added, which for Bank, added first, is straight after the pool.
  Scenario Outline: Making another account the pool account puts it first, and the rest stay in the order they were added
    Given I have an account "Savings" with a starting balance of 5000 euro
    And I have an account "Cash" with a starting balance of 40 euro
    When I make "<new pool>" the pool account
    Then the accounts should be exactly these, in this order:
      | account  |
      | <first>  |
      | <second> |
      | <third>  |
    And the accounts offered for a new expense should be exactly these, in this order:
      | account  |
      | <first>  |
      | <second> |
      | <third>  |

    Examples:
      | new pool | first   | second | third   |
      | Cash     | Cash    | Bank   | Savings |
      | Savings  | Savings | Bank   | Cash    |

  # Balances are about today, so stepping to another period changes nothing in the strip. The
  # expense is dated in the previous period and is in the balance all the same.
  Scenario Outline: The strip is the same in every period
    Given my budget periods are one month long
    And I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 40 euro dated on the first day of the previous budget period
    And I have a category "Groceries"
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated on the last day of the previous budget period on the account "Cash"
    When I step <direction> one budget period
    Then the Overview should show the <period> budget period
    And the accounts should be exactly these, in this order:
      | account | balance |
      | Bank    | 1832.45 |
      | Cash    | 15.00   |
    And net worth should be 1847.45 euro

    Examples:
      | direction | period   |
      | back      | previous |
      | forward   | next     |

  # ----------------------------------------------------------------------------------
  # Net worth is what I have today
  #
  # A salary dated next month is in next month's Unassigned the moment it is recorded, so it can be
  # planned. It is not in any account yet, so it is not in the balance or in net worth until its
  # date comes.
  # ----------------------------------------------------------------------------------

  Scenario: An income dated in the future is in Unassigned at once, and in its account's balance only on its date
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1500 euro labelled "Salaris" dated today
    When I record an income of 1800 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    Then the income should be recorded
    And Unassigned in the next budget period should be 1800 euro
    And the balance of "Bank" should still be 1500 euro
    And net worth should still be 1500 euro
    When the next budget period begins while MoneyBud is open
    Then the balance of "Bank" should be 3300 euro
    And net worth should be 3300 euro

  # Net worth is the plain sum, an overdrawn account's negative balance included. Below zero, it
  # carries the same marker as an overdrawn account, with the badge "Rood": ruled by the stakeholder
  # on 2026-09-27. Never blocked, never warned about.
  Scenario Outline: Net worth below zero is shown as the negative figure, with the marker and the badge "Rood"
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    When I record an expense of <amount> euro for "Groceries" labelled "Markt"
    Then I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance | overdrawn |
      | Bank    | <bank>  | yes       |
      | Cash    | 40.00   | no        |
    And net worth should be <net worth> euro
    And net worth should be marked below zero, with the marker a category over budget has and the badge "Rood"

    Examples:
      | amount | bank    | net worth |
      | 40.01  | -40.01  | -0.01     |
      | 100.00 | -100.00 | -60.00    |

  # Exactly zero is not below zero, the line every marker draws. Bank is overdrawn, and net worth
  # is not.
  Scenario: Net worth of exactly zero carries no marker
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    When I record an expense of 40 euro for "Groceries" labelled "Markt"
    Then the balance of "Bank" should be -40 euro
    And "Bank" should be shown as overdrawn, with the marker a category over budget has and the badge "Rood"
    And net worth should be 0.00 euro
    And net worth should not be marked below zero

  # Savings is not backed, so assigning to it moves no money, even while there is an account Savings.
  # The two share a name and nothing else: they are different dimensions.
  Scenario: Assigning to an unbacked category moves no money, so no balance changes
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have an account "Savings" with a starting balance of 5000 euro
    And I have a category "Savings"
    When I assign 500 euro to "Savings" in the current budget period
    Then the assignment should go through
    And the budget for "Savings" in the current budget period should be 500 euro
    And Unassigned in the current budget period should be 1500 euro
    And the balance of "Bank" should still be 2000 euro
    And the balance of "Savings" should still be 5000 euro
    And net worth should still be 7000 euro

  # ----------------------------------------------------------------------------------
  # An overdrawn account carries the marker, and nothing stops it
  #
  # Reachable in this increment by an expense or a transfer out of an account that has not got the
  # money, and by a negative starting balance (add-an-account.feature) or balance correction. Since
  # the backing increment, also by assigning to a backed category, and by backing, unbacking or
  # re-pointing a category (back-a-category.feature, assign-to-a-backed-category.feature). Exactly
  # zero is the account spent down to nothing, not overdrawn, the same line Over budget draws.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An account below zero is shown as overdrawn, and exactly zero is not
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    When <act>
    Then I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance   | overdrawn   |
      | Bank    | <bank>    | no          |
      | Cash    | <balance> | <overdrawn> |

    Examples:
      | act                                                                                      | bank  | balance | overdrawn |
      | I record an expense of 40 euro for "Groceries" labelled "Markt" on the account "Cash"    | 0.00  | 0.00    | no        |
      | I record an expense of 40.01 euro for "Groceries" labelled "Markt" on the account "Cash" | 0.00  | -0.01   | yes       |
      | I record a transfer of 40.01 euro from "Cash" to "Bank"                                  | 40.01 | -0.01   | yes       |
      | I correct the balance of "Cash" to -0.01 euro                                            | 0.00  | -0.01   | yes       |

  Scenario: The overdrawn marker is the one over budget uses, with the badge "Rood"
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    When I record an expense of 52.50 euro for "Groceries" labelled "Markt" on the account "Cash"
    Then the expense should be recorded
    And the balance of "Cash" should be -12.50 euro
    And "Cash" should be shown as overdrawn, with the marker a category over budget has and the badge "Rood"
    And "Bank" should not be shown as overdrawn

  # ----------------------------------------------------------------------------------
  # An account's history
  #
  # Everything that makes up the balance, and everything that is on the account whether or not the
  # balance counts it. Verkocht is dated before Cash's starting balance, so the starting balance
  # already has it in (correct-a-balance.feature), and it is listed all the same. The period on
  # screen does not limit it: Verkocht is in the previous period and the Overview shows the current
  # one.
  #
  # A STARTING BALANCE'S ROW SHOWS NO DIFFERENCE, only the balance typed. Ruled by the stakeholder
  # on 2026-09-27: at the moment it is typed, the difference would be all of it.
  # ----------------------------------------------------------------------------------

  # Cash: 40 to start, Verkocht already in it, +50 from Bank, -25 at the market: 65. Then corrected
  # to 70, which is 5 more than MoneyBud had.
  Scenario: An account's history lists everything on it, in every period, newest first
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Groceries"
    And I have recorded an income of 1800 euro labelled "Salaris" dated on the last day of the previous budget period
    And I have an account "Cash" with a starting balance of 40 euro dated on the first day of the current budget period
    And I have recorded an income of 10 euro labelled "Verkocht" dated on the last day of the previous budget period on the account "Cash"
    And I have recorded a transfer of 50 euro from "Bank" to "Cash" dated yesterday
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated today on the account "Cash"
    And I have corrected the balance of "Cash" to 70 euro
    Then the balance of "Cash" should be 70 euro
    And the history of "Cash" should be exactly these, newest first:
      | date                                       | entry              | category  | label    | from | to   | amount | balance | difference |
      | today                                      | balance correction |           |          |      |      |        | 70.00   | 5.00       |
      | today                                      | expense            | Groceries | Markt    |      |      | 25.00  |         |            |
      | yesterday                                  | transfer           |           |          | Bank | Cash | 50.00  |         |            |
      | the first day of the current budget period | starting balance   |           |          |      |      |        | 40.00   |            |
      | the last day of the previous budget period | income             |           | Verkocht |      |      | 10.00  |         |            |
    And the history of "Bank" should be exactly these, newest first:
      | date                                       | entry    | label   | from | to   | amount  |
      | yesterday                                  | transfer |         | Bank | Cash | 50.00   |
      | the last day of the previous budget period | income   | Salaris |      |      | 1800.00 |

  # The history is about the account, not the period, so it does not follow the Overview.
  Scenario Outline: An account's history is the same whichever period is on screen
    Given my budget periods are one month long
    And I have an account "Cash" with a starting balance of 40 euro dated on the first day of the previous budget period
    And I have a category "Groceries"
    And I have recorded an expense of 12.50 euro for "Groceries" labelled "Kiosk" dated on the last day of the previous budget period on the account "Cash"
    And I have recorded an expense of 3.50 euro for "Groceries" labelled "Coffee" dated today on the account "Cash"
    And the Overview shows the <shown> budget period
    Then the history of "Cash" should be exactly these, newest first:
      | date                                        | entry            | label  | amount | balance |
      | today                                       | expense          | Coffee | 3.50   |         |
      | the last day of the previous budget period  | expense          | Kiosk  | 12.50  |         |
      | the first day of the previous budget period | starting balance |        |        | 40.00   |

    Examples:
      | shown    |
      | previous |
      | current  |
      | next     |

  # Which rows of a history can be acted on, ruled by the stakeholder on 2026-09-27. A transfer is
  # changed and removed only here, because it is in no period's list. A balance correction, a
  # starting balance included, is removed here and never changed: to change one, correct again. An
  # income or an expense is only listed here: it is changed and removed from the Overview's lists,
  # as before, so there is one place to correct an entry, not two.
  Scenario: In an account's history a transfer can be changed and removed, a balance correction removed, and an income or expense neither
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    And I have recorded an income of 10 euro labelled "Verkocht" dated today on the account "Cash"
    And I have recorded an expense of 25 euro for "Groceries" labelled "Markt" dated today on the account "Cash"
    And I have recorded a transfer of 50 euro from "Bank" to "Cash" dated today
    And I have corrected the balance of "Cash" to 70 euro
    Then in the history of "Cash" I should be able to change the transfer of 50 euro from "Bank" to "Cash"
    And in the history of "Cash" I should be able to remove the transfer of 50 euro from "Bank" to "Cash"
    And in the history of "Cash" I should be able to remove the balance correction of "Cash" to 70 euro
    And in the history of "Cash" I should not be able to change the balance correction of "Cash" to 70 euro
    And in the history of "Cash" I should be able to remove the starting balance of "Cash"
    And in the history of "Cash" I should not be able to change the starting balance of "Cash"
    And in the history of "Cash" I should not be able to change or remove the income labelled "Verkocht"
    And in the history of "Cash" I should not be able to change or remove the expense labelled "Markt"

  # Transfers and balance corrections are in the histories only. A period's lists are its incomes
  # and its expenses, and nothing else.
  Scenario: Transfers and balance corrections are not listed among a period's incomes and expenses
    Given I have an account "Cash" with a starting balance of 40 euro
    When I record a transfer of 50 euro from "Bank" to "Cash"
    And I correct the balance of "Bank" to 1000 euro
    Then no incomes should be listed in the current budget period
    And no expenses should be listed in the current budget period

  # ----------------------------------------------------------------------------------
  # An income or expense row names its account only when it is not the pool account
  #
  # So a cash expense stands out. Settled by the stakeholder. What it does NOT do, as the glossary
  # says plainly: a cash expense wrongly left on the pool account looks like every other row.
  # ----------------------------------------------------------------------------------

  Scenario: A row names its account only when that is not the pool account
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have an account "Savings" with a starting balance of 5000 euro
    And I have a category "Groceries"
    When I record an income of 1832.45 euro labelled "Salaris"
    And I record an income of 12.50 euro labelled "Rente" on the account "Savings"
    And I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn"
    And I record an expense of 3.50 euro for "Groceries" labelled "Kiosk" on the account "Cash"
    Then the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  | account |
      | today | Rente   | 12.50   | Savings |
      | today | Salaris | 1832.45 |         |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount | account |
      | today | Groceries | Kiosk        | 3.50   | Cash    |
      | today | Groceries | Albert Heijn | 32.15  |         |

  # Derived from the ruling's wording, and not asked: a row reads the pool account as it is when it
  # is shown. Nothing about the entries changes, only which account is the pool.
  Scenario: After another account is made the pool account, rows on the old one name it and rows on the new one do not
    Given I have an account "Cash" with a starting balance of 40 euro
    And I have a category "Groceries"
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    And I have recorded an expense of 3.50 euro for "Groceries" labelled "Kiosk" dated today on the account "Cash"
    When I make "Cash" the pool account
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount | account |
      | today | Groceries | Kiosk        | 3.50   |         |
      | today | Groceries | Albert Heijn | 32.15  | Bank    |
