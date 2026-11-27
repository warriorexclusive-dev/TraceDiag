using System.Net;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Sets a hop diagnosis when ICMP replies come from changing source addresses.</summary>
internal sealed class RouteStabilityController : IRouteStabilityController
{
    public void Apply(HopInfo hop)
    {
        if (!CanApply(hop.Diagnosis)) return;
        var addresses = RouteSequence.Build(hop.Address, hop.Responders);
        var diagnosis = RouteSequence.Detect(addresses);
        if (diagnosis is not null) hop.Diagnosis = diagnosis;
    }

    private static bool CanApply(string diagnosis) =>
        diagnosis == Strings.Diagnosis.None ||
        diagnosis == Strings.Diagnosis.RouteChange ||
        diagnosis == Strings.Diagnosis.EcmpBalancing;
}

internal static class RouteSequence
{
    internal static IReadOnlyList<IPAddress> Build(IPAddress baseline, IReadOnlyList<IPAddress> responders)
    {
        var result = new List<IPAddress> { baseline };
        foreach (var address in responders)
            if (!address.Equals(result[result.Count - 1])) result.Add(address);
        return result;
    }

    internal static string? Detect(IReadOnlyList<IPAddress> addresses) =>
        addresses.Count < 2 ? null : Repeats(addresses)
            ? Strings.Diagnosis.EcmpBalancing
            : Strings.Diagnosis.RouteChange;

    private static bool Repeats(IReadOnlyList<IPAddress> addresses)
    {
        var seen = new HashSet<IPAddress>();
        foreach (var address in addresses)
            if (!seen.Add(address)) return true;
        return false;
    }
}
