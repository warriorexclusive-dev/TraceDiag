namespace TraceDiag.Models;

/// <summary>One observed route for a DNS answer.</summary>
public sealed class RouteNode
{
    public required int Uid { get; init; }
    public required string Key { get; init; }
    public required DnsAnswer Target { get; init; }
    public List<HopInfo> Hops { get; } = new();
    public List<int> Flows { get; } = new();
    public List<RouteSample> Samples { get; } = new();
    public List<string> Deviations { get; } = new();
    public List<int> SharedPathTtls { get; } = new();
    public int SampleCount => Samples.Count;
}

/// <summary>One observation of a route, used for stability checks.</summary>
public sealed record RouteSample(DateTimeOffset At, int Flow, string PathKey);

/// <summary>A DNS answer with all routes observed for it.</summary>
public sealed class HostRouteNode
{
    public required DnsAnswer Target { get; init; }
    public string Host { get; init; } = string.Empty;
    public List<RouteNode> Routes { get; } = new();
}
