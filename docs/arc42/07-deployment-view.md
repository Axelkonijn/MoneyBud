# 7. Deployment View

**What belongs here:** The infrastructure MoneyBud runs on — machines, processes, and which
building blocks are mapped onto them. For a local desktop or console app this stays very short,
and that is a legitimate answer rather than a missing one.

---

MoneyBud is a desktop application — see
[ADR 0002](../decisions/0002-desktop-application-first.md). That makes this section short, which is
the right answer here rather than a missing one.

**Since 2026-09-29 (settled, not built) there are two nodes.** The stakeholder ruled that MoneyBud is
**used on his Android phone** and that the desktop stays **for development**
([ADR 0013](../decisions/0013-an-android-phone-app.md), which supersedes ADR 0002). They run the same
code over the same three projects, each with its own data file, and **nothing connects them**: the file
is moved by hand, over USB. *The phone*, below, is new. *What is determined*, after it, is the desktop,
as built.

## The phone

| | |
|---|---|
| **Node** | The stakeholder's own phone: a **Red Magic 11 Pro, Android 16**, developer options and USB debugging on. **Phone only, portrait** ([§12](12-glossary.md), *MoneyBud on the phone*) |
| **Process** | One Android app, with the domain, the presentation layer and storage loaded into it, as on the desktop. Which projects make up the phone head is left to the plan (ADR 0013, decision 6). **Android runs only one instance of an app**, so "a second start says MoneyBud is already open" is not needed on the phone. **The last save attempt is made when MoneyBud goes to the background**, where the desktop makes it on closing, and **unreadable data shows its message, and the app closes when the message is tapped away** (ruled 2026-09-29; [§12](12-glossary.md), *Android's lifecycle*). How the minute's timer runs there is for the plan |
| **Network** | None, as on the desktop. No sync, no server, no store |
| **Data** | **The app's own folder on shared storage**, `Android/data/<package>/files`, reachable over USB for the stakeholder's copies ([ADR 0014](../decisions/0014-real-use-and-the-phone-data.md)). One set of data, never chosen by the user, never in the repository. **Real data**, from the phone version the stakeholder accepts at the end review. It **starts fresh**, with a first start's six categories; the desktop's demo data is not moved over. The theme and light or dark setting are kept in the app's settings, not in this folder's data file. **Protected by the phone's own lock** and nothing else: MoneyBud asks for no PIN and encrypts nothing (ruled 2026-09-29) |
| **Identity of the app** | **Signed with one fixed key of the stakeholder's, kept outside the public repository**, and with a fixed package name (*derived*), so every update installs over the one before and Android never removes the folder |
| **Backup** | **Not MoneyBud's**, as on the desktop: the stakeholder's own copies of the file over the cable. **The app opts out of Android's automatic backup** to the Google account, so the data stays on the phone only (ruled 2026-09-29; [ADR 0014](../decisions/0014-real-use-and-the-phone-data.md)). Uninstalling the app, or clearing its storage in Android's settings, deletes the folder, and then only his last copy brings the data back ([§11](11-risks-and-technical-debt.md)) |
| **Building it** | The `android` .NET workload and JDK 17 or 21 on the development machine ([§8.5](08-crosscutting-concepts.md)). **This increment is built without the phone connected**, so the first time the app runs on the phone is the stakeholder's review |

**Moving the file between the two nodes is by hand, and outside the boundary** ([§3.3](03-context-and-scope.md)),
like backing it up. A desktop build must be built from the same code as the phone's to read its file: an
older one meets a newer version and refuses it, touching nothing.

## What is determined

| | |
|---|---|
| **Nodes** | One. The user's own machine. **Since 2026-09-29 this is the desktop node only**: the phone is the second, and the place of use (*The phone*, above) |
| **Processes** | One. The application and its data live on that machine together. A second start while MoneyBud is open says so and closes, so only one process ever holds the data ([§12](12-glossary.md), *Only one MoneyBud at a time*). **Built** as an exclusive lock on `moneybud.lock`, held from before the data is loaded until MoneyBud closes, and released by the operating system if the process dies ([ADR 0007](../decisions/0007-keeping-the-ledger.md)) |
| **Network** | None required. MoneyBud does not need the machine to be online, and there is no server, no service and no second instance to keep in step ([§3.3](03-context-and-scope.md)). The phone is a second instance, and by ruling nothing keeps the two in step: the file moves by hand |
| **Mapping of building blocks** | Everything, into that single process. `MoneyBud.Desktop` is the executable, and `MoneyBud.Presentation`, `MoneyBud.Storage` and `MoneyBud.Domain` are libraries loaded into it ([§5](05-building-block-view.md)). Nothing is distributed, so there is no allocation decision to make. The specs project never ships |
| **Operating systems** | **Follow the toolkit.** Avalonia's desktop targets are Windows, macOS and Linux ([ADR 0005](../decisions/0005-avalonia-ui-toolkit.md)). **Only Windows has been run.** macOS and Linux are possible, not verified. That now includes where the data folder lands and whether the lock holds there, both of which follow .NET's behaviour on those systems rather than anything MoneyBud decides |
| **Data** | **Built on 2026-09-26.** One set of data, in one folder of the user's **local** application data: `%LOCALAPPDATA%\MoneyBud` on Windows, and .NET's mapping of the same folder elsewhere, which the root README lists. Found through the profile, **never** relative to the working directory, and never chosen by the user. Local rather than roaming, because a file rewritten after every change should not be copied between machines at sign-in. Protected by the operating system's login and nothing else: no password, no encryption. Only the ledger is kept, not what is on screen ([§8.3](08-crosscutting-concepts.md), [§12](12-glossary.md), *What MoneyBud keeps*). The location is written in the README, not shown by MoneyBud, not even when it cannot be read |
| **Files in that folder** | **`moneybud.json`**: the whole ledger, as JSON in UTF-8, rewritten after every change. It is the one file that matters, and the one the README tells the user to copy. **`moneybud.json.tmp`**: exists only while a save is being written, then is renamed over `moneybud.json`. One left behind by a save that was cut off is never read, and the next save writes over it. **`moneybud.lock`**: an empty file, held open exclusively while MoneyBud runs. It stays on disk after closing, and that is harmless: holding it open is what locks, not its existence. The folder and the lock file are made at the first start. They are made by the claim, before anything is loaded, so they appear even when the data then turns out to be unreadable. **That is within "touches nothing"**, which means the data file, as the stakeholder confirmed on 2026-09-26: the folder and the lock are MoneyBud's own bookkeeping. Creating nothing at all was rejected, because the data would have to be checked before the lock was taken, leaving a window in which two MoneyBuds could start at once ([§12](12-glossary.md), *When the data cannot be read*). `moneybud.json` first appears with the first change. `.gitignore` lists all three names, as a second line of defence ([ADR 0007](../decisions/0007-keeping-the-ledger.md)) |
| **Backup** | **Not MoneyBud's.** The stakeholder ruled on 2026-09-26 that backing up is the user's business, outside MoneyBud, and MoneyBud keeps no copies. **Backing up is copying `moneybud.json`.** A save replaces that file by a rename rather than rewriting it in place, so, in this documentation's reading, a copy is always of one whole save and never of a half-written one. Not tested: whether a copy still being read can make a save fail on Windows. If it does, that save shows "not saved" and is retried like any other. The file is plain text, so a backup can be checked by opening it. If the machine dies without a backup of the user's own, the data is gone ([§11](11-risks-and-technical-debt.md)) |

## What is not determined

These are genuinely open, not omitted. How data is stored, and in which folder, used to head this
list, and the persistence increment settled both ([ADR 0007](../decisions/0007-keeping-the-ledger.md)).

| Open | Where it will be settled |
|---|---|
| Installation and distribution: installer or copied folder, self-contained or framework-dependent .NET, how updates reach the machine | Not yet needed, and still not decided now that there is a window. **There is no installer and no distribution.** The demo runs from the development machine, built from source, so nothing forces this choice yet. **On the desktop that stays true**: it is the development build ([ADR 0013](../decisions/0013-an-android-phone-app.md)) |
| How a phone build reaches the phone | **Not ruled.** What is ruled is that every build is signed with the one fixed key, so it installs over the last ([ADR 0014](../decisions/0014-real-use-and-the-phone-data.md)). No store is involved. The prototype was installed from the development machine over USB (`adb install -r` of a signed Release build), which is the route that has been used. For the plan |
| Whether macOS and Linux are supported, rather than merely possible | Nobody has run them, and nothing needs them while the only user is on Windows. It matters when a second machine does. The ruling on protection names the **Windows** login for the same reason; the phone has its own counterpart, the phone's lock (*The phone*, above) |

**Settled on 2026-09-29, and so no longer here**: whether Android may copy the phone's data to the
stakeholder's cloud backup. It may not: the app opts out ([ADR 0014](../decisions/0014-real-use-and-the-phone-data.md)).
