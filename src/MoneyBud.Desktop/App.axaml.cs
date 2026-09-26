using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using MoneyBud.Presentation;
using MoneyBud.Storage;

namespace MoneyBud.Desktop;

public sealed partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// Starts MoneyBud on the data kept in the user's profile (arc42 §12, *What MoneyBud keeps*).
    /// Whether it opens, and what it says when it does not, is <see cref="MoneyBudStart"/>'s to
    /// decide; this only shows the outcome.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = MoneyBudStart.Start(new FileLedgerStore(FileLedgerStore.DefaultFolder), TimeProvider.System) switch
            {
                StartResult.Opened opened => new MainWindow(opened.App),
                StartResult.Refused refused => Saying(refused.Text),
                _ => throw new InvalidOperationException("MoneyBud neither opened nor said why not."),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// A small window that says one thing and closes MoneyBud when it is closed: there is nothing
    /// to enter anything into (§12, <i>When the data cannot be read</i>).
    /// </summary>
    private static Window Saying(string text)
    {
        var ok = new Button { Content = Tekst.Ok, HorizontalAlignment = HorizontalAlignment.Right, IsDefault = true };
        var window = new Window
        {
            Title = "MoneyBud",
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 16,
                MaxWidth = 420,
                Children = { new TextBlock { Text = text, TextWrapping = Avalonia.Media.TextWrapping.Wrap }, ok },
            },
        };
        ok.Click += (_, _) => window.Close();
        return window;
    }
}
