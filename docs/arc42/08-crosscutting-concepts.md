# 8. Cross-cutting Concepts

**What belongs here:** Rules and patterns that apply across many building blocks — the domain
model, persistence, error handling, validation, logging, security. Anything a developer needs to
know regardless of which part of the system they're touching.

---

_§8.1 to §8.4 are filled in. §8.4 arrived with the UI._

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
  neither operation can reach the other's storage.
- **Recording an income touches neither layer.** `RecordIncome` appends an `Income` and stops.
  Because a budget is reachable only from `Assign` and an expense only from `RecordExpense`,
  *Recording income leaves every category's plan and spending untouched*
  ([`record-income.feature`](../../features/record-income.feature)) is a property of the wiring
  rather than an assertion anything has to uphold.
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
  budget or any spending, plus one unit test. **This stays a watch-out.** A query that can tell
  apart two states §12 says are one is safe only while nothing a user sees is built on it. If a
  view ever wants it, revisit §12 first. **The corrections increment adds a second place it must
  not reach**: whether a category can be deleted. §12 settles that on figures alone, a budget of more
  than zero or an expense in any period, and rejected the rule `HasBudget` would give
  ([§12](12-glossary.md), *Deleting a category that has no history anywhere*). Settled 2026-09-26;
  not built.

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

Carry-over at period open, which is where an archived category's figure is not offered back, is
still unbuilt for every category (*What has no code yet*, below).

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

Refusals are `ExpenseRefusal`, `IncomeRefusal`, `CategoryRefusal` and `AssignRefusal` **values, not messages**. The wording the user
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

**This changed the expense side, not only the income side.** An expense's label is now trimmed too,
and one that trims to nothing becomes no label — which an expense is allowed to have. What differs
between the two transactions is only what each does with an empty result: an income refuses it, an
expense accepts it. That is a difference in the requirement, not in what a label *is*, and keeping
it to one method is what stops it becoming two.

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

### `Ledger` is not a §12 term, and that is worth flagging

§12 has no word for *the whole model* — the thing holding the categories, the budgets, the expenses
and the incomes together. `Budget` was unavailable, because §12 pins it to the per-category plan.
`Ledger` was introduced in code to fill the gap.

**It should be revisited when accounts arrive.** In accounting a ledger is a book of *accounts*,
and *Account* is a §12 term for something MoneyBud deliberately has not got yet — so the name is
harmless now and will read oddly the moment the location dimension exists. Recorded here rather
than added to the glossary, because §12 holds the **domain's** vocabulary and this is a word the
implementation needed, not one the stakeholder uses.

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
is approved. Read the absence of a §12 term from this table as "built", not as "nobody wrote a row
for it".

