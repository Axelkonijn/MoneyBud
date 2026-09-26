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

## Backed categories accumulate

A backed category shows one figure that no other category has: **Accumulated** — everything its
accounts have built up on its behalf across every period, shown alongside the per-period *Budget*,
what has been spent, and *Remaining*. After three months of €200 assigned to Savings, Savings shows
**€600 accumulated**, even when this period's *Budget* and *Remaining* are small or zero.

**Accumulated is the running sum of *Remaining* over every period** — everything ever assigned to
the category minus everything ever spent against it. It is the sum of *Remaining* and **not** of
*Budget*: assign €200 three times and spend €50 once, and *Accumulated* reads **€550**.

It nets out spending because it has to: money spent out of a backing account has really left it,
and *Accumulated*'s whole job is to agree with what is in the accounts. A running sum of *Budget*
would read €600 and describe money that is not there.

This was first written here as a **derivation** — stated in full so that it could be contradicted
if it was wrong about what the stakeholder wanted. It was not contradicted: the stakeholder has
confirmed it, so it now stands as a decision that was taken rather than an inference waiting to be
checked. The reasoning above is the reason it was taken and stays part of the record.

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

**Not in the first increment**, which has no accounts and therefore no backed categories.

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

**Settled, not built.**

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
  category is still shown*, below).
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
| **Current or future** | **Shown** | Not shown |

A category with history in a period is shown there in every case.

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

**Not built.** Carry-over is not built for any category yet.

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

**Settled, not built.** On screen the act is *Verwijderen*, the same word as removing an entry
(*Dutch display terms*, below).

### What archiving does not settle, because it cannot yet

Archiving says nothing about money, and for most categories there is no money to say anything about.
Two cases would be different, and **neither is reachable**: a **backed** category's money really sits
in an account, and the **sweep destination** must itself be backed (above). With no accounts there
are no backed categories and no sweep at all ([§11](11-risks-and-technical-debt.md)), so there is
nothing for either question to be true of. Recorded so that a later reader does not take the silence
for an answer.

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

## Terms

