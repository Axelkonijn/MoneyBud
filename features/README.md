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

**Every `When` acts through the screen**, not on the ledger: the steps drive `MoneyBudApp` in
`MoneyBud.Presentation`, the toolkit-free half of the UI, so the domain scenarios go through the
same doors the desktop window uses. `Given` steps set the ledger up directly, and `Then` steps
about what is *shown* read the screen — see [ADR 0006](../docs/decisions/0006-three-source-projects.md)
and arc42 §8.4.

**Thirty-seven feature files exist. All thirty-seven are approved and bound, and every scenario in
them passes**: 949 scenario cases, beside 638 developer unit tests, 1587 in all (2026-09-28). The
latest, the recurring-entries increment's two, were built with that increment.

The recurring-entries increment's files — `repeat-an-entry.feature` (22 scenarios, 29 cases) and
`change-a-repeat.feature` (25 scenarios, 28 cases), with a "Repeats are kept" section of 4 scenarios
added to `keep-data.feature`, 51 scenarios and 61 cases in all — were **approved at the scenario gate
on 2026-09-28**. They are bound since that increment was built, the same day, in `RecurringSteps.cs`,
with the frequency steps of an entry opened for changing in `CorrectionSteps.cs`. They are the first
files to name days by calendar date, since a monthly repeat keeps a day of the month;
`repeat-an-entry.feature` explains the steps the three share. What the build added to earlier steps,
none changing what an earlier scenario asserts: every record step, *Given* and *When*, may end in
", repeating monthly" or ", repeating weekly", and a *When* with that ending records through the form's
*Herhalen* list; "today is <calendar date>" makes a new empty ledger on that day and must come before
any other setup; a date phrase may be a calendar date, and an entry may be named by one ("the
expense labelled "X" dated 25 September 2026"); and the income and expense
list steps take an optional `repeats` column.

Before them, thirty-five feature files existed, with 888 scenario cases beside 583 developer unit
tests, 1471 in all (2026-09-28, at the close of the sweep increment).

The sweep increment's four files — `sweep-at-a-period-end.feature`, `show-an-ended-period.feature`,
`bring-a-swept-period-up-to-date.feature` and `choose-a-sweep-destination.feature` — were
**approved at the scenario gate on 2026-09-27**, with scenarios added to `start-moneybud.feature` and
`keep-data.feature`: 51 scenarios, 66 cases in all. They are bound since that increment was built, on
2026-09-28, in `SweepSteps.cs`. `sweep-at-a-period-end.feature` explains the steps the four share.
Three earlier steps were widened for them, none changing what an earlier scenario asserts: a backing
choice or a correction whose row is not on the ended period left on screen after a period boundary
steps forward to the current period first, as the user would; "I should not be warned or asked to
confirm" also holds when no act came before it, only time passing; and the notices of backing and of
deleting a category are checked to *contain* their sentence, since clearing the sweep destination
adds one of its own after it. "I close MoneyBud, and start it again on …" now opens MoneyBud over the
Givens first when no step has yet.

The backing increment's five files — `back-a-category.feature`, `assign-to-a-backed-category.feature`,
`spend-against-a-backed-category.feature`, `show-accumulated.feature` and `show-moved-money.feature`
— were **approved at the scenario gate on 2026-09-27**, with scenarios added to
`start-moneybud.feature` and `keep-data.feature` and header comments edited in five earlier files.
They are bound since that increment was built, the same day, in `BackingSteps.cs`. Two changes came
with the build, both approved by the stakeholder on 2026-09-27: one line of
`assign-to-a-backed-category.feature` now asserts that an archived category pulled back to zero is
not shown, instead of asking for the figure of a row no screen shows, and `show-accumulated.feature`
gained a scenario for his ruling that an archived backed category is shown while money is built up
for it.
`back-a-category.feature` explains the steps the five share. The increment added an `accumulated`
and an `accumulated marked` column to `overview.feature`'s categories-table step, an `accumulated`
column to `point-at-a-slice.feature`'s slice step, and the entry kind `movement` to
`show-accounts.feature`'s history step. That table's columns are now each checked on their own, so a
table may give the Budget without the rest. Recording an expense in a step now goes through the
expense form, so an account left unnamed is the one the form shows once the category is typed.

The accounts increment's six files — `add-an-account.feature`, `record-on-an-account.feature`,
`correct-a-balance.feature`, `transfer-between-accounts.feature`, `manage-accounts.feature` and
`show-accounts.feature` — were **approved at the scenario gate on 2026-09-27**, with one row added
to `add-an-account.feature` for the stakeholder's ruling at the gate (a starting balance of only
spaces is one left empty). They are bound since that increment was built, the same day. The same increment adds scenarios to
`start-moneybud.feature` and `keep-data.feature`, and edits header comments in six earlier files;
those were approved with them. `show-accounts.feature`
explains the steps the six share, including the one account, "Bank", that every scenario's empty
ledger now starts with.
The first demo feedback round's scenarios, all of `point-at-a-slice.feature` and the new
scenarios in `overview.feature`, were approved at the scenario gate on 2026-09-26.

