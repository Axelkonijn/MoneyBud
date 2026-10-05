# 0012 — The calendar is a history of start-day changes, and it is kept

**Status:** Accepted
**Date:** 2026-09-29
**Relates to:** [ADR 0007](0007-keeping-the-ledger.md), whose consequence "the start day is not in the
file" it answers, and whose file format it takes to **version 7**, reading versions 6, 5 and 4.
[ADR 0010](0010-sweeps-and-period-ends.md), whose consequence "a sweep's period is named with the default
calendar" it ends, and which gains the one sweep not dated the next period's first day.
[ADR 0009](0009-movements-are-entries.md), whose two marks on a backing stay as they were and gain a
first day each beside them. Nothing any of the three decided is reversed. 0007 and 0010 carry dated
notes that point here.

> **Note, 2026-10-04.** *Vrij* and moving *Opgebouwd* ([ADR 0015](0015-moves-of-purpose-and-a-backing-history.md)).
> **Decision 3 is changed in one point**: `ThereFor` no longer counts expenses from `HereFrom`, the
> first day of the period of the latest re-pointing, but from `AccumulatingFrom`, the period of backing,
> as `AccumulatedFor` does. Once the expense form's account list was locked from the period a category
> got its account, an expense dated in an earlier backed period and entered after a re-pointing went on
> the new account and was counted in *Opgebouwd* but not in what that account holds. `spec-reviewer`
> found it at the build. `PaidHereBefore` is remembered from the period of backing, and data from before
> version 8 is converted on reading, so every figure reads as before. **`HereFrom` is still kept,
> written and checked on loading**, and no figure counts from it any more. What decision 3 is for still
> holds: neither figure works its first day out again, so a change of start day changes neither. The
> body is unchanged.

## Context

The start-day rulings ([§12](../arc42/12-glossary.md), *A configurable period start day*) let the
user change the day budget periods start on, at any time, in a drop-down beside the period's name.
**Ruling 1** makes a change apply from the current period on: the current period keeps its first day
and ends the day before the new day first comes round after it, possibly on the spot, and **every
earlier period keeps its boundaries**. **Ruling 4** sends a plan made ahead for a period that no
longer exists into the period its old first day falls in. ***Follow-up 2*** dates money a change makes
MoneyBud move, a sweep of the period it ended and a backed plan whose new period has already begun,
**the day of the change**. ***Follow-up 5*** rules that **a change never changes *Opgebouwd***, which
the stakeholder put as "just resterend + earlier resterend + any money from sweeps". And the scenario
stage ruled that a period ended by a change is swept by itself even when its new end falls before the
first start.

The code those rulings met could not hold them:

- **`BudgetPeriodCalendar` took one start day**, and `Ledger` fixed it at construction, at the 1st.
  One start day describes every period or none. After one change it describes none: August was
  1–31, the period cut short 1–26 September, and the rest start on the 27th.
- **Everything is keyed by a period's first day**: budgets, a sweep's `sweptFor`, each period-end
  record, and settling's settled-through day. Loading refuses any of them on a day that starts no
  period. So the calendar had to survive a restart as it was, or kept data would be refused, or worse,
  read against the wrong periods.
- **`AccumulatedFor` and `ThereFor` worked out the period of backing again on every call**, with
  `Calendar.PeriodContaining` of the backing's mark. After a change to the 27th that gives
  27 September for a category backed on 28 September, where it was 1 September, and *Opgebouwd* would
  rise by everything spent from 1 to 26 September: money that never moved. Follow-up 5 forbids it.
- **Settling looks for the next boundary after the day already settled.** A change that ends the
  current period puts a boundary *before* that day, which settling would never pass.
- **`Tekst` named a sweep's period with a new default calendar**, right only while every period
  started on the 1st ([§11](../arc42/11-risks-and-technical-debt.md), the start-day row, now resolved).

These are requirements. How to hold them in code and on disk is architecture: it decides what the
calendar is, touches the file format, and adds a second place where MoneyBud passes a period boundary.
This record holds the answer. It was proposed as decision D1 of the start-day increment's
[implementation plan](../plans/increment-13-start-day.md), with the file-format question as D2, and
**approved by the stakeholder at the plan gate on 2026-09-29**: D1 and D2 as recommended, with the
plan's seven readings as written. **The build changed two details of D1**, each where the plan's shape
turned out too small for a case it had not seen. Both are marked below.

## Decision

### 1. The calendar is a start day since always, and a list of changes

`BudgetPeriodCalendar` holds a start day since always, the 1st, and a list of
**`StartDayChange(PeriodFrom, From, StartDay)`**:

- **`PeriodFrom`** is the first day of the period that was current when the change was made. The
  change cuts that period to end the day before `From`.
- **`From`** is the first day the new start day came round after `PeriodFrom`, clamped where a month
  is too short for it, as the clamp has always been.
- **`StartDay`** is the day periods start on from `From` on, until the next change.

