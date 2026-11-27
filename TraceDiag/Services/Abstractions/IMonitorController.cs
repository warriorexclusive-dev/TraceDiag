using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Abstractions;

/// <summary>Owns only the continuous monitoring algorithm.</summary>
internal interface IMonitorController
{
    Task RunAsync(
        DiagnosticContext context,
        MonitoringInput input,
        IProgress<string> progress,
        IProgress<HopStatSnapshot> stats,
        CancellationToken cancellationToken);
}
