using System.Diagnostics;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class HopNameResolver
{
    internal static async Task ResolveAsync(DiagnosticContext context, IReadOnlyList<HopInfo> hops, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        DiagnosticReporter.Report(context, progress, Strings.Diag.ResolvingDns);
        foreach (var hop in hops)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!NetworkAddressPolicy.IsProbeTarget(hop.Address)) continue;
            var sw = Stopwatch.StartNew();
            await ResolveOneAsync(context, hop, hops, sw, progress, stats, cancellationToken);
        }
    }

    private static async Task ResolveOneAsync(DiagnosticContext context, HopInfo hop, IReadOnlyList<HopInfo> hops, Stopwatch sw, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        try { hop.HostName = await context.DnsResolver.ResolveHostNameAsync(hop.Address, cancellationToken); }
        catch (Exception ex) when (ex is not OperationCanceledException) { ReportDnsError(context, hop, ex, progress); return; }
        finally { DiagnosticReporter.ReportStats(stats, hops); }
        ReportSlowDns(context, hop, sw);
        ReportResolved(context, hop, progress);
    }

    private static void ReportDnsError(DiagnosticContext context, HopInfo hop, Exception ex, IProgress<string> progress)
    {
        context.Logger?.Error(TextFormatter.Fmt(Strings.Diag.DnsResolveFailedFormat, hop.Address), ex);
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.DnsErrorFormat, hop.Ttl, hop.Address, ex.Message), isWarn: true, ex: ex);
    }

    private static void ReportSlowDns(DiagnosticContext context, HopInfo hop, Stopwatch sw)
    {
        if (sw.ElapsedMilliseconds <= DiagnosticConstants.SlowDnsThresholdMs) return;
        context.Logger?.Warn(TextFormatter.Fmt(Strings.Diag.SlowDnsFormat, hop.Address, sw.ElapsedMilliseconds));
    }

    private static void ReportResolved(DiagnosticContext context, HopInfo hop, IProgress<string> progress)
    {
        var name = string.IsNullOrWhiteSpace(hop.HostName) ? Strings.Diag.NoPtrName : hop.HostName;
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.HopDnsFormat, hop.Ttl, hop.Address, name));
    }
}
