using TraceDiag.Models;
using TraceDiag.Ui;

namespace TraceDiag;

public sealed partial class MainForm
{
    internal RunInput? ReadRunInput()
    {
        var host = ReadHost();
        if (host is null) return null;
        return new RunInput(host, (int)_maxPacketSizeInput.Value, (int)_attemptsInput.Value,
            (int)_lossDelayInput.Value, _dynamicPacketSizeInput.Checked, _dontFragmentInput.Checked,
            ReadIpFamily(), _ripeLookupInput.Checked,
            _routePrecheckInput.Checked, _deepRoutesInput.Checked);
    }

    internal MonitorInput? ReadMonitorInput()
    {
        var host = ReadHost();
        if (host is null) return null;
        return new MonitorInput(host, (int)_maxPacketSizeInput.Value, (int)_lossDelayInput.Value,
            _dontFragmentInput.Checked, ReadIpFamily(), _ripeLookupInput.Checked,
            _routePrecheckInput.Checked, _deepRoutesInput.Checked);
    }

    private IpFamily ReadIpFamily() =>
        _ipFamilyInput.SelectedIndex switch
        {
            1 => IpFamily.IPv4,
            2 => IpFamily.IPv6,
            _ => IpFamily.Auto
        };

    private string? ReadHost()
    {
        var host = _remoteHostTextBox.Text.Trim();
        _remoteHostTextBox.Text = host;
        if (!string.IsNullOrWhiteSpace(host)) return host;
        MessageBox.Show(this, Strings.T(Strings.Ui.RemoteHostRequired),
            Strings.T(Strings.Ui.ValidationTitle), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        _remoteHostTextBox.Focus();
        return null;
    }
}