| Term | Definition |
|---|---|
| **Account** | A place where money actually sits. Current account, savings account, investment account, or cash. Answers *where*. Cash is modelled as an account despite not being a bank account. May **back** one or more categories — see below. |
| **Location** | The dimension answered by "which account". Not a separate entity — a way of grouping. |
| **Category** | What money is earmarked for: groceries, hobby, moving out. Answers *what for*. A category is a label and exists independently of any amount assigned to it. Its **name** is **trimmed** at the ends. It is compared **case-insensitively**, with any run of inner whitespace counting as one space. It is stored trimmed, with its capitalisation and inner spacing as typed. So there are never two categories that differ only in case or spacing. A name that trims to nothing is **refused**. Adding a name that already exists hands back the category that already has it, **spelled as it already was**, with the user told so (see *A category name is compared case-insensitively* above). A category with history is taken out of use by **archiving**, never by deleting: its history stays, and adding its name again, recording an expense against it or assigning a positive amount to it brings it back (*A category is taken out of use, not deleted*, above). A category with **no history in any period** can instead be **deleted** (*Deleting a category that has no history anywhere*, above). It can be **renamed**, under the same name rules, to any name no other category has (*Renaming a category*, above). Deleting and renaming were settled on 2026-09-26 and are not built. MoneyBud ships with six **default categories** (above). |
| **Archived** | The state of a category that has been taken out of use. It is **no longer offered for new entry**, whether recording an expense or assigning, and its last figure is not offered back when a period opens. Everything it already owns stays: its expenses, its budgets, and its place in those periods' figures. It is **shown in every budget period where it has history** — a budget of more than zero or an expense in that period — **including the current one**, and not in a period where it has none, so a zero budget alone does not count (*Where an archived category is still shown*, above). Archiving is **never confirmed first**, and the user is **told afterwards** that the category was archived (*Archiving is announced, never confirmed*, above). Archiving destroys no record, which is why the state is not called *removed*, and it is **not permanent**. It is **brought back**, history and all and spelled as it was, by any of three acts the user already has: **adding its name** again, **recording an expense against it** (which records the expense rather than refusing it), or **assigning a positive amount to it**. Each way, the user is told it was brought back. A **negative or zero** assignment does **not** bring it back: pulling an archived category's money out is tidying up, not planning for it (*Only a positive assignment brings it back*, above). There is no separate act of un-archiving, for the same reason there is no separate act of unassigning; bringing back is a side-effect of those acts, always announced. Only a category in use can be archived. An archived category can be **renamed**, and stays archived (*Renaming a category*, above). On the Overview, an archived category that is shown carries a *Gearchiveerd* caption and has no archive button (*Where an archived category is still shown*, above). Distinct from a period being **closed** — a state MoneyBud deliberately has not got (*Ending versus closing a budget period*, below). See *A category is taken out of use, not deleted* above. Built in the category increment: `Ledger.ArchiveCategory`, specified by [`archive-category.feature`](../../features/archive-category.feature) and, for bringing back by recording, [`record-expense.feature`](../../features/record-expense.feature) ([§8.1](08-crosscutting-concepts.md)). Bringing back by assigning was built in the assigning increment, specified by [`assign-to-category.feature`](../../features/assign-to-category.feature). |
| **Rename** | Giving a category a new name. The new name follows the rules for adding one: trimmed, stored otherwise as typed, and refused if it trims to nothing. A name **another** category has, archived ones included, is **refused**, and the user is told it is taken. The category's **own** name in a new spelling is allowed, which is the front door *The existing spelling is kept* kept adding from being. **Past periods show the new name.** An archived category can be renamed and **stays archived**. See *Renaming a category* above. Settled 2026-09-26; not built. |
| **Delete** | Said of a **category** only: removing one that has **no history in any period**, meaning no expense and no budget of more than zero. It is gone, not archived, and adding its name again creates a new category. A separate act from archiving, with its own button, shown only on such a category. **Never confirmed**, and announced afterwards. A category with history cannot be deleted; it is archived. Decided by figures, never by `Ledger.HasBudget`. See *Deleting a category that has no history anywhere* above. Settled 2026-09-26; not built. |
| **Default categories** | The six categories MoneyBud ships with: **Boodschappen, Huur, Hobby, Sparen, Verzekeringen, Abonnementen**. A starting set chosen to be tried, not a claim about what a household needs. Their names are **Dutch** because they are user-facing **content**, unlike the names in the feature files, which are synthetic test data in whichever language suits the scenario — and unlike *Dutch source terms* below, which is vocabulary rather than content. *Sparen* ships **unbacked** and becomes an *account-backed category* when accounts exist. See *The default categories* above. |
| **Purpose** | The dimension answered by "which category". Not a separate entity — a way of grouping. |
| **Account-backed category** | A category that names one or more accounts its money really sits in — Savings, Stocks. Most categories are not backed. The relationship is **many-to-many**: a category may be backed by several accounts, and an account may back several categories. Backing changes what assigning, spending and the end of a period do to the category — see *Account-backed categories* above. Not in the first increment, which has no accounts. |
| **Backing account** | One of the accounts backing a category. A backed category names exactly one of them as its **default backing account**: the one used whenever money moves on that category's behalf, overridable per assignment or per expense. |
| **Pool account** | The one current account designated as where *Unassigned* money is assumed to live. It is the default **source** for every movement MoneyBud makes on its own initiative — assigning to a backed category, and the end-of-period sweep — overridable per movement. It is also the account an **expense against an unbacked category** is assumed to have left, again overridable, which is a guess about a past event rather than a choice of source and is the weaker of its two roles ([§11](11-risks-and-technical-debt.md)). May go *Overdrawn*; nothing blocks that. A fact about one account, not a redefinition of *Unassigned*, which remains a purpose and not a place. Not in the first increment, which has no accounts. |
| **Unassigned** | Two things under one name, deliberately. (a) The **absence of a purpose**: a value on the purpose dimension, not a location — unassigned money still sits in an account. (b) The **figure** that measures it for one budget period: that period's income minus everything assigned to categories in it. It is the pool that assigning draws from and that a negative assignment puts money back into. Starts at the period's full income, because carrying budgets over carries figures and not assignments; reaches zero when the user has finished budgeting the period; goes **negative** past that, which is *Over-assigned*. Shown prominently and assigned from directly, rather than being only a total the user has to work out — and never enforced. Not a category: nothing is budgeted for it and nothing is spent against it. Does not survive the end of a budget period: it is *swept* — see below. An income joins its period's *Unassigned* **when it is recorded**, which for a future-dated income is before its date arrives — so *Unassigned* covers a **whole period** where *Net worth* covers a **point in time**, and the two disagree about expected income by design (*The central distinction*, above). Formerly also called *Left to assign*; that name is retired — see *One figure, not two*. |
| **Assign** | The act of giving money a purpose: moving an amount out of *Unassigned* and into a category's **Budget**. An amount may be assigned **negatively**, which moves it back out of the category and into *Unassigned* — so there is no separate act of unassigning. A negative assignment larger than the category's *Budget* is **clipped** to what is there and the shortfall is **reported** to the user; it is never refused (see *An amount may be assigned negatively* above). For an unbacked category it is a planning act only — it changes what money is *for*, not where it is, and spends nothing. For an *account-backed* category it is also a real transfer, out of the *pool account* and into the category's default backing account, either end of which can be overridden — and which goes through even when the pool account has not got the money, leaving it *Overdrawn*. Possible in the **current budget period and any later one**; assigning in a **past** period is **refused** (*Assigning happens in the current budget period and later ones*, above). **Assigning zero** is accepted and changes nothing, unlike a zero expense or income, which is refused (*Assigning zero is accepted and moves nothing*, above). Refused only for its **target** or its cents, never for being zero or negative. The refusals, in the order the first one broken is reported, are: a name that trims to nothing, a name that is not one of your categories, an amount finer than a cent, and a past period. That is the same order recording an expense uses. An otherwise acceptable zero or clippable negative is still refused if its target is wrong (*When an assignment is refused*, above). An **archived** category is not offered for assigning. Assigning a **positive** amount to its name anyway **brings it back**, and the user is told; a negative or zero assignment leaves it archived (see *Assigning to an archived category brings it back* above). Built in the assigning increment for **unbacked** categories, which is every category while there are no accounts: `Ledger.Assign`, specified by [`assign-to-category.feature`](../../features/assign-to-category.feature) ([§8.1](08-crosscutting-concepts.md)). The backed half, the real transfer, is not built. Distinct from recording the income that brought the money in, and done whenever the user is ready rather than at the moment money arrives. |
| **Budget** | The **plan** for one category in one budget period: what the user intends that category to have. "€400 for groceries in October" is a budget; "groceries" on its own is a category. A budget is never a container that can run empty — see *plan and actual* above. It **floors at zero**: a plan for less than nothing is not a plan. That is a rule about the plan and not about money in general — *Remaining* still goes negative freely, and that is *Over budget*. For an unbacked category it is also not money that has moved; for a backed one the money really has moved, but the *Budget* is still the plan and *Remaining* still measures spending against it. Budgets **carry over as figures**, offered back at the start of the next period rather than applied to it — see below. A category for which **no budget has been set** behaves exactly as one budgeted at zero: there is no separate "unbudgeted" state, and a missing budget never blocks recording an expense. Assigning changes it only in the current period or a later one, so a **past** period's budgets cannot be re-planned (*Assigning happens in the current budget period and later ones*, above). |
| **Over-assigned** | The state of a budget period whose *Unassigned* is **negative** — more has been assigned to its categories than the period's income, which assigning is allowed to do. Shown, never blocked and never warned about, exactly like the other two members of its family: *Over budget* (a negative *Remaining*) and *Overdrawn* (a negative *Balance*). A property of a **budget period**, where those two are properties of a category and of an account. Exactly zero *Unassigned* is not over-assigned. Built in the assigning increment as `Ledger.IsOverAssigned`, derived from *Unassigned* and never stored — see *Over-assigned* below. On the Overview's ring an over-assigned period is drawn as its budgets only, and *Unassigned* is shown as the negative figure itself with the same marker as *Over budget*, a revision by the stakeholder on 2026-09-25 (*One marker for over budget and over-assigned*, below). The marker's badge reads *Te veel toegewezen*. Built in the UI increment. |
| **Budget period** | The span a budget covers — normally a month. The day it starts is configurable, so it does not necessarily align with a calendar month. A start day later than a month has — the 31st in February — **clamps to that month's last day**, see *A start day the month is too short for clamps to its last day* below. A budget period **ends**, but it is never **closed** — see below. In the UI increment the start day is **fixed at the 1st** and not offered for change: deferred, not rejected, because the code cannot yet change it once budgets exist (*The period start day stays at the 1st, for now*, above). |
| **Transaction** | A single movement of money, with an amount, a date and an account. Income and expenses are both transactions. **They differ in two ways, and each difference has its own reason rather than being an inconsistency**: whether the transaction names a **category** (an expense must, an income does not — the two rows below), and whether it may be dated in the **future** (an income may, an expense may not — see *Income may be dated in the future; an expense may not*). The amount rules are the same for both: more than zero, never finer than a cent, refused rather than rounded ([§8.2](08-crosscutting-concepts.md)). Once recorded, a transaction can be **changed** or **removed**, in any budget period (*An entry can be changed or removed*, above). Settled 2026-09-26; not built. |
| **Income** | A transaction that increases the total. It does **not** name a category: it lands as *Unassigned* and is given a purpose later, by a separate act of assigning. It **must** carry a **Label** — with no category on the record, the label is the only thing that says what the money is (see below). It **may be dated in the future**, unlike an expense; it counts against the budget period its date falls in, including a period still to come, and it joins that period's *Unassigned* **from the moment it is recorded** rather than when its date arrives. May be one-off or recurring, and both permanently — see *Recurring transaction*. In the income increment an income has an amount, a date and a label, and **no account at all** — the same gap an expense has ([§11](11-risks-and-technical-debt.md)). |
| **Expense** | A transaction that decreases the total, and it **must** name a category — money being spent is money whose purpose is known by definition. Carries an **optional** **Label** of its own, below. **May not be dated in the future**, unlike an income — money not yet spent is a plan, and the plan layer already has a word for it, the *Budget* (see *Income may be dated in the future; an expense may not*). The account it leaves is **defaulted, not asked for**: the category's default backing account if the category is backed, otherwise the *pool account*, overridable per expense — see *An expense defaults to the pool account* above. May be one-off or recurring. In the first increment an expense has an amount, a date, a label and a category, and no account at all. |
| **Label** | A transaction's own free-text name, distinct from a category: "Albert Heijn" labels an expense whose category is "Groceries"; "Salaris september" labels an income that has no category at all. It says *which particular movement this was*, where a category says *what kind of spending it counts as*. **Optional on an expense, required on an income** — the asymmetry and its reason are in *Income carries a label, and it is required* below. **Always trimmed**, on both transactions: surrounding whitespace is stripped and the inner text left alone, so a label that trims to nothing is not a label — which an income refuses and an expense simply records as having none. Nothing is derived from it either way, which is why trimming costs nothing. Settled by [§1.1](01-introduction-and-goals.md) ("each labelled and categorised"), [round 1](../stakeholder/2026-09-24-interview.md) ("ik moet duidelijk kunnen aangeven waar het van is") and [round 3](../stakeholder/2026-09-24-verdieping.md) ("met een label erop"). |
| **Change** | Said of an **entry**: correcting an expense or an income after it was recorded. Allowed in **any** budget period, past ones included. A change is judged exactly as the changed entry would be if it were recorded now, so it is refused on the same rules, and a refused change leaves the entry as it was. Changing an expense's category to an archived category's name brings that category back, announced. Fixing an expense that is already on an archived category does **not** bring it back. A change **overwrites** the entry: MoneyBud keeps no record of what it was. A changed date that moves the entry to another period leaves the screen where it was and says where the entry went. See *An entry can be changed or removed* above. Settled 2026-09-26; not built. |
| **Remove** | Said of an **entry**: taking an expense or an income away entirely. Allowed in any budget period. It **asks for confirmation first**, the only act that does, because it destroys a record. Removing an income may leave its period *Over-assigned*, which is allowed and shown with the marker. See *Removing an entry asks first* above. **Not said of a category** in this sense. Where *A category is taken out of use, not deleted* speaks of "removing a category", it means round 1's "hem eruit halen", which is **archiving** it. Destroying a category with no history is **deleting** it. Settled 2026-09-26; not built. |
| **Recurring transaction** | An income or expense that repeats on a schedule — weekly, monthly, yearly. Not part of the first increment, and not part of the income increment either. When it arrives it stands **beside** one-off entry rather than replacing it: entering an amount by hand, including a future-dated one, stays a first-class act ([§1.1](01-introduction-and-goals.md) lists one-off and recurring together, not one as a stopgap for the other). |
| **Remaining** | For a category in a budget period: its *Budget* minus what has been spent against it. The one figure where the plan and the actual meet. Goes negative when a category is overspent; nothing blocks that. A negative *Remaining* is the state called *Over budget*, next. |
| **Over budget** | The state of a category whose *Remaining* is **negative** — more has been spent against it than was budgeted for it in this period. Shown, never blocked and never warned about: the expense that causes it is recorded like any other. **Exactly zero *Remaining* is not over budget** — spending a category down to nothing is the plan working, not the plan failing — and one cent past zero is. Because a category with no budget set behaves as one budgeted at zero (see *Budget*), such a category is over budget from the first cent spent against it. A property of a category **within one budget period**, so the same category can be over budget in one period and not in the next. On screen *Remaining* is shown as the negative figure itself, **with a marker** it shares with *Over-assigned*. The marker's badge reads *Over budget*. The marker is information, not a warning. This is a revision by the stakeholder on 2026-09-25 (*One marker for over budget and over-assigned*, below). Built in the UI increment. |
| **Accumulated** | **Account-backed categories only.** Everything ever assigned to the category minus everything ever spent against it — the running sum of its *Remaining* across all periods, and so the money its backing accounts have built up on its behalf. Shown beside the period's *Budget* and *Remaining*, which reset at every boundary while *Accumulated* does not. An unbacked category has no such figure, because it is swept empty at every boundary and nothing accumulates. Related to, but not equal to, a backing account's *Balance* — see *Backed categories accumulate* above. Not in the first increment. |
| **Leftover** | A category's *Remaining* when its budget period ends — money that was assigned but not spent. For an unbacked category it is *swept* rather than allowed to vanish; a backed category keeps its leftover, because that money is already in its account — see below. A leftover is computed at the end of a period; computing it does not close the period — see below. |
| **Sweep** | What happens at the end of a budget period to money that has not landed anywhere: the *Unassigned* pool and the *Leftovers* of every unbacked category are moved together into one **sweep destination**, out of the *pool account* and into that destination's default backing account. Backed categories are not swept. Automatic, not prompted — see below. |
| **Sweep destination** | The category a sweep moves money into. **Must itself be account-backed**, so that swept money really arrives somewhere. Set once as a default, applied automatically at every period end, shown in the period summary, and redirectable afterwards — see below. |
| **Net worth** | The sum of the balances of all accounts. The "how am I doing" figure, and a **point-in-time** one: it is **what you have today**. Income dated in the future is **not** counted, because it is not money yet — there is nothing in any account for it to be part of. This is where net worth and *Unassigned* part company on purpose: *Unassigned* is a **period** figure and includes an expected income from the moment it is recorded, so the two views disagree about that amount by design and not by error. See *The central distinction* above. |
| **Balance** | How much is in one account. Changed by the transactions recorded against it, by assignments to any category it backs — which really move money in — by every assignment and every sweep if it is the *pool account*, which move money out, and by the user editing it directly, which round 2 settles is allowed alongside anything MoneyBud calculates. Two mechanisms writing one number is a known risk ([§11](11-risks-and-technical-debt.md)). |
| **Overdrawn** | The state of an *account* whose *Balance* is **negative**. Reachable by assigning more than the *pool account* holds, which MoneyBud allows without blocking or warning — see *Assigning may overdraw the pool account* above. Distinct from *Over budget*, which is a negative *Remaining*: that is a plan overrun inside MoneyBud, this is a claim about the world. Not in the first increment, which has no accounts. |
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

