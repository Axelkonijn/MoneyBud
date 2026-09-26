# Deleting a category: getting rid of one that has no history in any budget period (glossary:
# Delete; "Deleting a category that has no history anywhere", settled by the stakeholder on
# 2026-09-26). "Delete" is said of a category only. Removing is what happens to an entry
# (remove-an-entry.feature). Taking a category with history out of use is archiving it
# (archive-category.feature), which is unchanged.
#
# The rules, from arc42 §12:
#   - Only a category with NO HISTORY IN ANY PERIOD can be deleted. History is the glossary's
#     existing definition, applied to every period: a budget of more than zero, or an expense.
#   - Deleting is a SEPARATE ACT from archiving, with a delete button of its own that appears ONLY
#     on a category with no history anywhere. Two buttons, so that nothing is decided for me.
#   - A category WITH history cannot be deleted: it has no delete button, so deleting it is not
#     something I can do, and no behaviour is defined for trying. It can be archived.
#   - An ARCHIVED category with no history appears in no period, so it has no row and its delete
#     button cannot be reached. It stays archived, and adding its name brings it back.
#   - Deleting is NEVER CONFIRMED first, and I am TOLD AFTERWARDS that the category was deleted.
#     Nothing of value is lost, because the category has no history, and adding its name recreates
#     it. That is the same "confirm only where a record is lost" that makes removing an entry ask.
#   - A deleted category is GONE: not archived, and not brought back by anything. Adding its name
#     afterwards CREATES a new category.
#   - "No history" is decided by the figures as they are NOW:
#       - A category assigned to and then taken back to zero, with nothing spent, has no history.
#         Assigned-then-zero is not a separate state from never-assigned (glossary: Budget), so it
#         must not decide what I can do.
#       - A category whose only expense was removed, or changed to another category, has no
#         history. MoneyBud keeps no record of what an entry used to be, so it cannot know the
#         category once had an expense. The glossary records this as the documentation's
#         derivation.
#
# Why have deleting at all, when archiving a category with no history already hides it
# everywhere: an archived category keeps its name. Adding that name brings the old category back,
# and renaming another category to it is refused (rename-a-category.feature). A deleted category
# frees its name. That is the "So that" below, confirmed by the stakeholder as his reason on
# 2026-09-26.
#
# Reading the steps:
#   - "I should be able to delete the category "X"" means the delete act is offered for X: its row
#     carries the delete button. "I should not be able to delete the category "X"" means it is
#     offered nowhere, because its rows carry no delete button, or because it has no row at all.
#   - "I delete the category "X"" is the act. "I have deleted the category "X"" is the same act,
#     earlier, as a Given.
#   - "I should be told that "X" was deleted" is the message afterwards. What is fixed is that I am
#     told, not the wording.
#   - Every other step is reused unchanged from the file that introduced it, with the meaning that
#     file gives it.
#
# Every scenario starts from an empty ledger and names the categories it needs, except the last,
# which is about the default categories. The names are synthetic test data.

