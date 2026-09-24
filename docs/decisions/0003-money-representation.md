# 0003 — How money is represented in code

**Status:** Accepted
**Date:** 2026-09-24
**Amended:** 2026-09-24, same day — see *No currency field*. The decision is unchanged; one
statement it made about [§2](../arc42/02-architecture-constraints.md) stopped being true within
hours of being written, and the paragraph that made it has been corrected in place. Why that was
done here rather than in a superseding record is explained there.

## Context

The implementation plan for the first increment — making
[`features/record-expense.feature`](../../features/record-expense.feature) pass — is the first code
that will touch an amount, and the working agreement is that money handling is decided before it is
coded.

[§8.2](../arc42/08-crosscutting-concepts.md) had settled exactly one rule: every amount is a whole
number of cents, an amount finer than a cent is **refused** at entry rather than rounded, and
therefore MoneyBud has no rounding rule because it never rounds. Everything else about money was
listed there as open. Three of those open rows have to close now, because no money-handling code
can be written without them:

- how an amount is represented in code;
- the sign convention for inflow versus outflow;
- whether the model carries a currency field.

They are answered in one record because they are one question seen from three sides: what a
monetary amount *is* in this codebase. The answers also lean on each other — the sign convention is
only workable because the type is signed, and dropping the currency field is only safe because the
type itself means one particular currency.

[ADR 0001](0001-dotnet-and-reqnroll.md) cites C#'s built-in `decimal` as part of why C# suits this
domain, so this record also has to say where `decimal` ends up.

## Decision

**1. A `Money` value type, wrapping a whole number of cents as a `long`.** `Money` is **signed**:
it can hold a negative amount, because several of the figures MoneyBud shows are negative in normal
use.

**2. Amounts on transactions are positive magnitudes. Direction is carried by the transaction
type** — an *Income* increases, an *Expense* decreases — and neither stores a negative amount.
Derived figures are unaffected by this and still go negative freely.

**3. There is no currency field.** A `Money` means euro cents.

**And `Money` is the type of the domain's interior, not of its input boundary.** The API that
records an expense takes a **`decimal` euro amount**, not a `Money`; conversion happens only after
validation has passed. The reasoning for that is in *Consequences*, because it is a real constraint
the type imposes rather than an implementation detail.

## Why

### A whole-cents integer, rather than a `decimal`

The whole-cents rule already settled in §8.2 makes an integer of cents **exactly sufficient**:
there is no sub-cent value MoneyBud is ever allowed to hold, so nothing is lost by having no way to
express one. That is the whole argument, and it is worth stating the other way round too — with a
`long` of cents, a sub-cent amount is **unrepresentable rather than merely refused**. The rule is
enforced by the shape of the type instead of by a guard that has to be remembered wherever an
amount is constructed.

Arithmetic and equality are exact by construction. A `decimal` gives that too, so exactness is not
what decides this; what decides it is that `decimal` leaves the whole-cents invariant as something
the code has to keep on being right about, and `long` cents makes it structural.

Two alternatives were considered and rejected:

- **A `Money` wrapper over `decimal`.** It keeps ADR 0001's `decimal` closer to the surface and it
  is exact, but the whole-cents invariant becomes a runtime guard inside the wrapper rather than a
  property of the representation. Every `Money` would have a precision that has to be checked and
  maintained, where a cent integer simply has not got one.
- **A bare `decimal`, no wrapper at all.** The least code to write. Rejected because any `decimal`
  can then be passed anywhere money is expected — a rate, a count, a half-cent — so the cent rule
  has to be re-checked at every boundary in the system rather than established once. It also gives
  up the chance to make the domain's vocabulary visible in its types, which matters for quality
  goal 3, adaptability ([§1.2](../arc42/01-introduction-and-goals.md)).

**This does not contradict ADR 0001.** `decimal` remains the type amounts are *entered and parsed*
as, which is exactly the property ADR 0001 was pointing at when it said C# suits this domain: the
conversion from typed text to an exact base-10 amount happens in `decimal`, without the
binary-floating-point error that makes `double` unusable for money. What changes is that `decimal`
stops at the boundary instead of travelling through the domain.

### Why `Money` is signed

Because negative money is ordinary here, not an error. [§12](../arc42/12-glossary.md) has at least
three figures that go negative in normal use:

| Figure | Negative means |
|---|---|
| *Remaining* | The category is *Over budget* — a state the feature file asserts scenario by scenario |
| *Left to assign* | More has been assigned than the period's income, which assigning is allowed to do |
| *Balance* | The account is *Overdrawn*, which MoneyBud shows and never blocks |

An unsigned type would force every one of those to be modelled as a magnitude plus a separate flag,
or to escape the type altogether — and *Remaining* is computed by subtraction, so the very
operation that produces it would need somewhere to put a negative result.

### Positive magnitudes on transactions

The approved scenarios phrase the validation as **"an expense must be more than 0 euro"**, and they
refuse `-0.01`, `-10.00` and `0.00` on that ground. That phrasing is a statement *about what an
expense is* — money that was spent — and the positive-magnitude convention keeps it one. Under
signed storage the refusal of `-10.00` would be an arithmetic accident: a negative expense would be
an expense that adds money back, which is a coherent thing for the arithmetic to do and an
incoherent thing for the domain to mean.