**Settled, with scenarios approved, and not built.** The four feature files were approved at the
scenario gate on 2026-09-26: [`change-an-entry.feature`](../../features/change-an-entry.feature),
[`remove-an-entry.feature`](../../features/remove-an-entry.feature),
[`rename-a-category.feature`](../../features/rename-a-category.feature) and
[`delete-a-category.feature`](../../features/delete-a-category.feature). Six readings approved with
them are listed in *Approved at the scenario gate, 2026-09-26* (below). There is no code yet.

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

The four words are labels on buttons and on the form's state. They are not yet in *Dutch display
terms* (below), which says why.

**Left for the plan to propose**, as the ring's width was: what the form shows after *Opslaan*,
*Annuleren* or *Verwijderen*, and what happens to a change in progress when the user steps to
another period or clicks another row. One case of the first is settled (next): after saving an
unchanged entry, the form returns to normal.

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
the domain does. Not built.

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

**Settled for the next increment on 2026-09-26, and not built.** Correcting entries was taken up as
its own increment, as the paragraph above said it would be, and its rulings are in *An entry can be
changed or removed* (above). Until it is built, the paragraph above still describes MoneyBud.

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

**The last of these reasons goes once corrections are built** (*An entry can be changed or
removed*, above, settled 2026-09-26). A wrong amount will then be fixable. In the documentation's
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

