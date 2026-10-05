# Increment 15 — *Vrij*, and moving *Opgebouwd*: implementation plan

**Status:** written 2026-10-04, **approved by Axel at the second gate on 2026-10-04**: D1 as proposed, all ten readings as written. **A section added 2026-10-05**, *After the install: Vrij on the pool account*, with both gates waived by him.

**What it builds against:** the approved scenarios, `show-unclaimed` and `reallocate-an-amount`, and the
revisions to eleven approved files (48 new scenarios, 72 cases; 23 revised, 1 removed). They were **approved
by Axel at the first gate on 2026-10-04**, together with every reading in their headers. It also builds
against §12 *Vrij, and moving Opgebouwd*: its eight rulings, the follow-ups, the four scenario-stage rulings
and *For the plan*. Read that section before starting. This plan does not restate it.

---

## One decision for Axel at this gate

### D1. A move of purpose is a new kind of entry, and a category's backing becomes a short history (a new ADR 0015)

The rulings ask for two things the model cannot hold today.

- **A move that changes no balance must still be kept.** €3.000 from *Vrij* to *Sparen* on Spaarrekening
  moves no money, yet *Vrij*, *Opgebouwd* and a history row all depend on it, and it must survive a restart
  (`keep-data`).
- ***Opgebouwd* no longer starts over.** Today a backing remembers when it began, and "—" deletes it. Now
  "—" leaves older money behind, still the category's, and setting an account again carries on from there.
  One scenario also steps back after that and expects the period before to show what was built then.

**Proposed:**

1. **A `Reallocation` is a new kind of entry**, beside expenses, incomes, transfers, corrections and
   movements. It takes the next id from the shared counter, so it meets balance corrections by recording
   order, like every entry. It holds a date (always today), an amount, and its two ends. An end is *Vrij*
   on an account, a category, or *Niet toegewezen*. It also holds **the account each end was on at that
   moment**: for a category, that is its backing account or the account it left money on. Only that keeps
   the row in the right history after a later re-pointing.
   - When those two accounts differ, **the reallocation itself moves the money**, as a transfer does. No
     separate movement is written, so each history shows one row, as the scenarios ask.
   - A negative amount is stored as the move the other way.
   - *Niet toegewezen* is counted in the period its date falls in, like an income, so a change of start
     day carries it along.
2. **A category's backing becomes a history of stretches**, oldest first, of which only the last is live:
   - **A backed stretch** is today's `Backing`.
   - **A "—" stretch** is new: the account the money was left on, a mark, its period's first day, and the
     amount left behind.

   *Opgebouwd* for a period is worked out by the stretch the period falls in. **Setting "—"** closes the
   backed stretch and opens a "—" stretch. **Setting an account again** opens a new backed stretch. It
   writes a movement that carries the money left behind into it: from the old account to the new one, or
   from the account to itself when it is the same one, which moves no balance and shows in no history. So
   *Opgebouwd* carries on, and stepping back still shows each earlier period as it was built.
   **Re-pointing** stays inside one stretch, as today.
3. ***Vrij* is worked out, never stored**, the way a balance is (ADR 0008). It is today's balance, minus
   what is there for each category whose money is on the account: a backed category's `ThereFor`, or the
   amount a "—" category left there. The pool account has none.
4. **File format version 8**:
   - a `reallocations` list;
   - a category's earlier stretches beside its `backing`, and a "—" stretch where it has one;
   - the movement directions the rulings add: an overspending moving the other way on backing and on "—",
     and the new carrying movement.

   **Version 7 is read** (ruling 7), as data with no reallocations and no stretches. So is everything older
   that version 7 already reads. Reading it is silent.

| Rejected | Why |
|---|---|
| **A reallocation as one or two movements** | A movement is money moved for *one* category. A move between two categories has two, and *Vrij* to *Niet toegewezen* has none |
| **Setting an account again resumes the old backing's marks** | The old formula would count the expenses paid while on "—" twice. Their period's *Resterend* already counts them, and the follow-up says they do not touch the money left behind |
| **Setting an account again starts afresh, with the money left behind as an opening figure** | Every period before it would read zero, which the approved scenario *Backing a category again after setting it to none continues Accumulated* contradicts |
| **Store *Vrij*, or each account's claims** | A figure stored beside the entries it comes from, which a late receipt or a correction would leave stale |

**Consequences:**

- ADR 0015, and ADR 0009 gets a dated note: a movement is no longer the only entry that moves money for a
  purpose.
