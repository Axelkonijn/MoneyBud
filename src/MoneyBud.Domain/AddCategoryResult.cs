namespace MoneyBud.Domain;

/// <summary>
/// What came of adding a category name: exactly one of four things, and the user is told which
/// (arc42 §12). Three of them hand back a category; the fourth is a refusal.
///
/// <para>Two of the three successful outcomes hand back a category that already existed, and
/// that is the point of telling them apart. <see cref="AddCategoryOutcome.AlreadyThere"/>
/// confirms an end state that was already true; <see cref="AddCategoryOutcome.BroughtBack"/>
/// explains an outcome the user could not have predicted — old expenses reappearing under a
/// category they believe they just created. Either way the category comes back spelled as it
/// already was, not as typed this time: taking the new spelling would be a rename by the back
/// door. Renaming is its own act, <see cref="Ledger.RenameCategory"/>.</para>
/// </summary>
public sealed record AddCategoryResult
{
    private AddCategoryResult(Category? category, AddCategoryOutcome? outcome, CategoryRefusal? refusal)
    {
        Category = category;
        Outcome = outcome;
        Refusal = refusal;
    }

    public Category? Category { get; }

    public AddCategoryOutcome? Outcome { get; }

    public CategoryRefusal? Refusal { get; }

    public bool WasRefused => Refusal is not null;

    public static AddCategoryResult Created(Category category) =>
        new(category, AddCategoryOutcome.Created, null);

    public static AddCategoryResult AlreadyThere(Category category) =>
        new(category, AddCategoryOutcome.AlreadyThere, null);

    public static AddCategoryResult BroughtBack(Category category) =>
        new(category, AddCategoryOutcome.BroughtBack, null);

    public static AddCategoryResult Refused(CategoryRefusal refusal) => new(null, null, refusal);
}

/// <summary>What adding a name did, when it was not refused.</summary>
public enum AddCategoryOutcome
{
    /// <summary>No category had the name, so one was created.</summary>
    Created,

    /// <summary>A category in use already had the name. Not a refusal: the end state is true.</summary>
    AlreadyThere,

    /// <summary>An archived category had the name, and is in use again, history and all.</summary>
    BroughtBack,
}

/// <summary>
/// Why adding a category was refused. A reason, not a message — the wording belongs to a UI.
/// </summary>
public enum CategoryRefusal
{
    /// <summary>The name trimmed to nothing. A category cannot do without a name (arc42 §12).</summary>
    NameMissing,
}
