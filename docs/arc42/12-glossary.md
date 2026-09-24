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

**Income forms a pool.** Every euro that arrives is *Unassigned* until it is given a purpose.
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

## Assigning may overdraw the pool account

Assigning to a **backed** category moves real money out of the pool account. If the pool account
has not got it, **the assignment still goes through.** The account balance goes negative and is
shown as **overdrawn**. Nothing blocks, and nothing warns.

**Why.** Consistency with what MoneyBud already does everywhere else: it shows, it does not
enforce. *Left to assign* is shown and never enforced (below), and spending a category past its
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

## Terms

| Term | Definition |
|---|---|
| **Account** | A place where money actually sits. Current account, savings account, investment account, or cash. Answers *where*. Cash is modelled as an account despite not being a bank account. May **back** one or more categories — see below. |
| **Location** | The dimension answered by "which account". Not a separate entity — a way of grouping. |
| **Category** | What money is earmarked for: groceries, hobby, moving out. Answers *what for*. A category is a label and exists independently of any amount assigned to it. |
| **Purpose** | The dimension answered by "which category". Not a separate entity — a way of grouping. |
| **Account-backed category** | A category that names one or more accounts its money really sits in — Savings, Stocks. Most categories are not backed. The relationship is **many-to-many**: a category may be backed by several accounts, and an account may back several categories. Backing changes what assigning, spending and the end of a period do to the category — see *Account-backed categories* above. Not in the first increment, which has no accounts. |
| **Backing account** | One of the accounts backing a category. A backed category names exactly one of them as its **default backing account**: the one used whenever money moves on that category's behalf, overridable per assignment or per expense. |
| **Pool account** | The one current account designated as where *Unassigned* money is assumed to live. It is the default **source** for every movement MoneyBud makes on its own initiative — assigning to a backed category, and the end-of-period sweep — overridable per movement. It is also the account an **expense against an unbacked category** is assumed to have left, again overridable, which is a guess about a past event rather than a choice of source and is the weaker of its two roles ([§11](11-risks-and-technical-debt.md)). May go *Overdrawn*; nothing blocks that. A fact about one account, not a redefinition of *Unassigned*, which remains a purpose and not a place. Not in the first increment, which has no accounts. |
| **Unassigned** | Money that has arrived but has not been earmarked for anything yet — the pool that assigning draws from. A *purpose* — the absence of one — and not a location: unassigned money still sits in an account. Shown to the user and assigned from directly, rather than being only a figure derived from a total. Not a category, and it does not survive the end of a budget period: it is *swept* — see below. |
| **Assign** | The act of giving money a purpose: moving an amount out of *Unassigned* and into a category's **Budget**. For an unbacked category it is a planning act only — it changes what money is *for*, not where it is, and spends nothing. For an *account-backed* category it is also a real transfer, out of the *pool account* and into the category's default backing account, either end of which can be overridden — and which goes through even when the pool account has not got the money, leaving it *Overdrawn*. Distinct from recording the income that brought the money in, and done whenever the user is ready rather than at the moment money arrives. |
| **Budget** | The **plan** for one category in one budget period: what the user intends that category to have. "€400 for groceries in October" is a budget; "groceries" on its own is a category. A budget is never a container that can run empty — see *plan and actual* above. For an unbacked category it is also not money that has moved; for a backed one the money really has moved, but the *Budget* is still the plan and *Remaining* still measures spending against it. Budgets **carry over as figures**, offered back at the start of the next period rather than applied to it — see below. A category for which **no budget has been set** behaves exactly as one budgeted at zero: there is no separate "unbudgeted" state, and a missing budget never blocks recording an expense. |
| **Left to assign** | Income for a budget period minus everything assigned to categories in it. Starts at the period's full income, because carrying budgets over carries figures and not assignments. Reaches zero when the user has finished budgeting the period. Shown prominently, and never enforced — see below. |
| **Budget period** | The span a budget covers — normally a month. The day it starts is configurable, so it does not necessarily align with a calendar month. A budget period **ends**, but it is never **closed** — see below. |
| **Transaction** | A single movement of money, with an amount, a date and an account. Income and expenses are both transactions. Whether it also carries a category is not the same question for the two — see the two rows below. |
| **Income** | A transaction that increases the total. It does **not** name a category: it lands as *Unassigned* and is given a purpose later, by a separate act of assigning. May be one-off or recurring. |
| **Expense** | A transaction that decreases the total, and it **must** name a category — money being spent is money whose purpose is known by definition. Carries an optional **Label** of its own, below. The account it leaves is **defaulted, not asked for**: the category's default backing account if the category is backed, otherwise the *pool account*, overridable per expense — see *An expense defaults to the pool account* above. May be one-off or recurring. In the first increment an expense has an amount, a date, a label and a category, and no account at all. |
| **Label** | An expense's own free-text name, distinct from its category: "Albert Heijn" labels an expense whose category is "Groceries". It says *which particular purchase this was*, where the category says *what kind of spending it counts as*. **Optional** — an expense may have none, and nothing is derived from it. Settled by [§1.1](01-introduction-and-goals.md) ("each labelled and categorised") and [round 3](../stakeholder/2026-09-24-verdieping.md) ("met een label erop"). |
| **Recurring transaction** | An income or expense that repeats on a schedule — weekly, monthly, yearly. Not part of the first increment. |
| **Remaining** | For a category in a budget period: its *Budget* minus what has been spent against it. The one figure where the plan and the actual meet. Goes negative when a category is overspent; nothing blocks that. A negative *Remaining* is the state called *Over budget*, next. |
| **Over budget** | The state of a category whose *Remaining* is **negative** — more has been spent against it than was budgeted for it in this period. Shown, never blocked and never warned about: the expense that causes it is recorded like any other. **Exactly zero *Remaining* is not over budget** — spending a category down to nothing is the plan working, not the plan failing — and one cent past zero is. Because a category with no budget set behaves as one budgeted at zero (see *Budget*), such a category is over budget from the first cent spent against it. A property of a category **within one budget period**, so the same category can be over budget in one period and not in the next. |
| **Accumulated** | **Account-backed categories only.** Everything ever assigned to the category minus everything ever spent against it — the running sum of its *Remaining* across all periods, and so the money its backing accounts have built up on its behalf. Shown beside the period's *Budget* and *Remaining*, which reset at every boundary while *Accumulated* does not. An unbacked category has no such figure, because it is swept empty at every boundary and nothing accumulates. Related to, but not equal to, a backing account's *Balance* — see *Backed categories accumulate* above. Not in the first increment. |
| **Leftover** | A category's *Remaining* when its budget period ends — money that was assigned but not spent. For an unbacked category it is *swept* rather than allowed to vanish; a backed category keeps its leftover, because that money is already in its account — see below. A leftover is computed at the end of a period; computing it does not close the period — see below. |
| **Sweep** | What happens at the end of a budget period to money that has not landed anywhere: the *Unassigned* pool and the *Leftovers* of every unbacked category are moved together into one **sweep destination**, out of the *pool account* and into that destination's default backing account. Backed categories are not swept. Automatic, not prompted — see below. |
| **Sweep destination** | The category a sweep moves money into. **Must itself be account-backed**, so that swept money really arrives somewhere. Set once as a default, applied automatically at every period end, shown in the period summary, and redirectable afterwards — see below. |
| **Net worth** | The sum of the balances of all accounts. The "how am I doing" figure. |
| **Balance** | How much is in one account. Changed by the transactions recorded against it, by assignments to any category it backs — which really move money in — by every assignment and every sweep if it is the *pool account*, which move money out, and by the user editing it directly, which round 2 settles is allowed alongside anything MoneyBud calculates. Two mechanisms writing one number is a known risk ([§11](11-risks-and-technical-debt.md)). |
| **Overdrawn** | The state of an *account* whose *Balance* is **negative**. Reachable by assigning more than the *pool account* holds, which MoneyBud allows without blocking or warning — see *Assigning may overdraw the pool account* above. Distinct from *Over budget*, which is a negative *Remaining*: that is a plan overrun inside MoneyBud, this is a claim about the world. Not in the first increment, which has no accounts. |

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

