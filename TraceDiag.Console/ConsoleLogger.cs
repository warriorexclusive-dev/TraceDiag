using TraceDiag.Services;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Console;

/// <summary>Writes English lines to the file log and translated lines to the console.</summary>
internal sealed class ConsoleLogger(FileDiagnosticLogger file, Action<string> console)
    : IStructuredDiagnosticLogger, ILogPathProvider
{
    public string LogFilePath => file.LogFilePath;

    public void Info(string message) => Write(Strings.LogLevel.Info, message, message, null);

    public void Warn(string message) => Write(Strings.LogLevel.Warn, message, message, null);

    public void Error(string message, Exception? exception = null) =>
        Write(Strings.LogLevel.Error, message, message, exception);

    public void InfoStructured(string canonical, object?[] args) => WriteStructured(Strings.LogLevel.Info, canonical, args);

    public void WarnStructured(string canonical, object?[] args) => WriteStructured(Strings.LogLevel.Warn, canonical, args);

    public void ErrorStructured(string canonical, object?[] args, Exception? exception) =>
        Write(Strings.LogLevel.Error, TextFormatter.Fmt(canonical, args),
            TextFormatter.FmtDisplay(Strings.T(canonical), args), exception);

    internal void Flush() => file.Flush();

    private void WriteStructured(string level, string canonical, object?[] args) =>
        Write(level, TextFormatter.Fmt(canonical, args),
            TextFormatter.FmtDisplay(Strings.T(canonical), args), null);

    private void Write(string level, string fileMessage, string consoleMessage, Exception? exception)
    {
        var lines = LogLineBuilder.BuildPair(level, fileMessage, consoleMessage, exception);
        file.WriteLine(lines[0]);
        console(lines[1]);
    }
}
