# All amounts are in euro and are always a whole number of cents. An amount finer than a cent is
# refused rather than rounded (arc42 §8.2). Unlike an expense or an income, an assignment MAY be
# negative and MAY be zero. A negative amount moves money back, and zero is accepted and moves
# nothing (glossary: "An amount may be assigned negatively", "Assigning zero is accepted").
#
# What assigning is (glossary: "The second distinction: plan and actual"). Income forms a pool,
# and every euro of a budget period's income is Unassigned until it is given a purpose. Assigning
# gives it one. It moves an amount out of that period's Unassigned and into a category's Budget.
# That is planning, and only planning. Nothing is spent, so what has been spent against a category
# never changes here. Its Remaining (Budget minus spent) moves only because its Budget does.
#
# Scope of this capability, per the assigning increment (glossary: "Nothing here blocks the
# assigning increment"). There are no accounts, so every category is unbacked and assigning moves
# no money anywhere. These are out, and no scenario below touches them: backed categories,
# accounts, the pool account and overdrawing it; the one action that assigns last period's plan in
# full, and anything else about a period opening; the end-of-period sweep; any UI.
#
# Reading the Givens:
#   - "I have a budget of X euro for Y in the current (or next) budget period" means X was
#     assigned to Y out of that period's pool earlier. It has already come out of that period's
#     Unassigned. A budget is only ever made by assigning, so there is no other way to have one.
#   - A budget "in the previous budget period" was assigned back when that period was current.
#     It came out of that period's pool, like any other.
#   - "Unassigned in the current budget period is X euro" does not set anything. It states what
#     the income and budgets above it already add up to, so the figure before the act is on the
#     page and the reader does not have to work it out.
#
# What I am told is a fact about what happened, not a sentence. The wording belongs to a UI that
# does not exist yet. "I should be told of a shortfall of X euro" means MoneyBud reports that X
# euro of a negative assignment could not come back, because the Budget did not hold it
# (glossary: "An over-large negative assignment is clipped, and the shortfall is reported").
#
# Every scenario starts from an empty ledger and names the categories it needs. The names are
# synthetic test data.
#
# Step phrasing: every step that concerns a budget period names it the same way: "in the current
# budget period", "in the previous budget period", "in the next budget period". That holds even
# where only one period is in play. One grammar, so one step definition each.

