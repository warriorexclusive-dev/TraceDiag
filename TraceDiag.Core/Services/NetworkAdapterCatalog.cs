using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace TraceDiag.Services;

internal sealed record NetworkAdapterInfo(string Id, string Name, IPAddress? Source,
    IReadOnlyList<IPAddress> DnsServers);

/// <summary>Enumerates active network cards and their DNS/source addresses.</summary>
internal static class NetworkAdapterCatalog
{
    internal static IReadOnlyList<NetworkAdapterInfo> Get()
    {
        var result = new List<NetworkAdapterInfo>();
        try
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) Add(result, nic);
        }
        catch (NetworkInformationException) { }
        return result;
    }

    internal static IPAddress? Source(string? id) =>
        Find(id)?.Source;

    internal static IReadOnlyList<IPAddress> Dns(string? id) =>
        Find(id)?.DnsServers ?? DnsServers.Get();

    internal static NetworkAdapterInfo? Find(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        var adapters = Get();
        return adapters.FirstOrDefault(a => a.Id == id) ??
            adapters.FirstOrDefault(a => a.Name == id);
    }

    private static void Add(List<NetworkAdapterInfo> result, NetworkInterface nic)
    {
        if (nic.OperationalStatus != OperationalStatus.Up) return;
        var properties = nic.GetIPProperties();
        var source = properties.UnicastAddresses
            .Where(item => IsUsable(item.Address))
            .Select(item => item.Address)
            .FirstOrDefault();
        if (source is null) return;
        result.Add(new NetworkAdapterInfo(nic.Id, nic.Name, source,
            properties.DnsAddresses.Where(IsDns).ToArray()));
    }

    private static bool IsUsable(IPAddress address) =>
        address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6 &&
        !IPAddress.IsLoopback(address);

    private static bool IsDns(IPAddress address) =>
        address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6;
}
