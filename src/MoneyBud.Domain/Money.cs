using System.Globalization;

namespace MoneyBud.Domain;

/// <summary>
/// An amount of money, held as a whole number of cents. See arc42 §8.2 and ADR 0003.
///
/// <para>Signed. A <see cref="Ledger.RemainingFor"/> figure goes negative when a category is
/// overspent, and that is an ordinary value rather than an error. The rule that an *expense*
/// must be more than zero belongs to recording an expense, not to this type.</para>
///
/// <para>No currency field: MoneyBud is euro-only (arc42 §2, ADR 0003).</para>
///
/// <para>Nothing here rounds. The whole-cents rule makes rounding unnecessary rather than
/// careful — see arc42 §8.2 for the conditions under which that stops being true.</para>
/// </summary>
public readonly record struct Money
{
    public static readonly Money Zero = new(0);

    private Money(long cents) => Cents = cents;

    /// <summary>The amount as a whole number of cents. May be negative.</summary>
    public long Cents { get; }

    public decimal Euros => Cents / 100m;

    public bool IsNegative => Cents < 0;

    public static Money FromCents(long cents) => new(cents);

    /// <summary>
    /// True when <paramref name="euros"/> is a whole number of cents: 12.34 is, 12.345 is not.
    ///
    /// <para>Callers taking user input check this *before* converting, because a type that
    /// cannot hold the value cannot explain itself — and the user has to be told "an amount
    /// cannot be finer than a cent" in those words. This is the one place the cent rule needs
    /// checking at runtime; past it, the invariant holds by construction.</para>
    /// </summary>
    public static bool IsWholeCents(decimal euros) => euros == decimal.Round(euros, 2);

    /// <summary>
    /// Converts an amount already known to be a whole number of cents.
    /// Throws otherwise — check <see cref="IsWholeCents"/> first.
    /// </summary>
    public static Money FromEuros(decimal euros) =>
        IsWholeCents(euros)
            ? new Money((long)decimal.Round(euros * 100m))
            : throw new ArgumentOutOfRangeException(
                nameof(euros), euros, "An amount cannot be finer than a cent.");

    public static Money Sum(IEnumerable<Money> amounts)
    {
        var total = 0L;
        foreach (var amount in amounts) total += amount.Cents;
        return new Money(total);
    }

    public static Money operator +(Money left, Money right) => new(left.Cents + right.Cents);
    public static Money operator -(Money left, Money right) => new(left.Cents - right.Cents);
    public static Money operator -(Money value) => new(-value.Cents);

    public override string ToString() => Euros.ToString("0.00", CultureInfo.InvariantCulture);
}
