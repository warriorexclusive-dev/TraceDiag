using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;

namespace TraceDiag.Services;

/// <summary>Cached RIPEstat whois client for IP registration owners.</summary>
internal sealed class RipeOwnerClient
{
    private const string Endpoint = "https://stat.ripe.net/data/whois/data.json?resource=";
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(5) };
    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.Ordinal);

    internal async Task<string?> ResolveAsync(IPAddress address, CancellationToken token)
    {
        var key = address.ToString();
        if (_cache.TryGetValue(key, out var cached)) return EmptyToNull(cached);
        var uri = Endpoint + Uri.EscapeDataString(key);
        using var response = await Client.GetAsync(uri, token);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        var owner = EmptyToNull(RipeWhoisParser.Parse(json));
        if (owner is not null) _cache[key] = owner;
        return owner;
    }

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
