using TraceDiag.Models;

namespace TraceDiag.Console;

/// <summary>Keeps the latest snapshot per TTL until the final table is printed.</summary>
internal sealed class StatsCollector
{
    private readonly SortedDictionary<int, HopStatSnapshot> _hops = new();

    internal void Add(IReadOnlyList<HopStatSnapshot> snapshots)
    {
        foreach (var hop in snapshots) _hops[hop.Ttl] = hop;
    }

    internal void Add(HopStatSnapshot hop) => _hops[hop.Ttl] = hop;

    internal IReadOnlyList<HopStatSnapshot> Snapshot() => _hops.Values.ToArray();
}
