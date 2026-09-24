# 0002 — The first version is a desktop application

**Status:** Accepted
**Date:** 2026-09-24

## Context

[ADR 0001](0001-dotnet-and-reqnroll.md) settled the platform but explicitly left the UI shape open
— console, desktop, or ASP.NET. That gap has been blocking arc42 [§3.3](../arc42/03-context-and-scope.md)
and [§7](../arc42/07-deployment-view.md), neither of which can be written without knowing what
MoneyBud runs as.

Two things constrain the answer. Local operation is fixed: the stakeholder chose it over a web
application even after being told that a web app would cover desktop and mobile more easily
([§2](../arc42/02-architecture-constraints.md), quality goal 4). And both desktop and mobile are
wanted, with sync between them a stated nice-to-have.

There is a genuine tension here, and it should be recorded rather than smoothed over. **Expenses
are entered while out of the house** — at the till, on the way home — so mobile is where entry
actually happens. Quality goal 2, effortless entry, is the goal most directly served by being on
the device that is in the user's pocket, and it is the goal whose failure would make the app get
abandoned. On that argument alone, mobile-first would win.

## Decision

The first version of MoneyBud is a **desktop application**: a single application running locally on
the user's own machine, with its data on that same machine.

Mobile, and sync between desktop and mobile, are **deferred, not rejected**
([§3.2](../arc42/03-context-and-scope.md)).

## Why

Desktop is the fastest route to something that can be reacted to. The development machine is the
target machine, there is no store review, no device provisioning and no deployment step between
writing code and using it, and it sits comfortably with local operation instead of fighting it.

The tension above is real, and desktop-first loses that argument in the long run. It is chosen
anyway because of what the first version is: a **throwaway demo to gather feedback on, not an MVP**
([§1.1](../arc42/01-introduction-and-goals.md)). Its data is disposable, and nobody but the
stakeholder will ever use it. The thing that decides whether the project goes anywhere is how fast
the stakeholder gets something in front of him that he can have opinions about — not whether that
first artefact matches the usage pattern the eventual product will have.

So the trade is: fit the *feedback loop* now, fit the *usage pattern* later. That is only a good
trade while the demo stays a demo. The moment the stakeholder starts keeping data he would mind
losing, the argument that justified this decision has expired and it should be reopened — which is
what supersession is for.

The alternative worth naming is a **web application**, which would serve desktop and mobile from
one codebase. It was not chosen: the stakeholder ruled it out directly, and it would require a
server to be useful, which contradicts quality goal 4.

## Consequences

- One process on one machine. No server, no network dependency, no second instance to keep in step.
  arc42 [§7](../arc42/07-deployment-view.md) stays very short, and that is the correct answer rather
  than a missing one.
- [§3.3](../arc42/03-context-and-scope.md) can now be written. It has no external interfaces to
  describe, because [§3.1](../arc42/03-context-and-scope.md) has no external systems.
- **Which desktop UI framework is not decided by this record.** It decides the deployment form, not
  the toolkit; the toolkit is a smaller and much cheaper choice and can wait.
- Persistence ([§8.3](../arc42/08-crosscutting-concepts.md)) and installation and distribution
  remain undecided.
- Scenarios stay declarative, as `features/README.md` already requires. That is what lets the
  feature files survive being pointed at a mobile UI later.
- The residual risk — a desktop-shaped answer to a mobile-shaped problem — is carried in
  [§11](../arc42/11-risks-and-technical-debt.md) rather than being treated as closed.
