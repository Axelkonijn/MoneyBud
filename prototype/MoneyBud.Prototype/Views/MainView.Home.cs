using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MoneyBud.Prototype.Motion;
using MoneyBud.Prototype.Ring;
using MoneyBud.Prototype.Sample;

namespace MoneyBud.Prototype.Views;

/// <summary>The home screen: the period above, the ring, and what the ring's hole says.</summary>
public sealed partial class MainView
{
    private bool _stepping;
    private FrameLoop? _count;

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
        Ring.Show(SlicesFor(_offset), animate: false);
        Ring.Reveal = 0;
        Ring.Fill = 0;
        PeriodText.Opacity = 0;
        Hole.Opacity = 0;

        Tween.Run(this, 1.0, Ease.OutCubic, t => Ring.Reveal = t, delay: 0.1);
        Tween.Run(this, 0.8, Ease.InOutCubic, t => Ring.Fill = t, delay: 0.7);
        Tween.Run(this, 0.6, Ease.OutCubic, t =>
        {
            PeriodText.Opacity = t;
            PeriodText.RenderTransform = new TranslateTransform(0, 10 * (1 - t));
        }, delay: 0.25);
        Tween.Run(this, 0.5, Ease.OutCubic, t => Hole.Opacity = t, delay: 0.5);
        ShowHole(countUp: true, delay: 0.5);
        MaybeShowHints();
    }

    private void ShowPeriod()
    {
        PeriodTitle.Text = Dutch.Month(_ledger.FirstDay(_offset));
        PeriodSubtitle.Text = _offset switch
        {
            0 => "Huidige periode",
            < 0 => $"{Dutch.Day(_ledger.FirstDay(_offset))} – {Dutch.Day(_ledger.LastDay(_offset))} · voorbij",
            _ => $"{Dutch.Day(_ledger.FirstDay(_offset))} – {Dutch.Day(_ledger.LastDay(_offset))} · komt nog",
        };
    }

    /// <summary>
    /// Stepping to another period: the ring drains — the spent parts empty, then the bands pull
    /// back — and the new period's ring draws in, while the name slides out and the new one in.
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
            _offset += direction;
            Ring.Selected = -1;
            ShowPeriod();
            Ring.Show(SlicesFor(_offset), animate: false);
            Ring.Fill = 0;
            ShowHole(countUp: true);
            RebuildAll();

            Tween.Run(this, 0.6, Ease.OutCubic, t =>
            {
                Ring.Reveal = t;
                Hole.Opacity = t;
                PeriodText.Opacity = t;
                PeriodText.RenderTransform = new TranslateTransform(direction * 36 * (1 - t), 0);
            }, done: () => _stepping = false);
            Tween.Run(this, 0.55, Ease.InOutCubic, t => Ring.Fill = t, delay: 0.35);
        });
    }

    /// <summary>The slices for a period: largest Budget first, Niet toegewezen last.</summary>
    private IReadOnlyList<RingSlice> SlicesFor(int offset)
    {
        var slices = _ledger.Categories
            .Where(c => _ledger.BudgetOf(c, offset) > 0)
            .OrderByDescending(c => _ledger.BudgetOf(c, offset))
            .Select(c =>
            {
                var budget = _ledger.BudgetOf(c, offset);
                var spent = _ledger.SpentOn(c, offset);
                return new RingSlice(c.Name, budget, (double)Math.Min(1, spent / budget), spent > budget, c.Colour, false);
            })
            .ToList();

        var unassigned = _ledger.Unassigned(offset);
        if (unassigned > 0)
        {
            slices.Add(new RingSlice(UnassignedName, unassigned, 0, false, 0, true));
        }

        return slices;
    }

    private const string UnassignedName = "Niet toegewezen";

    /// <summary>Redraws the ring after a change, growing or shrinking the slices that changed.</summary>
    private void RefreshRing()
    {
        Ring.Show(SlicesFor(_offset), animate: true);
        ShowHole(countUp: false);
    }

    /// <summary>What the hole says: Niet toegewezen, or everything about the slice pointed at.</summary>
    private void ShowHole(bool countUp, double delay = 0)
    {
        Hole.Children.Clear();
        _count?.Stop();
        var category = Ring.SelectedName is { } name && name != UnassignedName ? _ledger.Find(name) : null;

        if (category is null)
        {
            var unassigned = _ledger.Unassigned(_offset);
            var label = Ui.Text(unassigned < 0 ? "Te veel toegewezen" : "Niet toegewezen", "muted");
            label.HorizontalAlignment = HorizontalAlignment.Center;
            var amount = Ui.Text(Dutch.Euro(unassigned), "big");
            if (unassigned < 0)
            {
                amount.Classes.Add("danger");
            }

            var income = Ui.Text($"van {Dutch.Euro(_ledger.Incomes(_offset).Sum(i => i.Amount))} inkomen", "faint");
            Hole.Children.Add(label);
            Hole.Children.Add(Centred(amount, unassigned < 0));
            Hole.Children.Add(income);
            income.HorizontalAlignment = HorizontalAlignment.Center;
            if (countUp)
            {
                _count = Tween.Run(this, 0.9, Ease.OutCubic, t => amount.Text = Dutch.Euro(decimal.Round(unassigned * (decimal)t, 2)), delay: delay);
            }
        }
        else
        {
            var budget = _ledger.BudgetOf(category, _offset);
            var spent = _ledger.SpentOn(category, _offset);
            var remaining = budget - spent;
            var title = Ui.Row(8, Ui.Dot(category.Colour, 9), Ui.Text(category.Name, "row"));
            title.HorizontalAlignment = HorizontalAlignment.Center;
            var amount = Ui.Text(Dutch.Euro(remaining), "big");
            if (remaining < 0)
            {
                amount.Classes.Add("danger");
            }

            var label = Ui.Text("Resterend", "muted");
            label.HorizontalAlignment = HorizontalAlignment.Center;
            var figures = Ui.Text($"{Dutch.Euro(budget)} budget · {Dutch.Euro(spent)} uit", "faint");
            figures.HorizontalAlignment = HorizontalAlignment.Center;
            Hole.Children.Add(title);
            Hole.Children.Add(Centred(amount, remaining < 0));
            Hole.Children.Add(label);
            Hole.Children.Add(figures);
            if (category.BackedBy is { } account)
            {
                var backed = Ui.Text($"Staat op {account}", "faint");
                backed.HorizontalAlignment = HorizontalAlignment.Center;
                Hole.Children.Add(backed);
            }
        }

        if (!countUp)
        {
            // A quick settle rather than a fade, so sliding across slices keeps up with the finger.
            Tween.Run(this, 0.14, Ease.OutCubic, t =>
            {
                Hole.Opacity = 0.55 + 0.45 * t;
                Hole.RenderTransform = new ScaleTransform(0.97 + 0.03 * t, 0.97 + 0.03 * t);
            });
        }

        static Control Centred(TextBlock amount, bool marked)
        {
            var row = Ui.Row(0, amount);
            if (marked)
            {
                row.Children.Add(Ui.Marker());
            }

            row.HorizontalAlignment = HorizontalAlignment.Center;
            return row;
        }
    }
}
