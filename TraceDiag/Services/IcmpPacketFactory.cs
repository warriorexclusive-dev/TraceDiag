using System.Net.NetworkInformation;

namespace TraceDiag.Services;

internal static class IcmpPacketFactory
{
    internal static byte[] CreatePayload(int bufferSize)
    {
        var size = Math.Max(0, bufferSize);
        var buffer = new byte[size];
        for (var i = 0; i < buffer.Length; i++) buffer[i] = (byte)(i % 256);
        return buffer;
    }

    internal static PingOptions CreateOptions(int ttl) => new(ttl, dontFragment: false);
}
