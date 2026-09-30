# 9. Architecture Decisions

**What belongs here:** The important architectural decisions, with their reasoning. Kept as
individual records in [`docs/decisions/`](../decisions/) and indexed here.

What makes a decision belong here: it was hard to make, it is costly to reverse, or someone will
later ask "why on earth is it like this?".

---

| # | Decision | Status | Date |
|---|---|---|---|
| [0001](../decisions/0001-dotnet-and-reqnroll.md) | .NET 10 and Reqnroll for BDD | Accepted | 2026-09-24 |
| [0002](../decisions/0002-desktop-application-first.md) | The first version is a desktop application | **Superseded by 0013** (2026-09-29) | 2026-09-24 |
| [0003](../decisions/0003-money-representation.md) | How money is represented in code | Accepted, amended same day | 2026-09-24 |
| [0004](../decisions/0004-solution-layout.md) | The layout of the solution: two projects, xUnit, linked feature files | Accepted; **decision 1 superseded by 0006** | 2026-09-24 |
| [0005](../decisions/0005-avalonia-ui-toolkit.md) | The desktop UI toolkit is Avalonia | Accepted. Dated note, 2026-09-29: **the phone's toolkit too**, by 0013 | 2026-09-25 |
| [0006](../decisions/0006-three-source-projects.md) | Three source projects: domain, presentation, desktop | Accepted; supersedes 0004's decision 1; **amended by 0007**. Dated notes, 2026-09-26: tests read the Desktop's markup, and a fourth project. Dated note, 2026-09-29: **a second head, the phone**, by 0013 | 2026-09-25 |
| [0007](../decisions/0007-keeping-the-ledger.md) | Keeping the ledger: one JSON file in the user's profile, in a fourth project | Accepted; amends 0006. **Format version 2 since 0008, version 3 since 0009, version 4 since 0010, version 5 since 0011** (the first to read the version before), **version 7 since 0012**, by dated notes. Dated note, 2026-09-29: **the start day is stored**, as the calendar's history, and version 6, from the change to *Opgebouwd*, had no record. **Superseded in part by 0014** (2026-09-29): from the phone version accepted at the end review, "cannot read" is no longer an answer to an older version, and reversal is no longer cheap | 2026-09-26 |
| [0008](../decisions/0008-balance-is-worked-out.md) | A balance is worked out; a typed balance is a dated statement | Accepted; takes 0007's format to version 2. Dated note: a fifth entry kind since 0009 | 2026-09-27 |
| [0009](../decisions/0009-movements-are-entries.md) | Money moved for a category is a stored entry, written on the day it moves | Accepted; takes 0007's format to version 3. Dated notes, 2026-09-28: **its saving gap narrowed for sweeps by 0010, and for occurrences by 0011**; settling event by event and no longer at most once a day, by 0011 | 2026-09-27 |
| [0010](../decisions/0010-sweeps-and-period-ends.md) | A sweep is a movement for a period, and settling records each period's end | Accepted; takes 0007's format to version 4. Dated note, 2026-09-28: occurrences recorded before their period's sweep, by 0011. Dated note, 2026-09-29: a sweep's period named through the ledger's calendar, and a sweep made by a change of start day dated that day, by 0012 | 2026-09-28 |
| [0011](../decisions/0011-recurring-entries.md) | A recurring entry is state beside its occurrences, and settling works event by event | Accepted; takes 0007's format to version 5 and reads version 4 | 2026-09-28 |
| [0012](../decisions/0012-the-calendar-is-a-history.md) | The calendar is a history of start-day changes, and it is kept | Accepted; takes 0007's format to version 7 and reads versions 6, 5 and 4 | 2026-09-29 |
| [0013](../decisions/0013-an-android-phone-app.md) | MoneyBud is used on an Android phone, as a second head over the same projects | Accepted; **supersedes 0002**; extends 0005 and 0006 by dated notes. Built 2026-09-30, its decision 6 answered by the phone plan's D1 without a record of its own | 2026-09-29 |
| [0014](../decisions/0014-real-use-and-the-phone-data.md) | Real use: every later version reads the data, which on the phone lives in the app's own folder under one fixed signing key | Accepted; **supersedes 0007 in part** | 2026-09-29 |

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

