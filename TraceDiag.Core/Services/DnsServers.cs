using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace TraceDiag.Services;

/// <summary>Returns the system DNS servers, best-effort.</summary>
internal static class DnsServers
{
    internal static IReadOnlyList<IPAddress> Get()
    {
        return Collect();
    }

    private static List<IPAddress> Collect()
    {
        var servers = new List<IPAddress>();
        try
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()) AddFrom(servers, nic);
        }
        catch (NetworkInformationException) { }
        return servers;
    }

    private static void AddFrom(List<IPAddress> servers, NetworkInterface nic)
    {
        if (nic.OperationalStatus != OperationalStatus.Up) return;
        foreach (var address in nic.GetIPProperties().DnsAddresses)
            if (address.AddressFamily == AddressFamily.InterNetwork && !servers.Contains(address))
                servers.Add(address);
    }
}
