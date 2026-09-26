# 0006 — Three source projects: domain, presentation, desktop

**Status:** Accepted; **amended by [ADR 0007](0007-keeping-the-ledger.md)** (2026-09-26), which adds a
fourth source project, `MoneyBud.Storage`. See the note at the end.
**Date:** 2026-09-25
**Supersedes:** decision 1 of [ADR 0004](0004-solution-layout.md), "two projects". ADR 0004's other
three decisions stand: xUnit as the runner, the feature files linked in from `features/`, and
developer unit tests in the specs project's `Unit/` folder.

## Context

[ADR 0004](0004-solution-layout.md) decided on two projects, a domain library and a specification
project, and rejected a layered set because its layers would have been empty or pass-through. It
named what would change that:

> If a UI or a store arrives and the domain starts accumulating things that are not domain, that is
> the signal — and the project count is one of the cheapest things in the repository to change.

The fifth increment brought the UI. It also brought a set of decisions that are about **what the
screen shows**, and not about the domain. All of them were settled with the stakeholder
([§12](../arc42/12-glossary.md), *The user interface*):

- which period is on screen, how stepping moves it, and that it stays put across a period boundary;
- which categories a period lists, in what order, and with which figures and markers;
- how the ring is sliced, sized and filled, and when it is empty;
- which categories are suggested, and in what order;
- what an entry's date and an assignment's period default to;
- that the screen says where an entry went when it lands in another period;
- how typed text is read as an amount;
- every word shown, in Dutch.

None of that is domain. Put in `MoneyBud.Domain`, it would be exactly the "accumulating things that
are not domain" ADR 0004 names. Dutch copy in particular would undo
[§8.1](../arc42/08-crosscutting-concepts.md)'s rule that refusals are reasons and not messages. Put
in the Avalonia project, it would be out of the scenarios' reach, because the scenarios do not start
a window.

The layout was proposed in the implementation plan and **approved by the stakeholder at the plan
gate on 2026-09-25**, together with [ADR 0005](0005-avalonia-ui-toolkit.md).

## Decision

Three source projects, and the specification project over two of them:

| Project | Holds | References |
|---|---|---|
| **`src/MoneyBud.Domain`** | As before: the model, its rules and its refusals. Unchanged in kind. The UI added two queries, and no behaviour (below) | The .NET base class library only |
| **`src/MoneyBud.Presentation`** | **Everything the screen decides, with no UI toolkit**: the period on screen and stepping (`MoneyBudApp`); `PeriodOverview`, its rows, lists and markers; `Ring`, its slices and their clockwise shares; the category suggestions; the entry forms and their defaults; the notice when an entry lands in another period; `AmountInput`, which reads typed text as an amount; and `Tekst`, all the Dutch | `MoneyBud.Domain`, and CommunityToolkit.Mvvm, which depends on no UI toolkit (ADR 0005) |
| **`src/MoneyBud.Desktop`** | The Avalonia window and `RingControl`, which draws the ring from `Ring`'s shares. **Deliberately thin, and with no automated tests** | `MoneyBud.Presentation`, and Avalonia |
| **`tests/MoneyBud.Specs`** | As before: step definitions and the unit tests in `Unit/` | `MoneyBud.Domain` and `MoneyBud.Presentation`. **Not** `MoneyBud.Desktop` |

The UI added two queries to the domain, and no behaviour. **`Ledger.CategoriesShownIn(period)`** is
the full display rule ([§12](../arc42/12-glossary.md), *When any category is shown in a period: the
full rule*), returning categories in the order they were added. **`Ledger.ExpensesIn(period)`**
returns a period's expenses in the order they were recorded. Both are facts about the model that any
view would need. The ordering by *Budget* and the newest-first lists are the screen's, and live in
`PeriodOverview`.

## Why

### This is not the pass-through layer ADR 0004 rejected

ADR 0004 rejected an Application layer because it "would hold methods that forward to `Ledger` and
do nothing else". `MoneyBud.Presentation` is not that. Its acts are thin calls on the ledger, but
around every one of them sits a decision the stakeholder took: the default date, the period
assigned to, the notice about where the entry went, the reading of the amount, the Dutch sentence.
Its read side is not thin at all. The ring's slicing, the order of rows, the markers and the empty
ring are rules, each with scenarios of its own.

### It is where the new scenarios are run

**Every scenario this increment added is about what the screen shows.** A layer that holds what the
screen decides, and needs no window, is a layer the scenarios can run against directly. The
alternatives both lose that:

- **Put it in the Avalonia project.** The scenarios would then need a running Avalonia application,
  headless or not, to reach any of it. The specification would depend on a toolkit, and replacing
  the toolkit would mean rewriting the step definitions as well as the window.
- **Put it in the domain.** It would run in the scenarios, but the domain would then hold Dutch copy,
  display order and date defaults. That is the signal ADR 0004 said to watch for, and it would
  break the refusals-are-reasons rule (§8.1).

This record's reasoning, rather than something weighed separately at the gate: both alternatives
fail on the grounds the plan gave.

### It keeps ADR 0002's "survives a mobile UI" property

