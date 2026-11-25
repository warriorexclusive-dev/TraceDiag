using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class DiagnosticBatch
{
    internal static async Task ProbeAllAsync(DiagnosticContext context, IReadOnlyList<int> packetSizes, List<HopInfo> hops, int attempts, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        var nonResponding = new HashSet<IPAddress>();
        foreach (var size in packetSizes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.PacketSizeHeaderFormat, size));
            await ProbeSizeAsync(context, nonResponding, hops, size, attempts, progress, stats, cancellationToken);
        }
    }

    private static async Task ProbeSizeAsync(DiagnosticContext context, HashSet<IPAddress> nonResponding, List<HopInfo> hops, int size, int attempts, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        foreach (var hop in hops)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ProbeHopAsync(context, nonResponding, hop, hops, size, attempts, progress, stats, cancellationToken);
        }
    }

    private static async Task ProbeHopAsync(DiagnosticContext context, HashSet<IPAddress> nonResponding, HopInfo hop, IReadOnlyList<HopInfo> hops, int size, int attempts, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        if (nonResponding.Contains(hop.Address)) { ReportSkipped(context, hop, progress, stats, hops); return; }
        hop.ResetStats();
        DiagnosticReporter.ReportStats(stats, hops);
        var stopped = await HopProber.ProbeAsync(context, hop, hops, new ProbeSettings(size, attempts), progress, stats, cancellationToken);
        if (stopped) nonResponding.Add(hop.Address);
    }

    private static void ReportSkipped(DiagnosticContext context, HopInfo hop, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, IReadOnlyList<HopInfo> hops)
    {
        hop.Status = Strings.Status.NoAnswer;
        DiagnosticReporter.ReportStats(stats, hops);
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.SkipHopFormat, hop.Ttl, hop.Address), isWarn: true);
    }
}
