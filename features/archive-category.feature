# Archiving takes a category out of NEW ENTRY. Its history stays (arc42 §12, "A category is taken
# out of use, not deleted"). The state is called Archived, not removed or deleted, because
# nothing is gone: its expenses, its budgets and its place in every period's figures remain
# exactly as they were. Archiving never destroys a record and never blocks.
#
# Archiving never asks for confirmation, and I am told afterwards that the category was
# ARCHIVED — both settled by the stakeholder on 2026-09-25. Nothing is lost by archiving and
# adding the name undoes it, so there is nothing a confirmation would protect. Being told is
# what makes the act visible. So what I am told, across both category files, is: CREATED,
# ALREADY THERE, BROUGHT BACK or REFUSED when I add a name, and ARCHIVED when I archive one.
#
# What the state fixes, and what the scenarios below assert:
#   - An archived category is not offered for a new expense.
#   - It still owns its expenses and its budgets. Its figures do not move.
#   - It is shown in every budget period where it has history — a budget of more than zero or
#     an expense in that period — INCLUDING THE CURRENT ONE, and not in a period where it has
#     none (glossary: Where an archived category is still shown).
#   - It is not permanent. Adding its name brings it back, history and all, spelled as it was,
#     and I am told it was brought back rather than created. There is no separate un-archive
#     act. Recording an expense against it brings it back too; that is specified in
#     record-expense.feature, because recording is what does it.
#
# Archiving says nothing about money. A category archived with part of its budget unspent keeps
# that budget: nothing returns to Unassigned, and no figure changes.
#
# Being told is not being warned. When MoneyBud archives a category or brings one back it says
# so, after doing it — that is information about what happened. It never asks first, and it
# never warns.
#
# Givens are listed in the order things happened. Where a scenario gives a category a budget or
# spending and then archives it, the history came first.
#
# Reading the steps: "the categories offered for a new expense" and "should (not) include" /
# "should list X once, and no other spelling of it" mean exactly what add-category.feature says
# they mean. "X should (not) be shown in the current budget period" is about that period's
# figures — whether the category appears among them.
#
# Out of scope, and deliberately so:
#   - Archiving a name I do not have, and archiving a category that is already archived. Archived
#     is a yes-or-no state of a category that exists, so neither is something I can do, and no
#     behaviour is defined for either (glossary: What the state fixes).
#   - Renaming, assigning, accounts and the end-of-period sweep.
#   - Budget figures carried over into a new period. They are not built. It is settled that an
#     archived category's figure is not offered back when a period opens (glossary: An archived
#     category's figure is not offered back when a period opens); its scenarios belong with
#     carry-over.

