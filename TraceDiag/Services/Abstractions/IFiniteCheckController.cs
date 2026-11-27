using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Abstractions;

/// <summary>Owns only the finite diagnostic algorithm.</summary>
internal interface IFiniteCheckController
{
    Task RunAsync(
        DiagnosticContext context,
        DiagnosticInput input,
        IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken);
}