## Budgets carry over as figures, not as assignments

**The figures are remembered; the money is not assigned.**

When a budget period opens, each category's amount from the previous period is remembered and
**offered back** — but nothing has been assigned yet. The pool starts whole: the period's income is
entirely *Unassigned*, every category's *Budget* is zero until the user acts, and *Left to assign*
starts at the full income. **One action assigns last period's plan in full**, after which individual
figures can be adjusted like any other.

**Why.** This is the shape that makes both of the things we want true at once.

- **A period genuinely starts with everything unassigned**, so the pool model holds without
  exception. *Left to assign* keeps the meaning it was given — "how much of my money still needs a
  job" — instead of starting deeply negative and climbing towards zero as the salary lands, which
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
budget period ends, whichever comes first — see next.

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
  forward would break *Left to assign*: the figure would stop being "this period's income minus
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
is no action to hang a choice on. A prompt would have to be raised out of nowhere, and *Left to
assign: shown, never enforced* (below) has already settled that MoneyBud does not nag or block over
money that has not been given a job. Automatic-but-visible-and-reversible is the only shape that
loses no money and demands nothing: the sweep always happens, the summary always says where it
went, and a user who disagrees moves it afterwards — which is an ordinary transfer between two
backed categories, not a special case.

This replaces an earlier reading of these as two separate decisions the user takes from two
different places. They are one automatic movement with one destination.

