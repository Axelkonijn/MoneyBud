namespace MoneyBud.Prototype.Platform;

/// <summary>
/// A light tick in the hand. The phone sets <see cref="Tick"/> to its own vibration; everywhere
/// else it does nothing.
/// </summary>
public static class Haptics
{
    public static Action Tick { get; set; } = () => { };
}
