using System.Text.Json;
using TraceDiag.Models;

namespace TraceDiag.Services;

internal static class ConfigFileIO
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    internal static AppConfig Load(string path)
    {
        if (!File.Exists(path)) return new AppConfig();
        var json = File.ReadAllText(path);
        return Normalize(JsonSerializer.Deserialize<AppConfig>(json, Options) ?? new AppConfig());
    }

    internal static void Save(string path, AppConfig config)
    {
        EnsureDirectory(path);
        var json = JsonSerializer.Serialize(Normalize(config), Options);
        File.WriteAllText(path, json);
    }

    private static void EnsureDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
    }

    private static AppConfig Normalize(AppConfig config)
    {
        config.OutputFontSize = Math.Clamp(config.OutputFontSize, 8f, 24f);
        return config;
    }
}
