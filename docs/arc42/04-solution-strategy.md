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
| **Deployment form** | A desktop application: one process on the user's own machine, no server, no network. **Since 2026-09-29: an Android app on the stakeholder's phone, where MoneyBud is used**, with the desktop kept for development. Each has its own file, and nothing connects them. **Built 2026-09-30**, a signed APK not yet run on the phone | [ADR 0002](../decisions/0002-desktop-application-first.md), **superseded by [ADR 0013](../decisions/0013-an-android-phone-app.md)**; [§7](07-deployment-view.md) |
| **UI toolkit** | Avalonia 12 with its Fluent theme, and CommunityToolkit.Mvvm for the view models. Chosen partly because it keeps the deferred mobile wish reachable. **It draws the phone too** (2026-09-29) | [ADR 0005](../decisions/0005-avalonia-ui-toolkit.md), [ADR 0013](../decisions/0013-an-android-phone-app.md) |
| **Money** | A signed `Money` value type over a whole number of cents; sub-cent amounts refused rather than rounded; direction carried by the transaction type; no currency field | [ADR 0003](../decisions/0003-money-representation.md), [§8.2](08-crosscutting-concepts.md) |
| **Decomposition** | Four source projects: the domain; a **presentation layer with no UI toolkit**, holding everything the screen decides; a thin Avalonia desktop; and, since the persistence increment, a storage project that knows the domain and nothing else. One specification project runs the scenarios against all but the desktop, with no window. **The phone is a second thin head over the same three** (2026-09-29), every feature on both, so no rule is written twice. **Built 2026-09-30 as three more projects**: the phone's screens, an Android host kept out of the solution, and a runner for the phone's screens on the PC; the phone's navigation and settings went into the presentation layer, where the tests reach them | [ADR 0006](../decisions/0006-three-source-projects.md), which supersedes the "two projects" of [ADR 0004](../decisions/0004-solution-layout.md), is amended by [ADR 0007](../decisions/0007-keeping-the-ledger.md) and extended by [ADR 0013](../decisions/0013-an-android-phone-app.md); [§5](05-building-block-view.md) |
| **Persistence** | **One JSON file, `moneybud.json`, in the user's local application data**, written whole after every change that alters the ledger, through a temporary file and a rename, and held by one MoneyBud at a time. Amounts are whole cents, and categories are linked by a key that exists only in the file. Its own project, `MoneyBud.Storage`, behind a port in the domain. Settled with the stakeholder and built on 2026-09-26. **Since 2026-09-29: real use**, so every later version reads what the phone version accepted at the end review wrote; on the phone the file is in the app's own folder, under a fixed signing key. **Built 2026-09-30** with the same store and file format version 7; the promise starts with the version he accepts | [ADR 0007](../decisions/0007-keeping-the-ledger.md), in part superseded by [ADR 0014](../decisions/0014-real-use-and-the-phone-data.md); [§8.3](08-crosscutting-concepts.md), [§12](12-glossary.md) |
| **Domain shape** | Purpose without location; the plan and the actual meeting in exactly one derived figure, *Remaining*; income forming a pool that belongs to neither layer, *Unassigned*. **Since the accounts increment, location too**: every income and expense on an account, and a balance worked out from the entries, never stored, with a typed balance as a dated statement | [§8.1](08-crosscutting-concepts.md), [§12](12-glossary.md), [ADR 0008](../decisions/0008-balance-is-worked-out.md) |

Read together, these say: **a domain library, a screen over it, and an executable specification
that reaches both.** For four increments the first and third were the whole of it. The fifth added
the screen and split it in two. What the screen *decides* sits in a layer the scenarios can run
without a window. What it *draws* sits in a thin toolkit project that nothing tests automatically.
A second process is still deferred. A store is no longer deferred: the persistence increment added
one, in a project of its own. The domain defines what is kept and the port it is kept through. The
presentation layer decides when to save and what to say about it. The storage project only writes
and reads the file ([ADR 0007](../decisions/0007-keeping-the-ledger.md), [§8.3](08-crosscutting-concepts.md)).

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
- an expense's label is optional, and a category with no budget records an expense like any other;
- since the corrections increment, a wrong entry is fixed by clicking its row, in the same form and
  by the same rules it was entered with, and a mistyped category name can be renamed
  ([§12](12-glossary.md), *An entry can be changed or removed*);
- since the opening-a-period increment, a period with no plan is offered the latest earlier one,
  figure by figure in grey, and one press takes it over in full, so a stable month is re-planned
  without retyping it ([§12](12-glossary.md), *Opening a period*).

Two things hold it back, and both are recorded rather than solved:

- **It is on the desktop, and entry happens out of the house**
  ([ADR 0002](../decisions/0002-desktop-application-first.md), [§11](11-risks-and-technical-debt.md)).
  This is the goal the desktop-first trade costs most. **Settled on 2026-09-29**: MoneyBud
  moves to the stakeholder's phone, where entry happens, with everything the desktop does
  ([ADR 0013](../decisions/0013-an-android-phone-app.md)). The prototype showed that two swipes to an
  expense form are not too slow ([§12](12-glossary.md), *MoneyBud on the phone*). It is not measured.
  **Built on 2026-09-30, and not yet in his hand**: the phone app has every act, but has never run on
  the phone, so entry still happens only on the desktop until he installs it at the review. The heading
  above stays "on desktop only" until then.
- **Much of what is visible is still refusal**: five reasons an expense is refused, three for an
  income, four for an assignment, and now two more before any of them, for text that is not an
  amount or is ambiguous ([§12](12-glossary.md), *Typing an amount*). Each is there to stop a wrong
  record, and none of them makes entry easier.

The decision that would serve it most belongs to a later increment: the account default
([§12](12-glossary.md)). The other one this paragraph used to name, the one-action carry-over of
last period's budgets, is built (above). **The account default is built**, in the accounts
increment (2026-09-27): the account is the last field on the expense and income forms, a list
pre-filled with the pool account, so an entry on the usual account asks nothing more than it did
before accounts ([§12](12-glossary.md), *Every income and expense is on an account*). Its known weak
spot, a cash expense left on the pool account, is now live ([§11](11-risks-and-technical-debt.md)).

### 3. Adaptability — **genuinely served**

- **`MoneyBud.Domain` still depends on nothing but the base class library.** It has no UI toolkit,
  no storage library and no ambient clock. The UI increment's two packages went into the new projects
  instead ([ADR 0006](../decisions/0006-three-source-projects.md)). Keeping data added a port and a
  snapshot to the domain and no file handling. The file is in `MoneyBud.Storage`, which adds no
  package ([ADR 0007](../decisions/0007-keeping-the-ledger.md)).
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

The caveat is smaller, but it stands: adaptability of the **software** is served; adaptability of
the **data**, which [§1.2](01-introduction-and-goals.md) names in the same breath, was served only
within a run until the persistence increment. Since the corrections increment, an entry can be
changed or removed and a category renamed without starting over, which is where the user met the gap
first ([§11](11-risks-and-technical-debt.md), *Resolved*). Since the persistence increment, what
was adjusted is kept when MoneyBud closes ([§8.3](08-crosscutting-concepts.md)). What remains is
narrower: the kept data may not survive a new version until the switch to real use, by ruling
([§12](12-glossary.md), *Demo data may not survive a new version*). Recurring entries, the other
thing that goal names, do not exist yet. **Both have since changed**: recurring entries were built on
2026-09-28, and the switch to real use was ruled on 2026-09-29, to come with the phone version he accepts at that increment's end review,
from which every later version reads the data ([ADR 0014](../decisions/0014-real-use-and-the-phone-data.md)).
That closes the data half of the caveat, at the cost of every future change of format carrying a
reading path for good.

### 4. Local operation — **served, and trivially so**

One process on one machine, no server, no network dependency
([ADR 0002](../decisions/0002-desktop-application-first.md), [§7](07-deployment-view.md)). The UI
changed nothing here. Keeping data writes one file, and keeps it local: one set of data in the
user's **local** application data on that machine, deliberately not the roaming profile, protected
by the login, and backed up, if at all, by the user outside MoneyBud
([ADR 0007](../decisions/0007-keeping-the-ledger.md), [§8.3](08-crosscutting-concepts.md)). This is the easiest of the four to satisfy today and the easiest to give away
later: desktop/mobile sync, deferred rather than rejected in [§3.2](03-context-and-scope.md), is the
thing that would put pressure on it. It should be an explicit trade when that arrives, not a drift.
**Mobile arrived without it** (settled 2026-09-29): the phone keeps its own file and talks to nothing,
and the stakeholder moves the file by hand ([ADR 0013](../decisions/0013-an-android-phone-app.md)). The
one drift left was Android's own backup copying the phone's file to the cloud, and he ruled it off: the
app opts out ([ADR 0014](../decisions/0014-real-use-and-the-phone-data.md), [§3.3](03-context-and-scope.md)).
The phone's own lock is the protection, as the login is on the desktop.

## The honest summary

**All four goals now have something built for them, and none has been measured.** Legibility has its
first real delivery in the ring. Effortless entry has an entry screen, on the device where entry
happens least. Adaptability and local operation are served as before.

That is the right shape for a demo that exists to be reacted to ([§1.1](01-introduction-and-goals.md)).
The stakeholder's reaction is the measure that matters now. When he names what "at a glance" or
"no effort" should mean in practice, [§10](10-quality-requirements.md) is where it gets written.
