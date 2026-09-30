using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MoneyBud.Domain;
using MoneyBud.Phone.Motion;
using MoneyBud.Phone.Ring;
using MoneyBud.Presentation;

namespace MoneyBud.Phone.Views;

/// <summary>The home screen: the ring, what its hole says, and the period under it; and the redraw after every change.</summary>
public sealed partial class MainView
{
    private bool _stepping;
    private bool _redrawWaiting;
    private FrameLoop? _count;
    private Notice? _noticeShown;
    private Question? _questionShown;

    /// <summary>Opening the app: the ring draws itself in, the spent parts fill, the figure counts up.</summary>
    private void Open()
    {
        if (_opened)
        {
            return;
        }

        _opened = true;
        LayOut();
        ShowPeriod();
        RefreshRing(animate: false);
        PeriodText.Opacity = 0;
        DrawRingIn(delay: 0.1);
        Tween.Run(this, 0.6, Ease.OutCubic, t =>
        {
            PeriodText.Opacity = t;
            PeriodText.RenderTransform = new TranslateTransform(0, 10 * (1 - t));
        }, delay: 0.25);
        RebuildAll();
        ShowMessages();
        if (_settings.ShowsHints)
        {
            ShowHints(delay: 1.7);
        }
    }

    /// <summary>
    /// The ring draws itself in, the spent parts fill, the figure counts up — and then the theme may
    /// finish it off: kintsugi's gold runs into the seams.
    /// </summary>
    private void DrawRingIn(double delay)
    {
        Ring.Reveal = 0;
        Ring.Fill = 0;
        Ring.Mend = 0;
        Hole.Opacity = 0;
        Tween.Run(this, 1.0, Ease.OutCubic, t => Ring.Reveal = t, delay: delay);
        Tween.Run(this, 0.8, Ease.InOutCubic, t => Ring.Fill = t, delay: delay + 0.6);
        Tween.Run(this, 1.1, Ease.InOutCubic, t => Ring.Mend = t, delay: delay + 1.1);
        Tween.Run(this, 0.5, Ease.OutCubic, t => Hole.Opacity = t, delay: delay + 0.4);
        ShowHole(countUp: true, delay: delay + 0.4);
    }

    /// <summary>The period's name, <i>Huidige periode</i> under it on the current one, and the save line.</summary>
    private void ShowPeriod()
    {
        PeriodTitle.Text = App.PeriodTitle;
        PeriodSubtitle.Text = App.PeriodLabel;
        PeriodSubtitle.IsVisible = App.PeriodLabel is not null;
        ShowSaveLine(SaveLine);
    }

    /// <summary>
    /// Stepping to another period: the ring drains — the spent parts empty, then the bands pull back
    /// — and the new period's ring draws in, while the name slides out and the new one in.
    /// </summary>
    private void StepPeriod(int direction)
    {
        if (_stepping)
        {
            return;
        }

        _stepping = true;
        Tween.Run(this, 0.22, Ease.InCubic, t => Ring.Fill = 1 - t);
        Tween.Run(this, 0.3, Ease.InCubic, t =>
        {
            Ring.Reveal = 1 - t;
            Hole.Opacity = 1 - t;
            PeriodText.Opacity = 1 - t;
            PeriodText.RenderTransform = new TranslateTransform(-direction * 36 * t, 0);
        }, delay: 0.14, done: () =>
        {
            if (direction < 0)
            {
                _screen.StepBack();
            }
            else
            {
                _screen.StepForward();
            }

            Redraw();
            Ring.Fill = 0;
            Ring.Mend = 0;
            ShowHole(countUp: true);

            Tween.Run(this, 0.6, Ease.OutCubic, t =>
            {
                Ring.Reveal = t;
                Hole.Opacity = t;
                PeriodText.Opacity = t;
                PeriodText.RenderTransform = new TranslateTransform(direction * 36 * (1 - t), 0);
            }, done: () => _stepping = false);
            Tween.Run(this, 0.55, Ease.InOutCubic, t => Ring.Fill = t, delay: 0.35);
            Tween.Run(this, 0.9, Ease.InOutCubic, t => Ring.Mend = t, delay: 0.55);
        });
    }