**Opening a period added none**, as its approved plan said in advance. It introduced no technology,
moved no boundary ([§5](05-building-block-view.md)), touched no money rule and changed nothing that
is kept, so ADR 0007's file format stands at version 1. The one choice with any weight is that
taking a plan over calls `Assign` once per figure instead of writing budgets itself, which keeps one
writer of a *Budget*. That applies a rule [§8.1](08-crosscutting-concepts.md) already recorded,
rather than making a new one, and it is recorded there.

**The accounts increment added one: ADR 0008**, approved at its plan gate on 2026-09-27 and built the
same day. The stakeholder's rulings (§12, *Accounts and net worth*) are requirements, as persistence's
were: a balance is worked out and a hand edit is a balance correction. How to hold them in code and
on disk is not a requirement. It answers the question [§11](11-risks-and-technical-debt.md) asked to
have settled before accounts were built, because the choice "shapes how balances are stored". That
passes this section's test on both counts: costly to reverse once data is real, and sure to draw a
"why on earth" without its reasoning. **No balance is stored**, in memory or in the file. A typed
balance is a dated entry holding the balance itself. All four entry kinds share one id counter, which
is the recording order. A cache of balances was rejected as a second author inside MoneyBud.

**It takes ADR 0007's format to version 2, without superseding 0007.** 0007 decided the file, its
strictness and its keys, and all of that stands. It also said that a change of format is a new
version number, and that until real use an older file is simply unreadable. 0008 does exactly that,
so 0007 carries a dated note pointing to it, as 0006 does for 0007.

**The backing increment added one: ADR 0009**, approved at its plan gate on 2026-09-27 (decision D1
of the plan) and built the same day. The rulings (§12, *Backing and Accumulated*) make MoneyBud move
money by itself. Two of them decide the shape of the code: several amounts are fixed on their day,
and money planned for a later period has no destination until that period's first day. The answer
is that **every movement is a stored `Movement` entry, written on the day the money moves.** Money
for a later period is written by **settling**, which runs before every act, at start and on every
tick, with the backing and pool account of that day. The ledger keeps the day it has settled
through. The rejected alternatives were working every movement out from new histories of
assignments, backings and pool changes, and writing planned money at assigning time to today's
backing account. It passes this section's test on both counts. It touches the file format, so it is
costly to reverse once data is real. And the one place MoneyBud now acts when a period begins would
certainly draw a "why on earth" without its reasoning. It is the question
[§11](11-risks-and-technical-debt.md)'s balance-writing row had left for this plan.

**It carries out ADR 0008 rather than superseding it.** 0008 expected backing to "write entries, not
balances", and a balance is still their sum. What grew is the number of entry kinds sharing the
counter, now five, and the holding rule, now shared with backing as `EntryMark`. So 0008 carries a
dated note, and so does 0007 for version 3. **Version 2 is refused, not read**, against the plan's
recommendation (D2). The stakeholder left it to the build ("I dont mind starting over"), and one
format read is one way in fewer. That is recorded in 0009's consequences and in §12, not as a record
of its own, because it applies an existing ruling rather than making a new decision.

**The sweep increment added one: ADR 0010**, approved at its plan gate on 2026-09-28 (decision D1 of
the plan) and built the same day. ADR 0009 had already said the sweep would be a movement written by
settling. What it could not say was how to keep what the rulings (§12, *The sweep and Restant*) ask to
be known later: which categories were backed when a period ended, what was swept for which period, what
a line let go for good, and the destination. The answer is that **a sweep is a `Movement` with reason
`Swept` that names the period it was for**, that **settling writes a period-end record**, the set of
categories backed at that moment, before it sweeps, that a let-go amount is kept per period, and that
the destination is one setting. A period with no record ended before the first start, so the first
start needed no field of its own. The rejected alternatives were a full backing history, storing each
period's *Restant*, and recording the let-go as a movement from an account to itself. It passes this
section's test for the same reasons 0009 did: it touches the file format, and a period-end record
written even when nothing moves would draw a "why on earth" without its reasoning.

