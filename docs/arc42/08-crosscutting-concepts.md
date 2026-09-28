# 8. Cross-cutting Concepts

**What belongs here:** Rules and patterns that apply across many building blocks — the domain
model, persistence, error handling, validation, logging, security. Anything a developer needs to
know regardless of which part of the system they're touching.

---

_§8.1 to §8.4 are filled in. §8.4 arrived with the UI. §8.3 records persistence as settled and
built, since 2026-09-26. Opening a period, accounts, backing and the sweep are built, and appear in
§8.1, §8.3 and §8.4._

## 8.1 Domain Model

The domain is one class library, `MoneyBud.Domain` ([§5](05-building-block-view.md)). Its types
carry XML doc comments pointing back at the section that decided each rule, so **the code is the
authority on what exists**; repeating a class list here would only create a second thing to keep in
step with it.

What this section is for is the part the code cannot say about itself: how [§12](12-glossary.md)'s
two distinctions survived being turned into C#, and — just as important — which of §12's concepts
have **no code at all**, so that a reader can tell *not built yet* from *missing*.

Money handling is not repeated here. It is [§8.2](#82-money-handling) and
[ADR 0003](../decisions/0003-money-representation.md).

### Both dimensions are built, and a balance is not a field

§12 opens with every amount having a **location** (an *Account*) and a **purpose** (a *Category*).
**Since the accounts increment (2026-09-27) both exist in code**
([§12](12-glossary.md), *Accounts and net worth*; [ADR 0008](../decisions/0008-balance-is-worked-out.md)).

- **`Account` is a class with identity, like `Category`,** and holds a name and nothing else. Only the
  ledger sets the name, and the name rule is shared with categories (`NameRule`). An account and a
  category may share a name, because each has its own index.
- **Every `Expense` and `Income` names an account**, the pool account unless another was chosen.
  `Ledger.PoolAccount` is always exactly one account, so the constructor takes its name: an empty
  ledger cannot have none. `Accounts` lists it first.
- **Four entry kinds implement `IEntry`**: `Expense`, `Income`, `Transfer` and `BalanceCorrection`.
  They draw their ids from one counter, and the id is the recording order across kinds. A change
  keeps the id. **Five since the backing increment**, with `Movement` (next subsection).
- **No balance is stored.** `BalanceOf` is the latest typed balance, by date then id, plus every
  entry on the account that it does not hold (`Holds`: dated before its day, or on its day with a
  lower id) and that is dated today or earlier. With no typed balance it is the plain sum. `NetWorth`,
  `IsOverdrawn`, `DifferenceOf` and `HistoryOf` are worked out the same way, on every call. That is
  the purpose side's rule, *worked out, never stored*, applied to the location side.
- **Accounts touch no budget figure.** *Unassigned*, every *Budget* and every *Remaining* answer
  exactly as before. The two dimensions still vary independently.
- **The acts hold "shows, never blocks" by their result types**, as the category acts do.
  `AddAccountResult`, `RecordTransferResult`, `ChangeTransferResult` and `CorrectBalanceResult`
  carry one refusal each, in a fixed order, and nothing refuses for an overdraft. Misuse the screen
  cannot reach throws, as before: an account not in the ledger, deleting a used account or the pool,
  making the pool the pool.
- **Backing is built**, since the backing increment (2026-09-27), and is where the two dimensions
  meet (next subsection). Until then this bullet read: "Backing is still absent, not stubbed."

#### How this section read until 2026-09-27

Kept as written, because it records why the location dimension was left out for eight increments,
and the reasoning, a half-built dimension being worse than none, is why backing is left out now.

> **Only one of the two dimensions is built.** Only purpose exists in code.

The location dimension is **absent, not stubbed.** There is no `Account` type, no account field on
`Expense` **or on `Income`**, no nullable placeholder waiting to be filled, and no hidden default
account that a balance could be computed against. Searching the source for "account" finds doc
comments saying it is not here and nothing else.

Income made the gap wider rather than different, and [§11](11-risks-and-technical-debt.md) records
that: an expense at least names a category, while an income names neither a category nor an
account, so an `Income` is an amount, a date and a label and nothing more.

That is a deliberate shape rather than an unfinished one. A half-built dimension — a nullable
account, a `DefaultAccount` constant — would let code start depending on a model nobody has
designed yet, and every §12 rule about accounts (backing, the pool account, the sweep destination)
is a rule about a *relationship* that cannot be expressed by a field left blank. The cost is that
money leaving the system has nowhere to leave from, which is the last row of
[§11](11-risks-and-technical-debt.md) and is accepted there.

**Settled for the accounts increment on 2026-09-27**, and then built the same day: see above. A note
written between the two said that how the balance ruling is expressed in the domain and kept on disk
was for the plan. The plan answered it with ADR 0008.

### Where the dimensions meet: backing, movements and settling

Since the backing increment (2026-09-27; [§12](12-glossary.md), *Backing and Accumulated*;
[ADR 0009](../decisions/0009-movements-are-entries.md)). The rulings are in §12. What the code adds
to them:

- **Money MoneyBud moves is an entry, not a change to a balance.** `Movement` is a fifth `IEntry`: a
  date, the category, from, to, a positive amount, a `MovementReason` and a `MovementDirection`. It
  is written on the day the money moves and never changed. So "no balance is stored" holds with an
  automatic writer too: `BalanceOf` adds movements as it adds transfers. The **direction** is stored
  because the accounts cannot say it. When the pool account backs a category, a movement goes from
  the pool to the pool, and only what it was for says whether it adds to *Accumulated* or takes from
  it.
- **Settling is the one thing the ledger does because a period began.** `Ledger.Settle` writes the
  money planned for each period that has begun since `settledThrough`, with the backing and pool
  account of that day, and moves `settledThrough` to today. Every act that changes the ledger calls
  it first. So "nothing acts when a period opens" (the carry-over row, below) is no longer true
  without exception, and this is the exception. It is kept to one member with one piece of state,
  and the sweep is meant to use the same step. The runtime is in [§6](06-runtime-view.md). **Since the
  sweep increment it does** (next subsection). **Since the recurring-entries increment it also acts
  because a day came**, recording occurrences (*Recurring entries: state beside the entries*, below).
- **Two figures, two questions, and neither stored.** `ThereFor` is location-side: what is in the
  backing account for the category since that account became its backing, counting only expenses
  paid from that account. `AccumulatedFor` is purpose-side: what has moved in since the category was
  last backed, counting its expenses on any account, up to the period asked about, plus what is
  planned but not yet settled. They differ on purpose ([§12](12-glossary.md), *Backing can be set,
  changed or removed at any time*, follow-up). `ThereFor` counts movements by direction, so money
  moved from the pool to the pool while the pool backed the category is still there for it, and
  re-pointing takes it along. Ruled that way after the build, superseding the plan's wording
  ([§12](12-glossary.md), *Backing: ruled after the build*, ruling 1).
- **One "after this" rule, `EntryMark`**, a date and an id. A balance correction holds every entry not
  after it ([ADR 0008](../decisions/0008-balance-is-worked-out.md)). The expenses that lower
  *Accumulated* are those after the backing. `Backing` carries two marks, `AccumulatingSince` and
  `HereSince`, because the two figures count from different moments: re-pointing resets only the
  second. A mark draws its id from the entries' counter without being an entry, so it orders against
  an expense recorded the same day.
- **"Shows, never blocks" is held by the result type again.** `SetBackingResult` has no refusal. Its
  `BackingOutcome` is `Backed`, `Repointed`, `Unbacked` or `Unchanged`, and it carries the movement,
  if one was written. `Unchanged`, the backing already set, is recognised **before** settling. It
  exists because the screen's lists write back what they show, and that must be safe, like an
  unchanged save of an entry. A name that is no category, or an account from another ledger, throws:
  both are picked from lists, so the user cannot reach them.
- **`Assign` stays the only writer of a *Budget*, and gains a second write.** For a backed category in
  the current period, it also writes one movement: the amount in, or for a negative amount the smaller
  of what the clip let through and `ThereFor`, back out. A later period's budget moves only when that
  period is settled.
- **Used, and deletable, read the movements.** `CanDeleteAccount` is false while an account backs a
  category or has a movement on it. `CanDelete` for a category is false while a movement between two
  **different** accounts stands for it (`m.From != m.To`), and `DeleteCategory` removes its remaining
  pool-to-pool movements with it. As first built, any movement blocked deleting; `spec-reviewer`
  found that too broad, and the stakeholder ruled the narrower rule after the build. It extends
  "history" for backing without bringing back "never touched" ([§12](12-glossary.md), *Backing:
  ruled after the build*, ruling 2).
- **`HasBudget` is not a fourth reader.** Settling and `AccumulatedFor` read the budgets' figures,
  `> 0`, and do not call it (*The two layers meet in exactly one method*, below).

### The sweep: a movement for a period, and what settling now records

Since the sweep increment (2026-09-28; [§12](12-glossary.md), *The sweep and Restant*;
[ADR 0010](../decisions/0010-sweeps-and-period-ends.md)). The rulings are in §12. What the code adds
to them:

- **A sweep is a `Movement`, not a new kind of entry.** `MovementReason.Swept`, with `SweptFor`, the
  first day of the period it was for, set exactly when the reason is `Swept`. It goes `In` to the
  destination or `Out` back from a category, never `Along`. So balances, histories, *Opgebouwd* and
  `ThereFor` count sweeps with no rule of their own, and a sweep from the pool account to itself
  changes no balance and has no history row, as other movements do.
- **Settling does one more thing, and still one thing only at a boundary.** At each period end it
  passes, `Settle` writes a **period-end record**, the set of categories backed at that moment,
  then sweeps that period if a destination is set and its difference is above zero, then moves the new
  period's planned money. The record is written **even when nothing moves**, because the *Restant* is
  worked out again, under that set, after every late entry. The sweeps made are kept in a list that
  `TakeSweepsMade` empties, so the ledger decides what moved and the screen decides what to say.
- **The *Period leftover* is worked out, never stored.** `PeriodLeftover` is `UnassignedIn` plus the
  `RemainingFor` of every category not backed at the period's end, archived ones included. "Backed at
  the end" is the period-end record; with none, a period that ended before the ledger was made had
  nothing backed, and a period whose end settling has not passed is judged by today's backing (§12,
  *Sweep: chosen in the build*). So the first start is not stored: it is the absence of a record.
- **The difference, the line and what can come back are worked out too.** `DifferenceFor` is
  `max(0, Restant) − swept`, net, and below zero only beyond what was let go (§12, *Sweep: ruled at the
  build*, ruling 2). `SweepLineFor` picks **one** line in a fixed order, still to sweep, swept too
  much, swept, shortfall, and returns null for the current period and later ones, which show no
  preview. `UndoableSweepsFor` lists the moves into categories that can still be undone, latest first
  by id, each with what is left of it, counting only a category still backed as it was when the money
  went in (its `AccumulatingSince` mark before the move). **The only stored state is the let-go amount
  per period**, because the cap it records, what is there for the category, grows again afterwards.
- **"Shows, never blocks" is held by the result types again.** `SetSweepDestinationResult` has no
  refusal: `Chosen`, `Removed` or `Unchanged`, and it carries the destination before and after, so the
  screen can say what changed. `Unchanged` is recognised **before settling**, as `SetBacking`'s is,
  because the list writes back what it shows. `BringUpToDateResult` lists the moves made and what was
  let go. A category that is unknown, unbacked or archived as a destination throws, and so does
  `BringUpToDate` where the line offers no button: none of these is on screen.
- **The destination is cleared by the acts that take its backing away.** `SetBacking(…, null)`,
  `ArchiveCategory` and `DeleteCategory` set it to none; re-pointing keeps it. The screen compares the
  destination before and after the act to say so, and the domain's result types stay as they were.
- **Used, and deletable, read the sweeps too.** `CanDelete` and `CanDeleteAccount` count **any** sweep,
  one from an account to itself included, where other movements from an account to itself still do
  not count (scenario-stage ruling 7 for categories; the build, for accounts). `DeleteCategory` also
  takes the category out of every period-end record.
- **`HasBudget` is still not a reader.** The *Restant* reads `UnassignedIn` and `RemainingFor`, which
  read the figures.

### Recurring entries: state beside the entries, and settling day by day

Since the recurring-entries increment (2026-09-28; [§12](12-glossary.md), *Recurring entries*;
[ADR 0011](../decisions/0011-recurring-entries.md)). The rulings are in §12. What the code adds to
them:

- **An occurrence is an ordinary entry.** `Expense` and `Income` keep their shape. Nothing that works
  out a balance, a *Budget* figure, *Remaining*, *Unassigned* or the *Restant* knows whether an entry
  repeats, so every rule about a recorded entry, a balance correction holding it included, applies to
  an occurrence with no new code.
- **The repeat is state beside the entries, in `Recurring.cs`.** `Frequency` is `Weekly` or `Monthly`.
  **One-off is `null`**, the absence of a repeat rather than a third kind, so an entry left at it is
  exactly the one-off entry of every earlier increment. `RecurringEntry`, internal to the domain,
  holds the ids of its occurrences, its `Frequency?` (null once stopped), its `Day` (a monthly one's,
  null otherwise) and its `Next` date (null once stopped). It is kept because no single entry can say
  any of those: after 28 February no date says "the 31st", the next date must outlive the removal of
  the latest occurrence, an earlier occurrence must be known as one to open locked, and a stopped
  repeat must still be a repeat. `SetFrom(date, frequency)` is **the only thing that sets the day**, so
  a date MoneyBud clamped to a short month's last day never moves it (§12, ruling 5 read with
  follow-up 3). Weekly adds 7 days; monthly takes the kept day in the next month, or that month's last
  day.
- **The latest occurrence is the highest id, and is not stored.** The id is the recording order
  (*An entry has an id*, below), and an occurrence always takes the next one, so the last id in a
  repeat's list is the one recorded most recently. Changing an earlier occurrence's date past the
  latest's does not make it the latest. Removing the latest hands the role to the one before, with
  nothing to update.
- **Two queries hold the drop-down, and the domain enforces the lock.** `FrequencyOf(entry)` is the
  frequency for the latest occurrence of a running repeat and null for every other entry: it is both
  the value the *Herhalen* list loads and the grey label. `SetsTheRepeat(entry)` is true for a one-off,
  the latest occurrence and a stopped repeat's last one. For any other entry the frequency handed to a
  change is **ignored**, and the change is to that entry alone.
- **A frequency is one more field of a change.** `ChangeExpense` and `ChangeIncome` gained an overload
  that takes a `Frequency?`. The overload without it passes `FrequencyOf(entry)`, so existing callers
  keep the frequency as it is; a changed date on the latest occurrence still moves the day, as
  follow-up 3 rules for any change of that date. The plan had offered a `RepeatChoice?` wrapper or an overload, because
  `null` already means one-off and cannot also mean "unchanged"; the build chose the overload. The
  unchanged check includes the frequency only for an entry that sets the repeat, so a frequency alone
  is a change and a locked entry saved unchanged is still unchanged. The repeat is touched only after
  the change has gone through, so **a refused change leaves the repeat as it was**. Setting it to
  one-off stops the repeat; a new frequency or a new date on the latest sets the day and next date
  from its date; a frequency on a stopped repeat's last starts it again.
- **Removing takes the id out.** The next date does not move, and a repeat left with no occurrence is
  dropped: the repeat ends.
- **Settling becomes a third writer, of incomes and expenses, and acts because a day came.** Until
  now it acted only because a period began (*Where the dimensions meet*, above). `Settle` now steps
  from event to event: the earliest due occurrence if it falls before the next boundary, otherwise
  the boundary. So an occurrence is recorded before its period is swept, and on a boundary day after
  the boundary. The once-a-day early return is gone, because a repeat set up or moved back in the
  past has occurrences due on days already settled through. The runtime is in
  [§6](06-runtime-view.md), *Settling day by day*.
- **An occurrence is never checked, and still never in the future.** It copies the latest occurrence
  and cannot be refused. Only an occurrence whose date has come is recorded, so "a future expense is
  never recorded" holds without a check. One on an archived category brings it back, as recording by
  hand does (§12, ruling 8). `OccurrenceMade` carries the entry and the category brought back, and
  `TakeOccurrencesMade` empties the list, as `TakeSweepsMade` does: the ledger decides what was
  recorded, the screen what to say.
- **"Shows, never blocks" needs nothing new.** No act gained a refusal. A refused recording sets no
  repeat up.
- **`FromSnapshot` checks the repeats**: each has occurrences, all existing expenses or all existing
  incomes; no entry is in two repeats or twice in one; a running one has a frequency and a next date,
  a stopped one neither; a monthly one has a day from 1 to 31, any other none; and the frequency is
  one MoneyBud knows.
- **`HasBudget` is still not a reader.** An occurrence touches no *Budget*, and does not change whether
  a period is offered a plan.

### The two layers meet in exactly one method

§12's second distinction — the **plan** and the **actual** — is the one the code is arranged
around. `Ledger` holds both and keeps them apart:

| §12 layer | In code |
|---|---|
| **The plan** — a category's *Budget* for a period | `Ledger.BudgetFor(category, period)`, over budgets stored per category per period |
| **The actual** — what was really spent | `Ledger.SpentOn(category, period)`, the sum of the `Expense` records whose date falls in that period |
| **Where they meet** | `Ledger.RemainingFor(category, period)` — the plan minus the actual, and the **only** place the two are combined |
| **Neither layer** — the pool a plan is made *out of* | `Ledger.UnassignedIn(period)`: the `Income` records dated in that period, minus every *Budget* in it. Income is not a plan and not a spend, so it sits outside both rows above rather than inside either |
| **The one act that writes the plan** | `Ledger.Assign(amount, category, period)`, which moves an amount out of *Unassigned* and onto a *Budget*. Nothing else writes a budget (*Assigning is the only way to write a plan*, below) |

