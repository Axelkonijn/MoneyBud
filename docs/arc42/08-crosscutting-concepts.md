# 8. Cross-cutting Concepts

**What belongs here:** Rules and patterns that apply across many building blocks — the domain
model, persistence, error handling, validation, logging, security. Anything a developer needs to
know regardless of which part of the system they're touching.

---

_§8.1, §8.2 and §8.3 are filled in._

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

### Only one of the two dimensions is built

§12 opens with every amount having a **location** (an *Account*) and a **purpose** (a *Category*).
**Only purpose exists in code.**

The location dimension is **absent, not stubbed.** There is no `Account` type, no account field on
`Expense`, no nullable placeholder waiting to be filled, and no hidden default account that a
balance could be computed against. Searching the source for "account" finds doc comments saying it
is not here and nothing else.

That is a deliberate shape rather than an unfinished one. A half-built dimension — a nullable
account, a `DefaultAccount` constant — would let code start depending on a model nobody has
designed yet, and every §12 rule about accounts (backing, the pool account, the sweep destination)
is a rule about a *relationship* that cannot be expressed by a field left blank. The cost is that
money leaving the system has nowhere to leave from, which is the last row of
[§11](11-risks-and-technical-debt.md) and is accepted there.

### The two layers meet in exactly one method

§12's second distinction — the **plan** and the **actual** — is the one the code is arranged
around. `Ledger` holds both and keeps them apart:

| §12 layer | In code |
|---|---|
| **The plan** — a category's *Budget* for a period | `Ledger.BudgetFor(category, period)`, over budgets stored per category per period |
| **The actual** — what was really spent | `Ledger.SpentOn(category, period)`, the sum of the `Expense` records whose date falls in that period |
| **Where they meet** | `Ledger.RemainingFor(category, period)` — the plan minus the actual, and the **only** place the two are combined |

Two of §12's rules are structural rather than checked, which is why they need no code of their own:

- **Recording an expense never touches a budget.** `RecordExpense` appends an `Expense` and does
  nothing else; there is no path from it to the budgets. "Assigning spends nothing" and "spending
  does not re-plan" are both true because neither operation can reach the other's storage.
- **A category with no budget set behaves as one budgeted at zero.** `BudgetFor` returns
  `Money.Zero` when it finds nothing. There is no "unbudgeted" state to represent, so nothing can
  branch on one, and a missing budget has no way to block a recording.

*Over budget* follows from *Remaining* alone — a negative `RemainingFor`. Exactly zero is not
negative, so §12's "spending a category down to nothing is the plan working" needs no special case
either.

### Setting a budget stands in for assigning

In §12 a *Budget* is what **assigning** from the pool produces. There is no income, no pool and no
*Unassigned* in this increment, so `Ledger.SetBudget` writes the plan directly.

**That is a scaffold for the scenarios, not a model of assigning.** It is the method to re-examine
first when income and assigning arrive: assigning has a source, may overdraw it, moves real money
for a backed category, may be negative, and floors the *Budget* it writes at zero
([§12](12-glossary.md)). `SetBudget` expresses none of that — it takes a `Money` and stores it,
with no floor and no source, because no approved scenario reaches any of those rules yet.

### "MoneyBud shows, it never blocks" is held by a type

`RecordExpenseResult` has exactly two shapes: recorded, or refused for exactly one reason. There is
deliberately no third. A "recorded with a warning" or "needs confirming" outcome **has nowhere to
live**, so the scenario *I should not be warned or asked to confirm* holds by construction rather
than by anyone remembering the rule.

Refusals are `ExpenseRefusal` **values, not messages**. The wording the user sees belongs to a UI
that does not exist yet ([§5](05-building-block-view.md)); putting copy in the domain would put it
in the wrong place and would make re-wording it a domain change.

### `Ledger` is not a §12 term, and that is worth flagging

§12 has no word for *the whole model* — the thing holding the categories, the budgets and the
expenses together. `Budget` was unavailable, because §12 pins it to the per-category plan.
`Ledger` was introduced in code to fill the gap.

**It should be revisited when accounts arrive.** In accounting a ledger is a book of *accounts*,
and *Account* is a §12 term for something MoneyBud deliberately has not got yet — so the name is
harmless now and will read oddly the moment the location dimension exists. Recorded here rather
than added to the glossary, because §12 holds the **domain's** vocabulary and this is a word the
implementation needed, not one the stakeholder uses.

### What has no code yet, and why

Everything below is in §12 and absent from `MoneyBud.Domain`. Nothing here is an oversight; the
reason differs by row, and the difference is the point.

| §12 concept | Why there is no code |
|---|---|
| *Account*, *Location*, *Balance*, *Net worth*, *Overdrawn* | The location dimension is not in the first increment ([§11](11-risks-and-technical-debt.md)) |
| *Account-backed category*, *Backing account*, *Accumulated* | Same. All three are relationships between a category and an account, so none can exist before accounts do |
| *Pool account*, *Sweep*, *Sweep destination* | Same, and doubly so: §12 requires a sweep destination to be account-backed, so the sweep cannot run at all ([§11](11-risks-and-technical-debt.md)) |
| *Income*, *Unassigned*, *Assign*, *Left to assign* | No approved scenarios. These are specifiable today — they need no accounts — and are simply not specified yet. This is the row that is *next*, not the row that is blocked |
| *Leftover* | Needs a period end to be computed at, and a sweep to be computed for. Nothing acts on a period boundary yet |
| *Recurring transaction* | A later increment ([§1.1](01-introduction-and-goals.md)) |
| *Over budget* as a stored state | Not missing — deliberately never stored. It is derived from *Remaining* wherever it is asked for, because §12 defines it as a property of a figure rather than a flag on a category |
| A period **closing** | Not missing — deliberately impossible. `BudgetPeriod` is a pair of dates with no state at all, so there is nothing that could ever refuse an expense on grounds of age (§12, *Ending versus closing*) |

