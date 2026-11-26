using System.Text;
using TraceDiag.Services;
using TraceDiag.Services.Logging;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class LogRotationTests
{
    [Fact]
    public void Logger_SwitchesToFileWhenLimitReached()
    {
        using var temp = new TempDirectory();
        var path = temp.File("logs/tracediag.log");
        using var logger = new FileDiagnosticLogger(path);
        File.WriteAllText(path, new string('x', (int)LogRotationController.MaxFileBytes), Encoding.UTF8);
        logger.Flush();
        var original = File.ReadAllText(path, Encoding.UTF8);
        logger.Info("after rotation");
        logger.Flush();
        Assert.NotEqual(path, logger.LogFilePath);
        Assert.Equal(original, File.ReadAllText(path, Encoding.UTF8));
        Assert.Contains("after rotation", File.ReadAllText(logger.LogFilePath));
    }

    [Fact]
    public void LogFileNameFactory_ReturnsNextFreeName()
    {
        using var temp = new TempDirectory();
        var first = temp.File("tracediag.log");
        var second = temp.File("tracediag-1.log");
        File.WriteAllText(first, "a");
        File.WriteAllText(second, "b");
        Assert.EndsWith("tracediag-2.log", LogFileNameFactory.Next(first));
    }
}
