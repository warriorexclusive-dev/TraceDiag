using System.Net;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Tests.TestData;

internal sealed class FakeDnsResolver : IDnsResolverService
{
    private readonly Func<IPAddress, string?> _resolve;
    private readonly Func<IPAddress, string?> _resolveAsn;

    internal FakeDnsResolver(Func<IPAddress, string?> resolve, Func<IPAddress, string?>? resolveAsn = null)
    {
        _resolve = resolve;
        _resolveAsn = resolveAsn ?? (_ => null);
    }

    internal List<IPAddress> Calls { get; } = new();

    public Task<string?> ResolveHostNameAsync(IPAddress address, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls.Add(address);
        return Task.FromResult(_resolve(address));
    }

    public Task<string?> ResolveAsnAsync(IPAddress address, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_resolveAsn(address));
    }
}
