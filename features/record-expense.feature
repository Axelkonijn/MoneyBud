# All amounts are in euro (stakeholder round 3), and are always a whole number of cents —
# an amount finer than a cent is refused rather than rounded, so no rounding rule applies
# anywhere in this capability. The rule and its reasoning live in arc42 §8.2.
#
# Scope of this capability, per the first increment: an expense has an amount, a date,
# a label and a category. It has no account — the location dimension is not in the first
# increment (accepted risk, arc42 §11). Creating a category and setting its budget belong
# to other capabilities and appear here only as setup.
#
# Step phrasing: every step that concerns a budget period names it the same way — "in the
# current budget period", "in the previous budget period", "in the next budget period" —
# even where only one period is in play. One grammar, so one step definition each.

@budget
Feature: Record an expense
  As someone tracking my spending
  I want to record what I spent, on what, and out of which category
  So that I can see at any moment how much of that category's budget I have left

  # ----------------------------------------------------------------------------------
  # Recording an expense and what it does to the remaining budget
  # ----------------------------------------------------------------------------------

  Scenario: The first expense against a category comes off its full budget
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I record an expense of 25 euro for "Groceries"
    Then the remaining "Groceries" budget in the current budget period should be 375 euro

  Scenario Outline: The remaining budget is the budget minus everything spent against it
    Given I have a budget of <budget> euro for "Groceries" in the current budget period
    And I have already spent <already spent> euro on "Groceries" in the current budget period
    When I record an expense of <expense> euro for "Groceries"
    Then the remaining "Groceries" budget in the current budget period should be <remaining> euro

    Examples: whole euros
      | budget | already spent | expense | remaining |
      | 400.00 | 150.00        | 25.00   | 225.00    |
      | 400.00 | 380.00        | 45.00   | -25.00    |
      | 400.00 | 420.00        | 15.00   | -35.00    |

    Examples: cents
      | budget | already spent | expense | remaining |
      | 400.00 | 367.55        | 32.45   | 0.00      |
      | 400.00 | 367.55        | 32.46   | -0.01     |
      | 75.50  | 12.34         | 0.01    | 63.15     |
      | 75.50  | 75.49         | 0.02    | -0.01     |

  Scenario: Spending in one category leaves every other category untouched
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 40 euro on "Hobby" in the current budget period
    When I record an expense of 25 euro for "Groceries"
    Then the remaining "Groceries" budget in the current budget period should be 225 euro
    And the remaining "Hobby" budget in the current budget period should be 20 euro

  # Nothing is deduplicated. Two genuinely identical purchases on one day are two expenses.
  Scenario: Two identical expenses on the same day are both recorded
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I record an expense of 3.50 euro for "Groceries" labelled "Coffee" dated today
    And I record an expense of 3.50 euro for "Groceries" labelled "Coffee" dated today
    Then my "Groceries" spending in the current budget period should include 2 expenses of 3.50 euro
    And the remaining "Groceries" budget in the current budget period should be 393 euro

  # ----------------------------------------------------------------------------------
  # Going over budget — allowed, never blocked, shown as negative
  # ----------------------------------------------------------------------------------

  Scenario: Spending a category down to exactly nothing is not over budget
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 367.55 euro on "Groceries" in the current budget period
    When I record an expense of 32.45 euro for "Groceries"
    Then the remaining "Groceries" budget in the current budget period should be 0.00 euro
    And "Groceries" should not be shown as over budget

  Scenario: An expense that takes a category past its budget is still recorded
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 380 euro on "Groceries" in the current budget period
    When I record an expense of 45 euro for "Groceries"
    Then the expense should be recorded
    And I should not be warned or asked to confirm
    And the remaining "Groceries" budget in the current budget period should be -25 euro
    And "Groceries" should be shown as over budget

  Scenario: One cent over budget already counts as over budget
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 400 euro on "Groceries" in the current budget period
    When I record an expense of 0.01 euro for "Groceries"
    Then the remaining "Groceries" budget in the current budget period should be -0.01 euro
    And "Groceries" should be shown as over budget

  Scenario: A category that is already over budget can be spent against again
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 420 euro on "Groceries" in the current budget period
    When I record an expense of 15 euro for "Groceries"
    Then the expense should be recorded
    And I should not be warned or asked to confirm
    And the remaining "Groceries" budget in the current budget period should be -35 euro
    And "Groceries" should be shown as over budget

  Scenario: A category budgeted at nothing is over budget from the first cent
    Given I have a budget of 0 euro for "Subscriptions" in the current budget period
    And I have not yet spent anything on "Subscriptions" in the current budget period
    When I record an expense of 0.01 euro for "Subscriptions"
    Then the remaining "Subscriptions" budget in the current budget period should be -0.01 euro
    And "Subscriptions" should be shown as over budget

  # A category that has never had a budget set behaves exactly as one budgeted at 0 — there
  # is no separate "unbudgeted" state (glossary: Budget), and recording is never blocked by
  # a missing budget. Since budgets carry over as figures rather than as assignments, this
  # is the normal state at the start of every period, not a corner case.
  Scenario: A category with no budget set is over budget as soon as anything is spent
    Given I have a category "Gifts"
    And I have never set a budget for "Gifts"
    And I have not yet spent anything on "Gifts" in the current budget period
    When I record an expense of 20 euro for "Gifts"
    Then the expense should be recorded
    And the remaining "Gifts" budget in the current budget period should be -20 euro
    And "Gifts" should be shown as over budget

  # ----------------------------------------------------------------------------------
  # The label
  #
  # A label is trimmed: surrounding whitespace is stripped and the inner text is left alone,
  # so "  Albert Heijn  " is stored as "Albert Heijn" and "Albert  Heijn" keeps its double
  # space. MoneyBud does not tidy the user's prose; the rule is about text at the edges of a
  # field, where it is almost always an accident of typing or pasting.
  #
  # This is one rule covering every label, not an income rule. Something has to trim "   " in
  # order to judge it blank, so storing a label untrimmed would leave the two rules disagreeing
  # about what a label is — one trimming to decide, the other keeping whatever it was handed.
  # And nothing is derived from a label, on either transaction: no lookup, no grouping, no
  # comparison a leading space could carry meaning for. The only thing stripping it destroys is
  # whitespace nobody meant to type.
  #
  # Required versus optional decides only what happens when the result is empty. On an income
  # a label that trims to nothing is refused, because an income's label is required. On an
  # expense it is simply no label, which an expense is allowed to have — so an expense labelled
  # "   " is the same expense as one recorded without a label, not one labelled with spaces.
  # Both halves are written up in arc42 §12, "A label is trimmed, and that is what makes
  # 'blank' mean anything".
  # ----------------------------------------------------------------------------------

  Scenario: An expense keeps its own label alongside its category
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn"
    Then my "Groceries" spending in the current budget period should include 32.15 euro labelled "Albert Heijn"
    And the remaining "Groceries" budget in the current budget period should be 367.85 euro

  Scenario: An expense may be recorded without a label
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I record an expense of 18 euro for "Groceries" without a label
    Then the expense should be recorded
    And the remaining "Groceries" budget in the current budget period should be 382 euro

  Scenario: An expense label loses the whitespace around it and keeps everything inside it
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I record an expense of 27.40 euro for "Groceries" labelled "  Albert Heijn  "
    Then the expense should be recorded
    And my "Groceries" spending in the current budget period should include 27.40 euro labelled "Albert Heijn"
    And the remaining "Groceries" budget in the current budget period should be 372.60 euro

  # The same 18 euro and the same 382 euro as the scenario above it, on purpose: a label of
  # nothing but spaces trims to nothing, and an expense with no label is exactly what that is.
  Scenario: An expense labelled with nothing but spaces is recorded with no label
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I record an expense of 18 euro for "Groceries" labelled "   "
    Then the expense should be recorded
    And my "Groceries" spending in the current budget period should include 18 euro without a label
    And the remaining "Groceries" budget in the current budget period should be 382 euro

  # ----------------------------------------------------------------------------------
  # Input that is refused — in every case nothing is recorded and no budget moves
  #
  # Two rules can reject the same amount: -12.345 is both not more than zero and finer
  # than a cent. The sign is checked first, so "must be more than 0 euro" is what the user
  # is told. Stated here because the two outlines below would otherwise both claim it.
  # ----------------------------------------------------------------------------------

  Scenario: An expense must name a category
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    When I try to record an expense of 25 euro without naming a category
    Then the expense should not be recorded
    And I should be told that an expense needs a category
    And the remaining "Groceries" budget in the current budget period should still be 250 euro

  Scenario: An expense must name a category I actually have
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    And I have no category called "Holiday"
    When I try to record an expense of 25 euro for "Holiday"
    Then the expense should not be recorded
    And I should be told that "Holiday" is not one of my categories
    And the remaining "Groceries" budget in the current budget period should still be 250 euro

  Scenario Outline: An expense amount must be more than zero
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    When I try to record an expense of <amount> euro for "Groceries"
    Then the expense should not be recorded
    And I should be told that an expense must be more than 0 euro
    And the remaining "Groceries" budget in the current budget period should still be 250 euro

    Examples:
      | amount  |
      | 0.00    |
      | -0.01   |
      | -10.00  |
      | -12.345 |

  Scenario Outline: An expense amount cannot be finer than a cent
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    When I try to record an expense of <amount> euro for "Groceries"
    Then the expense should not be recorded
    And I should be told that an amount cannot be finer than a cent
    And the remaining "Groceries" budget in the current budget period should still be 250 euro

    Examples:
      | amount |
      | 0.001  |
      | 12.345 |
      | 25.999 |

  # ----------------------------------------------------------------------------------
  # Budget period boundaries
  #
  # The day a budget period starts is configurable and need not be the 1st of the month
  # (glossary: Budget period). These scenarios are therefore written in terms of "the
  # previous" and "the current" budget period rather than calendar months, so they hold
  # whatever start day is configured. Worked example if the start day were the 25th: the
  # current period runs 25 September to 24 October, and "the last day of the previous
  # budget period" is 24 September.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An expense counts against the budget period its date falls in
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in either budget period
    When I record an expense of 60 euro for "Groceries" dated on <day>
    Then the remaining "Groceries" budget in the previous budget period should be <remaining previous> euro
    And the remaining "Groceries" budget in the current budget period should be <remaining current> euro

    Examples:
      | day                                         | remaining previous | remaining current |
      | the first day of the previous budget period | 340                | 400               |
      | the last day of the previous budget period  | 340                | 400               |
      | the first day of the current budget period  | 400                | 340               |
      | today                                       | 400                | 340               |

  # A past budget period is never closed: because everything is entered by hand, expenses
  # get remembered late, and correcting history has to stay possible.
  Scenario: An expense dated in the previous budget period does not disturb the current one
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have already spent 395 euro on "Groceries" in the previous budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 120 euro on "Groceries" in the current budget period
    When I record an expense of 12.50 euro for "Groceries" dated on the last day of the previous budget period
    Then the remaining "Groceries" budget in the previous budget period should be -7.50 euro
    And "Groceries" should be shown as over budget in the previous budget period
    And the remaining "Groceries" budget in the current budget period should be 280 euro

  # An expense records money already spent, so no expense may be dated in the future. This
  # covers a date later in the current period as well as one in a period still to come;
  # known future costs are what recurring transactions are for, in a later increment.
  Scenario Outline: An expense cannot be dated in the future
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    When I try to record an expense of 25 euro for "Groceries" dated on <day>
    Then the expense should not be recorded
    And I should be told that an expense cannot be dated in the future
    And the remaining "Groceries" budget in the current budget period should still be 250 euro

    Examples:
      | day                                     |
      | tomorrow                                |
      | the first day of the next budget period |
