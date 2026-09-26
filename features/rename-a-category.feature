# Renaming a category: giving it a new name (glossary: Rename; "Renaming a category", settled by
# the stakeholder on 2026-09-26). It supersedes "Renaming a category is not in this increment",
# which deferred it without rejecting it.
#
# The rules, from arc42 §12:
#   - The new name follows the rules for ADDING one: trimmed at the ends, stored otherwise as
#     typed, and refused if it trims to nothing (add-category.feature).
#   - A name ANOTHER category already has is refused, and I am told the name is taken. "Another
#     category" includes archived ones. Names are compared as always: ends trimmed, any run of
#     inner spaces counted as one, case ignored. Refusing was chosen over merging the two
#     categories, which would silently rewrite both histories.
#   - Changing only the SPELLING of a category's own name is allowed: "boodschappen" can become
#     "Boodschappen". Under the name rule the new spelling is the same name, so no other category
#     can hold it, and the clash rule never reaches it. This is the front door that adding keeps
#     shut: adding a name I already have still hands back the existing spelling
#     (add-category.feature).
#   - PAST PERIODS SHOW THE NEW NAME, everywhere. It is one category with a new label, not a new
#     category, and nothing has to remember old names.
#   - An ARCHIVED category can be renamed, and it STAYS ARCHIVED. Fixing a typo in a name is not
#     using the category again, so it is not a route back. On screen a category is renamed from its
#     row, so an archived one can be renamed only in a period where it has history and is shown.
#   - It stays the SAME category: its history, its place in "order added" and whether it is
#     archived are untouched. The glossary records this as the documentation's reading.
#   - Renaming is never confirmed first. Only removing an entry asks (remove-an-entry.feature).
#   - A rename that goes through is ANNOUNCED: I am told the category was renamed, from what to
#     what. Settled by the stakeholder on 2026-09-26.
#   - Renaming a category to its own name, spelled exactly as it already is, is treated like saving
#     an entry with nothing changed (change-an-entry.feature): never refused, and quiet. Nothing
#     changes and nothing is announced. Settled by the stakeholder on 2026-09-26.
#
# Reading the steps:
#   - "I rename the category "X" to "Y"" names the category by its current name, and "Y" is the new
#     name exactly as typed. The quotation marks are part of the step, so a name really does start
#     or end with spaces. "I try to rename" is the same act, where the scenario expects a refusal.
#   - "the rename should go through" means the category now has the new name. "the rename should be
#     refused" means it does not.
#   - "I should be told that "X" was renamed to "Y"" is the announcement. "X" is the name as it
#     was and "Y" the name as now stored, trimmed. What is fixed is that I am told, from what to
#     what, not the wording. "I should not have been told anything" is
#     step-between-periods.feature's: no message of any kind appeared.
#   - "I should be told that the new name is already taken" is the refusal for a clash. What is
#     fixed is the fact, not the wording, and not which spelling of the name the message shows.
#   - "I should be told that a category needs a name" is add-category.feature's refusal, reached by
#     renaming instead of adding.
#   - "my categories should be unchanged", and the steps about the categories offered, are
#     add-category.feature's, with the meanings its header gives them.
#   - Every other step is reused unchanged from the file that introduced it.
#
# Every scenario starts from an empty ledger and names the categories it needs. The names are
# synthetic test data.

