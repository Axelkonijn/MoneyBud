# Increment 14 — MoneyBud on the phone: implementation plan

**Status:** written 2026-09-30. **Not approved at a gate, and will not be**: Axel waived both gates for
this increment only ("Beide vervallen; ik beoordeel achteraf"). The plan is written as always, the
build follows it straight away, and the plan, the scenarios and the app are reviewed together at the
end. Every decision below was taken without him, and is numbered so it can be answered by number.

**Accepted by Axel on 2026-09-30**, after a few hours on the phone and one round of two changes
([beoordeling ronde 1](../stakeholder/2026-09-30-telefoon-beoordeling-1.md)): the decisions below stand
as built, with the budget's half-way stop removed at his ruling. **File format version 7 is the first
promised version** (ADR 0014, decision 1). B8 and B12, the phone's extra figures, are to be **added to
the desktop at a later moment**, his ruling; until then they are a known gap in parity.

**What it builds against:** arc42 §12 *MoneyBud on the phone* (every ruling, with its reasoning, and
its *For the plan* list, which this plan answers point by point), §8.5 *Drawing on the phone*, ADRs
0013 and 0014, the prototype's README (its four Avalonia traps bind the build), and this increment's
scenarios (listed under *Scenarios*, below). Read §12's section before this plan. It is not restated
here.

**In one line:** a second Avalonia head, for Android, over the same `Domain`, `Presentation` and
`Storage`, laid out and animated as the approved prototype was, with every act the desktop has, and
two small additions to the shared layers: the phone's navigation rules and its settings, where the
tests reach them.

---

## Decisions taken without Axel

### D1. Three projects for the phone; the Android one is built on its own

ADR 0013 decision 6 leaves the project layout to this plan.

| Project | What it is | In `MoneyBud.slnx` |
|---|---|---|
| `src/MoneyBud.Phone` | The phone's screens, the two themes (colours, ring painters, the slab painter, fonts, the wood textures), motion. Everything but Android itself, so it also runs on the PC. The prototype's `MoneyBud.Prototype`, carried over and wired to `MoneyBudApp` | Yes |
| `src/MoneyBud.Phone.Android` | The Android host: the activity, where the data and settings live, vibration, signing, the backup opt-out. As small as the prototype's | **No** |
| `src/MoneyBud.Phone.Desktop` | The phone in a phone-sized window on the PC, on a data folder of its own, for developing without the phone; and, with `snapshot`, pictures of every screen from a headless run, for checking without the phone | Yes |

**Why the Android host is not in the solution:** building it needs the `android` workload and JDK 21,
and adds a minute or more to every `dotnet build` and `dotnet test`. Everything that can break when
the shared layers change is in `MoneyBud.Phone`, which the solution builds. The host is built with
its own command (in the README), and every build of this increment is checked with it.

**Rejected:** putting the phone's screens into the Android project (they could not be run or pictured
on the PC, and the phone is not connected); a separate solution for the phone (one codebase is the
ruling, and the shared layers would not be built with the phone).

### D2. The phone's navigation rules are decided in `MoneyBud.Presentation`, as `PhoneScreen`

ADR 0013 decision 4 says the phone head decides nothing, and leaves to the plan which of the phone's
interaction rules count as decisions. **All the ruled ones do**, and are held by one toolkit-free class,
`PhoneScreen`, over `MoneyBudApp`, with developer unit tests (the precedent for window behaviour: the
field order and the emptying category box were unit tests, not scenarios):

- **Which panel is open, and at which step**: none, income, expenses, budget or accounts; the list or
  the form (income, expenses); the list or an account's history (accounts); half or full (budget).
- **The chosen slice.** Touching the ring points at the slice in the finger's direction (the share
  the view hands over, as the desktop's `PointAt`); it **stays chosen when the finger lifts**; a tap
  (no slide) on the slice already chosen, or on the ring's centre, lets go; a tap anywhere else on the
  home screen lets go.
- **The budget panel's subject**: the chosen category slice, or a row tapped in the budget list;
  otherwise the whole list. Tapping a row chooses its slice, when it has one.