The corrections increment's four files — `change-an-entry.feature`, `remove-an-entry.feature`,
`rename-a-category.feature` and `delete-a-category.feature` — were **approved at the scenario
gate on 2026-09-26** and are bound since that increment was built.

The persistence increment's three files — `keep-data.feature`, `start-moneybud.feature` and
`carry-on-when-saving-fails.feature` — were **approved at the scenario gate on 2026-09-26** and are
bound since that increment was built, in `KeepingSteps`. They keep data through the real file store,
in a temporary folder of each scenario's own, and so does every other scenario (arc42 §8.4). The
same increment edited one comment each in `overview.feature` and `step-between-periods.feature`,
approved with them.

The opening-a-period increment's file, `take-over-a-plan.feature`, was **approved at the scenario
gate on 2026-09-26**, with the stakeholder's rulings on the questions it raised applied: the
take-over asks for no confirmation, a one-cent plan in one period is what the next period is
offered, and while a plan is offered the rows are in order of their plan figures. Its steps are in
`TakeOverSteps.cs`, and a `plan` column was added to `overview.feature`'s categories-table step.

| Capability | File |
|---|---|
| Recording | `record-expense.feature`, `record-income.feature` |
| Correcting | `change-an-entry.feature`, `remove-an-entry.feature` |
| Categories | `add-category.feature`, `archive-category.feature`, `rename-a-category.feature`, `delete-a-category.feature` |
| Planning | `assign-to-category.feature`, `take-over-a-plan.feature` |
| The screen | `overview.feature`, `step-between-periods.feature`, `show-categories-in-a-period.feature`, `list-transactions-in-a-period.feature`, `suggest-categories.feature`, `point-at-a-slice.feature` |
| Accounts | `add-an-account.feature`, `record-on-an-account.feature`, `correct-a-balance.feature`, `transfer-between-accounts.feature`, `manage-accounts.feature`, `show-accounts.feature` — approved and bound 2026-09-27 |
| Backing | `back-a-category.feature`, `assign-to-a-backed-category.feature`, `spend-against-a-backed-category.feature`, `show-accumulated.feature`, `show-moved-money.feature` — approved and bound 2026-09-27 |
| The sweep | `sweep-at-a-period-end.feature`, `show-an-ended-period.feature`, `bring-a-swept-period-up-to-date.feature`, `choose-a-sweep-destination.feature` — approved 2026-09-27, bound 2026-09-28 |
| Recurring entries | `repeat-an-entry.feature`, `change-a-repeat.feature` — approved and bound 2026-09-28 |
| Keeping data | `keep-data.feature`, `start-moneybud.feature`, `carry-on-when-saving-fails.feature` |
| Typing | `type-an-amount.feature` — the file whose amounts are quoted text as typed, not numbers. `change-an-entry.feature` borrows that grammar for two outlines |
