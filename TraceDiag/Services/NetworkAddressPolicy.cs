using System.Net;

namespace TraceDiag.Services;

internal static class NetworkAddressPolicy
{
    internal static bool IsProbeTarget(IPAddress address) =>
        !address.Equals(IPAddress.Any) &&
        !address.Equals(IPAddress.IPv6Any) &&
        !address.Equals(IPAddress.IPv6None);
}
