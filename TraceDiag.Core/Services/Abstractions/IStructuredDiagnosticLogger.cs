namespace TraceDiag.Services.Abstractions;

/// <summary>Logger that keeps canonical English text separate from localized output.</summary>
public interface IStructuredDiagnosticLogger : IDiagnosticLogger
{
    void InfoStructured(string canonical, object?[] args);
    void WarnStructured(string canonical, object?[] args);
    void ErrorStructured(string canonical, object?[] args, Exception? exception);
}
