using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MoneyBud.Domain;
using MoneyBud.Phone.Views;
using MoneyBud.Presentation;
using MoneyBud.Storage;

namespace MoneyBud.Phone.Desktop;

/// <summary>
/// Pictures of the phone's screens without a phone (plan for increment 14, D9): the real screens,
/// over a real <see cref="MoneyBudApp"/> on synthetic data kept in a temporary folder, driven by a
/// pretend finger in a headless window. For looking, not for testing: what only the phone can show
/// (arc42 §8.5) is not here. A headless run is slow, so animations are judged on the phone.
/// </summary>
internal static class Snapshot
{
    // The day every picture is taken on: mid-March 2026, with February behind it.
    private static readonly DateTimeOffset Today = new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);

    private static double slow = 1;

    public static void Run(string[] args)
    {
        var folder = args.Length > 0 ? args[0] : "snapshots";
        Directory.CreateDirectory(folder);

        // A shader-drawn plate renders slowly without a GPU, and animations only advance per frame:
        // SLOW=10 waits ten times as long before and after each act.
        slow = double.TryParse(Environment.GetEnvironmentVariable("SLOW"), out var factor) ? factor : 1;

        AppBuilder.Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .WithInterFont()
            .SetupWithoutStarting();

        foreach (var variant in args.Length > 1 ? args[1..] : ["dark"])
        {
            var theme = variant.StartsWith("kintsugi") ? PhoneTheme.Kintsugi : PhoneTheme.Standaard;
            var appearance = variant.EndsWith("light") ? Appearance.Light : Appearance.Dark;
            void Shot(string name, Action<Window> act, bool hints = false, double settle = 1.2) =>
                Shoot(folder, $"{variant}-{name}", theme, appearance, act, hints, settle);

            Shot("01-opening", _ => Wait(0.5), hints: true, settle: 0);
            Shot("02-home", _ => { });
            Shot("03-slice", w =>
            {
                w.MouseDown(new Point(206, 300), MouseButton.Left);
                Wait(0.2);
                w.MouseMove(new Point(330, 420));
                Wait(0.2);
                w.MouseUp(new Point(330, 420), MouseButton.Left);
            });
            Shot("04-income", w => Drag(w, new Point(60, 830), new Point(330, 830)));
            Shot("05-income-form", w =>
            {
                Drag(w, new Point(60, 830), new Point(330, 830));
                Drag(w, new Point(60, 500), new Point(360, 500));
            });
            Shot("06-expenses", w => Drag(w, new Point(350, 830), new Point(80, 830)));
            Shot("07-expense-form", w =>
            {
                Drag(w, new Point(350, 830), new Point(80, 830));
                Drag(w, new Point(350, 500), new Point(50, 500));
            });
            Shot("08-expense-change", w =>
            {
                Drag(w, new Point(350, 830), new Point(80, 830));
                ClickText(w, "Albert Heijn");
            });
            Shot("09-refused", w =>
            {
                Drag(w, new Point(350, 830), new Point(80, 830));
                Drag(w, new Point(350, 500), new Point(50, 500));
                ClickOn(w, v => v is Button { Width: 48 });
            });
            Shot("10-budget", w => Drag(w, new Point(206, 850), new Point(206, 120)));
            Shot("11-budget-slice", w =>
            {
                w.MouseDown(new Point(300, 250), MouseButton.Left);
                w.MouseUp(new Point(300, 250), MouseButton.Left);
                Wait(0.3);
                Drag(w, new Point(206, 850), new Point(206, 120));
            });
            Shot("12-budget-full", w => Drag(w, new Point(206, 850), new Point(206, 120)));
            Shot("13-budget-category", w =>
            {
                Drag(w, new Point(206, 850), new Point(206, 120));
                ClickOn(w, v => v is Button b && b is not ToggleButton && b.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text == "Boodschappen"));
            });
            Shot("14-accounts", w => Drag(w, new Point(206, 820), new Point(206, 1150)));
            Shot("15-history", w =>
            {
                Drag(w, new Point(206, 820), new Point(206, 1150));
                ClickText(w, "Spaarrekening");
            });
            Shot("16-transfer", w =>
            {
                Drag(w, new Point(206, 820), new Point(206, 1150));
                ClickText(w, Tekst.TransferAct);
            });
            Shot("17-settings", w =>
            {
                Drag(w, new Point(206, 820), new Point(206, 1150));
                ClickOn(w, v => v is Button { Name: "Gear" });
            }, settle: 0.8);
            Shot("18-previous-period", w =>
            {
                ClickOn(w, v => v is Button { Name: "PreviousPeriod" });
                Wait(1.2);
            });
            Shot("19-start-day", w => ClickOn(w, v => v is Button { Name: "PeriodButton" }), settle: 0.8);
        }

        Console.WriteLine($"Saved to {Path.GetFullPath(folder)}");
    }

    private static void Shoot(string folder, string name, PhoneTheme theme, Appearance appearance, Action<Window> act, bool hints, double settle)
    {
        if (Environment.GetEnvironmentVariable("SHOTS") is { Length: > 0 } only && !only.Split(',').Any(name.Contains))
        {
            return;
        }

        var work = Path.Combine(Path.GetTempPath(), "MoneyBud.Phone.Snapshot", Guid.NewGuid().ToString("N"));
        var host = new PhoneHost
        {
            DataFolder = Path.Combine(work, "data"),
            SettingsFolder = Path.Combine(work, "settings"),
            Clock = new FixedTime(Today),
        };
        Seed(host.DataFolder, host.Clock);
        var file = new SettingsFile(host.SettingsFolder);
        var settings = PhoneSettings.Open(file.Read, file.TryWrite);
        settings.ChooseTheme(theme);
        settings.ChooseAppearance(appearance);
        if (!hints)
        {
            settings.HomeScreenOpened();
        }

        PhoneHost.Current = host;
        var view = App.Start(host);
        var window = new Window { Width = 412, Height = 900, Content = view };
        window.Show();
        Wait((hints ? 0.4 : 3.2) * slow);
        act(window);
        Wait(settle * slow);
        window.CaptureRenderedFrame()!.Save(Path.Combine(folder, name + ".png"), new Avalonia.Media.Imaging.PngBitmapEncoderOptions());
        window.Close();
        (view as MainView)?.Closing();
        try
        {
            Directory.Delete(work, recursive: true);
        }
        catch (IOException)
        {
            // A folder left in the temporary folder is harmless.
        }
    }

    /// <summary>
    /// A household's February and first half of March, all synthetic: a salary that repeats, a plan
    /// taken over, a savings account that backs <i>Sparen</i> and takes February's leftover, and a
    /// hobby spent over budget.
    /// </summary>
    private static void Seed(string folder, TimeProvider shown)
    {
        var clock = new FixedTime(new(2026, 2, 3, 12, 0, 0, TimeSpan.Zero));
        var ledger = Ledger.StartNew(clock);
        var february = ledger.CurrentPeriod;
        ledger.RecordIncome(2450m, "Salaris", new DateOnly(2026, 2, 1), repeat: Frequency.Monthly);
        foreach (var (category, amount) in (ReadOnlySpan<(string, decimal)>)[("Huur", 950m), ("Boodschappen", 380m), ("Sparen", 300m), ("Verzekeringen", 120m), ("Abonnementen", 45m), ("Hobby", 60m)])
        {
            ledger.Assign(amount, category, february);
        }

        ledger.RecordExpense(950m, "Huur", new DateOnly(2026, 2, 1), "Huur februari");
        ledger.RecordExpense(118.40m, "Verzekeringen", new DateOnly(2026, 2, 1), "Zorgverzekering");
        var savings = ledger.AddAccount("Spaarrekening", 1500m).Account!;
        ledger.SetBacking("Sparen", savings);
        ledger.SetSweepDestination("Sparen");

        clock.Now = new(2026, 2, 26, 12, 0, 0, TimeSpan.Zero);
        ledger.RecordExpense(342.10m, "Boodschappen", new DateOnly(2026, 2, 26), "Weekboodschappen");
        ledger.RecordExpense(13.99m, "Abonnementen", new DateOnly(2026, 2, 5), "Netflix", repeat: Frequency.Monthly);

        clock.Now = shown.GetUtcNow();
        ledger.TakeOverPlan(ledger.CurrentPeriod);
        ledger.RecordExpense(950m, "Huur", new DateOnly(2026, 3, 1), "Huur maart");
        ledger.RecordExpense(64.30m, "Boodschappen", new DateOnly(2026, 3, 3), "Jumbo");
        ledger.RecordExpense(72.50m, "Hobby", new DateOnly(2026, 3, 9), "Boekhandel");
        ledger.RecordExpense(18m, "Boodschappen", new DateOnly(2026, 3, 12), "Bakker");
        ledger.RecordExpense(32.15m, "Boodschappen", new DateOnly(2026, 3, 14), "Albert Heijn");
        ledger.RecordExpense(41.20m, "Boodschappen", new DateOnly(2026, 3, 15), "Lidl");
        ledger.RecordIncome(120m, "Teruggave belasting", new DateOnly(2026, 3, 10));
        ledger.Settle();
        ledger.TakeOccurrencesMade();
        ledger.TakeSweepsMade();

        using var store = new FileLedgerStore(folder);
        store.TryClaim();
        store.TrySave(ledger.ToSnapshot());
    }

    private static void ClickText(Window window, string text) =>
        ClickOn(window, v => v is Button button && button.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text == text));

    private static void ClickOn(Window window, Func<Visual, bool> which)
    {
        var target = window.GetVisualDescendants().OfType<Control>().Where(c => c.IsEffectivelyVisible).LastOrDefault(which)
            ?? throw new InvalidOperationException("Nothing to click.");
        var point = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window)!.Value;
        if (Environment.GetEnvironmentVariable("REPORT") is not null)
        {
            Console.WriteLine($"click {target.GetType().Name} {target.Bounds.Size} at {point}; hit {window.InputHitTest(point)?.GetType().Name}");
        }

        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Wait(0.8);
    }

    private static void Drag(Window window, Point from, Point to, bool release = true)
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

        Wait(0.8);
    }

    private static void Wait(double seconds)
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

    /// <summary>A clock that stands still where it is put.</summary>
    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }
}
