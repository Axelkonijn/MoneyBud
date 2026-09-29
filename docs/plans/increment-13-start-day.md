# Increment 13 — a configurable period start day: implementation plan

**Status:** written 2026-09-29, **approved by Axel at the second gate on 2026-09-29**: D1 and D2 as recommended, all seven readings as written.

**What it builds against:** the approved scenarios, `change-the-period-start-day`,
`carry-plans-and-money-across-a-start-day-change` and `name-a-budget-period`, plus the additions to
`keep-data` and `start-moneybud` (33 scenarios, 50 cases), **approved by Axel at the first gate on
2026-09-29** together with every reading in their headers. And §12 *A configurable period start day*,
which holds all sixteen rulings with their reasoning, including *For the plan*. Read that section
before starting. This plan does not restate it.

---

## Two decisions for Axel at this gate

### D1. The calendar becomes a history of start days, and is kept (a new ADR 0012)

Ruling 1 keeps every earlier period's boundaries, so one start day can no longer describe the
periods. `keep-data`'s second new scenario checks exactly that: after two changes, both short periods
are still there after starting again.

**Proposed:**

1. **`BudgetPeriodCalendar` holds a list of changes**: each one has the day it takes effect from and
   the start day after it. It starts as "the 1st, since always". A change made while period *P* is
   current adds "from *N*, day *d*", where *N* is the first time day *d* comes round after *P*'s first
   day (clamped, as now). It drops any change that was still to take effect after *P*'s first day,
   which a second change on the same day can leave behind. A period is worked out as today, with the
   start day in force at its date, and cut at the next change. So periods keep tiling, and a period
   before *P* never moves. The calendar stays immutable: a change makes a new calendar, and the
   `Ledger` swaps it in.
2. **`Ledger.ChangeStartDay(int day)`** is the one act. It settles first, under the old calendar, as
   every act does. It then re-keys each budget for a period after *P* whose first day no longer starts
   a period, into the period that day falls in, adding up if two land together (ruling 4). It swaps in
   the new calendar. **If *N* is today or earlier**, it passes into the new current period itself,
   **dated today** (follow-up 2): it records the end of the cut period, sweeps it, and moves the
   backed plan for the new period. The ordinary `Settle` cannot do this, because it looks for the next
   boundary *after* the day already settled, and *N* lies before it. The same path makes the
   scenario-stage ruling come out by itself: a period ended by a change is swept even when its new end
   is before the first start.
3. **A backing remembers the first day of its period of backing** (follow-up 5). `AccumulatedFor`,
   `ThereFor` and the two figures remembered since version 6 today work that day out again with
   `Calendar.PeriodContaining`. After a change that gives 27 September instead of 1 September, and
   *Opgebouwd* would jump. A new field on `Backing` fixes it at backing.
4. **File format version 7**: a `calendar` list of `{ "from": "2026-09-27", "startDay": 27 }`, empty
   for "the 1st, since always", and the backing's `periodFrom`. `FromSnapshot` builds the calendar
   **before** it checks budgets, period ends, let-go amounts and sweeps against it, so a budget on a
   day that starts no period is still refused.

| Rejected | Why |
|---|---|
| **Keep only the current start day, and every period's first day beside it** | The same information stored twice, and they could disagree |
| **Store every period's boundaries** | A list that grows every month, when a change is rare. A history of changes gives the same periods from a handful of rows |
| **Work out the backing's period again, from the calendar as it was then** | The period of backing may have been cut since, so "the period as it was" exists nowhere any more. Remembering one date is simpler than keeping the old calendar |

**Consequences:** ADR 0012. ADR 0007's "the period start day is not stored" and ADR 0010's "a sweep's
period is named with the default calendar" get dated notes pointing to it. §11's start-day row is
closed as debt.

### D2. Data saved by the current version is read, not refused

Version 6 has no start day, and that is exactly true of the data: every period in it began on the 1st.
Its backings' period of backing is the calendar month of their mark, which is what `PeriodContaining`
already gives under a calendar of the 1st. So nothing is guessed. **Recommended: read version 6**, and
version 5 and 4 through it as now. Your current data keeps working. The cost is one branch in the
reader and one unit test. The alternative is to refuse it: MoneyBud says it cannot read the data, and
you delete the file.

---

## Chosen in this plan, for Axel to see at the gate

These are the plan's readings where the rulings and the scenarios leave a detail open. Each is the
smallest rule the scenarios pass with.

