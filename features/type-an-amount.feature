# How the amount I type is read (glossary: "Typing an amount", ruled by the stakeholder on
# 2026-09-25). One rule covers every place I type an amount: recording an expense, recording an
# income, and assigning. Changing an entry's amount is typing it again, under the same rule
# (change-an-entry.feature).
#
#   - A comma or a point is the decimal mark. "12,50" and "12.50" are both twelve euros fifty.
#   - No thousands separator is accepted. "1.832,45", "1,832.45" and "1 832" are not amounts.
#   - A mark followed by exactly three digits ending in 0, such as "2.000" or "1,500", is refused
#     as AMBIGUOUS. Read as a decimal it is two euros, read the way MoneyBud itself writes
#     thousands it is two thousand, and both are whole cents, so nothing later would catch the
#     wrong one. An entry can be corrected afterwards (change-an-entry.feature), but only once
#     the wrong amount is noticed, so MoneyBud refuses rather than guesses, and names both readings.
#   - A mark followed by four or more digits that are all whole cents, such as "2.0000", is not an
#     amount (ruled 2026-09-26). MoneyBud shows at most two decimals, and "2.0000" would otherwise
#     be recorded as 2,00 without a word.
#   - A mark needs a digit before it and after it. ",50" and "12," are not amounts (ruled
#     2026-09-26): always type a digit before the mark.
#   - Text that is not an amount is refused before anything is recorded.
#   - Tolerated: a euro sign in front, a true minus sign ("−") as well as a hyphen ("-"), and spaces
#     around the number.
#
# What reading does NOT decide. It never rounds, and it never judges a sign. "1.832" is three
# digits after the mark, not ending in 0, so it is read as a decimal, one euro and 832
# thousandths. It is not read as 1832. The cent rule then refuses it, exactly as it refuses
# 12.345 in record-expense.feature ("An expense amount cannot be finer than a cent"), and in the
# matching outlines of record-income.feature and assign-to-category.feature. A minus is read as
# typed, and whether a negative amount is allowed is the rule of the thing being entered: an
# assignment takes it, an expense and an income refuse it. Those rules are not restated here.
#
# When text cannot be read, the entry is refused for THAT, before anything else about it is
# looked at (arc42 §8.2: "two layers of refusal, in a fixed order"). So typed text that is not an
# amount, aimed at a name that is not one of my categories, is refused as not an amount.
#
# Reading the steps:
#   - In THIS file an amount is written in quotation marks, "12,50", because it is the text I
#     typed, exactly, and it may not be an amount at all. The quotation marks are not typed; what
#     is between them is, spaces included. Elsewhere amounts are written unquoted, as 12.50 euro,
#     and always in a form these rules read without comment: those files are about what happens
#     to an amount, and this one is about how typed text becomes one. The two grammars are kept
#     apart on purpose. An unquoted amount always ends in "euro". A quoted one never does. The one
#     exception is change-an-entry.feature, which borrows this grammar for two outlines about
#     typing a changed amount.
#   - In a table, the quotation marks in a cell are part of the step, as in record-expense.feature.
#   - "I should be told that "abc" is not an amount" means the entry is refused because the text
#     is not an amount. It holds for empty text too. What I am told is a fact, not a sentence: the
#     Dutch wording is copy and belongs to the UI.
#   - "I should be told that "2.000" is ambiguous: 2000 or 2,00" means the entry is refused as
#     ambiguous, and the refusal names both readings: first as a whole number, as if the mark were
#     a thousands separator, then as a decimal. Again, the fact is fixed, not the sentence.
#   - Figures after a refusal ("should still be", "no expenses should be listed") are how each
#     scenario shows that nothing was recorded.
#
# Every scenario starts from an empty ledger and names the categories it needs. The names, labels
# and amounts are synthetic test data.