@budget
Feature: Archive a category
  As someone budgeting my money
  I want to put away a category that does not apply to me, without losing what was recorded against it
  So that I am only offered the categories I use, while every period's figures still add up

  # ----------------------------------------------------------------------------------
  # Archiving takes a category out of new entry
  # ----------------------------------------------------------------------------------

  Scenario: An archived category is no longer offered for a new expense
    Given I have a category "Groceries"
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 25 euro on "Hobby" in the current budget period
    When I archive the category "Hobby"
    Then I should be told that "Hobby" was archived
    And I should not be warned or asked to confirm
    And the categories offered for a new expense should not include "Hobby"
    And the categories offered for a new expense should include "Groceries"

  # The interview's own case: a default that does not apply to you — Verzekeringen, for someone
  # with no insurance to pay. It has no history anywhere, so once archived it appears nowhere.
  # This and the last scenario in the file are the only two that depend on the defaults,
  # because the defaults are what they are about.
  Scenario: An unused default category can be archived, and then appears nowhere
    Given I have just started using MoneyBud for the first time
    And my budget periods are one month long
    When I archive the category "Verzekeringen"
    Then I should be told that "Verzekeringen" was archived
    And the categories offered for a new expense should be exactly these, in any order:
      | category     |
      | Boodschappen |
      | Huur         |
      | Hobby        |
      | Sparen       |
      | Abonnementen |
    And "Verzekeringen" should not be shown in the current budget period
    And "Verzekeringen" should not be shown in the previous budget period

  # ----------------------------------------------------------------------------------
  # Its history stays, and is shown wherever it is
  #
  # A period's figures have to add up on screen. If an archived category vanished from the
  # current period, the expenses recorded against it earlier in the period would still exist and
  # still count, but nothing would show them. So the rule is about history, not about whether a
  # period is past: a category archived midway through the current period is still shown in it,
  # and a past period in which it has no history does not show it.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A category archived midway through a period is still shown there, figures unchanged
    Given my budget periods are one month long
    And I have set no budget for "Hobby" in the previous budget period
    And I have not yet spent anything on "Hobby" in the previous budget period
    And I have a budget of <budget> euro for "Hobby" in the current budget period
    And I have already spent <spent> euro on "Hobby" in the current budget period
    When I archive the category "Hobby"
    Then "Hobby" should be shown in the current budget period
    And the budget for "Hobby" in the current budget period should still be <budget> euro
    And the remaining "Hobby" budget in the current budget period should still be <remaining> euro
    And "Hobby" should not be shown in the previous budget period

    Examples:
      | budget | spent | remaining |
      | 60.00  | 25.00 | 35.00     |
      | 60.00  | 60.00 | 0.00      |

  Scenario: An archived category that is over budget is still shown as over budget
    Given I have a budget of 60 euro for "Hobby" in the current budget period
    And I have already spent 72.50 euro on "Hobby" in the current budget period
    When I archive the category "Hobby"
    Then "Hobby" should be shown in the current budget period
    And the remaining "Hobby" budget in the current budget period should still be -12.50 euro
    And "Hobby" should be shown as over budget

  Scenario: A budget alone is history, so a category with nothing spent is still shown
    Given I have a budget of 60 euro for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    When I archive the category "Hobby"
    Then "Hobby" should be shown in the current budget period
    And the budget for "Hobby" in the current budget period should still be 60 euro

  Scenario: An expense alone is history, so a category with no budget set is still shown
    Given I have a category "Gifts"
    And I have never set a budget for "Gifts"
    And I have already spent 20 euro on "Gifts" in the current budget period
    When I archive the category "Gifts"
    Then "Gifts" should be shown in the current budget period
    And the remaining "Gifts" budget in the current budget period should still be -20 euro
    And "Gifts" should be shown as over budget

  Scenario: A category with history only in the previous period is shown there and not in the current one
    Given my budget periods are one month long
    And I have a budget of 60 euro for "Hobby" in the previous budget period
    And I have already spent 45 euro on "Hobby" in the previous budget period
    And I have set no budget for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    When I archive the category "Hobby"
    Then "Hobby" should be shown in the previous budget period
    And the remaining "Hobby" budget in the previous budget period should still be 15 euro
    And "Hobby" should not be shown in the current budget period

  # A category with no budget set behaves exactly as one budgeted at zero; there is no separate
  # "unbudgeted" state (glossary: Budget). So a budget of 0 with nothing spent against it is read
  # here as NO history, the same as never having set one. This was derived from that rule
  # rather than stated about archiving, and was then confirmed by the stakeholder on 2026-09-25.
  Scenario: A category budgeted at nothing with nothing spent has no history, and once archived is not shown
    Given I have a budget of 0 euro for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    When I archive the category "Hobby"
    Then "Hobby" should not be shown in the current budget period

  # ----------------------------------------------------------------------------------
  # Adding its name brings it back
  #
  # It is the interview's own case run backwards: you put away a category that did not apply to
  # you, and now it does. One action, nothing blocked, and no second category sharing a name
  # (glossary: Adding an archived category's name brings it back).
  #
  # Being told is the part that makes this safe. Old expenses reappearing under a category I
  # believe I have just created would be a genuine surprise, and the only thing that removes it
  # is being told the category was BROUGHT BACK, not created. It comes back spelled as it was:
  # adding "hobby" brings back "Hobby".
  # ----------------------------------------------------------------------------------

  Scenario Outline: Adding an archived category's name brings it back, history and all, spelled as it was
    Given my budget periods are one month long
    And I have a budget of 60 euro for "Hobby" in the previous budget period
    And I have already spent 45 euro on "Hobby" in the previous budget period
    And I have archived the category "Hobby"
    When I add a category <typed>
    Then I should be told that "Hobby" was brought back
    And I should not be warned or asked to confirm
    And the categories offered for a new expense should list "Hobby" once, and no other spelling of it
    And "Hobby" should be shown in the previous budget period
    And the remaining "Hobby" budget in the previous budget period should still be 15 euro

    Examples:
      | typed         |
      | "Hobby"       |
      | "hobby"       |
      | "  HOBBY  "   |

  # The rule does not depend on whether there is any history to bring back. An archived default
  # that never had any is still brought back, not created, and I am told so.
  Scenario: Adding the name of an archived default brings it back rather than creating it
    Given I have just started using MoneyBud for the first time
    And I have archived the category "Verzekeringen"
    When I add a category "Verzekeringen"
    Then I should be told that "Verzekeringen" was brought back
    And the categories offered for a new expense should list "Verzekeringen" once, and no other spelling of it
