using System.Text.Json;
using TraceDiag.Localization;

namespace TraceDiag.Tests.TestData;

internal sealed class LocalizationDirectory : IDisposable
{
    private readonly TempDirectory _temp = new();

    internal LocalizationDirectory()
    {
        Path = System.IO.Path.Combine(_temp.Path, "localizations");
        Directory.CreateDirectory(Path);
    }

    internal string Path { get; }

    internal LocalizationDirectory Add(string code, params (string Key, string Value)[] strings)
    {
        var values = new Dictionary<string, string>();
        foreach (var entry in strings) values[entry.Key] = entry.Value;
        var json = JsonSerializer.Serialize(new { language = code, strings = values });
        System.IO.File.WriteAllText(System.IO.Path.Combine(Path, code + ".json"), json);
        return this;
    }

    internal void Activate()
    {
        Translator.UseDirectory(Path);
        Translator.SetLanguage(Parse(Path));
    }

    public void Dispose()
    {
        Translator.UseDirectory(null);
        Translator.SetLanguage(Language.En);
        _temp.Dispose();
    }

    private static Language Parse(string path)
    {
        var code = System.IO.Path.GetFileNameWithoutExtension(
            Directory.GetFiles(path, "*.json").First());
        return (Language)Enum.Parse(typeof(Language), code, true);
    }
}
