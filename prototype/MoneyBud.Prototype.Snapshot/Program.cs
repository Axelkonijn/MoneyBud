using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using MoneyBud.Prototype;
using MoneyBud.Prototype.Platform;
using MoneyBud.Prototype.Views;

// Usage: dotnet run -- <folder> [dark|light]
var folder = args.Length > 0 ? args[0] : "snapshots";
Directory.CreateDirectory(folder);
var hadHints = Flags.HintsShown;

AppBuilder.Configure<App>()
    .UseSkia()
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
    .WithInterFont()
    .SetupWithoutStarting();

foreach (var variant in args.Length > 1 ? args[1..] : ["dark"])
{
    Application.Current!.RequestedThemeVariant = variant == "light" ? ThemeVariant.Light : ThemeVariant.Dark;

    Shoot($"{variant}-01-opening", w => Wait(0.5), hints: true);
    Shoot($"{variant}-02-home", _ => { }, hints: true);
    Shoot($"{variant}-03-scrub", w =>
    {
        w.MouseDown(new Point(206, 300), MouseButton.Left);
        Wait(0.2);
        w.MouseMove(new Point(330, 420));
        Wait(0.4);
    });
    Shoot($"{variant}-04-income-dragging", w => Drag(w, new Point(60, 800), new Point(250, 800), release: false));
    Shoot($"{variant}-05-income", w => Drag(w, new Point(60, 800), new Point(330, 800)));
    Shoot($"{variant}-06-income-form", w =>
    {
        Drag(w, new Point(60, 800), new Point(330, 800));
        Drag(w, new Point(60, 500), new Point(360, 500));
    });
    Shoot($"{variant}-07-expenses", w => Drag(w, new Point(350, 800), new Point(80, 800)));
    Shoot($"{variant}-08-expense-form", w =>
    {
        Drag(w, new Point(350, 800), new Point(80, 800));
        Drag(w, new Point(350, 500), new Point(50, 500));
    });
    Shoot($"{variant}-09-budget-half", w => Drag(w, new Point(206, 820), new Point(206, 560)));
    Shoot($"{variant}-10-budget-slice", w =>
    {
        w.MouseDown(new Point(300, 250), MouseButton.Left);
        w.MouseUp(new Point(300, 250), MouseButton.Left);
        Wait(0.3);
        Drag(w, new Point(206, 820), new Point(206, 560));
    });
    Shoot($"{variant}-11-budget-full", w => Drag(w, new Point(206, 820), new Point(206, 150)));
    Shoot($"{variant}-12-accounts", w => Drag(w, new Point(206, 800), new Point(206, 1150)));
    Shoot($"{variant}-13-previous-period", w =>
    {
        w.MouseDown(new Point(30, 760), MouseButton.Left);
        w.MouseUp(new Point(30, 760), MouseButton.Left);
        Wait(0.35);
    }, settle: 0.2);
}

Flags.HintsShown = hadHints;
Console.WriteLine($"Saved to {Path.GetFullPath(folder)}");

void Shoot(string name, Action<Window> act, bool hints = false, double settle = 1.2)
{
    if (Environment.GetEnvironmentVariable("SHOTS") is { Length: > 0 } only && !only.Split(',').Any(name.Contains))
    {
        return;
    }

    Flags.HintsShown = !hints;
    var window = new Window { Width = 412, Height = 900, Content = new MainView() };
    window.Show();
    Wait(name.EndsWith("opening") ? 0.4 : 3.2);
    Report("before", window);
    act(window);
    Wait(settle);
    Report("after", window);
    window.CaptureRenderedFrame()!.Save(Path.Combine(folder, name + ".png"), new Avalonia.Media.Imaging.PngBitmapEncoderOptions());
    window.Close();
}

void Drag(Window window, Point from, Point to, bool release = true)
{
    window.MouseDown(from, MouseButton.Left);
    const int steps = 12;
    for (var i = 1; i <= steps; i++)
    {
        window.MouseMove(new Point(from.X + (to.X - from.X) * i / steps, from.Y + (to.Y - from.Y) * i / steps));
        Wait(0.016);
    }

    if (release)
    {
        window.MouseUp(to, MouseButton.Left);
    }

    Wait(0.6);
}

void Wait(double seconds)
{
    var until = DateTime.UtcNow.AddSeconds(seconds);
    do
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Thread.Sleep(8);
    }
    while (DateTime.UtcNow < until);
}

void Report(string when, Window window)
{
    if (Environment.GetEnvironmentVariable("REPORT") is null)
    {
        return;
    }

    var view = (MainView)window.Content!;
    var ring = view.FindControl<MoneyBud.Prototype.Ring.RingView>("Ring")!;
    var home = view.FindControl<Panel>("Home")!;
    Console.WriteLine($"{when}: reveal={ring.Reveal:0.00} fill={ring.Fill:0.00} selected={ring.Selected} ring={ring.Bounds} homeOpacity={home.Opacity} period={view.FindControl<StackPanel>("PeriodText")!.Opacity:0.00}");
}
