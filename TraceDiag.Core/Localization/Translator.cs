using TraceDiag.Localization;

namespace TraceDiag;

/// <summary>Translates interface text; unknown text stays in English.</summary>
public static class Translator
{
    private static Language _current = Language.En;

    public static Language Current
    {
        get => _current;
        set => _current = Enum.IsDefined(typeof(Language), value) ? value : Language.En;
    }

    public static string Text(string english) =>
        LanguageTables.For(_current).TryGetValue(english, out var value) ? value : english;

    /// <summary>Sets the interface language with a fallback to English.</summary>
    public static void SetLanguage(Language language) => Current = language;

    /// <summary>Points translations at another folder; null restores the default folder.</summary>
    public static void UseDirectory(string? directory) => LanguageTables.UseDirectory(directory);
}
