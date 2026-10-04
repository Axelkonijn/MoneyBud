# 0015 — A move of purpose is an entry of its own, and a category's backing is a history of stretches

**Status:** Accepted
**Date:** 2026-10-04
**Relates to:** [ADR 0009](0009-movements-are-entries.md), whose `Movement` is no longer the only entry
that moves money for a purpose, and whose single `Backing` per category becomes the last stretch of a
history. [ADR 0012](0012-the-calendar-is-a-history.md), whose decision 3 is changed in one point: what
is there for a category now counts its account's expenses from the period of backing, not from the
period of the latest re-pointing. [ADR 0007](0007-keeping-the-ledger.md), whose file format it takes to
**version 8**. [ADR 0014](0014-real-use-and-the-phone-data.md), whose promise this is the first record
to keep: version 7 is read. [ADR 0008](0008-balance-is-worked-out.md), whose rule *Vrij* follows: worked
out, never stored. Nothing any of them decided is reversed. 0007, 0009 and 0012 carry dated notes that
point here.

## Context

The rulings of 2026-10-04 ([§12](../arc42/12-glossary.md), *Vrij, and moving Opgebouwd*) came from a few
days of real use on the phone. Every account but the pool account shows ***Vrij***, the money on it that
no category claims. One new act, ***Verplaatsen***, moves an amount of purpose between an account's
*Vrij*, a backed category's *Opgebouwd* and the current period's *Niet toegewezen*, and money moves
between accounts only when the two ends are on different accounts. **Setting *Staat op* to "—" now sends
only this period's money back to the pool account**: older money stays on the account, still the
category's *Opgebouwd*, and setting an account again takes it along, so *Opgebouwd* carries on instead of
starting over. And version 7, the first promised version, must be read.

The model those rulings met could not hold two of them:

- **A move that changes no balance must still be kept.** €3.000 from *Vrij* to *Sparen*, both on
  Spaarrekening, moves no money. Yet *Vrij*, *Opgebouwd* and a history row all depend on it, and it must
  survive a restart. A `Movement` ([ADR 0009](0009-movements-are-entries.md)) is money moved for **one**
  category between two accounts: a move between two categories has two, and *Vrij* to *Niet toegewezen*
  has none.
- ***Opgebouwd* no longer starts over.** A `Backing` remembered when it began, and "—" deleted it.
  Now "—" leaves older money behind, still the category's, and an account set again carries on from
  there. One approved scenario also steps back after that, and expects each earlier period to show what
  it had built then.

And a third was a requirement on the shape: ***Vrij* depends on everything that reaches a balance**,
late receipts and balance corrections included, so a stored *Vrij* would go stale the way a stored
balance would.

These are requirements. How to hold them in code and on disk is architecture: it adds an entry kind,
changes what a backing is, and changes the file format under a promise to read the old one. This record
holds the answer. It was proposed as decision D1 of the increment's
[implementation plan](../plans/increment-15-vrij.md) and **approved by the stakeholder at the plan gate
on 2026-10-04**, D1 as proposed and all ten of the plan's readings as written. It was built the same
day. **The build changed one detail of D1**, and review at the build changed one point of ADR 0012's
decision 3. Both are marked below.

## Decision

### 1. A `Reallocation` is a sixth kind of entry

A `Reallocation` implements `IEntry` beside expenses, incomes, transfers, balance corrections and
movements, and **takes the next id from the shared counter**, so it meets balance corrections by
recording order, as every entry does ([ADR 0008](0008-balance-is-worked-out.md)). It holds:

- **its date**, always today, the day it is made;
- **its amount**, a positive magnitude, as every entry's is ([§8.2](../arc42/08-crosscutting-concepts.md)).
  A negative amount typed is **stored as the move the other way**;
