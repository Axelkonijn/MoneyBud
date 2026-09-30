using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using MoneyBud.Presentation;

namespace MoneyBud.Phone.Views;

/// <summary>
/// The one thing MoneyBud says when it does not open, and nothing else: there is nothing to enter
/// anything into. Tapping <i>OK</i>, or the back button, ends the app (arc42 §12, <i>Android's
/// lifecycle</i>: "the message shows, and the app closes when it is tapped away").
/// </summary>
public sealed class MessageView : UserControl
{
    private readonly Action _quit;

    public MessageView(string text, Action quit)
    {
        _quit = quit;
        this.Res(BackgroundProperty, "Bg");

        var message = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, FontSize = 17, LineHeight = 25 };
        message.Res(TextBlock.ForegroundProperty, "Text");

        var ok = Ui.Pill(Tekst.Ok, quit);
        ok.HorizontalAlignment = HorizontalAlignment.Stretch;

        var card = Ui.Card(Ui.Stack(22, message, ok), 24);
        card.Margin = new Thickness(22);
        card.VerticalAlignment = VerticalAlignment.Center;
        Content = card;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is { } top)
        {
            top.BackRequested += OnBack;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is { } top)
        {
            top.BackRequested -= OnBack;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void OnBack(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        _quit();
    }
}