## Left to assign: shown, never enforced

MoneyBud shows a prominent **Left to assign** figure: income for the period minus everything
assigned. When it reaches zero, every euro has a job and the user has finished budgeting the
period.

That is the whole of it. MoneyBud never blocks an action, refuses a period, or nags because *Left
to assign* is not zero. **A period is never "incomplete"** in any state the software recognises;
the figure is information, not a gate.

**Why.** The stakeholder's stated motivation is to be *more motivated*, not better policed
([§1.1](01-introduction-and-goals.md)). Making the gap obvious serves goal 1 — you can see at a
glance whether your money has been given jobs — and costs goal 2 nothing. Enforcing it would add
precisely the friction that gets budgeting apps abandoned, and would punish the user for the normal
case of not having decided yet.

## Dutch source terms

The stakeholder material is in Dutch. This table fixes the mapping, so that reading the interviews
alongside this documentation does not introduce drift.

| Dutch (stakeholder) | English (this project) |
|---|---|
| Potje | Category, together with its Budget for the current period. The Dutch word is a container metaphor — a little pot money sits in — and that part does **not** carry over: a Budget is a plan, not a pot (see *plan and actual* above) |
| Potje dat op een plek staat | Account-backed category. The *staat op* is the backing: this pot's money really is in that account |
| Plek | Location — expressed as an Account |
| Doel | Purpose — expressed as a Category |
| Rekening | Account |
| Vermogen | Net worth |
| Inkomsten / Uitgaven | Income / Expenses |
| Overzichtelijk | Legible, clear at a glance — see quality goal 1 in [§1](01-introduction-and-goals.md) |

## Open questions

**None.** Every question that has stood here has been answered by the stakeholder, and each answer
is written up in the section it belongs to rather than kept in a list here:

| Question | Where the answer lives |
|---|---|
| Does assigning move real money? | *Account-backed categories* — exactly when the category is backed |
| What happens to leftover and unassigned money at a period end? | *The sweep* — one automatic movement into a preset backed destination, shown and reversible |
| Where does money MoneyBud moves itself come from? | *The pool account* |
| Does a backed category show a total across periods? | *Backed categories accumulate* — **Accumulated** |
| Is *Accumulated* a sum of *Budget* or of *Remaining*? | *Backed categories accumulate* — of *Remaining*, so it nets out spending |
| Does an expense default to an account? | *An expense defaults to the pool account* — yes, overridable per expense, with a known weak spot recorded there and in [§11](11-risks-and-technical-debt.md) |
| May assigning overdraw the pool account? | *Assigning may overdraw the pool account* — yes, shown, not blocked |

Two of these answers were taken with their drawbacks visible rather than resolved: the expense
default is wrong for cash and nothing outside MoneyBud will say so, and an overdrawn account is
shown exactly like an overspent budget despite being a harder fact. Both are written up where the
decision is, and the first is carried in [§11](11-risks-and-technical-debt.md). They are accepted
costs, not open questions.

**Nothing here blocks the first increment**, which has no accounts at all
([§11](11-risks-and-technical-debt.md)) and so reaches none of the account-related answers above.
