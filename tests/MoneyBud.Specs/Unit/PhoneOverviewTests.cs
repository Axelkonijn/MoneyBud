using MoneyBud.Domain;
using MoneyBud.Presentation;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// What the phone shows that the desktop does not, worked out in the shared layer so the phone head
/// adds up and words nothing itself (plan for increment 14, B8 and B12): the period's totals, a
/// category's own expenses, and the phone's headings.
/// </summary>
public sealed class PhoneOverviewTests
{
    private readonly MoneyBudApp app = new(Ledger.StartNew(new FixedClock(new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero))));

    public PhoneOverviewTests()
    {
        app.RecordIncome("2000", "Salaris", new DateOnly(2026, 3, 1));
        app.RecordIncome("120,50", "Teruggave", new DateOnly(2026, 3, 20));
        app.RecordExpense("18", "Boodschappen", "Bakker", new DateOnly(2026, 3, 12));
        app.RecordExpense("32,15", "Boodschappen", "Albert Heijn", new DateOnly(2026, 3, 14));
        app.RecordExpense("950", "Huur", null, new DateOnly(2026, 3, 1));
    }

    [Fact]
    public void The_income_total_is_every_income_listed_future_dated_included() =>
        Assert.Equal(Money.FromCents(212050), app.Overview.IncomeTotal);

    [Fact]
    public void The_expense_total_is_every_expense_listed() =>
        Assert.Equal(Money.FromCents(100015), app.Overview.ExpenseTotal);

    [Fact]
    public void An_empty_period_totals_zero()
    {
        app.StepForward();

        Assert.Equal(Money.Zero, app.Overview.IncomeTotal);
        Assert.Equal(Money.Zero, app.Overview.ExpenseTotal);
    }

    [Fact]
    public void A_categorys_expenses_are_its_own_newest_first()
    {
        var own = app.Overview.ExpensesOn("Boodschappen");

        Assert.Equal(["Albert Heijn", "Bakker"], own.Select(e => e.Label));
        Assert.Empty(app.Overview.ExpensesOn("Hobby"));
    }

    [Fact]
    public void A_day_is_headed_today_yesterday_or_by_its_date()
    {
        var today = new DateOnly(2026, 3, 15);

        Assert.Equal("Vandaag", Tekst.DayHeading(today, today));
        Assert.Equal("Gisteren", Tekst.DayHeading(today.AddDays(-1), today));
        Assert.Equal("13 maart 2026", Tekst.DayHeading(today.AddDays(-2), today));
        Assert.Equal("16 maart 2026", Tekst.DayHeading(today.AddDays(1), today));
    }

    [Fact]
    public void The_phones_headings_and_lines_are_worded_from_the_terms()
    {
        Assert.Equal("van € 2.120,50 inkomen", Tekst.OfIncome(Money.FromCents(212050)));
        Assert.Equal("€ 50,15 van € 400,00", Tekst.SpentOf(Money.FromCents(5015), Money.FromCents(40000)));
        Assert.Equal("Toewijzen aan Huur", Tekst.AssignTo("Huur"));
        Assert.Equal("Sparen staat op", Tekst.BackingOf("Sparen"));
        Assert.Equal("Staat op Spaarrekening", Tekst.BackedBy("Spaarrekening"));
        Assert.Equal("Uitgegeven −€ 1,00", Tekst.Figure(Tekst.Spent, Money.FromCents(-100)));
    }
}
