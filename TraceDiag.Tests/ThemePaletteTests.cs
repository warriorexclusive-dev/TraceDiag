using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class ThemePaletteTests
{
    [Fact]
    public void DarkPalette_UsesDarkSurfaceAndLightText()
    {
        var palette = ThemePalette.Dark;
        Assert.True(palette.Window.R < 80);
        Assert.True(palette.Text.R > 200);
    }

    [Fact]
    public void LightPalette_HasBrighterSurfaceThanDarkPalette()
    {
        Assert.True(ThemePalette.Light.Window.R > ThemePalette.Dark.Window.R);
    }
}
