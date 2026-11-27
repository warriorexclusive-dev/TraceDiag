using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Monitoring;

/// <summary>Describes the continuous monitoring algorithm only.</summary>
internal sealed class MonitorController(
    IProbeJobFactory factory,
    IProbeJobRunner runner,
    IFragmentationPrecheckController precheck,
    IMonitorHopTask hopTask) : IMonitorController
{
    public async Task RunAsync(DiagnosticContext context, MonitoringInput input,
        IProgress<string> progress, IProgress<HopStatSnapshot> stats,
        CancellationToken cancellationToken)
    {
        var normalized = Normalize(input);
        MonitoringReporter.Header(context, progress, normalized);
        var discovery = await DiscoverAsync(context, normalized, progress, stats, cancellationToken);
        if (discovery.Hops.Count == 0) return;
        var checkable = discovery.Hops.Where(hop => !hop.IsSkipped).ToList();
        if (checkable.Count > 0) await precheck.RunAsync(context, checkable, progress, cancellationToken);
        await RunHopsAsync(context, normalized, discovery.Hops, progress, stats, cancellationToken);
    }

    private Task<HostDiscoveryResult> DiscoverAsync(DiagnosticContext context, MonitoringInput input,
        IProgress<string> progress, IProgress<HopStatSnapshot> stats, CancellationToken cancellationToken) =>
        RouteDiscovery.DiscoverAsync(context, input.RemoteAddress, input.LossDelayMs,
            input.DontFragment, factory, runner, progress, new MonitoringStatsAdapter(stats), cancellationToken);

    private async Task RunHopsAsync(DiagnosticContext context, MonitoringInput input,
        IReadOnlyList<HopInfo> hops, IProgress<string> progress, IProgress<HopStatSnapshot> stats,
        CancellationToken cancellationToken)
    {
        ReportExcluded(context, hops, progress);
        var workers = CreateWorkers(context, input, hops, progress, stats, cancellationToken);
        await Task.WhenAll(workers);
        DiagnosticReporter.Report(context, progress, Strings.Diag.MonitorStopped);
    }

    private Task[] CreateWorkers(DiagnosticContext context, MonitoringInput input, IReadOnlyList<HopInfo> hops,
        IProgress<string> progress, IProgress<HopStatSnapshot> stats, CancellationToken cancellationToken)
    {
        var trigger = ProbeTriggers.Create(input.DontFragment);
        return hops.Where(MonitorEligibility.IsEligible)
            .Select(hop => hopTask.RunAsync(context, hop, input.PacketSize, trigger, progress, stats, cancellationToken))
            .ToArray();
    }

    private static void ReportExcluded(DiagnosticContext context, IReadOnlyList<HopInfo> hops, IProgress<string> progress)
    {
        foreach (var hop in hops.Where(hop => !MonitorEligibility.IsEligible(hop)))
            DiagnosticReporter.ReportFormat(context, progress, Strings.Diag.MonitorExcludedFormat,
                hop.Ttl, DiagnosticText.HopArg(hop), LocalizedArg.Of(hop.Diagnosis));
    }

    private static MonitoringInput Normalize(MonitoringInput input) =>
        input.DontFragment
            ? input with { PacketSize = Math.Min(input.PacketSize, ProbeLimits.DontFragmentPacketSize) }
            : input;
}
