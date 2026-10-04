namespace MoneyBud.Domain;

/// <summary>
/// Everything a <see cref="Ledger"/> holds, as plain data: what is kept between runs (arc42 §8.3,
/// ADR 0007). Nothing else is kept — not the clock, and nothing of the screen. The calendar is kept
/// since the period start day could be changed (<see cref="StartDayChanges"/>).
///
/// <para><b>Categories are referred to by <see cref="CategorySnapshot.Key"/></b>, never by name.
/// Since renaming, a name is not an identity: a renamed category keeps its history, and its old
/// name can belong to a new category (§8.3, <i>How identity is stored</i>). The key is the
/// category's place in the order added, counted from 1, and is made afresh by every
/// <see cref="Ledger.ToSnapshot"/>. It only has to hold within one snapshot, because a snapshot is
/// always the whole ledger.</para>
///
/// <para><b>Accounts are referred to the same way</b>, by <see cref="AccountSnapshot.Key"/>, their
/// place in the order added, and for the same reason: an account can be renamed.
/// <see cref="PoolAccount"/> is the key of the pool account. No balance is kept: a balance is
/// worked out from what is kept (ADR 0008).</para>
///
/// <para>Lists are in the ledger's own order: categories and accounts in the order added, entries
/// in the order recorded. That order is what the Overview breaks ties by and lists newest first by,
/// so it is part of what is kept. The entries' ids are kept too, because the order they were issued
/// in decides what a balance correction has in it.</para>
///
/// <para><b>Since the sweep</b> (ADR 0010): <see cref="SweepDestination"/>, the key of the category
/// a period's leftover goes to, or null; <see cref="PeriodEnds"/>, which categories were backed when
/// each period ended; and <see cref="LetGo"/>, what a period's line stopped asking for.</para>
///
/// <para><b>Since recurring entries</b> (ADR 0011): <see cref="Repeats"/>, every recurring entry with
/// the ids of its occurrences, how often it repeats, the day it was last set to and its next date.</para>
///
/// <para><b>Since the period start day</b> (ADR 0012): <see cref="StartDayChanges"/>, the calendar
/// as a history of changes, empty for periods that have always started on the 1st; and on each
/// backing the first day of the period of each mark.</para>
///
/// <para><b>Since <i>Vrij</i></b> (ADR 0015): <see cref="Reallocations"/>, every move of purpose the user
/// made; on a category set to "—", <see cref="CategorySnapshot.LeftBehind"/>; and on a backing, the
/// stretch before it, <see cref="BackingSnapshot.Earlier"/>. Null in data from before version 8, which
/// had none.</para>
/// </summary>
public sealed record LedgerSnapshot(
    IReadOnlyList<CategorySnapshot> Categories,
    IReadOnlyList<BudgetSnapshot> Budgets,
    IReadOnlyList<ExpenseSnapshot> Expenses,
    IReadOnlyList<IncomeSnapshot> Incomes,
    int LastEntryId,
    IReadOnlyList<AccountSnapshot> Accounts,
    int PoolAccount,
    IReadOnlyList<TransferSnapshot> Transfers,
    IReadOnlyList<BalanceCorrectionSnapshot> BalanceCorrections,
    IReadOnlyList<MovementSnapshot> Movements,
    DateOnly SettledThrough,
    int? SweepDestination,
    IReadOnlyList<PeriodEndSnapshot> PeriodEnds,
    IReadOnlyList<LetGoSnapshot> LetGo,
    IReadOnlyList<RepeatSnapshot> Repeats,
    IReadOnlyList<StartDayChange>? StartDayChanges = null,
    IReadOnlyList<ReallocationSnapshot>? Reallocations = null);

public sealed record AccountSnapshot(int Key, string Name);

public sealed record CategorySnapshot(
    int Key, string Name, bool IsArchived, BackingSnapshot? Backing = null, LeftBehindSnapshot? LeftBehind = null);

/// <summary>
/// A category's backing: the account's key, the two marks and the figure remembered with each
/// (<see cref="Domain.Backing"/>). The figures are null in data from before version 6, which did not
/// keep them, and are then worked out again on loading. So are the first days of the marks' periods
/// in data from before version 7, when every period started on the 1st. <see cref="Earlier"/> is the
/// "—" before it, since version 8.
/// </summary>
public sealed record BackingSnapshot(
    int Account, EntryMark AccumulatingSince, EntryMark HereSince, Money? NotMoved = null, Money? PaidHereBefore = null,
    DateOnly? AccumulatingFrom = null, DateOnly? HereFrom = null, LeftBehindSnapshot? Earlier = null);

/// <summary>
/// A category set to "—" (<see cref="Domain.LeftBehind"/>): the account its older money was left on, the
/// mark drawn then, its period's first day, the amount left, and the backing it ended.
/// </summary>
public sealed record LeftBehindSnapshot(int Account, EntryMark Since, DateOnly From, Money Amount, BackingSnapshot Before);

/// <summary>
/// One end of a reallocation: <see cref="Kind"/> with the key of its account, for <i>Vrij</i>, or of its
/// category.
/// </summary>
public sealed record ReallocationEndSnapshot(ReallocationEndKind Kind, int? Account = null, int? Category = null);

/// <summary>A reallocation (<see cref="Reallocation"/>), with the keys of the accounts its two ends were on.</summary>
public sealed record ReallocationSnapshot(
    int Id, DateOnly Date, ReallocationEndSnapshot From, ReallocationEndSnapshot To, int FromAccount, int ToAccount,
    Money Amount);

/// <summary>
/// A category's <i>Budget</i> in the period that starts on <see cref="PeriodStart"/>. A budget of
/// zero is kept too: it is what <see cref="Ledger.HasBudget"/> tells apart from none.
/// </summary>
public sealed record BudgetSnapshot(int Category, DateOnly PeriodStart, Money Amount);

public sealed record ExpenseSnapshot(int Id, Money Amount, DateOnly Date, int Category, string? Label, int Account);

public sealed record IncomeSnapshot(int Id, Money Amount, DateOnly Date, string Label, int Account);

public sealed record TransferSnapshot(int Id, Money Amount, DateOnly Date, int From, int To);

public sealed record BalanceCorrectionSnapshot(int Id, DateOnly Date, int Account, Money Balance, bool IsStartingBalance);

public sealed record MovementSnapshot(
    int Id, DateOnly Date, int Category, int From, int To, Money Amount,
    MovementReason Reason, MovementDirection Direction, DateOnly? SweptFor = null);

/// <summary>
/// A period that ended while MoneyBud was in use: its first day, and the keys of the categories
/// backed at that moment. A period with none ended before MoneyBud was first started.
/// </summary>
public sealed record PeriodEndSnapshot(DateOnly PeriodStart, IReadOnlyList<int> Backed);

/// <summary>What a period's line stopped asking for, for good: swept too much, and nowhere to come back from.</summary>
public sealed record LetGoSnapshot(DateOnly PeriodStart, Money Amount);

/// <summary>
/// A recurring entry (ADR 0011): the ids of its occurrences, which are expenses or incomes, and — while
/// it runs — how often it repeats, the day a monthly one falls on and the date of the next. A stopped
/// one has none of the three.
/// </summary>
public sealed record RepeatSnapshot(IReadOnlyList<int> Occurrences, Frequency? Frequency, int? Day, DateOnly? Next);
