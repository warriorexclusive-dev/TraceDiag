using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Abstractions;

/// <summary>One independent periodic monitoring task.</summary>
internal interface IMonitorHopTask
{
    Task RunAsync(
        DiagnosticContext context,
        HopInfo hop,
        int packetSize,
        IProbeTrigger trigger,
        IProgress<string> progress,
        IProgress<HopStatSnapshot> stats,
        CancellationToken cancellationToken);
}
