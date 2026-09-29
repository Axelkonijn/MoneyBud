# 1. Introduction and Goals

**What belongs here:** A short description of what MoneyBud does and for whom. The three to five
**top quality goals** — the qualities that matter most to stakeholders and that architecture must
serve. And a stakeholder table: who cares about this system, and what they expect from it.

The quality goals are the important part. They are what later architectural decisions get judged
against, so keep the list short and ranked — a list of eight goals is a list of none.

---

## 1.1 Requirements Overview

MoneyBud is a personal budgeting application for a single user, running locally on their own
machine. It answers two questions:

- **Where does my money go?** — income, spending, and how much is left in each category
- **How am I doing?** — total net worth across all accounts

These are not two separate features. Every amount has both a **location** (which account it sits
in) and a **purpose** (which category it is earmarked for, or *Unassigned* until the user decides),
and those vary independently. Net worth
is the same data grouped by location; the budget is that data grouped by purpose. One model, two
views. See [§12 Glossary](12-glossary.md) for the precise definitions.

### Why it exists

The stakeholder is in his mid-twenties, does not currently budget, and expects to move out within a
few years — which will demand considerably more attention to money. He was prompted by personal
finance YouTubers, Caleb Hammer in particular.

His stated motivation is worth recording exactly, because it drives the quality goals: he wants not
only a **better overview**, but to be **more motivated** to handle money well. Existing budgeting
apps deliver the former. The latter is what he is actually after, and he was explicit that clarity
and feedback are what produce it.

He is aware that commercial alternatives exist. Building his own is a deliberate choice, so that it
fits his own needs.

MoneyBud is a hobby project. It follows a professional way of working because that is useful and
because the stakeholder wants the practice, but it is not a course assignment and there is no
assessor whose requirements have to be met.

### Core capabilities

| Capability | |
|---|---|
| Record income and expenses, one-off or recurring, each labelled and each given a purpose — an expense as it is entered, income later, by assigning it ([§12](12-glossary.md)) | |
| Assign monthly amounts to categories, with sensible defaults and easy customisation | |
| See at a glance where money is going, and how the period compares to the budget and to earlier periods | |
| Track balances across accounts and see total net worth | |
| Direct what is left over at the end of a period somewhere useful rather than losing it | |

### The first version

The stakeholder works in SCRUM terms: the first version is **a working demo to give feedback on,
not an MVP**. It covers manual entry of income and expenses, and categories with amounts.

**That first version takes more than one increment to build, and the two words are not
interchangeable.** The **first increment** built recording an *expense* and nothing else; recording
*income* was the second, and is now built too. Where [§12](12-glossary.md) says "not in the first
increment" it means that increment, not the demo as a whole — so a thing absent from it may still
be part of the first version. Other sections now say "not built yet" or "in any increment so far"
instead, because the count has moved past one and a claim pinned to the first increment stops
telling the reader what is true today.

**The fifth increment is the first with a screen, and it is built.** The four before it (expenses,
income, categories, assigning) built a domain library with no user interface. The fifth puts a
desktop UI, in Dutch, over everything the domain does, and so turns the library into the demo
[ADR 0002](../decisions/0002-desktop-application-first.md) is about. Its start screen is the one
round 3 asked for: *"waar mijn geld heen gaat — het radiale diagram"*, the Overview headed by a ring
that shows each category's plan and spending in one picture. As first built, it kept nothing when
it closed. **Keeping data came two increments later**, settled with the stakeholder and built on
2026-09-26 ([§8.3](08-crosscutting-concepts.md)). It does not end the demo: the data kept is still
demo data, and the switch to real use is expected to follow it rather than come with it
([§12](12-glossary.md), *What MoneyBud keeps*). What it covers, and how the ring is drawn, are in
[§12](12-glossary.md), *The user interface*. It is built with Avalonia
([ADR 0005](../decisions/0005-avalonia-ui-toolkit.md)), over a presentation layer that holds
everything the screen decides ([ADR 0006](../decisions/0006-three-source-projects.md)).

Accounts, net worth, and recurring transactions are later increments. They are deferred, not
dropped — the model above describes MoneyBud as intended, and the documentation should not be read
as if any one increment is the product. **Recurring transactions in particular do not replace
one-off entry**: the capability table above lists the two together because the stakeholder wants
both, permanently, and entering an amount by hand stays first-class once schedules exist
([§12](12-glossary.md), *Income may be dated in the future*).

