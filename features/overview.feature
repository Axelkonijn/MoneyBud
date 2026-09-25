# The Overview is the screen MoneyBud opens on (glossary: Overview). It shows one budget period at
# a time and is headed by the ring (glossary: Ring; "The overview, and its ring"). Below the ring,
# it lists the period's categories, one row each. This file specifies the ring, what each row
# shows, the order of the rows and the slices, and what the Overview shows on a first start.
#
# WHICH categories the Overview lists is the display rule, specified in
# show-categories-in-a-period.feature. The ring decides only which of those categories get a
# slice. Stepping from one period to another is in step-between-periods.feature.
#
# The domain behaviour behind every figure here is already specified in the five earlier feature
# files and is not restated: recording, assigning, archiving, Remaining, Unassigned, Over budget
# and Over-assigned. The Givens use their grammar and mean what those files say they mean.
#
# How the ring is drawn, settled by the stakeholder on 2026-09-25:
#   - One slice per category with a Budget above zero. It is sized to that Budget and filled in as
#     far as the category has been spent, so the unfilled part is its Remaining. Exactly zero
#     Remaining is a completely filled slice, and it is NOT over budget.
#   - Unassigned above zero is a slice of its own. So, unless the period is Over-assigned, the
#     whole ring is the period's income. That holds only because archived categories' budgets are
#     slices too: Unassigned subtracts them.
#   - An overspent slice stays budget-sized, is drawn completely filled, and is marked as over
#     budget. It never grows. The slice is the plan, and overspending does not change the plan.
#   - A category with a Budget of zero gets no slice, however the zero came about: never assigned,
#     or assigned and then taken back. The ring must not tell those two apart. If something was
#     spent against it, it is over budget from the first cent, and it is listed with the marker
#     instead of being drawn.
#   - An Over-assigned period has no Unassigned slice. Its ring shows the budgets only, and
#     Unassigned is shown as the negative figure itself, with the same marker as Over budget.
#   - A period with neither income nor any Budget has an empty ring, with a hint that there is no
#     income in the period. Income with no budgets, and budgets with no income, are NOT empty
#     rings.
#
# What each row shows, and in what order, settled by the stakeholder on 2026-09-25:
#   - Each category row shows its Budget, what was spent, and its Remaining, and the over-budget
#     marker when it is over budget. Remaining is the negative figure itself when it is below
#     zero, never a positive "over by" amount.
#   - Rows are in order of Budget, largest first. Categories with equal budgets keep the order in
#     which they were added. That covers every category with a Budget of zero, so those all come
#     after every budgeted category, in the order they were added. A category brought back from
#     archived keeps its original place in that order: bringing it back is not adding it again.
#   - The ring's category slices are in that same order.
#   - The Unassigned slice is always the last slice, after every category slice, whatever the
#     sizes. (Which way round the ring is read is presentation, and is not asserted.)
#
# The marker is information, never a warning (glossary: "One marker for over budget and
# over-assigned"). What is fixed is what is drawn, what is marked, and the order. Colours, the
# marker's form and the layout are not fixed, and nothing below asserts them.
#
# Reading the steps:
#   - "the ring for the current budget period" is the ring the Overview shows when that period is
#     the one on screen.
#   - "should have exactly these category slices, in this order" lists every category slice, in
#     the ring's order, and no other. SIZE is the slice's size in euro, which is its Budget; the
#     ring draws each slice in proportion to the ring's total. FILLED is how much of the slice is
#     drawn filled in: what has been spent, and never more than the size. OVER BUDGET says whether
#     the slice is marked.
#   - "should have no category slices" means the table above would be empty.
#   - "should have an Unassigned slice of X euro" and "should have no Unassigned slice" are about
#     Unassigned's own slice. "the Unassigned slice should be the last slice in the ring ..."
#     asserts its place.
#   - "should add up to X euro" is the total of every slice, Unassigned's included.
#   - "should be empty, with a hint that there is no income in it" is the empty ring. The hint's
#     wording is copy, not a term; what is fixed is what it says.
#   - "X should have no slice in the ring for ..." is the one-category form of what the table
#     already implies, written out where it is the point of the scenario.
#   - "Unassigned ... should be marked the same way as a category that is over budget" is the one
#     marker for both states. It asserts that the two are marked alike, not what the marker is.
#   - "the categories shown in the ... budget period should be exactly these, in this order" lists
#     every row, in order, and no other. It checks the columns its table has: with only CATEGORY,
#     the names and their order; with BUDGET, SPENT, REMAINING and OVER BUDGET as well, the
#     row's figures and its marker.
#   - "in the order they were added" means the order in which the scenario's Givens first name
#     each category.
#
# Reused unchanged: "X should be shown in the ... budget period" (archive-category.feature),
# "X should be shown as over budget in the ... budget period" (record-expense.feature), and "the
# ... budget period should (not) be shown as over-assigned" (assign-to-category.feature).
#
# Every scenario starts from an empty ledger and names the categories it needs, except the one
# about a first start, because the first start is its subject. The names are synthetic test data.

