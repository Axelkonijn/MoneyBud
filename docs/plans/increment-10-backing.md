# Increment 10 — backing and *Accumulated*: implementation plan

**Status:** written 2026-09-27, **approved by Axel at the second gate** the same day, with D1 as
recommended. D2 he left open ("Chose what is best for you. I dont mind starting over"), and the build
**refuses version 2** rather than reading it: one way in fewer, and nothing lost that he minds.
Built the same day on branch `increment-10-backing`. Where the build departed from this text, §12
*Backing: chosen in the build, not put to the stakeholder* says so.

**What it builds against:** the approved scenarios — `back-a-category`, `assign-to-a-backed-category`,
`spend-against-a-backed-category`, `show-accumulated`, `show-moved-money` (62 scenarios, 84 cases),
plus the additions to `start-moneybud` and `keep-data` — and §12 *Backing and Accumulated*, which holds
every ruling with its reasoning. Read that section before starting; this plan does not restate it.

---

## Two decisions for Axel at this gate

### D1. Money moved for a category is an entry, written on the day it moves (ADR 0009)

The rulings fix two things that decide the shape of the code:

- **Several amounts are fixed on their day.** What moves at backing, at unbacking and at re-pointing is
  worked out from the figures *at that moment*, and a late expense recorded afterwards must not change
  it (§12, follow-ups). So those amounts cannot be worked out afresh every time; they must be stored.
- **Money planned for a later period has no destination until that period's first day.** It goes to
  whatever backs the category *then*, out of whatever is the pool *then* (ruling 6 and its derivation).

