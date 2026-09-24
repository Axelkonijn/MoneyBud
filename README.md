# MoneyBud

A personal budgeting app — track income, expenses and savings goals for one person or household.

## Status

**Very early.** There is no application yet — the repository currently holds the architecture
documentation skeleton and the way of working. Nothing here runs.

## Stack

.NET 10 / C#, with [Reqnroll](https://reqnroll.net) for Gherkin scenarios — see
[ADR 0001](docs/decisions/0001-dotnet-and-reqnroll.md).

## How this project is built

Requirements are documented with [arc42](docs/arc42/), specified as
[Gherkin scenarios](features/), and only then implemented. The scenarios are the contract: they
are reviewed and approved before any code is written against them.

## A note on data

This repository is public. Any example or test data in it is **synthetic** — no real financial
data, account numbers or statements. Please keep it that way if you contribute.

## License

Not licensed yet — all rights reserved for the moment.
