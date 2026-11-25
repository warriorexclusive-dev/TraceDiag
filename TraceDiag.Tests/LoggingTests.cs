using TraceDiag.Services;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class LoggingTests
{
    [Fact]
    public void LogLineBuilder_OutputsTimestampLevelAndMessage()
    {
        var line = LogLineBuilder.Build("INFO", "hello", null);
        Assert.Contains("[INFO] hello", line);
        Assert.Matches(@"^\[\d{4}-\d{2}-\d{2} ", line);
    }

    [Fact]
    public void LogLineBuilder_AppendsExceptionDetails()
    {
        var line = LogLineBuilder.Build("ERROR", "failed", new InvalidOperationException("boom"));
        Assert.Contains("failed", line);
        Assert.Contains("InvalidOperationException", line);
        Assert.Contains("boom", line);
    }

    [Fact]
    public void LogFileWriter_CreatesDirectoryAndAppends()
    {
        using var temp = new TempDirectory();
        var path = temp.File("logs/test.log");
        var sync = new object();
        LogFileWriter.EnsureDirectory(path);
        LogFileWriter.Append(path, "first", sync);
        LogFileWriter.Append(path, "second", sync);
        Assert.Equal(2, File.ReadAllLines(path).Length);
    }

    [Fact]
    public void FileDiagnosticLogger_WritesAllLevels()
    {
        using var temp = new TempDirectory();
        var logger = new FileDiagnosticLogger(temp.File("test.log"));
        logger.Info("i");
        logger.Warn("w");
        logger.Error("e");
        var text = File.ReadAllText(logger.LogFilePath);
        Assert.Contains("[INFO] i", text);
        Assert.Contains("[WARN] w", text);
        Assert.Contains("[ERROR] e", text);
    }

    [Fact]
    public void FileDiagnosticLogger_WithInvalidPath_DoesNotThrow()
    {
        var logger = new FileDiagnosticLogger(string.Empty);
        logger.Info("ignored");
    }
}
