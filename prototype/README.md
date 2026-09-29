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

## Round 2 — a second theme: kintsugi

A first version was built on 2026-09-29 from [Axel's answers](../docs/stakeholder/2026-09-29-mobiel-prototype-ronde-2.md):
new colours, a glazed ring with gold seams, gold hairlines, a serif for headings. **Axel's verdict:**
it looked cool but was not what he meant by a theme, and he disliked the ring, "a weird matte tube
with squiggly lines" ([his feedback](../docs/stakeholder/2026-09-29-mobiel-prototype-ronde-2-feedback.md)).
A theme should be a different world: for kintsugi, the home screen a **table** and every panel a
**mended porcelain slab** pulled onto it. The ensō was set aside earlier because black and white
cannot tell the slices apart; ukiyo-e is kept for later, as it needs illustration.

**Rebuilt, ring first** (his order): the ring and the table, then the panels as slabs. **Tried by
Axel on 2026-09-29 and approved** ("this is great"), after two rounds of changes to the plate and a
performance fix.

- **The table:** dark is an oiled walnut, light a pale hinoki, from CC0 photo textures graded for
  the phone (`Theme/Kintsugi/`, sources in its README). A warm lamp light over the bowl.
- **The ring is a porcelain plate lying on it**, seen from above and lit from the top left: a flat
  well where the figures are, a soft rise to a gently sloping rim that carries the shards, a
  rounded lip, and a short soft shadow on the table. It was a bowl first; Axel found it read better
  as a plate ([his feedback](../docs/stakeholder/2026-09-29-mobiel-prototype-ronde-2-feedback-3.md)). Black
  porcelain in dark, white in light. Drawn per pixel by a **Skia shader**
  (`Ring/KintsugiRingPainter.cs`); flat shapes and gradients were what made the first one a tube.
  What never changes for a bowl of a given size (its light, the veins, how the breaks wander) is
  worked out once in the background, about half a second on the phone, and a light shader combines
  it with the slices each frame.
- **Each shard is glazed as far as it is spent** and bare porcelain beyond (his choice);
  *Niet toegewezen* is raw clay. Gold-mended breaks run between the shards, and **finer gold veins
  run through every piece**, grown from a crack pattern rather than drawn. An overspent shard's
  veins turn red lacquer, with a lacquered break down its middle.
- **The gold runs right across the bowl**, bottom and all, under the figures
  ([Axel's choice](../docs/stakeholder/2026-09-29-mobiel-prototype-ronde-2-feedback-2.md)): an
  earlier version faded it out before the middle, which read as a haze in light mode and a black
  pit in dark mode. Breaks between shards carry on into the bowl and end in tips. In dark mode a
  tight shadow on the figures (`HoleEffect`), a soft dark oval behind them (`HoleBackdrop`) and a
  lighter `Faint` keep them readable; light mode needs none of it.
- **Pointing at a shard** fades the others toward bare porcelain.
- **The one kintsugi moment** stays: after the ring draws in, the gold runs into the breaks and
  then the veins, going round the bowl.
- If a device cannot compile the shader, the plain default ring is drawn instead.

- **Every panel is a porcelain slab** lying on the table (`Theme/SlabPainter.cs`, drawn by
  `Views/Surface.cs`, which is plain glass in the default theme): a little smaller than the
  screen, its edges uneven and chipped, lit like the plate, with a soft shadow, and two or three
  gold-mended breaks across it with finer branches; a pop-up gets one or two. Each slab is drawn
  once into a picture and reused; drawn as shapes every frame it was far too slow. The picture is
  made at 96 dpi and scaled by hand: at the screen's own dpi it came out magnified on the phone and
  looked right on the desktop, where the two are the same. Black porcelain in
  dark, white in light. **The table stays sharp** under a slab and only darkens a little
  (`HomeBlur`, `HomeDim`), where glass blurs the home screen.

Kept from the first version: switching in *Instellingen* (the gear on the accounts panel) with a
cross-fade, not remembered between starts; gold accents; Cormorant Garamond for headings.

**Dropped:** a third choice with kintsugi's own motions throughout (the bowl breaking into shards
when stepping periods, for instance), Axel's idea. At first kept for a small round of its own; after
round 2 he dropped it — he will bring it up again if he wants it
([na het prototype](../docs/stakeholder/2026-09-29-mobiel-na-het-prototype.md)).

**The prototype is closed** (2026-09-29). What it taught is in arc42 — §12 *MoneyBud on the phone*,
§8.5 *Drawing on the phone*, ADRs 0013 and 0014 — and the real phone app is built from those, not
from this code. Its layout, styling, animations and theme files are what is meant to carry over.

How a theme is built: a `ResourceDictionary` with the same keys as `Theme/Default.axaml`, and a
`RingPainter` of its own, named together in `Theme/Looks.cs`. Nothing else in the prototype names a
colour. `PanelEdge`, `CardEdge`, `GrainOpacity`, `HeadingFont`, `H1Size` and `H2Size` exist for
kintsugi; the default sets them to leave its look as it was. `Bg` may be an image brush, as
kintsugi's table is. A painter asks for colours only it uses (kintsugi's `BowlColor`, `SeamColor`,
`LacquerColor`) through `RingFrame.Named`, and may use `RingFrame.Mend`, the finishing touch after
the ring is drawn in.

## Four Avalonia traps found here

- **Avalonia's GPU budget is small for screen-sized pictures.** Kintsugi keeps several on the
  GPU: the table, the slabs, the plate's textures. Past Avalonia's default budget, pictures are
  pushed out and sent again every frame, which held a drag to about 24 frames a second. The Android
  app now allows 256 MB (`SkiaOptions.MaxGpuResourceSizeBytes` in `MainActivity.cs`); a drag runs at
  the phone's full 120. Measured with `dumpsys SurfaceFlinger --latency` on the app's layer, which
  gives the frames that really reached the screen, rather than a frame counter in the app.

- **On the phone, drawing happens on a render thread.** A custom draw operation's `Render` runs
  there on Android, but on the UI thread on the desktop and in a headless run, so a mistake shows
  only on the phone. Reading a theme resource from it throws; every frame of the first shader ring
  did, which froze the screen. Read everything a drawing needs in `Paint`, before handing it over.

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

To start it in a theme straight from the PC (a swipe cannot be sent over `adb` here, only taps):

```
adb shell am start -n app.moneybud.prototype/crc6478116d77fa210a8f.MainActivity --es look Kintsugi
```

`adb` is in `%LOCALAPPDATA%\Android\Sdk\platform-tools`; Axel's phone (a Red Magic 11 Pro) shows
as `NX809J`. The Android build wants JDK 17 or 21 — the `java` on the PATH is 25, too new — so the
project points at JDK 21 when it finds it in its usual place. The `android` .NET workload is
installed. Install the **Release** build to judge the feel: it is compiled ahead of time.

Pictures of every screen, without a phone:

```
dotnet run --project prototype/MoneyBud.Prototype.Snapshot -- <folder> dark light kintsugi-dark kintsugi-light
```

A headless run is slow, so its pictures cannot be trusted for anything under a second or so: one
taken 0.1 seconds into the half-second theme fade showed the fade already over. Judge timing on the
phone. Kintsugi's shader ring renders very slowly without a GPU and animations advance per frame,
so give it `SLOW=12` to see where the animations end.
