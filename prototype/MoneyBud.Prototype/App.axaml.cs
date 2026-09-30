using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MoneyBud.Prototype.Views;

namespace MoneyBud.Prototype;

public sealed partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                // A phone-sized window: 412 × 900 is roughly the phone in portrait.
                desktop.MainWindow = new Window
                {
                    Title = "MoneyBud prototype",
                    Width = 412,
                    Height = 900,
                    CanResize = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Content = new MainView(),
                };
                break;

            case ISingleViewApplicationLifetime phone:
                phone.MainView = new MainView();
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
