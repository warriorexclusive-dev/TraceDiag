using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class PacketSizePolicyControllerTests
{
    [Fact]
    public void Refresh_WithoutDontFragment_UsesSafeMaximum()
    {
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        new PacketSizePolicyController(controls, () => true).Refresh();
        Assert.Equal(UiDefaults.MaxPacketSize, (int)controls.MaxPacketSize.Maximum);
        Assert.True(controls.DynamicPacketSize.Enabled);
    }

    [Fact]
    public void Refresh_WithDontFragment_CapsSizeAndDisablesDynamic()
    {
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        controls.MaxPacketSize.Value = 2048;
        controls.DynamicPacketSize.Checked = true;
        controls.DontFragment.Checked = true;
        new PacketSizePolicyController(controls, () => true).Refresh();
        Assert.Equal(1024, (int)controls.MaxPacketSize.Maximum);
        Assert.Equal(1024, (int)controls.MaxPacketSize.Value);
        Assert.False(controls.DynamicPacketSize.Checked);
        Assert.False(controls.DynamicPacketSize.Enabled);
    }

    [Fact]
    public void Refresh_AfterDontFragmentOff_RestoresDynamicAvailability()
    {
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        var policy = new PacketSizePolicyController(controls, () => true);
        controls.DontFragment.Checked = true;
        policy.Refresh();
        controls.DontFragment.Checked = false;
        policy.Refresh();
        Assert.Equal(UiDefaults.MaxPacketSize, (int)controls.MaxPacketSize.Maximum);
        Assert.True(controls.DynamicPacketSize.Enabled);
        Assert.False(controls.DynamicPacketSize.Checked);
    }

    [Fact]
    public void Refresh_InMonitoringMode_DisablesDynamicAndAttempts()
    {
        var controls = FormControlsFactory.Create(12f);
        using var root = controls.Root;
        controls.MonitoringMode.Checked = true;
        new PacketSizePolicyController(controls, () => true).Refresh();
        Assert.False(controls.DynamicPacketSize.Enabled);
        Assert.False(controls.Attempts.Enabled);
        Assert.True(controls.MaxPacketSize.Enabled);
    }
}
