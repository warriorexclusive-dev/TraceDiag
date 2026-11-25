using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Abstractions;

public interface IIcmpProbeClient
{
    Task<ProbeResult> SendAsync(IPAddress address, int ttl, int bufferSize, int timeoutMs, CancellationToken cancellationToken);
}
