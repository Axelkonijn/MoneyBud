using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace MoneyBud.Desktop;

/// <summary>
/// The colours the ring's slices are drawn in, and the one colour of the over marker. Colours are
/// presentation and nothing in arc42 fixes them; the only rule kept here is that no slice colour
/// can be mistaken for the marker's.
/// </summary>
public static class SliceColours
{
    private static readonly Color[] Palette =
    [
        Color.Parse("#2A9D8F"), Color.Parse("#3A86FF"), Color.Parse("#8E6CCF"), Color.Parse("#E9A23B"),
        Color.Parse("#4DA167"), Color.Parse("#E07BB0"), Color.Parse("#5C6F82"), Color.Parse("#C9B458"),
    ];

    public static readonly Color Unassigned = Color.Parse("#B8C2CC");
    public static readonly Color Over = Color.Parse("#C0392B");
    public static readonly Color Empty = Color.Parse("#D5DBE1");

    public static Color ForSlice(int index) => Palette[index % Palette.Length];

    /// <summary>A row's swatch: its slice's colour, or nothing when it has no slice.</summary>
    public static readonly IValueConverter Swatch = new FuncValueConverter<int?, IBrush>(
        index => index is { } i ? new SolidColorBrush(ForSlice(i)) : Brushes.Transparent);

    public static readonly IBrush OverBrush = new SolidColorBrush(Over);
}

/// <summary>A refusal is shown in the marker's colour family; anything else in a calm one.</summary>
public sealed class NoticeBackground : IValueConverter
{
    public static readonly NoticeBackground Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? new SolidColorBrush(Color.Parse("#FBE9E7")) : new SolidColorBrush(Color.Parse("#E8F4F1"));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
