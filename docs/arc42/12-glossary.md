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
it belongs to a UI that does not exist, and nothing in this section should be read as specifying a
sentence.

**Nothing is built for this.** There is no assigning in the code at all: `Ledger.SetBudget` writes
a plan directly, standing in for an act that does not exist yet
([§8.1](08-crosscutting-concepts.md)), and no approved scenario assigns anything. What this section
settles is the model that the assign-from-the-pool scenarios will be written against.

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
than an inference. It **changes current behaviour**: today that expense is refused as naming a
category the user does not have ([§8.1](08-crosscutting-concepts.md)). And an expense whose category
name trims to nothing has named no category, so it takes the existing refusal for that
([`features/record-expense.feature`](../../features/record-expense.feature), *An expense must name
a category*) — a name that trims to nothing is no name, whether it is being added or recorded
against. No approved scenario spells a category with surrounding spaces or as spaces alone; this
is what those scenarios will be written against.

**This is a correction, not a new rule.** Category names are case-**sensitive** today: `Ledger` keys
its categories with `StringComparer.Ordinal`. Nothing chose that — it arrived with the first
increment's scaffold, where no approved scenario ever spelled one category two ways, so the choice
was never visible enough to be made. It is recorded here as something decided rather than inherited,
and [§8.1](08-crosscutting-concepts.md) carries the fact that the code disagrees until the category
increment is built. **The whitespace half is a correction of the same kind**: `Ledger.AddCategory`
neither trims a name nor refuses a blank one today, for the same reason — nothing ever asked it to.

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

**Deferred, not rejected.** Renaming has questions of its own that nothing here answers: whether past
periods show the old name or the new one, and what happens when the new name is one the user already
has — the rule above answers that for *adding*, where handing back the existing category is an
available move, and it is not available to someone who is already holding a category.

It is cheap to add later and nothing settled here forecloses it.

## A category is taken out of use, not deleted

> **Removing a category takes it out of new entry. Its history stays.** It is no longer offered when
> recording. Its expenses, its budgets and its place in every period's figures all remain exactly
> as they were, and **every budget period in which it has history still shows it — including the
> current one** (*Where an archived category is still shown*, below).

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

- The category is **not offered when recording** something new. Not being offered is not the same
  as being refused: an expense recorded against its name is recorded, and brings it back (*Recording
  an expense against an archived category brings it back*, below).
- It **still owns its expenses**. Nothing is reassigned, nothing is orphaned.
- It **still appears in every budget period where it has history** — a budget of more than zero
  or an expense in that period — with the budgets and figures it had there. **That includes the
  current period.** A period in which it has no history does not show it (*Where an archived
  category is still shown*, below).
- Archiving is **never confirmed first**, and the user is **told afterwards** that the category was
  archived (next).
- The state is about **new entry only**. It says nothing about money.
- It is **not permanent**: adding its name again brings the category back, and so does recording
  an expense against it — both below.

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

**Being told is not being warned.** The message is information after the fact, never a question
first. This is what separates it from the rejected confirmations: those ask before, and this only
reports after. Like the other outcomes, what is fixed is **that** the user is told, not the wording.

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
next section says how far it still does.

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

### They are Dutch, and the feature files are English. That is not an inconsistency

| | What it is | Language |
|---|---|---|
| "Groceries", "Hobby", "Subscriptions", "Gifts", "Holiday" in [`record-expense.feature`](../../features/record-expense.feature) | **Synthetic test data** — names invented to make a scenario readable, standing in for whatever the user really has | English, with the rest of the specification |
| Boodschappen, Huur, Hobby, Sparen, Verzekeringen, Abonnementen | **User-facing content** — the strings MoneyBud actually puts on the user's screen | Dutch, because the user is Dutch |

Three of those test names — Groceries, Hobby, Subscriptions — happen to be the English of three
defaults, which makes the distinction easy to miss. It holds anyway: **the approved scenarios do not
set up the default set.** Each names the categories it needs with an explicit *Given*, which is what
keeps them readable and what lets the default list change without touching a single scenario.

[§2](02-architecture-constraints.md) requires the documentation and the specification to be English.
That constrains what MoneyBud's authors write **about** the app; it says nothing about what the app
**displays** to its one Dutch user.

