# 11. Risks and Technical Debt

**What belongs here:** Known technical risks and accumulated debt, stated plainly, ideally with
what could be done about each.

This section is only worth anything if it is honest. A risk section listing no real risks tells the
reader the document isn't maintained.

---

| Risk / debt | Impact | Mitigation |
|---|---|---|
| Money representation not yet decided (see [§8.2](08-crosscutting-concepts.md)) | High — expensive to change once data exists | Decide before the first money-handling code is written |
| UI form undecided, so section 7 cannot be written | Low — deferring costs little at this stage | Decide when the first user-facing feature is specified |
