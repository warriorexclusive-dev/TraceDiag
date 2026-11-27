namespace TraceDiag.Services.Abstractions;

public interface IDiagnosticLogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? exception = null);
}
