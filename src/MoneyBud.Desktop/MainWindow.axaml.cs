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
        // ends while MoneyBud is open. The period on screen stays where it is (arc42 §12).
        clock = new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.Background, (_, _) => app?.Refresh());
        clock.Start();
        Closed += (_, _) => clock.Stop();
    }
}
