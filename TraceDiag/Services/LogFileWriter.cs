using System.Text;

namespace TraceDiag.Services;

internal static class LogFileWriter
{
    internal static void EnsureDirectory(string logFilePath)
    {
        var directory = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
    }

    internal static void Append(string logFilePath, string line, object sync)
    {
        lock (sync) File.AppendAllText(logFilePath, line + Environment.NewLine, Encoding.UTF8);
    }
}