**This is a different thing from *Dutch source terms* (below), and confusing the two would be easy.**
That table maps the stakeholder's Dutch **vocabulary** onto this project's English **terms**, so that
reading the interviews alongside this documentation does not introduce drift — "potje" is a word he
says, not a word MoneyBud shows anyone. These six names are **not vocabulary**. They are content:
strings the app ships with, which the user sets to zero or archives like any other category. Nothing
translates them, because there is nothing to keep in step.

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
| **Category** | What money is earmarked for: groceries, hobby, moving out. Answers *what for*. A category is a label and exists independently of any amount assigned to it. Its **name** is **trimmed** at the ends. It is compared **case-insensitively**, with any run of inner whitespace counting as one space. It is stored trimmed, with its capitalisation and inner spacing as typed. So there are never two categories that differ only in case or spacing. A name that trims to nothing is **refused**. Adding a name that already exists hands back the category that already has it, **spelled as it already was**, with the user told so (see *A category name is compared case-insensitively* above). Taken out of use by **archiving**, never by deleting: its history stays, and adding its name again or recording an expense against it brings it back (*A category is taken out of use, not deleted*, above). **Renaming** is not in this increment. MoneyBud ships with six **default categories** (above). |
| **Archived** | The state of a category that has been taken out of use. It is **no longer offered when recording**, and everything it already owns stays: its expenses, its budgets, and its place in those periods' figures. It is **shown in every budget period where it has history** — a budget of more than zero or an expense in that period — **including the current one**, and not in a period where it has none, so a zero budget alone does not count (*Where an archived category is still shown*, above). Archiving is **never confirmed first**, and the user is **told afterwards** that the category was archived (*Archiving is announced, never confirmed*, above). Archiving destroys no record, which is why the state is not called *removed*, and it is **not permanent**. It is **brought back**, history and all and spelled as it was, by either of two acts the user already has: **adding its name** again, or **recording an expense against it** — which records the expense rather than refusing it. Either way the user is told it was brought back. There is no separate act of un-archiving, for the same reason there is no separate act of unassigning; bringing back is a side-effect of those two acts, always announced. Only a category in use can be archived. Distinct from a period being **closed** — a state MoneyBud deliberately has not got (*Ending versus closing a budget period*, below). See *A category is taken out of use, not deleted* above. No code and no scenarios ([§8.1](08-crosscutting-concepts.md)). |
| **Default categories** | The six categories MoneyBud ships with: **Boodschappen, Huur, Hobby, Sparen, Verzekeringen, Abonnementen**. A starting set chosen to be tried, not a claim about what a household needs. Their names are **Dutch** because they are user-facing **content**, unlike the English names in the feature files, which are synthetic test data — and unlike *Dutch source terms* below, which is vocabulary rather than content. *Sparen* ships **unbacked** and becomes an *account-backed category* when accounts exist. See *The default categories* above. |
| **Purpose** | The dimension answered by "which category". Not a separate entity — a way of grouping. |
| **Account-backed category** | A category that names one or more accounts its money really sits in — Savings, Stocks. Most categories are not backed. The relationship is **many-to-many**: a category may be backed by several accounts, and an account may back several categories. Backing changes what assigning, spending and the end of a period do to the category — see *Account-backed categories* above. Not in the first increment, which has no accounts. |
| **Backing account** | One of the accounts backing a category. A backed category names exactly one of them as its **default backing account**: the one used whenever money moves on that category's behalf, overridable per assignment or per expense. |
| **Pool account** | The one current account designated as where *Unassigned* money is assumed to live. It is the default **source** for every movement MoneyBud makes on its own initiative — assigning to a backed category, and the end-of-period sweep — overridable per movement. It is also the account an **expense against an unbacked category** is assumed to have left, again overridable, which is a guess about a past event rather than a choice of source and is the weaker of its two roles ([§11](11-risks-and-technical-debt.md)). May go *Overdrawn*; nothing blocks that. A fact about one account, not a redefinition of *Unassigned*, which remains a purpose and not a place. Not in the first increment, which has no accounts. |
| **Unassigned** | Two things under one name, deliberately. (a) The **absence of a purpose**: a value on the purpose dimension, not a location — unassigned money still sits in an account. (b) The **figure** that measures it for one budget period: that period's income minus everything assigned to categories in it. It is the pool that assigning draws from and that a negative assignment puts money back into. Starts at the period's full income, because carrying budgets over carries figures and not assignments; reaches zero when the user has finished budgeting the period; goes **negative** past that, which is *Over-assigned*. Shown prominently and assigned from directly, rather than being only a total the user has to work out — and never enforced. Not a category: nothing is budgeted for it and nothing is spent against it. Does not survive the end of a budget period: it is *swept* — see below. An income joins its period's *Unassigned* **when it is recorded**, which for a future-dated income is before its date arrives — so *Unassigned* covers a **whole period** where *Net worth* covers a **point in time**, and the two disagree about expected income by design (*The central distinction*, above). Formerly also called *Left to assign*; that name is retired — see *One figure, not two*. |
| **Assign** | The act of giving money a purpose: moving an amount out of *Unassigned* and into a category's **Budget**. An amount may be assigned **negatively**, which moves it back out of the category and into *Unassigned* — so there is no separate act of unassigning. A negative assignment larger than the category's *Budget* is **clipped** to what is there and the shortfall is **reported** to the user; it is never refused (see *An amount may be assigned negatively* above). For an unbacked category it is a planning act only — it changes what money is *for*, not where it is, and spends nothing. For an *account-backed* category it is also a real transfer, out of the *pool account* and into the category's default backing account, either end of which can be overridden — and which goes through even when the pool account has not got the money, leaving it *Overdrawn*. Distinct from recording the income that brought the money in, and done whenever the user is ready rather than at the moment money arrives. |
| **Budget** | The **plan** for one category in one budget period: what the user intends that category to have. "€400 for groceries in October" is a budget; "groceries" on its own is a category. A budget is never a container that can run empty — see *plan and actual* above. It **floors at zero**: a plan for less than nothing is not a plan. That is a rule about the plan and not about money in general — *Remaining* still goes negative freely, and that is *Over budget*. For an unbacked category it is also not money that has moved; for a backed one the money really has moved, but the *Budget* is still the plan and *Remaining* still measures spending against it. Budgets **carry over as figures**, offered back at the start of the next period rather than applied to it — see below. A category for which **no budget has been set** behaves exactly as one budgeted at zero: there is no separate "unbudgeted" state, and a missing budget never blocks recording an expense. |
| **Over-assigned** | The state of a budget period whose *Unassigned* is **negative** — more has been assigned to its categories than the period's income, which assigning is allowed to do. Shown, never blocked and never warned about, exactly like the other two members of its family: *Over budget* (a negative *Remaining*) and *Overdrawn* (a negative *Balance*). A property of a **budget period**, where those two are properties of a category and of an account. **Not in the income increment**: with no act of assigning, nothing subtracts from *Unassigned*, so it cannot go negative yet — see *Over-assigned* below. |
| **Budget period** | The span a budget covers — normally a month. The day it starts is configurable, so it does not necessarily align with a calendar month. A start day later than a month has — the 31st in February — **clamps to that month's last day**, see *A start day the month is too short for clamps to its last day* below. A budget period **ends**, but it is never **closed** — see below. |
| **Transaction** | A single movement of money, with an amount, a date and an account. Income and expenses are both transactions. **They differ in two ways, and each difference has its own reason rather than being an inconsistency**: whether the transaction names a **category** (an expense must, an income does not — the two rows below), and whether it may be dated in the **future** (an income may, an expense may not — see *Income may be dated in the future; an expense may not*). The amount rules are the same for both: more than zero, never finer than a cent, refused rather than rounded ([§8.2](08-crosscutting-concepts.md)). |
| **Income** | A transaction that increases the total. It does **not** name a category: it lands as *Unassigned* and is given a purpose later, by a separate act of assigning. It **must** carry a **Label** — with no category on the record, the label is the only thing that says what the money is (see below). It **may be dated in the future**, unlike an expense; it counts against the budget period its date falls in, including a period still to come, and it joins that period's *Unassigned* **from the moment it is recorded** rather than when its date arrives. May be one-off or recurring, and both permanently — see *Recurring transaction*. In the income increment an income has an amount, a date and a label, and **no account at all** — the same gap an expense has ([§11](11-risks-and-technical-debt.md)). |
| **Expense** | A transaction that decreases the total, and it **must** name a category — money being spent is money whose purpose is known by definition. Carries an **optional** **Label** of its own, below. **May not be dated in the future**, unlike an income — money not yet spent is a plan, and the plan layer already has a word for it, the *Budget* (see *Income may be dated in the future; an expense may not*). The account it leaves is **defaulted, not asked for**: the category's default backing account if the category is backed, otherwise the *pool account*, overridable per expense — see *An expense defaults to the pool account* above. May be one-off or recurring. In the first increment an expense has an amount, a date, a label and a category, and no account at all. |
| **Label** | A transaction's own free-text name, distinct from a category: "Albert Heijn" labels an expense whose category is "Groceries"; "Salaris september" labels an income that has no category at all. It says *which particular movement this was*, where a category says *what kind of spending it counts as*. **Optional on an expense, required on an income** — the asymmetry and its reason are in *Income carries a label, and it is required* below. **Always trimmed**, on both transactions: surrounding whitespace is stripped and the inner text left alone, so a label that trims to nothing is not a label — which an income refuses and an expense simply records as having none. Nothing is derived from it either way, which is why trimming costs nothing. Settled by [§1.1](01-introduction-and-goals.md) ("each labelled and categorised"), [round 1](../stakeholder/2026-09-24-interview.md) ("ik moet duidelijk kunnen aangeven waar het van is") and [round 3](../stakeholder/2026-09-24-verdieping.md) ("met een label erop"). |
| **Recurring transaction** | An income or expense that repeats on a schedule — weekly, monthly, yearly. Not part of the first increment, and not part of the income increment either. When it arrives it stands **beside** one-off entry rather than replacing it: entering an amount by hand, including a future-dated one, stays a first-class act ([§1.1](01-introduction-and-goals.md) lists one-off and recurring together, not one as a stopgap for the other). |
| **Remaining** | For a category in a budget period: its *Budget* minus what has been spent against it. The one figure where the plan and the actual meet. Goes negative when a category is overspent; nothing blocks that. A negative *Remaining* is the state called *Over budget*, next. |
| **Over budget** | The state of a category whose *Remaining* is **negative** — more has been spent against it than was budgeted for it in this period. Shown, never blocked and never warned about: the expense that causes it is recorded like any other. **Exactly zero *Remaining* is not over budget** — spending a category down to nothing is the plan working, not the plan failing — and one cent past zero is. Because a category with no budget set behaves as one budgeted at zero (see *Budget*), such a category is over budget from the first cent spent against it. A property of a category **within one budget period**, so the same category can be over budget in one period and not in the next. |
| **Accumulated** | **Account-backed categories only.** Everything ever assigned to the category minus everything ever spent against it — the running sum of its *Remaining* across all periods, and so the money its backing accounts have built up on its behalf. Shown beside the period's *Budget* and *Remaining*, which reset at every boundary while *Accumulated* does not. An unbacked category has no such figure, because it is swept empty at every boundary and nothing accumulates. Related to, but not equal to, a backing account's *Balance* — see *Backed categories accumulate* above. Not in the first increment. |
| **Leftover** | A category's *Remaining* when its budget period ends — money that was assigned but not spent. For an unbacked category it is *swept* rather than allowed to vanish; a backed category keeps its leftover, because that money is already in its account — see below. A leftover is computed at the end of a period; computing it does not close the period — see below. |
| **Sweep** | What happens at the end of a budget period to money that has not landed anywhere: the *Unassigned* pool and the *Leftovers* of every unbacked category are moved together into one **sweep destination**, out of the *pool account* and into that destination's default backing account. Backed categories are not swept. Automatic, not prompted — see below. |
| **Sweep destination** | The category a sweep moves money into. **Must itself be account-backed**, so that swept money really arrives somewhere. Set once as a default, applied automatically at every period end, shown in the period summary, and redirectable afterwards — see below. |
| **Net worth** | The sum of the balances of all accounts. The "how am I doing" figure, and a **point-in-time** one: it is **what you have today**. Income dated in the future is **not** counted, because it is not money yet — there is nothing in any account for it to be part of. This is where net worth and *Unassigned* part company on purpose: *Unassigned* is a **period** figure and includes an expected income from the moment it is recorded, so the two views disagree about that amount by design and not by error. See *The central distinction* above. |
| **Balance** | How much is in one account. Changed by the transactions recorded against it, by assignments to any category it backs — which really move money in — by every assignment and every sweep if it is the *pool account*, which move money out, and by the user editing it directly, which round 2 settles is allowed alongside anything MoneyBud calculates. Two mechanisms writing one number is a known risk ([§11](11-risks-and-technical-debt.md)). |
| **Overdrawn** | The state of an *account* whose *Balance* is **negative**. Reachable by assigning more than the *pool account* holds, which MoneyBud allows without blocking or warning — see *Assigning may overdraw the pool account* above. Distinct from *Over budget*, which is a negative *Remaining*: that is a plan overrun inside MoneyBud, this is a claim about the world. Not in the first increment, which has no accounts. |

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

