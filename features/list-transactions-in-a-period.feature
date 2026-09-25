# Each budget period lists the expenses and the incomes dated in it (glossary: "It covers what the
# domain does, and nothing more": "seeing each period's expenses and income"). An entry is listed
# in the period its date falls in, which is the same period its amount counts against
# (record-expense.feature, record-income.feature). Nothing else decides where it is listed: not
# the period on screen when it was recorded, and not whether its category has since been
# archived.
#
# An expense is listed with its date, its category, its label and its amount. An income is listed
# with its date, its label and its amount. An income names no category, so there is none to list.
# An expense's label is optional. An empty label cell below means the expense has no label, not a
# label made of nothing. Labels are listed as stored, which is trimmed (record-expense.feature,
# record-income.feature).
#
# Expenses and incomes are two separate lists, settled by the stakeholder on 2026-09-25. Each is
# in the same order: newest date first, and entries on the same date newest-recorded first,
# within their own list. So each list runs by date, not grouped by category, and an income never
# sits among the expenses or the other way round.
#
# There is no editing or deleting of an entry. The domain has neither, so the list has neither
# (glossary: "Category entry is free text with suggestions", which records that consequence and
# the stakeholder's acceptance of it for the demo). Nothing below asserts that absence, because
# there is no act to assert it against.
#
# Reading the steps:
#   - "the expenses listed in the ... budget period should be exactly these, in this order" lists
#     every expense in that period, in the order listed, and no other. A row that appears twice is
#     two expenses. "the incomes listed ..." is the same for incomes.
#   - "no expenses (incomes) should be listed in the ... budget period" means the list is empty.
#   - Dates are named relative to the budget periods, as in record-expense.feature, so they hold
#     whatever day today is. Amounts are in euro, in whole cents.
#   - "today is the last day of the current budget period" is record-income.feature's "today is
#     the first day of the current budget period", naming a different day.
#   - record-expense.feature and record-income.feature already have "my ... spending (income) in
#     the ... budget period should include ...". Those check that one entry is among a period's
#     entries, without its date. These steps are the whole list a user sees.
#
# Every scenario starts from an empty ledger and names the categories it needs. The names and
# labels are synthetic test data.