A period is found as before, with the start day in force at its date, and cut at the next change. So
periods still tile, and a change never moves a period before the one it was made in.

**The calendar is immutable.** `ChangedFrom(current, day)` makes a new one, and the `Ledger` swaps it
in. It drops any change still to take effect after the current period's first day, so **a second
change in the same period replaces the first**. Choosing the day already in force for the current
period, which a second change can do, adds no change at all. `FromChanges` rebuilds a kept history by
making each change again, in order, and **refuses a list no series of changes could have made**: out of
order, made in a period that was never current, a day outside 1 to 31 or already in force, or a `From`
the new day does not first come round on.

**A ledger always begins on the 1st.** Its constructor, `StartNew` and `FromSnapshot` no longer take a
calendar. A first start begins on the 1st by ruling, and kept data carries its own history.

> **Changed in the build: the plan's change had two fields, the build's has three.** D1 said each change
> holds "the day it takes effect from and the start day after it". `From` alone cannot say which period
> a change cut. Under the 29th in 2027, February's period begins on a clamped 28 February. A change to
> the 31st made in that period, whose own February clamp is also 28 February, next comes round on
> 31 March. So does a change to the 31st made in the period from 29 March. Both take effect on
> 31 March, and they give different periods: 28 February – 30 March in the first case, 29–30 March
> in the second. `PeriodFrom` tells them apart. It came out of the stakeholder's ruling on the first
> case at the build ([§12](../arc42/12-glossary.md), *A configurable period start day: chosen in the
> build*), and it is why the file's calendar entries have three fields.

### 2. One act, `Ledger.ChangeStartDay`, which passes a boundary itself when it has to

`Ledger.ChangeStartDay(day)` returns a **`ChangeStartDayResult(Day, WasChanged, Current, Ended)`**:
the day, whether anything changed, the current period under the calendar as changed, and the period
the change ended on the spot, if any. `Ledger.PreviewStartDay(day)` gives the same answer without
acting, for the question the screen asks first. The act is never refused: every day in the list is
allowed. In order:

1. **The day already set is a complete no-op**, recognised before settling, like `SetBacking`'s: the
   drop-down writes back what it shows on every redraw.
2. **It settles**, under the old calendar, as every act does.
3. **It re-keys the plans made ahead.** Each budget for a period after the current one whose first day
   no longer starts a period moves into the period that day falls in, **adding up** with any budget
   already there (ruling 4). The current period's own budgets stay where they are (follow-up 4).
4. **It swaps in the new calendar.**
5. **If the current period ended on the spot**, it passes into the new current period itself, as
   settling passes any boundary but **dated today** (follow-up 2): the ended period's period-end record
   with what is backed now, its sweep, and the new period's planned money. `PassInto(period, on)` now
   takes the day, so settling and the change share it.
6. **If the current period goes on**, a plan made ahead that has landed in it has its backed money
   moved at once, dated today. Settling would never move it, because the current period's first day
   is already settled.

### 3. A backing remembers the first day of its periods

`Backing` gains **`AccumulatingFrom`** and **`HereFrom`**, one for each of its two marks
(`AccumulatingSince`, `HereSince`, [ADR 0009](0009-movements-are-entries.md)): the first day of the
period the mark was set in, **as that period was when it was set**. `AccumulatedFor` counts expenses
from `AccumulatingFrom`, and `ThereFor` from `HereFrom`. Neither works the day out again, so a change
never changes *Opgebouwd*, nor what is there for a category.

> **Changed in the build: the plan named one field, the build has two.** D1 said "a backing remembers
> the first day of its period of backing", as `PeriodFrom`. A backing has two marks, and each counts
> expenses from its own period: re-pointing resets `HereSince` and leaves `AccumulatingSince`, so after
> a re-pointing in a later period the two periods differ. So there are two days.

### 4. File format version 7

`LedgerJson` writes a top-level **`calendar`** list of `{ "periodFrom", "from", "startDay" }`, **empty**
for periods that have always started on the 1st, and on each backing **`accumulatingFrom`** and
**`hereFrom`**. `Ledger.FromSnapshot` **builds the calendar first**, through `FromChanges`, and checks
budgets, sweeps and period-end records against it, so a budget on a day that starts no period is still
refused. It checks that a backing's two days are period starts on or before their marks.

## Why

### A history, because ruling 1 keeps every earlier period

One start day cannot describe periods that ruling 1 keeps as they were. A list of the changes, each
saying where it cut, gives every period there has ever been from a handful of rows, and it grows only
when the user changes the day, which is rare. Rebuilding it change by change on loading means the
kept list is checked by the same code that made it, so a hand edit that describes an impossible
history is refused rather than read.

### Replacing a pending change, because the user may change his mind the same day

A change made on 29 September to the 30th leaves September current until the 29th, with the 30th
pending. Changing again the same day is changing his mind about that one change, not making a second
cut. Keeping both would leave a period that was never current in the history.

### The change passes the boundary itself, because settling cannot