Three of §12's rules are structural rather than checked, which is why they need no code of their
own:

- **Recording an expense never touches a budget, and assigning never touches an expense.**
  `RecordExpense` appends an `Expense` and does nothing else; `Assign` writes a budget and does
  nothing else. "Assigning spends nothing" and "spending does not re-plan" are both true because
  neither operation can reach the other's storage. **Since the backing increment `Assign` also
  writes a movement for a backed category**, which is location, not the actual layer: it still
  touches no expense, and recording an expense still touches no budget and writes no movement.
- **Recording an income touches neither layer.** `RecordIncome` appends an `Income` and stops.
  Because a budget is reachable only from `Assign` and an expense only from `RecordExpense`,
  *Recording income leaves every category's plan and spending untouched*
  ([`record-income.feature`](../../features/record-income.feature)) is a property of the wiring
  rather than an assertion anything has to uphold.
- **Correcting an entry touches no budget either.** `ChangeExpense`, `ChangeIncome`,
  `RemoveExpense` and `RemoveIncome` rewrite or drop one entry in its list and stop, apart from a
  change bringing an archived category back. So removing or lowering an income that leaves its
  period *Over-assigned* needs nothing to allow it: no budget moves to match, and none could, which
  is why a past period left that way stays that way ([§12](12-glossary.md), *An entry in a past
  period can be corrected*).
- **A category with no budget set behaves as one budgeted at zero.** `BudgetFor` returns
  `Money.Zero` when it finds nothing, and no figure distinguishes the two, so a missing budget has
  no way to block a recording. **One query does tell them apart**: `HasBudget` says whether
  anything was ever assigned to a category in a period, even if it was since taken back to zero. It
  exists so that a scenario can state *I have never set a budget* as a precondition. No figure and
  no refusal consults it. To keep it honest, an assignment that changes nothing — zero, or a
  negative clipped against a *Budget* already at zero — **writes nothing**, so it cannot make
  `HasBudget` true. The clip case once did, by storing a zero; `spec-reviewer` found it, and it was
  fixed before the increment closed. **The UI must not consult it either, and it does not.** The
  Overview's ring treats "no budget" as a *Budget* of zero, however it got there ([§12](12-glossary.md),
  *The overview, and its ring*), so a ring built on `HasBudget` would draw a difference §12 says
  does not exist. `PeriodOverview` and `Ring` read `BudgetFor` and nothing else, and no production
  code calls `HasBudget` at all. **Only the specs use it**: the *I have never set a budget* and
  *I have set no budget* *Givens*, and one *Then*, the first-start check that no category has a
  budget or any spending, plus two unit tests. The second came with the corrections increment: it
  uses `HasBudget` to show that a budget taken back to zero is still stored, and that deleting the
  category takes it away. **This stays a watch-out.** A query that can tell
  apart two states §12 says are one is safe only while nothing a user sees is built on it. If a
  view ever wants it, revisit §12 first. **Since the persistence increment the distinction is also
  kept on disk**: a stored budget of zero is written and read back, so `HasBudget` answers the same
  after a restart. That keeps a restart from changing any answer, and it means the file, too, holds
  a difference §12 says the user never sees. **The corrections increment added a second place it must
  not reach**: whether a category can be deleted. §12 settles that on figures alone, a budget of more
  than zero or an expense in any period, and rejected the rule `HasBudget` would give
  ([§12](12-glossary.md), *Deleting a category that has no history anywhere*). **Built that way:**
  `Ledger.CanDelete` reads the expenses and the budgets of more than zero directly, and
  deliberately does not call `HasBudget`. So a category assigned to and then taken back to zero can
  be deleted, as an approved scenario asserts. Its stored budgets of zero are not history, and
  `DeleteCategory` drops them with it, so nothing of a deleted category is left behind for
  `HasBudget` or anything else to find. **Opening a period added a third**: whether a period has a
  plan. It decides whether an earlier plan is offered to a period and whether an earlier period can
  be the one offered. §12 settles both on figures: every *Budget* zero, and a *Budget* of more than
  zero for a category not archived now. Assigning €100 and taking it back must bring the offer back
  ([§12](12-glossary.md), *Opening a period*). A build on `HasBudget` would not, because the stored
  zero would still be there. **Built that way:** `Ledger.PlanOfferedIn` reads the stored budgets'
  figures directly, `> 0` in both tests, and deliberately does not call `HasBudget`. So the stored
  zeros a taken-back budget leaves, on disk as well as in memory, cannot hold an offer back. Three
  places now read figures where `HasBudget` would read a difference, and still no production code
  calls it.

*Over budget* follows from *Remaining* alone — a negative `RemainingFor`. Exactly zero is not
negative, so §12's "spending a category down to nothing is the plan working" needs no special case
either. ***Over-assigned* follows from *Unassigned* the same way**: `IsOverAssigned(period)` is a
negative `UnassignedIn`, derived where it is asked for and never stored, and exactly zero is every
euro having a job rather than one too many.

### `UnassignedIn` was named for the figure, not for the arithmetic it did then

§12 defines *Unassigned* as a period's income **minus everything assigned to categories in it**.
When the income increment built it, nothing assigned, so the subtraction had nothing to subtract
and the method returned the period's income and nothing more. It was called `UnassignedIn` anyway.

**`IncomeIn` was the alternative and was rejected at the plan gate**, for a reason worth keeping:
*Unassigned* is what the scenarios assert and what the user will eventually be shown, whereas "the
period's income" was only how that figure happened to be computed while one of its two terms was
missing. Naming the method after that arithmetic would have meant renaming it — and every call site
and every step definition with it — at the exact moment assigning arrived and the code was already
changing.

**The assigning increment bore that out.** `UnassignedIn` gained its subtraction and kept its name,
its signature and its callers. It now subtracts **every** *Budget* in the period, **archived
categories' included**: archiving says nothing about money ([§12](12-glossary.md)), so an archived
category's *Budget* is still assigned money until it is taken back out.

The list and the figure are deliberately two members: `IncomesIn(period)` returns the `Income`
records, `UnassignedIn(period)` returns the amount. Only the second changed when assigning landed.
That separation mattered in one step binding as well. *Given I have recorded no income* used to
check for an *Unassigned* of zero. That stopped meaning "no income" once a period planned before
any income arrived could be below zero, so the step now checks for no `Income` records.

### Assigning is the only way to write a plan

In §12 a *Budget* is what **assigning** from the pool produces. `Ledger.Assign` is that act, and
since the assigning increment it is the **only** member that writes a budget.

**What it replaced.** For the first three increments, `Ledger.SetBudget` wrote the plan directly,
as a scaffold for the scenarios. It took a `Money` and stored it for any period, with no pool to
draw from, no floor and no clip. Handed an archived category, it did the thing that decided least
and brought nothing back. That disagreed with a rule decided on 2026-09-25
([§12](12-glossary.md), *Assigning to an archived category brings it back*). This section recorded
the gap and said the assigning increment must **retire `SetBudget` or align it**.

**It was deleted, not aligned.** Aligned, it would have had to draw from *Unassigned*, floor, clip,
refuse a past period and bring an archived category back. That is `Assign` under a second name.
Left as it was, it would have been a second door able to make states the rules forbid: a budget
made today in a past period, a budget that never left the pool, a plan for an archived category
that stayed archived. A scenario set up through that door starts from a state the user cannot
reach, and passing on top of it proves nothing about MoneyBud. So there is one writer, and the
specs use it too.

**Two things in approved scenarios stood in the way of simply retiring it.** Both were about what
an approved *Given* means. The first was settled in the approved plan, as a matter of how the
specs produce a state; the second changed an approved scenario, so it was approved at the scenario
gate, together with the assign scenarios themselves.

- **Budgets set in a past period.** [`record-expense.feature`](../../features/record-expense.feature)
  and [`archive-category.feature`](../../features/archive-category.feature) set up budgets "in the
  previous budget period". `Assign` refuses a past period ([§12](12-glossary.md), *Assigning
  happens in the current budget period and later ones*). The state is still real: it was made when
  that period was current, and [`assign-to-category.feature`](../../features/assign-to-category.feature)'s
  header now says that is what such a *Given* means. **The binding does exactly that.** It moves the
  test clock to the first day of that period (`SpecContext.AsIfToday`), assigns through
  `Ledger.Assign`, and puts the clock back. The rejected alternative was a test-only door into the
  domain: a setter, or an internal member visible to the specs. Either would have been `SetBudget`
  again under a safer-sounding name. Moving the clock needs nothing from the domain, because the
  clock is already passed in (`TimeProvider`, [§5](05-building-block-view.md)).
- **Budgets that never left the pool.**
  [`record-income.feature`](../../features/record-income.feature), *Recording income leaves every
  category's plan and spending untouched*, set €460 of budgets, recorded a €2,000 income, and
  asserted *Unassigned* was €2,000. That held only because `SetBudget` bypassed the pool. Either the
  budgets were assigned and the figure is €1,540, or they were not and the ledger held budgets
  nobody assigned, which §12 has no word for. **The first reading was taken.** The scenario now
  states *Unassigned* is −€460 before the income and asserts €1,540 after it.

**Every budget *Given* checks what it made**: that the assignment went through, that nothing was
clipped, that no category was brought back, and that the resulting *Budget* is the figure written.
The last check is needed because assigning **adds**. Two budget *Givens* for one category and period
would set up their sum, and the step fails rather than let a scenario start from a figure it does
not state. Setup goes through the same door as the scenario under test, so a setup the rules would
refuse, clip or turn into a bring-back fails loudly instead of quietly making some other state.

**Taking a plan over goes through the same door.** This section once argued, ahead of the build and
not as a ruling, that when carry-over was built, taking an earlier plan over should be an assignment
of each figure rather than a second writer. The opening-a-period increment built it that way.
`Ledger.TakeOverPlan` checks for a past period first, then calls `Assign` once per figure in the
plan, into the period given. It writes no budget itself. So the floor, the past-period refusal and
the rule that writing nothing leaves no mark all hold for a take-over without being repeated. It
also checks what each `Assign` returned, and throws if one was refused, clipped or brought a
category back: none of those can happen to a figure in a plan, so any of them would mean the offer
and the ledger disagreed.

### "MoneyBud shows, it never blocks" is held by a type

`RecordExpenseResult` and `RecordIncomeResult` each have exactly two shapes: recorded, or refused
for exactly one reason. Neither has a third, deliberately. A "recorded with a warning" or "needs
confirming" outcome **has nowhere to live**, so the scenario *I should not be warned or asked to
confirm* holds by construction rather than by anyone remembering the rule.

**The same argument now carries two different scenarios**, which is why it is worth having twice:
in [`record-expense.feature`](../../features/record-expense.feature) it is spending past a budget
that goes unremarked, and in [`record-income.feature`](../../features/record-income.feature) it is
an income dated in the future. Two unrelated rules, one structural reason.

**The category acts are held the same way.** They add a third place where the rule is carried by a
signature, and a second rule, "every category act tells its outcome", which is carried the same way
([§12](12-glossary.md), *Archiving is announced, never confirmed*):

| Member | Its shape | What the shape holds |
|---|---|---|
| `Ledger.AddCategory` → `AddCategoryResult` | Exactly **four** outcomes: `Created`, `AlreadyThere` or `BroughtBack`, each handing back the category, or **refused** with `CategoryRefusal.NameMissing` | Adding a name you already have is **not a refusal**, because the end state is already true. *Already there* and *brought back* are different outcomes because they tell the user different things. A name that trims to nothing is the only refusal |
| `Ledger.ArchiveCategory` → `Category` | Returns the archived category, so the user can be told what was archived, spelled as MoneyBud has it. **No confirmation parameter and no third outcome** | "Archiving is never confirmed" holds **by signature**: there is nowhere to put a question. It **throws** for a name that is not one of your categories and for a category already archived. §12 defines no user-facing behaviour for either, so reaching one is a mistake in the caller, not a situation to report to the user |
| `RecordExpenseResult.CategoryBroughtBack` | A flag on a **recorded** result. Always false on a refused one | Information after the fact, **not a third outcome and not a warning**. Recording still has exactly two shapes. This only says that recording brought an archived category back, which nothing else about recording an expense would show |
| `Ledger.Assign` → `AssignResult` | Exactly **two** shapes: assigned, handing back the category, or **refused** for one `AssignRefusal`. Two facts ride on an assigned result: `Shortfall`, how much of a negative amount could not come back, and `CategoryBroughtBack`. Both are zero or false on a refused one | Going *Over-assigned* produces a plain assignment, because there is nowhere else for it to go. `Shortfall` is the clip being **said rather than absorbed** ([§12](12-glossary.md), *An over-large negative assignment is clipped*). Like `CategoryBroughtBack`, it is information after the fact, not a warning and not a third outcome. `CategoryBroughtBack` is only ever true for a **positive** amount, and only once every check has passed. It **throws** for a `BudgetPeriod` that is not one of the ledger's calendar periods, such as a hand-made date range. A user picks a period from the calendar and cannot reach that, so, as with archiving, it is a caller's mistake rather than a situation to report |

**The corrections increment held its acts the same way**, and added one thing the others did not
have: an outcome that is **neither success to announce nor refusal**.

