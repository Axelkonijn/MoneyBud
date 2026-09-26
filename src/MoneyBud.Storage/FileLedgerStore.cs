using System.Text;
using MoneyBud.Domain;

namespace MoneyBud.Storage;

/// <summary>
/// Keeps the ledger in one file, <c>moneybud.json</c>, in a folder of its own (ADR 0007).
///
/// <list type="bullet">
/// <item><b>Written whole, never in place.</b> A save writes <c>moneybud.json.tmp</c>, flushes it
/// to the disk, and only then renames it over <c>moneybud.json</c>. A save cut off at any point
/// leaves the file that was there before, whole, or the new one, whole — never part of either
/// (arc42 §12, <i>An interrupted save never damages the previous one</i>). A <c>.tmp</c> left behind
/// by a save that was cut off is never read, and the next save writes over it.</item>
/// <item><b>Held by one MoneyBud.</b> <see cref="TryClaim"/> opens <c>moneybud.lock</c> so that
/// nothing else can, and keeps it open until disposed. The operating system lets go when the
/// process ends, however it ends, so a crash never leaves the data held (§12, <i>Only one
/// MoneyBud at a time</i>).</item>
/// <item><b>Never writes what it could not read.</b> Loading changes nothing, and the screen never
/// saves over data that could not be read, because MoneyBud does not start (§12).</item>
/// </list>
/// </summary>
public sealed class FileLedgerStore : ILedgerStore
{
    public const string DataFileName = "moneybud.json";
    public const string TemporaryFileName = "moneybud.json.tmp";
    public const string LockFileName = "moneybud.lock";

    // Strict, so that bytes which are not UTF-8 make the file unreadable rather than being read
    // as replacement characters.
    private static readonly Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private FileStream? held;

    public FileLedgerStore(string folder) => Folder = folder;

    /// <summary>
    /// Where MoneyBud keeps its data: a folder of its own in the user's local application data —
    /// on Windows <c>%LOCALAPPDATA%\MoneyBud</c> (§12, <i>A fixed place in the user's
    /// profile</i>). Read from the profile and <b>never</b> from the working directory, because
    /// MoneyBud is run from inside a working copy of a public repository, where data must never
    /// land. Local rather than roaming: a file rewritten after every change is not one to sync.
    ///
    /// <para>Empty when the profile has no such folder; the store then cannot be reached.</para>
    /// </summary>
    public static string DefaultFolder =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)
            is { Length: > 0 } profile
            ? Path.Combine(profile, "MoneyBud")
            : string.Empty;

    public string Folder { get; }

    public string DataFile => Path.Combine(Folder, DataFileName);

    private string TemporaryFile => Path.Combine(Folder, TemporaryFileName);

    private string LockFile => Path.Combine(Folder, LockFileName);

    public Claim TryClaim()
    {
        if (held is not null) return Claim.Claimed;
        if (!Path.IsPathFullyQualified(Folder)) return Claim.Unreachable;

        try
        {
            Directory.CreateDirectory(Folder);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return Claim.Unreachable;
        }

        try
        {
            held = new FileStream(LockFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            return Claim.Claimed;
        }
        catch (IOException)
        {
            // The lock file is there and open elsewhere: another MoneyBud holds it.
            return File.Exists(LockFile) ? Claim.HeldElsewhere : Claim.Unreachable;
        }
        catch (UnauthorizedAccessException)
        {
            return Claim.Unreachable;
        }
    }

    public LoadResult Load()
    {
        EnsureClaimed();

        if (!File.Exists(DataFile)) return new LoadResult.NoData();

        try
        {
            return LedgerJson.Read(File.ReadAllText(DataFile, Utf8)) is { } snapshot
                ? new LoadResult.Loaded(snapshot)
                : new LoadResult.Unreadable();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or DecoderFallbackException)
        {
            return new LoadResult.Unreadable();
        }
    }

    public bool TrySave(LedgerSnapshot snapshot)
    {
        EnsureClaimed();

        var bytes = Utf8.GetBytes(LedgerJson.Write(snapshot));

        try
        {
            Directory.CreateDirectory(Folder);

            using (var temporary = new FileStream(TemporaryFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                temporary.Write(bytes);
                temporary.Flush(flushToDisk: true);
            }

            File.Move(TemporaryFile, DataFile, overwrite: true);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public void Dispose()
    {
        held?.Dispose();
        held = null;
    }

    private void EnsureClaimed()
    {
        if (held is null)
            throw new InvalidOperationException("The store is used before it was claimed.");
    }
}
