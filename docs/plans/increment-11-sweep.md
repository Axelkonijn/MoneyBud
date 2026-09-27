# Increment 11 — the sweep (*Restant*): implementation plan

**Status:** written 2026-09-27, **waiting for Axel at the second gate**. Nothing is built.

**What it builds against:** the approved scenarios — `sweep-at-a-period-end`, `show-an-ended-period`,
`bring-a-swept-period-up-to-date`, `choose-a-sweep-destination` (47 scenarios, 62 cases), plus two
scenarios added to each of `start-moneybud` and `keep-data` — **approved by Axel at the first gate on
2026-09-27**, with every reading in their headers. And §12 *The sweep and Restant*, which holds every
ruling with its reasoning, including *Ruled at the scenario stage* and *For the plan*. Read that
section before starting; this plan does not restate it.

---

## Two decisions for Axel at this gate

### D1. How a sweep is kept (a new ADR 0010)

The rulings need four things the ledger does not keep today (§12, *For the plan*):

- **Which categories were backed when a period ended.** A category counts in a period's *Restant* when
  it was unbacked at that period's end, also when the figure is worked out again months later. The
  ledger keeps only today's backing, and backing an overspent category moves nothing, so there is no
  movement to rebuild it from.
- **What was swept for which period, into which category, in which order**, for the line, for "take
  back latest first", and for the difference.
- **What was let go**: once a take-back is capped, the line stops asking for good (scenario-stage
  ruling 4). That cannot be worked out afresh, because what is there for the category grows again.
- **The destination**, one setting for the whole ledger.

**Proposed:**

1. **A sweep is a `Movement`** (ADR 0009) with a new reason, **`Swept`**, and a new field, the
   **period it is for**. Direction `In` (pool → the destination's backing account) for the automatic
   sweep and for "nog niet weggezet" moved by the button; `Out` (backing account → pool) for money
   taken back. So balances, histories, *Opgebouwd* and "what is there for it" all count sweeps with
   no new rule, and "latest first" is the order of the ids.
2. **Settling records each period's end**: when `Settle` passes a period boundary, it writes a
   **period-end record** for the period that ended — its start and the set of categories backed at
   that moment — and then sweeps it. Nothing can change while MoneyBud is closed, so that set is
   exactly what the period's end had (ADR 0009's own argument). **A period with no record ended before
   the first start**: nothing was backed then, and it was never swept by itself (ruling 7). So the
   first start needs no field of its own.
3. **A let-go amount per period**, written when the button takes back less than was swept too much.
4. **The destination**: a category, or none.

| Rejected | Why |
|---|---|
| **A full backing history** per category (every back, re-point and unback, with its mark) | Answers the same question for every day, where only a period's last day is ever asked. More to store and to check on load, for nothing the rulings need |
| **Store each period's *Restant* when it is swept** | It is a figure, and a period never closes: the rulings need it worked out again after every late entry. Only the backing it was worked out under must be kept |
| **Record the let-go as a movement from an account to itself** | Keeps one list, but makes the line's "what really went" (ruling 4) subtract money that never moved |

**Consequences:** file format **version 4**, and ADR 0009's accepted gap narrows: **a sweep that moved
money is saved straight away** (scenario-stage ruling 1). The new ADR says so, and 0009 gets a dated
note pointing to it, as 0007 got for 0009.

### D2. Data saved by the backing version is refused, not read

Version 3 has no period-end records. Reading it would mean guessing the backing at every past
period's end, which is the one thing the rulings say must not be guessed. **Recommended: refuse
version 3 as unreadable**, like versions 1 and 2. MoneyBud says it cannot read the data, touches
nothing and closes, and you delete the file. It is still demo data (§12, *Demo data may not survive a
new version*). The alternative is to read it with today's backing standing in for every past
period's end, and no period counted as swept.

---

## Chosen in this plan, for Axel to see at the gate

These are the plan's readings where the rulings and the scenarios leave a detail open. Each is the
smallest rule the scenarios pass with.

1. **The difference** for an ended period is `max(0, Restant) − (swept − let go)`, where *swept* is
   what really moved for it, net, over all categories. Above zero: "nog niet weggezet". Below zero:
   "te veel weggezet".
2. **Who can give money back.** A category that received money for the period can give some back only
   while it is **still backed by the backing it had when the money went in**: backed now, and not
   unbacked and backed again since (its *AccumulatingSince* mark is before the sweep). Re-pointing
   keeps it. If no category can, the line stops asking **by itself**, names what went, and shows no
   button: the scenario *no longer backed*. Because the mark never moves back, that is permanent too.
3. **Pressing the button when too much was swept** goes through the categories latest first. Each gives
   back at most what it received for the period, and at most what is there for it. **What none can give
   back is let go**, and stored (D1.3), so the line stops asking for good. If the latest category is
   capped, the one before it is asked for the rest before anything is let go.
4. **The line**, for an ended period, one at a time (scenario-stage ruling 2):
   - difference above zero → what went, if anything did, **and** "€ N nog niet weggezet"; the button
     only with a destination set;
   - difference below zero and some category can give back → "€ N te veel weggezet", with the button;
   - otherwise, something swept → "Restant € N naar X", a part per category, a category at zero left off;
   - otherwise, a *Restant* below zero → the negative figure with the marker, badge *Tekort*;
   - otherwise → no line.
