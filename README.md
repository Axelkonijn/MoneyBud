<img src="docs/logo/moneybud.svg" alt="MoneyBud's logo: a gold coin with a sprout, as a sticker" width="120" align="right">

# MoneyBud

A personal budgeting app — track income, expenses and savings goals for one person or household.

## Status

**An Android app for one phone, with the desktop app beside it for development** (since increment
14; [ADR 0013](docs/decisions/0013-an-android-phone-app.md)). Both do everything below, and read and
write the same file; only the phone has themes. Until then it was **a demo, to gather feedback
on**: a desktop app that records income and expenses, assigns
income to categories, offers a new period the last plan made to take over in one go, and shows
each budget period as a ring. It keeps accounts with their balances and your net worth, and moves
money between accounts. A category can be backed by an account, so that money assigned to it really
moves there and what has been built up for it shows. When a period ends, what is left of its money
moves by itself into one backed category you choose, and a period whose figures change afterwards
shows the difference and moves it in one click. An income or an expense can repeat weekly or
monthly, and the day budget periods start on can be set to payday. **Real use starts with the phone
version accepted at increment 14's review**: from that version on, every later MoneyBud reads the
data it saved ([ADR 0014](docs/decisions/0014-real-use-and-the-phone-data.md)).

```
dotnet run --project src/MoneyBud.Desktop          # the desktop app
dotnet run --project src/MoneyBud.Phone.Desktop    # the phone's screens in a window, on data of their own
dotnet test MoneyBud.slnx                           # the scenarios and unit tests
```

## MoneyBud on the phone

Android 12 or later, built with the `android` .NET workload and JDK 21 (the project looks for it in
`C:\Program Files\Java\jdk-21.0.10`). The Android project is not in `MoneyBud.slnx`, because
building it takes minutes:

```
dotnet publish src/MoneyBud.Phone.Android -c Release
adb install -r src/MoneyBud.Phone.Android/bin/Release/net10.0-android/publish/app.moneybud-Signed.apk
```

**Every Release build is signed with one key, kept outside this repository** in
`%USERPROFILE%\MoneyBud-signing\` (the key, its password, and the `signing.props` the build reads).
A Release build refuses to build without it. **Keep that folder, and a copy of it**: Android installs
an update over MoneyBud only when it is signed with the same key, and otherwise only after
uninstalling, which deletes the phone's data. A Debug build is a separate app, `app.moneybud.debug`,
so it can never replace the real one.

Pictures of every phone screen, without a phone (`SLOW=8` lets kintsugi's plate finish drawing):

```
dotnet run --project src/MoneyBud.Phone.Desktop -- snapshot <folder> dark light kintsugi-dark kintsugi-light
```

## Stack

.NET 10 / C#, with [Reqnroll](https://reqnroll.net) for Gherkin scenarios — see
[ADR 0001](docs/decisions/0001-dotnet-and-reqnroll.md). The desktop UI is
[Avalonia](https://avaloniaui.net) — [ADR 0005](docs/decisions/0005-avalonia-ui-toolkit.md).

## Where your data is kept

MoneyBud keeps everything you enter in one file, saved after every change:

| System | File |
|---|---|
| Windows | `%LOCALAPPDATA%\MoneyBud\moneybud.json` |
| macOS | `~/Library/Application Support/MoneyBud/moneybud.json` |
| Linux | `~/.local/share/MoneyBud/moneybud.json` |
| Android (the phone) | `Android/data/app.moneybud/files/moneybud.json`, reachable over USB from a PC |

MoneyBud does not show this location itself, and it makes **no backups**, on the phone as on the
desktop: the phone app is kept out of Android's own backup too. Copy that file to back it up; on the
phone, over the cable. The phone's theme and appearance are not in that file: they are in the app's
private settings, so the file is the same whichever device wrote it. To move data between the phone
and the desktop, copy the file by hand; nothing syncs them, and a copy replaces what was there. To start over, close MoneyBud and delete the file; the next start begins with the six
default categories and one account, Betaalrekening. If MoneyBud says it cannot read your data, it
has changed nothing: the file is still there as it was.

**Data saved by the versions with the sweep and with recurring entries is read** by the version
with a configurable start day, as data whose periods have always started on the 1st, and nothing
repeats in the sweep's. **Data saved before the sweep existed cannot be read**, and nor can data
saved before backing or before accounts existed. MoneyBud says it cannot read your data and closes.
Delete the file to start fresh.

The file lives in your user profile, never in this repository, whichever folder MoneyBud is run
from. On the phone, uninstalling MoneyBud, or *Clear storage* in Android's settings for it, deletes
the file: keep your own copy.

## How this project is built

Requirements are documented with [arc42](docs/arc42/), specified as
[Gherkin scenarios](features/), and only then implemented. The scenarios are the contract: they
are reviewed and approved before any code is written against them.

## A note on data

This repository is public. Any example or test data in it is **synthetic** — no real financial
data, account numbers or statements. Please keep it that way if you contribute.

## License

Not licensed yet — all rights reserved for the moment.
