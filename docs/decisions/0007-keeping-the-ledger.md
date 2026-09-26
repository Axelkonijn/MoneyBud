# 0007 — Keeping the ledger: one JSON file in the user's profile, in a fourth project

**Status:** Accepted
**Date:** 2026-09-26
**Amends:** [ADR 0006](0006-three-source-projects.md). It adds a fourth source project, `MoneyBud.Storage`,
beside the three that record set out. Nothing 0006 decided is reversed: the domain, the presentation
layer and the desktop keep the roles and boundaries it gave them. 0006 carries a dated note that
points here.

## Context

From its first increment until 2026-09-26, MoneyBud kept nothing. [§8.3](../arc42/08-crosscutting-concepts.md)
recorded that as a deferral with a trigger. On 2026-09-26 the stakeholder chose to build persistence
next without the trigger having fired ("demo now, real soon"), and settled what it must do
([§12](../arc42/12-glossary.md), *What MoneyBud keeps*):

- everything is kept, as one history, saved automatically after every change;
- one set of data, in a fixed place in the user's profile, never in the repository;
- an interrupted save never damages the previous one;
- a failed save is said and the user carries on, and one save that works catches up every failure;
- data that cannot be read is left untouched, and MoneyBud closes;
- only one MoneyBud at a time;
- no backups kept by MoneyBud, whose file the user backs up himself;
- demo data need not survive a new version, at least up to and including the accounts increment.

Those rulings are requirements. They left four technical questions open, and §8.3 listed them for
the plan: the form storage takes and the exact folder, how amounts are stored, how identity is
stored, and where storage sits in the solution. The corrections increment had sharpened the third.
Since renaming, a category's name is not its identity, and an entry's id was a counter that started
again at every run.

This record holds the answers. They were proposed in the persistence increment's implementation plan
and **approved by the stakeholder at the plan gate on 2026-09-26**.

## Decision

### 1. One JSON file, `moneybud.json`, written whole on every save

The ledger is kept as one JSON document, written with `System.Text.Json`, which is part of the
runtime, so no package is added. **Every save writes the whole ledger.** A save never writes only
the last change.

```json
{
  "format": "MoneyBud",
  "version": 1,
  "lastEntryId": 2,
  "categories": [ { "key": 1, "name": "Boodschappen", "archived": false } ],
  "budgets":    [ { "category": 1, "periodStart": "2026-03-01", "cents": 40000 } ],
  "expenses":   [ { "id": 1, "cents": 3215, "date": "2026-03-15", "category": 1, "label": "Albert Heijn" } ],
  "incomes":    [ { "id": 2, "cents": 183245, "date": "2026-03-15", "label": "Salaris" } ]
}
```