- **Android's back button**: first lets go of the budget's category, then goes back one step (form to
  list, history to accounts), then closes the panel; on the home screen with nothing open it is not
  handled, and Android does what it does.
- **What an act does to the panel**: a new entry that goes through closes its panel, so the slice
  grows where it can be seen (the prototype's behaviour, approved with it); a change saved, or an
  entry removed, goes back to the list; a refusal stays on the form. (A transfer or an account added
  closes its window by itself: its form closes when it goes through, as on the desktop —
  `TransferForm.IsOpen`, `AccountForm.IsOpen` — so `PhoneScreen` has nothing to decide there.)

The view mirrors `PhoneScreen` with its springs: a drag moves a spring, and where it comes to rest is
reported back (`Settled`), so a swipe and a tap end in the same state.

### D3. The phone's settings are a small file of their own, in the app's private folder

§12 rules that the theme and *Weergave* are remembered in the app's settings, not in the data file,
and derives the same for the hints. **Built as `PhoneSettings`** in `MoneyBud.Presentation` (theme,
*Weergave*, hints shown), which turns itself into a few lines of JSON text and back, and **a
`SettingsFile`** in `MoneyBud.Storage` that reads and writes that text, saved whole through a
temporary file as the data is. `PhoneSettings` is handed the file's read and write as two functions,
so neither project takes a dependency on the other.

- **Where:** the app's private folder (Android's `FilesDir`), **not** the folder copied over USB, so
  that folder holds the data file and nothing else.
- **Settings that cannot be read** (damaged, blank, an unknown theme name) are passed over without a
  word: *Standaard*, *Systeem*, hints shown. Settings are about the device and never stop MoneyBud.
- **A first start:** *Standaard*, *Systeem*, and the hints shown.
- **Written on every change**; a failed write says nothing and is tried again with the next change.
  The worst it costs is the theme on the next start.

**Rejected:** Android's own `SharedPreferences` (untestable here, and the desktop runner would need a
second way); putting them in the data file (the ruling).

### D4. Going to the background, and coming back

§12 *Android's lifecycle* rules that the last attempt to save is made when MoneyBud goes to the
background, and leaves the rest to the plan.

- **`MoneyBudApp.GoToBackground()`**: if something is unsaved, one more attempt; otherwise nothing is
  written. It asks nothing, keeps the store, and **MoneyBud stays open**: going to the background is
  not closing.
- **`MoneyBudApp.ComeBack()`**: **looks again at once**, exactly as the minute's `Tick` does
  (retrying a failed save, settling, recording what came due, sweeping, moving *Huidige periode*), so
  after a night in the background the screen is right the moment it is seen, not up to a minute
  later.
- **The minute's timer runs only in the foreground.** Stopped on going to the background, started
  again on coming back.
- **The lock is still claimed on the phone.** Android runs one instance, so *MoneyBud is al geopend*
  cannot be reached there, but the store is the same code as the desktop's and claiming costs
  nothing.
- **Unreadable data:** the message shows with *OK*; tapping it, or the back button, closes the app.

### D5. Where the data lives, the package name, and the key

ADR 0014 decisions 3 and 4 leave these to the plan.

- **The data file is `Android/data/app.moneybud/files/moneybud.json`**, straight in the folder
  Android's `GetExternalFilesDir(null)` gives, with its lock file and, while a save runs, its
  temporary file beside it. The same `FileLedgerStore` as the desktop's, handed that folder.
- **The package name is `app.moneybud`**, fixed from this build on. **A Debug build is
  `app.moneybud.debug`**, a different app with a folder of its own, so a build signed with Android's
  debug key can never be installed over the real one, nor its data mistaken for the real data.