@budget
Feature: Rename a category
  As someone budgeting my money
  I want to give a category a better name, or fix how its name is spelled
  So that my categories read the way I think of them, in every period, without losing anything recorded against them

  # ----------------------------------------------------------------------------------
  # Renaming
  # ----------------------------------------------------------------------------------

  Scenario: Renaming a category gives it the new name, and its budget and spending stay with it
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    When I rename the category "Groceries" to "Food"
    Then the rename should go through
    And I should be told that "Groceries" was renamed to "Food"
    And I should not be warned or asked to confirm
    And the categories offered for a new expense should list "Food" once, and no other spelling of it
    And the categories offered for a new expense should not include "Groceries"
    And the budget for "Food" in the current budget period should still be 400 euro
    And the remaining "Food" budget in the current budget period should still be 250 euro

  Scenario: A new name loses the whitespace around it and keeps everything inside it
    Given I have a category "Groceries"
    When I rename the category "Groceries" to "  Vaste lasten  "
    Then the rename should go through
    And I should be told that "Groceries" was renamed to "Vaste lasten"
    And the categories offered for a new expense should list "Vaste lasten" once, and no other spelling of it

  Scenario Outline: A new name that trims to nothing is refused
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    When I try to rename the category "Groceries" to <name>
    Then the rename should be refused
    And I should be told that a category needs a name
    And my categories should be unchanged
    And the categories offered for a new expense should list "Groceries" once, and no other spelling of it
    And the budget for "Groceries" in the current budget period should still be 400 euro

    Examples:
      | name  |
      | ""    |
      | " "   |
      | "   " |

  # ----------------------------------------------------------------------------------
  # A name another category has is taken
  #
  # Compared the way MoneyBud always compares names, so no capitalisation or spacing gets round
  # it. Both categories keep their figures: nothing is merged.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A name another category already has is refused, however I capitalise or space it
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a budget of 120 euro for "Eating out" in the current budget period
    When I try to rename the category "Groceries" to <typed>
    Then the rename should be refused
    And I should be told that the new name is already taken
    And my categories should be unchanged
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And the budget for "Eating out" in the current budget period should still be 120 euro

    Examples:
      | typed            |
      | "Eating out"     |
      | "eating out"     |
      | "  EATING OUT  " |
      | "Eating   out"   |

  # An archived category still has its name. Adding that name brings it back, so it cannot be
  # given to another category either.
  Scenario: A name an archived category has is taken too
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a category "Hobby"
    And I have archived the category "Hobby"
    When I try to rename the category "Groceries" to "hobby"
    Then the rename should be refused
    And I should be told that the new name is already taken
    And my categories should be unchanged
    And the categories offered for a new expense should include "Groceries"
    And the categories offered for a new expense should not include "Hobby"

  # ----------------------------------------------------------------------------------
  # Respelling a category's own name
  #
  # The new spelling is the same name under the name rule, so there is no clash to refuse. What
  # is offered afterwards is spelled exactly as typed, letter for letter and space for space.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A category's own name can be respelled
    Given I have a category <name>
    When I rename the category <name> to <new spelling>
    Then the rename should go through
    And I should be told that <name> was renamed to <new spelling>
    And the categories offered for a new expense should list <new spelling> once, and no other spelling of it

    Examples:
      | name            | new spelling    |
      | "boodschappen"  | "Boodschappen"  |
      | "Vaste  lasten" | "Vaste lasten"  |
      | "Hobby"         | "HOBBY"         |

  # The same name, spelled exactly as it already is: nothing to change, so nothing is refused and
  # nothing is said, as when an entry is saved unchanged.
  Scenario: Renaming a category to exactly its own name goes through quietly
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    When I rename the category "Groceries" to "Groceries"
    Then the rename should go through
    And I should not have been told anything
    And my categories should be unchanged
    And the categories offered for a new expense should list "Groceries" once, and no other spelling of it
    And the budget for "Groceries" in the current budget period should still be 400 euro

  # ----------------------------------------------------------------------------------
  # One category with a new name
  # ----------------------------------------------------------------------------------

  # The expense's row, the category's row and its figures all carry the new name in the past
  # period too. Nothing keeps the old name anywhere.
  Scenario: Past periods show the new name
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have recorded an expense of 380 euro for "Groceries" labelled "Markt" dated on the last day of the previous budget period
    When I rename the category "Groceries" to "Food"
    Then the rename should go through
    And I should be told that "Groceries" was renamed to "Food"
    And "Food" should be shown in the previous budget period
    And "Groceries" should not be shown in the previous budget period
    And the remaining "Food" budget in the previous budget period should be 20 euro
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date                                       | category | label | amount |
      | the last day of the previous budget period | Food     | Markt | 380.00 |

  # Renaming is not new entry, so it does not bring the category back. It is still shown where it
  # has history, under its new name.
  Scenario: An archived category can be renamed, and it stays archived
    Given I have a budget of 60 euro for "Hobby" in the current budget period
    And I have recorded an expense of 25 euro for "Hobby" labelled "Verf" dated today
    And I have archived the category "Hobby"
    When I rename the category "Hobby" to "Crafts"
    Then the rename should go through
    And I should be told that "Hobby" was renamed to "Crafts"
    And "Crafts" should be shown in the current budget period
    And the remaining "Crafts" budget in the current budget period should be 35 euro
    And the categories offered for a new expense should not include "Crafts"
    And the categories offered for a new expense should not include "Hobby"

  # With every budget at zero, the rows are in the order the categories were added
  # (overview.feature). Renaming Groceries to a name that sorts last moves it neither to the end
  # nor into alphabetical order: it keeps the place it was added in. The documentation's reading
  # (glossary: "Renaming a category", "It stays the same category").
  Scenario: A renamed category keeps its place in the order categories were added
    Given I have a category "Groceries"
    And I have a category "Hobby"
    And I have a category "Gifts"
    When I rename the category "Groceries" to "Wine"
    Then the rename should go through
    And I should be told that "Groceries" was renamed to "Wine"
    And the categories shown in the current budget period should be exactly these, in this order:
      | category |
      | Wine     |
      | Hobby    |
      | Gifts    |

  # Nothing remembers old names, so once Groceries is Food, no category has the name
  # "Groceries". Adding it creates a new, empty category, and Food keeps its figures. Derived from
  # "nothing has to remember old names" and the rule for adding; not put to the stakeholder
  # separately. Two When/Then pairs, so that each act's announcement is checked after that act.
  Scenario: Once a category is renamed, its old name is free
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    When I rename the category "Groceries" to "Food"
    Then I should be told that "Groceries" was renamed to "Food"
    When I add a category "Groceries"
    Then I should be told that "Groceries" was created
    And the categories offered for a new expense should list "Groceries" once, and no other spelling of it
    And the categories offered for a new expense should list "Food" once, and no other spelling of it
    And the budget for "Groceries" in the current budget period should be 0.00 euro
    And the budget for "Food" in the current budget period should still be 400 euro
