# Increment 12 — recurring entries: implementation plan

**Status:** written 2026-09-28, **approved by Axel at the second gate on 2026-09-28**: D1 as proposed,
D2 as recommended (version 4 read), and all ten readings under *Chosen in this plan* accepted.
**Built on 2026-09-28** (ADR 0011). During the build Axel ruled on one point, on the recommendation,
and the build follows the ruling rather than reading 7 as first written:

- **Reading 7, revised: in the order it happened.** Reading 7 put what MoneyBud did by itself in front
  of the act's own sentence. The approved increment-6 step "I should be told that the expense was
  changed" needs the notice to open with the change. So what settling did *before* an act still comes
  first, but the occurrences the act itself caused (a repeat set up in the past, started again, or its
  latest date moved back) come *after* the act's sentence. Rejected: occurrences always first, with the
  increment-6 step loosened to "contains".

**What it builds against:** the approved scenarios, `repeat-an-entry` and `change-a-repeat`, plus the
"Repeats are kept" section of `keep-data` (51 scenarios, 61 cases), **approved by Axel at the first
gate on 2026-09-28** together with every reading in their headers. And §12 *Recurring entries*, which
holds every ruling with its reasoning, including *Ruled at the scenario stage* and *For the plan*. Read
that section before starting. This plan does not restate it.

---

## Two decisions for Axel at this gate

### D1. How a recurring entry is kept, and how settling writes it (a new ADR 0011)

The rulings need four things that no single entry can hold (§12, *For the plan*):

- **The day it was last set to.** After 28 February, no occurrence's date says "the 31st" (ruling 5).
  Follow-up 3 moves that day only when the latest occurrence's *date* is changed, so a change has to
  tell a changed date from an unchanged one.
- **The next date.** It must survive the latest occurrence being removed (follow-up 1), and a day moved
  on the removed one stays moved.
- **Which entries are its occurrences.** An earlier occurrence opens locked (follow-up 2), and when the
  latest is removed the one recorded before it takes over (follow-up 1).
- **Whether it was stopped.** A stopped repeat's earlier occurrences stay locked, and its last one stays
  changeable (scenario-stage ruling 1). So a stopped repeat is still a repeat. It is not a set of plain
  entries.

**Proposed:**

1. **A recurring entry is state beside the entries.** It holds its **occurrences** (entry ids), its
   **frequency** (weekly, monthly, or none once stopped), the **day** a monthly one falls on, and the
   **next date**. The entries stay ordinary `Expense` and `Income` records, unchanged in shape. Nothing
   in the balance, budget or sweep figures needs to know whether an entry repeats.
2. **The latest occurrence is the one with the highest id, and it is not stored.** The id is the
   recording order (IEntry), so the highest id is "the one recorded most recently" by definition. When
   the latest is removed, the next-highest takes over by itself. Removing the last occurrence leaves
   the recurring entry with none, and it is dropped: the repeat ends (follow-up 1).
3. **Settling works through the days in order** (follow-up 5). Today, `Settle` steps from period
   boundary to period boundary. It will step from event to event instead. Each step takes whichever
   comes first: the earliest next date that is due, or the next period boundary. On a boundary day the
   boundary comes first (the period's end, its sweep, then the new period's planned money), and that
   day's occurrences come after it. So an occurrence dated in a period is always recorded before that
   period is swept. An occurrence already due when a recurring entry is set up, or when its latest
   date is moved back, comes before any boundary not yet passed. That is how follow-up 4's "at once"
   works: the act settles again after it has made its change.
4. **An occurrence is saved straight away** (ruling 7), as a sweep is. ADR 0009's accepted saving gap
   narrows again.

| Rejected | Why |
|---|---|
| **A frequency field on the latest entry only** | Loses the day after a short month, and the next date once the latest is removed. Those are the two things `keep-data`'s "Repeats are kept" section checks |
| **Occurrences worked out, not stored**: a rule that shows an entry for every date without recording one | Against ruling 2: an occurrence is an ordinary entry, removed or changed on its own, and counted in balances and in a balance correction by the moment it was recorded |
| **The latest stored as its own field** | A second source for what the ids already say, and the two could disagree after a removal |
| **A count of steps since the day was set**, instead of the next date | The same information, less readable in the file, and the scenarios talk in dates |

**Consequences:** file format **version 5**, with a `repeats` list. `Settle` loses its "once per day
at most" early return: a due occurrence can be dated on or before the day already settled through.
The new ADR records all of this. 0009 and 0010 get dated notes pointing to it, since settling gains a
third writer and a new order.

### D2. Data saved by the sweep version is read, not refused

