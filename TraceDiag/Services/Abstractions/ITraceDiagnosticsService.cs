using TraceDiag.Models;

namespace TraceDiag.Services.Abstractions;

public interface ITraceDiagnosticsService
{
    Task RunAsync(
        DiagnosticInput input,
        IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> statsProgress,
        CancellationToken cancellationToken);
}
