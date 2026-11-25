using System.Net;

namespace TraceDiag.Services;

internal static class DnsLookup
{
    internal static async Task<string?> ResolveAsync(IPAddress address)
    {
        var entry = await Dns.GetHostEntryAsync(address);
        return string.IsNullOrWhiteSpace(entry.HostName) ? null : entry.HostName;
    }

    internal static bool IsExpectedFailure(Exception ex) =>
        ex is System.Net.Sockets.SocketException or ArgumentException;
}
