using System.Globalization;
using System.Net;
using System.Net.Sockets;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Monitoring;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Runs the existing algorithms for every frozen route in one diagnostic run.</summary>
internal sealed class RouteRunCoordinator(
    IFiniteCheckController finite,
    IMonitorController monitor,
    IAlternateRouteDetector detector)
{
    public Task RunFiniteAsync(DiagnosticContext context, DiagnosticInput input,
        IProgress<string> text, IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken token)
    {
        var settings = RoutePrecheckSettings.Create(input.RoutePrecheck, input.DeepRoutes,
            input.ProbeTimeoutMs, input.AlternativeDns && context.ExternalSourcesEnabled);
        return RunAsync(context, input.RemoteAddress, Host(input.RemoteHostLabel), settings, text, stats, token,
            (route, progress) => finite.RunAsync(context,
                WithRoute(input, route), progress, progress, token));
    }

    public Task RunMonitorAsync(DiagnosticContext context, MonitoringInput input,
        IProgress<string> text, IProgress<HopStatSnapshot> stats,
        CancellationToken token)
    {
        var settings = RoutePrecheckSettings.Create(input.RoutePrecheck, input.DeepRoutes,
            input.ProbeTimeoutMs, input.AlternativeDns && context.ExternalSourcesEnabled);
        return RunAsync(context, input.RemoteAddress, Host(input.RemoteHostLabel), settings, text,
            new BatchStatsAdapter(stats), token,
            (route, progress) => monitor.RunAsync(context,
                WithRoute(input, route), progress, new SingleHopAdapter(progress), token));
    }

    private async Task RunAsync(DiagnosticContext context, IPAddress direct, string host,
        RoutePrecheckSettings settings, IProgress<string> text,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken token,
        Func<RouteNode, RouteProgress, Task> runRoute)
    {
        var tree = await PrepareAsync(context, direct, host, settings, text, token);
        var decorated = new RouteStatsDecorator(stats);
        var progress = new RouteProgress(text, decorated);
        foreach (var pair in tree.AllRoutes())
        {
            token.ThrowIfCancellationRequested();
            progress.Use(pair.Route);
            await runRoute(pair.Route, progress);
            ReportDeviations(context, pair.Route, decorated.ForRoute(pair.Route.Uid), text);
        }
    }

    private async Task<RouteDiscoveryResult> PrepareAsync(DiagnosticContext context, IPAddress direct,
        string host, RoutePrecheckSettings settings, IProgress<string> text, CancellationToken token)
    {
        var registry = new RouteRegistry();
        var targets = await ResolveTargetsAsync(context, host, direct, settings, token);
        var tree = settings.Enabled
            ? await detector.DetectAsync(context, host, targets, settings, registry, text, token)
            : detector.CreateFallback(host, targets, registry);
        DiagnosticReporter.ReportRouteMeta(context, text, tree, settings, host);
        return tree;
    }

    private static async Task<IReadOnlyList<DnsAnswer>> ResolveTargetsAsync(DiagnosticContext context,
        string host, IPAddress direct, RoutePrecheckSettings settings, CancellationToken token)
    {
        var fallback = Direct(host, direct);
        if (!settings.Enabled) return [fallback];
        try
        {
            var service = context.DnsAnswers ?? new DnsAnswerService(context.AdapterId, context.DnsFilePath);
            var answers = await service.ResolveAsync(host, settings.AlternativeDns, token);
            return answers.Count == 0 ? [fallback] : answers;
        }
        catch (Exception) { return [fallback]; }
    }

    private static DnsAnswer Direct(string host, IPAddress fallback)
    {
        var address = IPAddress.TryParse(host, out var parsed) ? parsed : fallback;
        return new DnsAnswer(address, Strings.Diag.NoDns,
            address.AddressFamily == AddressFamily.InterNetworkV6);
    }

    private static string Host(string label) => label.Split(' ')[0];

    private static DiagnosticInput WithRoute(DiagnosticInput input, RouteNode route) =>
        input with { RemoteAddress = route.Target.Address, RemoteHostLabel = Label(input.RemoteHostLabel, route) };

    private static MonitoringInput WithRoute(MonitoringInput input, RouteNode route) =>
        input with { RemoteAddress = route.Target.Address, RemoteHostLabel = Label(input.RemoteHostLabel, route) };

    private static string Label(string label, RouteNode route) =>
        Host(label).Equals(route.Target.Address.ToString(), StringComparison.OrdinalIgnoreCase)
            ? Host(label)
            : TextFormatter.Fmt(Strings.Ui.HostLabelFormat, Host(label), route.Target.Address);

    private static void ReportDeviations(DiagnosticContext context, RouteNode route,
        IReadOnlyList<HopStatSnapshot> stats, IProgress<string> progress)
    {
        foreach (var hop in stats)
        {
            var expected = route.Hops.FirstOrDefault(item => item.Ttl == hop.Ttl);
            if (expected is null || expected.Address.Equals(hop.Address)) continue;
            route.Deviations.Add(hop.Ttl.ToString(CultureInfo.InvariantCulture));
            DiagnosticReporter.ReportFormatWarn(context, progress, Strings.Diag.RouteDeviationFormat,
                route.Target.Address, route.Uid, hop.Ttl, hop.Address);
        }
    }
}
