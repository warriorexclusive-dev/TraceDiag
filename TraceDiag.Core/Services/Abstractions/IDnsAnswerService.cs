using TraceDiag.Models;

namespace TraceDiag.Services.Abstractions;

public interface IDnsAnswerService
{
    Task<IReadOnlyList<DnsAnswer>> ResolveAsync(string host, bool alternativeDns, CancellationToken cancellationToken);
}
