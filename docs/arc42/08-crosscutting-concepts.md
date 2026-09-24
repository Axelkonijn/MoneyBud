# 8. Cross-cutting Concepts

**What belongs here:** Rules and patterns that apply across many building blocks — the domain
model, persistence, error handling, validation, logging, security. Anything a developer needs to
know regardless of which part of the system they're touching.

---

_Only §8.2 is filled in, and only partly. §8.1 and §8.3 are empty._

## 8.1 Domain Model

_Empty._

## 8.2 Money Handling

**This section should be written before the first line of money-handling code.** Money decisions
are quietly expensive to reverse once data exists, and a budgeting app that gets them wrong is
wrong in ways that are hard to notice. One rule is settled; the rest of the section is still open,
and is listed below rather than guessed at.

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

### Still open

None of the following has been decided. They are listed so that it is clear they were considered
and left open, not overlooked.

| Question | Note |
|---|---|
| How amounts are represented in code | C#'s `decimal` avoids the binary floating-point errors that make `double` unsuitable for money, but a whole-cents integer is also a candidate precisely because the rule above makes it sufficient. Not chosen |
| How amounts are stored | Depends on the persistence choice in §8.3, which is itself empty |
| Sign convention for inflow versus outflow | Chosen once, or not at all — but not yet |
| Whether the model carries a currency field | Single-currency (euro) for v1 per stakeholder round 3; whether the field exists anyway is a separate question |
| Period boundaries and timezones | The start day of a budget period is configurable (see [§12](12-glossary.md)); how that interacts with timezones is undecided |

## 8.3 Persistence

_Empty._
