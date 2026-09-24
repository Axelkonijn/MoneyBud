# Architecture Documentation — arc42

MoneyBud's architecture documentation, following the [arc42](https://arc42.org) template.

## How to read this

Sections are filled in **progressively**, not all at once. An empty section is normal and means
"we haven't needed to decide this yet" — it is not a gap to be padded out. arc42 is explicit about
this: write what carries information, leave the rest.

Each file states what belongs in it, so it's clear what's missing versus what's deliberately
still open.

## Sections

| # | Section | Purpose | Status |
|---|---|---|---|
| [01](01-introduction-and-goals.md) | Introduction and Goals | What MoneyBud does, for whom, and the top quality goals | **Filled** |
| [02](02-architecture-constraints.md) | Architecture Constraints | What we are not free to choose | **Filled** |
| [03](03-context-and-scope.md) | Context and Scope | System boundary; who and what it talks to | **Filled** |
| [04](04-solution-strategy.md) | Solution Strategy | The handful of decisions that shape everything else | **Filled** — the shaping decisions, and an honest account of which quality goals the first increment actually serves (two of four) |
| [05](05-building-block-view.md) | Building Block View | Static structure — the boxes and what's inside them | **Filled** — one library and one test project; short on purpose |
| [06](06-runtime-view.md) | Runtime View | How the blocks interact for key scenarios | Empty — with one library there is no collaboration between blocks to describe |
| [07](07-deployment-view.md) | Deployment View | Where it runs | **Filled** — desktop; persistence and distribution still open |
| [08](08-crosscutting-concepts.md) | Cross-cutting Concepts | Rules applying everywhere — money handling lives here | **Filled** — §8.1 maps §12's two distinctions onto the code and lists what has no code yet; §8.2 settles whole cents, the `Money` type, the sign convention and no currency field, with storage and timezones still open; §8.3 defers persistence, with the trigger stated |
| [09](09-architecture-decisions.md) | Architecture Decisions | Index of ADRs | 4 decisions |
| [10](10-quality-requirements.md) | Quality Requirements | Quality tree and concrete scenarios | Empty — goals 1 and 2 are about what the user sees, and there is no UI to measure |
| [11](11-risks-and-technical-debt.md) | Risks and Technical Debt | Known problems, named honestly | **Filled** |
| [12](12-glossary.md) | Glossary | Domain terms, defined once | **Filled, no open questions** — account-backed categories, the pool account, *Accumulated*, the end-of-period sweep, the expense account default, overdrawing, negative assignment against a *Budget* that floors at zero — including that an over-large one is clipped and the shortfall reported — and what a start day means in a month too short to contain it are all settled |

## Where this connects

Section 10's quality scenarios and the [Gherkin features](../../features/) describe different
things and should not be confused. Gherkin covers **functional** behaviour — what the user can do.
Section 10 covers **quality** attributes — how well, how fast, how safely.
