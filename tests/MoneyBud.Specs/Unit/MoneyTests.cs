using MoneyBud.Domain;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// Developer tests for the one type the scenarios exercise only indirectly. The scenarios remain
/// the contract (features/README.md); these cover the boundary where a typed-in decimal becomes
/// money, which is where arc42 §8.2's whole-cents rule is actually enforced.
///
/// <para>Amounts are written as strings and parsed the same way a feature file's amounts are.
/// An [InlineData] decimal is a double literal that xUnit converts, which is precisely the
/// representation these tests exist to keep out of the money path — and it cannot tell 3.5 from
/// 3.50 at all.</para>
/// </summary>
public class MoneyTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("12.34")]
    [InlineData("3.50")]
    [InlineData("-0.01")]
    [InlineData("400")]
    public void Amounts_down_to_the_cent_are_whole_cents(string euros) =>
        Assert.True(Money.IsWholeCents(SpecParsing.Amount(euros)));

    [Theory]
    [InlineData("0.001")]
    [InlineData("12.345")]
    [InlineData("25.999")]
    [InlineData("-12.345")]
    public void Amounts_finer_than_a_cent_are_not(string euros) =>
        Assert.False(Money.IsWholeCents(SpecParsing.Amount(euros)));

    [Theory]
    [InlineData("3.50", 350)]
    [InlineData("3.5", 350)]
    [InlineData("0.01", 1)]
    [InlineData("-0.01", -1)]
    [InlineData("0", 0)]
    [InlineData("400.00", 40000)]
    public void Converting_euros_gives_whole_cents(string euros, long expectedCents) =>
        Assert.Equal(expectedCents, Money.FromEuros(SpecParsing.Amount(euros)).Cents);

    [Fact]
    public void Converting_an_amount_finer_than_a_cent_is_refused()
    {
        // Nothing rounds it to 12.34 or 12.35. Callers check IsWholeCents first so they can say
        // so in the user's words; reaching here at all is a bug, not an input problem.
        Assert.Throws<ArgumentOutOfRangeException>(() => Money.FromEuros(12.345m));
    }

    [Fact]
    public void A_trailing_zero_does_not_make_a_different_amount() =>
        Assert.Equal(Money.FromEuros(3.5m), Money.FromEuros(3.50m));

    [Fact]
    public void Equal_amounts_are_equal_however_they_were_made() =>
        Assert.Equal(Money.FromCents(10), Money.FromEuros(0.10m));

    [Fact]
    public void Subtracting_more_than_there_is_gives_a_negative_amount()
    {
        var remaining = Money.FromEuros(400m) - Money.FromEuros(420m);

        Assert.Equal(-2000, remaining.Cents);
        Assert.True(remaining.IsNegative);
    }

    [Fact]
    public void Zero_is_not_negative() => Assert.False(Money.Zero.IsNegative);

    [Fact]
    public void Cents_add_up_exactly()
    {
        // The case that would drift under binary floating point: a hundred ten-cent amounts.
        var total = Money.Sum(Enumerable.Repeat(Money.FromEuros(0.10m), 100));

        Assert.Equal(Money.FromEuros(10m), total);
    }

    [Theory]
    [InlineData(-1, "-0.01")]
    [InlineData(6315, "63.15")]
    [InlineData(0, "0.00")]
    [InlineData(-3500, "-35.00")]
    public void Amounts_read_back_in_euros_and_cents(long cents, string expected) =>
        Assert.Equal(expected, Money.FromCents(cents).ToString());
}
