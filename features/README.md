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
    Given I have a budget of 400 euro for "Groceries" in the current budget period
    And I have already spent 150 euro on "Groceries" in the current budget period
    When I record an expense of 25 euro for "Groceries"
    Then the remaining "Groceries" budget in the current budget period should be 225 euro
```

**Name the budget period the same way every time** — `in the current budget period`, `in the
previous budget period`, `in the next budget period` — even in a scenario where only one period is
in play. Never "this month": the day a period starts is configurable and need not match a calendar
month (glossary: *Budget period*). One grammar means one step definition rather than several that
mean the same thing.

**Concrete numbers, not vague ones.** "a budget of 400 euro" beats "a budget". Money bugs hide in
rounding and edge cases, so scenarios should name exact amounts — and cover the awkward ones:
zero, negative, over-budget, month boundaries.

**Declarative over imperative.** `When I record an expense of 25 euro` — not a sequence of clicks
or field entries. Scenarios should survive a UI rewrite untouched.

**Tags** for grouping: `@budget`, `@wip`, `@slow`.

## Running them

```
dotnet test MoneyBud.slnx
```

Step definitions live in `tests/MoneyBud.Specs/Steps/`. **The feature files stay here** and are
linked into that project rather than copied into it, so there is one copy of the specification and
no way for a stale duplicate to keep passing — see
[ADR 0004](../docs/decisions/0004-solution-layout.md).

Five feature files exist so far — `record-expense.feature`, `record-income.feature`,
`add-category.feature`, `archive-category.feature` and `assign-to-category.feature`. Every
scenario in the first four passes; `assign-to-category.feature` is awaiting approval and is not
built yet.
