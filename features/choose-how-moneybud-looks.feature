# Choosing how MoneyBud looks on the phone: its theme and its appearance, dark, light or the phone's
# own, and the faint hints on the home screen the first time (glossary: "MoneyBud on the phone",
# its "Themes", "Where the data lives on the phone" and "Panels pulled over it"; settled with the
# stakeholder on 2026-09-29). Written 2026-09-30 for increment 14, whose two approval gates the
# stakeholder waived: nothing here is approved at a gate, and he reviews it with the plan and the app
# at the end of the increment.
#
# THIS FILE IS THE PHONE'S ALONE, BY RULING: "thema's vallen daarbuiten. Die zijn voor de
# desktopversie niet nodig." Themes are the one exception to "the builds stay the same" (ADR 0013,
# decision 5). The desktop has one look, no Instellingen, and none of these steps.
#
# What a theme LOOKS like is not specified here, and cannot be: the porcelain, the table, the glass,
# the gold and the cross-fade are drawing, seen on the phone and in the prototype. A theme CHANGES
# ONLY HOW MONEYBUD LOOKS AND MOVES, NEVER WHAT IT SHOWS, IN WHAT ORDER, OR WHAT AN ACT DOES (ADR
# 0013, decision 5). So every other feature file holds whichever theme and appearance are chosen, and
# none of them names one.
#
# The rules, from arc42 §12:
#   - RULED. TWO THEMES, "Standaard" and "Kintsugi". Each has a dark and a light form. Rejected
#     before: the ensō, and ukiyo-e for now; a third theme with kintsugi's own motions was dropped.
#   - RULED. THE APPEARANCE, on screen "Weergave", IS ONE SETTING FOR ALL THEMES, with the choices
#     "Systeem" (follow the phone), "Donker" and "Licht". Rejected: one per theme.
#   - RULED. THE CHOSEN THEME AND APPEARANCE ARE REMEMBERED BETWEEN STARTS, IN THE APP'S SETTINGS,
#     NOT IN THE DATA FILE, "zodat dat bestand voor desktop en telefoon hetzelfde blijft". Rejected:
#     always starting in Standaard.
#   - RULED. FAINT HINTS ON THE HOME SCREEN AT OPENING, THE FIRST TIME ONLY, gone after about ten
#     seconds; tapping them is not needed.
#   - APPROVED WITH THE PROTOTYPE, not ruled item by item: Instellingen holds Weergave, Thema,
#     "Aanwijzingen opnieuw tonen", which shows the home screen's hints again, and "Klaar". The
#     prototype offered the choices in the order below.
#   - DERIVED by the documentation: that the hints have been shown is remembered across starts, in the
#     app's settings, like the theme.
#
# CHOSEN IN THE BUILD, NOT RULED, and listed for the stakeholder's review:
#   - A FIRST START IS "Standaard" AND "Systeem", WITH THE HINTS SHOWN, as the prototype started.
#   - SETTINGS THAT CANNOT BE READ (damaged, blank, or not there at all while the data is) ARE PASSED
#     OVER WITHOUT A WORD: MoneyBud starts on Standaard and Systeem, shows the hints, says nothing
#     about it, and opens the data exactly as usual. The settings are about the phone in the hand,
#     not about the money, and must never stop MoneyBud. Unlike data it cannot read, there is nothing
#     in them worth protecting from being written over.
#   - CHOOSING A THEME OR AN APPEARANCE NEVER WRITES THE DATA FILE, and the data kept is the same
#     whichever are chosen. So a choice saves nothing, and says nothing about saving, even while
#     saving the data is not possible.
#   - SWITCHING THE THEME LEAVES THE APPEARANCE AS IT WAS, and choosing the appearance leaves the
#     theme: two settings, each on its own.
#   - THE SETTINGS ARE KEPT APART FROM THE DATA. So data MoneyBud cannot read is refused whatever the
#     settings say, and deleting the data, the one way to start over, leaves the theme, the
#     appearance and the hints as they were.
#   - The hints COUNT AS SHOWN THE MOMENT THEY ARE SHOWN, as in the prototype. "Aanwijzingen opnieuw
#     tonen" SHOWS THEM NOW, on the home screen, and not again at the next start.
#   - A choice is said by the screen changing, not by a notice: CHOOSING TELLS ME NOTHING.
#   - A choice is KEPT BY THE TIME MONEYBUD GOES TO THE BACKGROUND, so the phone ending MoneyBud there
#     loses none (carry-on-when-saving-fails.feature, its last section).
#
# NOT specified here: what each theme and appearance looks like, the cross-fade and the ring drawing
# itself again on switching, where the hints sit and what they point at, and where the settings are
# kept on the phone. All of that is drawing, or for the plan.
#
# Reading the steps:
#   - The themes and appearances are named in Dutch, as they are on screen, like the badge "Tekort"
#     and the display terms elsewhere: they are content MoneyBud ships with (glossary: "Proposed
#     display terms for the phone").
#   - "the theme should be "X"" is the theme MoneyBud is drawn in, which Instellingen shows as chosen.
#     "the appearance should be "X"" is the Weergave chosen. With "Systeem", whether the phone is dark
#     or light at that moment is the phone's, and not asserted. "should still be" says the steps
#     before did not change it, as everywhere.
#   - "the themes (appearances) offered should be exactly these, in this order" is the whole of that
#     choice in Instellingen, in its order.
#   - "I choose the theme "X"" and "I choose the appearance "X"" choose it in Instellingen.
#   - "I ask for the hints to be shown again" presses "Aanwijzingen opnieuw tonen".
#   - "the home screen should show the hints" means the faint hints are shown now, at this opening or
#     just after asking for them. That they fade by themselves after about ten seconds is not
#     asserted. "should not show the hints" means they are not shown at all.
#   - "the last time I used MoneyBud, I chose the theme "X" and the appearance "Y"" describes the
#     settings MoneyBud kept from an earlier use: those two choices, and the hints already shown, as
#     they are at any first start. It says nothing about the data, which the next Given describes.
#   - "the settings MoneyBud kept become damaged (become blank, with nothing at all in them; are gone)"
#     happens around MoneyBud while it is closed, as "MoneyBud has kept data that is damaged"
#     (start-moneybud.feature) does to the data. Only the settings are touched, never the data. "Are
#     gone" means no settings are kept while the data is there, as when a data file is copied by hand
#     onto a phone where MoneyBud was never started.
#   - "the data MoneyBud keeps should be exactly as it was before I chose how MoneyBud looks" means
#     nothing was written to it and nothing in it differs from just before the first choice in the
#     scenario. What the Givens set up was kept before that choice.
#   - "I have never used MoneyBud" (start-moneybud.feature) now also means that no settings are kept.
#   - "the phone ends MoneyBud while it is in the background" and "MoneyBud goes to the background"
#     are carry-on-when-saving-fails.feature's.
#   - Every other step is reused unchanged from the file that introduced it: starting, closing and
#     unreadable data are start-moneybud.feature's and keep-data.feature's, saving
#     carry-on-when-saving-fails.feature's.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", unless it is about a start
# with no data kept. The names, labels and amounts are synthetic test data. The six default category
# names and "Standaard", "Kintsugi", "Systeem", "Donker" and "Licht" are content MoneyBud ships with.

