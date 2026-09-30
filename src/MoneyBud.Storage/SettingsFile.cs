using System.Text;

namespace MoneyBud.Storage;

/// <summary>
/// The phone's settings, kept as text in <c>settings.json</c>, in a folder apart from the data: on the
/// phone the app's private folder, never the one copied over USB, so that folder holds the data file
/// and nothing else (plan for increment 14, D3). What the text says is <c>PhoneSettings</c>'s business;
/// this only keeps it.
///
/// <para>Written whole, never in place, as the data file is (<see cref="FileLedgerStore"/>): a write
/// cut off leaves the settings that were there before. Nothing here throws: settings are never worth
/// stopping MoneyBud for.</para>
/// </summary>
public sealed class SettingsFile(string folder)
{
    public const string FileName = "settings.json";
    private const string TemporaryFileName = "settings.json.tmp";

    public string Folder { get; } = folder;

    public string Path => System.IO.Path.Combine(Folder, FileName);

    /// <summary>The text kept, or null when there is none, or it cannot be read.</summary>
    public string? Read()
    {
        try
        {
            return File.Exists(Path) ? File.ReadAllText(Path, Encoding.UTF8) : null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return null;
        }
    }

    /// <summary>Keeps the text, whole. False when it could not be written.</summary>
    public bool TryWrite(string text)
    {
        try
        {
            Directory.CreateDirectory(Folder);
            var temporary = System.IO.Path.Combine(Folder, TemporaryFileName);
            File.WriteAllText(temporary, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            File.Move(temporary, Path, overwrite: true);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return false;
        }
    }
}
