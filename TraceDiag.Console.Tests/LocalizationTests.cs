using System.Net;
using System.Text;
using TraceDiag.Localization;
using TraceDiag.Models;
using TraceDiag.Services;

namespace TraceDiag.Console.Tests;

public sealed class LocalizationTests
{
    [Theory]
    [InlineData("en", Language.En)]
    [InlineData("cn", Language.Cn)]
    [InlineData("fr", Language.Fr)]
    [InlineData("de", Language.De)]
    [InlineData("ru", Language.Ru)]
    [InlineData("jp", Language.Jp)]
    public void Parse_AcceptsSupportedLanguage(string code, Language expected)
    {
        using var localization = new LocalizationDirectory(code);
        var options = CommandLineParser.Parse(["run", "--host", "127.0.0.1", "--language", code]);
        Assert.Equal(expected, options.Language);
        Assert.Equal(expected, Translator.Current);
    }

    [Fact]
    public void Parse_InvalidLanguage_ThrowsTranslatedError()
    {
        using var localization = new LocalizationDirectory("ru",
            ("Unknown language: {0}", "Неизвестный язык: {0}"));
        var error = Assert.Throws<CommandLineException>(() =>
            CommandLineParser.Parse(["run", "--host", "127.0.0.1", "--language", "xx"]));
        Assert.Equal("Неизвестный язык: xx", error.Message);
    }

    [Fact]
    public void OptionSuggestions_UsesSelectedLanguage()
    {
        using var localization = new LocalizationDirectory("ru", ("Options:", "Параметры:"));
        Assert.Contains("Параметры:", OptionSuggestions.List(null));
    }

    [Fact]
    public void ConsoleLogger_KeepsFileLogEnglishAndConsoleLocalized()
    {
        using var localization = new LocalizationDirectory("ru", ("Ready", "Готово"));
        var path = Path.GetTempFileName();
        try
        {
            using var file = new FileDiagnosticLogger(path);
            var console = new List<string>();
            new ConsoleLogger(file, console.Add).InfoFmt("Ready");
            file.Flush();
            Assert.Contains("[INFO] Ready", File.ReadAllText(path));
            Assert.Contains("[INFO] Готово", console[0]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ConsoleLogger_TranslatesFormattedTemplates()
    {
        using var localization = new LocalizationDirectory("de", ("Attempts: {0}", "Versuche: {0}"));
        var path = Path.GetTempFileName();
        try
        {
            using var file = new FileDiagnosticLogger(path);
            var console = new List<string>();
            new ConsoleLogger(file, console.Add).InfoFmt("Attempts: {0}", 7);
            file.Flush();
            Assert.Contains("[INFO] Attempts: 7", File.ReadAllText(path));
            Assert.Contains("[INFO] Versuche: 7", console[0]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ConsoleLogger_TranslatesArgumentsOnlyForConsole()
    {
        using var localization = new LocalizationDirectory("de",
            ("Hop: {0}", "Hop: {0}"), ("target", "Ziel"));
        var path = Path.GetTempFileName();
        try
        {
            using var file = new FileDiagnosticLogger(path);
            var console = new List<string>();
            new ConsoleLogger(file, console.Add).InfoFmt("Hop: {0}", LocalizedArg.Of("target"));
            file.Flush();
            Assert.Contains("[INFO] Hop: target", File.ReadAllText(path));
            Assert.Contains("[INFO] Hop: Ziel", console[0]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void OutputWriter_TranslatesSummaryValues()
    {
        using var localization = new LocalizationDirectory("ru",
            ("Summary:", "Итог:"),
            ("normal", "норма"));
        var output = new StringBuilder();
        var writer = new OutputWriter(Strings.Console.FormatPlain, new StringWriter(output));
        writer.Stats(Snapshot());
        writer.WriteSummary();
        Assert.Contains("Итог:", output.ToString());
        Assert.Contains("норма", output.ToString());
    }

    private static HopStatSnapshot Snapshot() => new(
        1, IPAddress.Parse("127.0.0.1"), "localhost", 1, 1, 0, 0, 0, 0,
        Strings.Status.Normal, Strings.Diagnosis.None);
}
