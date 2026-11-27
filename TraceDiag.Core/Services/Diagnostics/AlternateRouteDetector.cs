using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Optional precheck that discovers alternative routes before the main check.</summary>
internal sealed class AlternateRouteDetector : IAlternateRouteDetector
{
    private const int PacketStep = 32;

    public RouteDiscoveryResult CreateFallback(string host, IReadOnlyList<DnsAnswer> targets, RouteRegistry registry)
    {
        var hosts = targets.Select(target => FallbackHost(host, target, registry)).ToArray();
        return BuildResult(hosts, true, true, DateTimeOffset.UtcNow);
    }

    public async Task<RouteDiscoveryResult> DetectAsync(DiagnosticContext context, string host,
        IReadOnlyList<DnsAnswer> targets, RoutePrecheckSettings settings,
        RouteRegistry registry, CancellationToken cancellationToken)
    {
        var started = DateTimeOffset.UtcNow;
        if (!settings.Enabled) return BuildResult(FallbackHosts(host, targets, registry), true, true, started);
        using var runCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        runCts.CancelAfter(settings.DeadlineMs);
        var (hosts, partial) = await BuildHostsAsync(context, host, targets, settings, registry, runCts);
        return BuildResult(hosts, partial, partial, started);
    }

    private async Task<(List<HostRouteNode> Hosts, bool Partial)> BuildHostsAsync(
        DiagnosticContext context, string host, IReadOnlyList<DnsAnswer> targets,
        RoutePrecheckSettings settings, RouteRegistry registry, CancellationTokenSource runCts)
    {
        var hosts = new List<HostRouteNode>();
        var partial = false;
        foreach (var target in targets)
        {
            var expired = runCts.IsCancellationRequested;
            var node = expired
                ? FallbackHost(host, target, registry)
                : await SafeHostAsync(context, host, target, settings, registry, runCts.Token);
            if (node.Routes.Count == 0 || expired) { AddFallbackRoute(node, registry); partial = true; }
            ComputeSharedPaths(node);
            hosts.Add(node);
        }
        return (hosts, partial);
    }

    private static async Task<HostRouteNode> SafeHostAsync(DiagnosticContext context, string host,
        DnsAnswer target, RoutePrecheckSettings settings, RouteRegistry registry, CancellationToken token)
    {
        try { return await BuildHostAsync(context, host, target, settings, registry, token); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception) { return new HostRouteNode { Target = target, Host = host }; }
    }

    private static async Task<HostRouteNode> BuildHostAsync(DiagnosticContext context, string host,
        DnsAnswer target, RoutePrecheckSettings settings, RouteRegistry registry, CancellationToken token)
    {
        var node = new HostRouteNode { Target = target, Host = host };
        using var gate = new SemaphoreSlim(settings.Concurrency);
        var slots = Slots(settings).Select(slot => ProbeAsync(context, target, slot, settings, gate, token));
        var replies = await Task.WhenAll(slots);
        foreach (var flow in Enumerable.Range(0, settings.FlowCount)) AddFlow(node, replies, flow, target, registry);
        return node;
    }

    private static IEnumerable<ProbeSlot> Slots(RoutePrecheckSettings settings)
    {
        for (var flow = 0; flow < settings.FlowCount; flow++)
            for (var ttl = 1; ttl <= settings.MaxHops; ttl++) yield return new ProbeSlot(flow, ttl);
    }

    private static async Task<RouteReply> ProbeAsync(DiagnosticContext context, DnsAnswer target,
        ProbeSlot slot, RoutePrecheckSettings settings, SemaphoreSlim gate, CancellationToken token)
    {
        await gate.WaitAsync(token);
        try
        {
            var size = DiagnosticConstants.DiscoveryPacketSize + slot.Flow * PacketStep;
            var result = await RouteProbe.SendAsync(context, target.Address, slot.Ttl, size,
                settings.ProbeTimeoutMs, slot.Flow % 2 == 0, token);
            return new RouteReply(slot.Flow, slot.Ttl, result);
        }
        finally { gate.Release(); }
    }

    private static void AddFlow(HostRouteNode node, IReadOnlyList<RouteReply> replies, int flow,
        DnsAnswer target, RouteRegistry registry)
    {
        var ordered = replies.Where(reply => reply.Flow == flow).OrderBy(reply => reply.Ttl).ToArray();
        var signatures = ordered.Select(reply => PathKey.Format(reply.Result, target.Address)).ToArray();
        if (signatures.All(signature => signature == Strings.Diag.RouteMiss)) return;
        var key = PathKey.Build(signatures);
        var route = AddRoute(node, key, flow, target, registry);
        route.Hops.AddRange(CreateHops(ordered, route, target));
    }

    private static RouteNode AddRoute(HostRouteNode node, string key, int flow,
        DnsAnswer target, RouteRegistry registry)
    {
        var uid = registry.GetOrCreate(node.Target.Address + Strings.Diag.RouteSeparator + key);
        var route = node.Routes.FirstOrDefault(item => item.Uid == uid);
        if (route is null) { route = CreateRoute(uid, key, target); node.Routes.Add(route); }
        if (!route.Flows.Contains(flow)) route.Flows.Add(flow);
        route.Samples.Add(new RouteSample(DateTimeOffset.UtcNow, flow, key));
        return route;
    }

    private static RouteNode CreateRoute(int uid, string key, DnsAnswer target) => new()
    {
        Uid = uid,
        Key = key,
        Target = target
    };

    private static IEnumerable<HopInfo> CreateHops(IReadOnlyList<RouteReply> replies, RouteNode route, DnsAnswer target)
    {
        foreach (var reply in replies)
        {
            var address = reply.Result.Address;
            if (address is null) continue;
            yield return new HopInfo
            {
                Ttl = reply.Ttl,
                Address = address,
                RouteUid = route.Uid,
                RouteTarget = target.Address.ToString(),
                RouteResolver = target.Resolver
            };
        }
    }

    private static void ComputeSharedPaths(HostRouteNode node)
    {
        var first = node.Routes.FirstOrDefault();
        if (first is null) return;
        foreach (var route in node.Routes.Skip(1)) route.SharedPathTtls.AddRange(CommonTtls(first, route));
    }

    private static IEnumerable<int> CommonTtls(RouteNode first, RouteNode other)
    {
        var pairs = first.Hops.Zip(other.Hops,
            (left, right) => new { left.Ttl, First = left.Address, Other = right.Address });
        foreach (var pair in pairs)
        {
            if (!pair.First.Equals(pair.Other)) break;
            yield return pair.Ttl;
        }
    }

    private static HostRouteNode[] FallbackHosts(string host, IReadOnlyList<DnsAnswer> targets, RouteRegistry registry) =>
        targets.Select(target => FallbackHost(host, target, registry)).ToArray();

    private static HostRouteNode FallbackHost(string host, DnsAnswer target, RouteRegistry registry)
    {
        var node = new HostRouteNode { Target = target, Host = host };
        AddFallbackRoute(node, registry);
        return node;
    }

    private static void AddFallbackRoute(HostRouteNode node, RouteRegistry registry) =>
        AddRoute(node, Strings.Diag.RouteMiss, 0, node.Target, registry);

    private static RouteDiscoveryResult BuildResult(IReadOnlyList<HostRouteNode> hosts, bool partial,
        bool fallback, DateTimeOffset started) => new()
    {
        RunId = Guid.NewGuid(),
        StartedAt = started,
        Hosts = hosts,
        Partial = partial,
        Fallback = fallback
    };

    private sealed record ProbeSlot(int Flow, int Ttl);

    private sealed record RouteReply(int Flow, int Ttl, ProbeResult Result);
}
