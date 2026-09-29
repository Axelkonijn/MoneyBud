# Architecture Decisions

One file per decision that is annoying to reverse, numbered in order. Indexed from
[arc42 section 9](../arc42/09-architecture-decisions.md).

The point of these is the **reasoning**, not the ceremony — what we chose, and why it looked right
at the time. A decision that turns out wrong isn't deleted; it gets superseded by a later record
that says so.

| # | Decision | Status |
|---|---|---|
| [0001](0001-dotnet-and-reqnroll.md) | .NET 10 and Reqnroll for BDD | Accepted |
| [0002](0002-desktop-application-first.md) | The first version is a desktop application | **Superseded by 0013** |
| [0003](0003-money-representation.md) | How money is represented in code | Accepted |
| [0004](0004-solution-layout.md) | The layout of the solution: two projects, xUnit, linked feature files | Accepted; decision 1 superseded by 0006 |
| [0005](0005-avalonia-ui-toolkit.md) | The desktop UI toolkit is Avalonia | Accepted; the phone's toolkit too since 0013 |
| [0006](0006-three-source-projects.md) | Three source projects: domain, presentation, desktop | Accepted; supersedes 0004's decision 1; amended by 0007; a second head, the phone, since 0013 |
| [0007](0007-keeping-the-ledger.md) | Keeping the ledger: one JSON file in the user's profile, in a fourth project | Accepted; amends 0006. Format version 2 since 0008, version 3 since 0009, version 4 since 0010, version 5 since 0011, version 7 since 0012 (version 6 had no record); the start day is stored since 0012. **Superseded in part by 0014**: from the phone version accepted at the end review, older versions must be read |
| [0008](0008-balance-is-worked-out.md) | A balance is worked out; a typed balance is a dated statement | Accepted; takes 0007's format to version 2 |
| [0009](0009-movements-are-entries.md) | Money moved for a category is a stored entry, written on the day it moves | Accepted; takes 0007's format to version 3. Its saving gap narrowed for sweeps by 0010 and for occurrences by 0011; settling event by event since 0011 |
| [0010](0010-sweeps-and-period-ends.md) | A sweep is a movement for a period, and settling records each period's end | Accepted; takes 0007's format to version 4. A sweep's period named through the ledger's calendar, and a sweep made by a change dated that day, since 0012 |
| [0011](0011-recurring-entries.md) | A recurring entry is state beside its occurrences, and settling works event by event | Accepted; takes 0007's format to version 5 and reads version 4 |
| [0012](0012-the-calendar-is-a-history.md) | The calendar is a history of start-day changes, and it is kept | Accepted; takes 0007's format to version 7 and reads versions 6, 5 and 4 |
| [0013](0013-an-android-phone-app.md) | MoneyBud is used on an Android phone, as a second head over the same projects | Accepted; supersedes 0002; extends 0005 and 0006 |
| [0014](0014-real-use-and-the-phone-data.md) | Real use: every later version reads the data, which on the phone lives in the app's own folder under one fixed signing key | Accepted; supersedes 0007 in part |
