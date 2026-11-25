using System.Globalization;
using TraceDiag.Tests.TestData;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class TextFormatterTests
{
    [Fact]
    public void Fmt_UsesInvariantCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            Assert.Equal("1.5", TextFormatter.Fmt("{0:F1}", 1.5));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Now_UsesConfiguredTimeFormat()
    {
        Assert.Matches(@"^\d{2}:\d{2}:\d{2}$", TextFormatter.Now());
    }

    [Fact]
    public void UiDefaults_ClampsFontSize()
    {
        Assert.Equal(8m, UiDefaults.ClampFontSize(2f));
        Assert.Equal(24m, UiDefaults.ClampFontSize(100f));
        Assert.Equal(11.5m, UiDefaults.ClampFontSize(11.5f));
    }
}
