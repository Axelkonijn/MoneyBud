# 10. Quality Requirements

**What belongs here:** A quality tree refining the top goals from section 1, plus concrete
**quality scenarios** that make each one testable.

A quality scenario has a stimulus and a measurable response: "When a month's transactions are
loaded, the summary appears within 200ms" is a quality scenario. "The app should be fast" is not —
it cannot be checked, so it cannot be met or missed.

---

_Not yet filled in. Follows from the quality goals in section 1. The two highest-ranked of those,
legibility and effortless entry, are about what the user sees and does, so for four increments
there was nothing to attach a stimulus and a measurable response to. Writing measurable scenarios
for them against a class library would have been measuring the wrong thing._

_**The UI is now built, so this section is writable, and it is still empty on purpose: no measure
has been agreed with the stakeholder.** How quickly an expense can be entered, or how quickly the
Overview answers "where does my money go", are the obvious candidates. A threshold written here
without him would be invented. It would also look like a requirement that had been thought about,
which is the harm an invented one does. The demo exists to get his reaction
([§1.1](01-introduction-and-goals.md)), and that reaction is where a measure should come from.
[§4](04-solution-strategy.md) says which goals have something built for them. None is measured._

_**The phone (settled 2026-09-29, not built) brings the nearest thing yet to a measure, and it is still
not one.** The stakeholder wants the app to feel premium, "mostly through how quickly it responds", and
the mobile prototype was measured on his phone: a panel dragged at the phone's full 120 frames a second
once Avalonia's GPU budget was raised, against about 24 before, read with `dumpsys SurfaceFlinger
--latency` ([§8.5](08-crosscutting-concepts.md)). He found the speed fine and two swipes to an expense
not too slow ([§12](12-glossary.md), *MoneyBud on the phone*). That is a measurement and a reaction, not
an agreed threshold. "A drag runs at the phone's full frame rate, measured on the device" is the obvious
candidate for the first scenario here, and it waits for him to agree it. Writing it now would be the
invented threshold this note warns against._

## 10.1 Quality Tree

_Empty._

## 10.2 Quality Scenarios

| Quality | Scenario | Measure |
|---|---|---|
| _tbd_ | | |
