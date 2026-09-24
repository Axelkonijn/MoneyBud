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
dotnet test  MoneyBud.slnx     # 234 passing: 68 scenario cases, the rest developer unit tests
```

The solution file is `MoneyBud.slnx`, not `.sln` — the .NET 10 SDK's default format.

## Where we are

_Last updated 2026-09-25, after the income increment shipped green. Update this when a stage completes._

**Done: all five stages, twice — for `record-expense` and for `record-income`.** Both are built
and green.

- Stakeholder wishes gathered over three rounds in `docs/stakeholder/`, plus a long round of
  follow-up decisions taken on 2026-09-24 and recorded straight into arc42 rather than into a new
  interview round.
- arc42 §1–§5, §7, §8, §9, §11 and §12 filled. §6 and §10 are empty *with their reasons written
  down* — there is one building block that does anything, and nothing user-facing to measure.
- `features/record-expense.feature` — 21 scenarios, **approved at the first gate**. Two were added
  later, when label trimming was settled during the income increment.
- `features/record-income.feature` — 16 scenarios, **approved at the first gate** on 2026-09-25.
- Increment 1's plan, approved at the second gate on 2026-09-24, settled the money questions
  ([ADR 0003](docs/decisions/0003-money-representation.md)), deferred persistence with a stated
  trigger ([§8.3](docs/arc42/08-crosscutting-concepts.md)), and fixed the solution shape
  ([ADR 0004](docs/decisions/0004-solution-layout.md)). Increment 2's needed **no new ADR** —
  nothing in it was architectural ([§9](docs/arc42/09-architecture-decisions.md)).
- Both were reviewed by `spec-reviewer` and documented back into arc42.

**What exists in code:** `Money` (whole cents in a `long`), `Category`, `BudgetPeriod`,
`BudgetPeriodCalendar`, `Ledger`, and a matching pair per transaction — `Expense`,
`ExpenseRefusal`, `RecordExpenseResult` and `Income`, `IncomeRefusal`, `RecordIncomeResult`.
No UI, no storage, no accounts — all three deliberate, all three with their reasoning recorded.

**Increment 2 — recording income — is done and green.** All five stages, settled with Axel on
2026-09-24 and 2026-09-25. `features/record-income.feature` holds 16 scenarios, approved at the
first gate; the plan was approved at the second; `spec-reviewer` found no defect and no faked
scenario. What was settled along the way:

- An income has an amount, a date and a **required** label — an expense's label stays optional,
  because an expense's *category* already says what it is and an income has nothing else.
- **Future dates are allowed**, where an expense's are refused. Expenses already have a
  forward-looking layer — the *Budget* — so a planned expense duplicates a concept that exists,
  while there is no planned income and so nothing to budget a period with.
- ***Unassigned* and *Left to assign* were one figure under two names.** Merged, named
  *Unassigned*; **`Left to assign` is a retired term**. Its negative state is *Over-assigned*.
- **Net worth is what you have today** and excludes expected income. *Unassigned* is a period
  figure, net worth is a point-in-time figure, and they differ about future-dated income by design.

It also settled two rules that reach back into `record-expense`: **every label is trimmed** at the
ends and left alone inside, and an expense label that trims to nothing is **no label** rather than
one made of spaces. That is why `record-expense.feature` grew two scenarios.

**Next, in order** — the pipeline restarts at stage 1 for each; nothing skips ahead to code:

- **Creating and removing a category** — the next increment. **Stage 1 is done**: Axel settled it
  on 2026-09-25 and it is recorded in [§12](docs/arc42/12-glossary.md). **Stage 3, the scenarios,
  is next and carries the first gate.** What was settled:
    - **Removing a category takes it out of new entry; its history stays.** The state is called
      **Archived** — not *deleted*, because nothing is: its expenses and its past budgets remain
      and past periods still show it. Never destroys a record, never blocks — and the interview's
      actual case, a default category that does not apply to you, has no history at all.
    - **Adding the name of an archived category brings it back, history and all**, and MoneyBud
      says it was brought back rather than created. So there is **no separate un-archive act**,
      for the same reason there is no separate unassign act — one gesture rather than a second
      named concept. This also settles the late-expense case by derivation: recording against an
      archived category means bringing it back, which is one action, not a special case.
    - **Names are compared case-insensitively and stored exactly as typed** — the same shape as
      the label rule. This is a **correction**: `Ledger` keys categories with
      `StringComparer.Ordinal` today, so names are case-sensitive by accident, not by decision.
    - **Adding a name you already have** returns the category you already have, and MoneyBud says
      so rather than silently doing nothing. Derived and put to Axel; not contradicted. Note the
      argument behind it — "the end state you wanted is already true" — **does not** reach the
      archived case, which is why that needed its own decision above.
    - **Renaming is not in this increment** — deferred, not rejected.
    - **The default categories are Boodschappen, Huur, Hobby, Sparen, Verzekeringen,
      Abonnementen** — Axel's own list, "enough to get an idea and test". They are **Dutch**
      where the feature files use English names; those are synthetic test data, these are
      user-facing content. **`Sparen` is unbacked for now** and becomes account-backed when the
      location dimension arrives — not an oversight.
  In code it is only test scaffolding: `Ledger.AddCategory` with no user-facing behaviour.
- **Assigning to a category** — the real act behind `Ledger.SetBudget`, and what makes
  *Unassigned* move. Its model is already settled in §12; it needs scenarios, not decisions.
- **A UI**, which is what turns this into the demo ADR 0002 is about. It needs the above first,
  or there is nothing to show.
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

**The two transactions deliberately differ**, and this is the easiest thing to get wrong: an
**expense** requires a category, its label is optional, and a future date is **refused**; an
**income** names no category, its label is **required**, and a future date is **allowed** and
counts towards *Unassigned* from the moment it is recorded. Common to both: every label is
trimmed at the ends and left alone inside, a label that trims to nothing is "no label", exactly
zero *Remaining* is not over budget, and amounts are whole cents and never rounded
([§8.2](docs/arc42/08-crosscutting-concepts.md)).

**One open question, and it cannot be answered yet.** A budget period never closes, so an income
can be back-dated into a period whose *Unassigned* was already swept. §12 covers the analogous
case for a late *expense*, but the principle does not necessarily extend — a late expense means
MoneyBud moved too much, a late income means there was more to move, and those disagree about
which period's figures change. There is no sweep and no accounts, so there is nothing to decide
against; it goes live when the sweep is built. Recorded in [§12](docs/arc42/12-glossary.md).

Everything else is settled. Three questions arose while the first increment was being built and
all three were answered by Axel the same day:

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