5. **Settling on a period's first day sweeps the ended period first, then moves the new period's
   planned money.** The old period ends before the new one's money moves. Both are dated that day and
   recorded before anything typed on it, so the order changes no balance.
6. **The notice.** The ledger collects the sweeps settling made. The screen takes them after every act,
   on start and on every tick. It **says them in front of whatever the act says**, one sentence per
   period swept, in period order, and saves straight away even when the act itself changed nothing or
   was refused. Several periods swept at one start give **one notice with a sentence each**.
7. **A sweep notice on the minute's tick drops a waiting question** to remove an entry, since a
   question and a notice are never shown together. Money moved, so being told wins over the question.
   §12 *For the plan* leaves this to the plan; it is taken here as written.
8. **Clearing the destination is said by the act that clears it** (unbacking, archiving, deleting):
   the screen compares the destination before and after the act and adds "… is no longer the sweep
   destination". The domain methods keep their result types.

---

## Domain (`MoneyBud.Domain`)

**Changed types**
- `MovementReason.Swept`. `Movement` gains `DateOnly? SweptFor`: the first day of the period swept,
  set exactly when the reason is `Swept`. `Swept` goes `In` or `Out`, never `Along`.
- `LedgerSnapshot` gains `SweepDestination` (a category key, or null), `PeriodEnds` (period start and
  the keys of the categories backed then) and `LetGo` (period start and amount). `MovementSnapshot`
  gains `SweptFor`.

**New types**
- `SetSweepDestinationResult` — `Unchanged`, `Chosen` (the category) or `Removed`.
- `SweepLine` — what an ended period's line says: its kind (`Swept`, `StillToSweep`, `SweptTooMuch`,
  `Shortfall`, none), the amounts per category that went, the amount still to sweep or swept too much,
  the shortfall figure, and whether the button can act.
- `BringUpToDateResult` — each move made (category, amount, in or out) and the amount let go.
- `SweepMade` — a period and a movement, for the notice (`TakeSweepsMade`).

**`Ledger` additions**
- `SweepDestination` (`Category?`) and `SweepDestinationChoices`: backed categories not archived, in
  the order added. The alphabetical order is the screen's, as for suggestions.
- `SetSweepDestination(string? name)` — choosing the one already set is a **complete no-op, before
  settling** (the `SetBacking` rule, since the list writes back on every redraw). Otherwise settles,
  sets, and returns `Chosen` or `Removed`. **Throws** for a category that is unknown, not backed or
  archived: the list never offers one.
- `SetBacking(…, null)` on the destination, `ArchiveCategory` of it and `DeleteCategory` of it **clear
  the destination**. Re-pointing keeps it; bringing an archived category back does not set it again.
- `PeriodLeftover(period)` — *Unassigned* plus the *Remaining* of every category **not backed at the
  period's end**: the period-end record's set if there is one; none backed if the period ended with no
  record (before the first start); today's backing for the current period and later ones.
- `SweepLineFor(period)` → `SweepLine?`, null for the current period and later ones (rule 4 above).
- `BringUpToDate(period)` — settles first; moves the difference (rules 1–3 above), **dated today**:
  `In` from the pool account to today's destination's backing account, or `Out` from each category's
  current backing account to the pool account. It changes no budget. **Throws** when the line offers
  no button: the button is not there.
- `Settle()` — for each period boundary passed, in order: write the ended period's **period-end
  record**; if a destination is set and its *Restant* is above zero, write a `Swept` `In` movement
  for it, from the pool account to the destination's backing account, dated the new period's first
  day, and add it to the sweeps made; then the new period's planned money, as now. A pool account
  backing the destination writes a pool-to-pool movement: no balance changes, *Opgebouwd* counts it.
- `TakeSweepsMade()` → the sweeps made since last asked, and forgets them.
- `CanDelete` — false while **any** `Swept` movement names the category, pool-to-pool included
  (scenario-stage ruling 7). `DeleteCategory` also takes the category out of the period-end sets.
- `ToSnapshot` / `FromSnapshot` — the new fields. The checks on load: a `Swept` movement has a period
  and every other movement has none; the destination is backed and not archived; a period-end set
  names only categories that exist; a let-go amount is above zero and for a period with a record or
  a sweep.
- The class comment's "Not built yet: the end-of-period sweep" goes.

**Unit tests:** `PeriodLeftover` (netting, a backed category left out, backing at the period's end vs
today's in both directions, an archived unbacked category, before the first start); `Settle` (a sweep
per period over several, none at zero or below, none with no destination, pool-to-pool, the order with
planned money); `SweepLineFor` in each of its five states; `BringUpToDate` (both directions, latest
first, the cap and what is let go, continuing to the earlier category, no longer backed, re-backed
since); the no-op `SetSweepDestination`; clearing on unback, archive and delete, keeping on re-point;
`CanDelete`; `TakeSweepsMade` draining once.