**Naming it is what makes the merge above safe.** The one real objection to folding *Left to
assign* into *Unassigned* is that "unassigned" reads oddly below zero: money cannot be less than
unassigned, so the merged figure appears to describe something impossible. *Over-assigned* answers
it. Below zero the figure has stopped describing money waiting for a purpose and started describing
a plan that outruns the income, and that is a different enough thing to deserve its own word —
exactly as *Over budget* is the word for a *Remaining* that has stopped describing money left.

**Not in the income increment.** With no act of assigning, nothing subtracts from *Unassigned*:
recording income only ever increases it, so it cannot go negative. No code and no scenario in this
increment will reach *Over-assigned*. It is defined now because the merge above needed it, not
because anything is about to use it.

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
whitespace in a name is compared, and whether archiving is confirmed and announced. The *Answered* table below says where each answer lives.

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
| Are category names case-sensitive? | *A category name is compared case-insensitively and stored as typed, trimmed* — compared without case, kept as typed. A **correction**: the code is case-sensitive by accident, not by decision ([§8.1](08-crosscutting-concepts.md)) |
| What about whitespace around a category name, and a blank one? | Same section — **trimmed** at the ends, so "  Hobby  " is "Hobby" and the same category as "hobby"; a name that trims to nothing is **refused**. The label rule's whitespace half, with the required income label's consequence |
| What about whitespace inside a category name? | Same section — **ignored for comparison, kept as typed**. Any run of inner whitespace counts as one space, so "Vaste  lasten" is the same category as "Vaste lasten", and adding it hands back the existing one spelled as it was. Chosen over "inner text untouched, so two categories", which was only the literal consequence of the first wording. Settled 2026-09-25. This answer first said "inner text left alone", which is still true of what is stored |
| Does trimming and ignoring case apply when recording an expense against a name, or only when adding one? | Same section — **both**. Recording against "  groceries " records against Groceries. **Confirmed** by the stakeholder on 2026-09-25, not only inferred from "every comparison". It changes current behaviour, which refuses that expense as naming an unknown category ([§8.1](08-crosscutting-concepts.md)) |
| What happens when the user adds a name they already have? | Same section — they get the existing category back, and are told so. Derived and not contradicted. It does **not** answer the archived case, which *Adding an archived category's name brings it back* answers separately |
| Does adding a name in a different capitalisation change the existing category's spelling? | Same section, *The existing spelling is kept* — **no**, for an active category, an archived one brought back by adding its name, and one brought back by recording an expense against it. Taking the new spelling would be a rename by the back door |
| May a category be renamed? | *Renaming a category is not in this increment* — deferred with its two questions named, not rejected |
| Which categories does MoneyBud ship with? | *The default categories* — six, Dutch, a starting set chosen to be tried. *Sparen* ships unbacked until accounts exist |
| Does expected money have a location, given that future-dated income is *Unassigned* before it arrives? | *The central distinction* — the question does not arise: expected income is **not money yet**, so there is no euro to lack a location, and the rule survives untouched. What it does cost is stated there and under *Net worth*: net worth is a point-in-time figure that excludes expected income, *Unassigned* is a period figure that includes it, and the two disagree by design |

**Five** of these answers were taken with their drawbacks visible rather than resolved: the
expense default is wrong for cash and nothing outside MoneyBud will say so; an overdrawn account is
shown exactly like an overspent budget despite being a harder fact; a clamped start day
produces a period that is longer than its neighbours with nothing on screen explaining why; net
worth and *Unassigned* will disagree about an expected income, because they are answering about
different moments in time; and an archived category now has two routes back rather than one,
which weakens the argument for having no un-archive act without overturning it. Each is
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

**Nothing here blocks the category increment.** Everything it reaches is settled — the name rule, the
duplicate rule, archiving and bringing back, the default set — and it reaches nothing about accounts,
so backed categories, the pool account and the sweep stay out of scope exactly as they did for the
two increments before it. Two questions arose while this model was being written up and both were
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
