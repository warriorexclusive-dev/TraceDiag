using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Adds host/route/hop signatures to log lines and stamps stats with route data.</summary>
internal sealed class RouteProgress(
    IProgress<string> text,
    RouteStatsDecorator stats) : IProgress<string>, IProgress<IReadOnlyList<HopStatSnapshot>>
{
    private RouteNode? _route;
    private int _hop;

    internal void Use(RouteNode route)
    {
        _route = route;
        _hop = route.Hops.Count == 0 ? 0 : route.Hops[route.Hops.Count - 1].Ttl;
        stats.Use(route);
    }

    public void Report(string value) =>
        text.Report(_route is null ? value : Signature() + " " + value);

    public void Report(IReadOnlyList<HopStatSnapshot> value)
    {
        if (value.Count > 0) _hop = value[value.Count - 1].Ttl;
        stats.Report(value);
    }

    private string Signature() => TextFormatter.Fmt(
        Strings.Diag.RouteSignatureFormat, _route!.Target.Address, _route.Uid, _hop);
}
