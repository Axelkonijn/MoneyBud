# 2. Architecture Constraints

**What belongs here:** Anything that limits our freedom to choose — technical, organisational, or
conventional. Constraints are things we must work within, not decisions we made. If we chose it,
it belongs in section 4 or an ADR instead.

---

## Technical

| Constraint | Background |
|---|---|
| .NET 10 / C# | See [ADR 0001](../decisions/0001-dotnet-and-reqnroll.md) — chosen, but now fixed |
| Runs locally on the user's own machine | Stakeholder preference, stated in the interview even after acknowledging that a web app would more easily cover desktop and mobile. Rules out designs that require a server to be useful |
| Desktop and mobile are both wanted | Stated as "het hoeft niet alleen een app te zijn". Syncing between them is desirable but explicitly a nice-to-have, not a requirement |
| Single user | No sharing, no multi-user accounts, no permissions |
| Euro only | Assumption, stated back to the stakeholder and not contradicted |

## Organisational

| Constraint | Background |
|---|---|
| Solo developer, hobby time budget | Limits scope and rules out operationally heavy designs |
| Gherkin scenarios as the specification contract | Required by the way of working |
| arc42 for architecture documentation, in English | Required by the way of working |
| Incremental delivery, SCRUM-style | The first version is a demo to gather feedback on, not a minimum viable product |
| Public repository — no real financial data | Repository is public on GitHub |
