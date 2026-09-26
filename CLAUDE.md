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
The toolkit is **Avalonia 12** ([ADR 0005](docs/decisions/0005-avalonia-ui-toolkit.md)), over a
**toolkit-free presentation layer** that decides everything the screen shows
([ADR 0006](docs/decisions/0006-three-source-projects.md)).

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
| `src/` | `MoneyBud.Domain` — the rules. `MoneyBud.Presentation` — everything the screen decides, with no UI toolkit, and all the Dutch text (`Tekst`). `MoneyBud.Desktop` — the Avalonia window and the ring's drawing, deliberately thin and untested by plan ([ADR 0006](docs/decisions/0006-three-source-projects.md)) |
| `tests/` | `MoneyBud.Specs` — Reqnroll step definitions, plus developer unit tests under `Unit/`. Every `When` acts through `MoneyBudApp`, not the ledger |

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
dotnet test  MoneyBud.slnx     # 574 passing: 324 scenario cases, 250 developer unit tests
dotnet run --project src/MoneyBud.Desktop    # the app itself; every start is a first start
```

The solution file is `MoneyBud.slnx`, not `.sln` — the .NET 10 SDK's default format.

## Where we are

_Last updated 2026-09-26, after the UI increment shipped green and was merged into `main`. Update this when a stage completes._

**Done: all five stages, five times — for `record-expense`, `record-income`, categories,
assigning and the desktop UI.** All five are built and green.

- Stakeholder wishes gathered over three rounds in `docs/stakeholder/`, plus a long round of
  follow-up decisions taken on 2026-09-24 and recorded straight into arc42 rather than into a new
  interview round.
- arc42 §1–§5, §7, §8, §9, §11 and §12 filled. §6 and §10 are empty *with their reasons written
  down* — there is one building block that does anything, and nothing user-facing to measure.
- `features/record-expense.feature` — 21 scenarios, **approved at the first gate**. Two were added
  later, when label trimming was settled during the income increment.
- `features/record-income.feature` — 16 scenarios, **approved at the first gate** on 2026-09-25.
- `features/add-category.feature` and `features/archive-category.feature`, plus five scenarios added
  to `record-expense.feature` — **approved at the first gate** on 2026-09-25.
- `features/assign-to-category.feature` — 32 scenarios (65 cases), plus one edited scenario in
  `record-income.feature` — **approved at the first gate** on 2026-09-25.
- Increment 1's plan, approved at the second gate on 2026-09-24, settled the money questions
  ([ADR 0003](docs/decisions/0003-money-representation.md)), deferred persistence with a stated
  trigger ([§8.3](docs/arc42/08-crosscutting-concepts.md)), and fixed the solution shape
  ([ADR 0004](docs/decisions/0004-solution-layout.md)). Increment 2's needed **no new ADR** —
  nothing in it was architectural ([§9](docs/arc42/09-architecture-decisions.md)), and neither did
  increment 3's or increment 4's. Axel **waived the plan gate** for increment 3 and said to go
  straight to code; increment 4's plan was **approved at the second gate**.
- All four were reviewed by `spec-reviewer` and documented back into arc42.
- The UI's six feature files — `overview`, `step-between-periods`, `show-categories-in-a-period`,
  `list-transactions-in-a-period`, `suggest-categories` (approved 2026-09-25) and `type-an-amount`
  (approved 2026-09-26, added after review) — and its plan, approved at the second gate, which
  brought ADRs 0005 and 0006.

**What exists in code:** a desktop app (below) over `Money` (whole cents in a `long`), `Category` and `CategoryName` (the
name rule), `AddCategoryResult`, `BudgetPeriod`, `BudgetPeriodCalendar`, `Ledger` (which also
archives and assigns, and whose `StartNew` seeds the default categories), `AssignResult` and
`AssignRefusal`, and a matching pair per transaction — `Expense`, `ExpenseRefusal`,
`RecordExpenseResult` and `Income`, `IncomeRefusal`, `RecordIncomeResult`.
In `MoneyBud.Presentation`: `MoneyBudApp` (the screen), `PeriodOverview` and `Ring`, the entry
forms, `AmountInput` and `Tekst`. No storage and no accounts — both deliberate, with their
reasoning recorded.

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

**Increment 3 — adding and archiving a category — is done and green**, on branch
`increment-3-categories`. Settled with Axel on 2026-09-25; `spec-reviewer` found no faked scenario
and one low defect, fixed. Everything is in [§12](docs/arc42/12-glossary.md); in outline:

- **Removing a category takes it out of new entry; its history stays.** The state is called
  **Archived** — not *deleted*, because nothing is. Archiving **never asks for confirmation** and
  **says afterwards** that it was archived. An archived category is **shown in every period where
  it has history** — a budget of more than zero, or an expense — **the current period included**,
  and nowhere else. A zero budget with nothing spent is not history.
- **Two routes back, and still no un-archive act.** Adding an archived category's name brings it
  back, history and all. So does **recording an expense against it**: the expense is recorded,
  the category comes back, and MoneyBud says so. That was **decided**, overturning an earlier
  derivation that assumed recording could add a category — it cannot; an unknown name is refused.
  An expense refused for another reason brings nothing back.
- **Category names: trim the ends, count a run of inner whitespace as one, ignore case** — for
  every comparison, adding and recording alike, and ordinally so the machine's culture cannot
  change the answer. Stored trimmed, otherwise as typed. A name that trims to nothing is refused.
- **Adding a name you already have** returns that category, **spelled as it already was**, and says
  it was already there. Taking the new spelling would be a rename by the back door.
- **Archiving a name you don't have, or archiving twice, are non-cases** — Axel's ruling. The code
  throws for both, as caller mistakes rather than user situations.
- **Renaming is not in this increment** — deferred, not rejected.
- **The default categories are Boodschappen, Huur, Hobby, Sparen, Verzekeringen, Abonnementen** —
  Axel's own list, "enough to get an idea and test". **Dutch** where the feature files use English
  names: those are synthetic test data, these are user-facing content. **`Sparen` is unbacked for
  now** and becomes account-backed when the location dimension arrives — not an oversight. The
  scenarios start from an **empty** ledger unless they are about the first start, which is what
  proves no other scenario depends on the defaults.

**Increment 4 — assigning to a category — is done and green**, built on branch
`increment-4-assigning` and merged into `main`. Settled with Axel on 2026-09-25; `spec-reviewer` found no faked scenario
and one low defect, fixed. All in [§12](docs/arc42/12-glossary.md); in outline:

- **Assigning moves an amount; it does not set a figure.** Out of the period's *Unassigned*, onto
  the category's *Budget*. A **negative** amount moves it back; the *Budget* floors at zero, so an
  over-large one is **clipped and the shortfall reported**. Going *Over-assigned* is allowed.
- **Only the current period and later ones can be assigned in.** A past period is **refused** —
  "past is past": a forgotten plan stays unfixed, and its over-budget figure is true. Accepted
  with that consequence shown.
- **Assigning zero is accepted and changes nothing** — deliberately unlike a zero expense or
  income, which would record an event that never happened.
- **Only a positive assignment brings an archived category back.** A negative one is tidying up
  after putting it away (how an archived category's leftover budget gets back to *Unassigned*);
  zero plans nothing.
- **Refusals are about the target, reported in expense order:** blank name, unknown name, finer
  than a cent, past period. Zero and negatives are never refused *for their amount*, but a wrong
  target still refuses them — Axel: zero "would still be canceled because of the other problems".
- **`SetBudget` is gone.** A budget is made only by `Ledger.Assign`. The specs make a past
  period's budget by moving the test clock into that period and assigning — no test-only door.

**Increment 5 — the desktop UI — is done and green**, built on branch `increment-5-ui` and merged into `main`.
Settled with Axel on 2026-09-25 and 2026-09-26; `spec-reviewer` found no faked scenario, one
medium defect ("2.000" was recorded as €2,00 — now refused) and one low (accented names sorted
after Z), both fixed. All in [§12](docs/arc42/12-glossary.md), *The user interface*; in outline:

- **It covers what the domain does, and nothing more** — so **no editing or deleting** an entry,
  accepted for the demo. **Everything is Dutch**, in §12's *Dutch display terms*, which a unit
  test reads from the markdown and holds `Tekst` to.
- **One window, Axel's layout:** income left, the plan in the middle, expenses right. The start
  screen is the **Overview**, headed by the **ring**: a slice per category sized to its *Budget*
  and filled as far as spent, *Unassigned* as the last slice clockwise, an overspent slice kept
  budget-sized and **marked**, over-assigned drawn as budgets only. **Over budget and
  over-assigned now carry one marker** beside the negative figure — a **revision** of "unremarked";
  how an overdraft is shown is open again, until accounts exist.
- **Order:** rows and slices largest *Budget* first, ties in order added; transactions newest
  first, in **two** lists; suggestions alphabetical, narrowing on "contains".
- **Periods:** step back and forward, **no** jump to today. The screen holds its period as a
  period, so at midnight it **stays** and just loses its *Huidige periode* label. A date left
  empty is **today**; an assignment defaults to the **period on screen**. An entry landing
  elsewhere leaves the screen put and **says where it went**.
- **The category box is free text with suggestions**, so all three routes back stay reachable.
- **Typing an amount:** comma or point is the decimal mark, no thousands separator; "2.000"
  (three digits ending in 0) is refused as **ambiguous**; four-plus decimals that are whole cents,
  and ",50", are not amounts. The cent rule stays the domain's.
- **Start day fixed at the 1st**: the ledger can't change it once budgets exist. Deferred.
- **Starts with the six defaults, keeps nothing** — so §8.3's trigger for storage is now
  *reachable*: the first time Axel minds re-entering data, storage is due.

**Watch out for:** `Ledger.HasBudget` can tell "never assigned" from "assigned, then taken back
to zero", although §12 says there is no separate "unbudgeted" state. Only test code uses it — two
setup steps, the first-start check and one unit test — and the ring deliberately does not. If a
screen ever needs it, revisit §12 first ([§8.1](docs/arc42/08-crosscutting-concepts.md)).

**Watch out for:** `MoneyBud.Desktop` must **decide nothing**. Narrowing the suggestions was once
the toolkit's own filter and had to be moved into Presentation. Anything the window chooses is
untested by plan, so a choice belongs in `MoneyBud.Presentation`, with a test.

**Settled ahead of later increments** (in §12; not built): **when a period opens**, an archived
category's last figure is **not offered back**.

**Next, in order** — the pipeline restarts at stage 1 for each; nothing skips ahead to code:

- **Demo it to Axel and gather feedback** — the point of ADR 0002. That feedback may reorder
  everything below.
- **Opening a period** — offering last period's figures back and the one action that assigns them
  in full. Settled in §12, not built.
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

**Two open questions, and neither can be answered yet.** The first: a budget period never closes, so an income
can be back-dated into a period whose *Unassigned* was already swept. §12 covers the analogous
case for a late *expense*, but the principle does not necessarily extend — a late expense means
MoneyBud moved too much, a late income means there was more to move, and those disagree about
which period's figures change. There is no sweep and no accounts, so there is nothing to decide
against; it goes live when the sweep is built. The second: **how an overdrawn account is shown**,
now that over budget and over-assigned carry a marker — reopened by the UI increment's marker
revision, and met when accounts are built. Both recorded in [§12](docs/arc42/12-glossary.md).

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
  20. Built in increment 4.
- **Euro-only is a decision, not an assumption.** §2 is hardened accordingly, which is what
  [ADR 0003](docs/decisions/0003-money-representation.md)'s no-currency-field argument rests on.

Earlier, seven questions were settled while the model was being agreed, two of them accepted with
their drawbacks written down rather than solved (the expense account default will silently be
wrong for cash; one display covers both an overspent budget and a real overdraft — the second now
reopened, as above).

The live risks are in [§11](docs/arc42/11-risks-and-technical-debt.md), not here.

**Watch out for:** the first version is a **demo to gather feedback on, not an MVP**. Its data is
throwaway. Do not argue for building things now on the grounds that migrating real data later would
be painful — there is no real data yet.
