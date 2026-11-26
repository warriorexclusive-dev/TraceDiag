using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal sealed class DiscoveryState
{
    private readonly SortedDictionary<int, HopInfo> _hops = new();

    internal bool Reached { get; private set; }
    internal IReadOnlyList<HopInfo> Hops => _hops.Values.ToArray();
    internal IReadOnlySet<int> Skipped =>
        _hops.Where(pair => IsSkipped(pair.Value)).Select(pair => pair.Key).ToHashSet();

    internal bool ShouldProbe(int ttl) =>
        !_hops.TryGetValue(ttl, out var hop) || IsSkipped(hop);

    internal bool MarkNoResponse(int ttl)
    {
        if (_hops.ContainsKey(ttl)) return false;
        _hops[ttl] = CreateSkippedHop(ttl);
        return true;
    }

    internal bool MarkResolved(int ttl, HopInfo hop)
    {
        if (_hops.TryGetValue(ttl, out var existing) && IsResolved(existing) && existing.Address.Equals(hop.Address)) return false;
        _hops[ttl] = hop;
        return true;
    }

    internal void MarkReached() => Reached = true;

    private static bool IsSkipped(HopInfo hop) =>
        hop.IsSkipped || !NetworkAddressPolicy.IsProbeTarget(hop.Address);

    private static bool IsResolved(HopInfo hop) => !IsSkipped(hop);

    private static HopInfo CreateSkippedHop(int ttl) => new()
    {
        Ttl = ttl,
        Address = IPAddress.Any,
        HostName = Strings.Ui.NoAnswer,
        Status = Strings.Status.NoAnswer,
        IsSkipped = true,
        Diagnosis = Strings.Diagnosis.IcmpDisabled
    };
}
