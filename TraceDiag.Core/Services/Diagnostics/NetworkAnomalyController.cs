using System.Net;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Adds advanced diagnoses on top of basic per-hop checks.</summary>
internal sealed class NetworkAnomalyController : INetworkAnomalyController
{
    public void Apply(HopInfo hop, IReadOnlyList<HopSizeResult> results,
        bool dontFragment, IReadOnlyCollection<int> blockedTtls)
    {
        ApplyMtuBlackHole(hop, results, dontFragment);
        ApplyAsymmetry(hop, results);
        ApplyIcmpBlock(hop, blockedTtls);
        ApplyFiltering(hop);
    }

    private static void ApplyMtuBlackHole(HopInfo hop, IReadOnlyList<HopSizeResult> results, bool dontFragment)
    {
        if (!dontFragment) return;
        var passing = results.Where(result => result.Received > 0).Select(result => result.Size).ToArray();
        if (passing.Length == 0) return;
        var limit = passing.Max();
        if (results.Any(result => result.Size > limit && result.Sent > 0 && result.Received == 0))
            hop.AddDiagnosis(Strings.Diagnosis.MtuBlackHole);
    }

    private static void ApplyAsymmetry(HopInfo hop, IReadOnlyList<HopSizeResult> results)
    {
        var sources = results.Where(result => result.Responders is { Count: > 0 })
            .Select(result => ResponderKey(result.Responders!))
            .Distinct()
            .ToArray();
        if (sources.Length > 1) hop.AddDiagnosis(Strings.Diagnosis.RouteAsymmetry);
    }

    private static string ResponderKey(IReadOnlyList<IPAddress> addresses) =>
        string.Join(",", addresses.Select(address => address.ToString()).OrderBy(value => value));

    private static void ApplyIcmpBlock(HopInfo hop, IReadOnlyCollection<int> blockedTtls)
    {
        if (!blockedTtls.Contains(hop.Ttl) || !hop.IsSkipped) return;
        hop.RemoveDiagnosis(Strings.Diagnosis.IcmpDisabled);
        hop.AddDiagnosis(Strings.Diagnosis.IcmpBlocked);
    }

    private static void ApplyFiltering(HopInfo hop)
    {
        if (hop.ObservedOutcomes.Contains(ProbeOutcome.PortUnreachable) ||
            hop.ObservedOutcomes.Contains(ProbeOutcome.Prohibited))
            hop.AddDiagnosis(Strings.Diagnosis.PortFiltered);
    }
}
