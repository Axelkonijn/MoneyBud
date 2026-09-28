using MoneyBud.Domain;

namespace MoneyBud.Presentation;

/// <summary>
/// Whether a figure carries the over marker. One marker serves both states it is used for, a
/// category over budget and a period over-assigned (arc42 §12, *One marker for over budget and
/// over-assigned*). It is information, never a warning: it sits beside the negative figure and
/// replaces nothing.
/// </summary>
public enum Marker
{
    None,
    Over,
}

/// <summary>
/// One choice in a category row's <i>Staat op</i> list: an account, or <see cref="Tekst.NoBacking"/>
/// for none (arc42 §12, <i>On screen: Staat op and Opgebouwd</i>).
/// </summary>
public sealed record BackingChoice(Account? Account)
{
    public string Text => Account?.Name ?? Tekst.NoBacking;

    public override string ToString() => Text;
}

/// <summary>
/// One choice in the <i>Restant naar</i> list: a backed category, or <see cref="Tekst.NoSweepDestination"/>
/// for none (arc42 §12, <i>The destination is one list</i>).
/// </summary>
public sealed record SweepChoice(Category? Category)
{
    public string Text => Category?.Name ?? Tekst.NoSweepDestination;

    public override string ToString() => Text;
}

/// <summary>A category's row on the Overview: its <i>Budget</i>, what was spent, and <i>Remaining</i>.</summary>
public sealed record CategoryRow(string Name, Money Budget, Money Spent, Money Remaining, bool IsArchived)
{
    public Marker Marker => Remaining.IsNegative ? Marker.Over : Marker.None;

    /// <summary>
    /// Which slice of the ring is this category's, counting from the first clockwise; null when
    /// it has none. Rows and slices share one order and the budgeted rows come first, so this is
    /// simply the row's place — it lets a list show each row in its slice's colour.
    /// </summary>
    public int? SliceIndex { get; init; }

    /// <summary>
    /// Whether the row carries the delete button: the category has no history in any period
    /// (arc42 §12, *Deleting a category that has no history anywhere*). The archive button is a
    /// separate act and stays, so nothing is decided for the user.
    /// </summary>
    public bool CanDelete { get; init; }

    /// <summary>Whether this row's name is being renamed, and so shows a text box in its place.</summary>
    public bool IsRenaming { get; init; }

    /// <summary>
    /// The figure this category would take over from the plan offered, shown in grey under its
    /// <i>Budget</i> (arc42 §12, <i>The offer is a button, and a figure on each row</i>). Null while
    /// no plan is offered, and for a category not in the plan — which shows no figure, not a zero.
    /// </summary>
    public Money? PlanFigure { get; init; }

    /// <summary>"plan: € 400,00", or null when the row shows no plan figure.</summary>
    public string? PlanText => PlanFigure is { } figure ? Tekst.PlanFigure(figure) : null;

    /// <summary>
    /// The category's <i>Accumulated</i> as the period shown sees it, on screen <i>Opgebouwd</i> (arc42
    /// §12). Null for a category that is not backed, which shows none at all, not a zero.
    /// </summary>
    public Money? Accumulated { get; init; }

    /// <summary>Below zero, <i>Accumulated</i> carries the one marker, badge <i>Rood</i>. Exactly zero does not.</summary>
    public Marker AccumulatedMarker => Accumulated is { IsNegative: true } ? Marker.Over : Marker.None;

    public bool IsAccumulatedBelowZero => AccumulatedMarker == Marker.Over;

    /// <summary>"Opgebouwd: € 600,00" under the row's figures, or null for an unbacked category.</summary>
    public string? AccumulatedText => Accumulated is { } accumulated ? Tekst.AccumulatedFigure(accumulated) : null;

    /// <summary>The account backing the category, or null. Today's backing, whichever period is shown.</summary>
    public Account? BackingAccount { get; init; }

    /// <summary>The <i>Staat op</i> list: none, then every account in the strip's order.</summary>
    public IReadOnlyList<BackingChoice> BackingChoices { get; init; } = [];

