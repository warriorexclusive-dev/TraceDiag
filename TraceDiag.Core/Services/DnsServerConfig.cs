using System.Net;
using System.Text.Json;

namespace TraceDiag.Services;

/// <summary>Loads the resolver list from a JSON file with a safe fallback.</summary>
internal static class DnsServerConfig
{
    internal const string FileName = "dns-servers.json";

    internal static string DefaultPath() =>
        Path.Combine(AppFiles.GetAppDataDirectory(), FileName);

    internal static IReadOnlyList<IPAddress> Load(string? path = null)
    {
        var file = string.IsNullOrWhiteSpace(path) ? FindFile() : path;
        if (file is null || !File.Exists(file)) return DnsServers.Get();
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file));
            if (!document.RootElement.TryGetProperty("resolvers", out var list)) return DnsServers.Get();
            var servers = Collect(list);
            return servers.Count == 0 ? DnsServers.Get() : servers;
        }
        catch (Exception) { return DnsServers.Get(); }
    }

    internal static IReadOnlyList<IPAddress> Read(string path)
    {
        if (!File.Exists(path)) return [];
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            return document.RootElement.TryGetProperty("resolvers", out var list) ? Collect(list) : [];
        }
        catch (Exception) { return []; }
    }

    internal static void Save(string path, IEnumerable<IPAddress> servers)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        var items = servers.Select(server => $"\"{server}\"");
        File.WriteAllText(path, "{\n  \"resolvers\": [\n    " +
            string.Join(",\n    ", items) + "\n  ]\n}\n");
    }

    private static IReadOnlyList<IPAddress> Collect(JsonElement list)
    {
        var result = new List<IPAddress>();
        foreach (var item in list.EnumerateArray()) Add(result, item.GetString());
        return result;
    }

    private static void Add(List<IPAddress> result, string? text)
    {
        if (IPAddress.TryParse(text, out var address) && !result.Contains(address)) result.Add(address);
    }

    private static string? FindFile()
    {
        var candidates = new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory(), DefaultPath() };
        foreach (var path in candidates)
            if (File.Exists(path)) return path;
        return null;
    }
}
