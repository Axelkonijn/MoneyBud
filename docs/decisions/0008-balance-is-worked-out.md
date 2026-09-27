# 0008 — A balance is worked out; a typed balance is a dated statement

**Status:** Accepted
**Date:** 2026-09-27
**Relates to:** [ADR 0007](0007-keeping-the-ledger.md). It takes 0007's file format to **version 2**,
as 0007's consequences foresaw for any change of format. Nothing 0007 decided is reversed: one JSON
file, written whole, strict reading, keys that exist only in the file. 0007 carries a dated note that
points here.

> **Note, 2026-09-27, later the same day.** Backing arrived as this record expected, writing entries
> and not balances ([ADR 0009](0009-movements-are-entries.md)). A fifth entry kind, `Movement`,
> shares the id counter, and `BalanceOf` and `HistoryOf` include it. A backing's marks also draw ids
> from the counter without being entries. The rule for what a typed balance holds (decision 4) is
> now `EntryMark`, used for backing as well. The file format is version 3. Everything this record
> decided stands. The body is unchanged.

## Context

[§11](../arc42/11-risks-and-technical-debt.md) carried a risk for as long as accounts were planned:
**two mechanisms would write the same account balance.** MoneyBud would move money by itself, by
assigning to a backed category and by the sweep. Round 2 also let the user edit a balance freely by
hand ([round 2](../stakeholder/2026-09-24-verdieping.md): *"Je kunt het saldo zelf vrij
bijwerken"*). Nothing outside MoneyBud can say which of the two authors is right. §11 asked for this
to be decided **before** accounts were built, because the choice shapes how a balance is stored:

- is a hand edit an overwrite or an adjustment entry?
- is a balance stored, or derived from its transactions?
- are entered and calculated balances shown side by side?

The stakeholder answered the requirement on 2026-09-27, as ruling 2 of the accounts increment
([§12](../arc42/12-glossary.md), *A balance is worked out from the entries, never stored as a free
number*): **a balance is worked out from the entries, and a wrong one is put right by typing the real
balance, which MoneyBud records as a balance correction.** Ruling 3 then said what a typed balance
*means*: what the bank said that day, so every entry dated before it is already in it (*A typed
balance is what the bank said that day*). Follow-ups the same day settled the rest:

- an account may have no typed balance at all, as the first start's Betaalrekening has none;
- a balance correction shows its difference, recomputed as what is still unexplained;
- a changed entry keeps the moment it was first recorded.

Those rulings are requirements. How to hold them in code and on disk is architecture, because it
decides what exists as data, and it is expensive to change once data is real. This record holds that
answer. It was proposed in the accounts increment's implementation plan and **approved by the
stakeholder at the plan gate on 2026-09-27**.

## Decision

### 1. No balance is stored anywhere

Not in memory and not in the file. `Account` has a name and no balance. `Ledger.BalanceOf(account)`
works a balance out every time it is asked, and `Ledger.NetWorth` is the sum of those.

### 2. A typed balance is an entry with a date and an id

A starting balance and a balance correction are both a **`BalanceCorrection`**: an id, the date it
was typed (always today), the account, the balance typed, and whether it is the starting balance. It
holds the **balance itself**, never an adjustment.

### 3. All four entry kinds share one id counter, and the id is the recording order

Expenses, incomes, transfers and balance corrections all implement `IEntry` and draw their ids from
the ledger's one `lastEntryId`. So an id says which of two entries was recorded first, across kinds.
**A change keeps the id**, so a changed entry keeps its first recording moment.

### 4. What a typed balance holds

A typed balance **holds** an entry that is dated **before its day**, or dated **on its day with a
lower id** (`Ledger.Holds`). A date has no time of day, so on the day itself only the recording order
can tell.

### 5. How a balance and a difference are worked out

- **`BalanceOf`** is the account's **latest typed balance**, by date then id, plus every income,
  expense and transfer on the account that it does not hold and that is dated **today or earlier**.
  With no typed balance, it is the plain sum of those. "Today or earlier" keeps a future-dated income
  out of the balance until its date.
- **`DifferenceOf`** is worked out afresh each time. It is the typed balance minus the previous typed
  balance, or zero, plus everything this one holds that the previous did not. So it always shows what
  is still unexplained. A starting balance has none.

## Why

### Derived, because a derived balance has one author

A balance worked out from its entries can always be explained by them, and a mistake is put right
where it was made. An expense on the wrong account is one entry to re-point, not two balances to
unpick. It also matches the purpose side, where *Remaining*, *Unassigned* and whether a period has a
plan are all worked out rather than stored ([§8.1](../arc42/08-crosscutting-concepts.md)).

**When backing and the sweep arrive, they will write entries, not balances.** Transfers are "the same
kind of movement", in the stakeholder's own framing (§12, *Transfers*). So the automatic author and
the manual one will both add entries, and the balance stays a sum with one rule.

### A typed balance as a dated statement, not an adjustment

Storing the balance typed, rather than "+ € 23,40", is what makes ruling 3 hold. A receipt entered
late, dated before the balance correction, is already in it, so the checked balance stays checked.
Storing an adjustment would make every late receipt move a balance the user had just verified. It
would also freeze the difference at the moment of typing, which the stakeholder rejected: the
difference must shrink as forgotten entries are found.

### One counter, because the tie-break needs an order across kinds

"Recorded before the balance correction" compares an expense with a balance correction. Separate
counters per kind could not say which came first. `lastEntryId` already existed and was already kept
([ADR 0007](0007-keeping-the-ledger.md)), so sharing it cost nothing. Because a change keeps the id,
editing an entry cannot move it across a balance correction by accident.

### Rejected

| Rejected | Why |
|---|---|
| **A stored balance, overwritten by hand** | After an overwrite nothing explains the number. Ruling 2 |
| **Entered and calculated balances side by side** | The busiest screen, with two figures for every account. Ruling 2 |
| **A running balance kept in memory as a cache** | It would be a second author inside MoneyBud: a number that every act has to remember to update, and that can drift from the entries it summarises. That is the §11 risk again, in the code rather than between the user and MoneyBud |

## Consequences

- **File format version 2.** `LedgerJson` writes:
  - `accounts`: key and name. The key is the place in the order added, as for categories.
  - `poolAccount`: the pool account's key.
  - an `account` key on every expense and income.
  - `transfers`: id, cents, date, `from` and `to`.
  - `balanceCorrections`: id, date, account, cents and `starting`.

  No balance is written. **Version 1 is refused** as unreadable, and so is anything else that is not
  a whole version-2 document. That applies the stakeholder's ruling that data saved before accounts
  is not carried over (§12, *Saved data from before accounts*). `Ledger.FromSnapshot` re-checks the
  new rules, for example: an entry pointing at no account, a transfer from an account to itself, two
  starting balances on one account, and ids unique across all four kinds.
- **Nothing can disagree with a stored figure**, because there is none. The accepted cost, from
  ruling 3: **re-pointing or changing an entry dated before a balance correction does not move the
  corrected balance.** The balance correction already holds it. Only its difference changes.
- **Balances are recomputed on every read.** Every account, every history difference and net worth
  are worked out again at each redraw, once a minute and after every act. That is a scan of the
  entries per account, which is nothing at a household's scale. If it is ever noticed, a cache would
  reopen this record, since the cache is what was rejected.
- **`Ledger`'s constructor now takes the pool account's name.** There is always exactly one pool
  account, so an empty ledger cannot have none. `Ledger.StartNew` names it *Betaalrekening*, and the
  scenarios' empty ledger names it "Bank".
- **The history is a query, not a list.** `Ledger.HistoryOf` merges the four kinds on an account,
  newest first by date then id.
- **Reversal is cheap while the data is demo data.** A different shape changes one class and the
  file version, and costs the user a fresh start.
