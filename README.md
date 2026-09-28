# MoneyBud

A personal budgeting app — track income, expenses and savings goals for one person or household.

## Status

**A demo, to gather feedback on.** A desktop app that records income and expenses, assigns
income to categories, offers a new period the last plan made to take over in one go, and shows
each budget period as a ring. It keeps accounts with their balances and your net worth, and moves
money between accounts. A category can be backed by an account, so that money assigned to it really
moves there and what has been built up for it shows. When a period ends, what is left of its money
moves by itself into one backed category you choose, and a period whose figures change afterwards
shows the difference and moves it in one click. Its data may not survive a new version until the
switch to real use.

```
dotnet run --project src/MoneyBud.Desktop    # the app
dotnet test MoneyBud.slnx                     # the scenarios and unit tests
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

MoneyBud does not show this location itself, and it makes **no backups**. Copy that file to back
it up. To start over, close MoneyBud and delete the file; the next start begins with the six
default categories and one account, Betaalrekening. If MoneyBud says it cannot read your data, it
has changed nothing: the file is still there as it was.

**Data saved before the sweep existed cannot be read** by the version with the sweep, and nor can
data saved before backing or before accounts existed. MoneyBud says it cannot read your data and
closes. Delete the file to start fresh.

The file lives in your user profile, never in this repository, whichever folder MoneyBud is run
from.

## How this project is built

Requirements are documented with [arc42](docs/arc42/), specified as
[Gherkin scenarios](features/), and only then implemented. The scenarios are the contract: they
are reviewed and approved before any code is written against them.

## A note on data

This repository is public. Any example or test data in it is **synthetic** — no real financial
data, account numbers or statements. Please keep it that way if you contribute.

## License

Not licensed yet — all rights reserved for the moment.