Version 4 has no repeats, and that is exactly true of the data: nothing could repeat when it was
saved. So reading it as version 5 with no repeats guesses nothing. That is unlike versions 1 to 3,
where reading meant guessing something the rulings said must not be guessed. **Recommended: read
version 4.** Your current data file keeps working, and every entry in it is a one-off. The cost is one
branch in the reader and one unit test. The alternative is to refuse it as before: MoneyBud says it
cannot read the data and closes, and you delete the file.

---

## Chosen in this plan, for Axel to see at the gate

These are the plan's readings where the rulings and the scenarios leave a detail open. Each is the
smallest rule the scenarios pass with.

1. **How the next date is worked out.** Weekly: 7 days after the date before. Monthly: the kept day in
   the following month, or that month's last day if it is too short. When the day is set (by setting
   up the repeat, by changing the latest occurrence's date, or by changing its frequency), it becomes
   that date's day, and the next date is one step after that date. Nothing else sets it. A clamped
   date never does (§12, follow-up 3 read with ruling 5, approved at the gate).
2. **Changing the latest occurrence's frequency** also sets the day from its date, so weekly → monthly
   on 10 September gives 10 October. That is the approved derivation. **Stopping** clears the day and
   the next date. **Starting again** (the last occurrence of a stopped repeat set back to weekly or
   monthly) sets them from its date, and anything already due is recorded at once.
3. **Which entry can change the repeat.** A one-off, the latest occurrence of a running repeat, and the
   last occurrence of a stopped one. Every other occurrence opens on *Eenmalig*, locked. The domain
   **ignores** the frequency handed in for a locked entry. Changing one changes that entry alone.
4. **An entry open in the form when MoneyBud records the next one** (§12 leaves this to the plan).
   The tick's notice names the new occurrence, and the form, still open, now shows *Eenmalig*, locked,
   because the entry is no longer the latest. Saving it changes that entry alone, by rule 3.
   **The cost:** a new price or a stop typed into the form in the minute before the next occurrence
   is recorded applies to the old entry only, and has to be made again on the new latest row. The
   alternative was to empty the form, as stepping does, which loses what was typed.
5. **Saving unchanged** compares the frequency too, for an entry that can change the repeat: the
   loaded value handed back is unchanged, so saving says nothing. **Changing only the frequency** is a
   change, announced with the existing "Uitgave gewijzigd" / "Inkomen gewijzigd" text. A refused change
   touches nothing, the repeat included.
6. **Several occurrences due on one day** are recorded in the order their recurring entries were set
   up. The notice lists them in the order recorded.
7. **The notice.** One sentence, *"Herhaald: …"*, names every occurrence MoneyBud recorded by itself,
   whether settling recorded it before an act or the act caused it. It is followed by *"… is weer in
   gebruik."* for each category an occurrence brought back, then by the sweep sentences, and last by
   what the act itself says. What MoneyBud did by itself comes first, as the sweep did. Proposed copy,
   with dates so that four *Markt* at once do not read like a mistake:
   *"Herhaald: Netflix € 13,99 (25 september), Salaris € 2.500,00 (27 september)."* An expense with no
   label is named by its category. On the minute's tick, the notice **drops a waiting removal
   question** (approved reading).
8. **The drop-down's items are capitalised**: *Eenmalig*, *Wekelijks*, *Maandelijks*. The grey row
   label is lower-case, *maandelijks* / *wekelijks*, as ruling 6 writes it. `Tekst` holds both.
9. **The removal question on the latest occurrence of a running repeat** adds one sentence, proposed
   as *"De herhaling gaat door; zet hem op Eenmalig om te stoppen."* That is copy, because ruling 4's
   rejected option shows that removing could be mistaken for stopping. Every other removal question
   stays as it is.
10. **The grey label is not in an account's history** (scenario-stage ruling 3). `HistoryLine` gets no
    field for it, and a unit test holds that.

---

## Domain (`MoneyBud.Domain`)

**New types** (a new file, `Recurring.cs`)
- `Frequency` — `Weekly`, `Monthly`. One-off is `null` wherever a `Frequency?` is taken: "the absence
  of a repeat, not a third kind" (§12, derived).