    /// <summary>
    /// What the <i>Staat op</i> list shows and sets. Choosing hands the category and the account to
    /// the screen, which backs, re-points or unbacks it. A list writes back what it shows, and
    /// nothing while its items are replaced: nothing is ignored here, and what it shows is ignored
    /// by <see cref="MoneyBudApp.SetBacking"/>, which changes nothing for the backing already set.
    /// </summary>
    public BackingChoice? ChosenBacking
    {
        get => BackingChoices.FirstOrDefault(c => c.Account == BackingAccount);
        set
        {
            if (value is not null) ChooseBacking?.Invoke(Name, value.Account);
        }
    }

    internal Action<string, Account?>? ChooseBacking { get; init; }

    public bool IsOverBudget => Marker == Marker.Over;
    public string BudgetText => Tekst.Euro(Budget);
    public string SpentText => Tekst.Euro(Spent);
    public string RemainingText => Tekst.Euro(Remaining);
}

/// <summary>
/// An expense's row in the period's list. It carries the expense itself, because clicking the row
/// is how that expense is picked to be changed or removed (arc42 §12, *On screen: picking an entry
/// to correct*), and two identical rows are still two expenses.
/// </summary>
public sealed record ExpenseLine(Expense Entry)
{
    public DateOnly Date => Entry.Date;
    public string Category => Entry.Category.Name;
    public string? Label => Entry.Label;
    public Money Amount => Entry.Amount;
    public string DateText => Tekst.DayName(Date);
    public string AmountText => Tekst.Euro(Amount);

    /// <summary>
    /// The account the expense is on, when that is not the pool account; null when it is, so the
    /// usual row stays uncluttered and one on another account stands out (arc42 §12). Read against
    /// the pool account as it is when shown.
    /// </summary>
    public string? AccountName { get; init; }

    /// <summary>
    /// "maandelijks" or "wekelijks" on the latest occurrence of a running repeat, the row to open to
    /// change or stop it; null on every other row (arc42 §12, ruling 6). Only the Overview's lists
    /// carry it, not an account's history (ruled at the scenario stage, 3).
    /// </summary>
    public string? RepeatLabel { get; init; }
}

/// <summary>An income's row in the period's list, carrying the income for the reason <see cref="ExpenseLine"/> does.</summary>
public sealed record IncomeLine(Income Entry)
{
    public DateOnly Date => Entry.Date;
    public string Label => Entry.Label;
    public Money Amount => Entry.Amount;
    public string DateText => Tekst.DayName(Date);
    public string AmountText => Tekst.Euro(Amount);

    /// <summary>The account the income is on, when that is not the pool account, as for an expense.</summary>
    public string? AccountName { get; init; }

    /// <summary>The repeat's label on its latest occurrence, as for an expense.</summary>
    public string? RepeatLabel { get; init; }
}

