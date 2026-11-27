using System.Net;
using System.Net.Http;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services;

public sealed class RipeOwnerService : IRipeOwnerService
{
    private readonly RipeOwnerClient _client = new();

    public async Task<string?> ResolveOwnerAsync(IPAddress address, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try { return await _client.ResolveAsync(address, cancellationToken); }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or IOException) { return null; }
    }
}
