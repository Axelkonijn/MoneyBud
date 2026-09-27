# 12. Glossary

**What belongs here:** Domain and technical terms, defined once. In a budgeting app this matters
more than usual — words like "account", "balance", "category" and "budget" all carry everyday
meanings that are close to, but not the same as, what they mean in the system.

Gherkin scenarios should use exactly the vocabulary defined here. Where they disagree, one of the
two is wrong.

---

## The central distinction

Every amount of money in MoneyBud has **two independent properties at once**:

| Dimension | Question it answers | Expressed as |
|---|---|---|
| **Location** | Where is this money? | An **Account** |
| **Purpose** | What is this money for? | A **Category** |

These vary independently. Two amounts in the same account can have different purposes, and one
purpose can be spread across several accounts.

This is what makes MoneyBud's two halves one system rather than two: **net worth** is everything
grouped by location, **budget** is everything grouped by purpose.

**Money can lack a purpose; it never lacks a location.** A salary that has just landed sits in an
account from the moment it exists, but nothing has yet said what it is for. The name for that state
is **Unassigned** — a value on the *purpose* dimension, the absence of a purpose,
**not a place money is kept**. Unassigned money is still in an account and still counts towards net
worth like any other money. It appears in the budget view beside the categories, never in the list
of accounts.

**Future-dated income does not weaken that rule, because it is not money yet.** An income dated
next month counts towards its period's *Unassigned* from the moment it is recorded (*Income may be
dated in the future*, below), which looks at first like an amount with a purpose and no location.
It is not. **There is no euro in existence to lack a location.** What MoneyBud holds until the date
arrives is the *record of a transaction that will happen*, not money sitting nowhere. The claim
above is about money, so it survives untouched.

**What that does cost is a difference in time base between the two views, and it is by design.**

| View | Time base | Expected income |
|---|---|---|
| **Net worth** (location) | A **point in time** — today's balances | **Not counted.** Net worth is what you have today |
| ***Unassigned*** (purpose) | A **whole budget period** | **Counted**, from the moment the income is recorded |

So the two views will disagree about future-dated income, on purpose. [§1.1](01-introduction-and-goals.md)
calls them "one model, two views"; the views answering *when* differently is not a crack in that
model — it is what the two questions mean. Net worth answers "how am I doing **now**", and an
amount that has not arrived is not something you have. *Unassigned* answers "what is there to
budget **for this period**", and an amount arriving inside the period is exactly what you have to
budget. **Nobody should have to discover this by noticing the two figures disagree**, which is why
it is stated here and again under *Net worth* in the terms table.

**A category may be *backed* by accounts, and that does not fold the two dimensions into one.**
Savings and Stocks are categories whose whole point is that the money really lands somewhere, so
they name the accounts it lands in. The relationship is **many-to-many**: one category can be
backed by several accounts, and one account can hold money belonging to several categories.
Backing records that a particular purpose's money really lives somewhere — it does not pin a
purpose to a place. Purpose and location still vary independently, and most categories are backed
by nothing at all. See *Account-backed categories* below.

**Raised on 2026-09-27, not settled.** The accounts rulings make a starting balance and a balance
correction count in net worth and nowhere on the purpose side, not even in *Unassigned*. That is
money seen by location only, which this section's "one set of data, two ways" does not allow for.
In the accounts increment that is accepted, because nothing on the purpose side reads a balance yet.
It is deferred to the backing increment's first stage (*A question for the backing increment*, under
*Accounts and net worth*, below).

**Answered on 2026-09-27, at that stage: such money stays seen by location only.** Backing a category
with an account moves nothing already in it, and *Accumulated* starts at €0 (*Money the user already
had stays by location only*, under *Backing and Accumulated*, below). So "one set of data, two ways"
holds for money MoneyBud saw given a purpose, and **not** for money that arrived as a typed balance,
which has a location and no purpose, and no act in MoneyBud gives it one. The stakeholder
chose that with its cost in view: *Accumulated* and a backing account's balance differ by it. The
paragraph above is left as written.

**The many-to-many relationship above stays the intended model**, but the backing increment builds
only part of it (settled 2026-09-27): **one backing account per category, or none**, with several
deferred until missed. An account may still back several categories (*What the backing increment
covers, and what waits*, below).

## The second distinction: plan and actual

Categories carry **two layers** over the same set of names. Keeping the layers apart is what makes
the rest of this vocabulary consistent, and running them together is the easiest mistake to make
here, because everyday speech uses "budget" for both.

| Layer | What it holds | How it changes |
|---|---|---|
| **The plan** | What each category is *meant* to get this period — its **Budget** | By *assigning* from the pool |
| **The actual** | What has *really* been spent against each category | By recording expenses |

**Income forms a pool.** Every euro of a period's income is *Unassigned* until it is given a
purpose — from the moment the income is **recorded**, which for a future-dated income is before the
money arrives (*Income may be dated in the future*, below).
Assigning €200 to Groceries moves €200 out of *Unassigned* and into the Groceries **Budget**; the
rest of the pool stays *Unassigned*. **Nothing has been spent at this point.** Assigning is
planning, and only planning.

Spending happens on the other layer. An expense is recorded against a category without touching
the plan, and the two layers meet in exactly one derived figure:

> **Remaining** = the category's *Budget* minus everything spent against it

So a *Budget* is a **plan for** a category, never **money sitting in** one. "€400 for groceries in
October" states an intention about €400 that is, physically, still wherever it was. This is why
budgets can carry over as figures without any money moving, and why a category can be spent past
its budget at all: the plan is not a container that can run empty, it is a number the actual is
compared against.

A minority of categories — the *backed* ones, next — do move real money to match the plan. That
changes where the money sits, not what the figure means: the *Budget* is the plan there too.

## Account-backed categories

**Most categories are backed by nothing.** Groceries and Hobby are plans and only plans: assigning
to them moves no money, and the plan and the actual meet only in *Remaining*, exactly as above.

**Some categories are backed**, because their point is that the money really lands somewhere. Such
a category names one or more **backing accounts**, one of which is its **default**. The default is
the account MoneyBud uses whenever money moves on that category's behalf, and it can be overridden
for an individual assignment or expense.

Backing is **many-to-many**, and that is what keeps *the central distinction* intact: Savings may
be backed by two savings accounts, and one savings account may back both Savings and a house
deposit. Backing says where a purpose's money really is; it does not make purpose and location the
same thing.

| | Unbacked category (Groceries, Hobby) | Backed category (Savings, Stocks) |
|---|---|---|
| **Assigning to it** | Pure planning. **No money moves** | Money really moves into its default backing account |
| **Spending against it** | Reduces *Remaining* only | Reduces *Remaining* **and** the backing account's balance |
| **When the period ends** | Its *Leftover* is swept away with the unassigned pool | Keeps its money — nothing to sweep, the money already landed |

**Every expense leaves an account**, including one recorded against an unbacked category. A
transaction always names an account (see *Transaction* below); the *category* simply has no opinion
about which one, and backing is what supplies an opinion — for a backed category the expense comes
out of the default backing account unless the user says otherwise. Where the category has no
opinion, the *pool account* supplies the default instead (*An expense defaults to the pool
account*, below). So **net worth falls when you
buy groceries**, exactly as it does for any other spending. Being unbacked changes what happens when
money is *assigned* to the category — nothing moves — not what happens when it is *spent*.

**This settles what assigning is.** Assigning is planning for an unbacked category and a real
transfer for a backed one, and which of the two happens is a property of the **category**, not of
the act. The round 2 wish — *"de spaarrekening krijgt erbij wat ik daarvoor gebudgetteerd heb"*,
the savings account gains whatever was budgeted for it
([round 2](../stakeholder/2026-09-24-verdieping.md)) — is therefore exactly right, and right
*because Savings is backed*. It was recorded here as an open question, on the grounds that it
seemed to contradict a *Budget* being a plan rather than money that has moved. It does not: for
every category the *Budget* is still the plan and *Remaining* still measures spending against it.
Backing changes where the money sits while that plan is in force, not what the plan means.

Round 2 already had the idea in its own words — *"overblijfsels gaan dus in een budgetpotje, dat op
zijn beurt op een specifieke plek staat"*, a pot that in turn stands in a specific place. Backing
is that *staat op*.

**None of this is in the first increment**, which has no accounts and therefore no backed
categories ([§11](11-risks-and-technical-debt.md)).

**Nor in the accounts increment** (settled and built 2026-09-27). It builds accounts without
backing, and backing is the increment after it (*Accounts and net worth*, below).

**Settled for the backing increment on 2026-09-27, and built the same day** (*Backing and Accumulated*, below).
Everything above stands as the intended model, with three things added or narrowed:

- **One backing account per category, or none.** Several backing accounts, a default among them,
  and choosing the account for one assignment are **deferred until missed**, not rejected. So "its
  default backing account" in the table above is simply its backing account. An expense against it is
  pre-filled with that account and can still be put on another.
- **When the money moves**: on the day of assigning, or on the period's first day if that is later,
  to whatever the category is backed by on that day. A negative assignment moves back what the clip
  lets through.
- **Backing is set, changed and removed by the user at any time**, from the category's row, *Staat
  op*, which is round 2's phrase above. Setting it moves the category's unspent *Remaining* for the
  current period off the pool account, and nothing already in the account. **Removing it returns to
  the pool account what is there for the category in the backing account, and pointing it elsewhere
  takes that along**: what was moved in for it, minus its expenses paid from that account (revised by
  the stakeholder the same day, and made exact in a follow-up; first ruled, money that had moved
  stayed where it went).
- **The pool account may itself back a category**, and archiving does nothing to backing (follow-ups
  the same day).

## The pool account

Backing names where money *goes* when MoneyBud moves it. This names where it comes *from*.

One current account is designated as the **pool account**: the place where *Unassigned* money is
assumed to live. Every movement MoneyBud makes on its own initiative draws from it, unless the user
overrides that particular movement — and an expense that names no account falls back to it as well,
which is a different kind of default and is treated separately below.

| Movement | Source | Destination |
|---|---|---|
| Assigning to a **backed** category | Pool account | The category's default backing account |
| The end-of-period **sweep** | Pool account | The sweep destination's default backing account |
| Assigning to an **unbacked** category | — | — (no money moves, so there is no source) |

**Why.** Two reasons, and the second is the stronger one.

- **It matches reality.** Salary lands in one place. Designating that place lets MoneyBud's
  assumption be right almost every time, and the override is there for the times it is not.
- **Assigning needs no second decision** (quality goal 2,
  [§1.2](01-introduction-and-goals.md)). Without a designated source, every assignment to a backed
  category would ask two questions — how much, and out of which account — where the second has an
  obvious answer nearly always. That is exactly the friction that gets budgeting apps abandoned.

**Designating a pool account does not make *Unassigned* a place.** *Unassigned* stays what *the
central distinction* says it is: a value on the purpose dimension, the absence of a purpose. Cash in
a wallet that has not been earmarked is just as unassigned as the salary in the pool account. The
pool account is the *account MoneyBud assumes when nothing else names one* — when it moves money
itself, and when an expense leaves the account unsaid (next) — and nothing more. Being the pool
account is a fact about one account, not a redefinition of *Unassigned*.

**Not in the first increment**, which has no accounts at all
([§11](11-risks-and-technical-debt.md)).

**Settled for the accounts increment on 2026-09-27, and built the same day** (*Accounts and net worth*,
below). **Any account can be the pool, not only a current account**: MoneyBud has no account kinds,
so "one current account" above describes the usual case, not a rule. There is always exactly one,
and changing it changes the default for new entries only. On screen it is *Hoofdrekening*. In that
increment the pool is the default for every income and expense. Its role as the **source** of
MoneyBud's own movements waits for backing and the sweep.

**Settled for the backing increment on 2026-09-27, and built the same day** (*Backing and Accumulated*, below).
The pool account is the source of every movement made for a backed category: an assignment, on the
day of assigning or the period's first day if that is later, and the move made when a category is
backed, of its unspent *Remaining*. A negative assignment returns money to it. **The per-movement
override above is deferred until missed**, not rejected. **One thing was confirmed**, asked by the
stakeholder himself: the money assigned to an unbacked category is on the pool account, which is what
this section and *The sweep* assumed. Its role as the sweep's source still waits for the sweep.
**Follow-ups the same day:** unbacking a category returns to the pool account what is there for it in
the backing account, which keeps that assumption true after backing ends (*Backing can be set, changed or removed at any time*),
and the pool account may itself back a category, in which case assigning moves no balance (*The pool
account may back a category*), both under *Backing and Accumulated* (below).

## An expense defaults to the pool account

Every expense leaves an account (*Account-backed categories*, above). For a backed category the
account is already known — its default backing account. For an **unbacked** category nothing
supplied one, and that was the last open question in this section. It is now settled:

> **An expense assumes the pool account unless the user names a different one.** The override is
> **per expense**: it applies to that one expense and changes nothing about the next.

| Expense recorded against | Account it is taken out of |
|---|---|
| An **unbacked** category | The **pool account** |
| A **backed** category | The category's **default backing account** |
| Either, with an account named | Whatever account the user named |

**Why.** Expenses are the highest-frequency action in the app — the thing recorded several times a
week, often standing in a shop. Asking for the account every time is a second decision on every
entry, and that is exactly the friction quality goal 2 ([§1.2](01-introduction-and-goals.md))
exists to prevent. The guess is also usually right: money assigned to an unbacked category never
physically moved, so it is still wherever the income landed, which is the pool account by
definition.

**The known weak spot, which is not withdrawn.** The argument against this default was made before
it was taken, and taking the default does not answer it:

- **Cash is an account here too**, and cash spending is precisely the case that will silently take
  the default and be wrong. Nothing about paying in cash looks different at the moment of entry —
  the amount, the date, the category and the label are all as usual — so there is no cue to prompt
  the override.
- **A wrongly-defaulted account writes a balance nothing outside MoneyBud can contradict.** Two
  balances go wrong at once and in opposite directions: the pool account reads low by an amount
  that never left it, and the cash account reads high by an amount that did leave. That is the
  failure the first row of [§11](11-risks-and-technical-debt.md) is about, and this default is a
  new route into it.
- **It is a guess about a fact, not a choice of source.** This is what makes it different from the
  pool account's other role. When MoneyBud assigns or sweeps, it is *deciding* where to take money
  from, and either answer would have been defensible. An expense is a real event the user is
  *reporting*, and the account is something that already happened. A wrong default there is not a
  defensible alternative — it is a wrong record.

So this is a decision with a known weak spot rather than a clean win: it buys a saved decision on
every expense at the price of an occasionally wrong account, most likely for cash, discovered late
or never. It was taken with that price visible. [§11](11-risks-and-technical-debt.md) carries it.

**Not in the first increment**, where an expense has no account at all — the location dimension is
not built yet ([§11](11-risks-and-technical-debt.md)), which is why
[`features/record-expense.feature`](../../features/record-expense.feature) records expenses without
one.

**Settled for the accounts increment on 2026-09-27, and built the same day.** The account is the last field
on the expense form, a list pre-filled with the pool account. With no category backed yet, every
expense is pre-filled with the pool. A row names its account only when it is not the pool account
(*Every income and expense is on an account*, under *Accounts and net worth*, below). The weak spot
above is not withdrawn: that row rule shows a cash expense recorded on the cash account, and does not
show one wrongly left on the pool.

**Settled for the backing increment on 2026-09-27, and built the same day.** A category has at most one backing
account, so the table's "default backing account" is simply **its backing account**. An expense
against a backed category is **pre-filled with its backing account**, and the account list still lets
the user pick another, per expense, as the table says. The table's third row becomes reachable for a
backed category for the first time. **Follow-ups the same day:** the account **follows the category
typed**, the backing account or the pool, until the user picks one himself, and an expense being
changed keeps its account (*What the backing increment covers, and what waits*). An expense against
a backed category put on another account **still lowers *Opgebouwd*** (*Spending against a backed
category from another account*). Both are under *Backing and Accumulated* (below).

## An amount may be assigned negatively, and a *Budget* floors at zero

Assigning moves an amount out of *Unassigned* and into a category's *Budget* (*plan and actual*,
above). It runs **both ways**:

> **Assigning −50 to Groceries moves €50 back out of the Groceries *Budget* and into
> *Unassigned*.**

**So there is no separate act of unassigning.** The rejected alternative was exactly that: a
dedicated "unassign" action standing beside "assign". It was rejected as a second concept doing
what a minus sign already does — two names to learn, two places in the interface and two sets of
scenarios, for one movement whose only variable is direction.

**A *Budget* floors at zero.** A *Budget* is a plan, and a plan for less than nothing is not a
plan. There is no state in which a category is planned to have −€50.

**This is a rule about the plan layer and about nothing else.** It is easy to read a floor at zero
as a general statement that money in MoneyBud does not go negative, and that is not what it says.
Three things are untouched by it:

| | Unaffected |
|---|---|
| ***Remaining*** | Still goes negative freely — that is the state called *Over budget*, and the approved scenarios assert it |
| ***Unassigned*** | Still goes negative when more is assigned than the period's income, which assigning is allowed to do — that state is *Over-assigned* |
| **Transaction amounts** | [ADR 0003](../decisions/0003-money-representation.md)'s positive-magnitude convention is about *Transactions* and their direction. Assigning is not a transaction, so the two rules do not meet |

**An over-large negative assignment is clipped, and the shortfall is reported.** Assigning −50
where the *Budget* is 30 cannot leave the *Budget* at −20, because of the floor. What happens
instead:

> **The *Budget* goes to zero, €30 returns to *Unassigned*, and the user is told that only €30
> could come back.**

The first half follows from conservation: assigning moves money, so nothing can come back out of a
category that never went in, and €30 is all there is to move. That much was written here as a
derivation, stated in full so it could be contradicted if it was wrong about what the stakeholder
wanted. It was not contradicted. The second half — that the shortfall is *said* rather than
silently absorbed — is the part that was genuinely chosen, against two alternatives:

| Rejected | Why |
|---|---|
| **Clip silently** | The lowest-friction option, and the one that leaves the user holding a wrong figure. They asked for 50, got 30, and nothing acknowledged the difference — so anyone working from a number in their head is now wrong and was not told. That is precisely the legibility failure quality goal 1 ([§1.2](01-introduction-and-goals.md)) exists to prevent |
| **Refuse the assignment outright** | It would be the one place MoneyBud blocked something the user plainly meant, and it would hand the user arithmetic MoneyBud could do for them (goal 2) |

**Refusing was weighed against the fact that MoneyBud does refuse things** — a sub-cent amount
([§8.2](08-crosscutting-concepts.md)), a future expense date
([`features/record-expense.feature`](../../features/record-expense.feature)) — and the distinction
that settled it is
worth keeping: those are **bad input**, where this is a **sensible intention that can only be
half-honoured**. The two deserve different treatment. Bad input has no correct reading to act on;
this one has a correct reading, it is just smaller than what was asked for.

So the shape is **honour as much of the intention as conservation allows, and never let the
shortfall go unmentioned** — which is the same shape as *A late expense against a leftover that has
already been directed* below: recalculate what MoneyBud owns, report what it does not. This is an
instance of an existing principle rather than a rule of its own.

**What is fixed here is that a shortfall is reported, not how it is worded.** The wording is copy,
it belongs to the UI, and nothing in this section should be read as specifying a sentence. The UI
increment fixed the Dutch **term** for it, *Niet teruggezet* (*Dutch display terms*, below). The
sentence around that term is still copy.

**Built in the assigning increment.** `Ledger.Assign` moves a negative amount back, floors the
*Budget* at zero, and reports what a clip held back as `AssignResult.Shortfall`, specified by
[`assign-to-category.feature`](../../features/assign-to-category.feature). It replaced
`Ledger.SetBudget`, a scaffold that wrote a plan directly and knew none of this. `SetBudget` has been
deleted ([§8.1](08-crosscutting-concepts.md)).

## Assigning happens in the current budget period and later ones, never in a past one

> **An amount can be assigned in the current budget period or any later one. Assigning in a past
> period is refused.** That holds for a negative amount too: money cannot be pulled back out of a
> past period's plan either.

Settled by the stakeholder on 2026-09-25, before any assigning was built. Built in the assigning
increment, as the refusal `AssignRefusal.PeriodInPast`.

**Why.** Two reasons.

- **It is the display rule's own split.** *When any category is shown in a period* (below) already
  divides periods this way. The current and later periods are the ones you **plan**, and a past
  period is a **record of what happened**. Assigning is planning, so it belongs to the periods that
  are planned. Re-planning a record would stop it being one.
- **It keeps assigning out of the sweep's territory.** A past period's *Unassigned* and *Leftovers*
  are what the end-of-period sweep acts on (*The sweep*, below). A changed plan there would change a
  *Leftover* that has already been swept. That is the same shape as this glossary's one open
  question, *What happens to an income back-dated into a period that has already been swept?*
  (below), and nothing should widen that question before there is a sweep to answer it against.

| Rejected | Why |
|---|---|
| **Any period, since periods never close** | It reads *Ending versus closing a budget period* (below) as covering the plan, and it does not. A period never closes so that **late transactions**, real events entered after the fact, can land where they belong. Re-planning a period after it has ended records no event. It rewrites what the plan was |
| **Move the assignment into the current period** | It would act on a reading the user never gave. They named a past period, and MoneyBud would quietly plan a different one |

**Refused, not half-honoured.** The stakeholder's first answer was "current and future only". That
an assignment in a past period is then **refused** was first written here as a derivation, stated so
that it could be contradicted, and was **confirmed by the stakeholder on 2026-09-25**. The reasoning
is why it stands. It follows the distinction *An amount may be assigned negatively* (above) draws.
An assignment in a past period is **bad input**, with no correct reading to act on, like a
future-dated expense. It is not a sensible intention that can be half-honoured, like an over-large
negative assignment.

**Future periods are allowed without limit.** This matches future-dated income, which counts
towards its own period's *Unassigned* however far ahead that period is (*Income may be dated in the
future*, below). A future period can have a pool to assign from, so it can be planned.

**This fixes a past period's plan, not the period.** A past period still accepts expenses and income
(*Ending versus closing a budget period*, below). What stops changing when a period ends is its
*Budgets*. Its *Remaining* can still move, but only from the actual side, when a late expense lands.

**The accepted cost: a forgotten plan cannot be fixed once its period is over.** Forget to assign to
Groceries in October, and October shows Groceries over budget for good. This was noticed while the
rule was being written up, put to the stakeholder, and **accepted by him on 2026-09-25**. In his
words, *"past is past"*. A past period is a record of what happened, and that includes the plan you
actually had. No plan was made for October's Groceries, so an over-budget figure there is **true**,
not a mistake to be tidied away.

## Assigning zero is accepted and moves nothing

> **Assigning 0 to a category is accepted, and changes no figure.** It does not bring an archived
> category back either (*Only a positive assignment brings it back*, below).

In the stakeholder's words, settled on 2026-09-25: *"Harmless, however it also does not bring an
archived category back."*

**This is deliberately not symmetric with transactions**, and a reader will expect it to be. An
expense must be more than 0 euro
([`record-expense.feature`](../../features/record-expense.feature)), and so must an income
([`record-income.feature`](../../features/record-income.feature)). Assigning follows neither rule.

**Why.** Zero is refused on a transaction because a transaction of nothing is a record of an event
that never happened. The actual layer would gain an entry describing nothing, and that is bad input
with no correct reading. Assigning zero moves nothing and changes no figure, so there is nothing it
could make false and nothing to refuse. It is also not an act of planning for the category, which is
why the reason for bringing an archived category back does not reach it.

| Rejected | Why |
|---|---|
| **Refuse it, like a zero expense** | The symmetry is only in the number. A transaction of zero is refused because it would be a false record; an assignment of zero leaves every figure as it was. Refusing it would block an act that cannot hurt, which is the opposite of *Shown, never enforced* (below) |

## When an assignment is refused

> **An assignment is refused for its target, never for its amount being zero or negative.** The
> refusals are: a name that trims to nothing, a name that is not one of your categories (in use or
> archived), an amount finer than a cent, and a past budget period.

**The amount rules and the target rules are separate.** Zero is accepted and an over-large negative
is clipped (above), but both still need a valid target. So 0 in a past period is refused, and so is
0 to a name you do not have. −500 against a *Budget* of 400 in a past period is **refused, not
clipped**, so no shortfall is reported. In the stakeholder's words: *"zero is accepted, but it would
still be canceled because of the other problems."* This clarifies *Assigning zero is accepted* and
the clipping rule. It changes neither.

**When several rules are broken, the user is told the first of them, in this order:** a name that
trims to nothing, then a name that is not one of your categories, then an amount finer than a cent,
then a past period. **Why:** it is the order recording an expense already uses (category, then
amount, then date), and one order across every kind of entry is one thing to learn. Only which
refusal is **reported** is decided here. Every one of them refuses.

Both settled by the stakeholder on 2026-09-25. Built in the assigning increment, as
`AssignRefusal`, whose members are declared in this order ([§8.1](08-crosscutting-concepts.md)).

## Assigning may overdraw the pool account

Assigning to a **backed** category moves real money out of the pool account. If the pool account
has not got it, **the assignment still goes through.** The account balance goes negative and is
shown as **overdrawn**. Nothing blocks, and nothing warns.

**Why.** Consistency with what MoneyBud already does everywhere else: it shows, it does not
enforce. *Unassigned* is shown and never enforced (below), and spending a category past its
budget is allowed, unwarned, and shown as a negative *Remaining* — which
[`features/record-expense.feature`](../../features/record-expense.feature) asserts scenario by
scenario. A block or a warning here would be the one place the app second-guessed the user, and it
would do so about money the user can see the state of perfectly well.

**The caveat, recorded deliberately.** A negative **account balance** is a harder fact than a
negative *Remaining*, and the two are being given the same treatment anyway:

| | What a negative number there means |
|---|---|
| **Remaining** below zero | A plan overrun. It exists only inside MoneyBud; nothing happened in the world, and the fix is to re-plan |
| **Balance** below zero | A claim about the world: that the account really is in the red, and that the bank let something through or bounced it |

Showing both as a plain negative figure, unremarked, covers two quite different severities with one
display. That is a deliberate choice, not an oversight. Two things argue for it: an overdrawn pool
account in MoneyBud is at least as likely to mean the *balance* is wrong as to mean the *bank* is
overdrawn — see the first row of [§11](11-risks-and-technical-debt.md) — so raising it as an alarm
would cry wolf at a data-entry gap; and an assignment is a planning act, so refusing one because of
a balance would let the location dimension veto the purpose dimension, which *the central
distinction* says it does not get to do. If the display ever does need to tell the two severities
apart, this is the paragraph to revisit.

**Revised for *Remaining*, on 2026-09-25, and not for *Balance*.** The stakeholder revised how an
over-budget *Remaining* is shown: it is still the negative figure, but now with a marker, so it is
no longer "unremarked" (*One marker for over budget and over-assigned*, below). That revision does
**not** reach an overdrawn *Balance*, which has no accounts to be shown on yet. So the "one display
for two severities" above is **open again for the *Balance* side**: an overdraft either gets the same
marker, which keeps this paragraph's decision, or it does not, which reopens it. That is to be
settled when accounts are built.

**Answered on 2026-09-27, and built the same day.** An overdrawn account **gets the same marker**, with a badge
of its own, still never blocked or warned about. So the decision of this section stands: one display
for both severities, now a marked one (*An overdrawn account carries the marker*, under *Accounts
and net worth*, below).

**An expense does exactly the same.** Recording an expense larger than its account holds records
and overdraws like any other: never blocked, never questioned, shown as overdrawn. This was first
written here as a derivation from the assigning case and has since been confirmed by the
stakeholder, so it stands as one rule covering both routes to a negative balance rather than two
rules that happen to agree.

It is worth noting which route is the common one. An over-*assignment* is rare; an expense larger
than its account holds is not, and will usually mean the recorded balance is stale rather than
that the bank bounced anything — MoneyBud cannot tell the two apart, which is the first row of
[§11](11-risks-and-technical-debt.md) again. Recording is still never blocked or questioned
([`features/record-expense.feature`](../../features/record-expense.feature) asserts this for going
over budget); the overdrawn marker is left to carry the meaning on its own.

**Not in the first increment**, which has no accounts and therefore nothing to overdraw.

**The accounts increment** (settled and built 2026-09-27) makes an account overdrawable by an
expense, by a transfer out, and by a negative starting balance or balance correction. **Not by assigning**,
the route this section is named after, because nothing is backed until the increment after.

**The backing increment makes this section's own route reachable** (settled and built 2026-09-27).
Assigning to a backed category moves money out of the pool account on the day of assigning, or on
the period's first day if that is later, and goes through when the pool has not got it. Nothing new
was ruled about overdrawing: this section's decision covers it as it stands. **Two more routes arrive
with it, and neither was put to the stakeholder as an overdraft question.** Backing a category moves
its unspent *Remaining* off the pool account, and that can overdraw it too. And, since the revision
of unbacking the same day, **unbacking and re-pointing move what is there for a category** out of its
backing account, which **may overdraw that account**, ruled in a follow-up: when groceries were paid
from the savings account, the money is really gone (*Backing can be set, changed or removed at any
time*, under *Backing and Accumulated*, below). Both are shown with the marker and never blocked, by
this section's rule.

## Backed categories accumulate

A backed category shows one figure that no other category has: **Accumulated** — what its backing
account has built up on its behalf, shown alongside the per-period *Budget*, what has been spent, and
*Remaining*. After three months of €200 assigned to Savings, Savings shows **€600 accumulated**, even
when this period's *Budget* and *Remaining* are small or zero.

> ***Accumulated* is what has moved in on the category's behalf since it was last backed, minus what
> has been spent against it since.** "Moved in" is net: an assignment moves money in and a negative
> assignment moves it back out. The move made when a category is backed counts as moved in.

**Revised on 2026-09-27**, for the backing increment, as a consequence of the stakeholder's rulings
there (*Re-backing starts Accumulated over*, *Money the user already had stays by location only*,
under *Backing and Accumulated*, below). Built the same day (`Ledger.AccumulatedFor`). What changed,
and what did not:

- **It still nets out spending, for the reason it always did.** Money spent out of a backing account
  has really left it, and *Accumulated*'s whole job is to agree with what is in the accounts. Assign
  €200 three times and spend €50 once, and it reads **€550**, not €600. A running sum of *Budget*
  would describe money that is not there.
- **It is no longer a sum over every period.** It begins when the category is backed. Money already
  in the account then is not part of it, and neither is the category's money in earlier periods,
  which never moved. It **starts over** when the category is backed again after being unbacked:
  unbacking returned the old total to the pool account (as revised the same day; first ruled, it
  stayed in the account as money with no purpose). It does **not** start over when the backing is
  pointed at another account, which takes the money along.
- **For a category backed before anything was assigned to it, and never unbacked, the two
  definitions give the same figure.** Everything ever assigned to it moved in on its behalf, and
  everything ever spent against it was spent since. They part only for a category backed later, or
  unbacked and backed again.
- **It is shown up to and including the period on screen**, so a later period includes what is
  planned for it (*Accumulated covers everything up to the period on screen*, below). On screen it is
  ***Opgebouwd***.

**"Since" was settled in a follow-up the same day**: the expenses dated after the backing day, or on
it and recorded after the backing, the test a balance correction applies. An expense put on another
account counts too. **A negative *Accumulated* carries the one marker, badge *Rood*** (*Backing and
Accumulated*, below).

**How the definition read until 2026-09-27**, kept because it was a decision and what was believed is
part of the record:

> **Accumulated is the running sum of *Remaining* over every period** — everything ever assigned to
> the category minus everything ever spent against it. It is the sum of *Remaining* and **not** of
> *Budget*: assign €200 three times and spend €50 once, and *Accumulated* reads **€550**.
>
> It nets out spending because it has to: money spent out of a backing account has really left it,
> and *Accumulated*'s whole job is to agree with what is in the accounts. A running sum of *Budget*
> would read €600 and describe money that is not there.

That was first written here as a **derivation**, stated in full so that it could be contradicted if
it was wrong about what the stakeholder wanted. It was not contradicted: the stakeholder confirmed
it, so it stood as a decision rather than an inference waiting to be checked. Its netting-out stands.
Its "over every period" was written before anyone asked what happens to money that was not moved by
MoneyBud, or to a category whose backing changes, and the backing rulings answered both.

**Why it exists.** Backed categories are never swept (*Nothing crosses a period boundary without a
purpose*, below), so their money genuinely piles up in their accounts, while *Budget* and
*Remaining* are per-period figures that reset at every boundary. Without *Accumulated*, that €600
would exist and have a purpose while **nothing on the purpose side showed it** — and *the central
distinction* promises that net worth and the budget are one set of data seen two ways. A sum that
appears on the location side and nowhere on the purpose side breaks that promise, and with it
quality goal 1 ([§1.2](01-introduction-and-goals.md)).

It is also **the figure a savings goal is measured against**. "Moving out"
([§1.1](01-introduction-and-goals.md)) is a target on the purpose side; *Accumulated* is progress
towards it.

**An unbacked category has no such figure, and that is not an omission.** An unbacked category is
swept empty at every period boundary — its *Leftover* goes to the sweep destination and its next
*Budget* starts at zero. Nothing survives a boundary, so there is nothing to accumulate.
*Accumulated* exists precisely because backed categories are the exception to the sweep.

**It is not the same number as a backing account's balance**, and should not be read as one.
*Accumulated* is a purpose-side figure; a balance is a location-side one, and backing is
many-to-many — an account backing two categories holds the sum of both, plus any money in it that is
merely unassigned. The two are related, not equal. What happens when they disagree is a known risk
([§11](11-risks-and-technical-debt.md)).

**The backing rulings make them differ in ruled ways** (2026-09-27): by whatever was in the account
before it backed the category (*Money the user already had stays by location only*), by spending
against the category put on another account (*Spending against a backed category from another
account*), and by the period on screen, since the balance is always today's (*Accumulated covers
everything up to the period on screen*), all under *Backing and Accumulated* (below). A first version
of the rulings added money left in an account when a category is unbacked or re-pointed. The
stakeholder revised that the same day: the money now returns or goes along.

**Not in the first increment**, which has no accounts and therefore no backed categories. **Nor in
the accounts increment** (settled 2026-09-27): it is the next one. **Settled for the backing
increment on 2026-09-27**, and built the same day.

## A category name is compared case-insensitively and stored as typed, trimmed

> **"boodschappen" finds "Boodschappen".** Every comparison between category names ignores case;
> the name is kept with the capitalisation the user typed.
>
> **"  Hobby  " is stored as "Hobby"**, and is the same category as "hobby". Surrounding whitespace
> is stripped before anything else happens. **A name that trims to nothing is refused** — a
> category must have a name.
>
> **"Vaste  lasten", with a double space, is the same category as "Vaste lasten".** When names are
> compared, any run of inner whitespace counts as one space. The name is still **stored as typed**,
> inner spacing included.

So a comparison is **trim the ends, treat every run of inner whitespace as one space, then compare
ignoring case**. What is stored is **the trimmed text as typed**: its capitalisation and its inner
spacing are kept. The heading of this section first read "stored exactly as typed". The whitespace
half was settled by the stakeholder on 2026-09-25, after the case half had been written up, and
"exactly" stopped being true. The inner-whitespace part was settled later the same day, while the
scenarios were being reviewed.

**Why compare without case.** Nobody means two categories that differ only in case. A capitalisation
slip — "hobby" one evening and "Hobby" the next — would otherwise create a second category and
**split one month's spending silently across two pots**. Nothing would look wrong at the moment of
entry, and the only cue afterwards is that there are two rows where the user remembers one. That is
precisely the legibility failure quality goal 1 ([§1.2](01-introduction-and-goals.md)) exists to
prevent, and it is the kind that gets discovered long after the entries that caused it.

**Why store what was typed.** Nothing is derived from a category name's case, so folding it away
would buy nothing and would show the user a word they did not write. "Boodschappen" is how the word
is spelled; MoneyBud has no business rendering it back in lower case merely because a lower-case
spelling once matched it.

**Why trim, and why that is not the same as folding case.** Surrounding whitespace is an accident of
typing or pasting, and on a category name it is a worse accident than on a label, because a name is
**compared**: "Hobby " with a trailing space would otherwise be a second Hobby, which is the same
silently split pot the case rule exists to prevent, reached by a keystroke nobody can see. Unlike
capitalisation, nobody *meant* the space, so there is nothing to keep — it is discarded, not merely
ignored for the comparison.

**Why inner whitespace is ignored for comparison, but kept.** "Vaste  lasten" and "Vaste lasten"
look identical on screen. If they were two categories, a stray second space would split one pot's
spending silently across two rows that nobody can tell apart. That is the failure the trimming and
case rules exist to prevent, reached by a keystroke nobody can see. Unlike the edges, though, the
inside of a name is the user's own writing, so the spacing is ignored for the decision and **kept**
for display, the way capitalisation is.

| Rejected | Why |
|---|---|
| **Inner text untouched, so two categories** | The literal consequence of this section as first written ("inner text is left alone"). It was never chosen, it simply followed. It allows exactly the invisible duplicate this section exists to prevent |

**Why a blank name is refused.** A name that trims to nothing is not a name, and a category without
one could not be offered, found or told apart from anything. This is the income label's shape
exactly: **trimming is the rule and refusing blank is its consequence**, because something has to
trim `"   "` in order to judge it blank (*A label is trimmed*, below). It is *not* the expense
label's shape, where a blank result is accepted as "no label" — an expense can do without a label,
and a category cannot do without a name.

**This is the same shape as the label rule**, and the parallel is worth stating because it is what
makes both easy to remember. *A label is trimmed, and that is what makes "blank" mean anything*
(below) and this rule both separate **what MoneyBud normalises in order to decide whether two things
are the same** from **what it keeps in order to show the user**. In both, MoneyBud normalises exactly
as much as the decision needs and does not tidy the user's text beyond that.

They agree about whitespace at the edges and differ about everything that is only compared. The
difference has a reason rather than being an inconsistency: a name is compared, and a label never
is.

| | Whitespace at the edges | Inner whitespace | Case | What is stored |
|---|---|---|---|---|
| **Label** | Normalised **and discarded**. Nobody intended it, so nothing is lost | Not normalised. Nothing is ever compared against a label | Not normalised, for the same reason | The **trimmed** text, inner spacing as typed |
| **Category name** | Normalised **and discarded**, for the same reason | Normalised **for the decision only**: any run counts as one space. Kept for display | Normalised **for the decision only**, and kept for display. Capitalisation *is* intended: it is how the user writes the word | The **trimmed** text **as typed**, capitalisation and inner spacing included |

**What is stored keeps its inner text, under both rules.** `"Vaste  lasten"` keeps its double space
as a category name, exactly as it would as a label. MoneyBud does not tidy the user's prose. This
sentence first read "Inner text is untouched by both rules". That is still true of **what is
stored**, and still true of labels, which nothing compares. It is **no longer true of how category
names are compared**: inner whitespace runs are collapsed for that.

**The rule holds wherever a name is compared, not only when one is added.** Recording an expense
against "  groceries " records it against Groceries. This was first inferred from "every comparison"
above and was then **confirmed by the stakeholder on 2026-09-25**, as a rule in its own right rather
than an inference. When it was written it **changed current behaviour**: the code refused that
expense as naming a category the user does not have. The category increment corrected that (next
paragraph). And an expense whose category name trims to nothing has named no category, so it takes
the refusal for that — a name that trims to nothing is no name, whether it is being added or
recorded against. Both are now asserted by approved scenarios in
[`features/record-expense.feature`](../../features/record-expense.feature). Surrounding spaces and
different case are covered, and so is *A category name of nothing but spaces names no category*.

**This was a correction, not a new rule, and it has been made.** When this section was written,
category names were case-**sensitive** in code: `Ledger` keyed its categories with
`StringComparer.Ordinal`. Nothing chose that. It arrived with the first increment's scaffold, where
no approved scenario ever spelled one category two ways, so the choice was never visible enough to
be made. The whitespace half was a correction of the same kind: `Ledger.AddCategory` neither
trimmed a name nor refused a blank one, because nothing had ever asked it to. The rule was recorded
here as decided rather than inherited, and [§8.1](08-crosscutting-concepts.md) carried the
disagreement until the category increment was built. **It is built, and the code now applies the
whole rule** — trim, collapse inner whitespace, ignore case — in one comparer. §8.1 describes how,
and keeps the record of what it replaced.

### Adding a name you already have gives back the category you already have

> **Not a refusal, and not silent.** MoneyBud hands back the existing category and says that it was
> already there.

**Why not a refusal.** The end state the user asked for — "I want a Boodschappen category" — is
**already true**. There is nothing to refuse them, and refusing would be the one place MoneyBud
blocked an act whose outcome it agrees with (*Shown, never enforced*, below).

**Why not silent.** A user who types a name and sees nothing happen does not know whether it worked.
Under case-insensitive matching they may also be looking at a capitalisation they did not type, which
is a second small thing to be confused by. Saying so costs one sentence and removes both — quality
goal 1 again.

**The existing spelling is kept.** Adding "boodschappen" when "Boodschappen" exists hands back
"Boodschappen", spelled as it was; the new capitalisation is not taken. Spacing works the same way:
adding "Vaste  lasten" when "Vaste lasten" exists hands back "Vaste lasten", and says it was already
there. Settled by the stakeholder on
2026-09-25, and it holds the same way when the existing category is **archived** (below) and when it
is brought back by recording an expense against it (also below). **Why:** you get the category that
already had the name, as it was. Taking the new capitalisation would change a category's name as a
side-effect of adding one — a **rename by the back door**, and renaming is deliberately not in this
increment (*Renaming a category is not in this increment*, below).

**Renaming has a front door since 2026-09-26** (*Renaming a category*, below), and changing a name's
spelling is exactly what it allows. This rule is unchanged: adding still never renames.

**Derived, and not contradicted.** This was worked out from legibility plus *shows, never blocks*
rather than from anything the interviews say in so many words, written up in full so that it could be
contradicted, and put to the stakeholder. It was not contradicted. That puts it on the same footing
as *Backed categories accumulate* (above): a derivation that now stands, with the reasoning kept
because the reasoning is why it stands.

**The archived case is genuinely different, and it is settled separately below.** This rule's whole
argument is that the end state is **already true**; for an archived category it is not — the user
asked for a category they can record against, and there is not one. So the reasoning here does not
reach that case, and it gets its own answer in *Adding an archived category's name brings it back*
(below). The two rules land in the same place — the user ends up holding the category that already
had the name, **spelled as it already was**, and is told — but they get there by different
arguments, and they say different things to the user: *already there* in one case, **brought back**
in the other.

### Renaming a category is not in this increment

**Superseded on 2026-09-26** by *Renaming a category* (next), which answers both questions named
here. It is left as written, because what was believed is part of the record.

**Deferred, not rejected.** Renaming has questions of its own that nothing here answers: whether past
periods show the old name or the new one, and what happens when the new name is one the user already
has — the rule above answers that for *adding*, where handing back the existing category is an
available move, and it is not available to someone who is already holding a category.

It is cheap to add later and nothing settled here forecloses it.

### Renaming a category

> **A category can be renamed.** The new name follows the rules for adding one: it is **trimmed** at
> the ends, stored otherwise **as typed**, and a name that trims to nothing is **refused**.
>
> **A name another category already has is refused**, and the user is told the name is taken.
> "Another category" includes archived ones. Names are compared as always: trimmed, any run of inner
> whitespace counted as one space, case ignored, ordinally.
>
> **Changing only the spelling of a category's own name is allowed.** "boodschappen" can become
> "Boodschappen".

Settled by the stakeholder on 2026-09-26, with the reasoning the documentation's (*An entry can be
changed or removed*, below, says how these rulings were taken). It answers both questions that
*Renaming a category is not in this increment* (above) named.

**Why a taken name is refused.** Chosen over merging the two categories. A merge would silently
rewrite both histories: two categories' expenses and budgets would become one, in every period.

**Why the own name, respelled, is allowed.** Changing the capitalisation or spacing of a name is a
legitimate use of renaming. It is exactly what *The existing spelling is kept* (above) kept out of
**adding**, where it called it "a rename by the back door". That argument was never that a spelling
should not change. It was that a spelling should not change as a **side effect** of adding. Renaming
is the front door. Under the name rule the new spelling is the same name, so no other category can
hold it, and the clash rule never reaches it.

> **Past periods show the new name everywhere.**

Chosen over keeping the old name in old periods. **Why:** it is one category with a new label, not a
new category. It is simple, and nothing has to remember old names.

> **An archived category can be renamed, and it stays archived.**

**Why:** a name is a label, and fixing a typo in it is not using the category again. Bringing a
category back is a side-effect of new entry (*Only a positive assignment brings it back*, below),
and renaming is not new entry. The clash rule applies as normal.

**It stays the same category.** Its history, its place in "order added" (*The order of categories
and slices*, below) and whether it is archived are untouched. First the documentation's reading,
then **approved at the scenario gate on 2026-09-26** (*Approved at the scenario gate, 2026-09-26*,
below).

**Its old name is free once it is renamed.** Adding the old name afterwards creates a new, empty
category. Nothing remembers old names, so nothing holds the old one. **Approved at the scenario gate
on 2026-09-26.**

**A successful rename is announced afterwards**, for example *Categorie hernoemd*. The wording is
copy. Settled 2026-09-26 (*Changes and renames are announced*, below).

**On screen**, a rename button on the category row turns the name into a text box. That is a
default the stakeholder accepted. So a category can be renamed wherever it has a row, and an
archived one therefore only in a period where it has history and is shown.

| Rejected | Why |
|---|---|
| **Merge the two categories** | It would silently rewrite both histories |
| **The old name in old periods** | MoneyBud would have to remember every name a category has had, and one category would read differently depending on the period on screen |

**Built in the corrections increment**, 2026-09-26: `Ledger.RenameCategory`, specified by
[`rename-a-category.feature`](../../features/rename-a-category.feature). "Past periods show the new
name" needs no code of its own. A category is one object that everything holds, so a rename is one
assignment ([§8.1](08-crosscutting-concepts.md)). What the screen does around a rename that the
rulings above do not say is in *Chosen in the build, not put to the stakeholder* (below).

## A category is taken out of use, not deleted

> **Removing a category takes it out of new entry. Its history stays.** It is no longer offered when
> recording. Its expenses, its budgets and its place in every period's figures all remain exactly
> as they were, and **every budget period in which it has history still shows it — including the
> current one** (*Where an archived category is still shown*, below).

**Deleting exists since 2026-09-26, for a category with no history, and it does not contradict
this.** This section is about a category that **has** history, and for that case it stands:
archiving is the only way to take it out of use, because deleting it would rewrite past periods. A
category with no history in any period can now also be deleted, as a separate act (*Deleting a
category that has no history anywhere*, below).

The wish is round 1's, and it names both ways out of a category you do not use:
*"Ook moet een categorie op nul kunnen staan, voor als het niet voor jou geldt. Of kun je hem er
gewoon uithalen als hij niet voor jou geldt"*
([round 1](../stakeholder/2026-09-24-interview.md)) — it can sit at zero, or you can simply take it
out.

**Why this answer.** It has two properties together, and neither rejected alternative has both:
**it never destroys a record, and it never blocks.**

It is also worth noticing what the interview's actual case is. It is a **default category that does
not apply to you** — Verzekeringen, for someone with no insurance to pay. Such a category has **no
history at all**. So the hard case, a category with two years of spending behind it, is one the wish
does not even reach, and this answer costs nothing in the easy case while losing nothing in the hard
one.

| Rejected | Why |
|---|---|
| **Refuse to remove a category that has history** | It would be the one place MoneyBud blocks something the user plainly meant. Everywhere else it shows and does not enforce — *Over budget*, *Over-assigned* and *Overdrawn* are all allowed, unwarned (*Shown, never enforced*, below). The block would also fall hardest on the user who has used the app longest, which is the wrong way round |
| **Ask the user where to move its expenses** | It demands a decision at exactly the moment the user wanted something **gone**, which is the friction quality goal 2 ([§1.2](01-introduction-and-goals.md)) exists to prevent. And there may be **no honest destination**: money spent on a hobby was spent on a hobby, and filing it under Groceries would make the history wrong in order to make a list shorter |

### The name for that state

"Removed" cannot be the word. It invites the reading that the data is gone, and the whole point is
that it is not. The state needs a word that says **put away, not thrown away**.

> **The name is *Archived*.**

| Considered | Why not |
|---|---|
| **Removed**, **Deleted** | They say the record is gone, which is the one thing that is not true |
| **Closed** | Collides head-on with *Ending versus closing a budget period* (below), where this glossary already fixes *closed* to mean "nothing may be recorded against it any more" and then says **MoneyBud has no such state**. Reusing the word for a state that does exist would make the sharpest distinction in this document unreadable |
| **Hidden** | Describes a display rather than a state — and gets the display wrong, because an archived category is still shown in every period where it has history |
| **Retired** | The runner-up, and a good fit for "no longer in service, past service stands". Rejected because this glossary already uses *retired* for a **term** that was withdrawn (*One figure, not two*, below), so the word would carry two meanings in one document |

*Archived* was put to the stakeholder as a recommendation with the table above as its argument, and
**he took it.** The reason worth keeping is the *Closed* row: a casual reader reaches for "closed"
first, and this glossary has already spent that word on something MoneyBud deliberately has not
got.

### What the state fixes

- The category is **not offered for new entry**: not when recording an expense, and not when
  assigning. Not being offered is not the same as being refused. An expense recorded against its
  name is recorded, and an amount assigned to it is assigned. Recording an expense brings it back,
  and so does assigning a **positive** amount; a negative or zero assignment is carried out and
  leaves it archived (*Recording an expense against an archived category brings it back* and
  *Assigning to an archived category brings it back*, below).
- Its last figure is **not offered back** when a new period opens (*An archived category's figure
  is not offered back when a period opens*, below).
- It **still owns its expenses**. Nothing is reassigned, nothing is orphaned.
- It **still appears in every budget period where it has history** — a budget of more than zero
  or an expense in that period — with the budgets and figures it had there. **That includes the
  current period.** A period in which it has no history does not show it (*Where an archived
  category is still shown*, below), with one exception since backing (ruled 2026-09-27): a backed
  one is also shown in the current period and later ones while its *Accumulated* there is not zero.
- Archiving is **never confirmed first**, and the user is **told afterwards** that the category was
  archived (next).
- The state is about **new entry only**. It says nothing about money.
- It is **not permanent**: adding its name again brings the category back, and so does recording
  an expense against it or assigning a positive amount to it — all three below.

Archiving applies to a category **you have and that is in use**. *Archived* is a yes-or-no state of
an existing category, so archiving a name you do not have, or archiving a category that is already
archived, is not something the user can do, and **no user-facing behaviour is defined for either**.

### Archiving is announced, never confirmed

> **Archiving never asks for confirmation. Afterwards, the user is told the category was archived.**

**Why no confirmation.** A confirmation protects against losing something, and archiving loses
nothing. It destroys no record, and adding the name undoes it. Asking first would be MoneyBud
second-guessing an act that cannot hurt, which is the opposite of *shows, never blocks* (*Shown,
never enforced*, below) and adds friction for no protection (quality goal 2,
[§1.2](01-introduction-and-goals.md)). Settled by the stakeholder on 2026-09-25.

| Rejected | Why |
|---|---|
| **Confirm only when the category has history** | Still a question guarding against a loss that does not happen. History survives archiving, so a category with two years behind it has exactly as little to lose as one with none |
| **Confirm always** | The same, and it charges the interview's own easy case too: an unused default that does not apply to you |

**Why tell the user afterwards.** Every category act tells its outcome: **created**, **already
there**, **brought back**, and now **archived**. An act that silently did something would be the one
exception, and the user would have to look to find out whether it worked (quality goal 1). Also
settled on 2026-09-25.

**The list grew on 2026-09-26.** A successful **rename** and a successful **change** to an entry are
announced afterwards too (*Changes and renames are announced*, below), and so is a **deletion**
(*Deleting a category that has no history anywhere*, below).

**Being told is not being warned.** The message is information after the fact, never a question
first. This is what separates it from the rejected confirmations: those ask before, and this only
reports after. Like the other outcomes, what is fixed is **that** the user is told, not the wording.

**The principle is "confirm only where a record is lost", and since 2026-09-26 it gives a second
answer.** Removing an entry destroys a record, so it asks first (*Removing an entry asks first*,
below). Deleting a category with no history loses nothing, so, like archiving, it does not
(*Deleting a category that has no history anywhere*, below).

### Where an archived category is still shown

> **An archived category is shown in every budget period where it has history — a budget of more
> than zero or an expense in that period — including the current one.** An archived category with **no** history
> in a period does not appear in that period.

Archiving takes a category out of **new entry**. It does not take it out of any period's figures.
So a category archived halfway through the current period, while it has a budget or expenses in
that period, is still shown in the current period. An unused default that was archived — the
interview's own case — has no history anywhere, and appears nowhere.

**A budget of 0 with nothing spent is not history.** An archived category whose only trace in a
period is a zero budget does not appear in that period. This follows from *Budget* (terms table):
a category with no budget set behaves exactly as one budgeted at zero. So a zero budget with nothing
spent is indistinguishable from no budget, and it would be odd for an invisible difference to decide
whether a row appears. Derived from that rule, then **confirmed by the stakeholder on 2026-09-25**.
So "history" in a period means **a budget of more than zero, or at least one expense**.

**Why.** A period's figures have to add up on screen. If an archived category vanished from the
current period, the expenses recorded against it earlier in that period would still exist and still
count, but nothing would show them, and the period's spending would stop adding up to what the user
knows they spent. That is the legibility failure quality goal 1 ([§1.2](01-introduction-and-goals.md))
exists to prevent. Settled by the stakeholder on 2026-09-25.

| Rejected | Why |
|---|---|
| **Past periods only** | This section used to say only that past periods still show an archived category, which reads as this rule. From the moment of archiving the category would vanish from the current period. Its expenses there would still exist and would go unshown, which is the failure described above |

**"Past periods still show it" was never the whole rule.** Earlier wording in this glossary said only
that. The rule is about history, not about whether a period is past: a past period in which the
category has no history does not show it either.

> ***Ruled on 2026-09-27, for backing:* an archived backed category is also shown in the current
> period and every later one while its *Accumulated* in that period is not zero**, even with no
> history there. Past periods are unchanged.

**Why:** money still there for the category is never hidden, and it can be unbacked in the period it
is in. Keeping the rule as it was, with the money showing only by stepping back or in a balance, was
rejected (*Backing: ruled after the build*, ruling 3, under *Backing and Accumulated*, below).

> **On the Overview, an archived category that is still shown carries a *Gearchiveerd* caption, and
> has no archive button.**

Built this way in the UI increment. The stakeholder saw it after the spec review and said to keep it
(2026-09-25). **Why**, in the documentation's reasoning: a row that looks like every other row would
let the user believe a category is in use when it is put away, and then wonder why it is not
suggested. The caption says why. There is no archive button because archiving an archived category
is not something the user can do (*What the state fixes*, above). The caption is the display term
for *Archived* (*Dutch display terms*, below), so no new word was needed.

### When any category is shown in a period: the full rule

The rule above covers archived categories. It left one case open, which the category increment met
and did not need to answer: whether a category **in use** is shown in a period where it has **no**
history. The stakeholder settled it on 2026-09-25, and with it the whole display rule:

> **A category is shown in period P if it has history in P** (a budget of more than zero, or an
> expense) **or if it is in use and P is the current period or a later one.**

| Period | A category in use with no history there | An archived category with no history there |
|---|---|---|
| **Past** | Not shown | Not shown |
| **Current or future** | **Shown** | Not shown, **unless it is backed and its *Accumulated* there is not zero** (ruled 2026-09-27) |

A category with history in a period is shown there in every case.

> ***Extended on 2026-09-27, for backing:* a category is also shown in period P if it is archived,
> backed, P is the current period or a later one, and its *Accumulated* in P is not zero.**

**Why:** the planning reason does not reach an archived category, but its money does. A backed
category's *Accumulated* is money really set aside for it, and a row that could not be seen would
hide that money and the *Staat op* list that returns it (*Where an archived category is still
shown*, above; *Backing: ruled after the build*, ruling 3, under *Backing and Accumulated*, below).
A past period stays a record of what happened, so it is unchanged. **Built** as one more clause in
`Ledger.CategoriesShownIn`.

**Why the two halves differ.** The current and future periods are the ones you **plan**. Every
category you could assign to has to be there to be assigned to, whether or not anything has
happened to it yet. A past period is a **record of what happened**, so it shows only what has
history there, which is the same rule already settled for an archived category. The planning reason
does not reach an archived category, because an archived category is not offered for assigning
(*Assigning to an archived category brings it back*, below). Assigning a positive amount to its name
anyway brings it back, and then it is a category in use.

| Rejected | Why |
|---|---|
| **Every period, always** | It fills every past period with rows of zeros, including periods from before the category existed. A record of what happened would then list things that did not happen, which costs legibility (goal 1) |
| **Only with history, always** | In the period you are planning, a category would not appear until you had already assigned to it, so you could not see it in order to assign to it |

### Adding an archived category's name brings it back

> **Adding a name that an archived category already carries brings that category back — history and
> all — and MoneyBud says it was brought back rather than created.** It comes back **spelled as it
> was**: adding "hobby" brings back "Hobby", not a "hobby" (*The existing spelling is kept*, above).

*Adding a name you already have gives back the category you already have* (above) does not reach
this case, because its argument is that the end state is already true and here it is not. This rule
is decided on its own grounds.

**Why.** It is **the interview's own case run backwards**: you took out a default that did not apply
to you, and now it does. One action instead of two, nothing blocked, and no second category sharing
a name.

**Being told is not a nicety here — it is the part that makes the rule safe.** Old expenses
reappearing under a category the user believes they have just created is a genuine surprise, and the
only thing that removes it is a message saying the category was **brought back**. This is where
*Adding a name you already have* and this rule differ in substance rather than in wording: there,
telling the user confirms an end state that was already true; here, it explains an outcome they did
not ask for and could not have predicted.

| Rejected | Why |
|---|---|
| **Ask whether to bring it back** | Explicit, and it charges a decision for an action whose intent is obvious. That is the friction quality goal 2 ([§1.2](01-introduction-and-goals.md)) exists to prevent |
| **Create a separate category with the same name** | It would give two categories one name, undoing *A category name is compared case-insensitively* (above) outright — the same failure that rule exists to prevent, reached by another route |

**So an archived category can be brought back, and adding its name is how. There is no separate
un-archive act.** That is the same shape as *An amount may be assigned negatively, and a Budget
floors at zero* (above), where a minus sign does the work rather than a dedicated "unassign"
standing beside "assign". The model has a standing preference for **one gesture over a second named
concept**, and the reason it gives there carries here unchanged: a second act would be two names to
learn, two places in the interface and two sets of scenarios, for something an act the user already
has can express on its own.

**Adding its name is no longer the only way back.** When this paragraph was written it was; since
2026-09-25 recording an expense against the category brings it back too (next). There is still no
separate un-archive act, but the argument above now has to carry two routes instead of one, and the
next section says how far it still does. Assigning a positive amount became a third route later the
same day (*Assigning to an archived category brings it back*, below).

### Recording an expense against an archived category brings it back

A budget period **ends but never closes** (below), so an expense can be remembered weeks late — a
receipt found in a coat pocket. If its category has been archived in the meantime, that category is
not among the ones offered.

> **An expense recorded against an archived category's name is recorded, and the category is
> brought back** — history and all, spelled as it was, exactly as when its name is added — **and
> MoneyBud says it was brought back.** From then on the category is in use again like any other.

The late receipt is the case that raised it, but **the rule does not depend on the expense's date**:
an expense dated today against an archived category's name does exactly the same.

**A name that is neither one of your categories nor an archived one is still refused**, unchanged
([`features/record-expense.feature`](../../features/record-expense.feature), *An expense must name a
category I actually have*). This rule is about archived names only.

**This is a decision, and it replaces a derivation that was wrong.** This subsection first read *"A
late expense against an archived category — derived, not separately decided"*, and argued that the
case needed no rule of its own: recording against the category "means adding its name, which brings
it back". **That route does not exist.** Recording an expense never adds a category — an expense
naming a category you do not have is *refused*, not created, and an approved scenario says so. The
derivation had quietly assumed that naming a category while recording is the same act as adding
one, and it is not. So the case was put to the stakeholder as a real question, with three answers:

| Option | What happens | Verdict |
|---|---|---|
| **(a) Refuse, then add** | The expense is refused, the user is told the category is archived, adds its name to bring it back, and records again | **Rejected.** This was the recommendation put to him, on the ground that it keeps adding the name as the only way back. Against it: it charges two actions for one obvious intent, and the late receipt is exactly where that friction costs: entering a forgotten expense is already a chore, and a refusal turns it into a detour (quality goal 2, [§1.2](01-introduction-and-goals.md)) |
| **(b) Record, and bring back** | The expense is recorded, the category is brought back, and the user is told it was brought back | **Taken** |
| **(c) Record, and leave archived** | The expense is recorded against the category, which stays archived | **Rejected** — an archived category silently collecting new expenses contradicts what *Archived* means: taken out of **new entry**. And the user would not know the category was being used again, so its spending would accumulate under a category they believe is put away — the legibility failure goal 1 exists to prevent |

**What this costs, stated plainly.** There are now **two routes back**: adding the name, and recording
an expense against it. That softens the *one gesture over a second named concept* argument in the
section above, which was made when there was one. **The reconciliation is that there is still no
separate *un-archive* act.** Both routes are acts the user already has, and bringing back is a
side-effect of each — never a thing done on its own, and **always announced**. What the preference
was protecting against was a second *concept* to learn; a second *occasion* on which the same thing
happens is a smaller cost, and it was accepted for the friction it saves.

**Being told matters more here than on the add route.** Adding a name at least shows the intent to
have the category; recording an expense does not, so the message is the only thing that tells the
user a category they put away is back in their list.

**A refused expense brings nothing back.** Bringing back is a side-effect of recording. If the
expense is refused for any other reason — its amount is not more than zero, is finer than a cent, or
its date is in the future — nothing is recorded, and **the category stays archived**. This was first
written here as a **derivation**, stated so that it could be contradicted, and was **confirmed by
the stakeholder on 2026-09-25**. That puts it on the same footing as *Backed categories accumulate*
(above): a derivation that now stands, with the reasoning kept because it is why it stands.

So the archiving rule and the never-closes rule do **not** pull against each other, which is what
they appeared to do before this was decided — and now that is true by decision rather than by an
inference that turned out to rest on a route that was never there.

### Assigning to an archived category brings it back

> **An archived category is not offered when assigning. Assigning a positive amount to its name
> anyway brings it back** — history and all, spelled as it was — **and the user is told**, exactly as
> when an expense is recorded against it. A negative or zero assignment does not bring it back
> (*Only a positive assignment brings it back*, below).

Settled by the stakeholder on 2026-09-25, before any assigning was built. The rule first read
"assigning to its name anyway brings it back", without regard to sign; the sign was settled later
the same day.

**Why.** It is **one rule for every kind of new entry.** *Archived* means taken out of new entry,
and naming an archived category is how you bring it back. Recording an expense and assigning are
both new entry, so they behave the same way. A negative or zero assignment is not new entry for the
category in that sense, which is why it does not count (*Only a positive assignment brings it
back*, below). This makes a positive assignment a **third route back**, after adding the name and recording an expense. There is still **no separate un-archive act**: all three
are acts the user already has, and bringing back is an announced side-effect of each. The
reconciliation in the section above carries a third route as well as a second.

| Rejected | Why |
|---|---|
| **Not offered, and refused** | A detour for an obvious intent. This is the reason option (a) lost for expenses, and it applies unchanged |
| **Offered** | Archiving would then hide a category from expense entry only. That contradicts what *Archived* means, which is out of **new entry**, not out of one kind of it |

#### Only a positive assignment brings it back

> **A negative assignment to an archived category does not bring it back, and neither does
> assigning zero. Only a positive assignment does.**

Take Hobby, archived while €60 is still budgeted for it in the current period. Archiving returned
nothing to *Unassigned* — archiving says nothing about money, and
[`archive-category.feature`](../../features/archive-category.feature) says so — and Hobby is still
shown in the current period, because a budget of more than zero is history. Assigning −60 to Hobby
moves the €60 back into *Unassigned*, and **Hobby stays archived**. The clip still applies: −80
against that €60 moves €60, reports the €20 that could not come back, and Hobby stays archived.

**Why.** Bringing back exists because naming an archived category **for new entry** is a sign that
you want it again — that is the reason the three routes back share. Pulling its money out is the
opposite sign. It is **tidying up** after putting the category away, not planning for it. Assigning
zero is neither: it plans nothing, so the reason for bringing back does not reach it
(*Assigning zero is accepted and moves nothing*, above). Settled by the stakeholder on 2026-09-25.

| Rejected | Why |
|---|---|
| **One rule regardless of sign** | Simpler to state, and it would make the only way to reclaim an archived category's leftover budget a bring-back followed by a second archive: two acts, the second undoing a side-effect of the first, to finish tidying a category already put away. That is the friction quality goal 2 ([§1.2](01-introduction-and-goals.md)) exists to prevent |

**What happens next, derived rather than asked.** Once the money is out, an archived category with
nothing spent in that period has no history there — a zero budget with nothing spent is not history
(*Where an archived category is still shown*, above) — so it stops being shown in that period. That
completes the tidying up. If something was spent, it has history and stays shown, like any archived
category with history. This follows from the display rule and was not put to the stakeholder
separately.

**Built in the assigning increment**, together with the rule above it. `Ledger.Assign` brings an
archived category back only for a **positive** amount, and only once every check has passed, so a
refused assignment leaves it archived, like a refused expense. `AssignResult.CategoryBroughtBack`
reports it. Specified by [`assign-to-category.feature`](../../features/assign-to-category.feature).
Until then, the scaffold `Ledger.SetBudget` brought nothing back whatever the sign. That was a
recorded gap against this rule, and it closed when `SetBudget` was deleted
([§8.1](08-crosscutting-concepts.md)).

### An archived category's figure is not offered back when a period opens

> **When a new budget period opens, an archived category's last figure is not offered back.** If
> the category is brought back later, it is assigned to like any other category.

*Budgets carry over as figures, not as assignments* (below) offers each category's previous figure
back at the start of a period. For an archived category it does not. **Why:** you put the category
away, so MoneyBud does not suggest planning for it again. Settled by the stakeholder on 2026-09-25.

| Rejected | Why |
|---|---|
| **Offer it back** | It would plan money for a category the user archived, while leaving the category archived |

**Detailed on 2026-09-26, and built** in the opening-a-period increment (*Opening a period*, below).
An archived category's figure is not offered, and it does not make an earlier period count as having
a plan to offer either. `Ledger.PlanOfferedIn` reads "archived" at the moment it is asked, so
archiving a category and bringing it back change the offer at once
([§8.1](08-crosscutting-concepts.md)).

### Deleting a category that has no history anywhere

> **A category with no history in any budget period — no expense, and no budget of more than zero —
> can be deleted.** It is gone: not archived, and not brought back by anything. Adding its name
> afterwards creates a new category.
>
> **Deleting is a separate act from archiving.** Archiving stays exactly as it is. A separate
> delete button appears **only** on a category with no history anywhere.
>
> **Deleting is not confirmed, and the user is told afterwards** that the category was deleted.

Settled by the stakeholder on 2026-09-26, with the reasoning the documentation's (*An entry can be
changed or removed*, below, says how these rulings were taken).

**He chose to have it at all, with the argument against it in front of him.** The recommendation
put to him was to leave deleting out. Archiving a category with no history already makes it vanish
everywhere (*Where an archived category is still shown*, above), so the only visible difference
seemed to be what adding its name again says: *brought back* rather than *created*. **He kept it in
anyway.** No reason came with that choice at first.

> **His reason, confirmed by him as his own on 2026-09-26:** *so that it is gone for good and its
> name is free again, instead of being kept out of sight in the archive.*

It rests on a settled rule: **an archived category keeps its name.** Renaming another category onto
that name is refused, because archived names count (*Renaming a category*, above), and adding the
name brings the old category back rather than making a new one (*Adding an archived category's name
brings it back*, above). Only deleting frees the name. **So the argument put to him undercounted the
difference.** It named only what adding the name again says. It missed that an archived category
goes on holding its name against every other category, which was the point he cared about.

**A second visible difference, noticed while this was written up and not part of the argument put
to him.** A deleted category added again is a new category, so it goes last in "order added". An
archived category brought back keeps its original place (*The order of categories and slices*,
below). The difference shows only among categories with equal budgets. It was then **approved at the
scenario gate on 2026-09-26** (*Approved at the scenario gate, 2026-09-26*, below).

**Why "no history anywhere".** It is this glossary's existing definition of history, a budget of
more than zero or an expense (*Where an archived category is still shown*, above), applied to every
period. It was chosen over a stricter "never touched" rule, under which a category ever assigned to,
even if the amount was taken back to zero, could not be deleted. That rule was rejected because this
glossary says "assigned, then taken back to zero" is **not a separate state** (*Budget*, terms
table; *"A Budget of zero" means zero*, below). **So this rule must not be decided by
`Ledger.HasBudget`**, the one query that can tell the two apart
([§8.1](08-crosscutting-concepts.md)).

**"No expense" means no expense now.** A removed entry leaves no trace (*A change overwrites the
entry*, below), so MoneyBud cannot know that a category once had an expense. A category whose only
expense was removed, or changed to another category, has no history and can be deleted. It gives
the same answer for expenses that the rejected "never touched" rule was refused over for budgets.
First the documentation's derivation, then **approved at the scenario gate on 2026-09-26**.

**Why two buttons.** Chosen over one remove button with MoneyBud picking delete or archive. **Why:**
two distinct acts the user can see, and nothing decided for the user.

**Why it is not confirmed.** The same reasoning as archiving (*Archiving is announced, never
confirmed*, above). Nothing of value is lost, because the category has no history, and adding its
name recreates it. Chosen over asking first, as removing an entry does. **The contrast is
deliberate.** Removing an entry is confirmed because a record is lost, and one principle, confirm
only where a record is lost, gives both answers (*Removing an entry asks first*, below).

**Why it does not contradict *A category is taken out of use, not deleted*** (above). That section
is about a category with history, where deleting would rewrite past periods, and it still decides
that case. A category with no history has no past to rewrite, and deleting it destroys no record.

**An archived category with no history appears nowhere**, so it has no row, and its delete button
cannot be reached. It stays archived, and adding its name brings it back.

| Rejected | Why |
|---|---|
| **"Never touched": nothing ever assigned, even if taken back to zero** | It would let a difference this glossary says does not exist decide what the user can do |
| **One remove button, with MoneyBud picking delete or archive** | It decides for the user |
| **Ask first, like removing an entry** | A confirmation guards against a loss, and nothing is lost |

**Built in the corrections increment**, 2026-09-26: `Ledger.CanDelete` and `Ledger.DeleteCategory`,
specified by [`delete-a-category.feature`](../../features/delete-a-category.feature). `CanDelete`
reads the figures directly, expenses and budgets of more than zero, and **not** `HasBudget`, as
this section requires. A category's budgets of zero are not history, so they go with it when it is
deleted ([§8.1](08-crosscutting-concepts.md)). On screen the act is *Verwijderen*, the same word as
removing an entry (*Dutch display terms*, below), and its button shows only on the row of a
category that `CanDelete`.

> ***Ruled on 2026-09-27, for backing:* a category also cannot be deleted while money moved on its
> behalf between two different accounts still stands.** *Archiveren* is still offered. Money moved
> from the pool account to itself does not count, and goes with the category when it is deleted.

It **extends** what "history" means, for backing: a movement between two accounts is a row in both
accounts' histories, naming the category, as an expense is a row in a period's list. **It does not
reverse the rejection of "never touched"** above. A budget assigned and taken back to zero is still
no history. What blocks is a movement row that stands in a history now. Erasing those rows with the
category was rejected, because balances could shift after the fact where a balance correction lies
in between. The reasoning, and the first build's broader rule that `spec-reviewer` found, are in
*Backing: ruled after the build*, ruling 2, under *Backing and Accumulated* (below).

### What archiving does not settle, because it cannot yet

Archiving says nothing about money, and for most categories there is no money to say anything about.
Two cases would be different, and **neither is reachable**: a **backed** category's money really sits
in an account, and the **sweep destination** must itself be backed (above). With no accounts there
are no backed categories and no sweep at all ([§11](11-risks-and-technical-debt.md)), so there is
nothing for either question to be true of. Recorded so that a later reader does not take the silence
for an answer.

**The first became reachable with the backing increment** (settled and built 2026-09-27): a backed
category's money really sits in its account. **Ruled in a follow-up the same day: archiving does
nothing to backing.** The category stays backed, keeps and shows *Opgebouwd*, and its later budgets
still move; unbacking is a separate act (*Archiving does nothing to backing*, under *Backing and
Accumulated*, below). The second waits for the sweep.

## The default categories

> **Boodschappen, Huur, Hobby, Sparen, Verzekeringen, Abonnementen.**

Round 1 asks for them: *"Er moeten standaardcategorieën zijn, want sommige dingen is normaal dat ze
er zijn — bijvoorbeeld boodschappen of hobby"*, while noting in the same breath that *"natuurlijk kan
het per persoon verschillen wat ze nodig hebben"*
([round 1](../stakeholder/2026-09-24-interview.md)).

**This is the stakeholder's own list, and it is a starting set chosen to be tried** — in his words,
enough to get an idea and test. It is **not** a claim about the right categories for a household, and
nothing should be built that treats it as one. Others get added later, which is what *adding is easy*
and *taking one out* (above) are for.

### They are Dutch because they are content. A test name's language decides nothing

| | What it is | Language |
|---|---|---|
| Category names in the feature files — "Groceries", "Hobby", "Gifts" in [`record-expense.feature`](../../features/record-expense.feature), "Boodschappen", "Vaste lasten" in [`add-category.feature`](../../features/add-category.feature) | **Synthetic test data** — names invented to make a scenario readable, standing in for whatever the user really has | **Whichever suits the scenario.** English is common; Dutch is fine |
| Boodschappen, Huur, Hobby, Sparen, Verzekeringen, Abonnementen | **User-facing content** — the strings MoneyBud actually puts on the user's screen | Dutch, because the user is Dutch |

**What makes a name test data is not its language.** It is test data because it is synthetic and
because no scenario leans on the default list being there. This table first said test names were
"English, with the rest of the specification". `add-category.feature` then used Dutch names, and the
stakeholder settled on 2026-09-25 that §12 was too strict, not the feature file. So a
"Boodschappen" set up by an explicit *Given* is test data that happens to share a default's name,
exactly as "Hobby" always did.

Several test names — Groceries, Hobby, Subscriptions, Boodschappen — coincide with defaults or
their English, which makes the distinction easy to miss. It holds anyway, because **the approved
scenarios depend on the default set only where the default set is the subject.** Before the
category increment none of them used it. Now exactly three do: *A new MoneyBud starts with the six
default categories* in [`add-category.feature`](../../features/add-category.feature), and two in
[`archive-category.feature`](../../features/archive-category.feature) about archiving an unused
default and bringing one back. Every other scenario names the categories it needs with an explicit
*Given*. That keeps them readable and lets the default list change without touching them.

**That independence is enforced, not hoped for.** Every scenario starts from an **empty** ledger. Only
the "first time" steps start from the defaults (`Ledger.StartNew`, the app's door for a first start,
where the constructor gives an empty ledger), and they must come before any other setup. A scenario
that quietly leaned on Boodschappen being there would fail ([§8.1](08-crosscutting-concepts.md)).

[§2](02-architecture-constraints.md) requires the documentation and the specification to be English.
That constrains what MoneyBud's authors write **about** the app; it says nothing about what the app
**displays** to its one Dutch user.

**This is a different thing from *Dutch source terms* (below), and confusing the two would be easy.**
That table maps the stakeholder's Dutch **vocabulary** onto this project's English **terms**, so that
reading the interviews alongside this documentation does not introduce drift — "potje" is a word he
says, not a word MoneyBud shows anyone. These six names are **not vocabulary**. They are content:
strings the app ships with, which the user sets to zero or archives like any other category. Nothing
translates them, because there is nothing to keep in step.

**A third kind of Dutch arrived with the UI**: the terms MoneyBud displays, such as *Uitgave* for
*Expense* and *Niet toegewezen* for *Unassigned*. They are neither vocabulary nor content, and they
have a table of their own (*Dutch display terms*, below), which says how the three differ.

### Sparen ships unbacked, and that is temporary

A savings pot is exactly what *Account-backed categories* (above) is for: its whole point is that the
money really lands somewhere. Accounts do not exist ([§11](11-risks-and-technical-debt.md)), so there
can be no backed category, and **Sparen therefore ships as a plan and only a plan** — assigning to it
moves no money, and it is not exempt from the end-of-period sweep the way a backed category is,
though no sweep runs today either. It becomes backed when the location dimension arrives.

Recorded so that a later reader does not take it for an oversight. It is the one default category
whose eventual behaviour differs from the behaviour it has now.

**Refined on 2026-09-27: not when accounts arrive, but when backing does.** The accounts increment
builds accounts without backing, so *Sparen* stays unbacked through it, and becomes backed in the
increment after (*What this increment covers, and what waits*, under *Accounts and net worth*,
below). A first start has no savings account for it to be backed by. The user adds one.

**Revised on 2026-09-27, for the backing increment: it is no longer temporary in the sense that
MoneyBud will back it. The user does.** A first start still leaves *Sparen* unbacked once backing is
built, because a first start has only *Betaalrekening*. The user adds a savings account and sets
*Sparen*'s *Staat op* to it (*A first start leaves Sparen unbacked*, under *Backing and
Accumulated*, below). It stays the one default category whose purpose is to be backed. What makes it
different from the others is now the user's act, not a later version of MoneyBud.

## Terms

| Term | Definition |
|---|---|
| **Account** | A place where money actually sits. Current account, savings account, investment account, or cash. Answers *where*. Cash is modelled as an account despite not being a bank account. May **back** one or more categories — see below. **Settled for the accounts increment on 2026-09-27, and built the same day** (*Accounts and net worth*, above): an account is **a name and what is on it**. The kinds above are examples, not a type, and nothing behaves differently by kind (*derived*). It is added with a name and, if the user types one, a *Starting balance* (follow-up, 2026-09-27: left empty, the account has none), can be renamed, and can be deleted only while **unused**: no income, expense or transfer on it (*derived*). Deleting one is **never confirmed** and is announced afterwards, even with a starting balance (follow-up, 2026-09-27). Its name follows the category name rules and is unique among accounts, and it may share a name with a category (*derived*). **Adding a name another account has is refused**, not handed back as a category's would be, so a starting balance just typed is never dropped (follow-up, 2026-09-27). Accounts are listed **pool account first, then in the order added**, in the strip and in the forms (follow-up, 2026-09-27). Archiving an account is deferred until missed. A first start has one, *Betaalrekening*, as the *Pool account*, **with no starting balance**, so its balance is the sum of what is on it until first corrected. Backing waits for the increment after. On screen *Rekening*, renamed with *Hernoemen* and deleted with *Verwijderen* (approved 2026-09-27; into the display-terms table at the build). **Backing settled on 2026-09-27, and built the same day** (*Backing and Accumulated*, above): an account may back a category, chosen in the category's *Staat op* list (several categories, in the documentation's reading), and **an account that backs a category, or has a *Movement* on it, counts as used**, so it cannot be deleted. Money MoneyBud moves into or out of it on a category's behalf shows in its history, one row per movement. Money already in it when it starts backing a category keeps no purpose. |
| **Location** | The dimension answered by "which account". Not a separate entity — a way of grouping. |
| **Category** | What money is earmarked for: groceries, hobby, moving out. Answers *what for*. A category is a label and exists independently of any amount assigned to it. Its **name** is **trimmed** at the ends. It is compared **case-insensitively**, with any run of inner whitespace counting as one space. It is stored trimmed, with its capitalisation and inner spacing as typed. So there are never two categories that differ only in case or spacing. A name that trims to nothing is **refused**. Adding a name that already exists hands back the category that already has it, **spelled as it already was**, with the user told so (see *A category name is compared case-insensitively* above). A category with history is taken out of use by **archiving**, never by deleting: its history stays, and adding its name again, recording an expense against it or assigning a positive amount to it brings it back (*A category is taken out of use, not deleted*, above). A category with **no history in any period** can instead be **deleted** (*Deleting a category that has no history anywhere*, above). It can be **renamed**, under the same name rules, to any name no other category has (*Renaming a category*, above). Deleting and renaming were settled on 2026-09-26 and built in the corrections increment. MoneyBud ships with six **default categories** (above). |
| **Archived** | The state of a category that has been taken out of use. It is **no longer offered for new entry**, whether recording an expense or assigning, and its last figure is not offered back when a period opens. Everything it already owns stays: its expenses, its budgets, and its place in those periods' figures. It is **shown in every budget period where it has history** — a budget of more than zero or an expense in that period — **including the current one**, and not in a period where it has none, so a zero budget alone does not count (*Where an archived category is still shown*, above). Archiving is **never confirmed first**, and the user is **told afterwards** that the category was archived (*Archiving is announced, never confirmed*, above). Archiving destroys no record, which is why the state is not called *removed*, and it is **not permanent**. It is **brought back**, history and all and spelled as it was, by any of three acts the user already has: **adding its name** again, **recording an expense against it** (which records the expense rather than refusing it), or **assigning a positive amount to it**. Each way, the user is told it was brought back. A **negative or zero** assignment does **not** bring it back: pulling an archived category's money out is tidying up, not planning for it (*Only a positive assignment brings it back*, above). There is no separate act of un-archiving, for the same reason there is no separate act of unassigning; bringing back is a side-effect of those acts, always announced. Only a category in use can be archived. An archived category can be **renamed**, and stays archived (*Renaming a category*, above). On the Overview, an archived category that is shown carries a *Gearchiveerd* caption and has no archive button (*Where an archived category is still shown*, above). Distinct from a period being **closed** — a state MoneyBud deliberately has not got (*Ending versus closing a budget period*, below). See *A category is taken out of use, not deleted* above. Built in the category increment: `Ledger.ArchiveCategory`, specified by [`archive-category.feature`](../../features/archive-category.feature) and, for bringing back by recording, [`record-expense.feature`](../../features/record-expense.feature) ([§8.1](08-crosscutting-concepts.md)). Bringing back by assigning was built in the assigning increment, specified by [`assign-to-category.feature`](../../features/assign-to-category.feature). **Archiving does nothing to backing** (follow-up, 2026-09-27, built the same day): an archived backed category stays backed, keeps and shows *Opgebouwd*, and its later budgets still move; unbacking it is a separate act (*Archiving does nothing to backing*, above). **An archived backed category is also shown in the current period and every later one while its *Accumulated* there is not zero**, even with no history there, so money still there for it is never hidden (ruled after the build, 2026-09-27; *Backing: ruled after the build*, above). |
| **Rename** | Giving a category a new name. The new name follows the rules for adding one: trimmed, stored otherwise as typed, and refused if it trims to nothing. A name **another** category has, archived ones included, is **refused**, and the user is told it is taken. The category's **own** name in a new spelling is allowed, which is the front door *The existing spelling is kept* kept adding from being. **Past periods show the new name.** An archived category can be renamed and **stays archived**. See *Renaming a category* above. Settled 2026-09-26; built in the corrections increment, as `Ledger.RenameCategory`. |
| **Delete** | Said of a **category** only: removing one that has **no history in any period**, meaning no expense and no budget of more than zero. It is gone, not archived, and adding its name again creates a new category. A separate act from archiving, with its own button, shown only on such a category. **Never confirmed**, and announced afterwards. A category with history cannot be deleted; it is archived. Decided by figures, never by `Ledger.HasBudget`. See *Deleting a category that has no history anywhere* above. Settled 2026-09-26; built in the corrections increment, as `Ledger.CanDelete` and `Ledger.DeleteCategory`. **Since backing** (ruled 2026-09-27), **a category also cannot be deleted while a *Movement* between two different accounts stands for it**, since those rows name it in both histories; *Archiveren* is still offered. A movement from the pool account to itself does not count and goes with the category. This extends "history" for backing and does not reverse the rejection of "never touched" (*Backing: ruled after the build*, above). |
| **Default categories** | The six categories MoneyBud ships with: **Boodschappen, Huur, Hobby, Sparen, Verzekeringen, Abonnementen**. A starting set chosen to be tried, not a claim about what a household needs. Their names are **Dutch** because they are user-facing **content**, unlike the names in the feature files, which are synthetic test data in whichever language suits the scenario — and unlike *Dutch source terms* below, which is vocabulary rather than content. *Sparen* ships **unbacked** and becomes an *account-backed category* when accounts exist. See *The default categories* above. Refined 2026-09-27: when **backing** exists, the increment after accounts (*Sparen ships unbacked, and that is temporary*, above). **Revised the same day, for the backing increment:** a first start leaves *Sparen* unbacked even once backing is built. It has only *Betaalrekening*, so the user adds a savings account and backs *Sparen* himself (*A first start leaves Sparen unbacked*, above). |
| **Purpose** | The dimension answered by "which category". Not a separate entity — a way of grouping. |
| **Account-backed category** | A category that names one or more accounts its money really sits in — Savings, Stocks. Most categories are not backed. The relationship is **many-to-many**: a category may be backed by several accounts, and an account may back several categories. Backing changes what assigning, spending and the end of a period do to the category — see *Account-backed categories* above. Not in the first increment, which has no accounts, and not in the accounts increment either: it is the one after (settled 2026-09-27). **Settled for the backing increment on 2026-09-27, and built the same day:** **one backing account per category, or none**. Several backing accounts, and a default among them, are deferred until missed, not rejected. Backing can be set, re-pointed or removed at any time. Setting it moves the category's unspent *Remaining* in the current period off the pool account, and moves nothing already in the account. **Removing it returns to the pool account what is there for the category in the backing account, and re-pointing takes that along**, on that day: what MoneyBud moved in for it, minus its expenses paid from that account, and nothing if there is none (revised by the stakeholder the same day, made exact in a follow-up). Either may overdraw the account the money leaves (follow-up). **Archiving does nothing to backing** (follow-up). See *Backing and Accumulated* above. |
| **Backing account** | One of the accounts backing a category. A backed category names exactly one of them as its **default backing account**: the one used whenever money moves on that category's behalf, overridable per assignment or per expense. **Settled for the backing increment on 2026-09-27, and built the same day:** a category has **at most one**, so its default backing account is simply its backing account, and overriding it **per assignment is deferred until missed**. Per expense it can still be overridden: an expense against the category is pre-filled with it, and the pre-fill follows the category typed until the user picks an account himself (follow-up). It is chosen in the category row's ***Staat op*** list, and can be pointed elsewhere or removed at any time. **Pointing it elsewhere takes along what is there for the category; removing it returns that to the pool account**: what MoneyBud moved into it for the category, minus the category's expenses paid from it, which can differ from *Opgebouwd* (revised the same day and made exact in a follow-up; first ruled, money that had moved stayed where it went). Either may overdraw it, marked *Rood* (follow-up). Money planned for a later period goes to whatever the category is backed by on that period's first day. **Any account may be one, the pool account included**, in which case assigning moves no balance (follow-up). An account that backs a category counts as used (*Backing and Accumulated*, above). |
| **Pool account** | The one current account designated as where *Unassigned* money is assumed to live. It is the default **source** for every movement MoneyBud makes on its own initiative — assigning to a backed category, and the end-of-period sweep — overridable per movement. It is also the account an **expense against an unbacked category** is assumed to have left, again overridable, which is a guess about a past event rather than a choice of source and is the weaker of its two roles ([§11](11-risks-and-technical-debt.md)). May go *Overdrawn*; nothing blocks that. A fact about one account, not a redefinition of *Unassigned*, which remains a purpose and not a place. Not in the first increment, which has no accounts. **Settled for the accounts increment on 2026-09-27, and built the same day**: **any** account can be made the pool, since there are no account kinds, and there is **always exactly one**. It pre-fills the account field of every new income and expense. Making another account the pool changes that default for new entries only. The pool cannot be deleted while it is the pool, so there is always at least one account (*derived*). A first start's *Betaalrekening* is the pool. It is **listed first**, in the strip and in the forms' account list, the rest following in the order added (follow-up, 2026-09-27). On screen it is ***Hoofdrekening***, made so by *Maak hoofdrekening* (*The pool account can be any account*, above). Its role as source of MoneyBud's own movements waits for backing and the sweep. **Settled for the backing increment on 2026-09-27, and built the same day:** it is the **source** of every movement made for a backed category, an assignment and the move made when a category is backed, and a negative assignment returns money to it. Overriding the source per movement is deferred until missed. **Confirmed the same day:** the money assigned to an unbacked category is on the pool account (*Backing a category that already has money*, above). **Follow-ups the same day:** unbacking a category returns to the pool account what is there for it in the backing account, the part from earlier periods with no purpose; and the pool account may itself back a category, in which case assigning moves no balance but *Opgebouwd* counts. Money planned for a later period comes out of the pool as it is on the day it moves (*derived*). Its role as the sweep's source still waits for the sweep. |
| **Unassigned** | Two things under one name, deliberately. (a) The **absence of a purpose**: a value on the purpose dimension, not a location — unassigned money still sits in an account. (b) The **figure** that measures it for one budget period: that period's income minus everything assigned to categories in it. It is the pool that assigning draws from and that a negative assignment puts money back into. Starts at the period's full income, because carrying budgets over carries figures and not assignments; reaches zero when the user has finished budgeting the period; goes **negative** past that, which is *Over-assigned*. Shown prominently and assigned from directly, rather than being only a total the user has to work out — and never enforced. Not a category: nothing is budgeted for it and nothing is spent against it. Does not survive the end of a budget period: it is *swept* — see below. An income joins its period's *Unassigned* **when it is recorded**, which for a future-dated income is before its date arrives — so *Unassigned* covers a **whole period** where *Net worth* covers a **point in time**, and the two disagree about expected income by design (*The central distinction*, above). Formerly also called *Left to assign*; that name is retired — see *One figure, not two*. |
| **Assign** | The act of giving money a purpose: moving an amount out of *Unassigned* and into a category's **Budget**. An amount may be assigned **negatively**, which moves it back out of the category and into *Unassigned* — so there is no separate act of unassigning. A negative assignment larger than the category's *Budget* is **clipped** to what is there and the shortfall is **reported** to the user; it is never refused (see *An amount may be assigned negatively* above). For an unbacked category it is a planning act only — it changes what money is *for*, not where it is, and spends nothing. For an *account-backed* category it is also a real transfer, out of the *pool account* and into the category's default backing account, either end of which can be overridden — and which goes through even when the pool account has not got the money, leaving it *Overdrawn*. Possible in the **current budget period and any later one**; assigning in a **past** period is **refused** (*Assigning happens in the current budget period and later ones*, above). **Assigning zero** is accepted and changes nothing, unlike a zero expense or income, which is refused (*Assigning zero is accepted and moves nothing*, above). Refused only for its **target** or its cents, never for being zero or negative. The refusals, in the order the first one broken is reported, are: a name that trims to nothing, a name that is not one of your categories, an amount finer than a cent, and a past period. That is the same order recording an expense uses. An otherwise acceptable zero or clippable negative is still refused if its target is wrong (*When an assignment is refused*, above). An **archived** category is not offered for assigning. Assigning a **positive** amount to its name anyway **brings it back**, and the user is told; a negative or zero assignment leaves it archived (see *Assigning to an archived category brings it back* above). Built in the assigning increment for **unbacked** categories, which is every category while there are no accounts: `Ledger.Assign`, specified by [`assign-to-category.feature`](../../features/assign-to-category.feature) ([§8.1](08-crosscutting-concepts.md)). The backed half, the real transfer, was built in the backing increment. **Settled on 2026-09-27, and built the same day:** for a backed category the amount moves from the pool account to its backing account **on the day of assigning, or on the period's first day if that is later**, to whatever the category is backed by on that day; a negative assignment moves back only what the clip lets through, and never more money than is there for the category in its backing account (follow-up, 2026-09-27); and overriding either end is **deferred until missed**. Taking a plan over follows the same rule (*Assigning to a backed category moves money*, above). Distinct from recording the income that brought the money in, and done whenever the user is ready rather than at the moment money arrives. |
| **Budget** | The **plan** for one category in one budget period: what the user intends that category to have. "€400 for groceries in October" is a budget; "groceries" on its own is a category. A budget is never a container that can run empty — see *plan and actual* above. It **floors at zero**: a plan for less than nothing is not a plan. That is a rule about the plan and not about money in general — *Remaining* still goes negative freely, and that is *Over budget*. For an unbacked category it is also not money that has moved; for a backed one the money really has moved, but the *Budget* is still the plan and *Remaining* still measures spending against it. Budgets **carry over as figures**, offered back rather than applied — see below. Since 2026-09-26 that is settled in detail, and it is built: a current or later period whose every *Budget* is zero is offered the plan of the latest earlier period that has one, and can **take it over** (*Opening a period*, below). A category for which **no budget has been set** behaves exactly as one budgeted at zero: there is no separate "unbudgeted" state, and a missing budget never blocks recording an expense. Assigning changes it only in the current period or a later one, so a **past** period's budgets cannot be re-planned (*Assigning happens in the current budget period and later ones*, above). |
| **Take over (a plan)** | The one act that assigns an earlier period's plan in full. It is **offered** in the current period and every later one while every *Budget* there is zero, a budget taken back to zero included and an archived category's budget counting, and only then. It acts on the **period on screen**, never on a period the assign form has been stepped to. The plan offered is that of the **latest earlier period that has a plan**: a *Budget* of more than zero for a category not archived now, however small, one cent included. Taking it over assigns each of that period's figures, for categories not archived, even past *Unassigned*, which may leave the period *Over-assigned*. It is **not confirmed first**, and a notice names the period it went into. Not offered in a past period: past a boundary it disappears quietly at the next refresh, and pressing it before then is refused like any past-period assignment. No undo: a take-over is corrected row by row by negative assignments. See *Opening a period* above. Settled 2026-09-26, specified by [`take-over-a-plan.feature`](../../features/take-over-a-plan.feature), and built in the opening-a-period increment as `Ledger.TakeOverPlan`, which assigns each figure through `Ledger.Assign`, with the offer worked out by `Ledger.PlanOfferedIn` ([§8.1](08-crosscutting-concepts.md)). On screen it is *Plan overnemen*. |
| **Remembered figure** | A category's *Budget* in the plan being offered: what taking it over would assign to it. Shown in grey on the category's row, labelled *plan* ("plan: € 400,00"), *only while the plan is offered*, and gone once the period has a plan. It is not a comparison between months. A category archived now, or whose figure in that period is zero, is not part of the plan, and its row shows no label. While the plan is offered, the rows are **ordered by this figure**, largest first, ties in order added, so nothing jumps on taking it over. See *Opening a period* above. Settled 2026-09-26, specified by [`take-over-a-plan.feature`](../../features/take-over-a-plan.feature), and built in the opening-a-period increment as a `PlanFigure` in the `PlanOffer` and `CategoryRow.PlanFigure` on screen ([§8.4](08-crosscutting-concepts.md)). |
| **Over-assigned** | The state of a budget period whose *Unassigned* is **negative** — more has been assigned to its categories than the period's income, which assigning is allowed to do. Shown, never blocked and never warned about, exactly like the other two members of its family: *Over budget* (a negative *Remaining*) and *Overdrawn* (a negative *Balance*). A property of a **budget period**, where those two are properties of a category and of an account. Exactly zero *Unassigned* is not over-assigned. Built in the assigning increment as `Ledger.IsOverAssigned`, derived from *Unassigned* and never stored — see *Over-assigned* below. On the Overview's ring an over-assigned period is drawn as its budgets only, and *Unassigned* is shown as the negative figure itself with the same marker as *Over budget*, a revision by the stakeholder on 2026-09-25 (*One marker for over budget and over-assigned*, below). The marker's badge reads *Te veel toegewezen*. Built in the UI increment. |
| **Budget period** | The span a budget covers — normally a month. The day it starts is configurable, so it does not necessarily align with a calendar month. A start day later than a month has — the 31st in February — **clamps to that month's last day**, see *A start day the month is too short for clamps to its last day* below. A budget period **ends**, but it is never **closed** — see below. In the UI increment the start day is **fixed at the 1st** and not offered for change: deferred, not rejected, because the code cannot yet change it once budgets exist (*The period start day stays at the 1st, for now*, above). |
| **Transaction** | A single movement of money, with an amount, a date and an account. Income and expenses are both transactions. **They differ in two ways, and each difference has its own reason rather than being an inconsistency**: whether the transaction names a **category** (an expense must, an income does not — the two rows below), and whether it may be dated in the **future** (an income may, an expense may not — see *Income may be dated in the future; an expense may not*). The amount rules are the same for both: more than zero, never finer than a cent, refused rather than rounded ([§8.2](08-crosscutting-concepts.md)). Once recorded, a transaction can be **changed** or **removed**, in any budget period (*An entry can be changed or removed*, above). Settled 2026-09-26; built in the corrections increment. **Every transaction is on an account** from the accounts increment on (settled and built 2026-09-27). Until then none has one. A *Transfer* is **not** a transaction in this glossary's sense: the word stays for income and expenses, so that the two differences above stay the only two (the documentation's wording, *Transfers*, above). |
| **Income** | A transaction that increases the total. It does **not** name a category: it lands as *Unassigned* and is given a purpose later, by a separate act of assigning. It **must** carry a **Label** — with no category on the record, the label is the only thing that says what the money is (see below). It **may be dated in the future**, unlike an expense; it counts against the budget period its date falls in, including a period still to come, and it joins that period's *Unassigned* **from the moment it is recorded** rather than when its date arrives. May be one-off or recurring, and both permanently — see *Recurring transaction*. In the income increment an income has an amount, a date and a label, and **no account at all** — the same gap an expense has ([§11](11-risks-and-technical-debt.md)). **Settled for the accounts increment on 2026-09-27, and built the same day:** every income is **on an account**, chosen from a list as the form's last field and pre-filled with the *Pool account*. A future-dated income reaches its account's *Balance*, and net worth, **only on its date**, while it still counts in its period's *Unassigned* from the moment it is recorded (*derived*). Its row names its account only when that is not the pool account. |
| **Expense** | A transaction that decreases the total, and it **must** name a category — money being spent is money whose purpose is known by definition. Carries an **optional** **Label** of its own, below. **May not be dated in the future**, unlike an income — money not yet spent is a plan, and the plan layer already has a word for it, the *Budget* (see *Income may be dated in the future; an expense may not*). The account it leaves is **defaulted, not asked for**: the category's default backing account if the category is backed, otherwise the *pool account*, overridable per expense — see *An expense defaults to the pool account* above. May be one-off or recurring. In the first increment an expense has an amount, a date, a label and a category, and no account at all. **Settled for the accounts increment on 2026-09-27, and built the same day:** every expense is **on an account**, chosen from a list as the form's last field and pre-filled with the *Pool account*, since no category is backed yet. Its row names its account only when that is not the pool account. **Settled for backing on 2026-09-27, and built the same day** (follow-up): the account **follows the category typed**, its backing account or the pool account, until the user picks one himself; an expense being changed keeps its account. Against a backed category it lowers *Opgebouwd* whichever account paid (*Backing and Accumulated*, above). |
| **Label** | A transaction's own free-text name, distinct from a category: "Albert Heijn" labels an expense whose category is "Groceries"; "Salaris september" labels an income that has no category at all. It says *which particular movement this was*, where a category says *what kind of spending it counts as*. **Optional on an expense, required on an income** — the asymmetry and its reason are in *Income carries a label, and it is required* below. **Always trimmed**, on both transactions: surrounding whitespace is stripped and the inner text left alone, so a label that trims to nothing is not a label — which an income refuses and an expense simply records as having none. Nothing is derived from it either way, which is why trimming costs nothing. Settled by [§1.1](01-introduction-and-goals.md) ("each labelled and categorised"), [round 1](../stakeholder/2026-09-24-interview.md) ("ik moet duidelijk kunnen aangeven waar het van is") and [round 3](../stakeholder/2026-09-24-verdieping.md) ("met een label erop"). |
| **Change** | Said of an **entry**: correcting an expense or an income after it was recorded. Allowed in **any** budget period, past ones included. A change is judged exactly as the changed entry would be if it were recorded now, so it is refused on the same rules, and a refused change leaves the entry as it was. Changing an expense's category to an archived category's name brings that category back, announced. Fixing an expense that is already on an archived category does **not** bring it back. A change **overwrites** the entry: MoneyBud keeps no record of what it was. A changed date that moves the entry to another period leaves the screen where it was and says where the entry went. See *An entry can be changed or removed* above. Settled 2026-09-26; built in the corrections increment, as `Ledger.ChangeExpense` and `Ledger.ChangeIncome`. |
| **Remove** | Said of an **entry**: taking an expense or an income away entirely. From the accounts increment, only from the Overview's lists, never from an account's history (follow-up, 2026-09-27; built). Allowed in any budget period. It **asks for confirmation first**, the only act that does, because it destroys a record. Removing an income may leave its period *Over-assigned*, which is allowed and shown with the marker. See *Removing an entry asks first* above. **Not said of a category** in this sense. Where *A category is taken out of use, not deleted* speaks of "removing a category", it means round 1's "hem eruit halen", which is **archiving** it. Destroying a category with no history is **deleting** it. Settled 2026-09-26; built in the corrections increment, as `Ledger.RemoveExpense` and `Ledger.RemoveIncome`, with the question asked by the screen before either is called. **From the accounts increment** (settled and built 2026-09-27), a *Transfer* and a *Balance correction* can be removed too, from an account's history, and removing either asks first by the same principle (*derived*). "The only act that does" then means the only **kind** of act: removing a record. Deleting an unused account does **not** ask, even with a starting balance: a follow-up ruling of 2026-09-27 (*Managing accounts*, above). |
| **Recurring transaction** | An income or expense that repeats on a schedule — weekly, monthly, yearly. Not part of the first increment, and not part of the income increment either. When it arrives it stands **beside** one-off entry rather than replacing it: entering an amount by hand, including a future-dated one, stays a first-class act ([§1.1](01-introduction-and-goals.md) lists one-off and recurring together, not one as a stopgap for the other). |
| **Remaining** | For a category in a budget period: its *Budget* minus what has been spent against it. The one figure where the plan and the actual meet. Goes negative when a category is overspent; nothing blocks that. A negative *Remaining* is the state called *Over budget*, next. |
| **Over budget** | The state of a category whose *Remaining* is **negative** — more has been spent against it than was budgeted for it in this period. Shown, never blocked and never warned about: the expense that causes it is recorded like any other. **Exactly zero *Remaining* is not over budget** — spending a category down to nothing is the plan working, not the plan failing — and one cent past zero is. Because a category with no budget set behaves as one budgeted at zero (see *Budget*), such a category is over budget from the first cent spent against it. A property of a category **within one budget period**, so the same category can be over budget in one period and not in the next. On screen *Remaining* is shown as the negative figure itself, **with a marker** it shares with *Over-assigned*. The marker's badge reads *Over budget*. The marker is information, not a warning. This is a revision by the stakeholder on 2026-09-25 (*One marker for over budget and over-assigned*, below). Built in the UI increment. |
| **Accumulated** | **Account-backed categories only.** Everything ever assigned to the category minus everything ever spent against it — the running sum of its *Remaining* across all periods, and so the money its backing accounts have built up on its behalf. Shown beside the period's *Budget* and *Remaining*, which reset at every boundary while *Accumulated* does not. An unbacked category has no such figure, because it is swept empty at every boundary and nothing accumulates. Related to, but not equal to, a backing account's *Balance* — see *Backed categories accumulate* above. Not in the first increment. **Revised on 2026-09-27, and built the same day:** it is **what has moved in on the category's behalf since it was last backed, minus what has been spent against it since**. It is no longer the running sum of *Remaining* over every period, although for a category backed before anything was assigned to it, and never unbacked, the two agree. It starts at €0, or at what moves when the category is backed. Money already in the account is not part of it. **It starts over when a category is backed again after being unbacked**, not when its backing is pointed at another account, and an unbacked category shows none. It is **not** what unbacking and re-pointing move: they move what is there for the category in the backing account, which counts only its expenses paid from that account, where *Accumulated* counts every one, whichever account paid (follow-up). The two differ by the category's expenses paid from other accounts. It is shown **up to and including the period on screen**, so a later period includes what is planned; in periods before the backing it follows today's backing, €0 before anything moved (follow-up). **The expenses that lower it** are those dated after the backing day, or on it and recorded after the backing, **on any account** (follow-ups). **Below zero it carries the one marker, badge *Rood*** (follow-up). An archived backed category keeps and shows it (follow-up). Counts a later period's *Budget* as planned until that period is settled (*Backing: chosen in the build*, above). On screen ***Opgebouwd*** (ruled, *Backing and Accumulated*, above; in *Dutch display terms* since the build). |
| **Leftover** | A category's *Remaining* when its budget period ends — money that was assigned but not spent. For an unbacked category it is *swept* rather than allowed to vanish; a backed category keeps its leftover, because that money is already in its account — see below. A leftover is computed at the end of a period; computing it does not close the period — see below. |
| **Sweep** | What happens at the end of a budget period to money that has not landed anywhere: the *Unassigned* pool and the *Leftovers* of every unbacked category are moved together into one **sweep destination**, out of the *pool account* and into that destination's default backing account. Backed categories are not swept. Automatic, not prompted — see below. |
| **Sweep destination** | The category a sweep moves money into. **Must itself be account-backed**, so that swept money really arrives somewhere. Set once as a default, applied automatically at every period end, shown in the period summary, and redirectable afterwards — see below. |
| **Net worth** | The sum of the balances of all accounts. The "how am I doing" figure, and a **point-in-time** one: it is **what you have today**. Income dated in the future is **not** counted, because it is not money yet — there is nothing in any account for it to be part of. This is where net worth and *Unassigned* part company on purpose: *Unassigned* is a **period** figure and includes an expected income from the moment it is recorded, so the two views disagree about that amount by design and not by error. See *The central distinction* above. **Settled for the accounts increment on 2026-09-27, and built the same day:** shown as ***Vermogen***, the stakeholder's own word, at the end of a strip of accounts across the top of the Overview, the same in every period. A *Starting balance* or *Balance correction* changes it and nothing on the purpose side. A *Transfer* leaves it unchanged **except where a balance correction has already counted one side**, which is true rather than a flaw, since each account follows its own balance corrections and net worth is their sum (follow-up, 2026-09-27; *Transfers*, *Accounts and net worth*, above). **Below zero it carries the one marker**, with the badge *Rood*, like an overdrawn account (follow-up, 2026-09-27). |
| **Balance** | How much is in one account. Changed by the transactions recorded against it, by assignments to any category it backs — which really move money in — by every assignment and every sweep if it is the *pool account*, which move money out, and by the user editing it directly, which round 2 settles is allowed alongside anything MoneyBud calculates. Two mechanisms writing one number is a known risk ([§11](11-risks-and-technical-debt.md)). **Revised on 2026-09-27, and built the same day: a balance is worked out, never stored as a free number.** It is the account's latest *Balance correction* (its typed *Starting balance*, if there is no other), plus or minus every income, expense and transfer on it dated **after** that balance correction's day, or on that day and first recorded after it (*derived*). Entries dated before it are already in it. An account with **no** typed balance at all, such as a first start's *Betaalrekening* until it is first corrected, has as its balance the plain sum of everything on it, whatever the dates (follow-up, 2026-09-27). "Editing it directly" is now **typing the real balance**, which records a *Balance correction* rather than overwriting a number. A future-dated income reaches it only on its date (*derived*). See *A balance is worked out from the entries* and *A typed balance is what the bank said that day* (*Accounts and net worth*, above). Assignments and sweeps move no balance until backing and the sweep are built. On screen *Saldo* (approved 2026-09-27). **Settled for backing on 2026-09-27, and built the same day**, so assignments now move balances, and a balance adds up *Movement*s as it adds up transfers; sweeps still move none: an assignment to a backed category lowers the pool account's balance and raises the backing account's **on the day the money moves**, the day of assigning or the period's first day if that is later; so does the move made when a category is backed, on that day. Those movements meet balance corrections like any entry. **When backing is removed, what is there for the category moves from the backing account to the pool account; when it is pointed elsewhere, from the old account to the new one**, on that day: what MoneyBud moved in for it, minus its expenses paid from that account (revised the same day and made exact in a follow-up; first ruled, money already moved stayed where it went). Either may overdraw the account it leaves, marked *Rood*, because the money was spent on something else (follow-up) (*Backing and Accumulated*, above). |
| **Starting balance** | The balance an account is added with, **typed by the user**: what the bank says **today**. It is the account's **first *Balance correction***, with everything that follows from that (*derived*): dated the day it is typed, may be negative or zero, net worth only, removable but not changeable. **A first start's *Betaalrekening* has none** (follow-up, 2026-09-27): only a balance the user actually types takes in earlier entries, so until he first corrects it, its balance is the plain sum of what is on it (*A first start has one account*, above). **Nor does an account added with the starting balance left empty**, which behaves the same way; **a typed 0 is a starting balance** and takes in earlier entries (follow-up, 2026-09-27). Its row in the account's history shows **no difference**, "Startsaldo — € 1.000,00", because there is nothing it corrected (follow-up, 2026-09-27). Settled 2026-09-27, and built the same day. On screen *Startsaldo* (approved 2026-09-27). |
| **Balance correction** | A balance the user types for an account, recorded as **what that account really held on that day**: dated today (*derived*), and taking in every entry dated before it, so an expense remembered late does not knock it off. It changes the account's *Balance* and net worth, and **nothing on the purpose side**: it is not income, counts in no *Unassigned*, and changes no budget figure. May be **negative or zero** (*derived*). Can be **removed**, which asks first, but **not changed**: to change one, correct again (*derived*). Listed in the account's history **with the new balance and the difference** (follow-up, 2026-09-27), the only trace left of something forgotten. The difference is **recomputed, not fixed**: it is what is still unexplained, the typed balance minus what the previous balance correction and the entries it takes in would give now, so it shrinks to €0,00 as forgotten entries are recorded, while the balance itself does not move (follow-up, 2026-09-27). Distinct from a *Change* to an entry, which corrects a record rather than a balance. **Why two words.** This glossary already says "correction" for changing or removing an entry: *the corrections increment*, *Corrections and the sweep*, and the entries in *Answered* about them, all left as written. A one-word term would have collided with that older use in every sentence that reached both, so the term was renamed from *Correction* to *Balance correction* on 2026-09-27, before anything was specified or built with it. The Dutch keeps one word, *Correctie*, because on screen entries are changed through *Wijzigen* and the word never appears beside them. Settled 2026-09-27, and built the same day. On screen *Correctie*, and the act *Saldo corrigeren* (approved 2026-09-27). |
| **Transfer** | Money moved by the user **from one account to another**, on a date: an ATM withdrawal from Betaalrekening to Contant. It moves both balances and changes **neither net worth nor any budget figure**, except that net worth **may** change where a *Balance correction* dated after the transfer has already counted one side; that is true, not a flaw (follow-up, 2026-09-27). Not an income or an expense, and not a *Transaction* in this glossary's sense. **May not be dated in the future**, like an expense. Needs two different accounts and an amount above zero, never finer than a cent (*derived*). Changed or removed from an account's history, like an entry, removing asking first (*derived*). That history is the only place a transfer is changed; an income or expense, by contrast, is changed only from the Overview's lists (follow-up, 2026-09-27). It carries **no label**, and a transfer breaking several rules reports the first of: two different accounts, more than 0, whole cents, not in the future (approved at the scenario gate, 2026-09-27). The same kind of movement MoneyBud will make itself once backing and the sweep exist, in the stakeholder's own framing. Settled 2026-09-27, and built the same day (*Transfers*, *Accounts and net worth*, above). On screen *Overboeking*, the act *Overboeken*, with *Van* and *Naar* (approved 2026-09-27). Since the backing increment MoneyBud makes its own counterpart, a *Movement* (next). |
| **Movement** | Money **MoneyBud** moves on a category's behalf, from one account to another: the user's own counterpart is a *Transfer*. Made by assigning to a backed category, by backing, unbacking and re-pointing, and by money planned for a later period moving on that period's first day. It moves two balances and no budget figure, and it is neither income nor an expense, so it is in neither of the Overview's lists. It shows as a row in **both accounts' histories**, read-only: it is changed by assigning again, never from the history. **A movement from an account to itself**, when the pool account backs the category, changes no balance and has no row, but *Opgebouwd* counts it. Written on the day the money moves and never changed, so the amounts fixed on their day stay fixed. Money for a later period is written by **settling**: the first time MoneyBud runs on or after that period's first day, before anything else is done, with the backing and the pool account of that moment. An account with a movement between two different accounts on it counts as used and cannot be deleted; movements from it to itself do not count. A category cannot be deleted while a movement between two different accounts stands for it; one from the pool account to itself does not block, and goes with the category (ruled 2026-09-27). The feature files say *movement* for a history row of this kind. Settled and built 2026-09-27 (*Moved money in the account's history*, *Backing and Accumulated*, above; [ADR 0009](../decisions/0009-movements-are-entries.md)). No display term: the row's words are copy. |
| **Overdrawn** | The state of an *account* whose *Balance* is **negative**. Reachable by assigning more than the *pool account* holds, which MoneyBud allows without blocking or warning — see *Assigning may overdraw the pool account* above. Distinct from *Over budget*, which is a negative *Remaining*: that is a plan overrun inside MoneyBud, this is a claim about the world. Not in the first increment, which has no accounts. **Settled for the accounts increment on 2026-09-27, and built the same day:** shown with **the same marker** as *Over budget* and *Over-assigned*, with its own badge, ***Rood*** (ruled in a follow-up the same day). Never blocked or warned about. In that increment it is reached by an expense, a transfer out, or a negative starting balance or balance correction, and not by assigning, which moves no money until backing. **Since the backing increment** (built 2026-09-27) it is also reached by assigning to a backed category, by backing a category, and by unbacking or re-pointing one, each of which may overdraw the account the money leaves. |
| **Overview** | The screen MoneyBud opens on, displayed as *Overzicht*. It shows one budget period at a time, starting at the current one and stepping back and forward. It is headed by the **Ring** and lists the categories the display rule shows for that period (*When any category is shown in a period: the full rule*). It is laid out income left, plan middle, expenses right. Built in the UI increment, as `PeriodOverview` in the presentation layer (*The user interface*, above; [§8.4](08-crosscutting-concepts.md)). |
| **Ring** | The radial diagram at the head of the Overview. One **slice** per category with a *Budget* above zero, sized to that *Budget* and filled in as far as it has been spent, so the unfilled part is its *Remaining*. *Unassigned*, when above zero, is a slice of its own, so the whole ring is the period's income. An overspent slice stays budget-sized, completely filled and marked. A category with spending and no budget gets no slice and is listed with the marker instead. An *Over-assigned* period's ring shows its budgets only. A period with neither income nor any *Budget* shows an **empty ring**, a grey outline with a hint. The full rules are in *The overview, and its ring*, above. Built in the UI increment, as `Ring`. **Revised at the first demo, 2026-09-26, and built:** every slice, *Unassigned* included, is drawn at least **2% of the ring**, so the ring is no longer drawn exactly in proportion, although the slices' figures still add up to the income. The fill stays exact. Pointing at a slice shows its figures in the ring's hole, which otherwise shows *Unassigned*, and the ring is the middle column's centrepiece (*Every slice has a minimum width*, *Hovering a slice shows its figures*, *The Overview's layout*, above). |

## A start day the month is too short for clamps to its last day

*Budget period* above says the day a period starts on is configurable. February has no 31st, so a
configured start day cannot always be honoured literally:

> **A start day later than the month has clamps to that month's last day.** Configure the 31st and
> February's period starts on the **28th** — the 29th in a leap year.

**Why.**

- **"Configurable" stays true for every day of the month**, instead of true with an exception. The
  rejected alternative below buys clarity by making the 29th, 30th and 31st unchoosable, which is a
  rule the user has to discover about a setting that otherwise has none.
- **It is what billing cycles conventionally do.** A user who has ever had a card statement or a
  subscription dated at the end of the month has met this behaviour already, so the app is not
  inventing a convention of its own.
- **Periods still tile.** Every day belongs to exactly one budget period, none to two and none to
  none. That is the property that stops an expense counting twice or vanishing at a boundary, and
  it is what makes any answer here safe *except* one that leaves a gap.

**The accepted cost, stated plainly.** Configure the 31st and the period containing 15 March 2026
runs **28 February to 30 March**. It starts on a day the user did not pick, and it is **31 days
long** against 28 for the period before it. This happens twice a year, and **nothing on screen
explains why** — a period is silently longer than its neighbours, which is a direct cost to
legibility, quality goal 1 ([§1.2](01-introduction-and-goals.md)).

**The rejected alternative: restrict the configurable start day to 1–28**, so a month too short for
it can never occur. This was the recommendation put to the stakeholder, on the ground that the
unexplained 31-day period costs more than the three lost choices do, and **it was not taken.** The
stakeholder chose clamping with that cost in front of him. The argument for restricting does not
evaporate by being declined: if the odd period length ever confuses anyone in practice, this is the
paragraph to reopen and restricting the range is the answer already on the table.

A third option was noticed and is weaker than either: anchoring to one chosen start *date* and
counting whole months from it. It produces the same dates and only moves the question, which
returns as "what is one month after 31 January".

This behaviour was in the code as a **placeholder** before it was a decision. It is now a decision,
pinned by `BudgetPeriodCalendar`'s doc comment and by developer tests that assert the clamp for
start days 30 and 31 including a leap year, alongside the tiling property for every start day from
1 to 31.

**No approved scenario configures a start day.**
[`features/record-expense.feature`](../../features/record-expense.feature) is written in terms of
"the previous", "the current" and "the next budget period", so nothing already approved depended on
which way this went, and nothing has to change now that it has gone this way.

## Ending versus closing a budget period

These are two different things, and the difference matters enough that the words are not
interchangeable.

| | Meaning |
|---|---|
| A period **ends** | Its span is over. Time has moved on, the *Remaining* figure for each category is final in the ordinary case, and a *Leftover* can be computed |
| A period **closes** | Nothing may be recorded against it any more. **MoneyBud has no such state** |

**A budget period is never closed.** A past period always accepts new expenses, however long ago
it ended.

**Why.** Everything is entered by hand ([§3.1](03-context-and-scope.md)), so expenses are
routinely remembered late — a receipt found in a coat pocket, a payment noticed on a statement
weeks afterwards. If a period could close, the user's only options would be to lose the expense or
to record it against the wrong period, and both corrupt exactly the history the app exists to
show. Correcting history has to stay possible, so there is no point in the lifecycle at which a
period stops accepting entries.

The consequence is that any figure derived from a past period — *Remaining*, and *Leftover* above
all — has to be understood as the current best answer rather than a permanently fixed one. What
that means when a leftover has already been acted on is set out next.

**Assigning is refused in a past period, and that does not close it.** Closing is about what may be
*recorded*, and a past period goes on accepting expenses and income. What a past period stops
accepting is changes to its **plan** (*Assigning happens in the current budget period and later
ones, never in a past one*, above). A late transaction is a real event arriving late. Re-planning
after the fact is not an event at all.

## A late expense against a leftover that has already been directed

Because a period never closes, an expense can land in a period whose *Leftover* was computed and
already swept into the destination category's account — into savings, say. Because the sweep is
automatic (see below), this is the ordinary case rather than a rare one. When it happens:

1. The period's *Leftover* is **recomputed**. It is a derived figure, so it simply becomes smaller.
2. The user is **shown the discrepancy**: more was moved out of the period than the period turned
   out to have left.
3. The user decides what to do about the transfer. **MoneyBud does not adjust it.**

**Why.** The transfer is a record of real money that really moved between accounts; rewriting it
silently would make the history disagree with the bank. But a discrepancy that nobody is told about
is worse — it would leave two figures quietly contradicting each other, which is exactly the failure
that quality goal 1 (legibility, [§1.2](01-introduction-and-goals.md)) exists to prevent. So the
app recalculates what it owns, reports what it does not, and leaves the correction to the person who
knows whether the money really went back.

**A correction is this same case when it raises a swept period's spending**: an expense's amount
raised, or an expense moved into the period by a changed date. A correction that lowers it is not.
It joins the open question at the end of this glossary (*Corrections and the sweep*, below).

## Budgets carry over as figures, not as assignments

**The figures are remembered; the money is not assigned.**

When a budget period opens, each category's amount from the previous period is remembered and
**offered back** — but nothing has been assigned yet. An **archived** category's figure is not
offered back (*An archived category's figure is not offered back when a period opens*, above). The pool starts whole: the period's income is
entirely *Unassigned*, every category's *Budget* is zero until the user acts, and the *Unassigned*
figure starts at the full income. **One action assigns last period's plan in full**, after which individual
figures can be adjusted like any other.

**Why.** This is the shape that makes both of the things we want true at once.

- **A period genuinely starts with everything unassigned**, so the pool model holds without
  exception. *Unassigned* keeps the meaning it was given — "how much of my money still needs a
  job" — instead of starting deeply *Over-assigned* and climbing towards zero as the salary lands, which
  is what happens if a new period opens with last period's assignments already in place and no
  income yet received.
- **Re-planning a stable month stays near-zero work** (quality goal 2,
  [§1.2](01-introduction-and-goals.md)). Groceries are roughly groceries again in November, and
  nobody should have to retype what has not changed. That is what carrying budgets over was
  protecting in the first place, and it survives intact — the figures are still there, they are
  just offered rather than applied behind the user's back.

Carrying figures rather than assignments follows from a *Budget* being a plan (above). There is no
money sitting in a category to carry anywhere; there is only last period's intention, which is a
good first guess at this period's.

**Refined on 2026-09-26, and since built.** "The previous period" and "last period's plan" above now
mean **the latest earlier period that has a plan**, which is the previous period whenever that one
was planned. When the plan is offered, how it is shown and what taking it over does were settled
the same day (*Opening a period*, next), and built in the opening-a-period increment. The wording
above is left as it was written. It is still right in the ordinary month, and what was believed is
part of the record.

### Opening a period

The rulings for the next increment, settled with the stakeholder on 2026-09-26. Like the
persistence round, they were answers to multiple-choice questions, each put to him with a
recommendation, and they went straight into this glossary rather than into a new interview round.
**He took the recommended option every time but once** (*The offer is a button, and a figure on
each row*, below). **The reasoning given with each ruling is the documentation's.** It was offered
with the recommended option as the argument for it, and he chose that option without adding
reasons of his own. Where he chose otherwise, that is said.

**Nine rulings came first, and five follow-ups the same day.** Writing the nine up left five
points open. They were put to him as multiple-choice questions on 2026-09-26, and he took the
recommendation on all four questions. The fifth point was stated in a question and he did not
contest it, so it is recorded as confirmed. Each follow-up sits in the subsection it belongs to,
marked *follow-up*. **Three more came at the scenario gate**, the same day, on assumptions the
scenario writer raised. He took the recommendation on all three. They are marked *gate*.

**Settled, specified and built.**
[`take-over-a-plan.feature`](../../features/take-over-a-plan.feature), 19 scenarios and 23 cases,
was **approved at the scenario gate on 2026-09-26**, with the three gate rulings. The plan was
approved at the plan gate the same day, and the increment was built to it and is green. The domain
works out the offer with `Ledger.PlanOfferedIn` and takes it over with `Ledger.TakeOverPlan`, which
assigns through `Ledger.Assign` and writes no budget of its own ([§8.1](08-crosscutting-concepts.md)).
The screen holds the offer in `PeriodOverview` and acts through `MoneyBudApp.TakeOverPlan`
([§8.4](08-crosscutting-concepts.md)). No record was written for it
([§9](09-architecture-decisions.md)), and nothing new is kept. What the plan and the build chose
where the rulings are silent is under *Taking a plan over: chosen in the build, not put to the
stakeholder* (below).

> **In the current period and every later one, while every *Budget* in the period is zero,
> MoneyBud offers the plan of the latest earlier period that has one.** The offer is a button in
> the assign area that names that period and the plan's total, and each category row shows, in
> grey, the figure it would take over. **Taking the plan over assigns every figure in full**, even
> past *Unassigned*, and a notice names the period it went into. The first amount assigned, by hand
> or by taking the plan over, ends the offer, and taking every budget back to zero brings it back.
> There is no undo.

An example, with synthetic figures. August was planned: Boodschappen €400, Huur €900, Hobby €150.
September went by unplanned. It is 1 October, nothing is assigned in October, and the salary is not
in yet. October offers August's plan: a button reading *"Plan van augustus 2026 overnemen
(€ 1.450,00)"*, and *"plan: € 400,00"* in grey on the Boodschappen row, and so on. Taking it over
sets the three budgets to 400, 900 and 150, *Niet toegewezen* shows −€ 1.450,00 with the marker
until the salary is recorded, and a notice says the plan went into October. The offer and the grey
figures are gone.

**A state, not an event, in the documentation's reading.** Nothing has to happen at the moment a
period begins. The offer is there whenever a current or later period has no plan and an earlier one
has, and gone otherwise. That fits the rest of the model, in which a period ends but nothing ever
closes it (*Ending versus closing a budget period*, above), and nothing is announced when one begins
(*Staying open across a period boundary*, below).

#### The offer stands only while the period has no plan

> **The plan is offered only while nothing is assigned in the period.** The first assignment, by
> hand or through the offer, makes the offer disappear.

**Why.** Offered only to a period with no plan, taking it over has one meaning: this period's plan
becomes that one. Once something has been planned by hand, every way of combining the two needs a
rule the user would have to learn.

| Rejected | Why |
|---|---|
| **Always offered, topping each category up to at least its remembered figure** | Hard to explain, and it partly overwrites what was planned by hand: a category assigned more than its remembered figure keeps its own, one assigned less is raised |
| **Always offered, adding on top** | Taking it twice doubles the plan |

#### "Nothing is assigned" means every *Budget* is zero

> **A period has no plan while every *Budget* in it is zero, however it got there.** Assign €100,
> take it back with −100, and the offer is back.

**Why.** There is no separate "never budgeted" state (*Budget*, terms table; *"A Budget of zero"
means zero*, below). A budget taken back to zero is the same as one never made.

| Rejected | Why |
|---|---|
| **Once anything was assigned, the offer is gone for good** | MoneyBud would have to remember a difference it deliberately remembers nowhere else |

**So this must not be decided by `Ledger.HasBudget`**, the one query that can tell "never assigned"
from "assigned, then taken back to zero". It is the third place that query must not reach, after
the ring and deleting a category ([§8.1](08-crosscutting-concepts.md)). The same goes for whether an
earlier period has a plan (next but one). **Built that way**: `Ledger.PlanOfferedIn` reads whether
the figures are above zero and never calls `HasBudget`.

**What follows, derived rather than asked.** Assigning zero, and a negative assignment clipped
against a *Budget* already at zero, change no figure (*Assigning zero is accepted and moves
nothing*, above), so neither ends the offer.

> **An archived category's *Budget* counts.** A period in which an archived category still holds a
> *Budget* above zero is not empty, and gets no offer until that budget is taken back.

*Follow-up*, 2026-09-26. **Why:** it is still assigned money. *Unassigned* subtracts it
(*Over-assigned*, below), and a budget above zero is history that keeps the category shown (*Where
an archived category is still shown*, above). Taking it back is already how an archived category's
leftover budget returns to *Unassigned* (*Only a positive assignment brings it back*, above), and
once it is back the offer appears.

| Rejected | Why |
|---|---|
| **Ignore archived categories when deciding whether the period is empty** | The offer would show while money was already assigned in the period, so "empty" would mean two things: no plan for the offer, and something assigned for *Unassigned* |

#### The plan offered is the latest earlier one

> **The plan offered is that of the latest earlier period that has a plan**, not only the period
> immediately before. **The offer names that period**, so it is visible when the plan is not last
> month's.

**Why.** A skipped month costs nothing. A month nobody planned, a holiday or a month MoneyBud was not
opened, would otherwise leave nothing to offer, and the month after it would be planned from
scratch.

| Rejected | Why |
|---|---|
| **The previous period only** | After one skipped month, nothing is offered |

**"Earlier" is earlier than the period the offer is for**, not earlier than today, in the
documentation's reading. A future period can be offered the current period's plan, or another future
period's. The search has no limit: MoneyBud looks back until it finds a plan or runs out of periods.
**Built without a search at all**: budgets are stored against their period's first day, so the
source is simply the latest first day before this period that carries a qualifying *Budget*
([§8.1](08-crosscutting-concepts.md)). The effect is the ruling's, however far back that is.

#### What counts as having a plan

> **An earlier period has a plan only if it has a *Budget* of more than zero for a category that is
> not archived now.** If it has none, MoneyBud keeps looking further back. If no earlier period
> qualifies, there is no offer.

**Why.** A period whose only budgets belong to archived categories has nothing to offer, because an
archived category's figure is not offered back (*An archived category's figure is not offered back
when a period opens*, above).

| Rejected | Why |
|---|---|
| **Take that period as the source anyway, and offer nothing** | After a clear-out, the offer is lost until a month is planned by hand |

> **Any *Budget* above zero makes a plan, however small.** That cost is accepted.

*Gate*, 2026-09-26. Assign €0,01 in October by mistake, and November is offered October's one-cent
plan instead of September's. **Why:** the button shows the source period and the total, so the
mistake is visible, and taking October back to zero brings September's plan back into the offer. No
extra rule is needed.

| Rejected | Why |
|---|---|
| **A further rule, such as a threshold on how much of the plan must be covered** | A number nobody picked, which would have to be designed and explained |

**What it means.**

- **An archived category's figure is skipped**, the existing ruling, and it cannot make a period
  count as planned either.
- **A category whose remembered figure is zero is not part of the plan.** Taking the plan over
  leaves its *Budget* at zero. That includes a category added since the source period.
- **"Archived now" is read when the offer is shown**, in the documentation's reading: archive a
  category and an earlier period may stop counting as planned; bring it back and it counts again.
- **Figures follow the category, not its name**, in the documentation's reading. A category renamed
  since is offered under its new name, because renaming changes only the label (*Renaming a
  category*, above). A deleted category cannot be in a plan: a *Budget* above zero is history, and a
  category with history cannot be deleted.

#### Offered in the current period and later ones

> **The plan is offered in the current period and later ones**, where assigning is allowed. **A past
> period shows no offer**, because a past plan cannot be changed.

| Rejected | Why |
|---|---|
| **The current period only** | A future period would have to be planned by hand until it became current |
| **Shown in every period, and refused in a past one, like the assign form** | The offer is an offer, not a form through which to reach a refusal |

**Why this differs from the assign form, which a past period does show.** The assign form is shown
in a past period and refuses there (*Defaults, and entering while another period is shown*, below).
That was chosen so that the past-period refusal can be reached
from the screen at all, the UI's scope being everything the domain does. That reason does not reach
the offer. The assign form already reaches the refusal, and in a past period a take-over could do
nothing else.

> **If MoneyBud stays open past a period boundary, the offer and the grey figures disappear quietly
> at the next refresh.** Nothing is announced. **Pressing the button before the refresh is refused**,
> like any assignment in a past period.

*Follow-up*, 2026-09-26. The screen stays on the period it showed, which becomes a past period at
the boundary (*Staying open across a period boundary*, below). **Why:** it is what the *Huidige
periode* label already does, and what a category in use with no history already does when it drops
off that period. For up to a minute the button can still be on screen, and pressing it then meets
the past-period refusal, which is the only thing a take-over in a past period could do.

| Rejected | Why |
|---|---|
| **A notice that the offer has gone** | Nothing is announced at a period boundary anywhere else, and the stakeholder ruled that out for the boundary itself |

#### The offer is a button, and a figure on each row

> **A button in the assign area names the period whose plan is offered and the plan's total**, for
> example *"Plan van augustus 2026 overnemen (€ 1.450,00)"*. **Each category row also shows, in grey,
> the figure it would take over**: *"plan: € 400,00"*.

**Here he did not take the recommendation.** It was the button alone, with its total: less to read.
He chose the grey figure on each row as well. The reason, as that option put it when he chose it:
**you can see what you are taking over before you take it.**

| Rejected | Why |
|---|---|
| **The button alone, with the total** (the recommendation) | Less to read, but the figures cannot be seen before they are taken over |
| **A line in the message bar** | A notice from any entry would push it away. The bar holds one thing at a time (*Chosen in the build, not put to the stakeholder*, under *An entry can be changed or removed*, below) |

**The wording is copy; the terms are display terms.** *Plan overnemen* and *plan* were proposed
here, kept as proposed by the plan, and are now rows of *Dutch display terms* (below), which a test
holds `Tekst` to. The button's text is built from the *Plan overnemen* constant, so the table's words
are the words on the button. The sentence around them is copy. The period is named the way MoneyBud
names any period, "augustus 2026", so the year that the example put to him left out is part of the
name.

> **The grey figure is labelled *plan*: "plan: € 400,00".**

*Follow-up*, 2026-09-26. The example he first chose from read *"vorige: € 400,00"*, and this
replaces it. **Why:** it matches the button's word *Plan*, it is true whichever period the figure
came from, and it cannot be confused with *Vorige periode*, the button that steps back a period.

| Rejected | Why |
|---|---|
| ***vorige*** (what he first saw) | Wrong after a skipped month, when the plan is older than the previous period, and it clashes with the step button |
| **The month on every row, "augustus: € 400,00"** | It repeats what the button already says, and lengthens every row |

> **The button takes the plan into the period on screen**, not into the period the assign form has
> been stepped to.

*Follow-up*, 2026-09-26. The assign form has a period of its own, which starts at the period on
screen and can be stepped away from it (*Defaults, and entering while another period is shown*,
below). **Why the screen's:** the button belongs with the grey figures, which are on the rows of the
period on screen, so the two can never disagree. The form's own period governs *Toewijzen* only.

| Rejected | Why |
|---|---|
| **The assign form's period** | The grey figures would describe one period while the button acted on another |

**Which rows carry a figure.** Every category in the plan offered is in use, and the period is
current or later, so every one of them has a row (*When any category is shown in a period: the full
rule*, above).

> **A row whose category is not in the plan shows no label**, not "plan: € 0,00".

*Confirmed*, 2026-09-26: stated in a follow-up question, and not contested. **Why:** nothing would be
taken over for it (*What counts as having a plan*, above).

#### The figure on each row goes with the offer

> **The grey figures are shown only while the plan is offered.** Once the period has a plan, the rows
> look as they do now.

**Why.** The figure shows what you would take over. It is not a month-on-month comparison.

| Rejected | Why |
|---|---|
| **Keep it all period, for comparison** | A wider row for good, and a step towards comparing months, which the stakeholder floated early on (*"misschien ook met andere maanden"*, [the follow-up interview](../stakeholder/2026-09-24-verdieping.md)) and which is not this increment |

> **While the plan is offered, the category rows are ordered by their grey figure**, largest first,
> ties in the order the categories were added. A row with no grey figure counts as zero.

*Gate*, 2026-09-26. This **extends** *The order of categories and slices* (below) for as long as the
offer stands. That rule orders by *Budget*, and with every *Budget* zero it falls back to the order
added. **Why:** it is the order the rows will have once the plan is taken over. The taken-over
*Budgets* equal the grey figures, so after taking over the rows are already in *Budget* order and
nothing jumps.

| Rejected | Why |
|---|---|
| **Order added**, as the existing rule gives with every *Budget* zero | The rows would jump when the plan is taken over |

**The ring is not affected**, checked against *The overview, and its ring* (below). While the plan is
offered every *Budget* is zero, so no category has a slice. The ring is **all *Niet toegewezen*** if
the period has income, and the **empty ring** with its hint if it has none. It cannot be
*Over-assigned*, because nothing is assigned. Either way there is nothing to put in order.

#### Taking the plan over assigns it in full

> **Taking the plan over assigns every figure in full, even when *Unassigned* is smaller.**
> Afterwards a notice says the plan was taken over and names the period it went into.

On the 1st, before the salary is recorded, taking over €1.450 leaves *Niet toegewezen* at
−€ 1.450,00 with the marker until the salary arrives.

**Why.** MoneyBud shows, it never blocks (*Shown, never enforced*, below), and assigning by hand may
already go *Over-assigned*.

| Rejected | Why |
|---|---|
| **Only up to *Unassigned*** | It needs an order and a rule for splitting, and it stops being "in full" |
| **Refuse until there is enough** | The only place MoneyBud would block over-assigning |

**This does not undo *Budgets carry over as figures***, although it can look as if it does. That
section rejected a period that **starts** *Over-assigned*, with last period's assignments already in
place before any income. Here the period still starts whole. It goes *Over-assigned* only when the
user takes the plan over, visibly and marked, and it stops being so when the income is recorded. The
difference is who acted, and that the user can see the result of acting.

**Why the notice names the period.** Taking a plan over can happen in a future period as well as the
current one, and the name is what shows a take-over made in the wrong one (next).

**What follows, derived rather than asked.** Taking over brings no archived category back, because
no archived category's figure is in the plan. It touches no expense. It is one act, with one notice.

> **Taking the plan over does not ask for confirmation.**

*Gate*, 2026-09-26. **Why:** only removing an entry asks, because it destroys a record (*Removing an
entry asks first*, above). Taking a plan over loses nothing, since the period had no plan, and the
notice names the period it went into. That is the principle "confirm only where a record is lost",
giving the same answer it gives for archiving and deleting a category.

| Rejected | Why |
|---|---|
| **Ask first, with the question in the message bar** | A second act that asks, over something that can be corrected, and one extra click every month |

#### No undo, and no act to clear a plan

> **A plan taken over cannot be undone in one step, and there is no act to clear a period's plan.**
> Taken over in the wrong period, it is corrected row by row with negative assignments.

**Why.** A negative assignment is how a *Budget* is lowered anyway (*An amount may be assigned
negatively*, above), and the notice naming the period makes the mistake visible. Undo can be a wish
of its own later.

| Rejected, for now | Why |
|---|---|
| **A *Plan leegmaken* act** | A new act with rules of its own to settle: whether it asks first, and what it does to archived categories |

**The cost, accepted:** undoing a take-over by hand is one negative assignment per category in the
plan. Once every *Budget* is back to zero, the offer is back (*"Nothing is assigned" means every
Budget is zero*, above), so the period ends up as it started.

#### Taking a plan over: chosen in the build, not put to the stakeholder

The rulings above settle what is offered, when, and what taking it over does. A few visible details
they leave open were settled later, some by the plan and some while building. **Only the second
kind are the build's readings rather than rulings**, and either kind can be put to the stakeholder
if he reacts to it. None contradicts a ruling.

**Proposed by the plan and approved at the plan gate, 2026-09-26, and built that way:**

- **The button sits directly under the assign form**, shown only while a plan is offered. That is
  "in the assign area" made exact.
- **The grey figure sits under the *Budget* figure** on each row, not in a column of its own.
- **The notice reads** *"Plan van augustus 2026 overgenomen in oktober 2026: € 1.450,00
  toegewezen."* It names the period the plan came from, the period it went into and the total.
  The period it went into is named **even when it is the period on screen**, because the name is
  what shows a take-over made in the wrong period (*Taking the plan over assigns it in full*,
  above). The wording is copy.

**Chosen in the build:**

| Reading | Why it was built this way |
|---|---|
| **The *Budget* column is wider**, 104 px where it was 86, in the header and on every row | The grey figure shares the *Budget* figure's column, and "plan: € 1.450,00" is longer than a *Budget* figure alone. Widening that column is what "no column of its own" leaves. No reason beyond fitting the figure was recorded; this is the documentation's reading of the change |
| **A refused take-over says what a refused assignment in a past period says**: *"In een voorbije periode kan niets meer worden toegewezen."* It does not mention the plan | The ruling is that pressing the button after a boundary "is refused like any past-period assignment", and it is the same refusal reason, `AssignRefusal.PeriodInPast`, so it has the same sentence. A sentence of its own would be a second wording of one rule. The refusal is shown in the message bar like any other, and the redraw after it takes the button away |

**Not a reading, although it could look like one: the row order while a plan is offered.** Rows
without a grey figure come after rows with one, and that is the gate ruling itself, under which a
row with no grey figure counts as zero (*The figure on each row goes with the offer*, above). What
the build chose is only how: one sort for the rows at all times, by *Budget*, then plan figure, then
order added. While a plan is offered every *Budget* is zero, so the plan figure decides. With no
offer no row has a plan figure, so the second key changes nothing
([§8.4](08-crosscutting-concepts.md)).

## Income carries a label, and it is required

> **An income must have a label. An expense's label stays optional.**

The requirement is the stakeholder's own, from [round 1](../stakeholder/2026-09-24-interview.md):
*"En ik moet duidelijk kunnen aangeven waar het van is"* — I must be able to say clearly what it is
from.

**Why the two differ, which is the part worth recording.** The asymmetry looks arbitrary until you
notice that the two transactions are not carrying the same amount of information to begin with:

| | What already says what it is | What the label adds |
|---|---|---|
| **Expense** | Its **category**. "Groceries, €32.15, Tuesday" is a complete record of what kind of spending this was | *Which particular purchase* — "Albert Heijn". Detail on top of a record that already reads |
| **Income** | **Nothing.** An income names no category; it lands *Unassigned* by definition | *What this money is* — salary, refund, birthday gift. Without it the record is a bare amount |

So an unlabelled expense is still legible and an unlabelled income is genuinely blank. The two
rules are the same principle — *a transaction must say what it is* — applied to records that start
from different places.

**What it buys (goal 1, [§1.2](01-introduction-and-goals.md)).** The pool is what the user budgets
from. A pool of three anonymous amounts makes deciding where they should go guesswork, and a
windfall that cannot be told apart from a salary is exactly the case *Unassigned* exists to hold
honestly.

**What it costs (goal 2), accepted.** It is a required field on an entry MoneyBud otherwise wants
frictionless. Accepted because the frequency argument runs the other way here than it does for
expenses: income is recorded a handful of times a period, not several times a week standing in a
shop, so one more field costs far less than the same field would on an expense — which is the same
reasoning that makes *An expense defaults to the pool account* (above) worth its known weak spot.

**Nothing is derived from the label**, on either transaction. It is free text for the reader, not a
key, not a category by another name.

### A label is trimmed, and that is what makes "blank" mean anything

> **Surrounding whitespace is stripped; the inner text is left alone.** `"  Salaris september  "`
> is stored as `"Salaris september"`.

**Trimming is the primary rule, and refusing a blank label is its consequence.** A label that trims
to nothing is not a label — so an income labelled `"   "` is an income with no label at all, and
the requirement above refuses it on exactly the same ground as one with no label given.

The order matters, because it was first written the other way round: "required means the label must
actually say something", with the trimming left implicit. That implicit step was the problem.
**Something has to trim `"   "` in order to judge it blank**, so if the stored label were *not*
trimmed, the two rules would disagree about what a label is — one trimming to decide, the other
keeping whatever it was handed. Making the trim the rule and the refusal its consequence leaves one
answer to that question instead of two.

**Trimming loses nothing anyone wants**, which is what makes it safe. Nothing is derived from a
label (above): no lookup, no grouping, no comparison that a leading space could carry meaning for.
The only thing stripping it can destroy is whitespace nobody meant to type.

**Inner whitespace is untouched.** `"Salaris  september"` keeps its double space. The argument is
about text at the edges of a field, where it is almost always an accident of typing or pasting; it
says nothing about what someone wrote in the middle, and MoneyBud does not tidy the user's prose.

**This is one rule covering both labels, not an income rule.** An expense's label is *optional*, so
there is no blank-is-refused rule there for trimming to follow from — but that is a difference in
what the two transactions do with an empty result, not a difference in what a label **is**. Storing
`"  Albert Heijn  "` untrimmed while storing `"  Salaris september  "` trimmed would be an
inconsistency with no argument behind it, and the "nothing is derived from a label" reason applies
to both equally. So **every label is trimmed**, and the required/optional difference decides only
what happens when the result is empty:

| Label that trims to nothing | Outcome |
|---|---|
| On an **income** | **Refused.** The label is required and this is not one |
| On an **expense** | Accepted as **no label**, which an expense is allowed to have (*Label*, in the terms table) |

**How the two halves were settled.** The blank-is-refused half was written here first as a
**derivation** — it follows from the reason the requirement exists rather than from anything the
interviews say in so many words — stated in full so that it could be contradicted. It was not
contradicted; the stakeholder confirmed it. The trimming half is his own decision, taken once it
was noticed that the derivation had quietly assumed it. Both now stand as decisions, and the
scenarios for recording income assert them.

## Income may be dated in the future; an expense may not

> **An income may be dated in the future.** It counts against the budget period its date falls in,
> including a period still to come, and it joins that period's *Unassigned* **from the moment it is
> recorded, not from its date**.

An expense may not: [`features/record-expense.feature`](../../features/record-expense.feature)
refuses one dated tomorrow or in the next period, and that stays true.

**A derivation about "from the moment it is recorded".** *Unassigned* is a per-period figure, so
these two statements do not conflict: the income belongs to **its own** period's *Unassigned* — the
one its date falls in — and what happens on recording is that the figure for that period changes
**straight away**, rather than the income sitting invisible until its date arrives. Recording next
month's salary today makes next month's pool show it today. That is the whole point of the rule; if
the figure waited for the date, future-dating would buy nothing.

**Why — the stakeholder's reasoning, recorded as his.**

- **To budget a period you have to know what is coming into it.** Budgeting a period out of the
  previous period's money is the wrong shape. And nothing rolls forward across a boundary
  (*Nothing crosses a period boundary without a purpose*, below), so the next period's pool is its
  own income and nothing else — without future-dated income that pool is empty until the money
  physically lands, and the period cannot be planned until it has already begun.
- **Expenses already have a forward-looking layer, and income has none.** A planned expense *is* a
  budget: assigning €400 to Groceries is exactly the statement "I expect to spend €400 here". To
  record that as a future-dated expense would be to say the same thing twice, in a concept that
  already exists, on the wrong layer — and the *actual* layer would then contain money that has not
  moved. There is no matching "planned income" anywhere in the model, so on the income side
  future-dating is not a duplicate of anything; it is the only way to state a future amount at all.

**So the asymmetry is a consequence of the plan/actual split, not an inconsistency in it.** The
plan layer covers the future for spending; income has only the actual layer, and therefore has to
cover its own future. Anyone reading "income may be future-dated, expenses may not" as MoneyBud
being inconsistent has found the wrong explanation — this is the right one.

**A scope note, not a claim about budgeting in general.** The stakeholder observed that future
expenses are not a thing for him personally, because he has no loans, while acknowledging that
loans would be a genuine case for them. That is a fact about this user, recorded as such. If a loan
or any other committed future payment ever enters the picture, this is the paragraph to reopen, and
the question to reopen it with is whether such a payment is an expense or a plan.

**Future-dated entry is not a stand-in for recurring transactions.** [§1.1](01-introduction-and-goals.md)
lists one-off and recurring side by side as capabilities; both are wanted, permanently and
together. Nothing here should be read as "recurring will replace this later" — entering an amount
by hand, dated whenever it belongs, stays a first-class act once recurring income exists.

## An entry can be changed or removed

> **An expense or an income can be changed, and it can be removed, in any budget period.** Until
> 2026-09-26 neither was possible (*There is no editing or deleting of a transaction*, in *The user
> interface*, below).

The rulings in this section, and in *Renaming a category* and *Deleting a category that has no
history anywhere* (both above), were **settled by the stakeholder on 2026-09-26**. They came from a
set of multiple-choice questions put to him in English. Like the follow-ups of 2026-09-24, they went
straight into this glossary rather than into a new interview round. **He chose each answer. The
reasoning recorded with each is the documentation's, not his.** It was offered with the options as
the argument for them, and he chose among them without adding reasons of his own. He also chose the
scope, all four parts of it, and confirmed that explicitly: changing an entry, removing one, renaming
a category, and deleting a category that was never used.

**Settled, specified, and built.** The four feature files were approved at the scenario gate on
2026-09-26: [`change-an-entry.feature`](../../features/change-an-entry.feature),
[`remove-an-entry.feature`](../../features/remove-an-entry.feature),
[`rename-a-category.feature`](../../features/rename-a-category.feature) and
[`delete-a-category.feature`](../../features/delete-a-category.feature). Six readings approved with
them are listed in *Approved at the scenario gate, 2026-09-26* (below). The plan was **approved at
the plan gate** the same day. It proposed what the form does around a correction, which this
section had left to it (*On screen: picking an entry to correct*, below). The increment was built to
it and reviewed by `spec-reviewer`, which found no faked scenario and one low defect, since fixed
(*Chosen in the build, not put to the stakeholder*, below). How the domain holds these rulings is in
[§8.1](08-crosscutting-concepts.md), and how the screen holds them is in
[§8.4](08-crosscutting-concepts.md).

### Removing an entry asks first

> **Removing an expense or an income asks for confirmation first.** Nothing is removed until the
> user confirms. The question, *Weet je het zeker?*, is copy, not a term.

Chosen over removing and then offering undo, and over removing and just saying so, as archiving does.

**Why.** A confirmation protects against losing something (*Archiving is announced, never
confirmed*, above). Archiving goes unconfirmed because it loses nothing: it destroys no record, and
adding the name undoes it. Removing an entry destroys a record, and nothing undoes it, because
MoneyBud keeps no copy (*A change overwrites the entry*, below). So the same reasoning gives the
opposite answer. **One principle, two answers: confirm only where a record is lost.** The same
principle leaves deleting a category unconfirmed (*Deleting a category that has no history
anywhere*, above).

| Rejected | Why |
|---|---|
| **Remove, and just say so, like archiving** | Archiving goes unconfirmed because nothing is lost. That reason does not reach an act that loses a record |
| **Remove, then offer undo** | It protects the same record by another route, and it would need MoneyBud to hold on to a removed entry. That is a kind of history MoneyBud keeps nowhere else (*A change overwrites the entry*, below) |

**Being asked is not being warned.** The confirmation is about the act the user is taking, not
about the state of their money. MoneyBud still never warns or asks about *Over budget*,
*Over-assigned* or anything else it shows (*Shown, never enforced*, below). This is the **first**
place MoneyBud asks anything before it acts, and the only one. The approved scenarios that say *I
should not be warned or asked to confirm* are about recording, assigning and archiving, and stay
true.

**The user is told afterwards that the entry was removed.** This was not asked. Every other act
tells its outcome (*Archiving is announced, never confirmed*, above), so the documentation first
recorded it as a derivation, so that it could be contradicted. The approved scenarios assert it, so
it was **approved at the scenario gate on 2026-09-26**. The ruling that a change is announced
(*Changes and renames are announced*, below) rests on the same reason.

> **After the user declines *Weet je het zeker?*, nothing is said.** The entry simply stays.

Settled by the stakeholder on 2026-09-26, chosen over saying that the entry was kept. **Why**, in
the documentation's reasoning, which he chose: the user chose not to act, so there is nothing to
report. This does not break "every act tells its outcome": declining is not an act, it is choosing
not to take one.

### An entry in a past period can be corrected

> **An entry can be changed or removed whatever budget period it is in, past periods included.**

Chosen over "no, past is past, like assigning".

**Why.** An entry is a **fact**, and a wrong fact should be fixable wherever it sits. *"Past is
past"* (*Assigning happens in the current budget period and later ones, never in a past one*, above)
is about not rewriting a **plan** after the event. A forgotten plan stays forgotten, because the
plan you actually had is part of what happened. A wrong entry is a record of something that did
**not** happen, and correcting it makes the past period truer. *Ending versus closing a budget
period* (above) already draws this line. A past period keeps accepting transactions because
*"correcting history has to stay possible"*, and correcting an entry is that same possibility, one
step further.

**A correction can change a past period's figures, and that is its purpose.** Fix an expense's
amount in October, and October's *Remaining* moves, and with it whether that category was over
budget there.

| Rejected | Why |
|---|---|
| **No: past is past, like assigning** | It reads "past is past" as covering facts, and it covers plans. A wrong October expense would stay wrong for good, although the reason a period never closes is so that October's record can be put right |

> **A past period left *Over-assigned* by a correction stays that way. That is accepted.**

Removing or lowering an income in a **past** period can leave that period *Over-assigned* (next),
and **nothing can undo that**. Assigning in a past period stays refused, a negative amount included,
so no budget there can be reduced to match. The period then shows *Over-assigned* for good.

This was first noticed while the rulings were written up, as a consequence of three settled rules
acting together. It was then **put to the stakeholder and accepted on 2026-09-26**, so it stands as
a ruling and not as a derivation. **Why**, in the documentation's reasoning, which he chose: it is
the **true figure**, because more was assigned in that period than came in. The marker shows it. And
it is the same "past is past" cost already accepted for assigning (*Assigning happens in the current
budget period and later ones, never in a past one*, above).

| Rejected | Why |
|---|---|
| **Allow a negative assignment in a past period** | It would reopen the ruling that a past period's plan is not re-planned, to tidy a figure that is true |

### Removing or lowering an income may leave its period over-assigned

> **Removing an income, or lowering its amount, may leave its budget period *Over-assigned*. That is
> allowed, and it is shown with the existing marker.** Nothing more is said about it: **neither the
> confirmation question nor the message afterwards mentions *Over-assigned*.**

Chosen over allowing it but saying so in the message, and over refusing it. **Why:** MoneyBud
shows, it never blocks (*Shown, never enforced*, below). Assigning is already allowed to reach this
state, and a correction reaching it is shown the same way.

The same holds for any change that takes income out of a period. **Moving an income to another
period can leave the period it left *Over-assigned*, the same as removing it.** First the
documentation's reading, then **approved at the scenario gate on 2026-09-26**.

| Rejected | Why |
|---|---|
| **Refuse it** | It would block correcting a fact because of the figure the correction leaves. The figure is information, not a gate |
| **Allow it, and say so in the message** | The marker already shows the state where the figure is, the same way however the period got there |

### A changed entry is judged as if it were recorded now

> **A change passes or fails exactly as the changed entry would if it were typed in fresh now. A
> refused change leaves the entry as it was.**

So every rule that decides whether a recording is accepted applies to a change, and nothing else
does:

- an expense dated in the future is refused, and an income dated in the future is allowed;
- an income needs a label, an expense's label stays optional, and every label is trimmed;
- an expense must name one of your categories, compared by the name rule, and a name that is none
  of them is refused;
- an amount must be more than zero and whole cents, and typed text is read as any amount is
  (*Typing an amount*, below);
- when several rules are broken, the one reported is the one recording would report.

> **Changing an expense's category to an archived category's name brings that category back**,
> history and all, spelled as it was, **and the user is told**, as when an expense is recorded
> against it.

Chosen over the same rules with an archived category staying archived. **Why:** one rule to
remember. A correction is a second go at recording an entry, so it is judged by the rules the user
already knows, and there is nothing to learn about corrections in particular.

**A refused change brings nothing back.** Nothing is changed, so the category stays archived. This
follows from the rule that a refused expense brings nothing back (*Recording an expense against an
archived category brings it back*, above), and is the documentation's derivation.

| Rejected | Why |
|---|---|
| **The same rules, but an archived category stays archived** | Two rules for one kind of entry, differing only in whether it is new. A receipt typed in fresh against an archived Hobby would bring Hobby back, and correcting an existing receipt to Hobby would not. It would also let an archived category collect an expense while staying archived, which is why option (c) lost in *Recording an expense against an archived category brings it back* (above) |

> **Fixing an expense that is already on an archived category does not bring the category back.**
> Only moving an expense **onto** an archived category, by changing its category to one that is
> archived, brings it back.

Take an expense recorded against Hobby, which has since been archived, and fix only its label, its
amount or its date. The change is judged as if typed in now, and passes or fails on that. **Hobby
stays archived.**

This was first recorded here as **not settled**, because the two rulings above read differently on
it. "Judged as if typed in now" suggested that any change to an expense on archived Hobby would
bring Hobby back. The bring-back ruling spoke only of changing the category **to** an archived one.
The stakeholder **settled it on 2026-09-26**, chosen over bringing it back strictly as a fresh
recording would. **Why**, in the documentation's reasoning, which he chose: fixing an expense that
is already there is **correcting history**, not using the category again.

**How the two rules fit.** "Judged as if recorded now" decides whether a change **passes or fails**.
Bringing a category back is a **side effect**, and it follows the **category change**, not the
presence of an archived category on the edited entry. That is the reason bringing back exists at
all: naming an archived category **for new entry** is a sign that you want it again (*Only a
positive assignment brings it back*, above). Moving an expense onto Hobby names Hobby anew. Fixing an
expense that was always on Hobby does not.

| Rejected | Why |
|---|---|
| **Bring it back, strictly as a fresh recording would** | Correcting a typo in an old receipt would take a category out of the archive, as a side effect of an act that had nothing to do with using it |

### A changed date can move an entry to another period

> **When a change moves an entry into another budget period, the Overview stays on the period it
> showed and says which period the entry went to. The entry leaves this period's list and its
> figures.**

Chosen over the screen following the entry. **Why:** it extends the rule for a new entry that lands
elsewhere (*Defaults, and entering while another period is shown*, below) to a changed one. An entry
that ends up out of view is treated one way, however it got there.

### A change overwrites the entry

> **A change overwrites the entry. MoneyBud keeps no record of what it was before, and shows no
> history of changes.**

A default the stakeholder accepted. It answers the question [§11](11-risks-and-technical-debt.md)
carried from the UI increment on, *whether an edit is a new record or a rewrite*: it is a
**rewrite**.

**What it costs**, in the documentation's reading. A change saved by mistake can be put right only
by changing it back, from memory. That is also why removing asks first and changing does not. After
a change the entry is still there to be changed again. After a removal there is nothing left.

**It also fixes what "ever" can mean.** With no history kept, MoneyBud cannot know that an entry once
said something else, or that a removed one existed. So a rule about what a category has "ever" had
can only be about what exists now (*Deleting a category that has no history anywhere*, above).

**A changed entry keeps its place among entries on the same date.** A period's entries on one date
are listed newest-recorded first (*A period's entries are listed newest first*, below). A rewrite is
the same entry, not a newly recorded one, so it keeps its place. First the documentation's reading,
then **approved at the scenario gate on 2026-09-26**.

### Correcting can change where a category is shown

> **Removing an archived category's last expense in a period drops the category from that period**,
> unless it still has a budget of more than zero there.

Derived from the history rule (*Where an archived category is still shown* and *When any category
is shown in a period: the full rule*, above), then **accepted by the stakeholder**.

**What else follows**, in the documentation's reading and not put to him separately. An expense
changed so that it no longer counts there, to another category or to a date in another period, has
the same effect as removing it. The same holds for a category **in use** in a **past** period,
because a past period shows only what has history there. And it works the other way: an expense
moved into a period gives its category history there, so the category is shown.

### On screen: picking an entry to correct

> **Clicking a row in a transaction list loads that entry into its entry form, which switches to a
> *Wijzigen* state with *Opslaan*, *Annuleren* and *Verwijderen*.**

Chosen over edit and delete icons on every row with a dialog window, and over editing inline in the
list. **Why:** it reuses what the user already knows, which is the form's field order (*The fields
ask what before how much*, below) and how a typed amount is read (*Typing an amount*, below). A
dialog or an inline editor would be a second place to enter the same fields, with the same rules to
keep in step. *Verwijderen* is where removing is reached, and it is the act that asks first
(*Removing an entry asks first*, above).

*Wijzigen* and *Verwijderen* are display terms, and are in *Dutch display terms* (below), with
*Hernoemen* and the *Verwijderen* for deleting a category. *Opslaan* and *Annuleren* are the form's
controls, not terms of the model, and are not in the table.

**Left for the plan to propose**, as the ring's width was: what the form shows after *Opslaan*,
*Annuleren* or *Verwijderen*, and what happens to a change in progress when the user steps to
another period or clicks another row. One case of the first was already settled (next): after
saving an unchanged entry, the form returns to normal.

**Proposed by the plan, approved at the plan gate on 2026-09-26, and built:**

- **The form empties and goes back to recording** once a change goes through, a change saved
  unchanged included, after *Annuleren*, and after a confirmed removal.
- **After a refused change the form keeps what was typed and stays in *Wijzigen*.** A refused
  recording keeps what was typed for the same reason: a refusal is corrected in place.
- **Stepping to another period drops an entry being changed** and a waiting question. **A new
  entry being typed is kept.** Only an entry being changed was picked from a row of the period
  that was on screen. A new one belongs to no period until it is recorded.
- **Clicking another row loads that entry** in place of the one loaded.
- **Declining the question says nothing** (*Removing an entry asks first*, above) and **leaves the
  entry loaded in its form**, so the user can go on to change it or cancel.

**The question is asked inline, in the message bar**, rather than in a dialog window. The
stakeholder asked for that at the plan gate. Its two answers, and which other acts drop it, were
chosen in the build (*Chosen in the build, not put to the stakeholder*, below), and so was
cancelling a rename when the screen steps.

The stepping rule is **not** what was first built. As first built, stepping emptied a new entry
being typed as well. `spec-reviewer` found it, and it was fixed before the increment closed: a new
entry is not tied to the period on screen, so there was nothing to drop it for.

### Changes and renames are announced

> **A successful change to an entry, and a successful rename, are announced afterwards**, for
> example *Uitgave gewijzigd* and *Categorie hernoemd*. The wording is copy.

Settled by the stakeholder on 2026-09-26, while the scenarios were being written. Chosen over
announcing neither and over announcing only a rename. **Why**, in the documentation's reasoning,
which he chose: every act tells its outcome, and a silent one looks the same as a failure. It
extends the list in *Archiving is announced, never confirmed* (above): **created**, **already
there**, **brought back**, **archived**, and now **changed** and **renamed**. As before, the message
is information after the fact, never a question first.

> **Saving an entry with nothing changed is never refused, and goes through quietly.** Nothing
> changes, nothing is announced, and the form returns to normal.

Settled by the stakeholder on 2026-09-26, chosen over letting it go through and saying so. No
separate reason came with the choice. It is not an exception to the announcing rule above: there is
no outcome to tell.

**A consequence for the build.** Written down here because it is easy to miss. An unchanged entry is
judged as if recorded now (*A changed entry is judged as if it were recorded now*, above), so what the
form hands back must pass the same reading as typed text. **The amount loaded into the form must
therefore be in a form the amount box accepts.** MoneyBud's display form, "€ 2.000,00", cannot be
typed back in (*Typing an amount*, below). If the form were loaded with it, saving an unchanged
entry would be refused as "not an amount", which this ruling forbids.

**How the build met it.** An amount is loaded as **"2000,00"**: a comma, always two decimals, no
thousands separator and no euro sign (`AmountInput.Format`). Two decimals always means a loaded
amount can never end in a mark and three digits, so it is never read as ambiguous either. A unit
test holds that every such text reads back to the same amount, and every *saved with nothing
changed* scenario loads an entry and saves it back through the same form. The domain also
recognises an unchanged save **before** it runs any check, so it cannot be refused by construction
([§8.1](08-crosscutting-concepts.md)).

### Corrections and the sweep

There is no sweep, so there is nothing to decide against (*Why it cannot be answered yet*, below).
What is fixed is which existing rule or question each kind of correction meets, if it lands in a
period that has already been swept:

| A correction that... | What MoneyBud discovers | Meets |
|---|---|---|
| **Raises the period's spending**: an expense's amount raised, or an expense moved in | It moved **too much** | *A late expense against a leftover that has already been directed* (above). Answered: recalculate, show the discrepancy, leave the transfer to the user |
| **Lowers the period's spending**: an expense removed, reduced, or moved out | There was **more to move** | *What happens to an income back-dated into a period that has already been swept?* (below). It **joins that open question**, and is not answered here |
| **Lowers the period's income** | It moved **too much** | The late-expense rule, as in the first row |
| **Raises the period's income** | There was **more to move** | The open question, as in the second row |

Nothing about the sweep was decided on 2026-09-26. The first two rows only place two cases where
they already belong. **The last two are the documentation's derivation**: they apply the same test,
which way the swept figure moved, to income.

### Approved at the scenario gate, 2026-09-26

These were first written up here as the documentation's readings, or as consequences noticed
afterwards. They were put to the stakeholder with the four corrections feature files, and **he
approved the files with all of them on 2026-09-26**, so they now stand as decisions. The reasoning is
kept, because it is why they stand.

| Approved | What it rests on |
|---|---|
| **A changed entry keeps its place among entries on the same date** | A change is a rewrite of the same entry, not a newly recorded one (*A change overwrites the entry*, above) |
| **A renamed category keeps its place in "order added"** | Renaming gives one category a new label. It does not add a category (*Renaming a category*, above) |
| **Once a category is renamed, its old name is free: adding it creates a new, empty category** | Nothing remembers old names (*Renaming a category*, above) |
| **A deleted category added again goes last in "order added"** | It is a new category, where an archived one brought back keeps its place (*Deleting a category that has no history anywhere*, above) |
| **Moving an income to another period can leave the period it left *Over-assigned*, the same as removing it** | It takes income out of that period, which is what *Removing or lowering an income may leave its period over-assigned* (above) allows |
| **"No history" means no history now: a category whose only expense was removed, or moved to another category, can be deleted** | A removed or changed entry leaves no trace, so "ever" can only mean "now" (*Deleting a category that has no history anywhere*, above) |

**The user is told afterwards that an entry was removed.** This was a derivation too (*Removing an
entry asks first*, above). The approved scenarios assert it, so it was approved with them.

### Chosen in the build, not put to the stakeholder

These are visible to the user, and were chosen while building. **They are the build's readings, not
rulings.** None was put to the stakeholder, and any of them can be contradicted. Each fills a gap
that the rulings above leave, and none contradicts one.

| Reading | Why it was built this way |
|---|---|
| **A rename rewrites the category box in the expense form and the assign form** when the box names the old name under the name rule | A rename frees the old name. Without this, an entry loaded before the rename and saved unchanged afterwards would name a category that no longer has that name. It would be refused, or it would land on a new category that has since taken the old name. Either breaks *saving an unchanged entry is never refused* (*Changes and renames are announced*, above). The rewrite applies to a new entry being typed as well, since that box names the same category |
| **Stepping to another period also cancels a rename in progress** | A category is renamed from its row, and that row may not exist in the period stepped to. The same reason drops an entry being changed (*On screen: picking an entry to correct*, above) |
| **Anything else the user does drops a waiting removal question**: loading another row, *Annuleren*, and any act after which MoneyBud says something or deliberately says nothing, such as saving a change, recording, assigning or renaming (stepping drops it too, which the plan settled) | Each means the user has moved on from the entry the question was about. The question is never left asking about an entry that has since been changed or put away, and **a question and a notice are never shown together**: the question is shown in the notice's place. Added after `arc42-keeper` noticed that, as first built, recording or assigning left the question standing beside the new notice. **Stands beside one thing since 2026-09-26, and built that way:** the save line. The lasting "not saved" state is shown beside any notice and beside the question, and stepping does not clear it (*When a save fails, MoneyBud says so and keeps going*). The one-time "saved again" is said on the same line, so it too can stand beside the question. Neither is an ordinary notice, and between the question and an ordinary notice this reading still holds |
| **The archive button moved under the category's name**, beside *Hernoemen* and *Verwijderen*, out of the column it had of its own | A row can now offer three acts, and three buttons did not fit in the column. *Hernoemen* is on every row, *Archiveren* only on a category in use, as before, and *Verwijderen* only on a category with no history anywhere |
| **The question's answers are *Verwijderen* and *Annuleren*** | No reason beyond consistency was recorded. They are the words the screen already uses: *Verwijderen* is the button that raised the question, and *Annuleren* is how a form backs out |
| **A rename box that is open when the screen refreshes itself loses keyboard focus.** Known, and not fixed | The Desktop refreshes once a minute so that the *Huidige periode* label can move ([§8.4](08-crosscutting-concepts.md)). That rebuilds the category rows, the rename box with them. **The text typed is kept**, because it is held by the screen, not by the box. Only the cursor is lost, at most once a minute |

**The one defect `spec-reviewer` found** is recorded under *On screen: picking an entry to correct*
(above). Stepping used to empty a new entry being typed, and now drops only an entry being changed.

## Unassigned money is something you can see, not something you work out

Income is recorded without saying what it is for. It lands in **Unassigned**, which the user can
see on screen and assign from. Assigning is a separate act, taken whenever the user is ready.

Unassigned behaves like a holding place for purpose, but it is **not a category**: nothing is
budgeted for it, nothing is spent against it, and it is not one of the categories the user creates.
It is where money waits until it becomes one.

**Why.** Two reasons, both from [§1.2](01-introduction-and-goals.md):

- **Effortless entry (goal 2).** Deciding a purpose is the hard part of recording income. Splitting
  the two means money can be entered in seconds, with the thinking deferred to when the user is
  actually budgeting.
- **Legibility (goal 1).** A windfall — a gift, a refund, a bonus — needs somewhere honest to sit
  while it waits. If unassigned money were only a derived total, it would be something the user has
  to work out; shown on screen, it is something the user can point at and move.

The wait is not open-ended. *Unassigned* holds money until the user gives it a purpose or the
budget period ends, whichever comes first — see *Nothing crosses a period boundary without a
purpose*, below.

### One figure, not two

***Unassigned* and *Left to assign* were two names for one number.** Both were defined as the
period's income minus everything assigned to categories in it; there is no state in which they
differ, and no operation that moves one without moving the other. They are now **one figure, named
*Unassigned***. *Left to assign* is retired as a term and should not appear in new documentation,
scenarios or code.

**The retirement holds in Dutch too.** "Nog toe te wijzen", its literal translation, is not used on
screen; *Unassigned* is displayed as *Niet toegewezen* (*Dutch display terms*, below).

**Why that name and not the other.** The argument is the one immediately above, and it is why this
merge goes in this direction rather than the reverse:

- ***Unassigned* names a thing; *Left to assign* names an arithmetic result.** One is money you
  point at and move; the other is a remainder you work out. The section above settles that it has
  to be the first, because a pool that is only a derived total costs goal 1 — so keeping the
  derived-sounding name would have argued against the decision it was supposed to describe.
- **One word already does both jobs.** *Unassigned* is the name of the **value on the purpose
  dimension** (*the central distinction*, at the top of this glossary) as well as of the figure
  that measures it. Two names would have been a value name and a figure name that always had to be
  kept in step, for no difference in meaning.

**Where the retired name survives, read it as *Unassigned*.**
[ADR 0003](../decisions/0003-money-representation.md) still lists *Left to assign* among the
figures that go negative. It is left as written, because records are not rewritten
([§9](09-architecture-decisions.md)) and its point — that `Money` has to be signed — is unaffected
by what the figure is called.

### Shown, never enforced

MoneyBud shows the *Unassigned* figure prominently. When it reaches zero, every euro has a job and
the user has finished budgeting the period.

That is the whole of it. MoneyBud never blocks an action, refuses a period, or nags because
*Unassigned* is not zero. **A period is never "incomplete"** in any state the software recognises;
the figure is information, not a gate.

**Why.** The stakeholder's stated motivation is to be *more motivated*, not better policed
([§1.1](01-introduction-and-goals.md)). Making the gap obvious serves goal 1 — you can see at a
glance whether your money has been given jobs — and costs goal 2 nothing. Enforcing it would add
precisely the friction that gets budgeting apps abandoned, and would punish the user for the normal
case of not having decided yet.

### Over-assigned

**The negative state of *Unassigned* is called *Over-assigned*:** more has been assigned to the
period's categories than the period's income. This is allowed — assigning is never refused, and may
even overdraw the pool account (above) — so the state needed a name.

| Figure | Below zero it is called | It is a property of |
|---|---|---|
| *Remaining* | **Over budget** | a category, within one period |
| *Balance* | **Overdrawn** | an account |
| *Unassigned* | **Over-assigned** | a budget period |

All three are shown as a plain negative figure, never blocked and never warned about — the same
treatment, and the same reasoning, as *Assigning may overdraw the pool account* above.

**Revised on 2026-09-25 for two of the three.** *Over budget* and *Over-assigned* are still shown as
the negative figure, still never blocked and never warned about, but now **with a marker**. That
was the stakeholder's revision (*One marker for over budget and over-assigned*, below). *Overdrawn*
is not covered by it and is not settled.

**And for the third on 2026-09-27**, settled and built the same day: *Overdrawn* gets the same marker, with a
badge of its own (*An overdrawn account carries the marker*, under *Accounts and net worth*, below).
All three are marked alike again.

**Naming it is what makes the merge above safe.** The one real objection to folding *Left to
assign* into *Unassigned* is that "unassigned" reads oddly below zero: money cannot be less than
unassigned, so the merged figure appears to describe something impossible. *Over-assigned* answers
it. Below zero the figure has stopped describing money waiting for a purpose and started describing
a plan that outruns the income, and that is a different enough thing to deserve its own word —
exactly as *Over budget* is the word for a *Remaining* that has stopped describing money left.

**Built in the assigning increment.** It was defined during the income increment because the merge
above needed it, but nothing could reach it then: recording income only ever increases
*Unassigned*. Assigning is what made it reachable. `Ledger.UnassignedIn` now subtracts every
*Budget* in the period, archived categories' included, and `Ledger.IsOverAssigned` is that figure
below zero. [`assign-to-category.feature`](../../features/assign-to-category.feature) asserts it,
including that exactly zero is not over-assigned and one cent more is
([§8.1](08-crosscutting-concepts.md)).

## Nothing crosses a period boundary without a purpose

When a budget period ends, money that has not been spent is in one of three states. Money that has
not landed anywhere is **swept**; money that has already landed stays where it is.

| State at period end | What it is | What happens to it |
|---|---|---|
| **Unassigned** | Money that was never assigned to anything | **Swept** |
| **Leftover of an unbacked category** | Assigned but not spent, and the category names no account, so the money never moved | **Swept** |
| **Leftover of a backed category** | Assigned but not spent, and already sitting in the category's backing account | **Not swept** — it has landed; there is nothing to move |

Nothing rolls forward. Money still *Unassigned* when a period ends does **not** flow into the next
period's pool and does **not** linger in *Unassigned*: the next period's pool is that period's
income and nothing else.

**Why.**

- The leftover rule is the stakeholder's own, from
  [round 2](../stakeholder/2026-09-24-verdieping.md) — *"overblijfsels gaan dus in een
  budgetpotje"*. Directing what is left somewhere useful rather than losing it is one of the core
  capabilities in [§1.1](01-introduction-and-goals.md), not a detail.
- The unassigned rule is the same argument applied to the pool. Money that has sat purposeless
  across a period boundary is precisely the state the app exists to remove, and letting it roll
  forward would break *Unassigned*: the figure would stop being "this period's income minus
  what I assigned from it" and become a running total over an unbounded history — a number the user
  can no longer read at a glance, which costs goal 1.
- Together they are also what lets the next period start whole, which *Budgets carry over* (above)
  depends on.
- Backed categories are exempt because the rule is about money without a place, and theirs has one.
  Sweeping them would move money out of the account it was deliberately put into.

### The sweep

The two swept states are collected into **one movement, into one destination category**, and two
things about that destination are fixed.

**The destination must itself be account-backed.** The whole point of the sweep is that leftover
money stops being notional and actually lands somewhere. An unbacked destination would move
nothing: the figures would shuffle, the money would still be adrift, and the next period would
inherit the problem the sweep exists to end.

**The destination is a default the user sets once**, applied automatically when a period ends,
shown plainly in the period summary, and redirectable afterwards.

**The money is collected from the *pool account*** (above), which is where unassigned money and the
leftovers of unbacked categories are assumed to be sitting — neither of them ever having moved
anywhere else.

**Why automatic.** A period ends because time passed, not because the user did something, so there
is no action to hang a choice on. A prompt would have to be raised out of nowhere, and *Shown,
never enforced* (above) has already settled that MoneyBud does not nag or block over
money that has not been given a job. Automatic-but-visible-and-reversible is the only shape that
loses no money and demands nothing: the sweep always happens, the summary always says where it
went, and a user who disagrees moves it afterwards — which is an ordinary transfer between two
backed categories, not a special case.

This replaces an earlier reading of these as two separate decisions the user takes from two
different places. They are one automatic movement with one destination.

## The user interface

The fifth increment puts a desktop UI over the domain. Everything in this section was settled with
the stakeholder on 2026-09-25, in conversation. Like the follow-ups of 2026-09-24, it went straight
into this glossary rather than into a new interview round.

**It is built.** Its scenarios were approved at the scenario gate, its plan at the plan gate, and it
was reviewed by `spec-reviewer`, all on 2026-09-25. How a typed amount is read was ruled at the plan
gate. Five more rulings followed the review: the refusal of an ambiguous amount, how suggestions
narrow, what the marker's badge says, the *Gearchiveerd* caption, and a refinement of
"alphabetical". Two more came on 2026-09-26, while typed amounts got scenarios of their own: "2.0000"
and ",50" are not amounts. Each sits in the section it belongs to. The toolkit is
Avalonia (*The toolkit*, below). How the screen is arranged in code is in
[§8.4](08-crosscutting-concepts.md).

**The first demo, on 2026-09-26, brought five more rulings, and they are built.** The source is
[the demo feedback](../stakeholder/2026-09-26-demo-feedback.md), plus follow-up questions put to
the stakeholder the same day. The five rulings are: the category box empties after an entry goes
through, the ring becomes the middle column's centrepiece, hovering a slice shows its figures, every
slice has a minimum width, and each form asks *what* before *how much*. The minimum width **revises**
an approved rule, so [`overview.feature`](../../features/overview.feature) went back through the
scenario gate, and [`point-at-a-slice.feature`](../../features/point-at-a-slice.feature) is new. The
plan was **approved at the plan gate on 2026-09-26**, and the choices it made stand as rulings: the
width of the minimum, how minimums squeeze the other slices, where the pointed-at figures appear,
and where the *Unassigned* figure and the assign form went. `spec-reviewer` found no faked scenario,
and raised three low findings, all fixed. Each ruling sits in the section it belongs to and is
marked *first demo*.

### It covers what the domain does, and nothing more

> **The UI covers everything the domain already does**: recording an expense and an income; adding
> a category and archiving one; bringing an archived category back by **all three routes**
> (adding its name, recording an expense against it, and assigning a positive amount to it);
> assigning, including a negative amount (clipped with the shortfall reported), zero, future
> periods and the refusal in a past period; and seeing each period's expenses and income.

In the stakeholder's words: *"It should implement all that is currently working on the backend."*

**The list above is about to grow, and the rule does not change** (2026-09-26). Changing and removing
an entry, renaming a category and deleting one with no history are settled for the domain (*An entry
can be changed or removed*, *Renaming a category*, *Deleting a category that has no history
anywhere*, all above), with how each is reached on screen. By this rule, the UI will cover them when
the domain does. **Both were built in the corrections increment**, the domain and the screen
together, so the list above now includes changing and removing an entry, renaming a category, and
deleting one with no history.

### Category entry is free text with suggestions

> **When recording an expense or assigning, the category box is editable. It suggests the categories
> offered for new entry, and accepts any text.**

Settled by the stakeholder on 2026-09-25, over a pick-list alone.

**Why.** The reasoning recorded here is the documentation's, not his. Only free text reaches
everything the domain does, which is the scope above. An archived category is **not offered**, and
two of the three routes back consist of naming it **anyway**: recording an expense against it, and
assigning a positive amount to it (*Recording an expense against an archived category brings it
back*, *Assigning to an archived category brings it back*, above). A pick-list of offered categories
would leave nothing to name it with. The same goes for the refusal of a name that is not one of your
categories. So a pick-list would make two routes back and that refusal unreachable, and the UI
would cover less than the domain does.

**This settles what "not offered" means on screen: not suggested.** An archived category does not
appear among the suggestions. Typing its name is still accepted, and brings it back, announced, as
the sections above already say. Nothing about *Archived* changes.

> **The suggestions are listed alphabetically.**

Settled by the stakeholder on 2026-09-25, over the Overview's order and over the order added.
**Why**, in the documentation's reasoning: a suggestion list is for finding a name you already have
in mind, and alphabetical is the order in which a name is found by its spelling. The Overview's
order follows the *Budget*, so it changes as you assign (*The order of categories and slices*,
below), and a list that reshuffles is harder to scan. The order added means nothing to someone
looking for a name. This is a separate order from the Overview's, and the two are **not** meant to
match.

**"Alphabetical" means the invariant culture's order, ignoring case.** So "hobby" sorts as if it
were "Hobby", and a name starting with an accented letter sorts among its letter: "Één keer" among
the E's. The order still does not depend on the machine's language. This **refines** what the
scenario gate approved, which assumed alphabetical meant the same ordinal comparison as category
names use. Ordinal puts "Één" after "Z", because it compares character codes, and nobody reading a
list alphabetically expects that. The stakeholder settled it on 2026-09-25, after the review. Names
are still **compared** ordinally ([§8.1](08-crosscutting-concepts.md)). Only the order they are
**listed** in uses the invariant culture. The two are different questions: whether two names are
one category must never depend on language data, and the order a person scans a list in is about
language.

> **The suggestions narrow as you type, on "contains".** Typing "schap" leaves Boodschappen among
> the suggestions.

Settled by the stakeholder on 2026-09-25, after the review, over narrowing on "starts with" and over
not narrowing at all. It settles what
[`suggest-categories.feature`](../../features/suggest-categories.feature) had left open as
unsettled. No reasoning came with the choice. **What it does not change:** narrowing only filters
the **offers**. What is typed is still judged as typed, so "Groc" is still refused as an unknown name
and never completed to a suggestion (*Approved at the scenario gate*, below).

**How "contains" is judged.** Case is ignored, and a run of whitespace counts as one space, as in
the name rule (*A category name is compared case-insensitively*, above), so "vaste  l" still finds
Vaste lasten. The comparison is ordinal, so the machine's language plays no part. Nothing typed, or
only spaces, keeps every suggestion. What is left stays in alphabetical order.

**Built** in the presentation layer as `MoneyBudApp.SuggestionMatches` and `SuggestionsFor`, and
held by unit tests. The stakeholder did not ask for a scenario for it. It was first built as the
toolkit's own filter in the Desktop, out of every test's reach, and was moved the same day
([§11](11-risks-and-technical-debt.md), *Resolved*).

| Rejected | Why |
|---|---|
| **A pick-list alone** | It would put the recording and assigning routes back, and the unknown-name refusal, out of the user's reach. A UI meant to cover what the domain does would silently cover less |

> **The category box empties once an expense or an assignment has gone through. After a refusal it
> keeps what was typed**, like every other field.

Settled by the stakeholder at the first demo, 2026-09-26. In a follow-up question the same day he
confirmed that the box empties **only on success**. **Why**, in the documentation's reasoning: the
box now behaves like the rest of its form. Every other field already cleared after success and kept
its text after a refusal ([§8.4](08-crosscutting-concepts.md), *Decided while building*). A cleared
form shows that the entry went through, and a refusal is fixed in place rather than retyped. As
built, the expense and assign forms both kept their category after success. That was a choice made
during the build and never put to him, and this ruling replaces it. **What it does not touch:** the
assign form's period still follows the screen (*Defaults, and entering while another period is
shown*, below).

**A defect came with it, and it is not a rule.** At the demo he typed "groc", picked the suggestion
*Groceries* and recorded the expense, and the box went back to "groc". It showed what was typed, not
what was picked. The feedback does not say whether that expense was recorded against Groceries or
refused as "groc". If it was refused, the box was handing on the typed text instead of the picked
name, which would be worse than a display glitch. What a submitted "Groc" does stays as approved
(*Approved at the scenario gate*, below).

**What the build found.** The real window was run headless in a scratch harness, kept outside the
repository. Typing "groc" and picking *Groceries* with the keyboard **does** reach the form, and the
expense and the assignment are both recorded against Groceries. The exact symptom did **not**
reproduce, with the old code or the new, using keyboard picking. Picking with the mouse could not be
driven headlessly. So the demo expense was **most likely** recorded against Groceries. The reported
symptom is gone now that the box empties after success, and it stays empty after later acts. That is
as far as the evidence goes.

**Built** in `ExpenseForm` and `AssignForm`, and held by unit tests, not scenarios, because this is
form behaviour.

**There is no editing or deleting of a transaction.** That is not a separate decision. The domain
has neither, so a UI that covers what the domain does has neither. The consequence is worth
stating, because a user meets it on the first typo: an expense or income entered wrongly stays as
entered for the rest of the run. Nothing is kept when MoneyBud closes (*What the UI starts with, and
what it keeps*, below), so the only way to be rid of it is to close MoneyBud and lose everything else with it. A
wrong **plan** is different: assigning, negatively if need be, is how a *Budget* is changed, and that
is covered. **Accepted by the stakeholder for the demo** on 2026-09-25, with that consequence in
front of him. Correcting entries becomes its own later increment. Carried in
[§11](11-risks-and-technical-debt.md).

**Named at the first demo, and still later** (2026-09-26). The stakeholder listed deleting entries
as missing, alongside keeping data, and deferred both in the same breath: *"Maar dat komt later."*
That confirms the acceptance above rather than reopening it.

**Settled for the next increment on 2026-09-26, and built the same day.** Correcting entries was
taken up as its own increment, as the paragraph above said it would be, and its rulings are in *An
entry can be changed or removed* (above). **The paragraph above no longer describes MoneyBud.** A
wrong entry is corrected by clicking its row. It is kept as written, because what was accepted for
the demo, and why, is part of the record. Its other half, that nothing is kept when MoneyBud
closes, still holds, until the persistence increment is built (*What MoneyBud keeps*, below).

### Typing an amount

> **A comma or a point is the decimal mark, so "12,50" and "12.50" are both twelve fifty. No
> thousands separator is accepted.** Text that is not an amount is **refused before anything is
> recorded**, and the user is told so. A euro sign, a true minus sign ("−") and spaces around the
> number are tolerated.

Ruled by the stakeholder at the plan gate, 2026-09-25. **Why either mark**, in the documentation's
reasoning: Dutch writes a comma, and a point is just as easily typed. Refusing either would make the
user learn which one MoneyBud wants, for no gain. **Why no thousands separator**: with both marks
meaning "decimal", a separator could not be told apart from one.

> **A point or comma followed by exactly three digits ending in 0, such as "2.000" or "1,500", is
> refused as ambiguous**, and the refusal names both readings: *bedoel je 2000 of 2,00?*

Settled by the stakeholder on 2026-09-25, **after the spec review**, which found the problem.
**Why**:

- **"2.000" was being recorded as €2,00 without a word.** Under the rule above it reads as two
  euros.
- **Nothing downstream could catch it.** Both readings, two euros and two thousand, are whole
  cents, so the cent rule ([§8.2](08-crosscutting-concepts.md)) passes either.
- **MoneyBud itself shows thousands with a point**: "€ 2.000,00". A user who has seen that on screen
  has every reason to type "2.000" and mean two thousand.
- **An entry cannot be corrected** ([§11](11-risks-and-technical-debt.md)). A wrong amount stays
  wrong for the rest of the run.

**The last of these reasons went when corrections were built** (*An entry can be changed or
removed*, above, settled and built 2026-09-26). A wrong amount is now fixable. In the documentation's
reading the ruling does not rest on that reason alone. The first three still hold, and a wrong
amount recorded without a word still has to be noticed before it can be fixed. The ruling has not
been put back to the stakeholder.

**Why only when the three digits end in 0.** That is exactly the case where both readings are whole
cents. "1.832" is also a mark and three digits, but read as a decimal it is finer than a cent, so
**it goes on to be refused as finer than a cent**, which is a refusal the user sees. Only the case
that would otherwise pass silently is caught here. "2.00" and "2,5" are not ambiguous and are read
as decimals.

**Refused, not guessed.** It follows the distinction *An amount may be assigned negatively* (above)
draws: an ambiguous amount is **bad input**. It has two correct readings, not one, and MoneyBud
cannot tell which was meant. Naming both readings lets the user fix it in one step, because the form
keeps what was typed after a refusal.

> **Four or more digits after the mark that are all whole cents, such as "2.0000", "12,5000" or
> "7,00000", are not an amount.**

Ruled by the stakeholder on 2026-09-26, while the scenarios were being written. **Why:** it is the
same silent shape as "2.000". "2.0000" is whole cents, so the domain would record it as €2,00 without
a word, and MoneyBud never shows more than two decimals, so it is not a form the user has seen on
screen. **Why "not an amount" and not "ambiguous"**, in the documentation's reasoning: nobody writes
a thousands group of four digits, so there is no second reading to name. There is only a reading
that nothing downstream would catch.

**"All whole cents" means every digit after the second is 0.** A four-or-more tail that is **not**
whole cents is read as a decimal and refused by the cent rule, as "1.832" is: "12,3456", and also
"12,3450", which is 12.345. That second one ends in 0 but is finer than a cent, so the user sees the
cent refusal, not this one. **Past ten decimals the cent rule is never reached**: "12,34567890123" is
refused as **not an amount**, by the reader's ten-decimal limit, which exists so that the
conversion never rounds ([§8.2](08-crosscutting-concepts.md)). Both routes refuse it. Only what the
user is told differs.

> **A mark needs a digit before it and a digit after it.** ",50" and "12," are not amounts.

Ruled by the stakeholder on 2026-09-26, **chosen over reading ",50" as 0,50**. No reasoning came with
the choice. What it costs is small and visible: someone who types ",50" meaning fifty cents is
refused and types "0,50". Nothing is ever recorded as something other than what was meant.

**What reading an amount does not decide.** It never rounds, and it never judges a sign. "12,345" is
read as typed and refused by the cent rule. A minus is read, and whether it is allowed is the
domain's to say: an assignment takes it, and an expense refuses it. So the rules of *Transaction*
and *Assign* are untouched. Only text now comes before them
([§8.2](08-crosscutting-concepts.md)).

#### Approved at the scenario gate, 2026-09-26

The scenario writer made these choices in
[`type-an-amount.feature`](../../features/type-an-amount.feature). **The stakeholder approved the
scenarios with all of them on 2026-09-26**, so they stand as decisions. The reasoning is the
documentation's, kept because it is why they stand.

| Approved | What it rests on |
|---|---|
| **"−€ 50,00" is read as minus fifty, and "€ −50" is refused as not an amount** | "−€ 50,00" is exactly how MoneyBud shows a negative amount, minus sign first ([§8.2](08-crosscutting-concepts.md), *display formatting is fixed*), so a user copying what is on screen is understood. "€ −50" is a form MoneyBud never shows. The minus goes in front of everything, the euro sign in front of the number. **No scenario types "€ −50"**; a unit test in `AmountInputTests` holds the refusal. |
| **"0,100" is refused as ambiguous**, although almost nobody means one hundred by it | The rule is applied as written. An exception for the unlikely reading would be MoneyBud guessing after all, and the refusal names "0,10", so the fix is one step |
| **Text that cannot be read is refused for that, before the category is looked at** | The two layers of refusal run in a fixed order ([§8.2](08-crosscutting-concepts.md), [§6](06-runtime-view.md)). "abc" aimed at a name that is not a category is told it is not an amount, and is not told about the category |
| **Text that cannot be read brings no archived category back** | Bringing back is a side-effect of **recording** (*Recording an expense against an archived category brings it back*, above), and an expense refused for another reason already brings nothing back. Unreadable text records nothing |
| **Spaces only counts as empty**, and is refused as not an amount | Spaces around the number are tolerated. With no number, what is left is empty text |
| **"€ 2.000,00", MoneyBud's own display form, cannot be typed back in** | It follows from *no thousands separator is accepted* (above). The cost is that a figure copied off the screen is refused. That is accepted: it is refused, with a reason, not misread |

**Built** in the presentation layer as `AmountInput`, held by unit tests, and **specified by
[`type-an-amount.feature`](../../features/type-an-amount.feature)**: 14 scenario outlines, 73 cases,
covering expenses, incomes and assigning. Written at the stakeholder's request, **approved at the
scenario gate on 2026-09-26**, and bound and green. That closes the [§11](11-risks-and-technical-debt.md)
row saying no scenario held these rulings (*Resolved*).

### Stepping between periods

> **The UI steps back and forward from the current budget period**, to the previous and the next.

**Why.** A UI that showed only the current period would accept entries it could never show. Assigning
works in any later period, an expense can be back-dated into any earlier one, and an income can be
dated either way. Each lands in its own period, so each needs a way to reach that period. Nothing
settled bounds how far the stepping goes: assigning is allowed in future periods without limit, and
no rule bounds how far back an expense or income may be dated.

**This is where the display rule got built.** Which categories a period shows is already settled
(*When any category is shown in a period: the full rule*, above): every category with history
there, plus every category in use in the current and later periods, and in a past period only what
has history. Stepping was the first view that needed the whole rule. It is now one domain query,
`Ledger.CategoriesShownIn`, and the "shown" steps read the Overview built on it. The gap
[§8.1](08-crosscutting-concepts.md) and [§11](11-risks-and-technical-debt.md) used to record, a
settled rule with no implementation, is **closed**.

**Stepping clears the last thing MoneyBud said.** This was built that way and not put to the
stakeholder ([§8.4](08-crosscutting-concepts.md)).

#### No one-step way back to the current period

> **Stepping back and forward is the only way to move between periods.** There is no action that
> jumps straight back to the current period.

Settled by the stakeholder on 2026-09-25, over a *Huidige periode* action. *Huidige periode* stays in
*Dutch display terms* (below), because it still **labels** the current period when that period is on
screen. It is a label, not an action.

No reasoning came with the choice, and the documentation does not supply one. What it costs is
stated so it is not rediscovered: getting back from a period *n* steps away takes *n* steps.

#### Staying open across a period boundary

> **If MoneyBud stays open past a period boundary, the screen stays on the period it showed.**

Settled by the stakeholder on 2026-09-25, over following today into the new period.

**What follows.** The consequences below are the documentation's reading, not the stakeholder's
words. *Current* means the period today falls in, so once the boundary passes, the new period is
current and **the period on screen has become the previous one**. From then on:

- **Assigning in it is refused**, as in any past period (*Assigning happens in the current budget
  period and later ones, never in a past one*, above). It is still offered (*Defaults, and entering
  while another period is shown*, below), so the user meets the refusal rather than a missing action.
- **It is a past period for the display rule** (*When any category is shown in a period: the full
  rule*, above). It shows only categories with history there, so a category in use with no history
  in it **drops off the screen** the next time the view is drawn, without the user doing anything.
  This consequence was put in front of the stakeholder at the scenario gate, and he approved the
  scenarios with it (*Approved at the scenario gate*, below).
- **A new entry's date defaults to the new today**, so it lands in the new period, and the Overview
  says so (below).

> **Nothing is announced when a new period begins while MoneyBud is open.** The only thing that
> changes is the label of the period on screen: it stops being labelled as the current period
> (*Huidige periode*).

Settled by the stakeholder on 2026-09-25, over a short notice. **What it means**, in the
documentation's reading: the user can find out that the boundary has passed from the label, or from
one of the effects above. For example, an assignment in the period on screen is refused, with the
past-period reason. MoneyBud does not tell the user ahead of time.

**Why this needs no extra machinery.** This part is the documentation's reading of the code, not a
claim the stakeholder made. `Ledger.Today` reads the clock every time it is asked, and
`Ledger.CurrentPeriod` is worked out from it, so the ledger's idea of "current" moves at the
boundary by itself. The past-period refusal and the display rule both follow from that. What the
screen has to do is **hold the period it shows as a period**, and not as "the current one" or as an
offset from it. A view that held "current" would follow today into the new period, which is the
option not chosen. **Built that way**, as `MoneyBudApp.ShownPeriod`.

**"The next time the view is drawn" is at most a minute away.** While MoneyBud sits untouched, the
Desktop looks again once a minute, so for up to a minute after the boundary the label can still read
*Huidige periode*. Any act makes it look again at once. Nothing is announced either way, as ruled
above ([§8.4](08-crosscutting-concepts.md)).

### Defaults, and entering while another period is shown

> **An expense's or income's date defaults to today, whatever period is on screen. Assigning
> defaults to the period on screen.**

Settled by the stakeholder on 2026-09-25. **Why**, in reasoning that is the documentation's, not
his: an entry is recorded as it happens, and that is today. Stepping to look at another period does
not change when the money moved. Assigning is **planning**, and the period on screen is the plan
being worked on.

These are **defaults**, starting values that can be changed. An expense can still be back-dated, an
income dated either way, and an assignment made for another period. Where each one lands is decided
by its date or the period it names, never by what is on screen (*Stepping between periods*, above).

**Raised at the first demo and confirmed, not changed** (2026-09-26). The stakeholder stepped to
October to enter an income and found the date still defaulting to a day in September. For the demo
that was a nuisance. He then added that in real use he would have to set the date anyway, so it
matters less. The ruling above stands.

> **Assigning is offered while a past period is shown, and is refused with its reason.**

Settled by the stakeholder on 2026-09-25, over not offering it. **Why**, in the documentation's
reasoning: with assigning defaulting to the period on screen, this is **how the past-period refusal
is reached from the UI**, and the UI's scope includes that refusal (*It covers what the domain does,
and nothing more*, above). Not offering it would make the refusal unreachable. That is the same
argument that made category entry free text. The refusal's wording is copy and belongs to the UI
(*The UI is in Dutch*, below).

> **After an expense, an income or an assignment lands in a period other than the one on screen, the
> Overview stays on the period it showed, and says which period it went to.**

Settled by the stakeholder on 2026-09-25, over jumping to that period and over staying silent.
**Why**, in the documentation's reasoning: it fits MoneyBud telling the user the outcome of what
they did, which the category acts already do (*Archiving is announced, never confirmed*, above).
Staying silent would record something with nothing on screen changing, which looks the same as its
having failed. Jumping would take the user away from the period they chose to look at. The message
is copy, not a term.

**Assignments are covered too.** The first ruling spoke of an *entry*, and in this glossary an entry
is an expense or an income. An assignment can also land outside the period on screen, because the
period it names can be changed from its default. Asked, the stakeholder ruled the same day that it
is treated **the same as expenses and incomes**: the Overview stays where it was, and MoneyBud says
which period the assignment went to.

### The fields ask what before how much

> **Every entry form asks what the entry is before its amount.** Expense: *Omschrijving*,
> *Categorie*, *Bedrag*, *Datum*. Income: *Omschrijving*, *Bedrag*, *Datum*. Assigning:
> *Categorie*, *Bedrag*.

Settled by the stakeholder at the first demo, 2026-09-26. The three orders were his answer to a
follow-up question. **Why**, in his words (translated from English): he fills in *what* it is first
and then the amount, "because that is how I think about those things". As built, every form asked for
the amount first.

**What it does not change:** which fields there are, what each one accepts, and their defaults
(above). The assign form's period stepper was not part of the question.

**Extended on 2026-09-27, and built the same day:** the expense and income forms gain *Rekening* as their last
field, after *Datum* (*Every income and expense is on an account*, under *Accounts and net worth*,
below). The order above is unchanged.

This is window behaviour, so a unit test holds it, not scenarios. **Built** in the Desktop's
markup, `MainWindow.axaml`, and held by `WindowMarkupTests`, which reads that markup as text, the
way `TekstTests` reads this glossary. That was approved at the plan gate on 2026-09-26 as a small
departure from the Desktop having no automated tests
([§8.4](08-crosscutting-concepts.md), [ADR 0006](../decisions/0006-three-source-projects.md)).

### The period start day stays at the 1st, for now

> **In this increment every budget period starts on the 1st, and the UI does not offer to change
> it.** Deferred, not rejected. *Budget period*'s rule that the start day is configurable, and *A
> start day the month is too short for clamps to its last day*, both stand as they are. The UI simply
> does not expose the setting yet.

The stakeholder's ruling was conditional: *"Is the backend ready for this? If not no, if yes then
do."* Checked against the code, it is not ready:

- `Ledger` takes its `BudgetPeriodCalendar` when it is constructed and has no way to change it.
- **Budgets are stored against the first day of their period.** Change the start day once a budget
  exists, and that budget is attached to a period that no longer exists. Expenses and income do not
  have this problem: they carry their own date, and would fall into the new periods by themselves.
- No approved scenario configures a start day.

**What it would take** is a decision, not only code: what happens to existing budgets when the start
day changes. That question goes to the stakeholder when this comes back. The code half is carried in
[§11](11-risks-and-technical-debt.md).

### The overview, and its ring

> **The start screen is the *Overview*, headed by a *ring*: the radial diagram.**

The wish is [round 3](../stakeholder/2026-09-24-verdieping.md)'s: the start screen shows *"waar
mijn geld heen gaat — het radiale diagram, de verdeling over categorieën in één oogopslag"*. The same
document had already named it, earlier, as what "feedback" should mean: *"een radiaal diagram,
zoals dat ook in de app van Caleb Hammer gebruikt wordt"*. How the ring is drawn was settled on
2026-09-25:

| What the period holds | How the ring shows it |
|---|---|
| A category with a *Budget* above zero | **One slice, sized to its *Budget*, filled in as far as it has been spent.** The unfilled part is its *Remaining*. Exactly zero *Remaining* is a completely filled slice and is **not** over budget (*Over budget*, terms table) |
| *Unassigned* above zero | **A slice of its own** |
| A category over budget, with a *Budget* above zero | The slice **stays budget-sized**. It is drawn completely filled, **marked as over budget**, and its *Remaining* is shown as the **negative figure itself** ("Resterend: −€ 20") |
| A category with spending and a *Budget* of zero | **No slice.** Such a category is over budget from the first cent (*Over budget*), and it is **listed with the same marker** instead |
| A category with a *Budget* of zero and nothing spent | **No slice** |
| The period is *Over-assigned* | There is no *Unassigned* slice to draw. **The ring shows the budgets only**, and *Unassigned* is shown as the negative figure itself ("Niet toegewezen: −€ 20") **with the same marker** |
| **Neither income nor any *Budget*** in the period — the first start, for one | **An empty ring**: a grey outline with a hint that there is no income in this period yet |

**So, unless the period is *Over-assigned*, the whole ring is the period's income**: every
category's *Budget* plus what is still unassigned. That follows from the definition of *Unassigned*
(income minus every *Budget* in the period). It holds only because **archived categories' budgets
are slices too**. *Unassigned* subtracts them ([§8.1](08-crosscutting-concepts.md)), and an archived
category with a *Budget* above zero has history in that period, so the display rule shows it anyway.

*Revised at the first demo, 2026-09-26.* The ring is no longer drawn exactly in proportion. The
slices' figures still add up to the income, but a small slice is drawn wider than its share. See
*Every slice has a minimum width*, below. The wording above and in the table is left as the record of
the approved rule.

**The ring is empty only when there is neither income nor a *Budget*.** Settled by the stakeholder
on 2026-09-25. The two half-empty cases are not empty rings. Income with no budgets gives a ring that
is **all *Unassigned***. Budgets with no income is the *Over-assigned* case, a ring of budgets only.
The hint's wording is **copy, not a term**. The option he chose used *"Nog geen inkomsten in deze
periode"* as its example, and that is an example, not a fixed string. **Why an outline with a hint**
(the documentation's reasoning, not his): a blank space where the start screen's centrepiece should
be looks broken, and the hint says what fills it.

**"A *Budget* of zero" means zero, however it got there.** A category never assigned to and one
assigned to and then taken back to zero are drawn the same way, because there is no separate
"unbudgeted" state (*Budget*, terms table). The ring must not tell them apart.

**Which categories the overview lists at all is the display rule**, not the ring
(*Stepping between periods*, above). The ring decides only which of them get a slice.

**Why a slice sized to the plan and filled with the actual.** The stakeholder chose it over a ring
of what was spent and over a ring of what was budgeted. The reasoning recorded here is the
documentation's, not his. It puts the plan and the actual in one picture. *Remaining*, the one
figure where the two layers meet (*The second distinction*, above), becomes visible as the unfilled
part of each slice, with no separate figure to find.

**Why *Unassigned* gets a slice.** Without one the ring would show only money that already has a
job, and *Unassigned* is meant to be shown prominently rather than worked out (*Unassigned money is
something you can see*, above). With it, the ring answers "where does my income go" whole, the part
without a purpose included.

**Why an overspent slice does not grow.** The slice is the plan, and overspending does not change
the plan. A slice that grew would push the others smaller and would stop the ring adding up to the
income. The overspend is **marked** instead, which fits *MoneyBud shows, it never blocks*.

**Why a zero budget gets no slice, and spending against it gets listed.** A slice of zero size cannot
be seen. But spending that nothing on screen shows is the failure *Where an archived category is
still shown* (above) exists to prevent: the period would stop adding up to what the user knows they
spent. So it is listed, with the marker.

**Why an over-assigned ring shows budgets only.** The stakeholder took this over drawing the ring at
income size with an overflowing segment. The reasoning recorded here is the documentation's, not
his. Budgets-only keeps every category slice sized the same way in every state, and shows
over-assignment with the marker an overspent category already uses, rather than as a second kind of
drawing.

#### Every slice has a minimum width

> **Every slice is drawn at least a minimum width, the *Unassigned* slice included, so that a small
> budget stays visible, and with it room to see its fill.** The ring is then no longer exactly in
> proportion.

**This is a revision, made by the stakeholder at the first demo, 2026-09-26.** It is not an
inference from what was already written. His complaint was that small budgets were barely visible in
the ring, let alone whether anything had been spent against them. He chose a minimum width over
keeping the ring exact and over putting names beside the ring. In a follow-up question the same day
he ruled that the minimum applies to **every** slice, *Unassigned* included.

**What changed.** Until now every slice was drawn exactly in proportion to its size in euro, so the
ring's angles added up to the income as exactly as its figures did. *The overview, and its ring*
(above) says so, and that wording is left there with a pointer here, because what was approved is
part of the record. Now a slice below the minimum is drawn at the minimum, and the others give way.
The ring still **shows** the period's income, but no longer **measures** it exactly.

**What did not change:**

- **Which slices exist.** A category with a *Budget* of zero still gets no slice. The minimum widens
  slices that exist. It creates none.
- **Their order** (*The order of categories and slices*, below).
- **How a slice is filled.** It is filled as far as its own *Budget* has been spent, so the fill is a
  fraction of the slice as drawn. That fraction stays exact (next).
- **The marker** (*One marker for over budget and over-assigned*, below).
- **A slice's size as a figure**, in the documentation's reading. It is still its *Budget*, and unless
  the period is *Over-assigned* the figures still add up to the income. Only the drawing departs
  from them, which is what [§8.2](08-crosscutting-concepts.md) already says the ring's shares are:
  drawing, not amounts.
- **An overspent slice still does not grow.** That argument rests on the slice being the plan, and
  still holds. The half of it about the ring adding up exactly is weakened by this revision.

**Why**, beyond his complaint, in the documentation's reasoning: the ring exists for legibility
(quality goal 1, [§1](01-introduction-and-goals.md)). An exact ring that hides a budget fails that
goal, and a ring slightly out of proportion does not. The exact figures are still in the rows, and
now at the slice itself (*Hovering a slice shows its figures*, below).

> **The fill stays exact. There is no minimum fill.** The minimum slice width is the only place the
> ring departs from exact.

Ruled by the stakeholder on 2026-09-26, in a follow-up question raised by the scenario writer. He
chose this over drawing any spending above zero as at least a visible sliver. **The cost, stated so
it is not rediscovered:** a tiny amount spent against a slice can still look like nothing spent.
Hovering the slice shows the exact figure (*Hovering a slice shows its figures*, below), and so does
the row. So the minimum width answers "I cannot see this budget", and it does not promise that any
spending at all is visible.

**Left for the plan to propose, not open questions:** how wide the minimum is, and what happens when
minimums squeeze the other slices, for instance when many small slices together would need more of
the ring than there is. Both are now settled (below).

**A requirement on the squeeze rule, from the documentation and not from the stakeholder:** whatever
the plan proposes, **a larger *Budget* must never be drawn narrower than a smaller one**. Slices run
largest *Budget* first (*The order of categories and slices*, below), so a ring whose widths broke
that order would look wrong. The *Unassigned* slice sits outside that order, because it is always
last, so whether the requirement also compares it with the category slices was left for the plan.

> **The minimum is 2% of the ring (7.2°). A slice whose share falls below it is drawn at 2%, and
> the other slices share what is left in proportion to their sizes. This repeats until no slice
> falls below. *Unassigned* counts like any other slice.** When the minimum cannot fit every slice,
> at 50 slices or more, every slice is drawn equally wide.

Proposed in the plan and **approved by the stakeholder at the plan gate, 2026-09-26**. **What it
guarantees**, which is why it was proposed: a larger size is never drawn narrower than a smaller
one, equal sizes are drawn equally wide, and the shares still close the circle. That meets the
requirement above. Counting *Unassigned* like any other slice answers the question the requirement
left open: it **is** compared with the category slices. The two reasons that follow are the
documentation's. **Why repeat:** giving way can push another
slice below the minimum, and one pass would leave it there. **Why equal widths at 50 or more:** at
2% each there is no room for anything else, and equal widths are the one answer that still keeps
the order.

**Built** as `Ring.MinimumSweep` and `Ring.Sweeps` in the presentation layer. The Desktop's
`RingControl` used to have a hidden minimum of its own, a floor on how thin it would draw a slice.
That has been **removed**, so the width is decided in one place and tests reach it. Scenarios in
[`overview.feature`](../../features/overview.feature) hold the minimum for one-cent slices, the
*Unassigned* slice included, and the exact fill. The squeeze and the 50-slice case are held by unit
tests only, and have no scenario.

**It changes an approved scenario file.** [`overview.feature`](../../features/overview.feature)
says in its header that the whole ring is the period's income, and in its reading notes that "the
ring draws each slice in proportion to the ring's total". It went back through the scenario gate:
both were reworded, three scenarios were added for the minimum width, and hovering got its own
file, [`point-at-a-slice.feature`](../../features/point-at-a-slice.feature). **The stakeholder
approved them at the scenario gate on 2026-09-26.**

#### Hovering a slice shows its figures

> **Pointing at a category slice shows its category, *Budget*, *Uitgegeven* and *Resterend*.
> Pointing at the *Unassigned* slice shows its name and figure**, as in "Niet toegewezen: € 350,00".
> Every slice answers a hover.

Settled by the stakeholder at the first demo, 2026-09-26, over clicking to select the category and
over clicking to zoom. The *Unassigned* half was ruled in a follow-up question the same day. It
answers his complaint that the ring "doet niets", does nothing. No reasoning came with the choice
of hovering over clicking.

> **Pointing at a slice tells you everything its row does.** An over-budget slice shows *Resterend*
> as the negative figure itself, with the marker. An archived category's slice shows the
> *Gearchiveerd* caption.

Ruled by the stakeholder on 2026-09-26, in two follow-up questions raised by the scenario writer
(*What each category row shows*, below; *Where an archived category is still shown*, above). The
over-budget half was first recorded here as the documentation's reading, and it is now his ruling.
**Why**, in the documentation's reasoning: a hover that showed less than the row would make the
slice and the row disagree about the same category. The hover is also where the exact figures are,
because the fill may hide a tiny amount spent (*Every slice has a minimum width*, above).

**What follows without a ruling:** a category with spending and a *Budget* of zero has no slice, so it
has nothing to hover, and it is still listed with the marker. An *Over-assigned* period has no
*Unassigned* slice to hover.

**Extended for the backing increment on 2026-09-27, and built the same day:** a backed category's slice also shows
*Opgebouwd*, as its row does (*On screen: Staat op and Opgebouwd*, under *Backing and Accumulated*,
below).

**What the hover shows is decided in the presentation layer**, as the rows are. The Desktop only
shows it ([§8.4](08-crosscutting-concepts.md), [§11](11-risks-and-technical-debt.md)). Every word
in it is already in *Dutch display terms* (below), and no word was added.

> **The figures appear in the ring's hole, not in a tooltip. With nothing pointed at, the hole shows
> *Niet toegewezen* and its figure, with the marker when the period is over-assigned. An empty ring
> shows its hint there. The slice pointed at is drawn highlighted. Stepping to another period points
> at nothing.**

Proposed in the plan and **approved by the stakeholder at the plan gate, 2026-09-26**. It settles
where the *Unassigned* figure went when the ring took over the middle column (*The Overview's
layout*, below). **What follows**, in the documentation's reading of the build: pointing at the
*Unassigned* slice shows in the hole what the hole shows anyway, its name and figure, so the
highlight is what makes that slice visibly answer. Because the hole shows *Unassigned* whenever
nothing else is pointed at, an over-assigned period's figure and marker still have a place, although
there is no *Unassigned* slice. The figures are read afresh on every act, so a slice pointed at never
shows stale figures, and stepping never leaves a slice from another period pointed at.

**Built.** The presentation layer decides everything: `Ring.SliceAt` finds the slice at a share of
the ring, `RingSlice.Row` is a category slice's row, and `MoneyBudApp.PointAt`, `PointedSlice`,
`PointedRow` and `RingCentreShowsUnassigned` are what the window binds to. The Desktop only turns
the pointer's position into a share. Specified by
[`point-at-a-slice.feature`](../../features/point-at-a-slice.feature), with `PointingTests` for the
centre and for stepping.

#### One marker for over budget and over-assigned

> ***Over budget* and *Over-assigned* are shown with the same marker, beside the negative figure
> itself.** "Resterend: −€ 20" with the marker, and "Niet toegewezen: −€ 20" with the marker. The
> figure is **not** turned into a positive "over by" amount.

**This is a revision, made by the stakeholder on 2026-09-25.** It is not an inference from what was
already written. Before it, *Assigning may overdraw the pool account* and *Over-assigned* (both
above) said these states are shown as "a plain negative figure, unremarked". For *Over budget* and
*Over-assigned* that is no longer true: they are **marked**. The old wording is left in those
sections, each with a note pointing here, because what was believed is part of the record.

**What did not change.** The marker is **information only**: never a warning, never a question and
never a block. MoneyBud still shows these states and never enforces anything about them (*Shown,
never enforced*, above). The figure stays the negative number it is, so the marker adds to the
figure and replaces nothing.

**Why one marker for both.** The reasoning recorded here is the documentation's, not his. The two
are members of one family, a figure below zero that is shown and never blocked (*Over-assigned*,
above), and one marker keeps them one thing to learn.

> **"The same marker" means the same look. Each badge names its own state**: *Over budget* beside
> a category's *Resterend*, and *Te veel toegewezen* beside *Niet toegewezen*.

As built, the marker is one badge style with the state's display term on it. Asked after the review
whether two wordings still count as "the same marker", the stakeholder **confirmed it does**
(2026-09-25). **Why it is right**, in the documentation's reasoning: the look carries "this figure is
below zero, and that is allowed", which is what the two states share, and the word says which of the
two it is, which a user would otherwise have to work out from where the badge sits. On the ring, an
overspent slice is marked by an outer edge in the marker's colour. The scenarios assert that the two
states carry the same marker. The look itself is the Desktop's, and stays out of the scenarios.

**It does not cover *Overdrawn*.** *Overdrawn* is the third member of the family, a negative account
*Balance*. There are no accounts, so it is not in this increment, and **this revision says nothing
about how it is shown.** *Assigning may overdraw the pool account* (above) had decided that an
overspent budget and an overdrawn account share one display despite their different severities.
Whether an overdraft now gets this marker too, or whether that decision is reopened, is **not
settled**. It is met when accounts are built.

**Answered on 2026-09-27, and built the same day:** it gets this marker, with its own badge, *Rood* (ruled the same day)
(*An overdrawn account carries the marker*, under *Accounts and net worth*, below).

**What is fixed is what is drawn, what is marked, and in what order** (*The order of categories
and slices*, below), plus the three-part arrangement of the screen (*The Overview's layout*, below).
Since the first demo, three more things are fixed: the ring's place as the middle column's
centrepiece, a minimum slice width, and what a hover shows (above). Colours, the marker's form and
the rest of the layout are not fixed here, and the scenarios stay declarative (`features/README.md`).

#### What each category row shows

> **Each category row on the Overview shows its *Budget*, *Spent* and *Remaining*, and the marker
> when it is over budget.**

Settled by the stakeholder on 2026-09-25, over showing *Remaining* only. **Why**, in the
documentation's reasoning: the slice already draws all three figures (its size is the *Budget*, its
fill is what was *Spent*, and the unfilled part is *Remaining*), but nobody can read an amount off a
slice. The row states in figures what the slice draws. *Remaining* alone would not say whether a
small figure means a small plan or a plan mostly spent.

The rule holds for **every listed category**, not only the ones with a slice. A category with
spending and a *Budget* of zero shows 0, what was spent, and the negative *Remaining*, with the
marker. One with a zero *Budget* and nothing spent shows 0, 0 and 0. On screen the three are
*Budget*, *Uitgegeven* and *Resterend* (*Dutch display terms*, below).

**Extended for the backing increment on 2026-09-27, and built the same day:** every row also has a *Staat op*
list beside *Hernoemen*, and a **backed** row shows *"Opgebouwd: € 600,00"* under its figures (*On
screen: Staat op and Opgebouwd*, under *Backing and Accumulated*, below).

#### The order of categories and slices

> **Categories are listed, and the ring's slices drawn, largest *Budget* first. Equal budgets keep
> the order the categories were added in**, so every category with a *Budget* of zero follows the
> budgeted ones, in the order they were added. **The *Unassigned* slice is always last, going
> clockwise**, and the ring runs clockwise in this order.

Settled by the stakeholder on 2026-09-25. He took largest-first over order added and over
alphabetical; order added over alphabetical for ties; and *Unassigned* always last over sorting it
by size and over putting it first. "Last" means **last going clockwise**, which is his own
specification.

**Why**, in reasoning that is the documentation's, not his: largest first puts the biggest parts of
the plan where the eye starts. Order added is a tie-break that does not change unless the user adds
something, and it does not depend on how a name is spelled or capitalised. A fixed place for
*Unassigned* means it is found in the same spot whatever its size.

**A consequence, stated neutrally:** the order changes as you assign. A category moves up or down
the list, and its slice around the ring, whenever its *Budget* passes another's.

**What the rule does and does not reach:**

- **"Equal" means equal figures, however they got there.** A *Budget* never assigned and one
  assigned and then taken back to zero are both zero and tie. They are ordered by when the category
  was added, never by `Ledger.HasBudget` (*"A Budget of zero" means zero*, above).
- **The six default categories count as added in the order the stakeholder listed them**:
  Boodschappen, Huur, Hobby, Sparen, Verzekeringen, Abonnementen. **Settled** by the stakeholder on
  2026-09-25. It follows from his order-added ruling and from the list being his own
  (*The default categories*, above). At a first start every *Budget* is zero, so this is the order
  they appear in. `Ledger.StartNew` adds them in this order, so the code already matches. The code
  follows the ruling, not the other way round.
- **When the period is *Over-assigned*** there is no *Unassigned* slice, so no slice is placed last
  by this rule.
- **A category brought back keeps its original place in "order added".** It was first recorded as
  the documentation's reading of the code, then **confirmed by the stakeholder** on 2026-09-25.
  Bringing a category back does not add it again: it comes back "history and all", and its place
  among the categories is part of that. The code already does this: `Ledger` keeps its categories
  in the order they were added, and bringing one back does not append it.
- **Not fixed:** where around the ring the first slice starts, and where the *Unassigned* figure
  sits among the rows. The rule places the *Unassigned* **slice**, and says nothing about rows.
- **Extended while an earlier plan is offered** (scenario gate, 2026-09-26; built). The rows are
  then ordered by their grey plan figure instead, largest first, ties in order added, a row without
  one counting as zero, so nothing jumps when the plan is taken over. The ring is unaffected, because
  while the offer stands no category has a slice (*Opening a period*, above).

### A period's entries are listed newest first

> **A period's expenses and its incomes are two separate lists. Each is listed newest date first,
> and entries on the same date appear newest-recorded first within their own list.**

Settled by the stakeholder on 2026-09-25: newest first over oldest first, and two lists
("definitely") over one. **Why newest first**, in the documentation's reasoning: the newest entry is
the one the user most likely just made and wants to check. The tie-break is needed because a date
has no time of day. Without it, the order of a day's entries would be undefined, and could differ
from one showing to the next.

**Two lists means no recording order is needed across them.** `Ledger` keeps its expenses in one
list and its incomes in another, each in the order they were recorded. That is exactly what
"newest-recorded first within its own list" needs, so the rule needs no new state.

### The Overview's layout: income, plan, expenses

> **The Overview is laid out with income on the left, the plan in the middle (the ring and the
> category rows), and expenses on the right.**

This is the stakeholder's **wish**, given on 2026-09-25 alongside the two-lists ruling: *"I almost
see it as left side income, middle budget/plan, right side expenses."* It is **presentation**, so
it is fixed here and **not** in the scenarios, which stay declarative (`features/README.md`).

**Why it fits**, in reasoning that is the documentation's, not his: the layout follows the model.
Income is the pool the plan is made from. The plan is the middle layer. Expenses are the *actual*
layer, and the plan and the actual meet in *Remaining* (*The second distinction: plan and actual*,
above). Read left to right, the screen goes from where money comes from, to what it is meant for, to
where it went.

**What it does not fix:** proportions, spacing, colours, and anything about how the three parts
behave in a narrow window. Those stay open, as *One marker for over budget and over-assigned*
(above) says of layout in general.

> **The ring is the centrepiece of the middle column. It fills most of the column, with the category
> rows below it.**

Settled by the stakeholder at the first demo, 2026-09-26, answering a follow-up question about how
large the ring should be. His complaint was that the ring was too small. He had pictured it much
larger, and small budgets could hardly be seen in it. This **refines** the layout above. It does not
replace it: the plan is still in the middle. Within that column the ring now comes first and takes
most of the room. As first built, it sat at a fixed size beside the *Unassigned* figure and the
assign form. Where those two go now was not part of the question, and was left for the plan.

> **The ring takes most of the middle column's height and grows with the window. Below it come the
> assign form, then the category rows, which scroll, then adding a category. The *Unassigned* figure
> sits in the ring's hole.**

Proposed in the plan and **approved by the stakeholder at the plan gate, 2026-09-26**. It settles
both places left open: the assign form went below the ring, and the *Unassigned* figure went into the
hole (*Hovering a slice shows its figures*, above). **Built** in the Desktop's markup. The ring was
first built at a fixed 340 px, and `spec-reviewer` found that it did not grow. It now grows with the
window.

This is presentation, so it stays out of the scenarios. It works together with the minimum slice
width (*Every slice has a minimum width*, above): a larger ring and a minimum width both answer
the same complaint.

**Refined on 2026-09-27, and built the same day:** a strip of accounts and net worth runs across the top, above
the three columns, the same in every period (*The accounts strip, and an account's history*, under
*Accounts and net worth*, below). The three columns stay as they are.

### Approved at the scenario gate

The items below were written up here, while this increment's scenarios were being written, as
**assumptions** the scenario writer had made and one **consequence** of a ruling already taken. They
waited on the scenario gate. **The stakeholder approved the scenarios with all of them on
2026-09-25**, so they now stand as decisions. The reasoning is kept, because it is why they stand.
One of them was later refined (the alphabetical row).

| Approved | What it rests on |
|---|---|
| **An expense row shows its category**, alongside its date, label and amount | An expense's category is what says what it is, which is why its label is optional (*Income carries a label, and it is required*, above). A list without it would lose that. An income has no category, so its row has none |
| **With spending but neither income nor any *Budget*, the ring is empty and the spending is listed with the marker** | It combines two rows of the ring table in *The overview, and its ring*: the ring is empty when there is neither income nor a *Budget*, and spending against a *Budget* of zero gets no slice and is listed with the marker. Spending does not appear in the first condition, so it does not stop the ring being empty. The hint that there is no income yet stays true |
| **A partial name such as "Groc", submitted as typed, is judged as typed**: refused as an unknown category, not silently completed to a suggestion | The category box accepts any text, and suggestions are offers (*Category entry is free text with suggestions*, above). Completing a name silently would record against a category the user did not name. Narrowing the suggestions as you type, settled after the review, changes nothing here |
| **Suggestions are alphabetical, with case ignored** | Approved as "the same ordinal comparison as category names". **Refined after the review** to the invariant culture's order, case ignored, so that an accented first letter sorts among its letter (*Category entry is free text with suggestions*, above) |
| **Consequence: once a period boundary passes while MoneyBud is open, in-use categories with no history in the period on screen drop off it** the next time the view is drawn, without the user doing anything | *Staying open across a period boundary* (above): the screen stays on the period it showed, which has become a past period, and the display rule shows a past period only what has history in it. Not a problem to solve. It is a consequence of two settled rules acting together, and he approved the scenarios with it in front of him |

### What the UI starts with, and what it keeps

> **The UI starts as a first start does: the six default categories (`Ledger.StartNew`) and nothing
> else. Everything entered is lost when MoneyBud closes.**

The stakeholder chose this over starting with synthetic demo data and over saving to a file. The
reasoning recorded here is the documentation's, not his:

- **[§8.3](08-crosscutting-concepts.md) stands unchanged.** Its trigger for storing anything is the
  first time the stakeholder is asked to re-enter data he would mind re-entering. That has not
  happened. The demo is still a demo, and the argument of
  [ADR 0002](../decisions/0002-desktop-application-first.md) still holds.
- **A first start is already specified**, by *A new MoneyBud starts with the six default
  categories* in [`add-category.feature`](../../features/add-category.feature). Demo data would be
  a second starting state, one that no scenario specifies.

**This increment is where §8.3's trigger first becomes reachable.** Until now nobody could enter
anything, so nobody could mind losing it. From here on, a request to keep data is the signal §8.3
is waiting for.

**Built that way.** The Desktop starts every run with `Ledger.StartNew`, and keeps nothing.

**Named at the first demo, and the trigger did not fire** (2026-09-26). The stakeholder listed
keeping data as missing, together with deleting entries, and deferred both in the same breath:
*"Maar dat komt later."* §8.3's trigger is the first time he is asked to re-enter data **he would
mind re-entering**. He did not say he minded re-entering anything. He said storage is missing, and
put it later himself. The paragraph above calls "a request to keep data" the signal, which is
looser than §8.3's wording. This remark shows why the looser wording is not enough: it names the
gap without asking for it to be filled. §8.3's wording is the one that counts.

**The second half is superseded, and the first half stands** (2026-09-26, built the same day). The
stakeholder chose to have MoneyBud keep its data, as the next increment (*What MoneyBud keeps*,
below). **§8.3's trigger still has not fired.** Keeping data is his choice of what to build next,
not a response to having to re-enter anything. That increment is built, so "everything entered is
lost when MoneyBud closes" is no longer true. The first half does not change: when there is no data
yet, MoneyBud starts with the six default categories and nothing else. This section is kept as it
was written, because what was decided for the UI, and why, is part of the record.

### The UI is in Dutch

> **Everything MoneyBud shows is in Dutch**, in the terms of *Dutch display terms* (below), which the
> stakeholder approved.

**Why.** What the app displays is user-facing content, and its one user is Dutch. That is the same
reasoning that made the default categories Dutch (*They are Dutch because they are content*,
above). [§2](02-architecture-constraints.md)'s English constraint covers what this project's authors
write, not what the app shows.

**Refusals stay reasons in the domain. Their Dutch text belongs to the UI.** Nothing new was decided
here. It is what [§8.1](08-crosscutting-concepts.md) already set up: refusals are reasons rather than
messages so that a UI can word them without the domain changing. The display terms fix **terms**,
not sentences. The sentence that tells a user a refusal, a shortfall or a bring-back is copy, and
belongs to the UI.

### The toolkit

It is **Avalonia**, chosen at the plan gate on 2026-09-25 and recorded in
[ADR 0005](../decisions/0005-avalonia-ui-toolkit.md). It was not chosen here, and nothing in this
section depends on it. This section first read *The toolkit is not chosen here*, and that stayed
true: the requirements were settled first, and the toolkit was picked to serve them. The scenarios
for this increment are declarative and toolkit-free, as `features/README.md` requires, and they run
against a layer with no toolkit in it ([ADR 0006](../decisions/0006-three-source-projects.md)).

## What MoneyBud keeps

The persistence increment's rulings, settled with the stakeholder on 2026-09-26. They were answers
to multiple-choice questions, each put to him with a recommendation. Like the follow-ups of
2026-09-24, they went straight into this glossary rather than into a new interview round. **He took
the recommended option unless a ruling below says otherwise.** Where a reason is his, it is given as
his. Where it is only this documentation's reading, it says so.

**All of it is built**, in the persistence increment, on 2026-09-26. How the data is stored was not a
question for the stakeholder, and none of these rulings answers it: the form storage takes, how
amounts are written, and how an entry and a category keep their identity across a restart were left
to the plan. The plan answered them, and [ADR 0007](../decisions/0007-keeping-the-ledger.md) records
them: one JSON file, whole cents as integers, entry ids kept and categories keyed in the file
([§8.3](08-crosscutting-concepts.md), *Answered by the plan*). Where a ruling below left a detail to
the plan, what was built is said beside it, marked **Built**.

### Why now: demo now, real soon

> **MoneyBud keeps its data so that it does not have to be re-entered between sessions. It is still
> a demo.** The stakeholder expects to switch to real use not long after keeping data is built.

**This is his choice, not [§8.3](08-crosscutting-concepts.md)'s trigger firing.** That trigger is
the first time he is asked to re-enter data he would mind re-entering, and it has not happened. The
order was agreed the same day: correcting things first, deliberately, because while nothing is kept
closing MoneyBud discards every mistake, and once data is kept an uncorrectable typo is permanent.
Keeping data comes next.

**What stays as it was.** The data is still demo data, so every argument that rests on it being
throwaway still holds for now (*Demo data may not survive a new version*, below). Three things this
documentation says expire "when the demo stops being a demo" do **not** expire with this increment:

- reopening desktop-first ([ADR 0002](../decisions/0002-desktop-application-first.md));
- the sweep's money having nowhere to go while there are no accounts;
- the period start day that cannot change once budgets exist.

As first written up, all three came due at the **switch to real use** instead, and the switch was
called **foreseeable**, where before this ruling it had been hypothetical.

**Softened the same day** (*Real use before accounts*, below). Asked whether real use starting
before accounts was acceptable, the stakeholder found the question beside the point: he will try
MoneyBud out before accounts exist, and does not mind his saved data being deleted when they arrive.
So the switch to real use is **not a line this increment or opening a period has to plan around**,
and his data stays throwaway at least up to and including the accounts increment. The three things
above still wait for the demo to stop being a demo. Nothing about them is due in the meantime
([§11](11-risks-and-technical-debt.md)).

### Everything is kept, for good

> **MoneyBud keeps everything, as one continuous history, for as long as the user has it.** There
> is no fresh start per year.

Chosen over a fresh start each year. **Why:** a budget period ends but never closes (*Ending versus
closing a budget period*, above), so the model has no yearly boundary, and a yearly cut would create
one it has not got. Settled rules already reach across periods and need the history to be there: an
archived category is shown in every period where it has history (*Where an archived category is
still shown*), and a new period remembers the last one's figures (*Budgets carry over as figures*).

**For demo data, "for good" is qualified** by *Demo data may not survive a new version* (below).

> **"Everything" is the ledger: categories, archived or not, budgets, expenses and incomes.** What is
> on screen is not kept: the period shown, an entry half typed, a question waiting for an answer, a
> rename in progress. **So MoneyBud always opens on the current period.**

A follow-up question, the same day. The ruling above had not said whether "everything" includes
what is on screen, and this settles it.

### Saved by itself, after every change

> **MoneyBud saves automatically after every change. There is no save button.**

**Why:** a forgotten save loses entries, and a save button is one more act for every entry, which
is the friction quality goal 2 is about ([§1.2](01-introduction-and-goals.md)).

### When a save fails, MoneyBud says so and keeps going

> **If a change cannot be saved (a full disk, a locked file, a profile that is not there), MoneyBud
> says the change was not saved and lets the user carry on.** Every later change tries again to save
> everything. **Closing MoneyBud before a save has succeeded loses the changes that were not saved.**

A follow-up question, the same day. **Rejected:** undoing the change that could not be saved, and
refusing further entries until a save succeeds. The loss on closing was **accepted with that
consequence in front of him**. In the documentation's reading, this is "MoneyBud shows, it never
blocks" applied to saving. The change stands in the ledger and on screen, the failure is shown, and
nothing is refused because of it. Because every save writes everything, one save that succeeds
catches up every change that failed before it.

> **The "not saved" notice stays on screen until a later save succeeds.** It is a lasting state,
> not a one-off message that the next notice replaces.

A further follow-up, the same day. **Rejected:** saying it once, like any other message. **Why**,
his: otherwise the next message would hide it, and the user could close MoneyBud without knowing
that changes were unsaved.

> **Closing MoneyBud with unsaved changes just closes.** There is no question first. The unsaved
> changes are lost, as the ruling above already accepted.

A further follow-up, the same day. **Rejected:** asking before closing. **Why:** the lasting notice
is the warning. MoneyBud shows, it never blocks, and that holds for closing as well.

**Left for the plan and the scenarios, not settled here:** how the lasting notice relates to the
screen's rule that a question and a notice are never shown together (*Chosen in the build, not put to
the stakeholder*, above; [§8.4](08-crosscutting-concepts.md)). That rule was made for one message bar
holding one thing at a time. A notice that has to stay while other notices and the removal question
come and go does not fit it as it stands, and how the two fit together is for the plan and the
scenarios to work out.

**Settled the same day, while the scenarios were written** (the rulings below, 2026-09-26, since
built). The paragraph above is kept because it is why they were asked. **Built:** the plan made room
with a **save line**, a line of its own beside the notice and the question, rather than by changing
the one-message rule ([§8.4](08-crosscutting-concepts.md), *Keeping the ledger*).

> **The "not saved" notice is shown beside any other notice and beside the removal question.**
> Stepping between periods does not clear it.

**Rejected:** other messages displacing it for a while. **Why**, in the documentation's reading: it
is the same reason the notice lasts at all. Anything that hides it, even briefly, is a moment in
which the user could close without knowing. **The one-message rule yields to it.** That rule was a
choice made in the build, not a ruling, and it still holds between the removal question and an
ordinary notice. Only the "not saved" state stands beside either (*Chosen in the build, not put to
the stakeholder*, above).

> **What retries a save: every change, and MoneyBud itself, now and then.** So the notice goes soon
> after the problem is fixed, without the user doing anything.

This **refines** "every later change tries again" above. **Rejected:** retrying only on real
changes, which was that ruling's literal reading: a user who had fixed the problem would see the
notice until he next changed something. How often MoneyBud retries was left to the plan. **Built:
once a minute**, on the timer the Desktop already ran for the *Huidige periode* label. It retries
only while something is unsaved. A change that alters the ledger retries as well. A refusal, or an
act that changes nothing, does not, because it is not a change.

> **When saving succeeds again after a failure, MoneyBud says so once**, with a short notice that
> everything is saved again.

**Rejected:** the "not saved" notice simply disappearing. **Why**, in the documentation's reading: a
state the user has been shown should be seen to end, not just be gone.

**Built, and a clarification of "a short notice".** The recovery is said **on the save line**, the
same line that said "not saved", and **not as an ordinary notice**: *"Alles is weer opgeslagen."* It
stays until the next act, the removal question being asked, or a step to another period. Because it is on the save line, it too stands
beside the removal question, which it would do when the minute's retry works while a question is
waiting. So the one-message rule is untouched: it still holds between the question and ordinary
notices, and neither line of the save line is an ordinary notice. This paragraph's first write-up
called the recovery "a short notice" and implied the one-message rule applied to it. The approved
plan put it on the save line, and `spec-reviewer` found the two disagreeing. The build is right and
this text was wrong. The ruling itself, said once, is unchanged.

> **Closing makes one last attempt to save.** If it works, nothing is lost. If it fails, MoneyBud
> closes without asking, as ruled above.

**Rejected:** no last attempt. The ruling that closing asks nothing stands. **Built:** closing the
window calls `MoneyBudApp.Close`, which tries once if something is unsaved, and then lets go of the
data.

> **Saving that works says nothing.** Only a failure is announced, and the recovery after one.

**Rejected:** a brief confirmation for every save. **Why**, in the documentation's reading: saving
happens after every change, so a confirmation would follow every act, and a message that always
appears stops being read.

### An interrupted save never damages the previous one

> **If a save is interrupted, by a crash or a power cut, the previous save is left intact.** At worst
> the change being saved is lost.

A follow-up question, 2026-09-26. **Rejected:** no guarantee. **Why:** there are no backups
(*Backing up is the user's business*, below), so a half-written save could otherwise make the whole
history unreadable. How this is guaranteed was left to the plan. **Built:** a save is written to a
temporary file, flushed to the disk, and only then renamed over the data file. The data file is
never written in place. A temporary file left by a save that was cut off is never read, and the next
save writes over it ([ADR 0007](../decisions/0007-keeping-the-ledger.md)).

> **After an interrupted save, the next start opens normally and says nothing.** The change that
> was being saved may be missing, and nothing mentions it.

A follow-up question, 2026-09-26. **Rejected:** saying that the last change may not have been kept.
**Why**, as put to him and accepted: the user saw the crash, and MoneyBud could not say which change
is missing anyway. It matches closing without a word after a last save attempt has failed (*When a
save fails, MoneyBud says so and keeps going*, above).

### In a fixed place, never in the repository

> **The data lives in a fixed place in the user's profile.** The user never chooses where. It is
> always outside the repository.

**Why:** the repository is public, and real data must never land in it
([§2](02-architecture-constraints.md)). **Rejected:** a file the user opens and saves wherever he
likes. In the documentation's reading, a fixed place is what makes "never in the repository" a
property rather than a habit: a location the user picks, or one taken relative to where MoneyBud is
run from, could land inside a working copy of the repository.

Which folder in the profile was a plan question. **Built:** the user's **local** application data,
`%LOCALAPPDATA%\MoneyBud` on Windows, found through the profile and never relative to the working
directory. Local rather than roaming, because a file rewritten after every change should not be
copied between machines ([ADR 0007](../decisions/0007-keeping-the-ledger.md), [§7](07-deployment-view.md)).

### When the data cannot be read, MoneyBud says so and touches nothing

> **If the data is damaged, or was written by a newer version of MoneyBud, MoneyBud says it cannot
> read it and leaves it exactly as it is.** It does not start with empty data instead.

**Rejected:** starting empty. **Why:** saving is automatic, so an empty start would save over the
history at the first change. In the documentation's reading there is a second reason: data that
cannot be read may still be recoverable, by the user or by a later version, but only while nothing
has written over it.

> **Having said so, MoneyBud closes.** Nothing can be entered, so nothing typed can be lost unsaved.

A follow-up question, the same day. **Rejected:** opening with nothing that can be entered, and
opening usable but saving nothing. **Why**, his: nothing can be entered, so nothing typed can be
lost unsaved. MoneyBud cannot save while it leaves the data untouched, so anything entered would
only be lost at closing.

**What the message says** is ruled under *Where the data is, is written in the README* (below):
only that the data cannot be read.

> **Kept data that is there but blank, with nothing at all in it, counts as unreadable**: MoneyBud
> says so, touches nothing and closes. **A saved budget that is empty**, with no categories and
> nothing recorded, **is valid**, and loads as no categories.

A follow-up question, 2026-09-26, and a second one the same day that settled what "empty" meant.
**Rejected:** treating blank data as a first start. **Why:** MoneyBud never writes a **blank** save,
so blank data means something went wrong, and treating it as a first start would save over it. It
does write an **empty budget**, after the last category with no history is deleted, and that save
has to load as it was (*A first start is unchanged*, below). The first write-up of this ruling said
"empty" for both. The documentation raised the clash, and the stakeholder confirmed the distinction.
Only **no kept data at all** is a first start.

**Built:** a file that is not a whole document in MoneyBud's format and version, or that holds data
breaking a rule the ledger keeps, is refused as unreadable, blank included. MoneyBud shows *"MoneyBud
kan je gegevens niet lezen. Er is niets aan veranderd."* in a small window, and closes when it is
closed. The file is not written to ([§8.3](08-crosscutting-concepts.md)).

> ***Reworded*, 2026-09-27.** Meeting it for real after the accounts build (data from before accounts),
> the stakeholder found the message confusing: it said neither why nor what "niets" was. Asked what it
> should say, he answered that if MoneyBud can tell the reasons apart it should explain, and if it
> cannot, it should just say the save file is wrong. **It cannot**: every reason — damaged, blank, a
> newer or older version, a folder out of reach — arrives as one "cannot read". So it is one sentence
> naming the reasons there can be, still naming no place: *"MoneyBud kan je opgeslagen gegevens niet openen. Het bestand is beschadigd, niet bereikbaar of gemaakt door een andere versie van MoneyBud. MoneyBud heeft het niet gewijzigd."* The ruling that
> the message points nowhere stands. Telling the reasons apart is possible, and was left for if it is
> wanted.

> **"Touches nothing" means the data file.** Making the folder if it is missing, and the lock file
> beside the data, is MoneyBud's own bookkeeping, and acceptable.

Confirmed by the stakeholder on 2026-09-26, after the review. The build claims the data before it
loads it, and the claim makes the folder and `moneybud.lock`, so both can appear on a start that
then says it cannot read the data. The documentation raised whether that squares with "touches
nothing". **Rejected:** creating nothing at all. **Why:** the data would have to be checked before
the lock was taken, and in between two MoneyBuds could start at once, which *Only one MoneyBud at a
time* (below) rules out.

### When the data's folder cannot be reached

> **If MoneyBud cannot reach the folder its data lives in at all, it does what it does for data it
> cannot read**: it says it cannot read the data, touches nothing, and closes. That covers a profile
> that is not there, a folder MoneyBud is not allowed to open, and a *file* named MoneyBud standing
> where the folder should be.

Put to the stakeholder on 2026-09-26, during the review of the persistence increment, and ruled the
same day. The rulings above covered data that is there and cannot be read, and data that is not
there at all, which is a first start. A folder that cannot be reached is neither, and the build had
to do something with it. **Rejected:** starting empty and showing "not saved". **Why:** if the real
data came back while MoneyBud was open, a profile mounted late or a permission restored, the first
save that worked would write the empty start over it. That is the same danger that rules out
starting empty on unreadable data, reached by another road. The message is the same one, and still
points nowhere.

**Built:** the store reports the folder as unreachable when it cannot be made or opened, or when it
is not a full path, and starting turns that into the unreadable-data message. It was ruled after
`start-moneybud.feature` was approved, so **the stakeholder approved adding one row, `cannot be
reached`, to that file's approved "cannot read" outline** (2026-09-26), and the file's header says
so. The row makes the folder unreachable for real and checks the data file byte for byte afterwards
([§8.4](08-crosscutting-concepts.md)).

### Backing up is the user's business

> **MoneyBud keeps one set of data and makes no copies of it.** Backing it up is up to the user,
> outside MoneyBud.

He was offered keeping earlier copies, and declined. **No recommendation was made on this
question.** No reason came with the choice. The cost, in the documentation's reading: a file lost
or damaged, with no backup of the user's own, is the whole history gone
([§11](11-risks-and-technical-debt.md)).

### One set of data, and no way to reset it in MoneyBud

> **There is one set of data. MoneyBud has no act for starting over.** Starting over means deleting
> the file yourself. The next start is then a first start (below).

**Rejected:** a start-over button, and separate budgets side by side, such as a play budget beside
the real one.

### Only one MoneyBud at a time

> **Starting MoneyBud while it is already open is refused.** The second start says MoneyBud is
> already open, and closes.

A follow-up question, the same day. **Rejected:** letting both run. **Why:** two windows saving one
set of data would overwrite each other's changes. **Built:** the second start says *"MoneyBud is al
geopend."* and closes. The first holds a lock on the data from before it reads it until it closes,
and a crash lets go of it, so a crash never blocks the next start
([ADR 0007](../decisions/0007-keeping-the-ledger.md)).

### The login is the protection

> **No password and no encryption. The Windows login is enough.**

**Why:** the data is local (quality goal 4, [§1.2](01-introduction-and-goals.md)), and a forgotten
password would lock the user out of his own data for good. The ruling names **Windows** because that
is the only system MoneyBud has run on ([§7](07-deployment-view.md)).

### Demo data may not survive a new version

> **Until real use starts, a new version of MoneyBud may be unable to read demo data saved by an
> older one.** It then says it cannot read it and touches nothing, as above, and the user deletes
> the file and starts fresh. **Carrying data from one version to the next becomes a requirement at
> the switch to real use**, and not before.

A follow-up question, the same day. **Rejected:** carrying the data over from the first file ever
saved. **Why**, in the documentation's reasoning: until the switch there is no real data, and
building a way to carry demo data forward pays to preserve something nobody would mind losing. It
is the argument [ADR 0003](../decisions/0003-money-representation.md) makes against a currency field
("paying now to avoid a migration of nothing is paying for nothing"), and it rests on the same fact:
the first version is a demo to react to, not an MVP ([§1.1](01-introduction-and-goals.md)).

**What it means for the build.** Until the switch, the way data is stored can change between
versions without anything carrying the old data across. From the switch on, it cannot.

**First exercised by the accounts increment** (ruled and built 2026-09-27): the version with
accounts will not read data saved without them (*Saved data from before accounts*, under *Accounts
and net worth*, below).

**Extended the same day** (*Real use before accounts*, next): saved data need not survive into the
version that adds accounts either. So the stored form may change freely **at least up to and
including the accounts increment**, whatever the stakeholder is doing with MoneyBud by then.

**Exercised a second time by the backing increment** (2026-09-27). The version with backing writes
format version 3 and **does not read version 2**, the accounts version's data. This time it was not
covered by the extension above, which ended with accounts, so it was put to the stakeholder at the
plan gate. The plan recommended reading version 2, because it was cheap: version 2 has no backing
and no movements, so it reads as "nothing backed, settled through today". He answered: *"Chose what
is best for you. I dont mind starting over"*. **The build chose to refuse it**, like version 1.
Every older format read is one more way in that must be kept correct, and nothing would be kept that
he minds losing. So data saved by the accounts version meets the ruled response: MoneyBud says it
cannot read it, touches nothing and closes, and the user deletes the file
([ADR 0009](../decisions/0009-movements-are-entries.md)). **The ruling above still stands as
written**: carrying data across versions becomes a requirement at the switch to real use, and not
before.

### Real use before accounts

Asked whether real use starting before accounts exist was acceptable, given that until then
leftover and unassigned money is carried nowhere at a period's end, the stakeholder answered:

> *"I dont really see how this is relevant. I'll likely try it out a bit before accounts are added
> but if the saves need to be deleted when accounts get added i dont mind"*

A follow-up question, 2026-09-26, answered in English. **What it settles:** his data may be
dropped **at least up to and including the accounts increment**. That extends *Demo data may not
survive a new version* (above) past the point this documentation had assumed. **What it does not
change:** the acceptance that the sweep's money vanishes while there are no accounts stands as it
was. He will try MoneyBud out, and what he enters is still throwaway enough to delete.

He found the question beside the point, and it is recorded that way. The documentation had raised
it because it had tied several deadlines to "the switch to real use"
([§11](11-risks-and-technical-debt.md)). His answer means none of them needs planning around yet.

**Applied on 2026-09-27**: asked again for the accounts increment, he took the recommendation that
saved data is not carried over (*Saved data from before accounts*, under *Accounts and net worth*,
below).

### Where the data is, is written in the README

> **The data's location is documented in the README, and only there.** MoneyBud itself does not show
> it: not on screen, and not in the message that it cannot read the data.

A follow-up question, the same day. **Why it is documented at all:** backing up is the user's job
(above), and a file he cannot find is one he cannot back up. **The recommendation was that MoneyBud
show it**, on screen and always when it cannot read the file. He declined both parts. So the message
that the data cannot be read does not have to name where it is. A user who meets that message and
wants to start fresh (*Demo data may not survive a new version*) finds the file through the README.

> **The message that the data cannot be read points nowhere.** It names no path and does not refer
> to the README. It says only that the data cannot be read.

A follow-up question, the same day, which **confirms the literal reading** of the ruling above.
**Rejected:** pointing to the README, and naming the path in this message alone.

### A first start is unchanged

> **When there is no data yet, MoneyBud starts as it does today:** the six default categories and
> nothing else.

**Not asked in this round.** It is carried over unchanged from *What the UI starts with, and what it
keeps* (above) and from *A new MoneyBud starts with the six default categories* in
[`add-category.feature`](../../features/add-category.feature). "No data yet" covers both the very
first start and a start after the user has deleted the file (*One set of data*, above).

> **The default categories come only with a first start: no kept data at all.** A user who deletes
> all six and starts MoneyBud again still has no categories.

A follow-up question, 2026-09-26. **Rejected:** adding the defaults again whenever there are no
categories. In the documentation's reading, kept data with no categories is the user's own doing,
and handing back what he deleted would undo it.

**Where this meets the blank-data ruling, confirmed by the stakeholder** (2026-09-26). Deleting all
six defaults with no history leaves a budget with nothing in it, and MoneyBud saves it after that
change. That save is valid and loads as no categories. Only a **blank** save, with nothing at all
in it, is unreadable (*When the data cannot be read*, above).

**Refined on 2026-09-27, and built the same day.** A first start will also come with one account,
*Betaalrekening*, with no starting balance, as the pool account (*A first start has one account*, under *Accounts
and net worth*, below). The heading above is left as written.

## Accounts and net worth

The rulings for the accounts increment, settled with the stakeholder on 2026-09-27. Like *Opening a
period* and *What MoneyBud keeps*, they were answers to multiple-choice questions, each put to him
with a recommendation, and they went straight into this glossary rather than into a new interview
round. **He took the recommended option every time.** **The reasoning given with each ruling is the
documentation's.** It was offered with the recommended option as the argument for it, and he chose
that option without adding reasons of his own. The one framing that is his is recorded as his
(*Transfers*, below).

**One ruling took a second asking.** The first question about transfers confused him, and he tied
it to backing and the sweep. It was reworded around an ATM withdrawal, and he then took the
recommendation.

**Seven points were derived rather than asked.** They were stated to him alongside the rulings, not
put as questions. **One more, the same-day tie-break for a balance correction, is the
documentation's own** and was not put to him. Each is marked *derived*. They stand as the
documentation's reading and are **open to contradiction at the scenario gate**, like the derivations
of earlier increments. Where this section reads a ruling's wording further, it says so ("in the
documentation's reading").

**Seven more points were found while this section was first written up**, and listed as open. **Four
went back to the stakeholder the same day, 2026-09-27**, each as a multiple-choice question with a
recommendation, and **he took the recommendation every time**. Each sits in the subsection it
belongs to, marked *follow-up*. **The other three were handled by the documentation without asking
him**: one is deferred to the backing increment as a question for its first stage, one is recorded
as usage rather than a rule, and one is a derivation. Each is marked where it sits. *What this
section leaves open* (below) now says where each of the seven went.

**Seven more came from the scenario writer the same day**, each put to the stakeholder with a
recommendation, which he took every time. They are marked *follow-up* too. For those seven, where a
rejected option's reason is not his, it is the documentation's, drawn from the reason for the option
he chose.

**One term was renamed the same day.** The balance one is ***Balance correction***, not
*Correction*, so that it does not collide with this glossary's older use of "correction" for
changing or removing an entry (*Balance correction*, *Terms*, above).

**Settled, specified and built, all on 2026-09-27.** This section was first written when nothing
existed but the rulings, and it said so: "no feature file, no plan and no code". Then:

- **Specified.** The scenarios were approved at the scenario gate, with one more ruling and nine
  choices of the scenario writer (*Approved at the scenario gate, 2026-09-27*, below).
- **Planned.** The plan was approved at the plan gate, and brought
  [ADR 0008](../decisions/0008-balance-is-worked-out.md): no balance is stored anywhere, a typed
  balance is a dated entry, and all four entry kinds share one id counter, which is the recording
  order ([§9](09-architecture-decisions.md), [§8.3](08-crosscutting-concepts.md)).
- **Built** on branch `increment-9-accounts` and green. `spec-reviewer` found no faked scenario, one
  vacuous scenario and some defects, all fixed.

What the build chose where the rulings are silent is under *Accounts: chosen in the build, not put
to the stakeholder* (below). Where the text below says "will", it was written before the build and
is left as written.

> **Every income and expense is on an account, and each account's *Balance* is worked out from what
> is on it**, starting from the last balance the user typed for it, or from zero if he has typed
> none. **Net worth**, *Vermogen*, is the sum of the balances, shown with every account in a strip
> across the top of the Overview. Money is moved between accounts by a **transfer**. A balance that
> has gone wrong is put right by typing the real one, which MoneyBud records as a **balance
> correction**. **No category is backed yet**: assigning still moves no money.

An example, with synthetic figures. A first start has one account, Betaalrekening, with no starting
balance, and it is the pool account. The user corrects it to €1.000, the bank's figure today. He adds Contant with a
starting balance of €40 and Spaarrekening with €5.000, and *Vermogen* reads €6.040. None of that
€6.040 is in any period's *Niet toegewezen*. He records €25 at the market against Boodschappen,
picking Contant in the account field. Contant reads €15, *Vermogen* €6.015, and Boodschappen's
*Resterend* falls by €25, as it always did. He withdraws €50 at an ATM, a transfer from
Betaalrekening to Contant. Betaalrekening reads €950 and Contant €65, and *Vermogen* and every budget
figure stay as they were.

### What this increment covers, and what waits

> **Accounts and net worth only.** Accounts with balances, every income and expense on an account,
> net worth on screen, and transfers between accounts. **No backing**: assigning to *Sparen* still
> moves nothing, and there is no *Accumulated*.

| Not in this increment | When |
|---|---|
| **Account-backed categories** and ***Accumulated*** | The next increment |
| **The sweep** | The increment after that. Its destination must be backed |
| **Archiving an account** | Deferred until missed (*Managing accounts*, below) |
| **Account kinds** | None, *derived* (*Managing accounts*, below) |
| **Recurring transactions** | A later increment, as before |

**The sweep's money still vanishes at a period end** until the sweep is built, as accepted
(*Real use before accounts*, above; [§11](11-risks-and-technical-debt.md)). Accounts give it somewhere
real to go, but nothing sends it there yet.

### A balance is worked out from the entries, never stored as a free number

> **An account's *Balance* is worked out from what is on it. It is never kept as a number of its
> own.** When it is wrong, the user types the real balance, and MoneyBud records a **balance
> correction**.

**Why.** A balance worked out from its entries can always be explained by them. Every figure on the
purpose side is already worked out rather than stored (*Remaining*, *Unassigned*, whether a period
has a plan), so the location side now works the same way. And a mistake is put right where it was
made: an expense on the wrong account is one entry to re-point, not two balances to unpick
([§11](11-risks-and-technical-debt.md) had noted this as a point in this option's favour).

| Rejected | Why |
|---|---|
| **A stored balance, overwritten by hand** | After an overwrite, nothing explains the number |
| **Entered and calculated balances side by side** | The busiest screen of the three, with two figures for every account |

**This answers the question [§11](11-risks-and-technical-debt.md) asked to have decided before
accounts are built**: whether a hand edit is an overwrite or an adjustment entry, and whether a
balance is stored or derived from its transactions. It is an adjustment, and derived.

**It keeps round 2's wish, in another form.** Asked whether a balance is typed in or calculated, the
stakeholder answered *"Allebei. Je kunt het saldo zelf vrij bijwerken, en daarnaast kunnen we zoveel
automatische berekeningen toevoegen als we tijd voor hebben"*
([round 2](../stakeholder/2026-09-24-verdieping.md)). He can still set a balance to whatever he
likes. What that does has changed: it records a balance correction beside the entries, rather than
overwriting a number. The *Balance* row of *Terms* (above) is revised to match.

### A typed balance is what the bank said that day

> **A balance the user types is the real balance on the day it is typed, so every entry dated before
> that day is already in it.** Entries dated after it move the balance as normal. **The same holds
> for every balance correction**, and a starting balance the user types is the account's first
> balance correction.

The stakeholder's example. Today he adds Betaalrekening at €1.000, and then records last Tuesday's
€50 of groceries. The balance stays **€1.000**. The €50 left the account before today, and the €1.000
the bank shows already reflects it. The groceries still count against Boodschappen in Tuesday's
period, as any expense does. (A first start's Betaalrekening already exists, *A first start has one
account*, below. Correcting it to €1.000 does the same.)

**Why.** A checked balance stays checked. Everything is entered by hand, so late receipts are the
ordinary case (*Ending versus closing a budget period*, above), and each one would otherwise knock a
balance he had just made right off again.

| Rejected | Why |
|---|---|
| **A typed balance is only a starting figure, which every entry moves whatever its date** (€950 in the example) | Every late receipt would knock a checked balance off again |

> ***Derived:* an entry dated on the balance correction's own day is in it if it was recorded before
> the balance correction, and moves the balance if it was recorded after.**

Add the account in the morning, record a €30 lunch after it, and the balance falls by €30. **Why**, in
the documentation's reasoning: a date has no time of day, so "before the balance was typed" can only
be told on the same day by the order things were recorded. Not asked.

> ***Derived:* a changed entry keeps the moment it was first recorded.** So whether it was recorded
> before or after a balance correction is decided by when it was **first** recorded, never by when
> it was changed, even when the change moves its date onto or past the balance correction's day.

Handled by the documentation on 2026-09-27, not put to the stakeholder. **Why:** a change overwrites
an entry in place, and it keeps its id and its place in the lists (*A change overwrites the entry*,
above). Its recording moment is part of that place. Were a change to count as a new recording, the
same edit would move a balance or not depending on whether it was made before or after a balance
correction, which nothing on screen would explain.

> ***Derived:* a typed starting balance and a balance correction are dated today**, the day they are
> typed.

So the balance to type is today's, not the one on last week's statement: entries recorded since that
statement's date would be taken as already in it.

**What this costs, stated plainly.**

- **Changing or re-pointing an entry dated before an account's latest balance correction does not
  change that account's balance.** The balance correction has already taken it in. That weakens
  [§11](11-risks-and-technical-debt.md)'s "one entry to re-point" for any entry the user has
  corrected past. In the documentation's reading it is usually the right outcome: the corrected
  balance is the bank's, and an old entry on the wrong account did not make it wrong. Re-pointing
  still moves the balance of the account the entry goes to, when that one has no later balance
  correction.
- **An expense dated before a balance correction lowers neither the balance nor net worth.** The
  balance correction already did. It counts on the purpose side as any expense does.
- **A transfer across a balance correction can change net worth**, and that is ruled to be true
  (*Transfers*, below).

### A starting balance or a balance correction is net worth only

> **A starting balance or a balance correction changes the account's *Balance*, and so net worth,
> and nothing else.** It is not income, it counts in no period's *Unassigned*, and it changes no
> budget figure. It is money the user already had.

| Rejected | Why |
|---|---|
| **Count it as income** | Adding a savings account would suddenly give thousands to assign, and a downward balance correction would need a category, as an expense does |

> ***Derived:* a starting balance or a balance correction may be negative**, for an account already
> overdrawn, **or zero.** The cent rules apply ([§8.2](08-crosscutting-concepts.md)).

**In this increment such money is seen by location only.** *The central distinction* (above)
promises that net worth and the budget are one set of data seen two ways, and a starting balance or
a balance correction is seen only one way. **Deferred to the backing increment**, where it first has
consequences, as a question for that increment's first stage (*A question for the backing
increment*, below). Handled by the documentation on 2026-09-27, not put to the stakeholder, and not
a ruling.

> ***Derived, as usage:* interest is recorded as an income if the user wants to budget it, and a
> change in an investment's value as a balance correction.**

Handled by the documentation on 2026-09-27, not put to the stakeholder. **This is not a system
rule**, because MoneyBud decides nothing here: both acts exist, and which one the user takes is his.
Round 2 says investments are, for now, *"alleen wat ik zelf invul"*
([round 2](../stakeholder/2026-09-24-verdieping.md)). What follows from each, so the choice is
informed: an income joins its period's *Unassigned* and can be assigned; a balance correction
reaches net worth only.

### A first start has one account

> **A first start comes with one account, *Betaalrekening*, at €0,00, and it is the pool account.**
> The user adds the rest.

**Why.** It is the approach the default categories take: something that works at once, without
guessing at what the user has. Every income and expense is on an account, so a start with none could
record nothing until an account was added.

| Rejected | Why |
|---|---|
| **Several default accounts** | Guesses about his accounts |
| **None** | Nothing could be recorded until an account was added. It would be the first thing MoneyBud ever blocks |

**It is content, so it is Dutch**, like the six default categories (*They are Dutch because they are
content*, above). It refines *A first start is unchanged* (above): a first start is the six
categories and this account.

> ***Follow-up*, 2026-09-27: the first start's Betaalrekening has no starting balance.** Its balance
> is the plain sum of what is on it, whatever the dates, until the user first corrects it. **Only a
> balance the user actually types takes in earlier entries.**

So "at €0,00" above is a sum of nothing, not a typed figure. Record last week's salary and groceries
on the first day, and both move it. **Why:** *A typed balance is what the bank said that day* rests on
the user having checked the figure, and nobody checked this one. **It refines the derivation that a
starting balance is the account's first balance correction**: that holds for an account the user
adds with a balance he types, and the first-start account simply has none.

| Rejected | Why |
|---|---|
| **A starting balance of €0 on the first start's account** | Back-dated entries on the first day would then do nothing to its balance |

### Every income and expense is on an account

> **On the expense and income forms, the account is the last field, after *Datum*: a list to choose
> from, pre-filled with the pool account.** It is not free text.

The forms then ask: expense *Omschrijving*, *Categorie*, *Bedrag*, *Datum*, *Rekening*; income
*Omschrijving*, *Bedrag*, *Datum*, *Rekening*. That extends *The fields ask what before how much*
(above) without reordering it.

**Why last.** The stakeholder's order, the what before the amount, stays. The account is the field
left as it is most often.

**Why a list.** The category box is free text so that an archived category can be brought back by
typing its name, and so that an unknown name reaches its refusal (*Category entry is free text with
suggestions*, above). Neither reason reaches accounts. No account is archived, and an unknown name
could only be refused. **Typing an unknown name creates nothing in either field**: an expense naming
a category you do not have is refused (*Recording an expense against an archived category brings it
back*, above). So the difference is how the field is filled, not what it can create.

| Rejected | Why |
|---|---|
| **Hidden until asked for** | It makes a cash expense even easier to get wrong: the default would be taken without being seen |
| **The first field** | It goes against "the what before the amount" |

**The pre-fill is the pool account for every expense in this increment**, because no category is
backed. Once backing exists, a backed category's expense defaults to its default backing account
instead (*An expense defaults to the pool account*, above).

> **An income or expense row on the Overview names its account only when that is not the pool
> account**, in small grey text, so a cash expense stands out.

| Rejected | Why |
|---|---|
| **Always** | The pool account's name would repeat on nearly every row |
| **Never** | The lists would not show where any money went |

**What it does not do, stated plainly.** It shows a cash expense recorded *on* Contant. A cash
expense wrongly left on the pool account looks like every other row, so it does not catch the weak
spot of *An expense defaults to the pool account*. **Derived** from the ruling's wording: a row reads
the pool account as it is when shown, so after another account is made the pool, rows on the old
pool gain its name and rows on the new one lose theirs.

### The accounts strip, and an account's history

> **Accounts and net worth are a strip across the top of the Overview, above the three columns.**
> Each account shows its *Balance*, and *Vermogen* comes at the end. The strip is where an account is
> added, a balance corrected and a transfer made. **It is the same in every period**, because
> balances are about **today**, not about the period on screen.

**Why.** Net worth is a point-in-time figure (*The central distinction*, above). Above the columns
that step with the period, nothing suggests the strip belongs to one.

| Rejected | Why |
|---|---|
| **Under the income column** | It reads as if it belonged to the period on screen |
| **A separate screen** | Offered without an argument of its own. In the documentation's reading, it would put one of MoneyBud's two questions off the screen it opens on ([§1.1](01-introduction-and-goals.md)) |

This **refines** *The Overview's layout* (above): income, plan and expenses stay as they are, under
the strip.

> ***Follow-up*, 2026-09-27: accounts are ordered pool account first, then in the order they were
> added**, in the strip and in the forms' account list alike.

Raised by the scenario writer. **Why:** the default sits where you look for it, and nothing jumps
when balances change. When another account is made the pool, it moves to the front and the one it
replaced goes back to its place in the order added, in the documentation's reading. *Vermogen* stays
at the end of the strip.

| Rejected | Why |
|---|---|
| **Strictly the order added** | The pool account, the default for every entry, could sit anywhere |
| **Alphabetical** | The default would sit wherever its name falls |

> **Clicking an account in the strip opens its history, newest first**: its starting balance, its
> balance corrections, its transfers, and the incomes and expenses on it. **Transfers are changed or
> removed there, and a balance correction is removed there.** The Overview's income and expense lists
> stay incomes and expenses only.

| Rejected | Why |
|---|---|
| **A third list on the Overview, for the period's transfers and balance corrections** | They would be tied to the period on screen |

**In the documentation's reading**, for the reason that list was rejected, the history covers every
period, not the one on screen. Where it opens is for the plan. Whether an income or expense can also
be changed from it is for the scenario stage.

> ***Follow-up*, 2026-09-27: incomes and expenses are changed and removed only from the Overview's
> lists, never from an account's history.** The history is where transfers are changed or removed and
> balance corrections removed.

Raised by the scenario writer, and it settles the point the paragraph above left to the scenario
stage. **Why:** it is smaller, with one place per kind of record. An income or expense in the history
is there to be seen, as part of what makes up the balance.

| Rejected | Why |
|---|---|
| **Both routes** | Two places to change the same entry, with a form around each |

> ***Follow-up*, 2026-09-27: a balance correction in the history shows the new balance and the
> difference it made**, for example *"Correctie — saldo € 1.000,00 (− € 23,40)"*.

**Why:** a balance correction is the only trace of something forgotten. Once it takes in the entries
dated before it, a missing expense stops showing anywhere on the location side, and the difference is
what is left of it. That recovers, in part, the "report the difference" option in the first row of
[§11](11-risks-and-technical-debt.md). The wording of the example is copy; the difference is the
ruling.

| Rejected | Why |
|---|---|
| **The new balance only** | The one trace of what was forgotten would be lost |

> ***Follow-up*, 2026-09-27: the difference is recomputed, not fixed.** It always means **what is
> still unexplained**: the typed balance minus what the previous balance correction and the entries
> this one takes in would give **now**.

The stakeholder's example. MoneyBud had worked out €1.023,40, and he corrected Betaalrekening to
€1.000, so the history shows (− € 23,40). He then finds the receipt and records the forgotten €23,40
expense, dated before that day. The difference now shows **€ 0,00**. **The balance stays €1.000
either way**, because the balance correction took the expense in (*A typed balance is what the bank
said that day*, above). Only the explanation of it changed.

**Why:** as forgotten entries are found, the difference shrinks, and at €0,00 you know you have found
everything. That makes the difference a **working** answer to "report the difference", in the first
row of [§11](11-risks-and-technical-debt.md), and not only a trace.

**This overturns the documentation's own reading**, written up earlier the same day and not put to
him until now: that the difference is fixed at the moment of typing, and that a later change to an
entry the balance correction took in does not alter it. It was the rejected option.

| Rejected | Why |
|---|---|
| **Fixed at the moment of typing** (the documentation's earlier reading) | A found receipt would leave the difference unchanged, so it could never show that everything has been found |

> ***Follow-up*, 2026-09-27: a starting balance's history row shows no difference**, for example
> *"Startsaldo — € 1.000,00"*.

Raised by the scenario writer. **Why:** there is nothing it corrected. It is the first figure the
account has, so there is no worked-out balance before it to differ from. The wording is copy.

| Rejected | Why |
|---|---|
| **Show the whole amount as a difference** | It would read as € 1.000,00 unexplained, as if something had been forgotten |

### Transfers

> **A transfer, *Overboeking*, moves an amount from one account to another, on a date.** It moves
> both balances and leaves net worth and every budget figure unchanged. It can be changed or removed
> like an entry. **It may not be dated in the future**, like an expense.

**The framing is the stakeholder's own**, given when the question was put the second time, around
an ATM withdrawal: money leaves Betaalrekening, arrives in Contant, and nothing is spent. **Backing
and the sweep will need money to move between accounts. MoneyBud will make those moves by itself,
and a transfer is the same kind of movement, made by the user.** So a transfer is not a side feature.
It is the movement the next two increments are built from.

| Rejected | Why |
|---|---|
| **Correct both balances by hand** | A balance correction should mean "MoneyBud was wrong". Here nothing was wrong: money moved |
| **No transfers until backing** | Offered without an argument of its own. In the documentation's reading, an ATM withdrawal could then only be entered as the two balance corrections above |

> ***Follow-up*, 2026-09-27: a transfer across a balance correction may change net worth, and that
> is true, not a flaw.** Each account follows its own balance corrections, and net worth is their
> sum. So *"leaves net worth unchanged"* above holds **except where a balance correction has already
> counted one side**.

The example. After correcting Betaalrekening, the user records an ATM withdrawal of €50 dated the
day before. Betaalrekening's balance correction already has it taken off. Contant has no balance
correction since, so it rises by €50, and net worth with it. **Why that is right:** the cash really is
in the wallet, and the checked bank balance already had it taken off. Before the transfer was
recorded, net worth was €50 short, because the balance correction had dropped money that had gone
into the wallet. The transfer puts it back.

| Rejected | Why |
|---|---|
| **Force a transfer to move both balances or neither** | One of the two would then disagree with a balance the user checked |

**Why not in the future.** A transfer reports money that has moved. An income may be dated in the
future only because nothing else in the model plans income (*Income may be dated in the future; an
expense may not*, above), and that reason does not reach a transfer.

| Rejected | Why |
|---|---|
| **Future-dated, like an income** | The reason income may be does not reach transfers |

***Derived:***

- A transfer needs **two different accounts** and an amount **above zero**, under the same cent rules
  ([§8.2](08-crosscutting-concepts.md)). Its direction is carried by *from* and *to*, as an income's
  and an expense's is by what they are. With only one account, no transfer can be made.
- It is **not an income or an expense**. It has no category, needs no label, and counts in no
  period's *Unassigned*. **Whether it has a label at all is for the scenario stage.** Settled
  there: it has none (*Approved at the scenario gate, 2026-09-27*, below).
- In the documentation's wording, *Transaction* (*Terms*, above) stays the word for income and
  expenses, so that its two stated differences stay the only two. A transfer is a movement of its
  own.
- "Changed or removed like an entry" is read to mean: **removing one asks first**, by the principle
  "confirm only where a record is lost" (*Removing an entry asks first*, above), and **a change is
  judged as if the transfer were recorded now** (*A changed entry is judged as if it were recorded
  now*, above).

### An overdrawn account carries the marker

> **An account whose *Balance* is below zero is shown with the same marker as *Over budget* and
> *Over-assigned*, with a badge of its own.** Never blocked, never warned about.

**This answers the question left open on 2026-09-25** (*Assigning may overdraw the pool account* and
*One marker for over budget and over-assigned*, above): whether an overdraft gets the marker, now
that the other two have it. It does, which keeps the decision that one display covers both
severities. Each badge names its own state, and this one's was first proposed as ***Rood***, from
*rood staan*, before it had been put to the stakeholder (*Proposed display terms*, below). It has
since been ruled (follow-up, below).

| Rejected | Why |
|---|---|
| **A plain negative figure** | It would be the only negative state without a marker |
| **A stronger look** | It cries wolf when a balance is only out of date, as *Assigning may overdraw the pool account* argued |

> ***Follow-up*, 2026-09-27: the badge reads *Rood*.** Ruled, no longer only proposed.

**Rejected:** *Rood staan* and *Negatief saldo*. No reason for either was recorded with the answer.
In the documentation's reading, *Rood* is short like the other two badges and is the everyday word
for an account in the red, while *Negatief saldo* would only repeat what the figure beside it shows.

**Reachable in this increment** by an expense or a transfer out of an account that has not got the
money, and by a negative starting balance or balance correction. **Not by assigning**, which moves
no money until backing exists.

> ***Follow-up*, 2026-09-27: a negative net worth, *Vermogen* below zero, carries the same marker,
> with the badge *Rood*.**

Raised by the scenario writer. **Why:** every negative figure carries the one marker. Net worth is a
sum of balances, so it can only go below zero when at least one account is overdrawn, and it is the
same kind of fact about the world as an overdraft. That is why it shares its badge, in the
documentation's reading.

| Rejected | Why |
|---|---|
| **A plain negative figure** | It would be the only negative figure without the marker |

### Managing accounts

> **An account is added with a name and a starting balance. It can be renamed. It can be deleted
> only while unused**, which is for an account added by mistake. **Archiving an account with
> history, a real account that has been closed, is deferred until missed**, not rejected.

| Rejected, for now | Why |
|---|---|
| **The full category treatment, archiving included** | Offered without an argument of its own. In the documentation's reading, archiving needs rules of its own (where an archived account is shown, how it comes back), and nobody has closed an account yet |
| **Adding and renaming only** | An account added by mistake could never go |

> ***Derived:* account names follow the category name rules.**

Trimmed at the ends, compared without case and with a run of inner whitespace counting as one space,
stored as typed, and refused if they trim to nothing. Unique among accounts: a rename to another
account's name is refused, and an account's own name in a new spelling is allowed, as for a category
(*Renaming a category*, above). **An account and a category may share a name**, Sparen the category
and Sparen the account, because they are different dimensions.

> ***Follow-up*, 2026-09-27: adding an account whose name another account already has is refused**,
> with *"Er is al een rekening met die naam"*.

Raised by the scenario writer. **This is where accounts part from categories on purpose.** Adding a
category's name again hands back the category you already have (*Adding a name you already have gives
back the category you already have*, above). **Why not for accounts:** an account is added with a
starting balance, and handing back the existing one would quietly drop the balance just typed. The
wording is copy.

| Rejected | Why |
|---|---|
| **Hand back the existing account, as for a category** | The starting balance just typed would be dropped without a word |

> ***Follow-up*, 2026-09-27: a starting balance left empty means no starting balance.** The account
> then behaves like the first start's Betaalrekening: its balance is the sum of what is on it, whatever
> the dates, until it is first corrected. **A typed 0 is a starting balance**, and takes in earlier
> entries like any other.

Raised by the scenario writer. **Why:** it is the rule the first-start account already has, so there
is nothing new to learn. It refines ruling 10, *an account is added with a name and a starting
balance*: the starting balance may be left out. And it refines the derivation that a starting
balance is the account's first balance correction once more: that holds whenever one is typed, 0
included.

| Rejected | Why |
|---|---|
| **Refuse an empty starting balance** | The rule the first-start account has would be unavailable for any other account |

> ***Derived:* "unused" means no income, expense or transfer on it.** Its own starting balance and
> balance corrections do not count.

**Extended for the backing increment on 2026-09-27, and built the same day:** an account that **backs a
category** counts as used too, by ruling. That an account with a movement in its history counts as
used is derived, and was kept as a derivation after the follow-ups (*Backing can be set, changed or
removed at any time*, under *Backing and Accumulated*, below).

> ***Follow-up*, 2026-09-27: deleting an unused account is never confirmed, and it is announced
> afterwards**, like deleting an unused category, **even when it has a starting balance other than
> zero.**

**Why:** the act exists for mistakes, and the only thing lost is a number just typed. In the
documentation's reading, that squares with "confirm only where a record is lost" (*Removing an entry
asks first*, above) read as being about records of what happened, which a starting balance on an
account added by mistake is not. Deleting it takes
its starting balance and any balance corrections with it, and net worth changes accordingly.

| Rejected | Why |
|---|---|
| **Ask first** | A question for undoing a mistake, where only a number just typed is lost |

> ***Derived:* there are no account kinds.** Current account, savings account, cash: an account is a
> name and what is on it.

That is the smaller reading, and nothing in this increment behaves differently by kind. The *Account*
row of *Terms* names kinds as examples, not as a type.

### The pool account can be any account

> **Any account can be made the pool account, and there is always exactly one.** Making another
> account the pool changes the default for **new** entries only; existing entries keep their account.
> **The pool account cannot be deleted while it is the pool.**

**Why.** The pool is meant to be where income lands (*The pool account*, above), and that need not be
the account MoneyBud starts with.

| Rejected | Why |
|---|---|
| **Fixed to Betaalrekening** | The account the salary lands in may be another one |

**Any account, not "one current account".** *The pool account* (above) says one *current* account is
designated. With no account kinds, any account qualifies. That wording is left as written, with a
note.

**What follows, derived.** The pool cannot be deleted and there is always one, so **there is always
at least one account**, and the account list on the forms is never empty.

> **On screen, the pool account is *Hoofdrekening*, and making an account the pool is *Maak
> hoofdrekening*.**

Rejected: *Standaardrekening* and *Potrekening*.

### Future-dated income and balances

> ***Derived:* an income dated in the future counts in its period's *Unassigned* from the moment it
> is recorded, as before, but reaches its account's *Balance*, and so net worth, only on its date.**

It is *The central distinction*'s "net worth is what you have today" applied to one account.

### Saved data from before accounts

> **Data saved by a version without accounts is not carried over.** The version with accounts cannot
> read it. MoneyBud says so and closes, and the user deletes the file, which the README says where to
> find.

It applies *Demo data may not survive a new version* and *Real use before accounts* (above). This is
the **first time either is exercised.** Nothing new is ruled.

**Data saved by the accounts version is not carried over either**, since the backing increment
(2026-09-27). The file is version 3, and version 2 is refused as version 1 is. The stakeholder left
it to the build, and the build chose to refuse it. The reasons are in *Demo data may not survive a
new version* (above).

### Proposed display terms

Proposed on 2026-09-27, and **all approved by the stakeholder the same day** (follow-up, raised by
the scenario writer). **They stay in this table, not in *Dutch display terms* (below), until the
build**, and move there with the build that gives `Tekst` their constants. **Moved on 2026-09-27,
with the accounts build**: every row below is now also a row of *Dutch display terms*, where
`TekstTests` holds `Tekst` to it. The balance correction's row is split there into the record
(*Correctie*) and the act (*Saldo corrigeren*), one row each, because the test reads a cell's
" / " as parts that share a last word, and these two do not. This table is kept as the record of
what was proposed and approved. That is the precedent of
*Plan overnemen* and of the corrections increment's four rows. The reason is mechanical:
`TekstTests` reads that table and holds `Tekst` to every row in it, so a row added before its
constant fails the test suite.

| English (this project) | Proposed Dutch | Status |
|---|---|---|
| Account / Accounts | Rekening / Rekeningen | **Approved** 2026-09-27 |
| Balance | Saldo | **Approved** 2026-09-27 |
| Net worth | Vermogen | **Ruled**: the stakeholder's own word, from round 1 of [the follow-up interview](../stakeholder/2026-09-24-verdieping.md) |
| Pool account | Hoofdrekening | **Ruled** |
| Make (an account) the pool account | Maak hoofdrekening | **Ruled** |
| Transfer (the record / the act) | Overboeking / Overboeken | **Approved** 2026-09-27 |
| A transfer's two accounts | Van / Naar | **Approved** 2026-09-27 |
| Starting balance | Startsaldo | **Approved** 2026-09-27 |
| Balance correction (the record) / Correct a balance (the act) | Correctie / Saldo corrigeren | **Approved** 2026-09-27. One Dutch word for a two-word English term: on screen *Correctie* only ever appears beside a balance, so it cannot be mistaken for changing an entry |
| Add account | Rekening toevoegen | **Approved** 2026-09-27 |
| Rename (an account) / Delete (an account) | Hernoemen / Verwijderen | **Approved** 2026-09-27, **reused** from the category rows. Deleting an account is *Verwijderen* like deleting a category and removing an entry; each acts on a different thing, so it is not ambiguous on screen |
| Overdrawn, and a negative net worth (the marker's badge) | Rood | **Ruled** in a follow-up, 2026-09-27, over *Rood staan* and *Negatief saldo* (*An overdrawn account carries the marker*) |

**The reused words need no new constants.** *Hernoemen* and *Verwijderen* are already rows of
*Dutch display terms*, and `Tekst` already has them.

### Approved at the scenario gate, 2026-09-27

**The stakeholder approved the accounts scenarios at the scenario gate on 2026-09-27.** There are six
new feature files:
[`add-an-account.feature`](../../features/add-an-account.feature),
[`record-on-an-account.feature`](../../features/record-on-an-account.feature),
[`correct-a-balance.feature`](../../features/correct-a-balance.feature),
[`transfer-between-accounts.feature`](../../features/transfer-between-accounts.feature),
[`manage-accounts.feature`](../../features/manage-accounts.feature) and
[`show-accounts.feature`](../../features/show-accounts.feature). Together they hold 94 scenarios and
174 cases, one row of which was added after the gate (next). Scenarios were also added to
[`start-moneybud.feature`](../../features/start-moneybud.feature) and
[`keep-data.feature`](../../features/keep-data.feature). When this was written there was no plan
yet. **All of them are bound and green since the build**, the same day.

> ***Gate*, 2026-09-27: a starting balance typed as only spaces is the same as one left empty**, so
> the account has no starting balance.

**Why:** it is the rule for a label. A label that trims to nothing is no label (*A label is trimmed,
and that is what makes "blank" mean anything*, above), and a starting balance that trims to nothing
is no starting balance (*Managing accounts*, above). **Added after the gate** as one row in
`add-an-account.feature`.

| Rejected | Why |
|---|---|
| **Refuse it as not an amount** | Spaces would mean something different from nothing, where everywhere else in MoneyBud they mean the same |

**Chosen by the scenario writer, approved with the scenarios, and never put to the stakeholder one
by one.** They were written into the approved files, and he approved the files with them in front of
him, so they now stand as decisions. None was a separate question. The reasoning in the right-hand
column is the documentation's.

| Approved with the scenarios | What it rests on |
|---|---|
| **A transfer has no label.** When a transfer breaks several rules, the first broken is reported, in this order: two different accounts, more than 0, whole cents, not in the future | A transfer says what it is through its two accounts, as an expense does through its category. This settles what *Transfers* (above) left to the scenario stage. The order puts what the transfer is between before how much it moves and when, as recording an expense puts the category checks first (*When an assignment is refused*, above) |
| **Adding an account checks the name before the starting balance** | The same order: *what* before *how much* (*The fields ask what before how much*, above) |
| **The scenarios assert the notices for adding, renaming and deleting an account, making one the pool account ("is now the pool account"), and removing a balance correction or a starting balance.** They do **not** assert a notice for recording a transfer or a balance correction | Every act on an account says what it did, as the category acts do (*Archiving is announced, never confirmed*, above). For a new record, the scenarios check where it lands, in the strip and the history, rather than what is said. **Corrected after the build (2026-09-27): a documentation error, not a ruling changed.** This row first read that recording a transfer or a balance correction "is not" announced, "like recording an entry". Both halves were wrong. Recording an entry **is** announced (`Tekst.ExpenseRecorded`), and the build announces a recorded transfer and balance correction the same way. What the scenario writer chose was only which notices the scenarios assert. `spec-reviewer` found the mismatch |
| **"Unused" means unused now.** An account whose only entry was removed, or moved to another account, can be deleted | A removed or changed entry leaves no trace, so "ever" can only mean "now": the same as for a category (*Approved at the scenario gate, 2026-09-26*, above) |
| **A renamed account keeps its place in the order. An account deleted and added again goes last** | Renaming gives one account a new label; a deleted account added again is a new account. The same as for categories (*Approved at the scenario gate, 2026-09-26*, above) |
| **Correcting to the figure MoneyBud already shows is recorded**, as a balance correction with a difference of € 0,00, not treated as nothing | It is a checked balance, and checking is exactly what a balance correction records: from then on, entries dated before it are in it. Treating it as nothing would lose that |
| **The same-day tie-break's consequences, shown in scenarios and approved knowingly.** An entry dated on a balance correction's day but recorded the next day moves the balance. Changing an entry's date to before the balance correction's day takes it into the balance correction. Moving an entry dated before a balance correction to another account leaves the corrected balance unchanged | They follow from the tie-break and from a changed entry keeping the moment it was first recorded (*A typed balance is what the bank said that day*, above). The scenarios show them so that none comes as a surprise |
| **History rows show amounts unsigned. Rows with the same date are newest-recorded first. The history covers every period** | What a row is, a transfer in or out, an income, an expense, says which way the money went. The order is that of a period's lists (*A period's entries are listed newest first*, above). Covering every period confirms the documentation's reading in *The accounts strip, and an account's history* (above) |
| **The empty ledger every scenario starts from holds one synthetic pool account, "Bank", with no starting balance** | So that no scenario depends on the first start's *Betaalrekening*, the same way scenarios avoid the default categories (*They are Dutch because they are content*, above). Every income and expense needs an account, and the pool always exists, so an empty ledger with no account at all would not be one MoneyBud can have. Only the first-start scenarios see *Betaalrekening* |

### Accounts: chosen in the build, not put to the stakeholder

The rulings and the approved scenarios settle what is on the screen and what each act does. Some
visible details they leave open were settled while building. **These are the build's readings, not
rulings**, and any of them can be put to the stakeholder if he reacts to it. None contradicts a
ruling. Most of the reasons are the documentation's, and the last group was found by a headless run
of the real window.

**The history panel.**

- It opens **under the strip, full width**. It closes with ***Sluiten***, a control word like
  *Opslaan* and not a term in the display table. Clicking the open account again closes it too.
- It holds the account's acts:
  - *Hernoemen*;
  - *Verwijderen*, shown only while the account is unused and not the pool account;
  - *Maak hoofdrekening*, not shown on the pool account;
  - the *Saldo* box with *Saldo corrigeren*.

  Showing an act only where it can go through is how the category rows already treat *Verwijderen*.

**The two small forms.** *Rekening toevoegen* and *Overboeken* each open a small form under the
strip. A transfer loaded from a history row shows the *Wijzigen* badge with *Opslaan*, *Annuleren* and
*Verwijderen*, as the entry forms do (*On screen: picking an entry to correct*, above).

**The account lists.**

- On the expense and income forms they are **lists**, last, as ruled. In Avalonia that is a
  `ComboBox`, not free text.
- The transfer form asks **Van → Naar → Bedrag → Datum**: the "what", which is between which
  accounts, before the amount (*The fields ask what before how much*, above).
- `WindowMarkupTests` holds both orders.

**Naar's default.** *Naar* starts out on the first account that is not *Van*. With only one account
that is *Van* itself, so trying to transfer is refused as "two different accounts". **Why:** the
refusal the scenarios specify is then reachable, rather than hidden behind a form that cannot be
filled in.

**What a form holds**, settled after a headless run of the real window showed the first design
going wrong:

| Reading | Why it was built this way |
|---|---|
| **A form's account is always a plain account once set, and a list writing back "nothing" is ignored** | The first design treated the pool account, written back by the list, as "not chosen". On a form it pinned *Naar* on the only account there was. `spec-reviewer` then showed it discarding a *Naar* chosen before *Van*. A list that lets go of its selection while it is rebuilt writes back "nothing", and that must not become a choice the user never made |
| **Defaults move only at defined moments** | On *Maak hoofdrekening*, a new entry still on the old pool account moves to the new one, and so does a new transfer's *Van*. When an account is added, a new transfer's *Naar* moves off *Van*. An entry or transfer **being changed keeps its own accounts** |
| **The lists get a new collection only when the accounts, their order or their names change**, and the forms are told after the redraw | A list handed a new collection on every refresh would drop its selection once a minute. Told before the redraw, a form could name an account the list does not hold yet |

**Screen tweaks.**

- **The ring's minimum height dropped from 260 to 180**, and the ring clips. With the history and the
  transfer form both open, it would otherwise run into the assign row. This is drawing, and it
  changes nothing the ring shows (*The Overview's layout*, above).
- **The history list scrolls from 150 px.**

**The notice wording for the new acts is copy**: adding, renaming, deleting, making the pool
account, recording and changing a transfer, correcting a balance, and removing a transfer or a typed
balance. The display terms are in the table, and the sentences are `Tekst`'s.

### What this section leaves open

**Nothing for this increment.** When this section was first written up, on 2026-09-27, seven points
stood here, found while the rulings were being recorded. All seven were dealt with the same day. The
list is kept as a record of where each went; the analysis each carried is now in the subsection it
went to.

| Point raised | Where it went |
|---|---|
| 1. Is a first start's €0,00 a starting balance, absorbing back-dated entries? | **Ruled by the stakeholder**: no, the first start's Betaalrekening has no starting balance (*A first start has one account*, follow-up) |
| 2. A transfer across a balance correction changes net worth, against "leaves net worth unchanged" | **Ruled by the stakeholder**: that is true, not a flaw (*Transfers*, follow-up) |
| 3. Money the user already had has no purpose, and is in no *Unassigned* | **Deferred by the documentation** to the backing increment's first stage, not ruled (*A question for the backing increment*, next). In this increment such money is seen by location only |
| 4. Does deleting an unused account with a starting balance ask first? | **Ruled by the stakeholder**: never confirmed, announced afterwards (*Managing accounts*, follow-up) |
| 5. Is interest, or an investment's change in value, an income or a balance correction? | **Recorded by the documentation as usage, derived**: MoneyBud decides nothing, and either act can be taken (*A starting balance or a balance correction is net worth only*) |
| 6. Does an account's history show how far each balance correction moved the balance? | **Ruled by the stakeholder**: yes, the new balance and the difference (*The accounts strip, and an account's history*, follow-up) |
| 7. Is a changed entry recorded before or after a balance correction? | **Derived by the documentation**: by when it was first recorded, never by when it was changed (*A typed balance is what the bank said that day*) |

### A question for the backing increment

**Not a ruling.** Recorded by the documentation on 2026-09-27 for the first stage of the backing
increment, where it first has consequences. It is not filed under *Open questions* (below), because
it has a place and a time to be asked, like the overdraft question before it.

> **What purpose, if any, does money the user already had carry?** A starting balance and a balance
> correction are seen by location only: they change net worth and nothing on the purpose side.

**Why it is a question at all.** *The central distinction* (above) says net worth and the budget are
one set of data seen two ways, and *Backed categories accumulate* (above) says that "a sum that
appears on the location side and nowhere on the purpose side breaks that promise". A starting
balance is such a sum, and so is a balance correction. *Unassigned* is two things under one name,
deliberately: the absence of a purpose, and the period figure (*Terms*). The €5.000 in a savings
account added today is the first and not the second, so for money the user already had, the two
senses come apart.

**Why it can wait.** In the accounts increment nothing on the purpose side reads a balance. Assigning
moves no money, no category is backed, and there is no *Accumulated*. So the gap is visible only as
net worth being larger than anything the budget accounts for, which is true and harmless.

**Why it cannot wait past backing.** Backing makes a category's money live in an account, and
*Accumulated* counts only what was assigned to it, minus what was spent. A savings account added with
€5.000 and backing *Sparen* would hold €5.000 while *Sparen* showed €0 accumulated, which is the
disagreement *Backed categories accumulate* exists to prevent. The answers the documentation can see,
for that stage to weigh and not decided here:

| Possible answer | What it says | What it costs |
|---|---|---|
| **Leave it by location only** | Money the user already had is outside the budget, and *Accumulated* is only what MoneyBud saw assigned | *Accumulated* and a backing account's balance disagree from the first day, by the starting balance |
| **Give a starting balance a purpose when it is typed** | Adding an account asks what its money is for, a backed category among the answers | A second decision when adding an account, and a new act between the two dimensions |
| **Let *Accumulated* start from a figure of its own** | A backed category is given an opening *Accumulated*, like an account's starting balance | A second opening figure to keep in step with the first |

**Answered by the stakeholder on 2026-09-27**, at the backing increment's first stage, as it was
meant to be: **leave it by location only**, the first row of the table. Backing a category with an
account moves nothing already in it, and *Accumulated* starts at €0. The cost in the table's first
row was accepted with it. The ruling, his words and the reasoning are in *Money the user already had
stays by location only*, under *Backing and Accumulated* (next). His answer also raised a question of
his own, about money a category already has when it is backed, which became a ruling of its own
(*Backing a category that already has money*, next).

## Backing and *Accumulated*

The rulings for the backing increment, settled with the stakeholder on 2026-09-27. Like *Accounts
and net worth*, they were answers to multiple-choice questions, each put to him with a
recommendation, and they went straight into this glossary rather than into a new interview round.
**He took the recommended option every time.** **The reasoning given with each ruling is the
documentation's**, offered with the recommended option as the argument for it, unless it is marked
as his. Where no argument was recorded with a ruling, the section says so, and any reason given is
the documentation's reading.

**Two rulings carry his own words.** Asked what purpose money he already had carries, he answered in
his own terms (*Money the user already had stays by location only*, below), and in the same answer
**raised a case nobody had put to him**: a category that already has money when it is backed. That
became a question of its own, and its ruling is *Backing a category that already has money* (below).

**One ruling was revised by him the same day, on his own idea.** When he was shown that two rulings
contradicted each other, he changed what unbacking does: it **returns the money** (*Backing can be
set, changed or removed at any time*, below). That is the one place where his choice was not the
recommendation. The first version is kept there as written.

**Follow-ups, the same day.** Writing the eleven rulings up left fourteen points open. All were
put back to him on 2026-09-27, each with the documentation's reading as the recommendation. **He took
the recommendation every time**, apart from the revision above, which also settled two of the
points. Three points were left as the documentation's derivations. Each sits in the subsection it
belongs to, marked *follow-up* or *derived*. *What this section leaves open* (below) now says where
each point went, and lists what is still open. **Two more follow-ups came after the revision**, on
derivations it brought with it, and he took the recommendation both times: what unbacking and
re-pointing move is what is there for the category in the backing account, not *Opgebouwd*, and
either may overdraw the account the money leaves.

**Settled, specified and built.** When this section was written there was no feature file, no plan
and no code. **The scenarios were approved at the scenario gate on 2026-09-27**: five new feature
files, [`back-a-category.feature`](../../features/back-a-category.feature),
[`assign-to-a-backed-category.feature`](../../features/assign-to-a-backed-category.feature),
[`spend-against-a-backed-category.feature`](../../features/spend-against-a-backed-category.feature),
[`show-accumulated.feature`](../../features/show-accumulated.feature) and
[`show-moved-money.feature`](../../features/show-moved-money.feature), with 62 scenarios and 84
cases, plus additions to `start-moneybud.feature` and `keep-data.feature`. **The plan was approved
at the plan gate the same day**, with [ADR 0009](../decisions/0009-movements-are-entries.md): every
movement is a stored entry, written on the day the money moves, and money for a later period is
written by *settling* on that period's first day. **All of it was built and is green the same day.**
Where a subsection below says something is "for the plan", the plan has answered it. What the build
chose beyond the rulings is in *Backing: chosen in the build, not put to the stakeholder* (below).
**Five points raised after the build were ruled the same day** (*Backing: ruled after the build*,
below): re-pointing from the pool account, deleting a category money was moved for, a new display
rule that keeps an archived backed category with money built up on screen, one amended scenario
line, and picking the account already shown.

> **A category is backed by one account, or by none, and the user can set, change or remove that at
> any time.** Assigning to a backed category **really moves money**, from the pool account to its
> backing account, on the day of assigning or on the period's first day if that is later. **Backing a
> category moves nothing already in the account**, but it does move the category's unspent
> *Remaining* for the current period off the pool account. A backed category shows ***Opgebouwd***,
> its *Accumulated*: what has moved in on its behalf since it was backed, minus what has been spent
> against it since. **What MoneyBud moved in on a category's behalf, MoneyBud moves back when the
> backing ends, and takes along when the backing is pointed at another account.**

An example, with synthetic figures. It is 10 October. Betaalrekening is the pool account, and the
user has added Spaarrekening with a starting balance of €5.000. *Sparen* is unbacked, with a *Budget*
of €300 in October, €100 of which has been spent. He sets *Sparen*'s *Staat op* to Spaarrekening.
**€200 moves today**, from Betaalrekening to Spaarrekening, which now reads €5.200, and *Sparen*
shows *Opgebouwd: € 200,00*. The €5.000 is still there, still with no purpose. He assigns another
€50 to *Sparen* in October: €50 moves today, and *Opgebouwd* reads €250. He assigns €300 to *Sparen*
in November: **nothing moves until 1 November**, so today's balances stay as they are, and stepping
to November shows *Opgebouwd: € 550,00*. Both accounts' histories show each movement as a row of its
own, such as *"Toegewezen aan Sparen — € 200,00"*. If he unbacks *Sparen* on 15 October, the €250
moved into Spaarrekening for it goes back to Betaalrekening that day, and the November €300 later moves
nowhere.

### What the backing increment covers, and what waits

> **Backing and *Accumulated*. One backing account per category, or none.** No sweep.

| Not in this increment | When |
|---|---|
| **The sweep** | The next increment. Its destination must be backed, which it now can be. Until then its money still vanishes at a period end, as accepted ([§11](11-risks-and-technical-debt.md)) |
| **Several backing accounts per category**, and a **default** among them | **Deferred until missed**, not rejected |
| **Choosing the account for one assignment**, overriding the backing account | **Deferred until missed**, not rejected |

**So, in this increment, "the category's default backing account" is simply "its backing
account"**, wherever this glossary says the former. The many-to-many model of *Account-backed
categories* (above) stays the intended one; this increment builds the part of it where a category
has one account. **An account may still back several categories.** Nothing in the ruling limits
that side, and the settled model already allowed it. That is the documentation's reading, not asked.

> **An expense against a backed category is pre-filled with its backing account.** The account list
> on the expense form still lets the user pick another.

That was already settled (*An expense defaults to the pool account*, above). With backing it becomes
reachable. No argument was recorded with the scope ruling. In the documentation's reading, one
account per category answers "where does this money go" with no second decision, and the deferred
parts can be added later without undoing it.

> ***Follow-up*, 2026-09-27: the expense form's account follows the category typed**: the backing
> account for a backed category, the pool account for an unbacked one. **Until the user picks an
> account himself**; that choice then sticks for that entry. An expense being changed keeps its
> account.

**Why:** the pre-fill is only useful if it is right for the category actually typed, and a choice the
user made on purpose must not be undone by typing. It adds one defined moment at which the form's
default moves, to the ones *Accounts: chosen in the build* (above) lists: typing a category.

| Rejected | Why |
|---|---|
| **The account always follows the category, overwriting a choice** | Typing the category after choosing the account, which the field order allows, would silently undo the choice |

***Derived*, not asked, at the scenario stage: while the category box holds text that is not a
category** — a name half typed, or one no category has — **the account is the pool account.** There
is no category to have an opinion, and the pool account is the default wherever none has one (*An
expense defaults to the pool account*, above).

### Assigning to a backed category moves money

> **Assigning to a backed category moves the amount from the pool account to its backing account. A
> negative assignment moves it back**, but only what the clip lets through (*An amount may be
> assigned negatively*, above). **The money moves on the day of assigning, or on the period's first
> day if that is later.**

So assigning €300 to November's *Sparen* on 20 October leaves today's balances alone. On 1 November
Betaalrekening falls by €300 and Spaarrekening rises by €300.

**Why**, the reason offered with the recommendation and accepted: each of the other two dates goes
wrong somewhere.

| Rejected | Why |
|---|---|
| **Always on the period's first day** | A mid-month assignment would be dated before a balance correction taken earlier that month, and that balance correction would then hold it (*A typed balance is what the bank said that day*, above): the move would be swallowed and no balance would change |
| **Always today, even for a later period** | Money would be shown as moved for a plan that has not started |

***Derived*, not asked:**

- **Taking a plan over is assigning** (*Taking the plan over assigns it in full*, above), so each
  figure it assigns to a backed category moves money by the same rule: today in the current period,
  on the period's first day in a later one.
- **Assigning zero moves nothing**, and a clipped negative assignment moves back only what came back.
  Capped further by the follow-up below.
- **A movement goes through when the pool account has not got the money**, leaving it overdrawn and
  marked (*Assigning may overdraw the pool account*, above).
- **A movement is between two of the user's accounts**, so it leaves net worth unchanged, except
  where a balance correction has already counted one side, as for a transfer (*Transfers*, above). It
  changes no budget figure beyond what the assignment itself changes.
- **A movement meets balance corrections by the same rule as any entry**: one typed after its day
  holds it.
- ***Derived*, kept after the follow-ups: on its own day, a movement orders against a balance
  correction by recording order, like every entry** (ADR 0008). A movement for November was
  recorded when it was assigned, in October, so a balance correction typed on 1 November holds it:
  the bank is taken to show it moved already. One rule for every kind of entry was preferred over a
  planned movement counting as recorded at the start of its day, or after anything typed that day.
- ***Derived*, kept after the follow-ups: a planned movement is not in either account's history
  before its day**, because which account it lands in is decided only on that day (*Planned money
  follows the backing on the day it moves*, below). Showing it ahead in today's backing account, and
  moving it if the backing changed, was the alternative.

> ***Follow-up*, 2026-09-27, from the scenario stage: a negative assignment to a backed category
> moves back at most what is there for it**, by the test unbacking uses: what MoneyBud moved in on
> its behalf, minus its expenses paid from the backing account (*Backing can be set, changed or
> removed at any time*, below).

The example. *Budget* €300, €100 spent from the pool account before backing, so backing moved €200.
Assigning −300 takes the *Budget* to 0 and moves back **€200**, not €300. Hoofdrekening ends exactly
€100 down for the €100 spent, and *Opgebouwd* reads €0.

**Why:** the money side can only return what is there for the category; the €100 was never moved in,
because the pool account paid it before the backing.

| Rejected | Why |
|---|---|
| **Move the full €300** | Spaarrekening would go €100 below zero for an expense Hoofdrekening paid, with *Opgebouwd* at −€100 |

**The clip against the *Budget* is unchanged** (*An amount may be assigned negatively*, above). That
clip is about the plan, and this cap is about money only: the *Budget* still falls by the full clipped
amount, and *Unassigned* rises by it.

***Derived*, not asked:**

- **What is reported.** The plan side reports as it always has: a shortfall against the *Budget* is
  said as *Niet teruggezet*, and nothing else is. The money side is shown by the history row, which
  names the amount that really moved (€200 in the example). No second shortfall message.
- **If there is nothing there for the category, the negative assignment moves no money**, and still
  changes the plan.
- **For a later period, nothing has moved yet**, so a negative assignment there only lowers what will
  move on that period's first day, and no cap is needed.

### Money the user already had stays by location only

> **Backing a category with an account moves nothing that is already in the account.**
> *Accumulated* starts at €0, or at what moves under the next ruling.

In his own words: *"If i have an account with 5000 and then add a categorie as backed by that
account, then nothing happens except that they are now backed. Now if i do budget money to the
categorie it should move to that account."*

This answers *A question for the backing increment* (above). Of the three answers that section
could see, he took the first.

| Rejected | Why |
|---|---|
| **Give a starting balance a purpose when it is typed** | A second decision when adding an account, and a new act between the two dimensions |
| **Let *Accumulated* start from a figure of its own** | A second opening figure, to keep in step with the account's starting balance |

**The accepted cost.** *Accumulated* and a backing account's balance differ by whatever was in the
account before, from the first day. That was the cost this answer carried in the table it was chosen
from. *Backed categories accumulate* (above) already said the two are not the same number.
**Money the user already had stays seen one way**, by location. *The central distinction*'s "one
set of data, two ways" is kept for money MoneyBud saw given a purpose, and not for money that arrived
as a typed balance (*The central distinction*, above).

### Backing a category that already has money

His case, raised in his answer to the last ruling: *"If there is already money in the category and
then i move the backing to the account then ... it should move from the default account to the
selected account."*

He first asked whether an unbacked category's money is on the pool account. **The answer given was
yes**, and it is recorded here as confirmed: assigning to an unbacked category moved nothing, its
expenses default to the pool account (*An expense defaults to the pool account*, above), and the
sweep will collect from there (*The sweep*, above).

> **On the day of backing, the category's unspent *Remaining* in the current period moves from the
> pool account to the backing account.** A *Budget* of €300 with €100 spent moves €200: the €100
> spent has already left the pool. **If the category is overspent, nothing moves.** Budgets already
> set for **later** periods move on those periods' first day, like any assignment (*Planned money
> follows the backing on the day it moves*, below). ***Accumulated* starts at what moved.**

| Rejected | Why |
|---|---|
| **Move the whole *Budget*** | What was spent has already left the pool account, so it would be moved a second time, in the documentation's reasoning |
| **Move nothing** | The category's money for this period would stay on the pool account while the category says it is somewhere else: the case he raised, in the documentation's reading |

**In the documentation's reading**, earlier periods' leftovers do not move. The ruling names the
current period, and a past period's *Leftover* of an unbacked category is for the sweep, not for
backing.

> ***Follow-up*, 2026-09-27: the expenses that lower *Accumulated* are those dated after the backing
> day, or on it and recorded after the backing.** It is the test a balance correction applies (*A
> typed balance is what the bank said that day*, above). **A forgotten expense dated before the
> backing, recorded later, does not touch *Accumulated*, and the amount moved at backing stays what
> it was.** The same holds for an expense before the backing that is changed or removed.

An example. *Budget* €300, €350 spent, backed on 10 October: overspent, so nothing moves. €100
assigned on 15 October moves €100, and a €30 expense on 20 October leaves *Opgebouwd* at €70, while
that period's *Resterend* is €20. **Why:** the backing move was what the *Remaining* was on the
backing day, and an expense from before it was already paid out of the pool account. One test for
"before" and "after" across MoneyBud is one thing to learn.

| Rejected | Why |
|---|---|
| **Recompute the backing move from *Remaining* as it now reads** | Balances would shift after the fact for a move already shown in both histories |
| **Every expense in the backing period** | It contradicts "*Accumulated* starts at what moved": €300 with €100 spent would move €200 and read €100 |

**The accepted cost:** after a late expense dated before the backing, the period's *Remaining* and
what moved disagree, and nothing on screen says so.

### Backing can be set, changed or removed at any time

> **A category's backing can be set, pointed at another account, or removed at any time.**
> **Revised the same day (below): unbacking returns the money to the pool account, and re-pointing
> takes it along.** After unbacking, the category shows no *Accumulated*.

**As first ruled**, and kept because what was believed is part of the record:

> *Money that has already moved stays where it went; only later movements follow the new setting.
> Unbacking moves nothing back, and neither does re-pointing.*
>
> *Why, the reason given: in real life the user moved that money at the bank. MoneyBud moving it
> back would record a movement that did not happen.*

> ***Revised by the stakeholder on 2026-09-27, his own idea: what MoneyBud moved in on a category's
> behalf, MoneyBud moves back when the backing ends.*** "The money" below is what is there for the
> category in the backing account, as a follow-up the same day settled (below).
>
> - **Unbacking moves the money back** from the backing account to the pool account, on the day
>   of unbacking. **If there is none, nothing moves**, which mirrors backing an overspent category.
> - **Re-pointing takes the money along** from the old account to the new one, on that day.
>   *Opgebouwd* **continues**; it does not restart (*Re-backing starts Accumulated over*, below,
>   stands).
> - **Money planned for a later period is unchanged**: it moves on that period's first day to
>   whatever the category is backed by then (*Planned money follows the backing on the day it moves*,
>   below).

In his words, when shown that the first version let unbacking and backing again move the same money
twice: *"I feel we might need to rethink the earlier part, so if you unback a categorie its money
return to the default account. This way it makes way more sense what is actually happening."*

**Why**, the reasoning put to him with it, which he accepted:

- **It removes the contradiction.** Under the first version, backing a category again in the period
  it was unbacked moved its unspent *Remaining* off the pool account a second time, though that money
  had been left in the old account (*Backing a category that already has money*, above, rests on an
  unbacked category's money being on the pool). Now the money is back on the pool by then.
- **Nothing is stranded.** The first version left money in the savings account with no purpose,
  while MoneyBud assumed an unbacked category's money was on the pool account, which is where the
  sweep will collect from (*The sweep*, above).
- **Its only cost is the one backing already has**: the balances match the bank only once the user
  makes the same transfer himself. That was true of every assignment to a backed category already,
  and it is why the first version's reason, "a movement that did not happen", does not weigh more
  here than there.

| Rejected | Why |
|---|---|
| **The first version: money that moved stays where it went** | The contradiction above, and money stranded with no purpose where MoneyBud does not look for it |
| **Unbacking returns only this period's *Remaining*** | Money built up in earlier periods would still be stranded in the backing account |

**What follows, written down with the ruling.** Money built up in **earlier** periods goes back to the
pool account **with no purpose**, because it is in no period's *Unassigned*: a period's *Unassigned*
is that period's income minus what was assigned in it, and that money was assigned long ago. It is the
same situation as the €5.000 of *Money the user already had stays by location only* (above): money
with a location and no purpose.

**As the revision was first written up**, unbacking and re-pointing moved "all of *Opgebouwd*",
and nothing if *Opgebouwd* was negative. That was the documentation's wording of his idea, and the
next follow-up replaced it.

> ***Follow-up*, 2026-09-27: what unbacking and re-pointing move is not *Opgebouwd*. It is what
> MoneyBud moved into the backing account on the category's behalf, minus that category's expenses
> paid from the backing account itself**: put simply, **what is there for it**. Re-pointing moves the
> same amount from the old account to the new one.

The example. €200 moved into Spaarrekening for *Sparen*, then a €50 *Sparen* expense was paid from
Betaalrekening, the pool account, so *Opgebouwd* is €150. **Unbacking returns €200.** Spaarrekening
ends up with nothing on *Sparen*'s behalf, and Betaalrekening ends up exactly €50 down for the €50
spent. Nothing is stranded.

**Why.** It moves what is really in the account for the category, so the account is left with nothing
on its behalf, and the pool account is left exactly where MoneyBud assumes it is.

| Rejected | Why |
|---|---|
| **Return exactly *Opgebouwd*** (€150 in the example) | €50 would stay in Spaarrekening with no purpose, and Betaalrekening would be €50 short of what MoneyBud assumes is there |

**The amount moved and *Opgebouwd* can now differ, on purpose.** *Opgebouwd* is unchanged: a
purpose-side figure that counts **every** expense against the category, whichever account paid
(*Spending against a backed category from another account*, below). The amount moved is a
location-side figure: what the backing account holds for the category, so it counts only the
expenses paid **from that account**. They differ by the category's expenses paid from other accounts
since the backing. Each answers its own question: how much has been built up for the purpose, and
how much of it is sitting in this account.

**"If there is none, nothing moves" applies to this amount**, not to *Opgebouwd*: when the category's
expenses paid from the backing account are more than what moved in, nothing moves. The account has
then paid out more for the category than it received for it, which is true, and it shows in its
balance.

> ***Follow-up*, 2026-09-27: unbacking or re-pointing may overdraw the account the money leaves.**
> For example, when groceries were paid from Spaarrekening. The balance is shown with the one marker,
> badge *Rood*, and nothing is blocked.

**Why:** it is true. The money MoneyBud moved there for the category was spent on something else, so
the account really is short of it.

| Rejected | Why |
|---|---|
| **Move only what the account holds** | The pool account would get back less than was taken from it for the category, and the shortfall, which is real, would be hidden instead of shown |

***Derived*, not asked:**

- **Which of the category's expenses count, for the amount moved**: those paid from the backing
  account and dated after the day it became the backing account, or on that day and recorded after,
  the test the follow-up on *Opgebouwd* uses (*Backing a category that already has money*, above).
  After re-pointing, the new account starts from what was taken along.
- **Re-pointing when there is none moves nothing**, by the same mirror rule, and *Opgebouwd*
  continues on the new account.
- **"What is there for it" means what has moved by that day.** Money planned for a later period has
  not moved yet, so it is not returned or taken along; it follows the backing on its own day. On
  screen *Opgebouwd* also includes what is planned, when a later period is shown (*Accumulated covers
  everything up to the period on screen*, below). The amount moved is the one for today.
- **The moves made on unbacking and re-pointing are movements like any other**: a row each in both
  accounts' histories, meeting balance corrections by the same rule.
- **The amount moved on unbacking or re-pointing stays what it was** when something dated before it
  is recorded later, as the amount moved at backing does (*follow-up*, *Backing a category that
  already has money*, above). So an expense paid from the backing account, dated inside the backed
  span and recorded only after unbacking, lowers that account after all the money has left it: the
  account ends up short by it, marked if below zero, and the pool account holds it. This mirror of
  the backing follow-up was not put to him.
- **This settles two of the points first left open.** The second backing move is gone, and a
  negative assignment after re-pointing or unbacking now draws from the account that really holds
  the money: the new backing account, or, after unbacking, the pool account.

> **An account that backs a category counts as used**, so it cannot be deleted.

This extends *Managing accounts* (above), where "unused" meant no income, expense or transfer on the
account. ***Derived*, kept after the follow-ups:** an account with a movement in its history is used
as well, even after the category it backed is unbacked, because a movement is on the account as a
transfer is.

### Planned money follows the backing on the day it moves

> **Money assigned now for a later period moves on that period's first day, to whatever the category
> is backed by then**: the new account after re-pointing, and nowhere if it has been unbacked. **The
> same rule** makes a category backed after its November *Budget* was set move that *Budget* on
> 1 November.

No argument was recorded with this ruling. In the documentation's reading, it is *Backing can be set,
changed or removed at any time* applied to money that has not moved yet: until its day it has not
gone anywhere, so the backing on that day decides.

**What follows, for the plan.** A movement for a later period **has no destination until its day**.
So it cannot be written down as a finished record at the moment of assigning, unless that record is
rewritten whenever the backing changes first. That is a question for the plan, not for the
stakeholder ([§11](11-risks-and-technical-debt.md), the balance-writing row).

**Answered by the plan** (2026-09-27, [ADR 0009](../decisions/0009-movements-are-entries.md)):
nothing is written when money is assigned for a later period. The money is written by **settling**,
the first time MoneyBud runs on or after that period's first day, before anything else is done. It
is dated the first day and goes to the backing account and out of the pool account of that moment.
Nothing can change while MoneyBud is closed, so that is exactly what the first day had.

### Re-backing starts *Accumulated* over

> **Backing a category again after unbacking it starts *Accumulated* over**, from what moves at the
> re-backing (*Backing a category that already has money*, above). The old total stayed in its account
> as money with no purpose, like the €5.000 of *Money the user already had*. **Re-pointing without
> unbacking does not restart it.**

No argument was recorded beyond the ruling's own. As first written, it was consistent with the two
before it: money that moved stayed where it went, and money in an account that no category claims is
money with no purpose.

**The ruling stands after the revision of unbacking** (2026-09-27), with its second sentence no longer
true: the old total now **goes back to the pool account** when the category is unbacked, and so the
*Remaining* that moves at re-backing is money really on the pool. Re-backing still starts over. The
part of the old total from earlier periods is on the pool with no purpose, as *Backing can be set,
changed or removed at any time* (above) says. Re-pointing takes the money along and *Opgebouwd*
continues.

**This changes what *Accumulated* is.** It is **no longer "the running sum of *Remaining* over every
period"**. It is what has moved in on the category's behalf since it was last backed, minus what has
been spent against it since. The definition in *Backed categories accumulate* (above) is rewritten to
match, and the old one is kept there with its reasoning.

### *Accumulated* covers everything up to the period on screen

> ***Accumulated* is shown up to and including the period on screen.** Stepping back shows what had
> been built up by then; stepping forward includes what is planned.

| Rejected | Why |
|---|---|
| **Always as of today, like *Vermogen*** | No reason was recorded. In the documentation's reading, *Accumulated* is a purpose-side figure, and the purpose side steps with the period on screen, where balances are about today |

**What follows.** *Opgebouwd* and the backing account's *Saldo* answer different moments: the strip
is always today, and *Opgebouwd* is the period on screen. In a later period they differ by what is
planned, on purpose. It is the same shape as net worth and *Unassigned* disagreeing about an expected
income (*The central distinction*, above).

> ***Follow-up*, 2026-09-27: in earlier periods, *Opgebouwd* follows today's backing.** A category
> backed now shows it in every period, €0 before anything moved. An unbacked category shows it
> nowhere.

**Why:** one question, "is it backed", answered once, the same on every row and in every period.

| Rejected | Why |
|---|---|
| **Follow the backing as it was in the period on screen** | A row would show *Opgebouwd* in some periods and not others, for reasons nothing on screen shows |

> ***Follow-up*, 2026-09-27: a negative *Opgebouwd* carries the one marker, with the badge *Rood***,
> the same as an overdrawn account. Shown, never blocked.

It is reached when more has been spent against the category since the backing than has moved in,
for instance after backing an overspent category, or by spending put on another account (*Spending
against a backed category from another account*, below). **Why:** every negative figure carries the one marker, and *Rood* is the badge for money that
is not there. No new display term is needed.

| Rejected | Why |
|---|---|
| **A plain figure, or floored at zero** | The only negative figure without the marker; or a figure that hides spending |
| **The badge *Over budget*, or a word of its own** | *Over budget* is about one period's plan, and this spans periods |

### On screen: *Staat op* and *Opgebouwd*

> **Each category row gets a list, *Staat op*, next to *Hernoemen***, with "—" for none and the
> accounts after it. **A backed row shows *"Opgebouwd: € 600,00"* under its figures**, and so do its
> slice's hover details.

***Staat op*** is the stakeholder's own phrase from round 2: *"overblijfsels gaan dus in een
budgetpotje, dat op zijn beurt op een specifieke plek staat"*
([round 2](../stakeholder/2026-09-24-verdieping.md)), which *Account-backed categories* (above) had
already called "that *staat op*". ***Opgebouwd*** was chosen over *Gespaard*, because it also fits an
investment category.

This **extends** *What each category row shows* and *Hovering a slice shows its figures* (above): a
backed row, and its slice, show one figure more. **In the documentation's reading**, the accounts in
the list follow the strip's order, pool account first and then the order added, as every account
list does (*The accounts strip, and an account's history*, above).

### A first start leaves *Sparen* unbacked

> **A first start leaves *Sparen* unbacked.** It has only *Betaalrekening*; the user adds a savings
> account and backs *Sparen* with it.

This revises *Sparen ships unbacked, and that is temporary* (above): it is no longer temporary in the
sense that MoneyBud will back it. The user does. No argument was recorded beyond the ruling's own:
the only account a first start has is the pool account.

### Moved money in the account's history

> **Moved money shows in both accounts' histories, read-only, one row per movement**: each
> assignment, and the move made on backing, each on its own day, such as *"Toegewezen aan Sparen —
> € 200,00"*. It is changed by assigning again, not from the history.

| Rejected | Why |
|---|---|
| **One row per period** | Each row is when that amount moved, and balance corrections need that: whether one holds a movement depends on its day |

**This extends** *The accounts strip, and an account's history* (above), whose history held a
starting balance, balance corrections, transfers, incomes and expenses. ***Derived*, not asked:** a
movement row cannot be removed from the history, and a negative assignment adds a row going the other
way rather than changing the earlier one. The row's wording is copy. Since the revision of unbacking,
the moves made on **unbacking** and **re-pointing** are rows too, one each, in both accounts. **A
movement from the pool account to itself leaves no row** (follow-up, *The pool account may back a
category*, below).

### Spending against a backed category from another account

> ***Follow-up*, 2026-09-27: an expense against a backed category put on another account still
> lowers *Opgebouwd*.** *Sparen* money was spent, whichever account paid.

**Why:** *Opgebouwd* is a purpose-side figure, and the purpose side counts every expense against a
category, as *Remaining* does. The backing account's balance and *Opgebouwd* then differ by that
amount, which is visible and true: the backing account holds money the pool account paid out.

| Rejected | Why |
|---|---|
| **Only expenses on the backing account count** | *Opgebouwd* would claim money for *Sparen* that had been spent on *Sparen* |

**What unbacking and re-pointing do with such an expense, ruled in two further follow-ups.** They do
not move *Opgebouwd*. They move what is there for the category in the backing account: what was moved
in, minus the category's expenses paid from that account. After €200 moved in and €50 spent against
*Sparen* from Betaalrekening, unbacking moves **€200** back, not €150, so nothing stays in the backing
account with no purpose, and Betaalrekening ends exactly €50 down for the €50 spent (*Backing can be
set, changed or removed at any time*, above). This replaces a derivation first written here, that €150
would move and €50 be left behind. The other way round, an expense against **another** category put
on the backing account lowers its balance but not what is there for *Sparen*, so unbacking can
overdraw it, which is ruled true.

**One case the example does not cover**, *derived* and not put to him: a *Sparen* expense paid from a
**third** account, neither the pool nor the backing account, Contant say. Unbacking still returns what
was moved in, so the pool account gets the €50 back while it was Contant that paid. Nothing is
stranded, but the €50 lands on the pool rather than where it was spent from. In the documentation's
reading that is acceptable, and the same as any expense put on another account than the one MoneyBud
assumes.

### The pool account may back a category

> ***Follow-up*, 2026-09-27: the pool account may back a category.** It is offered in *Staat op* like
> any account. Assigning then moves money from the pool to the pool, so **no balance changes, but
> *Opgebouwd* still counts.** The same holds when a backing account is later made the pool.

**Why:** nothing about the pool account makes it unfit to hold a purpose's money, and refusing it
would be the one account missing from the list. *Opgebouwd* keeps meaning what has been set aside for
the category, wherever that is.

| Rejected | Why |
|---|---|
| **Refuse backing with the pool account** | An exception in the list for no reason the user can see |
| **Allow it, and show no *Opgebouwd*** | Whether a figure shows would depend on which account is the pool, which the user changes for another reason |

> ***Follow-up*, 2026-09-27, from the scenario stage: a movement from the pool account to itself
> leaves no history row.** No balance changes, so there is nothing for a row to explain.
> *Opgebouwd* still counts it.

| Rejected | Why |
|---|---|
| **One row, netting to zero** | It would show that the assignment happened, but the history explains balances, and this one did not move |

> ***Derived*, not asked: money planned for a later period comes out of the pool account as it is on
> the day the money moves**, not as it was when the money was assigned. It is *Planned money follows
> the backing on the day it moves* (below) applied to the source.

### Archiving does nothing to backing

> ***Follow-up*, 2026-09-27: archiving does nothing to backing.** The category stays backed and keeps
> *Opgebouwd*, which is shown wherever the archived row is shown, and in its slice's hover. **Its
> later budgets still move on their day.** It can be unbacked separately, which returns its money
> (*Backing can be set, changed or removed at any time*, above).

**Why:** archiving destroys nothing and changes no figure (*What the state fixes*, above); it only
takes the category out of new entry. A negative assignment to an archived backed category moves the
money back, as tidying up, like any negative assignment. This answers the backed half of *What
archiving does not settle, because it cannot yet* (above).

| Rejected | Why |
|---|---|
| **Archiving unbacks** | Archiving would move money, where it has never changed a figure |
| **An archived row shows no *Opgebouwd*** | The money is still there for the category, and the row would hide it |

> ***Ruled on 2026-09-27, after the build:* where the archived row would not otherwise be shown, it is
> shown anyway in the current period and every later one while its *Accumulated* there is not zero.**

**Why:** "shown wherever the archived row is shown" left a gap that the rejection above was meant to
close. An archived category with no history in the period on screen had no row, so its *Opgebouwd*
was hidden with the row, however much was built up, and it could be unbacked only from an earlier
period. Found by the documentation while the build's readings were written up (*Backing: ruled after
the build*, ruling 3, below; *When any category is shown in a period: the full rule*, above).

> ***Follow-up*, 2026-09-27, from the scenario stage: an archived category can be backed and
> re-pointed, and it stays archived.** *Staat op* works on an archived row as on any other. Backing
> is not one of the three acts that bring a category back (adding its name, recording an expense
> against it, assigning a positive amount to it), so it does not bring it back.

| Rejected | Why |
|---|---|
| **An archived row offers only "—"** | Unbacking would be possible and backing not, for no reason the user can see |
| **Backing brings it back, like a positive assignment** | A fourth route back, where the three ruled ones are all acts of using the category |

***Derived*, not asked:** deleting a category with no history takes its backing with it, and nothing
moves: with no budget above zero and no expense anywhere, its *Opgebouwd* is €0. **Since the ruling
after the build**, "no history" also means no movement between two different accounts standing for
it. Any movements from the pool account to itself go with the category (*Backing: ruled after the
build*, ruling 2, below).

### Backing, re-pointing and unbacking are announced, never confirmed

> ***Follow-up*, 2026-09-27: setting, re-pointing and removing backing are never confirmed, and are
> announced afterwards, naming what moved.**

The proposed wording is **copy, not a ruling**: *"Sparen staat nu op Spaarrekening: € 200,00
overgeboekt van Hoofdrekening."* **Why:** every act on a category or an account is announced
(*Archiving is announced, never confirmed*, *Managing accounts*, above), and each of these can move
money, which the user should not have to find in the strip. Nothing is lost by any of them, so by
"confirm only where a record is lost" (*Removing an entry asks first*, above) none asks first.

| Rejected | Why |
|---|---|
| **Quiet, with the balances as the only sign** | A silent act looks like a failure, and money would move without a word |

> ***Follow-up*, 2026-09-27, from the scenario stage: when backing or re-pointing moves nothing,
> the notice names only the backing** — *"Sparen staat nu op Spaarrekening."* — and says nothing
> about money. The sentence is copy; the shape is the ruling.

| Rejected | Why |
|---|---|
| **Say that nothing moved** | *"… er is niets overgeboekt"* reports a non-event |

### Proposed display terms for backing

Ruled on 2026-09-27 with *On screen: Staat op and Opgebouwd* (above). **Both are now rows of *Dutch
display terms* (below)**: they moved there with the build that gave `Tekst` their constants, by the
precedent of *Proposed display terms* under *Accounts and net worth* (above), because `TekstTests`
reads that table and holds `Tekst` to every row in it. This table is kept as the record of the
ruling.

| English (this project) | Proposed Dutch | Status |
|---|---|---|
| Backing account (the list on a category row that sets it) | Staat op | **Ruled** 2026-09-27: the stakeholder's phrase from round 2 |
| Accumulated | Opgebouwd | **Ruled** 2026-09-27, over *Gespaard* |

"—", for no backing, is a symbol rather than a term. The history row's *"Toegewezen aan …"* uses
*Toewijzen*, which is already in the table, and the sentence is copy. **A negative *Opgebouwd*
reuses *Rood*** (follow-up), which is already a row of *Dutch display terms*; its English cell,
"Overdrawn, and a negative net worth", gained "a negative Accumulated" with the build, because
`TekstTests` reads that table. The announcement's wording is copy.

### Backing: ruled after the build, 2026-09-27

After the build and its review, **five points were put to the stakeholder** on 2026-09-27, each as a
multiple-choice question with a recommendation. **He took the recommendation every time.** Four had
first been written down as the build's readings (*Backing: chosen in the build*, next). The fifth
was found by the documentation while writing those readings up. The reasoning is the
documentation's, offered with the recommendation, unless it is marked as his.

> **1. Re-pointing takes along the money assigned while the pool account backed the category.**
> What is there for a category counts its movements by direction, not by their accounts. When the
> pool account backs a category, money assigned moves from the pool to the pool: no balance changes
> and there is no history row, but the money is still there for the category. So re-pointing to
> another account moves it, and *Opgebouwd* stays true.

**Why:** it is what *Backing can be set, changed or removed at any time* ("re-pointing takes the
money along") and *The pool account may back a category* ("*Opgebouwd* still counts") already said,
applied to the pool account. **The approved plan's wording is superseded.** It said that a
pool-to-pool movement "counts in and out, so it adds nothing". The build did not follow it, and this
ruling confirms the build.

| Rejected | Why |
|---|---|
| **The plan's wording: a pool-to-pool movement adds nothing to what is there** | Re-pointing from the pool account to a savings account would move nothing, while *Opgebouwd* went on saying the money was set aside. That is the money-with-no-purpose that the revision of unbacking exists to prevent |

> **2. A category cannot be deleted while money moved on its behalf between two different accounts
> still stands.** *Archiveren* is still offered for it. **Money moved from the pool account to itself
> does not count**, and it is removed with the category when the category is deleted.

**Why:** a movement between two accounts is a row in both accounts' histories, naming the category.
The history is what explains those balances, so the category it names cannot go while those rows
stand. A movement from the pool account to itself is in no history and moves no balance, so nothing
is lost when it goes.

| Rejected | Why |
|---|---|
| **Delete the category and erase its movement rows too** | Balances could shift after the fact where a balance correction lies between the movement and today: removing an entry it holds changes its difference, and removing a later one changes the balance |
| **Any movement at all blocks deleting**, which was the first build | A movement from the pool account to itself shows nowhere and moves nothing, so it would block deleting for a reason the user cannot see. `spec-reviewer` found that the first build was too broad in this way |

**What this does to "no history".** It **extends** the definition in *Deleting a category that has
no history anywhere* (above) for backing: a movement between two different accounts is history, as
an expense is. **It does not reverse the rejection of "never touched".** A budget assigned and taken
back to zero is still no history, and a category that has only that can be deleted. What blocks is a
movement row that stands in an account's history **now**, which is the "unused means unused now"
reading applied to movements.

**Built:** `Ledger.CanDelete` reads `!movements.Any(m => m.Category == category && m.From != m.To)`
beside the expenses and the budgets above zero. `DeleteCategory` removes the category's remaining
movements, which can only be pool-to-pool, with its backing. A unit test holds the pool-to-pool case.

> **3. An archived backed category is also shown in the current period and every later one while
> its *Accumulated* in that period is not zero**, even with no budget and no expense there. Past
> periods are unchanged.

**Why:** money still there for a category is never hidden, and the category can be unbacked in the
period it is in, since *Staat op* is on its row. It is what *Archiving does nothing to backing*
(above) meant in rejecting "an archived row shows no *Opgebouwd*": the row would hide money still
there for the category. Under the old display rule the whole row did exactly that.

| Rejected | Why |
|---|---|
| **Keep the display rule as it was** | The money would show only by stepping back to a period where the category had history, or as part of an account's balance |

**Details.** A negative *Accumulated* counts, because "not zero" is the rule. Once the category is
unbacked it has no *Accumulated*, so the row goes. *Accumulated* follows today's backing (*Accumulated
covers everything up to the period on screen*, follow-up), so that happens in every period at once.
**Built:** `Ledger.CategoriesShownIn` adds
`|| (plannable && AccumulatedFor(c.Name, period) is { Cents: not 0 })`, where *plannable* means the
current period or a later one. A new scenario ends
[`show-accumulated.feature`](../../features/show-accumulated.feature): *An archived backed category
with money built up is shown in the current period and later ones, until it is unbacked*. It was
added after the scenario gate, with this ruling, so the five backing files now hold 63 scenarios and
85 cases.

> **4. One approved scenario line is amended.** In
> [`assign-to-a-backed-category.feature`](../../features/assign-to-a-backed-category.feature), *A
> negative amount assigned to an archived backed category…*, the last line asserted that
> *Accumulated* for Savings was 0.00 euro. **With his approval**, it now asserts that Savings should
> not be shown in the current budget period.

**Why:** after the pull-back the category has no history there and an *Accumulated* of zero, so by
the display rule, ruling 3 included, its row is not shown. The old line asserted a figure on a row
that is not there. **The step's fallback is removed.** It had checked the figure the row *would*
show when the display rule left the row out, and "Accumulated … should be N" now always reads the
row.

| Rejected | Why |
|---|---|
| **Keep the line and the step's fallback** | A *Then* about what is shown would read a figure that no screen shows |

> **5. Picking the account already shown does not make it stick. Accepted as built.** The expense
> form takes a write of the account its list already shows as the list writing back, and any other
> account as a pick. So picking the account already shown leaves the form following the category
> typed.

**Why:** the list writes back what it shows, on first show and whenever its items change (*Accounts:
chosen in the build*, above). Telling a deliberate pick of the shown account apart from that would
need the window to decide what a click means, and the Desktop decides nothing
([§11](11-risks-and-technical-debt.md), the Desktop row). The case it gets wrong changes nothing the
user sees at that moment. It matters only if he then types another category, when the account
follows it. Held by `FormTests`.

### Backing: chosen in the build, not put to the stakeholder

The rulings, the approved scenarios and the approved plan settle what each act does and what the
screen shows. Some details they leave open were settled while building. **These are the build's
readings, not rulings**, and any of them can be put to the stakeholder if he reacts to it. The
reasons are the documentation's. **Four readings first listed here were put to the stakeholder and
ruled** (*Backing: ruled after the build*, above): re-pointing from the pool account, deleting a
category money was moved for, picking the account already shown, and the step reading an archived
category's hidden row. They left this list. The rest stand as readings.

**What moves.**

| Reading | Why it was built this way |
|---|---|
| **Movements from an account to itself do not keep the account from being deleted either**, and go with it. They are made while the account was the pool account and backed a category | Ruling 2's reason applied to accounts (*Backing: ruled after the build*): such a movement is in no history and moves no balance, so blocking would refuse deleting for a reason the user cannot see. Found by the documentation after the rulings; not put to the stakeholder, since it follows from his reason |
| **Money moved from the pool account to itself is not named in the notice.** Backing a category with the pool account says only *"Sparen" staat nu op "Betaalrekening".* | Nothing moved that anyone can see, and it has no history row (*The pool account may back a category*, follow-up). A notice naming an amount "overgeboekt van" the same account would report a move with nothing to show for it. It is the ruling that a backing which moves nothing names only the backing, applied to a move that changes no balance |

**The screen, and when it settles.**

| Reading | Why it was built this way |
|---|---|
| **Choosing the backing already set does nothing at all.** No settling, nothing said, nothing saved, and the notice stays as the last act left it | Every row's *Staat op* list writes back its current choice on every redraw, which follows every act. If that counted as an act, each redraw would replace the last act's notice. `Ledger.SetBacking` recognises it before it settles, and `MoneyBudApp.SetBacking` says nothing for it. Checked by the headless run: the lists writing back on first show and when the accounts change moved no money and announced nothing |
| **Settling runs inside every act that changes the ledger, and the screen settles when it opens and on every tick**, keeping what moved. **An act refused straight after a period began leaves the moved money unsaved until the next change** | Settling before every act puts a new period's money in place before anything is done in that period. That includes a balance correction typed on its first day, which must hold the movement ([ADR 0009](../decisions/0009-movements-are-entries.md)). Settling at start and on the tick means that the balances in the strip are right without an act. **The gap:** a refused act saves nothing, so what its settling moved stays unsaved. Nothing is lost, because the next start settles the kept data the same way and with the same result: nothing can change while MoneyBud is closed. Saving on a refusal was rejected. It would break the rule that only an act that changes the ledger saves (*Watch out for* in `MoneyBudApp`, `Tell(changed:)`) |
| ***Opgebouwd* counts a period's *Budget* as planned until that period is settled** | Otherwise, for up to a minute after a period began, *Opgebouwd* would drop by that period's *Budget*: no longer counted as planned, and not yet moved. Counting it as planned until it moves keeps the figure steady across the boundary |
| **The *Staat op* list is on every row in every period.** It is compact, beside *Hernoemen*, with "—" for none, and the accounts follow the strip's order. ***Opgebouwd* is a caption under the *Budget*, *Uitgegeven* and *Resterend* figures**, with the *Rood* marker below zero, and it is in the ring's hole when the slice is pointed at | The scenarios left where the list shows to the plan. The backing is today's in every period (*Accumulated covers everything up to the period on screen*, follow-up), so the list means the same on every row, and hiding it in past periods would hide nothing that is different there. The caption follows the grey plan figure's precedent: under the figures, no new column (*Taking a plan over: chosen in the build*) |

**Copy.**

| Reading | Why it was built this way |
|---|---|
| **The wording.** Notices: *"Sparen" staat nu op "Spaarrekening": € 200,00 overgeboekt van "Betaalrekening".*, *"Sparen" staat niet meer op "Spaarrekening": € 200,00 teruggeboekt naar "Betaalrekening".*, and, when no money moved, only *"Sparen" staat nu op "Spaarrekening".* Movement rows in a history: *Toegewezen aan "Sparen" — van "A" naar "B"*, *Teruggezet van "Sparen" — …*, *"Sparen" staat op "X" — …*, *… staat niet meer op …* and *… staat nu op …*, with the amount in the row's own column | Copy, in `Tekst`, not rulings. The shape follows the rulings: every backing act names what moved, or only the backing when nothing did, and a movement row names its category and both accounts. Names are in plain double quotes, as the stakeholder asked after the accounts increment. The first row's example in *Moved money in the account's history* (above), *"Toegewezen aan Sparen — € 200,00"*, was an illustration and is superseded by this wording |

### What this section leaves open

**Nothing that was found while the rulings were written up is still unruled.** Fourteen points stood
here first, each with the documentation's reading and its alternatives. All fourteen were dealt with
the same day, 2026-09-27. The list is kept as a record of where each went; the ruling and its
reasoning are in the subsection it went to.

| Point raised | Where it went |
|---|---|
| 1. Which expenses count against *Accumulated*? | ***Follow-up***: those dated after the backing day, or on it and recorded after the backing (*Backing a category that already has money*) |
| 2. An expense dated before the backing, recorded, changed or removed later | ***Follow-up***, settled with point 1: it does not touch *Accumulated*, and the backing move stays what it was |
| 3. The pool account as a backing account | ***Follow-up***: allowed, no balance moves, *Opgebouwd* counts (*The pool account may back a category*). Which pool planned money comes from is *derived* there |
| 4. Archived backed categories | ***Follow-up***: archiving does nothing to backing (*Archiving does nothing to backing*) |
| 5. Unbacking and backing again in one period moved the same money twice, **a contradiction between rulings** | **Removed by the stakeholder's revision** of unbacking: the money returns to the pool account (*Backing can be set, changed or removed at any time*) |
| 6. Pulling money back after re-pointing or unbacking drew from the wrong account | **Removed by the same revision**: the money is where a negative assignment draws from |
| 7. A planned movement and a balance correction on the same day | ***Derived***, kept: recording order, like every entry (*Assigning to a backed category moves money*) |
| 8. A planned movement in the history before its day | ***Derived***, kept: not shown until its day (*Assigning to a backed category moves money*) |
| 9. *Opgebouwd* in periods when the category was not backed | ***Follow-up***: it follows today's backing (*Accumulated covers everything up to the period on screen*) |
| 10. A negative *Opgebouwd* | ***Follow-up***: the one marker, badge *Rood* (same subsection) |
| 11. An expense against a backed category put on another account | ***Follow-up***: it lowers *Opgebouwd* (*Spending against a backed category from another account*). What unbacking then moves was settled by a further follow-up: not *Opgebouwd*, but what is there for the category in the backing account |
| 12. The expense form's account when the category is typed | ***Follow-up***: it follows the category until the user picks one (*What the backing increment covers, and what waits*) |
| 13. Are backing acts announced? | ***Follow-up***: announced afterwards, never confirmed (*Backing, re-pointing and unbacking are announced, never confirmed*) |
| 14. Is an account used only by movements "used"? | ***Derived***, kept: yes (*Backing can be set, changed or removed at any time*) |

**What the revision of unbacking brought with it.** Four consequences were first written here as
derivations. **Two were then ruled** in follow-ups the same day, each on the recommendation:

- **What moves is what is there for the category**, not *Opgebouwd*: what MoneyBud moved into the
  backing account for it, minus its expenses paid from that account. So nothing is left behind with
  no purpose, which settles the derivation that €50 would be stranded (*Backing can be set, changed or
  removed at any time*).
- **Unbacking or re-pointing may overdraw the account the money leaves**, marked *Rood*, never
  blocked, because the money was spent on something else (same subsection).

**Two stay derived**, the documentation's reading, **open to contradiction at the scenario gate**:

- **The amount moved on unbacking or re-pointing is fixed** when something dated before it is
  recorded later, as the amount moved at backing is. A late expense from the backing account then
  leaves that account short.
- **"What is there for it" is what has moved by that day**, so money planned for a later period is
  not returned or taken along. It follows the backing on its own day.

Two more are *derived* with the follow-ups: which of the category's expenses count for the amount
moved (those paid from the backing account, by the same dated test), and that an expense paid from a
**third** account comes back to the pool account on unbacking, not to the account that paid
(*Spending against a backed category from another account*).

**Money from earlier periods returns to the pool with no purpose**, in no period's *Unassigned*.
That one is ruled: it was written down with the revision.

**Four more follow-ups came from the scenario stage**, 2026-09-27, each on the recommendation: a
negative assignment moves back at most what is there for the category (*Assigning to a backed
category moves money*); a movement from the pool account to itself leaves no row (*The pool account
may back a category*); an archived category can be backed and re-pointed and stays archived
(*Archiving does nothing to backing*); and a backing that moves nothing is announced without money
(*Backing, re-pointing and unbacking are announced, never confirmed*). One derivation came with them:
while the category box holds no category, the expense form's account is the pool account (*What the
backing increment covers, and what waits*).

**The one point left for the plan is answered**: how a movement for a later period is held, when
its destination is decided only on its day. It is written on that day by settling
([ADR 0009](../decisions/0009-movements-are-entries.md); *Planned money follows the backing on the
day it moves*, above).

**Five more points came after the build**, and all five were ruled on 2026-09-27, each on the
recommendation (*Backing: ruled after the build*, above). Four had been listed as the build's
readings, and one was a finding of the documentation:

| Point raised | Where it went |
|---|---|
| 15. Re-pointing from the pool account: the plan said a pool-to-pool movement adds nothing, and the build takes it along | **Ruled, as built**. The plan's wording is superseded |
| 16. Deleting a category money was moved for | **Ruled, narrowed**: movements between two different accounts block it; pool-to-pool ones do not, and go with the category |
| 17. An archived backed category's *Opgebouwd* hidden with its row, the documentation's finding | **Ruled, a new display rule**: shown in the current period and later ones while its *Accumulated* is not zero |
| 18. A scenario line asserting a figure on a row the display rule hides | **Ruled, amended**: the line now asserts that the row is not shown, and the step's fallback is gone |
| 19. Picking the account already shown does not stick | **Accepted, as built** |

**Nothing is open in this section.**

## Dutch source terms

The stakeholder material is in Dutch. This table fixes the mapping, so that reading the interviews
alongside this documentation does not introduce drift.

**This table is about vocabulary, not about content.** It fixes how the stakeholder's Dutch words map
onto this project's English terms. It says nothing about what language MoneyBud *displays* — the
**default categories** keep their Dutch names precisely because those are content the user reads
rather than terms this project reasons in (*The default categories* above).

| Dutch (stakeholder) | English (this project) |
|---|---|
| Potje | Category, together with its Budget for the current period. The Dutch word is a container metaphor — a little pot money sits in — and that part does **not** carry over: a Budget is a plan, not a pot (see *plan and actual* above) |
| Categorie | Category |
| Standaardcategorieën | Default categories — the set MoneyBud ships with. The *term* is translated; the six *names* in the set are not, because they are content (above) |
| Hem eruit halen | **Archiving** a category — taking it out of new entry while its history stays. The Dutch is literally "take it out", which sounds like deletion and is not; that mismatch is why the state needed a word of its own (*A category is taken out of use, not deleted* above) |
| Potje dat op een plek staat | Account-backed category. The *staat op* is the backing: this pot's money really is in that account |
| Plek | Location — expressed as an Account |
| Doel | Purpose — expressed as a Category |
| Rekening | Account |
| Saldo | Balance. In round 2 he can *"het saldo zelf vrij bijwerken"*, which since 2026-09-27 records a **Balance correction** rather than overwriting a number (*A balance is worked out from the entries*, above) |
| Betaalrekening, spaarrekening, aandelenrekening, contant | Examples of accounts, from round 2. Not account kinds: MoneyBud has none (*Managing accounts*, above) |
| Vermogen | Net worth |
| Inkomsten / Uitgaven | Income / Expenses |
| Waar het van is | What an income **is from** — carried by the income's **Label**, which is why that label is required. Not a category: it says what this money is, not what it is for |
| Overzichtelijk | Legible, clear at a glance — see quality goal 1 in [§1](01-introduction-and-goals.md) |
| Startscherm | The **Overview**, the screen MoneyBud opens on (*The overview, and its ring* above). In the interviews "overzicht" is also the everyday word for insight in general, which is not a screen |
| Radiaal diagram | The **Ring** at the head of the Overview |

## Dutch display terms

What MoneyBud **shows on screen** for each term. Approved by the stakeholder on 2026-09-25, for the
UI increment (*The UI is in Dutch*, above).

**This is not the *Dutch source terms* table**, and the difference matters. There are now three
kinds of Dutch in this glossary, and each fixes something different:

| | What it fixes | Direction |
|---|---|---|
| *Dutch source terms* (above) | How the stakeholder's words in the interviews map onto this project's terms. **Vocabulary** | Dutch → English |
| **Dutch display terms** (this table) | What MoneyBud displays for each of this project's terms. **Display** | English → Dutch |
| *The default categories* (above) | Six strings the app ships with. **Content**, which nothing translates | Dutch only |

A word can appear in more than one of them without that being a conflict. *Standaardcategorieën* is
the stakeholder's word for the default categories and also what MoneyBud displays for them. "Potje"
is his word, and MoneyBud displays *Categorie* and *Budget* instead.

| English (this project) | Dutch (on screen) |
|---|---|
| Expense | Uitgave |
| Income | Inkomst |
| Category | Categorie |
| Default categories | Standaardcategorieën |
| Budget period | Periode |
| Current period | Huidige periode |
| Budget | Budget |
| Assign | Toewijzen |
| Spent | Uitgegeven |
| Remaining | Resterend |
| Over budget | Over budget |
| Unassigned | Niet toegewezen |
| Over-assigned | Te veel toegewezen |
| Label | Omschrijving |
| Archive (the act) / Archived (the state) | Archiveren / Gearchiveerd |
| Brought back | Weer in gebruik |
| The shortfall of a clipped negative assignment | Niet teruggezet |
| Amount / Date | Bedrag / Datum |
| Previous / next period | Vorige / volgende periode |
| Add category / Record expense / Record income | Categorie toevoegen / Uitgave toevoegen / Inkomst toevoegen |
| Overview (the start screen) | Overzicht |
| Change (an entry) | Wijzigen |
| Remove (an entry) | Verwijderen |
| Rename (a category) | Hernoemen |
| Delete (a category) | Verwijderen |
| Take over (a plan) | Plan overnemen |
| Remembered figure | plan |
| Account / Accounts | Rekening / Rekeningen |
| Balance | Saldo |
| Net worth | Vermogen |
| Pool account | Hoofdrekening |
| Make (an account) the pool account | Maak hoofdrekening |
| Transfer (the record) / Transfer (the act) | Overboeking / Overboeken |
| A transfer's two accounts | Van / Naar |
| Starting balance | Startsaldo |
| Balance correction (the record) | Correctie |
| Correct a balance (the act) | Saldo corrigeren |
| Add account | Rekening toevoegen |
| Overdrawn, a negative net worth, and a negative Accumulated (the marker's badge) | Rood |
| Backing account (the list on a category row that sets it) | Staat op |
| Accumulated | Opgebouwd |

**"Nog toe te wijzen" is deliberately absent.** It is the literal Dutch for *Left to assign*, which
is retired: it was merged into *Unassigned* (*One figure, not two*, above). **The retirement holds
in both languages.** On screen the figure is *Niet toegewezen*, and below zero it is *Te veel
toegewezen*.

**A term not in this table has no display term yet.** *Account*, *Net worth*, *Leftover*, *Sweep*
and the other terms of the location dimension have nothing to display in this increment. Their
Dutch is fixed when they are built, not guessed ahead of it. **Since the accounts increment**
(2026-09-27), *Account*, *Balance*, *Net worth*, *Pool account* and the rest of that increment's
terms are in the table. *Leftover* and *Sweep* are still waiting. **Backing's two terms, *Staat op*
and *Opgebouwd*, were ruled on 2026-09-27** and came into the table with the backing increment's
build, which gave `Tekst` their constants, by the precedent below. *Rood*'s English cell gained "a
negative Accumulated" in the same build: the badge is reused, and no word was added (*Proposed
display terms for backing*, under *Backing and Accumulated*, above).

**The accounts increment's twelve rows came with its build** (2026-09-27). They were proposed and
approved in *Proposed display terms*, under *Accounts and net worth* (above), and held there until
`Tekst` had their constants, by the precedent below. The balance correction's record and act are
two rows here, as that section explains. *Hernoemen* and *Verwijderen* are reused for renaming and
deleting an account, and had rows already. *Sluiten*, which closes an account's history, is a
control word like *Opslaan* and *Annuleren*, and is not in the table.

**This table is read by a test.** `TekstTests` parses it from this file and holds the constants in
`Tekst` to it ([§8.4](08-crosscutting-concepts.md)). Changing a Dutch cell, or adding or removing a
row, fails the test suite until the code follows. Keep this section's heading and the table's header row
exactly as they are, because the test finds the table by them. The marker badges, the *Gearchiveerd*
caption and the *Niet teruggezet* notice all use terms from this table. No word was added for them.

**The four rows from *Change* to *Delete* came with the corrections increment** (2026-09-26). By this table's own
precedent, an act's button label is a display term: *Archiveren*, *Toewijzen* and *Categorie
toevoegen* are here. So the acts settled in *An entry can be changed or removed*, *Renaming a
category* and *Deleting a category that has no history anywhere* (above) needed rows: *Change* (an
entry) is **Wijzigen**, the entry form's state; *Remove* (an entry) is **Verwijderen**; *Rename* (a
category) is **Hernoemen**; and *Delete* (a category) is **Verwijderen** as well.

All four were **approved by the stakeholder on 2026-09-26**, as proposed. They were held out of the
table until the build, because adding a row fails `TekstTests` until `Tekst` has the constant, and
went in with the build that added the constants. **One Dutch word for two English terms is chosen,
not fallen into.** **Why**, in the documentation's reasoning, which he
chose: removing and deleting act on different things, an entry and a category, so *Verwijderen* is
never ambiguous on screen. This documentation keeps the two English
terms apart all the same, *remove* for an entry and *delete* for a category (*Remove* and *Delete*,
terms table), because in English "removing a category" already means archiving it.

*Opslaan* and *Annuleren* are the form's controls, not terms of the model, and nothing like them is
in the table. `Tekst` has them as constants all the same, beside the table's terms, and so is
*Weet je het zeker?*, which is copy. None of the three is held to this table.

**The last two rows came with opening a period.** Proposed on 2026-09-26 and held out of the table
until the build, by the same precedent as the corrections increment's four rows: adding a row fails
`TekstTests` until `Tekst` has the constant. The plan kept them as proposed, and they went in with
the build that added the constants. How each appears:

| English (this project) | As it appears |
|---|---|
| Take over (a plan) | The button: *"Plan van augustus 2026 overnemen (€ 1.450,00)"*. The period's name and the total are filled in, and the sentence around the term is copy |
| Remembered figure | In grey on a category row: *"plan: € 400,00"*. No label on a row whose category is not in the plan |

**The second row first read *vorige***, as in the example the stakeholder first saw. He ruled for
*plan* in a follow-up on 2026-09-26: it is true whichever period the figure came from, and it cannot
be confused with *Vorige periode*, the step button (*The offer is a button, and a figure on each
row*, above). The two rows share the word *Plan*, which is deliberate.

## Open questions

**One**, below. It is recorded so that it is not rediscovered late, and it is **not** waiting on an
answer from anyone: there is nothing yet for either answer to be true of (*Why it cannot be answered
yet*).

Everything else that has ever stood here has been answered, including the questions that arose while
the first increment was being built, the one raised by future-dated income rather than by an
interview, and the ones raised by the category model — what an added name does when an archived
category carries it, and whether an archived category can be brought back at all, which turned out to
be one question with one answer; and then, on 2026-09-25, what recording an expense against an
archived category does, how whitespace around a category name is treated, whose spelling wins
when a name is added again, in which periods an archived category is still shown, how inner
whitespace in a name is compared, and whether archiving is confirmed and announced. All of the
category answers are now **built**: the category increment shipped green against them. The
*Answered* table below says where each answer lives.

The category increment met two questions it did not need to answer, and they were never filed here
because each belonged to a later increment. The first was whether a category **in use** is shown in
a period where it has no history. The second was what **assigning** to an archived category does.
Both were **answered on 2026-09-25**, together with a third that belongs to the same later work:
whether an archived category's figure is offered back when a period opens. All three were
**decided** (*When any category is shown in a period: the full rule*, *Assigning to an archived
category brings it back*, *An archived category's figure is not offered back when a period opens*).
The assigning one was **built** in the assigning increment, and the display rule in the UI increment
([§8.1](08-crosscutting-concepts.md)). Carry-over was built last, in the opening-a-period increment.

Four more were answered on 2026-09-25 for the **assigning increment**: what it covers, which periods
can be assigned in, whether a negative assignment brings an archived category back, and what
assigning zero does. The increment was built to all four.

Nine more were answered the same day for the **UI increment**, which is now built to all of them:
what the UI covers, whether it shows periods other than the current one, whether it lets the start day be
changed, how the ring is drawn, how it shows overspending and over-assignment, what data it starts
with and keeps, and what language it displays (*The user interface*, above). Two followed from
those: whether the category box is free text, and what an empty ring shows. Eight more were
answered the same day while the UI scenarios were being written: what a new entry's date and an
assignment's period default to, whether assigning is offered in a past period, what the Overview
does after an entry lands in another period, whether there is a one-step way back to the current
period, what a category row shows, the order of categories and slices, the order of a period's
entries, and what happens when MoneyBud stays open past a period boundary. Three smaller points
they left open were answered the same day: "stays and says where it went" covers an assignment too;
a period's expenses and incomes are two lists, with the Overview's layout as the stakeholder's wish
alongside that answer; and a brought-back category keeps its original place in "order added". The
scenario writer's assumptions, and one consequence of staying open across a boundary, were approved
with the scenarios (*Approved at the scenario gate*, above). How a typed amount is read was ruled at
the plan gate. Five more points were settled after the spec review: an ambiguous amount is refused,
suggestions narrow on "contains", each marker badge names its own state, an archived category that
is still shown is captioned, and "alphabetical" means the invariant culture's order. Two more were
ruled on 2026-09-26, while `type-an-amount.feature` was written: four or more whole-cent digits after
the mark are not an amount, and a mark needs a digit on both sides. Five more came from the first
demo on 2026-09-26, with follow-up questions the same day: the category box empties after an entry
goes through, the ring is the middle column's centrepiece, hovering a slice shows its figures, every
slice has a minimum width, and the fields ask what before how much. Follow-ups the same day settled
what pointing at an over-budget or archived slice shows, and that the fill stays exact. How wide the
minimum is, and what happens when minimums squeeze the other slices, were left for the plan rather
than filed here. The plan gate settled both on 2026-09-26, together with where pointed-at figures
appear and where the *Unassigned* figure and the assign form went. All of it is built.

One question is **opened** by them rather than answered: whether an overdrawn account gets the
marker *Over budget* and *Over-assigned* now have. It cannot be answered before accounts exist, and
it is recorded where it arises (*Assigning may overdraw the pool account*, above), not filed here.
**Answered on 2026-09-27**, with the accounts rulings: it gets the marker, with a badge of its own
(*An overdrawn account carries the marker*, above).

Twelve more were answered on 2026-09-26 for the **corrections increment**: changing and removing an
entry, renaming a category, and deleting one with no history (*An entry can be changed or removed*,
*Renaming a category*, *Deleting a category that has no history anywhere*, above). Eleven came with
the first set of rulings. They left two points open, and the stakeholder settled both later the same
day, together with the Dutch words for the new acts. Fixing an expense already on an archived
category does not bring it back (*A changed entry is judged as if it were recorded now*). A past
period left *Over-assigned* by a correction stays that way, and that is accepted (*An entry in a past
period can be corrected*). Three more were ruled while the scenarios were written: a change and a
rename are announced, saving an unchanged entry is quiet, and declining to remove an entry is met
with silence (*Changes and renames are announced*, *Removing an entry asks first*). That makes
fifteen. The plan gate then settled what the form does around a correction, which had been left to
it (*On screen: picking an entry to correct*). All of it is built. The same rulings **widen** the
question below without answering it.

Ten more were answered on 2026-09-26 for the **persistence increment** (*What MoneyBud keeps*,
above): why keep data now, what is kept, when it is saved, where it lives, what happens when it
cannot be read, backups, one set of data or several, protection, whether a new version must read an
older one's demo data, and whether MoneyBud shows where its data is. All of it is built. What a first
start does was not asked again and stands. None of them touches the question below. Six follow-ups
were answered the same day: what the screen does when the data cannot be read, what a failed save
does, a second start while MoneyBud is open, what "everything" covers, what the unreadable-data
message says, and whether real use before accounts matters. The last extended how long saved data
may be dropped. Two more the same day refined the failed save: the "not saved" notice lasts, and
closing does not ask. Eight more came while the scenarios were being written: the notice stands
beside other messages, MoneyBud retries by itself, recovery is announced once, closing makes a last
attempt, a save that works says nothing, an interrupted save never damages the previous one, empty
data is unreadable, and the defaults come only with a first start.

Nine more were answered on 2026-09-26 for **opening a period** (*Opening a period*, above): when the
plan is offered, what "nothing assigned" means, which earlier period it comes from, what counts as
having a plan, in which periods it is offered, how it is shown, how long the figure on each row
stays, what taking it over does when *Unassigned* is smaller, and whether it can be undone. They
left five points open, and all five were settled in follow-ups the same day: the button acts on the
period on screen, an archived category's budget makes a period not empty, the offer goes quietly at
a boundary, the grey figure is labelled *plan*, and a row not in the plan shows no label. Three more
were ruled at the scenario gate, on the scenario writer's assumptions: taking over is not confirmed,
any *Budget* above zero makes a plan however small, and while the plan is offered the rows are
ordered by their plan figure. `take-over-a-plan.feature` was approved with them. All of it is built,
and none of it touches the question below.

Sixteen more were answered on 2026-09-27 for the **accounts increment** (*Accounts and net worth*,
above), one of them, transfers, on a second asking: the increment's scope, how a balance is kept,
what a typed balance includes, whether a starting balance is income, what a first start has, where
the account goes on the forms, where accounts and net worth are shown, transfers, whether an
overdraft gets the marker, what can be done to an account, which account can be the pool, whether a
transfer may be future-dated, what happens to saved data, what an account's history holds, when a
row names its account, and the Dutch for the pool account. Seven derivations were stated with them.
**All of it was built the same day** (*Accounts and net worth*, above). Writing them up raised seven more points. **Four were answered by the
stakeholder the same day**, each on the recommendation: a first start's Betaalrekening has no
starting balance, a transfer across a balance correction may change net worth and that is true,
deleting an unused account is never confirmed, and a balance correction shows the difference it
made. **Three were handled by the documentation**: a changed entry keeps its first recording moment
(derived), interest and investment value are the user's choice of act (usage, derived), and the
purpose of money the user already had is **deferred** to the backing increment's first stage (*A
question for the backing increment*, above). That last one is recorded where it arises rather than
filed here, because it has a stage to be asked at. None of them touches the question below, which
still waits for the sweep. Two more follow-ups the same day made the balance correction's difference
recomputed rather than fixed, and the overdrawn badge *Rood*. **Seven more were raised by the
scenario writer** and ruled the same day, each on the recommendation: a duplicate account name is
refused on adding, an empty starting balance means none, incomes and expenses are changed only from
the Overview, accounts are listed pool first then in the order added, a negative net worth is
marked, a starting balance's history row shows no difference, and the proposed Dutch is approved.

Eleven more were answered on 2026-09-27 for the **backing increment** (*Backing and Accumulated*,
above), each on the recommendation, with one confirmation besides: the increment's scope, when an
assignment's money moves, what purpose money the user already had carries (the question the accounts
increment deferred), what moves when a category is backed, whether backing can change and what
happens to money already moved, where planned money goes when backing changes first, whether
*Accumulated* survives unbacking, as of when it is shown, how backing is shown and set, whether a
first start backs *Sparen*, and where moved money shows. The confirmation is that an unbacked
category's money is assumed to be on the pool account. **Writing them up raised fourteen points**,
one of them a contradiction between two rulings. **All were dealt with the same day.** The
stakeholder **revised** what unbacking does, on his own idea: it returns the money, which removed
the contradiction and one more point. Eight follow-ups, each on the recommendation, settled nine
more: which expenses lower *Accumulated* (settling a second point with it), the pool account as a backing
account, archiving, *Opgebouwd* in earlier periods, a negative *Opgebouwd*, spending from another
account, the expense form's account, and announcing backing acts. Three stayed the documentation's
derivations. Two more follow-ups settled what the revision itself brought: unbacking and re-pointing
move what is there for the category in the backing account rather than *Accumulated*, and may
overdraw the account the money leaves. *What this section leaves open*, under *Backing and
Accumulated*, says where each went.
None of them answers the question below. The revision keeps one of its assumptions true: an unbacked
category's money is on the pool account, where the sweep will collect from, even after unbacking.

### What happens to an income back-dated into a period that has already been swept?

A budget period **ends but never closes** (*Ending versus closing a budget period*, above), so an
income can be dated into a period whose *Unassigned* has already been swept away. The pool that the
sweep emptied then gains money after the fact, and nothing settled so far says what becomes of it.

**The analogy that nearly answers it.** *A late expense against a leftover that has already been
directed* (above) is the same situation on the other layer, and it has an answer with a principle
behind it: **recalculate what MoneyBud owns, report what it does not.** The derived figure is
recomputed, the discrepancy is shown, and the real transfer is left for the user to correct, because
MoneyBud does not silently rewrite a record of money that really moved. It is tempting to carry that
straight across — sweep the extra too, report it — and it may well turn out to be right.

**Where the analogy stops being safe to lean on.** The two cases push the already-acted-on figure in
opposite directions, and that is not a detail of sign:

| | What the late transaction does to the figure the sweep acted on | What MoneyBud discovers |
|---|---|---|
| A late **expense** | **Reduces** a *Leftover* that has already been moved | It moved **too much** — the correct figure is smaller than the one it acted on |
| A late **income** | **Increases** an *Unassigned* that has already been emptied | There was **more to move** — the correct figure is larger than the one it acted on |

Discovering it moved too much leaves only the destination to correct. Discovering there was more to
move leaves a live amount that has to go somewhere, and two things about it are true at once that
are not true of the late expense: **the money was never given a purpose** — where the swept
*Leftover* had one, the category it was assigned to — and **the period it belongs to is over**, so
there is no act of assigning left to perform inside it. That is enough to make at least two answers
defensible, and they disagree about *which period's figures change*:

| Answer | What it says | What argues for it |
|---|---|---|
| **Sweep it too** | The money belongs to its own period, that period's pool has already gone to the sweep destination, so this follows it there and the user is told | Consistency with the late expense, and with *Nothing crosses a period boundary without a purpose*: money that sat purposeless across a boundary is precisely what the sweep exists for |
| **It belongs to the period you are in now** | The money still has no purpose and there is a live period in which to give it one, so it joins the **current** period's *Unassigned* instead | It is the only answer that leaves the money assignable at all. The other hands a still-purposeless amount to a destination the user never chose for *it*, in a period he has finished with |

**What the second answer would cost, which is why this is not a formality.** It would be an
exception to two things already settled. *Nothing crosses a period boundary without a purpose* says
the next period's pool is "that period's income and nothing else"; and
[`features/record-income.feature`](../../features/record-income.feature) already asserts, in
approved scenarios, that an income counts against the budget period its **date** falls in and that a
back-dated one raises that period's *Unassigned* without disturbing the current one. Those scenarios
do not decide this question — they are written against periods that have not been swept, and no
sweep exists to have run — but they do mean the second answer arrives as an exception to an approved
rule rather than as a free choice. The first answer costs nothing already written and is for that
reason the likelier outcome; it is not recorded as the answer, because "likelier" is not "decided",
and the two produce different figures for different periods.

**Why it cannot be answered yet.** There is no sweep in the code, no accounts, and therefore no
valid sweep destination at all — a destination must be *account-backed*, and with no accounts there
can be no backed category ([§11](11-risks-and-technical-debt.md)). In the demo the money a sweep
would move simply vanishes at the boundary, which the stakeholder accepted explicitly on the grounds
that the data is throwaway. So there is nothing to decide **against**: both answers describe what
happens to a movement that does not exist. **This becomes live when the sweep is built, and not
before** — it is filed here so that it is met then, rather than discovered afterwards by someone
looking at a swept period that has grown an income.

**Half of that reason is out of date since 2026-09-27**: accounts and backed categories are built,
so a valid sweep destination can now exist. The other half stands. There is still no sweep, so there
is still nothing to decide against.

It arose while the income scenarios were being written and was parked there rather than answered,
because nothing in the income increment reaches a sweep.

**Corrections join it** (2026-09-26). Once an entry can be changed or removed (*An entry can be
changed or removed*, above), a correction can **lower** a swept period's spending: an expense
removed, reduced, or moved out by a changed date. Then there was more to move, which is this
question's shape exactly. It is filed here with it, not answered. So, in the documentation's
reading, is a correction that **raises** a swept period's income. A correction that raises spending,
or lowers income, means MoneyBud moved too much, and that is the late-expense case instead
(*Corrections and the sweep*, above).

One further question about the period boundary is open **elsewhere**: how a configurable period
start day interacts with timezones ([§8.2](08-crosscutting-concepts.md), *Still open*). It is a
different question from the short-month one answered above — that one is about the calendar, this
one is about which instant a day begins at — and it is recorded there rather than here.

### Answered

Each answer is written up in the section it belongs to rather than kept in a list here:

| Question | Where the answer lives |
|---|---|
| Does assigning move real money? | *Account-backed categories* — exactly when the category is backed |
| What happens to leftover and unassigned money at a period end? | *The sweep* — one automatic movement into a preset backed destination, shown and reversible |
| Where does money MoneyBud moves itself come from? | *The pool account* |
| Does a backed category show a total across periods? | *Backed categories accumulate* — **Accumulated** |
| Is *Accumulated* a sum of *Budget* or of *Remaining*? | *Backed categories accumulate* — of *Remaining*, so it nets out spending. **Revised on 2026-09-27**: it still nets out spending, but it is no longer a sum over every period. It is what has moved in on the category's behalf since it was last backed, minus what has been spent against it since (*Re-backing starts Accumulated over*, under *Backing and Accumulated*) |
| Does an expense default to an account? | *An expense defaults to the pool account* — yes, overridable per expense, with a known weak spot recorded there and in [§11](11-risks-and-technical-debt.md) |
| May assigning overdraw the pool account? | *Assigning may overdraw the pool account* — yes, shown, not blocked |
| May an amount be assigned negatively, and may a *Budget* go negative? | *An amount may be assigned negatively, and a Budget floors at zero* — yes to the first, no to the second. What an over-large negative assignment does was answered in the same section before it was ever filed here as a question: it is clipped and the shortfall reported |
| What does a configured start day mean in a month too short to contain it? | *A start day the month is too short for clamps to its last day* — it clamps, with an unexplained 31-day period accepted as the cost |
| Does an income need a label, given it has no category? | *Income carries a label, and it is required* — yes, required, where an expense's stays optional |
| What is stored when a label has whitespace around it, and does a blank one count? | *A label is trimmed, and that is what makes "blank" mean anything* — surrounding whitespace is stripped and inner text left alone, on **both** labels. Trimming is the rule; refusing a blank income label is its consequence, because a label that trims to nothing is not a label |
| May an income be dated in the future, when an expense may not? | *Income may be dated in the future; an expense may not* — yes, and the asymmetry follows from the plan/actual split |
| Are *Unassigned* and *Left to assign* two figures or one? | *One figure, not two* — one, named *Unassigned*; *Left to assign* retired |
| What is a negative *Unassigned* called? | *Over-assigned* — the third member of the family with *Over budget* and *Overdrawn* |
| May a category be removed, and what happens to its history? | *A category is taken out of use, not deleted* — it is **archived**: out of new entry, history intact, still shown wherever it has history (next row) |
| In which periods is an archived category still shown? | *Where an archived category is still shown* — in **every period where it has history** (a budget or an expense), **including the current one**, and in no period where it has none. Decided 2026-09-25 over "past periods only", which would leave current-period expenses unshown. This replaces "still shown in past periods", which the earlier answer said. **Extended on 2026-09-27**: an archived **backed** category is also shown in the current period and later ones while its *Accumulated* there is not zero (*Backing: ruled after the build*) |
| Does a budget of 0 count as history? | Same section — **no**, not when nothing is spent. A zero budget behaves exactly like no budget (*Budget*), so it cannot decide whether a row appears. Derived, then **confirmed** by the stakeholder on 2026-09-25 |
| Does archiving ask for confirmation, and is the user told? | *Archiving is announced, never confirmed* — **never asked**, because it destroys nothing and adding the name undoes it. The user is **told afterwards**, like every other category act (created, already there, brought back, archived). Chosen over confirming always and over confirming only when there is history. Settled 2026-09-25 |
| What is that state called? | Same section — ***Archived***, chosen over *Removed*, *Closed*, *Hidden* and *Retired*, the decisive one being that *Closed* is already spent on a period state MoneyBud deliberately has not got |
| What happens when the user adds a name an **archived** category carries, and can an archived category be brought back at all? | *Adding an archived category's name brings it back* — one question with one answer. It comes back with its history and the user is told it was **brought back** rather than created; there is no separate act of un-archiving, on the same argument that leaves assigning without a separate "unassign". Adding the name was the only way back when this was answered; recording an expense is now a second (next row) |
| What about an expense remembered late, whose category has since been archived? | *Recording an expense against an archived category brings it back* — the expense is **recorded**, the category is **brought back** and the user is told so. A **decision**, chosen over refusing and over recording while leaving it archived. It **replaces** an earlier answer here — "nothing extra", derived on the assumption that recording against a category means adding its name, which it does not: an expense naming a category you do not have is refused. It leaves two routes back and still no separate un-archive act |
| Does an expense that is refused for another reason still bring its archived category back? | Same section — **no**. Bringing back is a side-effect of recording, so where nothing is recorded the category stays archived. Derived, then **confirmed** by the stakeholder on 2026-09-25 |
| Are category names case-sensitive? | *A category name is compared case-insensitively and stored as typed, trimmed* — compared without case, kept as typed. A **correction**: the code was case-sensitive by accident, not by decision. The category increment made the correction ([§8.1](08-crosscutting-concepts.md)) |
| What about whitespace around a category name, and a blank one? | Same section — **trimmed** at the ends, so "  Hobby  " is "Hobby" and the same category as "hobby"; a name that trims to nothing is **refused**. The label rule's whitespace half, with the required income label's consequence |
| What about whitespace inside a category name? | Same section — **ignored for comparison, kept as typed**. Any run of inner whitespace counts as one space, so "Vaste  lasten" is the same category as "Vaste lasten", and adding it hands back the existing one spelled as it was. Chosen over "inner text untouched, so two categories", which was only the literal consequence of the first wording. Settled 2026-09-25. This answer first said "inner text left alone", which is still true of what is stored |
| Does trimming and ignoring case apply when recording an expense against a name, or only when adding one? | Same section — **both**. Recording against "  groceries " records against Groceries. **Confirmed** by the stakeholder on 2026-09-25, not only inferred from "every comparison". When answered, it changed the code's behaviour, which refused that expense as naming an unknown category. It is now built and asserted ([§8.1](08-crosscutting-concepts.md)) |
| What happens when the user adds a name they already have? | Same section — they get the existing category back, and are told so. Derived and not contradicted. It does **not** answer the archived case, which *Adding an archived category's name brings it back* answers separately |
| Does adding a name in a different capitalisation change the existing category's spelling? | Same section, *The existing spelling is kept* — **no**, for an active category, an archived one brought back by adding its name, and one brought back by recording an expense against it. Taking the new spelling would be a rename by the back door |
| May a category be renamed? | *Renaming a category* — **yes**, since 2026-09-26. It was first deferred with its two questions named (*Renaming a category is not in this increment*), and both are now answered: past periods show the **new** name, and a name **another** category has, archived ones included, is **refused**. Chosen over the old name in old periods and over merging. The category's own name in a new spelling is allowed, which is the front door to what adding kept out as "a rename by the back door". Same name rules as adding. An archived category can be renamed and stays archived. Settled 2026-09-26; built in the corrections increment |
| May a category be deleted? | *Deleting a category that has no history anywhere* — **only one with no history in any period**: no expense, and no budget of more than zero. Chosen over a stricter "never touched" rule, which would have made "assigned, then taken back to zero" a state. A separate act from archiving, with its own button on such a category only, chosen over one button with MoneyBud picking. **Not confirmed**, and announced afterwards. The recommendation was to leave deleting out, since archiving already hides such a category everywhere; the stakeholder kept it in, in his own words *so that it is gone for good and its name is free again*, because an archived category keeps its name. Settled 2026-09-26; built in the corrections increment |
| Which categories does MoneyBud ship with? | *The default categories* — six, Dutch, a starting set chosen to be tried. *Sparen* ships unbacked until accounts exist. **Revised on 2026-09-27**: it ships unbacked for good, and the user backs it once he has added a savings account (*Sparen ships unbacked, and that is temporary*) |
| Is a category in use shown in a period where it has no history? | *When any category is shown in a period: the full rule* — **yes in the current and future periods**, because those are the periods you plan; **no in a past one**, which is a record of what happened. A category is shown in P if it has history in P, or if it is in use and P is current or later. Chosen over "every period, always" and "only with history, always". Settled 2026-09-25; built in the UI increment |
| What does assigning to an archived category do? | *Assigning to an archived category brings it back* — the category is **not offered**, and assigning to its name anyway **brings it back**, announced, like recording an expense. A third route back, and still no un-archive act. Chosen over refusing and over offering it. Settled 2026-09-25; built in the assigning increment, which also retired the `SetBudget` scaffold that did not do this ([§8.1](08-crosscutting-concepts.md)). Refined the same day: only a **positive** assignment brings it back (next rows) |
| Does a negative assignment to an archived category bring it back? | *Only a positive assignment brings it back* — **no**. Pulling its money out is tidying up, not planning for it, and the category stays archived; an over-large one is still clipped and the shortfall reported. Chosen over one rule regardless of sign, which would make reclaiming an archived category's budget a bring-back followed by a second archive. Settled 2026-09-25; built in the assigning increment |
| May zero be assigned, and does it bring an archived category back? | *Assigning zero is accepted and moves nothing* — **accepted**, changes nothing, and does **not** bring an archived category back. Deliberately unlike a zero expense or income, which is refused because it records a transaction that never happened. Chosen over refusing it. Settled 2026-09-25; built in the assigning increment |
| In which budget periods can an amount be assigned? | *Assigning happens in the current budget period and later ones, never in a past one* — current and later, future ones without limit. It matches the display rule's split between planned periods and past records, and keeps assigning out of the sweep's territory. A past-period assignment is **refused**: first derived, then confirmed. Chosen over "any period, since periods never close" and over moving the assignment into the current period. The cost, that a forgotten plan cannot be fixed after its period, was accepted: "past is past". Settled 2026-09-25; built in the assigning increment |
| Which refusal is reported when an assignment breaks several rules, and is a zero or clippable negative still refused for a wrong target? | *When an assignment is refused* — blank name, then unknown name, then finer than a cent, then past period, the same order as recording an expense. **Yes**: the amount rules and the target rules are separate, so 0 or −500 in a past period is refused, not accepted or clipped. Settled 2026-09-25; built in the assigning increment |
| What does the assigning increment cover? | *Nothing here blocks the assigning increment* — assigning in current and later periods, *Over-assigned*, the clipped shortfall and bringing back. Backed categories, the pool account, one-action carry-over, the sweep and any UI are out, each for its own reason. Settled 2026-09-25; built to that scope |
| Is an archived category's figure offered back when a period opens? | *An archived category's figure is not offered back when a period opens* — **no**. You put it away, so MoneyBud does not suggest planning for it. If it is brought back, it is assigned to like any other. Settled 2026-09-25; built with carry-over in the opening-a-period increment |
| Must category names in feature files be English? | *They are Dutch because they are content* — **no**. A name is test data because it is synthetic and no scenario leans on the defaults, not because of its language. Settled 2026-09-25, loosening the earlier "English, with the rest of the specification" |
| What does the UI cover? | *It covers what the domain does, and nothing more* — everything the domain already does, in the stakeholder's words "all that is currently working on the backend", all three routes back included. No editing or deleting of a transaction, because the domain has neither; that consequence was **accepted** by the stakeholder for the demo, and correcting entries becomes its own later increment ([§11](11-risks-and-technical-debt.md)). Settled 2026-09-25; built in the UI increment. **That later increment was settled on 2026-09-26** (*An entry can be changed or removed*), and built the same day, so the UI now covers correcting too |
| Is the category box a pick-list or free text? | *Category entry is free text with suggestions* — **free text**, suggesting the offered categories and accepting any name. Chosen over a pick-list alone, which would put two routes back and the unknown-name refusal out of reach. "Not offered" means not suggested. Settled 2026-09-25; built in the UI increment |
| What does the ring show when there is nothing to draw? | *The overview, and its ring* — an **empty grey outline with a hint**, only when the period has neither income nor any *Budget*. The hint's wording is copy. Settled 2026-09-25; built in the UI increment |
| Does the UI show periods other than the current one? | *Stepping between periods* — **yes**, back and forward, because entries land in past and future periods and a current-only UI would accept entries it could never show. This is where the display rule got built, as `Ledger.CategoriesShownIn`. Settled 2026-09-25; built in the UI increment |
| Can the UI change the period start day? | *The period start day stays at the 1st, for now* — **not in this increment**. The stakeholder's ruling was "if the backend is ready, yes"; it is not, because budgets are stored against their period's first day. Deferred, not rejected; the configurable-start-day rule stands. Settled 2026-09-25 |
| What does the start screen show, and how is the ring drawn? | *The overview, and its ring* — the Overview, headed by a ring with one slice per category, sized to its *Budget* and filled as far as spent, plus a slice for *Unassigned*. Chosen over a ring of spending only and a ring of budgets only. Settled 2026-09-25; built in the UI increment |
| How does the ring show an overspent category, spending with no budget, and an over-assigned period? | Same section — an overspent slice stays budget-sized, filled and marked; spending with no budget gets no slice and is listed with the marker; an over-assigned ring shows budgets only, with *Unassigned* marked. One marker for all of them, beside the **negative figure itself**, not a positive "over by". The marker is a **revision** by the stakeholder of the earlier "plain negative figure, unremarked", for *Over budget* and *Over-assigned* only; how *Overdrawn* is shown is not settled. The over-assigned answer was chosen over a ring at income size with an overflowing segment. Settled 2026-09-25; built in the UI increment. "The same marker" was confirmed after the review to mean the same look, with each badge naming its own state (next rows) |
| What data does the UI start with, and is it kept? | *What the UI starts with, and what it keeps* — the six default categories and nothing else, and nothing is kept on close. Chosen over synthetic demo data and over saving to a file. [§8.3](08-crosscutting-concepts.md) stands. Settled 2026-09-25; built in the UI increment. **Its second half is superseded on 2026-09-26** (*What MoneyBud keeps*): MoneyBud keeps its data, by the stakeholder's choice rather than §8.3's trigger. Built the same day. The starting state stands |
| What language does MoneyBud display, and in which terms? | *The UI is in Dutch* and *Dutch display terms* — Dutch, in terms the stakeholder approved. "Nog toe te wijzen" is not used, because *Left to assign* is retired in both languages. Settled 2026-09-25; built in the UI increment |
| What do a new entry's date and an assignment's period default to while a period is on screen? | *Defaults, and entering while another period is shown* — an expense's or income's date defaults to **today**, whatever period is shown; assigning defaults to the **period on screen**. Entries are recorded as they happen, and assigning is planning the period being looked at. Settled 2026-09-25; built in the UI increment |
| Is assigning offered while a past period is shown? | Same section — **yes, and it is refused with its reason**. With the default above, that is how the past-period refusal is reached from the UI. Chosen over not offering it. Settled 2026-09-25; built in the UI increment |
| What does the Overview do after an entry lands in a period other than the one on screen? | Same section — it **stays** on the period it showed and **says which period** the entry went to. Chosen over jumping to that period and over staying silent. Settled 2026-09-25; built in the UI increment |
| Does that cover an assignment made into a period other than the one on screen? | Same section, *Assignments are covered too* — **yes**, the same as expenses and incomes: the Overview stays, and MoneyBud says which period the assignment went to. Settled 2026-09-25; built in the UI increment |
| Is there a one-step way back to the current period? | *No one-step way back to the current period* — **no**. Stepping back and forward is the only way to move. *Huidige periode* stays as a label, not an action. Chosen over a "Huidige periode" action. Settled 2026-09-25; built in the UI increment |
| What does a category row on the Overview show? | *What each category row shows* — its *Budget*, *Spent* and *Remaining*, with the marker when over budget. Chosen over *Remaining* only. Settled 2026-09-25; built in the UI increment |
| In what order are categories listed and slices drawn? | *The order of categories and slices* — **largest *Budget* first**, ties in the **order added**, and the *Unassigned* slice **always last going clockwise**, the ring running clockwise. Chosen over order added and alphabetical for the main order, over alphabetical for ties, and over sorting *Unassigned* by size or putting it first. The order changes as you assign. Settled 2026-09-25; built in the UI increment |
| In what order are a period's entries listed? | *A period's entries are listed newest first* — newest date first, and on the same date newest-recorded first. Chosen over oldest first. Settled 2026-09-25; built in the UI increment |
| Are a period's expenses and incomes one list or two? | Same section — **two**, "definitely", each ordered within itself, so no recording order across both is needed. Settled 2026-09-25; built in the UI increment |
| How is the Overview laid out? | *The Overview's layout: income, plan, expenses* — the stakeholder's wish: income on the left, the plan (ring and category rows) in the middle, expenses on the right. Presentation, so fixed in this glossary and not in the scenarios. Given 2026-09-25; built in the UI increment |
| Where does a brought-back category fall in "order added"? | *The order of categories and slices* — in its **original place**. Bringing back is not adding again. First the documentation's reading of the code, then confirmed by the stakeholder on 2026-09-25; the code already does this |
| In what order do the six default categories count as added? | Same section — **in the order the stakeholder listed them**: Boodschappen, Huur, Hobby, Sparen, Verzekeringen, Abonnementen, which is their order at a first start. Follows from order added and the list being his. Settled 2026-09-25 |
| In what order are category suggestions listed? | *Category entry is free text with suggestions* — **alphabetically**. Chosen over the Overview's order and over the order added. Settled 2026-09-25; built in the UI increment. **Refined after the review**: alphabetical means the invariant culture's order with case ignored, so "Één" sorts among the E's, not after Z as ordinal comparison put it. Still independent of the machine's language |
| Is anything announced when a new period begins while MoneyBud is open? | *Staying open across a period boundary* — **no**. Only the label of the period on screen changes: it stops being labelled as the current period. Chosen over a short notice. Settled 2026-09-25; built in the UI increment |
| What happens when MoneyBud stays open past a period boundary? | *Staying open across a period boundary* — the screen **stays on the period it showed**, which has now become the previous period, so assigning in it is refused from then on. Chosen over following today into the new period. Settled 2026-09-25; built in the UI increment |
| How is typed text read as an amount? | *Typing an amount* — a comma or a point is the decimal mark, no thousands separator is accepted, and text that is not an amount is refused before anything is recorded. A euro sign, a true minus sign and surrounding spaces are tolerated. Ruled at the plan gate, 2026-09-25; built in the UI increment; held by `type-an-amount.feature` since the scenario gate of 2026-09-26 |
| Is "2.000" two thousand or two euros? | Same section — **neither: it is refused as ambiguous**, naming both readings. Any point or comma followed by exactly three digits ending in 0 is, because both readings are whole cents and nothing downstream could catch the wrong one. "1.832" still reaches the domain and is refused as finer than a cent. Found by the spec review; settled after it on 2026-09-25; built |
| Is "2.0000" an amount? | *Typing an amount* — **no**. Four or more digits after the mark that are all whole cents are not an amount: the same silent shape as "2.000", in a form MoneyBud never shows. A four-or-more tail that is not whole cents, "12,3450" included, is left to the cent rule. Ruled 2026-09-26; built, and held by `type-an-amount.feature` |
| Is ",50" fifty cents? | Same section — **no**. A mark needs a digit on both sides, so ",50" and "12," are not amounts. Chosen over reading ",50" as 0,50. Ruled 2026-09-26; built, and held by `type-an-amount.feature` |
| Do the suggestions narrow as you type? | *Category entry is free text with suggestions* — **yes, on "contains"**: "schap" finds Boodschappen. Chosen over "starts with" and over not narrowing. Settles what `suggest-categories.feature` left open. Case and whitespace runs are ignored, ordinally. Settled after the review, 2026-09-25; built in the presentation layer and held by unit tests, with no scenario, which the stakeholder did not ask for |
| Is a marker that reads *Over budget* in one place and *Te veel toegewezen* in the other still "the same marker"? | *One marker for over budget and over-assigned* — **yes**: the same look, with each badge naming its own state. Confirmed after the review, 2026-09-25 |
| How does the Overview show an archived category that is still shown? | *Where an archived category is still shown* — with a **Gearchiveerd** caption, and **no archive button**. Built that way, seen after the review, and kept by the stakeholder, 2026-09-25 |
| Does the category box empty after an entry? | *Category entry is free text with suggestions* — **yes, after an expense or an assignment goes through**, and it keeps what was typed after a refusal, like every other field. It replaces the old behaviour of keeping the category. The "groc" symptom did not reproduce, and that expense was most likely recorded against Groceries. Ruled at the first demo, 2026-09-26; built |
| How large is the ring? | *The Overview's layout* — **the centrepiece of the middle column**, taking most of its height and growing with the window. Below it come the assign form, the rows and adding a category, and the *Unassigned* figure sits in the ring's hole. Ruled at the first demo and at the plan gate, 2026-09-26; built |
| Does the ring respond to the pointer? | *Hovering a slice shows its figures* — **hovering** a category slice shows its category, *Budget*, *Uitgegeven* and *Resterend*, and the *Unassigned* slice shows its name and figure. Chosen over clicking to select and over clicking to zoom. A hover tells you everything its row does: an over-budget slice shows the negative *Resterend* with the marker, and an archived category's slice shows *Gearchiveerd*. The figures appear in the ring's hole, which shows *Unassigned* when nothing is pointed at, and the slice pointed at is highlighted. Ruled at the first demo, in follow-ups and at the plan gate, 2026-09-26; built |
| How do small budgets stay visible in the ring? | *Every slice has a minimum width* — **a minimum width for every slice**, *Unassigned* included, so the ring is no longer drawn exactly in proportion. A **revision** of the approved ring. Chosen over keeping the ring exact and over names beside the ring. The fill stays **exact**, with no minimum fill, chosen over a visible sliver for any spending. The minimum is **2%**. Slices below it are drawn at 2% and the rest share what is left in proportion, repeated until none falls below, with *Unassigned* counted like any other slice. So a larger *Budget* is never drawn narrower than a smaller one. At 50 slices or more, all are drawn equal. Ruled at the first demo, in follow-ups and at the plan gate, 2026-09-26; built |
| In what order does a form ask for its fields? | *The fields ask what before how much* — the "what" before the amount: expense *Omschrijving*, *Categorie*, *Bedrag*, *Datum*; income *Omschrijving*, *Bedrag*, *Datum*; assigning *Categorie*, *Bedrag*. Ruled at the first demo, 2026-09-26; built, and held by `WindowMarkupTests`, which reads the window's markup |
| Does expected money have a location, given that future-dated income is *Unassigned* before it arrives? | *The central distinction* — the question does not arise: expected income is **not money yet**, so there is no euro to lack a location, and the rule survives untouched. What it does cost is stated there and under *Net worth*: net worth is a point-in-time figure that excludes expected income, *Unassigned* is a period figure that includes it, and the two disagree by design |
| Can an entry be changed or removed, and in which periods? | *An entry can be changed or removed* and *An entry in a past period can be corrected* — **yes, both, in any period**, past ones included. An entry is a fact, and "past is past" is about plans. Chosen over refusing corrections in a past period. Settled 2026-09-26; built |
| Does removing an entry ask first? | *Removing an entry asks first* — **yes**, the only act that does, because it destroys a record. Chosen over removing then offering undo, and over removing and just saying so. The principle that leaves archiving unconfirmed, confirm only where a record is lost, gives this answer. Settled 2026-09-26; built |
| May removing or lowering an income leave its period over-assigned? | *Removing or lowering an income may leave its period over-assigned* — **yes**, shown with the existing marker and nothing more. Chosen over saying so in the message and over refusing it. Settled 2026-09-26; built |
| How is a changed entry judged? | *A changed entry is judged as if it were recorded now* — **exactly as if typed in fresh now**, and a refused change leaves the entry as it was. Changing an expense's category to an archived category's name brings it back, announced. Chosen over the same rules with the category staying archived. Fixing an expense that is **already on** an archived category does **not** bring it back, because that is correcting history, not using the category again. Chosen over bringing it back strictly as a fresh recording would. Settled 2026-09-26; built |
| May a correction leave a past period over-assigned for good? | *An entry in a past period can be corrected* — **yes, accepted**. It is the true figure, the marker shows it, and it is the "past is past" cost already accepted for assigning. Chosen over allowing a negative assignment in a past period, which would have reopened that ruling. First derived, then put to the stakeholder and accepted, 2026-09-26; built |
| What happens when a changed date moves an entry to another period? | *A changed date can move an entry to another period* — as for a new entry landing elsewhere: the screen **stays** and says where the entry went. Chosen over the screen following the entry. Settled 2026-09-26; built |
| Is a change a new record or a rewrite? | *A change overwrites the entry* — a **rewrite**. MoneyBud keeps no record of what the entry was, and shows no history. A default the stakeholder accepted, 2026-09-26; built |
| What happens to an archived category when its last expense in a period is removed? | *Correcting can change where a category is shown* — it **drops out of that period**, unless a budget of more than zero keeps it there. Derived from the history rule, then accepted by the stakeholder, 2026-09-26; built |
| How is an entry picked for correcting? | *On screen: picking an entry to correct* — clicking its row loads it into its entry form, in a *Wijzigen* state with *Opslaan*, *Annuleren* and *Verwijderen*. Chosen over icons with a dialog and over editing inline. Settled 2026-09-26; built |
| What does the form do after *Opslaan*, *Annuleren* or a removal, and when the screen steps or another row is clicked? | *On screen: picking an entry to correct* — it **empties and goes back to recording** after a change goes through (unchanged included), after *Annuleren* and after a confirmed removal; after a refused change it **keeps what was typed** and stays in *Wijzigen*. **Stepping** drops an entry being changed and a waiting question, and **keeps a new entry** being typed. **Clicking another row** loads that one. Declining the question leaves the entry loaded, and the question is asked inline in the message bar. Proposed by the plan and approved at the plan gate, 2026-09-26; built, with the stepping rule corrected after the spec review. What else drops a waiting question or a rename in progress was chosen in the build (*Chosen in the build, not put to the stakeholder*) |
| Are a change and a rename announced? | *Changes and renames are announced* — **yes, both**, afterwards, because a silent act looks the same as a failure. Chosen over announcing neither and over announcing only a rename. Settled 2026-09-26; built |
| What happens when an entry is saved with nothing changed? | Same section — **never refused, and quiet**: nothing changes, nothing is announced, and the form returns to normal. Chosen over saying so. So the amount loaded into the form must be one the amount box accepts. Settled 2026-09-26; built |
| What is said when the user declines to remove an entry? | *Removing an entry asks first* — **nothing**. The entry stays. Chosen over saying it was kept. Settled 2026-09-26; built |
| What happens when a correction lands in a period already swept? | *Corrections and the sweep* — nothing is decided, because there is no sweep. A correction that means MoneyBud moved too much meets the late-expense rule. One that means there was more to move **joins the open question** above |
| Why keep data now, when §8.3's trigger has not fired? | *Why now: demo now, real soon* — **the stakeholder's choice**, to save re-entering between sessions. MoneyBud is still a demo, and he expects to switch to real use not long after this is built. What expires "when the demo stops being a demo" does not expire yet. It was first written up as due at a now-foreseeable switch to real use; softened the same day, because his data may be dropped at least up to and including the accounts increment, so no increment before then plans around the switch (*Real use before accounts*). Settled 2026-09-26; built |
| What is kept, and for how long? | *Everything is kept, for good* — **everything, as one continuous history**, with no fresh start per year, because periods never close and the model has no yearly boundary. Chosen over a yearly fresh start. Settled 2026-09-26; built |
| When is data saved? | *Saved by itself, after every change* — **automatically, after every change**, with no save button, because a forgotten save loses entries and a save button is entry friction. Settled 2026-09-26; built |
| Where does the data live? | *In a fixed place, never in the repository* — **a fixed place in the user's profile**, never chosen by the user and always outside the repository, which is public. Chosen over a file the user opens and saves wherever he likes. Settled 2026-09-26; built, in `%LOCALAPPDATA%\MoneyBud` on Windows, a folder the plan chose |
| What happens when the data cannot be read? | *When the data cannot be read, MoneyBud says so and touches nothing* — whether damaged or written by a newer version, MoneyBud **says so and leaves it untouched**. Chosen over starting empty, which automatic saving would turn into overwriting the history. Settled 2026-09-26; built |
| Does MoneyBud keep backups? | *Backing up is the user's business* — **no**. One set of data; backing up happens outside MoneyBud. Offered keeping earlier copies, and declined; no recommendation was made on this one. Settled 2026-09-26 |
| Can there be more than one set of data, or a reset? | *One set of data, and no way to reset it in MoneyBud* — **one set, and no reset act**: starting over means deleting the file. Chosen over a start-over button and over separate budgets side by side. Settled 2026-09-26; built |
| Is the data protected? | *The login is the protection* — **the Windows login is enough**: no password, no encryption, because the data is local and a forgotten password would lock the user out for good. Settled 2026-09-26 |
| Must a new version read demo data an older one saved? | *Demo data may not survive a new version* — **not until real use starts**. Until then an unreadable file is met by saying so and touching nothing, and the user deletes it and starts fresh. Carrying data across versions is a requirement from the switch to real use. Chosen over carrying data over from the first file saved. Settled 2026-09-26. **Extended the same day**: not before the version that adds accounts either (*Real use before accounts*) |
| Does MoneyBud show where its data is? | *Where the data is, is written in the README* — **no**: it is documented in the README only, not shown on screen and not named in the message that the data cannot be read. The recommendation was to show it, on screen and always when the file cannot be read; he declined both parts. Settled 2026-09-26; built |
| What does MoneyBud do after saying it cannot read its data? | *When the data cannot be read, MoneyBud says so and touches nothing* — **it closes**, so nothing can be entered and nothing typed can be lost unsaved. Chosen over opening with nothing enterable and over opening usable but unsaved. Settled 2026-09-26; built |
| What happens when a save fails? | *When a save fails, MoneyBud says so and keeps going* — MoneyBud **says the change was not saved and lets the user carry on**, and every later change tries again to save everything. Closing before a save succeeds loses the unsaved changes, accepted with that consequence. Chosen over undoing the change and over refusing entries until a save succeeds. Settled 2026-09-26; built |
| Does the "not saved" notice go away when the next message appears? | *When a save fails, MoneyBud says so and keeps going* — **no**: it stays on screen until a later save succeeds, as a lasting state, so the next message cannot hide it and nobody closes MoneyBud without knowing. Chosen over saying it once, like any message. How it sits beside the one-message rule was left to the plan and scenarios, and answered in the next row. Settled 2026-09-26; built |
| Does closing with unsaved changes ask first? | Same section — **no, it just closes**, and the unsaved changes are lost, as already accepted. The lasting notice is the warning: MoneyBud shows, it never blocks. Chosen over asking before closing. Settled 2026-09-26; built |
| Can other messages hide the "not saved" notice? | *When a save fails, MoneyBud says so and keeps going* — **no**: it is shown beside any other notice and beside the removal question, and stepping does not clear it. The build's one-message rule yields to it, and still holds between the question and an ordinary notice. Chosen over other messages displacing it for a while. Settled 2026-09-26; built |
| What retries a save after a failure? | Same section — **every change, and MoneyBud itself now and then**, so the notice goes soon after the problem is fixed. Refines "every later change tries again". The interval was left to the plan, which made it **once a minute**. Chosen over retrying only on real changes. Settled 2026-09-26; built |
| Is recovery after a failed save announced? | Same section — **yes, once**: a short message that everything is saved again. Chosen over the notice just disappearing. Settled 2026-09-26; built. **Clarified after review**: it is said **on the save line**, where "not saved" was, not as an ordinary notice, so it too can stand beside the removal question. It stays until the next act, question or step. First written up as "a short notice", which read as if the one-message rule applied to it |
| What if the data's folder cannot be reached at all? | *When the data's folder cannot be reached* — **the same as data that cannot be read**: say so, touch nothing, close. That covers a missing profile, a folder MoneyBud may not open, and a file where the folder should be. Chosen over starting empty and showing "not saved", because if the real data came back, the first save that worked would write the empty start over it. Put to the stakeholder during review and ruled 2026-09-26; built, and held by a `cannot be reached` row added to `start-moneybud.feature` with his approval |
| Does "touches nothing" forbid making the folder and the lock file? | *When the data cannot be read, MoneyBud says so and touches nothing* — **no**: it means the data file. Making the folder if it is missing, and the lock file beside the data, is MoneyBud's own bookkeeping. Chosen over creating nothing at all, which would mean checking the data before taking the lock and leave a window in which two MoneyBuds start at once. Confirmed by the stakeholder, 2026-09-26; built that way |
| Does closing try to save? | Same section — **one last attempt**. If it works nothing is lost; if it fails MoneyBud closes without asking. Chosen over no last attempt. Settled 2026-09-26; built |
| Is a save that works announced? | Same section — **no**. Only a failure, and the recovery after one, is announced. Chosen over a brief confirmation for each save. Settled 2026-09-26; built |
| What does an interrupted save do to the data? | *An interrupted save never damages the previous one* — a crash or power cut **leaves the previous save intact**; at worst the change being saved is lost, because with no backups a half-written save could make the whole history unreadable. Chosen over no guarantee. The means was left to the plan: a temporary file, flushed, then renamed over the data file. Settled 2026-09-26; built |
| What does the next start say after an interrupted save? | *An interrupted save never damages the previous one* — **nothing**: it opens normally, and a change missing because its save was interrupted goes unmentioned. The user saw the crash, and MoneyBud could not say which change is missing. Consistent with closing silently after a failed last attempt. Chosen over saying the last change may not have been kept. Settled 2026-09-26; built |
| What if the kept data is there but blank? | *When the data cannot be read, MoneyBud says so and touches nothing* — **blank** data, with nothing at all in it, **counts as unreadable**: say so, touch nothing, close. MoneyBud never writes a blank save, so blank data means something went wrong. Chosen over treating it as a first start. A saved **empty budget** (no categories, nothing recorded) is valid and loads as no categories, confirmed by the stakeholder the same day (next row). Settled 2026-09-26; built |
| Do the default categories come back when there are none? | *A first start is unchanged* — **no**: they come only with a first start, when there is no kept data at all. Deleting all six and restarting leaves no categories. Chosen over re-adding them when there are no categories. Settled 2026-09-26; built |
| What happens when MoneyBud is started while it is already open? | *Only one MoneyBud at a time* — **the second start refuses**: it says MoneyBud is already open, and closes, because two windows saving one file would overwrite each other. Chosen over letting both run. Settled 2026-09-26; built |
| What does "everything is kept" cover? | *Everything is kept, for good* — **the ledger only**: categories, archived or not, budgets, expenses and incomes. Screen state is not kept (the period shown, a half-typed entry, a waiting question, a rename in progress), so MoneyBud always opens on the current period. Settled 2026-09-26; built |
| Does the unreadable-data message say where the data is? | *Where the data is, is written in the README* — **no**: no path and no pointer to the README, only that the data cannot be read. Confirms the literal reading of the ruling before it. Chosen over pointing to the README and over naming the path in this message alone. Settled 2026-09-26; built |
| Does it matter that real use may start before accounts exist? | *Real use before accounts* — in the stakeholder's words, *"I dont really see how this is relevant"*: he will try it out before accounts, and does not mind the saves being deleted when accounts arrive. So saved data may be dropped **at least up to and including the accounts increment**, extending *Demo data may not survive a new version*. The vanishing sweep money stays accepted as it was. Answered 2026-09-26 |
| When is an earlier plan offered back? | *The offer stands only while the period has no plan* — **only while nothing is assigned in the period**. The first assignment, by hand or by taking the plan over, ends the offer. Chosen over always offering it and topping each category up to its remembered figure, which is hard to explain and partly overwrites a hand-made plan, and over always offering it and adding on top, which doubles the plan if taken twice. Settled 2026-09-26; built |
| What does "nothing assigned" mean for the offer? | *"Nothing is assigned" means every Budget is zero* — **every *Budget* in the period is zero**, however it got there, so taking €100 back after assigning it brings the offer back. There is no separate "never budgeted" state, so this is never decided by `Ledger.HasBudget`. Chosen over the offer going for good once anything was assigned, which would need a difference MoneyBud remembers nowhere else. Settled 2026-09-26; built |
| Which earlier period's plan is offered? | *The plan offered is the latest earlier one* — **the latest earlier period that has a plan**, not only the one before, so a skipped month costs nothing. The offer **names that period**. Chosen over the previous period only, which offers nothing after a skipped month. Settled 2026-09-26; built |
| What counts as an earlier period having a plan? | *What counts as having a plan* — **a *Budget* of more than zero for a category not archived now**. Otherwise MoneyBud looks further back, and with no qualifying period there is no offer. An archived category's figure is skipped, and a category with a remembered figure of zero is not part of the plan. Chosen over taking that period anyway and offering nothing, which would lose the offer after a clear-out. Settled 2026-09-26; built |
| In which periods is the plan offered? | *Offered in the current period and later ones* — **the current period and later ones**, where assigning is allowed; never in a past one, whose plan cannot be changed. Chosen over the current period only, and over showing it everywhere and refusing it in a past period as the assign form does: the offer is an offer, not a form through which to reach a refusal. Settled 2026-09-26; built |
| How is the offer shown? | *The offer is a button, and a figure on each row* — **a button in the assign area naming the source period and the total, and a grey figure on each category row** (labelled *plan* since a follow-up, next rows), so you can see what you are taking over before you take it. **The stakeholder chose this over the recommendation**, the button alone, which is less to read but hides the figures. Also chosen over a line in the message bar, which a notice would push away. Settled 2026-09-26; built |
| Does the grey figure stay once the period has a plan? | *The figure on each row goes with the offer* — **no**: it is shown only while the plan is offered. It shows what would be taken over, not a month-on-month comparison. Chosen over keeping it all period, which widens every row for good and edges towards comparing months, not this increment. Settled 2026-09-26; built |
| What does taking the plan over do when *Unassigned* is smaller? | *Taking the plan over assigns it in full* — **it assigns everything anyway**, which can leave the period *Over-assigned*, marked, until the income is recorded. A notice names the period the plan went into. MoneyBud shows, it never blocks. Chosen over assigning only up to *Unassigned*, which needs an order and a splitting rule, and over refusing until there is enough, the only place MoneyBud would block over-assigning. Settled 2026-09-26; built |
| Can taking a plan over be undone? | *No undo, and no act to clear a plan* — **no**, and there is no act to clear a plan. A take-over in the wrong period is corrected row by row with negative assignments; the notice naming the period makes the mistake visible. Undo can be its own wish later. A *Plan leegmaken* act was rejected for now, as a new act with rules of its own to settle. Settled 2026-09-26; built |
| Which period does the take-over button act on? | *The offer is a button, and a figure on each row* — **the period on screen**, not the assign form's stepped period. The button belongs with the grey figures, which are the screen's period, so the two never disagree; the form's own period governs *Toewijzen* only. Chosen over the assign form's period, under which the figures would describe one period and the button act on another. Follow-up, 2026-09-26; built |
| Does an archived category's budget stop a period counting as empty? | *"Nothing is assigned" means every Budget is zero* — **yes**: a *Budget* above zero held by an archived category is still assigned money, which *Unassigned* subtracts and which counts as history. Taking it back is already how its leftover returns to *Unassigned*. Chosen over ignoring archived categories, which would show the offer with money already assigned and make "empty" mean two things. Follow-up, 2026-09-26; built |
| What happens to the offer when MoneyBud stays open past a period boundary? | *Offered in the current period and later ones* — **it and the grey figures disappear quietly at the next refresh**, like the *Huidige periode* label, and pressing the button before then is refused like any past-period assignment. Chosen over a notice, since nothing is announced at a period boundary anywhere else. Follow-up, 2026-09-26; built |
| How is the grey figure labelled? | *The offer is a button, and a figure on each row* — **"plan: € 400,00"**. It matches the button's *Plan*, is true whichever period the figure came from, and cannot be confused with the *Vorige periode* step button. Chosen over *vorige*, which the stakeholder first saw and which is wrong after a skipped month, and over the month on every row, which repeats the button and lengthens every row. Follow-up, 2026-09-26; built |
| Does a row not in the plan show "plan: € 0,00"? | Same section — **no, it shows no label**, because nothing would be taken over for it. Stated in a follow-up question and not contested: confirmed, 2026-09-26; built |
| Does taking a plan over ask for confirmation? | *Taking the plan over assigns it in full* — **no**. Only removing an entry asks, because it destroys a record; taking over loses nothing, and the notice names the period. Chosen over asking first through the message-bar question, which would be a second asking act over something correctable, and one extra click every month. Ruled at the scenario gate, 2026-09-26; built |
| Does a tiny *Budget* make a period a plan? | *What counts as having a plan* — **yes, any *Budget* above zero, however small**, accepted with its cost: €0,01 assigned in October by mistake means November is offered October's one-cent plan rather than September's. The button shows the source period and total, and taking October back to zero restores September's. Chosen over a threshold on how much of the plan must be covered, a number nobody picked. Ruled at the scenario gate, 2026-09-26; built |
| In what order are the rows while a plan is offered? | *The figure on each row goes with the offer* — **by their plan figure**, largest first, ties in order added, a row without one counting as zero. That is their order after taking over, so nothing jumps. It extends *The order of categories and slices* while the offer stands. The ring is unaffected: with every *Budget* zero it is all *Niet toegewezen* or the empty ring. Chosen over order added, under which the rows would jump on taking over. Ruled at the scenario gate, 2026-09-26; built |
| What does the accounts increment cover? | *What this increment covers, and what waits* — **accounts and net worth only**: accounts with balances, every income and expense on an account, net worth on screen, and transfers. No backing and no *Accumulated*, which are the next increment, and no sweep, the one after; its money still vanishes at a period end, as accepted. Settled 2026-09-27; built |
| Is a balance stored or worked out, and what is a hand edit? | *A balance is worked out from the entries, never stored as a free number* — **worked out**; typing the real balance records a **balance correction**. The question [§11](11-risks-and-technical-debt.md) asked to have decided before accounts. Chosen over a stored balance overwritten by hand, which nothing then explains, and over entered and calculated balances side by side, the busiest screen. Settled 2026-09-27; built |
| Does a typed balance include entries dated before it? | *A typed balance is what the bank said that day* — **yes**: it is what the bank said that day, so a late receipt does not knock it off. Entries dated after it move it. The same for every balance correction. Chosen over a starting figure every entry moves. Same-day entries are in it if recorded before it, *derived*, and a changed entry counts from when it was first recorded, *derived*. Settled 2026-09-27; built |
| Is a starting balance or a balance correction income? | *A starting balance or a balance correction is net worth only* — **no**: it changes the balance and net worth and nothing on the purpose side. Chosen over counting it as income, which would give thousands to assign on adding a savings account. What purpose such money carries is deferred to the backing increment (*A question for the backing increment*). Settled 2026-09-27; built |
| What accounts does a first start have? | *A first start has one account* — **one, *Betaalrekening*, at €0,00, as the pool account**. Chosen over several default accounts, which guess, and over none, which would block the first entry. Settled 2026-09-27; built. The €0,00 was made exact in the next row |
| Does a first start's Betaalrekening have a starting balance? | *A first start has one account*, follow-up — **no**. Its balance is the plain sum of what is on it, whatever the dates, until the user first corrects it; only a balance he actually types takes in earlier entries. Refines "a starting balance is the account's first balance correction" to accounts added with a typed balance. Chosen over a €0 starting balance, under which back-dated entries on the first day would do nothing. Follow-up, 2026-09-27; built |
| May a transfer change net worth? | *Transfers*, follow-up — **yes, where a balance correction has already counted one side**, and that is true, not a flaw: each account follows its own balance corrections, and net worth is their sum. Chosen over forcing a transfer to move both balances or neither, which would make one disagree with a balance the user checked. Follow-up, 2026-09-27; built |
| Does deleting an unused account ask first? | *Managing accounts*, follow-up — **no, never**, and it is announced afterwards, like deleting an unused category, even with a starting balance other than zero: the act is for mistakes, and only a number just typed is lost. Chosen over asking first. Follow-up, 2026-09-27; built |
| What does a balance correction show in the history? | *The accounts strip, and an account's history*, follow-up — **the new balance and the difference it made**, "Correctie — saldo € 1.000,00 (− € 23,40)": the only trace of something forgotten. Chosen over the new balance only. Follow-up, 2026-09-27; built |
| Is a balance correction's difference fixed when typed, or recomputed? | *The accounts strip, and an account's history*, follow-up — **recomputed**: it always means what is still unexplained, the typed balance minus what the previous balance correction and the entries it takes in would give now. Record the forgotten €23,40 and (− € 23,40) becomes € 0,00, while the balance stays €1.000. At €0,00 everything has been found. Chosen over fixing it at the moment of typing, the documentation's own earlier reading, which this **overturns**. Follow-up, 2026-09-27; built |
| What does the overdrawn badge say? | *An overdrawn account carries the marker*, follow-up — ***Rood***. Chosen over *Rood staan* and *Negatief saldo*. Follow-up, 2026-09-27; built |
| What happens when an account is added under a name another account has? | *Managing accounts*, follow-up — **it is refused**, "Er is al een rekening met die naam". Unlike a category, whose existing one is handed back, because that would quietly drop the starting balance just typed. Chosen over handing back the existing account. Raised by the scenario writer; follow-up, 2026-09-27; built |
| What does a starting balance left empty mean? | *Managing accounts*, follow-up — **no starting balance**: the account behaves like the first start's Betaalrekening, its balance the sum of its entries until first corrected. A typed 0 is a starting balance and takes in earlier entries. The same rule as the first-start account, so nothing new to learn. Chosen over refusing an empty starting balance. Raised by the scenario writer; follow-up, 2026-09-27; built |
| Can an income or expense be changed from an account's history? | *The accounts strip, and an account's history*, follow-up — **no**: incomes and expenses are changed and removed only from the Overview's lists. The history is where transfers are changed or removed and balance corrections removed. Smaller, one place per kind. Chosen over both routes. Raised by the scenario writer; follow-up, 2026-09-27; built |
| In what order are accounts listed? | *The accounts strip, and an account's history*, follow-up — **pool account first, then in the order added**, in the strip and in the forms' account list. The default sits where you look for it, and nothing jumps when balances change. Chosen over strictly the order added and over alphabetical. Raised by the scenario writer; follow-up, 2026-09-27; built |
| Is a negative net worth marked? | *An overdrawn account carries the marker*, follow-up — **yes, with the same marker and the badge *Rood***: every negative figure carries the one marker. Chosen over a plain figure. Raised by the scenario writer; follow-up, 2026-09-27; built |
| Does a starting balance's history row show a difference? | *The accounts strip, and an account's history*, follow-up — **no**, "Startsaldo — € 1.000,00": there is nothing it corrected. Chosen over showing the whole amount as a difference. Raised by the scenario writer; follow-up, 2026-09-27; built |
| Are the proposed Dutch terms for accounts approved? | *Proposed display terms* — **yes, all**: Rekening/Rekeningen, Saldo, Overboeking/Overboeken, Van/Naar, Startsaldo, Correctie/Saldo corrigeren, Rekening toevoegen, and *Hernoemen*/*Verwijderen* reused for accounts, beside the already ruled Vermogen, Hoofdrekening, Maak hoofdrekening and Rood. They stay in that table, marked approved, and move into *Dutch display terms* with the build that gives `Tekst` their constants, because `TekstTests` reads that table: the *Plan overnemen* precedent. Raised by the scenario writer; follow-up, 2026-09-27; built |
| Is a starting balance of only spaces a starting balance? | *Approved at the scenario gate, 2026-09-27* — **no, it is the same as one left empty**: no starting balance, just as a label that trims to nothing is no label. Chosen over refusing it as not an amount. Ruled at the scenario gate, 2026-09-27, and added as one row to `add-an-account.feature` after the gate; built |
| Where does the account go on the entry forms? | *Every income and expense is on an account* — **the last field, after *Datum***, a list pre-filled with the pool account, not free text. Chosen over hiding it until asked, which makes a cash expense easier to get wrong, and over the first field. Settled 2026-09-27; built |
| Where are accounts and net worth shown? | *The accounts strip, and an account's history* — **a strip across the top of the Overview**, each balance and then *Vermogen*, the same in every period because balances are about today. Chosen over the income column, which reads as the period's, and over a separate screen. Settled 2026-09-27; built |
| Can money be moved between accounts? | *Transfers* — **yes, by a transfer**: from, to, amount, date. Both balances move; net worth and budgets do not. Changed or removed like an entry. In the stakeholder's framing, the same movement backing and the sweep will make by themselves. Chosen over correcting both balances and over no transfers until backing. Settled 2026-09-27, on a second asking; built |
| How is an overdrawn account shown? | *An overdrawn account carries the marker* — **with the same marker** as *Over budget* and *Over-assigned*, and a badge of its own, *Rood* (a follow-up row above). Answers the question the marker revision of 2026-09-25 left open. Chosen over a plain negative figure and over a stronger look. Settled 2026-09-27; built |
| What can be done to an account? | *Managing accounts* — **add** (name and starting balance), **rename**, and **delete only while unused**. Archiving is deferred until missed, not rejected. Chosen for now over the full category treatment and over adding and renaming only. Settled 2026-09-27; built |
| Which account is the pool? | *The pool account can be any account* — **any**, always exactly one; changing it changes the default for new entries only, and the pool cannot be deleted. Chosen over fixing it to Betaalrekening. Settled 2026-09-27; built |
| May a transfer be dated in the future? | *Transfers* — **no**, like an expense: it reports money that has moved. Chosen over allowing it like an income. Settled 2026-09-27; built |
| Is data saved before accounts carried over? | *Saved data from before accounts* — **no**: the new version says it cannot read it and closes, and the user deletes the file. The first time *Demo data may not survive a new version* is exercised. Settled 2026-09-27; built |
| Where are transfers and balance corrections seen? | *The accounts strip, and an account's history* — **in the account's history**, opened by clicking it in the strip, newest first, where transfers are changed or removed and balance corrections removed. Chosen over a third list on the Overview, which would tie them to the period on screen. Settled 2026-09-27; built |
| Does an income or expense row name its account? | *Every income and expense is on an account* — **only when it is not the pool account**, small and grey. Chosen over always and never. Settled 2026-09-27; built |
| What is the pool account called on screen, and net worth? | *The pool account can be any account* — ***Hoofdrekening***, made so by *Maak hoofdrekening*. Chosen over *Standaardrekening* and *Potrekening*. Net worth is *Vermogen*, his own word from round 1. The rest of the Dutch is proposed (*Proposed display terms*). Settled 2026-09-27; built |
| What does the backing increment cover? | *What the backing increment covers, and what waits* — **backing and *Accumulated***, with **one backing account per category, or none**. Several backing accounts, a default among them, and choosing the account per assignment are **deferred until missed**, not rejected. No sweep, the next increment. Settled 2026-09-27; built |
| When does money move for an assignment to a backed category? | *Assigning to a backed category moves money* — from the pool account to the backing account, **on the day of assigning, or on the period's first day if that is later**; a negative assignment moves back what the clip lets through. Taking a plan over follows the same rule (*derived*). Chosen over always on the period's first day, which a balance correction taken earlier that month would swallow, and over always today, which shows money moved for a plan not yet started. Settled 2026-09-27; built |
| What purpose does money the user already had carry? | *Money the user already had stays by location only* — **none**: backing a category moves nothing already in the account, and *Accumulated* starts at €0. Answers *A question for the backing increment*. Chosen over giving a starting balance a purpose when typed and over an opening *Accumulated*. Settled 2026-09-27, in his words; built |
| Where is an unbacked category's money? | *Backing a category that already has money* — **on the pool account**: assigning to it moved nothing, its expenses default to the pool, and the sweep will collect from there. Asked by the stakeholder and confirmed, 2026-09-27 |
| What happens to a category's money this period when it is backed? | *Backing a category that already has money* — **its unspent *Remaining* in the current period moves** from the pool account to the backing account, on the day of backing; **nothing if it is overspent**. Later periods' budgets move on their first day. *Accumulated* starts at what moved. The stakeholder's own idea. Chosen over moving the whole *Budget* and over moving nothing. Settled 2026-09-27; built |
| Can backing be changed, and what happens to money already moved? | *Backing can be set, changed or removed at any time* — **yes, at any time**. First ruled: money already moved stays where it went, and unbacking and re-pointing move nothing back. **Revised by the stakeholder the same day, his own idea: unbacking moves the category's money back to the pool account, and re-pointing takes it to the new account**, on that day, nothing if there is none. What moves was first written up as all of *Opgebouwd*; a follow-up made it what is there for the category in the backing account (next rows). What MoneyBud moved in on a category's behalf, it moves back when the backing ends. It removed a contradiction (unbacking and backing again moved the same money twice) and money stranded with no purpose. Money from earlier periods returns with no purpose. Chosen over the first version and over returning only this period's *Remaining*. An unbacked category shows no *Accumulated*. An account that backs a category counts as used. Settled 2026-09-27; built |
| Where does money planned for a later period go if backing changes first? | *Planned money follows the backing on the day it moves* — **to whatever the category is backed by on that period's first day**, or nowhere if it is unbacked then. Settled 2026-09-27; built |
| How much do unbacking and re-pointing move? | *Backing can be set, changed or removed at any time*, follow-up — **what is there for the category in the backing account**: what MoneyBud moved in for it, minus its expenses paid from that account. €200 moved in and €50 spent from Betaalrekening: *Opgebouwd* €150, unbacking returns €200, so nothing is stranded and Betaalrekening is exactly €50 down. It can differ from *Opgebouwd*, which counts every expense whichever account paid. Nothing moves if there is none. Chosen over returning exactly *Opgebouwd*, which strands €50 and leaves the pool short. Follow-up, 2026-09-27; built |
| May unbacking or re-pointing overdraw the account the money leaves? | Same section, follow-up — **yes**, marked *Rood*, never blocked: the money was spent on something else, so the account really is short. Chosen over moving only what the account holds. Follow-up, 2026-09-27; built |
| How much money does a negative assignment to a backed category move back? | *Assigning to a backed category moves money*, follow-up from the scenario stage — **at most what is there for the category**, by the test unbacking uses. *Budget* 300, 100 spent before backing, 200 moved: −300 takes the *Budget* to 0 and moves back **€200**. The clip against the *Budget* is unchanged; the cap is about money only. Chosen over moving the full €300, which would overdraw the backing account for an expense the pool paid. Follow-up, 2026-09-27; built |
| Does a movement from the pool account to itself show in the history? | *The pool account may back a category*, follow-up from the scenario stage — **no row**: no balance changes. *Opgebouwd* still counts it. Chosen over one row netting to zero. Follow-up, 2026-09-27; built |
| Can an archived category be backed or re-pointed? | *Archiving does nothing to backing*, follow-up from the scenario stage — **yes, and it stays archived**: backing is not one of the three acts that bring a category back. Chosen over offering only "—" and over backing bringing it back. Follow-up, 2026-09-27; built |
| What does the notice say when backing or re-pointing moves nothing? | *Backing, re-pointing and unbacking are announced, never confirmed*, follow-up from the scenario stage — **only the backing**, no mention of money. Chosen over saying that nothing moved. Follow-up, 2026-09-27; built |
| Does *Accumulated* survive unbacking and re-backing? | *Re-backing starts Accumulated over* — **no**: it starts over from what moves at the re-backing. Re-pointing without unbacking does not restart it. Redefines *Accumulated*. Stands after the revision of unbacking, which returns the old total to the pool. Settled 2026-09-27; built |
| Which expenses lower *Accumulated*? | *Backing a category that already has money*, follow-up — **those dated after the backing day, or on it and recorded after the backing**, the balance-correction test. A forgotten expense dated before the backing does not touch it, and the amount moved at backing stays what it was. Chosen over recomputing the move and over counting every expense in the backing period. Follow-up, 2026-09-27; built |
| May the pool account back a category? | *The pool account may back a category*, follow-up — **yes**, offered in *Staat op* like any account; assigning moves no balance but *Opgebouwd* counts, and the same when a backing account is later made the pool. Chosen over refusing it and over showing no *Opgebouwd*. Follow-up, 2026-09-27; built |
| What does archiving do to backing? | *Archiving does nothing to backing*, follow-up — **nothing**: the category stays backed, keeps and shows *Opgebouwd*, and its later budgets still move; it can be unbacked separately. Chosen over archiving unbacking and over hiding *Opgebouwd*. Follow-up, 2026-09-27; built. **Ruled after the build, the same day**: an archived backed category is also shown in the current period and later ones while its *Accumulated* there is not zero, so its money is never hidden with its row |
| Does *Opgebouwd* show in periods before the backing? | *Accumulated covers everything up to the period on screen*, follow-up — **it follows today's backing**: a category backed now shows it in every period, €0 before anything moved; an unbacked one nowhere. Chosen over following the backing as it was in each period. Follow-up, 2026-09-27; built |
| How is a negative *Opgebouwd* shown? | Same section, follow-up — **with the one marker and the badge *Rood***, like an overdrawn account; never blocked. Chosen over a plain figure, a floor at zero and another badge. Follow-up, 2026-09-27; built |
| Does an expense against a backed category on another account lower *Opgebouwd*? | *Spending against a backed category from another account*, follow-up — **yes**: the category's money was spent, whichever account paid. The balance and *Opgebouwd* then differ by it, visibly and truly. Chosen over counting only expenses on the backing account. Follow-up, 2026-09-27; built |
| What account does the expense form pre-fill once categories can be backed? | *What the backing increment covers, and what waits*, follow-up — **it follows the category typed**, the backing account or the pool, **until the user picks one himself**, which then sticks for that entry; an expense being changed keeps its account. Chosen over always following the category. Follow-up, 2026-09-27; built |
| Are backing, re-pointing and unbacking confirmed or announced? | *Backing, re-pointing and unbacking are announced, never confirmed*, follow-up — **never confirmed, announced afterwards**, naming what moved. The wording is copy. Chosen over a quiet act. Follow-up, 2026-09-27; built |
| As of when is *Accumulated* shown? | *Accumulated covers everything up to the period on screen* — **up to and including the period on screen**, so stepping forward includes what is planned. Chosen over always as of today, like *Vermogen*. Settled 2026-09-27; built |
| How is backing shown and set? | *On screen: Staat op and Opgebouwd* — a list ***Staat op*** on each category row beside *Hernoemen*, "—" for none; a backed row shows ***Opgebouwd***, and so does its slice's hover. *Opgebouwd* chosen over *Gespaard*. Settled 2026-09-27; built |
| Does a first start back *Sparen*? | *A first start leaves Sparen unbacked* — **no**; the user adds a savings account and backs it. Settled 2026-09-27; built |
| Where does moved money show? | *Moved money in the account's history* — **in both accounts' histories, read-only, one row per movement**, each on its own day, changed by assigning again. Chosen over one row per period. Settled 2026-09-27; built |
| When the pool account backs a category and it is re-pointed, does the money assigned meanwhile go along? | *Backing: ruled after the build*, ruling 1 — **yes**: what is there for a category counts movements by direction, so money moved from the pool to the pool is still there for it. The plan's "adds nothing" is superseded. Ruled after the build, 2026-09-27, as built |
| Can a category be deleted once money has been moved for it? | *Backing: ruled after the build*, ruling 2 — **not while a movement between two different accounts stands for it**, because those rows name it in both histories; *Archiveren* is still offered. Movements from the pool account to itself do not count, and go with it. Chosen over deleting it and erasing the rows, which could shift balances after the fact. Extends "history", does not reverse the rejection of "never touched". Ruled after the build, 2026-09-27 |
| Does picking the account already shown on the expense form make it stick? | *Backing: ruled after the build*, ruling 5 — **no, accepted as built**: the list writes back what it shows, and telling a deliberate pick apart would need the window to decide what a click means. Ruled after the build, 2026-09-27 |

**Eight** of these answers were taken with their drawbacks visible rather than resolved: the
expense default is wrong for cash and nothing outside MoneyBud will say so; an overdrawn account is
shown exactly like an overspent budget despite being a harder fact (since the marker revision of
2026-09-25, this one is open again on the account side, see *Assigning may overdraw the pool
account*); the demo cannot correct a wrong entry except by starting over (a cost that **no longer
applies**: corrections were settled and built on 2026-09-26); a clamped start day
produces a period that is longer than its neighbours with nothing on screen explaining why; net
worth and *Unassigned* will disagree about an expected income, because they are answering about
different moments in time; an archived category now has three routes back rather than one
(adding its name, recording an expense, assigning a positive amount), which weakens the argument for having no
un-archive act without overturning it; a plan forgotten in a period that has since ended cannot
be fixed, so that period shows over budget for good ("past is past"); and a plan taken over into
the wrong period can be undone only row by row, one negative assignment per category (settled
2026-09-26, and built). Each is
written up where the decision is, and the first is carried in
[§11](11-risks-and-technical-debt.md). They are accepted costs, not open questions.

**The second of those eight was settled on 2026-09-27**: an overdrawn account gets the marker, so it
is shown exactly like an overspent budget again, marked, with its own badge. **The accounts rulings
add one cost of their own**, taken with it visible: a balance correction takes in every entry dated
before it, so changing or re-pointing such an entry no longer changes that account's balance (*A
typed balance is what the bank said that day*, above; [§11](11-risks-and-technical-debt.md)). The
difference the balance correction shows in the account's history is what is left to see of it.

**The backing rulings add one more** (2026-09-27, built the same day), taken with it visible: money already in
an account when it starts backing a category stays by location only, so *Accumulated* and the
backing account's balance differ by it from the first day (*Money the user already had stays by
location only*, above). So does spending against a backed category put on another account, ruled
true in a follow-up (*Spending against a backed category from another account*, above). Money
returned to the pool account on unbacking carries no purpose for its earlier periods, the same
situation as money the user already had (*Backing can be set, changed or removed at any time*,
above).

**Nothing here blocks the first increment.** It has no accounts at all
([§11](11-risks-and-technical-debt.md)), so it reaches none of the account-related answers above,
and it has no assigning either. The start-day answer changes nothing already built: no approved
scenario configures a start day, and the default of the 1st fits in every month.

**Nothing here blocks the income increment either.** Recording income reaches *Income*, *Label*,
*Unassigned* and the future-dating rule, all four of which are settled above. It does **not** reach
assigning, so it does not reach *Over-assigned*, the *Budget* floor or the clipping rule; and it has
no account, so an income lands in *Unassigned* without landing anywhere on the location dimension —
the same accepted gap an expense has ([§11](11-risks-and-technical-debt.md)).

**Nothing here blocked the category increment, and it is built.** Everything it reaches was settled
— the name rule, the duplicate rule, archiving and bringing back, the default set — and it reaches
nothing about accounts, so backed categories, the pool account and the sweep stayed out of scope
exactly as they did for the two increments before it.
[`add-category.feature`](../../features/add-category.feature) and
[`archive-category.feature`](../../features/archive-category.feature) are approved and pass, and
the category scenarios in [`record-expense.feature`](../../features/record-expense.feature) pass with
them. It left two things unsettled that belong to later increments. The first was whether a category
**in use** is shown in a period where it has no history, which belongs to the period view. The
second was what **assigning** to an archived category does, which belongs to the assigning
increment. Both were answered on 2026-09-25, along with carry-over for an archived category. The
assigning answer has since been built, and so has the display rule, and carry-over was built last,
in the opening-a-period increment. Two questions arose while this model was being written up and both were
answered the same day: what an added name does when an archived category carries it, and whether an
archived category can be brought back at all. They turned out to be one question. Three more were
answered on 2026-09-25, before the scenarios were written: recording an expense against an archived
category records it and brings the category back — **overturning** a derivation that had assumed a
route that does not exist — category names are trimmed and a blank one refused, and adding a name
again keeps the existing spelling. Answered the same day: a refused expense brings nothing back,
trimming and ignoring case apply when recording as well as when adding, and an archived category is
shown in every period where it has history, the current one included. Four more were answered while
the scenarios were being reviewed. Inner whitespace in a name is ignored for comparison and kept as
typed. Archiving is never confirmed first. The user is told afterwards that a category was archived.
A zero budget with nothing spent is not history.

**Nothing here blocked the assigning increment, and it is built** to the scope settled on
2026-09-25, below. [`assign-to-category.feature`](../../features/assign-to-category.feature) is
approved and passes. The build settled two things for itself, both below the level of anything a user can reach, so both are
recorded in [§8.1](08-crosscutting-concepts.md) rather than here. A period that is not one of the
calendar's own is a caller's mistake and throws. An assignment that changes nothing writes nothing.
It also **retired `Ledger.SetBudget`**, the scaffold that had stood in for assigning, so a *Budget*
is now made only by assigning.

**In:** assigning a positive, negative or zero amount to a category, in the current budget period
or a later one. *Unassigned* moves, and can go *Over-assigned*. An over-large negative assignment is
clipped and the shortfall reported. A positive assignment to an archived category's name brings it
back, announced. **Refused**, on the same rules as recording an expense and [§8.2](08-crosscutting-concepts.md):
a name that is none of your categories, in use or archived; a name that trims to nothing; an amount
finer than a cent. Also refused: any past period. The order in which these are reported, and how
they combine with zero and clipping, are in *When an assignment is refused* (above).

| Out | Why |
|---|---|
| Account-backed categories and the pool account | They need the location dimension, and there are no accounts ([§11](11-risks-and-technical-debt.md)). Every category is unbacked, so assigning in this increment is planning only and moves no money |
| *One action assigns last period's plan in full* (*Budgets carry over as figures*) | It belongs with **period opening**, which is a slice of its own |
| The sweep | Needs accounts and a period end to act on, as it did before |
| Any UI | This increment is a domain library and its scenarios, like the three before it. A UI is the increment after it |

**Nothing here blocked the UI increment, and it is built** to *The user interface* (above). Its five
new feature files were approved at the scenario gate, the plan at the
plan gate, and `spec-reviewer` reviewed the result, all on 2026-09-25. Two records came with it: the
toolkit, [ADR 0005](../decisions/0005-avalonia-ui-toolkit.md), and the project layout,
[ADR 0006](../decisions/0006-three-source-projects.md). The domain gained two queries and no
behaviour. It reaches nothing about accounts, so the location dimension stays out, as before.
**One of the rulings it settled is held by unit tests rather than scenarios**: narrowing
suggestions as you type, for which the stakeholder did not ask for one. How a typed amount is read
was the other, until [`type-an-amount.feature`](../../features/type-an-amount.feature) was approved
at the scenario gate on 2026-09-26 and bound, with two further rulings of that day in it
([§11](11-risks-and-technical-debt.md), *Resolved*).

**Nothing here blocked the corrections increment, and it is built.** Its rulings are in *An entry
can be changed or removed*, *Renaming a category* and *Deleting a category that has no history
anywhere* (above), all of 2026-09-26. The two points its first write-up left open were settled the
same day. Its four feature files were approved at the scenario gate, its plan at the plan gate, and
`spec-reviewer` reviewed the result, all on 2026-09-26. It added no record: what it settled in code,
a `Category` with identity, is how the domain expresses renaming, not architecture
([§9](09-architecture-decisions.md), [§8.1](08-crosscutting-concepts.md)). It reaches nothing about
accounts. It meets the sweep only as a question already filed, which it widens without needing an
answer.

**Nothing here blocked the persistence increment, and it is built.** Its rulings are in *What
MoneyBud keeps* (above), all of 2026-09-26. Its three feature files, `keep-data.feature`,
`start-moneybud.feature` and `carry-on-when-saving-fails.feature`, were approved at the scenario
gate. Its plan was approved at the plan gate and brought [ADR 0007](../decisions/0007-keeping-the-ledger.md),
which answers what the rulings left to it: the form, the folder, how amounts and identity are
stored, and where storage sits. `spec-reviewer` reviewed the result, all on 2026-09-26. The review
led to one new ruling, *When the data's folder cannot be reached*, with a scenario row approved
after the gate; one confirmed reading, that "touches nothing" means the data file; and one correction
to this glossary: "saved again" is said on the save line, not as an ordinary notice. It reaches nothing
about accounts, and the stored form may change when accounts arrive (*Real use before accounts*).

**Nothing here blocked opening a period, and it is built.** Its nine rulings, five follow-ups and
three gate rulings are in *Opening a period* (above), all of 2026-09-26. The follow-ups closed every
point the nine had left open.
[`take-over-a-plan.feature`](../../features/take-over-a-plan.feature), 19 scenarios and 23 cases,
was approved at the scenario gate the same day, with the three gate rulings, and the plan at the plan
gate. `spec-reviewer` reviewed the result and found no faked scenario and no domain defect. It added
no record ([§9](09-architecture-decisions.md)), and it keeps nothing new. It reaches nothing about
accounts or the sweep. A plan offered is a plan, and taking it over is assigning, which already
existed: `Ledger.TakeOverPlan` calls `Ledger.Assign` once per figure. What the plan and the build
chose beyond the rulings is in *Taking a plan over: chosen in the build, not put to the stakeholder*
(above).

**Nothing here blocked the accounts increment, and it is built** (2026-09-27). This paragraph first
read "settled, not built", with no plan yet. Its sixteen rulings and seven derivations are in
*Accounts and net worth* (above), with four follow-up rulings and three points the documentation
handled, all of the same day. **Its six feature files, and additions to two existing ones, were
approved at the scenario gate on 2026-09-27**, 94 scenarios and 174 cases, with one gate ruling and
nine scenario-writer choices (*Approved at the scenario gate, 2026-09-27*, above). The plan was
approved at the plan gate the same day and brought [ADR 0008](../decisions/0008-balance-is-worked-out.md).
The increment was built to it and is green. `spec-reviewer` found no faked scenario, one vacuous
scenario and some defects, all fixed. **Nothing blocked it**: the seven points its first write-up left open were all dealt with on 2026-09-27,
and the one deferred, what purpose money the user already had carries, has no consequence until
backing (*A question for the backing increment*, above). It is the first increment to reach the location
dimension, and so the first to reach the account-related answers in the table above. It reaches
only some of them: the pool account as a default for entries, the expense default, and overdrawing.
Backing, *Accumulated*, the pool account as a source, and the sweep stay out. So does the question
above, which still waits for the sweep.

**The backing increment is settled, not specified** (2026-09-27). Its eleven rulings are in *Backing
and Accumulated* (above), one of them the answer to the question the accounts increment deferred. It
is the first increment to reach the pool account as a **source**, overdrawing it by assigning, and
*Accumulated*, and so the answers in the table above about those. It does not reach the sweep, and
the question above still waits for it. **Nothing here blocks its scenarios.** The fourteen points
the first write-up left open were all dealt with the same day: one ruling revised by the
stakeholder, eight follow-ups, and three derivations (*What this section leaves open*, under *Backing
and Accumulated*, above). Of what the revision brought with it, two points were ruled in further
follow-ups, and the rest is stated there as derivations, open to contradiction at the scenario gate. How a planned movement is held is for the plan.

**Nothing here blocked the backing increment, and it is built** (2026-09-27). The paragraph above
is kept as it read before the scenarios. **Its five feature files, and additions to two existing
ones, were approved at the scenario gate on 2026-09-27**, 62 scenarios and 84 cases, and the plan at
the plan gate the same day, with [ADR 0009](../decisions/0009-movements-are-entries.md): a planned
movement is written on its own day by settling. The increment was built to it and is green. What the
build chose beyond the rulings is in *Backing: chosen in the build, not put to the stakeholder*
(above). Five points raised after the build were ruled the same day, among them a new display rule
for archived backed categories (*Backing: ruled after the build*, above). It still does not reach the sweep, so the
question above still waits for it. What it adds for that question: a backed sweep destination can now
exist, and settling is the step the sweep will use at a period's end.
