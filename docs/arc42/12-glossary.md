# 12. Glossary

**What belongs here:** Domain and technical terms, defined once. In a budgeting app this matters
more than usual — words like "account", "balance", "category" and "budget" all carry everyday
meanings that are close to, but not the same as, what they mean in the system.

Gherkin scenarios should use exactly the vocabulary defined here. Where they disagree, one of the
two is wrong.

---

## The central distinction

Every amount of money in MoneyBud has **two independent properties at once**:

| Dimension | Question it answers | Expressed as |
|---|---|---|
| **Location** | Where is this money? | An **Account** |
| **Purpose** | What is this money for? | A **Category** |

These vary independently. Two amounts in the same account can have different purposes, and one
purpose can be spread across several accounts.

This is what makes MoneyBud's two halves one system rather than two: **net worth** is everything
grouped by location, **budget** is everything grouped by purpose.

## Terms

| Term | Definition |
|---|---|
| **Account** | A place where money actually sits. Current account, savings account, investment account, or cash. Answers *where*. Cash is modelled as an account despite not being a bank account. |
| **Location** | The dimension answered by "which account". Not a separate entity — a way of grouping. |
| **Category** | What money is earmarked for: groceries, hobby, moving out. Answers *what for*. A category is a label and exists independently of any amount assigned to it. |
| **Purpose** | The dimension answered by "which category". Not a separate entity — a way of grouping. |
| **Budget** | The amount assigned to one category for one budget period. "€400 for groceries in October" is a budget; "groceries" on its own is a category. |
| **Budget period** | The span a budget covers — normally a month. The day it starts is configurable, so it does not necessarily align with a calendar month. |
| **Transaction** | A single movement of money, with an amount, a date, an account and a category. Income and expenses are both transactions. |
| **Income** | A transaction that increases the total. May be one-off or recurring. |
| **Expense** | A transaction that decreases the total. May be one-off or recurring. |
| **Recurring transaction** | An income or expense that repeats on a schedule — weekly, monthly, yearly. Not part of the first increment. |
| **Remaining** | For a category in a budget period: the budget minus what has been spent against it. |
| **Leftover** | What remains in a category when a budget period ends. The stakeholder wants to direct this somewhere rather than let it vanish. |
| **Net worth** | The sum of the balances of all accounts. The "how am I doing" figure. |
| **Balance** | How much is in one account. |

## Dutch source terms

The stakeholder material is in Dutch. This table fixes the mapping, so that reading the interviews
alongside this documentation does not introduce drift.

| Dutch (stakeholder) | English (this project) |
|---|---|
| Potje | Category, together with its Budget for the current period |
| Plek | Location — expressed as an Account |
| Doel | Purpose — expressed as a Category |
| Rekening | Account |
| Vermogen | Net worth |
| Inkomsten / Uitgaven | Income / Expenses |
| Overzichtelijk | Legible, clear at a glance — see quality goal 1 in [§1](01-introduction-and-goals.md) |

## Open question

**Money that has a location but no purpose yet.** When income arrives it sits in an account before
it has been assigned to any category. The model needs a name for that state — provisionally
*unassigned* — but the stakeholder has not been asked how it should behave, or whether it should be
visible at all. Raised rather than decided.
