using TraceDiag.Models;

namespace TraceDiag.Console;

/// <summary>Keeps the latest snapshot per route and TTL until the final table is printed.</summary>
internal sealed class StatsCollector
{
    private readonly SortedDictionary<string, HopStatSnapshot> _hops = new(StringComparer.Ordinal);

    internal void Add(IReadOnlyList<HopStatSnapshot> snapshots)
    {
        foreach (var hop in snapshots) Add(hop);
    }

    internal void Add(HopStatSnapshot hop) => _hops[Key(hop)] = hop;

    internal IReadOnlyList<HopStatSnapshot> Snapshot() => _hops.Values.ToArray();

    private static string Key(HopStatSnapshot hop) =>
        hop.RouteUid.ToString("D4") + ":" + hop.Ttl.ToString("D3");
}