    /// <summary>
    /// The ring on screen, as the Overview has it: the shared ring's slices, shares and fills (plan
    /// for increment 14, D6), each slice in the colour of its place. A slice that changed grows or
    /// shrinks to its new size.
    /// </summary>
    private void RefreshRing(bool animate)
    {
        var ring = App.Overview.Ring;
        var drawn = ring.Slices
            .Select((s, i) => new DrawnSlice(s.Category, s.FilledShare, s.Marker == Marker.Over, i, s.IsUnassigned))
            .ToList();
        Ring.Show(drawn, ring.Slices.Select(s => (s.Start, s.Sweep)).ToList(), animate && !_stepping);
        ShowChosen(ring);
    }

    /// <summary>The slice chosen lifts, and the others fade back.</summary>
    private void ShowChosen(Presentation.Ring ring)
    {
        var chosen = _screen.ChosenSlice;
        Ring.Selected = chosen is null ? -1 : ring.Slices.ToList().FindIndex(s => s.Category == chosen.Category && s.IsUnassigned == chosen.IsUnassigned);
    }

    /// <summary>Another slice chosen, or none: the ring, its centre, and an open budget panel follow; nothing else changed.</summary>
    private void ChoiceChanged()
    {
        ShowChosen(App.Overview.Ring);
        ShowHole(countUp: false, settle: true);
        if (BudgetSheet.IsVisible)
        {
            BuildBudget();
        }
    }