@budget
Feature: Type an amount
  As someone entering my money by hand
  I want to type an amount the way I naturally write it, with a comma or a point
  So that what MoneyBud records is exactly the amount I meant, and anything it cannot be sure I meant is refused instead of guessed

  # ----------------------------------------------------------------------------------
  # A comma or a point is the decimal mark
  #
  # "2.00" and "2,5" have a mark followed by one or two digits, so they are never ambiguous. "2000"
  # and "2,00" are the two readings of the ambiguous "2.000" (below), each typed so that it has only
  # one reading. That is how a refused ambiguous amount is fixed.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An amount typed with a comma or a point is read the same way
    Given I have a budget of 2500 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I record an expense of <typed> for "Groceries"
    Then the expense should be recorded
    And my "Groceries" spending in the current budget period should include <read as> euro without a label
    And the remaining "Groceries" budget in the current budget period should be <remaining> euro

    Examples: either mark
      | typed     | read as | remaining |
      | "12,50"   | 12.50   | 2487.50   |
      | "12.50"   | 12.50   | 2487.50   |
      | "12"      | 12.00   | 2488.00   |
      | "7,5"     | 7.50    | 2492.50   |
      | "0,01"    | 0.01    | 2499.99   |
      | "1832,45" | 1832.45 | 667.55    |
      | "1832.45" | 1832.45 | 667.55    |

    Examples: never ambiguous
      | typed  | read as | remaining |
      | "2.00" | 2.00    | 2498.00   |
      | "2,5"  | 2.50    | 2497.50   |
      | "2000" | 2000.00 | 500.00    |
      | "2,00" | 2.00    | 2498.00   |

  # ----------------------------------------------------------------------------------
  # What is tolerated around the number
  # ----------------------------------------------------------------------------------

  Scenario Outline: A euro sign in front and spaces around the number are tolerated
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I record an expense of <typed> for "Groceries"
    Then the expense should be recorded
    And my "Groceries" spending in the current budget period should include 3.50 euro without a label
    And the remaining "Groceries" budget in the current budget period should be 396.50 euro

    Examples:
      | typed        |
      | "€ 3,50"     |
      | "€3.50"      |
      | "  3,50  "   |
      | " € 3,50 "   |

  # A negative amount matters for assigning, where it moves money back into Unassigned
  # (assign-to-category.feature). So a minus is read whether it is typed as a hyphen or as a true
  # minus sign. "−€ 50,00" is how MoneyBud itself shows a negative amount: the minus, then the euro
  # sign.
  Scenario Outline: A minus is read as a hyphen or as a true minus sign
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And Unassigned in the current budget period is 1600 euro
    When I assign <typed> to "Groceries" in the current budget period
    Then the assignment should go through
    And I should not be told of any shortfall
    And the budget for "Groceries" in the current budget period should be 350 euro
    And Unassigned in the current budget period should be 1650 euro

    Examples:
      | typed      |
      | "-50"      |
      | "−50"      |
      | "-50,00"   |
      | "−50.00"   |
      | "−€ 50,00" |

  # Reading never judges a sign. Zero and a minus are read as typed, and it is the expense's own
  # rule that refuses them: "An expense amount must be more than zero" in record-expense.feature.
  Scenario Outline: Zero and a minus are read as typed, and the expense rule refuses them
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I try to record an expense of <typed> for "Groceries"
    Then the expense should not be recorded
    And I should be told that an expense must be more than 0 euro
    And no expenses should be listed in the current budget period
    And the remaining "Groceries" budget in the current budget period should still be 400 euro

    Examples:
      | typed    |
      | "0"      |
      | "0,00"   |
      | "-12,50" |
      | "−12,50" |

  # ----------------------------------------------------------------------------------
  # No thousands separator
  #
  # With both marks meaning "decimal", a thousands separator could not be told apart from one. So
  # an amount with a separator in it is not an amount. That includes the way MoneyBud itself shows
  # two thousand, "€ 2.000,00": it can be read on screen, and it cannot be typed back in.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An amount with a thousands separator is not an amount
    Given I have a budget of 2500 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I try to record an expense of <typed> for "Groceries"
    Then the expense should not be recorded
    And I should be told that <typed> is not an amount
    And no expenses should be listed in the current budget period
    And the remaining "Groceries" budget in the current budget period should still be 2500 euro

    Examples:
      | typed        |
      | "1.832,45"   |
      | "1,832.45"   |
      | "1 832"      |
      | "1.000.000"  |
      | "€ 2.000,00" |

  # ----------------------------------------------------------------------------------
  # Three digits after the mark
  #
  # A mark followed by exactly three digits is the one shape the rules above can read wrongly.
  #   - Ending in 0, as in "2.000": both readings are whole cents, so either would be recorded
  #     without a word. That is refused as ambiguous, and both readings are named. This was found
  #     in review, where "2.000" was being recorded as 2,00.
  #   - Not ending in 0, as in "1.832": read as a decimal it is finer than a cent, so the cent rule
  #     refuses it, and I see that refusal. It is not read as 1832.
  #
  # Four or more digits after the mark (ruled 2026-09-26). Nobody writes a thousands group of four,
  # so there is no second reading, but there is still a silent one: "2.0000" is whole cents and
  # would be recorded as 2,00, while MoneyBud never shows more than two decimals. So four or more
  # digits that are all whole cents ("2.0000", "12,5000") are NOT AN AMOUNT, under "Text that is
  # not an amount" below. Four or more digits that are not whole cents ("12,3456", and "12,3450",
  # which is 12.345) are read as a decimal and refused by the cent rule, the same as "1.832".
  #
  # The rule is applied as written, including where one reading is far less likely than the other:
  # "0,100" is refused although almost nobody means one hundred by it.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A mark followed by three digits ending in 0 is refused as ambiguous, naming both readings
    Given I have a budget of 2500 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I try to record an expense of <typed> for "Groceries"
    Then the expense should not be recorded
    And I should be told that <typed> is ambiguous: <as whole number> or <as decimal>
    And no expenses should be listed in the current budget period
    And the remaining "Groceries" budget in the current budget period should still be 2500 euro

    Examples:
      | typed     | as whole number | as decimal |
      | "2.000"   | 2000            | 2,00       |
      | "2,000"   | 2000            | 2,00       |
      | "1.500"   | 1500            | 1,50       |
      | "1,500"   | 1500            | 1,50       |
      | "12.340"  | 12340           | 12,34      |
      | "€ 2.000" | 2000            | 2,00       |
      | "0,100"   | 100             | 0,10       |

  Scenario Outline: Three or more digits after the mark that are finer than a cent are read as a decimal, and refused by the cent rule
    Given I have a budget of 2500 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I try to record an expense of <typed> for "Groceries"
    Then the expense should not be recorded
    And I should be told that an amount cannot be finer than a cent
    And no expenses should be listed in the current budget period
    And the remaining "Groceries" budget in the current budget period should still be 2500 euro

    Examples: three digits, not ending in 0
      | typed    |
      | "1.832"  |
      | "1,832"  |
      | "12,345" |

    Examples: four digits, not whole cents
      | typed     |
      | "12,3456" |
      | "12,3450" |

  # ----------------------------------------------------------------------------------
  # Text that is not an amount
  # ----------------------------------------------------------------------------------

  Scenario Outline: Text that is not an amount is refused before anything is recorded
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have not yet spent anything on "Groceries" in the current budget period
    When I try to record an expense of <typed> for "Groceries"
    Then the expense should not be recorded
    And I should be told that <typed> is not an amount
    And no expenses should be listed in the current budget period
    And the remaining "Groceries" budget in the current budget period should still be 400 euro

    Examples: not a number at all
      | typed |
      | "abc" |
      | ""    |
      | "   " |

    Examples: no digit before or after the mark
      | typed |
      | ",50" |
      | "12," |

    Examples: four or more digits after the mark, all whole cents
      | typed     |
      | "2.0000"  |
      | "12,5000" |
      | "7,00000" |

  # Text that cannot be read is refused for that, before the category is looked at (arc42 §8.2).
  # "Holiday" is not one of my categories, and that is not what I am told.
  Scenario Outline: An amount that cannot be read is refused for that, before the category is looked at
    Given I have a category "Groceries"
    And I have no category called "Holiday"
    When I try to record an expense of <typed> for "Holiday"
    Then the expense should not be recorded
    And I should be told that <reason>
    And no expenses should be listed in the current budget period

    Examples:
      | typed   | reason                             |
      | "abc"   | "abc" is not an amount             |
      | "2.000" | "2.000" is ambiguous: 2000 or 2,00 |

  # Bringing an archived category back is a side-effect of RECORDING (record-expense.feature). An
  # amount that cannot be read records nothing, so it brings nothing back.
  Scenario Outline: An amount that cannot be read leaves an archived category archived
    Given I have a budget of 60 euro for "Hobby" in the current budget period
    And I have not yet spent anything on "Hobby" in the current budget period
    And I have archived the category "Hobby"
    When I try to record an expense of <typed> for "Hobby"
    Then the expense should not be recorded
    And I should be told that <reason>
    And the categories offered for a new expense should not include "Hobby"
    And the remaining "Hobby" budget in the current budget period should still be 60 euro

    Examples:
      | typed   | reason                             |
      | "abc"   | "abc" is not an amount             |
      | "2.000" | "2.000" is ambiguous: 2000 or 2,00 |

  # ----------------------------------------------------------------------------------
  # An income and an assignment are read the same way
  #
  # Everything above is shown on an expense. The outlines below show that the same reading applies
  # when recording an income and when assigning, one row per rule rather than every case again.
  # The rows that reach a refusal from the thing being entered (finer than a cent, a minus on an
  # income) show that the text was read, and that the entry's own rule then decided.
  # ----------------------------------------------------------------------------------

  Scenario Outline: An income amount is read by the same rules
    Given I have recorded no income in the current budget period
    When I record an income of <typed> labelled "Statiegeld"
    Then the income should be recorded
    And my income in the current budget period should include <read as> euro labelled "Statiegeld"
    And Unassigned in the current budget period should be <read as> euro

    Examples:
      | typed       | read as |
      | "12,50"     | 12.50   |
      | "12.50"     | 12.50   |
      | "€ 1832,45" | 1832.45 |
      | "  47,55 "  | 47.55   |

  Scenario Outline: An income amount that cannot be read, or reads as a refused amount, is refused
    Given I have recorded no income in the current budget period
    When I try to record an income of <typed> labelled "Salaris september"
    Then the income should not be recorded
    And I should be told that <reason>
    And no incomes should be listed in the current budget period
    And Unassigned in the current budget period should still be 0 euro

    Examples:
      | typed      | reason                                |
      | "2.000"    | "2.000" is ambiguous: 2000 or 2,00    |
      | "1.832,45" | "1.832,45" is not an amount           |
      | "abc"      | "abc" is not an amount                |
      | ""         | "" is not an amount                   |
      | "1.832"    | an amount cannot be finer than a cent |
      | "−250"     | an income must be more than 0 euro    |

  # Zero is accepted by assigning and changes nothing (assign-to-category.feature), so "0,00" here
  # shows it is read as zero, not refused as text.
  Scenario Outline: An assignment amount is read by the same rules
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    And Unassigned in the current budget period is 1600 euro
    When I assign <typed> to "Groceries" in the current budget period
    Then the assignment should go through
    And the budget for "Groceries" in the current budget period should be <budget> euro
    And Unassigned in the current budget period should be <unassigned> euro

    Examples:
      | typed   | budget | unassigned |
      | "12,50" | 412.50 | 1587.50    |
      | "12.50" | 412.50 | 1587.50    |
      | "€ 100" | 500.00 | 1500.00    |
      | "0,00"  | 400.00 | 1600.00    |

  # A negative ambiguous amount is still ambiguous, and both readings keep the minus.
  Scenario Outline: An assignment amount that cannot be read, or reads as a refused amount, is refused
    Given I have already recorded 2000 euro of income in the current budget period
    And I have a budget of 400 euro for "Groceries" in the current budget period
    When I try to assign <typed> to "Groceries" in the current budget period
    Then the assignment should be refused
    And I should be told that <reason>
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And Unassigned in the current budget period should still be 1600 euro

    Examples:
      | typed      | reason                                |
      | "2.000"    | "2.000" is ambiguous: 2000 or 2,00    |
      | "-2.000"   | "-2.000" is ambiguous: -2000 or -2,00 |
      | "1.832,45" | "1.832,45" is not an amount           |
      | "abc"      | "abc" is not an amount                |
      | ""         | "" is not an amount                   |
      | "12,345"   | an amount cannot be finer than a cent |
