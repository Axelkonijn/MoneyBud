# 5. Building Block View

**What belongs here:** The static structure, decomposed in levels. Level 1 is the whitebox view of
the whole system — its top-level parts and their responsibilities. Deeper levels zoom into any
part that is complex enough to warrant it.

Only decompose where it earns its keep. A level-3 breakdown of a trivial component is noise.

---

MoneyBud is **four source projects and one test project**. Until the fifth increment it was one
library and one test project. The UI split into a layer that decides what the screen shows and a
thin one that draws it. The reasoning is in [ADR 0006](../decisions/0006-three-source-projects.md),
which supersedes the "two projects" of [ADR 0004](../decisions/0004-solution-layout.md). The
persistence increment added the fourth, `MoneyBud.Storage`, which keeps the ledger in a file
([ADR 0007](../decisions/0007-keeping-the-ledger.md), which amends 0006). This section stays short,
for the same reason [§7](07-deployment-view.md) is.

## Level 1 — Whitebox: MoneyBud

```
MoneyBud.Desktop ─────► MoneyBud.Presentation ─────► MoneyBud.Domain
   (Avalonia)              (no UI toolkit)           (base class library only;
       │                                              defines ILedgerStore)
       │                                                     ▲
       └──────────────► MoneyBud.Storage ────────────────────┘
                          (no package)

MoneyBud.Specs ─────► Presentation, Domain and Storage   (never the Desktop)
```

An arrow is a project reference. The Desktop references the Presentation layer and Storage. The
specs reference Presentation, Domain and Storage. Nothing references the Desktop. **The presentation
layer does not reference Storage.** It knows a store only through `ILedgerStore`, which the domain
defines and `MoneyBud.Storage` implements, and the Desktop is the one place that puts the two
together.

| Building block | Responsibility |
|---|---|
| **`src/MoneyBud.Domain`** | The model and its rules. Categories, budgets, expenses, income, budget periods and the `Money` type. Adds and archives categories and brings archived ones back, under one category name rule (`CategoryName`). A `Category` is a class **with identity**, one instance per category, whose name only the domain can set. So it can be renamed (`Ledger.RenameCategory` → `RenameCategoryResult`, with `RenameOutcome` and `RenameRefusal`), and one with no history anywhere can be deleted (`Ledger.CanDelete`, `Ledger.DeleteCategory`). Ships the six default categories for a first start (`Ledger.StartNew`). Assigns to categories (`Ledger.Assign`), which is the only way a budget is made. Records expenses and incomes, each with a ledger-issued `Id`, and changes them in place (`ChangeExpense` → `ChangeExpenseResult`, `ChangeIncome` → `ChangeIncomeResult`, both with `ChangeOutcome`) or removes them (`RemoveExpense`, `RemoveIncome`), judging a change by recording's own checks. Computes *Remaining*, *Unassigned* and whether a period is *Over-assigned*, decides what to refuse, and reports what each act did. Decides which categories a period shows (`Ledger.CategoriesShownIn`). Since the persistence increment it also defines **what is kept and the port it is kept through**, without knowing what a store is made of: `LedgerSnapshot`, the whole ledger as plain data, with categories referred to by a key rather than a name; `Ledger.ToSnapshot` and `Ledger.FromSnapshot`, the second re-checking every rule the running ledger keeps and throwing `InvalidDataException` for kept data that breaks one; and `ILedgerStore`, with its `Claim` and `LoadResult`. Depends on nothing but the .NET base class library: no UI framework, no file handling, not even an ambient clock (`TimeProvider` is passed in) |
| **`src/MoneyBud.Presentation`** | **Everything the screen decides, with no UI toolkit.** `MoneyBudApp` is the whole screen: the period on screen and stepping, the acts the user can take with their defaults, and the notice afterwards, including where an entry went when it landed in another period. Since the corrections increment it also holds the one question MoneyBud asks, whether to remove an entry (`Question`, `Confirm`, `Decline`), and the category being renamed. The expense and income forms have a *Wijzigen* state for an entry loaded from its row, and the rows carry their entries. `PeriodOverview` is one period as the Overview shows it (rows, markers, the two lists), and `Ring` is its slices with their clockwise shares, each at least `Ring.MinimumSweep`. `MoneyBudApp.PointAt` and `Ring.SliceAt` decide which slice is pointed at and what the ring's hole shows. It also holds the category suggestions, the entry forms, `AmountInput` for reading typed text, and `Tekst` for every Dutch word shown. **Since the persistence increment it decides everything about keeping data that the user meets**: `MoneyBudStart.Start` claims and loads a store and returns the screen or the one reason MoneyBud does not start (`StartResult`, `StartRefusal`); `MoneyBudApp` saves after every act that changed the ledger, holds the save line (`IsUnsaved`, `SaveLine`), retries on `Tick` and makes the last attempt on `Close`. It knows a store only through the domain's `ILedgerStore`. References the domain, and CommunityToolkit.Mvvm, which depends on no toolkit ([ADR 0005](../decisions/0005-avalonia-ui-toolkit.md)) |
| **`src/MoneyBud.Storage`** | **Where the ledger is kept, and in what form** ([ADR 0007](../decisions/0007-keeping-the-ledger.md)). `FileLedgerStore` implements `ILedgerStore` on one folder: it claims `moneybud.lock` exclusively, loads `moneybud.json`, and saves by writing `moneybud.json.tmp`, flushing it and renaming it over the data file. `FileLedgerStore.DefaultFolder` is the user's local application data plus `MoneyBud`. `LedgerJson` writes and strictly reads the format: whole cents as integers, `yyyy-MM-dd` dates, a format name and version. Decides nothing the user sees. References the domain only, and no package |
| **`src/MoneyBud.Desktop`** | The Avalonia window, laid out income, plan, expenses, and `RingControl`, which draws the ring from `Ring`'s shares and turns the pointer's position into a share. Transaction rows are buttons that load their entry, and each category row has its acts in a line under its name. At start it makes a `FileLedgerStore` on the default folder and hands it to `MoneyBudStart`, then shows either the window or a small message window with the refusal's text. It calls `MoneyBudApp.Tick` once a minute and `Close` when the window closes. **Deliberately thin, and with no automated tests except two that read the window's markup as text**: the order of the form fields, and the save line standing beside the notice and the question ([§8.4](08-crosscutting-concepts.md), [§11](11-risks-and-technical-debt.md)) |
| **`tests/MoneyBud.Specs`** | Runs the specification: Reqnroll step definitions on xUnit, with the scenario's world in `Support/`. *Given* steps set up the ledger; every *When* acts through `MoneyBudApp`; *Then* steps about what is shown read the presentation layer ([§8.4](08-crosscutting-concepts.md)). **Every scenario keeps its data through the real `FileLedgerStore`**, in a temporary folder of its own. Also holds the developer unit tests in `Unit/`, for the domain, the presentation layer and storage. They are tests and not specification; ADR 0004 says why they share a project and what rule keeps them subordinate |

