using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services;

/// <summary>ICMP probes bound to an explicitly selected network adapter.</summary>
public sealed class RawIcmpProbeClient(IPAddress? source) : IIcmpProbeClient
{
    private static int _sequence;

    public async Task<ProbeResult> SendAsync(IPAddress address, int ttl, int bufferSize, int timeoutMs,
        CancellationToken cancellationToken, bool dontFragment = false)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var payload = IcmpPacketFactory.CreatePayload(bufferSize);
        try
        {
            return await Task.Run(
                () => Echo(address, ttl, payload, timeoutMs, cancellationToken), cancellationToken);
        }
        catch (SocketException) { return IcmpReplyMapper.Failed(bufferSize, ttl); }
        catch (OperationCanceledException) { throw; }
    }

    private ProbeResult Echo(IPAddress address, int ttl, byte[] payload, int timeoutMs, CancellationToken token)
    {
        var ipv6 = address.AddressFamily == AddressFamily.InterNetworkV6;
        using var socket = Open(ipv6, ttl);
        var packet = RawIcmpPacket.Create(ipv6, payload,
            Guid.NewGuid().GetHashCode() & 0xffff, Interlocked.Increment(ref _sequence) & 0xffff);
        socket.SendTo(packet, new IPEndPoint(address, 0));
        var reply = Receive(socket, ipv6, timeoutMs, token);
        if (reply is null) return IcmpReplyMapper.Failed(payload.Length, ttl);
        var type = reply.Buffer[RawIcmpPacket.HeaderOffset(reply.Buffer)];
        return RawIcmpReplyMapper.Map(type, reply.Sender, reply.RoundtripTimeMs,
            payload.Length, ttl, ipv6);
    }

    private Socket Open(bool ipv6, int ttl)
    {
        var socket = RawSocketCapability.Create(ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
        if (source is not null && source.AddressFamily == socket.AddressFamily)
            socket.Bind(new IPEndPoint(source, 0));
        socket.SetSocketOption(ipv6 ? SocketOptionLevel.IPv6 : SocketOptionLevel.IP,
            ipv6 ? SocketOptionName.HopLimit : SocketOptionName.IpTimeToLive, ttl);
        return socket;
    }

    private static IcmpRawReply? Receive(Socket socket, bool ipv6, int timeoutMs, CancellationToken token)
    {
        var buffer = new byte[65535];
        EndPoint remote = new IPEndPoint(ipv6 ? IPAddress.IPv6Any : IPAddress.Any, 0);
        var watch = Stopwatch.StartNew();
        while (!token.IsCancellationRequested && watch.ElapsedMilliseconds < timeoutMs)
        {
            if (!socket.Poll(20000, SelectMode.SelectRead)) continue;
            var count = socket.ReceiveFrom(buffer, ref remote);
            return new IcmpRawReply(buffer, count, ((IPEndPoint)remote).Address, watch.ElapsedMilliseconds);
        }
        return null;
    }

    private sealed record IcmpRawReply(byte[] Buffer, int Count, IPAddress Sender, long RoundtripTimeMs);
}
