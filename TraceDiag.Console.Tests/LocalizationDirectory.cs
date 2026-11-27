using System.Text.Json;
using TraceDiag.Localization;

namespace TraceDiag.Console.Tests;

internal sealed class LocalizationDirectory : IDisposable
{
    private readonly string _path = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(), "TraceDiagConsoleTests-" + Guid.NewGuid().ToString("N"));

    internal LocalizationDirectory(string code, params (string Key, string Value)[] strings)
    {
        Directory.CreateDirectory(_path);
        var values = new Dictionary<string, string>();
        foreach (var entry in strings) values[entry.Key] = entry.Value;
        var json = JsonSerializer.Serialize(new { language = code, strings = values });
        System.IO.File.WriteAllText(System.IO.Path.Combine(_path, code + ".json"), json);
        Translator.UseDirectory(_path);
        Translator.SetLanguage((Language)Enum.Parse(typeof(Language), code, true));
    }

    public void Dispose()
    {
        Translator.UseDirectory(null);
        Translator.SetLanguage(Language.En);
        if (Directory.Exists(_path)) Directory.Delete(_path, recursive: true);
    }
}
