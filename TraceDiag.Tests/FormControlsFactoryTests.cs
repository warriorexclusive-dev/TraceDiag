using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class FormControlsFactoryTests
{
    [Fact]
    public void Create_UsesDefaultLossDelayOfOneSecond()
    {
        var controls = FormControlsFactory.Create(12f);
        Assert.Equal(UiDefaults.DefaultLossDelayMs, (int)controls.LossDelay.Value);
        Assert.Equal(UiDefaults.MaxLossDelayMs, (int)controls.LossDelay.Maximum);
    }

    [Fact]
    public void Create_StartsWithRunButtonText()
    {
        var controls = FormControlsFactory.Create(12f);
        Assert.Equal(Strings.Ui.RunButton, controls.Run.Text);
    }
}