Settling works forward from the day already settled, and a change puts a boundary behind it. Rather
than teach settling to look backwards, the act passes the boundary it made, with the same steps
settling uses, `PassInto`. So the ended period is recorded, swept and followed by the new period's
money exactly as any period's end is, and the scenario-stage ruling comes out by itself: a period
ended by a change is swept even when its new end falls before the first start, because the act
sweeps it without asking settling's first-start question.

### Dated today, for follow-up 2's reason

A balance correction dated between the new period's first day and the change would take a move dated
that first day in, so the balance would not rise although MoneyBud says the money moved. Dated today,
every move a change makes comes after anything already typed.

### A remembered first day, because the period of backing may no longer exist

After a cut, "the period of backing" as it was exists nowhere in the calendar. Remembering the one
date it began on is simpler than keeping the old calendar beside the new one, and is exactly what
follow-up 5 asks to count from.

### Rejected

| Rejected | Why |
|---|---|
| **Keep only the current start day, and every period's first day beside it** | The same information stored twice, and the two could disagree |
| **Store every period's boundaries** | A list that grows every month, when a change is rare. A history of changes gives the same periods from a handful of rows |
| **Work out the backing's period again, from the calendar as it was then** | The period of backing may have been cut since, so "the period as it was" exists nowhere any more. Remembering one date is simpler than keeping the old calendar |
| **A change as the day it takes effect from and the start day only** (the plan's own shape) | Two changes made in different periods can take effect on the same day and give different periods, so a kept history would be ambiguous (decision 1, the note) |
| **Leave passing the boundary to settling** | Settling looks for the next boundary after the day already settled, and a change's boundary lies before it. And settling dates a sweep the new period's first day, which follow-up 2 rejected |

## Consequences

- **File format version 7.** Beside everything version 6 had, a `calendar` list and a backing's two
  first days (decision 4).
- **Versions 6, 5 and 4 are read** (decision D2 of the plan, **approved by the stakeholder at the plan
  gate on 2026-09-29** on the recommendation). Version 6 has no start day, and that is exactly true of
  the data: every period in it began on the 1st. It is read as a calendar never changed, and each
  mark's period is the calendar month of its date, which is what `PeriodContaining` gave under the 1st.
  So nothing is guessed. Versions 5 and 4 are read through it as before. **A version 6, 5 or 4 document
  that has a `calendar` is refused**, since that is not what those versions wrote. Versions 1 to 3 stay
  refused. The next save writes version 7. As with version 4 in ADR 0011, reading was free, and
  promises no migration.
- **Every period is found and named through the ledger's calendar.** `Tekst` no longer makes a default
  calendar. A sweep's history row names its period through `HistoryLine.SweptPeriod`, filled from the
  ledger by `MoneyBudApp.History`. That closes the place [§11](../arc42/11-risks-and-technical-debt.md)
  named and ADR 0010's consequence.
- **A sweep made by a change is dated the day of the change**, the one sweep not dated the next period's
  first day. It is still a sweep of the period it ended, and names it.
- **Two plans can land in one period, and they add up.** The plan (reading 7) and §12 found no change
  that does it. The build found one: two changes in one period, the second dropping the first's pending
  cut, so a plan made ahead for the period that was about to begin lands back in the current one. Its
  backed money then moves at once, dated today (decision 2, step 6). Adding whole cents divides nothing,
  so [§8.2](../arc42/08-crosscutting-concepts.md) is not reopened.
- **A change can make the current period longer**, against a derivation §12 had written. In one corner
  that is ruling 1 followed exactly, as the stakeholder ruled at the build: a period begun on a clamped
  day under the 29th or 30th, changed to a later day whose clamp is that same day. And a second change
  in one period that drops the first's pending cut gives the period back the days the first had taken
  ([§12](../arc42/12-glossary.md), *A configurable period start day: chosen in the build*).
- **The clamp is unchanged**, and applies to the day a new start day first comes round as to every
  period after it.
- **Settling is unchanged.** It still finds each boundary through the ledger's calendar, which is now
  the history, and repeats keep a day of the month and a next date, not a period
  ([ADR 0011](0011-recurring-entries.md)). The change is a second place a boundary is passed, and it
  passes it with settling's own steps.
- **A clock once set ahead reaches the change too.** Settling may then already have moved a backed
  plan made ahead, dated its old first day. The change still re-keys it, since kept data needs every
  budget on a period's first day, but moves none of its money again (`settledAlready`, which
  `PassInto` takes). `spec-reviewer` found the second move, and a unit test holds the fix. The clock
  risk itself is [§11](../arc42/11-risks-and-technical-debt.md)'s, as for settling (ADR 0009).
- **`Ledger.HasBudget` is not a reader.** Re-keying reads the stored budgets as they are, zero ones
  included, which go with their first day like any other.
- **Reversal is cheap while the data is demo data.** A different shape changes the calendar, one act
  and the file version, and costs the user a fresh start, or none if a later version reads this one.
