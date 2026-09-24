# 4. Solution Strategy

**What belongs here:** A short summary of the fundamental decisions that shape everything else —
technology choices, top-level decomposition, and how the quality goals from section 1 are actually
achieved. One page, not ten. The detail lives in sections 5–8 and in the ADRs; this is the
overview that makes them make sense together.

---

## The decisions that shape everything else

Each has its own record or section; this table is the map, not the reasoning.

| | What was decided | Where |
|---|---|---|
| **Technology** | .NET 10 / C#, with Reqnroll for the Gherkin scenarios | [ADR 0001](../decisions/0001-dotnet-and-reqnroll.md) |
| **Deployment form** | A desktop application: one process on the user's own machine, no server, no network | [ADR 0002](../decisions/0002-desktop-application-first.md), [§7](07-deployment-view.md) |
| **Money** | A signed `Money` value type over a whole number of cents; sub-cent amounts refused rather than rounded; direction carried by the transaction type; no currency field | [ADR 0003](../decisions/0003-money-representation.md), [§8.2](08-crosscutting-concepts.md) |
| **Decomposition** | One domain library and one specification project. No layered set, no application layer | [ADR 0004](../decisions/0004-solution-layout.md), [§5](05-building-block-view.md) |
| **Persistence** | None. State lives in memory for the lifetime of a run | [§8.3](08-crosscutting-concepts.md) |
| **Domain shape** | Purpose without location; the plan and the actual meeting in exactly one derived figure, *Remaining* | [§8.1](08-crosscutting-concepts.md), [§12](12-glossary.md) |

Read together, these say one thing: **the first increment is a domain library and its executable
specification, and nothing else.** Everything that would normally surround it — a UI, a store, a
process boundary — has been deferred with a stated trigger rather than sketched.

## How the quality goals fare

[§1.2](01-introduction-and-goals.md) ranks four quality goals. Two of them are not served by this
increment at all, and saying so is more useful than claiming otherwise.

### 1. Legibility — **not yet served**

There is no user interface, so nothing is shown to anyone. Nothing in the first increment delivers
this goal.

What exists is a **precondition**, and it should not be mistaken for a delivery: *Remaining* is
computed in exactly one place, from the plan minus the actual, so there can be no second figure
quietly disagreeing with the first ([§8.1](08-crosscutting-concepts.md)); *Over budget* is derived
from it rather than stored, so the two cannot drift apart. A single, consistent number is what a
legible display needs to have underneath it. It is not a legible display.

### 2. Effortless entry — **not yet served**

Same reason: entry happens through a UI, and there isn't one.

Two things were shaped with the goal in mind and cost nothing to note — the label is optional, and
a category with no budget set records an expense like any other, so nothing has to be set up before
spending can be recorded. Against that, the visible behaviour of this increment is mostly
*refusal*: five reasons an expense is rejected. Refusing bad input is not the same as making entry
effortless, and can easily be its opposite. The decisions that will actually serve this goal — the
account default, the one-action carry-over of last period's budgets ([§12](12-glossary.md)) —
belong to later increments.

### 3. Adaptability — **genuinely served**

This is the goal the increment's structure is actually built around.

- **`MoneyBud.Domain` depends on nothing but the base class library** — no UI toolkit, no storage
  library, not even an ambient clock. Adding any of those later is an addition rather than an
  untangling, which is what makes ADR 0004's two-project layout safe to keep.
- **Refusals are reasons, not messages** ([§8.1](08-crosscutting-concepts.md)), so re-wording
  anything the user reads never reaches the domain.
- **The scenarios are declarative** (`features/README.md`), so the specification survives being
  pointed at a different UI — the property [ADR 0002](../decisions/0002-desktop-application-first.md)
  relies on when it defers mobile.
- **`Money` has no division and no fractional multiplication** by deliberate omission
  ([ADR 0003](../decisions/0003-money-representation.md)), which makes the one rule that would be
  expensive to break hard to break by accident.

The caveat: adaptability of the **software** is served; adaptability of the **data**, which
[§1.2](01-introduction-and-goals.md) names in the same breath, is untouched because there is no
stored data to adapt.

### 4. Local operation — **served, and trivially so**

One process on one machine, no server, no network dependency, nothing written anywhere
([ADR 0002](../decisions/0002-desktop-application-first.md), [§7](07-deployment-view.md)). This is
the easiest of the four to satisfy today and the easiest to give away later: desktop/mobile sync,
deferred rather than rejected in [§3.2](03-context-and-scope.md), is the thing that would put
pressure on it. It should be an explicit trade when that arrives, not a drift.

## The honest summary

**Two of the four ranked goals have no implementation.** That is the expected shape of an increment
with no user interface, and it follows from what the first version is meant to be — a demo to
gather feedback on ([§1.1](01-introduction-and-goals.md)). But it means this section cannot yet
claim the architecture is meeting the goals it is ranked against. It is meeting two of them and
laying groundwork for the other two.

Revisit when a UI exists. That is also when [§10](10-quality-requirements.md) becomes writable:
goals 1 and 2 have no measurable scenario today because there is nothing to measure them on.
