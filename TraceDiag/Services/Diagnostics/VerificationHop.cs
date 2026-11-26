using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class VerificationHop
{
    internal static HopInfo Create(HopInfo source) => new()
    {
        Ttl = source.Ttl,
        Address = source.Address,
        HostName = source.HostName,
        Status = source.Status,
        IsSkipped = source.IsSkipped
    };
}
