using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace MoneyBud.Prototype.Motion;

/// <summary>
/// Calls a step once per screen frame, with the seconds since the last one, for as long as the
/// step returns true. Everything that moves in the prototype moves through this, so it moves at
/// the screen's own rate rather than a timer's.
/// </summary>
public sealed class FrameLoop
{
    private readonly Visual _visual;
    private readonly Func<double, bool> _step;
    private TimeSpan? _last;

    private FrameLoop(Visual visual, Func<double, bool> step)
    {
        _visual = visual;
        _step = step;
    }

    public bool IsRunning { get; private set; } = true;

    public static FrameLoop Start(Visual visual, Func<double, bool> step)
    {
        var loop = new FrameLoop(visual, step);
        loop.Request();
        return loop;
    }

    public void Stop() => IsRunning = false;

    private void Request()
    {
        if (TopLevel.GetTopLevel(_visual) is { } top)
        {
            // A frame request on its own does not always cause a frame: when nothing else on
            // screen has changed yet — just after opening — it can wait forever. Asking for a
            // redraw as well makes sure the frame comes.
            top.RequestAnimationFrame(OnFrame);
            _visual.InvalidateVisual();
            return;
        }

        // Not on screen yet: finish at once rather than animate something nobody sees.
        Dispatcher.UIThread.Post(() =>
        {
            for (var i = 0; IsRunning && i < 200 && _step(0.05); i++)
            {
            }

            IsRunning = false;
        });
    }

    private void OnFrame(TimeSpan now)
    {
        if (!IsRunning)
        {
            return;
        }

        var dt = _last is { } last ? (now - last).TotalSeconds : 1 / 120.0;
        _last = now;
        if (_step(Math.Clamp(dt, 0, 0.05)))
        {
            Request();
        }
        else
        {
            IsRunning = false;
        }
    }
}

public static class Ease
{
    public static double OutCubic(double t) => 1 - Math.Pow(1 - t, 3);

    public static double InCubic(double t) => t * t * t;

    public static double InOutCubic(double t) => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;

    /// <summary>Overshoots a little and settles: for things that pop in.</summary>
    public static double OutBack(double t)
    {
        const double c1 = 1.4;
        const double c3 = c1 + 1;
        return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
    }

    public static double Lerp(double from, double to, double t) => from + (to - from) * t;
}

public static class Tween
{
    /// <summary>Runs <paramref name="apply"/> from 0 to 1 over the given time, eased.</summary>
    public static FrameLoop Run(Visual visual, double seconds, Func<double, double> ease, Action<double> apply, Action? done = null, double delay = 0)
    {
        var elapsed = -delay;
        apply(ease(0));
        return FrameLoop.Start(visual, dt =>
        {
            elapsed += dt;
            if (elapsed < 0)
            {
                return true;
            }

            var t = Math.Min(1, elapsed / seconds);
            apply(ease(t));
            if (t < 1)
            {
                return true;
            }

            done?.Invoke();
            return false;
        });
    }
}

/// <summary>
/// A value that follows the finger while dragged and springs to a resting point when let go,
/// carrying the speed of the fling into the spring. This is what makes a panel feel attached to
/// the thumb instead of played as an animation.
/// </summary>
public sealed class Spring(Visual visual, Action<double> apply)
{
    private FrameLoop? _loop;

    public double Value { get; private set; }

    public double Velocity { get; private set; }

    public double Target { get; private set; }

    /// <summary>Stiffer settles faster. 420 settles in about a quarter of a second.</summary>
    public double Stiffness { get; set; } = 420;

    public double DampingRatio { get; set; } = 1.0;

    public bool IsMoving => _loop?.IsRunning == true;

    public event Action<double>? Settled;

    /// <summary>Puts the value somewhere directly, as a drag does; stops any spring in flight.</summary>
    public void Set(double value)
    {
        _loop?.Stop();
        Value = value;
        Target = value;
        Velocity = 0;
        apply(value);
    }

    public void AnimateTo(double target, double velocity = 0)
    {
        Target = target;
        Velocity = velocity;
        if (IsMoving)
        {
            return;
        }

        _loop = FrameLoop.Start(visual, Step);
    }

    private bool Step(double dt)
    {
        const double h = 1 / 480.0;
        var damping = 2 * Math.Sqrt(Stiffness) * DampingRatio;
        for (var t = 0.0; t < dt; t += h)
        {
            var acceleration = -Stiffness * (Value - Target) - damping * Velocity;
            Velocity += acceleration * h;
            Value += Velocity * h;
        }

        if (Math.Abs(Value - Target) < 0.4 && Math.Abs(Velocity) < 12)
        {
            Value = Target;
            Velocity = 0;
            apply(Value);
            Settled?.Invoke(Value);
            return false;
        }

        apply(Value);
        return true;
    }
}
