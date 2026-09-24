# 0001 — .NET 10 and Reqnroll for BDD

**Status:** Accepted
**Date:** 2026-09-24

## Context

MoneyBud follows an arc42 + Gherkin workflow, so the executable-specification tooling is not a
side concern — it sits on the critical path of every feature. Feature files are expensive to
migrate between ecosystems once step definitions exist, so this is effectively a one-way door and
needs deciding before the first scenario is written.

Three candidates were considered: .NET with Reqnroll, TypeScript with Cucumber.js, and Python with
pytest-bdd.

## Decision

Build MoneyBud on **.NET 10 / C#**, using **Reqnroll** for Gherkin scenarios.

## Why

Reqnroll (the maintained successor to SpecFlow) has the strongest Gherkin tooling of the three —
step binding, navigation from scenario to step definition, and living documentation generation.
Since the whole workflow is built around scenarios being the contract between specification and
implementation, the quality of that tooling matters more here than it would on a project where BDD
was an add-on.

C# also suits the domain. A budgeting app is relational, and its money handling benefits from a
strong type system and the built-in `decimal` type.

.NET 10 is already installed locally, and there is existing C# familiarity to build on.

## Consequences

- Scenarios live in a Reqnroll test project; step definitions are C#.
- UI shape is still open — console, desktop, or ASP.NET — and gets its own decision record later.
- Moving away from .NET later would mean rewriting step definitions, though the `.feature` files
  themselves are tool-agnostic and would survive.
