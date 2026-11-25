using TraceDiag.Services.Abstractions;

namespace TraceDiag.Tests.TestData;

internal sealed class FakeDiagnosticLogger : IDiagnosticLogger
{
    internal List<string> Infos { get; } = new();
    internal List<string> Warnings { get; } = new();
    internal List<string> Errors { get; } = new();

    public void Info(string message) => Infos.Add(message);

    public void Warn(string message) => Warnings.Add(message);

    public void Error(string message, Exception? exception = null) => Errors.Add(message);
}
