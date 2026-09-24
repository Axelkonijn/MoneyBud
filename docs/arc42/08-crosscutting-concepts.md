# 8. Cross-cutting Concepts

**What belongs here:** Rules and patterns that apply across many building blocks — the domain
model, persistence, error handling, validation, logging, security. Anything a developer needs to
know regardless of which part of the system they're touching.

---

_Not yet filled in._

## 8.1 Domain Model

_Empty._

## 8.2 Money Handling

**This section should be written before the first line of money-handling code.** Money decisions
are quietly expensive to reverse once data exists, and a budgeting app that gets them wrong is
wrong in ways that are hard to notice. At minimum it needs to settle:

- How amounts are represented and stored — C#'s `decimal` avoids the binary floating-point errors
  that make `double` unsuitable for money, but the storage format still needs deciding.
- Rounding rule, applied in exactly one place rather than scattered.
- Currency: single-currency for v1, but whether the model carries a currency field anyway.
- Sign convention for inflow versus outflow, chosen once.
- Period boundaries: when a "month" starts, and how timezones are handled.

_Empty._

## 8.3 Persistence

_Empty._
