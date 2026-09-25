# All amounts are in euro (stakeholder round 3, hardened into arc42 §2 as a decision rather
# than an assumption), and are always a whole number of cents — an amount finer than a cent is
# refused rather than rounded, so no rounding rule applies anywhere in this capability. The rule
# and its reasoning live in arc42 §8.2.
#
# Scope of this capability, per the income increment: an income has an amount, a date and a
# label. It has **no category** — it lands *Unassigned* and is given a purpose later, by
# assigning, which is specified in assign-to-category.feature, not here. It has no account either — the
# location dimension is not built (accepted gap, arc42 §11), the same gap an expense has.
#
# Unassigned is a figure on the purpose dimension, not a category and not a place (glossary:
# Unassigned). For one budget period it is that period's income minus everything assigned to
# categories in it. Recording income only ever adds to it. Assigning subtracts from it, and can
# take it below zero into *Over-assigned* (assign-to-category.feature). One scenario below starts
# from a period whose budgets were assigned before any income arrived, so it starts below zero;
# none asserts the over-assigned state itself.
#
# Step phrasing: every step that concerns a budget period names it the same way — "in the
# current budget period", "in the previous budget period", "in the next budget period" — even
# where only one period is in play. One grammar, so one step definition each.

@budget
Feature: Record an income
  As someone budgeting my money
  I want to record money coming in, with a note of what it is from
  So that I can see what there is to budget for a period before I decide what it is for

  # ----------------------------------------------------------------------------------
  # Recording an income and what it does to Unassigned
  #
  # Income forms a pool. Every euro of it is Unassigned until it is given a purpose, and
  # giving it one is a separate act, assigning (assign-to-category.feature; glossary: Unassigned,
  # Assign).
  # ----------------------------------------------------------------------------------

  Scenario: The first income of a period fills an empty pool
    Given I have recorded no income in the current budget period
    When I record an income of 1832.45 euro labelled "Salaris september"
    Then the income should be recorded
    And Unassigned in the current budget period should be 1832.45 euro

  Scenario Outline: Unassigned is everything recorded into the period
    Given I have already recorded <already recorded> euro of income in the current budget period
    When I record an income of <income> euro labelled "Terugbetaling zorgverzekering"
    Then Unassigned in the current budget period should be <unassigned> euro

    Examples: whole euros
      | already recorded | income  | unassigned |
      | 1500.00          | 250.00  | 1750.00    |
      | 2000.50          | 49.50   | 2050.00    |

    Examples: cents
      | already recorded | income | unassigned |
      | 1500.00          | 0.01   | 1500.01    |
      | 1499.99          | 0.02   | 1500.01    |
      | 0.01             | 0.01   | 0.02       |

  Scenario: Several incomes in one budget period add up
    Given I have recorded no income in the current budget period
    When I record an income of 1832.45 euro labelled "Salaris september"
    And I record an income of 120.00 euro labelled "Bijbaan"
    And I record an income of 47.55 euro labelled "Verjaardagsgeld"
    Then Unassigned in the current budget period should be 2000.00 euro

  # Nothing is deduplicated. Two genuinely separate amounts that happen to match are two
  # incomes — the same rule the expenses capability states.
  Scenario: Two identical incomes on the same day are both recorded
    Given I have recorded no income in the current budget period
    When I record an income of 25 euro labelled "Statiegeld" dated today
    And I record an income of 25 euro labelled "Statiegeld" dated today
    Then my income in the current budget period should include 2 incomes of 25 euro
    And Unassigned in the current budget period should be 50 euro

  # ----------------------------------------------------------------------------------
  # The label
  #
  # An income names no category, so the label is the only thing on the record that says what
  # the money is. That is why it is required here and optional on an expense (glossary:
  # "Income carries a label, and it is required"). Nothing is derived from it — it is free
  # text for the reader, not a key and not a category by another name.
  #
  # A label is trimmed: whitespace at either end is stripped before it is stored. Required
  # and trimmed are one rule, not two — deciding that "   " is blank is already an act of
  # trimming, so storing the untrimmed text would leave the two halves disagreeing about
  # what the label is. Nothing is derived from a label, so trimming loses nothing a reader
  # wants. Only the ends are touched; the text itself is left exactly as typed.
  # ----------------------------------------------------------------------------------

  Scenario: An income keeps the label it was given
    Given I have recorded no income in the current budget period
    When I record an income of 1832.45 euro labelled "Salaris september"
    Then my income in the current budget period should include 1832.45 euro labelled "Salaris september"
    And Unassigned in the current budget period should be 1832.45 euro

  Scenario: A label loses the whitespace around it and keeps everything inside it
    Given I have recorded no income in the current budget period
    When I record an income of 1832.45 euro labelled "  Salaris september  "
    Then the income should be recorded
    And my income in the current budget period should include 1832.45 euro labelled "Salaris september"
    And Unassigned in the current budget period should be 1832.45 euro

  # ----------------------------------------------------------------------------------
  # Income and the budget layer do not meet
  #
  # Income arrives on the pool; a Budget is a plan; an expense is the actual. Recording income
  # moves money into Unassigned and touches neither layer of any category — only assigning
  # does that (assign-to-category.feature; glossary: "The second distinction: plan and actual").
  # ----------------------------------------------------------------------------------

  Scenario: Recording income leaves every category's plan and spending untouched
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have recorded no income in the current budget period
    And Unassigned in the current budget period is -460 euro
    When I record an income of 2000 euro labelled "Salaris september"
    Then Unassigned in the current budget period should be 1540 euro
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And the remaining "Groceries" budget in the current budget period should still be 250 euro
    And the budget for "Hobby" in the current budget period should still be 60 euro

  # ----------------------------------------------------------------------------------
  # Input that is refused — in every case nothing is recorded and Unassigned does not move
  #
  # Two rules can reject the same entry: an income of -20 euro labelled "   " has both a
  # label that says nothing and an amount that is not more than zero. The label is checked
  # first, so "an income needs a label" is what the user is told — mirroring the expenses
  # capability, where the category is checked before the amount. The last scenario in this
  # section pins that order down, and it is stated here because the outlines below would
  # otherwise each appear to claim it.
  # ----------------------------------------------------------------------------------

  Scenario: An income must carry a label
    Given I have already recorded 1500 euro of income in the current budget period
    When I try to record an income of 250 euro without a label
    Then the income should not be recorded
    And I should be told that an income needs a label
    And Unassigned in the current budget period should still be 1500 euro

  # "Required" means the label must actually say something: a label that is absent and one
  # that is blank read the same to someone looking at the pool. Confirmed by the stakeholder,
  # not inferred (glossary: "Income carries a label, and it is required").
  #
  # These refusals are the trimming rule above meeting the requirement, not a second rule
  # beside it: a label of spaces trims to nothing, and nothing is not a label. So the three
  # rows below are the same case as "without a label" reached by a different route — which
  # is why they are told the same thing. The quotation marks are part of the step, so the
  # label really is empty or really is spaces.
  Scenario Outline: A label that says nothing is not a label
    Given I have already recorded 1500 euro of income in the current budget period
    When I try to record an income of 250 euro labelled <label>
    Then the income should not be recorded
    And I should be told that an income needs a label
    And Unassigned in the current budget period should still be 1500 euro

    Examples:
      | label |
      | ""    |
      | " "   |
      | "   " |

  Scenario Outline: An income amount must be more than zero
    Given I have already recorded 1500 euro of income in the current budget period
    When I try to record an income of <amount> euro labelled "Salaris september"
    Then the income should not be recorded
    And I should be told that an income must be more than 0 euro
    And Unassigned in the current budget period should still be 1500 euro

    Examples:
      | amount  |
      | 0.00    |
      | -0.01   |
      | -10.00  |
      | -12.345 |

  Scenario Outline: An income amount cannot be finer than a cent
    Given I have already recorded 1500 euro of income in the current budget period
    When I try to record an income of <amount> euro labelled "Salaris september"
    Then the income should not be recorded
    And I should be told that an amount cannot be finer than a cent
    And Unassigned in the current budget period should still be 1500 euro

    Examples:
      | amount   |
      | 0.001    |
      | 12.345   |
      | 1832.459 |

  Scenario: An income that breaks two rules at once is refused for its label
    Given I have already recorded 1500 euro of income in the current budget period
    When I try to record an income of -20 euro labelled "   "
    Then the income should not be recorded
    And I should be told that an income needs a label
    And Unassigned in the current budget period should still be 1500 euro

  # ----------------------------------------------------------------------------------
  # Budget period boundaries
  #
  # The day a budget period starts is configurable and need not be the 1st of the month
  # (glossary: Budget period), so these scenarios are written in terms of "the previous",
  # "the current" and "the next" budget period rather than calendar months, and hold whatever
  # start day is configured. Worked example if the start day were the 25th: the current period
  # runs 25 September to 24 October, and "the last day of the previous budget period" is
  # 24 September.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An income counts against the budget period its date falls in
    Given my budget periods are one month long
    And I have recorded no income in the previous budget period
    And I have recorded no income in the current budget period
    And I have recorded no income in the next budget period
    When I record an income of 1200 euro labelled "Salaris" dated on <day>
    Then Unassigned in the previous budget period should be <previous> euro
    And Unassigned in the current budget period should be <current> euro
    And Unassigned in the next budget period should be <next> euro

    Examples:
      | day                                         | previous | current | next |
      | the first day of the previous budget period | 1200     | 0       | 0    |
      | the last day of the previous budget period  | 1200     | 0       | 0    |
      | the first day of the current budget period  | 0        | 1200    | 0    |
      | today                                       | 0        | 1200    | 0    |
      | the first day of the next budget period     | 0        | 0       | 1200 |
      | the last day of the next budget period      | 0        | 0       | 1200 |

  # A past budget period is never closed: everything is entered by hand, so income gets
  # remembered late too, and correcting history has to stay possible.
  Scenario: An income dated in the previous budget period does not disturb the current one
    Given my budget periods are one month long
    And I have already recorded 1800 euro of income in the previous budget period
    And I have already recorded 1500 euro of income in the current budget period
    When I record an income of 62.50 euro labelled "Teruggave energie" dated on the last day of the previous budget period
    Then Unassigned in the previous budget period should be 1862.50 euro
    And Unassigned in the current budget period should still be 1500 euro

  # ----------------------------------------------------------------------------------
  # An income MAY be dated in the future — the opposite of an expense
  #
  # This is the one place where income and expense behave differently about dates, and it is
  # deliberate. An expense dated tomorrow or in a period still to come is REFUSED (see
  # record-expense.feature); an income dated the same way is recorded. The reason is the
  # plan/actual split: a future expense is already expressible as a Budget, while there is no
  # "planned income" anywhere in the model, so future-dating is the only way to state an
  # amount that is coming (glossary: "Income may be dated in the future; an expense may not").
  #
  # And it joins its period's Unassigned FROM THE MOMENT IT IS RECORDED, not from its date.
  # If the figure waited for the date, future-dating would buy nothing: the point is to be
  # able to budget a period before it has begun.
  #
  # "Today is the first day of the current budget period" is fixed in the first scenario so
  # that "the last day of the current budget period" is unambiguously still to come.
  # ----------------------------------------------------------------------------------

  Scenario: An income dated later in the current budget period is in the pool as soon as it is recorded
    Given my budget periods are one month long
    And today is the first day of the current budget period
    And I have recorded no income in the current budget period
    When I record an income of 1800 euro labelled "Salaris deze maand" dated on the last day of the current budget period
    Then the income should be recorded
    And I should not be warned or asked to confirm
    And Unassigned in the current budget period should be 1800 euro

  Scenario: An income dated in the next budget period is in that period's pool as soon as it is recorded
    Given my budget periods are one month long
    And I have already recorded 1500 euro of income in the current budget period
    And I have recorded no income in the next budget period
    When I record an income of 1800 euro labelled "Salaris volgende maand" dated on the first day of the next budget period
    Then the income should be recorded
    And I should not be warned or asked to confirm
    And Unassigned in the next budget period should be 1800 euro
    And Unassigned in the current budget period should still be 1500 euro
