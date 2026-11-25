using System.Net;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Tests.TestData;

internal sealed record ProbeCall(IPAddress Address, int Ttl, int BufferSize);

internal sealed class FakeProbeClient : IIcmpProbeClient
{
    private readonly Func<ProbeCall, ProbeResult> _reply;

    internal FakeProbeClient(Func<ProbeCall, ProbeResult> reply) => _reply = reply;

    internal List<ProbeCall> Calls { get; } = new();

    public Task<ProbeResult> SendAsync(IPAddress address, int ttl, int bufferSize, int timeoutMs, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var call = new ProbeCall(address, ttl, bufferSize);
        Calls.Add(call);
        return Task.FromResult(_reply(call));
    }
}
