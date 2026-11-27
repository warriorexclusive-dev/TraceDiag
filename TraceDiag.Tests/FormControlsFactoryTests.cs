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
    public void Create_UsesStandardCheckBoxes()
    {
        var controls = FormControlsFactory.Create(12f);
        Assert.Equal(typeof(CheckBox), controls.DynamicPacketSize.GetType());
        Assert.Equal(typeof(CheckBox), controls.DontFragment.GetType());
        Assert.Equal(typeof(CheckBox), controls.MonitoringMode.GetType());
        Assert.Equal(typeof(CheckBox), controls.DarkTheme.GetType());
        Assert.Equal(typeof(CheckBox), controls.AlternativeDns.GetType());
    }

    [Fact]
    public void Create_StartsWithAlternativeDnsEnabled()
    {
        var controls = FormControlsFactory.Create(12f);
        Assert.True(controls.AlternativeDns.Checked);
        Assert.Equal(Strings.Ui.AltDnsLabel, controls.AlternativeDns.Text);
    }

    [Fact]
    public void Create_StartsWithAutoIpFamily()
    {
        var controls = FormControlsFactory.Create(12f);
        Assert.Equal(["Auto", "IPv4", "IPv6"], controls.IpFamily.Items.Cast<string>().ToArray());
        Assert.Equal(0, controls.IpFamily.SelectedIndex);
    }

    [Fact]
    public void Create_StartsWithDontFragmentDisabled()
    {
        var controls = FormControlsFactory.Create(12f);
        Assert.False(controls.DontFragment.Checked);
        Assert.Equal(Strings.Ui.DontFragmentLabel, controls.DontFragment.Text);
    }

    [Fact]
    public void Create_UsesSafePacketSizeBoundsAndFormattedLabel()
    {
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        var label = FindLabel(root, TextFormatter.Fmt(Strings.Ui.PacketSizeLabelFormat,
            UiDefaults.MinPacketSize, UiDefaults.MaxPacketSize));
        Assert.Equal(UiDefaults.MinPacketSize, (int)controls.MaxPacketSize.Minimum);
        Assert.Equal(UiDefaults.MaxPacketSize, (int)controls.MaxPacketSize.Maximum);
        Assert.NotNull(label);
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

    [Theory]
    [InlineData(800, 600)]
    [InlineData(2400, 900)]
    public void Checkboxes_AreLeftOfRunButton(int width, int height)
    {
        using var form = new Form { ClientSize = new Size(width, height) };
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        form.Controls.Add(root);
        form.Show();
        form.Hide();
        var runX = controls.Run.PointToScreen(Point.Empty).X;
        AssertLeftOfRun(controls.DynamicPacketSize, runX);
        AssertLeftOfRun(controls.DontFragment, runX);
        AssertLeftOfRun(controls.MonitoringMode, runX);
        AssertLeftOfRun(controls.DarkTheme, runX);
    }

    private static void AssertLeftOfRun(Control control, int runX) =>
        Assert.True(control.PointToScreen(Point.Empty).X < runX, $"{control.Text}: x={control.PointToScreen(Point.Empty).X}, runX={runX}");

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
