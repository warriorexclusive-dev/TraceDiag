using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Stamps route metadata onto stats and accumulates all routes in one run.</summary>
internal sealed class RouteStatsDecorator : IProgress<IReadOnlyList<HopStatSnapshot>>
{
    private readonly IProgress<IReadOnlyList<HopStatSnapshot>> _inner;
    private readonly SortedDictionary<string, HopStatSnapshot> _all = new(StringComparer.Ordinal);
    private RouteNode? _current;

    internal RouteStatsDecorator(IProgress<IReadOnlyList<HopStatSnapshot>> inner) => _inner = inner;

    internal void Use(RouteNode route) => _current = route;

    public void Report(IReadOnlyList<HopStatSnapshot> value)
    {
        foreach (var hop in value) _all[Key(hop)] = Stamp(hop);
        _inner.Report(All());
    }

    internal IReadOnlyList<HopStatSnapshot> All() => _all.Values.ToArray();

    internal IReadOnlyList<HopStatSnapshot> ForRoute(int uid) =>
        _all.Values.Where(hop => hop.RouteUid == uid).ToArray();

    private static string Key(HopStatSnapshot hop) => hop.RouteUid + ":" + hop.Ttl;

    private HopStatSnapshot Stamp(HopStatSnapshot hop) =>
        _current is null ? hop : hop with
        {
            RouteUid = _current.Uid,
            RouteTarget = _current.Target.Address.ToString(),
            RouteResolver = _current.Target.Resolver
        };
}
