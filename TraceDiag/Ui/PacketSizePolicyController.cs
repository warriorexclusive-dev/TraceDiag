using TraceDiag.Models;

namespace TraceDiag.Ui;

/// <summary>Applies the DF size cap to the input panel without owning other controls.</summary>
internal sealed class PacketSizePolicyController(MainControls controls, Func<bool> isActive) : IPacketSizePolicy
{
    public void ApplyDontFragment()
    {
        controls.MaxPacketSize.Maximum = AllowedMaximum;
        if (!controls.DontFragment.Checked) return;
        controls.DynamicPacketSize.Checked = false;
        if (controls.MaxPacketSize.Value > AllowedMaximum) controls.MaxPacketSize.Value = AllowedMaximum;
    }

    public void Refresh()
    {
        ApplyDontFragment();
        var active = isActive();
        var monitoring = controls.MonitoringMode.Checked;
        controls.RemoteHost.Enabled = active;
        controls.MaxPacketSize.Enabled = active;
        controls.Attempts.Enabled = active && !monitoring;
        controls.LossDelay.Enabled = active;
        controls.DynamicPacketSize.Enabled = active && !monitoring && !controls.DontFragment.Checked;
        controls.MonitoringMode.Enabled = active;
        controls.DontFragment.Enabled = active;
        controls.DarkTheme.Enabled = active;
    }

    private decimal AllowedMaximum => controls.DontFragment.Checked
        ? ProbeLimits.DontFragmentPacketSize
        : UiDefaults.MaxPacketSize;
}
