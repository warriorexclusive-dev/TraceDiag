using System.Text.Json;
using System.Text.Json.Serialization;
using TraceDiag.Localization;
using TraceDiag.Models;
using TraceDiag.Ui;

namespace TraceDiag.Services;

internal static class ConfigFileIO
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

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
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        }
        catch (ArgumentException) { /* Invalid paths must not break diagnostics. */ }
    }

    private static AppConfig Normalize(AppConfig config)
    {
        config.OutputFontSize = Compat.Clamp(config.OutputFontSize, 8f, 24f);
        if (!Enum.IsDefined(typeof(Language), config.Language)) config.Language = Language.En;
        if (!UiDefaults.IconSizes.Contains(config.IconSizePx)) config.IconSizePx = UiDefaults.DefaultIconSizePx;
        return config;
    }
}
