# 8. Cross-cutting Concepts

**What belongs here:** Rules and patterns that apply across many building blocks — the domain
model, persistence, error handling, validation, logging, security. Anything a developer needs to
know regardless of which part of the system they're touching.

---

_§8.1 to §8.4 are filled in. §8.4 arrived with the UI. §8.3 records persistence as settled and
built, since 2026-09-26._

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
  `HasBudget` or anything else to find.

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

Refusals are `ExpenseRefusal`, `IncomeRefusal`, `CategoryRefusal`, `AssignRefusal` and, since the corrections increment, `RenameRefusal` **values, not messages**. The wording the user
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
([§12](12-glossary.md), *Demo data may not survive a new version*, *Real use before accounts*).

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
| **"Everything" is the ledger only**: categories, archived or not, budgets, expenses, incomes | Screen state is not stored: the period shown, a half-typed entry, a waiting question, a rename in progress. MoneyBud always opens on the current period. **Built:** `LedgerSnapshot` has the ledger's four lists and `lastEntryId`, and nothing else. `MoneyBudApp` is made fresh at every start, and its constructor puts the current period on screen |
| **Saved automatically after every change.** No save button. **A save that works says nothing** | Every act that changes the ledger ends with the data written. There is no save act and no "save now" state to offer, and no notice for a save that succeeds. **Built:** `MoneyBudApp.Tell` calls `Keep` after every act that went through **and changed the ledger** (`Tell(changed:)`). Adding a name already there, assigning zero and a negative assignment clipped in full against a *Budget* of zero are said but not saved. A refusal, an unchanged save and a declined question never reach `Tell`. A save that works sets nothing the screen shows, unless it ends a failure (below). The scenario "offer no act for saving" lists every command of the screen and the forms in full, and checks every `Command` binding in the window's markup against them |
| **An interrupted save never damages the previous one.** A crash or power cut loses at most the change being saved. **The next start opens normally and says nothing** about it | Writing must never leave a half-written save in place of a whole one. Nothing is recorded to detect or report a missing change at the next start. **Built:** `FileLedgerStore.TrySave` writes `moneybud.json.tmp`, flushes it to the disk, and renames it over `moneybud.json`. A leftover `.tmp` is never read and is overwritten by the next save. The next start loads `moneybud.json` as usual and says nothing. Held by `StorageTests`, and by a scenario that rebuilds the disk state a cut-off save leaves, approved at the plan gate as a simulation (§8.4) |
| **A failed save is said and the user carries on.** Closing before a save succeeds loses what was not saved, accepted. **The "not saved" notice stays on screen until a later save succeeds**, shown beside any other notice and beside the removal question, and not cleared by stepping. **Retried by every change and by MoneyBud itself now and then.** **Recovery is said once.** **Closing makes one last attempt**, and if it fails just closes, with no question | Nothing is undone and nothing is refused because a save failed. Each save writes the whole ledger, not the last change, so one success catches up every failure before it. "Not saved" is a **lasting state** of the screen, cleared only by a successful save. **Built:** `TrySave` reports `false`, and `MoneyBudApp.IsUnsaved` becomes true. **The save line**, `MoneyBudApp.SaveLine`, is a line of its own beside the notice and the question, so the one-message rule between those two is untouched ([§8.4](#84-the-presentation-layer)). It reads *"Je wijzigingen zijn niet opgeslagen. MoneyBud probeert het opnieuw."* until a save works, and stepping leaves it. Every later act that changes the ledger retries. **"Now and then" is once a minute**: `MoneyBudApp.Tick`, on the Desktop's existing timer, retries while something is unsaved. The save that works puts *"Alles is weer opgeslagen."* **on the same save line**, not in the notice, until the next act or step. `MoneyBudApp.Close` makes one last `TrySave` if something is unsaved, asks nothing, and lets go of the store |
| **A fixed place in the user's profile**, never chosen by the user, always outside the repository | The location is derived from the user's profile, **never** from the working directory. `dotnet run --project src/MoneyBud.Desktop` runs inside a working copy of the public repository, so a relative path would put data exactly where it must never be. `.gitignore` is a second line, not the protection. **Built:** `FileLedgerStore.DefaultFolder` is `Environment.SpecialFolder.LocalApplicationData` plus `MoneyBud`, which is `%LOCALAPPDATA%\MoneyBud` on Windows. A folder that is not a full path is refused as unreachable, so a relative path cannot be used even by mistake. `.gitignore` lists `moneybud.json`, `moneybud.json.tmp` and `moneybud.lock`. A unit test holds that the folder is outside the repository |
| **Unreadable data, damaged or written by a newer version: say so, touch nothing, and close.** Never start empty instead. The message says only that the data cannot be read: no path, no pointer to the README | A load that fails must leave nothing able to save over the file, and no screen to enter anything into. Automatic saving is what makes an empty start dangerous here. **Built:** `MoneyBudStart.Start` returns `Refused(CannotRead)` for a file `LedgerJson` cannot read and for kept data `Ledger.FromSnapshot` refuses. No `MoneyBudApp` is made, so nothing can save. The Desktop shows *"MoneyBud kan je gegevens niet lezen. Er is niets aan veranderd."* in a small window, and closing it closes MoneyBud. The message is copy in `Tekst`, not a display term. **"Touches nothing" means the data file**, as the stakeholder confirmed (2026-09-26): the claim, which comes before loading, may make the folder if it is missing and the lock file beside the data, and that is MoneyBud's own bookkeeping. Creating nothing at all was rejected, because it would mean checking the data before taking the lock, which leaves a window in which two MoneyBuds start at once |
| **A folder that cannot be reached at all is met the same way** (ruled 2026-09-26, during review): a profile that is not there, a folder MoneyBud may not open, a *file* standing where the folder should be | Say it cannot read the data, touch nothing, close. **Rejected:** starting empty and showing "not saved", because if the real data came back, the first save that worked would write the empty start over it. **Built:** `FileLedgerStore.TryClaim` returns `Claim.Unreachable`, and `MoneyBudStart` turns it into `Refused(CannotRead)`, the same message as above. Held by a `cannot be reached` row in `start-moneybud.feature`'s "cannot read" outline, added after the scenario gate with the stakeholder's approval (§8.4) |
| **A second start while MoneyBud is open is refused**: it says MoneyBud is already open, and closes | Only one process may hold the data. Two would overwrite each other's saves. **Built:** `TryClaim` opens `moneybud.lock` exclusively and holds it until `Close`. It is claimed **before** loading. A second start gets `Claim.HeldElsewhere`, and `MoneyBudStart` returns `Refused(AlreadyOpen)`: *"MoneyBud is al geopend."* The operating system lets go of the lock when a process dies, so a crash never blocks the next start |
| **Backups are not MoneyBud's job** | One set of data, and no copies kept by MoneyBud. **Built:** there is one data file and nothing copies it. The one extra file a save makes, `moneybud.json.tmp`, is renamed away, not kept |
| **Kept data that is there but blank is unreadable**: say so, touch nothing, close. **A saved empty budget is valid** | MoneyBud never writes a blank save, so blank kept data is a failure, not a first start. A save of a budget with no categories and nothing recorded is written, loads, and shows no categories (next row). Confirmed by the stakeholder, 2026-09-26. **Built:** `LedgerJson.Read` returns nothing for blank or whitespace-only text, which is unreadable. An empty ledger is written as a whole document with four empty lists and reads back as one |
| **One set of data, no in-app reset.** Starting over means deleting the file. **The defaults come only with a first start**, when there is no kept data at all | No act to start over, and no second set of data beside the first. A missing file is a first start, and nothing else is. A ledger saved with no categories loads with no categories. **Built:** only `LoadResult.NoData`, no `moneybud.json`, leads to `Ledger.StartNew`. A first start saves nothing until the first change |
| **No password, no encryption.** The Windows login is enough | Nothing to build. Security is the operating system's user account. **Built:** nothing, as ruled. The file is plain JSON |
| **Until real use starts, a new version may be unable to read an older one's demo data.** It then says so and touches nothing, and the user starts fresh. **Extended the same day: at least up to and including the accounts increment** | The stored form may change between versions without anything carrying old data across, the version that adds accounts included. Carrying data across versions becomes a requirement only at the switch to real use, which no increment before accounts plans around. **Built:** the file says `"format": "MoneyBud"` and `"version": 1`, and any other format or version is unreadable. There is no older version to read |
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
below). The consequence, stated so it is not
rediscovered: **for up to a minute after a period boundary the screen can be out of date.** The label
can still read *Huidige periode*, and in-use categories with no history can still be listed in a
period that has just become past. An assignment made in that minute is refused as past, because the
domain reads the clock itself. Any act refreshes at once, so the refusal also corrects the label.
Nothing is announced either way, which is what §12 asks.

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
to the stakeholder*, beside the rulings each one fills in.

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
  The two sentences, *"MoneyBud kan je gegevens niet lezen. Er is niets aan veranderd."* and
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
line. ADR 0004's rule applies to them unchanged: a unit test is never the reason a behaviour exists.

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
