namespace TraceDiag.Services.Logging;

/// <summary>Builds the next free file name for log rotation.</summary>
internal static class LogFileNameFactory
{
    internal static string Next(string currentPath)
    {
        var directory = Path.GetDirectoryName(currentPath) ?? string.Empty;
        var stem = Path.GetFileNameWithoutExtension(currentPath);
        var extension = Path.GetExtension(currentPath);
        for (var index = 1; ; index++)
        {
            var candidate = Path.Combine(directory, BuildName(stem, index, extension));
            if (!File.Exists(candidate)) return candidate;
        }
    }

    private static string BuildName(string stem, int index, string extension) =>
        TextFormatter.Fmt(Strings.App.RotatedLogFileNameFormat, stem, index, extension);
}
