namespace MoneyBud.Domain;

/// <summary>
/// A place where money actually sits: a current account, a savings account, cash (arc42 §12,
/// <i>Account</i>). Answers <i>where</i>, where a <see cref="Category"/> answers <i>what for</i>.
///
/// <para>An account is a name and what is on it. There are no account kinds: nothing in MoneyBud
/// behaves differently for a savings account than for cash, so a kind would be a field nothing
/// reads (§12, <i>Accounts and net worth</i>).</para>
///
/// <para><b>Its balance is not on this type</b>, and not stored anywhere. It is worked out by the
/// <see cref="Ledger"/> from what is on the account — its typed balances, incomes, expenses and
/// transfers (<see cref="Ledger.BalanceOf"/>). A balance written down as a number of its own would
/// be a second author of the same figure, which is the risk §11 asked to be settled before
/// accounts were built (ADR 0008).</para>
///
/// <para>Like a category, an account has <b>identity</b>: the ledger makes one instance per account
/// and hands it out everywhere, so a rename is one assignment and every entry on the account follows.
/// Only the ledger sets the name, because only the ledger keeps its index of names in step. Whether
/// it is the <i>pool account</i> is a fact about the ledger, not about the account.</para>
/// </summary>
public sealed class Account
{
    internal Account(string name) => Name = name;

    public string Name { get; internal set; }

    public override string ToString() => Name;
}
