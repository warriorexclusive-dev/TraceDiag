using System.Net;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services;

public sealed class DnsResolverService : IDnsResolverService
{
    public async Task<string?> ResolveHostNameAsync(IPAddress address, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await DnsLookup.ResolveAsync(address);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (DnsLookup.IsExpectedFailure(ex)) { return null; }
    }

    public async Task<string?> ResolveAsnAsync(IPAddress address, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await AsnLookup.ResolveAsync(address, cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (DnsLookup.IsExpectedFailure(ex)) { return null; }
    }
}
