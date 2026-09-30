using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Path = Avalonia.Controls.Shapes.Path;

namespace MoneyBud.Prototype.Views;

/// <summary>Small building blocks for the screens, all in the theme's named colours.</summary>
public static class Ui
{
    public const string ChevronLeft = "M15 5 L8 12 L15 19";
    public const string ChevronRight = "M9 5 L16 12 L9 19";
    public const string ChevronUp = "M5 15 L12 8 L19 15";
    public const string ChevronDown = "M5 9 L12 16 L19 9";
    public const string Close = "M6 6 L18 18 M18 6 L6 18";
    public const string Plus = "M12 5 L12 19 M5 12 L19 12";
    public const string Tick = "M5 12.5 L10 17.5 L19 7";
    public const string House = "M3.5 11 L12 3.5 L20.5 11 M6 9 L6 20 L18 20 L18 9";

    public const string Gear =
        "M19.14,12.94c0.04-0.3,0.06-0.61,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61 l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59-0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41 h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87 C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58 c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12,0.22,0.37,0.29,0.59,0.22l2.39-0.96c0.5,0.38,1.03,0.7,1.62,0.94l0.36,2.54 c0.05,0.24,0.24,0.41,0.48,0.41h3.84c0.24,0,0.44-0.17,0.47-0.41l0.36-2.54c0.59-0.24,1.13-0.56,1.62-0.94l2.39,0.96 c0.22,0.08,0.47,0,0.59-0.22l1.92-3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6 s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z";

    public static ControlTheme Theme(string key) =>
        Application.Current!.TryFindResource(key, out var theme) && theme is ControlTheme controlTheme
            ? controlTheme
            : throw new InvalidOperationException($"No control theme '{key}'.");

    /// <summary>Follows a named theme resource, so switching light and dark repaints it.</summary>
    public static T Res<T>(this T control, AvaloniaProperty property, string key) where T : Control
    {
        control.Bind(property, control.GetResourceObservable(key));
        return control;
    }

    /// <summary>A slice colour (a Color resource) as a brush.</summary>
    public static T SliceBrush<T>(this T control, AvaloniaProperty property, int colour) where T : Control
    {
        control.Bind(property, control.GetResourceObservable("Slice" + colour % 8, c => c is Color value ? new SolidColorBrush(value) : null));
        return control;
    }

    public static TextBlock Text(string text, params string[] classes)
    {
        var block = new TextBlock { Text = text, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center };
        foreach (var name in classes)
        {
            block.Classes.Add(name);
        }

        return block;
    }

    public static Control Icon(string data, string brush = "Text", double size = 22, double thickness = 2.2) =>
        new Path
        {
            Data = Geometry.Parse(data),
            StrokeThickness = thickness,
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round,
            Stretch = Stretch.Uniform,
            Width = size,
            Height = size,
            Margin = new Thickness(size * 0.12),
            VerticalAlignment = VerticalAlignment.Center,
        }.Res(Shape.StrokeProperty, brush);

    /// <summary>The small arrow at the end of a row that leads somewhere.</summary>
    public static Control Chevron()
    {
        var chevron = Icon(ChevronRight, "Faint", 18);
        chevron.Margin = new Thickness(8, 0, 0, 0);
        return chevron;
    }

    public static Control FilledIcon(string data, string brush = "Text", double size = 22) =>
        new Path { Data = Geometry.Parse(data), Stretch = Stretch.Uniform, Width = size, Height = size }.Res(Shape.FillProperty, brush);

    /// <summary>Three dots, for a menu of things done less often.</summary>
    public static Control Dots(string brush = "Muted")
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        for (var i = 0; i < 3; i++)
        {
            row.Children.Add(new Ellipse { Width = 5, Height = 5 }.Res(Shape.FillProperty, brush));
        }

