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
    }

    [Fact]
    public void SaveThenLoad_PreservesFontSize()
    {
        using var temp = new TempDirectory();
        var path = temp.File("settings.json");
        ConfigFileIO.Save(path, new AppConfig { OutputFontSize = 13.5f });
        Assert.Equal(13.5f, ConfigFileIO.Load(path).OutputFontSize);
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
        Assert.Equal(9.5f, store.Load().OutputFontSize);
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
