# 7. Deployment View

**What belongs here:** The infrastructure MoneyBud runs on — machines, processes, and which
building blocks are mapped onto them. For a local desktop or console app this stays very short,
and that is a legitimate answer rather than a missing one.

---

MoneyBud is a desktop application — see
[ADR 0002](../decisions/0002-desktop-application-first.md). That makes this section short, which is
the right answer here rather than a missing one.

## What is determined

| | |
|---|---|
| **Nodes** | One. The user's own machine |
| **Processes** | One. The application and its data live on that machine together |
| **Network** | None required. MoneyBud does not need the machine to be online, and there is no server, no service and no second instance to keep in step ([§3.3](03-context-and-scope.md)) |
| **Mapping of building blocks** | Everything, into that single process. Nothing is distributed, so there is no allocation decision to make. [§5](05-building-block-view.md) is one library and one test project, and does not change this |

## What is not determined

These are genuinely open, not omitted:

| Open | Where it will be settled |
|---|---|
| How data is stored on the machine — file, embedded database, something else — and where it lives | [§8.3](08-crosscutting-concepts.md), which defers it: **nothing is stored in the first increment**, and §8.3 states what will force the choice |
| Installation and distribution: installer or copied folder, self-contained or framework-dependent .NET, how updates reach the machine | Not yet needed. The demo runs from the development machine, so nothing forces this choice yet |
| Which operating systems are supported | Follows the UI toolkit, which ADR 0002 deliberately leaves open |
| Backup, and what happens to the data if the machine dies | Untouched while the demo's data is throwaway ([§1.1](01-introduction-and-goals.md)). Becomes a real question the moment it is not |
