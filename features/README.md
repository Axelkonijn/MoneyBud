# Features

Gherkin feature files — the executable specification for MoneyBud.

These are the **contract**. An approved `.feature` file is what implementation is built against,
and the gate where human review happens. Nothing gets implemented before its scenarios are agreed.

## Conventions

**Write from the user's perspective, not the system's.** A scenario describes what someone wants
to achieve and what they observe — never which class or method is involved. If a scenario mentions
a repository, a service or a database, it is written at the wrong level.

**One `.feature` file per capability**, named after the capability: `record-expense.feature`,
`set-monthly-budget.feature`.

**Structure.** Every feature opens with the user story it serves:

```gherkin
Feature: Record an expense
  As someone tracking my spending
  I want to record what I spent and on what
  So that I can see where my money goes

  Scenario: Recording an expense reduces the remaining budget
    Given I have a monthly budget of 400 euro for "Groceries"
    And I have already spent 150 euro on "Groceries" this month
    When I record an expense of 25 euro for "Groceries"
    Then the remaining "Groceries" budget should be 225 euro
```

**Concrete numbers, not vague ones.** "a budget of 400 euro" beats "a budget". Money bugs hide in
rounding and edge cases, so scenarios should name exact amounts — and cover the awkward ones:
zero, negative, over-budget, month boundaries.

**Declarative over imperative.** `When I record an expense of 25 euro` — not a sequence of clicks
or field entries. Scenarios should survive a UI rewrite untouched.

**Tags** for grouping: `@budget`, `@wip`, `@slow`.

## Running them

Not yet — no test project exists. Reqnroll step definitions will live in the test project once the
solution is scaffolded. See [ADR 0001](../docs/decisions/0001-dotnet-and-reqnroll.md).