## 8.2 Money Handling

**This section should be written before the first line of money-handling code.** Money decisions
are quietly expensive to reverse once data exists, and a budgeting app that gets them wrong is
wrong in ways that are hard to notice. Four rules are settled below. Two questions are still open,
and are listed at the end rather than guessed at.

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
re-argued. **A second currency belongs on that list as well** — a conversion rate is a computed
amount, and the "no currency field" rule below would be reopened at the same moment.

### Decided: in code, an amount is a `Money` value type over a `long` of cents

Reasoning in [ADR 0003](../decisions/0003-money-representation.md). What a developer needs to know
here:

- **`Money` wraps a whole number of cents as a `long`.** A sub-cent amount is not merely refused,
  it is **unrepresentable** — the rule above is a property of the type rather than a guard anyone
  has to remember. Addition, subtraction and equality are exact by construction.
- **`Money` is signed.** *Remaining*, *Left to assign* and *Balance* all go negative in normal use
  ([§12](12-glossary.md)); a negative amount is an ordinary value here, not an error.
- **`Money` is the domain's interior type, not its input boundary type.** The API that records an
  expense takes a **`decimal` euro amount**, and conversion to `Money` happens only *after*
  validation has passed. This is not stylistic: `-12.345` breaks the sign rule and the cent rule at
  once, and [`record-expense.feature`](../../features/record-expense.feature) requires the user to
  be told about the **sign** — so the cent check cannot be something that fires during
  construction, ahead of it.
- **`Money` has no division and no multiplication by a fraction.** Those are the operations that
  would produce a value the cent rule cannot hold, and they are exactly what the features in *when
  this has to be revisited* above would need. Their absence is what makes that list enforceable
  rather than advisory.

`decimal` is still the type amounts are **entered and parsed** as, which is what
[ADR 0001](../decisions/0001-dotnet-and-reqnroll.md) was pointing at when it named `decimal` as a
reason C# suits this domain. The two records agree; `decimal` simply stops at the boundary.

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
*Balance* below zero is *Overdrawn*, and *Left to assign* may go negative too
([§12](12-glossary.md)). Derived figures are not touched by this rule and must stay free to go
negative.

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

### Still open

Neither of the following has been decided. They are listed so that it is clear they were considered
and left open, not overlooked.

| Question | Note |
|---|---|
| How amounts are stored | Open **because [§8.3](#83-persistence) defers persistence as a whole**, not because it was overlooked. [ADR 0003](../decisions/0003-money-representation.md) settles representation in code; nothing is written to disk in the first increment, so there is nothing yet for a storage format to be wrong about |
| Period boundaries and timezones | The start day of a budget period is configurable (see [§12](12-glossary.md)); how that interacts with timezones is undecided, and `Ledger.Today` reads local time in the meantime as a stand-in rather than an answer. A **second, separate** question about the same boundary — which day a period starts in a month too short to contain the configured start day — **is now settled**, in [§12](12-glossary.md) rather than here, because it is about the calendar and not about money: the start day clamps to the month's last day |

## 8.3 Persistence

**Nothing is stored. State lives in memory for the lifetime of a run, and is gone when the
application exits.** This is a deliberate deferral with its reasoning recorded, not an unmade
decision.

**Why.**

- **No approved scenario observes persistence.**
  [`record-expense.feature`](../../features/record-expense.feature) never restarts anything and
  never asks whether an expense survives. Building storage now would be implementing behaviour
  nobody has specified, against a specification contract that is meant to be the thing driving what
  gets built.
- **The demo's data is explicitly throwaway.** The first version exists to be reacted to, not lived
  in ([§1.1](01-introduction-and-goals.md), [ADR 0002](../decisions/0002-desktop-application-first.md)),
  so there is nothing yet that anyone would mind losing.
- **The expensive part of this decision is not in this increment.** The second row of
  [§11](11-risks-and-technical-debt.md) argues that the storage shape should be settled **before
  accounts are built**, because that is when it becomes costly — whether a *Balance* is stored or
  derived from its transactions is the question that makes it so. Accounts are not in the first
  increment. Choosing a storage shape now would commit the hardest part of the decision at the
  moment we know least about it, and would do so to serve no scenario.

**What will force the decision.** The first time the stakeholder is asked to re-enter data he would
mind re-entering. That is the same moment [ADR 0002](../decisions/0002-desktop-application-first.md)
names as the point at which the demo has stopped being a demo, and it is deliberately the same
trigger: both decisions were bought with the same argument, so both expire together.

Two things should be settled at that point rather than drifted into: the form storage takes
([§7](07-deployment-view.md) lists it as open, along with where on the machine it lives), and how
amounts are stored (§8.2, *Still open*).

This is a **scope** decision rather than an architectural one, which is why it lives here and not
as a record in [`docs/decisions/`](../decisions/). MoneyBud already records "not in the first
increment" in the section the thing belongs to — [§12](12-glossary.md) does it for accounts, the
pool account, backed categories and the sweep — and none of those got a record of their own either.
