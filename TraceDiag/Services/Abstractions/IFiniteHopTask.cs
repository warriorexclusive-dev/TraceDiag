using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Abstractions;

/// <summary>One finite per-hop probe task with attempts and stop rules.</summary>
internal interface IFiniteHopTask
{
    Task<bool> ProbeAsync(
        DiagnosticContext context,
        HopInfo hop,
        IReadOnlyList<HopInfo> allHops,
        ProbeSettings settings,
        IProbeTrigger trigger,
        IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken);
}