@budget
Feature: List a period's transactions
  As someone tracking my money
  I want to see every expense and income recorded in a budget period, each with its amount, date and label
  So that I can recognise each entry, and check that the period's figures are made of what I think they are

  # The entries are recorded oldest date first, so the order below holds even on a day when today
  # is the first day of the current period.
  Scenario: A period lists each of its expenses with its date, category, label and amount
    Given my budget periods are one month long
    And I have a category "Groceries"
    And I have a category "Hobby"
    When I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated on the first day of the current budget period
    And I record an expense of 18 euro for "Groceries" dated today
    And I record an expense of 0.01 euro for "Hobby" labelled "Sticker" dated today
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date                                       | category  | label        | amount |
      | today                                      | Hobby     | Sticker      | 0.01   |
      | today                                      | Groceries |              | 18.00  |
      | the first day of the current budget period | Groceries | Albert Heijn | 32.15  |

  Scenario: A period lists each of its incomes with its date, label and amount
    Given my budget periods are one month long
    When I record an income of 1832.45 euro labelled "Salaris" dated on the first day of the current budget period
    And I record an income of 47.55 euro labelled "Verjaardagsgeld" dated today
    Then the incomes listed in the current budget period should be exactly these, in this order:
      | date                                       | label           | amount  |
      | today                                      | Verjaardagsgeld | 47.55   |
      | the first day of the current budget period | Salaris         | 1832.45 |

  # ----------------------------------------------------------------------------------
  # The order: newest date first, then newest recorded first
  #
  # Today is fixed as the last day of the current period so that "today" and "the first day of
  # the current budget period" are different dates. The entry dated earliest is recorded between
  # the other two, so date order and recording order disagree, and the list has to follow the
  # date. The two entries dated today are then in the reverse of the order they were recorded.
  # ----------------------------------------------------------------------------------

  Scenario: Entries are listed newest date first, and newest recorded first on the same date
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Groceries"
    When I record an expense of 18 euro for "Groceries" labelled "Bakker" dated today
    And I record an expense of 60 euro for "Groceries" labelled "Markt" dated on the first day of the current budget period
    And I record an expense of 3.50 euro for "Groceries" labelled "Coffee" dated today
    And I record an income of 25 euro labelled "Statiegeld" dated today
    And I record an income of 1800 euro labelled "Salaris" dated on the first day of the current budget period
    And I record an income of 47.55 euro labelled "Verjaardagsgeld" dated today
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date                                       | category  | label  | amount |
      | today                                      | Groceries | Coffee | 3.50   |
      | today                                      | Groceries | Bakker | 18.00  |
      | the first day of the current budget period | Groceries | Markt  | 60.00  |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date                                       | label           | amount  |
      | today                                      | Verjaardagsgeld | 47.55   |
      | today                                      | Statiegeld      | 25.00   |
      | the first day of the current budget period | Salaris         | 1800.00 |

  # ----------------------------------------------------------------------------------
  # Which period an entry is listed in
  # ----------------------------------------------------------------------------------

  # The boundaries: the last day of one period and the first day of the next are in different
  # lists. A future income is listed in its own period from the moment it is recorded.
  Scenario: Each period lists only the entries dated in it, on its first and last days too
    Given my budget periods are one month long
    And I have a category "Groceries"
    When I record an expense of 60 euro for "Groceries" labelled "Markt" dated on the last day of the previous budget period
    And I record an expense of 40 euro for "Groceries" labelled "Bakker" dated on the first day of the current budget period
    And I record an income of 1850 euro labelled "Salaris vorige maand" dated on the last day of the previous budget period
    And I record an income of 1800 euro labelled "Salaris" dated on the first day of the current budget period
    And I record an income of 1900 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    Then the expenses listed in the previous budget period should be exactly these, in this order:
      | date                                       | category  | label | amount |
      | the last day of the previous budget period | Groceries | Markt | 60.00  |
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date                                       | category  | label  | amount |
      | the first day of the current budget period | Groceries | Bakker | 40.00  |
    And no expenses should be listed in the next budget period
    And the incomes listed in the previous budget period should be exactly these, in this order:
      | date                                       | label                | amount  |
      | the last day of the previous budget period | Salaris vorige maand | 1850.00 |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date                                       | label   | amount  |
      | the first day of the current budget period | Salaris | 1800.00 |
    And the incomes listed in the next budget period should be exactly these, in this order:
      | date                                    | label                  | amount  |
      | the first day of the next budget period | Salaris volgende maand | 1900.00 |

  # Nothing is deduplicated (record-expense.feature, record-income.feature), and the list must not
  # quietly fold two identical entries into one.
  Scenario: Two identical entries are listed as two
    Given I have a category "Groceries"
    When I record an expense of 3.50 euro for "Groceries" labelled "Coffee" dated today
    And I record an expense of 3.50 euro for "Groceries" labelled "Coffee" dated today
    And I record an income of 25 euro labelled "Statiegeld" dated today
    And I record an income of 25 euro labelled "Statiegeld" dated today
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label  | amount |
      | today | Groceries | Coffee | 3.50   |
      | today | Groceries | Coffee | 3.50   |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label      | amount |
      | today | Statiegeld | 25.00  |
      | today | Statiegeld | 25.00  |

  # Archiving takes a category out of new entry. Its history stays, expenses included
  # (archive-category.feature).
  Scenario: An archived category's expenses are still listed in their period, under its name
    Given I have a category "Hobby"
    When I record an expense of 25 euro for "Hobby" labelled "Verf" dated today
    And I archive the category "Hobby"
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category | label | amount |
      | today | Hobby    | Verf  | 25.00  |
