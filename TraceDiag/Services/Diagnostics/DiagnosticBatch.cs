using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class DiagnosticBatch
{
    internal static async Task ProbeAllAsync(DiagnosticContext context, IReadOnlyList<int> packetSizes, IReadOnlyList<HopInfo> hops, IReadOnlySet<int> skippedTtls, int attempts, int lossDelayMs, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        var nonResponding = new HashSet<IPAddress>();
        foreach (var hop in hops)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ProbeHopSizesAsync(context, nonResponding, skippedTtls, hop, hops, packetSizes, attempts, lossDelayMs, progress, stats, cancellationToken);
        }
    }

    private static async Task ProbeHopSizesAsync(DiagnosticContext context, HashSet<IPAddress> nonResponding, IReadOnlySet<int> skippedTtls, HopInfo hop, IReadOnlyList<HopInfo> hops, IReadOnlyList<int> packetSizes, int attempts, int lossDelayMs, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        foreach (var size in packetSizes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.PacketSizeHeaderFormat, size));
            await ProbeHopAsync(context, nonResponding, skippedTtls, hop, hops, size, attempts, lossDelayMs, progress, stats, cancellationToken);
        }
    }

    private static async Task ProbeHopAsync(DiagnosticContext context, HashSet<IPAddress> nonResponding, IReadOnlySet<int> skippedTtls, HopInfo hop, IReadOnlyList<HopInfo> hops, int size, int attempts, int lossDelayMs, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        if (hop.IsSkipped || skippedTtls.Contains(hop.Ttl) || nonResponding.Contains(hop.Address)) { ReportSkipped(context, hop, progress, stats, hops); return; }
        var stopped = await HopProber.ProbeAsync(context, hop, hops, new ProbeSettings(size, attempts, lossDelayMs), progress, stats, cancellationToken);
        if (stopped) nonResponding.Add(hop.Address);
    }

    private static void ReportSkipped(DiagnosticContext context, HopInfo hop, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, IReadOnlyList<HopInfo> hops)
    {
        hop.Status = Strings.Status.NoAnswer;
        DiagnosticReporter.ReportStats(stats, hops);
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.SkipHopFormat, hop.Ttl, hop.Address), isWarn: true);
    }
}
