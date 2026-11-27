using System.Net;
using System.Net.Sockets;

namespace TraceDiag.Services;

/// <summary>Creates DNS sockets bound to the selected adapter when possible.</summary>
internal static class UdpSocketFactory
{
    internal static UdpClient Create(AddressFamily family, IPAddress? source)
    {
        if (source is null || source.AddressFamily != family) return new UdpClient(family);
        try { return new UdpClient(new IPEndPoint(source, 0)); }
        catch (SocketException) { return new UdpClient(family); }
    }
}
