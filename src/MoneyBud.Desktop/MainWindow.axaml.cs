using Avalonia.Controls;
using Avalonia.Threading;
using MoneyBud.Presentation;

namespace MoneyBud.Desktop;

public sealed partial class MainWindow : Window
{
    private readonly DispatcherTimer clock;

    public MainWindow() : this(null!) { }

    public MainWindow(MoneyBudApp app)
    {
        InitializeComponent();
        DataContext = app;

        // Once a minute the screen looks again, so the current-period label moves when a period
        // ends while MoneyBud is open, and a save that failed is tried again. What either does is
        // MoneyBudApp.Tick's to decide (arc42 §12).
        clock = new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.Background, (_, _) => app?.Tick());
        clock.Start();

        // Closing asks nothing; MoneyBudApp.Close makes its last try at saving, and lets go.
        Closed += (_, _) =>
        {
            clock.Stop();
            app?.Close();
        };
    }
}
