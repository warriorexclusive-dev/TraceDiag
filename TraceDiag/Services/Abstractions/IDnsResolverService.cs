using System.Net;

namespace TraceDiag.Services.Abstractions;

public interface IDnsResolverService
{
    Task<string?> ResolveHostNameAsync(IPAddress address, CancellationToken cancellationToken);
}
