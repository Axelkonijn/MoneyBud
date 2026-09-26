# 6. Runtime View

**What belongs here:** How the building blocks actually collaborate, for a few important
scenarios: the main use case, a tricky edge case, startup, error handling. Sequence diagrams work
well.

Cover the scenarios that are *hard to infer* from the static view. Documenting every interaction
is wasted effort — pick the ones where the collaboration is non-obvious.

---

> Note: "scenario" here means a runtime interaction between components, which is a different thing
> from a [Gherkin scenario](../../features/). Gherkin describes user-observable behaviour; this
> section describes internal collaboration.

This section became writable with the fifth increment, when a second building block that does
something arrived ([§5](05-building-block-view.md)). One interaction is worth drawing. Every act the
user can take follows its shape, so it is not repeated for income, assigning or categories.
Changing an entry and renaming a category follow it too. **Removing an entry is the one exception**,
described in words after the diagram, because it is the only act split across two presses.

## Recording an expense through the screen

The main use case, and the one that shows where each decision is taken.

```mermaid
sequenceDiagram
    actor User
    participant Window as Desktop (window)
    participant Form as ExpenseForm
    participant App as MoneyBudApp
    participant Input as AmountInput
    participant Ledger
    participant Tekst

    User->>Window: types amount, category, label; presses "Uitgave toevoegen"
    Window->>Form: SubmitCommand
    Form->>App: RecordExpense(amount text, category, label, date or none)
    App->>Input: read the amount text
    alt not an amount, or ambiguous
        Input-->>App: NotAnAmount / Ambiguous
        App->>Tekst: the refusal, in Dutch
        Note over App,Ledger: the ledger is never called
    else read
        Input-->>App: a decimal, exactly as typed
        App->>Ledger: RecordExpense(decimal, category, date or today, label)
        Ledger-->>App: recorded, or refused for one reason
        App->>Tekst: the outcome or the refusal, in Dutch
        Note over App: recorded in a period other than the one on screen:<br/>add which period it went into
    end
    App->>App: set Notice, then Refresh (property changes)
    App-->>Form: the result
    Note over Form: recorded: clear amount, label, date<br/>refused: keep what was typed
    Window->>App: bindings read Overview again
    App->>Ledger: PeriodOverview.Of: CategoriesShownIn, BudgetFor, SpentOn,<br/>RemainingFor, UnassignedIn, ExpensesIn, IncomesIn
    Note over Window: RingControl draws from Ring's shares
```

What the diagram shows that the static view does not:

- **Two layers of refusal, in a fixed order.** The presentation layer refuses text that is not an
  amount, or that is ambiguous, before the domain sees anything. Only an amount that has been read
  reaches the ledger, and the ledger refuses by its own rules, in its own order
  ([§12](12-glossary.md), *Typing an amount*; [§8.2](08-crosscutting-concepts.md)).
- **The domain answers with a reason. The Dutch is added on the way back**, by `Tekst`. The ledger
  never sees a word the user reads ([§8.1](08-crosscutting-concepts.md)).
- **The screen decides nothing about where the expense lands.** The expense's date decides that, in
  the ledger. `MoneyBudApp` only compares the resulting period with the one on screen, to say where
  it went. The screen does not move ([§12](12-glossary.md), *Defaults, and entering while another
  period is shown*).
- **Nothing on the screen holds a figure.** `Refresh` changes nothing. It tells the window that
  everything may have changed, and each binding reads `Overview` again, which `PeriodOverview.Of`
  works out afresh from the ledger. So a figure on screen cannot be stale against the domain after an
  act. The cost is that the Overview is recomputed on every read. At the demo's data sizes nobody
  can notice that. If it ever matters, the place to cache is once per `Refresh`.

**The same `Refresh` runs on a timer.** The Desktop calls it once a minute, with nothing else,
so that the *Huidige periode* label follows the clock when a period ends while MoneyBud is open
([§8.4](08-crosscutting-concepts.md)).

**The scenarios enter at `MoneyBudApp`**, not at the window. Every *When* step makes the same call
the form makes ([§8.4](08-crosscutting-concepts.md)). The window and the form's clearing are the
only parts of this diagram that no scenario runs through. The form's clearing has unit tests of its
own.

## Removing an entry: two presses, one call to the ledger

Since the corrections increment. Pressing *Verwijderen* on a loaded entry calls
`MoneyBudApp.AskToRemove`, which puts up a `Question` and holds the removal to be done beside it.
**Nothing reaches the ledger at this point**, and the Overview still lists the entry. The second
press answers the question. `Confirm` drops the question, calls `Ledger.RemoveExpense` or
`RemoveIncome`, empties the form if it still holds that entry, and announces the removal. `Decline`
drops the question, calls nothing, and says nothing. Stepping, loading another row, saving a change
or *Annuleren* drop a waiting question the same way, without calling the ledger. So the domain never
sees a removal the user did not confirm, and has no confirmation of its own
([§8.1](08-crosscutting-concepts.md), [§8.4](08-crosscutting-concepts.md)). The removal scenarios
run both presses, and check that the question is waiting while the entry is still listed.
