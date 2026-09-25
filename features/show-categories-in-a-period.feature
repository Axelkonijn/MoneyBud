# Which categories a budget period shows: the categories the Overview lists when that period is
# the one on screen. The rule was settled in full by the stakeholder on 2026-09-25 (glossary:
# "When any category is shown in a period: the full rule"), and this increment is where it is
# built:
#
#   A category is shown in period P if it has HISTORY in P (a budget of more than zero, or an
#   expense), or if it is IN USE and P is the current period or a later one.
#
#   | Period            | In use, no history there | Archived, no history there |
#   | Past              | not shown                | not shown                  |
#   | Current or future | SHOWN                    | not shown                  |
#
#   A category with history in a period is shown there in every case.
#
# Why the halves differ. The current and later periods are the ones I plan, so every category I
# could assign to has to be there to be assigned to, whether or not anything has happened to it
# yet. A past period is a record of what happened, so it shows only what has history there.
#
# A budget of zero with nothing spent is not history, for a category in use exactly as for an
# archived one. A category with no budget set behaves exactly as one budgeted at zero (glossary:
# Budget), so an invisible difference cannot decide whether a row appears.
#
# What is already asserted elsewhere, and not repeated: an archived category is shown in the
# current and previous periods where it has history and not where it has none
# (archive-category.feature), and an archived category whose money is taken back, with nothing
# spent, stops being shown (assign-to-category.feature). What is new here is the in-use half of
# the rule, and the NEXT period for both halves.
#
# Reading the steps:
#   - "X should (not) be shown in the ... budget period" is archive-category.feature's grammar,
#     unchanged: whether X appears among that period's figures.
#   - "the categories shown in the ... budget period should be exactly these, in this order" lists
#     every category shown there, in the order shown, and no other. The order is settled and
#     specified in overview.feature: largest Budget first, and equal budgets (every zero among
#     them) in the order the categories were added, which is the order the Givens first name them.
#     A category brought back keeps its original place in that order.
#     Here the table has only the CATEGORY column, so only the names and their order are checked.
#   - The grammar names one period each way. As in assign-to-category.feature, "the next budget
#     period" stands for every later period and "the previous budget period" for every earlier
#     one.
#
# Every scenario starts from an empty ledger and names the categories it needs. The names are
# synthetic test data.

@budget
Feature: Show a period's categories
  As someone budgeting my money
  I want each budget period to list the categories that matter to it
  So that I can plan every category I use in the periods ahead, while a past period shows what actually happened in it and nothing else

  # ----------------------------------------------------------------------------------
  # A category in use
  # ----------------------------------------------------------------------------------

  Scenario: A category I have just added is shown in the current and next periods, and not in the previous one
    Given my budget periods are one month long
    And I have no category called "Vaste lasten"
    When I add a category "Vaste lasten"
    Then "Vaste lasten" should be shown in the current budget period
    And "Vaste lasten" should be shown in the next budget period
    And "Vaste lasten" should not be shown in the previous budget period

  Scenario: A budget alone is history, so a past period shows the category
    Given my budget periods are one month long
    And I have a budget of 60 euro for "Hobby" in the previous budget period
    And I have not yet spent anything on "Hobby" in the previous budget period
    Then "Hobby" should be shown in the previous budget period

  Scenario: An expense alone is history, so a past period shows the category
    Given my budget periods are one month long
    And I have a category "Gifts"
    And I have set no budget for "Gifts" in the previous budget period
    And I have already spent 20 euro on "Gifts" in the previous budget period
    Then "Gifts" should be shown in the previous budget period
    And "Gifts" should be shown as over budget in the previous budget period

  # The budget of zero in the previous period is one that was assigned and then taken back to
  # zero while that period was current. It is not history. The current period still shows Hobby,
  # because Hobby is in use and the current period is one I plan.
  Scenario: A budget of zero with nothing spent is not history, so a past period does not show the category
    Given my budget periods are one month long
    And I have a budget of 0 euro for "Hobby" in the previous budget period
    And I have not yet spent anything on "Hobby" in the previous budget period
    Then "Hobby" should not be shown in the previous budget period
    And "Hobby" should be shown in the current budget period

  # A period never closes, so a late receipt can still land in a past period. When it does, the
  # category has history there, and the period shows it.
  Scenario: A back-dated expense gives a category history in a past period, and it is shown there
    Given my budget periods are one month long
    And I have a category "Groceries"
    And I have set no budget for "Groceries" in the previous budget period
    And I have not yet spent anything on "Groceries" in the previous budget period
    When I record an expense of 12.50 euro for "Groceries" dated on the last day of the previous budget period
    Then "Groceries" should be shown in the previous budget period

  # ----------------------------------------------------------------------------------
  # An archived category, in the periods ahead
  #
  # An archived category is not offered for assigning, so the planning reason does not reach it.
  # In a future period, as in any other, it is shown only where it has history.
  # ----------------------------------------------------------------------------------

  Scenario: An archived category is not shown in the next period when it has no history there
    Given my budget periods are one month long
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 25 euro on "Hobby" in the current budget period
    And I have set no budget for "Hobby" in the next budget period
    When I archive the category "Hobby"
    Then "Hobby" should be shown in the current budget period
    And "Hobby" should not be shown in the next budget period

  # A budget assigned ahead is history in that future period, so archiving does not hide it.
  Scenario: An archived category is shown in the next period when it has a budget there
    Given my budget periods are one month long
    And I have a budget of 40 euro for "Hobby" in the next budget period
    And I have set no budget for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    When I archive the category "Hobby"
    Then "Hobby" should be shown in the next budget period
    And "Hobby" should not be shown in the current budget period

  # Brought back means in use again, so the in-use half of the rule applies to it once more.
  # Bringing back is specified in archive-category.feature; only where it is shown is new here.
  Scenario: A category brought back is in use again, so it is shown in the current and next periods
    Given my budget periods are one month long
    And I have a category "Insurance"
    And I have archived the category "Insurance"
    When I add a category "Insurance"
    Then "Insurance" should be shown in the current budget period
    And "Insurance" should be shown in the next budget period
    And "Insurance" should not be shown in the previous budget period

  # ----------------------------------------------------------------------------------
  # The whole rule at once
  #
  #   Groceries  in use,   history in the previous and current periods
  #   Hobby      in use,   no history anywhere
  #   Gifts      archived, an expense in the previous period only
  #   Magazines  archived, no history anywhere
  #
  # In the previous period Gifts has an expense but no budget, so it comes after Groceries. In the
  # next period neither Groceries nor Hobby has a budget, so they tie at zero and keep the order
  # they were added in.
  # ----------------------------------------------------------------------------------

  Scenario: Each period lists exactly the categories the rule selects
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have already spent 380 euro on "Groceries" in the previous budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a category "Hobby"
    And I have never set a budget for "Hobby"
    And I have a category "Gifts"
    And I have set no budget for "Gifts" in the previous budget period
    And I have already spent 35 euro on "Gifts" in the previous budget period
    And I have archived the category "Gifts"
    And I have a category "Magazines"
    And I have archived the category "Magazines"
    Then the categories shown in the previous budget period should be exactly these, in this order:
      | category  |
      | Groceries |
      | Gifts     |
    And the categories shown in the current budget period should be exactly these, in this order:
      | category  |
      | Groceries |
      | Hobby     |
    And the categories shown in the next budget period should be exactly these, in this order:
      | category  |
      | Groceries |
      | Hobby     |
