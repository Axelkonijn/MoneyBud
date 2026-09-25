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

**The fifth increment is the first with a screen.** The four before it (expenses, income,
categories, assigning) built a domain library with no user interface. The fifth puts a desktop UI,
in Dutch, over everything the domain does, and so turns the library into the demo
[ADR 0002](../decisions/0002-desktop-application-first.md) is about. Its start screen is the one
round 3 asked for: *"waar mijn geld heen gaat — het radiale diagram"*, the Overview headed by a ring
that shows each category's plan and spending in one picture. It keeps nothing when it closes
([§8.3](08-crosscutting-concepts.md)). What it covers, and how the ring is drawn, are in
[§12](12-glossary.md), *The user interface*.

Accounts, net worth, and recurring transactions are later increments. They are deferred, not
dropped — the model above describes MoneyBud as intended, and the documentation should not be read
as if any one increment is the product. **Recurring transactions in particular do not replace
one-off entry**: the capability table above lists the two together because the stakeholder wants
both, permanently, and entering an amount by hand stays first-class once schedules exist
([§12](12-glossary.md), *Income may be dated in the future*).

## 1.2 Quality Goals

Ranked. These are what architectural decisions get judged against.

| # | Quality | Motivation |
|---|---|---|
| 1 | **Legibility** — the state of your money is clear at a glance | The stakeholder raised this himself at the end of the interview, saying it mattered more than he had made it sound. It is also the mechanism by which the app is meant to motivate: seeing clearly where you stand is what makes you act on it. |
| 2 | **Effortless entry** — recording something takes almost no work | Repeated for income, categories and expenses alike ("geen gedoe"). Since everything is entered by hand, friction here is what would make the app get abandoned, and that failure would make every other quality irrelevant. |
| 3 | **Adaptability** — both the data and the software are easy to change | Stated as "heel belangrijk". Two distinct things: amounts and recurring entries must be adjustable without starting over, and the application must be easy to extend as the stakeholder discovers what he wants while using it. |
| 4 | **Local operation** — it runs on your own machine and your data stays there | Preferred explicitly over a web application, even though a web app would more easily serve both desktop and mobile. |

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
