namespace MoneyBud.Domain;

/// <summary>What one end of a reallocation is (arc42 §12, <i>One act moves an amount of purpose</i>).</summary>
public enum ReallocationEndKind
{
    /// <summary>An account's <i>Vrij</i>: the money on it that no category claims.</summary>
    Unclaimed,

    /// <summary>A category's <i>Opgebouwd</i>: a backed category, or one set to "—" that left money behind.</summary>
    Category,

    /// <summary>A period's <i>Niet toegewezen</i>. Only ever a destination.</summary>
    Unassigned,
}

/// <summary>
/// One end of a reallocation: <i>Vrij</i> on an account, a category, or <i>Niet toegewezen</i>. Two ends
/// are the same end when they are equal, which is what a reallocation between one end and itself is
/// refused for.
/// </summary>
public sealed record ReallocationEnd(ReallocationEndKind Kind, Account? Account = null, Category? Category = null)
{
    public static ReallocationEnd UnclaimedOn(Account account) => new(ReallocationEndKind.Unclaimed, Account: account);

    public static ReallocationEnd For(Category category) => new(ReallocationEndKind.Category, Category: category);

    public static ReallocationEnd Unassigned { get; } = new(ReallocationEndKind.Unassigned);
}

/// <summary>
/// An amount of purpose moved by the user, on screen <i>Verplaatsen</i> (arc42 §12, <i>One act moves an
/// amount of purpose</i>; ADR 0015): from one end to the other, dated the day it was made.
///
/// <para><see cref="FromAccount"/> and <see cref="ToAccount"/> are the accounts the two ends were on at
/// that moment: <i>Vrij</i> is on its own account, a category on its backing account or the account it
/// left money on, and <i>Niet toegewezen</i> on the pool account. They are kept, not worked out again,
/// because a category can be pointed elsewhere and the pool account changed afterwards, and the row must
/// stay in the histories it was written to. <b>When they differ the reallocation itself moves the
/// money</b>, from one to the other, as a transfer does; when they are the same it moves no balance, and
/// is still a row in that account's history (follow-up 9).</para>
///
/// <para><see cref="Amount"/> is a positive magnitude, from <see cref="From"/> to <see cref="To"/>: a
/// negative amount typed is kept as the move the other way. A move to <i>Niet toegewezen</i> belongs to
/// the period its date falls in, like an income. It is never changed or removed: it is undone by
/// moving back.</para>
/// </summary>
public sealed record Reallocation(
    int Id, DateOnly Date, ReallocationEnd From, ReallocationEnd To, Account FromAccount, Account ToAccount, Money Amount)
    : IEntry
{
    /// <summary>Whether money moved between two accounts.</summary>
    public bool MovedMoney => FromAccount != ToAccount;

    /// <summary>What it does to a category's <i>Opgebouwd</i>: plus the amount into it, minus out of it.</summary>
    public Money For(Category category) =>
        To.Category == category ? Amount : From.Category == category ? -Amount : Money.Zero;
}

/// <summary>
/// Why a reallocation was refused. One reason is reported, the first that applies, in this order
/// (arc42 §12, readings of the scenario stage).
/// </summary>
public enum ReallocationRefusal
{
    /// <summary>Van and Naar are the same end.</summary>
    SameEnd,

    /// <summary>
    /// <i>Vrij</i> on one account to <i>Vrij</i> on another: that gives nothing a purpose, and is
    /// <i>Overboeken</i> (ruled by the stakeholder at the build, 2026-10-04).
    /// </summary>
    UnclaimedToUnclaimed,

    /// <summary>The amount is finer than a cent.</summary>
    AmountFinerThanCent,

    /// <summary>The amount would move money out of <i>Niet toegewezen</i>, which is assigning.</summary>
    OutOfUnassigned,

    /// <summary>The amount would move money into an archived category, or one set to "—".</summary>
    IntoGivingEnd,

    /// <summary><i>Niet toegewezen</i> of a period other than the current one.</summary>
    UnassignedNotCurrent,
}

/// <summary>
/// What came of a reallocation: the move made, nothing for zero, or a refusal. <see cref="Into"/> is the
/// category money could not go into, for <see cref="ReallocationRefusal.IntoGivingEnd"/>.
/// </summary>
public sealed record ReallocateResult(Reallocation? Made, ReallocationRefusal? Refusal, Category? Into = null)
{
    public bool WasRefused => Refusal is not null;

    public static ReallocateResult Nothing { get; } = new(null, null);

    public static ReallocateResult Refused(ReallocationRefusal refusal, Category? into = null) => new(null, refusal, into);
}
