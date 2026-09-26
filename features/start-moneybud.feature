# Starting MoneyBud, now that it keeps its data (glossary: "What MoneyBud keeps", settled by the
# stakeholder on 2026-09-26). What is kept, and that it is all there after starting again, is in
# keep-data.feature. A save that fails is in carry-on-when-saving-fails.feature.
#
# A start meets one of four situations, and this file has a section for each:
#   - NO KEPT DATA YET. MoneyBud starts as it always has: the six default categories and nothing
#     else. Carried over unchanged, not newly ruled (glossary: "A first start is unchanged"). "No
#     data yet" covers the very first start and a start after I have deleted what MoneyBud kept.
#     Deleting it myself is the only way to start over: MoneyBud has no act for it.
#   - KEPT DATA. MoneyBud opens it as it was left, and does NOT add the default categories back. A
#     default I deleted, renamed or archived stays that way.
#   - KEPT DATA IT CANNOT READ, because it is damaged, is blank, or was written by a newer version
#     of MoneyBud. MoneyBud says it cannot read it, touches nothing, and closes. It does not start
#     with empty data instead, because saving is automatic and the first change would write over the
#     history. The message POINTS NOWHERE: it names no place and does not refer to the README. It
#     only says the data cannot be read (glossary: "Where the data is, is written in the README").
#     BLANK means kept data with nothing at all in it: not even an empty budget. It is NOT "no data
#     yet". Settled by the stakeholder on 2026-09-26.
#     Kept data MoneyBud CANNOT REACH at all — a blocked folder, a profile that is not there — is
#     treated the same way: it may be there, out of reach, and starting empty would write over it
#     once it can be reached again. Settled by the stakeholder on 2026-09-26, after this file was
#     first approved, and added as a row of the outline with his approval.
#     A blank save is not the same as an EMPTY BUDGET. A budget with no categories and nothing
#     recorded in it (every default deleted before anything was entered, say) is valid kept data,
#     and opens as no categories. The stakeholder drew that line on 2026-09-26.
#   - MONEYBUD IS ALREADY OPEN. The second start says MoneyBud is already open, and closes. Two
#     MoneyBuds saving one set of data would overwrite each other's changes.
#
# Not specified here, and deliberately so: where the data is kept (a fixed place in my profile,
# never chosen by me, and written down in the README only), and data kept by an OLDER version of
# MoneyBud. Until the switch to real use, a new version may be unable to read an older one's data,
# and is then in the third situation above. No scenario involves more than one version apart from
# the "newer version" row, which is the ruling's own case.
#
# Reading the steps:
#   - "I start MoneyBud" is starting it in whatever situation the Givens describe.
#   - "I have never used MoneyBud" means nothing is kept. "I used MoneyBud, ..., and have since
#     deleted the data it kept" and "MoneyBud could not read the data it kept, and I have since
#     deleted it" are the two other ways of having no kept data: I removed it myself, outside
#     MoneyBud, having found where it is in the README.
#   - "MoneyBud has kept data that is damaged (is blank, with nothing at all in it; was written by
#     a newer version of MoneyBud)" is what is there when MoneyBud starts. How it got that way is
#     not the point. "Cannot be reached" means MoneyBud cannot get at its data at all when it starts.
#     "Blank, with nothing at all in it" means the kept data is there and holds
#     nothing whatsoever. It does NOT mean an empty budget: kept data that holds a budget with no
#     categories and nothing recorded is readable, and is the scenario "Kept data with every
#     default deleted still has no categories after starting again".
#   - "I should be told that MoneyBud cannot read my data" is the message. What is fixed is what it
#     says, not the wording. "that message should not say where my data is kept, nor where to find
#     out" means it names no folder or file, and does not mention the README.
#   - "MoneyBud should close without opening the Overview" means nothing is shown to enter anything
#     into, before or after the message.
#   - "the data MoneyBud could not read should be exactly as it was" means MoneyBud changed nothing
#     in it, removed nothing and added nothing.
#   - "MoneyBud is open" and "I start MoneyBud again while it is open" are two starts of MoneyBud at
#     once, on one computer, by one user. "the second start" is the one that is refused, and "the
#     MoneyBud that was already open" is the first.
#   - "no categories should be offered for a new expense" means the list of categories offered is
#     empty.
#   - "I close MoneyBud and start it again" is keep-data.feature's.
#   - Every other step is reused unchanged from the file that introduced it.
#
# The names, labels and amounts are synthetic test data. The six default category names are
# content MoneyBud ships with, not test data (glossary: "The default categories").