**"Nog toe te wijzen" is deliberately absent.** It is the literal Dutch for *Left to assign*, which
is retired: it was merged into *Unassigned* (*One figure, not two*, above). **The retirement holds
in both languages.** On screen the figure is *Niet toegewezen*, and below zero it is *Te veel
toegewezen*.

**A term not in this table has no display term yet.** *Account*, *Net worth*, *Leftover*, *Sweep*
and the other terms of the location dimension have nothing to display in this increment. Their
Dutch is fixed when they are built, not guessed ahead of it.

**This table is read by a test.** `TekstTests` parses it from this file and holds the constants in
`Tekst` to it ([§8.4](08-crosscutting-concepts.md)). Changing a Dutch cell, or adding or removing a
row, fails the test suite until the code follows. Keep this section's heading and the table's header row
exactly as they are, because the test finds the table by them. The marker badges, the *Gearchiveerd*
caption and the *Niet teruggezet* notice all use terms from this table. No word was added for them.

**Due with the corrections increment, and deliberately not in the table yet** (2026-09-26). By this
table's own precedent, an act's button label is a display term: *Archiveren*, *Toewijzen* and
*Categorie toevoegen* are here. So the acts settled in *An entry can be changed or removed*,
*Renaming a category* and *Deleting a category that has no history anywhere* (above) will need rows:

