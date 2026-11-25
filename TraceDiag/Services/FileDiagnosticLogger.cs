using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services;

public sealed class FileDiagnosticLogger : IDiagnosticLogger
{
    private readonly string _logFilePath;
    private readonly object _sync = new();

    public FileDiagnosticLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
        LogFileWriter.EnsureDirectory(logFilePath);
    }

    public string LogFilePath => _logFilePath;

    public void Info(string message) => Write(Strings.LogLevel.Info, message, null);

    public void Warn(string message) => Write(Strings.LogLevel.Warn, message, null);

    public void Error(string message, Exception? exception = null) => Write(Strings.LogLevel.Error, message, exception);

    private void Write(string level, string message, Exception? exception)
    {
        try { LogFileWriter.Append(_logFilePath, LogLineBuilder.Build(level, message, exception), _sync); }
        catch
        {
            // Logging must never break diagnostics flow.
        }
    }
}
