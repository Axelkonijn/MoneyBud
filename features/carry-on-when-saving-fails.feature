# When a change cannot be saved: a full disk, something else holding on to the data, a profile that
# is not there (glossary: "When a save fails, MoneyBud says so and keeps going", settled by the
# stakeholder on 2026-09-26). What is kept when saving works is in keep-data.feature.
#
# The rules, from arc42 §12:
#   - MoneyBud SAYS THE CHANGE WAS NOT SAVED, and LETS ME CARRY ON. The change stands in MoneyBud
#     and on screen. Nothing is undone and nothing is refused because a save failed: "MoneyBud shows,
#     it never blocks", applied to saving. Undoing the change, and refusing further entries until a
#     save succeeds, were both rejected.
#   - The "not saved" notice is A LASTING STATE, not a one-off message: it STAYS ON SCREEN UNTIL A
#     LATER SAVE SUCCEEDS. The stakeholder's reason: otherwise the next message would hide it, and I
#     could close MoneyBud without knowing that changes were not saved. So it stays shown BESIDE any
#     other notice and BESIDE the removal question, and stepping between periods, which clears the
#     last notice, does not clear it. This is the one exception to "one message at a time". Where
#     and how the two are laid out is for the plan.
#   - EVERY LATER CHANGE TRIES AGAIN, and so does MoneyBud BY ITSELF, NOW AND THEN, with nothing
#     done. Every save keeps EVERYTHING, not just the last change. So one save that succeeds keeps
#     every change whose own save failed before it, and once saving is possible again the notice
#     goes without my doing anything. How often MoneyBud tries by itself is for the plan.
#   - When a save succeeds after a failure, MoneyBud SAYS ONCE THAT EVERYTHING IS SAVED AGAIN, as
#     the lasting notice goes. That is an ordinary notice, like any other. When saving has never
#     failed, nothing is said about saving (keep-data.feature).
#   - CLOSING MAKES ONE LAST ATTEMPT TO SAVE. If it succeeds, nothing is lost. If it fails, the
#     changes that were not saved are lost: accepted, with that consequence in front of the
#     stakeholder. Either way CLOSING JUST CLOSES: there is no question first. The lasting notice is
#     the warning.
#   - A SAVE THAT IS INTERRUPTED (MoneyBud or the computer stopping in the middle of it) NEVER
#     DAMAGES WHAT WAS SAVED BEFORE. At worst the change that was being saved is lost. The next
#     start opens as usual and SAYS NOTHING about it.
#
# All of the above was settled by the stakeholder on 2026-09-26, the last four points in answer to
# questions raised while this file was written.
#
# "I should not have been told anything" in the interrupted-save scenario is checked at the start
# after the interruption: that start shows no message of any kind, and neither the loss nor the
# data being unreadable is mentioned.
#
# Reading the steps:
#   - "saving is not possible" means every save MoneyBud tries fails, until "saving is possible
#     again". What causes it is not the point: the rulings treat every cause alike. Like "the next
#     budget period begins while MoneyBud is open" (step-between-periods.feature), "saving is
#     possible again" and "a while passes with nothing done" are Whens that happen around MoneyBud,
#     not on its screen. "A while" is long enough for MoneyBud to have tried again by itself. Its
#     length is not named here, so the scenario holds for whatever the plan chooses.
#   - "MoneyBud should show that my changes are not saved" means the lasting notice is on screen.
#     "should still show" is the same, said after something else has happened. "should no longer
#     show" and "should not show" mean it is not on screen. What is fixed is what the notice says,
#     not its wording.
#   - "I should be told that everything is saved again" is the one-off notice as the lasting one
#     goes. What is fixed is what it says, not its wording.
#   - "I should not have been told anything" (step-between-periods.feature) means no NEW message
#     appeared. The lasting notice is not a new message, so it does not count as being told.
#   - "I close MoneyBud" and "I start MoneyBud" are the two halves of keep-data.feature's "I close
#     MoneyBud and start it again", written apart so that closing can be checked on its own. "MoneyBud
#     should close without asking me anything" means it closes, and no question came first. "I close
#     MoneyBud before it has tried to save again" means I close it after saving has become possible
#     again, but before any change or MoneyBud itself has tried.
#   - "MoneyBud is interrupted while saving it" means MoneyBud stops, without closing properly, in
#     the middle of saving the change just made, and before that save has finished.
#   - "I ask to remove the expense labelled "X"", "MoneyBud should be asking me to confirm" and "I
#     confirm" are the removal act of remove-an-entry.feature in its two halves, as in
#     keep-data.feature.
#   - Every Given describes something done in MoneyBud earlier, and whatever the Givens set up
#     BEFORE "saving is not possible" has been kept.
#   - Every other step is reused unchanged from the file that introduced it.
#
# Every scenario starts from an empty ledger and names the categories it needs. The names, labels
# and amounts are synthetic test data.

