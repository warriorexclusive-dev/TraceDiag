using System.Text.Json;

namespace TraceDiag.Services;

/// <summary>Extracts the registration owner from a RIPEstat whois response.</summary>
internal static class RipeWhoisParser
{
    private static readonly string[] PreferredKeys = ["OrgName", "Organization", "org-name", "descr", "NetName"];

    internal static string? Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (!Success(root) || !root.TryGetProperty("data", out var data)) return null;
            var values = Collect(data);
            foreach (var key in PreferredKeys)
                if (values.TryGetValue(key, out var value)) return value;
            return null;
        }
        catch (JsonException) { return null; }
    }

    private static bool Success(JsonElement root) =>
        root.TryGetProperty("status", out var status) && status.GetString() == "ok";

    private static Dictionary<string, string> Collect(JsonElement data)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        AddRecords(values, data, "records");
        AddRecords(values, data, "irr_records");
        return values;
    }

    private static void AddRecords(Dictionary<string, string> values, JsonElement data, string name)
    {
        if (!data.TryGetProperty(name, out var groups)) return;
        foreach (var group in groups.EnumerateArray())
            foreach (var field in group.EnumerateArray())
                AddField(values, field);
    }

    private static void AddField(Dictionary<string, string> values, JsonElement field)
    {
        if (!field.TryGetProperty("key", out var keyElement) ||
            !field.TryGetProperty("value", out var valueElement)) return;
        var key = keyElement.GetString();
        var value = valueElement.GetString()?.Trim();
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value) && !values.ContainsKey(key!))
            values[key!] = value!;
    }
}