@budget
Feature: Assign to a category
  As someone budgeting my money
  I want to give the money coming in each period a purpose, by assigning it to my categories
  So that I decide in advance what each category may spend, and can see at a glance how much of the period's money still has no job

  # ----------------------------------------------------------------------------------
  # Assigning moves an amount out of Unassigned and into a Budget
  # ----------------------------------------------------------------------------------

  Scenario: Assigning moves an amount out of Unassigned and into the category's budget
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a category "Groceries"
    And I have set no budget for "Groceries" in the current budget period
    When I assign 400 euro to "Groceries" in the current budget period
    Then the assignment should go through
    And Unassigned in the current budget period should be 1600 euro
    And the budget for "Groceries" in the current budget period should be 400 euro

  # Assigning is planning. The 120 euro already spent stays spent, so Remaining goes up by
  # exactly what was assigned: from 180 to 230.
  Scenario: Assigning spends nothing, so the remaining budget rises by exactly what was assigned
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 300 euro for "Groceries" in the current budget period
    And I have already spent 120 euro on "Groceries" in the current budget period
    And Unassigned in the current budget period is 1700 euro
    When I assign 50 euro to "Groceries" in the current budget period
    Then Unassigned in the current budget period should be 1650 euro
    And the budget for "Groceries" in the current budget period should be 350 euro
    And the remaining "Groceries" budget in the current budget period should be 230 euro

  # Exactly zero Remaining is not over budget. That is the rule record-expense.feature
  # states, reached here from the plan side instead of the spending side.
  Scenario: Assigning to an overspent category can bring it back to exactly nothing remaining
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 300 euro for "Groceries" in the current budget period
    And I have already spent 320 euro on "Groceries" in the current budget period
    When I assign 20 euro to "Groceries" in the current budget period
    Then the budget for "Groceries" in the current budget period should be 320 euro
    And the remaining "Groceries" budget in the current budget period should be 0.00 euro
    And "Groceries" should not be shown as over budget
    And Unassigned in the current budget period should be 1680 euro

  # Assigning moves an amount. It does not set a figure. So assigning 400 to a budget of 400
  # makes 800, not 400.
  Scenario Outline: Assigning adds to what is already budgeted
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of <budget> euro for "Groceries" in the current budget period
    When I assign <amount> euro to "Groceries" in the current budget period
    Then the budget for "Groceries" in the current budget period should be <new budget> euro
    And Unassigned in the current budget period should be <unassigned> euro

    Examples: whole euros
      | budget | amount | new budget | unassigned |
      | 400.00 | 50.00  | 450.00     | 1550.00    |
      | 400.00 | 400.00 | 800.00     | 1200.00    |

    Examples: cents
      | budget | amount | new budget | unassigned |
      | 400.00 | 0.01   | 400.01     | 1599.99    |
      | 75.50  | 24.50  | 100.00     | 1900.00    |
      | 0.00   | 32.45  | 32.45      | 1967.55    |

  Scenario: Assigning to one category leaves every other category untouched
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    When I assign 25 euro to "Hobby" in the current budget period
    Then the budget for "Hobby" in the current budget period should be 85 euro
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And the remaining "Groceries" budget in the current budget period should still be 250 euro
    And Unassigned in the current budget period should be 1515 euro

  # ----------------------------------------------------------------------------------
  # A negative amount moves money back into Unassigned
  #
  # There is no separate act of unassigning. A minus sign does it (glossary: "An amount may be
  # assigned negatively"). As long as the Budget holds the amount, all of it comes back and there
  # is no shortfall to report. That includes taking a Budget down to exactly zero.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A negative amount moves money back out of the budget and into Unassigned
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of <budget> euro for "Groceries" in the current budget period
    And Unassigned in the current budget period is <before> euro
    When I assign <amount> euro to "Groceries" in the current budget period
    Then the assignment should go through
    And I should not be told of any shortfall
    And the budget for "Groceries" in the current budget period should be <new budget> euro
    And Unassigned in the current budget period should be <unassigned> euro

    Examples: whole euros
      | budget | before  | amount  | new budget | unassigned |
      | 400.00 | 1600.00 | -50.00  | 350.00     | 1650.00    |
      | 400.00 | 1600.00 | -400.00 | 0.00       | 2000.00    |

    Examples: cents
      | budget | before  | amount | new budget | unassigned |
      | 75.50  | 1924.50 | -0.01  | 75.49      | 1924.51    |
      | 32.45  | 1967.55 | -32.45 | 0.00       | 2000.00    |

  # The floor is on the Budget, not on Remaining (glossary: "A Budget floors at zero", which
  # leaves Remaining free to go negative). So money can be taken back out of a plan that has
  # already been partly spent. What was spent stays spent, and the category goes over budget.
  # MoneyBud shows that. It does not block it or warn about it.
  Scenario: Taking back money that was already spent leaves the category over budget
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 40 euro on "Hobby" in the current budget period
    When I assign -50 euro to "Hobby" in the current budget period
    Then the assignment should go through
    And I should not be warned or asked to confirm
    And I should not be told of any shortfall
    And the budget for "Hobby" in the current budget period should be 10 euro
    And the remaining "Hobby" budget in the current budget period should be -30 euro
    And "Hobby" should be shown as over budget
    And Unassigned in the current budget period should be 1990 euro

  # ----------------------------------------------------------------------------------
  # A negative amount larger than the budget is clipped, and the shortfall is reported
  #
  # A Budget floors at zero, and nothing can come back out of a category that never went in.
  # So -50 against a Budget of 30 takes the Budget to zero, puts 30 back into Unassigned, and
  # tells me the other 20 could not come back. It is never refused for being too large, and the difference is never
  # absorbed silently (glossary: "An over-large negative assignment is clipped, and the shortfall
  # is reported"). A Budget of zero has nothing to give back, so all of it is shortfall.
  #
  # In every row the whole Budget comes back, which is why Unassigned ends at the full 2000.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A negative amount larger than the budget takes back only what is there, and says how much could not come back
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of <budget> euro for "Groceries" in the current budget period
    And Unassigned in the current budget period is <before> euro
    When I assign <amount> euro to "Groceries" in the current budget period
    Then the assignment should go through
    And I should not be warned or asked to confirm
    And I should be told of a shortfall of <shortfall> euro
    And the budget for "Groceries" in the current budget period should be 0.00 euro
    And Unassigned in the current budget period should be 2000 euro

    Examples:
      | budget | before  | amount  | shortfall |
      | 30.00  | 1970.00 | -50.00  | 20.00     |
      | 30.00  | 1970.00 | -30.01  | 0.01      |
      | 0.01   | 1999.99 | -100.00 | 99.99     |
      | 0.00   | 2000.00 | -20.00  | 20.00     |

  # A category with no budget set behaves exactly as one budgeted at zero (glossary: Budget).
  # At the start of every period that is the normal state, so it gets its own scenario.
  Scenario: A category with no budget set has nothing to take back
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a category "Gifts"
    And I have never set a budget for "Gifts"
    When I assign -20 euro to "Gifts" in the current budget period
    Then the assignment should go through
    And I should be told of a shortfall of 20 euro
    And the budget for "Gifts" in the current budget period should be 0.00 euro
    And Unassigned in the current budget period should still be 2000 euro

  # ----------------------------------------------------------------------------------
  # Assigning more than Unassigned holds: Over-assigned
  #
  # Assigning is never blocked by the size of the pool. When more has been assigned than the
  # period's income, Unassigned goes negative, and the period is Over-assigned. That is the third
  # member of a family with Over budget and Overdrawn. It is shown, never blocked and never warned
  # about (glossary: Over-assigned, "Shown, never enforced"). Exactly zero is not Over-assigned:
  # it is every euro having a job.
  # ----------------------------------------------------------------------------------

  Scenario: Assigning more than Unassigned holds goes through, and the period is shown as over-assigned
    Given I have already recorded 1500 euro of income in the current budget period
    And I have a category "Groceries"
    And Unassigned in the current budget period is 1500 euro
    When I assign 1600 euro to "Groceries" in the current budget period
    Then the assignment should go through
    And I should not be warned or asked to confirm
    And the budget for "Groceries" in the current budget period should be 1600 euro
    And Unassigned in the current budget period should be -100 euro
    And the current budget period should be shown as over-assigned

  Scenario: Assigning exactly what is left brings Unassigned to zero, which is not over-assigned
    Given I have already recorded 1500 euro of income in the current budget period
    And I have a budget of 1200 euro for "Groceries" in the current budget period
    And I have a category "Hobby"
    And Unassigned in the current budget period is 300 euro
    When I assign 300 euro to "Hobby" in the current budget period
    Then Unassigned in the current budget period should be 0.00 euro
    And the current budget period should not be shown as over-assigned

  Scenario: One cent more than is left is already over-assigned
    Given I have already recorded 1500 euro of income in the current budget period
    And I have a budget of 1200 euro for "Groceries" in the current budget period
    And I have a category "Hobby"
    And Unassigned in the current budget period is 300 euro
    When I assign 300.01 euro to "Hobby" in the current budget period
    Then the assignment should go through
    And Unassigned in the current budget period should be -0.01 euro
    And the current budget period should be shown as over-assigned

  # Planning before the salary is in is allowed. The period is over-assigned until the income
  # is recorded.
  Scenario: With no income at all, assigning makes the period over-assigned
    Given I have recorded no income in the current budget period
    And I have a category "Groceries"
    When I assign 50 euro to "Groceries" in the current budget period
    Then the assignment should go through
    And I should not be warned or asked to confirm
    And the budget for "Groceries" in the current budget period should be 50 euro
    And Unassigned in the current budget period should be -50 euro
    And the current budget period should be shown as over-assigned

  Scenario: Taking money back out of a category can end over-assigned
    Given I have already recorded 1500 euro of income in the current budget period
    And I have a budget of 1600 euro for "Groceries" in the current budget period
    And Unassigned in the current budget period is -100 euro
    When I assign -100 euro to "Groceries" in the current budget period
    Then the assignment should go through
    And I should not be told of any shortfall
    And the budget for "Groceries" in the current budget period should be 1500 euro
    And Unassigned in the current budget period should be 0.00 euro
    And the current budget period should not be shown as over-assigned

  # ----------------------------------------------------------------------------------
  # Which budget periods can be assigned in
  #
  # The current period and any later one can be assigned in. A past period cannot, and that
  # holds for a negative amount too (glossary: "Assigning happens in the current budget period
  # and later ones, never in a past one"). The current and later periods are the ones I plan. A
  # past period is a record of what happened, and that includes the plan I actually had. An
  # assignment in a past period is bad input, so it is refused, not half-honoured, and nothing
  # changes.
  #
  # Each period's pool is its own income and nothing else (glossary: "Nothing crosses a period
  # boundary without a purpose"). So the next period is planned out of income dated in it. An
  # income may be dated in the future, and it joins its period's Unassigned from the moment it is
  # recorded (record-income.feature). The current period's money is not available to it.
  #
  # Future periods can be assigned in without limit. The step grammar reaches only one period
  # ahead, so "the next budget period" stands for all of them.
  # ----------------------------------------------------------------------------------

  Scenario: Assigning in the next budget period draws on that period's own income
    Given my budget periods are one month long
    And I have already recorded 1500 euro of income in the current budget period
    And I have already recorded 1800 euro of income in the next budget period
    And I have a category "Groceries"
    When I assign 400 euro to "Groceries" in the next budget period
    Then the assignment should go through
    And the budget for "Groceries" in the next budget period should be 400 euro
    And Unassigned in the next budget period should be 1400 euro
    And the budget for "Groceries" in the current budget period should still be 0 euro
    And Unassigned in the current budget period should still be 1500 euro

  Scenario: The next budget period cannot draw on the current period's income
    Given my budget periods are one month long
    And I have already recorded 1500 euro of income in the current budget period
    And I have recorded no income in the next budget period
    And I have a category "Groceries"
    When I assign 400 euro to "Groceries" in the next budget period
    Then the assignment should go through
    And I should not be warned or asked to confirm
    And Unassigned in the next budget period should be -400 euro
    And the next budget period should be shown as over-assigned
    And Unassigned in the current budget period should still be 1500 euro
    And the current budget period should not be shown as over-assigned

  # -400 is exactly the previous period's budget. In a current period it would come back in
  # full. In a past period it is refused like any other amount.
  Scenario Outline: Assigning in the previous budget period is refused, whatever the amount
    Given my budget periods are one month long
    And I have already recorded 1800 euro of income in the previous budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have already recorded 1500 euro of income in the current budget period
    And I have a budget of 300 euro for "Groceries" in the current budget period
    When I try to assign <amount> euro to "Groceries" in the previous budget period
    Then the assignment should be refused
    And I should be told that nothing can be assigned in a past budget period
    And the budget for "Groceries" in the previous budget period should still be 400 euro
    And Unassigned in the previous budget period should still be 1400 euro
    And the budget for "Groceries" in the current budget period should still be 300 euro
    And Unassigned in the current budget period should still be 1200 euro

    Examples:
      | amount  |
      | 50.00   |
      | 0.01    |
      | -50.00  |
      | -400.00 |

  # ----------------------------------------------------------------------------------
  # Assigning zero
  #
  # Accepted, and changes nothing. This is deliberately not the rule for an expense or an income.
  # There a zero amount is refused, because it would record an event that never happened.
  # Assigning zero records nothing and changes no figure, so there is nothing to refuse
  # (glossary: "Assigning zero is accepted and moves nothing"). That is about the amount only.
  # Zero to a name I do not have, or in a past period, is still refused. See the end of this file.
  # ----------------------------------------------------------------------------------

  Scenario: Assigning zero is accepted and changes nothing
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    When I assign 0.00 euro to "Groceries" in the current budget period
    Then the assignment should go through
    And I should not be told of any shortfall
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And the remaining "Groceries" budget in the current budget period should still be 250 euro
    And Unassigned in the current budget period should still be 1600 euro

  # ----------------------------------------------------------------------------------
  # Assigning to an archived category
  #
  # An archived category is not offered for assigning, just as it is not offered for a new
  # expense. Not being offered is not the same as being refused. An amount assigned to its name
  # goes through. What happens to the category then depends on the sign, and on nothing else
  # (glossary: "Assigning to an archived category brings it back", "Only a positive assignment
  # brings it back"):
  #   - POSITIVE: the category is brought back, history and all and spelled as it was, and I am
  #     told so. That is planning for it again, the same new entry as recording an expense
  #     against it.
  #   - NEGATIVE: it stays archived. Taking an archived category's money back is tidying up after
  #     putting it away, not planning for it. The clip still applies.
  #   - ZERO: it stays archived. Zero plans nothing.
  #   - REFUSED for another reason: nothing is assigned, so nothing is brought back.
  #
  # Archiving itself returned nothing to Unassigned, because archiving says nothing about money
  # (archive-category.feature). A negative assignment is how that money comes back.
  #
  # "the categories offered for a new expense should (not) include" is how this file, like
  # record-expense.feature, observes whether a category is archived.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A positive assignment to an archived category goes through and brings the category back
    Given my budget periods are one month long
    And I have already recorded 2000 euro of income in the current budget period
    And I have already recorded 1800 euro of income in the next budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have archived the category "Hobby"
    When I assign 40 euro to "Hobby" in the <period> budget period
    Then the assignment should go through
    And I should be told that "Hobby" was brought back
    And I should not be warned or asked to confirm
    And the categories offered for a new expense should include "Hobby"
    And the budget for "Hobby" in the <period> budget period should be <budget> euro
    And Unassigned in the <period> budget period should be <unassigned> euro

    Examples:
      | period  | budget | unassigned |
      | current | 100.00 | 1900.00    |
      | next    | 40.00  | 1760.00    |

  Scenario: A positive assignment to an archived category in another spelling brings it back spelled as it was
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have archived the category "Hobby"
    When I assign 12.50 euro to "  hobby " in the current budget period
    Then I should be told that "Hobby" was brought back
    And the categories offered for a new expense should list "Hobby" once, and no other spelling of it
    And the budget for "Hobby" in the current budget period should be 72.50 euro
    And Unassigned in the current budget period should be 1927.50 euro

  # The glossary's own example.
  Scenario: A negative assignment to an archived category takes its money back and leaves it archived
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    And I have archived the category "Hobby"
    And Unassigned in the current budget period is 1940 euro
    When I assign -60 euro to "Hobby" in the current budget period
    Then the assignment should go through
    And I should not be told of any shortfall
    And the budget for "Hobby" in the current budget period should be 0.00 euro
    And Unassigned in the current budget period should be 2000 euro
    And the categories offered for a new expense should not include "Hobby"

  Scenario: An over-large negative assignment to an archived category is clipped, and the category stays archived
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    And I have archived the category "Hobby"
    And Unassigned in the current budget period is 1940 euro
    When I assign -80 euro to "Hobby" in the current budget period
    Then the assignment should go through
    And I should be told of a shortfall of 20 euro
    And the budget for "Hobby" in the current budget period should be 0.00 euro
    And Unassigned in the current budget period should be 2000 euro
    And the categories offered for a new expense should not include "Hobby"

  Scenario: Assigning zero to an archived category leaves it archived
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have archived the category "Hobby"
    When I assign 0.00 euro to "Hobby" in the current budget period
    Then the assignment should go through
    And the categories offered for a new expense should not include "Hobby"
    And the budget for "Hobby" in the current budget period should still be 60 euro
    And Unassigned in the current budget period should still be 1940 euro

  # The table carries the rest of the "told" step, because each refusal gives its own reason, in
  # the same words as the refusal scenarios elsewhere in this file. What the rows share is the
  # rule under test: nothing assigned, so nothing brought back and no figure moved.
  Scenario Outline: An assignment that is refused leaves its archived category archived
    Given my budget periods are one month long
    And I have already recorded 1800 euro of income in the previous budget period
    And I have a budget of 60 euro for "Hobby" in the previous budget period
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have archived the category "Hobby"
    When I try to assign <amount> euro to "Hobby" in the <period> budget period
    Then the assignment should be refused
    And I should be told that <reason>
    And the categories offered for a new expense should not include "Hobby"
    And the budget for "Hobby" in the previous budget period should still be 60 euro
    And Unassigned in the previous budget period should still be 1740 euro
    And the budget for "Hobby" in the current budget period should still be 60 euro
    And Unassigned in the current budget period should still be 1940 euro

    Examples:
      | amount | period   | reason                                          |
      | 40.00  | previous | nothing can be assigned in a past budget period |
      | -60.00 | previous | nothing can be assigned in a past budget period |
      | 12.345 | current  | an amount cannot be finer than a cent           |

  # Derived from the display rule, not put to the stakeholder separately (glossary: "What
  # happens next, derived rather than asked"). A zero budget with nothing spent is not history.
  # So once its money is out, an archived category with nothing spent that period is no longer
  # shown there. That is where the tidying up ends. If something was spent, that expense is
  # history, and the category stays shown. That is the second scenario.
  Scenario: Once its money is taken back, an archived category with nothing spent is no longer shown in that period
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    And I have archived the category "Hobby"
    When I assign -60 euro to "Hobby" in the current budget period
    Then "Hobby" should not be shown in the current budget period

  Scenario: Taking back an archived category's unspent money leaves it shown where something was spent
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 15 euro on "Hobby" in the current budget period
    And I have archived the category "Hobby"
    When I assign -45 euro to "Hobby" in the current budget period
    Then the assignment should go through
    And the budget for "Hobby" in the current budget period should be 15 euro
    And the remaining "Hobby" budget in the current budget period should be 0.00 euro
    And "Hobby" should not be shown as over budget
    And "Hobby" should be shown in the current budget period
    And Unassigned in the current budget period should be 1985 euro
    And the categories offered for a new expense should not include "Hobby"

  # ----------------------------------------------------------------------------------
  # Which category an assignment counts against
  #
  # A category name is compared the same way wherever it is compared. Trim the ends, count a run
  # of spaces inside the name as one, then ignore case (glossary: "A category name is compared
  # case-insensitively and stored as typed, trimmed"). Assigning never adds a category, so the
  # category keeps its own spelling. The quotation marks in the table are part of the step, so a
  # name really does start or end with spaces. "should list X once, and no other spelling of it"
  # is explained in add-category.feature.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An assignment counts against my category however I capitalise or space its name
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Eating out" in the current budget period
    When I assign 50 euro to <typed> in the current budget period
    Then the assignment should go through
    And the budget for "Eating out" in the current budget period should be 450 euro
    And Unassigned in the current budget period should be 1550 euro
    And the categories offered for a new expense should list "Eating out" once, and no other spelling of it

    Examples:
      | typed            |
      | "eating out"     |
      | "EATING OUT"     |
      | "  Eating out  " |
      | "Eating   out"   |
      | "  eating  OUT " |

  # ----------------------------------------------------------------------------------
  # Input that is refused. In every case nothing is assigned, and neither Unassigned nor any
  # Budget moves.
  #
  # These are the refusals recording an expense has, less the ones that do not apply. A negative
  # amount is NOT refused, because it moves money back. Zero is NOT refused either. So -12.345
  # is refused only because it is finer than a cent. For an expense the same amount is refused
  # first for its sign.
  #
  # A past budget period is refused too. That is specified above, under "Which budget periods can
  # be assigned in".
  #
  # When one assignment breaks more than one rule, I am told about the first of them, in this
  # order: the category (a name that trims to nothing, then a name that is not one of mine),
  # then the amount (finer than a cent), then the period (a past one). Recording an expense
  # uses the same order. Settled by the stakeholder on 2026-09-25.
  #
  # The amount rules never refuse zero or a negative amount, but that does not rescue an
  # assignment the category or period rules refuse. In the stakeholder's words, zero is
  # harmless, "but it would still be canceled because of the other problems" (2026-09-25). So
  # zero in a past period, or to a name I do not have, is refused. So is -500 against a budget of
  # 400 in a past period: it is refused for the period, so nothing is clipped and no shortfall is
  # reported.
  # ----------------------------------------------------------------------------------

  Scenario: An assignment must name a category I actually have
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have no category called "Holiday"
    When I try to assign 50 euro to "Holiday" in the current budget period
    Then the assignment should be refused
    And I should be told that "Holiday" is not one of my categories
    And the categories offered for a new expense should not include "Holiday"
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And Unassigned in the current budget period should still be 1600 euro

  # A category name that trims to nothing is no name (glossary: "The rule holds wherever a name
  # is compared"). The quotation marks are part of the step.
  Scenario Outline: A category name that trims to nothing names no category
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    When I try to assign 50 euro to <name> in the current budget period
    Then the assignment should be refused
    And I should be told that an assignment needs a category
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And Unassigned in the current budget period should still be 1600 euro

    Examples:
      | name  |
      | ""    |
      | "   " |

  Scenario Outline: An assignment amount cannot be finer than a cent
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    When I try to assign <amount> euro to "Groceries" in the current budget period
    Then the assignment should be refused
    And I should be told that an amount cannot be finer than a cent
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And Unassigned in the current budget period should still be 1600 euro

    Examples:
      | amount  |
      | 0.001   |
      | 12.345  |
      | 49.999  |
      | -0.001  |
      | -12.345 |

  # The table carries the rest of the "told" step, as in the archived section above. Each row
  # breaks at least two rules, and the reason told is the first rule in the order stated at the
  # top of this section. The last row breaks all three.
  Scenario Outline: An assignment that breaks more than one rule is refused for the first of them
    Given my budget periods are one month long
    And I have already recorded 1800 euro of income in the previous budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have no category called "Holiday"
    When I try to assign <amount> euro to <name> in the <period> budget period
    Then the assignment should be refused
    And I should be told that <reason>
    And the budget for "Groceries" in the previous budget period should still be 400 euro
    And Unassigned in the previous budget period should still be 1400 euro
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And Unassigned in the current budget period should still be 1600 euro

    Examples:
      | amount | name        | period   | reason                                |
      | 12.345 | "Groceries" | previous | an amount cannot be finer than a cent |
      | 50.00  | "Holiday"   | previous | "Holiday" is not one of my categories |
      | 50.00  | "   "       | previous | an assignment needs a category        |
      | 12.345 | "Holiday"   | current  | "Holiday" is not one of my categories |
      | 12.345 | "Holiday"   | previous | "Holiday" is not one of my categories |

  # Zero is never refused for its amount, and neither is an over-large negative. Both are still
  # refused when the category or the period is wrong. A refused assignment moves nothing, so
  # there is nothing to clip and no shortfall to report.
  Scenario Outline: Zero and an over-large negative amount are still refused for the category or the period
    Given my budget periods are one month long
    And I have already recorded 1800 euro of income in the previous budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have no category called "Holiday"
    When I try to assign <amount> euro to <name> in the <period> budget period
    Then the assignment should be refused
    And I should be told that <reason>
    And I should not be told of any shortfall
    And the budget for "Groceries" in the previous budget period should still be 400 euro
    And Unassigned in the previous budget period should still be 1400 euro
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And Unassigned in the current budget period should still be 1600 euro

    Examples:
      | amount  | name        | period   | reason                                          |
      | 0.00    | "Groceries" | previous | nothing can be assigned in a past budget period |
      | -500.00 | "Groceries" | previous | nothing can be assigned in a past budget period |
      | 0.00    | "Holiday"   | current  | "Holiday" is not one of my categories           |
      | 0.00    | "   "       | current  | an assignment needs a category                  |
      | -500.00 | "Holiday"   | current  | "Holiday" is not one of my categories           |
