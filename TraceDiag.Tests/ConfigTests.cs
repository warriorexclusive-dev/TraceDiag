using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class ConfigTests
{
    [Fact]
    public void Load_WhenFileMissing_ReturnsDefaultConfig()
    {
        using var temp = new TempDirectory();
        var config = ConfigFileIO.Load(temp.File("missing.json"));
        Assert.Equal(9.5f, config.OutputFontSize);
        Assert.Equal(AppTheme.Dark, config.Theme);
    }

    [Fact]
    public void SaveThenLoad_PreservesSettings()
    {
        using var temp = new TempDirectory();
        var path = temp.File("settings.json");
        ConfigFileIO.Save(path, new AppConfig { OutputFontSize = 13.5f, Theme = AppTheme.Light });
        var loaded = ConfigFileIO.Load(path);
        Assert.Equal(13.5f, loaded.OutputFontSize);
        Assert.Equal(AppTheme.Light, loaded.Theme);
    }

    [Fact]
    public void Load_ClampsFontSizeToAllowedRange()
    {
        using var temp = new TempDirectory();
        var path = temp.File("settings.json");
        ConfigFileIO.Save(path, new AppConfig { OutputFontSize = 100f });
        Assert.Equal(24f, ConfigFileIO.Load(path).OutputFontSize);
    }

    [Fact]
    public void AppConfigStore_WithCorruptedJson_ReturnsDefault()
    {
        using var temp = new TempDirectory();
        var path = temp.File("settings.json");
        File.WriteAllText(path, "{not json");
        var store = new AppConfigStore(path);
        var config = store.Load();
        Assert.Equal(9.5f, config.OutputFontSize);
        Assert.Equal(AppTheme.Dark, config.Theme);
    }

    [Fact]
    public void AppConfigStore_SaveCreatesDirectory()
    {
        using var temp = new TempDirectory();
        var path = temp.File("nested/settings.json");
        new AppConfigStore(path).Save(new AppConfig { OutputFontSize = 11f });
        Assert.True(File.Exists(path));
    }
}
