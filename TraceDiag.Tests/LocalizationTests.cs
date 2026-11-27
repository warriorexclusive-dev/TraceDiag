using TraceDiag.Localization;
using TraceDiag.Tests.TestData;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class LocalizationTests
{
    [Theory]
    [InlineData("en", "Ready", "Ready")]
    [InlineData("cn", "Ready", "就绪")]
    [InlineData("fr", "Ready", "Prêt")]
    [InlineData("de", "Ready", "Bereit")]
    [InlineData("ru", "Ready", "Готово")]
    [InlineData("jp", "Ready", "準備完了")]
    public void Translator_UsesLanguageFile(string code, string key, string expected)
    {
        using var localization = new LocalizationDirectory().Add(code, (key, expected));
        localization.Activate();
        Assert.Equal(expected, Strings.T(key));
    }

    [Fact]
    public void Translator_UnknownKeyFallsBackToEnglish()
    {
        using var localization = new LocalizationDirectory().Add("ru", ("Ready", "Готово"));
        localization.Activate();
        Assert.Equal("Brand new text", Strings.T("Brand new text"));
    }

    [Fact]
    public void LocalizationApplier_UpdatesTaggedControlsWithoutRestart()
    {
        using var localization = new LocalizationDirectory().Add("ru",
            ("TraceDiag - Traceroute Diagnostic", "TraceDiag — диагностика маршрута"),
            ("Ready", "Готово"),
            ("Run", "Пуск"),
            ("Language:", "Язык:"),
            ("Host name", "Имя узла"));
        localization.Activate();
        using var form = new Form();
        var controls = FormControlsFactory.Create(9.5f);
        form.Controls.Add(controls.Root);
        LocalizationApplier.Apply(form, controls);
        Assert.Equal("TraceDiag — диагностика маршрута", form.Text);
        Assert.Equal("Пуск", controls.Run.Text);
        Assert.Equal("Готово", controls.Status.Text);
        Assert.Equal("Имя узла", controls.Stats.Columns[Strings.Ui.ColumnKeyHostName]!.HeaderText);
    }
}
