using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Abstractions;

/// <summary>Runs the preliminary DF ladder and reports it as extra information.</summary>
internal interface IFragmentationPrecheckController
{
    Task RunAsync(
        DiagnosticContext context,
        IReadOnlyList<HopInfo> hops,
        IProgress<string> progress,
        CancellationToken cancellationToken);
}
