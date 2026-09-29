# MoneyBud mobile prototype

A **stage-1 prototype** of the mobile front-end: how MoneyBud looks and feels on a phone, built so
the stakeholder can react to it before anything is specified or planned. The wishes behind it are in
[`docs/stakeholder/2026-09-29-mobiel.md`](../docs/stakeholder/2026-09-29-mobiel.md).

It is **not the app**. It runs on invented data, saves nothing, and is not connected to
`MoneyBud.Domain` or `MoneyBud.Presentation`. Nothing in it is a ruling: what survives goes back
through the normal stages first. The layout, styling, animations and theme files are what is meant
to carry over; the sample data and its arithmetic (`Sample/`) are not.

## Round 1 — layout, navigation, feel

- The ring is the home screen and never moves; everything is pulled over it. Swiping right brings
  **Inkomsten** in from the left, swiping left brings **Uitgaven** in from the right. Up brings the
  **Budget**, down the **Rekeningen**. A second swipe on income or expenses opens the form; tapping
  an entry opens it filled in. Tapping an account opens its history.
- Swipes start anywhere but on the ring. The ring is for **holding and sliding**: the slice in the
  direction of the finger is shown in the hole (a pizza-shaped hit area), with a tick in the hand
  on the phone. It stays selected after letting go, and the budget then opens on that category.
- Panels follow the finger and spring open or shut; the home screen sinks back and blurs under
  them. Opening draws the ring in, stepping periods drains it and draws the next.
- Every colour is a named resource in `Theme/Default.axaml`, in a dark and a light set that follow
  the phone. The ring is drawn by a replaceable `RingPainter`. Round 2 adds a second theme.

**Tried by Axel on 2026-09-29 and approved** ("that is perfect") after one pass of changes, all from
[his feedback](../docs/stakeholder/2026-09-29-mobiel-prototype-ronde-1.md):

- The period (‹ date ›) moved **under** the ring, and the ring is centred in the room above: swipes
  started too close to the ring's touch zone and sometimes missed. The touch margin went from 30 to
  20 px.
- The home screen **stays exactly still** under the income, expense and account panels: it only
  blurs and darkens. Only the half-open budget sheet moves it, shrinking it to fit above; that is
  deliberate and was not objected to.
- A **＋ Categorie toevoegen** row ends the budget's category list (it had been forgotten).
- A round ＋ (new) or ✓ (save) replaces *Toevoegen*/*Opslaan* in the forms; a small house marks the
  pool account instead of the word *Hoofdrekening*.
- Android's back button on a chosen category goes back to the list first.
- **Vibration follows the phone.** The ticks are sent; Android drops them when touch feedback is off
  in the phone's settings, as Axel's is. His choice: no bypass.

## Round 2 — a second theme (next)

Kintsugi, and switching between themes in *Instellingen* (on the accounts panel, the gear). Axel's
words: "the gold with black is just beautiful". The ensō was set aside because black and white
cannot tell the slices apart; ukiyo-e is kept for later, as it needs illustration.

The idea as proposed to Axel, **not yet agreed in detail**: the ring as the rim of a dark ceramic
bowl, **each slice a different glaze** (celadon, tenmoku brown, indigo, rust, ash) so categories
stay distinguishable, **gold seams** where slices meet, perhaps a gold-filled crack on an overspent
slice, the hole as the bowl's inside. It suits dark mode by nature; what it does in light mode is
open. Put the details to Axel before building.

What round 2 builds on: a theme is a `ResourceDictionary` with the same keys as
`Theme/Default.axaml`, plus a `RingPainter` of its own. Nothing else in the prototype names a colour.

## Two Avalonia traps found here

- **A frame request alone may never get its frame.** `TopLevel.RequestAnimationFrame` waited forever
  just after opening, when nothing else had changed; the opening animation hung until a touch.
  `FrameLoop` now invalidates the visual with every request.
- **A control is only touchable where it draws.** The ring's hole and margin ignored touches until
  `RingView` implemented `ICustomHitTest`.

## Running it

On the PC, in a phone-sized window (the mouse stands in for a finger):

```
dotnet run --project prototype/MoneyBud.Prototype.Desktop
```

On the phone (Android 12 or later, USB debugging on, cable in):

```
dotnet publish prototype/MoneyBud.Prototype.Android -c Release
adb install -r prototype/MoneyBud.Prototype.Android/bin/Release/net10.0-android/publish/app.moneybud.prototype-Signed.apk
```

`adb` is in `%LOCALAPPDATA%\Android\Sdk\platform-tools`; Axel's phone (a Red Magic 11 Pro) shows
as `NX809J`. The Android build wants JDK 17 or 21 — the `java` on the PATH is 25, too new — so the
project points at JDK 21 when it finds it in its usual place. The `android` .NET workload is
installed. Install the **Release** build to judge the feel: it is compiled ahead of time.

Pictures of every screen, without a phone:

```
dotnet run --project prototype/MoneyBud.Prototype.Snapshot -- <folder> dark light
```
