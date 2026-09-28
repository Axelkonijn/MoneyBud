# 0010 — A sweep is a movement for a period, and settling records each period's end

**Status:** Accepted
**Date:** 2026-09-28
**Relates to:** [ADR 0009](0009-movements-are-entries.md), whose expectation it carries out: "the
sweep will use the same step" as settling, at a period's end. Nothing 0009 decided is reversed. Money
MoneyBud moves is still a stored `Movement`, written on the day it moves and never changed, and a
balance is still the sum of its entries. It **narrows one consequence of 0009**, the accepted gap in
saving, for sweeps only (below). It takes [ADR 0007](0007-keeping-the-ledger.md)'s file format to
**version 4**, as 0007's consequences foresaw for any change of format. 0009 and 0007 carry dated
notes that point here.

> **Note, 2026-09-28, later the same day.** Recurring entries give settling a third writer and a new
> order ([ADR 0011](0011-recurring-entries.md)). Settling works **event by event**, so the occurrences
> dated in a period are recorded **before that period is swept**, and on a boundary day the boundary,
> as decision 2 describes it, comes first and that day's occurrences after it. Decision 2's three
> steps, and their order, are unchanged. **An occurrence is saved straight away, as a sweep is**, for
> the same reason, and the screen takes the occurrences made wherever it takes the sweeps made. A
> clock set wrongly ahead now reaches further still: it would record occurrences as well as sweep. The
> file format is version 5, and **version 4, this record's, is read** as data with no repeats. The body
> is unchanged.

## Context

The sweep's rulings ([§12](../arc42/12-glossary.md), *The sweep and Restant*) settle what happens to a
period's money when the period ends. Its *Restant* (the *period leftover*: *Unassigned* plus every
unbacked category's *Remaining*, netted, never below zero) moves from the pool account into one backed
category, the sweep destination. When a swept period changes later, the period shows the difference
and one button, *Restant bijwerken*, moves exactly that. MoneyBud never adjusts a sweep by itself.

ADR 0009 had already given the sweep its shape: a sweep is money MoneyBud moves for a category, so it
is a `Movement`, written by settling on the new period's first day. What 0009 could not give it is the
state the rulings ask to be **known later**. The ledger kept none of it:

- **Which categories were backed when a period ended.** A category counts in a period's *Restant* only
  if it was unbacked at that period's end, and the figure is worked out again months later, after
  every late entry (*What a period sweeps*, derived). The ledger kept only today's backing. Backing an
  overspent category moves nothing, so there is no movement to rebuild the backing from either.
- **What was swept for which period, into which category, in which order.** The ended period's line
  names what went to each category (scenario-stage ruling 5). Money swept too much comes back from the
  category it went into (ruling 5), latest move first (follow-up). And the difference is measured
  against what really moved (scenario-stage ruling 4). A sweep is dated in the *next* period, so its
  date does not say which period it was for.
- **What was let go.** Once a take-back is capped by what is there for the category, the line stops
  asking, **for good** (scenario-stage ruling 4). That cannot be worked out afresh, because what is
  there for a category grows again with every later assignment or sweep.
- **The destination**: one setting for the whole ledger (ruling 9).

§12's *For the plan* also listed **the day of the first start**, to tell periods swept by themselves
from earlier ones (ruling 7). The decision below makes it unnecessary.

These rulings are requirements. How to hold them in code and on disk is architecture: it decides what
exists as data, it touches the file format, and it is expensive to change once data is real. This
record holds the answer. It was proposed as decision D1 of the sweep increment's implementation plan
and **approved by the stakeholder at the plan gate on 2026-09-28**.

## Decision

### 1. A sweep is a `Movement` with reason `Swept`, and names the period it was for

