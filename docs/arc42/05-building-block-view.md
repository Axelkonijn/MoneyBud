# 5. Building Block View

**What belongs here:** The static structure, decomposed in levels. Level 1 is the whitebox view of
the whole system — its top-level parts and their responsibilities. Deeper levels zoom into any
part that is complex enough to warrant it.

Only decompose where it earns its keep. A level-3 breakdown of a trivial component is noise.

---

MoneyBud is **one class library and one test project**. That is the whole structure, and it is a
deliberate choice rather than an unfinished one — the reasoning is in
[ADR 0004](../decisions/0004-solution-layout.md). This section is therefore short, for the same
reason [§7](07-deployment-view.md) is: a short section can be the right answer rather than a
missing one.

## Level 1 — Whitebox: MoneyBud

| Building block | Responsibility |
|---|---|
| **`src/MoneyBud.Domain`** | The whole application. Categories, budgets, expenses, budget periods and the `Money` type. Computes *Remaining* and decides what to refuse. Depends on nothing but the .NET base class library — no UI framework, no storage, not even an ambient clock (`TimeProvider` is passed in) |
| **`tests/MoneyBud.Specs`** | Runs the specification against the domain: Reqnroll step definitions on top of xUnit, with the scenario's world in `Support/`. Also holds the developer unit tests in `Unit/`, which are tests and not specification — see ADR 0004 for why they share a project and what rule keeps them subordinate |

`features/` is **not a building block.** It holds the Gherkin specification, and the specs project
*links* those files in rather than owning a copy of them (ADR 0004). It is listed here only because
a reader looking at the solution will see the linked files and wonder where they really live.

The solution file is `MoneyBud.slnx` at the repository root.

## There is no level 2

`MoneyBud.Domain` is a handful of types with no internal boundary worth drawing. A breakdown of it
would restate the class list, and [§8.1](08-crosscutting-concepts.md) already says the thing about
those types that is actually worth saying — how [§12](12-glossary.md)'s two distinctions are
expressed, and which of its concepts have no code at all. The code itself is the level-2 view.

## What is not here yet

Two building blocks are conspicuously absent, and both are absent for a recorded reason rather
than by omission:

| Absent | Why |
|---|---|
| **A user interface** | [ADR 0002](../decisions/0002-desktop-application-first.md) settles that MoneyBud is a desktop application but deliberately leaves the toolkit open, and no approved scenario needs a screen. Nothing in the domain anticipates one: refusals are reasons rather than messages precisely so that the UI can be added without the domain changing ([§8.1](08-crosscutting-concepts.md)) |
| **Anything that stores data** | [§8.3](08-crosscutting-concepts.md) defers persistence entirely — state lives in `Ledger` for the lifetime of a run and is gone afterwards — and states what will force the decision |

Adding either is the moment to re-examine whether two projects are still the right number
([ADR 0004](../decisions/0004-solution-layout.md)).

[§7](07-deployment-view.md) maps all of this onto one process on one machine, which no part of
this section changes.
