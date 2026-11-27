using System.Net;
using System.Net.Sockets;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services;

/// <summary>Collects A/AAAA answers from system DNS and optional alternative resolvers.</summary>
public sealed class DnsAnswerService(string? adapterId = null, string? dnsFilePath = null) : IDnsAnswerService
{
    private const int DnsPort = 53;
    private const int TimeoutMs = 3000;
    private const int MaxAnswers = 8;

    public async Task<IReadOnlyList<DnsAnswer>> ResolveAsync(string host, bool alternativeDns,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (IPAddress.TryParse(host, out var address)) return Direct(address);
        var servers = DnsResolverSelection.Get(alternativeDns, adapterId, dnsFilePath);
        var tasks = servers.Select(server => QueryAllAsync(server, host, cancellationToken));
        return Merge(await Task.WhenAll(tasks));
    }

    private static IReadOnlyList<DnsAnswer> Direct(IPAddress address) =>
        [new DnsAnswer(address, address.ToString(), address.AddressFamily == AddressFamily.InterNetworkV6)];

    private async Task<IReadOnlyList<DnsAnswer>> QueryAllAsync(IPAddress server, string host, CancellationToken token)
    {
        var queries = await Task.WhenAll(
            QueryAsync(server, host, false, token), QueryAsync(server, host, true, token));
        return queries.SelectMany(items => items).ToArray();
    }

    private async Task<IReadOnlyList<DnsAnswer>> QueryAsync(IPAddress server, string host,
        bool ipv6, CancellationToken token)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        cts.CancelAfter(TimeoutMs);
        try
        {
            var query = DnsAddressQuery.Create(host, ipv6);
            using var udp = UdpSocketFactory.Create(server.AddressFamily,
                NetworkAdapterCatalog.Source(adapterId));
            await udp.SendAsync(query, query.Length, new IPEndPoint(server, DnsPort));
            return await ReadAsync(udp, query, ipv6, cts.Token);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested) { return []; }
        catch (SocketException) { return []; }
    }

    private static async Task<IReadOnlyList<DnsAnswer>> ReadAsync(UdpClient udp, byte[] query,
        bool ipv6, CancellationToken token)
    {
        var receive = udp.ReceiveAsync();
        var done = await Task.WhenAny(receive, Task.Delay(TimeoutMs, token));
        if (done != receive) { token.ThrowIfCancellationRequested(); return []; }
        var response = await receive;
        return DnsAddressAnswer.Parse(response.Buffer, query, ipv6)
            .Select(address => new DnsAnswer(address, response.RemoteEndPoint.Address.ToString(), ipv6))
            .ToArray();
    }

    private static IReadOnlyList<DnsAnswer> Merge(IReadOnlyList<DnsAnswer>[] groups)
    {
        var seen = new HashSet<IPAddress>();
        var result = new List<DnsAnswer>();
        foreach (var answer in groups.SelectMany(items => items))
        {
            if (!seen.Add(answer.Address)) continue;
            if (result.Count >= MaxAnswers) break;
            result.Add(answer);
        }
        return result;
    }
}
