namespace MoneyBud.Domain;

/// <summary>
/// What came of renaming a category: it was renamed, the new name was exactly the one it already
/// had, or the new name was refused (arc42 §12, *Renaming a category*).
///
/// <para>Renaming to a new <i>spelling</i> of its own name — "boodschappen" to "Boodschappen" —
/// is a rename, not <see cref="RenameOutcome.Unchanged"/>. Only the name spelled exactly as it
/// already is changes nothing, and that goes through quietly, as saving an unchanged entry
/// does.</para>
/// </summary>
public sealed record RenameCategoryResult
{
    private RenameCategoryResult(
        Category? category, string? oldName, RenameOutcome? outcome, RenameRefusal? refusal)
    {
        Category = category;
        OldName = oldName;
        Outcome = outcome;
        Refusal = refusal;
    }

    /// <summary>The category, under its name as it now is. Null when refused.</summary>
    public Category? Category { get; }

    /// <summary>The name the category had before, as it was stored. Null when refused.</summary>
    public string? OldName { get; }

    public RenameOutcome? Outcome { get; }

    public RenameRefusal? Refusal { get; }

    public bool WasRefused => Refusal is not null;

    public static RenameCategoryResult Renamed(string oldName, Category category) =>
        new(category, oldName, RenameOutcome.Renamed, null);

    public static RenameCategoryResult Unchanged(Category category) =>
        new(category, category.Name, RenameOutcome.Unchanged, null);

    public static RenameCategoryResult Refused(RenameRefusal refusal) => new(null, null, null, refusal);
}

/// <summary>What renaming did, when it was not refused.</summary>
public enum RenameOutcome
{
    /// <summary>The category has the new name, everywhere. Announced, from what to what.</summary>
    Renamed,

    /// <summary>The new name was the old one, letter for letter. Nothing changed, nothing is said.</summary>
    Unchanged,
}

/// <summary>
/// Why a rename was refused. Reasons, not messages — the wording belongs to a UI.
/// </summary>
public enum RenameRefusal
{
    /// <summary>The new name trimmed to nothing — the rule for adding a name (arc42 §12).</summary>
    NameMissing,

    /// <summary>
    /// Another category already has the name under the name rule, archived ones included.
    /// Refused rather than merging the two, which would silently rewrite both histories.
    /// </summary>
    NameTaken,
}