1. **The question** (ruling 5; the loose end §12 left on copy): *"Perioden laten beginnen op de 27e?
   De huidige periode wordt dan 27 sep – 26 okt 2026."* It says **"de huidige periode"**, not "deze
   periode", so it reads right when it is asked from a later period. When the change ends the current
   period on the spot, it adds *"1 – 26 sep 2026 is dan afgelopen."* The answers are *Wijzigen* and
   *Annuleren*.
2. **The notice:** *"Perioden beginnen nu op de 27e. De huidige periode is 27 sep – 26 okt 2026."* A
   sweep the change caused comes **after** it, in the order it happened (increment 12's revised
   reading 7).
3. **The drop-down** shows *"1"* to *"31"*, captioned *Periode begint op*, to the right of the period
   name and its step buttons. What it shows while the question waits is the day being asked about.
   Dropping the question, whether by *Annuleren*, stepping or anything said, puts it back, because the
   list shows the ledger's day whenever no start-day question waits. *Periode begint op* moves from
   §12's proposed terms into the real display-terms table, which `TekstTests` holds `Tekst` to.
4. **The day changes while the question waits** (midnight). The tick drops the question and puts the
   list back, even when nothing else is said, so *Wijzigen* can never act on a current period the
   question did not name.
5. **Where the screen and the assign form go** (follow-up 1, scenario-stage ruling 3), one rule for
   both. From the current period: the new current period. From any other period: the period its first
   day now falls in. That leaves an earlier period where it was, and takes a later one to the period
   its plan went to.
6. **Period names** (ruling 3, follow-up 3, scenario-stage ruling 2). `Tekst.PeriodName` keeps its own
   list of short months, *jan … dec*, not the machine culture's. A sweep's period is named through the
   ledger's calendar, not a new default one, which fixes the place §11 names.
7. **Two budgets landing in one period add up.** No scenario reaches it, and §12 found no change that
   does, but the code must do something. Adding up is what ruling 4's reading says, and it moves no
   money by itself. A unit test holds it.

---

## Building it

**Domain.**
- `BudgetPeriodCalendar`: the change list, `StartDay` (the day in force now), `ChangedFrom(period,
  day)`, and a no-op when the day is already the one in force.
- `Ledger`:
  - `ChangeStartDay(day)` → `ChangeStartDayResult`, holding the new current period and whether it
    ended one.
  - The day for a pass into a period becomes a parameter of `PassInto`.
  - `Backing.PeriodFrom`, used by `AccumulatedFor`, `ThereFor`, `NotMovedBefore` and `PaidBefore`.
  - `ToSnapshot`/`FromSnapshot`.

**Storage.** `LedgerJson` version 7, which reads 6, 5 and 4.

**Presentation.**
- `MoneyBudApp`:
  - `StartDayChoices` (1–31) and `StartDayChoice`. The two-way list writes back, so the day already
    set is a complete no-op.
  - `ShowsStartDay`, true on the current and later periods.
  - Asking, confirming and dropping, as in readings 1–4.
  - Moving `ShownPeriod` and `AssignForm.Period` by reading 5.
  - The confirm path ends in `Tell`, which takes what settling did, as every act must.
- `Tekst`: `PeriodName`'s new forms, `StartDayQuestion`, `StartDayChanged`, and the sweep's period
  named by the ledger's calendar, which `Tekst` is handed.

**Desktop.** A `ComboBox` beside the period name, bound to the three properties above, with
`Margin="0"` as *Staat op* needed. It decides nothing.

**Tests.**
- **Specs:** step definitions for the new steps. Widen the "budget period N before/after" patterns to
  any N, and the confirm steps to the start-day question. A `today is` that comes before "I have never
  used MoneyBud" sets only the day (the note in `start-moneybud`).
- **Unit tests:**
  - The calendar: periods tile and never lengthen, for every start day × change day × a run of dates,
    and a second change on the same day drops the first.
  - Re-keying.
  - `PeriodFrom` holding *Opgebouwd*.
  - Version 7 round trip, version 6 read, and a v7 file with a budget on a day that starts no period
    refused.
  - `PeriodName`'s five forms.
  - `Tekst` against §12's table.
  - Reading 4.

**After green:** `spec-reviewer`; a few deliberate mutations; a headless run of the real window (the
list's write-back on first show and on stepping must say and save nothing; the question and
*Annuleren*); arc42 §5, §6, §8, §9 (ADR 0012), §11 and §12 *Chosen in the build*; the README; CLAUDE.md;
then your try.
