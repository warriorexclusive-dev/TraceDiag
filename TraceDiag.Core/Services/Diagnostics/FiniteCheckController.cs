using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Describes the finite diagnostic algorithm only.</summary>
internal sealed class FiniteCheckController(
    IProbeJobFactory factory,
    IProbeJobRunner runner,
    IFragmentationPrecheckController precheck,
    IFiniteHopTask hopTask) : IFiniteCheckController
{
    public async Task RunAsync(DiagnosticContext context, DiagnosticInput input,
        IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken)
    {
        var normalized = Normalize(input);
        var sizes = ResolveSizes(normalized);
        DiagnosticReporter.ReportHeader(context, progress, normalized, sizes);
        var discovery = await DiscoverAsync(context, normalized, progress, stats, cancellationToken);
        if (discovery.Hops.Count == 0) return;
        var checkable = discovery.Hops.Where(hop => !hop.IsSkipped).ToList();
        if (checkable.Count > 0) await precheck.RunAsync(context, checkable, progress, cancellationToken);
        await ProbeAsync(context, normalized, sizes, discovery, progress, stats, cancellationToken);
        ReportFinish(context, normalized, discovery.Hops, progress, stats);
    }

    private Task<HostDiscoveryResult> DiscoverAsync(DiagnosticContext context, DiagnosticInput input,
        IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken) =>
        RouteDiscovery.DiscoverAsync(context, input.RemoteAddress, input.LossDelayMs,
            input.DontFragment, factory, runner, progress, stats, cancellationToken);

    private Task ProbeAsync(DiagnosticContext context, DiagnosticInput input, IReadOnlyList<int> sizes,
        HostDiscoveryResult discovery, IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken) =>
        DiagnosticBatch.ProbeAllAsync(context, sizes, discovery.Hops, discovery.SkippedTtls,
            input.Attempts, input.LossDelayMs, progress, stats, cancellationToken,
            hopTask, ProbeTriggers.Create(input.DontFragment));

    private static void ReportFinish(DiagnosticContext context, DiagnosticInput input,
        IReadOnlyList<HopInfo> hops, IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        DiagnosticReporter.Separator(context, progress);
        RunSummary.Report(context, input.RemoteAddress, hops, progress);
        DiagnosticReporter.ReportStats(stats, hops);
        DiagnosticReporter.Report(context, progress, Strings.Diag.RunFinished);
    }

    private static DiagnosticInput Normalize(DiagnosticInput input) =>
        input.DontFragment
            ? input with
            {
                DynamicPacketSize = false,
                MaxPacketSize = Math.Min(input.MaxPacketSize, ProbeLimits.DontFragmentPacketSize)
            }
            : input;

    private static IReadOnlyList<int> ResolveSizes(DiagnosticInput input) =>
        input.DontFragment
            ? [Math.Min(input.MaxPacketSize, ProbeLimits.DontFragmentPacketSize)]
            : PacketSizePlanner.Build(input.MaxPacketSize, input.DynamicPacketSize);
}