- **its two ends**, each a `ReallocationEnd`: *Vrij* on an account, a category, or *Niet toegewezen*;
- **the account each end was on at that moment**, `FromAccount` and `ToAccount`: *Vrij* on its own
  account, a category on its backing account or on the account it left money on, and *Niet toegewezen*
  on the pool account. They are kept, not worked out again, because a category can be re-pointed and
  the pool account changed afterwards, and the row must stay in the histories it was written to.

**When those two accounts differ, the reallocation itself moves the money**, as a transfer does. No
separate `Movement` is written, so each touched history shows one row, as the scenarios ask. **When they
are the same, it moves no balance and still leaves a row** in that account's history (§12, follow-up 9).
A move into *Niet toegewezen* belongs to the period its date falls in, like an income, so a change of
start day carries it along. A reallocation is never changed or removed: it is undone by moving back.

`Ledger.Reallocate` settles first, as every act does, and returns a `ReallocateResult`: the move made,
nothing for zero, or one `ReallocationRefusal`. Ends that no list offers are a caller's mistake and throw
(plan reading 8).

### 2. A category's backing is a history of stretches

A category's backing is no longer one record that "—" deletes. It is a short history, of which only the
last stretch is live:

- **A backed stretch** is a `Backing`, as before, which gains **`Earlier`**: the "—" stretch before it,
  or null for a category backed for the first time, or backed again under version 7, which started
  *Opgebouwd* over.
- **A "—" stretch** is a new record, **`LeftBehind`**: the **`Account`** the money was left on; a mark,
  **`Since`**, drawn from the counter when "—" was set; **`From`**, the first day its period had then;
  the **`Amount`** left behind; and **`Before`**, the backing it ended.

So a backing points back through `Earlier` to the "—" before it, which points through `Before` to the
backing before that. **The live stretch is either a `Backing` on the category or a `LeftBehind`, never
both.**

***Opgebouwd* for a period is worked out by the stretch that period falls in.** A period before the
live backing's `AccumulatingFrom` is worked out by its `Earlier`; a period before a "—" stretch's `From`
by its `Before`. So stepping back shows each earlier period as it was built. While on "—", the amount
left behind is a fixed figure: only a move out of it changes it, a reallocation or *Restant bijwerken*
taking an over-sweep back (plan reading 2). Its expenses do not.

**Setting an account again writes a movement with a new reason, `Rebacked`**, that carries the money
left behind into the new stretch: from the old account to the new one, or from the account to itself
when it is the same one, which moves no balance and shows in no history but still carries *Opgebouwd*
on. **Re-pointing stays inside one stretch**, as before.

> **Changed in the build: the stretches are a chain, not a list.** D1 described "a history of
> stretches, oldest first", and a "—" stretch as an account, a mark, a first day and an amount. The
> build links each stretch to the one before it instead (`Backing.Earlier`, `LeftBehind.Before`), so a
> "—" stretch also holds the backing it ended. Working out an earlier period then walks back from the
> live stretch, and the file keeps each stretch where it belongs, nested in the one after it. The
> history it holds is the one D1 described.

### 3. *Vrij* is worked out, never stored

`Ledger.UnclaimedOf(account)` is **today's balance minus each category's claim on that account**:

- a backed category's claim is what is there for it, `ThereFor`;
- a "—" category's claim is the amount it left there, less what has been moved out of it since.

**The pool account has none** (§12, ruling 5), and `UnclaimedOf` returns null for it. Like a balance
([ADR 0008](0008-balance-is-worked-out.md)), *Vrij* is worked out on every read from the entries, so a
late receipt or a balance correction moves it without anything updating it.

`ThereFor` and `AccumulatedFor` count reallocations: into a category adds, out of it takes away. A move
between two accounts counts in `ThereFor` by which account it went to.

