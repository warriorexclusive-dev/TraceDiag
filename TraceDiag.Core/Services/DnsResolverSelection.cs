using System.Net;

namespace TraceDiag.Services;

/// <summary>System DNS is always used; alternative resolvers are opt-in per run.</summary>
internal static class DnsResolverSelection
{
    internal static IReadOnlyList<IPAddress> Get(bool alternativeDns) =>
        alternativeDns ? Merge(DnsServers.Get(), DnsServerConfig.Load()) : DnsServers.Get();

    internal static IReadOnlyList<IPAddress> Merge(params IReadOnlyList<IPAddress>[] groups)
    {
        var result = new List<IPAddress>();
        foreach (var server in groups.SelectMany(group => group))
            if (!result.Contains(server)) result.Add(server);
        return result;
    }
}
