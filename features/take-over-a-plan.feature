# Opening a period: MoneyBud offers back the latest earlier plan, and one act takes it over in
# full (glossary: "Budgets carry over as figures, not as assignments" and "Opening a period",
# settled by the stakeholder on 2026-09-26). The figures are remembered. The money is not
# assigned until I take the plan over, so a period still starts with its income wholly
# Unassigned.
#
# The rules, from arc42 §12:
#   - WHEN IT IS OFFERED. In the current period and every later one, while every Budget in that
#     period is zero. A past period never shows the offer, because a past plan cannot be changed.
#     The offer is a state, not an event: it is there whenever those conditions hold and gone
#     otherwise.
#   - "EVERY BUDGET IS ZERO" means zero however it got there. The first amount assigned in the
#     period, by hand or by taking the plan over, ends the offer. Taking every Budget back to zero
#     brings it back. Assigning zero, and a negative amount clipped against a Budget already at
#     zero, change no figure, so neither ends it. An ARCHIVED category's Budget above zero counts:
#     it is still assigned money, so the period is not empty.
#   - WHERE THE PLAN COMES FROM. The latest period EARLIER THAN THE PERIOD ON SCREEN that has a
#     plan, however far back. So a future period can be offered the current period's plan. A
#     period has a plan only if it has a Budget above zero for a category that is NOT ARCHIVED
#     NOW. Spending is not a plan. If no earlier period qualifies, nothing is offered.
#   - WHAT IS OFFERED. Every category in that plan with a Budget above zero there, and not
#     archived now. A category whose figure there was zero is not in the plan, and that includes
#     a category added since. Figures follow the category, not its name, so a category renamed
#     since is offered under its new name.
#   - HOW IT IS SHOWN. A button in the assign area names the period whose plan is offered and the
#     plan's total, and each category row in the plan shows, in grey, the figure it would take
#     over. A row whose category is not in the plan shows no figure, not a zero. The grey figures
#     are shown only while the plan is offered.
#   - THE ORDER OF THE ROWS while the plan is offered is by grey figure, largest first, and equal
#     figures in the order the categories were added. A row with no grey figure counts as zero.
#     That is the order the rows will have once the plan is taken over, so nothing jumps.
#     Settled by the stakeholder at the scenario gate on 2026-09-26, over keeping the order of
#     the period's own Budgets, which are all zero while the plan is offered.
#   - TAKING IT OVER assigns every figure in full into the PERIOD ON SCREEN, even past what
#     Unassigned holds, so the period may go Over-assigned. It is not the period the assign form
#     may have been moved to: the button belongs with the grey figures, which are on the rows of
#     the period on screen. It is one act, with one notice, and the notice names the period the
#     plan went into. It brings no archived category back, because no archived category's figure
#     is in the plan, and it touches no expense. It cannot be undone in one step, and there is no
#     act to clear a plan. A mistake is corrected row by row with negative assignments.
#   - ACROSS A PERIOD BOUNDARY. If MoneyBud stays open into a new period, the screen stays on the
#     period it showed, which is now past. At the next redraw the offer and the grey figures go,
#     and nothing is said. Pressing the button before that redraw is refused, like any assignment
#     in a past period.
#
# On screen, as proposed in the glossary ("Proposed for opening a period"): the button reads, for
# example, "Plan van augustus 2026 overnemen (€ 1.450,00)", and the grey figure "plan: € 400,00".
# The period is named the way MoneyBud names any period. Both Dutch terms are proposals the plan
# may refine, and the sentence around them is copy. So, as in point-at-a-slice.feature, the steps
# below compare the PERIOD and the AMOUNTS, not the text. How a period's name and an amount are
# written on screen is held by developer unit tests.
#
# Reading the steps:
#   - "the budget period 2 before the current one" is keep-data.feature's way of naming a period
#     further back than the previous one.
#   - "the plan of the A budget period should be offered in the B budget period, with a total of
#     X euro" is about the Overview while period B is on screen: the offer is there, it names
#     period A, and its total is X. "no plan should be offered in the B budget period" means the
#     Overview shows no offer while B is on screen.
#   - "the categories shown in the ... budget period should be exactly these, in this order" is
#     overview.feature's step. It checks the columns its table has, and this file gives it a new
#     one: PLAN is the grey figure on that row, in euro, and "none" means the row shows no plan
#     figure at all. "no category row in the ... budget period should show a plan figure" is the
#     same "none" for every row.
#   - "the period on screen should offer no plan" and "no category row on screen should show a
#     plan figure" read the screen as it stands, like step-between-periods.feature's "the period
#     on screen should no longer be labelled as the current budget period". They are used where
#     the point is what the screen shows WITHOUT anything being done to it.
#   - "I take over the plan offered" presses the offer's button, in the period on screen. "I try
#     to take over the plan offered" is the same act, where the scenario expects a refusal. "the
#     take-over should go through" means the plan's figures were assigned. "the take-over should
#     be refused" means nothing was assigned anywhere.
#   - "I should be told that the plan was taken over into the ... budget period" is the notice.
#     What is fixed is that I am told, and which period it names, not the wording.
#   - "I have set the period to assign in to the next budget period, and assigned nothing" means
#     the assign form's own period was moved away from the period on screen, and left there.
#   - "the next budget period begins while MoneyBud is open, before the Overview is next drawn"
#     is step-between-periods.feature's step, stopped at the moment the clock has passed the
#     boundary and the screen still shows what it showed before. The plain step, without that
#     ending, includes the redraw. The Overview redraws itself at most a minute later, and at
#     once after any act.
#   - After a period boundary, periods are named relative to the NEW today, as in
#     step-between-periods.feature: the period that was current is the previous one.
#   - Every other step is reused unchanged from the file that introduced it, with the meaning its
#     header gives it.
#
# Out of scope: accounts and the sweep; undoing a take-over; any act that clears a period's plan;
# comparing a period with earlier ones. That a plan taken over is kept like any other assignment
# is keep-data.feature's rule and is not restated.
#
# Every scenario starts from an empty ledger and names the categories it needs, except the one
# about a first start. The names are synthetic test data.

