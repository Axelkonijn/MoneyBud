# 0004 — The layout of the solution: two projects, xUnit, linked feature files

**Status:** Accepted. **Decision 1, "two projects", superseded** by
[ADR 0006](0006-three-source-projects.md) on 2026-09-25, when the UI arrived. Decisions 2, 3 and 4
stand.
**Date:** 2026-09-24

> The record below is left as it was written. It predicted its own partial supersession ("if a UI
> or a store arrives and the domain starts accumulating things that are not domain, that is the
> signal"), and ADR 0006 cites that passage. Read what it says about two projects, empty layers and
> a very short §5 as true of the four increments before the UI.

## Context

Scaffolding the solution for the first increment forced four questions that
[ADR 0001](0001-dotnet-and-reqnroll.md) deliberately did not answer. It fixed .NET 10 and Reqnroll
and said only that "scenarios live in a Reqnroll test project", leaving open how many projects
there are, which test runner sits under Reqnroll, where the `.feature` files live relative to that
project, and where developer unit tests go.

All four were settled in the **approved implementation plan**. That plan was a conversation rather
than a file in the repository, so the reasoning currently exists nowhere — which is exactly what
the working agreement about writing reasoning down exists to prevent. This record is that reasoning,
written after the fact, from a solution that now builds clean and green.

**Why one record rather than four.** They are one question seen from four sides — what the solution
is made of — and three of the four follow from the first. None of them is big enough to carry a
record alone; together they answer what a reader opening the repository will actually ask.

**What is not decided here.** The feature files' *location* is not a choice: `CLAUDE.md` fixes
`features/` at the repository root. What is decided below is how a test project consumes them from
there.

## Decision

**1. Two projects.** `src/MoneyBud.Domain` — a class library holding the entire application — and
`tests/MoneyBud.Specs` — a Reqnroll test project. `MoneyBud.slnx` at the repository root ties them
together. No layered set, no application layer, no separate infrastructure or UI project.

**2. xUnit is the runner under Reqnroll**, via the `Reqnroll.xUnit` package.

**3. The `.feature` files stay in `features/` and are linked into the specs project**, not copied
and not moved. Reqnroll's generated code-behind is redirected into `obj/`.

**4. Developer unit tests live inside `MoneyBud.Specs`**, in a `Unit/` folder beside the step
definitions, rather than in a third project.

## Why

### Two projects, not a layered set

Nineteen scenarios do not earn an application layer.

The familiar four-project set — Domain, Application, Infrastructure, UI — exists to keep apart
things that entangle if they touch: persistence from domain rules, screens from use cases. Here
**three of those four projects would be empty.** There is no persistence
([§8.3](../arc42/08-crosscutting-concepts.md)) and no UI
([ADR 0002](0002-desktop-application-first.md) leaves the toolkit open). The fourth, Application,
would hold methods that forward to `Ledger` and do nothing else.

An empty project and a pass-through layer both **cost** quality goal 3, adaptability
([§1.2](../arc42/01-introduction-and-goals.md)), rather than buying it. They are structure a reader
has to learn before finding where anything happens, and every one of them is a place a future
change has to be threaded through.

The **boundary that actually matters is enforced anyway**: `MoneyBud.Domain` references nothing but
the base class library. That is the property a layered set is usually bought for, and it is held
here by a project reference list with nothing in it.

The rejected alternative is **scaffolding the layers now to avoid moving code later**. It was
declined on the same ground as the rest of the increment: nothing has been built into the wrong
place yet, and splitting a library of this size later is an afternoon's work against a solution
with no data in it and one consumer. If a UI or a store arrives and the domain starts accumulating
things that are not domain, that is the signal — and the project count is one of the cheapest
things in the repository to change.

### xUnit as the runner

Reqnroll ships plugins for xUnit, NUnit and MSTest and works with all three, so ADR 0001 genuinely
left this open. xUnit was chosen because:

- **It is the pairing with the least friction.** `Reqnroll.xUnit` is the most commonly used of the
  three plugins, so the documentation and the answers to problems are written against it.
- **Its isolation model already matches Reqnroll's.** xUnit constructs a fresh test class per test,
  which is the same lifetime Reqnroll gives a scenario's context. Nothing had to be configured to
  stop scenarios leaking into each other.
- **It also has to serve the unit tests.** `BudgetPeriodCalendarTests` runs one property over all
  31 possible start days; xUnit's `[Theory]` with `MemberData` expresses that directly.

The honest part: **this was not a hard choice and it is not expensive to reverse.** The feature
files are untouched by it, and the step definitions are runner-agnostic — only the unit tests carry
xUnit attributes. It is recorded because ADR 0001 explicitly left it open and because "why xUnit?"
is a question the repository would otherwise not answer.

### Feature files stay in `features/`, linked in

`CLAUDE.md` fixes the location: the feature files are **the specification, not test fixtures**, and
they are findable at the root of the repository without knowing anything about the solution layout.
What had to be decided is how the test project reaches them, and the options are copy, move, or
link.

- **Copying** was rejected outright. Two copies means the one that runs is not the one that gets
  edited, and the failure mode is silent: the specification changes and the tests carry on passing
  against the old text. A specification that can be edited without the tests noticing is not a
  specification.
- **Moving them into the project** was rejected because it contradicts the location `CLAUDE.md`
  fixes, and because burying the specification under `tests/MoneyBud.Specs/Features/` makes it look
  like test scaffolding, which is the opposite of what it is.
- **Linking** keeps one copy, in the place the specification belongs, and lets Reqnroll generate
  from it where it lies.

One consequence needed handling. Reqnroll writes its generated code-behind **beside the feature
file** by default, which would have put generated `.cs` files into `features/`. That directory
holds the specification and nothing else, so the generated output is redirected into `obj/` with
`ReqnrollUseIntermediateOutputPathForCodeBehind`.

### Unit tests in the specs project

There are two kinds of test in the repository and they do not have equal standing. The executed
scenarios are the **specification contract**; the unit tests on `Money` and `BudgetPeriodCalendar`
are developer tests, which exist because those two types have edge behaviour that is awkward to
reach through a scenario.

A third project, `MoneyBud.Domain.Tests`, would keep them apart at the cost of the two-project
layout decided above — a whole project to express a distinction that a folder name already carries
(`Steps/` against `Unit/`).

The distinction is worth keeping, but it is a **rule**, not a structure, and a project boundary
would not have enforced it either:

> **A unit test is never the reason a behaviour exists.** If a unit test asserts something no
> scenario covers, either a scenario is missing or the assertion is over-specified.

`BudgetPeriodCalendarTests` is the worked example of that rule being obeyed, and it has since shown
both halves of it. Most of it asserts only that periods tile time — no gaps, no overlaps — which is
true whatever the start day is. When this record was written it deliberately did **not** assert
which day a period starts on in a month too short to contain the configured start day, because that
was an open question and a passing test would quietly have turned a placeholder into a decision.

**That question was answered later the same day** — the start day clamps to the month's last day
([§12](../arc42/12-glossary.md)) — and the test file now pins the clamp, because it is a rule
rather than a placeholder. The rule above is what made the difference legible: the assertion was
absent while nothing had decided the behaviour, and appeared when something did. Nothing here
changes the decision this record is about; only the example moved on.

## Consequences

- **[§5](../arc42/05-building-block-view.md) is very short**, because this is the whole structure.
  Like [§7](../arc42/07-deployment-view.md), that is the right answer rather than a missing one.
- **`MoneyBud.Domain` referencing only the base class library is now a property worth protecting.**
  The first package reference added to it — a UI toolkit, a storage library, a serialiser — is the
  signal to revisit the project count, and should be treated as one rather than waved through.
- **`dotnet test MoneyBud.slnx` runs the specification and the unit tests together**, and there is
  no way to run only the specification without a filter. Accepted: nothing currently needs them
  run separately, and the totals are reported as one number.
- **The solution file is `MoneyBud.slnx`**, the .NET 10 SDK's XML solution format, rather than the
  older `.sln`. Nothing depends on this beyond tooling support for it.
- **Reversal is cheap, deliberately.** Splitting projects later moves files; it does not change
  behaviour, and no persisted data or public API depends on the current arrangement.
- **ADR 0001's "scenarios live in a Reqnroll test project" now has a concrete meaning**, and this
  record is where to look for it.