| Member | Its shape | What the shape holds |
|---|---|---|
| `Ledger.ChangeExpense` → `ChangeExpenseResult`, `Ledger.ChangeIncome` → `ChangeIncomeResult` | **Three** shapes: `ChangeOutcome.Changed`, `ChangeOutcome.Unchanged`, each handing back the entry as it now is, or **refused** for one of **recording's own** reasons, `ExpenseRefusal` or `IncomeRefusal`. `ChangeExpenseResult.CategoryBroughtBack` rides on a changed result, as on a recorded one | Reusing recording's refusal types is "a change is judged as if recorded now" ([§12](12-glossary.md)) said by a signature: there is no refusal a change can have that recording cannot. **`Unchanged` is not a refusal and not a warning.** It exists only because a change is announced and a save with nothing changed is not, so the screen has to tell them apart. **An unchanged save is recognised before any check runs**, so it cannot be refused by construction, whatever the rules have come to say about the entry since. `CategoryBroughtBack` is true only when the change moved an expense **onto** an archived category, never for fixing one already on it. Both **throw** for an entry that is not in the ledger, such as one already removed: an entry is changed from its row, so the user cannot reach that |
| `Ledger.RemoveExpense`, `Ledger.RemoveIncome` → nothing | **No confirmation parameter and no result**. The question is asked by the screen, and these are called only once the user has said yes | Removing **is** confirmed ([§12](12-glossary.md), *Removing an entry asks first*), unlike archiving. The confirmation lives in `MoneyBudApp` ([§8.4](#84-the-presentation-layer)) and not in the domain, because it is a question put to a person, and a domain method cannot wait for an answer. Both **throw** for an entry not in the ledger |
| `Ledger.RenameCategory` → `RenameCategoryResult` | **Three** shapes: `RenameOutcome.Renamed`, carrying the category and its `OldName` as it was stored, `RenameOutcome.Unchanged`, or **refused** with `RenameRefusal.NameMissing` or `NameTaken` | A new **spelling** of the category's own name is `Renamed`, not `Unchanged`. Only the name exactly as stored changes nothing, and that is quiet, like an unchanged entry. `OldName` is on the result because the announcement says what the category was called, and the category itself no longer knows. **Throws** for a name that is not one of your categories: a category is renamed from its row |
| `Ledger.CanDelete`, `Ledger.DeleteCategory` → `Category` | A query, and an act that returns the deleted category so that it can be named. **No confirmation parameter** | "Deleting is never confirmed" holds by signature, as archiving does. `DeleteCategory` **throws** for an unknown name and for a category with history, because the delete button is offered only where `CanDelete` is true |

Every "throws" in this increment is one of §12's non-cases: something the screen offers no way to
do. None of them is a refusal the user could meet.

**Opening a period added one query and one act, held the same way.**

| Member | Its shape | What the shape holds |
|---|---|---|
| `Ledger.PlanOfferedIn` → `PlanOffer?` | The plan offered, or null. A `PlanOffer` is the source period and one `PlanFigure` (a category and an amount) per category in the plan, in order added, with `Total` and `FigureFor(category)`. **Worked out on every call, never stored** | §12 reads the offer as a state, not an event, and the type says so: there is nothing to open, close or remember at a period boundary, and "current" and "archived now" are read afresh each time. The source is found from the stored budgets directly, as the latest period start before the given period with a *Budget* above zero for a category not archived now. No walk back period by period, and no limit. A figure holds the `Category`, not its name, so a category renamed since is offered under its new name |
| `Ledger.TakeOverPlan` → `TakeOverPlanResult` | **Two** shapes, like `AssignResult`: taken over, carrying the `Plan` and the period it went `Into`, or **refused** with an `AssignRefusal`, of which only `PeriodInPast` can occur | Reusing assigning's refusal type says that a take-over *is* assigning. **No outcome for going *Over-assigned***, because a take-over assigns in full whatever *Unassigned* holds, and that has nowhere else to go. The one refusal is reachable for up to a minute after a period boundary, before the screen redraws ([§8.4](#84-the-presentation-layer)). It **throws** when no plan is offered, since the button is shown only while one is |

Both **throw** for a `BudgetPeriod` that is not one of the calendar's own, as `Assign` does. The three
now share that check, one private `CheckIsAPeriod`, rather than each repeating it.

Refusals are `ExpenseRefusal`, `IncomeRefusal`, `CategoryRefusal`, `AssignRefusal` and, since the corrections increment, `RenameRefusal` **values, not messages**. Taking a plan over added no refusal type of its own. The wording the user
sees belongs to the UI; putting copy in the domain would put it in the wrong place and would make
re-wording it a domain change. The UI increment settled that the wording is **Dutch**
([§12](12-glossary.md), *The UI is in Dutch*), and built it that way. Every Dutch sentence is in
`Tekst`, in the presentation layer ([§8.4](#84-the-presentation-layer)), and the domain keeps its
reasons. **The arrangement held when tested:** the UI increment wrote every refusal in Dutch without
changing a line of any refusal type.

**`IncomeRefusal` has no `DateInFuture` member, and the absence is the decision.** `ExpenseRefusal`
has one, so the asymmetry is visible in the source and looks exactly like an oversight to anyone
who has not read §12 — and the obvious "fix" would silently delete a capability. An income *may* be
dated forward, and it joins its period's *Unassigned* from the moment it is recorded rather than
from its date. The reason the two differ is the plan/actual split: a future expense is already
expressible as a *Budget*, while the model has no planned income at all, so future-dating is the
only way to state an amount that is coming ([§12](12-glossary.md), *Income may be dated in the
future; an expense may not*). `RecordIncome` therefore does not look at the date at all.

**`AssignRefusal` has no `AmountNotPositive` member, for the same kind of reason.** Both
transactions refuse zero and below, so its absence here looks like the same oversight. It is the
rule: an assignment may be negative, which moves money back, and may be zero, which moves nothing
([§12](12-glossary.md), *Assigning zero is accepted and moves nothing*). Its four members are about
the **target** and the cent rule only. They are declared in the order a broken rule is reported:
`CategoryMissing`, `UnknownCategory`, `AmountFinerThanCent`, `PeriodInPast`. That is the order
recording an expense uses. A zero or clippable negative amount does not rescue an assignment whose
target is wrong.

### One label rule, held by one method

§12 argues that trimming a label and requiring one are **a single rule**: something has to trim
`"   "` in order to judge it blank, so a stored label that was not trimmed would leave the two
halves disagreeing about what a label is. The code holds that as one private `Ledger.NormaliseLabel`
that both `RecordExpense` and `RecordIncome` call. There is no second place a label could be
normalised differently.

**Changing an entry reuses the same checks rather than repeating them.** The corrections increment
extracted recording's checks into one private method per transaction, `CheckExpense` and
`CheckIncome`, and recording and changing both call them. So a change is refused on exactly
recording's rules, in recording's order, and a rule added to recording later reaches changing
without anyone remembering to add it twice ([§12](12-glossary.md), *A changed entry is judged as if
it were recorded now*).

**This changed the expense side, not only the income side.** An expense's label is now trimmed too,
and one that trims to nothing becomes no label — which an expense is allowed to have. What differs
between the two transactions is only what each does with an empty result: an income refuses it, an
expense accepts it. That is a difference in the requirement, not in what a label *is*, and keeping
it to one method is what stops it becoming two.

### An entry has an id, issued by the ledger

Since the corrections increment, `Expense` and `Income` each carry an `Id`, issued by the `Ledger`
from one counter when the entry is recorded. **Before that, an entry had no identity beyond its
values**, and nothing needed one: entries were only ever added. Changing and removing need to say
*which* entry, and values cannot:

- **Two identical entries are two entries.** Removing one of them must leave the other, and
  approved scenarios in [`remove-an-entry.feature`](../../features/remove-an-entry.feature) say so,
  for expenses and incomes both.
- **A change is a rewrite in place.** `ChangeExpense` and `ChangeIncome` find the entry by its id and
  replace it where it stands in the ledger's list, keeping the id. The list is in the order
  recorded, so a changed entry keeps its place among entries on the same date, which the approved
  scenarios require ([§12](12-glossary.md), *A change overwrites the entry*). Removing and re-adding
  would have put it last.
- **A stale copy still finds its entry.** `Expense` and `Income` are immutable records, so the
  screen may hold an older copy than the ledger's. Lookup is by id, not by the whole value, so it
  still reaches the right entry.

**The id is not shown and means nothing to the user.** It was a per-run counter while nothing was
kept. Since the persistence increment the ids are kept, and so is the last id issued, so the counter
carries on across restarts and an id is never issued twice ([§8.3](#83-persistence),
[ADR 0007](../decisions/0007-keeping-the-ledger.md)).

### One category name rule, held by one comparer

[§12](12-glossary.md)'s name rule has two halves, what is **stored** and what is **compared**. The
code keeps them in two members of one static class, `CategoryName`:

| Half | Member | What it does |
|---|---|---|
| **Stored** | `CategoryName.Normalise` | Trims the ends and keeps everything inside exactly as typed, capitalisation and spacing both. Returns null when nothing survives, which is how a name that trims to nothing becomes no name |
| **Compared** | `CategoryName.Comparer` | Trims the ends, reduces every run of inner whitespace to one space, and ignores case |

**Case is ignored ordinally, never by the machine's culture.** A culture-sensitive comparison could
make the same two names one category on one machine and two on another. The Turkish dotless i is
the classic case. The rule exists so that nobody ends up with two categories they meant as one, so
it cannot depend on where it runs. "Whitespace" means what `char.IsWhiteSpace` means, the same
reading the label rule takes.

**There is one `Category` object per name, and everything else is keyed by that object, not by a
string.** `Ledger` keys its name lookup with `CategoryName.Comparer`. Budgets, expenses and the
archived set all hold the `Category` itself. So once a name has been found, no second string
comparison can disagree with the first. Every `Ledger` member that takes a name goes through one
private `Find`, so every spelling the rule matches is accepted everywhere. Recording against
"  groceries " records against Groceries.

**Since the corrections increment, `Category` has identity.** It was a record, compared by value.
It is now a class, compared by reference, and its `Name` can be set only inside the
domain. A category is one thing that has a name, where before it effectively *was* its name. That
is what makes a rename cheap and complete. `Ledger.RenameCategory` is one assignment to `Name` plus
re-keying the ledger's name index: the category is taken out under its old name and put back under
the new one. Every budget and expense already holds the instance, so every period, past ones
included, shows the new name with nothing else touched. That is [§12](12-glossary.md)'s *Past
periods show the new name everywhere*, and *nothing has to remember old names*, as a property of the
wiring. It is also why the old name is free at once: the index is the only thing that held it.

**Why the setter is domain-only.** The name index is keyed by the name, so a name changed anywhere
but in the ledger would leave the index pointing at a name the category no longer has. Keeping the
setter `internal` keeps the only writer next to the index it has to keep in step.

**No record was written for this** ([§9](09-architecture-decisions.md)), as the plan said. It
changes how one type expresses §12's model and moves no boundary. It did matter to storage,
though. Budgets and expenses could not be kept against a category's name, so the file gives each
category a key of its own, made at every save, and the domain needed no id for it
([§8.3](#83-persistence), *Answered by the plan*).

**Whether a category is archived is not on `Category`.** It is the ledger's set of archived
categories. Archiving is a fact about how the ledger uses a category, not about the category, and
an expense recorded against it last year should not change because the category was put away today.

**This subsection used to record a divergence, and the category increment corrected it.** Until
then `Ledger` keyed categories with `StringComparer.Ordinal`, and `ExpensesFor` matched with
`e.Category.Name == categoryName`. So "boodschappen" and "Boodschappen" were two categories, and so
were "Hobby " and "Hobby" and "Vaste  lasten" and "Vaste lasten". A name made only of spaces could be
added, and an expense against "  groceries " was refused as `UnknownCategory`. **Nothing had decided
any of that.** It arrived with the first increment's scaffold, where no approved scenario ever spelled
one category two ways. §12 then settled the rule, and this section named the code as disagreeing until
the category increment was built. It has been built, and the disagreement is gone. No scenario in
the two earlier feature files uses two spellings of one name, so the correction did not reach any
of their assertions.
`AddCategory` had also returned the existing category silently. It now returns an
`AddCategoryResult` that says which of its four outcomes happened (above).

### Where a category is shown: one domain query, used by the view and by the steps

[§12](12-glossary.md) settles the **whole** display rule (*When any category is shown in a period:
the full rule*). A category is shown in period P if it has history in P (a budget of more than zero,
or an expense), **or** if it is in use and P is the current period or a later one. Since the UI
increment it is **one domain query, `Ledger.CategoriesShownIn(period)`**. It returns the categories
in the order they were added, so a category brought back keeps its first place. "Current" is read
from the clock on every call, so a period that was current becomes past the moment the next one
begins, with nothing rebuilt around it.

**Since backing, a third clause** (ruled after the build, 2026-09-27): an archived backed category
is also shown in the current period and later ones while its *Accumulated* there is not zero. It is
`|| (plannable && AccumulatedFor(c.Name, period) is { Cents: not 0 })`, so it reads the figure the
row shows and nothing else. Keeping the rule in one query is what made this one line: the rows, the
ring and the "shown" steps all follow it.

**Why in the domain, and not in the view.** Which categories a period has anything to say about is a
fact about the model, and it is the same fact for any view: a second screen, a mobile one, a report.
How they are ordered on the Overview, largest *Budget* first, is the screen's, and lives in
`PeriodOverview` ([§8.4](#84-the-presentation-layer)).

**The steps read the view, not the ledger.** The "should (not) be shown in … period" steps used by
the archive scenarios and two assign scenarios now read the rows of the period's Overview. The same
goes for "shown as over budget" (the row's marker), "shown as over-assigned" (the marker beside
*Unassigned*) and "offered" (the suggestions). So a green suite describes what the screen lists. Before the
UI existed they combined `HasHistoryIn` and `IsArchived` in the step definitions, and only the
archived half of the rule existed anywhere. That gap was carried in
[§11](11-risks-and-technical-debt.md) and is now closed.

**How this section read before, kept because the reasoning changed twice.** When the category
increment was built there was **no "is shown" query**, because the rule was unsettled and an
`IsShownIn` would have had to guess the in-use half. Once §12 settled it on 2026-09-25, the query
was still absent, now because no view needed it. The UI's stepping between periods was the first
view that did, and the query was built with it. `HasHistoryIn` and `IsArchived` remain, as the
halves the full rule is made of.

### A first start is a door of its own

`new Ledger(...)` gives an **empty** ledger. `Ledger.StartNew(...)` gives what a first start
gives: the six default categories (`Ledger.DefaultCategoryNames`, [§12](12-glossary.md)) and
nothing else. The spec context builds every scenario on the constructor. Only the "first time"
steps call `StartNew`, and they refuse to run after any other setup. So a scenario that quietly
relied on Boodschappen being there would fail. Independence from the default set is **enforced**,
not hoped for.

**Since the accounts increment, "empty" has one account in it.** There is always exactly one pool
account, so `new Ledger(clock, poolAccount)` takes its name and has no categories and nothing
recorded, but has that account, with no starting balance. The spec context names it **"Bank"**, a
synthetic name, so no scenario leans on the first start's *Betaalrekening* either. `StartNew` adds
the six defaults to a ledger whose pool account is `Ledger.DefaultAccountName`, *Betaalrekening*
([§12](12-glossary.md), *Approved at the scenario gate, 2026-09-27*).

### `Ledger` is not a §12 term, and that is worth flagging

§12 has no word for *the whole model* — the thing holding the categories, the budgets, the expenses
and the incomes together. `Budget` was unavailable, because §12 pins it to the per-category plan.
`Ledger` was introduced in code to fill the gap.

**It should be revisited when accounts arrive.** In accounting a ledger is a book of *accounts*,
and *Account* is a §12 term for something MoneyBud deliberately has not got yet — so the name is
harmless now and will read oddly the moment the location dimension exists. Recorded here rather
than added to the glossary, because §12 holds the **domain's** vocabulary and this is a word the
implementation needed, not one the stakeholder uses.

**Accounts arrived on 2026-09-27, and the name was kept.** No rename was part of the accounts
increment's plan. The class now does hold accounts, so the name reads more naturally than it did.
Whether it should become something else is still unasked, and nothing depends on the answer.

### What has no code yet, and why

Everything below is in §12 and absent from `MoneyBud.Domain`. Nothing here is an oversight; the
reason differs by row, and the difference is the point.

**A row leaves this table when it is built.** *Income* and *Unassigned* were here until the income
increment shipped; `Ledger.RecordIncome` and `Ledger.UnassignedIn` now exist and
[`record-income.feature`](../../features/record-income.feature) is approved, so the entry is gone
rather than amended. *Archived* and the **default categories** left the same way when the category
increment shipped. `Ledger.ArchiveCategory`, `Ledger.StartNew` and `Ledger.DefaultCategoryNames` now
exist, with [`add-category.feature`](../../features/add-category.feature) and
[`archive-category.feature`](../../features/archive-category.feature) approved. *Assign* and
*Over-assigned* left when the assigning increment shipped: `Ledger.Assign` and
`Ledger.IsOverAssigned` exist, and [`assign-to-category.feature`](../../features/assign-to-category.feature)
is approved. **Carry-over** at period opening left when the opening-a-period increment was built:
`Ledger.PlanOfferedIn` and `Ledger.TakeOverPlan` exist, and
[`take-over-a-plan.feature`](../../features/take-over-a-plan.feature) is approved and green. It had
been put out of the assigning increment's scope as a slice of its own. Its row said that nothing
acts when a period opens, and that is still true of the offer: it is a state worked out whenever it
is asked for, not an event handled at the boundary. **Since the backing increment one thing does act
when a period begins**: settling, which writes that period's planned money for backed categories
(*Where the dimensions meet*, above). **The location dimension** left when the accounts
increment was built (2026-09-27): *Account*, *Location*, *Balance*, *Net worth*, *Overdrawn* and the
*pool account* as a default, with *Transfer*, *Balance correction* and *Starting balance*, which
arrived with it (*Both dimensions are built*, above). **Backing** left when the backing increment
was built (2026-09-27): *Account-backed category*, *Backing account*, *Accumulated*, the backed half
of assigning, and the *pool account* as the source of MoneyBud's own movements for a category
(*Where the dimensions meet*, above). Its row said the question of how a movement for a later period
is held was for the plan, and ADR 0009 answered it. **The sweep and *Leftover*** left when the sweep
increment was built (2026-09-28): *Sweep*, *Sweep destination*, the *pool account* as the sweep's
source, *Leftover* and the *Period leftover* (*The sweep: a movement for a period*, above). The sweep's
row had named three things the plan must store. ADR 0010 stores two of them, the backing at a period's
end and which period a sweep was for, and shows the third, the day of the first start, to be
unneeded. The *Leftover* row said a leftover needs a period end to be computed at: the period-end
record is that. **Recurring transactions** left when the recurring-entries increment was built
(2026-09-28): *Recurring transaction*, *Occurrence* and *Frequency* (*Recurring entries: state beside
the entries*, above). Their row had left two things for the plan, a recurring entry kept beside its
entries and settling as a third writer working through the days in order, and ADR 0011 does both.
**With it the table holds no concept waiting to be built**: the two rows left are things that are
deliberately never coded. Read the absence of a §12 term from this table as "built", not as "nobody
wrote a row for it".

| §12 concept | Why there is no code |
|---|---|
| *Over budget* as a stored state | Not missing — deliberately never stored. It is derived from *Remaining* wherever it is asked for, because §12 defines it as a property of a figure rather than a flag on a category |
| A period **closing** | Not missing — deliberately impossible. `BudgetPeriod` is a pair of dates with no state at all, so there is nothing that could ever refuse an expense on grounds of age (§12, *Ending versus closing*) |

## 8.2 Money Handling

**This section should be written before the first line of money-handling code.** Money decisions
are quietly expensive to reverse once data exists, and a budgeting app that gets them wrong is
wrong in ways that are hard to notice. The rules below are settled, how amounts are stored among
them since the persistence increment. One question is still open, and is listed at the end rather
than guessed at.

### Decided: amounts are a whole number of cents

Every monetary amount in MoneyBud is a whole number of cents. An amount finer than a cent — 12.345
euro — is **refused at entry**, with the user told that an amount cannot be finer than a cent. It
is not rounded to the nearest cent, and it is not stored at full precision and rounded on display.

The consequence is worth stating plainly: **MoneyBud has no rounding rule, because it never
rounds.** There is no "round half to even in exactly one place" to get right, and no possibility of
a total disagreeing with the sum of its parts by a cent.

**Why.** Every figure in MoneyBud is typed in by hand — [§3.1](03-context-and-scope.md) records
that there are no external systems at all. Nothing inside the system can therefore *produce* a
sub-cent amount; the only way one can appear is if the user types it. Refusing that single input
removes the entire class of rounding bugs, rather than committing the project to managing them
correctly forever.

**When this has to be revisited.** The argument rests entirely on the premise that amounts are
only ever entered, never computed. The moment anything in MoneyBud *computes* an amount, the
premise fails and this decision must be reopened rather than assumed. Known candidates:

- Splitting a leftover across several categories.
- Any interest, growth or investment-return calculation.
- Bank import, or any other external source, which can deliver amounts MoneyBud did not validate
  (and, if a foreign currency ever appears, conversion).
- Percentage-based budgeting — "20% of income to savings".

Anyone adding one of these should treat "we never round" as no longer true until it has been
re-argued.

**Backing is the first feature that moves an amount MoneyBud worked out, and it does not reopen the
rule** (2026-09-27). What moves at backing is a *Remaining*, a *Budget* minus expenses. What moves at
unbacking and re-pointing is what is there for the category: movements in, minus movements out, minus
expenses. Both are sums and differences of whole-cent amounts that were typed, so they are whole cents
by addition alone, and nothing is divided or split. A backing that split money across several
accounts would be on the list above. Several backing accounts per category are deferred
([§12](12-glossary.md), *What the backing increment covers, and what waits*). **A second currency belongs on that list as well** — a conversion rate is a computed
amount, and the "no currency field" rule below would be reopened at the same moment.

### Decided: in code, an amount is a `Money` value type over a `long` of cents

Reasoning in [ADR 0003](../decisions/0003-money-representation.md). What a developer needs to know
here:

- **`Money` wraps a whole number of cents as a `long`.** A sub-cent amount is not merely refused,
  it is **unrepresentable** — the rule above is a property of the type rather than a guard anyone
  has to remember. Addition, subtraction and equality are exact by construction.
- **`Money` is signed.** *Remaining*, *Unassigned* and *Balance* all go negative in normal use
  ([§12](12-glossary.md)); a negative amount is an ordinary value here, not an error.
- **`Money` is the domain's interior type, not its input boundary type.** The API that records a
  transaction — `RecordExpense` and `RecordIncome` alike — takes a **`decimal` euro amount**, and
  conversion to `Money` happens only *after* validation has passed. This is not stylistic:
  `-12.345` breaks the sign rule and the cent rule at once, and both
  [`record-expense.feature`](../../features/record-expense.feature) and
  [`record-income.feature`](../../features/record-income.feature) require the user to be told about
  the **sign** — so the cent check cannot be something that fires during construction, ahead of it.
  **`Assign` takes a `decimal` too, for the cent rule alone.** An assignment may be negative or
  zero, so the sign argument does not reach it. But an amount finer than a cent still has to arrive
  somewhere it can be refused as `AssignRefusal.AmountFinerThanCent`, and it has to be refused
  *after* the category checks, which is the order reported.
- **`Money` has no division and no multiplication by a fraction.** Those are the operations that
  would produce a value the cent rule cannot hold, and they are exactly what the features in *when
  this has to be revisited* above would need. Their absence is what makes that list enforceable
  rather than advisory.

`decimal` is still the type amounts are **entered and parsed** as, which is what
[ADR 0001](../decisions/0001-dotnet-and-reqnroll.md) was pointing at when it named `decimal` as a
reason C# suits this domain. The two records agree; `decimal` simply stops at the boundary.

### Decided: typed text becomes a `decimal` exactly as typed, and the cent rule stays the domain's

The UI put one more step in front of that boundary: the user types **text**. `AmountInput`, in the
presentation layer, turns it into the `decimal` the domain takes. The rules it reads by are
[§12](12-glossary.md)'s (*Typing an amount*). What matters here is what it does **not** do:

- **It never rounds.** The `decimal` it passes on is exactly the digits typed. "12,345" becomes
  12.345, and the domain refuses it as finer than a cent, which is the refusal the user sees. The
  whole-cents rule has one home, and it is still the domain.
- **It never judges a sign.** A leading minus is read, because assigning a negative amount is how
  money goes back to *Unassigned*. Whether a negative is allowed is the domain's to say: an expense
  refuses it, and an assignment takes it.
- **It is bounded so that nothing can be lost in the conversion.** It reads at most thirteen whole
  digits and ten decimals. Ten decimals always fit a `decimal` exactly, where a longer tail would be
  rounded by the parse and would turn a refusable amount into an acceptable one without a word.
  Thirteen whole digits is far inside what a `Money`'s `long` of cents can hold. Anything longer is
  refused as not an amount.

So there are now **two layers of refusal, in a fixed order**: text that is not an amount, or is
ambiguous, is refused by the presentation layer before the domain sees it, and everything else is
refused, or not, by the domain's own rules ([§6](06-runtime-view.md)). The first layer's refusals are
not domain reasons and have no enum. They are the screen's, like the rest of what it reads.

### Decided: display formatting is fixed, not taken from the machine

An amount is shown as **"€ 1.832,45"**, and a negative one as **"−€ 20,00"** with a true minus sign.
`Tekst.Euro` builds this from fixed separators, not from the machine's nl-NL culture data. **Why:**
the same reason the category name comparer is ordinal ([§8.1](#81-domain-model)). What MoneyBud
shows should not depend on where it runs, and culture data differs between operating systems and
versions in exactly these details: the group separator, the minus sign, the space after the €.

That display is also **why "2.000" is ambiguous**. MoneyBud itself shows thousands with a point, so a
user who has seen "€ 2.000,00" on screen has every reason to type "2.000"
([§12](12-glossary.md), *Typing an amount*).

### The ring's proportions are drawing shares, not amounts

`Ring` works out each slice's start and sweep as a `double` share of the circle, and how far it is
filled as another. **Apart from the pixel geometry `RingControl` draws them with, these are the
only floating-point numbers in MoneyBud, and they are not money.** No amount is computed from them, and nothing flows back from them into a `Money`. Every
slice's size and fill is a `Money`, read from the domain, and the shares are derived from those for
drawing only. So the "never `double` for money" rule and the premise of the whole-cents rule above
(amounts are entered, never computed) both stand. A share that is off in the ninth decimal moves a
pixel, not a cent. Every scenario that checks what a ring adds up to also asserts that its shares
close the circle, to nine places.

**Since the first demo the shares depart from the sizes on purpose.** Every slice is drawn at least
`Ring.MinimumSweep`, 2% of the ring, and `Ring.Sweeps` shares out the rest
([§12](12-glossary.md), *Every slice has a minimum width*). That changes only the drawing. A slice's
size is still its `Money`, and the sizes still add up to the income. The shares are further from the
amounts than before, which is exactly why they are not money. The minimum is decided there and
nowhere else: `RingControl` used to floor a slice's sweep with a hidden `Math.Max` of its own, and
that has been removed.

### Decided: amounts are positive magnitudes, and direction comes from the transaction type

An *Income* increases the total and an *Expense* decreases it. **Neither stores a negative
amount**, so a total over a set of transactions is a subtraction of the expenses from the incomes,
not a plain sum.

**Why.** This keeps the refusal of `-10.00` a statement about what an expense *is* — money that was
spent — rather than an arithmetic accident. It is how the approved scenarios phrase the rule
("an expense must be more than 0 euro"), and under signed storage a negative expense would be a
perfectly coherent piece of arithmetic that adds money back. Full reasoning and the rejected
alternative are in [ADR 0003](../decisions/0003-money-representation.md).

**This constrains transaction amounts only.** *Remaining* below zero is the *Over budget* state,
*Balance* below zero is *Overdrawn*, and *Unassigned* below zero is *Over-assigned*
([§12](12-glossary.md)). Derived figures are not touched by this rule and must stay free to go
negative.

**It covers income exactly as it covers expenses.** An income is a transaction, so its amount is a
positive magnitude, a whole number of cents, and refused rather than rounded if it is finer —
nothing in [§12](12-glossary.md)'s income rules changes anything in this section. The two rules
that do differ between income and expense, the required label and future-dating, are not money
rules and are settled there rather than here.

**It covers MoneyBud's own movements too**, as it covers transfers. A `Movement`'s amount is a
positive magnitude from one account to another, and which way it counts for *Accumulated* is its
`MovementDirection`, never a sign. A negative assignment writes a new movement going the other way.

Nor does it say anything about the **plan** layer. Whether an amount may be assigned negatively and
whether a *Budget* may be negative are questions about assigning, not about transactions; they are
settled in [§12](12-glossary.md) — yes, and no — and neither follows from or affects this rule.

### Decided: there is no currency field

A `Money` means **euro cents**. [§2](02-architecture-constraints.md) constrains MoneyBud to euro
only and to a single user, so a currency field could only ever hold one value, and a field with one
possible value carries no information. Reasoning, including why the usual "carry it for the
migration" argument does not apply to a demo with disposable data, is in
[ADR 0003](../decisions/0003-money-representation.md).

The constraint this rests on is a **decision the stakeholder took deliberately** when it was put to
him, not an assumption that merely went uncontradicted — see [§2](02-architecture-constraints.md),
which records both the decision and the fact that this record needed it settled. A second currency
reopens it — see *when this has to be revisited* above.

### Decided: on disk, an amount is a whole number of cents, as a JSON integer

Settled at the persistence increment's plan gate on 2026-09-26, and built
([ADR 0007](../decisions/0007-keeping-the-ledger.md)). Until then this was the first row of *Still
open*, below.

- **What is written is `Money.Cents`**, as a JSON integer: `"cents": 3215` for €32,15. It is exactly
  what `Money` holds, so writing converts nothing, and reading is `Money.FromCents` of an integer. No
  decimal text is written or parsed, so the stored form has no decimal mark, no culture and nothing
  to round.
- **Reading is strict about it.** `"cents": 12.5`, a fraction of a cent, is not read and rounded.
  It makes the whole file unreadable. So is `"cents": "3215"`, cents written as text. An unreadable
  file is met by the ruled response: say so, touch nothing, close ([§12](12-glossary.md), *When the
  data cannot be read*). So the stored form cannot be a way around the cent rule.
- **Signs follow the rules in code.** Entries are stored as positive magnitudes, and budgets as
  zero or more. `Ledger.FromSnapshot` refuses a kept entry of zero or less and a negative budget, as
  unreadable, because the running ledger could never have made either ([§8.1](#81-domain-model)).
- **There is no currency field on disk either**, for the reason there is none in code (below).

**Why integers and not decimal text.** Text such as `"32.15"` would bring the parse back that
[ADR 0003](../decisions/0003-money-representation.md) keeps at the boundary, now at the file, with
its own questions about marks, precision and rounding. An integer of cents has none of them. JSON
numbers are read here with `GetInt64`, which refuses a fraction rather than truncating it.

**What would reopen it.** The same things that reopen the whole-cents rule (*When this has to be
revisited*, above). If MoneyBud ever computes an amount finer than a cent, the file has nowhere to
put it, and that is deliberate. The stored form **may change freely between versions until the
switch to real use**, at least up to and including the accounts increment
([§12](12-glossary.md), *Demo data may not survive a new version*, *Real use before accounts*). The
backing increment changed it again, to version 3, with the stakeholder's leave. A movement's amount
is stored as cents like every other entry's. The sweep increment changed it to version 4, approved at
its plan gate; an amount let go is stored as cents too. The recurring-entries increment's version 5
stores no new amount: an occurrence is an ordinary expense or income, and a repeat holds ids, a
frequency, a day and a date, but no money. An occurrence copies its latest's `Money` as it is, so
nothing is computed either.

**The sweep moves a worked-out amount without reopening whole cents.** The *Restant* is *Unassigned*
plus a set of *Remaining* figures, and the difference a swept period shows is that minus what moved:
sums and differences of typed whole cents. Taking back swept too much picks the smallest of three
such amounts per move. Nothing is divided. Taking an over-sweep back "in proportion to what each
received" was rejected by a follow-up, and the documentation's reading of that rejection names the
fractions of a cent it would have left to settle ([§12](12-glossary.md), *A swept period that
changes*).

### Still open

The following has not been decided. It is listed so that it is clear it was considered and left
open, not overlooked.

| Question | Note |
|---|---|
| Period boundaries and timezones | The start day of a budget period is configurable (see [§12](12-glossary.md)); how that interacts with timezones is undecided, and `Ledger.Today` reads local time in the meantime as a stand-in rather than an answer. A **second, separate** question about the same boundary — which day a period starts in a month too short to contain the configured start day — **is now settled**, in [§12](12-glossary.md) rather than here, because it is about the calendar and not about money: the start day clamps to the month's last day. **Storing adds nothing to the timezone question**: dates are kept as `yyyy-MM-dd`, with no time and no zone, which is the day the ledger already holds ([ADR 0007](../decisions/0007-keeping-the-ledger.md)) |

## 8.3 Persistence

**MoneyBud keeps its data. Settled with the stakeholder on 2026-09-26, and built the same day** in
the persistence increment. Its storage choices are [ADR 0007](../decisions/0007-keeping-the-ledger.md):
one JSON file in the user's local application data, written whole after every change, in a project
of its own. Its three feature files, `keep-data.feature`, `start-moneybud.feature` and
`carry-on-when-saving-fails.feature`, are approved and bound.

**The accounts increment took the file to version 2** (2026-09-27,
[ADR 0008](../decisions/0008-balance-is-worked-out.md)). It adds `accounts` (key and name),
`poolAccount`, an `account` key on every expense and income, `transfers` (id, cents, date, `from`,
`to`) and `balanceCorrections` (id, date, account, cents, `starting`). **No balance is written**:
balances are worked out from what is kept. Version 1 is refused as unreadable, which applies the
ruling that data saved before accounts is not carried over (*What the stakeholder ruled*, below, last
rows). `Ledger.FromSnapshot` checks the new rules too: an entry or a pool account pointing at no
account, a transfer from an account to itself, two starting balances on one account, and ids unique
across all four kinds of entry. Where this section says "version 1" below, it was written before.

**The backing increment took the file to version 3** (2026-09-27,
[ADR 0009](../decisions/0009-movements-are-entries.md)). It adds `backing` on each category (null, or
the account's key and two marks, `accumulatingSince` and `hereSince`, each a date and an id),
`movements` (id, date, category, `from`, `to`, cents, and `reason` and `direction` as words), and
`settledThrough`. Still no balance, and no *Accumulated*: both are worked out. **Version 2 is refused
as unreadable, as version 1 is.** The plan recommended reading it, and the stakeholder left it to the
build: *"Chose what is best for you. I dont mind starting over"*. The build chose one way in rather
than two, since nothing would be kept that he minds losing ([§12](12-glossary.md), *Demo data may
not survive a new version*). `Ledger.FromSnapshot` checks the new rules too: a movement's direction
must fit its reason, a re-pointing must be between two accounts, a backing's marks must be ids issued
and no entry's, and ids are unique across all five kinds. **A start may now save straight away**:
when a period has begun since `settledThrough`, its planned money is moved and kept before the user
does anything ([§6](06-runtime-view.md), *Settling*).

**The sweep increment took the file to version 4** (2026-09-28,
[ADR 0010](../decisions/0010-sweeps-and-period-ends.md)). It adds `sweptFor` on each movement (a date,
or null on every movement that is not a sweep) and the word `"swept"` for `reason`; `sweepDestination`
(a category key, or null); `periodEnds` (`periodStart` and `backed`, the keys of the categories backed
when that period ended); and `letGo` (`periodStart` and cents). Still no *Restant*, no difference and
no line: all three are worked out. **Version 3 is refused as unreadable, as versions 1 and 2 are**,
approved by the stakeholder at the plan gate on the recommendation: version 3 has no record of past
period ends, and reading it would mean guessing the backing at each, the one thing the rulings say
must not be guessed. `Ledger.FromSnapshot` checks the new rules too: a sweep names a day that starts a
period and no other movement names one; the destination is backed and not archived; a period-end
record is for a day that starts a period, once, for a period ended by `settledThrough`, and names
categories that exist; a let-go amount is above zero, once per period, and for a period something was
swept for. **A start that sweeps saves straight away**, like any sweep ([§6](06-runtime-view.md), *The
sweep at settling*).

**The recurring-entries increment took the file to version 5** (2026-09-28,
[ADR 0011](../decisions/0011-recurring-entries.md)). It adds a top-level `repeats` list, each with
`occurrences` (entry ids, in the order recorded), `frequency` (`"weekly"`, `"monthly"`, or null once
stopped), `day` (the day of the month a monthly one was last set to, or null) and `next` (a date, or
null once stopped). The occurrences themselves are ordinary `expenses` and `incomes`, and the latest
occurrence is not written, since it is the highest id. **Version 4 is read, not refused**, the first
older version any MoneyBud has read. It was decision D2 of the plan, approved at the plan gate on the
recommendation: version 4 has no repeats because nothing could repeat when it was written, so reading
it as data with no repeats guesses nothing, unlike versions 1 to 3. **A version-4 document that has a
`repeats` property is refused**, since that is not what version 4 wrote. Versions 1 to 3 are still
refused. The next save writes version 5. A frequency word MoneyBud does not know makes the file
unreadable, as an unknown movement reason does. `Ledger.FromSnapshot` checks the new rules too
([§8.1](#81-domain-model), *Recurring entries*). **A start that records an occurrence saves straight
away**, like one that sweeps ([§6](06-runtime-view.md), *Settling day by day*).

**Opening a period changed nothing here.** The plan offered is worked out from the budgets already
kept ([§8.1](#81-domain-model)), and nothing about it is stored, so the file's format and its version
are unchanged.

Through the first six increments this section recorded a **deferral**, with a trigger. That record is kept
below (*How this section read until 2026-09-26*), because the reasoning is still the reason nothing
was stored until then.

### Not the trigger firing

The trigger was the first time the stakeholder is asked to re-enter data he would mind re-entering.
**It had not fired.** He chose to build persistence next anyway: "demo now, real soon". Keeping
data saves re-entering it between sessions, MoneyBud is still a demo, and he expects to switch to
real use not long after persistence is built ([§12](12-glossary.md), *Why now: demo now, real
soon*).

**Why after corrections, deliberately** (order agreed 2026-09-26). While nothing is kept, closing
MoneyBud discards every mistake. Once data is kept, a typo that cannot be corrected is permanent. So
correcting came first.

**What does not expire yet.** The trigger was shared with [ADR 0002](../decisions/0002-desktop-application-first.md):
both were "bought with the same argument", that the demo's data is throwaway. That argument still
holds, because the kept data is still demo data (*Demo data may not survive a new version*, below).
So ADR 0002 was **not** reopened by this increment, and neither were the other things that wait for
the demo to stop being a demo ([§11](11-risks-and-technical-debt.md)). Keeping data and real use
used to be expected together; the stakeholder has separated them. **Nothing before accounts has to
plan around the switch to real use.** He ruled the same day that his saved data may be dropped at
least up to and including the accounts increment ([§12](12-glossary.md), *Real use before
accounts*).

### What the stakeholder ruled

In full, with the reasoning and what was rejected, in [§12](12-glossary.md), *What MoneyBud keeps*.
In outline, with what each means for whoever builds it, and **how it was built**, checked against
the code at the close of the increment:

| Ruling | What it means for the build, and how it is built |
|---|---|
| **Everything is kept**, as one continuous history, indefinitely. No fresh start per year | Nothing is pruned or archived by age. Periods never close, so there is no boundary to cut at. **Built:** `Ledger.ToSnapshot` takes every category, budget, expense and income, and nothing anywhere removes kept data by age |
| **"Everything" is the ledger only**: categories, archived or not, budgets, expenses, incomes | Screen state is not stored: the period shown, a half-typed entry, a waiting question, a rename in progress. MoneyBud always opens on the current period. **Built:** `LedgerSnapshot` has the ledger's four lists and `lastEntryId`, and nothing else. Since then it has gained accounts and two more entry kinds (ADR 0008), then backings, movements and `settledThrough` (ADR 0009), then the sweep destination, period-end records and amounts let go (ADR 0010), then the repeats (ADR 0011), all of them the ledger's and none of them screen state. `MoneyBudApp` is made fresh at every start, and its constructor puts the current period on screen |
| **Saved automatically after every change.** No save button. **A save that works says nothing** | Every act that changes the ledger ends with the data written. There is no save act and no "save now" state to offer, and no notice for a save that succeeds. **Built:** `MoneyBudApp.Tell` calls `Keep` after every act that went through **and changed the ledger** (`Tell(changed:)`). Adding a name already there, assigning zero and a negative assignment clipped in full against a *Budget* of zero are said but not saved. A refusal, an unchanged save and a declined question never reach `Tell`. A save that works sets nothing the screen shows, unless it ends a failure (below). The scenario "offer no act for saving" lists every command of the screen and the forms in full, and checks every `Command` binding in the window's markup against them |
| **An interrupted save never damages the previous one.** A crash or power cut loses at most the change being saved. **The next start opens normally and says nothing** about it | Writing must never leave a half-written save in place of a whole one. Nothing is recorded to detect or report a missing change at the next start. **Built:** `FileLedgerStore.TrySave` writes `moneybud.json.tmp`, flushes it to the disk, and renames it over `moneybud.json`. A leftover `.tmp` is never read and is overwritten by the next save. The next start loads `moneybud.json` as usual and says nothing. Held by `StorageTests`, and by a scenario that rebuilds the disk state a cut-off save leaves, approved at the plan gate as a simulation (§8.4) |
| **A failed save is said and the user carries on.** Closing before a save succeeds loses what was not saved, accepted. **The "not saved" notice stays on screen until a later save succeeds**, shown beside any other notice and beside the removal question, and not cleared by stepping. **Retried by every change and by MoneyBud itself now and then.** **Recovery is said once.** **Closing makes one last attempt**, and if it fails just closes, with no question | Nothing is undone and nothing is refused because a save failed. Each save writes the whole ledger, not the last change, so one success catches up every failure before it. "Not saved" is a **lasting state** of the screen, cleared only by a successful save. **Built:** `TrySave` reports `false`, and `MoneyBudApp.IsUnsaved` becomes true. **The save line**, `MoneyBudApp.SaveLine`, is a line of its own beside the notice and the question, so the one-message rule between those two is untouched ([§8.4](#84-the-presentation-layer)). It reads *"Je wijzigingen zijn niet opgeslagen. MoneyBud probeert het opnieuw."* until a save works, and stepping leaves it. Every later act that changes the ledger retries. **"Now and then" is once a minute**: `MoneyBudApp.Tick`, on the Desktop's existing timer, retries while something is unsaved. The save that works puts *"Alles is weer opgeslagen."* **on the same save line**, not in the notice, until the next act or step. `MoneyBudApp.Close` makes one last `TrySave` if something is unsaved, asks nothing, and lets go of the store |
| **A fixed place in the user's profile**, never chosen by the user, always outside the repository | The location is derived from the user's profile, **never** from the working directory. `dotnet run --project src/MoneyBud.Desktop` runs inside a working copy of the public repository, so a relative path would put data exactly where it must never be. `.gitignore` is a second line, not the protection. **Built:** `FileLedgerStore.DefaultFolder` is `Environment.SpecialFolder.LocalApplicationData` plus `MoneyBud`, which is `%LOCALAPPDATA%\MoneyBud` on Windows. A folder that is not a full path is refused as unreachable, so a relative path cannot be used even by mistake. `.gitignore` lists `moneybud.json`, `moneybud.json.tmp` and `moneybud.lock`. A unit test holds that the folder is outside the repository |
| **Unreadable data, damaged or written by a newer version: say so, touch nothing, and close.** Never start empty instead. The message says only that the data cannot be read: no path, no pointer to the README | A load that fails must leave nothing able to save over the file, and no screen to enter anything into. Automatic saving is what makes an empty start dangerous here. **Built:** `MoneyBudStart.Start` returns `Refused(CannotRead)` for a file `LedgerJson` cannot read and for kept data `Ledger.FromSnapshot` refuses. No `MoneyBudApp` is made, so nothing can save. The Desktop shows *"MoneyBud kan je opgeslagen gegevens niet openen. Het bestand is beschadigd, niet bereikbaar of gemaakt door een andere versie van MoneyBud. MoneyBud heeft het niet gewijzigd."* in a small window (reworded on 2026-09-27; it read *"MoneyBud kan je gegevens niet lezen. Er is niets aan veranderd."* until the stakeholder found that confusing), and closing it closes MoneyBud. The message is copy in `Tekst`, not a display term. **"Touches nothing" means the data file**, as the stakeholder confirmed (2026-09-26): the claim, which comes before loading, may make the folder if it is missing and the lock file beside the data, and that is MoneyBud's own bookkeeping. Creating nothing at all was rejected, because it would mean checking the data before taking the lock, which leaves a window in which two MoneyBuds start at once |
| **A folder that cannot be reached at all is met the same way** (ruled 2026-09-26, during review): a profile that is not there, a folder MoneyBud may not open, a *file* standing where the folder should be | Say it cannot read the data, touch nothing, close. **Rejected:** starting empty and showing "not saved", because if the real data came back, the first save that worked would write the empty start over it. **Built:** `FileLedgerStore.TryClaim` returns `Claim.Unreachable`, and `MoneyBudStart` turns it into `Refused(CannotRead)`, the same message as above. Held by a `cannot be reached` row in `start-moneybud.feature`'s "cannot read" outline, added after the scenario gate with the stakeholder's approval (§8.4) |
| **A second start while MoneyBud is open is refused**: it says MoneyBud is already open, and closes | Only one process may hold the data. Two would overwrite each other's saves. **Built:** `TryClaim` opens `moneybud.lock` exclusively and holds it until `Close`. It is claimed **before** loading. A second start gets `Claim.HeldElsewhere`, and `MoneyBudStart` returns `Refused(AlreadyOpen)`: *"MoneyBud is al geopend."* The operating system lets go of the lock when a process dies, so a crash never blocks the next start |
| **Backups are not MoneyBud's job** | One set of data, and no copies kept by MoneyBud. **Built:** there is one data file and nothing copies it. The one extra file a save makes, `moneybud.json.tmp`, is renamed away, not kept |
| **Kept data that is there but blank is unreadable**: say so, touch nothing, close. **A saved empty budget is valid** | MoneyBud never writes a blank save, so blank kept data is a failure, not a first start. A save of a budget with no categories and nothing recorded is written, loads, and shows no categories (next row). Confirmed by the stakeholder, 2026-09-26. **Built:** `LedgerJson.Read` returns nothing for blank or whitespace-only text, which is unreadable. An empty ledger is written as a whole document with four empty lists and reads back as one |
| **One set of data, no in-app reset.** Starting over means deleting the file. **The defaults come only with a first start**, when there is no kept data at all | No act to start over, and no second set of data beside the first. A missing file is a first start, and nothing else is. A ledger saved with no categories loads with no categories. **Built:** only `LoadResult.NoData`, no `moneybud.json`, leads to `Ledger.StartNew`. A first start saves nothing until the first change |
| **No password, no encryption.** The Windows login is enough | Nothing to build. Security is the operating system's user account. **Built:** nothing, as ruled. The file is plain JSON |
| **Until real use starts, a new version may be unable to read an older one's demo data.** It then says so and touches nothing, and the user starts fresh. **Extended the same day: at least up to and including the accounts increment** | The stored form may change between versions without anything carrying old data across, the version that adds accounts included. Carrying data across versions becomes a requirement only at the switch to real use, which no increment before accounts plans around. **Built:** the file says `"format": "MoneyBud"` and `"version": 1`, and any other format or version is unreadable. There is no older version to read. **Exercised on 2026-09-27**: the accounts increment writes `"version": 2` and refuses version 1, so data saved before accounts is not read, and the user deletes the file (ADR 0008). **Exercised again the same day**, past the extension's end, with the stakeholder's leave ("I dont mind starting over"): the backing increment writes `"version": 3` and refuses versions 1 and 2 (ADR 0009). **And again on 2026-09-28**, approved at the plan gate: the sweep increment writes `"version": 4` and refuses versions 1 to 3 (ADR 0010). **Not exercised on 2026-09-28** by the recurring-entries increment: it writes `"version": 5` and **reads version 4**, since nothing in version 4 has to be guessed, approved at the plan gate on the recommendation (ADR 0011). The ruling still stands for any later version, and versions 1 to 3 stay refused |
| **The location is documented in the README only.** MoneyBud does not show it, on screen or in the unreadable-data message | Nothing in the screen names a path. **Built:** the root README lists the file for Windows, macOS and Linux. No text in `Tekst` names a folder or a file, and a scenario checks the unreadable-data message for paths, file names and the README |

**Carried over unchanged, not newly ruled:** with no data yet, MoneyBud starts as it does today,
with the six default categories and nothing else (`Ledger.StartNew`, [§8.1](#81-domain-model)).

### Answered by the plan

These were **technical decisions, not stakeholder rulings**, and none of the rulings above answered
them. Until the plan they stood here as *Left for the plan*. The persistence increment's plan
answered all four at its gate on 2026-09-26, and [ADR 0007](../decisions/0007-keeping-the-ledger.md)
records them with their reasoning:

- **The form storage takes, and the exact folder.** One JSON file, `moneybud.json`, written whole on
  every save with the runtime's `System.Text.Json`, and read strictly. SQLite was rejected: it needs
  a package, it saves change by change where the rulings want one save to catch up everything, and
  it is opaque to the user who backs it up. The folder is `%LOCALAPPDATA%\MoneyBud`, local rather
  than roaming ([§7](07-deployment-view.md)).
- **How amounts are stored.** Whole cents, as JSON integers, and a fraction or cents as text make the
  file unreadable ([§8.2](#82-money-handling)).
- **How identity is stored.** The question as it stood: an entry's `Id` was a counter that started
  again at every run, and a category's **name is no longer an identity** since renaming, so a store
  keyed by name would turn a rename into a broken link or a silent reassignment. **The answer:**
  entry ids are kept, and so is `lastEntryId`, so an id is never issued twice. A category gets a key
  **only in the file**: its place in the order added, from 1, made afresh at every save. Budgets and
  expenses refer to it, never to the name. The domain gained no id, because `Category` already has
  object identity and every file is the whole ledger. Lists are kept in the ledger's order, so ties
  and newest-first survive a restart. `Ledger.FromSnapshot` re-checks every rule the running ledger
  keeps.
- **Where storage sits in the solution.** A fourth project, `MoneyBud.Storage`, that references the
  domain only. The domain holds `LedgerSnapshot` and the `ILedgerStore` port. The presentation layer
  holds all the behaviour and knows the store only through the port. The Desktop wires the two
  together ([§5](05-building-block-view.md); ADR 0007 amends [ADR 0006](../decisions/0006-three-source-projects.md)).

**The period start day is not stored.** The calendar is fixed at the 1st, and
[§11](11-risks-and-technical-debt.md)'s start-day row still applies. One consequence is new: a
budget is kept against its period's first day, and `FromSnapshot` refuses a budget on a day that
starts no period. So a start day changed in code would make an existing file unreadable rather than
misread.

**What the rulings changed about the cost of these choices.** The deferral argued that the costly
part of storage is the shape that accounts will need, and that choosing a shape early commits it
when least is known (third bullet below). Ruling that demo data need not survive a new version makes
the demo's storage cheap to change: a wrong shape costs a fresh start, not a migration. **That now
reaches through the accounts increment.** This section first warned that real use might start
before accounts, so that accounts would arrive against data that had to be carried across. The
stakeholder answered that he does not mind his saves being deleted when accounts are added
([§12](12-glossary.md), *Real use before accounts*). So the shape accounts need can be settled when
accounts are built, as the deferral wanted, without a migration. **The build used that freedom
openly.** The format is version 1, has no way to read anything older, and has nothing in it for
accounts. The version field is there so that a later format is met by "cannot read" rather than
misread ([ADR 0007](../decisions/0007-keeping-the-ledger.md)).

### How this section read until 2026-09-26

Kept as written, because it records why nothing was stored for six increments and what was
expected to end that. One paragraph, on how identity is stored, moved up to *Left for the plan*,
now *Answered by the plan*, above.

> **Nothing is stored. State lives in memory for the lifetime of a run, and is gone when the
> application exits.** This is a deliberate deferral with its reasoning recorded, not an unmade
> decision.

**Why.**

- **No approved scenario observes persistence.** Neither
  [`record-expense.feature`](../../features/record-expense.feature) nor
  [`record-income.feature`](../../features/record-income.feature) ever restarts anything, and
  neither asks whether what was recorded survives. Building storage now would be implementing
  behaviour nobody has specified, against a specification contract that is meant to be the thing
  driving what gets built.
- **The demo's data is explicitly throwaway.** The first version exists to be reacted to, not lived
  in ([§1.1](01-introduction-and-goals.md), [ADR 0002](../decisions/0002-desktop-application-first.md)),
  so there is nothing yet that anyone would mind losing.
- **The expensive part of this decision is not in any increment built so far.** The second row of
  [§11](11-risks-and-technical-debt.md) argues that the storage shape should be settled **before
  accounts are built**, because that is when it becomes costly — whether a *Balance* is stored or
  derived from its transactions is the question that makes it so. Neither the expense increment nor
  the income increment has accounts. Choosing a storage shape now would commit the hardest part of
  the decision at the moment we know least about it, and would do so to serve no scenario.

**What will force the decision.** The first time the stakeholder is asked to re-enter data he would
mind re-entering. That is the same moment [ADR 0002](../decisions/0002-desktop-application-first.md)
names as the point at which the demo has stopped being a demo, and it is deliberately the same
trigger: both decisions were bought with the same argument, so both expire together.

Two things should be settled at that point rather than drifted into: the form storage takes
([§7](07-deployment-view.md) lists it as open, along with where on the machine it lives), and how
amounts are stored (§8.2, *Still open*).

A third was added by the corrections increment: how identity is stored (since answered, under
*Answered by the plan*, above).

**Reconsidered for the UI increment, and kept** (2026-09-25). The UI starts with the default
categories and nothing else, and loses everything on close. The stakeholder chose that over saving
to a file and over starting with synthetic demo data ([§12](12-glossary.md), *What the UI starts
with, and what it keeps*). The trigger above has not fired. The UI is, though, the first increment
in which it **can** fire: until now nobody could enter anything, so nobody could mind losing it.

**The UI is built, and keeps nothing.** Every start is a first start: the Desktop builds its ledger
with `Ledger.StartNew`, and it is gone when the window closes. The trigger above is now reachable
and has not fired.

**Raised at the first demo, and still not fired** (2026-09-26). The stakeholder named keeping data as
missing and deferred it in the same sentence: *"Maar dat komt later."* That names a gap. It does not
say he minded re-entering anything, and that is what the trigger waits for
([§12](12-glossary.md), *What the UI starts with, and what it keeps*).

This is a **scope** decision rather than an architectural one, which is why it lives here and not
as a record in [`docs/decisions/`](../decisions/). MoneyBud already records "not in the first
increment" in the section the thing belongs to — [§12](12-glossary.md) does it for accounts, the
pool account, backed categories and the sweep — and none of those got a record of their own either.

**The deferral ended on 2026-09-26, without the trigger firing** (*Not the trigger firing*, above).
The stakeholder's rulings are scope and requirements too, so they live here and in
[§12](12-glossary.md) rather than in a record. The technical choices under *Left for the plan* were
the part that might need one, and they got one:
[ADR 0007](../decisions/0007-keeping-the-ledger.md) (*Answered by the plan*, above).

## 8.4 The presentation layer

`MoneyBud.Presentation` holds everything the screen decides and has no UI toolkit
([ADR 0006](../decisions/0006-three-source-projects.md), [§5](05-building-block-view.md)). What the
screen shows was settled with the stakeholder and is in [§12](12-glossary.md), *The user
interface*. This section covers what a developer needs to know about how the layer is arranged,
how the scenarios reach it, and which behaviour was decided while building rather than ruled on.

### The period on screen is held as a period

`MoneyBudApp.ShownPeriod` is a `BudgetPeriod`, never "the current one" and never an offset from it.
That is what makes §12's *Staying open across a period boundary* hold without machinery. When a new
period begins, the ledger's idea of "current" moves by itself, because `Ledger.CurrentPeriod` reads
the clock on every call. The period on screen does not move, so it becomes a past period. The
past-period refusal for assigning and the display rule then follow from the domain. Whether the
period is labelled *Huidige periode* is worked out on every read.

### Nothing is cached, and the screen is told to look again

`PeriodOverview` is worked out afresh from the ledger every time it is read, so nothing on screen can
hold a figure that has since changed ([§6](06-runtime-view.md)). `MoneyBudApp.Refresh` changes
nothing. It raises property changes so that whatever is bound looks again. Every act calls it.

**The Desktop also calls it once a minute**, through `MoneyBudApp.Tick`, and that is the only thing
that moves the *Huidige periode* label when a period ends while MoneyBud is open. Since the
persistence increment `Tick` first retries a save that failed, if one did (*Keeping the ledger*,
below). Since the backing increment it settles first, so money planned for a period that has just
begun moves within the minute and is kept (*Backing on screen*, below). The consequence, stated so it is not
rediscovered: **for up to a minute after a period boundary the screen can be out of date.** The label
can still read *Huidige periode*, and in-use categories with no history can still be listed in a
period that has just become past. An assignment made in that minute is refused as past, because the
domain reads the clock itself. Any act refreshes at once, so the refusal also corrects the label.
Nothing is announced either way, which is what §12 asks. **Since the opening-a-period increment the
same holds for the offer**: the take-over button and the grey plan figures can stay on a period that
has just become past, pressing the button then is refused as a past-period assignment, and the redraw
after the refusal takes them away (*Opening a period on screen*, below). **Since the sweep increment
the same minute reaches *Restant bijwerken***: the button can still stand on a period that settling
is about to sweep. Pressing it settles first and, if the line then offers no button, says the sweep
and moves nothing more (*The sweep on screen*, below).

**A second consequence, since the corrections increment, known and not fixed.** The minute's refresh
rebuilds the category rows, and with them a rename box that is open. **The box loses keyboard
focus.** What was typed is kept, because it lives in `MoneyBudApp.NewName` and not in the box, so
the cost is clicking back into it, at most once a minute. It is listed with the build's other
readings in [§12](12-glossary.md), *Chosen in the build, not put to the stakeholder*. It was left as
it is. In the documentation's reading, a fix would either put focus handling in the Desktop or make
the timer refresh less than everything, and the first is the kind of logic the Desktop is meant not
to collect ([§11](11-risks-and-technical-debt.md)).

### Decided while building, not put to the stakeholder

These are visible to the user and were chosen in the build. They are recorded here so that they are
not mistaken for rulings. None contradicts a ruling, and any of them can be put to the stakeholder
if he reacts to it. He reacted to one at the first demo, and part of the first row is now his
ruling.

| Behaviour | Why it was built this way |
|---|---|
| **A form clears after its entry goes through, and keeps what was typed after a refusal** | A refusal is corrected in place, not retyped. A cleared form after success shows that it went through. As first built, the expense form kept its category after success, and the assign form cleared only its amount, keeping its category and period. **The category half has since been ruled by the stakeholder** at the first demo, 2026-09-26: the category box empties after success too ([§12](12-glossary.md), *Category entry is free text with suggestions*). Now built, so that half is a ruling and no longer a build choice. The assign form still keeps its period, which follows the screen |
| **Every date starts empty, and empty means today** | §12's default ("an entry's date defaults to today, whatever period is on screen"), built so the date never follows the period on screen. The picker shows *Vandaag* until a date is chosen |
| **The assign form's period follows the screen when it steps, and can be moved on its own** | §12's "assigning defaults to the period on screen", plus a way to name another period without moving the screen, which is how an assignment lands elsewhere |
| **Stepping clears the last notice** | A notice is about the last thing done. After stepping it would sit beside a period it may not describe |
| **A period is named by its month, "maart 2026", or by its first and last day when it is not a calendar month** | Every period starts on the 1st in this increment, so the second form is not reachable yet. It exists so that a configurable start day would not produce a wrong month name ([§11](11-risks-and-technical-debt.md), the start-day row) |
| **Amounts are shown as "€ 1.832,45" and "−€ 20,00"** | [§8.2](#82-money-handling), *display formatting is fixed* |

**The corrections increment's choices are in [§12](12-glossary.md), not in this table.** What the
form does around a correction was proposed by the plan and approved at the plan gate, so it is a
ruling (*On screen: picking an entry to correct*). Six further choices were made in the build and
not put to the stakeholder: a rename rewriting the category box, stepping cancelling a rename,
which acts drop a waiting question, the archive button's new place, the question's two answers, and
the rename box losing focus on the minute's refresh. They sit in §12, *Chosen in the build, not put
to the stakeholder*, beside the rulings each one fills in. **Opening a period's are there too**, under
*Taking a plan over: chosen in the build, not put to the stakeholder*, for the same reason. So are the
accounts increment's (*Accounts: chosen in the build*), the backing increment's (*Backing: chosen
in the build, not put to the stakeholder*) and the sweep increment's (*Sweep: chosen in the build, not
put to the stakeholder*).

### All the Dutch is in `Tekst`, and a test holds it to §12

`Tekst` holds two kinds of text. The **display terms** are fixed. They are §12's *Dutch display
terms* table, and a unit test (`TekstTests`) **reads that table from `12-glossary.md`**. It fails if
a row's Dutch and its constant disagree, and if a row is added to or removed from the table without
the test's own list of rows following. So the table is load-bearing: editing it is a code change.
The **sentences**, meaning refusals, outcomes and the notice about where an entry went, are copy.
They are not pinned word for word. What is checked is that every refusal reason has one, and, by a
deliberately crude check, that none reads as English.

**A new refusal reason cannot go unworded.** Each refusal switch in `Tekst` lists every reason and
has **no fallback arm**. The project suppresses CS8524, the warning about values outside an enum's
names, so that **CS8509**, a missing named value, still fires. A reason added to the domain without
Dutch wording is therefore a warning, and the build is kept at zero warnings. A fallback arm would
have silenced exactly the case that matters.

**The toolkit's own text follows the thread culture**, which the Desktop's `Program` fixes to nl-NL.
That covers the date picker's month and day names, for example. MoneyBud's own text does not depend
on it.

### Correcting: a second state for the forms, and the one question

The rulings are in [§12](12-glossary.md), *An entry can be changed or removed* and what follows it.
How the presentation layer holds them:

- **A row carries its entry.** `ExpenseLine` and `IncomeLine` hold the `Expense` or `Income`
  itself, not a copy of its figures. Clicking a row calls `MoneyBudApp.EditExpense` or
  `EditIncome`, which loads that entry into its form.
- **The entry forms have a *Wijzigen* state.** `ExpenseForm` and `IncomeForm` hold the entry being
  changed in `Editing`, and `IsEditing` and `SubmitText` follow from it, so the submit button reads
  *Opslaan* instead of recording. `Load` fills the fields as the user would type them, the amount
  through `AmountInput.Format` ([§12](12-glossary.md), *Changes and renames are announced*, "a
  consequence for the build"). `Save`, `Remove`, `Cancel` and `Clear` are the rest of it. Saving
  reads the fields by the same rules as recording, `AmountInput` included, because it is the same
  form.
- **The question is held by `MoneyBudApp`, not by a dialog.** `Question` is the text waiting for an
  answer, and the act to do on a yes is held beside it. **The ledger is not called until
  `Confirm`**, so nothing is removed while the question stands. `Decline` drops the question and
  says nothing. There is only ever one question, because removing is the only act that asks
  ([§8.1](#81-domain-model) says why the domain has no confirmation of its own). The Desktop shows it
  in the message bar, where a notice would be.
- **Renaming is state on `MoneyBudApp`**: `Renaming` names the category whose row shows a text box,
  `NewName` holds what is typed in it, and `StartRename`, `SaveRename` and `CancelRename` move
  between the two. The row learns which one it is from `CategoryRow.IsRenaming`, and whether to
  offer *Verwijderen* from `CategoryRow.CanDelete`, which reads `Ledger.CanDelete`. So which buttons
  a row shows is decided here, and the Desktop only binds to it.
- **What drops what** is in [§12](12-glossary.md): stepping drops an entry being changed, a rename
  and a waiting question, and keeps a new entry; a change saved, *Annuleren*, or another row loaded
  drops a waiting question. `MoneyBudApp`'s stepping and each act do the dropping, so the rules are
  in the layer the tests reach.
- **A question and a notice are never shown together.** The rule was chosen in the build, not
  ruled by the stakeholder ([§12](12-glossary.md), *Chosen in the build, not put to the
  stakeholder*), and it still holds between the removal question and an ordinary notice. **The save
  line is not either of them**, and stands beside both (*Keeping the ledger*, below). The
  persistence increment made room for it with a line of its own, rather than by bending this rule.

### Keeping the ledger: when to save, the save line, and starting

The rulings are in [§12](12-glossary.md), *What MoneyBud keeps*. The storage choices are
[ADR 0007](../decisions/0007-keeping-the-ledger.md). **Everything about keeping that the user meets
is decided here**, in the presentation layer. The store only writes and reads a file, and the
Desktop only wires them together. The runtime order is drawn in [§6](06-runtime-view.md).

- **`MoneyBudStart.Start` decides whether MoneyBud opens.** It claims the store, loads it, and
  returns `Opened` with the screen, or `Refused` with one `StartRefusal`: `CannotRead` or
  `AlreadyOpen`. A folder that cannot be reached, a file that cannot be read and kept data that
  breaks a domain rule all become `CannotRead`. `Refused.Text` is the sentence the Desktop shows.
  The two sentences, *"MoneyBud kan je opgeslagen gegevens niet openen. Het bestand is beschadigd, niet bereikbaar of gemaakt door een andere versie van MoneyBud. MoneyBud heeft het niet gewijzigd."* and
  *"MoneyBud is al geopend."*, are copy in `Tekst`, not display terms, so §12's table does not hold
  them.
- **Only an act that changed the ledger saves.** Every act that goes through ends in
  `Tell(text, landedIn, changed)`, which calls `Keep` only when `changed` is true. Adding a name
  already there, assigning zero, and a negative assignment clipped in full against a *Budget* of
  zero pass `changed: false`. A refusal, an unchanged save and a declined question never reach
  `Tell`. **As first built, every act that went through saved.** `spec-reviewer` found that such an
  act could then show "not saved" about a change that never happened, and `Tell(changed:)` was the
  fix. A first start saves nothing until the first change, which is the same rule seen from the
  start: nothing has changed yet.
- **`Keep` saves the whole ledger** (`Ledger.ToSnapshot`) and notes the result in two fields.
  `IsUnsaved` is whether the last save failed and none has worked since. `savedAgain` is whether the
  save that just worked ended a failure.
- **`SaveLine` is what the save line says**, worked out from those two: *"Je wijzigingen zijn niet
  opgeslagen. MoneyBud probeert het opnieuw."* while `IsUnsaved`, *"Alles is weer opgeslagen."*
  after the save that ends it, and otherwise nothing. **It is its own property, not a `Notice`**,
  and the window shows it as a line of its own under the notice and the question. So "not saved"
  stands beside both, as ruled. **So does "saved again"**: it too is said on the save line, not as an
  ordinary notice. It can stand beside the question, when the minute's retry works while a question
  waits. The one-message rule between the question and an ordinary notice is untouched.
- **What clears what.** "Not saved" is cleared only by a save that works. Stepping, acts and
  questions leave it. "Saved again" is cleared by the next thing the user does that says something
  or deliberately says nothing, by asking the removal question, and by stepping.
- **`Tick` retries.** The Desktop's once-a-minute timer calls it. It retries only while something
  is unsaved, then refreshes. So "MoneyBud itself, now and then" in the ruling is **once a minute**,
  and on a healthy disk the timer writes nothing.
- **`Close` makes the last attempt**, only if something is unsaved, asks nothing whatever comes of
  it, and disposes the store. The Desktop calls it from the window's `Closed` event.
- **The save line's colour follows `IsUnsaved`** through the notice's own background converter:
  the refusal colour for "not saved", the ordinary one for "saved again". That is drawing, bound to a
  flag this layer decides.

**A second markup test holds the save line's place.** `WindowMarkupTests` checks that the save line
is a sibling of the notice and the question in `MainWindow.axaml`, so that it cannot be moved into
their place without a test failing. It widens the markup exception below, and was approved at the
plan gate on the same terms.

### Opening a period on screen

The rulings are in [§12](12-glossary.md), *Opening a period*. How the presentation layer holds them:

- **The offer is part of the Overview, worked out with it.** `PeriodOverview.Of` asks
  `Ledger.PlanOfferedIn` for the period it is building, and keeps the answer as `Offer`. `HasOffer`
  shows the button, and `OfferText` is its text, *"Plan van augustus 2026 overnemen (€ 1.450,00)"*.
  Because the Overview is rebuilt on every read ("Nothing is cached", above), an offer that has gone,
  because the period got a plan or became past, is simply absent at the next read. Nothing clears it.
- **Each row carries its own grey figure.** `CategoryRow.PlanFigure` is `Offer.FigureFor` the row's
  category: null with no offer, and null for a category not in the plan, which therefore shows no
  label rather than "plan: € 0,00". `PlanText` is *"plan: € 400,00"*, or null.
- **The row order has a second key.** Rows sort by *Budget*, then by plan figure (none counting as
  zero), then by order added, the last through a stable sort. While an offer stands every *Budget* is
  zero, so the plan figure decides. With no offer every plan figure is null, so the second key
  changes nothing and the order is the one the UI increment built. One sort serves both, rather
  than a second ordering switched on by the offer.
- **`MoneyBudApp.TakeOverPlan` acts on `ShownPeriod`**, never on the assign form's own period, so the
  button and the grey figures always describe the same period. It asks nothing. Taken over, the
  notice names both periods: *"Plan van augustus 2026 overgenomen in oktober 2026: € 1.450,00
  toegewezen."*, naming the period it went into even when that is the one on screen. It passes
  `changed: true`, so it saves like any change, and it always does change the ledger, since every
  figure in a plan is above zero. Refused, it shows the sentence the assign form shows for a past
  period, and saves nothing ([§12](12-glossary.md), *Taking a plan over: chosen in the build, not
  put to the stakeholder*).
- **The words are `Tekst.TakeOverPlan`, *Plan overnemen*, and `Tekst.Plan`, *plan***, two rows of §12's
  display-terms table that `TekstTests` holds. The button's text is built from the `TakeOverPlan`
  constant, split around the period's name, so the table's words are the words on the button.
  `spec-reviewer` found that the first build wrote the button's words out afresh, leaving the
  constant unused, and that was fixed.

**The Desktop only binds.** The button, under the assign form, binds its visibility to `HasOffer`,
its text to `OfferText` and its command to `TakeOverCommand`. The grey figure is a caption under the
*Budget* figure, shown when `PlanText` is not null. The *Budget* column was widened to fit it
([§12](12-glossary.md), *Taking a plan over: chosen in the build, not put to the stakeholder*). No
markup test covers either, and a headless run of the real window checked both
([§11](11-risks-and-technical-debt.md), the Desktop row).

### Backing on screen

The rulings are in [§12](12-glossary.md), *Backing and Accumulated*. The build's own readings are in
*Backing: chosen in the build, not put to the stakeholder*, there. How the presentation layer holds
them:

- **Each `CategoryRow` carries its backing.** `BackingAccount` is today's backing, the same in every
  period. `BackingChoices` is "—" and then the accounts in the strip's order. `Accumulated` is
  `Ledger.AccumulatedFor` the period being built, null for an unbacked category. `AccumulatedText`
  is *"Opgebouwd: € 600,00"*, and `AccumulatedMarker` is the one marker below zero, badge *Rood*.
  The pointed slice's details read the same row, so the ring's hole shows *Opgebouwd* too.
- **`ChosenBacking` is the two-way binding, and writing it is the act.** Its getter is the choice
  matching `BackingAccount`. Its setter calls `MoneyBudApp.SetBacking`, and ignores a null written
  while the list is rebuilt. `SetBacking` does nothing at all for the backing already set, so the
  list writing back what it shows, on first show and on every redraw, moves no money, says nothing
  and saves nothing.
- **`BackingChoices` is one collection shared by every row**, made anew only when `AccountChoices` is,
  so when the accounts, their order or their names change. That is the accounts increment's rule for
  lists (*Accounts: chosen in the build*), applied to a list on every row. A new collection on every
  refresh would make each row's list let go of its choice once a minute.
- **The expense form's account follows the category typed** until the user picks one.
  `ExpenseForm.ChosenAccount` reads the picked account, or else the typed category's backing account,
  or else the pool account. A write of the account already shown is taken as the list writing back,
  and any other account as a pick. That is the one inference the form makes from its list, kept as
  small as it can be. So picking the account already shown does not stick, which the stakeholder
  accepted after the build, because telling that pick apart would need the window to decide what a
  click means ([§12](12-glossary.md), *Backing: ruled after the build*, ruling 5). An entry being
  changed keeps its own account.
- **Settling is kept by the screen.** The constructor settles and saves if anything moved. `Tick`
  settles and saves if anything moved or an earlier save failed. An act settles inside the ledger,
  and what moved is saved with the act. The gap, a refused act straight after a period began, is
  in [§6](06-runtime-view.md).
- **The words are `Tekst.BackingAccount`, *Staat op*, and `Tekst.Accumulated`, *Opgebouwd***, two
  rows of §12's display-terms table that `TekstTests` holds. "—" is `Tekst.NoBacking`, a symbol and
  not a term. The notices and the movement row's wording are copy (`Tekst.BackingSet` and the history
  line's text).

**The Desktop only binds.** The *Staat op* `ComboBox` beside *Hernoemen* binds to `BackingChoices` and
`ChosenBacking`. The *Opgebouwd* caption under the row's figures binds to `AccumulatedText` and its
marker. Movement rows in the history bind like transfer rows, with no *Wijzigen* and no
*Verwijderen*. No markup test covers any of it. **A headless run of the real window checked the
write-back**: the lists writing back on first show and when the accounts changed moved no money and
announced nothing ([§11](11-risks-and-technical-debt.md), the Desktop row).

### The sweep on screen

The rulings are in [§12](12-glossary.md), *The sweep and Restant*. The build's own readings are in
*Sweep: chosen in the build, not put to the stakeholder*, there. How the presentation layer holds
them:

- **The Overview carries either the list or the line, never both.** In the current period and later
  ones `ShowsSweepDestination` is true, and `SweepChoices` and `ChosenSweepDestination` are the *Restant
  naar* list. In an ended period `SweepLine` is `Ledger.SweepLineFor` that period, `SweepLineText` its
  words, `SweepLineMarker` the one marker for a shortfall, badge *Tekort*, and `CanBringSweepUpToDate`
  whether the button shows. Rebuilt on every read, so a line that changes after a late entry changes
  at the next redraw, and nothing clears it.
- **The list is "—" and then the backed categories alphabetically**, compared as the category
  suggestions are (`MoneyBudApp.Alphabetical`), from `Ledger.SweepDestinationChoices`, which gives them
  in the order added. `MoneyBudApp.SweepChoices` is one collection, made anew only when what it offers
  or a name in it changes, for the reason the account lists are (*Accounts: chosen in the build*).
- **`ChosenSweepDestination` is the two-way binding, and writing it is the act.** Its setter hands the
  category's name, or null for "—", to `MoneyBudApp.SetSweepDestination`, and ignores a null written
  while the list is rebuilt. The destination already set does nothing at all, so the list writing back
  what it shows moves nothing, says nothing and saves nothing.
- **Sweeps are said and kept wherever settling ran.** `Tell`, `Refuse` and `SayNothing` take the
  ledger's sweeps made, put their sentences first and keep the ledger, whatever the act did. The
  constructor and `Tick` do the same, and a sweep on the tick drops a waiting question. The runtime
  is in [§6](06-runtime-view.md), *The sweep at settling*.
- **Clearing the destination is said by the act that cleared it.** Unbacking, archiving and deleting
  a category compare `Ledger.SweepDestination` before and after the act, and add *"Restant gaat niet
  meer naar …"* when it was cleared. The domain's result types did not change for it.
- **`BringSweepUpToDate` acts on `ShownPeriod`**, never asks, and says every move, a sentence per
  category. It settles first, for the minute after a boundary (*Nothing is cached*, above).
- **The words are six rows of §12's display-terms table**: `Tekst.PeriodLeftover` (*Restant*),
  `SweepDestination` (*Restant naar*), `BringUpToDate` (*Restant bijwerken*), `StillToSweep` (*nog niet
  weggezet*), `SweptTooMuch` (*te veel weggezet*) and `PeriodShortfall` (*Tekort*), held by
  `TekstTests`. "—" is `Tekst.NoSweepDestination`, the same symbol as `NoBacking`. The notices, the
  line's sentence and the history row are copy.
- **A sweep's period is named with the default calendar.** `Tekst` turns a sweep's `SweptFor` into a
  period with a new `BudgetPeriodCalendar`, which is right while the start day is fixed at the 1st and
  would be wrong under another ([§11](11-risks-and-technical-debt.md), the start-day row).

**The Desktop only binds.** A line directly under the ring holds either the *Restant naar* caption and
`ComboBox`, visible on `ShowsSweepDestination`, or the line's text, the marker with *Tekort* on
`IsSweepLineShort` and the *Restant bijwerken* button on `CanBringSweepUpToDate`, bound to
`BringUpToDateCommand`. No markup test covers it. **A headless run of the real window checked it**:
the list writing back on first show and when the backed categories change said and saved nothing, the
line and the button showed in an ended period, and a tick across a boundary showed the sweep's notice
([§11](11-risks-and-technical-debt.md), the Desktop row).

### Recurring entries on screen

The rulings are in [§12](12-glossary.md), *Recurring entries*, with the build's own readings and the
one ruling taken at the build in *Recurring entries: chosen in the build*, there. How the presentation
layer holds them:

- **Each entry form ends with a *Herhalen* list.** `FrequencyChoices` is `FrequencyChoice.All`:
  *Eenmalig*, *Wekelijks*, *Maandelijks*, in the ruled order, each a `FrequencyChoice` over a
  `Frequency?`. `Frequency` is what the form holds, null (*Eenmalig*) until chosen or loaded.
  `ChosenFrequency` is the two-way binding.
- **The lock is the domain's, read afresh.** `CanChangeFrequency` is true for a new entry and, in
  *Wijzigen*, `Ledger.SetsTheRepeat` of the entry. While locked, `ChosenFrequency` shows *Eenmalig*
  and ignores every write. `Load` takes the frequency from `Ledger.FrequencyOf`, and `Clear` resets
  it. `MoneyBudApp.Refresh` has both forms re-read the lock, so an entry open in the form when the
  tick records its next occurrence locks there (plan reading 4).
- **The list writes back like the account list**: a plain value, a null write ignored, and a write of
  the value already held changing nothing. So a list writing back on first show, on load or after a
  tick cannot turn an unchanged save into a change. `Record` hands on `Frequency`, and `Save` hands on
  `ChosenFrequency.Frequency`, so a locked entry always hands on one-off, which the domain ignores.
- **The grey label is on the Overview's rows only.** `ExpenseLine.RepeatLabel` and
  `IncomeLine.RepeatLabel` are *maandelijks* or *wekelijks* when `FrequencyOf` is not null, and null
  on every other row. `HistoryLine` has no such field (scenario-stage ruling 3).
- **Occurrences are said and kept wherever settling ran**, taken beside the sweeps in the
  constructor, `Tick`, `Tell`, `Refuse` and `SayNothing`. `Notice.Repeated` lists the occurrences a
  notice names, so a step can check what was named without parsing the sentence. The order of the
  sentences, and `SettleBeforeActing` in the four entry acts, are in [§6](06-runtime-view.md),
  *Settling day by day*.
- **Removing the latest occurrence of a running repeat asks one sentence more**, that the repeat goes
  on and how to stop it (`Tekst.AskToRemove` with `repeatGoesOn`). Every other removal question is as
  it was.
- **The words are two rows of §12's display-terms table**: `Tekst.Frequency` (*Herhalen*), and
  `OneOff`, `Weekly` and `Monthly` (*Eenmalig*, *Wekelijks*, *Maandelijks*), held by `TekstTests`. The
  grey label is `Tekst.RepeatLabel`, the same word lower-cased. The *"Herhaald: …"* sentence
  (`Tekst.Repeated`) and the removal question's extra sentence are copy.

**The Desktop only binds.** The *Herhalen* `ComboBox` is the last field of both entry forms, in a
`DockPanel` with its caption to the left, bound to `FrequencyChoices` and `ChosenFrequency` and
enabled by `CanChangeFrequency`. The grey label is a caption beside the account name on the row, on
one line. `WindowMarkupTests` holds the list last on both forms, after the account. No markup test
covers the lock, the write-back or the label. **A headless run of the real window checked them**, in
a scratch Avalonia.Headless harness outside the repository, over the real `MainWindow` with synthetic
data, and every check passed:

- on first show both *Herhalen* lists showed *Eenmalig*, and their write-back said and saved nothing;
- two rows carried *maandelijks*, one of them beside its account name, *Creditcard*, and the caption
  *Herhalen* was shown;
- the latest occurrence opened on *Maandelijks*, changeable, and saving it unchanged after the list's
  write-back said and saved nothing;
- picking *Wekelijks* in the window's own list and saving was a change, *"Uitgave gewijzigd…"*, and
  the row then said *wekelijks*;
- an income left open in the form across a tick that recorded its next occurrence stayed open, was
  then locked on *Eenmalig* with the list disabled, the tick's *"Herhaald: …"* notice was on screen,
  and the ledger was saved;
- the screen stayed on the period it showed; the earlier occurrence opened locked on *Eenmalig*, saving
  it unchanged said nothing, and the new latest occurrence, a period on, still carried the label;
- a rendered frame showed the caption off-centre by the style's bottom margin, the same margin the
  stakeholder had noticed on *Staat op*. With the margin moved to the `DockPanel`, it centres.

([§11](11-risks-and-technical-debt.md), the Desktop row.)

### Pointing at the ring: the Desktop hands over a share, and nothing more

`RingControl` turns the pointer's position into a share of the ring, read clockwise from the top.
When the pointer is off the band it passes nothing. It hands that to `MoneyBudApp.PointAt`, and
that is all it does. `Ring.SliceAt` decides which slice is at that share. `PointedSlice`,
`PointedRow` and `RingCentreShowsUnassigned` decide what the ring's hole shows. What is drawn
highlighted is the slice `PointedSlice` names. The app holds the **share**, not the slice, so the
slice is looked up afresh on every read. So a pointed slice can never show a figure that has since
changed ("Nothing is cached", above). Stepping clears the share. The rulings are in
[§12](12-glossary.md), *Hovering a slice shows its figures*.

### Tests that read the window's markup

**The Desktop has no automated tests, by plan** ([ADR 0006](../decisions/0006-three-source-projects.md),
[§11](11-risks-and-technical-debt.md)), **with one exception, which now holds two things.** The
second, since the persistence increment, is that the save line is a sibling of the notice and the
question (*Keeping the ledger*, above). The first, described here, is older. The order of a form's fields is a
stakeholder ruling ([§12](12-glossary.md), *The fields ask what before how much*), and it can live
only in `MainWindow.axaml`. `WindowMarkupTests` reads that file as XML text, through
`Support/Repository`, the same helper `TekstTests` uses to read §12. It checks the fields each form
lays out, in order: down a stack in the order written, and across a grid by `Grid.Column`. It starts
no window and references no Avalonia, so the specs project still does not reference the Desktop.
**Approved at the plan gate on 2026-09-26 as a small departure** from ADR 0006. `spec-reviewer`
found that it first ordered by position in the file rather than by column. That was fixed.

**What it does not change.** Everything else the Desktop does is still checked only by running it.
Reading markup as text is a narrow tool: it checks what is written, not what is drawn. It is not a
licence to leave a decision in the window because a text test could reach it. A decision that
*can* live in `MoneyBud.Presentation` still goes there.

### How the scenarios are run

| Step | Acts on | Why |
|---|---|---|
| ***Given*** | **The ledger, directly** | Setting up is not what is under test. A budget in a past period is still made by moving the test clock and assigning ([§8.1](#81-domain-model)), so setup cannot make a state the rules forbid |
| ***When*** | **`MoneyBudApp`, for every feature file**, the five that predate the UI included | So every scenario goes through the doors the Desktop uses. An amount arrives as the text in the scenario, read by `AmountInput`. A date the step does not name is left out, so the screen's own default decides it. A *When* whose amount is not read as one **fails the scenario** rather than passing as a refusal: those scenarios' amounts are all meant to reach the domain. The one exception is `type-an-amount.feature`, whose quoted amounts are sometimes meant not to (below). **A correction goes one step further in, through the form**: the step clicks the entry's row, changes the one field it names as the user would type it, and saves the form. So every *saved with nothing changed* scenario loads an entry and saves it back, and proves that what a form loads can be read again. A removal presses *Verwijderen*, checks that a question is waiting while the entry is still listed, and answers it |
| ***Then*** about **what is shown** | **The presentation layer**: the period's `PeriodOverview`, its rows, ring and lists, the suggestions, and the notice | Each is a claim about what MoneyBud shows. A period is read with `OverviewFor`, without stepping to it, so checking one period never moves the screen a later step asserts on. This is what closed the [§11](11-risks-and-technical-debt.md) row about "shown" steps bound to the ledger |
| ***Then*** about **figures and refusals** | **The domain** | A *Budget*, a *Remaining* or an *Unassigned* is a domain figure, and the rows show the same figures. A refusal is asserted as its **reason**, never as its Dutch sentence, because the sentence is copy |
| **Keeping, starting and closing** | **The real `FileLedgerStore`**, in a temporary folder of the scenario's own, through `MoneyBudStart.Start` and `MoneyBudApp.Close`, the doors the Desktop uses | Since the persistence increment, **every** scenario keeps its data for real, not only the three persistence files. What the *Givens* set up is saved when the screen first opens, as data MoneyBud already had. What happens *around* MoneyBud is done to the folder, as it would happen on a disk (below). Nothing in product code knows it is being tested |

**How the persistence scenarios reach the disk** (`SpecContext`, `KeepingSteps`):

- **"Saving is not possible"** puts a directory where `moneybud.json.tmp` goes. Opening the temporary
  file then fails as a full disk or a missing profile would, and `TrySave` reports `false` by its
  own code path. "Possible again" removes the directory.
- **"MoneyBud is interrupted while saving"** is a **simulation, approved as one at the plan gate.**
  A real save cannot be cut part-way from a test. The step rebuilds the disk state such a cut leaves,
  by the store's own design: the data file as it was before the last save, restored from what the
  store held before saving, and half of the new file in `moneybud.json.tmp`. It then drops MoneyBud
  without closing: no last attempt, and the lock let go as the operating system lets go of a dead
  process. That the store never writes the data file in place is held by `StorageTests`, not by the
  scenario.
- **"A while passes with nothing done"** is one call to `Tick`, which is what the Desktop's timer
  makes once a minute.
- **"MoneyBud should offer no act for saving / starting over"** lists every command of the screen and
  of the four forms **in full**, and checks every `Command` binding in `MainWindow.axaml` against
  that list. A new act, one to save or to start over, fails the step until someone looks at it.
  `spec-reviewer` found the first version of this step too weak, and it was strengthened to this.
- **Unreadable data** is written into the folder by the step: a file cut off mid-document, a blank
  file, or a file whose version is one higher. The *Then* checks the file byte for byte afterwards,
  and that no temporary file was made.

**How the opening-a-period scenarios reach the screen** (`TakeOverSteps`, with small additions to
`ScreenSteps`, `AssignSteps`, `KeepingSteps` and `SpecParsing`):

- **"I take over the plan offered"** first checks that the Overview has the button to press, then
  calls `MoneyBudApp.TakeOverPlan`, the button's own door. **"I try to take over the plan offered"**
  does not check, because it is the button as last drawn, pressed after a boundary the screen has not
  caught up with.
- **"…before the Overview is next drawn"** moves the clock into the next period without the timer's
  refresh, so the button is still there to be pressed on a period that has just become past. The
  older "the next budget period begins while MoneyBud is open" step, which does refresh, is
  **anchored**, because unanchored it also matched the start of the new one.
- **A refused take-over proves nothing was assigned anywhere**: the *When* keeps every budget in
  every period beforehand, and the *Then* compares. Being told is checked on the notice: that it is
  a refusal, and that it is the sentence `Tekst` makes for a past-period assignment. `spec-reviewer`
  found the notice unchecked at first. A take-over's notice is checked the same way, against the
  sentence `Tekst.PlanTakenOver` makes, and that it names the period. Comparing with what `Tekst`
  produces rather than with a quoted sentence keeps the wording copy: rewording it stays a change to
  `Tekst` alone.
- **The categories table has an optional `plan` column**, checked against both `PlanFigure` and
  `PlanText`, with `none` for a row that shows no figure. A period can be named as "the budget period
  2 before the current one", for the skipped month.
- **"MoneyBud should offer no act for saving / starting over"** lists `TakeOverCommand` among the
  screen's commands, as an act that only assigns.

**How the backing scenarios reach the screen** (`BackingSteps`, with additions to `ScreenSteps`,
`AccountSteps`, `RecordExpenseSteps`, `KeepingSteps` and `SharedSteps`):

- **Setting and removing a backing** goes through `MoneyBudApp.SetBacking`, the door the row's list
  uses. The notices are checked against what `Tekst.BackingSet` makes, so the wording stays copy. The
  short "is now backed by …" step is anchored, so that it does not also match the long form that
  names the money.
- **The categories and slice tables gained `accumulated` and `accumulated marked` columns**, read
  from the row and the slice. The history table gained the entry kind `movement` and the columns
  `category`, `from` and `to`. *Accumulated* Givens check the figure and set nothing.
- **The expense form's account** is typed and chosen through the form itself ("I type … as the
  category of the new expense", "I choose the account … for the new expense"), and read back from
  `ChosenAccount`.
- **"The next budget period begins while MoneyBud is open" now calls `MoneyBudApp.Tick`**, which is
  what the Desktop's timer calls, so a scenario sees the money settled by the tick. It used to call
  `Refresh`. The "…before the Overview is next drawn" variant only moves the clock, and the act that
  follows settles first.
- **"Accumulated for … should be N" always reads the row.** As first built it fell back to the figure
  a hidden row would show, for one approved scenario that asserted *Accumulated* on an archived
  category whose row the display rule hides. **That scenario line was amended with the stakeholder's
  approval** (2026-09-27): it now asserts that the category is not shown, and the fallback is gone
  ([§12](12-glossary.md), *Backing: ruled after the build*, ruling 4).
- **The new display clause has its scenario**: an archived backed category with money built up is
  shown in the current period and later ones until it is unbacked, the last scenario of
  `show-accumulated.feature`, added after the gate with the ruling.

**How the sweep scenarios reach the screen** (`SweepSteps`, with additions to `SharedSteps`,
`BackingSteps`, `CategorySteps`, `CorrectionSteps` and `KeepingSteps`):

- **Choosing a destination is a choice in the *Restant naar* list**, by what the list shows, handed to
  `MoneyBudApp.SetSweepDestination`. The list is only on the current period and later ones, so the step
  steps forward first when an ended period is on screen. **Pressing *Restant bijwerken*** shows its
  period, checks that the button is offered, and calls `MoneyBudApp.BringSweepUpToDate`.
- **Every figure is read from the screen**: the ended period's line, from `OverviewFor` that period
  without stepping to it; the list's choices in order; and the notice. The notices are checked to
  **contain** the sentence `Tekst` makes, not to equal it, because one notice can carry the sweeps of
  several periods, or a sweep in front of what an act says. The backing, archiving and deleting steps
  check their notices the same way, since clearing the destination adds a sentence to them.
- **A scenario's ledger is made in the current period**, so the previous period has no period-end
  record and counts as ended before the first start. That is the scenarios' first-start convention
  (§12, *For the plan*), and it holds by construction. A scenario that needs an automatic sweep lets a
  period begin while MoneyBud is open, through `Tick`, or starts MoneyBud again later.
- **"I should not be warned or asked to confirm"** accepts the sweep's two results, and the case of no
  act before it, when a period boundary alone swept. The backing and archive steps find the **current**
  period's row when an ended period with no row for the category is on screen, and the correction
  steps step back to the current period the same way.

**How the recurring-entries scenarios reach the screen** (`RecurringSteps`, with additions to
`RecordExpenseSteps`, `RecordIncomeSteps`, `CorrectionSteps`, `ScreenSteps`, `SharedSteps`,
`KeepingSteps`, `SpecContext` and `SpecParsing`). They are the first to use calendar dates, because a
monthly repeat keeps a day of the month:

- **"Today is 25 August 2026" makes a new empty ledger on that day** (`SpecContext.BeginOn`), so the
  empty ledger counts as first started then, which is the binding note approved at the scenario gate.
  It must come before any other setup, and throws otherwise rather than silently losing what was set
  up.
- **", repeating monthly" or ", repeating weekly" is an ending on the record steps**, *Given* and
  *When*, with and without a date, an account or a label. It is one group that **always matches**,
  empty or not (`SpecParsing.RepeatingEnding`), because a group that may not match would change the
  step method's number of arguments. The date and account patterns stop before it, so the ending is
  never swallowed into either. A *When* that repeats goes through the form, choosing in its *Herhalen*
  list. A *Given* sets the repeat up on the ledger, as every *Given* does, and **fails if an occurrence
  was already due**, since a setup step must not record one that no *Then* would account for.
- **"The day becomes D while MoneyBud is open"** moves the clock and calls `Tick`, as the Desktop's
  timer would. **"I close MoneyBud, and start it again on D"** restarts through the real store on
  that day.
- **"I should be told, in one notice, that these repeating entries were recorded"** compares the
  table, as a multiset, with `Notice.Repeated`, and checks that each occurrence's words are in the
  notice's text. It requires a notice **new since the last *When***: a `BeforeStep` hook notes the
  notice before each *When*, and a notice left over from an earlier step fails the step even when it
  names the same entries. `spec-reviewer` found the step accepting a stale notice; a mutation that
  stopped the tick announcing occurrences now fails it.
- **"MoneyBud should not have recorded any repeating entry"** uses the same hook: it notes, before
  each *When*, every occurrence MoneyBud has recorded (all of each repeat's but the first, which the
  user typed), and checks that none was added since, and that a new notice names none.
- **The list tables take an optional `repeats` column**, checked against the row's `RepeatLabel`; a
  table without it does not check it. Frequency changes and "should open with the frequency X,
  changeable / locked" go through the form, like every correction.

**What the unit tests cover** (`tests/MoneyBud.Specs/Unit/`): reading typed amounts, the Dutch
wording against §12, money formatting, the ring's shares and its minimum width, the forms,
narrowing the suggestions, pointing at the ring (`PointingTests`), and the order of the window's
fields (`WindowMarkupTests`, above), alongside the domain's tests from earlier increments. The
corrections increment added `CorrectionTests`, for what the scenarios cannot see: entry ids, a
change keeping its id and place, a stale copy still finding its entry, re-keying on a rename, a
deleted category's zero budgets going with it, and the non-cases that throw. It also added that
`AmountInput.Format` reads back to the same amount, and the forms' *Wijzigen* state. The persistence
increment added `StorageTests`: that everything the ledger holds comes back from the format exactly;
that ids carry on after loading; that anything but a whole version-1 document is unreadable, a
fraction of a cent and cents as text included; that kept data breaking any domain rule is refused,
a budget in the calendar's last month among them; that a failed save leaves the kept file byte for
byte; that half a save left in the temporary file is never read and is overwritten; the lock; an
unreachable folder; that the default folder is the local application data and never the
repository; and which acts save. It also added a second test to `WindowMarkupTests`, for the save
line. The opening-a-period increment added `TakeOverTests`: that the search back has no limit; that
the plan comes whole from one period, not from each category's own latest figure; that its figures
are in order added; the non-cases that throw (taking over where nothing is offered, and a date range
that is not a period); the button's, the grey figure's and the notice's wording against §12's
example; and that rows sort by plan figure, equal figures in order added, and do not move when the
plan is taken over. The accounts increment added `AccountTests`, which covers:

- how a balance is worked out: with no typed balance; what a typed balance holds, on its own day and
  before; that a changed entry keeps its first recording; that the latest typed balance counts; and
  that a future income reaches the balance only on its date;
- the difference: worked out again as forgotten entries are found, and none for a starting balance;
- transfers, including one across a balance correction changing net worth;
- the order of accounts, and that an account may share a category's name but not another account's;
- the history's order;
- that every balance and difference survives being kept and read back;
- the non-cases that throw.

`StorageTests` now reads version 2 and refuses version 1. `WindowMarkupTests` holds the account list
last on both entry forms and the transfer form's order, Van → Naar → Bedrag → Datum. ADR 0004's rule
applies to them unchanged: a unit test is never the reason a behaviour exists.

The backing increment added `BackingTests`, which covers:

- what is there for a category: only expenses paid from the backing account count, a third account's
  do not, it can go below zero, and when the pool account backs the category what was assigned is
  there and goes along on re-pointing;
- *Accumulated*: none for an unbacked category, €0 before anything moved, what is planned in a later
  period, money not yet settled counted as planned so the figure does not change when it moves, and
  starting over after unbacking but not after re-pointing;
- the cap on a negative assignment, and nothing moved when nothing is there;
- settling: nothing today for a later period, each period's money once on its first day to the
  backing of that moment, nothing for a category unbacked since, every act settling first so a balance
  correction that day holds the movement, and a clock turned back settling nothing and forgetting
  nothing;
- the backing already set changing nothing, a pool-to-pool movement not counting as money moved, and
  backing an archived category leaving it archived;
- what counts as used: an account that backs a category or has a movement, and a category money was
  moved for between two accounts, while a pool-to-pool movement does not block deleting and goes
  with the category; deleting a category with no history taking its backing with it; and the
  non-cases that throw.

`StorageTests` now reads version 3, refuses versions 1 and 2, and keeps backings, movements and
`settledThrough` exactly. `FormTests` covers the expense form's account following the category and
sticking once picked, including the write-back of the account already shown, and the *Staat op*
list's choices and its shared collection. `TekstTests` holds *Staat op* and *Opgebouwd* to §12's
table.

The sweep increment added `SweepTests`, for the domain, which covers:

- the *Period leftover*: netting, a backed category's *Remaining* left out, backing judged as it was
  at the period's end in both directions, a period not yet ended judged by today's backing, an
  archived unbacked category counting, and nothing backed before the first start;
- settling: each ended period swept at its own end, in order, and told once; nothing without a
  destination, nothing at zero or below; a sweep into a category the pool account backs moving no
  balance and counting in *Accumulated*; the ended period swept before the new period's planned money;
- the line in each state, and none for the current period and later ones;
- the button: still to sweep going to today's destination, dated today; swept too much coming back
  latest move first whichever category it went to, a move partly undone giving back only what is left
  of it, the earlier move asked before anything is let go, what none can give let go for good, nothing
  back from a category no longer backed or backed again since, re-pointing keeping what can come back;
  and the throw where the line offers no button;
- the destination: the choices, the no-op before settling, a change that settles first, the throws,
  clearing on unbacking, archiving and deleting and keeping on re-pointing;
- what counts as used: any sweep for a category, and a sweep from an account to itself for the
  account; and deleting a category taking it out of the period-end records.

`SweepScreenTests` covers the presentation layer: the list's order and its shared collection, the
write-back doing nothing at all, the line's wording in each state and with two categories, the
shortfall's marker, several periods in one notice oldest first, a sweep said before what an act says,
and said and kept after a refused act and an act that changed nothing, a sweep on the tick dropping a
waiting question, a tick that sweeps nothing saying nothing, and **the button pressed in the minute
after a boundary moving nothing more**. `StorageTests` now reads version 4, refuses versions 1 to 3,
keeps sweeps, the destination, period-end records and amounts let go exactly, and refuses a sweep with
no period. One of its tests stands in for a scenario that cannot tell kept period-end records from
lost ones (§12, *A note for the stakeholder*), and a mutation check confirmed it fails when they are
lost. `TekstTests` holds the six new rows.

The recurring-entries increment added `RecurringTests`, for the domain, which covers:

- the date arithmetic: weekly, and monthly on a late day through short months, back to its day after;
- settling day by day: a period's occurrences before its sweep and the boundary day's after it,
  several due on one day in the order set up, a repeat set up in the past recording what is due at
  once, the occurrences made given once, and a clock turned back recording nothing;
- an occurrence: copying the latest, its account included, and becoming the new latest; bringing an
  archived category back and saying so; an income repeating as an expense does;
- the latest by id, not by date, when an earlier occurrence is moved past it;
- changing: a locked occurrence ignoring the frequency it is given, the latest saved with its own
  frequency unchanged and with another a change, a change naming no frequency leaving the repeat, a
  refused change leaving it as it was, and only a changed date on the latest moving the day;
- removing: the latest handing over and keeping the next date, the only one ending the repeat, and a
  stopped repeat's last leaving it stopped; one-off stopping it and the last occurrence starting it
  again with what is due.

`RecurringScreenTests` covers the presentation layer: the list's choices, order and default; the list
writing back what it shows leaving an unchanged save unchanged; an entry open in the form locking once
the next occurrence is recorded; the label on the latest row of the Overview and nowhere in an
account's history, and *wekelijks* on a weekly income; the notice's sentence with each day, and its
order, occurrences an act caused after the act's sentence and those settling recorded before it in
front; occurrences said and kept after a refused act; a category brought back in the same notice; the
tick dropping a waiting question when it records one and saying nothing when it does not; and the
removal question's extra sentence. `StorageTests` now reads version 5, reads version 4 as data with no
repeats and writes version 5 back, refuses a version-4 document with repeats and versions 1 to 3,
keeps a running, a weekly and a stopped repeat exactly, a monthly one on the 31st included, refuses a
frequency word it does not know, and holds every new load check. `WindowMarkupTests` holds *Herhalen*
last on both entry forms, and `TekstTests` the two new rows.

**A ruling made after the scenario gate got its scenario: a data folder that cannot be reached.** It
was ruled during review (2026-09-26), after `start-moneybud.feature` was approved, and for a moment
it was held only by `StorageTests`. That test checks that the store reports such a folder as
unreachable, for a folder under a file, a relative path and an empty one. By ADR 0004's rule a
scenario was missing, so **the stakeholder approved adding one row, `cannot be reached`, to the
approved "cannot read" outline** (2026-09-26). The file's header says it was added after the first
approval, with his approval. The step makes the folder unreachable for real: a directory stands at
the `moneybud.lock` path, so the claim fails with access denied. The data file holds valid data and
is compared byte for byte afterwards. **A mutation check confirmed the row bites**: turning
`Claim.Unreachable` into `AlreadyOpen` in `MoneyBudStart` fails exactly that row
([§12](12-glossary.md), *When the data's folder cannot be reached*).

**Reading a typed amount has its own feature file.**
[`type-an-amount.feature`](../../features/type-an-amount.feature) was approved at the scenario gate
on 2026-09-26 and is bound, in `AmountSteps`. Its *When* steps quote the typed text and are the only
ones that **do not fail** when it cannot be read, because there that is the case under test. They go
through the same `MoneyBudApp` doors as every other *When*. A refusal that never reached the ledger
is recorded as such, so "should not be recorded" can tell it from a domain refusal. The *Then* checks
the reading and that a refusal was said, and for an ambiguous amount that the notice names both
readings, but never the Dutch sentence itself. Every other feature file keeps the rule above: an
unreadable amount fails the scenario. This closed the
[§11](11-risks-and-technical-debt.md) row saying reading was held by unit tests alone (*Resolved*).
One approved refusal has no scenario: "€ −50", minus after the euro sign. It is held by a unit
test in `AmountInputTests` instead ([§12](12-glossary.md), *Typing an amount*).

**One ruling is held by unit tests alone: narrowing the suggestions as you type.**
`MoneyBudApp.SuggestionMatches` and `SuggestionsFor` decide it, and `FormTests` holds it. The
stakeholder did not ask for a scenario for it. The Desktop only passes the predicate to its category
box, so the rule is in the layer the tests reach, not in the toolkit.

**At the close of the UI increment**: 495 tests passing with zero warnings. That is 251 scenario
cases, 169 from the four earlier increments and 82 new, and 244 developer unit tests. The last six
unit tests came with narrowing's move into the presentation layer.

**With typed amounts specified**: 574 tests passing. That is 324 scenario cases, the 251 above and
73 from `type-an-amount.feature`'s 14 outlines, and 250 developer unit tests.

**After the first demo's rulings**: 620 tests passing with zero warnings. That is 340 scenario
cases, 16 more than the 324 above, from `point-at-a-slice.feature` and the new scenarios in
`overview.feature`, and 280 developer unit tests, 30 more than above.

**At the close of the corrections increment**: 785 tests passing with zero warnings. That is 467
scenario cases, the 340 above and 127 from the four corrections feature files, and 318 developer
unit tests, 38 more than above.

**At the close of the persistence increment**: 901 tests passing with zero warnings. That is 531
scenario cases, the 467 above and 64 from the three persistence feature files' 37 scenarios, and 370
developer unit tests, 52 more than above. One of the 64 cases is the `cannot be reached` row added
after review, which took the count from 900 to 901. A headless run of the real window, outside the repository, also showed
the question and "not saved" together, "saved again" after a retry, a second start refused, a damaged
file left untouched, and the file holding what was entered.

**At the close of the opening-a-period increment**: 935 tests passing with zero warnings. That is 554
scenario cases, the 531 above and 23 from `take-over-a-plan.feature`'s 19 scenarios, and 381 developer
unit tests, the 370 above and 11 in `TakeOverTests`. A headless run of the real window showed the
button's text and the grey figures in plan order, and after taking over: the budgets set, the rows
where they were, the button and the grey figures gone, and *Niet toegewezen* at −€ 1.450,00 with the
marker.

**At the close of the accounts increment** (2026-09-27): **1183 tests passing** with zero warnings.
That is 734 scenario cases and 449 developer unit tests. The new cases come from the six accounts
feature files, 174 cases, and from the scenarios added to `start-moneybud.feature` and
`keep-data.feature`. `spec-reviewer` found no faked scenario, one vacuous scenario and some defects,
all fixed. A headless run of the real window found how the forms must hold their accounts
([§12](12-glossary.md), *Accounts: chosen in the build, not put to the stakeholder*).

**At the close of the backing increment** (2026-09-27): **1331 tests passing** with zero warnings.
That is 822 scenario cases, 88 more than the 734 above, and 509 developer unit tests, 60 more than
the 449 above. The new cases come from the five backing feature files, 85 cases, and from the
scenarios added to `start-moneybud.feature` and `keep-data.feature`. Three of the tests came with the
rulings made after the build: the scenario for the new display clause, and the unit tests for
deleting a category, or an account, that has only movements from an account to itself — the
account case applying ruling 2's reason to accounts. Before them the count was 1328 (821 and 507). A headless run of the real
window checked that the *Staat op* lists writing back what they show, on first show and when the
accounts change, move no money and announce nothing.

**At the close of the sweep increment** (2026-09-28): **1471 tests passing** with zero warnings. That
is 888 scenario cases, 66 more than the 822 above, from the four sweep feature files and the scenarios
added to `start-moneybud.feature` and `keep-data.feature`, and 583 developer unit tests, 74 more than
the 509 above. `spec-reviewer` found two plan readings short of §12's wording, ruled the same day
(§12, *Sweep: ruled at the build*), a crash when *Restant bijwerken* was pressed in the minute after a
boundary, now handled, and one kept-data scenario that could not tell what it was about, now held by a
unit test. A headless run of the real window checked that the *Restant naar* list writing back what it
shows says and saves nothing, that an ended period shows its line and the button, and that a tick
across a boundary shows the sweep's notice.

**At the close of the recurring-entries increment** (2026-09-28): **1587 tests passing** with zero
warnings. That is 949 scenario cases, 61 more than the 888 above, from `repeat-an-entry.feature` (29
cases), `change-a-repeat.feature` (28) and the four scenarios added to `keep-data.feature`, and 638
developer unit tests, 55 more than the 583 above. `spec-reviewer` found no faked scenario and no money
defect, and two low items, both fixed: the one-notice step accepted a notice left from before the last
*When*, and a class comment still described the plan's order of the notice. One approved corrections
step conflicted with the plan's order of the notice, and the stakeholder ruled at the build (§12,
*Recurring entries: chosen in the build*). A headless run of the real window passed every check the
plan listed, and more: the *Herhalen* lists writing back on first show, on load and after a tick said
and saved nothing, an earlier occurrence opened locked, the label sat beside an account name, an entry
open across a tick locked there with the tick's notice on screen, and a frequency picked in the window
was a change. It also found the caption off-centre on its list, now fixed (*Recurring entries on
screen*, above).
