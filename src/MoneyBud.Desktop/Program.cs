using System.Globalization;
using Avalonia;

namespace MoneyBud.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // The date picker formats with the thread's culture. Everything MoneyBud words itself is
        // fixed Dutch regardless (Tekst); this makes the toolkit's own text agree with it.
        var dutch = CultureInfo.GetCultureInfo("nl-NL");
        CultureInfo.DefaultThreadCurrentCulture = dutch;
        CultureInfo.DefaultThreadCurrentUICulture = dutch;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
}
