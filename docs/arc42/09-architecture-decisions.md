# 9. Architecture Decisions

**What belongs here:** The important architectural decisions, with their reasoning. Kept as
individual records in [`docs/decisions/`](../decisions/) and indexed here.

What makes a decision belong here: it was hard to make, it is costly to reverse, or someone will
later ask "why on earth is it like this?".

---

| # | Decision | Status | Date |
|---|---|---|---|
| [0001](../decisions/0001-dotnet-and-reqnroll.md) | .NET 10 and Reqnroll for BDD | Accepted | 2026-09-24 |
| [0002](../decisions/0002-desktop-application-first.md) | The first version is a desktop application | Accepted | 2026-09-24 |
| [0003](../decisions/0003-money-representation.md) | How money is represented in code | Accepted, amended same day | 2026-09-24 |
| [0004](../decisions/0004-solution-layout.md) | The layout of the solution: two projects, xUnit, linked feature files | Accepted; **decision 1 superseded by 0006** | 2026-09-24 |
| [0005](../decisions/0005-avalonia-ui-toolkit.md) | The desktop UI toolkit is Avalonia | Accepted | 2026-09-25 |
| [0006](../decisions/0006-three-source-projects.md) | Three source projects: domain, presentation, desktop | Accepted; supersedes 0004's decision 1. Dated note, 2026-09-26: one test reads the Desktop's markup | 2026-09-25 |

**Records are superseded, not rewritten**, so that what we believed stays readable. ADR 0003 is the
one exception so far and says why in the record itself: its decision did not change, but one
statement it made about [§2](02-architecture-constraints.md) stopped being true hours after it was
accepted, with nothing built on it. It was corrected in place, with the corrected belief quoted
rather than deleted. That is the bar for amending rather than superseding, and it is meant to be a
hard one to clear.

**ADR 0004 is the first record superseded, and only in part.** Its decision 1, two projects, was
replaced by ADR 0006 when the UI arrived. Its other three decisions stand, so the record stays
**Accepted**, with a status line naming the part that no longer holds. Its body is unchanged. That
is not the ADR 0003 kind of amendment: a decision *did* change, so a new record carries it and the
old one keeps what was believed.

Not every decision gets a record. **Persistence is still deferred — nothing is stored, state lives
in memory for the lifetime of a run** — and that is written up in
[§8.3](08-crosscutting-concepts.md) rather than here, because it is a scope decision rather than an
architectural one. It is noted in this section so that a reader scanning the index does not
conclude it was never decided.

**The income increment added no record, and that is the expected outcome**, not an omission. It
introduced no technology, moved no boundary ([§5](05-building-block-view.md)) and reopened no money
rule — [ADR 0003](../decisions/0003-money-representation.md) already covered income, because its
rules were written about transactions rather than about expenses. The two decisions it did settle
are both about how one class expresses the model rather than about the architecture, so they are
recorded in [§8.1](08-crosscutting-concepts.md): why the figure is called `UnassignedIn` before
anything assigns, and why `IncomeRefusal` deliberately has no future-date member.

**The category increment added none either, for the same reason.** It introduced no technology,
moved no boundary and touched no money rule. What it settled in code is how the domain expresses
§12's category model: one ordinal name comparer, categories keyed by object rather than by string,
result types that make "never confirmed" true by signature, and an "is shown" query left out on
purpose. That belongs in [§8.1](08-crosscutting-concepts.md), and it is recorded there.

**The assigning increment added none either.** It introduced no technology, moved no boundary and
touched no money rule. `Assign` takes a `decimal` and converts to `Money` after validation, the
same boundary [ADR 0003](../decisions/0003-money-representation.md) set for recording a transaction
([§8.2](08-crosscutting-concepts.md)). What it settled is again how one class expresses
the model: `Assign` as the only writer of a *Budget*, with `SetBudget` deleted rather than aligned,
and the past-period setup in the specs made by moving the test clock rather than through a
test-only door into the domain. Both are recorded in [§8.1](08-crosscutting-concepts.md). Deleting a
scaffold is not a reversal of anything a record had decided, so there was nothing to supersede.

**The UI increment added two**, both approved at its plan gate on 2026-09-25. **ADR 0005** chooses
the toolkit that ADR 0002 left open on purpose: Avalonia, over WPF, .NET MAUI and WinUI 3.
**ADR 0006** is the re-examination of ADR 0004's layout that [§5](05-building-block-view.md) asked
for once a UI existed. The answer was three source projects, the middle one a toolkit-free
presentation layer where the new scenarios run. What the UI shows and does was settled first, as
requirements, in [§12](12-glossary.md), *The user interface*, and none of that depends on either
record.

**Several things the UI increment settled are not records**, for the same reason as the earlier
increments' were not: they are about how one layer expresses the requirements, not about the
architecture. How typed text becomes an amount, why display formatting ignores the machine's
culture, and how the scenarios reach the screen are in [§8.2](08-crosscutting-concepts.md) and
[§8.4](08-crosscutting-concepts.md).

**The first demo's rulings added none** (2026-09-26). They introduced no technology and moved no
boundary. One of them **qualifies** a consequence of ADR 0006 without reversing its decision: the
order of form fields can live only in the window's markup, so one test now reads that markup as
text, and the Desktop is no longer entirely untested. That was approved at the plan gate as a small
departure. It is recorded as a dated note on ADR 0006 and in [§8.4](08-crosscutting-concepts.md),
not as a superseding record, because the three-project split stands.
