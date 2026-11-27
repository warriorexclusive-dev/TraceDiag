namespace TraceDiag.Services;

internal static class AppFiles
{
    internal static string GetAppDataDirectory() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        Strings.App.DataFolder);

    internal static AppConfigStore CreateConfigStore(string appDataDir) =>
        new(Path.Combine(appDataDir, Strings.App.ConfigFile));

    internal static UiStateStore CreateUiStateStore(string appDataDir) =>
        new(Path.Combine(appDataDir, Strings.App.UiStateFile));

    internal static FileDiagnosticLogger CreateLogger(string appDataDir) =>
        new(Path.Combine(appDataDir, Strings.App.LogsFolder, BuildLogFileName()));

    private static string BuildLogFileName() =>
        TextFormatter.Fmt(Strings.App.LogFileNameFormat, DateTime.Now);
}
