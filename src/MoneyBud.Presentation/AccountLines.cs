using MoneyBud.Domain;

namespace MoneyBud.Presentation;

/// <summary>
/// One account in the strip across the top of the Overview: its name, its <i>Saldo</i> today, and
/// whether it is the <i>Hoofdrekening</i> (arc42 §12, <i>Accounts and net worth</i>). The strip is
/// the same in every period, because a balance is about today and not the period on screen.
///
/// <para>An account below zero is overdrawn and carries the one marker over budget and
/// over-assigned carry, with its own badge, <see cref="Tekst.Overdrawn"/>. Exactly zero does not.
/// Nothing is blocked or warned about.</para>
///
/// <para>Since increment 15 every account but the pool account shows <see cref="Unclaimed"/>, on screen
/// <i>Vrij</i>: the money on it no category claims (arc42 §12, ruling 1). Below zero it carries the same
/// marker, badge <i>Rood</i>, on its own (ruling 3): an account can show a negative <i>Vrij</i> without
/// being overdrawn. Null on the pool account, which shows none (ruling 5).</para>
/// </summary>
public sealed record AccountLine(Account Account, Money Balance, bool IsPool, Money? Unclaimed = null)
{
    public string Name => Account.Name;

    public bool IsOverdrawn => Balance.IsNegative;

    public Marker Marker => IsOverdrawn ? Marker.Over : Marker.None;

    public string BalanceText => Tekst.Euro(Balance);

    public bool IsUnclaimedBelowZero => Unclaimed is { IsNegative: true };

    public Marker UnclaimedMarker => IsUnclaimedBelowZero ? Marker.Over : Marker.None;

    /// <summary>"Vrij € 5.000,00", or null on the pool account.</summary>
    public string? UnclaimedText => Unclaimed is { } unclaimed ? Tekst.UnclaimedFigure(unclaimed) : null;

    /// <summary>Shown on the pool account only: <i>Hoofdrekening</i>.</summary>
    public string? PoolText => IsPool ? Tekst.PoolAccount : null;

    /// <summary>Whether the account's history is open, so its block in the strip can show it.</summary>
    public bool IsOpen { get; init; }
}

/// <summary>What one row of an account's history is.</summary>
public enum HistoryKind
{
    StartingBalance,
    BalanceCorrection,
    Transfer,
    Income,
    Expense,
    Movement,
    Reallocation,
}

/// <summary>
/// One row of an account's history, opened by clicking the account in the strip (arc42 §12). The
/// history covers every period, newest first.
///
/// <para>Which acts a row offers is ruled: a <b>transfer</b> can be changed and removed there, a
/// <b>balance correction</b> or <b>starting balance</b> removed and never changed, and an
/// <b>income or expense</b> neither — those are changed and removed from the Overview's lists only
/// (§12, <i>Accounts and net worth</i>). A <b>movement</b> neither: money moved for a category is
/// changed by assigning again, not from the history (§12, <i>Moved money in the account's
/// history</i>).</para>
///
/// <para>A <b>reallocation</b> neither: it is undone by moving back (§12, <i>One act moves an amount of
/// purpose</i>). It is in the history of each account it touches, one within a single account
/// included, though that moves no balance (follow-up 9).</para>
///
/// <para>Amounts are shown without a sign: what a row is says which way the money went. A balance
/// correction shows the balance typed and the difference, worked out afresh, so that it always says
/// how much is still unexplained. A starting balance shows no difference: it corrected
/// nothing.</para>
///
/// <para><paramref name="SweptPeriod"/> is the period a sweep's row names, found through the ledger's
/// own calendar: a sweep keeps only that period's first day, and once the period start day has been
/// changed no calendar but the ledger's can say which period that was (arc42 §12, <i>What else a
/// change meets</i>). Null on every other row.</para>
/// </summary>
public sealed record HistoryLine(IEntry Entry, Account Account, Money? Difference, BudgetPeriod? SweptPeriod = null)
{
    public HistoryKind Kind => Entry switch
    {
        BalanceCorrection { IsStartingBalance: true } => HistoryKind.StartingBalance,
        BalanceCorrection => HistoryKind.BalanceCorrection,
        Transfer => HistoryKind.Transfer,
        Income => HistoryKind.Income,
        Expense => HistoryKind.Expense,
        Movement => HistoryKind.Movement,
        Reallocation => HistoryKind.Reallocation,
        _ => throw new InvalidOperationException($"{Entry.GetType().Name} is not in a history."),
    };

    public DateOnly Date => Entry.Date;

    public string DateText => Tekst.DayName(Date);

    /// <summary>What the row says it is, with the category and label, or the two accounts.</summary>
    public string Text => Tekst.HistoryText(this);

    /// <summary>The entry's amount, or for a typed balance the balance typed. Never signed.</summary>
    public Money? Amount => Entry switch
    {
        Expense e => e.Amount,
        Income i => i.Amount,
        Transfer t => t.Amount,
        Movement m => m.Amount,
        Reallocation r => r.Amount,
        _ => null,
    };

    public Money? Balance => Entry is BalanceCorrection c ? c.Balance : null;

    public string? AmountText => Amount is { } amount ? Tekst.Euro(amount) : null;

    public bool CanChange => Kind == HistoryKind.Transfer;

    public bool CanRemove => Kind is HistoryKind.Transfer or HistoryKind.BalanceCorrection or HistoryKind.StartingBalance;
}
