# 0011 — A recurring entry is state beside its occurrences, and settling works event by event

**Status:** Accepted
**Date:** 2026-09-28
**Relates to:** [ADR 0009](0009-movements-are-entries.md) and [ADR 0010](0010-sweeps-and-period-ends.md),
whose settling it extends. Nothing either decided is reversed. Money MoneyBud moves is still a stored
`Movement`, written on its day, and a period's end is still recorded and swept by settling, before
the new period's planned money moves. What changes is that **settling gains a third writer**, of
incomes and expenses, and **works through the days in order** instead of from boundary to boundary.
It **narrows 0009's accepted saving gap again**, for occurrences, as 0010 did for sweeps, and it
removes the "at most once a day" that 0009 stated. It takes [ADR 0007](0007-keeping-the-ledger.md)'s
file format to **version 5**, and for the first time **reads the version before**. 0007, 0009 and
0010 carry dated notes that point here.

## Context

The recurring-entries rulings ([§12](../arc42/12-glossary.md), *Recurring entries*) let an income or
an expense repeat, weekly or monthly, chosen in one drop-down. Each **occurrence** is an ordinary
entry, recorded by MoneyBud on its own date, the first time it runs on or after that date, said once
and saved straight away. **The latest occurrence sets the next**: changing it changes what follows,
and setting it to *Eenmalig* stops the repeat. Removing an occurrence removes only that one.

That much could almost be a field on an entry. The follow-ups and the scenario-stage rulings make it
need four things that no single entry can hold:

- **The day it was last set to.** A monthly repeat started on the 31st falls on 28 February and
  returns to 31 March (ruling 5). After 28 February no occurrence's date says "the 31st". Follow-up 3
  moves the day when, and only when, the latest occurrence's *date* is changed, so a change must tell
  a changed date from an unchanged one, and a date MoneyBud clamped must never set it.
- **The next date.** When the latest occurrence is removed, the one before takes over, and the next
  date does not move (follow-up 1). A day moved on the removed occurrence stays moved. So the next
  date must outlive the entry that set it.
- **Which entries are its occurrences.** An earlier occurrence opens with its drop-down locked on
  *Eenmalig* (follow-up 2), so that it cannot start a second repeat beside the running one. When the
  latest is removed, the most recently recorded one left becomes the latest. Both need to know which
  entries belong to which repeat.
- **Whether it was stopped.** A stopped repeat's earlier occurrences stay locked, and its last one
  stays changeable, which is how it is started again (scenario-stage ruling 1). Removing that last one
  leaves the repeat stopped. **So a stopped repeat is still a repeat**, not a set of plain entries.

And the settling rulings need an order. Follow-up 5 rules that settling works **through the days in
order, as if MoneyBud had been open**, so the occurrences dated in a period are recorded before that
period is swept. ADR 0009 and 0010 rest on settling seeing exactly what the day saw, because nothing
changes while MoneyBud is closed. Occurrences dated while it was closed are written later, so that
argument holds only if they are written in date order with the period ends between them. Follow-up 4
adds that a repeat set up in the past records what is already due **at once**, on days settling has
already passed.

These rulings are requirements. How to hold them in code and on disk is architecture: it decides what
exists as data, it touches the file format, and it changes the one place MoneyBud acts by itself. This
record holds the answer. It was proposed as decision D1 of the recurring-entries increment's
[implementation plan](../plans/increment-12-recurring.md), with the file-format question as D2, and
**approved by the stakeholder at the plan gate on 2026-09-28**: D1 as proposed, D2 as recommended.

## Decision

### 1. A recurring entry is state beside the entries

`RecurringEntry` (in `MoneyBud.Domain`, `Recurring.cs`, internal to the domain) holds:

- the **ids of its occurrences**, in the order recorded;
- its **frequency**, `Frequency.Weekly` or `Frequency.Monthly`, or **null once stopped**;
- the **day** a monthly one was last set to, 1 to 31, and null for a weekly or stopped one;
- the **next date**, null once stopped.

**The occurrences stay ordinary `Expense` and `Income` records, unchanged in shape.** Nothing that
works out a balance, a *Budget* figure, *Remaining*, *Unassigned* or the *Restant* needs to know
whether an entry repeats. One-off is **not** a third frequency: it is the absence of a repeat, `null`
wherever a `Frequency?` is taken, so an entry left at it is exactly the entry of every earlier
increment.

