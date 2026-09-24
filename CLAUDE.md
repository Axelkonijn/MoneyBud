# MoneyBud

A personal budgeting app — income, expenses, categories and savings goals for one person or
household.

A **hobby project**. It follows the professional way of working Axel is learning on a course, but
it is not a course assignment: there is no assessor, no rubric and no deadline. The practices are
here because they are useful, not because they are being marked.

Public repo: https://github.com/Axelkonijn/MoneyBud

## Stack

**.NET 10 / C#**, with **Reqnroll** for Gherkin scenarios. See
[ADR 0001](docs/decisions/0001-dotnet-and-reqnroll.md).

Desktop application, running locally — [ADR 0002](docs/decisions/0002-desktop-application-first.md).
**Which desktop UI toolkit is still open**, and nothing has been built against one: the first
increment is a domain library and its scenarios, with no UI at all.

## Way of working

An agentic workflow with human approval gates. Work moves through these stages in order, and
**does not skip ahead**:

| # | Stage | Who | Gate |
|---|---|---|---|
| 1 | Stakeholder wishes — what Axel actually wants, captured clearly | Conversation with Axel, not delegated | — |
| 2 | arc42 documentation updated from those wishes | `arc42-keeper` | — |
| 3 | Gherkin scenarios derived from the documented requirements | `scenario-writer` | **Axel approves** |
| 4 | Implementation plan | main agent | **Axel approves** |
| 5 | Build to green, then review | main agent, then `spec-reviewer` | — |

Stage 1 has to be a conversation — subagents report back to the main agent and cannot talk to Axel
directly.

The two gates are what make the rest of it safe to run unsupervised: once scenarios and a plan are
approved, implementation proceeds to passing tests without further check-ins. This puts the weight
on stages 3 and 4 being genuinely right, so raise ambiguities there rather than resolving them
quietly.

## Repository layout

| Path | Contents |
|---|---|
| `docs/stakeholder/` | **Read these first.** Stakeholder interviews, in Dutch, verbatim after cleanup. Source material — never rewritten. New wishes go in a new round, not by editing old ones |
| `docs/arc42/` | Architecture documentation, arc42 template, English. Sections filled progressively — empty sections are normal, not gaps to pad |
| `docs/decisions/` | ADRs, indexed from arc42 §9 |
| `features/` | Gherkin feature files. Conventions in `features/README.md`. They stay here and are *linked* into the test project, not copied — [ADR 0004](docs/decisions/0004-solution-layout.md) |
| `src/` | Application code. `MoneyBud.Domain` — the whole application for now |
| `tests/` | `MoneyBud.Specs` — Reqnroll step definitions, plus developer unit tests under `Unit/` |

The stakeholder material is Dutch and the documentation is English. `docs/arc42/12-glossary.md`
holds the agreed translation of the domain terms — use it rather than translating afresh.

## Subagents

`arc42-keeper` — writes and maintains the arc42 docs and ADRs.
`scenario-writer` — writes Gherkin from agreed requirements.
`spec-reviewer` — reviews code against approved scenarios and the docs.

## Working agreements

**Rhythm.** Agree the shape of a task up front, then run it to completion. Front-load the
questions you can foresee.

**Ask the moment a question arises.** A question that appears mid-task gets put to Axel straight
away, not recorded as an "open question" and reported at the end. Unanswered questions compound:
every decision taken around a gap risks being a decision that has to be rewritten once the gap is
filled. Batch questions that arise together; never hold one over to a later turn.

**Scope.** Take the literal ask, not the larger project implied behind it. When a request is
ambiguous about size, assume the smaller reading and confirm.

**Decisions get their reasoning written down** — why, not just what. The reasoning is the part
nobody can reconstruct later.

**Money handling is decided before it is coded.** See
[arc42 §8.2](docs/arc42/08-crosscutting-concepts.md). Never `double` or `float` for monetary
amounts.

**This repo is public.** All example, seed and test data is synthetic. Real financial data,
account numbers and statements never enter the repository.

**No license** by choice — all rights reserved for now. Don't add one unprompted.

## Commands

```
dotnet build MoneyBud.slnx     # expect 0 warnings — the suite is kept warning-free
dotnet test  MoneyBud.slnx     # 190 passing: 34 scenario cases, the rest developer unit tests
```

The solution file is `MoneyBud.slnx`, not `.sln` — the .NET 10 SDK's default format.

## Where we are

_Last updated 2026-09-24, after the first increment shipped green. Update this when a stage completes._

**Done: all five stages, for `record-expense` only.** The first increment is built and green.

- Stakeholder wishes gathered over three rounds in `docs/stakeholder/`, plus a long round of
  follow-up decisions taken on 2026-09-24 and recorded straight into arc42 rather than into a new
  interview round.
