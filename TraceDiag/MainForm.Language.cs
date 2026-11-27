using TraceDiag.Localization;
using TraceDiag.Ui;

namespace TraceDiag;

public sealed partial class MainForm
{
    private bool _languageChanging;

    private void InitializeLanguage()
    {
        Translator.SetLanguage(_appConfig.Language);
        ApplyTranslations();
    }

    private void SubscribeLanguage() =>
        _controls.Language.SelectedIndexChanged += LanguageOnSelectedChanged;

    private void LanguageOnSelectedChanged(object? sender, EventArgs e)
    {
        if (_languageChanging || _controls.Language.SelectedIndex < 0) return;
        var language = (Language)_controls.Language.SelectedIndex;
        if (language == Translator.Current) return;
        ApplyLanguage(language);
    }

    private void ApplyLanguage(Language language)
    {
        Translator.SetLanguage(language);
        _appConfig.Language = language;
        _configStore.Save(_appConfig);
        ApplyTranslations();
        _logger.Info(TextFormatter.Fmt(Strings.Log.LanguageChangedFormat, language));
    }

    private void ApplyTranslations()
    {
        _languageChanging = true;
        try { LocalizationApplier.Apply(this, _controls, _settings); }
        finally { _languageChanging = false; }
    }
}
