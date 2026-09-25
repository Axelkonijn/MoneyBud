# 0005 — The desktop UI toolkit is Avalonia

**Status:** Accepted
**Date:** 2026-09-25

## Context

[ADR 0002](0002-desktop-application-first.md) settled that the first version is a desktop
application and deliberately left the toolkit open: "it decides the deployment form, not the
toolkit; the toolkit is a smaller and much cheaper choice and can wait." It could wait until
something needed a screen.

The fifth increment is that thing. It puts a Dutch desktop UI over everything the domain does, with
the Overview and its ring as the start screen ([§12](../arc42/12-glossary.md), *The user
interface*). What the UI shows and does was settled first, as requirements, and none of it depended
on a toolkit. The toolkit was proposed in the implementation plan and **approved by the stakeholder
at the plan gate on 2026-09-25**.

The choice is constrained from two sides. [§2](../arc42/02-architecture-constraints.md) fixes .NET
10 / C# and records that desktop **and mobile** are both wanted. ADR 0002 defers mobile rather than
rejecting it, and says mobile reopens the deployment question when it comes.

## Decision

**The desktop UI is built with Avalonia 12** (12.1.3), using its **Fluent theme**.

**CommunityToolkit.Mvvm** (8.4.2) supplies the view-model plumbing: observable properties and
commands, generated from attributes.

Avalonia is referenced **only by `MoneyBud.Desktop`**. CommunityToolkit.Mvvm is referenced by
`MoneyBud.Presentation`, which has no UI toolkit. That split is
[ADR 0006](0006-three-source-projects.md)'s decision, not this one's.

## Why

**It keeps the deferred mobile wish reachable without settling it.** Avalonia covers Windows, macOS
and Linux desktop, and targets iOS and Android from the same code. Nothing about mobile is decided
here, and ADR 0002 says mobile reopens the question anyway. But a toolkit with no route to a phone
would make that reopening a rewrite of the UI by default. This one leaves the option open for the
cost of choosing it now.

**It has a mature Fluent theme.** The demo is meant to be reacted to
([§1.1](../arc42/01-introduction-and-goals.md)). A plain, ready-made look lets the stakeholder react
to what the screen shows rather than to how unfinished it looks, without anyone styling controls by
hand.

**It has a headless mode.** The window can be rendered without a display. That was used during the
build to render the window with synthetic data and check it visually. The check was done outside the
repository and left nothing in it. It is the only check the Desktop project gets beyond running it
(ADR 0006, *Consequences*).

### Rejected

| Toolkit | Why not |
|---|---|
| **WPF** | Windows only, with no route to mobile. Mature, and the path of least surprise on Windows, but it would turn ADR 0002's "deferred, not rejected" into a rewrite |
| **.NET MAUI** | The obvious candidate for "desktop and mobile from one codebase", and rejected on the desktop side: **no Linux**, and its Windows desktop side is the heaviest of the four to build and package. A demo on the development machine is where this increment lives, so desktop friction counts for more now than mobile reach |
| **WinUI 3** | Windows only, for the same reason as WPF |

### CommunityToolkit.Mvvm

It is recorded here because it arrived with the toolkit. What makes it fit is that **it depends on
no UI toolkit.** Its observable properties and commands sit on .NET's own `INotifyPropertyChanged`
and `ICommand`, so `MoneyBud.Presentation` can use it and still be run by the scenarios without a
window. A different toolkit, or a mobile head, could bind to the same view models.

## Consequences

- **The operating systems follow the toolkit.** Avalonia's desktop targets are Windows, macOS and
  Linux. **Only Windows has been run.** The other two are possible and unverified
  ([§7](../arc42/07-deployment-view.md)).
- **Installation and distribution are still not decided.** The demo runs from the development
  machine. Avalonia changes nothing about how that would be settled
  ([§7](../arc42/07-deployment-view.md)).
- **Compiled bindings are on by default** (`AvaloniaUseCompiledBindingsByDefault`). A binding to a
  property that does not exist fails the build rather than going quietly blank at run time. That is
  most of what the window's markup can get wrong, and it is checked with no test written.
- **The toolkit's controls are given MoneyBud's rules rather than using their own.** The category
  box narrows its suggestions with MoneyBud's own predicate, `MoneyBudApp.SuggestionMatches`,
  passed to the `AutoCompleteBox` as a custom filter ([§12](../arc42/12-glossary.md), *Category entry
  is free text with suggestions*). At first it used Avalonia's built-in *Contains* filter, which
  compares by the thread's culture and sat out of every test's reach. It was moved into the
  presentation layer on 2026-09-25 ([ADR 0006](0006-three-source-projects.md), *Consequences*).
  One piece of toolkit behaviour remains: Avalonia's own text, the date picker's month and day names
  for example, follows the thread culture. `Program` fixes that to nl-NL so that it agrees with the
  Dutch MoneyBud writes itself.
- **Reversal is kept cheap by ADR 0006, not by this record.** Everything the screen decides is in
  the toolkit-free presentation layer. Replacing Avalonia means rewriting one window and one drawing
  control, and no scenario would change.
- **ADR 0002's statement that the toolkit is open is now answered here.** ADR 0002 is not
  superseded. It decided the deployment form, and that stands.
