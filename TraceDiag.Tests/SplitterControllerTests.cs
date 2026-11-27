using System.Windows.Forms;
using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class SplitterControllerTests
{
    [Fact]
    public void Apply_ThenPercent_RoundTrips()
    {
        using var split = Measured();
        SplitterController.Apply(split, 55);
        Assert.Equal(55, SplitterController.Percent(split));
    }

    [Fact]
    public void Apply_ClampsToAllowedRange()
    {
        using var split = Measured();
        SplitterController.Apply(split, 5);
        Assert.Equal(UiDefaults.MinStatsPercent, SplitterController.Percent(split));
        SplitterController.Apply(split, 150);
        Assert.InRange(SplitterController.Percent(split), UiDefaults.MaxStatsPercent - 2, UiDefaults.MaxStatsPercent);
    }

    private static SplitContainer Measured() => new()
    {
        Width = 600,
        Height = 400,
        SplitterWidth = UiDefaults.SplitterWidth,
        Panel1MinSize = UiDefaults.MinPaneHeight,
        Panel2MinSize = UiDefaults.MinPaneHeight
    };
}