@budget
Feature: Take over a plan
  As someone budgeting my money
  I want a new budget period to offer me the plan I made last, and to take it over in one go
  So that re-planning a month much like the last one is next to no work, while nothing is assigned until I choose it

  # ----------------------------------------------------------------------------------
  # When the plan is offered
  # ----------------------------------------------------------------------------------

  # The plan was made two periods back. Nothing is assigned since. The past period between
  # shows no offer; the current and the next both do.
  Scenario: The plan is offered in the current period and later ones, never in a past one
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the budget period 2 before the current one
    Then no plan should be offered in the previous budget period
    And the plan of the budget period 2 before the current one should be offered in the current budget period, with a total of 400 euro
    And the plan of the budget period 2 before the current one should be offered in the next budget period, with a total of 400 euro

  # One cent to a category that is not even in the plan is enough: any amount assigned in the
  # period ends the offer, and the grey figures go with it. Nothing of the plan is taken over.
  #
  # The last step is a consequence of "the latest earlier plan", and the stakeholder accepted it
  # at the scenario gate on 2026-09-26: the current period now has a plan of its own, so it is
  # the latest plan before the next period, and the next period is offered that one-cent plan
  # instead of the previous period's.
  Scenario: The first amount assigned by hand ends the offer, and nothing of the plan is taken over
    Given my budget periods are one month long
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have a budget of 60 euro for "Hobby" in the previous budget period
    And I have a category "Gifts"
    And I have never set a budget for "Gifts"
    When I assign 0.01 euro to "Gifts" in the current budget period
    Then the assignment should go through
    And no plan should be offered in the current budget period
    And no category row in the current budget period should show a plan figure
    And the budget for "Groceries" in the current budget period should be 0.00 euro
    And the budget for "Hobby" in the current budget period should be 0.00 euro
    And the budget for "Gifts" in the current budget period should be 0.01 euro
    And the plan of the current budget period should be offered in the next budget period, with a total of 0.01 euro

  # A Budget taken back to zero is the same as one never made, whether all of it came back or
  # the negative amount was clipped at zero.
  Scenario Outline: Taking every budget back to zero brings the offer back
    Given my budget periods are one month long
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have a budget of 100 euro for "Hobby" in the current budget period
    When I assign <amount> euro to "Hobby" in the current budget period
    Then the assignment should go through
    And the budget for "Hobby" in the current budget period should be 0.00 euro
    And Unassigned in the current budget period should be 2000 euro
    And the plan of the previous budget period should be offered in the current budget period, with a total of 400 euro
    And the categories shown in the current budget period should be exactly these, in this order:
      | category  | plan   |
      | Groceries | 400.00 |
      | Hobby     | none   |

    Examples:
      | amount  |
      | -100.00 |
      | -150.00 |

  # Neither changes any figure (assign-to-category.feature), so every Budget is still zero.
  Scenario Outline: Assigning zero, or a negative amount against a budget of zero, leaves the offer standing
    Given my budget periods are one month long
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have set no budget for "Groceries" in the current budget period
    When I assign <amount> euro to "Groceries" in the current budget period
    Then the assignment should go through
    And the budget for "Groceries" in the current budget period should be 0.00 euro
    And the plan of the previous budget period should be offered in the current budget period, with a total of 400 euro
    And the categories shown in the current budget period should be exactly these, in this order:
      | category  | plan   |
      | Groceries | 400.00 |

    Examples:
      | amount |
      | 0.00   |
      | -20.00 |

  # Hobby was archived with 60 euro still assigned to it. That is money with a job, so the period
  # is not empty. Taking it back is how an archived category's budget returns to Unassigned, and
  # once it has, the offer appears.
  Scenario: An archived category's budget counts, so the period gets no offer until it is taken back
    Given my budget periods are one month long
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    And I have archived the category "Hobby"
    Then no plan should be offered in the current budget period
    When I assign -60 euro to "Hobby" in the current budget period
    Then the assignment should go through
    And the budget for "Hobby" in the current budget period should be 0.00 euro
    And the plan of the previous budget period should be offered in the current budget period, with a total of 400 euro

  # ----------------------------------------------------------------------------------
  # Where the plan comes from
  # ----------------------------------------------------------------------------------

  Scenario: On a first start there is no earlier plan, so nothing is offered
    When I start using MoneyBud for the first time
    Then no plan should be offered in the current budget period
    And no category row in the current budget period should show a plan figure

  # The glossary's own example. August was planned, September went by unplanned, and it is now
  # October, with nothing assigned and the salary not yet in. September has spending and a
  # budget of zero. Neither is a plan, so October is offered August's.
  #
  # While the plan is offered, the rows are in order of their grey figures, largest first. That
  # is the order they will have once it is taken over, not the order the categories were added
  # in (Groceries, Rent, Hobby).
  Scenario: A period with no plan is skipped, and the latest earlier plan is offered, however far back
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the budget period 2 before the current one
    And I have a budget of 900 euro for "Rent" in the budget period 2 before the current one
    And I have a budget of 150 euro for "Hobby" in the budget period 2 before the current one
    And I have a budget of 0 euro for "Rent" in the previous budget period
    And I have already spent 380 euro on "Groceries" in the previous budget period
    And I have recorded no income in the current budget period
    Then the plan of the budget period 2 before the current one should be offered in the current budget period, with a total of 1450 euro
    And the categories shown in the current budget period should be exactly these, in this order:
      | category  | plan   |
      | Rent      | 900.00 |
      | Groceries | 400.00 |
      | Hobby     | 150.00 |

  # The previous period's only budget belongs to a category archived since. Its figure would not
  # be offered, so that period has nothing to offer, and the search goes further back.
  Scenario: A period whose only budgets are for categories archived now has no plan, and is skipped
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the budget period 2 before the current one
    And I have a budget of 30 euro for "Magazines" in the previous budget period
    And I have archived the category "Magazines"
    Then the plan of the budget period 2 before the current one should be offered in the current budget period, with a total of 400 euro
    And the categories shown in the current budget period should be exactly these, in this order:
      | category  | plan   |
      | Groceries | 400.00 |

  # "Archived now" is read when the offer is shown. With Hobby archived, no earlier period has a
  # plan, so nothing is offered. Bring Hobby back and the previous period's plan counts again.
  Scenario: Whether a category is archived is read when the offer is shown
    Given my budget periods are one month long
    And I have a budget of 60 euro for "Hobby" in the previous budget period
    And I have archived the category "Hobby"
    Then no plan should be offered in the current budget period
    When I add a category "Hobby"
    Then I should be told that "Hobby" was brought back
    And the plan of the previous budget period should be offered in the current budget period, with a total of 60 euro
    And the categories shown in the current budget period should be exactly these, in this order:
      | category | plan  |
      | Hobby    | 60.00 |

  # "Earlier" is earlier than the period the offer is for, not earlier than today. The next
  # period's latest earlier plan is the current period's, not the previous one's. The current
  # period has a plan, so it has no offer itself. The total is the plan's figures added to the
  # cent. The notice names the next period, which is where the plan went.
  Scenario: A later period is offered the latest plan before it, which can be the current period's
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have a budget of 312.45 euro for "Groceries" in the current budget period
    And I have a budget of 0.01 euro for "Hobby" in the current budget period
    And I have already recorded 1800 euro of income in the next budget period
    And the Overview shows the next budget period
    Then no plan should be offered in the current budget period
    And the plan of the current budget period should be offered in the next budget period, with a total of 312.46 euro
    And the categories shown in the next budget period should be exactly these, in this order:
      | category  | plan   |
      | Groceries | 312.45 |
      | Hobby     | 0.01   |
    When I take over the plan offered
    Then the take-over should go through
    And I should be told that the plan was taken over into the next budget period
    And the budget for "Groceries" in the next budget period should be 312.45 euro
    And the budget for "Hobby" in the next budget period should be 0.01 euro
    And Unassigned in the next budget period should be 1487.54 euro
    And the budget for "Groceries" in the current budget period should still be 312.45 euro
    And the Overview should show the next budget period

  # ----------------------------------------------------------------------------------
  # What is offered
  # ----------------------------------------------------------------------------------

  # Hobby is archived, so its figure is not offered and is not in the total. It has no history in
  # the current period, so it has no row there either. Taking the plan over leaves it archived.
  Scenario: An archived category's figure is not offered, and taking the plan over leaves it archived
    Given my budget periods are one month long
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have a budget of 60 euro for "Hobby" in the previous budget period
    And I have archived the category "Hobby"
    And the Overview shows the current budget period
    Then the plan of the previous budget period should be offered in the current budget period, with a total of 400 euro
    And the categories shown in the current budget period should be exactly these, in this order:
      | category  | plan   |
      | Groceries | 400.00 |
    When I take over the plan offered
    Then the take-over should go through
    And the budget for "Groceries" in the current budget period should be 400 euro
    And the budget for "Hobby" in the current budget period should be 0.00 euro
    And Unassigned in the current budget period should be 1600 euro
    And the categories offered for a new expense should not include "Hobby"

  # Hobby's budget in the previous period was zero, and Gifts was added
  # since. Neither is in the plan, so neither row shows a figure, not even a zero, and taking the
  # plan over leaves both at zero.
  Scenario: A category with no figure in the plan shows no plan figure, and taking the plan over leaves it at zero
    Given my budget periods are one month long
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have a budget of 0 euro for "Hobby" in the previous budget period
    And I have a category "Gifts"
    And I have never set a budget for "Gifts"
    And the Overview shows the current budget period
    Then the plan of the previous budget period should be offered in the current budget period, with a total of 400 euro
    And the categories shown in the current budget period should be exactly these, in this order:
      | category  | plan   |
      | Groceries | 400.00 |
      | Hobby     | none   |
      | Gifts     | none   |
    When I take over the plan offered
    Then the take-over should go through
    And the budget for "Groceries" in the current budget period should be 400 euro
    And the budget for "Hobby" in the current budget period should be 0.00 euro
    And the budget for "Gifts" in the current budget period should be 0.00 euro
    And Unassigned in the current budget period should be 1600 euro

  # Renaming changes only the label (rename-a-category.feature). The figure follows the category.
  Scenario: A category renamed since is offered under its new name
    Given my budget periods are one month long
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And the Overview shows the current budget period
    When I rename the category "Groceries" to "Food"
    Then the plan of the previous budget period should be offered in the current budget period, with a total of 400 euro
    And the categories shown in the current budget period should be exactly these, in this order:
      | category | plan   |
      | Food     | 400.00 |
    When I take over the plan offered
    Then the take-over should go through
    And the budget for "Food" in the current budget period should be 400 euro

  # ----------------------------------------------------------------------------------
  # Taking the plan over
  # ----------------------------------------------------------------------------------

  # The glossary's own figures, on the 1st, before the salary is recorded. The plan goes in in
  # full, and the period is Over-assigned, shown with the marker and never warned about, until
  # the income is recorded. The offer and the grey figures are gone, so the plan cannot be taken
  # over twice.
  Scenario: Taking the plan over assigns every figure in full, even with no income yet, and says where it went
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have a budget of 900 euro for "Rent" in the previous budget period
    And I have a budget of 150 euro for "Hobby" in the previous budget period
    And I have recorded no income in the current budget period
    And the Overview shows the current budget period
    When I take over the plan offered
    Then the take-over should go through
    And I should not be warned or asked to confirm
    And I should be told that the plan was taken over into the current budget period
    And the budget for "Groceries" in the current budget period should be 400 euro
    And the budget for "Rent" in the current budget period should be 900 euro
    And the budget for "Hobby" in the current budget period should be 150 euro
    And Unassigned in the current budget period should be -1450 euro
    And the current budget period should be shown as over-assigned
    And Unassigned in the current budget period should be marked the same way as a category that is over budget
    And no plan should be offered in the current budget period
    And no category row in the current budget period should show a plan figure

  # Never only up to what Unassigned holds: one cent short, exactly enough and more than enough
  # all take the same 1450 euro.
  Scenario Outline: Taking the plan over assigns it in full, whatever Unassigned holds
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have a budget of 900 euro for "Rent" in the previous budget period
    And I have a budget of 150 euro for "Hobby" in the previous budget period
    And I have already recorded <income> euro of income in the current budget period
    And the Overview shows the current budget period
    When I take over the plan offered
    Then the take-over should go through
    And the budget for "Groceries" in the current budget period should be 400 euro
    And the budget for "Rent" in the current budget period should be 900 euro
    And the budget for "Hobby" in the current budget period should be 150 euro
    And Unassigned in the current budget period should be <unassigned> euro

    Examples:
      | income  | unassigned |
      | 1449.99 | -0.01      |
      | 1450.00 | 0.00       |
      | 2000.00 | 550.00     |

  # The assign form has a period of its own, moved here to the next period. The button still takes
  # the plan into the period on screen, the one whose rows carry the grey figures.
  Scenario: The plan goes into the period on screen, not the period the assign form was moved to
    Given my budget periods are one month long
    And I have already recorded 2000 euro of income in the current budget period
    And I have already recorded 1800 euro of income in the next budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And the Overview shows the current budget period
    And I have set the period to assign in to the next budget period, and assigned nothing
    When I take over the plan offered
    Then the take-over should go through
    And I should be told that the plan was taken over into the current budget period
    And the budget for "Groceries" in the current budget period should be 400 euro
    And Unassigned in the current budget period should be 1600 euro
    And the budget for "Groceries" in the next budget period should be 0.00 euro
    And Unassigned in the next budget period should still be 1800 euro
    And the Overview should show the current budget period

  # Spending is not a plan, so 120 euro already spent does not end the offer. Taking the plan over
  # is planning only: the 120 euro stays spent, and Remaining is the new Budget less what was
  # spent. Groceries was over budget with a Budget of zero, and is not any more.
  Scenario: Taking the plan over touches no expense
    Given my budget periods are one month long
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have already spent 120 euro on "Groceries" in the current budget period
    And the Overview shows the current budget period
    Then the plan of the previous budget period should be offered in the current budget period, with a total of 400 euro
    When I take over the plan offered
    Then the take-over should go through
    And the budget for "Groceries" in the current budget period should be 400 euro
    And the remaining "Groceries" budget in the current budget period should be 280 euro
    And "Groceries" should not be shown as over budget
    And Unassigned in the current budget period should be 1600 euro

  # ----------------------------------------------------------------------------------
  # Staying open across a period boundary
  #
  # The screen stays on the period it showed, which becomes the previous period at the boundary.
  # Nothing is announced at a boundary, and the offer's going is no exception. (The one exception, a
  # sweep that moved money, is in sweep-at-a-period-end.feature. There is no sweep destination here.)
  # ----------------------------------------------------------------------------------

  Scenario: When a new period begins while MoneyBud is open, the offer and its figures go without a word
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And the Overview shows the current budget period
    Then the plan of the previous budget period should be offered in the current budget period, with a total of 400 euro
    When the next budget period begins while MoneyBud is open
    Then the Overview should show the previous budget period
    And the period on screen should offer no plan
    And no category row on screen should show a plan figure
    And I should not have been told anything

  # For up to a minute after the boundary the button can still be on screen. Pressing it then
  # meets the past-period refusal, which is all a take-over in a past period could do. Nothing is
  # assigned in any period. The refusal is an act, and the screen redraws after any act, so the
  # offer is gone afterwards.
  Scenario: Taking the plan over after the period has ended, before the screen has caught up, is refused
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And the Overview shows the current budget period
    Then the plan of the previous budget period should be offered in the current budget period, with a total of 400 euro
    When the next budget period begins while MoneyBud is open, before the Overview is next drawn
    And I try to take over the plan offered
    Then the take-over should be refused
    And I should be told that nothing can be assigned in a past budget period
    And the budget for "Groceries" in the previous budget period should still be 0.00 euro
    And Unassigned in the previous budget period should still be 2000 euro
    And the budget for "Groceries" in the current budget period should be 0.00 euro
    And the period on screen should offer no plan
