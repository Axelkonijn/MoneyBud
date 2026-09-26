# 5. Building Block View

**What belongs here:** The static structure, decomposed in levels. Level 1 is the whitebox view of
the whole system — its top-level parts and their responsibilities. Deeper levels zoom into any
part that is complex enough to warrant it.

Only decompose where it earns its keep. A level-3 breakdown of a trivial component is noise.

---

MoneyBud is **three source projects and one test project**. Until the fifth increment it was one
library and one test project. The UI split into a layer that decides what the screen shows and a
thin one that draws it. The reasoning is in [ADR 0006](../decisions/0006-three-source-projects.md),
which supersedes the "two projects" of [ADR 0004](../decisions/0004-solution-layout.md). This
section stays short, for the same reason [§7](07-deployment-view.md) is.

## Level 1 — Whitebox: MoneyBud

```
MoneyBud.Desktop  ──►  MoneyBud.Presentation  ──►  MoneyBud.Domain
   (Avalonia)            (no UI toolkit)             (base class library only)
                               ▲                            ▲
                               └──────  MoneyBud.Specs  ────┘
```

An arrow is a project reference. Nothing references the Desktop.

| Building block | Responsibility |
|---|---|
| **`src/MoneyBud.Domain`** | The model and its rules. Categories, budgets, expenses, income, budget periods and the `Money` type. Adds and archives categories and brings archived ones back, under one category name rule (`CategoryName`). Ships the six default categories for a first start (`Ledger.StartNew`). Assigns to categories (`Ledger.Assign`), which is the only way a budget is made. Computes *Remaining*, *Unassigned* and whether a period is *Over-assigned*, decides what to refuse, and reports what each act did. Decides which categories a period shows (`Ledger.CategoriesShownIn`). Depends on nothing but the .NET base class library: no UI framework, no storage, not even an ambient clock (`TimeProvider` is passed in) |
| **`src/MoneyBud.Presentation`** | **Everything the screen decides, with no UI toolkit.** `MoneyBudApp` is the whole screen: the period on screen and stepping, the acts the user can take with their defaults, and the notice afterwards, including where an entry went when it landed in another period. `PeriodOverview` is one period as the Overview shows it (rows, markers, the two lists), and `Ring` is its slices with their clockwise shares, each at least `Ring.MinimumSweep`. `MoneyBudApp.PointAt` and `Ring.SliceAt` decide which slice is pointed at and what the ring's hole shows. It also holds the category suggestions, the entry forms, `AmountInput` for reading typed text, and `Tekst` for every Dutch word shown. References the domain, and CommunityToolkit.Mvvm, which depends on no toolkit ([ADR 0005](../decisions/0005-avalonia-ui-toolkit.md)) |
| **`src/MoneyBud.Desktop`** | The Avalonia window, laid out income, plan, expenses, and `RingControl`, which draws the ring from `Ring`'s shares and turns the pointer's position into a share. Starts a first-start ledger on the system clock, and tells the screen to look again once a minute. **Deliberately thin, and with no automated tests except one that reads the window's markup as text** for the order of the form fields ([§8.4](08-crosscutting-concepts.md), [§11](11-risks-and-technical-debt.md)) |
| **`tests/MoneyBud.Specs`** | Runs the specification: Reqnroll step definitions on xUnit, with the scenario's world in `Support/`. *Given* steps set up the ledger; every *When* acts through `MoneyBudApp`; *Then* steps about what is shown read the presentation layer ([§8.4](08-crosscutting-concepts.md)). Also holds the developer unit tests in `Unit/`, for both the domain and the presentation layer. They are tests and not specification; ADR 0004 says why they share a project and what rule keeps them subordinate |

`features/` is **not a building block.** It holds the Gherkin specification, and the specs project
*links* those files in rather than owning a copy of them (ADR 0004). It is listed here only because
a reader looking at the solution will see the linked files and wonder where they really live.

The solution file is `MoneyBud.slnx` at the repository root.

## There is no level 2

None of the three projects has an internal boundary worth drawing. The domain is a handful of types,
and [§8.1](08-crosscutting-concepts.md) says what is worth saying about them. The presentation layer
is a handful more, and [§8.4](08-crosscutting-concepts.md) does the same for it. The Desktop is one
window and one control. A breakdown of any of them would restate the class list. The code itself is
the level-2 view.

Adding a capability therefore updates the responsibilities above and §8, and leaves the rest of this
section alone. The income, category and assigning increments each added types to the domain and
moved no boundary. The UI increment is the one that did move them, and ADR 0006 records it. It added
two queries to the domain, `CategoriesShownIn` and `ExpensesIn`, and no behaviour.

## What is not here yet

One building block is conspicuously absent, for a recorded reason rather than by omission:

| Absent | Why |
|---|---|
| **Anything that stores data** | [§8.3](08-crosscutting-concepts.md) defers persistence entirely. State lives in `Ledger` for the lifetime of a run and is gone afterwards, and the UI starts from the default categories every time. §8.3 states what will force the decision, and the UI is the first increment in which it can |

Adding it is the moment to re-examine the project layout again
([ADR 0006](../decisions/0006-three-source-projects.md)).

[§7](07-deployment-view.md) maps all of this onto one process on one machine, which no part of
this section changes.
