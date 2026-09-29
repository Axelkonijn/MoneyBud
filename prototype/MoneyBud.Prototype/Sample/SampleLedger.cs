namespace MoneyBud.Prototype.Sample;

public sealed class SampleCategory(string name, int colour)
{
    public string Name { get; set; } = name;

    /// <summary>Which of the theme's slice colours it is drawn in.</summary>
    public int Colour { get; } = colour;

    /// <summary>Budget per period, by the period's offset from the current one.</summary>
    public Dictionary<int, decimal> Budgets { get; } = [];

    public string? BackedBy { get; set; }

    public decimal AccumulatedBefore { get; init; }
}

public sealed class SampleAccount(string name, decimal opening)
{
    public string Name { get; } = name;

    public decimal Opening { get; } = opening;
}

public enum EntryKind
{
    Expense,
    Income,
    Moved,
}

public sealed class SampleEntry
{
    public required int Id { get; init; }

    public required EntryKind Kind { get; init; }

    public string Label { get; set; } = "";

    public string? Category { get; set; }

    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public string Account { get; set; } = "";

    /// <summary>For a movement: where the money went.</summary>
    public string? ToAccount { get; set; }

    /// <summary>"wekelijks" or "maandelijks" on the latest occurrence of a repeat, else null.</summary>
    public string? Repeat { get; set; }
}

public sealed record PlanOffer(int FromOffset, IReadOnlyList<(SampleCategory Category, decimal Amount)> Figures)
{
    public decimal Total => Figures.Sum(f => f.Amount);
}

/// <summary>
/// Invented data for the prototype, and just enough arithmetic to make the screens move when
/// something is entered. None of MoneyBud's rules live here: the real app keeps using its own
/// domain, and nothing in this class is meant to be carried over.
/// </summary>
public sealed class SampleLedger
{
    private int _lastId;

    public SampleLedger(DateOnly today)
    {
        Today = today;
        Seed();
    }

    public DateOnly Today { get; }

    public List<SampleCategory> Categories { get; } = [];

    public List<SampleAccount> Accounts { get; } = [];

    public List<SampleEntry> Entries { get; } = [];

    public string PoolAccount => Accounts[0].Name;

    public string? SweepDestination { get; set; } = "Sparen";

    public DateOnly FirstDay(int offset) => new DateOnly(Today.Year, Today.Month, 1).AddMonths(offset);

    public DateOnly LastDay(int offset) => FirstDay(offset + 1).AddDays(-1);

    public bool IsIn(int offset, DateOnly date) => date >= FirstDay(offset) && date <= LastDay(offset);

    public IEnumerable<SampleEntry> Expenses(int offset) =>
        Entries.Where(e => e.Kind == EntryKind.Expense && IsIn(offset, e.Date))
            .OrderByDescending(e => e.Date).ThenByDescending(e => e.Id);

    public IEnumerable<SampleEntry> Incomes(int offset) =>
        Entries.Where(e => e.Kind == EntryKind.Income && IsIn(offset, e.Date))
            .OrderByDescending(e => e.Date).ThenByDescending(e => e.Id);

    public decimal BudgetOf(SampleCategory category, int offset) => category.Budgets.GetValueOrDefault(offset);

    public decimal SpentOn(SampleCategory category, int offset) =>
        Expenses(offset).Where(e => e.Category == category.Name).Sum(e => e.Amount);

    public decimal RemainingOf(SampleCategory category, int offset) => BudgetOf(category, offset) - SpentOn(category, offset);

    public decimal Unassigned(int offset) =>
        Incomes(offset).Sum(e => e.Amount) - Categories.Sum(c => BudgetOf(c, offset));

    public decimal AccumulatedOf(SampleCategory category, int offset) =>
        category.AccumulatedBefore + Enumerable.Range(-1, Math.Max(0, offset + 2)).Sum(o => RemainingOf(category, o));

