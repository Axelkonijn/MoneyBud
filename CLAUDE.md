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

UI form — console, desktop or web — is not decided yet.

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
| `features/` | Gherkin feature files. Conventions in `features/README.md` |
| `src/` | Application code. Empty — nothing built yet |

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

None yet — no solution has been scaffolded.

## Where we are

_Last updated 2026-09-24. Update this when a stage completes._

**Done:** stages 1, 2 and 3.

- Stakeholder wishes gathered over three rounds in `docs/stakeholder/`, plus a long round of
  follow-up decisions taken on 2026-09-24 and recorded straight into arc42 rather than into a new
  interview round.
- arc42 §1, §2, §3, §7, §11 and §12 filled; §8.2 partly. Deployment form decided — desktop first,
  [ADR 0002](docs/decisions/0002-desktop-application-first.md).
- `features/record-expense.feature` — 19 scenarios, **approved by Axel at the first gate**.

**Next: stage 4 — an implementation plan, which is the second approval gate.** Nothing gets built
until Axel approves it. The plan has to settle things deliberately left open:

- **The §8.2 money questions** — how amounts are represented in code and stored, the sign
  convention, whether a currency field exists. The whole-cents rule is decided; these are not.
- **Persistence (§8.3) is empty**, and §7 names it as the thing §7 cannot answer.
- **No solution has been scaffolded.** ADR 0001 fixes .NET 10 and Reqnroll; project layout, test
  project and desktop UI toolkit are all unchosen.

Only `record-expense` is specified. Recording income, creating categories, assigning from the
pool and the end-of-period sweep all still need scenarios of their own, and the sweep in
particular depends on accounts, which are not in the first increment.

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

**No open questions.** §12 ended the day with none — the seven that arose were all answered, two
of them accepted with their drawbacks written down rather than solved (the expense account
default will silently be wrong for cash; one marker covers both an overspent budget and a real
overdraft). The live risks are in [§11](docs/arc42/11-risks-and-technical-debt.md), not here.

**Watch out for:** the first version is a **demo to gather feedback on, not an MVP**. Its data is
throwaway. Do not argue for building things now on the grounds that migrating real data later would
be painful — there is no real data yet.
