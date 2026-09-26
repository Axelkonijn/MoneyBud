using System.Text.RegularExpressions;
using System.Xml.Linq;
using MoneyBud.Specs.Support;

namespace MoneyBud.Specs.Unit;

/// <summary>
/// The one ruling that lives only in the Desktop's markup: each form asks what the entry is
/// before its amount (arc42 §12, <i>The fields ask what before how much</i>). The Desktop is
/// otherwise untested by plan (ADR 0006); reading its markup as text is how this one ruling is
/// held, the way <see cref="TekstTests"/> holds the display terms by reading §12.
/// </summary>
public sealed partial class WindowMarkupTests
{
    [Theory]
    [InlineData("ExpenseForm", new[] { "Label", "Category", "Amount", "Date" })]
    [InlineData("IncomeForm", new[] { "Label", "Amount", "Date" })]
    [InlineData("AssignForm", new[] { "Category", "Amount" })]
    public void Each_form_asks_what_before_how_much(string form, string[] fields) =>
        Assert.Equal(fields, FieldsOf(form));

    private static IEnumerable<string> FieldsOf(string form)
    {
        var window = XDocument.Parse(Repository.ReadText("src", "MoneyBud.Desktop", "MainWindow.axaml"));
        var element = window.Descendants()
            .Single(e => (string?)e.Attribute("DataContext") == $"{{Binding {form}}}");

        // Fields in the order they are laid out: down a stack in the order written, and across a
        // grid by column, whatever order the columns are written in.
        return element.Descendants()
            .Where(e => e.Name.LocalName is "TextBox" or "AutoCompleteBox" or "CalendarDatePicker")
            .OrderBy(e => (int?)e.Attribute("Grid.Column") ?? 0)
            .Select(e => BoundTo().Match((string?)e.Attribute("Text") ?? (string?)e.Attribute("SelectedDate") ?? "").Groups[1].Value);
    }

    [GeneratedRegex(@"^\{Binding (\w+)\}$")]
    private static partial Regex BoundTo();
}
