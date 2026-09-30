using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MoneyBud.Phone.Themes;
using MoneyBud.Phone.Views;
using MoneyBud.Presentation;
using MoneyBud.Storage;

namespace MoneyBud.Phone;

public sealed partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                // The phone in a window on the PC: 412 × 900 is roughly the phone in portrait.
                var view = Start(PhoneHost.Current);
                var window = new Window
                {
                    Title = "MoneyBud — telefoon",
                    Width = 412,
                    Height = 900,
                    CanResize = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Content = view,
                };
                window.Closed += (_, _) => (view as MainView)?.Closing();
                desktop.MainWindow = window;
                break;

            case ISingleViewApplicationLifetime phone:
                phone.MainView = Start(PhoneHost.Current);
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Starts MoneyBud on the phone: the settings first, so it opens in the theme chosen, then the
    /// data. Whether it opens, and what it says when it does not, is <see cref="MoneyBudStart"/>'s to
    /// decide, as on the desktop; this only shows the outcome (arc42 §12, <i>Android's lifecycle</i>).
    /// </summary>
    public static Control Start(PhoneHost host)
    {
        // The toolkit's own words — the calendar's months and days — in Dutch, as the desktop's
        // Program has them (plan for increment 14, D10). MoneyBud's own text does not depend on it.
        var dutch = CultureInfo.GetCultureInfo("nl-NL");
        CultureInfo.DefaultThreadCurrentCulture = dutch;
        CultureInfo.DefaultThreadCurrentUICulture = dutch;
        CultureInfo.CurrentCulture = dutch;
        CultureInfo.CurrentUICulture = dutch;

        var file = new SettingsFile(host.SettingsFolder);
        var settings = PhoneSettings.Open(file.Read, file.TryWrite);
        Looks.Apply(settings);

        return MoneyBudStart.Start(new FileLedgerStore(host.DataFolder), host.Clock) switch
        {
            StartResult.Opened opened => Opened(opened.App),
            StartResult.Refused refused => new MessageView(refused.Text, host.Quit),
            _ => throw new InvalidOperationException("MoneyBud neither opened nor said why not."),
        };

        Control Opened(MoneyBudApp app)
        {
            settings.HomeScreenOpened();
            return new MainView(new PhoneScreen(app), settings, host);
        }
    }
}