@keeping
Feature: Start MoneyBud
  As someone whose whole budgeting history is in MoneyBud
  I want MoneyBud to start with a sensible set of categories the first time, and afterwards to open only data it can read, one MoneyBud at a time
  So that starting MoneyBud can never write over, or lose, what it has kept

  # ----------------------------------------------------------------------------------
  # No kept data yet: the six default categories, and nothing else
  #
  # The same start in all three rows. What was recorded before the data was deleted is gone with
  # it, and the income in the second row is there to show that.
  # ----------------------------------------------------------------------------------

  Scenario Outline: With no kept data, MoneyBud starts with the six default categories and nothing else
    Given <history>
    When I start MoneyBud
    Then the Overview should show the current budget period
    And the categories offered for a new expense should be exactly these, in any order:
      | category      |
      | Boodschappen  |
      | Huur          |
      | Hobby         |
      | Sparen        |
      | Verzekeringen |
      | Abonnementen  |
    And no category should have a budget or any spending in the current budget period
    And no incomes should be listed in the current budget period
    And no expenses should be listed in the current budget period

    Examples:
      | history                                                                                                               |
      | I have never used MoneyBud                                                                                            |
      | I used MoneyBud, recorded an income of 1832.45 euro labelled "Salaris" in it, and have since deleted the data it kept |
      | MoneyBud could not read the data it kept, and I have since deleted it                                                 |

  # ----------------------------------------------------------------------------------
  # Kept data: opened as it was left, and the defaults are not added back
  #
  # The defaults are what a first start gives, not something MoneyBud keeps topping up. Were they
  # added back by name, Verzekeringen would return, and Hobby would return beside Hobbies.
  # ----------------------------------------------------------------------------------

  # Sparen is archived with no history anywhere, so no period shows it (archive-category.feature).
  Scenario: A default category I deleted, renamed or archived stays that way after starting again
    Given I have just started using MoneyBud for the first time
    When I delete the category "Verzekeringen"
    And I rename the category "Hobby" to "Hobbies"
    And I archive the category "Sparen"
    And I close MoneyBud and start it again
    Then the categories offered for a new expense should be exactly these, in any order:
      | category     |
      | Boodschappen |
      | Huur         |
      | Hobbies      |
      | Abonnementen |
    And the categories shown in the current budget period should be exactly these, in this order:
      | category     |
      | Boodschappen |
      | Huur         |
      | Hobbies      |
      | Abonnementen |

  # The rule is "no kept data", not "no categories". Kept data with no categories in it is still
  # kept data. Confirmed by the stakeholder on 2026-09-26.
  #
  # This is the VALID EMPTY BUDGET: no categories and nothing recorded. It is readable and opens as
  # no categories. It is not the BLANK save of the unreadable outline below, which has nothing at
  # all in it and is refused.
  Scenario: Kept data with every default deleted still has no categories after starting again
    Given I have just started using MoneyBud for the first time
    When I delete the category "Boodschappen"
    And I delete the category "Huur"
    And I delete the category "Hobby"
    And I delete the category "Sparen"
    And I delete the category "Verzekeringen"
    And I delete the category "Abonnementen"
    And I close MoneyBud and start it again
    Then no categories should be offered for a new expense

  # ----------------------------------------------------------------------------------
  # Kept data MoneyBud cannot read: say so, touch nothing, and close
  #
  # Data that cannot be read may still be recovered, by me or by a later version, but only while
  # nothing has written over it. Opening with nothing to enter into was rejected, because MoneyBud
  # could not save it anyway, and whatever was typed would be lost at closing.
  # ----------------------------------------------------------------------------------

  Scenario Outline: When MoneyBud cannot read what it kept, it says so, touches nothing, and closes
    Given MoneyBud has kept data that <problem>
    When I start MoneyBud
    Then I should be told that MoneyBud cannot read my data
    And that message should not say where my data is kept, nor where to find out
    And MoneyBud should close without opening the Overview
    And the data MoneyBud could not read should be exactly as it was

    Examples:
      | problem                                    |
      | is damaged                                 |
      | is blank, with nothing at all in it        |
      | was written by a newer version of MoneyBud |
      | cannot be reached                          |

  # ----------------------------------------------------------------------------------
  # MoneyBud is already open: the second start says so, and closes
  #
  # The MoneyBud that was already open is not disturbed: it stays on the period it showed, and what
  # I enter in it is kept. Once it is closed, MoneyBud starts as usual.
  # ----------------------------------------------------------------------------------

  Scenario: Starting MoneyBud while it is already open says so and closes, and the open one carries on
    Given my budget periods are one month long
    And I have a category "Groceries"
    And MoneyBud is open
    And the Overview shows the previous budget period
    When I start MoneyBud again while it is open
    Then the second start should tell me that MoneyBud is already open
    And the second start should close without opening the Overview
    And the MoneyBud that was already open should still show the previous budget period
    When I record an expense of 12.50 euro for "Groceries" labelled "Kiosk"
    And I close MoneyBud and start it again
    Then the Overview should show the current budget period
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label | amount |
      | today | Groceries | Kiosk | 12.50  |
