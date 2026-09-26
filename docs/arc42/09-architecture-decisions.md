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
| [0006](../decisions/0006-three-source-projects.md) | Three source projects: domain, presentation, desktop | Accepted; supersedes 0004's decision 1; **amended by 0007**. Dated notes, 2026-09-26: tests read the Desktop's markup, and a fourth project | 2026-09-25 |
| [0007](../decisions/0007-keeping-the-ledger.md) | Keeping the ledger: one JSON file in the user's profile, in a fourth project | Accepted; amends 0006 | 2026-09-26 |

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

Not every decision gets a record. **Persistence was deferred for six increments** — nothing stored,
state in memory for the lifetime of a run — and that was written up in
[§8.3](08-crosscutting-concepts.md) rather than here, because it was a scope decision rather than an
architectural one. It is noted in this section so that a reader scanning the index does not
conclude it was never decided.

**On 2026-09-26 the deferral ended, and still no record was written.** The stakeholder ruled what is
kept, when it is saved, where it lives, and what happens when it cannot be read. Those are
requirements, recorded in [§8.3](08-crosscutting-concepts.md) and [§12](12-glossary.md), *What
MoneyBud keeps*. When that was written, the architectural part had not been decided: the form
storage takes, how amounts and identity are stored, and where storage sits in the solution. They
were left to the persistence increment's plan, which is where a record might come from. One did
(next paragraph).

**The persistence increment added one: ADR 0007**, approved at its plan gate on 2026-09-26 and
built the same day. It answers all four of §8.3's questions for the plan. It chooses one JSON file,
written whole on every save, over SQLite. It puts that file in the user's local application data,
never relative to the working directory. It saves through a temporary file, a flush and a rename,
and allows one MoneyBud at a time through an exclusive lock claimed before loading. It keeps entry
ids and gives categories a key that exists only in the file. And it adds a fourth project,
`MoneyBud.Storage`, behind a port in the domain. Each of those is costly to reverse once data is
real, and several would draw a "why on earth" without their reasoning, which is this section's test
for a record.

**It amends ADR 0006 rather than superseding it.** 0006's decision was the split between domain,
presentation and desktop, and all three keep the roles it gave them. What changed is the count and
two reference lists. That is closer to 0006's own dated note of the first demo, which qualified a
consequence without reversing the decision, than to 0004's decision 1, which was replaced. So 0006
carries a second dated note that points to 0007, and its body is unchanged.

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

**The corrections increment added none** (2026-09-26), and its approved plan said so in advance. It
introduced no technology, moved no boundary and touched no money rule. The one change that looks
structural is that `Category` became a class with identity instead of a record, and that expenses
and incomes gained a ledger-issued id. That is how the domain expresses renaming and changing in
place, not a choice between architectures, so it is in [§8.1](08-crosscutting-concepts.md). Its
consequence for storage, that a category's name can no longer serve as its key, is carried in
[§8.3](08-crosscutting-concepts.md) for the persistence increment, where it may well need a record.
It became one of the questions that increment's plan had to settle, and ADR 0007 answers it: a
category gets a key that exists only in the file, and the domain gained no id.
