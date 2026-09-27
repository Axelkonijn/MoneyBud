# Transferring between accounts: money moved by me from one of my accounts to another, such as cash
# taken from an ATM (glossary: Transfer; "Transfers", settled by the stakeholder on 2026-09-27). The
# framing is the stakeholder's own: money leaves Betaalrekening, arrives in Contant, and nothing is
# spent. The steps the accounts files share are explained in show-accounts.feature.
#
# The rules, from arc42 §12:
#   - A transfer, on screen "Overboeking", moves an amount FROM one account TO another, ON A DATE. It
#     lowers the one balance and raises the other.
#   - It is NOT AN INCOME OR AN EXPENSE. It has no category, it counts in no period's Unassigned, it
#     changes no budget figure, and it is not listed among a period's incomes or expenses. It is in
#     the history of both its accounts (show-accounts.feature).
#   - So it leaves NET WORTH as it was, EXCEPT WHERE A BALANCE CORRECTION HAS ALREADY COUNTED ONE
#     SIDE. Each account follows its own balance corrections (correct-a-balance.feature), and net
#     worth is their sum, so a transfer dated before one account's balance correction moves only the
#     other account. RULED BY THE STAKEHOLDER TO BE TRUE, NOT A FLAW: the cash really is in the
#     wallet, and the checked bank balance really had it taken off.
#   - It MAY NOT BE DATED IN THE FUTURE, like an expense: it reports money that has moved.
#   - It needs TWO DIFFERENT ACCOUNTS and an amount ABOVE ZERO, in whole cents (derived). Its
#     direction is carried by FROM and TO. With only one account, no transfer can be made.
#   - It can be CHANGED or REMOVED like an entry (derived): a change is judged as if the transfer
#     were recorded now, a refused change leaves it as it was, removing asks first, and after
#     declining nothing is said. It is changed and removed from an account's history. A changed
#     transfer keeps the moment it was first recorded, like any entry (correct-a-balance.feature).
#   - An overdraft it causes is allowed and shown with the marker (show-accounts.feature).
#
# FOUR THINGS HERE ARE THIS FILE'S CHOICES, NOT RULINGS, and are put to the stakeholder at the
# scenario gate:
#   - A TRANSFER HAS NO LABEL. The glossary left "whether it has a label at all" to this stage. It is
#     named by its accounts, amount and date, which is what an ATM withdrawal is. A label could be
#     added later without changing anything below.
#   - When a transfer breaks more than one rule, I am told the FIRST in this order: two different
#     accounts, then more than zero, then whole cents, then not in the future. It is the order an
#     expense uses: what it is on, then the amount, then the date.
#   - A change is ANNOUNCED ("the transfer was changed"), a removal is announced afterwards, and
#     saving one with NOTHING CHANGED goes through QUIETLY. All three follow "changed or removed
#     like an entry" (change-an-entry.feature, remove-an-entry.feature), and none was asked.
#   - A transfer is in no period's lists, so recording or changing one whatever its date says
#     NOTHING ABOUT WHICH PERIOD it went into, unlike an income or expense landing out of view.
#
# NOT specified here, and left to the plan: where the transfer form sits (the strip, per the
# glossary), and whether the act is offered at all while there is only one account. The scenario
# with one account shows what trying it does.
#
# Reading the steps:
#   - "I record a transfer of N euro from "A" to "B"" is the act, dated today unless "dated on
#     <day>" says otherwise. "I try to record" is the same act where the scenario expects a refusal.
#     "the transfer should (not) be recorded" says whether it went through. "I have recorded a
#     transfer ..." is the same act done earlier, as a Given.
#   - "the transfer of N euro from "A" to "B"" names a transfer by what its history row shows. Every
#     transfer a scenario acts on is the only one with those accounts and that amount.
#   - "I change the transfer ... into a transfer of N euro from "A" to "B" dated on <day>" gives the
#     whole transfer as it would be after the change, like change-an-entry.feature's "into an
#     expense of ...". "I try to change" is the same act where the scenario expects a refusal. "the
#     change should go through (be refused)" and "I save ... without changing anything" are
#     change-an-entry.feature's.
#   - "I remove the transfer ... and confirm" (or "but decline to confirm") is remove-an-entry.feature's
#     grammar.
#   - "net worth is N euro", as a Given, sets nothing. It states what the balances above it add up
#     to, so that the figure before the act is on the page, like correct-a-balance.feature's "the
#     balance of "X" is N euro".
#   - "I should be told that a transfer needs two different accounts", "... must be more than 0 euro"
#     and "... cannot be dated in the future" are the refusals of a transfer. "an amount cannot be
#     finer than a cent" is the one every amount shares.
#   - Every other step is reused unchanged from the file that introduced it, the account steps from
#     show-accounts.feature.
#
# Every scenario starts from an empty ledger, whose one account is "Bank", the pool account, with no
# starting balance and nothing on it (show-accounts.feature). The names and amounts are synthetic
# test data.

