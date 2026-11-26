using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Monitoring;

/// <summary>Runs one independent periodic probe loop for a single node.</summary>
internal static class MonitorHopWorker
{
    internal static async Task RunAsync(DiagnosticContext context, HopInfo hop, int packetSize,
        IProgress<string> progress, IProgress<HopStatSnapshot> stats, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(DiagnosticConstants.MonitorIntervalMs));
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await CheckAsync(context, hop, packetSize, progress, stats, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!await timer.WaitForNextTickAsync(cancellationToken)) return;
        }
    }

    private static async Task CheckAsync(DiagnosticContext context, HopInfo hop, int packetSize,
        IProgress<string> progress, IProgress<HopStatSnapshot> stats, CancellationToken cancellationToken)
    {
        var request = CreateRequest(hop, packetSize);
        var result = await ProbeExecutor.SendAsync(context, request, progress, cancellationToken);
        Apply(hop, result, context, progress, stats);
    }

    private static ProbeRequest CreateRequest(HopInfo hop, int packetSize) =>
        new(hop.Address, DiagnosticConstants.DirectProbeTtl, packetSize, DiagnosticConstants.ProbeTimeoutMs,
            TextFormatter.Fmt(Strings.Diag.MonitorContextFormat, hop.Ttl, hop.Address));

    private static void Apply(HopInfo hop, ProbeResult result, DiagnosticContext context,
        IProgress<string> progress, IProgress<HopStatSnapshot> stats)
    {
        var success = result.Outcome == ProbeOutcome.Success;
        hop.ApplyPing(success, result.RoundtripTimeMs);
        stats.Report(DiagnosticReporter.Snapshot(hop));
        Report(context, hop, result, success, progress);
    }

    private static void Report(DiagnosticContext context, HopInfo hop, ProbeResult result, bool success,
        IProgress<string> progress)
    {
        var rtt = success ? TextFormatter.Fmt(Strings.Diag.MsFormat, result.RoundtripTimeMs) : Strings.Diag.NoAnswer;
        DiagnosticReporter.Report(context, progress,
            TextFormatter.Fmt(Strings.Diag.MonitorCycleFormat, hop.Ttl, DiagnosticText.Hop(hop),
                result.BufferSize, rtt, hop.Sent, hop.Received),
            isWarn: !success);
    }
}