The day and the next date are set in **one place**: from a date and a frequency, when the repeat is
set up, when the latest occurrence's date or frequency is changed, and when a stopped repeat is
started again. The day becomes that date's day, and the next date is one step after it: 7 days for
weekly, the kept day in the following month for monthly, or that month's last day if it is too short.
Nothing else sets them, so a clamped date never moves the day.

### 2. The latest occurrence is the one with the highest id, and it is not stored

The id is the recording order, shared by every kind of entry ([ADR 0008](0008-balance-is-worked-out.md)),
and an occurrence always takes the next one. So the highest id among a repeat's occurrences is "the
one recorded most recently" by definition, which is the reading of "the latest" approved at the
scenario gate. Moving an earlier occurrence's date past the latest's does not hand the repeat to it.
When the latest is removed, the next-highest takes over **by itself**, with nothing to update.
Removing the last occurrence leaves the repeat with none, and it is dropped: the repeat ends
(follow-up 1).

### 3. A stopped repeat stays a repeat

Setting the latest occurrence to one-off clears the frequency, the day and the next date, and keeps
the occurrences. So its earlier occurrences stay locked, its last one can start it again, and removing
that last one makes the one before the stopped repeat's last, still stopped (scenario-stage ruling 1).
Which entry may change the drop-down is one rule, `Ledger.SetsTheRepeat`: a one-off, the latest
occurrence of a running repeat, or the last one of a stopped repeat. For any other entry the domain
**ignores** the frequency a change hands it, and the change is to that entry alone.

### 4. Settling works event by event

`Ledger.Settle` no longer steps from period boundary to period boundary. Each step takes whichever
comes first:

- **an occurrence**, when the earliest next date among the running repeats is due by today and falls
  **before** the next period boundary. It copies the latest occurrence's amount, label, category,
  account and kind, is dated the next date, takes the next id, is never checked and never refused,
  brings an archived category back (ruling 8), joins its repeat, and is kept to be announced
  (`TakeOccurrencesMade`). The repeat's next date moves on a step;
- **the next period boundary**, when it has begun: the ended period's period-end record, its sweep,
  and the new period's planned money, exactly as ADR 0010 decided.

So **on a boundary day the boundary comes first**, and that day's occurrences after it. **Several due
on one day are recorded in the order their repeats were set up.** An occurrence dated in a period is
always recorded before that period is swept.

**`Settle` loses its "at most once a day" early return.** A repeat set up in the past, started again,
or whose latest occurrence's date was moved back, has occurrences due on days already settled
through. An act that does any of those **settles again after its change**, so what is already due is
recorded within the act (follow-up 4), before any boundary not yet passed. A second call with nothing
due still does nothing.

### 5. An occurrence is saved straight away

Like a sweep ([ADR 0010](0010-sweeps-and-period-ends.md)): on opening, on the minute's tick, and after
any act, a refused one and one that changed nothing included (ruling 7). Wherever the screen takes the
sweeps settling made, it takes the occurrences too.

## Why

### State beside the entries, because no entry can hold it

Every one of the four needs in *Context* outlives, or reaches past, a single entry: the day survives a
clamped date, the next date survives a removal, the membership is about several entries, and a stopped
repeat must still be one. A record beside the entries holds exactly those and nothing else. Leaving
the entries unchanged in shape means every figure MoneyBud already works out stays correct for an
occurrence without being taught anything, which is ruling 2's "an ordinary entry" taken literally.

### The latest by id, because the id already says it

A stored "latest" would be a second source for what the ids already say, and the two could disagree
after a removal. The highest id is the recording order, which is the approved meaning, and it cannot
go stale.

### Event by event, because being closed must change nothing

The alternative orders both give a period's end a different answer depending on whether MoneyBud was
open. Sweeping first would leave every month-end the user was away with a *Restant bijwerken* to
press, for amounts MoneyBud recorded itself, which follow-up 5 rejected. Stepping through events in
date order keeps 0009's argument whole: settling late sees exactly what the day would have seen.

**The order within a boundary day changes no figure.** An occurrence dated a period's first day is in
that period whichever comes first, so it is never in the ended period's sweep, and everything settling
writes that day is recorded before anything typed on it (§12, follow-up 5, which left the order to the
plan). The plan put the boundary first, since a period ends as the next one's first day begins, and
set-up order for several occurrences on one day, since the ledger holds the repeats in that order.
Both are the smallest rule, not a requirement.

