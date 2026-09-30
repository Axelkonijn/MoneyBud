# 0014 — Real use: every later version reads the data, which on the phone lives in the app's own folder under one fixed signing key

**Status:** Accepted
**Date:** 2026-09-29 (the stakeholder's rulings; recorded 2026-09-30)
**Supersedes, in part:** [ADR 0007](0007-keeping-the-ledger.md). Its consequences that "until the
switch to real use an older file is simply unreadable to the new version" and that "reversal is cheap
while the data is demo data" end with **the phone version the stakeholder accepts at this increment's
end review**, and so does its argument under *The version field, while no version need read another's
data*, which was written for the time before. Decisions 1 to 6 of 0007 stand. Decision 2, the folder,
stands for the desktop, and decision 3 below adds the phone's. 0007 carries a dated note that points
here.
**Relates to:** [ADR 0013](0013-an-android-phone-app.md), which makes the phone the place MoneyBud is
used.

## Context

From the persistence increment on, MoneyBud kept its data, and it was demo data. The stakeholder ruled
on 2026-09-26 that **until real use starts, a new version may be unable to read an older one's data**,
and that **carrying data from one version to the next becomes a requirement at the switch to real use,
and not before** ([§12](../arc42/12-glossary.md), *Demo data may not survive a new version*). ADR 0007
put a format name and version in the file for that day: "The field also leaves the door open for the
day carrying data across versions becomes a requirement." The file is at version 7 today, and reads 6,
5 and 4, but each of those readings was chosen because it was free, "and promises nothing for the
next".

On 2026-09-29 the stakeholder settled the switch
([stakeholder round](../stakeholder/2026-09-29-mobiel.md)):

- **Real use starts with the phone.** From this version on, a new version must keep reading the data
  it saved. Rejected: another demo first.
- **The phone starts empty, with real data only**: the six default categories and nothing else, as any
  first start. The desktop's demo data stays behind. Rejected: moving the desktop's file over.

And, after the prototype, where the data lives on the phone
([stakeholder round](../stakeholder/2026-09-29-mobiel-na-het-prototype.md)). The question put to him
said that the simplest place, the app's own folder, reachable over USB, is **deleted if the app is
ever uninstalled**, for example when its signing key changes and an update will not install over it.
He chose:

- **The app's own folder, with a fixed key.** The data lives in the app's own folder
  (Android/data/…/files), copyable over USB. **The app is always signed with one key of Axel's, kept
  outside the public repository**, so an update always installs over the one before and nothing is
  ever deleted. **His own copies over the cable are the safety net**, as agreed for the desktop.
  Rejected: a shared folder such as Documents/MoneyBud, which survives uninstalling, with a one-time
  permission for file access. No reason was recorded beyond the options' own.

Writing this record up raised three more questions, put to him straight away and answered the same day,
each on the recommendation (the same round, *Vragen die bij het bijwerken van de documentatie
opkwamen*):

- **From which version the promise runs: the version he accepts at the end review.** "De beoordeling
  mag het bestandsformaat nog veranderen; wat vóór de goedkeuring is ingevoerd, moet misschien
  opnieuw." Rejected: the first version installed. The question arose because this increment's two
  approval gates are waived, so the first build he installs is the one he reviews.
- **What protects the data on the phone: the phone's own lock.** MoneyBud asks for nothing itself.
  Rejected: a PIN or fingerprint of MoneyBud's own.
- **Android's automatic backup: turned off.** Android can back an app's folder up to the user's Google
  account unless the app opts out. "De gegevens blijven alleen op de telefoon; Axels eigen kopieën via
  de kabel zijn de back-up." Rejected: allowing it, as an automatic safety net.

## Decision

### 1. From the accepted phone version, every later MoneyBud reads the data it wrote

**The file format written by the phone version the stakeholder accepts at this increment's end review
is the first promised version.** Every later MoneyBud, on either head, reads it and every version
written after it. A change of format is still a new version number, as ADR 0007 decided, but "cannot
read" is **no longer an allowed answer** to an older promised version. It stays the answer to a damaged
file, a blank one, and a newer version.

**Until he accepts it, the format may still change**, at the review, and what he entered on a build
before that may have to be entered again. That is his ruling, and it keeps the review free to change
anything.

**How each new version reads the one before is decided by the plan that makes it.** The precedent is
ADR 0011's and ADR 0012's: an older version is read as what it is, guessing nothing. Where a new
version adds something an older file cannot hold, that plan must say how the older file is read, and
the stakeholder decides it if it cannot be done without guessing.

**Nothing written before the accepted version is promised.** The desktop's demo files are not carried
forward by ruling. If the accepted version writes version 7, the desktop's current file happens to be
readable. That is an accident of numbering, not a promise about demo data.

### 2. The phone starts fresh

The phone's first start is a first start: the six default categories, *Betaalrekening* as the pool
account, and nothing else ([§12](../arc42/12-glossary.md), *A first start is unchanged*). Nothing from
the desktop is carried over.

### 3. On the phone, the data lives in the app's own folder

The data file lives in the phone app's **app-specific folder on shared storage**, Android's
`Android/data/<package>/files`, where Axel can reach it over USB to copy it by hand. It is a fixed
place the user never chooses, and never in the repository, as on the desktop
([§12](../arc42/12-glossary.md), *In a fixed place, never in the repository*). **How `FileLedgerStore`
finds it, and whether the lock and the temporary file work there as on the desktop, are for the
plan.** The desktop keeps `%LOCALAPPDATA%\MoneyBud` (ADR 0007, decision 2).

### 4. The phone app is always signed with one fixed key, kept outside the repository

Every build of the phone app that Axel installs is **signed with the same key**, which is his and is
**never in the public repository**. So every update installs over the one before, and Android never
removes the app, and with it the folder. **The app's package name is fixed for the same reason**
(*derived*): Android treats a different package name as a different app, with a different, empty
folder. How the build finds the key without the repository holding it is for the plan.

### 5. The data stays on the phone: no automatic backup, and no copies by MoneyBud

**The app opts out of Android's automatic backup**, so neither the data file nor the app's settings go
to the stakeholder's Google account. MoneyBud makes no copies, on the phone as on the desktop. **Axel's
copies over the cable are the backup** ([§12](../arc42/12-glossary.md), *Backing up is the user's
business*).

### 6. The phone's own lock is the protection

MoneyBud asks for no PIN, password or fingerprint, and encrypts nothing. **The phone's own lock
protects the data**, as the Windows login does on the desktop ([§12](../arc42/12-glossary.md), *The
login is the protection*).

## Why

### Why a promise now, and from the accepted version

It is the stakeholder's ruling, and the one [§12](../arc42/12-glossary.md) has pointed at since
2026-09-26: carrying data across versions becomes a requirement at the switch to real use. The version
field is where it starts, as ADR 0007 intended. Starting the promise with a fresh phone means the first
promised file holds only real data, so no version ever has to carry demo data forward, which was the
reason demo data was never promised. **Starting it at the accepted version, not the first installed**,
leaves the review, which replaces this increment's gates, free to change the format without a reading
path for a version nobody accepted.

### Why the app's own folder

His choice. In the documentation's reading, it is the one place on Android that needs **no permission**
and that other apps cannot read, and it is still reachable over the cable, which is how he backs up and
how he would move the file to the desktop. Its one cost is that uninstalling deletes it. That cost is
met by never uninstalling: a fixed key and a fixed package name mean every update installs over the
last.

### Why the key is outside the repository

The repository is public ([§2](../arc42/02-architecture-constraints.md)). In the documentation's
reading, a signing key in it would let anyone sign a build that installs over his, and it would be a
secret in a place where nothing secret belongs. Outside the repository, only he can sign MoneyBud for
his phone.

### Why no automatic backup

His ruling: the data stays on the phone only. In the documentation's reading it keeps quality goal 4,
local operation, whole, and it keeps *Backing up is the user's business* true on the phone: an
automatic copy in the cloud would be a copy MoneyBud had let happen, of every real figure, somewhere
he did not choose.

### Why the phone's lock

His ruling, the counterpart of the desktop's: the data is local, and a forgotten PIN of MoneyBud's own
would lock him out of his own data for good, which is the reason given for the Windows login.

### Rejected

| Rejected | Why |
|---|---|
| **Another demo before real use** | The stakeholder's ruling |
| **The promise from the first version installed** | The stakeholder's ruling: from the version he accepts at the review, which may still change the format |
| **Moving the desktop's demo file to the phone** | The stakeholder's ruling: the phone starts with real data only |
| **A shared folder that survives uninstalling** (Documents/MoneyBud, with a one-time permission) | The stakeholder's ruling. No reason recorded beyond the options' own |
| **Allowing Android's automatic backup**, as a safety net | The stakeholder's ruling: the data stays on the phone only |
| **A PIN or fingerprint in MoneyBud** | The stakeholder's ruling: the phone's own lock is enough |
| **Signing each build as it comes** (Android's debug key on whichever machine builds it) | The documentation's reasoning: a different key makes an update refuse to install, and the only way on is to uninstall, which deletes the data |

## Consequences

- **[ADR 0007](0007-keeping-the-ledger.md) is superseded in the parts named at the top**, and carries a
  dated note. Its file, its strictness, its keys, its atomic save and its lock stand.
- **The "reversal is cheap while the data is demo data" consequences of ADRs 0008 to 0012 end with the
  accepted phone version.** Each was written with that condition in it, so each ends by its own words.
  Their decisions stand, so they carry no note ([§9](../arc42/09-architecture-decisions.md) says so).
- **Every change of format after the accepted version costs a reading path**, kept for good, and a test
  that reads the older version. `StorageTests` already reads versions 6, 5 and 4. That is the shape
  every later version follows.
- **Data entered before acceptance may be lost**, if the review changes the format. Accepted by ruling.
- **Real data raises two risks** ([§11](../arc42/11-risks-and-technical-debt.md)): a wrong stored
  representation is now expensive to change, and the one file with no copies is now the whole real
  history.
- **The key and the package name are now load-bearing.** Losing the key means an update can be
  installed only after uninstalling, which deletes the folder, so the data then comes back only from
  Axel's last copy. Android's own *clear storage* in the app's settings deletes it too. Neither is
  something MoneyBud can prevent ([§11](../arc42/11-risks-and-technical-debt.md)).
- **Opting out of the automatic backup leaves the cable as the only copy.** A lost or broken phone
  takes everything since his last copy with it ([§11](../arc42/11-risks-and-technical-debt.md)).
- **That the folder is reachable over USB on his phone is assumed, not yet tried** with the real app.
  It is the backup route and the only way to move the file.
- **The root README will name the phone's folder**, by the ruling that the data's location is written
  there and nowhere in MoneyBud (*derived*; [§12](../arc42/12-glossary.md), *Where the data is, is
  written in the README*).
- **The desktop's own demo data is untouched.** It stays in `%LOCALAPPDATA%\MoneyBud`, and a phone file
  copied over it replaces it by hand.