**It carries out ADR 0009 and narrows one of its consequences, rather than superseding it.** 0009's
decision stands: movements are entries written on their day. What changed is the gap 0009 accepted in
saving, which no longer holds for a sweep that moved money, because a sweep is announced and settling
it again after a restart would announce it twice (scenario-stage ruling 1). That is a consequence
narrowed by a later ruling, not a decision reversed, so 0009 carries a dated note, as does 0007 for
version 4. **Version 3 is refused, not read** (decision D2, approved at the plan gate as recommended):
it has no record of past period ends, and reading it would mean guessing them. As for version 2, that
is in the record's consequences rather than a record of its own.

**The recurring-entries increment added one: ADR 0011**, approved at its plan gate on 2026-09-28
(decision D1 of the plan) and built the same day. The rulings (§12, *Recurring entries*) need four
things no single entry can hold: the day a monthly repeat was last set to, the next date once the
latest occurrence is removed, which entries are a repeat's occurrences, and whether it was stopped.
And follow-up 5 needs settling to work through the days in order. The answer is that **a recurring
entry is state beside its occurrences**, which stay ordinary expenses and incomes; that **the latest
occurrence is the highest id and is not stored**; that **a stopped repeat stays a repeat**; that
**settling works event by event**, the boundary first on a boundary day; and that **an occurrence is
saved straight away**. The rejected alternatives were a frequency field on the latest entry only,
occurrences worked out rather than stored, the latest stored as a field, and a count of steps instead
of the next date. It passes this section's test as 0009 and 0010 did: it touches the file format, and
settling that no longer runs at most once a day would draw a "why on earth" without its reasoning.

**It extends ADR 0009 and 0010 rather than superseding either.** Their decisions stand: movements are
entries written on their day, and a period's end is recorded and swept by settling. What changed in
0009 is one sentence of decision 3, "at most once a day", and its saving gap, narrowed again; what
changed in 0010 is where its boundary sits among other events. Those are dated notes, as before, and
so is version 5 on 0007. **Version 4 is read, not refused** (decision D2, approved as recommended),
the first time any MoneyBud reads an older version: version 4 has no repeats because nothing could
repeat, so reading it guesses nothing. That is in the record's consequences, not a record of its own.

**The change to *Opgebouwd* of 2026-09-28 added none**, and took the file to version 6. It was built
on a lean route, without subagents, and it added two remembered amounts to a backing, reading version 5
by working them out again ([§12](12-glossary.md), *Backing a category that already has money*). That is
how one figure is held, not a new shape, so no record was owed. What it left undone was the dated note
on ADR 0007 that every earlier change of version got; ADR 0012's note on 0007 now names version 6 too.

**The start-day increment added one: ADR 0012**, approved at its plan gate on 2026-09-29 (decision D1
of the plan) and built the same day. The rulings (§12, *A configurable period start day*) keep every
earlier period's boundaries, move plans made ahead, date money a change moves the day of the change,
and never change *Opgebouwd*. One start day could no longer describe the periods, and two places worked
a period out again that must not be worked out again. The answer is that **the calendar is a history of
changes**, each saying which period it cut, from when the new day applied and the day; that **one act,
`Ledger.ChangeStartDay`, re-keys plans made ahead and, when it ends the current period, passes the
boundary itself, dated today**; that **a backing remembers the first day of the period each of its two
marks was set in**; and file format version 7. The rejected alternatives were the current day with every
period's first day beside it, every period's boundaries stored, the backing's period worked out from the
calendar as it was, the plan's own two-field change, and leaving the boundary to settling. It passes
this section's test as 0009 to 0011 did: it touches the file format, and a change that holds which
period it was made in, beside the day it applies from, would draw a "why on earth" without its
reasoning. **The build changed two details of D1**, a third field on a change and a second first day on
a backing, and the record says why, rather than leaving the plan's wording to be contradicted by the
code.

