using TraceDiag.Services;

namespace TraceDiag.Console.Tests;

public sealed class ConsoleLoggerTests
{
    [Fact]
    public void Info_WritesSameLineToFileAndConsole()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var file = new FileDiagnosticLogger(path);
            var console = new List<string>();
            var logger = new ConsoleLogger(file, console.Add);
            logger.Info("hello");
            file.Flush();
            Assert.Single(console);
            Assert.Contains("[INFO] hello", console[0]);
            Assert.Equal(console[0], File.ReadAllText(path).TrimEnd());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Info_AppearsInFileWithoutFinalFlush()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var file = new FileDiagnosticLogger(path);
            new ConsoleLogger(file, _ => { }).Info("persisted");
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < deadline && !File.ReadAllText(path).Contains("persisted"))
                Thread.Sleep(10);
            Assert.Contains("persisted", File.ReadAllText(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Error_IncludesLevelAndMessage()
    {
        var path = Path.GetTempFileName();
        try
        {
            using var file = new FileDiagnosticLogger(path);
            var console = new List<string>();
            new ConsoleLogger(file, console.Add).Error("failed");
            Assert.Contains("[ERROR] failed", console[0]);
        }
        finally { File.Delete(path); }
    }
}