- `RecurringEntry` (internal state) — occurrence ids, `Frequency?` (null once stopped), `Day` (a
  monthly one's day, 1–31), `Next` (null once stopped). One method sets the day and the next date from
  a date and a frequency (rule 1). One works out the date after `Next`.
- `OccurrenceMade` — the entry recorded, and the category it brought back, if any. Used for the notice
  (`TakeOccurrencesMade`).

**`Ledger` additions and changes**
- `RecordExpense(…, Frequency? repeat = null)`, `RecordIncome(…, Frequency? repeat = null)`: after
  recording, a non-null repeat makes a recurring entry from the new entry, and the ledger settles again,
  which records anything already due (D1.3). A refused entry sets nothing up.
- `ChangeExpense(…, Frequency? repeat)`, `ChangeIncome(…, Frequency? repeat)`: the unchanged check
  includes the frequency when the entry can change the repeat (rule 5). Once the change goes through,
  in this order:
  - a one-off set to repeat gets a new recurring entry;
  - on the latest: a change of frequency or of date sets the day and the next date from the new date
    (rules 1 and 2), and *Eenmalig* stops the repeat;
  - on a stopped repeat's last occurrence: a frequency starts the repeat again;
  - a locked entry ignores `repeat` (rule 3).

  Then the ledger settles again. Existing callers keep compiling: the new parameter comes last, with a
  default that means "leave the repeat as it is". `null` cannot mean that, because `null` is
  *Eenmalig*. So the parameter is a small `RepeatChoice?` wrapper or an overload. Which one is the
  build's call.
- `RemoveExpense`, `RemoveIncome`: take the id out of its recurring entry, and drop the recurring entry
  when no occurrence is left. The next date does not move.
- `FrequencyOf(IEntry)` → `Frequency?`: the latest occurrence of a running repeat gets its frequency,
  every other entry gets null. It is both the row's label and the value the drop-down loads.
- `SetsTheRepeat(IEntry)` → `bool`: whether the drop-down can be changed (rule 3).
- `Settle()`: event by event (D1.3). An occurrence copies the latest's amount, label, category,
  account and kind, is dated `Next`, takes the next id, brings an archived category back (ruling 8),
  is added to its recurring entry and to the occurrences made, and moves `Next` on. It is never checked
  or refused. It returns whether anything changed.
- `TakeOccurrencesMade()` → the occurrences recorded since last asked, oldest first, then forgotten,
  as `TakeSweepsMade`.
- `ToSnapshot` / `FromSnapshot`: the recurring entries. Checks on load:
  - every occurrence is an existing expense or income, and all of one recurring entry's are of one kind;
  - no entry belongs to two recurring entries, and none has no occurrences;
  - a running one has a frequency and a next date, and a stopped one has neither;
  - a monthly one has a day from 1 to 31, and a weekly or stopped one has none.

**Unit tests:**
- the date arithmetic: weekly; monthly on the 29th, 30th and 31st through February, in a leap year and
  a common one, and April;
- settling day by day across a boundary, with a sweep and with planned money (the order on a
  boundary day);
- several recurring entries due on one day;
- latest by id after a date is moved past the latest's;
- removing the latest, the only occurrence, and a stopped repeat's last;
- stop and restart;
- the lock ignoring `repeat`;
- unchanged with and without the frequency;
- a refused change leaving the repeat as it was;
- a repeat set up in the past;
- an archived category brought back;
- the clock turned back recording nothing;
- `TakeOccurrencesMade` draining once;
- every load check.

## Storage (`MoneyBud.Storage`)

`LedgerJson` version **5**: a top-level `repeats` list, each with `occurrences` (entry ids, ascending),
`frequency` (`"weekly"`, `"monthly"` or null), `day` (a number or null) and `next` (a date or null). If
D2 is approved, **version 4 is read** with no repeats, and a version-4 document that has a `repeats`
property is refused, since it is not what version 4 wrote. Versions 1 to 3 stay refused. **Unit tests:**
a version-5 round trip with a running, a stopped and a monthly-on-the-31st repeat; version 4 read;
version 3 refused; an unknown frequency word refused.

## Presentation (`MoneyBud.Presentation`)

- `ExpenseForm`, `IncomeForm`:
  - `Frequency` (`Frequency?`, *Eenmalig* by default), `FrequencyChoices` (the three, in the ruled
    order) and `CanChangeFrequency`;
  - `Load` sets them from `FrequencyOf` and `SetsTheRepeat`, `Clear` resets them, and `Record` and
    `Save` hand the frequency on;
  - on every `Refresh`, a form in *Wijzigen* re-reads `CanChangeFrequency` and, once locked, shows
    *Eenmalig* (rule 4);
  - **the list writes back**, like the account list: a plain value, a null write ignored. A write-back
    of the value already shown changes nothing, so it cannot turn an unchanged save into a change.
- `ExpenseLine`, `IncomeLine`: `RepeatLabel` (*maandelijks* / *wekelijks* / null), from `FrequencyOf`.
  Nothing on `HistoryLine`.
- `MoneyBudApp`:
  - `RecordExpense`, `RecordIncome`, `ChangeExpense` and `ChangeIncome` take the frequency;
  - **everywhere a sweep is taken, occurrences are taken too**: the constructor, `Tick`, `Tell`,
    `Refuse` and `SayNothing`. One private helper takes both from the ledger, builds the sentences in
    rule 7's order, and says whether to keep. Occurrences are **kept straight away** even when the act
    was refused or changed nothing;
  - `Tick` drops a waiting question when it has occurrences to tell, as it does for sweeps;
  - the `Notice` record gains the list of occurrences it names, as it carries `WentInto`, so a step
    can check what was named without parsing the sentence.
- `Tekst`: *Herhalen*, *Eenmalig*, *Wekelijks*, *Maandelijks*, *wekelijks*, *maandelijks*, the
  *Herhaald* sentence and the removal question's sentence (rules 7 to 9). Move the ruled terms from
  §12's *Proposed display terms for recurring entries* into *Dutch display terms* at the same time,
  since `TekstTests` reads that table.

**Unit tests:**
- the lock after a tick, for an entry open in the form;
- a write-back of the value shown changing nothing;
- the label on the latest row only, and not in the history;
- the notice's order, with an act's own sentence and a sweep;
- occurrences kept on a refused act;
- the question dropped on a tick;
- `Tekst`.

## Desktop (`MoneyBud.Desktop`)

A *Herhalen* ComboBox as the **last field** of the expense and income forms, after *Rekening*. It is
bound to `FrequencyChoices` and `Frequency`, and enabled by `CanChangeFrequency`. The grey label goes
in the Overview's rows beside the account name, in the same style. `WindowMarkupTests` holds the new
field order. **The window decides nothing.**

## Step definitions (`tests/MoneyBud.Specs`)

- **"today is 25 August 2026"** (a calendar date) makes a new empty ledger on that day, so the empty
  ledger counts as first started then (the binding note at the gate). It throws if anything was set up
  before it, as `StartUsingMoneyBudForTheFirstTime` does.
- **Calendar dates** in `SpecParsing` ("25 August 2026", and "tomorrow"): in "dated …", in the list
  tables' `date` column, and in the entry phrase "the expense labelled "X" dated D", which
  `CorrectionSteps` and the removal steps take unchanged.
- **", repeating monthly/weekly"** as an optional ending on every record step, Given and When,
  including "on the account …" and "without a label". The existing regexes are tightened so that the
  ending is not swallowed into a date or an account name.
- A new `RecurringSteps.cs`:
  - the drop-down's default and choices;
  - "the day becomes D while MoneyBud is open" (clock, then `Tick`);
  - "I close MoneyBud, and start it again on D";
  - the one-notice table, compared as a multiset against `Notice`'s occurrences, each also found in
    the text;
  - "MoneyBud should not have recorded any repeating entry": no occurrences in the notice, and no
    recurring entry has gained one since the step before. A `BeforeStep` hook takes the count;
  - "I change the frequency of … to …";
  - "… should open with the frequency X, changeable/locked".
- **The list steps** take an optional `repeats` column. A table without it does not check it.
- Every `When` acts through `MoneyBudApp`, and every `Then` reads the screen where the screen shows it.

## Order of work

1. Domain, with its unit tests.
2. Storage, then the full suite green except the new scenarios.
3. Presentation, then step definitions, until the whole suite is green with 0 warnings.
4. Desktop, then a **headless run of the real window**. Check that:
   - the *Herhalen* list writing back on first show, on load, and after a tick says and saves nothing;
   - the drop-down locks on an earlier occurrence;
   - the label sits beside an account name;
   - the notice appears after a tick that records an occurrence.
5. `spec-reviewer`, fixing what it finds.
6. Documentation:
   - **ADR 0011** (D1, with D2 in its consequences), §9's index, and dated notes on ADR 0007, 0009 and
     0010;
   - arc42: §5, §6 (settling day by day), §8.1, §8.3 (the file format), §11, and §12's "not built"
     notes and *Chosen in the build*;
   - the arc42 README, `features/README.md` (files and counts), the README if D2 changes what it says
     about old data, and `CLAUDE.md`.
7. Axel tries it, then merge into `main`.

## Watch out for

- **Every path in `MoneyBudApp` that calls the ledger must end in `Tell`, `Refuse` or `SayNothing`** (or
  be `Tick` or the constructor). Settling may now record occurrences inside any act, and they must be
  said and saved, as sweeps must.
- **An act that sets up or moves a repeat settles again after its change**, or what is already due waits
  for the next tick and is said in a separate notice.
- The id is still the recording order across every kind. "The latest" now leans on it too.
- `SetBacking` and `SetSweepDestination` to the value already set stay complete no-ops. The new
  *Herhalen* list writes back like theirs, and needs the same care.
- `HasBudget` stays unused. An occurrence is not a *Budget* and does not touch the offer of a plan.