(Synthetic example, from `LedgerJson`'s own doc comment.)

- **Amounts are whole cents, as JSON integers.** That is exactly what `Money` holds
  ([ADR 0003](0003-money-representation.md)), so nothing is parsed from decimal text and nothing can
  round.
- **Dates are `yyyy-MM-dd`**, with no time and no zone.
- **The document names its format and version**: `"format": "MoneyBud"`, `"version": 1`.
- **Reading is strict.** A blank document, broken JSON, another format, another version, a fraction
  of a cent, or cents written as text all make the file **unreadable**. It is not read in part, and
  nothing is rounded or repaired. Properties this version does not know are ignored.
- **Text is written unescaped where it can be**, so an accented name reads as itself ("Één keer")
  and not as `Één`. The file is UTF-8 without a byte-order mark, and bytes that are not
  UTF-8 make it unreadable.

### 2. In `%LOCALAPPDATA%\MoneyBud`, never relative to the working directory

The folder is `Environment.SpecialFolder.LocalApplicationData` plus `MoneyBud`
(`FileLedgerStore.DefaultFolder`). On Windows that is `%LOCALAPPDATA%\MoneyBud`. macOS and Linux
follow .NET's own mapping of that folder, which the root README lists. Neither has been run
([§7](../arc42/07-deployment-view.md)).

`.gitignore` lists `moneybud.json`, `moneybud.json.tmp` and `moneybud.lock`, as a second line of
defence. It is not the protection.

### 3. A save writes a temporary file, flushes it, and renames it over the data file

`FileLedgerStore.TrySave` writes `moneybud.json.tmp`, flushes it to the disk, and then moves it over
`moneybud.json` with `File.Move(..., overwrite: true)`. A `.tmp` left behind by a save that was cut
off is **never read**, and the next save writes over it. A save that fails reports `false`, and the
data file is as it was.

### 4. One MoneyBud at a time, by an exclusive lock on `moneybud.lock`

`FileLedgerStore.TryClaim` opens `moneybud.lock` with `FileShare.None` and holds it until the store
is disposed, which `MoneyBudApp.Close` does. The operating system lets go of it when the process
ends, however it ends, so a crash never leaves the data held. **The lock is claimed before the data
is loaded**, so a second start never reads a file that the first is part-way through writing.

A claim has three outcomes: `Claimed`, `HeldElsewhere`, and `Unreachable`. `Unreachable` covers a
folder that cannot be made or opened: no profile folder, access denied, or a *file* standing where
the folder should be. It is met exactly like data that cannot be read (§12, ruled 2026-09-26 during
review; see *Why*).

### 5. Identity: entry ids are kept; categories get a key that exists only in the file

- **An entry keeps its id**, and the file keeps `lastEntryId`, the last id issued. So an id is never
  issued twice, not even for an entry removed before the save.
- **A category is given a key in the file**: its position in the order added, counted from 1,
  worked out afresh by every save. Budgets and expenses refer to that key, **never to the name**.
- **The domain gained no category id.** `Category` keeps the object identity the corrections
  increment gave it ([§8.1](../arc42/08-crosscutting-concepts.md)). The key needs to hold only within
  one file, because every file is the whole ledger.
- **Lists are kept in the ledger's own order**: categories in the order added, entries in the order
  recorded. The Overview breaks ties by that order, and lists newest first by it, so the order is
  part of what is kept.
- **`Ledger.FromSnapshot` checks kept data against every rule the running ledger keeps**, and
  throws `InvalidDataException` for any that is broken. Examples: a name not stored as the name
  rule stores it, two names the rule counts as one, a key that points at nothing, a negative budget
  or one not on a period's first day, an entry of zero or less, an income without a label, an id
  used twice or beyond `lastEntryId`. `MoneyBudStart` turns that into "cannot read". Dates are not
  checked against today.
- **The period start day is not stored.** The calendar is fixed at the 1st
  ([§11](../arc42/11-risks-and-technical-debt.md), the start-day row).

### 6. A fourth project, `MoneyBud.Storage`, behind a port in the domain

| Project | Holds for keeping data | References |
|---|---|---|
| **`MoneyBud.Domain`** | The plain-data **`LedgerSnapshot`**, `Ledger.ToSnapshot` and `Ledger.FromSnapshot`, and the **`ILedgerStore` port**: claim, load, try to save, let go. No file, no JSON | The base class library only, as before |
| **`MoneyBud.Storage`** (new) | **`FileLedgerStore`**, which implements the port on the file system, and **`LedgerJson`**, the format | `MoneyBud.Domain` only. No package |
| **`MoneyBud.Presentation`** | **All the behaviour**: `MoneyBudStart.Start`, which decides whether MoneyBud opens, and in `MoneyBudApp` when to save, the save line, the retry and the last attempt on closing | `MoneyBud.Domain`. **Not** `MoneyBud.Storage`: it knows a store only through the port |
| **`MoneyBud.Desktop`** | Wiring only: makes a `FileLedgerStore` on `DefaultFolder`, hands it to `MoneyBudStart`, and shows either the window or a small message window with the refusal's text | Adds `MoneyBud.Storage` |
| **`MoneyBud.Specs`** | Runs every scenario against the real `FileLedgerStore`, in a temporary folder of the scenario's own | Adds `MoneyBud.Storage`. Still not the Desktop |

## Why

### A JSON file, not SQLite

**SQLite was the alternative, and it was rejected on three counts.**

- **It needs a package**, where `System.Text.Json` is already part of the runtime.
- **It saves change by change, and the rulings want the opposite.** "One save that works catches up
  every failure" is true by construction when a save is the whole ledger. With a database, a failed
  save leaves some changes written and some not, and catching up means tracking which.
- **The file would be opaque to the person who backs it up.** Backing up is the user's business, and
  the README tells him which file to copy. A JSON file can be opened and checked by eye. A database
  file cannot.

Writing everything every time costs more than writing a change, and grows with the history. At a
demo's size, and at a household's for years, that is a small file. If it ever matters, the cost is
visible (*Consequences*).

### Strict reading, because a wrong read is worse than no read

A file that is half-understood is the dangerous case. MoneyBud saves automatically, so whatever it
read in is what it writes back at the first change. Reading leniently, by rounding a fraction of a
cent, skipping an entry that does not parse, or guessing a newer version's meaning, would turn a
damaged file into a quietly different history. Reading strictly turns it into the ruled response:
say so, touch nothing, close. So strictness serves the ruling. It is not caution for its own sake.

`Ledger.FromSnapshot` re-checks the domain's rules for the same reason. The file lives outside
MoneyBud and can be edited there. A ledger that holds a state its own rules forbid is a ledger whose
figures nothing else in the code was written to expect.

### The version field, while no version need read another's data

Until the switch to real use, a new version need not read an older one's data (§12, *Demo data may
not survive a new version*). The version field is what lets it *know* it cannot. Without the field,
a changed format would be met by whatever the parser happened to make of it. With the field, it is
met by "cannot read", which is the ruled response. The field also leaves the door open for the day
carrying data across versions becomes a requirement.