@budget
Feature: See where my money goes on the Overview
  As someone budgeting my money
  I want to see a period's income as one ring, split into what each category is planned to get and how much of that is spent
  So that I can see at a glance where my money goes, how much of it still has no job, and where I have spent past my plan

  # ----------------------------------------------------------------------------------
  # A slice per budget, filled as far as it has been spent
  # ----------------------------------------------------------------------------------

  Scenario: Each category with a budget is a slice sized to its budget and filled as far as it has been spent
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    Then the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size   | filled | over budget |
      | Groceries | 400.00 | 150.00 | no          |
      | Hobby     | 60.00  | 0.00   | no          |
    And the ring for the current budget period should have an Unassigned slice of 1540 euro
    And the ring for the current budget period should add up to 2000 euro

  # The ring adds up to 2000 euro in every row, including the overspent ones. That is what "an
  # overspent slice never grows" means: the slice is the plan, so the rest of the ring keeps its
  # size. One cent of budget is still a slice.
  Scenario Outline: A slice is filled as far as its category has been spent, and never grows past its budget
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of <budget> euro for "Groceries" in the current budget period
    And I have already spent <spent> euro on "Groceries" in the current budget period
    Then the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size     | filled   | over budget |
      | Groceries | <budget> | <filled> | <over>      |
    And the remaining "Groceries" budget in the current budget period should be <remaining> euro
    And the ring for the current budget period should have an Unassigned slice of <unassigned> euro
    And the ring for the current budget period should add up to 2000 euro

    Examples: within budget
      | budget | spent  | filled | over | remaining | unassigned |
      | 400.00 | 150.00 | 150.00 | no   | 250.00    | 1600.00    |
      | 75.50  | 75.49  | 75.49  | no   | 0.01      | 1924.50    |

    Examples: exactly on budget, so filled completely and not over budget
      | budget | spent  | filled | over | remaining | unassigned |
      | 400.00 | 400.00 | 400.00 | no   | 0.00      | 1600.00    |
      | 0.01   | 0.01   | 0.01   | no   | 0.00      | 1999.99    |

    Examples: over budget, so filled completely, the same size, and marked
      | budget | spent  | filled | over | remaining | unassigned |
      | 400.00 | 400.01 | 400.00 | yes  | -0.01     | 1600.00    |
      | 400.00 | 520.00 | 400.00 | yes  | -120.00   | 1600.00    |
      | 0.01   | 0.02   | 0.01   | yes  | -0.01     | 1999.99    |

  # The glossary's own example: "Resterend: −€ 20". The Remaining is shown as the negative figure
  # itself, marked. It is not turned into a positive "over by" amount.
  Scenario: An overspent slice keeps its budget's size, so every other slice keeps its own
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 420 euro on "Groceries" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 20 euro on "Hobby" in the current budget period
    Then the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size   | filled | over budget |
      | Groceries | 400.00 | 400.00 | yes         |
      | Hobby     | 60.00  | 20.00  | no          |
    And the ring for the current budget period should have an Unassigned slice of 1540 euro
    And the ring for the current budget period should add up to 2000 euro
    And the remaining "Groceries" budget in the current budget period should be -20 euro
    And "Groceries" should be shown as over budget in the current budget period

  # ----------------------------------------------------------------------------------
  # A budget of zero gets no slice
  #
  # A slice of zero size cannot be seen. But spending that nothing on screen shows would stop the
  # period adding up to what I know I spent, so a category with spending and no budget is listed
  # with the over-budget marker instead. The 20 euro spent on Gifts below is in no slice: the ring
  # is the income, and none of it was planned for Gifts.
  #
  # "A budget of zero" means zero however it got there. There is no separate "unbudgeted" state
  # (glossary: Budget), so a category never assigned to and one assigned to and then taken back to
  # zero are drawn exactly alike.
  # ----------------------------------------------------------------------------------

  Scenario: A category with spending and no budget gets no slice, and is listed as over budget instead
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    And I have a category "Gifts"
    And I have never set a budget for "Gifts"
    And I have already spent 20 euro on "Gifts" in the current budget period
    Then "Gifts" should have no slice in the ring for the current budget period
    And "Gifts" should be shown in the current budget period
    And "Gifts" should be shown as over budget in the current budget period
    And the remaining "Gifts" budget in the current budget period should be -20 euro
    And the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size   | filled | over budget |
      | Groceries | 400.00 | 150.00 | no          |
    And the ring for the current budget period should have an Unassigned slice of 1600 euro
    And the ring for the current budget period should add up to 2000 euro

  # Hobby is still listed, because it is in use and this is the current period
  # (show-categories-in-a-period.feature). It just has nothing to draw.
  Scenario: A category with no budget and nothing spent gets no slice
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a category "Hobby"
    And I have never set a budget for "Hobby"
    And I have not yet spent anything on "Hobby" in the current budget period
    Then "Hobby" should have no slice in the ring for the current budget period
    And "Hobby" should be shown in the current budget period
    And "Hobby" should not be shown as over budget
    And the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size   | filled | over budget |
      | Groceries | 400.00 | 0.00   | no          |
    And the ring for the current budget period should have an Unassigned slice of 1600 euro

  Scenario: A budget taken back to zero is drawn exactly like one that was never set
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a category "Gifts"
    And I have never set a budget for "Gifts"
    And I have not yet spent anything on "Gifts" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    When I assign -60 euro to "Hobby" in the current budget period
    Then "Hobby" should have no slice in the ring for the current budget period
    And "Gifts" should have no slice in the ring for the current budget period
    And "Hobby" should be shown in the current budget period
    And "Gifts" should be shown in the current budget period
    And "Hobby" should not be shown as over budget
    And "Gifts" should not be shown as over budget
    And the ring for the current budget period should have no category slices
    And the ring for the current budget period should have an Unassigned slice of 2000 euro

  Scenario: Spending against a budget taken back to zero is shown exactly like spending with no budget set
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a category "Gifts"
    And I have never set a budget for "Gifts"
    And I have already spent 20 euro on "Gifts" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 20 euro on "Hobby" in the current budget period
    When I assign -60 euro to "Hobby" in the current budget period
    Then "Hobby" should have no slice in the ring for the current budget period
    And "Gifts" should have no slice in the ring for the current budget period
    And "Hobby" should be shown as over budget in the current budget period
    And "Gifts" should be shown as over budget in the current budget period
    And the remaining "Hobby" budget in the current budget period should be -20 euro
    And the remaining "Gifts" budget in the current budget period should be -20 euro
    And the ring for the current budget period should have no category slices

  # ----------------------------------------------------------------------------------
  # Unassigned has a slice of its own, while it is above zero
  #
  # Without it the ring would show only money that already has a job, and Unassigned is meant to
  # be seen rather than worked out (glossary: "Unassigned money is something you can see"). At
  # exactly zero every euro has a job: there is nothing to draw, and nothing is marked.
  # ----------------------------------------------------------------------------------

  Scenario: Income with no budgets is a ring that is all Unassigned, not an empty ring
    Given I have a category "Groceries"
    And I have never set a budget for "Groceries"
    And I have already recorded 1832.45 euro of income in the current budget period
    Then the ring for the current budget period should not be empty
    And the ring for the current budget period should have no category slices
    And the ring for the current budget period should have an Unassigned slice of 1832.45 euro
    And the ring for the current budget period should add up to 1832.45 euro

  Scenario: One cent of Unassigned is still a slice of its own
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 1999.99 euro for "Groceries" in the current budget period
    Then the ring for the current budget period should have an Unassigned slice of 0.01 euro
    And the ring for the current budget period should add up to 2000 euro
    And the current budget period should not be shown as over-assigned

  Scenario: With every euro assigned there is no Unassigned slice, and the ring is still the income
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 1500 euro for "Groceries" in the current budget period
    And I have a budget of 500 euro for "Hobby" in the current budget period
    Then the ring for the current budget period should have no Unassigned slice
    And the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size    | filled | over budget |
      | Groceries | 1500.00 | 0.00   | no          |
      | Hobby     | 500.00  | 0.00   | no          |
    And the ring for the current budget period should add up to 2000 euro
    And the current budget period should not be shown as over-assigned

  # ----------------------------------------------------------------------------------
  # Over-assigned: the budgets only
  #
  # More has been assigned than the period's income, so there is no Unassigned slice to draw. The
  # ring shows the budgets, each sized exactly as it would be in any other state, and so it adds
  # up to more than the income. Unassigned is shown as the negative figure itself ("Niet
  # toegewezen: −€ 20"), with the marker an overspent category uses. Chosen by the stakeholder
  # over a ring at income size with an overflowing segment.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An over-assigned ring shows the budgets only, and Unassigned as its negative figure, marked
    Given I have already recorded 1500 euro of income in the current budget period
    And I have a budget of 1200 euro for "Groceries" in the current budget period
    And I have already spent 300 euro on "Groceries" in the current budget period
    And I have a budget of <hobby> euro for "Hobby" in the current budget period
    Then the ring for the current budget period should have no Unassigned slice
    And the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size    | filled | over budget |
      | Groceries | 1200.00 | 300.00 | no          |
      | Hobby     | <hobby> | 0.00   | no          |
    And the ring for the current budget period should add up to <total> euro
    And Unassigned in the current budget period should be <unassigned> euro
    And the current budget period should be shown as over-assigned
    And Unassigned in the current budget period should be marked the same way as a category that is over budget

    Examples:
      | hobby  | total   | unassigned |
      | 320.00 | 1520.00 | -20.00     |
      | 300.01 | 1500.01 | -0.01      |

  # Planning before the salary is in. The ring is not empty: it has a budget to draw.
  Scenario: Budgets with no income are an over-assigned ring of budgets only, not an empty ring
    Given I have recorded no income in the current budget period
    And I have a budget of 50 euro for "Groceries" in the current budget period
    Then the ring for the current budget period should not be empty
    And the ring for the current budget period should have no Unassigned slice
    And the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size  | filled | over budget |
      | Groceries | 50.00 | 0.00   | no          |
    And the ring for the current budget period should add up to 50 euro
    And Unassigned in the current budget period should be -50 euro
    And the current budget period should be shown as over-assigned

  # ----------------------------------------------------------------------------------
  # The empty ring
  #
  # Only when the period has neither income nor any Budget. A blank space where the Overview's
  # centrepiece should be would look broken, so the empty ring is an outline with a hint that
  # says what fills it.
  # ----------------------------------------------------------------------------------

  Scenario: A period with neither income nor any budget has an empty ring
    Given I have a category "Groceries"
    And I have never set a budget for "Groceries"
    And I have recorded no income in the current budget period
    Then the ring for the current budget period should be empty, with a hint that there is no income in it
    And the current budget period should not be shown as over-assigned

  # The rule is "neither income nor any Budget", and spending is neither. So spending alone does
  # not fill the ring. It is still shown: Gifts is listed with the over-budget marker, exactly as
  # it would be under a ring that had something in it.
  Scenario: Spending with neither income nor a budget leaves the ring empty, and is listed as over budget
    Given I have a category "Gifts"
    And I have never set a budget for "Gifts"
    And I have recorded no income in the current budget period
    And I have already spent 20 euro on "Gifts" in the current budget period
    Then the ring for the current budget period should be empty, with a hint that there is no income in it
    And "Gifts" should be shown in the current budget period
    And "Gifts" should be shown as over budget in the current budget period
    And the remaining "Gifts" budget in the current budget period should be -20 euro

  # "A budget of zero, however it got there" meeting the empty ring.
  Scenario: Taking the only budget back to zero, with no income, leaves the ring empty again
    Given I have recorded no income in the current budget period
    And I have a budget of 50 euro for "Groceries" in the current budget period
    When I assign -50 euro to "Groceries" in the current budget period
    Then the ring for the current budget period should be empty, with a hint that there is no income in it
    And the current budget period should not be shown as over-assigned

  # ----------------------------------------------------------------------------------
  # Each category's row: Budget, spent, Remaining
  #
  # One row per category shown. A row shows all three figures whether or not the category has a
  # slice, so a category with no budget still shows what was spent against it. The marker sits
  # beside a Remaining below zero, and only there: exactly zero is not over budget.
  # ----------------------------------------------------------------------------------

  Scenario: Each category row shows its budget, what was spent and what remains, marked when over budget
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 60 euro on "Hobby" in the current budget period
    And I have a budget of 30 euro for "Magazines" in the current budget period
    And I have already spent 30.01 euro on "Magazines" in the current budget period
    And I have a category "Gifts"
    And I have never set a budget for "Gifts"
    And I have already spent 20 euro on "Gifts" in the current budget period
    And I have a category "Rent"
    And I have never set a budget for "Rent"
    And I have not yet spent anything on "Rent" in the current budget period
    Then the categories shown in the current budget period should be exactly these, in this order:
      | category  | budget | spent  | remaining | over budget |
      | Groceries | 400.00 | 150.00 | 250.00    | no          |
      | Hobby     | 60.00  | 60.00  | 0.00      | no          |
      | Magazines | 30.00  | 30.01  | -0.01     | yes         |
      | Gifts     | 0.00   | 20.00  | -20.00    | yes         |
      | Rent      | 0.00   | 0.00   | 0.00      | no          |

  # ----------------------------------------------------------------------------------
  # The order of the rows and the slices
  #
  # Largest Budget first. Equal budgets keep the order the categories were added in, which is
  # also where every zero-budget category ends up: after all the budgeted ones, in the order
  # added. The ring's category slices follow the rows, and the Unassigned slice is always last.
  #
  # The order is by Budget: not by what was spent, not by Remaining, and not by name. The names
  # below are chosen so that alphabetical order and the order added would each give a different
  # answer from the rule.
  # ----------------------------------------------------------------------------------

  # Hobby has spent more than Groceries and is still below it: the order is by Budget, not by
  # spending.
  Scenario: Rows and slices are in order of budget, largest first, and share that order
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a category "Gifts"
    And I have never set a budget for "Gifts"
    And I have already spent 20 euro on "Gifts" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 180 euro on "Hobby" in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    And I have a category "Magazines"
    And I have never set a budget for "Magazines"
    And I have a budget of 900 euro for "Rent" in the current budget period
    Then the categories shown in the current budget period should be exactly these, in this order:
      | category  |
      | Rent      |
      | Groceries |
      | Hobby     |
      | Gifts     |
      | Magazines |
    And the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size   | filled | over budget |
      | Rent      | 900.00 | 0.00   | no          |
      | Groceries | 400.00 | 150.00 | no          |
      | Hobby     | 60.00  | 60.00  | yes         |
    And the ring for the current budget period should have an Unassigned slice of 640 euro
    And the Unassigned slice should be the last slice in the ring for the current budget period

  # Hobby was added before Groceries, so it comes first. Alphabetically it would not.
  Scenario: Categories with equal budgets keep the order they were added in
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 250 euro for "Hobby" in the current budget period
    And I have a budget of 250 euro for "Groceries" in the current budget period
    And I have a budget of 400 euro for "Rent" in the current budget period
    Then the categories shown in the current budget period should be exactly these, in this order:
      | category  |
      | Rent      |
      | Hobby     |
      | Groceries |
    And the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size   | filled | over budget |
      | Rent      | 400.00 | 0.00   | no          |
      | Hobby     | 250.00 | 0.00   | no          |
      | Groceries | 250.00 | 0.00   | no          |

  # Magazines was added first and has no budget, so one cent of budget puts Hobby above it. Rent
  # had a budget and gave it all back, so its zero is like any other zero: it goes among the
  # zero-budget categories, in the order it was added.
  Scenario: Every category with a budget of zero comes after the budgeted ones, in the order they were added
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a category "Magazines"
    And I have never set a budget for "Magazines"
    And I have a budget of 900 euro for "Rent" in the current budget period
    And I have a budget of 0.01 euro for "Hobby" in the current budget period
    And I have a category "Gifts"
    And I have never set a budget for "Gifts"
    And I have already spent 20 euro on "Gifts" in the current budget period
    When I assign -900 euro to "Rent" in the current budget period
    Then the categories shown in the current budget period should be exactly these, in this order:
      | category  |
      | Hobby     |
      | Magazines |
      | Rent      |
      | Gifts     |
    And the ring for the current budget period should have exactly these category slices, in this order:
      | category | size | filled | over budget |
      | Hobby    | 0.01 | 0.00   | no          |

  # Unassigned is last whether it is the largest slice in the ring or the smallest.
  Scenario Outline: The Unassigned slice is the last slice, whatever its size
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of <groceries> euro for "Groceries" in the current budget period
    And I have a budget of <hobby> euro for "Hobby" in the current budget period
    Then the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size        | filled | over budget |
      | Groceries | <groceries> | 0.00   | no          |
      | Hobby     | <hobby>     | 0.00   | no          |
    And the ring for the current budget period should have an Unassigned slice of <unassigned> euro
    And the Unassigned slice should be the last slice in the ring for the current budget period

    Examples:
      | groceries | hobby  | unassigned |
      | 400.00    | 60.00  | 1540.00    |
      | 1500.00   | 499.99 | 0.01       |

  # Groceries was added first and starts with the larger budget. Assigning to Hobby moves it up
  # only once its budget is larger. Exactly equal is a tie, and a tie keeps the order added.
  Scenario Outline: Assigning can change the order, once it changes which budget is larger
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a budget of 300 euro for "Hobby" in the current budget period
    When I assign <amount> euro to "Hobby" in the current budget period
    Then the categories shown in the current budget period should be exactly these, in this order:
      | category |
      | <first>  |
      | <second> |
    And the ring for the current budget period should have exactly these category slices, in this order:
      | category | size          | filled | over budget |
      | <first>  | <first size>  | 0.00   | no          |
      | <second> | <second size> | 0.00   | no          |
    And the Unassigned slice should be the last slice in the ring for the current budget period

    Examples:
      | amount | first     | first size | second    | second size |
      | 99.99  | Groceries | 400.00     | Hobby     | 399.99      |
      | 100.00 | Groceries | 400.00     | Hobby     | 400.00      |
      | 100.01 | Hobby     | 400.01     | Groceries | 400.00      |

  # Hobby was added before Groceries, archived, and then brought back after Groceries was added.
  # The two budgets are equal, so the order added decides, and Hobby's place is where it was first
  # added. Were it counted as added again, Groceries would come first.
  Scenario: A category brought back keeps its original place among equal budgets
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a category "Hobby"
    And I have archived the category "Hobby"
    And I have a budget of 250 euro for "Groceries" in the current budget period
    When I assign 250 euro to "Hobby" in the current budget period
    Then I should be told that "Hobby" was brought back
    And the categories shown in the current budget period should be exactly these, in this order:
      | category  |
      | Hobby     |
      | Groceries |
    And the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size   | filled | over budget |
      | Hobby     | 250.00 | 0.00   | no          |
      | Groceries | 250.00 | 0.00   | no          |

  # ----------------------------------------------------------------------------------
  # Archived categories and past periods
  # ----------------------------------------------------------------------------------

  # Unassigned subtracts every budget in the period, archived categories' included. If an archived
  # category's budget were not a slice, the ring would fall short of the income by exactly that
  # budget. An archived category with a budget above zero has history in the period, so it is
  # listed anyway (archive-category.feature).
  Scenario: An archived category's budget is still a slice, so the ring still adds up to the income
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 25 euro on "Hobby" in the current budget period
    When I archive the category "Hobby"
    Then the ring for the current budget period should have exactly these category slices, in this order:
      | category  | size   | filled | over budget |
      | Groceries | 400.00 | 150.00 | no          |
      | Hobby     | 60.00  | 25.00  | no          |
    And the ring for the current budget period should have an Unassigned slice of 1540 euro
    And the ring for the current budget period should add up to 2000 euro

  # "Past is past": a plan forgotten in a period that has ended stays forgotten, and the ring
  # shows that period as it was (glossary: "Assigning happens in the current budget period and
  # later ones").
  Scenario: A past period's ring is drawn by the same rules
    Given my budget periods are one month long
    And I have already recorded 1800 euro of income in the previous budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have already spent 412.50 euro on "Groceries" in the previous budget period
    And I have a category "Gifts"
    And I have never set a budget for "Gifts"
    And I have already spent 30 euro on "Gifts" in the previous budget period
    Then the ring for the previous budget period should have exactly these category slices, in this order:
      | category  | size   | filled | over budget |
      | Groceries | 400.00 | 400.00 | yes         |
    And the ring for the previous budget period should have an Unassigned slice of 1400 euro
    And the ring for the previous budget period should add up to 1800 euro
    And "Gifts" should have no slice in the ring for the previous budget period
    And "Gifts" should be shown as over budget in the previous budget period

  # ----------------------------------------------------------------------------------
  # A first start
  #
  # MoneyBud starts as a first start does: the six default categories and nothing else
  # (glossary: "What the UI starts with, and what it keeps"). That the defaults exist is specified
  # in add-category.feature and not repeated. What is new is what the Overview shows. Nothing is
  # kept when MoneyBud closes, so for now every start is a first start. That MoneyBud opens on the
  # current period is in step-between-periods.feature.
  #
  # All six have a budget of zero, so they are listed in the order they were added. The defaults
  # count as added in the order they ship in, which is the stakeholder's own list (settled
  # 2026-09-25). That order is neither alphabetical nor anything else a reader could work out.
  # ----------------------------------------------------------------------------------

  Scenario: On a first start, the current period lists the six default categories under an empty ring
    When I start using MoneyBud for the first time
    Then the categories shown in the current budget period should be exactly these, in this order:
      | category      |
      | Boodschappen  |
      | Huur          |
      | Hobby         |
      | Sparen        |
      | Verzekeringen |
      | Abonnementen  |
    And the ring for the current budget period should be empty, with a hint that there is no income in it
    And the current budget period should not be shown as over-assigned
