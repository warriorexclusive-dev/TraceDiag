using TraceDiag.Localization;
using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Tests.TestData;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class BugFixTests
{
    [Fact]
    public void ApplyConfig_SelectsConfiguredLanguage()
    {
        var controls = FormControlsFactory.Create(12f);
        using var settings = new SettingsForm(controls);
        settings.ApplyConfig(new AppConfig { Language = Language.Jp });
        Assert.Equal((int)Language.Jp, controls.Language.SelectedIndex);
    }

    [Fact]
    public void RefreshLanguage_TranslatesItemsAndKeepsSelection()
    {
        using var localization = new LocalizationDirectory().Add("ru", ("English", "Английский"));
        localization.Activate();
        var controls = FormControlsFactory.Create(12f);
        Assert.Equal("Английский", controls.Language.Items[0]);
        controls.Language.SelectedIndex = (int)Language.Ru;
        Translator.SetLanguage(Language.En);
        UiText.RefreshLanguage(controls.Language);
        Assert.Equal((int)Language.Ru, controls.Language.SelectedIndex);
        Assert.Equal("English", controls.Language.Items[0]);
    }

    [Fact]
    public void RemoteHostLabel_IsTopAlignedWithInput()
    {
        var controls = FormControlsFactory.Create(12f);
        var label = FindLabel(controls.Root, Strings.T(Strings.Ui.RemoteHostLabel));
        Assert.NotNull(label);
        Assert.Equal(AnchorStyles.Top | AnchorStyles.Left, label!.Anchor);
    }

    [Fact]
    public void Read_MissingPath_UsesBundledDefaultList()
    {
        using var temp = new TempDirectory();
        var servers = DnsServerConfig.Read(temp.File("not-created.json"));
        Assert.NotEmpty(servers);
    }

    private static Label? FindLabel(Control parent, string text)
    {
        foreach (Control child in parent.Controls)
        {
            if (child is Label label && label.Text == text) return label;
            var nested = FindLabel(child, text);
            if (nested is not null) return nested;
        }
        return null;
    }
}
