# 0009 — Money moved for a category is a stored entry, written on the day it moves

**Status:** Accepted
**Date:** 2026-09-27
**Relates to:** [ADR 0008](0008-balance-is-worked-out.md), whose expectation it carries out: "when
backing and the sweep arrive, they will write entries, not balances". Nothing 0008 decided is
reversed. A balance is still worked out and never stored, and the entry kinds still share one id
counter, now five of them. It takes [ADR 0007](0007-keeping-the-ledger.md)'s file format to
**version 3**, as 0007's consequences foresaw for any change of format. 0007 carries a dated note
that points here.

> **Note, 2026-09-28.** The sweep arrived as this record expected, using settling at a period's end
> ([ADR 0010](0010-sweeps-and-period-ends.md)). A sweep is a `Movement` with a fifth reason,
> `Swept`, and names the period it was for. Settling now also records which categories were backed
> when each period ended, and sweeps that period, before it moves the new period's planned money.
> **One consequence below is narrowed**: the accepted gap in saving no longer holds for a sweep that
> moved money, which is saved straight away because it is announced. Planned-money moves keep the
> gap. Any sweep also counts as use of its category and its account, a sweep from an account to
> itself included, where other such movements do not. The file format is version 4, and version 3
> is refused. Everything else this record decided stands. The body is unchanged.

## Context

The backing increment's rulings ([§12](../arc42/12-glossary.md), *Backing and Accumulated*) make
MoneyBud move money by itself for the first time. Assigning to a backed category moves the amount
from the pool account to the category's backing account. Backing a category moves its unspent
*Remaining* for the current period. Unbacking returns what is there for it to the pool account, and
re-pointing takes that along to the new account. [§11](../arc42/11-risks-and-technical-debt.md) had
carried the question this raises, in its balance-writing row, since before accounts were built: how
does an automatic writer of balances fit a model where no balance is stored?

ADR 0008 had answered half of it: the automatic writer writes entries, and a balance stays their
sum. The rulings then fixed two facts that decide what those entries have to be:

- **Several amounts are fixed on their day.** What moves at backing is the *Remaining* at that
  moment, and what moves at unbacking and re-pointing is what is there for the category at that
  moment. A follow-up ruled that an expense dated earlier but recorded later must **not** change
  the amount moved at backing, and §12 derives the same for unbacking and re-pointing. So these
  amounts cannot be worked out afresh on every read. Whatever else is chosen, they must be stored.
- **Money planned for a later period has no destination until that period's first day.** It goes to
  whatever backs the category *then*, out of whatever is the pool account *then* (§12, *Planned money
  follows the backing on the day it moves*, and its derivation for the source). So it cannot be
  written as a finished record when it is assigned, unless that record is rewritten whenever the
  backing or the pool changes first.

Those rulings are requirements. How to hold them in code and on disk is architecture: it decides
what exists as data, it touches the file format, and it is expensive to change once data is real.
This record holds that answer. It was proposed as decision D1 of the backing increment's
implementation plan and **approved by the stakeholder at the plan gate on 2026-09-27**.

## Decision

### 1. Every movement is a stored `Movement` entry

A `Movement` has an **id**, a **date**, the **category** it was for, the account it came **from**
and the account it went **to**, and an **amount**, always a positive magnitude, as every entry's is
([§8.2](../arc42/08-crosscutting-concepts.md)). It also stores two things that cannot be read off
the accounts:

- a **reason** (`Assigned`, `Backed`, `Unbacked`, `Repointed`), for the history's wording;
- a **direction** (`In` towards the backing account, `Out` back to the pool, `Along` between two
  backing accounts), for *Accumulated*.

It implements `IEntry` with `Expense`, `Income`, `Transfer` and `BalanceCorrection`, and draws its
id from the same `lastEntryId`. So it orders against every other kind by recording order, as ADR
0008 requires of any entry a balance correction may hold.

### 2. Written on the day the money moves, and never changed

`Ledger.Assign` writes one when assigning to a backed category in the **current** period.
`Ledger.SetBacking` writes one when backing, re-pointing or unbacking moves anything. A movement is
never changed or removed afterwards. A negative assignment writes a new one going the other way.

### 3. Money planned for a later period is written by settling

`Ledger.Settle()` looks at every period that has begun since the day the ledger has settled
through, in order, up to today. For each backed category with a *Budget* above zero in such a
period, it writes an `In` movement from the pool account to the backing account, dated **that
period's first day**, using **the backing and the pool account of that moment**. Then it sets
`settledThrough` to today. The ledger keeps `settledThrough`, and the file saves it.

**Every act that changes the ledger calls `Settle` first.** The screen also calls it when it opens
and on every tick of the once-a-minute timer. It runs at most once a day, and a clock turned back
finds nothing to do.

### 4. A backing is a category's account and two marks

`Backing` holds the **account** and two `EntryMark`s, each a date and an id:

- **`AccumulatingSince`**, from which *Accumulated* counts, reset only when an unbacked category is
  backed;
- **`HereSince`**, from which what is there for the category in this account counts, reset on backing
  and on re-pointing.

A mark draws its id from the shared counter **without being an entry**, so it orders against an
expense recorded the same day. `EntryMark` is also the rule a balance correction uses to hold an
entry (ADR 0008, decision 4). That rule is now factored out once, and used for both.

### 5. The figures stay worked out

- **`BalanceOf`** adds movements on either side of the account, as it adds transfers.
- A movement **from an account to itself** changes no balance and has no history row. That happens
  when the pool account backs the category.
- **`ThereFor`** (what is there for a category in its backing account) and **`AccumulatedFor`**
  are worked out from the movements, the expenses and the budgets on every call, and neither is
  stored.

