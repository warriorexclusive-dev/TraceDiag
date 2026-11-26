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

    [Fact]
    public void Create_StartsWithMonitoringModeDisabled()
    {
        var controls = FormControlsFactory.Create(12f);
        Assert.False(controls.MonitoringMode.Checked);
        Assert.Equal(Strings.Ui.MonitoringModeLabel, controls.MonitoringMode.Text);
    }

    [Theory]
    [InlineData(800, 600)]
    [InlineData(2400, 900)]
    public void AttemptsLabel_StaysAlignedWithInput(int width, int height)
    {
        using var form = new Form { ClientSize = new Size(width, height) };
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        form.Controls.Add(root);
        form.CreateControl();
        var label = FindLabel(root, Strings.Ui.AttemptsLabel);
        Assert.NotNull(label);
        Assert.InRange(Math.Abs(label!.PointToScreen(Point.Empty).Y - controls.Attempts.PointToScreen(Point.Empty).Y), 0, 8);
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