**Proposed:** every movement is a stored **`Movement` entry** — id, date, category, from, to, amount —
written when the money moves, exactly as ADR 0008 foresaw ("backing and the sweep will write entries,
not balances"). A balance stays the sum of its entries, with one rule. Money planned for a later
period is written by **settling**: the first time MoneyBud runs on or after a period's first day, and
before anything else is done, it writes that period's planned movements with the backing and pool of
that moment. The ledger keeps the day it has settled through, and saves it.

This works because nothing can change while MoneyBud is closed: settling on 5 November for a
1 November movement sees exactly the backing and pool that 1 November had. A settled movement gets
the next id, which is lower than anything typed after it that day, so a balance correction typed on
1 November holds it — the derivation the scenarios already assert.

| Rejected | Why |
|---|---|
| **Work every movement out** from a new record of each assignment, each backing change and each change of pool | Three new histories to store, and the backing, unbacking and re-pointing amounts must be stored anyway (above). It keeps "worked out" in name only |
| **Write planned money when it is assigned**, to today's backing account | Breaks ruling 6: re-pointing or unbacking before the period starts would have to rewrite it |

The sweep (next increment) will use the same settling step at a period's end.

### D2. Saved data from the accounts increment is read, not dropped

§12 says saved data may be dropped "**at least** up to and including the accounts increment". This is
the first increment after that, so it is Axel's call whether his data must survive. **Recommended:
read version 2** and write version 3. It is cheap — version 2 has no backing and no movements, so
reading it means "no category is backed, settled through today" — and it keeps whatever he has
entered since trying increment 9. The alternative is to refuse version 2 as unreadable, as version 1
was refused.

---

## Domain (`MoneyBud.Domain`)

**New types**
- `Movement : IEntry` — `Id`, `Date`, `Category`, `From`, `To` (accounts), `Amount` (positive
  `Money`), `Reason` (`Assigned`, `Backed`, `Unbacked`, `Repointed`) for the history's wording, and
  `Direction` (`In` towards the backing account, `Out` back to the pool, `Along` between two backing
  accounts) for *Accumulated*. Stored because neither can be worked out later.
- `EntryMark` — a date and an entry id: "after this" means dated later, or on that day with a higher
  id. The same test as `Holds` (ADR 0008); factor it out so both use one rule.
- `Backing` on a category — the `Account`, `AccumulatingSince` (an `EntryMark`, reset only when an
  unbacked category is backed) and `HereSince` (an `EntryMark`, reset on backing *and* re-pointing).
  Each backing change draws an id from `lastEntryId`, with no entry of its own, so it orders against
  same-day expenses.
- `SetBackingResult` — the backing, and the movement written, if any.

**`Ledger` additions**
- `SetBacking(Category, Account?)` — back, re-point or unback. **Choosing what is already set changes
  nothing and says nothing** (a list writes back what it shows, see Desktop). Otherwise:
  - *back* (was none): move this period's `RemainingFor` if > 0, pool → account, reason `Backed`.
  - *re-point*: move `ThereFor` if > 0, old → new, `Along`.
  - *unback*: move `ThereFor` if > 0, backing → pool, `Out`.
- `ThereFor(Category)` — what is there for it in its backing account: movements for it since
  `HereSince` into that account, minus those out of it, minus its expenses paid from that account
  after `HereSince`. A pool-to-pool movement counts in and out, so it adds nothing.
- `AccumulatedFor(Category, BudgetPeriod)` — null if not backed now. Otherwise, after
  `AccumulatingSince` and up to the period's last day: movements `In` minus `Out`, minus every
  expense against the category (any account). For a period after the current one, add the budgets of
  the later periods up to it, which will move when they begin. Gives €0 for a period before anything
  moved.
- `Assign` — for a backed category: in the current period, a positive amount writes `In` pool →
  backing today; a negative one writes `Out` for the amount the clip let through, **capped at
  `ThereFor`** (ruling at the scenario gate). A later period only changes the figure. The pool-to-pool
  case writes the movement too, so *Accumulated* counts it.
- `Settle()` → `bool` — for every period starting after `settledThrough` and on or before today, in
  order: for each backed category with a budget > 0 there, write `In` pool → backing, dated that
  period's first day. Then `settledThrough = today`. Called first by every act that changes the
  ledger, and by the screen at start and on every tick.
- `HistoryOf` — includes movements on either side; leaves out any whose `From` equals `To`.
- `CanDeleteAccount` — also false while the account backs a category or has a movement.
- `DeleteCategory` — drops the backing (nothing can have moved: no history).
- `ToSnapshot` / `FromSnapshot` — backing per category, movements, `settledThrough`; every rule
  re-checked on load, as now.

**Unit tests:** `ThereFor` (§12's €200 / €50 / €150 example, a third account, the pool backing),
`AccumulatedFor` (past, current, future, re-backing), the cap, `Settle` (once only, several periods
at once, after re-pointing and after unbacking), a no-op `SetBacking`, and `CanDeleteAccount`.

## Storage (`MoneyBud.Storage`)

`LedgerJson` version **3**: `backing` on a category (account key and the two marks), `movements` (id,
date, category, from, to, cents, reason, direction), and `settledThrough`. Reads version 2 as "nothing
backed, settled through today" if D2 is approved. **Unit tests:** version 3 round trip, version 2
read, and a movement naming a missing account refused.

## Presentation (`MoneyBud.Presentation`)

- `CategoryRow` — `BackingAccount`, `BackingChoices` (none, then the accounts in the strip's order),
  `Accumulated` (`Money?`), `AccumulatedMarker` (negative → the one marker, badge *Rood*). The slice's
  details gain *Accumulated*.
- `MoneyBudApp.SetBacking(categoryName, Account?)` — the notice names the move when there is one and
  only the backing when not; it is never confirmed, and saves (`changed: true`). The no-op case says
  and saves nothing.
- **Settling:** `MoneyBudStart` settles after loading; `Tick` settles; an act keeps what the
  settling before it wrote. Anything settled is saved.
- `ExpenseForm` — the account **follows the category typed** (the backing account, or the pool for
  anything that is not a backed category) until the user picks one; an entry being changed keeps
  its own account. **A user pick is a write of a different account from the one shown**. A write of
  the account already shown is the list writing back, and a null is ignored as now. This is the
  CLAUDE.md warning about lists, so it gets a headless check.
- `HistoryLine` for a movement: the category, from, to and amount, read-only (no *Wijzigen* and no
  *Verwijderen*).
- `Tekst` — *Staat op*, *Opgebouwd*, the backing notices and the movement row's wording (copy). Move
  both terms from §12's proposals table into *Dutch display terms*, and add "and a negative
  *Accumulated*" to *Rood*'s English cell, at the same time, since `TekstTests` reads that table.

**Unit tests:** row figures and marker, the form's follow and stick (including the write-back of the
same account), `Tekst` wording.

## Desktop (`MoneyBud.Desktop`)

A *Staat op* ComboBox on each category row beside *Hernoemen*, bound to `BackingChoices`, with "—" for
none. *Opgebouwd* under the row's figures and in the ring's hole on hover. Movement rows in the
history. **The window decides nothing.**

## Step definitions (`tests/MoneyBud.Specs`)

- A new `BackingSteps.cs`: set and remove the backing, "the backing account of … should be …",
  *Accumulated* Givens and Thens (the Given checks and sets nothing), the notices (anchor the short
  "is now backed by "Y"" so it does not also match the long form or ", and of no money moved"), and
  the *Staat op* choices.
- Extend the categories table and the slice table with `accumulated` / `accumulated marked`, and the
  history table with the entry kind `movement` and the columns `category`, `from` and `to`.
- Form steps: "I type … as the category of the new expense", "I choose the account … for the new
  expense", and "the account chosen for the new expense should be …".
- **"the next budget period begins while MoneyBud is open" calls `App.Tick()`**, which is what the
  Desktop timer really calls, not `Refresh()`. The "before the Overview is next drawn" variant stays
  as it is: the act that follows settles first.
- Every `When` acts through `MoneyBudApp`, and every `Then` reads the screen where the screen shows it.

## Order of work

1. Domain, with its unit tests.
2. Storage, then the full suite green except the new scenarios.
3. Presentation, then step definitions, until the whole suite is green with 0 warnings.
4. Desktop, then a **headless run of the real window**: the *Staat op* list writing back on first show
   and when accounts change must not move money or announce anything, and the expense form's account
   must follow the category until picked.
5. `spec-reviewer`, fixing what it finds.
6. Documentation: **ADR 0009** (D1, with D2 in its consequences) and §9's index, §5, §6 (settling at
   start and on each tick), §8.1, §11 (the balance-writing row, now built), §12's "not built" notes,
   the arc42 README, `features/README.md` (file count), and `CLAUDE.md`.
7. Axel tries it, then merge into `main`.

## Watch out for

- Every act that changes the ledger settles first, so a period that began while MoneyBud was open
  moves its planned money before anything is done in it.
- The id is the recording order across **five** entry kinds now, plus backing changes, which draw an
  id without being an entry.
- `HasBudget` stays unused by the screen; *Accumulated* and settling read the figures.
- No-op acts pass `changed: false` (CLAUDE.md), and a `SetBacking` to the backing already set is one.
