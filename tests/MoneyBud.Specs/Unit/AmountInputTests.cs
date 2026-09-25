using MoneyBud.Presentation;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// How typed amounts are read (arc42 §12, settled 2026-09-25): a comma or a point is the decimal
/// mark, and no thousands separator is accepted. Nothing is rounded here — the cent rule is the
/// domain's, so "12,345" is read as typed and refused there.
/// </summary>
public sealed class AmountInputTests
{
    [Theory]
    [InlineData("12,50", "12.50")]
    [InlineData("12.50", "12.50")]
    [InlineData("12", "12")]
    [InlineData("0,01", "0.01")]
    [InlineData("  7,5 ", "7.5")]
    [InlineData("€ 3,50", "3.50")]
    [InlineData("€3.50", "3.50")]
    [InlineData("-50", "-50")]
    [InlineData("−20,00", "-20.00")]
    [InlineData("- 20", "-20")]
    [InlineData("12,345", "12.345")]
    [InlineData("1.832", "1.832")]
    [InlineData("2.00", "2.00")]
    [InlineData("12,3456", "12.3456")]
    [InlineData("0", "0")]
    public void Reads_what_was_typed_exactly(string typed, string euros)
    {
        Assert.True(AmountInput.TryRead(typed, out var read));
        Assert.Equal(decimal.Parse(euros, System.Globalization.CultureInfo.InvariantCulture), read);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("12,50 euro")]
    [InlineData("1.832,45")]
    [InlineData("1,832.45")]
    [InlineData("1 832")]
    [InlineData("12,")]
    [InlineData(",50")]
    [InlineData("+12")]
    [InlineData("12e3")]
    [InlineData("--5")]
    [InlineData("12345678901234")]
    [InlineData("2.0000")]
    [InlineData("€ −50")]
    [InlineData("€ -50")]
    [InlineData("12,5000")]
    [InlineData("0,01000")]
    [InlineData("1,00000000000000000000000000001")]
    public void Is_not_an_amount(string? typed) =>
        Assert.Equal(AmountReading.NotAnAmount, AmountInput.Read(typed, out _));

    // Two readings, both whole cents: two thousand the Dutch way, or two euros. Refused rather
    // than guessed, because nothing further down could tell the wrong one.
    [Theory]
    [InlineData("2.000", "2000", "2,00")]
    [InlineData("1,500", "1500", "1,50")]
    [InlineData("0.100", "100", "0,10")]
    [InlineData("-2.000", "-2000", "-2,00")]
    public void A_mark_and_three_digits_ending_in_zero_is_ambiguous(string typed, string asThousands, string asDecimal)
    {
        Assert.Equal(AmountReading.Ambiguous, AmountInput.Read(typed, out _));
        Assert.Equal((asThousands, asDecimal), AmountInput.Readings(typed));
    }
}
