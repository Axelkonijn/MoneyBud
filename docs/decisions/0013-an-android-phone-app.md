# 0013 — MoneyBud is used on an Android phone, as a second head over the same projects

**Status:** Accepted
**Date:** 2026-09-29 (the stakeholder's rulings; recorded 2026-09-30)
**Supersedes:** [ADR 0002](0002-desktop-application-first.md), "The first version is a desktop
application", as that record said it should be once the demo stopped being a demo.
**Extends:** [ADR 0005](0005-avalonia-ui-toolkit.md), whose toolkit now draws the phone as well, and
[ADR 0006](0006-three-source-projects.md), whose toolkit-free presentation layer now has a second head
on it, as that record foresaw. Nothing either decided is reversed. Both carry dated notes that point
here.

## Context

[ADR 0002](0002-desktop-application-first.md) chose a desktop application for the first version
while knowing it was the wrong shape for how MoneyBud would be used: **expenses are entered out of the
house**, and effortless entry, quality goal 2, is served by the device in the user's pocket. It chose
the desktop anyway because the first version was a throwaway demo, and it named the moment that
argument would expire: *"The moment the stakeholder starts keeping data he would mind losing, the
argument that justified this decision has expired and it should be reopened."*

On 2026-09-28 the stakeholder chose a mobile front-end as the last of three increments, "the biggest
and last for now", and the one that would make him actually use MoneyBud. On 2026-09-29 he settled it
([stakeholder round](../stakeholder/2026-09-29-mobiel.md); [§12](../arc42/12-glossary.md), *MoneyBud
on the phone*):

- **Android**, on his own phone, a Red Magic 11 Pro running Android 16. Not an iPhone, not both.
- **Only the phone.** "De desktop is voor de ontwikkeling." He may one day copy the file to the
  desktop, and will do that by hand, probably with a cable: "Daar hoeft niets voor gemaakt te worden."
  Rejected: both kept in step, and the desktop as the main place with the phone for quick entry.
- **Everything the desktop does**, laid out for a small screen. Rejected: entry and the Overview only,
  and entry only.
- **"De builds moeten natuurlijk wel gelijk blijven."** The two builds stay the same.
- **Real use starts with this version**, rejecting another demo first. That is the moment ADR 0002
  named. What it means for the data file is [ADR 0014](0014-real-use-and-the-phone-data.md).

He asked to see the look before the technique, so stage 1 included a **prototype** in `prototype/`, on
invented data and connected to nothing in `src/`, built with Avalonia and run on his phone. Round 1
(layout, navigation, feel) and round 2 (a second theme, kintsugi) were both approved on 2026-09-29. The
prototype is not the app, but it showed that Avalonia runs this design on this phone at the phone's
full frame rate, once four traps were found and fixed ([§8.5](../arc42/08-crosscutting-concepts.md)).

After the prototype he added one exception to "the builds stay the same": **themes are the phone's
alone**. "Die zijn voor de desktopversie niet nodig." The chosen theme is remembered **in the app's
settings, not in the data file**, "zodat dat bestand voor desktop en telefoon hetzelfde blijft"
([stakeholder round](../stakeholder/2026-09-29-mobiel-na-het-prototype.md)).

[ADR 0005](0005-avalonia-ui-toolkit.md) had chosen Avalonia partly because it "keeps the deferred
mobile wish reachable without settling it". [ADR 0006](0006-three-source-projects.md) had said that a
mobile head "could sit on `MoneyBud.Presentation` as the Desktop does. Whether it should is for the ADR
that reopens mobile." This is that record.

## Decision

### 1. MoneyBud is used on Axel's Android phone; the desktop stays, for development

The place MoneyBud is **used** is an Android phone, **phone only and in portrait**: no tablet layout,
no iPhone. The desktop application stays, fully working, as the build development happens on and as
somewhere to open a copy of the file.

### 2. No sync

Each device has its own data file. **Moving the file between them is done by hand, over USB, and
MoneyBud has nothing for it**: no export, no import, no network. The two files are never merged.

### 3. One codebase: a second Avalonia head over the same three projects

The phone app is **a second head** beside `MoneyBud.Desktop`, built with **Avalonia** for Android, over
the same `MoneyBud.Domain`, `MoneyBud.Presentation` and `MoneyBud.Storage`. **Every feature is on
both, and both read and write the one file format.** A feature on one head only is a defect, with the
one exception in decision 5.

### 4. The phone head is held to the Desktop's rule: it decides nothing

What MoneyBud shows and decides stays in the shared layers, where the scenarios reach it. The phone
head lays out, draws, animates, and turns touches into the acts the presentation layer already has. A
decision found in it moves into `MoneyBud.Presentation`, as the Desktop's did
([ADR 0006](0006-three-source-projects.md), *Consequences*). **Which of the phone's own interaction
rules count as such decisions** (a slice staying chosen after the finger lifts, the budget opening on
the chosen category, the back button releasing a chosen category before closing a panel) **is for the
plan to settle**, one by one.

### 5. Themes are the phone's alone, and change only how MoneyBud looks

A **theme** changes how MoneyBud looks and moves: colours, textures, type, the ring's drawing, the
panels' surfaces, one moment of animation. **It never changes what is shown, in what order, or what an
act does.** Themes exist on the phone only. The chosen theme and the light/dark setting are kept **in
the app's settings on the phone, never in the data file**, so the file is the same whichever head
wrote it.

### 6. The project layout is left to the plan

Which new projects the phone app needs, what they are called, and where the theme mechanism and the
phone's own Dutch words live, are **not decided here**. They are for this increment's implementation
plan (stage 4), with a record of their own if they turn out to be architectural. This record decides
only that the phone is a second head over the existing three.

## Why

### Why ADR 0002 is superseded now

ADR 0002 traded the usage pattern for the feedback loop, "only a good trade while the demo stays a
demo". The stakeholder has ended the demo by his own ruling: real use starts with the phone. So the
usage argument, which ADR 0002 conceded it lost "in the long run", now decides: MoneyBud goes where
the entering happens. ADR 0002's decision was the deployment form, and the deployment form has
changed, so this is a superseding record, not a note.

### Why only the phone, and no sync

These are his rulings, and the documentation's reading of why they fit: sync would introduce a second
instance of the system and something between the two ([§3.2](../arc42/03-context-and-scope.md)), and
that something would be either a server, which local operation rules out (quality goal 4), or a
merge, which is a large piece of work for a desktop he uses only to develop on. Moving a file by hand
costs nothing to build and is what he already does for backups (*Backing up is the user's business*,
[§12](../arc42/12-glossary.md)).

### Why the same projects, and the same toolkit

**"No double work", and "the builds stay the same", are the requirement.** ADR 0006 is what makes them
cheap: everything the screen decides is already in a layer with no toolkit, and every scenario runs
against that layer without a window. A second head on it gets every rule, every Dutch word and every
scenario for nothing, and a rule changed once is changed on both. The specification covers both heads
by construction, which is the property ADR 0002 and ADR 0006 both kept open for this day.

**Avalonia, because it is already the desktop's and the prototype proved it on this phone.** One
toolkit means one way of laying out and styling, and the prototype's layout, styling, animations and
theme files can carry over to the app. The rejected alternative, in the documentation's reasoning and
not weighed with the stakeholder, is **a different toolkit for the phone** (.NET MAUI, or native
Android): the shared layers would survive it, but the window-side knowledge would be built twice, and
the prototype's approved look would have to be rebuilt in it. [ADR 0005](0005-avalonia-ui-toolkit.md)
had already rejected MAUI for the desktop.

### Why the phone head decides nothing

The Desktop has no automated tests, by plan, and that is acceptable only while it decides nothing
([ADR 0006](0006-three-source-projects.md)). The phone head is in the same position, with one thing
worse: the build is done without the phone connected, and some of its failures show only on the
device ([§8.5](../arc42/08-crosscutting-concepts.md), [§11](../arc42/11-risks-and-technical-debt.md)).
So anything that decides must be where the tests are.

### Why themes are only the look, and only on the phone

The phone-only part is his ruling. The rest is what makes the ruling safe: if a theme could change
what is shown or done, the two heads would behave differently, which decision 3 forbids. Kept to
looks, a theme belongs with drawing, and the desktop simply has one look. Keeping the choice out of
the data file is his reason in his words: the file stays the same for desktop and phone. A theme is
about the device in the hand, not about the money.

### Rejected

| Rejected | Why |
|---|---|
| **Keeping the desktop as the place of use** (ADR 0002 unchanged) | Its own argument has expired: the demo has ended, and entry happens out of the house |
| **Sync between phone and desktop** | The stakeholder's ruling: only the phone, the file moved by hand. In the documentation's reading, it would also need a server or a merge (above) |
| **iPhone, or both platforms** | The stakeholder's phone is Android |
| **A phone app with less than the desktop** (entry and Overview, or entry only) | The stakeholder's ruling: everything the desktop does |
| **A different toolkit on the phone** | The documentation's reasoning, above; not put to the stakeholder |
| **Themes on both heads** | The stakeholder's ruling: not needed on the desktop |

## Consequences

- **[ADR 0002](0002-desktop-application-first.md) is superseded.** Its body is left as written, because
  why the desktop came first is part of the record.
- **[ADR 0005](0005-avalonia-ui-toolkit.md) and [ADR 0006](0006-three-source-projects.md) carry dated
  notes.** The toolkit now also targets Android, and the presentation layer now has two heads. Their
  decisions stand.
- **Every increment from here on is built for both heads.** A change to what MoneyBud shows or does is
  one change in the shared layers, laid out twice. A feature that exists on one head only is a defect.
- **The builds must write the same file format version.** A desktop build older than the phone's meets
  the phone's file as written by a newer version and refuses it, touching nothing: the ruled response
  ([§12](../arc42/12-glossary.md), *When the data cannot be read*). So copying the phone's file to the
  desktop needs a desktop built from the same code.
- **The scenarios do not change in kind.** They stay declarative and run against the presentation
  layer, so they specify the phone as they specify the desktop. Where the phone's gesture differs from
  the desktop's (tapping or holding a slice rather than hovering), the scenarios' "point at" covers
  both.
- **The phone head, like the Desktop, is outside the automated tests** unless the plan decides
  otherwise, and some of its failures cannot be seen anywhere but on the phone
  ([§8.5](../arc42/08-crosscutting-concepts.md), [§11](../arc42/11-risks-and-technical-debt.md)).
- **[§7](../arc42/07-deployment-view.md) gains a second node**, the phone, as the place of use. The
  desktop stays a node, for development.
- **[§3.2](../arc42/03-context-and-scope.md)'s sync row is settled as not built**: the file moves by
  hand.
- **Quality goal 2 can finally be served where entry happens**; quality goal 4 is unchanged, since
  neither device talks to anything.
- **What the data file promises, and where it lives on the phone,** are
  [ADR 0014](0014-real-use-and-the-phone-data.md)'s.
- **Reversal would be expensive**, and that is new. From the phone version the stakeholder accepts at
  the end review the data is real (ADR 0014), and the phone is where it lives.