> **Changed at the build, after review: what is there for a category counts its account's expenses from
> the period of backing.** [ADR 0012](0012-the-calendar-is-a-history.md), decision 3, had `ThereFor`
> count from `HereFrom`, the first day of the period of the latest re-pointing. With the account list
> locked from the period a category got its account (§12, follow-up 16), an expense dated in an earlier
> backed period, entered after a re-pointing, goes on the **new** account. Counted from the re-pointing,
> it was in *Opgebouwd* and not in what that account holds, so *Vrij* went wrong and "—" moved the wrong
> amount. `spec-reviewer` found it. **`ThereFor` now counts the account's expenses from
> `AccumulatingFrom`**, and `PaidHereBefore` is remembered from there. Data from before version 8 is
> converted on reading (decision 4), so every figure reads as it did.

### 4. File format version 8

`LedgerJson` writes, beside everything version 7 has:

- a top-level **`reallocations`** list: id, date, the two ends (`{ "unclaimed": account }`,
  `{ "category": key }` or `{ "unassigned": true }`), `fromAccount`, `toAccount` and cents;
- on a category set to "—", **`leftBehind`**: `account`, the mark `since`, `from`, `cents`, and the
  backing it ended, `before`;
- on a backing, **`earlier`**, the "—" before it, in the same form;
- the movement reasons **`rebacked`** and **`adjusted`**; and **`backed`** and **`unbacked`** may now go
  either way, since an overspending moves the other way (§12, follow-up 15).

**Version 7 is read, and everything it already read** (versions 6, 5 and 4), as data in which nothing was
given a purpose, nothing moved between categories and nothing was left behind by "—" (§12, ruling 7).
**A version-7 document carrying any of the three is refused**, since that is not what version 7 wrote.
Versions 1 to 3 stay refused. Reading is silent, and the next save writes version 8.

**The reader hands the ledger no reallocation list at all for older data**, null rather than empty, so
`Ledger.FromSnapshot` knows the data came from before version 8. It uses that once: to convert a
backing's `PaidHereBefore` to count from the period of backing (decision 3, the note), adding the
expenses the account paid between that period and the period of the latest re-pointing, as they stand.
`FromSnapshot` also checks the new records: a reallocation's ends must be ends, not from *Niet
toegewezen*, not from an end to itself, and *Vrij* must be on the account it names; a category is not
both backed and on "—"; each stretch's marks are issued ids that are no entry's, and each stretch is
newer than the one before it.

## Why

### An entry of its own, because a move of purpose is not a movement

A movement says money moved for one category, in a direction that tells *Opgebouwd* what to do. A move
from *Sparen* to *Beleggen* changes two categories' *Opgebouwd*, and one from *Vrij* to *Niet
toegewezen* changes no category's. Bending `Movement` to hold both would give it optional categories
and a direction that means different things by case. As an entry of its own, sharing the counter, a
reallocation orders against balance corrections like everything else, and a balance is still the sum
of the entries.

### The reallocation moves the money itself, because the scenarios ask for one row

Writing a reallocation and a movement beside it for the money would put two rows in each history for
one act, and a second record that could disagree with the first. A transfer already moves money without
a movement, and a reallocation between two accounts is, as money, exactly that.

### The accounts at that moment are kept, because a category can move

A category's backing account today is not necessarily where it was when the move was made. Working the
accounts out again would move old rows between histories after a re-pointing, and change old balances.
Kept, they stay what they were, as a movement's `From` and `To` do.

### A history, because "—" no longer ends anything

The ruling makes "—" a pause rather than an end: money stays the category's, and an account set again
carries on. Old periods must still read what they built. Only a record of each stretch says which rule
worked a given period out, as the calendar's history does for periods ([ADR 0012](0012-the-calendar-is-a-history.md)).

### A carrying movement, because the money left behind has to be counted once

Setting an account again could have resumed the old backing's marks, or started afresh with an opening
figure. A movement that carries the amount left behind into the new stretch keeps the new stretch's
*Opgebouwd* the same formula as every backing's, with the old stretch closed, and puts a row in both
histories when money really moves.

### Worked out, because *Vrij* is a remainder

