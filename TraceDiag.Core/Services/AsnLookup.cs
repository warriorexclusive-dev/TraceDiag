using System.Net;
using System.Net.Sockets;

namespace TraceDiag.Services;

/// <summary>Resolves an IP address to its ASN using Team Cymru DNS TXT records.</summary>
internal static class AsnLookup
{
    private const int DnsPort = 53;
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    internal static async Task<string?> ResolveAsync(IPAddress address, CancellationToken cancellationToken,
        string? adapterId = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var query = DnsQuery.AsText(AsnName.For(address));
        foreach (var server in DnsServers.Get(adapterId))
        {
            var answer = await QueryAsync(server, query, adapterId, cancellationToken);
            if (answer is not null) return answer;
        }
        return null;
    }

    private static async Task<string?> QueryAsync(IPAddress server, byte[] query, string? adapterId, CancellationToken token)
    {
        try
        {
            using var udp = UdpSocketFactory.Create(server.AddressFamily,
                NetworkAdapterCatalog.Source(adapterId));
            await udp.SendAsync(query, query.Length, new IPEndPoint(server, DnsPort));
            return await ReadAsync(udp, query, token);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (DnsLookup.IsExpectedFailure(ex) || ex is SocketException) { return null; }
    }

    private static async Task<string?> ReadAsync(UdpClient udp, byte[] query, CancellationToken token)
    {
        var receive = udp.ReceiveAsync();
        var done = await Task.WhenAny(receive, Task.Delay(Timeout, token));
        if (done != receive) { token.ThrowIfCancellationRequested(); return null; }
        var response = await receive;
        return DnsTextAnswer.Parse(response.Buffer, query);
    }
}
