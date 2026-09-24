# 11. Risks and Technical Debt

**What belongs here:** Known technical risks and accumulated debt, stated plainly, ideally with
what could be done about each.

This section is only worth anything if it is honest. A risk section listing no real risks tells the
reader the document isn't maintained.

---

| Risk / debt | Impact | Mitigation |
|---|---|---|
| **Manual entry and accurate net worth pull against each other.** An accurate net worth requires every transaction to be recorded; requiring that is exactly the friction that makes people abandon budgeting apps. The app cannot detect a forgotten transaction, because nothing outside it knows what the truth is | High — this is the most likely cause of the product quietly becoming useless | Not solved. Options include letting balances be corrected by hand, or reconciling entered balances against calculated ones and reporting the difference. Bank import would largely remove it, but conflicts with local operation |
| **Single stakeholder, who is also the only user and only developer.** No external party will notice if the product solves the wrong problem | Medium — a wrong direction can run a long way before anything contradicts it | Stakeholder input is recorded verbatim in `docs/stakeholder/` so that later reasoning can be checked against what was actually said, not against what was remembered |
| **Money representation not yet decided** (see [§8.2](08-crosscutting-concepts.md)) | High — expensive to change once real data exists | Decide before the first money-handling code is written. Low urgency while the first increment is a throwaway demo |
| **Deployment form undecided.** Local operation is fixed, but desktop, mobile and the relationship between them are not, so [§7](07-deployment-view.md) cannot be written | Medium — a late choice here could invalidate earlier structural decisions | Decide when the first user-facing increment is specified |
| **The "location" dimension is not in the first increment.** Transactions recorded in the demo will have a category but no account | Low, while the data is throwaway. Would become high if the demo quietly turns into the real thing | Accepted deliberately. Revisit before any data is kept that the stakeholder would mind losing |