- *Change* (an entry): **Wijzigen**, the entry form's state.
- *Remove* (an entry): **Verwijderen**.
- *Rename* (a category): **Hernoemen**.
- *Delete* (a category): **Verwijderen** as well.

All four were **approved by the stakeholder on 2026-09-26**, as proposed. **One Dutch word for two
English terms is chosen, not fallen into.** **Why**, in the documentation's reasoning, which he
chose: removing and deleting act on different things, an entry and a category, so *Verwijderen* is
never ambiguous on screen. This documentation keeps the two English
terms apart all the same, *remove* for an entry and *delete* for a category (*Remove* and *Delete*,
terms table), because in English "removing a category" already means archiving it.

*Opslaan* and *Annuleren* are the form's controls, not terms of the model, and nothing like them is
in the table. **The rows are held out of the table** because adding a row fails `TekstTests` until
`Tekst` follows, and they belong with the build that adds the constants.

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
([§8.1](08-crosscutting-concepts.md)). Carry-over is not built, and §8.1 records what the code does
meanwhile.

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
fifteen. The same rulings **widen** the question below without answering it.

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
| Is *Accumulated* a sum of *Budget* or of *Remaining*? | *Backed categories accumulate* — of *Remaining*, so it nets out spending |
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
| In which periods is an archived category still shown? | *Where an archived category is still shown* — in **every period where it has history** (a budget or an expense), **including the current one**, and in no period where it has none. Decided 2026-09-25 over "past periods only", which would leave current-period expenses unshown. This replaces "still shown in past periods", which the earlier answer said |
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
| May a category be renamed? | *Renaming a category* — **yes**, since 2026-09-26. It was first deferred with its two questions named (*Renaming a category is not in this increment*), and both are now answered: past periods show the **new** name, and a name **another** category has, archived ones included, is **refused**. Chosen over the old name in old periods and over merging. The category's own name in a new spelling is allowed, which is the front door to what adding kept out as "a rename by the back door". Same name rules as adding. An archived category can be renamed and stays archived. Settled 2026-09-26; not built |
| May a category be deleted? | *Deleting a category that has no history anywhere* — **only one with no history in any period**: no expense, and no budget of more than zero. Chosen over a stricter "never touched" rule, which would have made "assigned, then taken back to zero" a state. A separate act from archiving, with its own button on such a category only, chosen over one button with MoneyBud picking. **Not confirmed**, and announced afterwards. The recommendation was to leave deleting out, since archiving already hides such a category everywhere; the stakeholder kept it in, in his own words *so that it is gone for good and its name is free again*, because an archived category keeps its name. Settled 2026-09-26; not built |
| Which categories does MoneyBud ship with? | *The default categories* — six, Dutch, a starting set chosen to be tried. *Sparen* ships unbacked until accounts exist |
| Is a category in use shown in a period where it has no history? | *When any category is shown in a period: the full rule* — **yes in the current and future periods**, because those are the periods you plan; **no in a past one**, which is a record of what happened. A category is shown in P if it has history in P, or if it is in use and P is current or later. Chosen over "every period, always" and "only with history, always". Settled 2026-09-25; built in the UI increment |
| What does assigning to an archived category do? | *Assigning to an archived category brings it back* — the category is **not offered**, and assigning to its name anyway **brings it back**, announced, like recording an expense. A third route back, and still no un-archive act. Chosen over refusing and over offering it. Settled 2026-09-25; built in the assigning increment, which also retired the `SetBudget` scaffold that did not do this ([§8.1](08-crosscutting-concepts.md)). Refined the same day: only a **positive** assignment brings it back (next rows) |
| Does a negative assignment to an archived category bring it back? | *Only a positive assignment brings it back* — **no**. Pulling its money out is tidying up, not planning for it, and the category stays archived; an over-large one is still clipped and the shortfall reported. Chosen over one rule regardless of sign, which would make reclaiming an archived category's budget a bring-back followed by a second archive. Settled 2026-09-25; built in the assigning increment |
| May zero be assigned, and does it bring an archived category back? | *Assigning zero is accepted and moves nothing* — **accepted**, changes nothing, and does **not** bring an archived category back. Deliberately unlike a zero expense or income, which is refused because it records a transaction that never happened. Chosen over refusing it. Settled 2026-09-25; built in the assigning increment |
| In which budget periods can an amount be assigned? | *Assigning happens in the current budget period and later ones, never in a past one* — current and later, future ones without limit. It matches the display rule's split between planned periods and past records, and keeps assigning out of the sweep's territory. A past-period assignment is **refused**: first derived, then confirmed. Chosen over "any period, since periods never close" and over moving the assignment into the current period. The cost, that a forgotten plan cannot be fixed after its period, was accepted: "past is past". Settled 2026-09-25; built in the assigning increment |
| Which refusal is reported when an assignment breaks several rules, and is a zero or clippable negative still refused for a wrong target? | *When an assignment is refused* — blank name, then unknown name, then finer than a cent, then past period, the same order as recording an expense. **Yes**: the amount rules and the target rules are separate, so 0 or −500 in a past period is refused, not accepted or clipped. Settled 2026-09-25; built in the assigning increment |
| What does the assigning increment cover? | *Nothing here blocks the assigning increment* — assigning in current and later periods, *Over-assigned*, the clipped shortfall and bringing back. Backed categories, the pool account, one-action carry-over, the sweep and any UI are out, each for its own reason. Settled 2026-09-25; built to that scope |
| Is an archived category's figure offered back when a period opens? | *An archived category's figure is not offered back when a period opens* — **no**. You put it away, so MoneyBud does not suggest planning for it. If it is brought back, it is assigned to like any other. Settled 2026-09-25; carry-over is not built |
| Must category names in feature files be English? | *They are Dutch because they are content* — **no**. A name is test data because it is synthetic and no scenario leans on the defaults, not because of its language. Settled 2026-09-25, loosening the earlier "English, with the rest of the specification" |
| What does the UI cover? | *It covers what the domain does, and nothing more* — everything the domain already does, in the stakeholder's words "all that is currently working on the backend", all three routes back included. No editing or deleting of a transaction, because the domain has neither; that consequence was **accepted** by the stakeholder for the demo, and correcting entries becomes its own later increment ([§11](11-risks-and-technical-debt.md)). Settled 2026-09-25; built in the UI increment. **That later increment was settled on 2026-09-26** (*An entry can be changed or removed*), and is not built |
| Is the category box a pick-list or free text? | *Category entry is free text with suggestions* — **free text**, suggesting the offered categories and accepting any name. Chosen over a pick-list alone, which would put two routes back and the unknown-name refusal out of reach. "Not offered" means not suggested. Settled 2026-09-25; built in the UI increment |
| What does the ring show when there is nothing to draw? | *The overview, and its ring* — an **empty grey outline with a hint**, only when the period has neither income nor any *Budget*. The hint's wording is copy. Settled 2026-09-25; built in the UI increment |
| Does the UI show periods other than the current one? | *Stepping between periods* — **yes**, back and forward, because entries land in past and future periods and a current-only UI would accept entries it could never show. This is where the display rule got built, as `Ledger.CategoriesShownIn`. Settled 2026-09-25; built in the UI increment |
| Can the UI change the period start day? | *The period start day stays at the 1st, for now* — **not in this increment**. The stakeholder's ruling was "if the backend is ready, yes"; it is not, because budgets are stored against their period's first day. Deferred, not rejected; the configurable-start-day rule stands. Settled 2026-09-25 |
| What does the start screen show, and how is the ring drawn? | *The overview, and its ring* — the Overview, headed by a ring with one slice per category, sized to its *Budget* and filled as far as spent, plus a slice for *Unassigned*. Chosen over a ring of spending only and a ring of budgets only. Settled 2026-09-25; built in the UI increment |
| How does the ring show an overspent category, spending with no budget, and an over-assigned period? | Same section — an overspent slice stays budget-sized, filled and marked; spending with no budget gets no slice and is listed with the marker; an over-assigned ring shows budgets only, with *Unassigned* marked. One marker for all of them, beside the **negative figure itself**, not a positive "over by". The marker is a **revision** by the stakeholder of the earlier "plain negative figure, unremarked", for *Over budget* and *Over-assigned* only; how *Overdrawn* is shown is not settled. The over-assigned answer was chosen over a ring at income size with an overflowing segment. Settled 2026-09-25; built in the UI increment. "The same marker" was confirmed after the review to mean the same look, with each badge naming its own state (next rows) |
| What data does the UI start with, and is it kept? | *What the UI starts with, and what it keeps* — the six default categories and nothing else, and nothing is kept on close. Chosen over synthetic demo data and over saving to a file. [§8.3](08-crosscutting-concepts.md) stands. Settled 2026-09-25; built in the UI increment |
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
| Can an entry be changed or removed, and in which periods? | *An entry can be changed or removed* and *An entry in a past period can be corrected* — **yes, both, in any period**, past ones included. An entry is a fact, and "past is past" is about plans. Chosen over refusing corrections in a past period. Settled 2026-09-26; not built |
| Does removing an entry ask first? | *Removing an entry asks first* — **yes**, the only act that does, because it destroys a record. Chosen over removing then offering undo, and over removing and just saying so. The principle that leaves archiving unconfirmed, confirm only where a record is lost, gives this answer. Settled 2026-09-26; not built |
| May removing or lowering an income leave its period over-assigned? | *Removing or lowering an income may leave its period over-assigned* — **yes**, shown with the existing marker and nothing more. Chosen over saying so in the message and over refusing it. Settled 2026-09-26; not built |
| How is a changed entry judged? | *A changed entry is judged as if it were recorded now* — **exactly as if typed in fresh now**, and a refused change leaves the entry as it was. Changing an expense's category to an archived category's name brings it back, announced. Chosen over the same rules with the category staying archived. Fixing an expense that is **already on** an archived category does **not** bring it back, because that is correcting history, not using the category again. Chosen over bringing it back strictly as a fresh recording would. Settled 2026-09-26; not built |
| May a correction leave a past period over-assigned for good? | *An entry in a past period can be corrected* — **yes, accepted**. It is the true figure, the marker shows it, and it is the "past is past" cost already accepted for assigning. Chosen over allowing a negative assignment in a past period, which would have reopened that ruling. First derived, then put to the stakeholder and accepted, 2026-09-26; not built |
| What happens when a changed date moves an entry to another period? | *A changed date can move an entry to another period* — as for a new entry landing elsewhere: the screen **stays** and says where the entry went. Chosen over the screen following the entry. Settled 2026-09-26; not built |
| Is a change a new record or a rewrite? | *A change overwrites the entry* — a **rewrite**. MoneyBud keeps no record of what the entry was, and shows no history. A default the stakeholder accepted, 2026-09-26; not built |
| What happens to an archived category when its last expense in a period is removed? | *Correcting can change where a category is shown* — it **drops out of that period**, unless a budget of more than zero keeps it there. Derived from the history rule, then accepted by the stakeholder, 2026-09-26; not built |
| How is an entry picked for correcting? | *On screen: picking an entry to correct* — clicking its row loads it into its entry form, in a *Wijzigen* state with *Opslaan*, *Annuleren* and *Verwijderen*. Chosen over icons with a dialog and over editing inline. Settled 2026-09-26; not built |
| Are a change and a rename announced? | *Changes and renames are announced* — **yes, both**, afterwards, because a silent act looks the same as a failure. Chosen over announcing neither and over announcing only a rename. Settled 2026-09-26; not built |
| What happens when an entry is saved with nothing changed? | Same section — **never refused, and quiet**: nothing changes, nothing is announced, and the form returns to normal. Chosen over saying so. So the amount loaded into the form must be one the amount box accepts. Settled 2026-09-26; not built |
| What is said when the user declines to remove an entry? | *Removing an entry asks first* — **nothing**. The entry stays. Chosen over saying it was kept. Settled 2026-09-26; not built |
| What happens when a correction lands in a period already swept? | *Corrections and the sweep* — nothing is decided, because there is no sweep. A correction that means MoneyBud moved too much meets the late-expense rule. One that means there was more to move **joins the open question** above |