| §12 concept | Why there is no code |
|---|---|
| *Account*, *Location*, *Balance*, *Net worth*, *Overdrawn* | The location dimension has not been in any increment so far ([§11](11-risks-and-technical-debt.md)) |
| *Account-backed category*, *Backing account*, *Accumulated* | Same. All three are relationships between a category and an account, so none can exist before accounts do. This includes the **backed half of assigning**, where assigning really moves money out of a source it may overdraw. The assigning increment left it out for this reason, so every category is unbacked and `Assign` is planning only |
| *Pool account*, *Sweep*, *Sweep destination* | Same, and doubly so: §12 requires a sweep destination to be account-backed, so the sweep cannot run at all ([§11](11-risks-and-technical-debt.md)) |
| **Carry-over** at period opening (§12, *Budgets carry over as figures*), including that an archived category's figure is not offered back | Belongs with **period opening**, a slice of its own, and was put out of the assigning increment's scope for that reason ([§12](12-glossary.md), *Nothing here blocks the assigning increment*). Nothing acts when a period opens: a new period simply has no budgets until something is assigned |
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
| How amounts are stored | Open **because [§8.3](#83-persistence) defers persistence as a whole**, not because it was overlooked. [ADR 0003](../decisions/0003-money-representation.md) settles representation in code; nothing is written to disk in any increment so far, so there is nothing yet for a storage format to be wrong about |
| Period boundaries and timezones | The start day of a budget period is configurable (see [§12](12-glossary.md)); how that interacts with timezones is undecided, and `Ledger.Today` reads local time in the meantime as a stand-in rather than an answer. A **second, separate** question about the same boundary — which day a period starts in a month too short to contain the configured start day — **is now settled**, in [§12](12-glossary.md) rather than here, because it is about the calendar and not about money: the start day clamps to the month's last day |

## 8.3 Persistence

**Nothing is stored. State lives in memory for the lifetime of a run, and is gone when the
application exits.** This is a deliberate deferral with its reasoning recorded, not an unmade
decision.

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

**The Desktop also calls it once a minute**, and that is the only thing that moves the *Huidige
periode* label when a period ends while MoneyBud is open. The consequence, stated so it is not
rediscovered: **for up to a minute after a period boundary the screen can be out of date.** The label
can still read *Huidige periode*, and in-use categories with no history can still be listed in a
period that has just become past. An assignment made in that minute is refused as past, because the
domain reads the clock itself. Any act refreshes at once, so the refusal also corrects the label.
Nothing is announced either way, which is what §12 asks.

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

### Pointing at the ring: the Desktop hands over a share, and nothing more

`RingControl` turns the pointer's position into a share of the ring, read clockwise from the top.
When the pointer is off the band it passes nothing. It hands that to `MoneyBudApp.PointAt`, and
that is all it does. `Ring.SliceAt` decides which slice is at that share. `PointedSlice`,
`PointedRow` and `RingCentreShowsUnassigned` decide what the ring's hole shows. What is drawn
highlighted is the slice `PointedSlice` names. The app holds the **share**, not the slice, so the
slice is looked up afresh on every read. So a pointed slice can never show a figure that has since
changed ("Nothing is cached", above). Stepping clears the share. The rulings are in
[§12](12-glossary.md), *Hovering a slice shows its figures*.

### One test reads the window's markup

**The Desktop has no automated tests, by plan** ([ADR 0006](../decisions/0006-three-source-projects.md),
[§11](11-risks-and-technical-debt.md)), **with one exception.** The order of a form's fields is a
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
| ***When*** | **`MoneyBudApp`, for every feature file**, the five that predate the UI included | So every scenario goes through the doors the Desktop uses. An amount arrives as the text in the scenario, read by `AmountInput`. A date the step does not name is left out, so the screen's own default decides it. A *When* whose amount is not read as one **fails the scenario** rather than passing as a refusal: those scenarios' amounts are all meant to reach the domain. The one exception is `type-an-amount.feature`, whose quoted amounts are sometimes meant not to (below) |
| ***Then*** about **what is shown** | **The presentation layer**: the period's `PeriodOverview`, its rows, ring and lists, the suggestions, and the notice | Each is a claim about what MoneyBud shows. A period is read with `OverviewFor`, without stepping to it, so checking one period never moves the screen a later step asserts on. This is what closed the [§11](11-risks-and-technical-debt.md) row about "shown" steps bound to the ledger |
| ***Then*** about **figures and refusals** | **The domain** | A *Budget*, a *Remaining* or an *Unassigned* is a domain figure, and the rows show the same figures. A refusal is asserted as its **reason**, never as its Dutch sentence, because the sentence is copy |

**What the unit tests cover** (`tests/MoneyBud.Specs/Unit/`): reading typed amounts, the Dutch
wording against §12, money formatting, the ring's shares and its minimum width, the forms,
narrowing the suggestions, pointing at the ring (`PointingTests`), and the order of the window's
fields (`WindowMarkupTests`, above), alongside the domain's tests from earlier increments. ADR 0004's rule applies to them unchanged: a
unit test is never the reason a behaviour exists.

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