## Storage (`MoneyBud.Storage`)

`LedgerJson` version **4**: `sweepDestination` (category key or null), `periodEnds` (`periodStart`,
`backed`: category keys), `letGo` (`periodStart`, cents), and `sweptFor` on a movement; `reason` gains
`"swept"`. Version 3 is refused if D2 is approved. **Unit tests:** a version 4 round trip with every
new field; version 3 refused; a swept movement without a period refused.

## Presentation (`MoneyBud.Presentation`)

- `PeriodOverview` — for the current period and later ones: `SweepChoices` ("—" first, then the backed
  categories alphabetically, compared as `CategorySuggestions` are) and `SweepDestination`. For an
  ended period: the `SweepLine`, its text, the *Tekort* marker, and whether *Restant bijwerken* is
  offered. No preview anywhere (ruling 10).
- `MoneyBudApp.SetSweepDestination(string? name)` — the notice for choosing and for removing; the
  no-op says and saves nothing (`changed: false` is not enough: nothing at all, like `SetBacking`).
- `MoneyBudApp.BringSweepUpToDate()` — acts on `ShownPeriod`; the notice names every move, one
  sentence per category, never confirmed; saves.
- **Sweeps are told and kept wherever settling can happen**: the constructor, `Tick`, and every act
  through `Tell`, `Refuse` and `SayNothing`, which take the ledger's sweeps made and put their
  sentences first (rule 6 above). A sweep **saves straight away**.
- **Clearing the destination**: the acts that can clear it (unbacking, archiving, deleting a category)
  add "… is no longer the sweep destination" when it was cleared (rule 8 above).
- `Tekst` — *Restant*, *Restant naar*, *Restant bijwerken*, *nog niet weggezet*, *te veel weggezet*,
  the badge *Tekort*, and the notices (copy). Move the ruled terms from §12's *Proposed display terms
  for the sweep* into *Dutch display terms* at the same time, since `TekstTests` reads that table.

**Unit tests:** the choices' order (case and accents, as suggestions), the line's text in each state,
the notice with several sweeps and with an act's own sentence, a sweep saved on a refused act, `Tekst`.

## Desktop (`MoneyBud.Desktop`)

Near *Niet toegewezen*: in the current and later periods a *Restant naar* ComboBox bound to the
choices, "—" for none; in an ended period the line and, when offered, a *Restant bijwerken* button.
Where exactly it sits beside the ring's hole is the build's call, checked in the headless run. **The
window decides nothing.** If the field-order test in `WindowMarkupTests` covers this area, extend it.

## Step definitions (`tests/MoneyBud.Specs`)

- A new `SweepSteps.cs` for every step in the header of `sweep-at-a-period-end.feature`: setting and
  removing the destination (stepping forward to the current period first when an ended period is on
  screen), the list's choices **in order**, the line's five readings and "no line", the button offered
  or not, pressing it, and the five announcements. A notice step checks that the notice **contains**
  its sentence, since one notice can carry several.
- **Extend** `I should not be warned or asked to confirm` to accept the new results, and the case where
  no act came before it (after a period boundary).
- **Extend** the backing and archive steps to find the **current** period's row when an ended period
  with no row for that category is on screen.
- **Test world:** a scenario's ledger is made on the day it begins, in the current period, so the
  previous period has no period-end record: "first started in the current period" holds by
  construction. Check that the Givens that move the clock within the current period ("today is the
  last day of …") run after the ledger is made and never cross a boundary, or `Settle` would record
  one.
- Every `When` acts through `MoneyBudApp`, and every `Then` reads the screen where the screen shows it.

## Order of work

1. Domain, with its unit tests.
2. Storage, then the full suite green except the new scenarios.
3. Presentation, then step definitions, until the whole suite is green with 0 warnings.
4. Desktop, then a **headless run of the real window**: the *Restant naar* list writing back on first
   show and whenever the backed categories change must say and save nothing; the line and the button
   in an ended period; the sweep notice after a tick across a boundary.
5. `spec-reviewer`, fixing what it finds.
6. Documentation: **ADR 0010** (D1, with D2 in its consequences) and §9's index, a dated note on ADR
   0009, §5, §6 (the sweep at settling, saved straight away), §8.1, §8.3 (the file format), §11 (the
   location row resolved, the balance-writing row updated), §12's "not built" notes and *Chosen in the
   build*, the arc42 README, `features/README.md` (files and counts), the README if anything the user
   sees about the data file changed, and `CLAUDE.md`.
7. Axel tries it, then merge into `main`.

## Watch out for

- Every act that changes the ledger settles first, and settling may now **sweep**: the screen must
  take the sweeps made after **every** act, refused and no-op ones included, and save.
- `SetSweepDestination` to the destination already set, like `SetBacking` to the backing already set,
  must stay a **complete no-op**: no settle, no notice, no save.
- The id is still the recording order across every entry kind and the backing marks. "Latest first"
  and "backed since before the sweep" both lean on it.
- `HasBudget` stays unused. The *Restant* reads the figures.
- An ended period's own figures never change for a sweep (ruling 11). The sweep is shown as a line
  beside them, never written into them.
