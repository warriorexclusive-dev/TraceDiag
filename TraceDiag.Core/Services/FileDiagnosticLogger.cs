using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Logging;

namespace TraceDiag.Services;

public sealed class FileDiagnosticLogger : IDiagnosticLogger, ILogPathProvider, IDisposable
{
    private readonly AsyncLogWriter _writer;

    public FileDiagnosticLogger(string logFilePath) => _writer = new AsyncLogWriter(logFilePath);

    public string LogFilePath => _writer.CurrentPath;

    public void Info(string message) => Write(Strings.LogLevel.Info, message, null);

    public void Warn(string message) => Write(Strings.LogLevel.Warn, message, null);

    public void Error(string message, Exception? exception = null) => Write(Strings.LogLevel.Error, message, exception);

    public void Flush() => _writer.Flush();

    internal void WriteLine(string line) => _writer.Write(line);

    public void Dispose() => _writer.Dispose();

    private void Write(string level, string message, Exception? exception) =>
        _writer.Write(LogLineBuilder.Build(level, message, exception));
}