- arc42 §1–§5, §7, §8, §9, §11 and §12 filled. §6 and §10 are empty *with their reasons written
  down* — there is one building block that does anything, and nothing user-facing to measure.
- `features/record-expense.feature` — 19 scenarios, **approved by Axel at the first gate**.
- Implementation plan **approved by Axel at the second gate** on 2026-09-24. It settled the money
  questions ([ADR 0003](docs/decisions/0003-money-representation.md)), deferred persistence with a
  stated trigger ([§8.3](docs/arc42/08-crosscutting-concepts.md)), and fixed the solution shape
  ([ADR 0004](docs/decisions/0004-solution-layout.md)).
- Built, reviewed by `spec-reviewer`, and documented back into arc42 §8.1, §5 and §4.

**What exists in code:** `Money` (whole cents in a `long`), `Category`, `Expense`, `BudgetPeriod`,
`BudgetPeriodCalendar`, `ExpenseRefusal`, `RecordExpenseResult`, `Ledger`. No UI, no storage, no
accounts — all three deliberate, all three with their reasoning recorded.

**Next: stage 1 again, for whatever capability comes after this.** The pipeline restarts at
wishes; nothing skips ahead to code. The obvious candidates, none of them specified:

- **Recording income**, and with it *Unassigned*, *Assign* and *Left to assign*. These are
  specifiable today — they need no accounts — and they are the smallest step towards something
  Axel can actually react to.
- **Creating a category and setting a budget.** Both exist in code only as test scaffolding
  (`Ledger.AddCategory`, `Ledger.SetBudget`) with no user-facing behaviour, because neither has
  scenarios.
- **A UI**, which is what turns this into the demo ADR 0002 is about. It needs the two above
  first, or there is nothing to show.
- **Accounts, net worth and the sweep** — later increments. The sweep depends on accounts.

**The model, as Axel settled it** — all in [§12](docs/arc42/12-glossary.md), which is long but is
the thing to read. In outline:

- **Two dimensions.** Location (Account) and purpose (Category) vary independently. Net worth is
  everything by location; budget is everything by purpose.
- **Two layers.** A **Budget is a plan**, not money that has moved. Income forms a pool; every
  euro is *Unassigned* until assigned, and assigning spends nothing. Expenses are the separate
  *actual* layer. `Remaining = Budget − spent` is the one place the layers meet.
- **Account-backed categories** are the exception, many-to-many with accounts. Assigning to a
  backed category really moves money; spending from one really reduces a balance; they are never
  swept, and they show an *Accumulated* running total net of spending. Unbacked categories are
  pure plan.
- **A pool account** holds unassigned money and is the default source for every movement,
  including an ordinary expense. Overridable throughout.
- **Period boundaries.** A period opens with the previous period's figures *remembered but not
  assigned*, so the pool starts whole. It ends but never closes. At the end, *Unassigned* plus
  every unbacked category's *Leftover* is swept automatically into one chosen backed destination,
  visibly and reversibly. Nothing rolls forward.
- **MoneyBud shows, it never blocks.** Overspending a budget, overdrawing an account by assigning,
  overdrawing it by spending — all allowed, all shown, none warned about.

Also settled: expenses require a category; exactly zero is not over budget; the label is
optional; future dates are refused; amounts are whole cents and never rounded
([§8.2](docs/arc42/08-crosscutting-concepts.md)).

**No open questions.** Three arose while the first increment was being built and all three were
answered by Axel the same day:

- **A start day the month is too short for clamps to that month's last day.** So a period
  configured to start on the 31st runs 28 February to 30 March — starting on a day nobody picked
  and 31 days long. Taken with that consequence visible, because it keeps periods tiling and
  keeps "configurable" true for every day rather than true with an exception.
- **An amount may be assigned negatively; a *Budget* floors at zero.** Assigning -50 pulls money
  back to *Unassigned*, so no separate "unassign" act is needed, but a plan for less than nothing
  is not a plan. **An over-large negative assignment is clipped and the shortfall reported** —
  -50 against a *Budget* of 30 moves 30, never refuses, and never stays quiet about the other
  20. Nothing is built yet — this settles the model the assign scenarios will be written against.
- **Euro-only is a decision, not an assumption.** §2 is hardened accordingly, which is what
  [ADR 0003](docs/decisions/0003-money-representation.md)'s no-currency-field argument rests on.

Earlier, seven questions were settled while the model was being agreed, two of them accepted with
their drawbacks written down rather than solved (the expense account default will silently be
wrong for cash; one marker covers both an overspent budget and a real overdraft).

The live risks are in [§11](docs/arc42/11-risks-and-technical-debt.md), not here.

**Watch out for:** the first version is a **demo to gather feedback on, not an MVP**. Its data is
throwaway. Do not argue for building things now on the grounds that migrating real data later would
be painful — there is no real data yet.
