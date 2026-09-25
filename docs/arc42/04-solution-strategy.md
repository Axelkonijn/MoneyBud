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
| **UI toolkit** | Avalonia 12 with its Fluent theme, and CommunityToolkit.Mvvm for the view models. Chosen partly because it keeps the deferred mobile wish reachable | [ADR 0005](../decisions/0005-avalonia-ui-toolkit.md) |
| **Money** | A signed `Money` value type over a whole number of cents; sub-cent amounts refused rather than rounded; direction carried by the transaction type; no currency field | [ADR 0003](../decisions/0003-money-representation.md), [§8.2](08-crosscutting-concepts.md) |
| **Decomposition** | Three source projects: the domain; a **presentation layer with no UI toolkit**, holding everything the screen decides; and a thin Avalonia desktop. One specification project runs the scenarios against the first two, with no window | [ADR 0006](../decisions/0006-three-source-projects.md), which supersedes the "two projects" of [ADR 0004](../decisions/0004-solution-layout.md); [§5](05-building-block-view.md) |
| **Persistence** | None. State lives in memory for the lifetime of a run | [§8.3](08-crosscutting-concepts.md) |
| **Domain shape** | Purpose without location; the plan and the actual meeting in exactly one derived figure, *Remaining*; income forming a pool that belongs to neither layer, *Unassigned* | [§8.1](08-crosscutting-concepts.md), [§12](12-glossary.md) |

Read together, these say: **a domain library, a screen over it, and an executable specification
that reaches both.** For four increments the first and third were the whole of it. The fifth added
the screen and split it in two. What the screen *decides* sits in a layer the scenarios can run
without a window. What it *draws* sits in a thin toolkit project that nothing tests automatically.
A store and a second process are still deferred with a stated trigger rather than sketched
([§8.3](08-crosscutting-concepts.md)).

## How the quality goals fare

[§1.2](01-introduction-and-goals.md) ranks four quality goals. For four increments, the two
highest-ranked had no implementation at all, because they are about what the user sees and does.
The UI increment changed that for both, to different degrees. **None of the four is measured**:
[§10](10-quality-requirements.md) is still empty, because no measure has been agreed with the
stakeholder. "Served" below means that something is built for the goal. It does not mean the goal
has been shown to be met.

### 1. Legibility — **served, through the ring**

The Overview is the first thing built for this goal. Its ring puts each category's plan and
spending in one picture. A slice is sized to the *Budget* and filled as far as it has been spent, so
*Remaining* is the unfilled part of a slice rather than a figure to look up. *Unassigned* is a slice
of its own, so the ring is the period's income, the part without a purpose included
([§12](12-glossary.md), *The overview, and its ring*). Each row states in figures what its slice
draws: *Budget*, *Uitgegeven*, *Resterend*. An overspent category and an over-assigned period carry
a marker beside the negative figure.

The earlier groundwork is what makes this safe rather than merely present. *Remaining* is computed
in exactly one place ([§8.1](08-crosscutting-concepts.md)), and the ring and the rows both read it
from there. So the picture and the figures cannot disagree with each other or with the domain.

**What is not yet known** is whether it is legible *to the stakeholder*, which is what the goal is
about. That is the demo's question to answer. Only the stakeholder can answer it, and nothing here
claims otherwise.

### 2. Effortless entry — **partly served: entry exists, on desktop only**

Entry now happens through a screen, and several things were shaped to take work out of it:

- an entry's date defaults to today, and an assignment's period to the period on screen;
- the category box suggests the categories in use, narrowing as you type, and still accepts any
  name;
- after a refusal a form keeps what was typed, so it is corrected in place rather than retyped;
- an expense's label is optional, and a category with no budget records an expense like any other.

Three things hold it back, and all three are recorded rather than solved:

- **It is on the desktop, and entry happens out of the house**
  ([ADR 0002](../decisions/0002-desktop-application-first.md), [§11](11-risks-and-technical-debt.md)).
  This is the goal the desktop-first trade costs most.
- **A wrong entry cannot be corrected** except by closing MoneyBud and losing everything, which the
  stakeholder accepted for the demo ([§11](11-risks-and-technical-debt.md)).
- **Much of what is visible is still refusal**: five reasons an expense is refused, three for an
  income, four for an assignment, and now two more before any of them, for text that is not an
  amount or is ambiguous ([§12](12-glossary.md), *Typing an amount*). Each is there to stop a wrong
  record, and none of them makes entry easier.

The decisions that would serve it most belong to later increments: the account default, and the
one-action carry-over of last period's budgets ([§12](12-glossary.md)).

### 3. Adaptability — **genuinely served**

- **`MoneyBud.Domain` still depends on nothing but the base class library.** It has no UI toolkit,
  no storage library and no ambient clock. The UI increment's two packages went into the new projects
  instead ([ADR 0006](../decisions/0006-three-source-projects.md)).
- **The toolkit is at the edge.** Everything the screen decides is in `MoneyBud.Presentation`, which
  has no reference to Avalonia. Replacing the toolkit, or adding a mobile head, means a new window,
  and no scenario would change.
- **Refusals are reasons, not messages** ([§8.1](08-crosscutting-concepts.md)). All the Dutch is in
  one class, `Tekst`, so rewording anything the user reads touches neither the domain nor the
  window. A refusal reason added to the domain without Dutch wording is a compiler warning, and the
  build is kept at zero warnings ([§8.4](08-crosscutting-concepts.md)).
- **The scenarios are declarative** (`features/README.md`), and the screen scenarios run against a
  layer with no toolkit. So the specification survives being pointed at a different UI, which is
  the property [ADR 0002](../decisions/0002-desktop-application-first.md) relies on when it defers
  mobile.
- **`Money` has no division and no fractional multiplication** by deliberate omission
  ([ADR 0003](../decisions/0003-money-representation.md)), so the one rule that would be expensive
  to break is hard to break by accident. The ring's proportions are drawing shares, not amounts, and
  do not reach `Money` ([§8.2](08-crosscutting-concepts.md)).

The caveat stands: adaptability of the **software** is served; adaptability of the **data**, which
[§1.2](01-introduction-and-goals.md) names in the same breath, is untouched because there is no
stored data to adapt. The inability to correct an entry is that caveat as the user meets it.

### 4. Local operation — **served, and trivially so**

One process on one machine, no server, no network dependency, nothing written anywhere
([ADR 0002](../decisions/0002-desktop-application-first.md), [§7](07-deployment-view.md)). The UI
changed nothing here. This is the easiest of the four to satisfy today and the easiest to give away
later: desktop/mobile sync, deferred rather than rejected in [§3.2](03-context-and-scope.md), is the
thing that would put pressure on it. It should be an explicit trade when that arrives, not a drift.

## The honest summary

**All four goals now have something built for them, and none has been measured.** Legibility has its
first real delivery in the ring. Effortless entry has an entry screen, on the device where entry
happens least. Adaptability and local operation are served as before.

That is the right shape for a demo that exists to be reacted to ([§1.1](01-introduction-and-goals.md)).
The stakeholder's reaction is the measure that matters now. When he names what "at a glance" or
"no effort" should mean in practice, [§10](10-quality-requirements.md) is where it gets written.