### Local, not Roaming, and never the working directory

The rulings fix a place in the profile. **Roaming** was the other profile folder. It was rejected
because roaming profiles copy their contents between machines at sign-in and sign-out, and a file
rewritten after every change is the wrong thing to sync that way. A second reason, this record's
reading rather than the plan's: roaming would quietly be a first step towards sync, which
[§3.2](../arc42/03-context-and-scope.md) defers and [§4](../arc42/04-solution-strategy.md) says must
be an explicit trade.

**The working directory was never an option.** `dotnet run --project src/MoneyBud.Desktop` runs
inside a working copy of a public repository, so a relative path would put real data exactly where
it must never be ([§2](../arc42/02-architecture-constraints.md)).

### Temporary file and rename, because it is the whole guarantee

"An interrupted save never damages the previous one" is met by never writing the data file in place.
Until the rename, the data file is the previous save, whole. After it, it is the new one, whole. The
flush comes before the rename, so the rename never exposes a file whose bytes are still in a cache.
There is nothing to recover on the next start, which fits the ruling that the next start says
nothing.

### A lock file, claimed before loading

Two MoneyBuds saving one file would each overwrite the other's saves with their own whole ledger.
An exclusive open is the simplest lock the operating system releases by itself on a crash. That
matters because a stale lock that outlived a crash would refuse every later start. Claiming before
loading closes the window in which a second start could read a file the first is half-way through
replacing.

