# When I record an expense or assign, I name the category by typing it. MoneyBud suggests the
# categories offered for new entry, and accepts any text (glossary: "Category entry is free text
# with suggestions", settled 2026-09-25 over a pick-list alone).
#
# "Not offered" means NOT SUGGESTED. An archived category is not suggested, for an expense or for
# assigning. Typing its name anyway is still accepted, and what happens then is already
# specified, so it is not restated here:
#   - recording an expense against it records the expense and brings the category back
#     (record-expense.feature);
#   - assigning a positive amount to it brings it back, and a negative or zero one leaves it
#     archived (assign-to-category.feature);
#   - a name that is none of my categories is refused (both files).
#
# Why free text. Two of the three routes back consist of naming an archived category ANYWAY. A
# pick-list of offered categories would leave nothing to name it with, and nothing to type a
# name that is none of mine either. So a pick-list would make those routes, and that refusal,
# unreachable, and the UI would cover less than the domain does.
#
# So the last two scenarios assert only that a typed name is judged AS TYPED. It reaches the rules
# above rather than being blocked, or swapped for the suggestion it most resembles.
#
# Reading the steps:
#   - "the categories offered for a new expense" is add-category.feature's grammar, unchanged. On
#     screen it is what is suggested when I record an expense.
#   - "the categories offered for assigning" is the same thing for assigning. "should include",
#     "should not include" and "should list X once, and no other spelling of it" mean exactly what
#     add-category.feature says they mean.
#   - "should be exactly these, in this order" lists every suggestion, in the order suggested, and
#     no other. Suggestions are in alphabetical order (settled by the stakeholder on 2026-09-25),
#     and case does not count: "hobby" sorts as if it were "Hobby". A name is still shown as it is
#     stored. This is NOT the order of the Overview's rows, which is by Budget.
#   - Not asserted here: that the suggestions narrow as I type, to the names containing what I
#     typed. Settled after this file was approved (glossary: "Category entry is free text with
#     suggestions") and held by the developer unit tests.
#
# Every scenario starts from an empty ledger and names the categories it needs. The names are
# synthetic test data.

@budget
Feature: Suggest categories
  As someone recording my spending and planning my budget
  I want MoneyBud to suggest the categories I use while still letting me type any name
  So that I pick the right category quickly, and can still name one it does not suggest, such as one I archived and want back

  # Added in an order that is not alphabetical, so the suggestions cannot pass by keeping it.
  # "hobby" is stored in lower case. A case-sensitive sort would put it after "Vaste lasten".
  Scenario: The categories in use are suggested in alphabetical order, when recording an expense and when assigning
    Given I have a category "Vaste lasten"
    And I have a category "Groceries"
    And I have a category "hobby"
    Then the categories offered for a new expense should be exactly these, in this order:
      | category     |
      | Groceries    |
      | hobby        |
      | Vaste lasten |
    And the categories offered for assigning should be exactly these, in this order:
      | category     |
      | Groceries    |
      | hobby        |
      | Vaste lasten |

  Scenario: An archived category is suggested neither when recording an expense nor when assigning
    Given I have a category "Groceries"
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have archived the category "Hobby"
    Then the categories offered for a new expense should not include "Hobby"
    And the categories offered for assigning should not include "Hobby"
    And the categories offered for a new expense should include "Groceries"
    And the categories offered for assigning should include "Groceries"

  Scenario: A category brought back is suggested again, for both, spelled as it was
    Given I have a category "Hobby"
    And I have archived the category "Hobby"
    When I add a category "hobby"
    Then the categories offered for a new expense should list "Hobby" once, and no other spelling of it
    And the categories offered for assigning should list "Hobby" once, and no other spelling of it

  # "Groc" is the start of a suggested name, and it is not that name. It is judged as typed, so it
  # is a name I do not have.
  Scenario Outline: A name that is not suggested is taken as typed when recording an expense
    Given I have a category "Groceries"
    And I have a category "Hobby"
    And I have archived the category "Hobby"
    And I have no category called "Holiday"
    And I have no category called "Groc"
    When I try to record an expense of 12.50 euro for <typed>
    Then I should be told that <outcome>

    Examples:
      | typed     | outcome                               |
      | "Hobby"   | "Hobby" was brought back              |
      | "Holiday" | "Holiday" is not one of my categories |
      | "Groc"    | "Groc" is not one of my categories    |

  Scenario Outline: A name that is not suggested is taken as typed when assigning
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a category "Groceries"
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have archived the category "Hobby"
    And I have no category called "Holiday"
    And I have no category called "Groc"
    When I try to assign 40 euro to <typed> in the current budget period
    Then I should be told that <outcome>

    Examples:
      | typed     | outcome                               |
      | "Hobby"   | "Hobby" was brought back              |
      | "Holiday" | "Holiday" is not one of my categories |
      | "Groc"    | "Groc" is not one of my categories    |
