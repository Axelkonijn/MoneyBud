using Avalonia;

namespace MoneyBud.Phone.Desktop;

/// <summary>
/// The phone on the PC. <c>dotnet run --project src/MoneyBud.Phone.Desktop</c> opens it in a
/// phone-sized window, the mouse standing in for a finger, on data of its own in
/// <c>%LOCALAPPDATA%\MoneyBudPhone</c> — never the desktop's. With <c>snapshot &lt;folder&gt; [dark|light|kintsugi-dark|kintsugi-light …]</c>
/// it saves pictures of every screen instead (<see cref="Snapshot"/>).
/// </summary>
public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args is ["snapshot", .. var rest])
        {
            Snapshot.Run(rest);
            return;
        }

        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MoneyBudPhone");
        PhoneHost.Current = new PhoneHost
        {
            DataFolder = Path.Combine(folder, "data"),
            SettingsFolder = Path.Combine(folder, "settings"),
        };

        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .StartWithClassicDesktopLifetime(args);
    }
}
