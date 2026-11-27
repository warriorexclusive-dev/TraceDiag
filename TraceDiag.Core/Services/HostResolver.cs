using System.Net;
using System.Net.Sockets;
using TraceDiag.Models;

namespace TraceDiag.Services;

internal static class HostResolver
{
    internal static Task<IPAddress?> ResolveAsync(string host, CancellationToken cancellationToken) =>
        ResolveAsync(host, IpFamily.Auto, cancellationToken);

    internal static async Task<IPAddress?> ResolveAsync(string host, IpFamily family, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (IPAddress.TryParse(host, out var address))
            return Matches(address, family) && IsUsable(address) ? address : null;
        var addresses = await Compat.WithCancellation(Dns.GetHostAddressesAsync(host), cancellationToken);
        return PickAddress(addresses, family);
    }

    internal static IPAddress? PickAddress(IPAddress[] addresses, IpFamily family)
    {
        foreach (var address in Ordered(addresses, family))
            if (IsUsable(address)) return address;
        return null;
    }

    private static IEnumerable<IPAddress> Ordered(IPAddress[] addresses, IpFamily family) =>
        family switch
        {
            IpFamily.IPv4 => addresses.Where(IsV4),
            IpFamily.IPv6 => addresses.Where(IsV6),
            _ => addresses.OrderBy(address => IsV6(address) ? 1 : 0)
        };

    private static bool Matches(IPAddress address, IpFamily family) =>
        family switch
        {
            IpFamily.IPv4 => IsV4(address),
            IpFamily.IPv6 => IsV6(address),
            _ => true
        };

    private static bool IsV4(IPAddress address) => address.AddressFamily == AddressFamily.InterNetwork;
    private static bool IsV6(IPAddress address) => address.AddressFamily == AddressFamily.InterNetworkV6;
    private static bool IsUsable(IPAddress address) => NetworkAddressPolicy.IsProbeTarget(address);
}