@phone
Feature: Choose how MoneyBud looks on the phone
  As someone who has MoneyBud in my hand every day
  I want to choose its theme and whether it is dark, light or follows my phone, and have that remembered, with a first-time hint at how to get around
  So that MoneyBud looks the way I like each time I open it, and finds its way without a manual, while the file with my money in it stays the same for the phone and the desktop

  # ----------------------------------------------------------------------------------
  # A first start
  # ----------------------------------------------------------------------------------

  Scenario: The first time, MoneyBud is in the Standaard theme, follows the phone for dark or light, and shows the hints
    Given I have never used MoneyBud
    When I start MoneyBud
    Then the theme should be "Standaard"
    And the appearance should be "Systeem"
    And the home screen should show the hints

  Scenario: Two themes and three appearances are offered
    Given MoneyBud is open
    Then the themes offered should be exactly these, in this order:
      | theme     |
      | Standaard |
      | Kintsugi  |
    And the appearances offered should be exactly these, in this order:
      | appearance |
      | Systeem    |
      | Donker     |
      | Licht      |

  # ----------------------------------------------------------------------------------
  # Choosing, and remembered between starts
  # ----------------------------------------------------------------------------------

  # Every pair but the first start's own, which the next scenario covers by going back to it.
  Scenario Outline: A theme and an appearance I choose are remembered when MoneyBud starts again
    Given MoneyBud is open
    When I choose the theme "<theme>"
    And I choose the appearance "<appearance>"
    Then the theme should be "<theme>"
    And the appearance should be "<appearance>"
    When I close MoneyBud and start it again
    Then the theme should still be "<theme>"
    And the appearance should still be "<appearance>"

    Examples:
      | theme     | appearance |
      | Kintsugi  | Systeem    |
      | Kintsugi  | Donker     |
      | Kintsugi  | Licht      |
      | Standaard | Donker     |
      | Standaard | Licht      |

  # Going back to the first start's pair is a choice like any other, and is remembered too, not
  # mistaken for never having chosen.
  Scenario: Going back to Standaard and Systeem is remembered as well
    Given MoneyBud is open
    When I choose the theme "Kintsugi"
    And I choose the appearance "Donker"
    And I close MoneyBud and start it again
    Then the theme should be "Kintsugi"
    And the appearance should be "Donker"
    When I choose the theme "Standaard"
    And I choose the appearance "Systeem"
    And I close MoneyBud and start it again
    Then the theme should be "Standaard"
    And the appearance should be "Systeem"

  # Ruled: one appearance for all themes. Had each theme its own, Kintsugi would come back Licht at
  # the end. Choosing tells me nothing: the screen itself changes.
  Scenario: The appearance is one setting for every theme, so switching the theme leaves it as it was
    Given MoneyBud is open
    When I choose the appearance "Licht"
    And I choose the theme "Kintsugi"
    Then the appearance should still be "Licht"
    When I choose the appearance "Donker"
    Then the theme should still be "Kintsugi"
    When I choose the theme "Standaard"
    Then the appearance should still be "Donker"
    When I choose the theme "Kintsugi"
    Then the appearance should still be "Donker"
    And I should not have been told anything

  # On the phone MoneyBud is not closed but sent to the background, where the phone may end it.
  Scenario: A choice is kept by the time MoneyBud goes to the background, so the phone ending it there loses nothing
    Given MoneyBud is open
    When I choose the theme "Kintsugi"
    And I choose the appearance "Licht"
    And MoneyBud goes to the background
    And the phone ends MoneyBud while it is in the background
    And I start MoneyBud
    Then the theme should be "Kintsugi"
    And the appearance should be "Licht"

  # ----------------------------------------------------------------------------------
  # The hints
  # ----------------------------------------------------------------------------------

  Scenario: The hints are shown the first time only
    Given I have never used MoneyBud
    When I start MoneyBud
    Then the home screen should show the hints
    When I close MoneyBud and start it again
    Then the home screen should not show the hints

  # As in the prototype: asking shows them now, and counts as showing them, so the next start does
  # not show them again.
  Scenario: Asking for the hints shows them again now, and not again at the next start
    Given I have never used MoneyBud
    When I start MoneyBud
    And I close MoneyBud and start it again
    Then the home screen should not show the hints
    When I ask for the hints to be shown again
    Then the home screen should show the hints
    When I close MoneyBud and start it again
    Then the home screen should not show the hints

  # Added in the build, after review (2026-09-30): the hints count as shown when the home screen
  # opens, and a start that is refused has no home screen. So a first start whose data could not be
  # read does not use them up.
  Scenario: A start that cannot read the data does not use up the hints
    Given MoneyBud could not read the data it kept, and I have since deleted it
    When I start MoneyBud
    Then the home screen should show the hints

  # ----------------------------------------------------------------------------------
  # Kept apart from the data
  #
  # The settings are not in the data file, so the file is the same for the phone and the desktop,
  # whatever is chosen. Each side survives what happens to the other.
  # ----------------------------------------------------------------------------------

  Scenario: Choosing how MoneyBud looks changes nothing in the data it keeps, and says nothing about saving
    Given I have a category "Groceries"
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I choose the theme "Kintsugi"
    And I choose the appearance "Donker"
    Then the data MoneyBud keeps should be exactly as it was before I chose how MoneyBud looks
    And nothing should have been said about saving

  # Were a choice written to the data file, it would fail here, and MoneyBud would say that my
  # changes are not saved.
  Scenario: Choosing how MoneyBud looks while saving is not possible does not say that my changes are not saved
    Given I have a category "Groceries"
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    And saving is not possible
    When I choose the theme "Kintsugi"
    And I choose the appearance "Licht"
    Then MoneyBud should not show that my changes are not saved
    And I should not have been told anything

  # Deleting the data is the one way to start over (start-moneybud.feature). It starts the money
  # over, with the six default categories, and leaves the phone's settings as they were: the hints
  # were shown the first time, and are not shown again.
  Scenario: Deleting the data MoneyBud kept leaves the theme, the appearance and the hints as they were
    Given the last time I used MoneyBud, I chose the theme "Kintsugi" and the appearance "Licht"
    And I used MoneyBud, recorded an income of 1832.45 euro labelled "Salaris" in it, and have since deleted the data it kept
    When I start MoneyBud
    Then the categories offered for a new expense should be exactly these, in any order:
      | category      |
      | Boodschappen  |
      | Huur          |
      | Hobby         |
      | Sparen        |
      | Verzekeringen |
      | Abonnementen  |
    And no incomes should be listed in the current budget period
    And the theme should be "Kintsugi"
    And the appearance should be "Licht"
    And the home screen should not show the hints

  # Settings are about the phone, not the money: when they cannot be read, MoneyBud starts as a first
  # start would look, and opens the data exactly as usual, without a word. Compare data it cannot
  # read, which it refuses (start-moneybud.feature).
  Scenario Outline: Settings MoneyBud cannot read are passed over without a word, and the data opens as usual
    Given I have a category "Groceries"
    And I have recorded an expense of 32.15 euro for "Groceries" labelled "Albert Heijn" dated today
    When I choose the theme "Kintsugi"
    And I choose the appearance "Donker"
    And I close MoneyBud
    And the settings MoneyBud kept <problem>
    And I start MoneyBud
    Then I should not have been told anything
    And the Overview should show the current budget period
    And the expenses listed in the current budget period should be exactly these, in this order:
      | date  | category  | label        | amount |
      | today | Groceries | Albert Heijn | 32.15  |
    And the theme should be "Standaard"
    And the appearance should be "Systeem"
    And the home screen should show the hints

    Examples:
      | problem                                   |
      | become damaged                            |
      | become blank, with nothing at all in them |
      | are gone                                  |

  # The settings cannot vouch for the data. The refusal is start-moneybud.feature's, unchanged.
  Scenario Outline: Data MoneyBud cannot read is refused whatever theme and appearance were chosen
    Given the last time I used MoneyBud, I chose the theme "Kintsugi" and the appearance "Donker"
    And MoneyBud has kept data that <problem>
    When I start MoneyBud
    Then I should be told that MoneyBud cannot read my data
    And MoneyBud should close without opening the Overview
    And the data MoneyBud could not read should be exactly as it was

    Examples:
      | problem                                    |
      | is damaged                                 |
      | was written by a newer version of MoneyBud |
