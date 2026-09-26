# A category is what money is earmarked for (glossary: Category). Adding one plans nothing and
# spends nothing: it creates a name that budgets can be set for and expenses recorded against.
# No amount moves anywhere in this capability, so no scenario below asserts one except to show
# that an existing category's figures were left alone.
#
# The name rule (arc42 §12, "A category name is compared case-insensitively and stored as typed,
# trimmed"). Storing and comparing are two different things:
#   - STORED: whitespace at either end is stripped and discarded; everything inside is kept
#     exactly as typed, capitalisation and spacing both.
#   - COMPARED: two names are the same name when they match after trimming the ends, counting
#     any run of spaces inside the name as one space, and ignoring case. So "Vaste  lasten",
#     with two spaces, is the same name as "Vaste lasten". Settled by the stakeholder on
#     2026-09-25.
# A name that trims to nothing is refused, because a category cannot do without a name.
#
# Adding a name has exactly one of four outcomes, and I am told which: it was CREATED, it was
# ALREADY THERE, it was BROUGHT BACK (an archived category — see archive-category.feature), or it
# was REFUSED because a category needs a name. Each scenario names the outcome it expects, so
# asserting one rules out the other three. Archiving is a separate act with an outcome of its
# own: I am told the category was ARCHIVED (archive-category.feature). What I am told is a fact
# about what happened, not a sentence: the wording belongs to a UI that does not exist yet.
#
# Reading the category steps:
#   - "the categories offered for a new expense" are the categories MoneyBud offers when I record
#     something new. An archived category is not among them.
#   - "should include" checks the name letter for letter, exactly as MoneyBud shows it.
#   - "should not include" means no spelling of that name is offered at all.
#   - "should list X once, and no other spelling of it" means exactly one offered category has
#     that name, compared the way MoneyBud compares names (ends trimmed, runs of inner spaces
#     counted as one, case ignored), and it is spelled exactly X, letter for letter and space
#     for space. This is the step that catches a second category, or a spelling that changed
#     underneath me.
#   - Where a name in a table needs spaces at its edges, the quotation marks are part of the
#     step (as in record-income.feature), so the name really does start or end with spaces.
#
# The default categories are Dutch because they are user-facing content, not test data
# (glossary: The default categories). Every other scenario names the categories it needs with an
# explicit Given, so none of them depends on whether the defaults are there — and a name I "do
# not have" is never one of the six.
#
# Out of scope: renaming a category (rename-a-category.feature), deleting one that has no history
# (delete-a-category.feature), assigning to one, and anything to do with accounts.

@budget
Feature: Add a category
  As someone budgeting my money
  I want to add categories of my own, starting from a sensible default set
  So that my categories fit what I actually spend on, rather than a list chosen for someone else

  # ----------------------------------------------------------------------------------
  # Adding a new name
  # ----------------------------------------------------------------------------------

  Scenario: Adding a name I do not have creates the category and offers it for a new expense
    Given I have no category called "Vaste lasten"
    When I add a category "Vaste lasten"
    Then I should be told that "Vaste lasten" was created
    And the categories offered for a new expense should include "Vaste lasten"

  Scenario: A category name loses the whitespace around it and keeps everything inside it
    Given I have no category called "Vaste lasten"
    When I add a category "  Vaste lasten  "
    Then I should be told that "Vaste lasten" was created
    And the categories offered for a new expense should list "Vaste lasten" once, and no other spelling of it

  # ----------------------------------------------------------------------------------
  # A name that says nothing is not a name
  #
  # Trimming is the rule and refusing blank is its consequence: something has to trim "   " in
  # order to judge it blank. This is the required income label's shape, not the optional
  # expense label's — an expense can do without a label, a category cannot do without a name.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A category name that trims to nothing is refused
    Given I have a category "Groceries"
    When I try to add a category <name>
    Then I should be told that a category needs a name
    And my categories should be unchanged

    Examples:
      | name  |
      | ""    |
      | " "   |
      | "   " |

  # ----------------------------------------------------------------------------------
  # Adding a name I already have
  #
  # Not a refusal, and not silent (glossary: "Adding a name you already have gives back the
  # category you already have"). The end state I asked for — a Boodschappen category — is
  # already true, so there is nothing to refuse; and saying so tells me it worked, and why the
  # capitalisation I see may not be the one I typed.
  #
  # The existing spelling is kept. Taking the new capitalisation would change a category's name
  # as a side-effect of adding one — a rename by the back door. Renaming has a front door of its
  # own since 2026-09-26 (rename-a-category.feature), and adding still never renames.
  #
  # The budget and the spending are there to show that what I get back is the category I
  # already had, figures and all, rather than a fresh one that happens to share its name.
  # ----------------------------------------------------------------------------------

  Scenario Outline: Adding a name I already have gives back the category I already have
    Given I have a budget of 300 euro for "Boodschappen" in the current budget period
    And I have already spent 120 euro on "Boodschappen" in the current budget period
    When I add a category <typed>
    Then I should be told that "Boodschappen" was already there
    And the categories offered for a new expense should list "Boodschappen" once, and no other spelling of it
    And the remaining "Boodschappen" budget in the current budget period should still be 180 euro

    Examples:
      | typed                |
      | "Boodschappen"       |
      | "boodschappen"       |
      | "BOODSCHAPPEN"       |
      | "  boodschappen  "   |

  # Spaces inside a name are ignored for comparison, and a run of them counts as one — so a
  # double space is the same name as a single one. A space nobody can see on screen would
  # otherwise make a second category that looks identical to the first, and silently split one
  # kind of spending across two of them: the failure the trimming rule exists to prevent,
  # reached from the inside of the name instead of its edges. Settled by the stakeholder on
  # 2026-09-25. The name is still STORED as typed, so what I get back is the category I already
  # had, spelled — and spaced — as it was.
  Scenario: A name that differs only in the spaces inside it is a name I already have
    Given I have a category "Vaste lasten"
    When I add a category "Vaste  lasten"
    Then I should be told that "Vaste lasten" was already there
    And the categories offered for a new expense should list "Vaste lasten" once, and no other spelling of it

  # ----------------------------------------------------------------------------------
  # The default categories
  #
  # Boodschappen, Huur, Hobby, Sparen, Verzekeringen, Abonnementen — the stakeholder's own list,
  # a starting set chosen to be tried rather than a claim about what a household needs. They are
  # ordinary categories: they can be archived like any other (archive-category.feature), and
  # adding one of their names behaves exactly as adding any name I already have.
  #
  # No order is implied: MoneyBud has no rule for ordering categories, so the list below is
  # compared as a set.
  #
  # They start with no history — nothing budgeted, nothing spent. That is what makes the
  # interview's own case ("voor als het niet voor jou geldt") cost nothing: a default that does
  # not apply to you has nothing behind it when you archive it. Sparen starts unbacked, like the
  # rest; it becomes account-backed when accounts exist, which is not this increment.
  # ----------------------------------------------------------------------------------

  Scenario: A new MoneyBud starts with the six default categories
    When I start using MoneyBud for the first time
    Then the categories offered for a new expense should be exactly these, in any order:
      | category      |
      | Boodschappen  |
      | Huur          |
      | Hobby         |
      | Sparen        |
      | Verzekeringen |
      | Abonnementen  |
    And no category should have a budget or any spending in the current budget period
