using TraceDiag.Models;

namespace TraceDiag.Ui;

/// <summary>Values read from the input panel when monitoring is selected.</summary>
internal sealed record MonitorInput(
    string Host,
    int PacketSize,
    int LossDelayMs,
    bool DontFragment = false,
    IpFamily Family = IpFamily.Auto,
    bool RipeLookup = true);
