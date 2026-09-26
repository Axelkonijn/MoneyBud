namespace MoneyBud.Specs.Support;

/// <summary>Files in the repository that tests read as text: §12's markdown, the window's markup.</summary>
public static class Repository
{
    public static string ReadText(params string[] path) =>
        File.ReadAllText(Path.Combine([Root, .. path]));

    /// <summary>The folder that holds MoneyBud.slnx.</summary>
    public static string Root
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "MoneyBud.slnx")))
                directory = directory.Parent;

            return directory?.FullName ?? throw new InvalidOperationException("Could not find the repository root.");
        }
    }
}
