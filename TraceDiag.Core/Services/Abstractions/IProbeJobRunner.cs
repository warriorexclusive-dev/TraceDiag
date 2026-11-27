using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Abstractions;

/// <summary>Executes one probe job; knows nothing about the calling algorithm.</summary>
internal interface IProbeJobRunner
{
    Task<ProbeResult> RunAsync(
        DiagnosticContext context,
        ProbeJob job,
        IProgress<string> progress,
        CancellationToken cancellationToken);
}
