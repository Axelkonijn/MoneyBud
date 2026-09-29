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
| `docs/stakeholder/` | **Read these first.** Stakeholder interviews, in Dutch, verbatim after cleanup. Source material — never rewritten. New wishes go in a new round, not by editing old ones. Feedback given in English is translated, and the round says so at the top |
| `docs/arc42/` | Architecture documentation, arc42 template, English. Sections filled progressively — empty sections are normal, not gaps to pad |
| `docs/decisions/` | ADRs, indexed from arc42 §9 |
| `docs/plans/` | An increment's implementation plan, when it is too long to keep here. Written at stage 4, approved at the second gate |
| `features/` | Gherkin feature files. Conventions in `features/README.md`. They stay here and are *linked* into the test project, not copied — [ADR 0004](docs/decisions/0004-solution-layout.md) |
| `src/` | `MoneyBud.Domain` — the rules. `MoneyBud.Presentation` — everything the screen decides, with no UI toolkit, and all the Dutch text (`Tekst`). `MoneyBud.Storage` — the data file: its JSON form, the lock, the atomic save ([ADR 0007](docs/decisions/0007-keeping-the-ledger.md)). `MoneyBud.Desktop` — the Avalonia window and the ring's drawing, deliberately thin and untested by plan ([ADR 0006](docs/decisions/0006-three-source-projects.md)) |
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
dotnet test  MoneyBud.slnx     # 1868 passing: 1001 scenario cases, 867 developer unit tests
dotnet run --project src/MoneyBud.Desktop    # the app itself; keeps its data in %LOCALAPPDATA%\MoneyBud
```

The solution file is `MoneyBud.slnx`, not `.sln` — the .NET 10 SDK's default format.

## Where we are

_Last updated 2026-09-29, after the start day was tried by Axel and merged. **Start here in a new conversation: the mobile front-end** (item 3 under *Next, in order*), at stage 1 — a conversation with Axel, whose big question is where the data lives with two devices. Update this when a stage completes._

**Done: all five stages, thirteen times — for `record-expense`, `record-income`, categories,
assigning, the desktop UI, correcting things, keeping data, opening a period, accounts, backing,
the sweep, recurring entries, and the period start day.** All thirteen are built and green, tried by Axel and merged into `main`.

- Stakeholder wishes gathered over three rounds in `docs/stakeholder/`, plus a long round of
  follow-up decisions taken on 2026-09-24 and recorded straight into arc42 rather than into a new
  interview round.
- arc42 §1–§9, §11 and §12 filled; §6 since the persistence increment, which gave start-up, saving
  and closing a runtime worth drawing. §10 is empty *with its reason written down* — no measure has
  been agreed.
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
`RecordExpenseResult` and `Income`, `IncomeRefusal`, `RecordIncomeResult`. Since increment 6:
entries carry a ledger-issued `Id`, `Category` is a class with identity, and there are
`ChangeExpenseResult`, `ChangeIncomeResult` (with `ChangeOutcome`) and `RenameCategoryResult`
(with `RenameOutcome`, `RenameRefusal`). Since increment 7: `LedgerSnapshot`, `Ledger.ToSnapshot` and
`FromSnapshot`, and the `ILedgerStore` port (with `Claim` and `LoadResult`). Since increment 8:
`Ledger.PlanOfferedIn` → `PlanOffer` (with `PlanFigure`) and `Ledger.TakeOverPlan` → `TakeOverPlanResult`.
In `MoneyBud.Presentation`: `MoneyBudApp` (the screen, including the one `Question` and the save
line), `MoneyBudStart`, `PeriodOverview` and `Ring`, the entry forms (with their *Wijzigen* state),
`AmountInput` and `Tekst`. In `MoneyBud.Storage`: `LedgerJson` and `FileLedgerStore`. Since
increment 9: `Account`, `NameRule` (the name rule, shared; `CategoryName` forwards to it), `IEntry`,
`Transfer`, `BalanceCorrection` and the results in `AccountResults.cs`; `Ledger.BalanceOf`,
`DifferenceOf`, `NetWorth`, `HistoryOf`, `Accounts`/`PoolAccount`; `AccountLine`, `HistoryLine`,
`AccountForm` and `TransferForm` in Presentation; file format version 2 (ADR 0008). Since increment
10: `Movement` (with `MovementReason`, `MovementDirection`), `EntryMark`, `Backing`, `SetBackingResult`;
`Ledger.SetBacking`, `BackingOf`, `ThereFor`, `AccumulatedFor`, `Settle`; `BackingChoice` and the
row's `Accumulated`/`ChosenBacking` in Presentation; file format version 3 (ADR 0009). Since increment
11: `MovementReason.Swept` and `Movement.SweptFor`, `Sweep.cs` (`SetSweepDestinationResult`,
`SweepLine`, `SweptPart`, `BringUpToDateResult`, `SweepMade`); `Ledger.SetSweepDestination`,
`PeriodLeftover`, `SweepLineFor`, `BringUpToDate`, `TakeSweepsMade`, and `Settle` recording each
period's end and sweeping it; `SweepChoice` and the Overview's sweep list and line in Presentation;
file format version 4 (ADR 0010). Since increment 13: `BudgetPeriodCalendar` as a history of
`StartDayChange`s, `ChangeStartDayResult`, `Ledger.ChangeStartDay`/`PreviewStartDay`, a `Backing`'s
`AccumulatingFrom`/`HereFrom`; `MoneyBudApp`'s *Periode begint op* list (`StartDayChoice`,
`StartDayShownIn`) and `Question.ConfirmText`; file format version 7 (ADR 0012).

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
to zero", although §12 says there is no separate "unbudgeted" state. Only test code uses it — setup
steps, the first-start check and unit tests — and the ring and `CanDelete` deliberately do not. If a
screen ever needs it, revisit §12 first ([§8.1](docs/arc42/08-crosscutting-concepts.md)).

**Watch out for:** `MoneyBud.Desktop` must **decide nothing**. Narrowing the suggestions was once
the toolkit's own filter and had to be moved into Presentation. Anything the window chooses is
untested by plan, so a choice belongs in `MoneyBud.Presentation`, with a test.

**Settled ahead, and since built** (increment 8): **when a period opens**, an archived category's
last figure is **not offered back**.

**The first feedback round is done and green, built on branch `feedback-round-1` and merged into
`main`.** Axel ran the result himself on 2026-09-26: "it looks great". Axel ran the demo on 2026-09-26 in a guided session; his feedback is in
[`docs/stakeholder/2026-09-26-demo-feedback.md`](docs/stakeholder/2026-09-26-demo-feedback.md).
All five stages ran the same day: rulings in §12, scenarios **approved at the first gate**
(`overview.feature` revised, `point-at-a-slice.feature` new), the plan **approved at the second**,
and `spec-reviewer` found no faked scenario and three low defects, all fixed. What the plan
settled: the minimum is **2%** of the ring (`Ring.MinimumSweep`); smaller slices are widened and
the rest give way in proportion, so a larger slice is never narrower and *Unassigned* counts like
any other; past 50 slices all are drawn equal. A slice's details show **in the ring's hole**,
which otherwise shows *Niet toegewezen*. The field order is held by a unit test that **reads
`MainWindow.axaml`** — an approved exception to the untested Desktop. The "groc" defect did not
reproduce in a headless run (a picked suggestion does reach the form); the box now empties
either way. Three changes, all small:

- **The category box empties after every entry that goes through**, expense and assign alike;
  a refusal keeps it, like every other field.
- **The ring becomes the centrepiece** of the middle column, rows below it; **hovering a slice
  shows** everything its row does — category, Budget, Uitgegeven, Resterend, the marker,
  *Gearchiveerd* — and the *Niet toegewezen* slice shows its figure; **every slice, Unassigned
  included, gets a minimum width**. That minimum **revised an approved rule** — the ring was
  exactly proportional. **The fill stays exact**: no minimum fill.
- **Field order: the "what" before the amount.** Expense: Omschrijving → Categorie → Bedrag →
  Datum. Income: Omschrijving → Bedrag → Datum. Assigning: Categorie → Bedrag.

The field order and the emptying box are window and form behaviour: unit tests, not scenarios.
Axel's remark that the date stays on today after stepping to another month is an **observation,
not a change** — the ruling (date = today) stands, since in real use he would set the date anyway.
Correcting entries and keeping data are acknowledged as missing and **explicitly later**.

**Increment 6 — correcting things — is done and green**, built on branch `increment-6-corrections`
and merged into `main` after Axel tried it on 2026-09-26: "looks good". Stages 1–4 ran on 2026-09-26 (rulings in §12, four feature files approved at
the first gate — `change-an-entry`, `remove-an-entry`, `rename-a-category`, `delete-a-category`,
73 scenarios, 127 cases — and the plan approved at the second); built on Axel's "continue" the same
day. `spec-reviewer` found no faked scenario and one low defect (stepping wiped a *new* entry being
typed; now only an entry being changed is dropped), fixed. arc42 §4, §5, §6, §8, §9, §11, §12 and the
README updated; no ADR. In outline:

- **Domain.** Entries have a ledger-issued id; a change replaces the entry in place, so it keeps its
  place. `Category` has identity and a domain-only `Name` setter, so a rename is one assignment plus
  re-keying the name index. Recording's checks are shared with changing (`CheckExpense`,
  `CheckIncome`); **an unchanged save is recognised before the checks**, so it cannot be refused.
  Bring-back follows a category *change* only. `CanDelete` reads expenses and budgets > 0 — **not
  `HasBudget`** — and `DeleteCategory` drops zero budgets too. Unreachable misuse throws.
- **Presentation.** Rows carry their entry; clicking loads it into the form's *Wijzigen* state
  (Opslaan / Annuleren / Verwijderen). `AmountInput.Format` loads "2000,00", unit-tested to read back
  identically. Removing asks via `MoneyBudApp.Question`, shown in the message bar; **a question and
  a notice are never shown together** — anything said next drops the question. Stepping drops an
  entry being changed, a rename in progress and a waiting question, and keeps a new entry being typed.
- **Built, not put to Axel** (§12, *Chosen in the build, not put to the stakeholder*): a rename
  rewrites the category box of the expense and assign forms when it names the old name (else an
  unchanged save could fail); the archive button moved under the category name beside Hernoemen and
  Verwijderen; the question's answers are Verwijderen/Annuleren; a rename box open during the
  once-a-minute refresh loses focus (text kept) — known, not fixed.
- **Headless check** of the real window passed: row click loads, Opslaan changes, Verwijderen asks in
  the bar, the rename box writes back, delete shows only on a row with no history.

**Increment 7 — keeping data — is done and green**, built on branch `increment-7-persistence` and
merged into `main` after Axel tried it on 2026-09-26: "It seems perfect". All five stages ran on 2026-09-26. Stage 1 was a long run of
multiple-choice questions: 28 rulings, in §12 *What MoneyBud keeps* and the §8.3 table.
Three feature files — `keep-data`, `start-moneybud`, `carry-on-when-saving-fails`, 37 scenarios,
64 cases (one row added after the gate, with Axel's approval, for ruling 28) — were **approved at the first gate**, the plan at the second; ADR 0007. `spec-reviewer`
found no faked scenario and five low defects, all fixed. In outline:

- **Axel's shape:** "demo now, real soon" — still a demo, and saved data may be dropped by any
  version **up to and including accounts**, so no migration is owed before then. Everything (the
  ledger only, not screen state) kept indefinitely, **saved after every change**, no save button,
  one set of data, no reset, no backups (his job), no password. Location fixed and **only in the
  README**, not shown by MoneyBud. Unreadable data — damaged, **blank**, newer version, or a folder
  that cannot be reached — **says so, touches nothing, closes**; a saved *empty budget* is valid.
  A second start says MoneyBud is already open. A failed save **carries on**, shows a lasting
  "not saved" line until a save works (retried by every change and once a minute), says once that
  all is saved again, and closing makes one last attempt without asking. An interrupted save never
  damages the previous one, and the next start says nothing.
- **Built as:** one JSON file, whole ledger per save, cents as integers, `"version": 1`, written to
  a `.tmp` and renamed over; a lock file claimed before loading; categories keyed by position per
  save (never by name), entry ids and `lastEntryId` kept; `Ledger.FromSnapshot` re-checks every rule.
  The save line is its own element beside the notice and the question — a second markup test holds it.
- **Tests use the real store** in a temp folder per scenario. Saving is made impossible by a folder
  at the `.tmp` path; "interrupted" rebuilds the post-crash disk state — an approved simulation.

**Watch out for:** only an act that *changes* the ledger saves — `Tell(changed:)`. Adding a name
already there, assigning 0 and a negative clipped against a zero budget are said but not saved; a
new no-op act must pass `changed: false`, or a failing disk shows a false "not saved".

**Increment 8 — opening a period — is done and green**, built on branch
`increment-8-opening-a-period` (2026-09-27), approved by Axel the same day and merged into `main`. Stages 1–4 on 2026-09-26:
seventeen rulings in §12 *Opening a period*; `features/take-over-a-plan.feature` (19 scenarios,
23 cases) **approved at the first gate**; the plan below **approved at the second**. Built as planned;
`spec-reviewer` found no faked scenario and three low items, all fixed (a refused take-over's notice
was unchecked; the button did not use `Tekst.TakeOverPlan`; §5 not updated). Headless check of the
real window passed. Budget column widened 86 → 104 px so "plan: € 1.450,00" fits. In outline: in the current period and later ones, while every Budget there is zero (archived
included), MoneyBud offers the latest earlier period's plan (a period counts only with a Budget > 0
for a category not archived now); a button names the source period and total, each row shows grey
"plan: € 400,00", rows sort by plan figure during the offer; taking it over assigns in full into the
**period on screen**, unconfirmed, may go over-assigned, and the notice names the period. The approved plan:

- **Domain.** `Ledger.PlanOfferedIn(period)` → `PlanOffer?` (source period, figures per category,
  total): null for a past period or when any Budget there is > 0; the source is found directly from
  stored budgets (latest earlier period with a Budget > 0 for a non-archived category), no loop, no
  limit — **never `HasBudget`**. `Ledger.TakeOverPlan(period)` → result: refuses a past period with
  `AssignRefusal.PeriodInPast` (same message as assigning); otherwise calls `Assign` per figure, the
  only writer of a budget; throws when nothing is offered (unreachable). No new ADR; nothing new saved.
- **Presentation.** `PeriodOverview.Offer` + button text; `CategoryRow.PlanFigure` + "plan: € …";
  rows sort Budget, then plan figure, then order added. `MoneyBudApp.TakeOverPlan` acts on
  `ShownPeriod`; notice always names the period, proposed *"Plan van augustus 2026 overgenomen in
  oktober 2026: € 1.450,00 toegewezen."*; saves (`changed: true`). `Tekst` gets *Plan overnemen* and
  *plan* — move them from §12's proposals table into the real display-terms table at the same time.
- **Desktop.** Button directly under the assign form, visible only with an offer; grey figure under
  the Budget figure in each row (no new column).
- **Tests.** `TakeOverSteps.cs`; a `plan` column in the categories-table step; a period phrase for
  "the budget period 2 before the current one"; **anchor** the existing "the next budget period
  begins while MoneyBud is open" step, or it also matches the new "…before the Overview is next
  drawn". Unit tests: the search back, the throw, `Tekst` wording, the sort incl. equal plan figures.
- **After green — done except the last:** `spec-reviewer`; stale header comments; §5, §8.1, §12;
  README; this file; headless check; Axel's approval.

**Increment 9 — accounts and net worth — is done and green**, built on branch
`increment-9-accounts` (2026-09-27), tried by Axel the same day ("I love how it is currently
working") and merged into `main`. All five stages ran on 2026-09-27. Stage 1 was
multiple-choice questions, each with a recommendation, and Axel took every recommendation (transfers
only once reworded around an ATM withdrawal): sixteen rulings, then eleven follow-ups raised by
`arc42-keeper` and `scenario-writer`, all in §12 *Accounts and net worth*. Six feature files —
`add-an-account`, `record-on-an-account`, `correct-a-balance`, `transfer-between-accounts`,
`manage-accounts`, `show-accounts`, 94 scenarios, 174 cases, plus additions to `start-moneybud` and
`keep-data` — **approved at the first gate** (one row added after it, for a ruling taken at the gate);
the plan **approved at the second**, with ADR 0008. In outline:

- **Scope: accounts and net worth only.** No backing: assigning to Sparen still moves nothing.
- **A balance is worked out, never stored.** A starting balance or a balance correction is what the
  bank said that day: it *holds* every entry dated before its day, and on its day those recorded
  before it (the id is the recording order; a change keeps its id). The first start's
  *Betaalrekening* — and an account added with its starting balance left empty — has no typed
  balance, so its balance is the plain sum of what is on it. A correction's difference is
  **recomputed**: it shows what is still unexplained. Net worth only: never income, never a budget
  figure. A future-dated income reaches the balance on its date.
- **Every income and expense is on an account**, the pool account (*Hoofdrekening*) unless another
  is chosen in a list, last on the form. Any account can be made the pool; pool first, then order
  added. Transfers (*Overboeking*) move two balances, no future date. Accounts: add, rename, delete
  only while unused (never confirmed). Overdrawn and negative net worth carry the one marker, badge
  *Rood*. Old saved data (version 1) cannot be read — Axel's ruling.
- **Screen:** a strip across the top (accounts, *Vermogen*, *Rekening toevoegen*, *Overboeken*);
  clicking an account opens its history underneath, where transfers are changed or removed, typed
  balances removed, and the account renamed, deleted, made the pool or corrected.

`spec-reviewer` found no step reading the ledger where it should read the screen, but one vacuous
scenario (*Choosing an account is for that one entry* never touched the form's list) and one real
defect (a transfer's Naar chosen before Van was discarded), plus low items; all fixed, the vacuous
one proven by a mutation now failing it. **A headless run of the real window found what no test
could:** an Avalonia list writes back what it shows, and nothing while its items are replaced, so
cleverness about "not chosen" pinned defaults as choices. Now a form holds plain accounts, ignores a
null write, and defaults move only at defined moments (see §12, *Chosen in the build*).

**Changed after Axel tried it** (2026-09-27), both copy, not rulings: the unreadable-data message
now names the possible causes (MoneyBud cannot tell them apart; still no place named, §12), and
every message quotes names with plain double quotes, not the Dutch „…” pair, which Axel disliked
(`Tekst.Quoted`). Keep plain quotes in new messages.

**Watch out for:** a form's account list is a two-way binding that **writes back** — on first show,
and whenever its items change. Anything a form infers from what a list writes will go wrong in the
window with every test green. Keep the rule: plain values, null ignored, defaults moved by the acts
that change them (`MakePool`, `AddAccount`), and the forms told **after** the redraw. Check a change
here with a headless run of the window.

**Increment 10 — backing and *Accumulated* — is done and green**, built on branch
`increment-10-backing` (2026-09-27), tried by Axel the same day ("voor de rest ziet het er goed uit")
and merged into `main`. His one remark was that the *Staat op* caption did not line up with its list:
the app's style gave the `ComboBox` a margin, now `Margin="0"`, which a measured headless run
confirmed.

- **Stage 1–2 done** on 2026-09-27: every ruling, follow-up and derivation is in §12 *Backing and
  Accumulated* — read it first. In one line: one backing account per category; assigning moves money
  pool → backing account on the day assigned (a later period's on its first day, to whatever backs it
  then); backing moves this period's unspent *Remaining*; unbacking and re-pointing move "what is
  there for it" (Axel's own revision); *Opgebouwd* counts from the last backing, up to the period on
  screen; on screen *Staat op* and *Opgebouwd*.
- **Stage 3 done:** five new feature files (`back-a-category`, `assign-to-a-backed-category`,
  `spend-against-a-backed-category`, `show-accumulated`, `show-moved-money`; 62 scenarios, 84 cases)
  plus additions to `start-moneybud` and `keep-data` and comment edits to five approved files —
  **approved by Axel at the first gate** on 2026-09-27.
- **Stage 4 done:** [the plan](docs/plans/increment-10-backing.md) **approved at the second gate** on
  2026-09-27. D1 as recommended ([ADR 0009](docs/decisions/0009-movements-are-entries.md)): every move
  is a stored `Movement`, and planned money is written by `Ledger.Settle` on the period's first day.
  D2 Axel left open ("I dont mind starting over"): **version-2 data is refused**, not read.
- **Stage 5 done except Axel's try.** Built the same day. `spec-reviewer` found no faked scenario and
  no money defect, and three low items. Two are fixed: money moved from the pool account to itself no
  longer blocks deleting a category, and the "no Accumulated" step now requires the row. The third,
  picking the account already shown not sticking, Axel accepted. A headless run of the real window
  passed: list write-backs move no money and say nothing, and the form's account follows the category.
- **Ruled at the build** (all in §12 *Backing: chosen in the build*, each on the recommendation):
  when the pool account backs a category, re-pointing takes its money along, which overrides the
  plan's wording. A category with money moved for it **between two accounts** cannot be deleted. **An
  archived backed category is shown in the current and later periods while its Opgebouwd there is not
  zero**, with one new scenario in `show-accumulated`. One line of `assign-to-a-backed-category` was
  amended to assert that a row is hidden.

**Watch out for:** every act that changes the ledger calls `Ledger.Settle()` first, which writes the
money planned for any period that has begun. The screen settles on opening and on every `Tick`. A new
mutator on `Ledger` must call it too. `SetBacking` to the backing already set must stay a complete
no-op (no settle, no notice, no save): each row's *Staat op* list writes back on every redraw.

**Watch out for:** since the sweep, settling may **sweep** an ended period, inside any act. So every
path in `MoneyBudApp` that calls the ledger must end in `Tell`, `Refuse` or `SayNothing` (or be
`Tick` or the constructor), which take `Ledger.TakeSweepsMade()`, say them first and save. A new act
must too, or a sweep goes unannounced and unsaved. `SetSweepDestination` to the destination already
set must stay a complete no-op, like `SetBacking`: the *Restant naar* list writes back on every redraw.

**The order of increments** — the pipeline restarts at stage 1 for each; nothing skips ahead to code.
Put questions to Axel as multiple choice with a recommendation (`AskUserQuestion`); that worked well
for persistence and for opening a period.

Order agreed with Axel on 2026-09-26:

1. **Correcting things — done** (increment 6, above).
2. **Persistence — done** (increment 7, above).
3. **Opening a period — done** (increment 8, above).
4. **Accounts and net worth — done** (increment 9, above).
5. **Backing and *Accumulated* — done** (increment 10, above).
6. **The sweep — done** (increment 11, below).

**Increment 11 — the sweep — is done**, built on branch `increment-11-sweep` on 2026-09-28, tried by
Axel ("looks good") and merged into `main`. Stages 1–3 on 2026-09-27 (every ruling in §12 *The sweep
and Restant* — read it first; four feature files plus additions to `start-moneybud` and `keep-data`,
51 scenarios, 66 cases, approved at the first gate); the plan approved at the second gate on
2026-09-28 (ADR 0010; version-3 data refused). In one line: at a period's end MoneyBud moves
*Unassigned* plus the unbacked categories' Resterend, **netted and never below zero**, from the pool
account to one chosen backed destination (*Restant naar*), and says so once; with no destination
nothing moves; a later change to a swept period shows the difference, and *Restant bijwerken* moves
it. `spec-reviewer` found no faked step; **Axel ruled at the build** that take-back is **per move**,
latest first, and that the difference is **measured against what really moved** (an amount let go
absorbs a later rise first); a crash in the minute after a boundary was fixed. **Open for Axel, not
urgent:** `keep-data`'s scenario about the backing at a period's end cannot tell kept records from
lost ones (a unit test holds it); he may want it rewritten (§12, *A note for the stakeholder*).

**Next, in order** — agreed with Axel on 2026-09-28, "the three still important to me", with one
small change put in front of them the same day:

0. **Opgebouwd follows Resterend — done**, merged into `main` (2026-09-29, lean route: no
   subagents; the suite and a mutation check instead of `spec-reviewer`). Found by Axel trying
   increment 12: a weekly *broodje kip* of € 4 set up from 14 September after backing Boodschappen
   (Budget 300) gave Resterend 288 but Opgebouwd 296, because only expenses dated after the backing day
   counted. **Ruled with him** (§12, *Backing a category that already has money*, ruling of
   2026-09-28), each on the recommendation after he reshaped the first proposal: **in the period of
   backing, Opgebouwd moves with Resterend** — every expense dated in that period or later counts,
   whenever entered, and a change to an older one moves it too; an expense dated before that period
   does not (it is for *Restant bijwerken*); an **overspent** category starts **below zero, at its
   Resterend** (−50, not 0); **ThereFor follows the same rule** on its account. Built as one
   remembered figure per mark on `Backing`: `NotMoved` (the backing period's Budget not moved at
   backing = min(Budget, spent)) and `PaidHereBefore` (what the backing account had paid in that
   period). **File format version 6**; version 5 is read, the figures worked out again from record
   order (`Ledger.NotMovedBefore`, `PaidBefore`). Scenarios revised in `spend-against-a-backed-category`
   (two new, his case among them), `back-a-category` and `show-accumulated`. Six mutations, all caught
   (one only after a unit test was added for `PaidHereBefore`). Built on branch
   `opgebouwd-follows-resterend`, full solution build clean, merged into `main` on Axel's word.

1. **Recurring entries** on income and expenses — increment 12, **done**: built, tried by Axel and
   merged into `main` on 2026-09-28 (below: stages 1–4 as they ran, then *Stage 5*). His shape:
   "just an extra drop-down where you can choose default one time, or weekly or monthly". Ruled so far
   (2026-09-28, each on the recommendation, not yet in §12): the choices are **Eenmalig** (default),
   **wekelijks**, **maandelijks** — yearly left until missed; each occurrence is **recorded on its own
   date** as an ordinary entry, the first time MoneyBud runs on or after it, missed ones all on the next
   start, **told once**; the same rule for income and expenses, so a future expense is still never
   recorded; **the latest occurrence sets the next** (amount, label, category, account, frequency),
   changing it changes what follows, setting it to *Eenmalig* stops it, earlier occurrences are never
   touched; **removing an occurrence removes only that one** and the repeat carries on; a monthly one
   that started on the 31st falls on a short month's **last day and returns to the 31st** (it keeps
   the day it started on); the **latest occurrence's row carries a grey "maandelijks"/"wekelijks"**,
   earlier ones are plain; occurrences recorded by themselves are **told in one notice, once**, and
   saved straight away, as the sweep is; an occurrence on an **archived category is recorded and
   brings it back**, as recording by hand does. **Stage 2** wrote them into §12 *Recurring entries*,
   and six follow-ups were ruled the same day: removing the latest makes the **newest remaining
   occurrence the latest**, and removing the only one ends the repeat; an **earlier occurrence shows
   *Eenmalig*, locked**; **changing the latest occurrence's date moves the day for all later ones**
   (against the recommendation, with the one-off-Saturday consequence put to him); a repeat set up in
   the past records what is already due **at once**; closed across a period end, MoneyBud settles
   **day by day**, so occurrences come before their period's sweep; the drop-down is **last on the
   form, captioned *Herhalen***. **Stage 3 written** (2026-09-28): `repeat-an-entry.feature` (its header
   explains the shared steps), `change-a-repeat.feature` and a "Repeats are kept" section in
   `keep-data.feature` — 51 scenarios, 61 cases — the **first files to use calendar dates** (a monthly
   repeat keeps a day of the month). Three more rulings at the scenario stage, all on the
   recommendation: removing a stopped repeat's last occurrence **leaves it stopped** (the one before
   shows *Eenmalig*, changeable); the drop-down order is **Eenmalig, wekelijks, maandelijks**; the grey
   label is **only in the Overview's lists**, not in an account's history (a unit test, not a
   scenario). Binding note: a "today is <date>" Given must also make that day the first start.
   **Approved by Axel at the first gate on 2026-09-28**, with every documentation's reading in their
   headers (§12, *Approved at the scenario gate*, under *Recurring entries*). **Stage 4 written**
   (2026-09-28): [the plan](docs/plans/increment-12-recurring.md) — D1, a recurring entry kept beside
   the entries (occurrence ids, frequency, day, next date; the latest is the highest id) and settling
   event by event (ADR 0011, version 5); D2, **read** version 4 as data with no repeats (recommended);
   ten readings under *Chosen in this plan*. **Approved by Axel at the second gate on 2026-09-28**,
   every point on the recommendation. **Stage 5 done** (2026-09-28), all but Axel's try:
   - **Built as planned** ([ADR 0011](docs/decisions/0011-recurring-entries.md), file format version 5,
     version 4 read). A `RecurringEntry` beside the entries (occurrence ids, frequency, day, next
     date), the latest being the highest id; `Settle` works event by event, a period's end before that
     day's occurrences; `FrequencyOf`/`SetsTheRepeat` drive the grey label and the *Herhalen* lock.
   - **Ruled at the build**, on the recommendation: the notice says things **in the order they
     happened**. What settling did before an act comes in front of it, and the occurrences the act
     caused come after it (`MoneyBudApp.SettleBeforeActing`), because the increment-6 "was changed"
     step needs the change first.
   - `spec-reviewer` found no faked scenario and no money defect, and two low items, both fixed. Three
     deliberate mutations, and a fourth for the review's finding, were each caught. A headless run of
     the real window passed. Its rendered frame showed the *Herhalen* caption off-centre by the fields'
     style margin, now on the panel.
   - **Tried by Axel** (2026-09-28). His one finding is item 0 above, a rule change, not a defect.
   - **For Axel, not urgent:** the approved derivation that only a repeat set up in the past or a latest
     date moved back can record into an already-swept period is incomplete. Restarting a stopped repeat
     from an old occurrence, and switching the latest from monthly to weekly, can do it too (a dated
     note in §12). Within what settling did, the notice lists all occurrences, then all sweeps: after
     a long absence that is not strictly chronological (plan reading 7).
2. **A configurable period start day** — increment 13, **done**: built on branch `increment-13-start-day`, tried by Axel ("looks good") and merged into `main` on 2026-09-29. His salary
   comes on the 27th. **Stages 1–3 done** (2026-09-29): sixteen rulings, all on the recommendation
   except follow-up 5, which he answered in his own words, in §12 *A configurable period start day*.
   Read it first. In one line: changeable any time, **from the current period on** (it keeps its
   first day and ends the day before the new day first comes round, possibly on the spot); earlier
   periods keep their boundaries; a plan made ahead goes to the period its old first day falls in;
   money a change moves is dated the day of the change; **a change never changes Opgebouwd** ("just
   resterend + earlier resterend + any money from sweeps"); a drop-down *Periode begint op* beside the
   period name, current and later periods only, **asking first**; names "27 sep – 26 okt 2026",
   "1 – 26 sep 2026", "27 sep 2026". Three feature files plus additions to `keep-data` and
   `start-moneybud`, 33 scenarios, 50 cases, **approved at the first gate**. **Stage 4 written:**
   [the plan](docs/plans/increment-13-start-day.md), D1 the calendar as a history of changes (ADR
   0012, version 7), D2 read version 6. **Approved by Axel at the second gate on 2026-09-29**, D1, D2 and all seven readings on the recommendation.
   **Stage 5 done** (2026-09-29), tried by Axel and merged. Built as planned
   ([ADR 0012](docs/decisions/0012-the-calendar-is-a-history.md), file format version 7, version 6 read), with these differences:
   - A `StartDayChange` keeps the first day of the period it was made in, as well as `From`, since
     `From` alone cannot tell which period was cut. A `Backing` keeps two first days, one per mark.
   - **Ruled at the build**, each on the recommendation: two cells of the approved February/31st
     outline corrected from 30 to 29 April (the clamp as ruled); and in the one corner where ruling 1
     makes the current period longer (begun on a clamped 28 Feb under the 29th, changed to the 31st:
     28 Feb – 30 Mar), ruling 1 is followed.
   - Found at the build: two changes in one period (to the 30th, plan ahead, back to the 1st) land a
     plan made ahead in the current period. It adds up, and its backed money moves at once.
   - `spec-reviewer` found no faked scenario and two low defects, both fixed with a test: a clock turned
     back could move a settled plan twice; Wijzigen after midnight before the tick could act on another
     period. Eleven mutations caught; a headless run of the real window passed 28 checks.
   - **For Axel, not urgent:** while a plan is offered, the grey "plan: € 400,00" runs into the
     *Staat op* list in the same row. Older than this increment, seen in the rendered frame.
3. **A mobile front-end** — "the biggest and last for now", and what makes him actually use it. Wanted
   with **no double work** between desktop and mobile: that is what the toolkit-free
   `MoneyBud.Presentation` is for (ADR 0006), and Avalonia runs on Android and iOS. The big stage-1
   question will be **where the data lives** with two devices.

Not chosen for now: switching to real use (a migration promise for the data file), the month in
review, importing bank transactions, and the items deferred until missed.

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

**No open questions since 2026-09-27.** The last one, an income back-dated into a period whose
*Unassigned* was already swept, was answered with the sweep: like a late expense, the difference is
**shown**, and the user moves it with *Restant bijwerken*; MoneyBud never adjusts a sweep by itself
([§12](docs/arc42/12-glossary.md), *The sweep and Restant*). The other former open question, **how an overdrawn account is shown**, was answered in increment 9: the same marker, badge
*Rood*.

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
