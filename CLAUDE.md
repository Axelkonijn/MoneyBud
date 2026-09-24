# MoneyBud

A personal budgeting app — income, expenses, categories and savings goals for one person or
household. Hobby project, built as an exercise in professional practice.

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
| `docs/arc42/` | Architecture documentation, arc42 template, English. Sections filled progressively — empty sections are normal, not gaps to pad |
| `docs/decisions/` | ADRs, indexed from arc42 §9 |
| `features/` | Gherkin feature files. Conventions in `features/README.md` |
| `src/` | Application code. Empty — nothing built yet |

## Subagents

`arc42-keeper` — writes and maintains the arc42 docs and ADRs.
`scenario-writer` — writes Gherkin from agreed requirements.
`spec-reviewer` — reviews code against approved scenarios and the docs.

## Working agreements

**Rhythm.** Agree the shape of a task up front, then run it to completion. Front-load the
questions rather than interrupting mid-way.

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
