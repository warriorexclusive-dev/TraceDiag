using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Compatibility facade for the finite per-hop task.</summary>
internal static class HopProber
{
    private static readonly IFiniteHopTask TaskImpl =
        new FiniteHopTask(new ProbeJobFactory(), new ProbeJobRunner());

    internal static IFiniteHopTask Default => TaskImpl;

    internal static Task<bool> ProbeAsync(DiagnosticContext context, HopInfo hop,
        IReadOnlyList<HopInfo> allHops, ProbeSettings settings,
        IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken) =>
        TaskImpl.ProbeAsync(context, hop, allHops, settings, new DefaultProbeTrigger(),
            progress, stats, cancellationToken);
}
