namespace MoneyBud.Domain;

/// <summary>
/// A transaction that decreases the total (arc42 §12).
///
/// <para><see cref="Id"/> is issued by the <see cref="Ledger"/> when the expense is recorded, and
/// is what tells two otherwise identical expenses apart — the same coffee typed twice is two
/// expenses, and removing one must leave the other (arc42 §12, *An entry can be changed or
/// removed*). A change keeps the id: it rewrites the entry rather than recording a new one, so the
/// entry keeps its place among those on its date (§12, *A change overwrites the entry*).</para>
///
/// <para><see cref="Amount"/> is a positive magnitude; that this is an expense rather than income
/// is what makes it a decrease (ADR 0003, sign convention).</para>
///
/// <para><see cref="Label"/> is the expense's own free-text name — "Albert Heijn" against the
/// category "Groceries" — and is optional. Nothing is derived from it.</para>
///
/// <para><see cref="Account"/> is the account the money left. Unless the user chose another, it is
/// the <i>pool account</i> at the time of recording (arc42 §12, *An expense defaults to the pool
/// account*). No category is backed yet, so the category never supplies an account.</para>
/// </summary>
public sealed record Expense(int Id, Money Amount, DateOnly Date, Category Category, string? Label, Account Account)
    : IEntry;
