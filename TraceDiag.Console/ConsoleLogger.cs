using TraceDiag.Services;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Console;

/// <summary>Writes each line to the file log and to the console immediately.</summary>
internal sealed class ConsoleLogger(FileDiagnosticLogger file, Action<string> console)
    : IDiagnosticLogger, ILogPathProvider
{
    public string LogFilePath => file.LogFilePath;

    public void Info(string message) => Write(Strings.LogLevel.Info, message, null);

    public void Warn(string message) => Write(Strings.LogLevel.Warn, message, null);

    public void Error(string message, Exception? exception = null) =>
        Write(Strings.LogLevel.Error, message, exception);

    internal void Flush() => file.Flush();

    private void Write(string level, string message, Exception? exception)
    {
        var line = LogLineBuilder.Build(level, message, exception);
        file.WriteLine(line);
        console(line);
    }
}
