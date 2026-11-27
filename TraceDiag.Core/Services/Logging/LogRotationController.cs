using System.Text;

namespace TraceDiag.Services.Logging;

/// <summary>Switches to a new log file when the current one reaches its size limit.</summary>
internal sealed class LogRotationController
{
    internal const long MaxFileBytes = 4L * 1024 * 1024;
    private readonly object _sync = new();
    private string _currentPath;

    internal LogRotationController(string logFilePath) => _currentPath = logFilePath;

    internal string CurrentPath
    {
        get { lock (_sync) return _currentPath; }
    }

    internal void Append(string line)
    {
        lock (_sync)
        {
            if (ShouldRotate(line)) Rotate();
            LogFileWriter.Append(_currentPath, line, _sync);
        }
    }

    private bool ShouldRotate(string line)
    {
        if (!File.Exists(_currentPath)) return false;
        var bytes = Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;
        return new FileInfo(_currentPath).Length + bytes > MaxFileBytes;
    }

    private void Rotate()
    {
        _currentPath = LogFileNameFactory.Next(_currentPath);
        var notice = LogLineBuilder.Build(Strings.LogLevel.Info, Strings.Log.LogRotated, null);
        LogFileWriter.Append(_currentPath, notice, _sync);
    }
}
