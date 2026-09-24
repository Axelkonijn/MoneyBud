# 3. Context and Scope

**What belongs here:** The system boundary. Who uses MoneyBud and what other systems it exchanges
data with — bank exports, file formats, anything external. Usually a diagram plus a table of
external interfaces.

Two views are conventional: **business context** (what data flows, in domain terms) and
**technical context** (the channels and protocols it flows over). For a small system, one is often
enough.

---

## 3.1 Business Context

The boundary is currently very small, and that is a real finding rather than a missing section:
**every figure in MoneyBud is entered by hand.** There are no external systems.

| External party | Sends to MoneyBud | Receives from MoneyBud |
|---|---|---|
| The user | Income and expenses; category budgets; account balances | Remaining budget per category; where money went over a period; net worth |

That single row is the entire context. The user is simultaneously the source of all data and the
consumer of all output, which is unusual and has a consequence worth stating: MoneyBud cannot
detect that something is missing, because nothing else knows what the truth is. See
[§11](11-risks-and-technical-debt.md).

## 3.2 Deferred boundaries

These would move the system boundary if taken up. They are **deferred, not rejected**, and are
recorded here because each one changes this section rather than merely adding a feature.

| Possible interface | Status |
|---|---|
| Bank transaction import (statement download, CSV or similar) | Deferred. The stakeholder wants automatic entry eventually but has no view yet on how it would work |
| Direct bank connection | Deferred, and in tension with the local-operation constraint in [§2](02-architecture-constraints.md) |
| Sync between desktop and mobile | Wanted, but explicitly a nice-to-have. Would introduce a second instance of the system and something between them |
| Live investment valuations | Deferred. Investment values are entered by hand; the stakeholder noted he might later look at whether realistic integration is possible |

## 3.3 Technical Context

MoneyBud is a single desktop application running on the user's own machine
([ADR 0002](../decisions/0002-desktop-application-first.md)). Combined with §3.1 — no external
systems at all — this makes the technical context almost empty, and the emptiness is the finding:

| Channel | How it works |
|---|---|
| The user ↔ MoneyBud | Directly, through the application's own interface, on the machine it is installed on. No network is involved at any point |
| MoneyBud ↔ its own data | Local storage on that same machine. What form that takes is undecided, and **there is no such channel at all yet** — nothing is stored in any increment built so far, see [§8.3](08-crosscutting-concepts.md) |

**There are no protocols, ports, APIs or interchange formats to document**, because there is nobody
on the other end of them. Nothing listens, nothing dials out, and MoneyBud does not need the machine
to be online to work.

The first external channel would arrive with bank import (§3.2 above). That is when this section
gains a file format or a protocol, and not before — nothing here should be designed in advance of
it.

The desktop UI framework is deliberately not named here; ADR 0002 decides the deployment form, not
the toolkit.
