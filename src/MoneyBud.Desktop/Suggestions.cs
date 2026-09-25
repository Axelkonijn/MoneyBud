using Avalonia.Controls;
using MoneyBud.Presentation;

namespace MoneyBud.Desktop;

/// <summary>
/// Hands the category boxes the presentation layer's own narrowing rule, so that which
/// suggestions remain while typing is decided and tested there, not by the toolkit (ADR 0006).
/// </summary>
public static class Suggestions
{
    public static readonly AutoCompleteFilterPredicate<string?> Filter =
        (typed, suggestion) => suggestion is not null && MoneyBudApp.SuggestionMatches(typed, suggestion);
}