        return row;
    }

    public static Border Dot(int colour, double size = 10) =>
        new Border { Width = size, Height = size, CornerRadius = new CornerRadius(size / 2), VerticalAlignment = VerticalAlignment.Center }
            .SliceBrush(Border.BackgroundProperty, colour);

    public static Border ColouredDot(string brush, double size = 10) =>
        new Border { Width = size, Height = size, CornerRadius = new CornerRadius(size / 2), VerticalAlignment = VerticalAlignment.Center }
            .Res(Border.BackgroundProperty, brush);

    public static Border Card(Control child, double padding = 6) =>
        new Border { CornerRadius = new CornerRadius(20), Padding = new Thickness(padding), BorderThickness = new Thickness(1), Child = child }
            .Res(Border.BackgroundProperty, "Card").Res(Border.BorderBrushProperty, "CardEdge");

    public static Border Handle() =>
        new Border
        {
            Width = 40, Height = 5, CornerRadius = new CornerRadius(2.5),
            HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 6),
        }.Res(Border.BackgroundProperty, "Handle");

    public static Border Badge(string text, string background = "Field", string foreground = "Muted")
    {
        var label = Text(text);
        label.FontSize = 11;
        label.FontWeight = FontWeight.SemiBold;
        label.Res(TextBlock.ForegroundProperty, foreground);
        return new Border
        {
            CornerRadius = new CornerRadius(8), Padding = new Thickness(7, 2), Child = label,
            VerticalAlignment = VerticalAlignment.Center,
        }.Res(Border.BackgroundProperty, background);
    }

    /// <summary>The one marker for "below zero": over budget, over-assigned, overdrawn.</summary>
    public static Border Marker()
    {
        var mark = Text("!");
        mark.FontSize = 11;
        mark.FontWeight = FontWeight.Bold;
        mark.HorizontalAlignment = HorizontalAlignment.Center;
        mark.Res(TextBlock.ForegroundProperty, "OnAccent");
        return new Border
        {
            Width = 17, Height = 17, CornerRadius = new CornerRadius(8.5), Child = mark,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0),
        }.Res(Border.BackgroundProperty, "Danger");
    }

    public static Button Plain(Control content, Action onClick, Thickness? padding = null)
    {
        var button = new Button { Theme = Theme("Plain"), Content = content, Padding = padding ?? new Thickness(14, 12) };
        button.Click += (_, _) => onClick();
        return button;
    }

    public static Button Pill(string text, Action onClick, string theme = "Pill")
    {
        var button = new Button { Theme = Theme(theme), Content = text };
        button.Click += (_, _) => onClick();
        return button;
    }

    /// <summary>A round button in the accent colour with just a sign on it: + to add, a tick to save.</summary>
    public static Button Round(string icon, Action onClick)
    {
        var button = new Button
        {
            Theme = Theme("Pill"), Content = Icon(icon, "OnAccent", 20, 2.6),
            Width = 48, Height = 48, Padding = new Thickness(0), CornerRadius = new CornerRadius(24),
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    public static Button IconButton(Control icon, Action onClick)
    {
        var button = new Button { Theme = Theme("Icon"), Content = icon };
        button.Click += (_, _) => onClick();
        return button;
    }

    public static TextBox Field(string watermark, string text = "")
    {
        var box = new TextBox { PlaceholderText = watermark, Text = text };
        box.Classes.Add("field");
        return box;
    }

    /// <summary>A row of chips of which exactly one is chosen.</summary>
    public static WrapPanel Chips(IEnumerable<string> options, string? chosen, Action<string> onPick, Func<string, bool>? isEnabled = null)
    {
        var panel = new WrapPanel();
        foreach (var option in options)
        {
            var chip = new ToggleButton
            {
                Theme = Theme("Chip"), Content = option, IsChecked = option == chosen,
                IsEnabled = isEnabled?.Invoke(option) ?? true,
            };
            chip.Click += (_, _) =>
            {
                foreach (var other in panel.Children.OfType<ToggleButton>())
                {
                    other.IsChecked = other == chip;
                }

                onPick(option);
            };
            panel.Children.Add(chip);
        }

        return panel;
    }

    /// <summary>A chip that does something when tapped rather than staying chosen.</summary>
    public static ToggleButton Suggestion(string text, int colour, Action onPick)
    {
        var chip = new ToggleButton { Theme = Theme("Chip"), Content = Row(8, Dot(colour, 8), Text(text)) };
        chip.Click += (_, _) =>
        {
            chip.IsChecked = false;
            onPick();
        };
        return chip;
    }

    public static TextBlock Caption(string text) => Text(text.ToUpperInvariant(), "caption");

    /// <summary>Rows in a card, with hairlines between them.</summary>
    public static Border List(IEnumerable<Control> rows)
    {
        var stack = new StackPanel();
        foreach (var row in rows)
        {
            if (stack.Children.Count > 0)
            {
                stack.Children.Add(new Border { Height = 1, Margin = new Thickness(16, 0) }.Res(Border.BackgroundProperty, "Line"));
            }

            stack.Children.Add(row);
        }

        return Card(stack, 0);
    }

    /// <summary>A thin bar showing how much of something is used.</summary>
    public static Control Bar(double fraction, int colour, bool over)
    {
        fraction = Math.Clamp(fraction, 0, 1);
        var grid = new Grid { Height = 4, Margin = new Thickness(0, 8, 0, 4) };
        grid.ColumnDefinitions.Add(new ColumnDefinition(fraction, GridUnitType.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(1 - fraction, GridUnitType.Star));
        var track = new Border { CornerRadius = new CornerRadius(2) }.Res(Border.BackgroundProperty, "Field");
        Grid.SetColumnSpan(track, 2);
        var fill = new Border { CornerRadius = new CornerRadius(2) };
        if (over)
        {
            fill.Res(Border.BackgroundProperty, "Danger");
        }
        else
        {
            fill.SliceBrush(Border.BackgroundProperty, colour);
        }

        grid.Children.Add(track);
        grid.Children.Add(fill);
        return grid;
    }

    public static Grid Columns(string definitions, params Control[] children)
    {
        var grid = new Grid { ColumnDefinitions = ColumnDefinitions.Parse(definitions) };
        for (var i = 0; i < children.Length; i++)
        {
            Grid.SetColumn(children[i], i);
            grid.Children.Add(children[i]);
        }

        return grid;
    }

    public static StackPanel Stack(double spacing, params Control[] children)
    {
        var stack = new StackPanel { Spacing = spacing };
        stack.Children.AddRange(children);
        return stack;
    }

    public static StackPanel Row(double spacing, params Control[] children)
    {
        var stack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = spacing };
        stack.Children.AddRange(children);
        return stack;
    }
}