*Vrij* is whatever is on an account and not claimed. Everything that reaches a balance reaches it. A
stored *Vrij* would need updating by every one of those, which is the second author inside MoneyBud that
ADR 0008 rejected for balances.

### Rejected

| Rejected | Why |
|---|---|
| **A reallocation as one or two movements** | A movement is money moved for *one* category. A move between two categories has two, and *Vrij* to *Niet toegewezen* has none |
| **Setting an account again resumes the old backing's marks** | The old formula would count the expenses paid while on "—" twice. Their period's *Resterend* already counts them, and the follow-up says they do not touch the money left behind |
| **Setting an account again starts afresh, with the money left behind as an opening figure** | Every period before it would read zero, which the approved scenario *Backing a category again after setting it to none continues Accumulated* contradicts |
| **Store *Vrij*, or each account's claims** | A figure stored beside the entries it comes from, which a late receipt or a correction would leave stale |

The table is the plan's own, D1, as approved.

## Consequences

- **File format version 8**, reading version 7 and through it 6, 5 and 4 (decision 4). **The first
  reading path owed rather than free**, and the first test of [ADR 0014](0014-real-use-and-the-phone-data.md):
  `StorageTests` reads a version-7 document and refuses one carrying what version 8 added. The
  stakeholder left reading version 7 to the documentation and expects to start over anyway (§12, ruling
  7). It was kept because it was cheap.
- **ADR 0009 gets a dated note.** A movement is no longer the only entry that moves money for a purpose,
  and a category's backing is now a history, which "—" no longer deletes.
- **ADR 0012 gets a dated note.** Its decision 3 no longer holds for `ThereFor`, which counts from
  `AccumulatingFrom` (decision 3, the note). `HereFrom` is still kept, written and checked on loading.
- **Movement reasons gain two:**
  - **`Rebacked`**: setting an account again after "—", carrying the money left behind. In; or, when
    what was left behind is below zero, out.
  - **`Adjusted`**: an expense recorded before the category's current account became its account was
    changed or removed (§12, ruled at the scenario stage, 2; plan reading 4). The difference that makes
    between *Opgebouwd* and what is there for the category moves **between the account the expense is on
    and the backing account**. It goes `Along`, because it changes what is there and never *Opgebouwd*.
- **`Backed` and `Unbacked` may go either way.** An overspent category moves its overspending out on
  backing and in on "—" (§12, follow-up 15). `Repointed` moves a shortfall from the new account to the
  old one (§12, ruled at the scenario stage, 1).
- **`SetBackingResult` holds a list of moves**, `Moves`, rather than one: backing a category again after
  "—" can carry the money left behind along and move this period's *Resterend* off the pool account.
  `MovedBetweenAccounts` is what is said.
- **More counts as use.** A category named by any reallocation cannot be deleted, nor one whose
  *Opgebouwd* is not zero (§12, follow-up 10 and scenario-stage ruling 3). An account is used while a
  reallocation row is on it or a "—" category's money is left on it (plan reading 9). **An account that
  only a "—" with nothing left behind names can still be deleted**, as an account that backed a category
  and never had money moved is unused again. Deleting it forgets that stretch of the category's history,
  as backing again under version 7 started over. `spec-reviewer` found that deleting such an account cut
  away another stretch's money left behind too; now only the stretch naming the account is cut.
- **The figures still store nothing.** *Vrij*, *Opgebouwd* and what is there are worked out on every
  read. A unit test checks the invariant across 1500 mixed acts: on every account but the pool, *Vrij*
  plus the *Opgebouwd* of its categories is its balance.
- **[§8.2](../arc42/08-crosscutting-concepts.md) is not reopened.** A reallocation moves a typed amount,
  *Vrij* is a difference of whole cents, and the carrying and adjusting movements move sums and
  differences of them.
- **Reversal is no longer cheap.** Data is real since [ADR 0014](0014-real-use-and-the-phone-data.md),
  so a different shape would need its own reading path for version 8, kept for good.