/// <summary>
/// One budget period as the Overview shows it: the categories it lists, its ring, its
/// <i>Unassigned</i>, its expenses and incomes, and the plan it is offered, if any. Worked out
/// afresh from the ledger each time, so it never holds a figure that has since changed — and an
/// offer that has gone, because the period has passed or has a plan now, is simply not there.
/// </summary>
public sealed record PeriodOverview(
    BudgetPeriod Period,
    IReadOnlyList<CategoryRow> Rows,
    Ring Ring,
    Money Unassigned,
    IReadOnlyList<ExpenseLine> Expenses,
    IReadOnlyList<IncomeLine> Incomes,
    PlanOffer? Offer = null)
{
    /// <summary>Whether the button that takes the plan over is shown.</summary>
    public bool HasOffer => Offer is not null;

    /// <summary>
    /// The button's text, naming the period whose plan is offered and its total: "Plan van
    /// augustus 2026 overnemen (€ 1.450,00)". Null while no plan is offered.
    /// </summary>
    public string? OfferText => Offer is { } offer ? Tekst.TakeOverPlanButton(offer) : null;

    public bool IsOverAssigned => Unassigned.IsNegative;

    /// <summary>The same marker an overspent category carries (§12).</summary>
    public Marker UnassignedMarker => IsOverAssigned ? Marker.Over : Marker.None;

    public string UnassignedText => Tekst.Euro(Unassigned);

    // ------------------------------------------------------------------ the sweep (§12, *The sweep and Restant*)

    /// <summary>
    /// Whether the <i>Restant naar</i> list is shown: in the current period and later ones. An ended
    /// period shows its line in its place (§12, ruling 9).
    /// </summary>
    public bool ShowsSweepDestination { get; init; }

    /// <summary>The <i>Restant naar</i> list: "—" first, then the backed categories alphabetically.</summary>
    public IReadOnlyList<SweepChoice> SweepChoices { get; init; } = [];

    /// <summary>The sweep destination, or null for none. The same in every period: one setting.</summary>
    public Category? SweepDestination { get; init; }

    /// <summary>
    /// What the <i>Restant naar</i> list shows and sets. Choosing hands the category's name, or null
    /// for none, to the screen. A list writes back what it shows, and nothing while its items are
    /// replaced: nothing is ignored here, and what it shows is ignored by
    /// <see cref="MoneyBudApp.SetSweepDestination"/>, which changes nothing for the destination
    /// already set, as <see cref="CategoryRow.ChosenBacking"/> is.
    /// </summary>
    public SweepChoice? ChosenSweepDestination
    {
        get => SweepChoices.FirstOrDefault(c => c.Category == SweepDestination);
        set
        {
            if (value is not null) ChooseSweepDestination?.Invoke(value.Category?.Name);
        }
    }

    internal Action<string?>? ChooseSweepDestination { get; init; }

    /// <summary>An ended period's line, or null when it has none: always null in the current period and later ones.</summary>
    public SweepLine? SweepLine { get; init; }

    public bool HasSweepLine => SweepLine is not null;

    public string? SweepLineText => SweepLine is { } line ? Tekst.SweepLineText(line) : null;

    /// <summary>A shortfall carries the one marker, badge <i>Tekort</i> (§12, <i>What an ended period shows</i>).</summary>
    public Marker SweepLineMarker => SweepLine is { Kind: SweepLineKind.Shortfall } ? Marker.Over : Marker.None;

    public bool IsSweepLineShort => SweepLineMarker == Marker.Over;

    /// <summary>Whether <i>Restant bijwerken</i> is offered on the line.</summary>
    public bool CanBringSweepUpToDate => SweepLine is { CanBringUpToDate: true };

    /// <summary>What an empty ring says instead of being blank. Null when there is a ring to draw.</summary>
    public string? RingHint => Ring.IsEmpty ? Tekst.EmptyRing : null;

    /// <param name="renaming">The name of the category being renamed, if any, whose row shows a text box.</param>
    /// <param name="backingChoices">The <i>Staat op</i> list every row offers; made afresh when not given.</param>
    /// <param name="chooseBacking">What choosing in a row's <i>Staat op</i> list does; nothing when not given.</param>
    /// <param name="sweepChoices">The <i>Restant naar</i> list; made afresh when not given.</param>
    /// <param name="chooseSweepDestination">What choosing in the <i>Restant naar</i> list does; nothing when not given.</param>
    public static PeriodOverview Of(
        Ledger ledger, BudgetPeriod period, string? renaming = null,
        IReadOnlyList<BackingChoice>? backingChoices = null, Action<string, Account?>? chooseBacking = null,
        IReadOnlyList<SweepChoice>? sweepChoices = null, Action<string?>? chooseSweepDestination = null)
    {
        var choices = backingChoices ?? BackingChoicesOf(ledger);

        var offer = ledger.PlanOfferedIn(period);

        // The display rule decides which categories are listed, in the order they were added.
        // Sorting by Budget is stable, so equal budgets — every zero among them — keep that
        // order, and a category brought back keeps the place it was first added in (§12).
        //
        // While a plan is offered every Budget is zero, so the plan figure decides instead, a row
        // with none counting as zero: that is the order the rows will have once the plan is taken
        // over, so nothing jumps (§12, *The figure on each row goes with the offer*). With no
        // offer every plan figure is null and the second key changes nothing.
        var rows = ledger.CategoriesShownIn(period)
            .Select(c => new CategoryRow(
                c.Name,
                ledger.BudgetFor(c.Name, period),
                ledger.SpentOn(c.Name, period),
                ledger.RemainingFor(c.Name, period),
                ledger.IsArchived(c.Name))
            {
                CanDelete = ledger.CanDelete(c.Name),
                IsRenaming = c.Name == renaming,
                PlanFigure = offer?.FigureFor(c),
                Accumulated = ledger.AccumulatedFor(c.Name, period),
                BackingAccount = ledger.BackingOf(c.Name),
                BackingChoices = choices,
                ChooseBacking = chooseBacking,
            })
            .OrderByDescending(r => r.Budget.Cents)
            .ThenByDescending(r => r.PlanFigure?.Cents ?? 0)
            .ToList();

        var incomes = ledger.IncomesIn(period);
        var unassigned = ledger.UnassignedIn(period);
        var ring = Ring.Of(rows, unassigned, hasIncome: incomes.Count > 0);

        rows = rows
            .Select((row, i) => row with { SliceIndex = row.Budget.Cents > 0 && !ring.IsEmpty ? i : null })
            .ToList();

        return new PeriodOverview(
            period,
            rows,
            ring,
            unassigned,
            NewestFirst(ledger.ExpensesIn(period), e => e.Date)
                .Select(e => new ExpenseLine(e) { AccountName = NameUnlessPool(e.Account), RepeatLabel = LabelOf(e) }).ToList(),
            NewestFirst(incomes, i => i.Date)
                .Select(i => new IncomeLine(i) { AccountName = NameUnlessPool(i.Account), RepeatLabel = LabelOf(i) }).ToList(),
            offer)
        {
            ShowsSweepDestination = period.FirstDay >= ledger.CurrentPeriod.FirstDay,
            SweepChoices = sweepChoices ?? SweepChoicesOf(ledger),
            SweepDestination = ledger.SweepDestination,
            ChooseSweepDestination = chooseSweepDestination,
            SweepLine = ledger.SweepLineFor(period),
        };

        string? NameUnlessPool(Account account) => account == ledger.PoolAccount ? null : account.Name;

        string? LabelOf(IEntry entry) => ledger.FrequencyOf(entry) is { } frequency ? Tekst.RepeatLabel(frequency) : null;
    }

    /// <summary>
    /// The <i>Staat op</i> list: "—" for none, then the accounts in the strip's order, the pool
    /// account first — every account can back a category, the pool account included (§12).
    /// </summary>
    public static IReadOnlyList<BackingChoice> BackingChoicesOf(Ledger ledger) =>
        [new BackingChoice(null), .. ledger.Accounts.Select(a => new BackingChoice(a))];

    /// <summary>
    /// The <i>Restant naar</i> list: "—" for none first, then every backed category that is not
    /// archived, alphabetically as the category suggestions are, case not counting (§12, ruled at the
    /// scenario stage, 8). Not in row order, which changes with every assignment.
    /// </summary>
    public static IReadOnlyList<SweepChoice> SweepChoicesOf(Ledger ledger) =>
        [new SweepChoice(null),
         .. ledger.SweepDestinationChoices.OrderBy(c => c.Name, MoneyBudApp.Alphabetical).Select(c => new SweepChoice(c))];

    /// <summary>
    /// Newest date first, and on the same date newest recorded first (§12). The ledger lists in
    /// the order recorded; reversing that and then sorting stably by date gives both at once.
    /// </summary>
    private static IEnumerable<T> NewestFirst<T>(IReadOnlyList<T> inOrderRecorded, Func<T, DateOnly> date) =>
        inOrderRecorded.Reverse().OrderByDescending(date);
}
