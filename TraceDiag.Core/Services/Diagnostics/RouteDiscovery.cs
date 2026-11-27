using System.Net;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Shared route discovery used by finite runs and monitoring.</summary>
internal static class RouteDiscovery
{
    internal static async Task<HostDiscoveryResult> DiscoverAsync(
        DiagnosticContext context,
        IPAddress target,
        int lossDelayMs,
        bool dontFragment,
        IProbeJobFactory factory,
        IProbeJobRunner runner,
        IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken)
    {
        var settings = DiscoverySettings.CreateDefault(lossDelayMs, dontFragment);
        var result = await HostDiscovery.DiscoverAsync(context, target, settings, factory, runner, progress, stats, cancellationToken);
        if (result.Hops.Count == 0) return ReportNoRoute(context, progress, result);
        await HopNameResolver.ResolveAsync(context, result.Hops, progress, stats, cancellationToken);
        DiagnosticReporter.ReportStats(stats, result.Hops);
        DiagnosticReporter.Separator(context, progress);
        return result;
    }

    private static HostDiscoveryResult ReportNoRoute(DiagnosticContext context, IProgress<string> progress, HostDiscoveryResult result)
    {
        DiagnosticReporter.Report(context, progress, Strings.Diag.NoRouteHops, isWarn: true);
        return result;
    }
}
