# Pointing at a slice of the Overview's ring shows that slice's figures (glossary: "Hovering a
# slice shows its figures"). Settled by the stakeholder at the first demo, 2026-09-26, over
# clicking to select the category and over clicking to zoom. It answers his complaint that the
# ring "doet niets", does nothing. The Unassigned half was ruled in a follow-up question the same
# day.
#
#   - Pointing at a category slice shows its category, Budget, Spent and Remaining (on screen:
#     Categorie, Budget, Uitgegeven, Resterend).
#   - Pointing at the Unassigned slice shows its name and its figure, as in "Niet toegewezen:
#     € 350,00".
#   - Every slice answers.
#
# Ruled by the stakeholder later the same day, in answer to questions raised while this file was
# written:
#   - An overspent slice shows what its row shows: Remaining as the negative figure itself, with
#     the over-budget marker.
#   - An archived category's slice shows that the category is archived (on screen: Gearchiveerd),
#     as its row does.
#   - The figures are exact. A slice's fill has no minimum (overview.feature), so a tiny amount
#     spent can look like nothing spent. Pointing at the slice shows the exact figure.
#
# The figures are the slice's own, exactly. Since the same demo a small slice is drawn wider than
# its share of the ring (overview.feature, "Every slice is drawn at least a minimum width"), so
# its width no longer says how much it holds. What it holds is shown here, and in its row.
#
# Which slices exist, and so what there is to point at, is overview.feature's, and is not restated
# here. A category with a Budget of zero has no slice. An Over-assigned period has no Unassigned
# slice. An empty ring has no slices at all. In each case there is nothing to point at, and the
# figures are still in the rows.
#
# What is specified is what the screen shows for a slice when it is pointed at. How it is pointed
# at is not: nothing below depends on a mouse, on where the figures appear, on how long they stay,
# or on what they look like. The Dutch words are already in the glossary's "Dutch display terms".
# How an amount is written on screen ("€ 350,00") is held by the developer unit tests, as it is
# everywhere else, so the steps below compare amounts, not text.
#
# Reading the steps:
#   - "I point at the "X" slice in the ring for the ... budget period" and "I point at the
#     Unassigned slice in the ring for the ... budget period" point at one slice of the ring that
#     period's Overview shows.
#   - "the slice pointed at should show these figures" checks every column its table has: the
#     category's name as stored, its Budget, what was spent, its Remaining, and whether the
#     over-budget marker is shown. The columns mean what they mean in overview.feature's table of
#     rows.
#   - "the slice pointed at should show Unassigned of X euro" checks that the slice shows the name
#     Unassigned and that figure.
#   - "the slice pointed at should show that its category is archived" checks that what is shown
#     says the category is archived, as the row's Gearchiveerd caption does. How it says so is not
#     fixed. "should not show that its category is archived" is its opposite, so that a slice
#     which always says archived cannot pass.
#
# Every scenario starts from an empty ledger and names the categories it needs. The names are
# synthetic test data.

@budget
Feature: Point at a slice of the ring to see its figures
  As someone budgeting my money
  I want to point at a slice of the ring and read what it is and exactly what it holds
  So that I can tell what each part of the ring means without finding its row, even for a slice too small to judge by its width

  # ----------------------------------------------------------------------------------
  # A category slice
  # ----------------------------------------------------------------------------------

  # The one-euro budget is drawn wider than its share of 2000 euro. What it shows is still exactly
  # one euro, not what its width would suggest.
  Scenario Outline: Pointing at a category slice shows its category, budget, what was spent and what remains
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of <budget> euro for "Groceries" in the current budget period
    And I have already spent <spent> euro on "Groceries" in the current budget period
    When I point at the "Groceries" slice in the ring for the current budget period
    Then the slice pointed at should show these figures:
      | category  | budget   | spent   | remaining   | over budget |
      | Groceries | <budget> | <spent> | <remaining> | no          |

    Examples: within budget
      | budget | spent  | remaining |
      | 400.00 | 150.00 | 250.00    |
      | 75.50  | 75.49  | 0.01      |

    Examples: exactly on budget, so nothing remains and it is not over budget
      | budget | spent  | remaining |
      | 400.00 | 400.00 | 0.00      |

    Examples: a slice drawn wider than its share still shows its exact figures
      | budget | spent | remaining |
      | 1.00   | 0.25  | 0.75      |

  # Two slices, and the one pointed at is not the first, so showing the first slice's figures,
  # or the largest one's, cannot pass.
  Scenario: Pointing at one slice shows that slice's figures, not another's
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    When I point at the "Hobby" slice in the ring for the current budget period
    Then the slice pointed at should show these figures:
      | category | budget | spent | remaining | over budget |
      | Hobby    | 60.00  | 0.00  | 60.00     | no          |
    And the slice pointed at should not show that its category is archived

  # Ruled by the stakeholder on 2026-09-26: an overspent slice shows what its row shows, which is
  # Remaining as the negative figure itself, never a positive "over by" amount, with the
  # over-budget marker.
  Scenario Outline: Pointing at an overspent slice shows its remaining as the negative figure, marked
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of <budget> euro for "Groceries" in the current budget period
    And I have already spent <spent> euro on "Groceries" in the current budget period
    When I point at the "Groceries" slice in the ring for the current budget period
    Then the slice pointed at should show these figures:
      | category  | budget   | spent   | remaining   | over budget |
      | Groceries | <budget> | <spent> | <remaining> | yes         |

    Examples:
      | budget | spent  | remaining |
      | 400.00 | 420.00 | -20.00    |
      | 30.00  | 30.01  | -0.01     |

  # An archived category with a Budget above zero still has a slice (overview.feature), and its
  # row carries the Gearchiveerd caption (glossary: "Where an archived category is still shown").
  # Ruled by the stakeholder on 2026-09-26: pointing at its slice says so too.
  Scenario: Pointing at an archived category's slice shows its figures and that it is archived
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 25 euro on "Hobby" in the current budget period
    And I have archived the category "Hobby"
    When I point at the "Hobby" slice in the ring for the current budget period
    Then the slice pointed at should show these figures:
      | category | budget | spent | remaining | over budget |
      | Hobby    | 60.00  | 25.00 | 35.00     | no          |
    And the slice pointed at should show that its category is archived

  # ----------------------------------------------------------------------------------
  # The Unassigned slice
  #
  # It exists only while Unassigned is above zero (overview.feature), so what it shows is never
  # negative and never marked.
  # ----------------------------------------------------------------------------------

  Scenario Outline: Pointing at the Unassigned slice shows its name and its figure
    Given I have already recorded <income> euro of income in the current budget period
    And I have a budget of <budget> euro for "Groceries" in the current budget period
    When I point at the Unassigned slice in the ring for the current budget period
    Then the slice pointed at should show Unassigned of <unassigned> euro

    Examples:
      | income  | budget  | unassigned |
      | 2000.00 | 400.00  | 1600.00    |
      | 1832.45 | 400.00  | 1432.45    |
      | 2000.00 | 1999.99 | 0.01       |
