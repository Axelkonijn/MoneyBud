# Removing an entry: taking an expense or an income away entirely (glossary: Remove; "An entry can
# be changed or removed", settled by the stakeholder on 2026-09-26). "Remove" is said of an entry
# only. Taking a category out of use is ARCHIVING it (archive-category.feature), and getting rid of
# a category that has no history is DELETING it (delete-a-category.feature).
#
# The rules, from arc42 §12:
#   - Removing ASKS FOR CONFIRMATION FIRST, and nothing is removed until I confirm. It is the only
#     act in MoneyBud that asks anything before it acts. The principle is "confirm only where a
#     record is lost": a removed entry is gone, and MoneyBud keeps no copy to undo it from. So
#     archiving, deleting a category, assigning and changing an entry do not ask, and this does.
#   - Being asked is not being warned. The question is about the act, not about the state of my
#     money. What the question says ("Weet je het zeker?") is copy, not a term, and not asserted.
#   - Afterwards, I am told the entry was removed. The glossary records this as a derivation from
#     "every act tells its outcome", stated so that it can be contradicted.
#   - Removing is allowed in ANY budget period, past ones included, and it changes that period's
#     figures. That is its purpose (glossary: "An entry in a past period can be corrected").
#   - Removing an income may leave its period Over-assigned. That is allowed and shown with the
#     existing marker, and nothing more is said about it: the confirmation and the message
#     afterwards do not mention it. In a PAST period it then stays over-assigned for good, because
#     nothing can be assigned in a past period, a negative amount included. The stakeholder
#     accepted that on 2026-09-26: it is the true figure.
#   - Removing an archived category's last expense in a period drops the category from that period,
#     unless it still has a budget of more than zero there. The same display rule holds for a
#     category in use in a past period, which shows only what has history there.
#   - Nothing is deduplicated, so two identical entries are two entries. Removing one leaves the
#     other. A duplicate typed twice is the most likely reason to remove anything.
#
# Which entry a step acts on is named the way change-an-entry.feature names it: by what its row in
# the period's list shows. "the expense labelled "X"" is the one expense with that label in the
# period on screen. "one of the two expenses labelled "Coffee"" is used only where the two are
# identical in every respect, so there is nothing to tell them apart by and it does not matter
# which one goes. That is the point of those scenarios. MoneyBud opens on the current period; a
# scenario that removes an entry from another period shows that period first.
#
# Reading the steps:
#   - "I remove ... and confirm" is the whole act: I ask to remove the entry, I am asked to
#     confirm, and I do. "I remove ... but decline to confirm" is the same, answered no.
#   - "I should have been asked to confirm first" means the question came before anything was
#     removed.
#   - "I should be told that the expense (income) was removed" is the message afterwards. What is
#     fixed is that I am told, not the wording.
#   - "I should not be warned about the budget period becoming over-assigned" means neither the
#     question nor the message afterwards mentions it. The figure and its marker show it. That
#     reading was confirmed by the stakeholder on 2026-09-26.
#   - After declining, NOTHING IS SAID (settled by the stakeholder on 2026-09-26). "I should not
#     have been told anything" is step-between-periods.feature's: no message of any kind appeared.
#     The confirmation question is asked, not told, so it does not count as a message.
#   - "I have recorded an expense (income) of ... dated ..." is a Given in recording's grammar, as
#     explained in change-an-entry.feature. Givens are listed in the order things happened.
#   - The list steps are list-transactions-in-a-period.feature's: the whole list, in order.
#   - Every other step is reused unchanged from the file that introduced it.
#
# Not specified here: what the form shows after Verwijderen, which is form behaviour left for the
# plan to propose.
#
# Every scenario starts from an empty ledger and names the categories it needs. The names, labels
# and amounts are synthetic test data.

