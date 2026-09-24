# 6. Runtime View

**What belongs here:** How the building blocks actually collaborate, for a few important
scenarios: the main use case, a tricky edge case, startup, error handling. Sequence diagrams work
well.

Cover the scenarios that are *hard to infer* from the static view. Documenting every interaction
is wasted effort — pick the ones where the collaboration is non-obvious.

---

_Not yet filled in, and now for a concrete reason: [§5](05-building-block-view.md) has one building
block that does anything, so there is no collaboration between components to describe. This becomes
writable when a second one exists — a UI, or something that stores data._

> Note: "scenario" here means a runtime interaction between components, which is a different thing
> from a [Gherkin scenario](../../features/). Gherkin describes user-observable behaviour; this
> section describes internal collaboration.
