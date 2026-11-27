using System.Net;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

internal static class DiagnosticBatch
{
    internal static async Task ProbeAllAsync(DiagnosticContext context, IReadOnlyList<int> packetSizes,
        IReadOnlyList<HopInfo> hops, IReadOnlyCollection<int> skippedTtls, int attempts, int lossDelayMs,
        IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken, IFiniteHopTask? task = null, IProbeTrigger? trigger = null,
        IRouteStabilityController? stability = null)
    {
        var hopTask = task ?? HopProber.Default;
        var probeTrigger = trigger ?? new DefaultProbeTrigger();
        var routeStability = stability ?? new RouteStabilityController();
        var nonResponding = new HashSet<IPAddress>();
        foreach (var hop in hops)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ProbeHopSizesAsync(context, nonResponding, skippedTtls, hop, hops, packetSizes,
                attempts, lossDelayMs, hopTask, probeTrigger, routeStability, progress, stats, cancellationToken);
        }
    }

    private static async Task ProbeHopSizesAsync(DiagnosticContext context, HashSet<IPAddress> nonResponding,
        IReadOnlyCollection<int> skippedTtls, HopInfo hop, IReadOnlyList<HopInfo> hops, IReadOnlyList<int> packetSizes,
        int attempts, int lossDelayMs, IFiniteHopTask task, IProbeTrigger trigger,
        IRouteStabilityController stability, IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        var results = new List<HopSizeResult>();
        foreach (var size in packetSizes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DiagnosticReporter.ReportFormat(context, progress, Strings.Diag.PacketSizeHeaderFormat, size);
            var result = await ProbeHopAsync(context, nonResponding, skippedTtls, hop, hops, size,
                attempts, lossDelayMs, task, trigger, progress, stats, cancellationToken);
            if (result is not null) results.Add(result);
        }
        FinishHop(context, hop, hops, results, stability, progress, stats);
    }

    private static void FinishHop(DiagnosticContext context, HopInfo hop, IReadOnlyList<HopInfo> hops,
        IReadOnlyList<HopSizeResult> results, IRouteStabilityController stability,
        IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        HopDiagnosis.Apply(hop, results);
        stability.Apply(hop);
        DiagnosticReporter.ReportStats(stats, hops);
        ReportDiagnosis(context, hop, progress);
    }

    private static void ReportDiagnosis(DiagnosticContext context, HopInfo hop, IProgress<string> progress)
    {
        if (hop.Diagnosis == Strings.Diagnosis.None) return;
        DiagnosticReporter.ReportFormatWarn(context, progress, Strings.Diag.DiagnosisFormat,
            hop.Ttl, hop.Address, LocalizedArg.Create(hop.Diagnosis, DiagnosisText.Localize(hop.Diagnosis)));
    }

    private static async Task<HopSizeResult?> ProbeHopAsync(DiagnosticContext context,
        HashSet<IPAddress> nonResponding, IReadOnlyCollection<int> skippedTtls, HopInfo hop,
        IReadOnlyList<HopInfo> hops, int size, int attempts, int lossDelayMs,
        IFiniteHopTask task, IProbeTrigger trigger, IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        if (hop.IsSkipped || skippedTtls.Contains(hop.Ttl) || nonResponding.Contains(hop.Address))
        {
            ReportSkipped(context, hop, progress, stats, hops);
            return null;
        }
        var before = PingCounters.Capture(hop);
        var stopped = await task.ProbeAsync(context, hop, hops, new ProbeSettings(size, attempts, lossDelayMs),
            trigger, progress, stats, cancellationToken);
        if (stopped) nonResponding.Add(hop.Address);
        return CreateResult(hop, before, size);
    }

    private static HopSizeResult CreateResult(HopInfo hop, PingCounters before, int size)
    {
        var after = PingCounters.Capture(hop);
        return new HopSizeResult(size, after.Sent - before.Sent, after.Received - before.Received);
    }

    private static void ReportSkipped(DiagnosticContext context, HopInfo hop, IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats, IReadOnlyList<HopInfo> hops)
    {
        hop.Status = Strings.Status.NoAnswer;
        DiagnosticReporter.ReportStats(stats, hops);
        DiagnosticReporter.ReportFormatWarn(context, progress, Strings.Diag.SkipHopFormat,
            hop.Ttl, hop.Address);
    }
}