- **The key:** none existed, so one is made in `%USERPROFILE%\MoneyBud-signing\`, outside the
  repository, with its password in a file beside it and a `signing.props` that the Android project
  imports. **A Release build refuses to build without it**, rather than falling back to the debug key,
  since a build signed with any other key cannot be installed over the real one without uninstalling,
  which deletes the data. Nothing about the key is in the repository except the path it is looked for
  at.
- **Automatic backup is off**: `allowBackup="false"`, and data extraction rules that exclude every
  folder from both cloud backup and device-to-device transfer, since on Android 12 and later the flag
  alone still allows the transfer. "De gegevens blijven alleen op de telefoon."
- **The file format stays at version 7.** Nothing new is kept in the data file (the settings are
  not in it), so the phone writes what the desktop reads. Version 7 is then the first promised
  version, if Axel accepts this build (ADR 0014 decision 1).

### D6. The phone draws the shared ring, and colours a slice by its place, as the desktop does

The prototype worked out its own slice sizes and gave each category a fixed colour. The app draws
**`PeriodOverview.Ring`**: its shares (the 2% minimum included), fills and markers. So the ring is
decided once, for both heads. The finger's direction becomes a share of the ring, and pointing is
`MoneyBudApp.PointAt`, as on the desktop. **A slice's colour follows its place in the ring**
(`CategoryRow.SliceIndex`), as the desktop's does, so a category's colour can change when budgets
reorder it. The prototype's fixed colour per category is not carried over.

### D7. The Dutch the phone adds lives in `Tekst`

All the Dutch stays in one place, which both heads share. §12's *Proposed display terms for the
phone* move into the *Dutch display terms* table, and `TekstTests` holds them: *Instellingen*,
*Weergave*, *Systeem*, *Donker*, *Licht*, *Thema*, *Standaard*, *Kintsugi*, *Aanwijzingen opnieuw
tonen*. **Klaar** is a control word, like *Opslaan* and *Sluiten*, and so not in the table. The
phone's headings and hints (*Nieuwe uitgave*, *Gisteren*, *Veeg nog eens …*) are copy.

### D8. How the desktop's acts appear on the phone

The prototype showed the layout, and faked about half the acts. Where it lacked one, or faked it with
different words, the app follows the desktop's rule in the prototype's style. These are layout, not
rulings, and none changes what an act does:

1. **Dates** are chips: *Vandaag* (the form's empty date, which means today), *Gisteren*, and
   *Andere datum…*, which opens a calendar. An expense's calendar ends at today; an income's does not.
2. **Accounts and *Herhalen*** are chips, as in the prototype; *Herhalen* is greyed and fixed on
   *Eenmalig* for an earlier occurrence, as the desktop's list is.
3. **The category box** is a text field with the suggestion chips under it, narrowing as the desktop's
   list does (`MoneyBudApp.SuggestionsFor`), so all three routes back for an archived category stay
   reachable by typing.
4. **Assigning** keeps its own period, stepped with ‹ › inside the assign card, as on the desktop.
5. **A category's acts** — *Hernoemen*, *Archiveren* (while in use), *Verwijderen* (only with no
   history) — sit under its ⋯ menu; renaming opens a small window with the name in a text box.
   *Staat op* and *Restant naar* open a list to choose from, with "—" for none, the desktop's symbol,
   where the prototype said *Nergens*.
6. **An account's acts** — *Hernoemen*, *Maak hoofdrekening*, *Saldo corrigeren*, *Verwijderen* (only
   while unused) — sit under the ⋯ menu of its history.
7. **In an account's history**, tapping a transfer opens it in the transfer form, to change or remove;
   tapping a starting balance or a correction asks straight away whether to remove it; an income,
   expense or movement does nothing there, as on the desktop.
8. **The notice** is the bar from below, a refusal in the warning colour; **the question** stays in
   it until answered, with its own confirming word and *Annuleren*; **the save line** is a small line
   under the period, in the warning colour, and in an open panel's header, as ruled.
9. **The period's name** opens *Periode begint op* (a grid of days 1 to 31) on the current and later
   periods; on an earlier one tapping it does nothing, as the desktop shows no list there.
10. **The ended period's line** and *Restant bijwerken* sit where *Restant naar* is, in the budget
    panel.

### D9. No automated test of the phone head; a headless run is the check

The phone head decides nothing (D2), so it is untested by plan, as the desktop is. **The check is a
headless run of the real screens** (`MoneyBud.Phone.Desktop snapshot`), which drives each panel with a
pretend finger over a real `MoneyBudApp` on synthetic data and saves a picture of it, and a Release
build of the Android app. What only the phone can show (§8.5) is first seen at the review.

### D10. The toolkit speaks Dutch on the phone too

The phone app fixes the thread culture to nl-NL at start, as the desktop's `Program` does, for the
calendar's month and day names.

---

## Chosen in the build, not in this plan

Numbered on from the plan's decisions, for answering by number. None changes a ruling.

- **B1. MoneyBud does nothing by itself in the background** (chosen in the scenarios): the minute's
  look waits for coming back, since a notice said while nobody looks would never be seen, and a sweep
  is said only once. `Tick` does nothing between `GoToBackground` and `ComeBack`.
- **B2. The phone ending MoneyBud after a failed background attempt loses the changes not saved**, as
  closing does when its last attempt fails (derived in the scenarios).
- **B3. The hints count as shown the moment they are shown**, at the opening; *Aanwijzingen opnieuw
  tonen* shows them now, not again at the next start; and settings missing while the data exists (a
  file copied onto a fresh install) count as unreadable: hints shown, *Standaard*, *Systeem*.
- **B4. Choosing a theme or *Weergave* says nothing**: the screen itself changes.
- **B5. Android's "background" is `onPause` and "back" is `onResume`**, the earliest moments Android
  gives, rather than `onStop`/`onStart`. A pause that is not a real leaving (a system dialog over
  MoneyBud) costs one save attempt only if something was unsaved, and one look on return.
- **B6. The unreadable-data message ends the whole app** on *OK* or back, so the next start reads the
  file afresh (after Axel has fixed or replaced it over USB).
- **B7. Leaving an entry's form lets go of an entry being changed** (a new one being typed stays); so
  a second swipe on the list always opens a form for a new entry. The desktop's *Annuleren* on a
  changed entry is this, on the phone.
- **B8. The phone shows the period's income total and expense total** at the head of its lists, and
  the income total under *Niet toegewezen* in the ring ("van € 2.570,00 inkomen"), as the approved
  prototype did. **The desktop shows neither**: a difference in figures between the heads, flagged
  rather than hidden. Both totals are worked out in `PeriodOverview`, not in the phone head.
- **B9. Over budget, over-assigned, overdrawn** carry the desktop's badges with their words (*Over
  budget*, *Te veel toegewezen*, *Rood*, *Tekort*) wherever there is room — the ring's centre, a
  category's page, the accounts — and the prototype's small round "!" marker in the dense category
  rows.
- **B10. No app icon of MoneyBud's own yet**: Android shows its default. The prototype had none.
- **B11. The signing key** is `%USERPROFILE%\MoneyBud-signing\moneybud.keystore`, alias `moneybud`,
  RSA 4096, valid until 2126, SHA-256 fingerprint
  `62:1D:10:6D:5F:A6:FF:CD:FC:D0:A8:8A:63:7A:59:53:16:5E:1C:C4:45:FE:EF:5A:67:F7:36:2A:86:5E:D4:9A`.
  Its password is in `password.txt` and again in `key-password.txt` beside it (Android's signing tool
  reads a password file once per password), with a `README.txt` saying what the folder is.
- **B12. A category's own page lists its expenses in the period**, tapping one opening it to change,
  as the approved prototype did. **The desktop has no such list**: a second phone-only addition,
  flagged like B8, worked out in `PeriodOverview.ExpensesOn`.
- **B13. Words the desktop never shows**, all copy in `Tekst`: *Gisteren* (a date chip), *Saldo
  vandaag* (over a history), *Deze periode*, *Toewijzen aan …*, *Aan het eind van de periode*, the
  hints, and the panels' headings.
- **B14. The back button first says no to a waiting question**, before anything else it does.
- **B15. The date chips work out *Gisteren* from the ledger's today, and an expense's or a transfer's
  calendar ends at today**, left in the phone head as layout: the domain still refuses a future
  expense or transfer by itself.
- **B16. Android 12 or later, 64-bit ARM only**, as the prototype was built; his phone runs Android 16.

### Axel's review, round 1 (2026-09-30)

After a few hours on the phone ([beoordeling ronde 1](../stakeholder/2026-09-30-telefoon-beoordeling-1.md)),
two points, both built the same day:

- **The ring's centre flickered and shrank slightly at every pull.** A defect: every panel coming to
  rest redrew the screen, which rebuilt the centre and played its settle. The centre is now rebuilt
  only when what it shows changes, and settles only when the finger moves to another slice.
- **The budget opens in one pull, all the way**, like the other three panels, and the ring no longer
  shrinks under it. His ruling; the half-way stop is gone, and with it the budget's second step
  (`PanelStep.First` is now the full sheet).

### What `spec-reviewer` found, and what came of it

No faked or vacuous step, no money defect, and every act and figure of the desktop has its phone
counterpart. Fixed, each with a test that a mutation showed fails without the fix:

- **The phone's totals and headings were held by no test**: now `PhoneOverviewTests`.
- **The hints were counted as shown even when MoneyBud refused to open**, so a first start on an
  unreadable file used them up. They are now counted when the home screen opens
  (`PhoneSettings.HomeScreenOpened`), with a scenario added to `choose-how-moneybud-looks.feature`.
- **A rebuilt budget page stayed alive**: every field listened to the shared form for good. A field
  now listens only while it is on screen.
- **Rules and words that had crept into the phone head**, moved out or removed: the category page's
  expense filter (`PeriodOverview.ExpensesOn`), "Staat op …" and the ring centre's figures (`Tekst`);
  and three rules the desktop does not have were dropped: hiding the suggestion that matches what is
  typed exactly, dimming a row with nothing planned or spent, and heading the budget *Te veel
  toegewezen* where the desktop keeps *Niet toegewezen* and adds the badge.

Left as they are, and noted: three scenarios in `choose-how-moneybud-looks.feature` hold by
construction (the settings have no way to the data), which records the design rather than tests it;
and the wiring of the phone's folders (settings not in the USB folder) is head code, untested by plan
and checked by reading.

---

## Scenarios

Stage 3 of this increment, written by `scenario-writer` and not approved at a gate (the list of
everything chosen in them is in their headers):

- **`carry-on-when-saving-fails.feature`**, a new last section: going to the background and coming
  back (D4, B1, B2) — 8 scenarios.
- **`choose-how-moneybud-looks.feature`**, new: theme, *Weergave* and the hints (D3, B3, B4) — 14
  scenarios, 21 cases (one added after review).
- **Header notes** in `point-at-a-slice.feature` and `start-moneybud.feature`, pointing at the phone.

22 scenarios, 29 cases, all tagged `@phone`, bound in `PhoneSteps.cs`. One figure was corrected in the
build: 400 − 18 − 32.15 is 349.85, where the scenario first said 367.85.

The phone's navigation rules (D2) are unit tests, not scenarios.

---

## Building it

1. **Shared layers.** `MoneyBudApp.GoToBackground`/`ComeBack`; `PhoneScreen`; `PhoneSettings` and
   `SettingsFile`; the phone's words in `Tekst`, and the display-terms table in §12 with them. Unit
   tests for each. Bind the new scenarios.
2. **`MoneyBud.Phone`.** Carry the prototype over (motion, ring view, painters, slab, surface, themes,
   controls, fonts, textures, the shell and its gestures), then replace `SampleLedger` with
   `MoneyBudApp` and `PhoneScreen`: every panel reads the Overview and binds the forms; every act goes
   through `MoneyBudApp` or a form. Keep the prototype's four traps fixed (§8.5).
3. **`MoneyBud.Phone.Desktop`**: the window, and the headless pictures.
4. **`MoneyBud.Phone.Android`**: package name, folders, backup opt-out, signing, vibration, the GPU
   budget, lifecycle. Make the key. Build the signed Release APK.
5. **Green**: `dotnet build` with no warnings, `dotnet test` all passing, the Android Release build.
   A headless run of every panel, both themes, dark and light.
6. **Review**: `spec-reviewer`; fix what it finds.
7. **Documents**: §5, §7, §8.4, §8.5, §11 and §12 (*chosen in the build*), the README (the phone's
   data folder, by ruling), `features/README.md`, `prototype/README.md` (what carried over), CLAUDE.md.
