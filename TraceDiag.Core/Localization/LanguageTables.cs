using System.Collections.Generic;
using System.Text.Json;

namespace TraceDiag.Localization;

/// <summary>Loads language files from the localizations folder next to the executable.</summary>
internal static class LanguageTables
{
    private static readonly Dictionary<string, string> Empty = new();
    private static string? _directory;
    private static Dictionary<Language, Dictionary<string, string>> _all = Load();

    internal static string Directory => _directory ?? Path.Combine(AppContext.BaseDirectory, "localizations");

    internal static void UseDirectory(string? directory)
    {
        _directory = directory;
        _all = Load();
    }

    internal static Dictionary<string, string> For(Language language) =>
        _all.TryGetValue(language, out var table) ? table : Empty;

    private static Dictionary<Language, Dictionary<string, string>> Load()
    {
        var result = new Dictionary<Language, Dictionary<string, string>>();
        if (!System.IO.Directory.Exists(Directory)) return result;
        foreach (var path in System.IO.Directory.GetFiles(Directory, "*.json"))
        {
            if (TryRead(path, out var language, out var strings)) result[language] = strings;
        }
        return result;
    }

    private static bool TryRead(string path, out Language language, out Dictionary<string, string> strings)
    {
        language = Language.En;
        strings = new Dictionary<string, string>();
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (!TryLanguage(document.RootElement, path, out language)) return false;
            strings = ReadStrings(document.RootElement);
            return true;
        }
        catch (JsonException) { return false; }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }

    private static bool TryLanguage(JsonElement root, string path, out Language language)
    {
        var code = root.TryGetProperty("language", out var value) ? value.GetString()
            : Path.GetFileNameWithoutExtension(path);
        return Enum.TryParse(code, true, out language);
    }

    private static Dictionary<string, string> ReadStrings(JsonElement root)
    {
        var strings = new Dictionary<string, string>();
        if (!root.TryGetProperty("strings", out var values)) return strings;
        foreach (var property in values.EnumerateObject())
            if (property.Value.ValueKind == JsonValueKind.String)
                strings[property.Name] = property.Value.GetString()!;
        return strings;
    }
}