**Seven** of these answers were taken with their drawbacks visible rather than resolved: the
expense default is wrong for cash and nothing outside MoneyBud will say so; an overdrawn account is
shown exactly like an overspent budget despite being a harder fact (since the marker revision of
2026-09-25, this one is open again on the account side, see *Assigning may overdraw the pool
account*); the demo cannot correct a wrong entry except by starting over (settled for correction
on 2026-09-26 and planned as the next increment, not yet built); a clamped start day
produces a period that is longer than its neighbours with nothing on screen explaining why; net
worth and *Unassigned* will disagree about an expected income, because they are answering about
different moments in time; and an archived category now has three routes back rather than one
(adding its name, recording an expense, assigning a positive amount), which weakens the argument for having no
un-archive act without overturning it; and a plan forgotten in a period that has since ended cannot
be fixed, so that period shows over budget for good ("past is past"). Each is
written up where the decision is, and the first is carried in
[§11](11-risks-and-technical-debt.md). They are accepted costs, not open questions.

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
assigning answer has since been built, and so has the display rule. Carry-over has not, and §8.1
records what the code does meanwhile. Two questions arose while this model was being written up and both were
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

**Nothing here blocks the corrections increment.** Its rulings are in *An entry can be changed or
removed*, *Renaming a category* and *Deleting a category that has no history anywhere* (above), all
of 2026-09-26. The two points its first write-up left open were settled the same day. It reaches
nothing about accounts. It meets the
sweep only as a question already filed, which it widens without needing an answer.