`MovementReason` gains **`Swept`**, and `Movement` gains **`SweptFor`**: the first day of the period
the sweep was for, set exactly when the reason is `Swept`. The direction is **`In`** (pool account to
the destination's backing account) for the automatic sweep and for money still to sweep moved by the
button, and **`Out`** (a backing account back to the pool account) for money swept too much taken
back. A sweep never goes `Along`.

So balances, histories, *Opgebouwd* and "what is there for it" (`ThereFor`) all count sweeps with no
new rule, and "latest first" is the order of the ids.

### 2. Settling records each period's end, then sweeps it

When `Ledger.Settle` passes a period boundary it does three things, in order, for the period that
ended and the one that began:

1. writes a **period-end record** for the ended period: its first day and the set of categories
   backed at that moment;
2. if a destination is set and the period's difference is above zero, writes a `Swept` `In` movement,
   from the pool account to the destination's backing account, dated the new period's first day, and
   keeps it to be announced;
3. moves the new period's planned money, as ADR 0009 decided.

The old period ends before the new one's money moves. Both are dated that day and recorded before
anything typed on it, so their order changes no balance.

**What a period's *Restant* is worked out under** (`Ledger.PeriodLeftover`):

- a period with a record: the set in its record;
- a period with no record that ended before the ledger was first made: **nothing backed**. No settling
  ever passed its end, so it was never swept by itself, which is ruling 7. **So the first start needs
  no field of its own**: "ended before the first start" and "has no record" are the same thing;
- a period that has not ended, or whose end settling has not yet passed: **today's backing**, which is
  what its end will have unless something changes first.

### 3. A let-go amount per period

`Ledger.BringUpToDate` writes an amount **let go** for a period when it takes back less than was swept
too much: the part no category could give back. It adds to what was let go before. The line never asks
for it again.

### 4. One destination

The ledger holds **one sweep destination**: a category, or none. Only a backed category that is not
archived can be it. Unbacking, archiving or deleting it clears it. Re-pointing keeps it.

### 5. Every figure stays worked out

The *Restant*, the difference, the line and what can still be taken back are worked out on every
call, from the entries, the budgets, the period-end records and the let-go amounts. None is stored.
As built after the two rulings of 2026-09-28 (§12, *Sweep: ruled at the build*):

- **The difference** is `max(0, Restant) − swept`, where *swept* is what really moved for the period,
  net. Above zero it is still to sweep. Below zero it is swept too much **only beyond what was already
  let go**, so an amount let go absorbs a later rise in the *Restant* first.
- **Taking back is per move**, latest first, whichever category each went to. A move can be undone
  only while its category is still backed as it was when the money went in: backed now, and not
  unbacked and backed again since.

## Why

### A movement, because it already means the right thing everywhere

Every figure that reads movements needed a sweep to count exactly as the rulings say, and they already
did: the destination's *Opgebouwd* rises from the day swept money moves, unbacking returns swept money
with the rest of what is there for the category, a sweep from the pool account to itself changes no
balance and has no history row. A new entry kind would have had to be taught to each of them.

### The period it was for, because the date cannot say it

A sweep is dated the day it moves: the next period's first day, or the day the button is pressed. The
line, the take-back and the difference all ask "what moved *for this period*", and none of those dates
answers it.

### Only the backing at a period's end, because only that is ever asked

The rulings ask one question about past backing: was this category backed when *that* period ended?
Settling passes every period's end exactly once, and nothing can change while MoneyBud is closed, so
the set it sees is exactly what the end had (ADR 0009's own argument). Writing that set down answers
the question for good. A full backing history answers it for every day, where only the last day of a
period is ever asked.

### A let-go amount, because the cap cannot be worked out again

What a take-back is capped by, what is there for the category, grows again with later assignments. A
line that worked out afresh what could come back would start asking again, which ruling 4 forbids. So
what was let go is a fact about the moment of the take-back, and is kept as one.

### Rejected

| Rejected | Why |
|---|---|
| **A full backing history** per category (every back, re-point and unback, with its mark) | Answers the same question for every day, where only a period's last day is ever asked. More to store and to check on load, for nothing the rulings need |
| **Store each period's *Restant* when it is swept** | It is a figure, and a period never closes: the rulings need it worked out again after every late entry. Only the backing it was worked out under must be kept |
| **Record the let-go as a movement from an account to itself** | Keeps one list, but makes the line's "what really went" (ruling 4) subtract money that never moved |
| **A field for the first start** | Unneeded: a period that ended before the first start is exactly a period with no period-end record |

## Consequences

- **File format version 4.** `LedgerJson` writes, beside everything version 3 had:
  - `sweptFor` on each movement: a date, or null on every movement that is not a sweep; `reason`
    gains the word `"swept"`;
  - `sweepDestination`: a category key, or null;
  - `periodEnds`: for each period that ended while MoneyBud was in use, `periodStart` and `backed`, the
    keys of the categories backed then;
  - `letGo`: `periodStart` and cents.

  `Ledger.FromSnapshot` checks the new rules too: a sweep names a period that starts a period, and no
  other movement names one; the destination is backed and not archived; a period-end record is for a
  day that starts a period, once, and for a period that has ended by `settledThrough`, and names only
  categories that exist; a let-go amount is above zero, once per period, and for a period something
  was swept for.
- **Version 3 is refused as unreadable, like versions 1 and 2** (decision D2 of the plan, **approved by
  the stakeholder at the plan gate on 2026-09-28**). Version 3 has no period-end records. Reading it
  would mean guessing the backing at every past period's end, which is the one thing the rulings say
  must not be guessed. The alternative, today's backing standing in for every past end and no period
  counted as swept, was set out with it. It is still demo data, and MoneyBud meets the file as any
  unreadable file: it says it cannot read it, touches nothing and closes, and the user deletes the
  file (§12, *Demo data may not survive a new version*). The third time that ruling has been used.
- **A sweep that moved money is saved straight away**, on opening, on the minute's tick, and after any
  act, a refused act or one that changed nothing included (scenario-stage ruling 1). **This narrows
  ADR 0009's accepted gap for sweeps only.** 0009 accepts that money moved by settling straight after
  a period began may stay unsaved until the next change, because settling it again after a restart
  gives the same result invisibly. A sweep is announced, so settling it again would announce it
  twice. Planned-money moves keep the gap.
- **Settling now writes something at every period end**, a period-end record, even when nothing moves.
  It is still the one place MoneyBud acts because a period began, and still depends on nothing but
  `settledThrough` and the clock. **A clock set wrongly ahead reaches further**: it would record and
  sweep periods that have not ended ([§11](../arc42/11-risks-and-technical-debt.md)).
- **What counts as used widens for sweeps.** Any sweep keeps its category from being deleted, a sweep
  from the pool account to itself included (scenario-stage ruling 7), and, by the same reason, keeps
  its account from being deleted (§12, *Sweep: chosen in the build*). Other movements from an account
  to itself still do not count (ADR 0009). Deleting a category that can be deleted also takes it out
  of every period-end record.
- **A sweep's period is kept as its first day, and the screen names it with the default calendar.**
  That holds while the start day is fixed at the 1st. A configurable start day would have to name it
  through the ledger's own calendar, which is the start-day debt [§11](../arc42/11-risks-and-technical-debt.md)
  already carries.
- **Reversal is cheap while the data is demo data.** A different shape changes the domain's sweep code
  and the file version, and costs the user a fresh start.
