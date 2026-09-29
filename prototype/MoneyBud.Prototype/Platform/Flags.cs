namespace MoneyBud.Prototype.Platform;

/// <summary>The one thing the prototype remembers between openings: whether the hints were shown.</summary>
public static class Flags
{
    private static string File => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MoneyBudPrototype", "hints-shown");

    public static bool HintsShown
    {
        get
        {
            try
            {
                return System.IO.File.Exists(File);
            }
            catch (IOException)
            {
                return false;
            }
        }

        set
        {
            try
            {
                if (value)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(File)!);
                    System.IO.File.WriteAllText(File, "");
                }
                else
                {
                    System.IO.File.Delete(File);
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // Showing the hints again next time is harmless.
            }
        }
    }
}
