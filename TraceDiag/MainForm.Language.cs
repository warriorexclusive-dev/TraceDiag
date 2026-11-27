using TraceDiag.Localization;
using TraceDiag.Ui;

namespace TraceDiag;

public sealed partial class MainForm
{
    private void InitializeLanguage()
    {
        Translator.SetLanguage(_appConfig.Language);
        LocalizationApplier.Apply(this, _controls);
    }

    private void SubscribeLanguage() =>
        _controls.Language.SelectedIndexChanged += LanguageOnSelectedChanged;

    private void LanguageOnSelectedChanged(object? sender, EventArgs e)
    {
        if (_controls.Language.SelectedIndex < 0) return;
        var language = (Language)_controls.Language.SelectedIndex;
        if (language == Translator.Current) return;
        ApplyLanguage(language);
    }

    private void ApplyLanguage(Language language)
    {
        Translator.SetLanguage(language);
        _appConfig.Language = language;
        _configStore.Save(_appConfig);
        LocalizationApplier.Apply(this, _controls);
        _logger.Info(TextFormatter.Fmt(Strings.Log.LanguageChangedFormat, language));
    }
}