The rejected alternative is **signed storage, with expenses held as negative amounts**. Its
attraction is real: a period total becomes a plain sum with no case analysis. It was not chosen
because it contradicts how the approved scenarios phrase the rule, and because the case analysis it
removes is one `switch` over two transaction types, in one place, against a rule that has to hold
everywhere.

Note what this convention does **not** cover. It constrains the amount on a *Transaction*. It says
nothing about *Budget*, and it deliberately does not constrain derived figures at all — *Remaining*
below zero is the *Over budget* state and must stay reachable.

### No currency field

[§2](../arc42/02-architecture-constraints.md) constrains MoneyBud to euro only, and to a single
user, so a currency field could only ever hold one value. A field with one possible value carries no
information: it cannot be read to learn anything, nothing can branch on it, and every comparison
between two amounts would have to handle a mismatch that cannot occur. Leaving it out means `Money`
means euro cents, full stop, and nothing in the code pretends otherwise.

The rejected alternative is **carrying the field anyway, against a future second currency**. It is
cheap, and the usual argument for it is migration: adding a currency to stored amounts later is
more painful than having it there from the start. That argument does not apply here, because there
is no data to migrate — the first version is a throwaway demo whose data is disposable
([§1.1](../arc42/01-introduction-and-goals.md), [ADR 0002](0002-desktop-application-first.md)).
Paying now to avoid a migration of nothing is paying for nothing.

**Amended 2026-09-24, hours after this record was accepted.** As first written, this section ended
by admitting that the euro-only constraint was recorded in §2 as an *assumption stated back to the
stakeholder and not contradicted*, that this made it the softest of the three decisions here, and
that §2 was weaker than the argument needed. That admission is what made the gap visible, and
acting on it is what closed it: the question was put to the stakeholder, and he settled euro only
**deliberately** ([§2](../arc42/02-architecture-constraints.md)). The argument above no longer
rests on silence, so the hedge is no longer true and has been replaced by this note rather than
left standing.

**Why amended here and not superseded by a new record.** MoneyBud does not rewrite decisions that
turn out wrong; it supersedes them, because what we believed is part of the record. This is not
that case. The **decision did not change** — there is still no currency field, for the same reason.
What changed is a fact this record cited about another document, and it changed the same day, with
nothing built on it and no other record citing it. A superseding ADR 0005 would have read "the same
decision, now better founded", splitting one decision across two records and making the index
report a count of decisions that were never separately taken. The belief being corrected is quoted
above rather than deleted, which is what the supersession rule is protecting.

## Consequences

- **`Money` is the domain's interior type, and `decimal` is its input boundary type.** The API that
  records an expense takes a `decimal` euro amount. This is forced, not stylistic. An amount
  arriving from the user is untrusted input that is not yet known to be money at all, and `Money`
  is a type all of whose values are valid — so an invalid amount cannot be expressed as one. The
  feature file makes the consequence concrete: `-12.345` breaks two rules at once and must be
  refused as *"an expense must be more than 0 euro"*, because the sign is checked first (the
  feature file states this in a comment above that section). If the boundary converted to `Money`
  on the way in, the cent rule would fire during construction and the user would be told the wrong
  thing. **Conversion to `Money` therefore happens only after validation passes.**
- **The cent rule still needs one explicit check** — at that conversion, because the user has to be
  told *which* rule refused the input, and a type that simply cannot hold the value cannot produce
  a message. What the type buys is not the absence of the check but its **uniqueness**: that is the
  only place it can ever be needed, nothing in the interior can construct or compute a sub-cent
  amount, and the invariant therefore holds for computed figures as well as entered ones.
- **Two conversions exist at every boundary** — `decimal` to `Money` on the way in, `Money` to
  `decimal` for display and formatting. Reqnroll step definitions sit on that boundary too, since
  scenario arguments arrive as `400.00`. This is a real cost of the choice and is accepted; it is
  bounded, because the boundary is where all of it lives.
- **`Money` offers addition, subtraction and negation, and deliberately no division or
  multiplication by a fraction.** Those are the operations that would produce a value the cent rule
  cannot represent, and §8.2's list of things that would reopen the no-rounding rule — splitting a
  leftover, percentages, interest — is exactly the list of features that would need them. Leaving
  them off means such a feature cannot be built without coming back to §8.2 first, which is the
  intended outcome.
- **Overflow is not a practical concern but is not impossible.** A `long` of cents reaches about
  ±92 quadrillion euro. C# wraps on `long` overflow silently rather than throwing, where `decimal`
  would throw — so the failure mode is worse in theory and unreachable in practice for a personal
  budget. Recorded rather than defended.
- **[§8.2](../arc42/08-crosscutting-concepts.md)'s "how amounts are stored" row stays open.** This
  record settles representation in code, not on disk. Storage is deferred with the rest of
  persistence in [§8.3](../arc42/08-crosscutting-concepts.md).
- **When a second currency appears, this is reopened**, and it is reopened as a whole: a currency
  field is the smallest part of that change, next to deciding what a conversion rate is, when it
  applies, and what it does to the no-rounding rule. §8.2's "when this has to be revisited" list is
  where that is noted.
- **[§11](../arc42/11-risks-and-technical-debt.md)'s money-representation row shrinks but does not
  close.** What remains of it is storage, and the standing condition that the whole-cents rule holds
  only while every amount is typed in rather than computed.
