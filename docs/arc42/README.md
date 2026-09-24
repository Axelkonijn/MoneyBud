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
| [01](01-introduction-and-goals.md) | Introduction and Goals | What MoneyBud does, for whom, and the top quality goals | **Filled** — including what "the first version" covers and why it is not the same span as "the first increment" |
| [02](02-architecture-constraints.md) | Architecture Constraints | What we are not free to choose | **Filled** |
| [03](03-context-and-scope.md) | Context and Scope | System boundary; who and what it talks to | **Filled** |
| [04](04-solution-strategy.md) | Solution Strategy | The handful of decisions that shape everything else | **Filled** — the shaping decisions, and an honest account of which quality goals the code actually serves (two of four, unchanged by the income increment) |
| [05](05-building-block-view.md) | Building Block View | Static structure — the boxes and what's inside them | **Filled** — one library and one test project; short on purpose, and the income increment moved no boundary |
| [06](06-runtime-view.md) | Runtime View | How the blocks interact for key scenarios | Empty — still one building block that does anything, so there is no collaboration between blocks to describe |
| [07](07-deployment-view.md) | Deployment View | Where it runs | **Filled** — desktop; persistence and distribution still open |
| [08](08-crosscutting-concepts.md) | Cross-cutting Concepts | Rules applying everywhere — money handling lives here | **Filled** — §8.1 maps §12's two distinctions onto the code, places income's pool outside both of them, and lists what has no code yet (*Assign* and *Over-assigned* lead that list, joined now by *Archived* and the default categories; income and *Unassigned* have left it, built). §8.1 also carries **one live divergence between the code and §12**: category names are compared case-sensitively in `Ledger` — in two places — by accident rather than by decision, where §12 now decides case-insensitively. §8.2 settles whole cents, the `Money` type, the sign convention and no currency field — unchanged by income, which those rules already covered — with storage and timezones still open; §8.3 defers persistence, with the trigger stated |
| [09](09-architecture-decisions.md) | Architecture Decisions | Index of ADRs | 4 decisions — the income increment added none, and §9 says why that is the expected outcome |
| [10](10-quality-requirements.md) | Quality Requirements | Quality tree and concrete scenarios | Empty — goals 1 and 2 are about what the user sees, and there is no UI to measure |
| [11](11-risks-and-technical-debt.md) | Risks and Technical Debt | Known problems, named honestly | **Filled** |
| [12](12-glossary.md) | Glossary | Domain terms, defined once | **Filled, one open question** — what happens to an income back-dated into a period that has **already been swept**, which the late-expense principle nearly but does not quite answer, and which cannot be decided until a sweep exists to decide against. Newly settled is the **category model**: names compared case-insensitively and stored as typed (a correction to code that was case-sensitive by accident), adding an existing name hands back the existing category and says so, removing a category **archives** it — out of new entry, history intact, still shown in past periods — adding an archived category's name **brings it back** and says so, which is also why a late expense against an archived category needs no rule of its own and why there is no separate un-archive act; renaming deferred; and six Dutch **default categories** whose names are user-facing content rather than project vocabulary. Everything else holds as before — account-backed categories, the pool account, *Accumulated*, the end-of-period sweep, the expense account default, overdrawing, negative assignment against a *Budget* that floors at zero — including that an over-large one is clipped and the shortfall reported — and what a start day means in a month too short to contain it are all settled. Now also the **income model**: a required label where an expense's is optional, future-dating allowed where an expense's is not, *Unassigned* and *Left to assign* merged into one figure named *Unassigned*, *Over-assigned* named for its negative state, and net worth settled as a point-in-time figure that excludes expected income — so the two views disagree about it by design. All of that income model is now built except *Over-assigned*, which needs assigning to be reachable at all ([§8.1](08-crosscutting-concepts.md)) |

## Where this connects

Section 10's quality scenarios and the [Gherkin features](../../features/) describe different
things and should not be confused. Gherkin covers **functional** behaviour — what the user can do.
Section 10 covers **quality** attributes — how well, how fast, how safely.
