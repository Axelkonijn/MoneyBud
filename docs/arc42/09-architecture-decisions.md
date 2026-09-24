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
| [0004](../decisions/0004-solution-layout.md) | The layout of the solution: two projects, xUnit, linked feature files | Accepted | 2026-09-24 |

**Records are superseded, not rewritten**, so that what we believed stays readable. ADR 0003 is the
one exception so far and says why in the record itself: its decision did not change, but one
statement it made about [§2](02-architecture-constraints.md) stopped being true hours after it was
accepted, with nothing built on it. It was corrected in place, with the corrected belief quoted
rather than deleted. That is the bar for amending rather than superseding, and it is meant to be a
hard one to clear.

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
