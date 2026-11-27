using System.Net;

namespace TraceDiag.Services.Abstractions;

public interface IRipeOwnerService
{
    Task<string?> ResolveOwnerAsync(IPAddress address, CancellationToken cancellationToken);
}
