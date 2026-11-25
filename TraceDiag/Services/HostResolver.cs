using System.Net;
using System.Net.Sockets;

namespace TraceDiag.Services;

internal static class HostResolver
{
    internal static async Task<IPAddress?> ResolveAsync(string host, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (IPAddress.TryParse(host, out var address)) return address;
        var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
        return PickAddress(addresses);
    }

    private static IPAddress? PickAddress(IPAddress[] addresses)
    {
        if (addresses.Length == 0) return null;
        return addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
            ?? addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6)
            ?? addresses[0];
    }
}
