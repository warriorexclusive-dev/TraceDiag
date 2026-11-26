using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Monitoring;

/// <summary>Starts and stops the independent per-node monitoring loops.</summary>
internal static class MonitorController
{
    internal static async Task RunAsync(DiagnosticContext context, IReadOnlyList<HopInfo> hops, int packetSize,
        IProgress<string> progress, IProgress<HopStatSnapshot> stats, CancellationToken cancellationToken)
    {
        ReportExcluded(context, hops, progress);
        var workers = CreateWorkers(context, hops, packetSize, progress, stats, cancellationToken);
        await Task.WhenAll(workers);
        DiagnosticReporter.Report(context, progress, Strings.Diag.MonitorStopped);
    }

    private static Task[] CreateWorkers(DiagnosticContext context, IReadOnlyList<HopInfo> hops, int packetSize,
        IProgress<string> progress, IProgress<HopStatSnapshot> stats, CancellationToken cancellationToken) =>
        hops.Where(MonitorEligibility.IsEligible)
            .Select(hop => MonitorHopWorker.RunAsync(context, hop, packetSize, progress, stats, cancellationToken))
            .ToArray();

    private static void ReportExcluded(DiagnosticContext context, IReadOnlyList<HopInfo> hops, IProgress<string> progress)
    {
        foreach (var hop in hops.Where(hop => !MonitorEligibility.IsEligible(hop)))
            DiagnosticReporter.Report(context, progress,
                TextFormatter.Fmt(Strings.Diag.MonitorExcludedFormat, hop.Ttl, DiagnosticText.Hop(hop), hop.Diagnosis));
    }
}