@accounts
Feature: Transfer between accounts
  As someone who keeps money in more than one place
  I want to record money moving from one of my accounts to another, such as cash taken from an ATM
  So that both balances stay right, while nothing counts as spent or earned

  # ----------------------------------------------------------------------------------
  # A transfer moves money between two balances, and nothing else
  # ----------------------------------------------------------------------------------

  # The ATM, in the glossary's own figures.
  Scenario: A transfer moves the amount from one balance to the other, and changes neither net worth nor any budget figure
    Given I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 15 euro
    And I have a budget of 400 euro for "Groceries" in the current budget period
    When I record a transfer of 50 euro from "Bank" to "Cash"
    Then the transfer should be recorded
    And I should not be warned or asked to confirm
    And the balance of "Bank" should be 950 euro
    And the balance of "Cash" should be 65 euro
    And net worth should still be 1015 euro
    And Unassigned in the current budget period should still be 600 euro
    And the budget for "Groceries" in the current budget period should still be 400 euro
    And the remaining "Groceries" budget in the current budget period should still be 400 euro
    And the incomes listed in the current budget period should be exactly these, in this order:
      | date  | label   | amount  |
      | today | Salaris | 1000.00 |
    And no expenses should be listed in the current budget period

  Scenario: A transfer is in the history of both its accounts
    Given I have an account "Cash" with a starting balance of 15 euro
    When I record a transfer of 50 euro from "Bank" to "Cash"
    Then the history of "Bank" should be exactly these, newest first:
      | date  | entry    | from | to   | amount |
      | today | transfer | Bank | Cash | 50.00  |
    And the history of "Cash" should be exactly these, newest first:
      | date  | entry            | from | to   | amount | balance |
      | today | transfer         | Bank | Cash | 50.00  |         |
      | today | starting balance |      |      |        | 15.00   |

  # All of Bank's balance is not an overdraft. One cent more is, and it is still recorded without a
  # word (show-accounts.feature).
  Scenario Outline: A transfer moves exactly its amount, even past what the account holds
    Given I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 15 euro
    When I record a transfer of <amount> euro from "Bank" to "Cash"
    Then the transfer should be recorded
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance | overdrawn   |
      | Bank    | <bank>  | <overdrawn> |
      | Cash    | <cash>  | no          |
    And net worth should still be 1015 euro

    Examples:
      | amount  | bank    | cash    | overdrawn |
      | 0.01    | 999.99  | 15.01   | no        |
      | 1000.00 | 0.00    | 1015.00 | no        |
      | 1000.01 | -0.01   | 1015.01 | yes       |

  # A transfer may be dated on any day up to today, past periods included, and it changes no period's
  # figures, whichever period its date is in. Cash was added before any of these dates, so no
  # balance correction has either side in it.
  Scenario Outline: A transfer may be dated on any day up to today, and changes no period's figures
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have an account "Cash" with a starting balance of 15 euro dated on the first day of the previous budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated on the first day of the previous budget period
    When I record a transfer of 50 euro from "Bank" to "Cash" dated on <day>
    Then the transfer should be recorded
    And the balance of "Bank" should be 950 euro
    And the balance of "Cash" should be 65 euro
    And Unassigned in the previous budget period should still be 1000 euro
    And Unassigned in the current budget period should still be 0.00 euro

    Examples:
      | day                                        |
      | today                                      |
      | the first day of the current budget period |
      | the last day of the previous budget period |

  # ----------------------------------------------------------------------------------
  # Across a balance correction
  #
  # The stakeholder's ruling. The ATM withdrawal of yesterday is recorded after one account's balance
  # was corrected today. That balance correction already has the transfer in it, so only the other
  # account moves, and net worth moves with it.
  #   - Bank corrected: the checked bank balance already had the 50 euro taken off, and Cash has had
  #     no balance correction since, so Cash rises. Before, net worth was 50 euro short: the money
  #     had left the bank and arrived nowhere. The transfer puts it back.
  #   - Cash corrected: the counted cash already had the 50 euro in it, and Bank has had no balance
  #     correction since, so Bank falls. Before, net worth was 50 euro over.
  # Net worth is 1015 euro in both rows before the transfer.
  # ----------------------------------------------------------------------------------

  Scenario Outline: A transfer dated before one account's balance correction moves only the other account, and net worth with it
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated on the first day of the current budget period
    And I have an account "Cash" with a starting balance of 15 euro dated on the first day of the current budget period
    And I have corrected the balance of "<corrected>" to <typed> euro
    And net worth is 1015 euro
    When I record a transfer of 50 euro from "Bank" to "Cash" dated yesterday
    Then the transfer should be recorded
    And I should not be warned or asked to confirm
    And the balance of "Bank" should be <bank> euro
    And the balance of "Cash" should be <cash> euro
    And net worth should be <net worth> euro

    Examples:
      | corrected | typed | bank    | cash  | net worth |
      | Bank      | 1000  | 1000.00 | 65.00 | 1065.00   |
      | Cash      | 15    | 950.00  | 15.00 | 965.00    |

  # The same transfer dated today, recorded after the balance correction, is not in it, so both
  # balances move and net worth does not.
  Scenario: A transfer recorded after a balance correction on the same day moves both balances
    Given I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 15 euro
    And I have corrected the balance of "Bank" to 1000 euro
    When I record a transfer of 50 euro from "Bank" to "Cash"
    Then the balance of "Bank" should be 950 euro
    And the balance of "Cash" should be 65 euro
    And net worth should still be 1015 euro

  # ----------------------------------------------------------------------------------
  # What is refused. Nothing is recorded, and no balance moves.
  # ----------------------------------------------------------------------------------

  # With only one account, from and to can only be the same account, so this is what any transfer
  # would meet. Whether the act is offered at all then is for the plan.
  Scenario: A transfer needs two different accounts
    Given I have recorded an income of 1000 euro labelled "Salaris" dated today
    When I try to record a transfer of 50 euro from "Bank" to "Bank"
    Then the transfer should not be recorded
    And I should be told that a transfer needs two different accounts
    And the balance of "Bank" should still be 1000 euro
    And the history of "Bank" should be exactly these, newest first:
      | date  | entry  | label   | amount  |
      | today | income | Salaris | 1000.00 |

  # The same rule an expense has, for the same reason: it reports money that has already moved.
  Scenario Outline: A transfer cannot be dated in the future
    Given my budget periods are one month long
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 15 euro
    When I try to record a transfer of 50 euro from "Bank" to "Cash" dated on <day>
    Then the transfer should not be recorded
    And I should be told that a transfer cannot be dated in the future
    And the balance of "Bank" should still be 1000 euro
    And the balance of "Cash" should still be 15 euro

    Examples:
      | day                                     |
      | tomorrow                                |
      | the first day of the next budget period |

  # Its direction is carried by from and to, so a negative amount is not a transfer the other way.
  # The sign is checked first, so -12.345 is told it must be more than 0 euro.
  Scenario Outline: A transfer amount must be more than zero and whole cents
    Given I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 15 euro
    When I try to record a transfer of <amount> euro from "Bank" to "Cash"
    Then the transfer should not be recorded
    And I should be told that <reason>
    And the balance of "Bank" should still be 1000 euro
    And the balance of "Cash" should still be 15 euro

    Examples:
      | amount  | reason                                |
      | 0.00    | a transfer must be more than 0 euro   |
      | -0.01   | a transfer must be more than 0 euro   |
      | -50.00  | a transfer must be more than 0 euro   |
      | -12.345 | a transfer must be more than 0 euro   |
      | 0.001   | an amount cannot be finer than a cent |
      | 50.005  | an amount cannot be finer than a cent |

  # This file's choice of order, not a ruling (see the header).
  Scenario Outline: A transfer that breaks more than one rule is refused for the first of them
    Given my budget periods are one month long
    And I have an account "Cash" with a starting balance of 15 euro
    When I try to record a transfer of <amount> euro from "<from>" to "<to>" dated tomorrow
    Then the transfer should not be recorded
    And I should be told that <reason>

    Examples:
      | from | to   | amount  | reason                                  |
      | Cash | Cash | -12.345 | a transfer needs two different accounts |
      | Bank | Cash | -12.345 | a transfer must be more than 0 euro     |
      | Bank | Cash | 12.345  | an amount cannot be finer than a cent   |

  # ----------------------------------------------------------------------------------
  # Changing a transfer
  #
  # Anything about it can change: the amount, either account, the direction, the date. Each change
  # takes the old transfer off both its balances and puts the new one on. Net worth stays 6015 euro
  # in every row, because no balance correction is involved.
  # ----------------------------------------------------------------------------------

  Scenario Outline: Changing a transfer moves the balances to match the transfer as changed
    Given I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 15 euro
    And I have an account "Savings" with a starting balance of 5000 euro
    And I have recorded a transfer of 50 euro from "Bank" to "Cash" dated today
    When I change the transfer of 50 euro from "Bank" to "Cash" into <changed>
    Then the change should go through
    And I should be told that the transfer was changed
    And I should not be warned or asked to confirm
    And the accounts should be exactly these, in this order:
      | account | balance   |
      | Bank    | <bank>    |
      | Cash    | <cash>    |
      | Savings | <savings> |
    And net worth should still be 6015 euro

    Examples:
      | changed                                                       | bank    | cash  | savings |
      | a transfer of 60 euro from "Bank" to "Cash" dated on today    | 940.00  | 75.00 | 5000.00 |
      | a transfer of 50 euro from "Savings" to "Cash" dated on today | 1000.00 | 65.00 | 4950.00 |
      | a transfer of 50 euro from "Bank" to "Savings" dated on today | 950.00  | 15.00 | 5050.00 |
      | a transfer of 50 euro from "Cash" to "Bank" dated on today    | 1050.00 | -35.00 | 5000.00 |

  Scenario Outline: A changed transfer must still follow every rule for a transfer, and a refused change leaves it as it was
    Given my budget periods are one month long
    And I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 15 euro
    And I have recorded a transfer of 50 euro from "Bank" to "Cash" dated today
    When I try to change the transfer of 50 euro from "Bank" to "Cash" into <changed>
    Then the change should be refused
    And I should be told that <reason>
    And the balance of "Bank" should still be 950 euro
    And the balance of "Cash" should still be 65 euro

    Examples:
      | changed                                                        | reason                                   |
      | a transfer of 50 euro from "Cash" to "Cash" dated on today     | a transfer needs two different accounts  |
      | a transfer of 0.00 euro from "Bank" to "Cash" dated on today   | a transfer must be more than 0 euro      |
      | a transfer of 50.005 euro from "Bank" to "Cash" dated on today | an amount cannot be finer than a cent    |
      | a transfer of 50 euro from "Bank" to "Cash" dated on tomorrow  | a transfer cannot be dated in the future |

  # The transfer is dated yesterday, and Bank was corrected today, so Bank's balance correction has
  # it in whatever its amount. Changing it moves Cash only, and Bank's difference shows the 10 euro
  # the entries no longer explain.
  Scenario: Changing a transfer that a balance correction already has in it moves only the other account
    Given my budget periods are one month long
    And today is the last day of the current budget period
    And I have recorded an income of 1000 euro labelled "Salaris" dated on the first day of the current budget period
    And I have an account "Cash" with a starting balance of 15 euro dated on the first day of the current budget period
    And I have recorded a transfer of 50 euro from "Bank" to "Cash" dated yesterday
    And I have corrected the balance of "Bank" to 950 euro
    When I change the transfer of 50 euro from "Bank" to "Cash" into a transfer of 60 euro from "Bank" to "Cash" dated on yesterday
    Then the change should go through
    And the balance of "Bank" should still be 950 euro
    And the balance of "Cash" should be 75 euro
    And net worth should be 1025 euro
    And the balance correction of "Bank" to 950 euro should show a difference of 10.00 euro

  Scenario: Saving a transfer with nothing changed goes through quietly
    Given I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 15 euro
    And I have recorded a transfer of 50 euro from "Bank" to "Cash" dated today
    When I save the transfer of 50 euro from "Bank" to "Cash" without changing anything
    Then the change should go through
    And I should not have been told anything
    And the balance of "Bank" should still be 950 euro
    And the balance of "Cash" should still be 65 euro

  # ----------------------------------------------------------------------------------
  # Removing a transfer
  #
  # It loses a record, so it asks first, as removing an entry does.
  # ----------------------------------------------------------------------------------

  Scenario: Removing a transfer asks first, and moves both balances back
    Given I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 15 euro
    And I have recorded a transfer of 50 euro from "Bank" to "Cash" dated today
    When I remove the transfer of 50 euro from "Bank" to "Cash" and confirm
    Then I should have been asked to confirm first
    And I should be told that the transfer was removed
    And the balance of "Bank" should be 1000 euro
    And the balance of "Cash" should be 15 euro
    And the history of "Cash" should be exactly these, newest first:
      | date  | entry            | balance |
      | today | starting balance | 15.00   |

  Scenario: Declining to confirm removes no transfer
    Given I have recorded an income of 1000 euro labelled "Salaris" dated today
    And I have an account "Cash" with a starting balance of 15 euro
    And I have recorded a transfer of 50 euro from "Bank" to "Cash" dated today
    When I remove the transfer of 50 euro from "Bank" to "Cash" but decline to confirm
    Then I should have been asked to confirm first
    And I should not have been told anything
    And the balance of "Bank" should still be 950 euro
    And the balance of "Cash" should still be 65 euro