@budget
Feature: Delete a category
  As someone setting up my categories
  I want to get rid of a category I have never used, such as one I added by mistake
  So that it is gone for good and its name is free again, instead of being kept out of sight in the archive

  # ----------------------------------------------------------------------------------
  # Deleting a category with no history
  # ----------------------------------------------------------------------------------

  Scenario: A category with no history anywhere can be deleted, without being asked, and I am told
    Given my budget periods are one month long
    And I have a category "Groceries"
    And I have a category "Magazines"
    And I have never set a budget for "Magazines"
    When I delete the category "Magazines"
    Then I should be told that "Magazines" was deleted
    And I should not be warned or asked to confirm
    And the categories offered for a new expense should not include "Magazines"
    And the categories offered for a new expense should include "Groceries"
    And "Magazines" should not be shown in the current budget period
    And "Magazines" should not be shown in the next budget period

  # This is the visible difference from archiving: adding an archived category's name says it was
  # BROUGHT BACK (archive-category.feature).
  Scenario: Adding a deleted category's name creates a new category
    Given I have a category "Magazines"
    And I have deleted the category "Magazines"
    When I add a category "Magazines"
    Then I should be told that "Magazines" was created
    And the categories offered for a new expense should list "Magazines" once, and no other spelling of it

  # A second visible difference, which the glossary noticed while writing the ruling up and which
  # was not part of what was put to the stakeholder. A new category goes last in the order
  # categories were added, where an archived one brought back keeps its original place
  # (overview.feature). With every budget at zero, the rows are in the order added.
  Scenario: A category added again after it was deleted comes last in the order categories were added
    Given I have a category "Magazines"
    And I have a category "Groceries"
    And I have a category "Hobby"
    And I have deleted the category "Magazines"
    When I add a category "Magazines"
    Then the categories shown in the current budget period should be exactly these, in this order:
      | category  |
      | Groceries |
      | Hobby     |
      | Magazines |

  # ----------------------------------------------------------------------------------
  # Only a category with no history anywhere can be deleted
  #
  # One cent is the smallest history there is. It counts in any period: the previous one, the
  # current one, or one still to come. An expense cannot be dated in the future, so spending has
  # only two rows.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A category with a budget in any period cannot be deleted
    Given my budget periods are one month long
    And I have a budget of 0.01 euro for "Hobby" in the <period> budget period
    Then I should not be able to delete the category "Hobby"

    Examples:
      | period   |
      | previous |
      | current  |
      | next     |

  Scenario Outline: A category with an expense in any period cannot be deleted
    Given my budget periods are one month long
    And I have a category "Hobby"
    And I have never set a budget for "Hobby"
    And I have already spent 0.01 euro on "Hobby" in the <period> budget period
    Then I should not be able to delete the category "Hobby"

    Examples:
      | period   |
      | previous |
      | current  |

  # Unreachable, not refused: an archived category with no history is shown in no period, so
  # there is no row to carry a delete button.
  Scenario: An archived category with no history cannot be reached to be deleted
    Given my budget periods are one month long
    And I have a category "Magazines"
    And I have archived the category "Magazines"
    Then "Magazines" should not be shown in the current budget period
    And "Magazines" should not be shown in the next budget period
    And I should not be able to delete the category "Magazines"

  # ----------------------------------------------------------------------------------
  # "No history" means no history now
  #
  # Two When/Then pairs in each: first the act that takes the history away, then the delete it
  # makes possible.
  # ----------------------------------------------------------------------------------

  Scenario: A category assigned to and then taken back to zero, with nothing spent, can be deleted
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    When I assign -60 euro to "Hobby" in the current budget period
    Then the budget for "Hobby" in the current budget period should be 0.00 euro
    And I should be able to delete the category "Hobby"
    When I delete the category "Hobby"
    Then I should be told that "Hobby" was deleted
    And Unassigned in the current budget period should still be 2000 euro

  Scenario: A category whose only expense was removed can be deleted
    Given I have a category "Hobby"
    And I have recorded an expense of 25 euro for "Hobby" labelled "Verf" dated today
    When I remove the expense labelled "Verf" and confirm
    Then I should be able to delete the category "Hobby"
    When I delete the category "Hobby"
    Then I should be told that "Hobby" was deleted

  # The case deleting is for: a category added with a typo, an expense recorded against it by
  # mistake, and the expense moved to the category it belonged in.
  Scenario: A category whose only expense was moved to another category can be deleted
    Given I have a category "Hobby"
    And I have a category "Hobyb"
    And I have recorded an expense of 25 euro for "Hobyb" labelled "Verf" dated today
    When I change the category of the expense labelled "Verf" to "Hobby"
    Then I should be told that the expense was changed
    And I should be able to delete the category "Hobyb"
    And I should not be able to delete the category "Hobby"
    When I delete the category "Hobyb"
    Then I should be told that "Hobyb" was deleted
    And the categories offered for a new expense should not include "Hobyb"

  # ----------------------------------------------------------------------------------
  # The default categories
  #
  # The interview's own case: a default that does not apply to me, such as Verzekeringen for
  # someone with no insurance to pay. It has no history, so it can be deleted. This is the one
  # scenario in the file that depends on the defaults, because the defaults are what it is about.
  # ----------------------------------------------------------------------------------

  Scenario: An unused default category can be deleted
    Given I have just started using MoneyBud for the first time
    When I delete the category "Verzekeringen"
    Then I should be told that "Verzekeringen" was deleted
    And the categories offered for a new expense should be exactly these, in any order:
      | category     |
      | Boodschappen |
      | Huur         |
      | Hobby        |
      | Sparen       |
      | Abonnementen |