    /// <summary>
    /// What the hole says: <i>Niet toegewezen</i> and its figure, or everything the row of the slice
    /// pointed at shows, as the desktop's ring centre does (arc42 §12, <i>Hovering a slice shows its
    /// figures</i>); an empty ring says so instead.
    /// </summary>
    /// <param name="countUp">The figure counts up from zero, as when the ring draws itself in.</param>
    /// <param name="settle">A quick settle, for another slice pointed at; never for a redraw that changed nothing here.</param>
    private void ShowHole(bool countUp, double delay = 0, bool settle = false)
    {
        var overview = App.Overview;

        // A redraw — after an act, a tick, a panel coming to rest — leaves the centre as it is when
        // what it shows has not changed: rebuilding it made it flicker at every pull (Axel, 2026-09-30).
        var shows = HoleShows(overview);
        if (!countUp && shows == _holeShows)
        {
            return;
        }

        _holeShows = shows;
        Hole.Children.Clear();
        _count?.Stop();

        if (overview.RingHint is { } hint)
        {
            var empty = Ui.Text(hint, "muted");
            empty.TextWrapping = TextWrapping.Wrap;
            empty.TextAlignment = TextAlignment.Center;
            empty.HorizontalAlignment = HorizontalAlignment.Center;
            Hole.Children.Add(empty);
        }
        else if (App.PointedRow is { } row)
        {
            // The ring's own rows are made before the colours are handed out; the Overview's have them.
            var slice = overview.Rows.FirstOrDefault(r => r.Name == row.Name)?.SliceIndex;
            var title = Ui.Row(8, Dot(slice, 9), Ui.Text(row.Name, "row"));
            title.HorizontalAlignment = HorizontalAlignment.Center;
            Hole.Children.Add(title);
            if (row.IsArchived)
            {
                Hole.Children.Add(Centred(Ui.Text(Tekst.Archived, "faint")));
            }

            var amount = Ui.Text(row.RemainingText, "big");
            if (row.IsOverBudget)
            {
                amount.Classes.Add("danger");
            }

            Hole.Children.Add(Centred(amount));
            Hole.Children.Add(Centred(Ui.Text(Tekst.Remaining, "muted")));
            if (row.IsOverBudget)
            {
                Hole.Children.Add(Centred(Ui.Badge(Tekst.OverBudget, "Danger", "OnAccent")));
            }

            Hole.Children.Add(Centred(Ui.Text(Tekst.Figure(Tekst.Budget, row.Budget), "faint")));
            Hole.Children.Add(Centred(Ui.Text(Tekst.Figure(Tekst.Spent, row.Spent), "faint")));
            if (row.AccumulatedText is { } accumulated)
            {
                var line = Ui.Row(6, Ui.Text(accumulated, "faint"));
                if (row.IsAccumulatedBelowZero)
                {
                    line.Children.Add(Ui.Badge(Tekst.Overdrawn, "Danger", "OnAccent"));
                }

                Hole.Children.Add(Centred(line));
            }
        }
        else
        {
            Hole.Children.Add(Centred(Ui.Text(Tekst.Unassigned, "muted")));
            var amount = Ui.Text(overview.UnassignedText, "big");
            if (overview.IsOverAssigned)
            {
                amount.Classes.Add("danger");
            }

            Hole.Children.Add(Centred(amount));
            if (overview.IsOverAssigned)
            {
                Hole.Children.Add(Centred(Ui.Badge(Tekst.OverAssigned, "Danger", "OnAccent")));
            }

            Hole.Children.Add(Centred(Ui.Text(Tekst.OfIncome(overview.IncomeTotal), "faint")));
            if (countUp)
            {
                var unassigned = overview.Unassigned;
                _count = Tween.Run(this, 0.9, Ease.OutCubic,
                    t => amount.Text = Tekst.Euro(Money.FromCents((long)Math.Round(unassigned.Cents * t))), delay: delay);
            }
        }

        if (settle)
        {
            // A quick settle rather than a fade, so sliding across slices keeps up with the finger.
            Tween.Run(this, 0.14, Ease.OutCubic, t =>
            {
                Hole.Opacity = 0.55 + 0.45 * t;
                Hole.RenderTransform = new ScaleTransform(0.97 + 0.03 * t, 0.97 + 0.03 * t);
            });
        }

        static Control Centred(Control control)
        {
            control.HorizontalAlignment = HorizontalAlignment.Center;
            return control;
        }
    }

    // What the centre shows now, as one string: the same string, the same centre.
    private string? _holeShows;

    private string HoleShows(PeriodOverview overview) =>
        overview.RingHint is { } hint ? hint
        : App.PointedRow is { } row
            ? string.Join('|', row.Name, row.IsArchived, row.RemainingText, row.IsOverBudget, row.BudgetText, row.SpentText,
                row.AccumulatedText, row.IsAccumulatedBelowZero, overview.Rows.FirstOrDefault(r => r.Name == row.Name)?.SliceIndex)
            : string.Join('|', overview.UnassignedText, overview.IsOverAssigned, overview.IncomeTotal.Cents);

    /// <summary>A category's dot in the colour of its slice, or plain when it has none.</summary>
    private static Border Dot(int? slice, double size = 10) =>
        slice is { } index ? Ui.Dot(index, size) : Ui.ColouredDot("Faint", size);

    // ---- Redrawing after a change ---------------------------------------------------------------

    /// <summary>Everything may have changed: redraw once, when the changes of this moment are done.</summary>
    private void ScheduleRedraw()
    {
        if (_redrawWaiting)
        {
            return;
        }

        _redrawWaiting = true;
        Dispatcher.UIThread.Post(Redraw, DispatcherPriority.Normal);
    }

    private void Redraw()
    {
        _redrawWaiting = false;
        if (!_opened)
        {
            return;
        }

        ShowPeriod();
        RefreshRing(animate: true);
        if (!_stepping)
        {
            ShowHole(countUp: false);
        }

        RebuildAll();
        ShowMessages();
    }
}
