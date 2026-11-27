using System.Net;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Tests.TestData;

internal sealed class FakeRipeOwnerService : IRipeOwnerService
{
    private readonly Func<IPAddress, string?> _resolve;

    internal FakeRipeOwnerService(Func<IPAddress, string?> resolve) => _resolve = resolve;

    internal List<IPAddress> Calls { get; } = new();

    public Task<string?> ResolveOwnerAsync(IPAddress address, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls.Add(address);
        return Task.FromResult(_resolve(address));
    }
}