**Accounts and net worth are built**, in the ninth increment (settled, specified and built on
2026-09-27; [§12](12-glossary.md), *Accounts and net worth*): accounts with balances, every income
and expense on an account, net worth on screen, and transfers between accounts. So the second
question above, "how am I doing?", has an answer on screen for the first time. A balance is worked out
from the entries, never stored ([ADR 0008](../decisions/0008-balance-is-worked-out.md)). Backing a
category with an account comes in the next increment, and the end-of-period sweep after that.
**Backing is built**, in the tenth increment (settled, specified and built on 2026-09-27;
[§12](12-glossary.md), *Backing and Accumulated*): one backing account per category or none,
assigning to a backed category really moving money, and its *Accumulated* (*Opgebouwd*) shown on its
row. So a savings goal now has progress on screen, the figure the "moving out" goal above is measured
against. The money MoneyBud moves is written as entries, never as balances
([ADR 0009](../decisions/0009-movements-are-entries.md)). The end-of-period sweep is next.
**The sweep is built**, in the eleventh increment (settled 2026-09-27, specified and built by
2026-09-28; [§12](12-glossary.md), *The sweep and Restant*): at a period's end, what is left of it
moves into one chosen backed category, and a period that changes later shows the difference, which
one button moves. So money left over at a period end no longer vanishes, and the capability "direct what
is left over at the end of a period somewhere useful rather than losing it" is built ([ADR 0010](../decisions/0010-sweeps-and-period-ends.md)).
**Recurring entries are next**, the twelfth increment, chosen by the stakeholder on 2026-09-28 as the
first of three: then a configurable period start day, because his salary comes on the 27th, then a
mobile front-end. Its rulings are settled, not yet specified or built ([§12](12-glossary.md),
*Recurring entries*): an income or expense can repeat weekly or monthly, set by one drop-down that
defaults to *Eenmalig*, so one-off entry stays the default and stays first-class, as the table above
says. Each occurrence is an ordinary entry that MoneyBud records on its own date, and the latest one
sets the next, so a changed price is adjusted without starting a new series, as round 1 asked.
**Recurring entries are built** (specified and built on 2026-09-28, [ADR 0011](../decisions/0011-recurring-entries.md)).
**A configurable period start day is next**, the thirteenth increment, settled with the stakeholder on
2026-09-29 and not yet specified or built ([§12](12-glossary.md), *A configurable period start day*).
His salary comes on the 27th, so he wants periods to run from payday. The start day can be changed at
any time, beside the period's name, and applies from the current period on, so a salary that has just
landed is in the period it pays for. That serves the first question above, "where does my money go?",
for the span he actually lives by, rather than for a calendar month his pay does not follow.
**A configurable period start day is built** (specified and built on 2026-09-29,
[ADR 0012](../decisions/0012-the-calendar-is-a-history.md)). A mobile front-end is the last of the
three.

**A mobile front-end is next, and with it the demo ends**, the fourteenth increment, settled with the
stakeholder on 2026-09-29 and not yet specified or built ([§12](12-glossary.md), *MoneyBud on the
phone*). It is the one he named as what would make him actually use MoneyBud. **An Android app for his
own phone**, phone only, with everything the desktop does, laid out for a phone: the Overview becomes
the ring alone, a still home screen that income, expenses, the budget and the accounts are pulled over
by swiping. The desktop stays, for development, and the two builds stay the same, except that only the
phone has themes. **Real use starts with it**: from the version he accepts at the end review, every later
version reads the data it saved, and it starts fresh with the six default categories
([ADR 0013](../decisions/0013-an-android-phone-app.md), [ADR 0014](../decisions/0014-real-use-and-the-phone-data.md)).
That ends "a working demo, not an MVP" above, as the stakeholder chose, and it serves effortless entry,
quality goal 2, on the device where entry happens. **This increment is run without its two approval
gates**, by his ruling: he reviews the scenarios, the plan and the app together at the end.

## 1.2 Quality Goals

Ranked. These are what architectural decisions get judged against.

| # | Quality | Motivation |
|---|---|---|
| 1 | **Legibility** — the state of your money is clear at a glance | The stakeholder raised this himself at the end of the interview, saying it mattered more than he had made it sound. It is also the mechanism by which the app is meant to motivate: seeing clearly where you stand is what makes you act on it. |
| 2 | **Effortless entry** — recording something takes almost no work | Repeated for income, categories and expenses alike ("geen gedoe"). Since everything is entered by hand, friction here is what would make the app get abandoned, and that failure would make every other quality irrelevant. |
| 3 | **Adaptability** — both the data and the software are easy to change | Stated as "heel belangrijk". Two distinct things: amounts and recurring entries must be adjustable without starting over, and the application must be easy to extend as the stakeholder discovers what he wants while using it. |
| 4 | **Local operation** — it runs on your own machine and your data stays there | Preferred explicitly over a web application, even though a web app would more easily serve both desktop and mobile. Since 2026-09-29 "your own machine" is his own phone, with no sync to the desktop ([§12](12-glossary.md), *MoneyBud on the phone*). |

Goal 2 is in tension with the net-worth half of goal 1: an accurate net worth requires every
transaction to be entered, and requiring that is itself friction. See
[§11](11-risks-and-technical-debt.md).

## 1.3 Stakeholders

MoneyBud has one stakeholder, who is also the only user and the only developer. This is worth
stating plainly: there is no external party who will notice if the product is wrong. See
[§11](11-risks-and-technical-debt.md).

| Role | Name | Expectations |
|---|---|---|
| Stakeholder, user and developer | Axel | An app built around his own needs rather than someone else's; clear insight into income, spending and net worth; enough motivation from that insight to actually manage his money well. As developer, also a codebase that stays pleasant to extend. |
