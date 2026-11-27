using System.Net;

namespace TraceDiag.Services;

internal static class NetworkAddressPolicy
{
    internal static bool IsProbeTarget(IPAddress address) =>
        !address.Equals(IPAddress.Any) &&
        !address.Equals(IPAddress.IPv6Any) &&
        !address.Equals(IPAddress.IPv6None);

    /// <summary>Private and link-local destinations should not trigger external lookups.</summary>
    internal static bool IsPrivateTarget(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            return (bytes[0] & 0xfe) == 0xfc || bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0x80;
        return bytes[0] == 10 || bytes[0] == 127 ||
            bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31 ||
            bytes[0] == 192 && bytes[1] == 168 ||
            bytes[0] == 169 && bytes[1] == 254 ||
            bytes[0] == 0;
    }
}
