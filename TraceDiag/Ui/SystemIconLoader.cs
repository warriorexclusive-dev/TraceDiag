namespace TraceDiag.Ui;

internal static class SystemIconLoader
{
    internal static Icon? TryLoadNetworkIcon()
    {
        foreach (var path in GetCandidatePaths())
        {
            var icon = TryExtract(path);
            if (icon is not null) return icon;
        }
        return null;
    }

    private static string[] GetCandidatePaths() =>
    [
        Path.Combine(Environment.SystemDirectory, Strings.App.NetworkIconFile1),
        Path.Combine(Environment.SystemDirectory, Strings.App.NetworkIconFile2),
        Path.Combine(Environment.SystemDirectory, Strings.App.NetworkIconFile3)
    ];

    private static Icon? TryExtract(string path)
    {
        try { return File.Exists(path) ? Icon.ExtractAssociatedIcon(path) : null; }
        catch { return null; }
    }
}
