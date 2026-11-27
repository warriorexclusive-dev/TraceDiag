using System.Net;
using System.Net.Sockets;

namespace TraceDiag.Services;

/// <summary>Checks whether the process can create privileged raw ICMP sockets.</summary>
internal static class RawSocketCapability
{
    internal static bool CanUse()
    {
        try
        {
            using var socket = Create(AddressFamily.InterNetwork);
            return true;
        }
        catch (SocketException) { return false; }
        catch (PlatformNotSupportedException) { return false; }
    }

    internal static Socket Create(AddressFamily family) =>
        new(family, SocketType.Raw, family == AddressFamily.InterNetworkV6
            ? ProtocolType.IcmpV6 : ProtocolType.Icmp);
}
