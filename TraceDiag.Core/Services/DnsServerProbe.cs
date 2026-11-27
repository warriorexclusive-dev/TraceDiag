using System.Net;
using System.Net.Sockets;

namespace TraceDiag.Services;

/// <summary>Verifies that a resolver actually answers a DNS query.</summary>
internal static class DnsServerProbe
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    internal static async Task<bool> RespondsAsync(IPAddress server, CancellationToken token)
    {
        try
        {
            using var udp = UdpSocketFactory.Create(server.AddressFamily, null);
            var query = DnsAddressQuery.Create(Strings.App.DnsProbeName, false);
            await udp.SendAsync(query, query.Length, new IPEndPoint(server, 53));
            var receive = udp.ReceiveAsync();
            var done = await Task.WhenAny(receive, Task.Delay(Timeout, token));
            if (done != receive) { token.ThrowIfCancellationRequested(); return false; }
            var response = await receive;
            return response.Buffer.Length > 2 && (response.Buffer[2] & 0x80) != 0;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception) { return false; }
    }
}