Only the amounts fixed on their day are stored, as movements.

## Why

### Stored, because the amounts would have to be stored anyway

The backing, unbacking and re-pointing amounts are fixed on their day by ruling. So some record of
each had to be kept whichever way movements were modelled. Once they are stored, storing assignment
movements the same way costs nothing extra, and it gives one shape for all of MoneyBud's own moves.
It is also the same shape as the user's own move, a `Transfer`: a dated entry between two accounts.
The stakeholder himself called a transfer "the same kind of movement" (§12, *Transfers*).

### Written when the money moves, not when it is planned

A movement written at assigning time, for November, would have to name today's backing account. If
the category were re-pointed or unbacked before 1 November, that record would be wrong, and
something would have to find and rewrite it. That is the second author inside MoneyBud that ADR 0008
rejected, just moved from a cached balance to a cached movement. **Writing it on its day means it is
never wrong when written, and never changed after.**

### Settling sees exactly what the day saw

Nothing can change the ledger while MoneyBud is closed, and every act settles before it does
anything. So when settling runs on 5 November for 1 November, the backing and the pool account are
exactly what they were on 1 November. Nobody could have changed them in between.

The same ordering puts the id right. A settled movement takes the next id, which is lower than
anything typed after it that day. So a balance correction typed on 1 November holds the movement:
the bank is taken to show it already. That is the derivation §12 records and the approved scenarios
assert (*Assigning to a backed category moves money*, derived: "on its own day, a movement orders
against a balance correction by recording order").

### Direction is stored, because the accounts cannot say it

When the pool account backs a category, money assigned moves from the pool to the pool. Whether
that adds to *Accumulated* or takes from it depends on what the movement was for, not on its
accounts. The same is true after the backing account is made the pool. So the direction is part of
the record.

### Rejected

| Rejected | Why |
|---|---|
| **Work every movement out** from a record of each assignment, each backing change and each change of pool account, keeping "worked out, never stored" | Three new histories to store and keep in step. The backing, unbacking and re-pointing amounts must be stored anyway, because they are fixed on their day. So it would be "worked out" in name only, with a harder rule behind every balance |
| **Write planned money when it is assigned**, to today's backing account | Breaks the ruling that planned money goes where the category is backed on its day. Re-pointing or unbacking before the period starts would have to rewrite it |

## Consequences

- **File format version 3.** `LedgerJson` writes:
  - `backing` on each category: null, or the account's key and the two marks
    (`accumulatingSince`, `hereSince`), each a date and an id;
  - `movements`: id, date, category, `from`, `to`, cents, and `reason` and `direction` as words;
  - `settledThrough`: the day planned money has been moved up to.

  `Ledger.FromSnapshot` checks the new rules too: a movement's direction must fit its reason, and a
  re-pointing must be between two accounts. A backing's marks must be ids that were issued and are
  no entry's. Ids must be unique across all five kinds of entry.
- **Version 2 is refused as unreadable, like version 1.** The plan recommended reading version 2:
  it has no backing and no movements, so reading it would mean "nothing backed, settled through
  today". The stakeholder answered: *"Chose what is best for you. I dont mind starting over"*.
  **The build chose not to read it.** Every format read is another way in that has to be kept
  correct, and nothing would be kept that he minds losing. So data saved by the accounts version is
  met like any other unreadable file: MoneyBud says it cannot read it, touches nothing, and closes,
  and the user deletes the file (§12, *Demo data may not survive a new version*). It is the second
  time that ruling has been used, after accounts.
- **MoneyBud now does something when a period begins**, where until now every period figure was
  worked out on request (§8.1, *carry-over*: "nothing acts when a period opens"). Settling is the
  one exception. It is kept as small as it can be: it writes only movements, it runs as a side effect
  of the next act, start or tick, and it depends on nothing but `settledThrough` and the clock.
  **The sweep will use the same step** at a period's end.
- **One gap is accepted.** An act that settles and is then refused, straight after a period began,
  has moved money that is not saved until the next change. Nothing is lost: the next start settles
  the kept data the same way, with the same result, because nothing could have changed in between
  (§12, *Backing: chosen in the build, not put to the stakeholder*).
- ***Accumulated* counts a period's budget as planned until that period is settled**, so the figure
  does not dip during the minute between a period beginning and the next tick.
- **The id counter now also issues marks.** Some ids name no entry: they belong to a backing change.
  No rule depends on ids being dense, so nothing is affected. `FromSnapshot` accepts an unused id
  only where a backing's mark claims it.
- **A category or account with a movement counts as used.** A movement between two different
  accounts stays in both accounts' histories and names its category, so the category cannot be
  deleted while such a movement stands, even once its budgets are back at zero. A movement from the
  pool account to itself is in no history and moves no balance: it does not block deleting and is
  removed with the category. An account with a movement between two different accounts on it, or
  backing any category, cannot be deleted either; by the same reason, movements from it to itself
  (made while it was the pool account and backed a category) do not count and go with it. As first built, any movement blocked deleting a category. `spec-reviewer` found
  that too broad, and the stakeholder ruled the narrower rule on 2026-09-27, rejecting erasing the
  rows with the category because balances could shift after the fact (§12, *Backing: ruled after
  the build*).
- **`ThereFor` counts movements by direction**, so when the pool account backs a category, the money
  assigned meanwhile is still there for it and goes along on re-pointing. The plan behind this record
  had said a pool-to-pool movement "adds nothing". The stakeholder ruled for the build on 2026-09-27,
  and that sentence of the plan is superseded (§12, *Backing: ruled after the build*).
- **Reversal is cheap while the data is demo data.** A different shape changes the domain's backing
  code and the file version, and costs the user a fresh start, which he has said he does not mind.
