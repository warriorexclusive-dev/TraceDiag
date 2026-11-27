using System.Net;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Tests.TestData;

internal sealed class GatedDnsResolver : IDnsResolverService
{
    private readonly TaskCompletionSource<bool> _gate = new();
    private int _started;

    internal GatedDnsResolver(CancellationToken token) =>
        token.Register(() => _gate.TrySetCanceled(token));

    internal int Started => Volatile.Read(ref _started);

    public Task<string?> ResolveHostNameAsync(IPAddress address, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(null);

    public async Task<string?> ResolveAsnAsync(IPAddress address, CancellationToken cancellationToken)
    {
        if (Interlocked.Increment(ref _started) >= 8) _gate.TrySetResult(true);
        await _gate.Task;
        return "ASN";
    }
}
