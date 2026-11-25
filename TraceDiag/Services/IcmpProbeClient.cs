using System.Net;
using System.Net.NetworkInformation;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services;

public sealed class IcmpProbeClient : IIcmpProbeClient
{
    public async Task<ProbeResult> SendAsync(IPAddress address, int ttl, int bufferSize, int timeoutMs, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var buffer = IcmpPacketFactory.CreatePayload(bufferSize);
        using var ping = new Ping();
        try
        {
            var reply = await ping.SendPingAsync(address, timeoutMs, buffer, IcmpPacketFactory.CreateOptions(ttl));
            return IcmpReplyMapper.Map(reply, buffer.Length, ttl);
        }
        catch (PingException) { return IcmpReplyMapper.Failed(buffer.Length, ttl); }
        catch (OperationCanceledException) { throw; }
    }
}
