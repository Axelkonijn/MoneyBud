namespace MoneyBud.Domain;

/// <summary>
/// A transaction that decreases the total (arc42 §12).
///
/// <para><see cref="Amount"/> is a positive magnitude; that this is an expense rather than income
/// is what makes it a decrease (ADR 0003, sign convention).</para>
///
/// <para><see cref="Label"/> is the expense's own free-text name — "Albert Heijn" against the
/// category "Groceries" — and is optional. Nothing is derived from it.</para>
///
/// <para>An expense has no account. The location dimension is not in the first increment
/// (arc42 §11), which is why §12's *An expense defaults to the pool account* does not apply
/// here yet.</para>
/// </summary>
public sealed record Expense(Money Amount, DateOnly Date, Category Category, string? Label);
