using System.Drawing;
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
        Assert.Equal(typeof(CheckBox), controls.AsnLookup.GetType());
    }

    [Fact]
    public void Create_StartsWithAlternativeDnsEnabled()
    {
        var controls = FormControlsFactory.Create(12f);
        Assert.True(controls.AlternativeDns.Checked);
        Assert.Equal(Strings.Ui.AltDnsLabel, controls.AlternativeDns.Text);
    }

    [Fact]
    public void Create_StartsWithAsnLookupEnabled()
    {
        var controls = FormControlsFactory.Create(12f);
        Assert.True(controls.AsnLookup.Checked);
        Assert.Equal(Strings.Ui.AsnLookupLabel, controls.AsnLookup.Text);
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
        using var settings = new SettingsForm(controls);
        var label = FindLabel(settings, TextFormatter.Fmt(Strings.Ui.PacketSizeLabelFormat,
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
        var controls = FormControlsFactory.Create(12f);
        using var settings = new SettingsForm(controls) { ClientSize = new Size(width, height) };
        settings.Show();
        settings.Hide();
        var label = FindLabel(settings, Strings.Ui.AttemptsLabel);
        Assert.NotNull(label);
        Assert.InRange(Math.Abs(label!.PointToScreen(Point.Empty).Y -
            controls.Attempts.PointToScreen(Point.Empty).Y), 0, 8);
    }

    [Fact]
    public void MainToolbar_HoldsOnlyHostAndThreeIcons()
    {
        var controls = FormControlsFactory.Create(12f);
        Assert.Contains(controls.RemoteHost, Flatten(controls.Root));
        Assert.Contains(controls.Play, Flatten(controls.Root));
        Assert.Contains(controls.Stop, Flatten(controls.Root));
        Assert.Contains(controls.Settings, Flatten(controls.Root));
        Assert.DoesNotContain(controls.MaxPacketSize, Flatten(controls.Root));
        Assert.DoesNotContain(controls.DarkTheme, Flatten(controls.Root));
    }

    [Fact]
    public void SettingsForm_HoldsFiveGroups()
    {
        var controls = FormControlsFactory.Create(12f);
        using var settings = new SettingsForm(controls);
        var tabs = Assert.IsType<TabControl>(settings.Controls.OfType<TabControl>().Single());
        Assert.Equal(5, tabs.TabPages.Count);
        Assert.Equal(
            [
                Strings.Ui.ConnectionGroup,
                Strings.Ui.SourcesGroup,
                Strings.Ui.ModesGroup,
                Strings.Ui.InterfaceGroup,
                Strings.Ui.CheckOptionsGroup
            ],
            tabs.TabPages.Cast<TabPage>().Select(page => page.Text).ToArray());
        Assert.Contains(controls.Adapter, Flatten(settings));
        Assert.Contains(controls.DnsFilePath, Flatten(settings));
        Assert.Contains(controls.ProbeTimeout, Flatten(settings));
    }

    [Fact]
    public void IconSizes_UseApprovedPixelValues()
    {
        Assert.Equal([64, 72, 96, 128], UiDefaults.IconSizes);
        Assert.Equal(64, UiDefaults.DefaultIconSizePx);
    }

    [Fact]
    public void SvgIconLoader_RendersRequestedPixelSize()
    {
        using var image = SvgIconLoader.Load("play", 72, Color.Red);
        Assert.Equal(72, image.Width);
        Assert.Equal(72, image.Height);
    }

    private static IEnumerable<Control> Flatten(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var nested in Flatten(child)) yield return nested;
        }
    }

    private static Label? FindLabel(Control parent, string text) =>
        Flatten(parent).OfType<Label>().FirstOrDefault(label => label.Text == text);
}
