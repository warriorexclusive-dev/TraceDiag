using System.Net;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Tests.TestData;

internal sealed class FakeDnsResolver : IDnsResolverService
{
    private readonly Func<IPAddress, string?> _resolve;

    internal FakeDnsResolver(Func<IPAddress, string?> resolve) => _resolve = resolve;

    internal List<IPAddress> Calls { get; } = new();

    public Task<string?> ResolveHostNameAsync(IPAddress address, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls.Add(address);
        return Task.FromResult(_resolve(address));
    }
}