**A folder that cannot be reached is met as unreadable data**, which was put to the stakeholder
during review and ruled on 2026-09-26 (§12, *When the data's folder cannot be reached*). The rejected
option was to start empty and show "not saved". If the real folder came back while MoneyBud was
open, its first successful save would write the empty start over the real history.

### A key in the file rather than an id in the domain

A category needed a stored key, because keying by name would turn a rename into a broken link or a
silent reassignment (§8.3). **The key could have lived in the domain**, as a `Category.Id`. It
does not need to. The domain already has an identity for a category, the object, and every file is
written whole from one ledger. So a key made fresh at each save, from the order added, is enough to
link budgets and expenses within that file. An id in the domain would be a second identity that
nothing in memory needs, kept only for the file's sake.

**Entry ids are different**: the domain already issues them, so keeping them costs nothing.
Keeping `lastEntryId` as well means an id removed before the save is not issued again after a
restart. No rule depends on that today, but an id that silently comes back is the kind of thing
that is expensive to discover late.

### A fourth project, behind a port in the domain

[§5](../arc42/05-building-block-view.md) named adding storage as the moment to re-examine the layout.
ADR 0004 had named the signal to watch for: the domain starting to accumulate things that are not
domain. **A file format and a file system are not domain**, so they do not go in `MoneyBud.Domain`.
They are not presentation either, and putting them in `MoneyBud.Presentation` would give the
toolkit-free layer a file system to reach, making the scenarios depend on how a file is written
rather than on what is kept.

So storage is a project of its own, and it depends on the domain only. The **port**, `ILedgerStore`,
sits in the domain beside the snapshot it carries. So the presentation layer, which decides *when*
to save and *what to say* about it, depends on nothing it does not already reference. The Desktop is
the one place that knows which store is real, and choosing it is wiring, not a decision.

The alternatives:

- **Storage inside the presentation layer.** One project fewer, but the layer the scenarios run
  against would hold file handling, and the rule that it decides what the screen shows would no
  longer describe it.
- **Storage inside the Desktop.** The file handling would be out of reach of every automated test,
  since the Desktop has none by plan ([ADR 0006](0006-three-source-projects.md)). The atomic save
  and the lock are exactly the parts that most need holding.

### The scenarios use the real store

Every scenario keeps its data through the real `FileLedgerStore`, in its own temporary folder, and
nothing in product code knows it is being tested. A fake store would have tested the screen's
decisions and left the file untested, which is the half where a mistake loses data. Saving is made
impossible by putting a directory where the temporary file goes. That is a real failure the store
has to meet, not a flag in the code.

**One scenario is a simulation, and was approved as one.** "MoneyBud is interrupted while saving"
cannot cut a real save part-way. The step rebuilds the state such a cut leaves on disk, by the
store's own design: the previous file, and half of the new one in the temporary file. Then it drops
MoneyBud without closing. That the store never writes the data file in place is held by
`StorageTests`, not by the scenario.

## Consequences

- **Every act that changes the ledger writes the whole file and flushes it to the disk.** The cost
  grows with the history, which is kept for good. It is unnoticeable at the demo's size. If it is
  ever noticed, the first thing to question is the flush per save, and the second is the whole-file
  write. Changing the second would reopen "one save catches up everything", and with it this record.
- **The file is readable, and so is editable.** A hand edit that breaks a rule is refused on the
  next start, as unreadable, rather than loaded. A hand edit that keeps every rule is loaded as if
  MoneyBud had written it, because nothing can tell the two apart.
- **A change of format is a new version number**, and until the switch to real use an older file is
  simply unreadable to the new version. From the switch on, reading older versions becomes a
  requirement, and the version field is where that starts.
- **The start day is not in the file.** If a configurable start day is ever built, a file saved
  under one calendar would, under another, hold budgets on days that start no period. It would be
  refused as unreadable rather than misread, which is safe, but it is not the answer §11's start-day
  row asks for.
- **`moneybud.lock` stays on disk after closing.** It is an empty file, and holding it open is what
  locks, not its existence. It is harmless, and the README does not mention it.
- **`MoneyBud.Domain` still references only the base class library, and `MoneyBud.Storage` adds no
  package.** ADR 0004's warning about the first package reference added to the domain is still
  unspent.
- **The Desktop is still deliberately thin.** It gained wiring (making the store, choosing a window)
  and one small message window whose text comes from `StartResult.Refused.Text`. It decides nothing
  about starting, saving or what is said ([§11](../arc42/11-risks-and-technical-debt.md)).
- **A second test reads the window's markup**: `WindowMarkupTests` holds that the save line is a
  sibling of the notice and the question, so that it stands beside them rather than in their place.
  That widens the exception recorded in ADR 0006's note of 2026-09-26, and it was approved at the
  plan gate on the same terms.
- **[§6](../arc42/06-runtime-view.md) gains start-up, saving and closing**, since four building blocks
  now collaborate in them.
- **Reversal is cheap while the data is demo data.** Replacing the format or the store changes one
  project and the version number, and costs the user a fresh start, which he has ruled acceptable at
  least up to and including the accounts increment.