@keeping
Feature: Carry on when saving fails
  As someone entering my money by hand
  I want to be told, for as long as it is true, that my changes could not be kept, and still be able to carry on
  So that a passing problem does not stop me working, and I never close MoneyBud believing something was kept when it was not

  # ----------------------------------------------------------------------------------
  # Every change tries to save, and says when it could not
  # ----------------------------------------------------------------------------------

  # One row per kind of change MoneyBud has. Each is a change to what MoneyBud keeps, so each
  # tries to save, and each says so when it cannot.
  Scenario Outline: Any change that cannot be saved still goes through, and MoneyBud shows that it was not saved
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    And I have a category "Magazines"
    And saving is not possible
    When I <change>
    Then MoneyBud should show that my changes are not saved

    Examples:
      | change                                                                 |
      | record an expense of 12.50 euro for "Groceries" labelled "Kiosk"       |
      | record an income of 25 euro labelled "Statiegeld"                      |
      | assign 50 euro to "Groceries" in the current budget period             |
      | add a category "Hobby"                                                 |
      | archive the category "Groceries"                                       |
      | rename the category "Groceries" to "Food"                              |
      | delete the category "Magazines"                                        |
      | change the amount of the expense labelled "Albert Heijn" to 23.15 euro |
      | remove the expense labelled "Albert Heijn" and confirm                 |

  Scenario: An expense that cannot be saved is still recorded, and counts in every figure
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And saving is not possible
    When I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn"
    Then the expense should be recorded
    And MoneyBud should show that my changes are not saved
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And the remaining "Groceries" budget in the current budget period should be 367.85 euro

  # ----------------------------------------------------------------------------------
  # The notice lasts until a save succeeds
  #
  # It is shown beside whatever else the screen says, and stepping does not clear it.
  # ----------------------------------------------------------------------------------

  # 1832.45 minus the 460 assigned leaves 1372.45 Unassigned. Hobby's budget still counts after it
  # is archived.
  Scenario: I can carry on after a failed save, and the notice stays for as long as saving fails
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And saving is not possible
    When I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn"
    Then MoneyBud should show that my changes are not saved
    When I archive the category "Hobby"
    Then I should be told that "Hobby" was archived
    And MoneyBud should still show that my changes are not saved
    When I step back one budget period
    And I step forward one budget period
    Then MoneyBud should still show that my changes are not saved
    When I record an income of 1832.45 euro labelled "Salaris"
    Then the income should be recorded
    And MoneyBud should still show that my changes are not saved
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And the categories offered for a new expense should not include "Hobby"
    And Unassigned in the current budget period should be 1372.45 euro

  Scenario: The removal question is asked while the notice stays
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 18 euro for "Groceries" labelled "Bakker" dated today
    And saving is not possible
    When I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn"
    And I ask to remove the expense labelled "Bakker"
    Then MoneyBud should be asking me to confirm
    And MoneyBud should still show that my changes are not saved
    When I confirm
    Then I should be told that the expense was removed
    And MoneyBud should still show that my changes are not saved

  # ----------------------------------------------------------------------------------
  # One save that succeeds catches up
  #
  # Every save keeps everything, so the first one that works keeps the changes whose own saves
  # failed: in both scenarios here, a new expense and a removal. Then the notice goes, and MoneyBud
  # says once that everything is saved again. The save that works can be the next change, or
  # MoneyBud trying again by itself.
  # ----------------------------------------------------------------------------------

  Scenario: The next change that saves keeps everything, the changes that failed included, and says so once
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 18 euro for "Groceries" labelled "Bakker" dated today
    And saving is not possible
    When I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn"
    And I remove the expense labelled "Bakker" and confirm
    Then MoneyBud should show that my changes are not saved
    When saving is possible again
    And I record an income of 1832.45 euro labelled "Salaris"
    Then the income should be recorded
    And MoneyBud should no longer show that my changes are not saved
    And I should be told that everything is saved again
    When I close MoneyBud and start it again
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | today | Salaris | 1832.45 |
    And the remaining "Groceries" budget in the current budget period should be 367.85 euro

  Scenario: Once saving is possible again, MoneyBud saves by itself a while later, keeps everything, and says so once
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 18 euro for "Groceries" labelled "Bakker" dated today
    And saving is not possible
    When I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn"
    And I remove the expense labelled "Bakker" and confirm
    Then MoneyBud should show that my changes are not saved
    When saving is possible again
    And a while passes with nothing done
    Then MoneyBud should no longer show that my changes are not saved
    And I should be told that everything is saved again
    When I close MoneyBud and start it again
    Then the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And the remaining "Groceries" budget in the current budget period should be 367.85 euro

  # ----------------------------------------------------------------------------------
  # Closing while changes are not saved
  #
  # Closing makes one last attempt to save, and asks nothing either way. If the attempt works,
  # nothing is lost. If it fails, the changes that were not saved are lost: the accepted cost.
  # Bakker was kept before saving stopped working, so it is there in both.
  # ----------------------------------------------------------------------------------

  Scenario: Closing while saving is still not possible loses the changes that were not saved, and asks nothing first
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 18 euro for "Groceries" labelled "Bakker" dated today
    And saving is not possible
    When I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn"
    Then MoneyBud should show that my changes are not saved
    When I close MoneyBud
    Then MoneyBud should close without asking me anything
    When saving is possible again
    And I start MoneyBud
    Then MoneyBud should not show that my changes are not saved
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label  | amount |
      | today | Groceries | Bakker | 18.00  |
    And the remaining "Groceries" budget in the current budget period should be 382 euro

  # Nothing has tried to save since saving became possible again, so the last attempt on closing is
  # the one that works.
  Scenario: Closing once saving is possible again keeps everything, even before MoneyBud has tried again
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 18 euro for "Groceries" labelled "Bakker" dated today
    And saving is not possible
    When I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn"
    Then MoneyBud should show that my changes are not saved
    When saving is possible again
    And I close MoneyBud before it has tried to save again
    Then MoneyBud should close without asking me anything
    When I start MoneyBud
    Then MoneyBud should not show that my changes are not saved
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
      | today | Groceries | Bakker       | 18.00  |
    And the remaining "Groceries" budget in the current budget period should be 349.85 euro

  # ----------------------------------------------------------------------------------
  # A save that is interrupted
  #
  # MoneyBud keeps one set of data and makes no copies (glossary: "Backing up is the user's
  # business"), so a save that damaged what was there before would lose the whole history. An
  # interrupted save never does. Here the interruption comes before the save of Albert Heijn has
  # finished, which is the worst case: Albert Heijn is lost, and nothing else is. The next start
  # opens as usual and says nothing, settled by the stakeholder on 2026-09-26.
  # ----------------------------------------------------------------------------------

  Scenario: A save that is interrupted leaves what was saved before it readable and whole
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 18 euro for "Groceries" labelled "Bakker" dated today
    When I record an expense of 32.15 euro for "Groceries" labelled "Albert Heijn"
    And MoneyBud is interrupted while saving it
    And I start MoneyBud
    Then I should not have been told anything
    And the Overview should show the current budget period
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label  | amount |
      | today | Groceries | Bakker | 18.00  |
    And the budget for "Groceries" in the current budget period should be 400 euro
    And the remaining "Groceries" budget in the current budget period should be 382 euro