    public SampleCategory? Find(string name) =>
        Categories.FirstOrDefault(c => string.Equals(c.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

    public decimal BalanceOf(string account) =>
        Accounts.First(a => a.Name == account).Opening
        + Entries.Where(e => e.Date <= Today).Sum(e => Effect(e, account));

    public decimal NetWorth => Accounts.Sum(a => BalanceOf(a.Name));

    /// <summary>What an entry did to an account's balance.</summary>
    public static decimal Effect(SampleEntry entry, string account) => entry.Kind switch
    {
        EntryKind.Income when entry.Account == account => entry.Amount,
        EntryKind.Expense when entry.Account == account => -entry.Amount,
        EntryKind.Moved when entry.Account == account => -entry.Amount,
        EntryKind.Moved when entry.ToAccount == account => entry.Amount,
        _ => 0,
    };

    public IEnumerable<SampleEntry> HistoryOf(string account) =>
        Entries.Where(e => e.Date <= Today && Effect(e, account) != 0)
            .OrderByDescending(e => e.Date).ThenByDescending(e => e.Id);

    public PlanOffer? OfferFor(int offset)
    {
        if (offset < 0 || Categories.Any(c => BudgetOf(c, offset) > 0))
        {
            return null;
        }

        for (var from = offset - 1; from >= offset - 24; from--)
        {
            var figures = Categories.Where(c => BudgetOf(c, from) > 0).Select(c => (c, BudgetOf(c, from))).ToList();
            if (figures.Count > 0)
            {
                return new PlanOffer(from, figures);
            }
        }

        return null;
    }

    public void TakeOverPlan(int offset)
    {
        foreach (var (category, amount) in OfferFor(offset)?.Figures ?? [])
        {
            category.Budgets[offset] = amount;
        }
    }

    /// <summary>Moves an amount onto a Budget; a Budget floors at zero.</summary>
    public void Assign(SampleCategory category, int offset, decimal amount) =>
        category.Budgets[offset] = Math.Max(0, BudgetOf(category, offset) + amount);

    public SampleEntry Add(EntryKind kind, string label, string? category, decimal amount, DateOnly date, string account, string? repeat)
    {
        var entry = new SampleEntry
        {
            Id = ++_lastId, Kind = kind, Label = label.Trim(), Category = category, Amount = amount,
            Date = date, Account = account, Repeat = repeat,
        };
        Entries.Add(entry);
        return entry;
    }

    private void Seed()
    {
        Accounts.Add(new SampleAccount("Betaalrekening", 812.40m));
        Accounts.Add(new SampleAccount("Spaarrekening", 5_600m));
        Accounts.Add(new SampleAccount("Contant", 60m));

        var groceries = Category("Boodschappen", 0, 400);
        var rent = Category("Huur", 1, 950);
        var hobby = Category("Hobby", 2, 120);
        var saving = Category("Sparen", 3, 200);
        var insurance = Category("Verzekeringen", 4, 145);
        var subscriptions = Category("Abonnementen", 5, 60);
        var travel = Category("Vervoer", 6, 90);
        saving.BackedBy = "Spaarrekening";

        foreach (var offset in new[] { -1, 0, 1 })
        {
            Income(offset, 25, "Salaris", 2_450m, offset == 1 ? "maandelijks" : null);
            if (offset < 1)
            {
                Income(offset, 20, "Zorgtoeslag", 123m, offset == 0 ? "maandelijks" : null);
                Move(offset, 1, saving.Name, 200m);
            }
        }

        // Day of the month, label, category, amount, account. The current period only gets the
        // ones up to today: an expense is never in the future.
        (int Day, string Label, SampleCategory Category, decimal Amount, string Account, string? Repeat)[] month =
        [
            (1, "Huur", rent, 950m, "Betaalrekening", "maandelijks"),
            (2, "Albert Heijn", groceries, 64.20m, "Betaalrekening", null),
            (3, "Netflix", subscriptions, 13.99m, "Betaalrekening", "maandelijks"),
            (5, "Spotify", subscriptions, 11.99m, "Betaalrekening", "maandelijks"),
            (6, "NS", travel, 23.40m, "Betaalrekening", null),
            (8, "Jumbo", groceries, 48.75m, "Betaalrekening", null),
            (9, "Zorgverzekering", insurance, 145m, "Betaalrekening", "maandelijks"),
            (11, "Bouwpakket", hobby, 89.95m, "Betaalrekening", null),
            (13, "Markt", groceries, 17.30m, "Contant", null),
            (16, "Tanken", travel, 45m, "Betaalrekening", null),
            (18, "Lidl", groceries, 52.10m, "Betaalrekening", null),
            (20, "Verf en kwasten", hobby, 55.55m, "Betaalrekening", null),
            (22, "Albert Heijn", groceries, 71.40m, "Betaalrekening", null),
            (24, "OV-chipkaart", travel, 20m, "Betaalrekening", null),
            (26, "Broodje kip", groceries, 4m, "Contant", "wekelijks"),
            (27, "Jumbo", groceries, 38.60m, "Betaalrekening", null),
        ];

        foreach (var offset in new[] { -1, 0 })
        {
            foreach (var (day, label, category, amount, account, repeat) in month)
            {
                var date = FirstDay(offset).AddDays(Math.Min(day, LastDay(offset).Day) - 1);
                if (date <= Today)
                {
                    // Last month a little different, so stepping back shows a different ring.
                    var shown = offset == 0 ? amount : decimal.Round(amount * (0.8m + day % 5 * 0.1m), 2);
                    Add(EntryKind.Expense, label, category.Name, category == rent || category == insurance ? amount : shown, date, account, offset == 0 ? repeat : null);
                }
            }
        }

        SampleCategory Category(string name, int colour, decimal budget)
        {
            var category = new SampleCategory(name, colour) { AccumulatedBefore = name == "Sparen" ? 1_000m : 0 };
            category.Budgets[-1] = budget;
            category.Budgets[0] = budget;
            Categories.Add(category);
            return category;
        }

        void Income(int offset, int day, string label, decimal amount, string? repeat) =>
            Add(EntryKind.Income, label, null, amount, FirstDay(offset).AddDays(day - 1), PoolAccount, repeat);

        void Move(int offset, int day, string category, decimal amount) =>
            Entries.Add(new SampleEntry
            {
                Id = ++_lastId, Kind = EntryKind.Moved, Label = $"Voor {category}", Category = category,
                Amount = amount, Date = FirstDay(offset).AddDays(day - 1), Account = PoolAccount, ToAccount = "Spaarrekening",
            });
    }
}
