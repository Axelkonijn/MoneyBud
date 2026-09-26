namespace MoneyBud.Domain;

/// <summary>
/// Everything a <see cref="Ledger"/> holds, as plain data: what is kept between runs (arc42 §8.3,
/// ADR 0007). Nothing else is kept — not the clock, not the calendar, and nothing of the screen.
///
/// <para><b>Categories are referred to by <see cref="CategorySnapshot.Key"/></b>, never by name.
/// Since renaming, a name is not an identity: a renamed category keeps its history, and its old
/// name can belong to a new category (§8.3, <i>How identity is stored</i>). The key is the
/// category's place in the order added, counted from 1, and is made afresh by every
/// <see cref="Ledger.ToSnapshot"/>. It only has to hold within one snapshot, because a snapshot is
/// always the whole ledger.</para>
///
/// <para>Lists are in the ledger's own order: categories in the order added, entries in the order
/// recorded. That order is what the Overview breaks ties by and lists newest first by, so it is
/// part of what is kept.</para>
/// </summary>
public sealed record LedgerSnapshot(
    IReadOnlyList<CategorySnapshot> Categories,
    IReadOnlyList<BudgetSnapshot> Budgets,
    IReadOnlyList<ExpenseSnapshot> Expenses,
    IReadOnlyList<IncomeSnapshot> Incomes,
    int LastEntryId);

public sealed record CategorySnapshot(int Key, string Name, bool IsArchived);

/// <summary>
/// A category's <i>Budget</i> in the period that starts on <see cref="PeriodStart"/>. A budget of
/// zero is kept too: it is what <see cref="Ledger.HasBudget"/> tells apart from none.
/// </summary>
public sealed record BudgetSnapshot(int Category, DateOnly PeriodStart, Money Amount);

public sealed record ExpenseSnapshot(int Id, Money Amount, DateOnly Date, int Category, string? Label);

public sealed record IncomeSnapshot(int Id, Money Amount, DateOnly Date, string Label);
