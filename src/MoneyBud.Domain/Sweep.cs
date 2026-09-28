namespace MoneyBud.Domain;

/// <summary>What choosing the sweep destination did (arc42 §12, <i>The destination is one list</i>).</summary>
public enum SweepDestinationOutcome
{
    /// <summary>A category is the destination now, where another or none was.</summary>
    Chosen,

    /// <summary>There is no destination now, where there was one.</summary>
    Removed,

    /// <summary>What was chosen was already set. Nothing changed, and nothing is said.</summary>
    Unchanged,
}

/// <summary>
/// What came of choosing the sweep destination: what it did, and the destination before and after.
/// Never refused: the list offers only what can be chosen.
/// </summary>
public sealed record SetSweepDestinationResult(SweepDestinationOutcome Outcome, Category? Before, Category? After);

/// <summary>What an ended period's line says about its leftover (arc42 §12, <i>What an ended period shows</i>).</summary>
public enum SweepLineKind
{
    /// <summary>"Restant € 130,00 naar Sparen": the leftover has gone where it should.</summary>
    Swept,

    /// <summary>"€ 40,00 nog niet weggezet": more should go than has.</summary>
    StillToSweep,

    /// <summary>"€ 40,00 te veel weggezet": more went than should have, and some can come back.</summary>
    SweptTooMuch,

    /// <summary>The leftover is below zero and nothing went: the negative figure, badge <i>Tekort</i>.</summary>
    Shortfall,
}

/// <summary>What really went to one category for a period: every sweep in, less what came back.</summary>
public sealed record SweptPart(Category Category, Money Amount);

/// <summary>
/// An ended period's line, near <i>Niet toegewezen</i>. One at a time, in the order of
/// <see cref="Ledger.SweepLineFor"/>.
/// </summary>
/// <param name="Parts">What went to each category for the period, a category at zero left off, in
/// the order each first received money.</param>
/// <param name="Amount">Still to sweep or swept too much, both above zero; for a
/// <see cref="SweepLineKind.Shortfall"/> the period leftover itself, below zero; for
/// <see cref="SweepLineKind.Swept"/> what went in all.</param>
/// <param name="CanBringUpToDate">Whether <i>Restant bijwerken</i> is offered: there is money to move
/// and somewhere to move it (§12, ruled at the scenario stage, 3).</param>
public sealed record SweepLine(SweepLineKind Kind, IReadOnlyList<SweptPart> Parts, Money Amount, bool CanBringUpToDate);

/// <summary>
/// What <i>Restant bijwerken</i> did for a period: each move it made, in the order made, and what it
/// let go — the part swept too much that no category could give back, which the line stops asking
/// for, for good (§12, ruled at the scenario stage, 4).
/// </summary>
public sealed record BringUpToDateResult(BudgetPeriod Period, IReadOnlyList<Movement> Moves, Money LetGo);

/// <summary>A sweep settling made at a period's end, to be announced once (§12, <i>When the sweep runs</i>).</summary>
public sealed record SweepMade(BudgetPeriod Period, Movement Movement);
