namespace MoneyBud.Phone;

/// <summary>
/// What the phone's screens need from whatever runs them: where the data and the settings live, the
/// tick in the hand, and how to end the app. Set by the Android host before the app starts, and by
/// the PC's phone window for developing (plan for increment 14, D1). Everything else is the same
/// wherever the screens run.
/// </summary>
public sealed class PhoneHost
{
    /// <summary>Set once, before the app starts.</summary>
    public static PhoneHost Current { get; set; } = new()
    {
        DataFolder = string.Empty,
        SettingsFolder = string.Empty,
    };

    /// <summary>
    /// The data file's folder. On the phone the app's own folder, the one copied over USB:
    /// <c>Android/data/app.moneybud/files</c> (ADR 0014, decision 3).
    /// </summary>
    public required string DataFolder { get; init; }

    /// <summary>The settings' folder, apart from the data's: on the phone the app's private folder (plan D3).</summary>
    public required string SettingsFolder { get; init; }

    /// <summary>A light tick in the hand. The phone sends its own; Android drops it when touch feedback is off.</summary>
    public Action Tick { get; init; } = () => { };

    /// <summary>Ends the app, after the one thing it had to say when the data cannot be read.</summary>
    public Action Quit { get; init; } = () => { };

    public TimeProvider Clock { get; init; } = TimeProvider.System;

    /// <summary>The app went to the background: the host calls <see cref="GoToBackground"/> when Android pauses it.</summary>
    public event Action? WentToBackground;

    /// <summary>The app is in the foreground again: the host calls <see cref="ComeBack"/> when Android resumes it.</summary>
    public event Action? CameBack;

    public void GoToBackground() => WentToBackground?.Invoke();

    public void ComeBack() => CameBack?.Invoke();
}
