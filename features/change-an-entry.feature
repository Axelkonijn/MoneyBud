# Changing an entry: correcting an expense or an income after it was recorded (glossary: Change;
# "An entry can be changed or removed", settled by the stakeholder on 2026-09-26). Removing one is
# its own capability, in remove-an-entry.feature.
#
# What can be changed: an expense's amount, date, label and category, and an income's amount, date
# and label. An income has no category, so there is none to change.
#
# The rules, from arc42 §12:
#   - A change is allowed in ANY budget period, past ones included. An entry is a fact, and a wrong
#     fact is fixable wherever it sits. "Past is past" is about not re-planning, and a change
#     re-plans nothing (glossary: "An entry in a past period can be corrected"). So a correction
#     can change a past period's figures, including whether a category was over budget there.
#   - A change is judged EXACTLY as the changed entry would be if it were recorded now. Every rule
#     that accepts or refuses a recording applies, and nothing else does: an expense dated in the
#     future is refused and an income dated in the future is allowed; an income needs a label and
#     an expense's label is optional; every label is trimmed; an expense must name one of my
#     categories, compared by the name rule; an amount must be more than zero and whole cents, and
#     typed text is read as any typed amount is (type-an-amount.feature). When several rules are
#     broken, I am told the one recording would tell me. The refusals are told in the same words
#     as recording's, so the steps are record-expense.feature's and record-income.feature's.
#   - A REFUSED CHANGE LEAVES THE ENTRY EXACTLY AS IT WAS. Every refusal below shows the entry's
#     list row unchanged, and the figures it feeds unchanged.
#   - Moving an expense ONTO an archived category, by changing its category, brings that category
#     back, spelled as it was, and I am told, as when an expense is recorded against it. Fixing an
#     expense that is ALREADY on an archived category (its amount, its label or its date) does not
#     bring it back: that is correcting history, not using the category again. A refused change
#     brings nothing back, because nothing changed.
#   - A change that moves an entry into another budget period takes it out of this period's list
#     and figures. The Overview stays on the period it showed and says which period the entry went
#     to, the rule step-between-periods.feature already has for a new entry.
#   - Lowering an income, or moving it out of its period, may leave that period Over-assigned. That
#     is allowed, and shown with the existing marker. Nothing more is said about it. In a past
#     period it then stays over-assigned for good, which remove-an-entry.feature shows.
#   - A change is never confirmed first. Only removing asks (remove-an-entry.feature), because a
#     change leaves the entry there to be changed again.
#   - A change that goes through is ANNOUNCED: I am told the expense (income) was changed. Where it
#     also brings an archived category back, or moves the entry to another period, I am told that
#     too. Settled by the stakeholder on 2026-09-26.
#   - Saving an entry with NOTHING CHANGED is never refused, and goes through QUIETLY: nothing
#     changes and nothing is announced. That holds whatever the amount looks like when typed, so
#     an entry of 2000 euro saved as it is goes through, although "2.000" typed fresh would be
#     refused as ambiguous. How the form shows the loaded amount is for the plan; what is fixed is
#     that saving it unchanged cannot fail. Settled by the stakeholder on 2026-09-26.
#   - A change overwrites the entry. MoneyBud keeps no record of what it was before.
#
# Which entry a step acts on. On screen, I pick an entry by clicking its row in the period's list,
# and that row is what I recognise it by. So a step names an entry by what its row shows:
#   - "the expense labelled "X"" and "the income labelled "X"" are the one entry with that label in
#     the period on screen. Every entry a scenario acts on has a label of its own, so this is never
#     ambiguous.
#   - "the expense of 18 euro for "Groceries" without a label" names the one expense with no label,
#     by the rest of its row. It is used once, for the case that the expense has no label to be
#     named by.
# The entry is picked from the period on screen, as on the real screen. MoneyBud opens on the
# current period, so unless a scenario says "the Overview shows" another one, that is where it is.
# A scenario that corrects an entry in the previous period shows that period first.
#
# Reading the steps:
#   - "I have recorded an expense (income) of ... dated ..." is recording's own grammar, as a Given:
#     the entry was recorded earlier, with exactly that amount, category, label and date. With no
#     date given, it is dated today. Givens are listed in the order things happened.
#   - "I change the amount (date, category, label) of ... to ..." changes that one thing and leaves
#     the rest of the entry as it is. "I try to change" is the same act, where the scenario expects
#     a refusal. A date is named as recording names one: "today", "tomorrow", "the first day of the
#     next budget period", and so on.
#   - "I try to change the expense (income) labelled "X" into an expense (income) of ..." gives the
#     whole entry as it would be after the change, in recording's grammar. It is used only where
#     more than one thing changes at once, to show which refusal is told first.
#   - "the change should go through" means the entry now reads as changed. "the change should be
#     refused" means it does not, and the Thens after it show the entry as it was.
#   - "I should be told that the expense (income) was changed" is the announcement. What is fixed
#     is that I am told, not the wording.
#   - "I save the expense (income) labelled "X" without changing anything" is picking the entry
#     and saving it as it was loaded. "I should not have been told anything" is
#     step-between-periods.feature's: no message of any kind appeared.
#   - A changed amount written in quotation marks, with no "euro", is the text as typed. That is
#     type-an-amount.feature's grammar, borrowed for the two outlines that are about typing.
#   - The list steps are list-transactions-in-a-period.feature's: the whole list, in order, and
#     nothing else. An empty label cell means the expense has no label.
#   - Every other step is reused unchanged from the file that introduced it.
#
# What is NOT specified here, and why:
#   - How a row loads into its form, and what the form shows after Opslaan or Annuleren. That is
#     form behaviour, left for the plan to propose (glossary: "On screen: picking an entry to
#     correct"), like the field order and the emptying category box before it.
#   - Corrections in a period that has already been swept. There is no sweep yet.
#
# Every scenario starts from an empty ledger and names the categories it needs. The names, labels
# and amounts are synthetic test data.