@budget
Feature: Remove an entry
  As someone entering my money by hand
  I want to take away an expense or an income that should never have been recorded, such as one I entered twice
  So that each period's figures are made only of what really happened

  # ----------------------------------------------------------------------------------
  # Removing an expense
  # ----------------------------------------------------------------------------------

  Scenario: Removing an expense asks first, then takes it out of the list and out of the remaining budget
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    And I have recorded an expense of 18 euro for "Groceries" labelled "Bakker" dated today
    When I remove the expense labelled "Albert Heijn" and confirm
    Then I should have been asked to confirm first
    And I should be told that the expense was removed
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label  | amount |
      | today | Groceries | Bakker | 18.00  |
    And the remaining "Groceries" budget in the current budget period should be 382 euro
    And the budget for "Groceries" in the current budget period should still be 400 euro

  Scenario: Declining to confirm removes no expense
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    And I have recorded an expense of 18 euro for "Groceries" labelled "Bakker" dated today
    When I remove the expense labelled "Albert Heijn" but decline to confirm
    Then I should have been asked to confirm first
    And I should not have been told anything
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Bakker       | 18.00  |
      | today | Groceries | Albert Heijn | 32.15  |
    And the remaining "Groceries" budget in the current budget period should still be 349.85 euro

  # The duplicate: the same coffee entered twice. One of the two goes, and the other stays.
  Scenario: Removing one of two identical expenses leaves the other
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 3.50 euro for "Groceries" labelled "Coffee" dated today
    And I have recorded an expense of 3.50 euro for "Groceries" labelled "Coffee" dated today
    When I remove one of the two expenses labelled "Coffee" and confirm
    Then I should be told that the expense was removed
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label  | amount |
      | today | Groceries | Coffee | 3.50   |
    And the remaining "Groceries" budget in the current budget period should be 396.50 euro

  # Before the removal Groceries is 12.50 over budget in the previous period. A period never
  # closes, and a wrong fact in it can be taken out.
  Scenario: Removing an expense in a past period changes that period's figures, over budget included
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have recorded an expense of 380 euro for "Groceries" labelled "Markt" dated on the last day of the previous budget period
    And I have recorded an expense of 32.50 euro for "Groceries" labelled "Bakker" dated on the last day of the previous budget period
    And the Overview shows the previous budget period
    When I remove the expense labelled "Bakker" and confirm
    Then I should have been asked to confirm first
    And I should be told that the expense was removed
    And the expenses listed in the previous budget period should be exactly these, in this order:
      | date                                       | category  | label | amount |
      | the last day of the previous budget period | Groceries | Markt | 380.00 |
    And the remaining "Groceries" budget in the previous budget period should be 20 euro
    And "Groceries" should not be shown as over budget in the previous budget period
    And the budget for "Groceries" in the previous budget period should still be 400 euro

  # ----------------------------------------------------------------------------------
  # Removing can change where a category is shown
  #
  # "History" in a period is a budget of more than zero, or an expense (glossary: "Where an
  # archived category is still shown"). An archived category is shown only where it has history,
  # and so is any category in a past period. Removing its last expense can take that away.
  # Removing never brings an archived category back: it is not new entry.
  # ----------------------------------------------------------------------------------

  Scenario: Removing an archived category's last expense in a period drops it from that period
    Given I have a category "Hobby"
    And I have recorded an expense of 25 euro for "Hobby" labelled "Verf" dated today
    And I have archived the category "Hobby"
    When I remove the expense labelled "Verf" and confirm
    Then I should be told that the expense was removed
    And no expenses should be listed in the current budget period
    And "Hobby" should not be shown in the current budget period
    And the categories offered for a new expense should not include "Hobby"

  Scenario: An archived category with a budget in the period is still shown there after its last expense is removed
    Given I have a budget of 60 euro for "Hobby" in the current budget period
    And I have recorded an expense of 25 euro for "Hobby" labelled "Verf" dated today
    And I have archived the category "Hobby"
    When I remove the expense labelled "Verf" and confirm
    Then "Hobby" should be shown in the current budget period
    And the remaining "Hobby" budget in the current budget period should be 60 euro
    And the categories offered for a new expense should not include "Hobby"

  # Groceries is in use, so the current period, which I plan, still shows it.
  Scenario: Removing a category's only expense in a past period drops the category from that period
    Given my budget periods are one month long
    And I have a category "Groceries"
    And I have set no budget for "Groceries" in the previous budget period
    And I have recorded an expense of 12.50 euro for "Groceries" labelled "Kiosk" dated on the last day of the previous budget period
    And the Overview shows the previous budget period
    When I remove the expense labelled "Kiosk" and confirm
    Then no expenses should be listed in the previous budget period
    And "Groceries" should not be shown in the previous budget period
    And "Groceries" should be shown in the current budget period

  # ----------------------------------------------------------------------------------
  # Removing an income
  # ----------------------------------------------------------------------------------

  Scenario: Removing an income asks first, then takes it out of the list and out of Unassigned
    Given I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    And I have recorded an income of 25 euro labelled "Statiegeld" dated today
    When I remove the income labelled "Statiegeld" and confirm
    Then I should have been asked to confirm first
    And I should be told that the income was removed
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | today | Salaris | 1832.45 |
    And Unassigned in the current budget period should be 1832.45 euro

  Scenario: Declining to confirm removes no income
    Given I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    And I have recorded an income of 25 euro labelled "Statiegeld" dated today
    When I remove the income labelled "Statiegeld" but decline to confirm
    Then I should have been asked to confirm first
    And I should not have been told anything
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label      | amount  |
      | today | Statiegeld | 25.00   |
      | today | Salaris    | 1832.45 |
    And Unassigned in the current budget period should still be 1857.45 euro

  Scenario: Removing one of two identical incomes leaves the other
    Given I have recorded an income of 25 euro labelled "Statiegeld" dated today
    And I have recorded an income of 25 euro labelled "Statiegeld" dated today
    When I remove one of the two incomes labelled "Statiegeld" and confirm
    Then I should be told that the income was removed
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label      | amount |
      | today | Statiegeld | 25.00  |
    And Unassigned in the current budget period should be 25 euro

  # ----------------------------------------------------------------------------------
  # Removing an income can leave its period over-assigned
  #
  # Allowed, and shown with the marker Over budget uses. Refusing would block correcting a fact
  # because of the figure the correction leaves, and saying so in the message was turned down
  # because the marker already shows it where the figure is. Exactly zero is not over-assigned;
  # one cent below is.
  # ----------------------------------------------------------------------------------

  Scenario Outline: Removing an income can leave the period over-assigned, which is shown and not warned about
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have recorded an income of 250 euro labelled "Bijbaan" dated today
    And I have a budget of <budget> euro for "Groceries" in the current budget period
    When I remove the income labelled "Bijbaan" and confirm
    Then I should be told that the income was removed
    And I should not be warned about the budget period becoming over-assigned
    And Unassigned in the current budget period should be <unassigned> euro
    And the current budget period should be shown as over-assigned
    And Unassigned in the current budget period should be marked the same way as a category that is over budget
    And the budget for "Groceries" in the current budget period should still be <budget> euro

    Examples:
      | budget  | unassigned |
      | 2100.00 | -100.00    |
      | 2000.01 | -0.01      |

  Scenario: Removing an income that leaves exactly nothing unassigned does not make the period over-assigned
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have recorded an income of 250 euro labelled "Bijbaan" dated today
    And I have a budget of 2000 euro for "Groceries" in the current budget period
    When I remove the income labelled "Bijbaan" and confirm
    Then Unassigned in the current budget period should be 0.00 euro
    And the current budget period should not be shown as over-assigned

  # The accepted cost, shown in full. Two When/Then pairs: the removal leaves the previous period
  # over-assigned, and then the one act that could balance it, taking money back out of the
  # budget, is refused, because nothing can be assigned in a past period.
  Scenario: Removing an income in a past period can leave that period over-assigned for good
    Given my budget periods are one month long
    And I have recorded an income of 1800 euro labelled "Salaris" dated on the last day of the previous budget period
    And I have recorded an income of 200 euro labelled "Teruggave" dated on the last day of the previous budget period
    And I have a budget of 1900 euro for "Groceries" in the previous budget period
    And Unassigned in the previous budget period is 100 euro
    And the Overview shows the previous budget period
    When I remove the income labelled "Teruggave" and confirm
    Then I should have been asked to confirm first
    And I should be told that the income was removed
    And Unassigned in the previous budget period should be -100 euro
    And the previous budget period should be shown as over-assigned
    When I try to assign -100 euro to "Groceries" in the previous budget period
    Then the assignment should be refused
    And I should be told that nothing can be assigned in a past budget period
    And the budget for "Groceries" in the previous budget period should still be 1900 euro
    And Unassigned in the previous budget period should still be -100 euro
    And the previous budget period should be shown as over-assigned
