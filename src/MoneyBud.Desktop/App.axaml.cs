using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MoneyBud.Domain;
using MoneyBud.Presentation;

namespace MoneyBud.Desktop;

public sealed partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// Every start is a first start: the default categories and nothing else, and nothing kept
    /// when MoneyBud closes (arc42 §12, *What the UI starts with, and what it keeps*; §8.3).
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow(new MoneyBudApp(Ledger.StartNew(TimeProvider.System)));

        base.OnFrameworkInitializationCompleted();
    }
}