- §5, §8.3 (the format) and §11 (the *Vrij* row) are updated, and §9 indexes the ADR.

---

## Point 16 is cheap, so it is built

§12 flags the lock that depends on the expense's date as low priority for you, to come back to you if it
proved costly. **It is not costly.** The lock is one comparison: is the expense dated on or after the first
day of the period the category got its account? The form already reads the date it would need. So the
scenarios are built as approved, and nothing goes back to you.

---

## Chosen in this plan, for Axel to see at the gate

These are the plan's readings where the rulings and the scenarios leave a detail open. Each is the smallest
rule the scenarios pass with. **Readings 1–4 are about money, so they are the ones to read closely.**

1. **What "—" returns is what is there for the category, less its *Opgebouwd* from before this period**:
   - Worked out, that is the money on the account, minus (*Opgebouwd* minus this period's *Resterend*).
   - Whenever every expense is on its account, this is exactly this period's *Resterend*, as ruled.
   - In kept version-7 data with an expense on another account, it is "this period's money still on the
     account", the glossary's own reading for that data. So the old account ends up holding exactly what is
     left behind.
   - What is left behind is *Opgebouwd* minus *Resterend*. It may be below zero, shown *Rood*.
2. **While on "—", the money left behind is a fixed amount.** Only moving it out changes it: a
   reallocation, or *Restant bijwerken* taking an over-sweep back. Its expenses do not, which is the
   follow-up as ruled. Nor does a receipt entered later but dated in an earlier, backed period: its money
   came off whatever account paid it, not off the money left behind. A "—" category whose amount reaches
   zero shows no *Opgebouwd*, and is otherwise simply unbacked. Its record is kept, so that setting an
   account later still carries on.
3. **Opened to be changed, an expense keeps its account, locked, when it was recorded before the category's
   current account became its account**:
   - This takes scenario-stage ruling 4 one step further: from "before the backing" to "before the backing
     or the latest re-pointing". An expense paid from Spaarrekening before *Sparen* was pointed at
     Aandelenrekening would otherwise move onto Aandelenrekening when it is saved unchanged, and then
     *Vrij* and *Opgebouwd* would part, on both accounts.
   - Every other expense against a backed category, dated from its backing period on, is locked on the
     backing account. That includes the kept version-7 *Fiets* in `keep-data`, which was recorded after the
     backing, so saving it moves it, as approved.
4. **The difference a changed old receipt makes** (scenario-stage ruling 2) is the change in *Opgebouwd*
   minus what is there for the category. It is only worked out for the receipts reading 3 keeps on their
   account. It moves **between the account the receipt is on and the backing account**. In the ruled case
   that receipt is on the pool account, so it is exactly the ruling. For a receipt paid from Contant, the
   money moves between Contant and the backing account rather than from the pool. One rule then covers
   that case and the re-pointing case in reading 3.
5. **The *Verplaatsen* lists.**
   - **Van** lists *Vrij* for each account but the pool, in the strip's order, as *"Vrij op Spaarrekening"*.
     Then come the categories it can take from (backed ones, archived ones among them, and "—" ones with
     money left behind), alphabetically, as every category list is.
   - **Naar** lists the same *Vrij* lines, then the backed categories that are not archived, then
     *Niet toegewezen* last.
   - *Niet toegewezen* means the period on screen, so in another period it is refused, as the scenarios
     read it.
   - **Desktop:** the form opens beside *Overboeken*, and from a *Verplaatsen* button on a backed row or a
     "—" row with money left behind. **Phone:** it opens from the accounts panel beside *Overboeken*, and
     from the category's ⋯ menu.
   - Opened from a row, it starts with that category as *Van*.

   > **Revised 2026-10-05** (*After the install*, below): *Vrij* is listed for **every** account, the pool
   > account first, on both sides; *Niet toegewezen* is last in *Van* too; and *Naar* opens on the first end
   > that can take from *Van* (P10).
6. **Copy**, all proposed:
   - **Strip:** *"Saldo € 5.200,00 · Vrij € 5.000,00"*. Below zero it carries the marker and *Rood*.
   - **Notice, no money moved:** *"€ 3.000,00 verplaatst van Vrij op Spaarrekening naar Sparen."*
   - **Notice, money moved:** the same, followed by *"Het geld gaat van Spaarrekening naar
     Aandelenrekening."*
   - **History row:** *"Verplaatst van Vrij naar Sparen"*.
   - **Row of a "—" category:** *"Opgebouwd € 5.000,00 op Spaarrekening"*.
   - **Refusals:**
     - *"Van en Naar zijn hetzelfde."*
     - *"Geld uit Niet toegewezen halen is toewijzen."*
     - *"Er kan geen geld naar "Sparen" verplaatst worden."*
     - *"Naar Niet toegewezen verplaatsen kan alleen in de huidige periode."* **Revised 2026-10-05**
       (P11): *"Verplaatsen met Niet toegewezen kan alleen in de huidige periode."*
     - The cent refusal is the existing one.
   - **The carrying movement's history row:** *"Sparen staat weer op "Aandelenrekening""*.
   - **Backing notices that move two amounts** list both, in the order they moved.
   - *Vrij* and *Verplaatsen* move from §12's proposed terms into the real display-terms table at the
     same time as `Tekst` gets them. *Van* and *Naar* are there already.
7. **The phone's *"van € … inkomen"* under the ring stays the period's income.** After a move into *Niet
   toegewezen* that figure can read more than it, *"€ 2.300,00 van € 2.000,00 inkomen"*, which is true.
8. **Ends the lists do not offer are caller mistakes, and throw**: *Vrij* on the pool account, a category
   with no account and nothing left behind, or *Niet toegewezen* as *Van*. **What a user can type is
   refused instead**, in the scenario-stage order: the same end on both sides, then finer than a cent, then
   a minus sign that would move money *into* an end that only gives, then *Niet toegewezen* outside the
   current period.

   > **Revised 2026-10-05** (*After the install*, below): *Vrij* on the pool account and *Niet toegewezen*
   > as *Van* are ends now, and no longer throw. A move out of *Niet toegewezen* to anything but the pool
   > account's *Vrij* is refused as a user situation.
9. **An account is used**, and cannot be deleted, while a reallocation row is on it or a "—" category's
   money is left on it. Both are history that explains its *Vrij*.
10. **A repeat's next occurrence against a backed category goes on the account backing it on its day**,
    when that day is in or after its backing period. Otherwise it copies the account, as now.

---

## Building it

**Domain.**
- `Reallocation`, its end type, `ReallocationRefusal` and a result type. `Ledger.Reallocate(amount, from,
  to)` settles first, and so may sweep, like every act.
- **Stretches.** A "—" record beside `Backing`, and the list of earlier stretches. `SetBacking` gives:
  - **backing:** this period's *Resterend* moves either way, so an overspending moves the other way
    (follow-up 15), and `NotMoved` is the *Budget* minus what moved;
  - **re-pointing:** a shortfall moves too (scenario-stage ruling 1);
  - **"—":** reading 1;
  - **an account set again after "—":** the carrying movement.

  `SetBackingResult` holds a list of moves rather than one.
- `ThereFor` counts reallocations and the carrying movement, and counts a move between accounts by the
  account it went to. `AccumulatedFor` works by stretch, counts reallocations, and returns the "—" amount
  for a "—" category (null when it is zero).
- `UnassignedIn` adds what was moved into it. `UnclaimedOf(account)` is *Vrij*. `LeftOn(category)` names
  the account a "—" category's money is on.
- **The lock**: `AccountFor(category, date, editing)` gives the locked account, or null when the list is
  open. Recording or changing onto another account throws. An expense left without an account goes on the
  locked account, and otherwise on the pool. Then ruling 2's difference, by reading 4. The repeat's
  occurrence follows reading 10.
- `CanDelete` refuses a category named by any reallocation, or whose *Opgebouwd* is not zero.
  `CanDeleteAccount` follows reading 9.
- `UndoableSweepsFor` reaches "—" categories, and every sweep since the oldest stretch. The amount taken
  back is capped by the money left behind.
- `ToSnapshot`/`FromSnapshot`, with the checks extended to the new records.

**Storage.** `LedgerJson` version 8, reading version 7 and, through it, everything older that it already
reads.

**Presentation.**
- `ReallocateForm` (*Van*, *Naar*, *Bedrag*): `StartFrom(category)`, and the lists by reading 5. Like the
  transfer form, a list that writes back a null is ignored.
- `MoneyBudApp.Reallocate` ends in `Tell`, as every act must.
- `AccountLine.Unclaimed`, and its marker.
- `CategoryRow.AccumulatedOn`, and the same on the slice's details.
- `HistoryKind.Reallocation`, read-only.
- `ExpenseForm`:
  - `IsAccountLocked`.
  - `ChosenAccount` reads the lock from the category *and the date*.
  - While locked, a write to the list is ignored, as the *Herhalen* list does. A pick made earlier is
    kept for when the list opens again.
- `Tekst` by reading 6.

**Desktop.** *Vrij* in the strip, the *Verplaatsen* button and form, the row button, the locked
`ComboBox`, and the "op …" line. As thin as ever: it decides nothing.

**Phone.** The same, in the accounts panel, the category's ⋯ menu, a modal like *Overboeken*'s, the
expense form's list and the category page.

**Tests.**
- **Specs:**
  - `ReallocateSteps.cs` for the new steps.
  - The *Unclaimed* and "accumulated on" columns in the existing table steps.
  - The lock steps ("locked" / "changeable", "should open on the account …").
  - The backing notice with a table of moves.
- **The two-version steps in `keep-data`:**
  - The Givens after *"kept by the version of MoneyBud from before Unclaimed"* run against today's ledger.
  - The step then writes the file as version 7 itself: it sets the version and leaves out what version 8
    added.
  - The one thing today's rules cannot record, a backed category's expense on another account, is recorded
    on the backing account and then moved to the account named, in that file. Balances are worked out from
    the entries, so that file is exactly what version 7 would have kept.
  - This is a simulation of old data, like increment 7's interrupted save, and needs no test-only door into
    the ledger.
- **Unit tests:**
  - Version 8 round trip, version 7 read, and broken version-8 data refused.
  - Every stretch transition, and *Opgebouwd* by period across them.
  - The lock by date, and by when the expense was recorded.
  - Reading 4's difference, on the pool account and on Contant.
  - The refusal order.
  - The list orders.
  - `Tekst` against §12's table.
  - An **invariant**: after a long run of mixed acts on today's rules, on every account but the pool,
    *Vrij* plus the *Opgebouwd* of its categories is its balance.

**After green:**
- `spec-reviewer`, and a few deliberate mutations.
- A headless run of the desktop window. The locked list's write-back must say and save nothing, and a
  *Verplaatsen* list rebuilt after an act must not pick an end by itself.
- The phone's snapshot pictures.
- arc42 §5, §6, §8, §9 (ADR 0015), §11, and §12's *Chosen in the build*; the README; CLAUDE.md.
- Then your try, on your phone, as an update that keeps your data. The new version reads your version-7
  file.

---

## After the install: *Vrij* on the pool account

**Status:** written and built 2026-10-05. **Both gates waived by Axel** for this change: the scenarios, this
section and the app are presented together, with every decision taken without him listed below.

**Why.** Installed on his phone, increment 15 showed no *Vrij* on the Betaalrekening (ruling 5), so the money
already on it could never be given a purpose: *Niet toegewezen* only ever holds income and what is moved into
it, never a starting balance or a correction. **Ruling 5 is revised** (2026-10-04, the round's *Na het
installeren op de telefoon*), and two more questions came up in building it, both answered by him on
2026-10-05 on the recommendation (the round's *Bij het bouwen*). Everything is in §12, *Vrij on the pool
account: ruled after the install*. This section does not restate it.

### What is built

1. ***Vrij* on the pool account is its balance less what is claimed there.** As on every account: what is
   there for each category it backs, and what a "—" category left on it. Plus two claims only the pool
   account has:
   - **The current period's**: its leftover so far (*Niet toegewezen* plus the *Resterend* of every category
     without an account, the figure the sweep would take), **less its income dated after today**, which
     counts in *Niet toegewezen* but is not on the account yet.
   - **The ended periods' lines** (his ruling, 2026-10-05): what each line still asks for, *still to sweep*
     as a claim and *swept too much* as a negative one, until *Restant bijwerken* moves it or lets it go.
   Worked out on every read, never stored, like every *Vrij* (ADR 0015, decision 3, with a dated note).
2. ***Niet toegewezen* gives to *Vrij* on the pool account** (his ruling, 2026-10-05), and to nothing else.
   It is now offered as *Van* too, last, as in *Naar*. *Niet toegewezen* counts a move out of it as it
   counts a move in. To a category it is still refused as assigning, and so is a move to *Vrij* on another
   account, with the same refusal. The refusal order is otherwise unchanged.
3. ***Vrij* on the pool account is an end on both sides**, like any account's. Nothing else in the act
   changes: money moves only when the two ends are on different accounts.
4. **No format change: still version 8.** Version 8 already keeps both ends and their accounts. Reading
   accepts a move out of *Niet toegewezen* only to *Vrij* on the account it was on, and refuses anything else
   as damaged data. Version 8 is not yet promised (ADR 0014: the promise starts with the version Axel
   accepts), but his phone's data is version 8 already, and this build reads it unchanged.
5. **Presentation and heads.** `Ledger.UnclaimedOf` returns a `Money`, no longer null; `AccountLine` always
   carries *Vrij*; the desktop strip and the phone's accounts panel show it on every account.
   `ReallocateForm.HasEnds` is gone: there are always two ends, so *Verplaatsen* is always offered.

### Chosen here, without Axel

| # | Choice | Why |
|---|---|---|
| P1 | **"This period" is the current period**, whatever is on screen | *Vrij* is today's, the same in every period, on every account |
| P2 | **Income dated later in the period is left out of the claim until its date** | CLAUDE.md asked that it not make *Vrij* dip; recording it, assigning it and its date arriving now leave *Vrij* where it was |
| P3 | **An income on another account stays in *Niet toegewezen*** (the known corner) | The pool's *Vrij* falls by it and that account's rises, until a transfer to the pool squares both. Summed over all accounts it is now right, where before the same euros were free twice |
| P4 | **An expense on a category without an account paid from another account** raises the pool's *Vrij* and lowers that account's | The general rule, on both accounts; a transfer from the pool account squares both |
| P5 | **A leftover below zero is not swept and nothing asks for it**, so the pool's *Vrij* falls by it at the period's end | During the period the overspending lowers the claim, not *Vrij*; at its end it shows it was paid from money with no purpose |
| P6 | **A capped negative assignment shows on the pool's *Vrij*** | Taking a backed category's *Budget* back moves at most what is there for it (increment 10); what it cannot bring back still joins *Niet toegewezen*. That gap was invisible before; now the pool's *Vrij* falls by it. Shown, not changed |
| P7 | **Making another account the pool moves the period's claim with it, and the ended periods' line claims too** | *Restant bijwerken* moves to and from whichever account is the pool then, so a line's claim left on the old one would make bringing it up to date move the pool's *Vrij*, against his ruling. The new pool's *Vrij* may go below zero (with no destination, by every unswept period) until the money is transferred; the old one's rises. The line claims were found by `spec-reviewer` |
| P8 | ***Niet toegewezen* to *Vrij* on another account is refused** as "out of *Niet toegewezen* is assigning" | It would move money between accounts with no purpose, which is *Overboeken*. A message of its own was not worth a new refusal |
| P9 | **No format version change** | Nothing new is stored, only a new valid pair of ends (point 4 above) |
| P10 | ***Naar* opens on the first end that can take from *Van***: not *Van*, and not a *Vrij* when *Van* is one | With a *Vrij* on every account the form would otherwise open on *Vrij* to *Vrij*, which is always refused. Seen in the phone's snapshot |
| P11 | **The refusal for another period's *Niet toegewezen* is reworded**: *"Verplaatsen met Niet toegewezen kan alleen in de huidige periode."* | It is now said for a move out of it too, where "Naar …" was wrong. The step reads "Unassigned can be used only in the current budget period" |

### Tests

- **Unit** (`VrijTests`, `StorageTests`): the figure; income dated later; income and cash on another account;
  each period end (swept, no destination, below zero); a late change to a swept period staying with its line
  through *Restant bijwerken*; *Niet toegewezen* to *Vrij* and every refusal around it; the first start's
  two ends; the lists' order; what a line lets go stops being claimed; a change of pool taking the line
  claims along; the pool's *Vrij* to a category the pool backs. A round trip with the new move, and the
  damaged-data case. **The invariant now
  covers the pool account** (1500 mixed acts), and **a second run of 1500 acts** that must leave the pool's
  *Vrij* exactly where it was — planning, spending, backing, "—", re-pointing, moves that do not touch it,
  income now and later in the period, days passing — found P6, which it now accounts for.
- **Specs:** blank *Vrij* cells for the pool account are gone (every account shows one); the step for "shows
  no *Vrij*" is removed; new and revised scenarios in `show-unclaimed` and `reallocate-an-amount`.
- **After green:** `spec-reviewer`, a few mutations, the phone's snapshot pictures, then a Release APK for his
  phone, installed with `adb install -r` over the version he has.
