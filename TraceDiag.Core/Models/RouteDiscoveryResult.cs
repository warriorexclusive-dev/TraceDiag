namespace TraceDiag.Models;

/// <summary>Frozen route tree produced before the main diagnostic check.</summary>
public sealed class RouteDiscoveryResult
{
    public required Guid RunId { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
    public required IReadOnlyList<HostRouteNode> Hosts { get; init; }
    public bool Partial { get; init; }
    public bool Fallback { get; init; }
    public bool Frozen { get; init; } = true;

    public int RouteCount => Hosts.Sum(host => host.Routes.Count);

    public IEnumerable<(DnsAnswer Target, RouteNode Route)> AllRoutes()
    {
        foreach (var host in Hosts)
            foreach (var route in host.Routes) yield return (host.Target, route);
    }
}
