namespace MoneyBud.Domain;

/// <summary>
/// What came of adding an account: it was added, or refused for one reason (arc42 §12,
/// <i>Accounts and net worth</i>).
///
/// <para>Unlike a category, a name already taken is <b>refused</b>, not handed back. Handing the
/// existing account back would quietly drop the starting balance just typed, or overwrite the one
/// the account had.</para>
/// </summary>
public sealed record AddAccountResult
{
    private AddAccountResult(Account? account, AccountRefusal? refusal)
    {
        Account = account;
        Refusal = refusal;
    }

    public Account? Account { get; }

    public AccountRefusal? Refusal { get; }

    public bool WasRefused => Refusal is not null;

    public static AddAccountResult Added(Account account) => new(account, null);

    public static AddAccountResult Refused(AccountRefusal refusal) => new(null, refusal);
}

/// <summary>
/// Why adding an account was refused, in the order the first one broken is reported: the name,
/// then the starting balance. A reason, not a message.
/// </summary>
public enum AccountRefusal
{
    /// <summary>The name trimmed to nothing.</summary>
    NameMissing,

    /// <summary>Another account already has the name under the name rule.</summary>
    NameTaken,

    /// <summary>The starting balance was finer than a cent. Refused, never rounded (arc42 §8.2).</summary>
    AmountFinerThanCent,
}

/// <summary>
/// What came of renaming an account. It mirrors <see cref="RenameCategoryResult"/>, with the same
/// outcomes and refusals: a name another <i>account</i> has is taken, and a category's name is not,
/// because an account and a category are different dimensions (arc42 §12).
/// </summary>
public sealed record RenameAccountResult
{
    private RenameAccountResult(Account? account, string? oldName, RenameOutcome? outcome, RenameRefusal? refusal)
    {
        Account = account;
        OldName = oldName;
        Outcome = outcome;
        Refusal = refusal;
    }

    public Account? Account { get; }

    public string? OldName { get; }

    public RenameOutcome? Outcome { get; }

    public RenameRefusal? Refusal { get; }

    public bool WasRefused => Refusal is not null;

    public static RenameAccountResult Renamed(string oldName, Account account) =>
        new(account, oldName, RenameOutcome.Renamed, null);

    public static RenameAccountResult Unchanged(Account account) =>
        new(account, account.Name, RenameOutcome.Unchanged, null);

    public static RenameAccountResult Refused(RenameRefusal refusal) => new(null, null, null, refusal);
}

/// <summary>
/// Why a transfer was refused, in the order the first one broken is reported. A reason, not a
/// message.
/// </summary>
public enum TransferRefusal
{
    /// <summary>From and to are the same account: nothing would move.</summary>
    SameAccount,

    /// <summary>The amount was zero or negative.</summary>
    AmountNotPositive,

    /// <summary>The amount was finer than a cent. Refused, never rounded (arc42 §8.2).</summary>
    AmountFinerThanCent,

    /// <summary>
    /// Dated after today. A transfer reports money that has moved, as an expense does; only an
    /// income may be dated ahead, because nothing else plans income (arc42 §12).
    /// </summary>
    DateInFuture,
}

/// <summary>What came of recording a transfer: recorded, or refused for one reason.</summary>
public sealed record RecordTransferResult
{
    private RecordTransferResult(Transfer? transfer, TransferRefusal? refusal)
    {
        Transfer = transfer;
        Refusal = refusal;
    }

    public Transfer? Transfer { get; }

    public TransferRefusal? Refusal { get; }

    public bool WasRecorded => Transfer is not null;

    public static RecordTransferResult Recorded(Transfer transfer) => new(transfer, null);

    public static RecordTransferResult Refused(TransferRefusal refusal) => new(null, refusal);
}

/// <summary>
/// What came of changing a transfer: changed, unchanged, or refused for one of recording's reasons —
/// as <see cref="ChangeIncomeResult"/> for an income.
/// </summary>
public sealed record ChangeTransferResult
{
    private ChangeTransferResult(Transfer? transfer, ChangeOutcome? outcome, TransferRefusal? refusal)
    {
        Transfer = transfer;
        Outcome = outcome;
        Refusal = refusal;
    }

    public Transfer? Transfer { get; }

    public ChangeOutcome? Outcome { get; }

    public TransferRefusal? Refusal { get; }

    public bool WasRefused => Refusal is not null;

    public static ChangeTransferResult Changed(Transfer transfer) => new(transfer, ChangeOutcome.Changed, null);

    public static ChangeTransferResult Unchanged(Transfer transfer) => new(transfer, ChangeOutcome.Unchanged, null);

    public static ChangeTransferResult Refused(TransferRefusal refusal) => new(null, null, refusal);
}

/// <summary>
/// What came of correcting a balance: recorded, or refused because the balance was finer than a
/// cent — the one rule a typed balance can break. Zero and below zero are balances.
/// </summary>
public sealed record CorrectBalanceResult
{
    private CorrectBalanceResult(BalanceCorrection? correction)
    {
        Correction = correction;
    }

    public BalanceCorrection? Correction { get; }

    public bool WasRecorded => Correction is not null;

    public static CorrectBalanceResult Recorded(BalanceCorrection correction) => new(correction);

    public static CorrectBalanceResult RefusedFinerThanCent() => new((BalanceCorrection?)null);
}
