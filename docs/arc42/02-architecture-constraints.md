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
| Desktop and mobile are both wanted | Stated as "het hoeft niet alleen een app te zijn". Syncing between them is desirable but explicitly a nice-to-have, not a requirement. **Made concrete on 2026-09-29** (next row) |
| **MoneyBud is used on the stakeholder's Android phone; the desktop is for development** | **Stakeholder decision, 2026-09-29** ([§12](12-glossary.md), *MoneyBud on the phone*). His phone is a Red Magic 11 Pro running Android 16, used in portrait; no iPhone, no tablet. The desktop build stays, with every feature the phone has, and the two builds stay the same, **except that themes are the phone's alone**. **No sync**: the file is moved by hand. How that is built is [ADR 0013](../decisions/0013-an-android-phone-app.md) |
| **Real use: every later version reads the data** | **Stakeholder decision, 2026-09-29.** From the phone version he accepts at that increment's end review, a new version must keep reading the data it saved; until he accepts it, the format may still change and data entered may have to be re-entered. Before this row, demo data could be dropped at a version change. How it is kept is [ADR 0014](../decisions/0014-real-use-and-the-phone-data.md) |
| Single user | No sharing, no multi-user accounts, no permissions |
| Euro only | **Stakeholder decision, taken deliberately on 2026-09-24.** It stood here as an assumption that had been stated back and not contradicted until [ADR 0003](../decisions/0003-money-representation.md) leaned on it to drop the currency field; an argument resting on silence is not an argument, so it was put to the stakeholder as a question and he settled it. Reversing it is now a decision to revisit rather than an assumption to correct — [§8.2](08-crosscutting-concepts.md) holds the trigger |

## Organisational

| Constraint | Background |
|---|---|
| Solo developer, hobby time budget | Limits scope and rules out operationally heavy designs |
| Gherkin scenarios as the specification contract | Required by the way of working |
| arc42 for architecture documentation, in English | Required by the way of working |
| Incremental delivery, SCRUM-style | The first version is a demo to gather feedback on, not a minimum viable product. **The demo ends with the phone** (2026-09-29): real use starts with the phone version accepted at the end review (above) |
| Public repository — no real financial data | Repository is public on GitHub. **Since 2026-09-29 this also keeps the phone app's signing key out**: every phone build is signed with one fixed key of the stakeholder's, kept outside the repository ([ADR 0014](../decisions/0014-real-use-and-the-phone-data.md)) |
