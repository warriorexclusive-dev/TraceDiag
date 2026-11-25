using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class RouteDiscovery
{
    internal static async Task<List<HopInfo>> DiscoverAsync(DiagnosticContext context, IPAddress target, int packetSize, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        DiagnosticReporter.Report(context, progress, Strings.Diag.TracingRoute);
        var hops = new List<HopInfo>();
        var reachedTarget = false;
        for (var ttl = 1; ttl <= DiagnosticConstants.MaxHops && !reachedTarget; ttl++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            reachedTarget = await ProcessTtlAsync(context, target, ttl, packetSize, hops, progress, stats, cancellationToken);
        }
        ReportLimit(context, reachedTarget, progress);
        return hops;
    }

    private static async Task<bool> ProcessTtlAsync(DiagnosticContext context, IPAddress target, int ttl, int packetSize, List<HopInfo> hops, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        var result = await ProbeTtlAsync(context, target, ttl, packetSize, progress, cancellationToken);
        if (result.Address is null) { ReportNoResponse(context, ttl, progress); return false; }
        var hop = CreateHop(ttl, result.Address, target);
        hops.Add(hop);
        DiagnosticReporter.ReportStats(stats, hops);
        ReportDiscovered(context, target, hop, ttl, progress);
        return IsReached(result, hop, target);
    }

    private static async Task<ProbeResult> ProbeTtlAsync(DiagnosticContext context, IPAddress target, int ttl, int packetSize, IProgress<string> progress, CancellationToken cancellationToken)
    {
        ProbeResult? last = null;
        IPAddress? address = null;
        for (var attempt = 1; attempt <= DiagnosticConstants.DiscoveryAttempts; attempt++)
        {
            last = await ProbeExecutor.SendAsync(context, CreateRequest(target, ttl, packetSize, attempt), progress, cancellationToken);
            if (last.Address is not null) address = last.Address;
            if (IsDiscoveryResponse(last.Outcome)) break;
        }
        return CloneResult(last, address, packetSize, ttl);
    }

    private static ProbeRequest CreateRequest(IPAddress target, int ttl, int packetSize, int attempt) =>
        new(target, ttl, packetSize, DiagnosticConstants.ProbeTimeoutMs, TextFormatter.Fmt(Strings.Diag.TraceContextFormat, ttl, attempt, DiagnosticConstants.DiscoveryAttempts));

    private static bool IsDiscoveryResponse(ProbeOutcome outcome) =>
        outcome is ProbeOutcome.TtlExpired or ProbeOutcome.Success or ProbeOutcome.DestinationUnreachable;

    private static HopInfo CreateHop(int ttl, IPAddress address, IPAddress target) => new()
    {
        Ttl = ttl,
        Address = address,
        Status = address.Equals(target) ? Strings.Status.Target : Strings.Status.Pending
    };

    private static bool IsReached(ProbeResult result, HopInfo hop, IPAddress target) =>
        result.Outcome == ProbeOutcome.Success || hop.Address.Equals(target);

    private static void ReportNoResponse(DiagnosticContext context, int ttl, IProgress<string> progress) =>
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.NoResponseHopFormat, ttl));

    private static void ReportDiscovered(DiagnosticContext context, IPAddress target, HopInfo hop, int ttl, IProgress<string> progress)
    {
        var marker = hop.Address.Equals(target) ? Strings.Diag.TargetMarker : string.Empty;
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.HopDiscoveredFormat, ttl, hop.Address, marker));
    }

    private static void ReportLimit(DiagnosticContext context, bool reachedTarget, IProgress<string> progress)
    {
        if (reachedTarget) return;
        DiagnosticReporter.Report(context, progress, Strings.Diag.TraceHopLimit, isWarn: true);
    }

    private static ProbeResult CloneResult(ProbeResult? last, IPAddress? address, int packetSize, int ttl)
    {
        return new ProbeResult(
            last?.Outcome ?? ProbeOutcome.TimedOut,
            address,
            last?.RoundtripTimeMs ?? 0,
            last?.BufferSize ?? packetSize,
            ttl);
    }
}