**It extends ADR 0007 and 0010 rather than superseding either.** 0007's consequence that the start day
is not in the file no longer holds, and 0010's that a sweep's period is named with the default
calendar holds only in its first half. Both were consequences written about a start day fixed at the
1st, which the rulings ended, not decisions reversed, so each carries a dated note. **Version 6 is
read, not refused** (decision D2, approved as recommended): it has no start day because every period in
it began on the 1st, so reading it guesses nothing. That is in the record's consequences, as version 4
was in 0011's.

**The mobile front-end added two, 0013 and 0014**, on 2026-09-29, and unlike every record since 0005
**they come from the stakeholder's rulings in stage 1, not from a plan**. He settled the platform, the
place of use, sync, parity, real use, where the data lives on the phone, its protection and its backup
himself, and those are
exactly the choices this section's test is about: costly to reverse, and sure to draw a "why on earth"
without their reasoning. So they are recorded now, ahead of this increment's scenarios and plan. What
they deliberately leave to the plan is the project layout for the phone. **The two approval gates are
waived for this increment**, by his ruling ([§12](12-glossary.md), *MoneyBud on the phone*), so no plan
gate will approve a record that the plan adds. He reviews the scenarios, the plan and the app together
at the end, and any record the plan adds is reviewed with them.

**ADR 0013 supersedes ADR 0002 outright.** 0002's decision was the deployment form, and it named its
own expiry: the moment the stakeholder keeps data he would mind losing. He ruled that real use starts
with the phone, so the deployment form changed, and a changed decision gets a new record, as 0004's
decision 1 did. 0002 is marked **Superseded**, with a note at its head, and its body is kept. 0013 also
reaches **ADR 0005**, whose toolkit now draws the phone, and **ADR 0006**, whose presentation layer now
has a second head, as 0006's own *Why* foresaw. Neither decision is reversed, so each carries a dated
note, as 0006 did for 0007.

**ADR 0014 supersedes ADR 0007 in part**, the second partial supersession after 0004. What changed is
not how the file is written but what it owes the future: from the phone version the stakeholder accepts
at the end review, every later version reads it. That ends 0007's consequence that an older file is simply unreadable until real use,
its "reversal is cheap" consequence, and the argument written for the time before real use. Its six
decisions stand. That is closer to 0004's case than to a dated note, because a stated part of the record
no longer holds, so 0007's status line names what is superseded and a note points to 0014. **ADRs 0008
to 0012 carry no note.** Each has a consequence "reversal is cheap while the data is demo data", which
ends with the accepted phone version by its own condition. Their decisions are untouched, and a note on
each would only repeat 0014.

**Three answers the stakeholder gave while these records were written went into 0014's body, not into
a note** (2026-09-29): the promise runs from the version he accepts at the review, not the first
installed; the phone's own lock is the protection; and the app opts out of Android's automatic backup.
0014 was written the same day and nothing had been built on it, so there was no earlier belief to keep.

**The phone increment's build added no record** (2026-09-30), and that is this section's judgement, not
an oversight. The choices that pass this section's test were already recorded: the second head, its
toolkit, the rule that it decides nothing, and themes as the one difference are ADR 0013's; where the
data lives, the fixed key and package name, the backup opt-out and the promise are ADR 0014's. **What
0013 left to the plan, the project layout (its decision 6), is recorded in the plan**, as decision D1
([the plan](../plans/increment-14-phone.md)): three new projects, with the Android host
outside `MoneyBud.slnx` because it needs the `android` workload and slows every build, and the phone's
screens in a project of their own that runs on the PC, with the reasons and the rejected alternatives
beside it. That is the kind of detail 0013 chose to leave out of a record, and [§5](05-building-block-view.md)
now carries it. **It is cheap to reverse**: moving the screens or the host between projects changes no
data and no rule, which is the other half of this section's test. The plan's other decisions carry out
0013 and 0014 rather than choosing between architectures: `PhoneScreen` is 0013 decision 4 applied, the
settings file is §12's ruling built, the package name and key's place are what 0014 left to the plan,
and **the file format stays at version 7**, so 0007 needs no dated note either. Had the build changed the
format, or put the phone's navigation in the head, a record or a note would have been owed. **None of it
was approved at a gate**, since the stakeholder waived both; if his review at the end overturns a plan
decision that a record would have held, that is the moment to write one.