### Saved straight away, for the sweep's reason

An occurrence is announced. Left unsaved, a restart with no change in between would settle the kept
data again, record the same occurrences again, and announce them twice. Planned money settled again is
invisible, so it may keep 0009's gap; an occurrence may not.

### Rejected

| Rejected | Why |
|---|---|
| **A frequency field on the latest entry only** | Loses the day after a short month, and the next date once the latest is removed. Those are the two things `keep-data`'s "Repeats are kept" section checks |
| **Occurrences worked out, not stored**: a rule that shows an entry for every date without recording one | Against ruling 2: an occurrence is an ordinary entry, removed or changed on its own, and counted in balances and in a balance correction by the moment it was recorded |
| **The latest stored as its own field** | A second source for what the ids already say, and the two could disagree after a removal |
| **A count of steps since the day was set**, instead of the next date | The same information, less readable in the file, and the scenarios talk in dates |

## Consequences

- **File format version 5.** `LedgerJson` writes, beside everything version 4 had, a top-level
  `repeats` list. Each has `occurrences` (entry ids, ascending), `frequency` (`"weekly"`, `"monthly"`,
  or null once stopped), `day` (a number, or null) and `next` (a date, or null). The latest occurrence
  is not written. A frequency word this version does not know makes the file unreadable, as an
  unknown movement reason does.

  `Ledger.FromSnapshot` checks the new rules: every occurrence is an existing expense or income, and
  all of one repeat's are of one kind; no entry belongs to two repeats or twice to one, and none has
  no occurrences; a running one has a frequency and a next date, and a stopped one neither; a monthly
  one has a day from 1 to 31, and a weekly or stopped one has none.
- **Version 4 is read, not refused** (decision D2 of the plan, **approved by the stakeholder at the
  plan gate on 2026-09-28** on the recommendation). Version 4 has no repeats, and that is exactly true
  of the data: nothing could repeat when it was written. So reading it as version 5 with no repeats
  guesses nothing, unlike versions 1 to 3, where reading meant guessing something the rulings said
  must not be guessed. The cost was one branch in the reader and one unit test; the alternative was to
  refuse it as before, and the user deletes the file. **A version-4 document that has a `repeats`
  property is refused**, since it is not what version 4 wrote. **Versions 1 to 3 stay refused.** The
  next save writes version 5. It is the first time a MoneyBud reads an older version. That is not a
  promise to carry data across versions, which waits for the switch to real use (§12, *Demo data may
  not survive a new version*); it was chosen because here it was free.
- **Settling acts because a day came, not only because a period began.** It is still the one place
  MoneyBud acts by itself, and still depends on nothing but `settledThrough`, the repeats and the
  clock. It now writes incomes and expenses as well as movements.
- **An occurrence is never checked, so "a future expense is never recorded" rests on the clock.**
  Only an occurrence whose date has come is recorded, which keeps ruling 2 true without a check. **A
  clock set wrongly ahead** would record occurrences up to that date, future-dated expenses included,
  which recording by hand refuses, and move each repeat's next date past them for good
  ([§11](../arc42/11-risks-and-technical-debt.md)). A clock turned back records nothing and undoes
  nothing.
- **0009's accepted saving gap narrows again.** It no longer holds for a sweep (0010) or an occurrence.
  Planned-money moves keep it.
- **The notice has an order to keep.** Occurrences may be recorded by settling before an act and by
  the act itself. As ruled at the build (§12, *Recurring entries: chosen in the build*), the notice
  says them in the order they happened: what settling did in front of the act's sentence, what the act
  caused after it. The screen settles before the four entry acts to tell the two apart
  (`MoneyBudApp.SettleBeforeActing`). That is presentation, not this record's decision, and is noted
  here because it follows from the act settling again after its change (decision 4).
- **What counts as used is unchanged.** An occurrence is history like any entry, so its category and
  account cannot be deleted while it stands, and a repeat always has an occurrence.
- **Reversal is cheap while the data is demo data.** A different shape changes the domain's recurring
  code and the file version, and costs the user a fresh start, or none if the new version reads this
  one as this one reads version 4.