`features/` is **not a building block.** It holds the Gherkin specification, and the specs project
*links* those files in rather than owning a copy of them (ADR 0004). It is listed here only because
a reader looking at the solution will see the linked files and wonder where they really live.

The solution file is `MoneyBud.slnx` at the repository root.

## There is no level 2

None of the four projects has an internal boundary worth drawing. The domain is a handful of types,
and [§8.1](08-crosscutting-concepts.md) says what is worth saying about them. The presentation layer
is a handful more, and [§8.4](08-crosscutting-concepts.md) does the same for it. Storage is two
classes, and [§8.3](08-crosscutting-concepts.md) and ADR 0007 say what matters about them. The
Desktop is one window and one control. A breakdown of any of them would restate the class list. The
code itself is the level-2 view.

Adding a capability therefore updates the responsibilities above and §8, and leaves the rest of this
section alone. The income, category and assigning increments each added types to the domain and
moved no boundary. The UI increment was the first that did move them, and ADR 0006 records it. It added
two queries to the domain, `CategoriesShownIn` and `ExpensesIn`, and no behaviour. The corrections
increment moved none either. It added acts and result types to the domain, and gave `Category` and
the entries identity, which is a change inside the domain and is described in
[§8.1](08-crosscutting-concepts.md). The persistence increment moved a boundary again, the second
increment to do so. It added `MoneyBud.Storage`, and a port and a snapshot to the domain
([ADR 0007](../decisions/0007-keeping-the-ledger.md)).

## What used to be absent

Until the persistence increment this section had a table of one: **anything that stores data**,
absent for a recorded reason. [§8.3](08-crosscutting-concepts.md) deferred persistence with a
trigger, and the section named adding a store as the moment to re-examine the project layout. The
trigger never fired. The stakeholder chose to build persistence anyway, on 2026-09-26, and the
re-examination produced the fourth project above rather than a store folded into an existing one
(ADR 0007 says why). Nothing else is conspicuously absent from the structure. What the model lacks,
such as accounts, is absent from the domain, and §8.1 lists it.

[§7](07-deployment-view.md) maps all of this onto one process on one machine, which no part of
this section changes.
