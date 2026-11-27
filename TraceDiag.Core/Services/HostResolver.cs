using System.Net;

namespace TraceDiag.Services;

internal static class HostResolver
{
    internal static async Task<IPAddress?> ResolveAsync(string host, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (IPAddress.TryParse(host, out var address)) return IsUsable(address) ? address : null;
        var addresses = await Compat.WithCancellation(Dns.GetHostAddressesAsync(host), cancellationToken);
        return PickAddress(addresses);
    }

    private static IPAddress? PickAddress(IPAddress[] addresses) =>
        addresses.FirstOrDefault(IsUsable);

    private static bool IsUsable(IPAddress address) =>
        NetworkAddressPolicy.IsProbeTarget(address);
}