@budget
Feature: Change an entry
  As someone entering my money by hand
  I want to correct an expense or an income after I have recorded it, in whichever period it is
  So that a slip of the keyboard does not leave a period's figures wrong for good

  # ----------------------------------------------------------------------------------
  # Changing an expense's amount
  #
  # Remaining is the Budget minus everything spent, so a changed amount moves it by exactly the
  # difference. The Budget is the plan and a change never touches it.
  # ----------------------------------------------------------------------------------

  Scenario Outline: Changing an expense's amount moves the remaining budget by the difference
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I change the amount of the expense labelled "Albert Heijn" to <new amount> euro
    Then the change should go through
    And I should be told that the expense was changed
    And I should not be warned or asked to confirm
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount       |
      | today | Groceries | Albert Heijn | <new amount> |
    And the remaining "Groceries" budget in the current budget period should be <remaining> euro
    And the budget for "Groceries" in the current budget period should still be 400 euro

    Examples: lower and higher
      | new amount | remaining |
      | 23.15      | 376.85    |
      | 321.50     | 78.50     |

    Examples: cents
      | new amount | remaining |
      | 32.16      | 367.84    |
      | 0.01       | 399.99    |

  Scenario: Raising an expense so the category is spent down to exactly nothing is not over budget
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 385 euro for "Groceries" labelled "Markt" dated today
    When I change the amount of the expense labelled "Markt" to 400.00 euro
    Then the change should go through
    And I should be told that the expense was changed
    And the remaining "Groceries" budget in the current budget period should be 0.00 euro
    And "Groceries" should not be shown as over budget

  Scenario: Raising an expense one cent past the budget is over budget, and the change still goes through
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 385 euro for "Groceries" labelled "Markt" dated today
    When I change the amount of the expense labelled "Markt" to 400.01 euro
    Then the change should go through
    And I should be told that the expense was changed
    And I should not be warned or asked to confirm
    And the remaining "Groceries" budget in the current budget period should be -0.01 euro
    And "Groceries" should be shown as over budget

  Scenario: Lowering an expense can take a category back under its budget
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 425 euro for "Groceries" labelled "Markt" dated today
    When I change the amount of the expense labelled "Markt" to 375 euro
    Then the change should go through
    And I should be told that the expense was changed
    And the remaining "Groceries" budget in the current budget period should be 25 euro
    And "Groceries" should not be shown as over budget

  # The same refusals, in the same words, as record-expense.feature's "An expense amount must be
  # more than zero" and "cannot be finer than a cent". The sign is checked first, so -12.345 is
  # told it must be more than 0 euro.
  Scenario Outline: A changed expense amount must still be more than zero and whole cents
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I try to change the amount of the expense labelled "Albert Heijn" to <amount> euro
    Then the change should be refused
    And I should be told that <reason>
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And the remaining "Groceries" budget in the current budget period should still be 367.85 euro

    Examples:
      | amount  | reason                                |
      | 0.00    | an expense must be more than 0 euro   |
      | -0.01   | an expense must be more than 0 euro   |
      | -32.15  | an expense must be more than 0 euro   |
      | -12.345 | an expense must be more than 0 euro   |
      | 0.001   | an amount cannot be finer than a cent |
      | 32.155  | an amount cannot be finer than a cent |

  # ----------------------------------------------------------------------------------
  # A changed amount is typed, and read like any typed amount
  #
  # Correcting an amount means typing it again, so type-an-amount.feature's rules apply unchanged.
  # One row per rule, not every case again.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A changed amount typed with a comma, a point or a euro sign is read as usual
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I change the amount of the expense labelled "Albert Heijn" to <typed>
    Then the change should go through
    And I should be told that the expense was changed
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 23.15  |
    And the remaining "Groceries" budget in the current budget period should be 376.85 euro

    Examples:
      | typed     |
      | "23,15"   |
      | "23.15"   |
      | "€ 23,15" |

  Scenario Outline: A changed amount that cannot be read, or reads as a refused amount, leaves the expense as it was
    Given I have a budget of 2500 euro for "Groceries" in the current budget period
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I try to change the amount of the expense labelled "Albert Heijn" to <typed>
    Then the change should be refused
    And I should be told that <reason>
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And the remaining "Groceries" budget in the current budget period should still be 2467.85 euro

    Examples:
      | typed      | reason                                |
      | "2.000"    | "2.000" is ambiguous: 2000 or 2,00    |
      | "1.832,45" | "1.832,45" is not an amount           |
      | "abc"      | "abc" is not an amount                |
      | ""         | "" is not an amount                   |
      | "1.832"    | an amount cannot be finer than a cent |

  # ----------------------------------------------------------------------------------
  # Changing an expense's category
  #
  # The spending moves with the expense: out of one category's Remaining and into the other's.
  # Neither Budget moves. The name is compared as everywhere else: ends trimmed, a run of inner
  # spaces counted as one, case ignored. The expense then shows the category spelled as it already
  # was, because changing an expense never renames a category.
  # ----------------------------------------------------------------------------------

  Scenario: Changing an expense's category moves its spending from one category to the other
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have recorded an expense of 25 euro for "Groceries" labelled "Verf" dated today
    When I change the category of the expense labelled "Verf" to "Hobby"
    Then the change should go through
    And I should be told that the expense was changed
    And I should not be warned or asked to confirm
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category | label | amount |
      | today | Hobby    | Verf  | 25.00  |
    And the remaining "Groceries" budget in the current budget period should be 400 euro
    And the remaining "Hobby" budget in the current budget period should be 35 euro
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And the budget for "Hobby" in the current budget period should still be 60 euro

  Scenario Outline: The new category is found however I capitalise or space its name
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have a budget of 120 euro for "Eating out" in the current budget period
    And I have recorded an expense of 25 euro for "Groceries" labelled "Pizza" dated today
    When I change the category of the expense labelled "Pizza" to <typed>
    Then the change should go through
    And I should be told that the expense was changed
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category   | label | amount |
      | today | Eating out | Pizza | 25.00  |
    And the remaining "Eating out" budget in the current budget period should be 95 euro
    And the categories offered for a new expense should list "Eating out" once, and no other spelling of it

    Examples:
      | typed            |
      | "eating out"     |
      | "  EATING OUT  " |
      | "Eating   out"   |

  Scenario: An expense cannot be changed to a category I do not have
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have no category called "Holiday"
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I try to change the category of the expense labelled "Albert Heijn" to "Holiday"
    Then the change should be refused
    And I should be told that "Holiday" is not one of my categories
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And the remaining "Groceries" budget in the current budget period should still be 367.85 euro
    And the categories offered for a new expense should not include "Holiday"

  # A name that trims to nothing is no name, so the expense would have no category. The
  # quotation marks are part of the step.
  Scenario Outline: An expense cannot be changed to a category name that trims to nothing
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I try to change the category of the expense labelled "Albert Heijn" to <name>
    Then the change should be refused
    And I should be told that an expense needs a category
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And the remaining "Groceries" budget in the current budget period should still be 367.85 euro

    Examples:
      | name  |
      | ""    |
      | "   " |

  # ----------------------------------------------------------------------------------
  # Changes and an archived category
  #
  # Bringing a category back is a side effect, and it follows the CATEGORY CHANGE, not the
  # presence of an archived category on the entry being edited (glossary: "A changed entry is
  # judged as if it were recorded now"). Moving an expense onto archived Hobby names Hobby anew,
  # the way recording against it does. Fixing an expense that was always on Hobby does not.
  #
  # "the categories offered for a new expense should (not) include" is how this file, like
  # record-expense.feature, observes whether a category is archived.
  # ----------------------------------------------------------------------------------

  Scenario Outline: Moving an expense onto an archived category brings the category back, spelled as it was
    Given I have a budget of 60 euro for "Hobby" in the current budget period
    And I have archived the category "Hobby"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 25 euro for "Groceries" labelled "Verf" dated today
    When I change the category of the expense labelled "Verf" to <typed>
    Then the change should go through
    And I should be told that the expense was changed
    And I should be told that "Hobby" was brought back
    And I should not be warned or asked to confirm
    And the categories offered for a new expense should list "Hobby" once, and no other spelling of it
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category | label | amount |
      | today | Hobby    | Verf  | 25.00  |
    And the remaining "Hobby" budget in the current budget period should be 35 euro
    And the remaining "Groceries" budget in the current budget period should be 400 euro

    Examples:
      | typed      |
      | "Hobby"    |
      | "  hobby " |

  # Today is fixed as the last day of the period, so that "the first day of the current budget
  # period" is a different date and the date row really changes something.
  Scenario Outline: Fixing an expense that is already on an archived category leaves the category archived
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have recorded an expense of 25 euro for "Hobby" labelled "Verf" dated today
    And I have archived the category "Hobby"
    When I change the <field> of the expense labelled "Verf" to <new value>
    Then the change should go through
    And I should be told that the expense was changed
    And the categories offered for a new expense should not include "Hobby"
    And "Hobby" should be shown in the current budget period
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date   | category | label   | amount   |
      | <date> | Hobby    | <label> | <amount> |

    Examples:
      | field  | new value                                  | date                                       | label           | amount |
      | amount | 20 euro                                    | today                                      | Verf            | 20.00  |
      | label  | "Verf en kwasten"                          | today                                      | Verf en kwasten | 25.00  |
      | date   | the first day of the current budget period | the first day of the current budget period | Verf            | 25.00  |

  # Nothing changed, so nothing was brought back. The table carries the rest of the "told" step,
  # each refusal in its own words, as in record-expense.feature's matching outline.
  Scenario Outline: A refused change onto an archived category leaves the category archived
    Given my budget periods are one month long
    And I have a budget of 60 euro for "Hobby" in the current budget period
    And I have archived the category "Hobby"
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 25 euro for "Groceries" labelled "Verf" dated today
    When I try to change the expense labelled "Verf" into an expense of <amount> euro for "Hobby" labelled "Verf" dated on <day>
    Then the change should be refused
    And I should be told that <reason>
    And the categories offered for a new expense should not include "Hobby"
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount |
      | today | Groceries | Verf  | 25.00  |
    And the remaining "Groceries" budget in the current budget period should still be 375 euro
    And the remaining "Hobby" budget in the current budget period should still be 60 euro

    Examples:
      | amount | day      | reason                                   |
      | 0.00   | today    | an expense must be more than 0 euro      |
      | 12.345 | today    | an amount cannot be finer than a cent    |
      | 25.00  | tomorrow | an expense cannot be dated in the future |

  # Moving an expense OFF an archived category is judged as a fresh recording against Groceries,
  # which is accepted. Hobby stays archived, and with its only expense gone and no budget, it has
  # no history left in the period, so the period stops showing it (glossary: "Correcting can
  # change where a category is shown").
  Scenario: Moving an archived category's last expense to another category drops it from the period
    Given I have a category "Groceries"
    And I have a category "Hobby"
    And I have recorded an expense of 25 euro for "Hobby" labelled "Verf" dated today
    And I have archived the category "Hobby"
    When I change the category of the expense labelled "Verf" to "Groceries"
    Then the change should go through
    And I should be told that the expense was changed
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount |
      | today | Groceries | Verf  | 25.00  |
    And "Hobby" should not be shown in the current budget period
    And the categories offered for a new expense should not include "Hobby"

  # ----------------------------------------------------------------------------------
  # Changing an expense's label
  #
  # The label rule is recording's: trimmed at the ends, left alone inside, and optional on an
  # expense, so a label that trims to nothing leaves the expense with no label.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An expense's label can be corrected, and is trimmed like any label
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Hein" dated today
    When I change the label of the expense labelled "Albert Hein" to <typed>
    Then the change should go through
    And I should be told that the expense was changed
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And the remaining "Groceries" budget in the current budget period should still be 367.85 euro

    Examples:
      | typed              |
      | "Albert Heijn"     |
      | "  Albert Heijn  " |

  Scenario Outline: An expense's label changed to nothing leaves the expense with no label
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I change the label of the expense labelled "Albert Heijn" to <typed>
    Then the change should go through
    And I should be told that the expense was changed
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount |
      | today | Groceries |       | 32.15  |

    Examples:
      | typed |
      | ""    |
      | "   " |

  Scenario: An expense recorded without a label can be given one
    Given I have a category "Groceries"
    And I have recorded an expense of 18 euro for "Groceries" without a label
    When I change the label of the expense of 18 euro for "Groceries" without a label to "Bakker"
    Then the change should go through
    And I should be told that the expense was changed
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label  | amount |
      | today | Groceries | Bakker | 18.00  |

  # ----------------------------------------------------------------------------------
  # Changing an expense's date
  #
  # An expense records money already spent, so a changed date in the future is refused, exactly as
  # a new one is. A date in another period moves the expense there: out of this period's list and
  # figures, and into that period's. The Overview stays where it was and says where the expense
  # went.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An expense cannot be changed to a date in the future
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I try to change the date of the expense labelled "Albert Heijn" to <day>
    Then the change should be refused
    And I should be told that an expense cannot be dated in the future
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And no expenses should be listed in the next budget period
    And the remaining "Groceries" budget in the current budget period should still be 367.85 euro

    Examples:
      | day                                     |
      | tomorrow                                |
      | the first day of the next budget period |

  # Markt and Bakker are both dated today, and Bakker was recorded later, so Bakker is listed
  # first. Once Bakker is dated earlier, the list follows the date.
  Scenario: A changed date within the same period moves the expense to its place in the list
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have a category "Groceries"
    And I have recorded an expense of 60 euro for "Groceries" labelled "Markt" dated today
    And I have recorded an expense of 18 euro for "Groceries" labelled "Bakker" dated today
    When I change the date of the expense labelled "Bakker" to the first day of the current budget period
    Then the change should go through
    And I should be told that the expense was changed
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date                                       | category  | label  | amount |
      | today                                      | Groceries | Markt  | 60.00  |
      | the first day of the current budget period | Groceries | Bakker | 18.00  |

  # Both directions: out of the current period into a past one, and out of a past one into the
  # current one. In each, the entry is corrected from its own period's screen.
  Scenario Outline: A changed date that moves an expense to another period takes it there, and the Overview stays and says where it went
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have recorded an expense of 12.50 euro for "Groceries" labelled "Kiosk" dated on <from>
    And the Overview shows the <shown> budget period
    When I change the date of the expense labelled "Kiosk" to <to>
    Then the change should go through
    And I should be told that the expense was changed
    And no expenses should be listed in the <shown> budget period
    And the expenses listed in the <other> budget period should be exactly these, in this order:
      | date | category  | label | amount |
      | <to> | Groceries | Kiosk | 12.50  |
    And the remaining "Groceries" budget in the <shown> budget period should be 400 euro
    And the remaining "Groceries" budget in the <other> budget period should be 387.50 euro
    And the Overview should show the <shown> budget period
    And I should be told that the expense went into the <other> budget period

    Examples:
      | from                                       | shown    | to                                         | other    |
      | today                                      | current  | the last day of the previous budget period | previous |
      | the last day of the previous budget period | previous | today                                      | current  |

  # A past period shows only what has history there. Groceries is in use, so the current period
  # shows it regardless; the previous one showed it only because of this expense.
  Scenario: Moving a category's only expense out of a past period drops the category from that period
    Given my budget periods are one month long
    And I have a category "Groceries"
    And I have set no budget for "Groceries" in the previous budget period
    And I have recorded an expense of 12.50 euro for "Groceries" labelled "Kiosk" dated on the last day of the previous budget period
    And the Overview shows the previous budget period
    When I change the date of the expense labelled "Kiosk" to today
    Then the change should go through
    And I should be told that the expense was changed
    And "Groceries" should not be shown in the previous budget period
    And "Groceries" should be shown in the current budget period
    And I should be told that the expense went into the current budget period

  # ----------------------------------------------------------------------------------
  # Correcting an expense in a past period
  #
  # A past period's plan cannot be changed, but its facts can. A wrong October expense is a record
  # of something that did not happen, and correcting it makes October truer, including whether a
  # category was over budget there (glossary: "An entry in a past period can be corrected").
  # ----------------------------------------------------------------------------------

  # 412.50 typed for 41.25.
  Scenario: Correcting an expense in a past period can take away that period's over-budget figure
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have recorded an expense of 412.50 euro for "Groceries" labelled "Markt" dated on the last day of the previous budget period
    And the Overview shows the previous budget period
    When I change the amount of the expense labelled "Markt" to 41.25 euro
    Then the change should go through
    And I should be told that the expense was changed
    And the remaining "Groceries" budget in the previous budget period should be 358.75 euro
    And "Groceries" should not be shown as over budget in the previous budget period
    And the budget for "Groceries" in the previous budget period should still be 400 euro

  # And the other way round: 41.25 typed for 412.50.
  Scenario: Correcting an expense in a past period can put that period over budget
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the previous budget period
    And I have recorded an expense of 41.25 euro for "Groceries" labelled "Markt" dated on the last day of the previous budget period
    And the Overview shows the previous budget period
    When I change the amount of the expense labelled "Markt" to 412.50 euro
    Then the change should go through
    And I should be told that the expense was changed
    And I should not be warned or asked to confirm
    And the remaining "Groceries" budget in the previous budget period should be -12.50 euro
    And "Groceries" should be shown as over budget in the previous budget period
    And the budget for "Groceries" in the previous budget period should still be 400 euro

  # ----------------------------------------------------------------------------------
  # Changing an income's amount
  #
  # Unassigned is the period's income minus everything assigned in it, so a changed income moves
  # it by exactly the difference. Lowering an income can take Unassigned below zero: the period is
  # then Over-assigned, shown with the same marker as Over budget, and never blocked or warned
  # about (glossary: "Removing or lowering an income may leave its period over-assigned").
  # ----------------------------------------------------------------------------------

  Scenario Outline: Changing an income's amount moves Unassigned by the difference
    Given I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And Unassigned in the current budget period is 1432.45 euro
    When I change the amount of the income labelled "Salaris" to <new amount> euro
    Then the change should go through
    And I should be told that the income was changed
    And I should not be warned or asked to confirm
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount       |
      | today | Salaris | <new amount> |
    And Unassigned in the current budget period should be <unassigned> euro
    And the budget for "Groceries" in the current budget period should still be 400 euro

    Examples:
      | new amount | unassigned |
      | 1823.45    | 1423.45    |
      | 2000.00    | 1600.00    |
      | 1832.46    | 1432.46    |

  Scenario: Lowering an income to exactly what was assigned leaves Unassigned at zero, which is not over-assigned
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have a budget of 1600 euro for "Groceries" in the current budget period
    When I change the amount of the income labelled "Salaris" to 1600.00 euro
    Then the change should go through
    And I should be told that the income was changed
    And Unassigned in the current budget period should be 0.00 euro
    And the current budget period should not be shown as over-assigned

  Scenario Outline: Lowering an income below what was assigned goes through, and the period is shown as over-assigned
    Given I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have a budget of 1600 euro for "Groceries" in the current budget period
    When I change the amount of the income labelled "Salaris" to <new amount> euro
    Then the change should go through
    And I should be told that the income was changed
    And I should not be warned or asked to confirm
    And Unassigned in the current budget period should be <unassigned> euro
    And the current budget period should be shown as over-assigned
    And Unassigned in the current budget period should be marked the same way as a category that is over budget
    And the budget for "Groceries" in the current budget period should still be 1600 euro

    Examples:
      | new amount | unassigned |
      | 1500.00    | -100.00    |
      | 1599.99    | -0.01      |

  # Nothing can be assigned in a past period, a negative amount included, so no budget there can
  # be lowered to match. That the period stays over-assigned for good is shown in
  # remove-an-entry.feature, where the same thing happens by removing an income.
  Scenario: Lowering an income in a past period can leave that period over-assigned
    Given my budget periods are one month long
    And I have recorded an income of 1800 euro labelled "Salaris" dated on the last day of the previous budget period
    And I have a budget of 1700 euro for "Groceries" in the previous budget period
    And Unassigned in the previous budget period is 100 euro
    And the Overview shows the previous budget period
    When I change the amount of the income labelled "Salaris" to 1600 euro
    Then the change should go through
    And I should be told that the income was changed
    And Unassigned in the previous budget period should be -100 euro
    And the previous budget period should be shown as over-assigned
    And the budget for "Groceries" in the previous budget period should still be 1700 euro

  Scenario Outline: A changed income amount must still be more than zero and whole cents
    Given I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    When I try to change the amount of the income labelled "Salaris" to <amount> euro
    Then the change should be refused
    And I should be told that <reason>
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | today | Salaris | 1832.45 |
    And Unassigned in the current budget period should still be 1832.45 euro

    Examples:
      | amount   | reason                                |
      | 0.00     | an income must be more than 0 euro    |
      | -0.01    | an income must be more than 0 euro    |
      | -12.345  | an income must be more than 0 euro    |
      | 0.001    | an amount cannot be finer than a cent |
      | 1832.459 | an amount cannot be finer than a cent |

  # ----------------------------------------------------------------------------------
  # Changing an income's label
  #
  # An income names no category, so its label is the only thing that says what the money is. It
  # is required, and a label that trims to nothing is not a label (record-income.feature).
  # ----------------------------------------------------------------------------------

  Scenario Outline: An income's label can be corrected, and is trimmed like any label
    Given I have recorded an income of 1832.45 euro labelled "Salaris oktbr" dated today
    When I change the label of the income labelled "Salaris oktbr" to <typed>
    Then the change should go through
    And I should be told that the income was changed
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label           | amount  |
      | today | Salaris oktober | 1832.45 |
    And Unassigned in the current budget period should still be 1832.45 euro

    Examples:
      | typed                 |
      | "Salaris oktober"     |
      | "  Salaris oktober  " |

  Scenario Outline: An income's label cannot be changed to nothing
    Given I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    When I try to change the label of the income labelled "Salaris" to <typed>
    Then the change should be refused
    And I should be told that an income needs a label
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | today | Salaris | 1832.45 |

    Examples:
      | typed |
      | ""    |
      | " "   |
      | "   " |

  # ----------------------------------------------------------------------------------
  # Changing an income's date
  #
  # An income may be dated in the future, so a changed date in the future is allowed, the opposite
  # of an expense. It joins its new period's Unassigned at once and leaves the old one's.
  # ----------------------------------------------------------------------------------

  Scenario: An income can be changed to a date in the next period, and the Overview stays and says where it went
    Given my budget periods are one month long
    And I have recorded an income of 1800 euro labelled "Salaris" dated today
    And the Overview shows the current budget period
    When I change the date of the income labelled "Salaris" to the first day of the next budget period
    Then the change should go through
    And I should be told that the income was changed
    And I should not be warned or asked to confirm
    And no incomes should be listed in the current budget period
    And the incomes listed in the next budget period should be exactly these, in this order:
      | date                                    | label   | amount  |
      | the first day of the next budget period | Salaris | 1800.00 |
    And Unassigned in the current budget period should be 0.00 euro
    And Unassigned in the next budget period should be 1800 euro
    And the Overview should show the current budget period
    And I should be told that the income went into the next budget period

  # Moving an income out of a period takes income out of it, the same as lowering it.
  Scenario: Moving an income out of a period that was assigned from leaves that period over-assigned
    Given my budget periods are one month long
    And I have recorded an income of 2000 euro labelled "Salaris" dated today
    And I have a budget of 400 euro for "Groceries" in the current budget period
    When I change the date of the income labelled "Salaris" to the first day of the next budget period
    Then the change should go through
    And I should be told that the income was changed
    And Unassigned in the current budget period should be -400 euro
    And the current budget period should be shown as over-assigned
    And Unassigned in the next budget period should be 2000 euro
    And the budget for "Groceries" in the current budget period should still be 400 euro

  # ----------------------------------------------------------------------------------
  # A change that breaks more than one rule
  #
  # Told the first broken rule in the order recording uses. For an expense: the category (a name
  # that trims to nothing, then a name that is not one of mine), then the amount (not more than
  # zero, then finer than a cent), then the date. For an income: the label, then the amount. The
  # table carries the rest of the "told" step.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An expense change that breaks more than one rule is refused for the first of them
    Given my budget periods are one month long
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And I have no category called "Holiday"
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I try to change the expense labelled "Albert Heijn" into an expense of <amount> euro for <category> labelled "Albert Heijn" dated on <day>
    Then the change should be refused
    And I should be told that <reason>
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And the remaining "Groceries" budget in the current budget period should still be 367.85 euro

    Examples:
      | amount  | category    | day      | reason                                |
      | 12.345  | "   "       | tomorrow | an expense needs a category           |
      | 0.00    | "Holiday"   | tomorrow | "Holiday" is not one of my categories |
      | -12.345 | "Groceries" | tomorrow | an expense must be more than 0 euro   |
      | 12.345  | "Groceries" | tomorrow | an amount cannot be finer than a cent |

  Scenario: An income change that breaks two rules at once is refused for its label
    Given I have recorded an income of 1832.45 euro labelled "Salaris" dated today
    When I try to change the income labelled "Salaris" into an income of -20 euro labelled "   " dated today
    Then the change should be refused
    And I should be told that an income needs a label
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | today | Salaris | 1832.45 |
    And Unassigned in the current budget period should still be 1832.45 euro

  # ----------------------------------------------------------------------------------
  # A changed entry is the same entry
  #
  # A period's entries on one date are listed newest-recorded first. A change rewrites an entry;
  # it does not record a new one, so the entry keeps its place among the entries on its date. This
  # is the documentation's reading, not put to the stakeholder (glossary: "A change overwrites the
  # entry"). Bakker was recorded first, so it stays below Coffee after it is changed.
  # ----------------------------------------------------------------------------------

  Scenario: A changed entry keeps its place among the entries on its date
    Given I have a category "Groceries"
    And I have recorded an expense of 18 euro for "Groceries" labelled "Bakker" dated today
    And I have recorded an expense of 3.50 euro for "Groceries" labelled "Coffee" dated today
    When I change the amount of the expense labelled "Bakker" to 20 euro
    Then the change should go through
    And I should be told that the expense was changed
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label  | amount |
      | today | Groceries | Coffee | 3.50   |
      | today | Groceries | Bakker | 20.00  |

  # ----------------------------------------------------------------------------------
  # Saving with nothing changed
  #
  # Never refused, and quiet: nothing changes and nothing is announced (settled by the stakeholder
  # on 2026-09-26). An entry that was accepted once is accepted again as it is. That includes an
  # amount that could not be typed fresh as it might be shown: 2000 euro and 1500 euro, typed as
  # "2.000" or "1.500", are refused as ambiguous (type-an-amount.feature), but saving them
  # unchanged cannot fail. How the form shows a loaded amount is for the plan.
  # ----------------------------------------------------------------------------------

  Scenario Outline: Saving an expense with nothing changed goes through quietly, whatever its amount
    Given I have a budget of 2500 euro for "Groceries" in the current budget period
    And I have recorded an expense of <amount> euro for "Groceries" labelled "Markt" dated today
    When I save the expense labelled "Markt" without changing anything
    Then the change should go through
    And I should not have been told anything
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount   |
      | today | Groceries | Markt | <amount> |
    And the remaining "Groceries" budget in the current budget period should still be <remaining> euro

    Examples:
      | amount  | remaining |
      | 32.15   | 2467.85   |
      | 2000.00 | 500.00    |
      | 1500.00 | 1000.00   |
      | 0.01    | 2499.99   |

  Scenario Outline: Saving an income with nothing changed goes through quietly, whatever its amount
    Given I have recorded an income of <amount> euro labelled "Salaris" dated today
    When I save the income labelled "Salaris" without changing anything
    Then the change should go through
    And I should not have been told anything
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount   |
      | today | Salaris | <amount> |
    And Unassigned in the current budget period should still be <amount> euro

    Examples:
      | amount  |
      | 1832.45 |
      | 2000.00 |

  # Recorded fresh, an expense against archived Hobby would bring Hobby back. Saved unchanged, it
  # names nothing anew, so Hobby stays archived, and nothing is said.
  Scenario: Saving an expense on an archived category with nothing changed leaves the category archived
    Given I have a budget of 60 euro for "Hobby" in the current budget period
    And I have recorded an expense of 25 euro for "Hobby" labelled "Verf" dated today
    And I have archived the category "Hobby"
    When I save the expense labelled "Verf" without changing anything
    Then the change should go through
    And I should not have been told anything
    And the categories offered for a new expense should not include "Hobby"
    And the remaining "Hobby" budget in the current budget period should still be 35 euro
