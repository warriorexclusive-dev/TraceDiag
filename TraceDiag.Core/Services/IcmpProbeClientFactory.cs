using System.Net;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services;

/// <summary>Selects the privileged raw client only when an adapter and rights are available.</summary>
internal static class IcmpProbeClientFactory
{
    internal static IIcmpProbeClient Create(string? adapterId)
    {
        var source = NetworkAdapterCatalog.Source(adapterId);
        return source is not null && RawSocketCapability.CanUse()
            ? new RawIcmpProbeClient(source)
            : new IcmpProbeClient();
    }

    internal static IPAddress? Source(string? adapterId) => NetworkAdapterCatalog.Source(adapterId);
}
