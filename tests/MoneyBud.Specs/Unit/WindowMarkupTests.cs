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

    /// <summary>
    /// The second thing only the markup can get wrong, approved with the persistence plan: the
    /// line for saving shows <i>beside</i> the notice and the question, never in their place (arc42
    /// §12, <i>The "not saved" notice is shown beside other messages</i>). So it has an element of
    /// its own, next to theirs rather than inside either.
    /// </summary>
    [Fact]
    public void The_save_line_stands_beside_the_notice_and_the_question()
    {
        var window = Window();
        var saveLine = Showing(window, "SaveLine");

        foreach (var other in (string[])["Notice.Text", "Question.Text"])
        {
            var element = Showing(window, other);
            Assert.NotSame(saveLine, element);
            Assert.DoesNotContain(saveLine, element.Descendants());
            Assert.DoesNotContain(element, saveLine.Descendants());
            Assert.Same(saveLine.Parent, element.Parent);
        }
    }

    // The bordered box whose text is bound to the property.
    private static XElement Showing(XDocument window, string property) =>
        window.Descendants()
            .Single(e => e.Name.LocalName == "TextBlock"
                         && ((string?)e.Attribute("Text"))?.StartsWith($"{{Binding {property}", StringComparison.Ordinal) == true)
            .Ancestors().First(e => e.Name.LocalName == "Border");

    private static XDocument Window() =>
        XDocument.Parse(Repository.ReadText("src", "MoneyBud.Desktop", "MainWindow.axaml"));

    private static IEnumerable<string> FieldsOf(string form)
    {
        var window = Window();
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
