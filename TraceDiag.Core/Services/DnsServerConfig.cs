using System.Net;
using System.Text.Json;

namespace TraceDiag.Services;

/// <summary>Loads the resolver list from dns-servers.json with a safe fallback.</summary>
internal static class DnsServerConfig
{
    private const string FileName = "dns-servers.json";

    internal static IReadOnlyList<IPAddress> Load()
    {
        var path = FindFile();
        if (path is null) return DnsServers.Get();
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (!document.RootElement.TryGetProperty("resolvers", out var list)) return DnsServers.Get();
            return Collect(list);
        }
        catch (Exception) { return DnsServers.Get(); }
    }

    private static IReadOnlyList<IPAddress> Collect(JsonElement list)
    {
        var result = new List<IPAddress>();
        foreach (var item in list.EnumerateArray()) Add(result, item.GetString());
        return result.Count == 0 ? DnsServers.Get() : result;
    }

    private static void Add(List<IPAddress> result, string? text)
    {
        if (IPAddress.TryParse(text, out var address) && !result.Contains(address)) result.Add(address);
    }

    private static string? FindFile()
    {
        var candidates = new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };
        foreach (var directory in candidates)
        {
            var path = Path.Combine(directory, FileName);
            if (File.Exists(path)) return path;
        }
        return null;
    }
}
