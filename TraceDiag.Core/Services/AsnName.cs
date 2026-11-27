using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace TraceDiag.Services;

/// <summary>Builds Team Cymru origin lookup names for IPv4 and IPv6 addresses.</summary>
internal static class AsnName
{
    internal static string For(IPAddress address) =>
        address.AddressFamily == AddressFamily.InterNetworkV6
            ? Nibbles(address) + ".origin6.asn.cymru.com"
            : string.Join(".", address.GetAddressBytes().Reverse()) + ".origin.asn.cymru.com";

    private static string Nibbles(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        var chars = new char[bytes.Length * 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            chars[i * 2] = Hex(bytes[i] >> 4);
            chars[i * 2 + 1] = Hex(bytes[i] & 0x0F);
        }
        return string.Join(".", chars.Reverse());
    }

    private static char Hex(int value) => value < 10 ? (char)('0' + value) : (char)('a' + value - 10);
}
