# Architecture Decisions

One file per decision that is annoying to reverse, numbered in order. Indexed from
[arc42 section 9](../arc42/09-architecture-decisions.md).

The point of these is the **reasoning**, not the ceremony — what we chose, and why it looked right
at the time. A decision that turns out wrong isn't deleted; it gets superseded by a later record
that says so.

| # | Decision | Status |
|---|---|---|
| [0001](0001-dotnet-and-reqnroll.md) | .NET 10 and Reqnroll for BDD | Accepted |
| [0002](0002-desktop-application-first.md) | The first version is a desktop application | Accepted |
| [0003](0003-money-representation.md) | How money is represented in code | Accepted |
| [0004](0004-solution-layout.md) | The layout of the solution: two projects, xUnit, linked feature files | Accepted; decision 1 superseded by 0006 |
| [0005](0005-avalonia-ui-toolkit.md) | The desktop UI toolkit is Avalonia | Accepted |
| [0006](0006-three-source-projects.md) | Three source projects: domain, presentation, desktop | Accepted; supersedes 0004's decision 1; amended by 0007 |
| [0007](0007-keeping-the-ledger.md) | Keeping the ledger: one JSON file in the user's profile, in a fourth project | Accepted; amends 0006 |