[ADR 0002](0002-desktop-application-first.md) relies on the scenarios surviving being pointed at a
mobile UI later. That was true of the domain scenarios because they are declarative. It is now true
of the screen scenarios for a second reason: they run against a layer with no toolkit in it. A mobile
head could sit on `MoneyBud.Presentation` as the Desktop does. Whether it should is for the ADR that
reopens mobile.

### The boundary is held the way ADR 0004 held the domain's

ADR 0004 enforced the domain's purity with an empty reference list. This record does the same one
layer out. **`MoneyBud.Presentation` has no reference to Avalonia**, so nothing that needs a window
can end up in it by accident, and the specs project does not reference the Desktop at all.

## Consequences

- **The Desktop has no automated tests, by plan.** It was checked by running it, on Windows, and by
  rendering it headless with synthetic data ([ADR 0005](0005-avalonia-ui-toolkit.md)). That is
  acceptable only while it holds nothing that decides anything, which makes **"deliberately thin" a
  rule to keep**, not a description. **The rule has already been enforced once.** As first built,
  narrowing the suggestions as you type was the toolkit's `AutoCompleteBox` *Contains* filter, set
  in the window's markup. That was a stakeholder ruling living in the Desktop, compared by the
  thread's culture and held by no test. It was found while this record was being written and **moved
  into `MoneyBud.Presentation` the same day**, as `MoneyBudApp.SuggestionMatches` and
  `SuggestionsFor`, with unit tests. The Desktop now only hands that predicate to the control as a
  custom filter. This is what "a decision found in the Desktop moves to the presentation layer"
  means in practice ([§11](../arc42/11-risks-and-technical-debt.md)).
- **Every *When* step now acts through `MoneyBudApp`**, including those of the five earlier feature
  files. So the domain scenarios go through the same doors as the screen: an amount is typed text
  read by `AmountInput`, and a date left out means today
  ([§8.4](../arc42/08-crosscutting-concepts.md)).
- **ADR 0004's decision 4 extends without changing.** The presentation layer's developer tests (amount
  reading, the Dutch wording, money formatting, the ring's shares, the forms) sit in
  `tests/MoneyBud.Specs/Unit/` beside the domain's. ADR 0004's rule for them holds unchanged: a unit
  test is never the reason a behaviour exists.
- **`MoneyBud.Domain` still references only the base class library.** The first two package
  references of the UI increment, Avalonia and CommunityToolkit.Mvvm, went into the new projects.
  ADR 0004's warning about the first package reference added to the domain is therefore still
  unspent, and still stands.
- **[§5](../arc42/05-building-block-view.md) gains two building blocks, and
  [§6](../arc42/06-runtime-view.md) becomes writable**, because two of them now collaborate.
- **Reversal stays cheap.** Merging `MoneyBud.Presentation` back into another project moves files and
  changes no behaviour. It would cost the scenarios their window-free home, which is the reason for
  the split.

## Note, 2026-09-26: one test now reads the Desktop's markup

This note adds to the record and rewrites nothing above. The first demo's rulings were built on
2026-09-26 ([§12](../arc42/12-glossary.md), *The user interface*, first demo). Two of them went where
this record says: the minimum slice width is `Ring.MinimumSweep`, and what pointing at the ring
shows is decided by `MoneyBudApp.PointAt` and `Ring.SliceAt`. The Desktop only turns the pointer's
position into a share. The third ruling, the order of a form's fields, can live only in the window's
markup. It is held by `WindowMarkupTests`, which reads `MainWindow.axaml` as text. **That qualifies
the first consequence above**, "the Desktop has no automated tests, by plan". It now has one, and it
starts no window. The specs project still does not reference the Desktop. **Approved by the
stakeholder at the plan gate on 2026-09-26 as a small departure**, not as a change to the decision.
The three-project split stands. Details are in [§8.4](../arc42/08-crosscutting-concepts.md), *One test
reads the window's markup*.

## Note, 2026-09-26: a fourth project, for keeping data

This note adds to the record and rewrites nothing above. The persistence increment added
**`MoneyBud.Storage`**, which holds the file the ledger is kept in, and **[ADR 0007](0007-keeping-the-ledger.md)**
records why. Approved by the stakeholder at the plan gate on 2026-09-26.

**What it changes here.** The title's "three" is now four. Two reference lists in the table above
grew: the Desktop also references `MoneyBud.Storage`, and so does `MoneyBud.Specs`. The specs still
do not reference the Desktop. `MoneyBud.Presentation` does **not** reference the new project. It
knows a store only through a port, `ILedgerStore`, which sits in the domain.

**What it does not change.** Each of the three projects keeps the role this record gave it. The
domain still references only the base class library. The presentation layer still holds everything
the screen decides, and now also decides when to save and what is said about it. The Desktop is
still deliberately thin: it makes the store and shows what the presentation layer returns. That is
why this is a note and not a superseding record. No decision above was reversed. One was extended.

**The markup exception widened, on the same terms.** A second test in `WindowMarkupTests` holds that
the save line is a sibling of the notice and the question. It was approved at the plan gate as part
of the same small departure described in the note above. The §8.4 subsection the note above names
is now called *Tests that read the window's markup*.
