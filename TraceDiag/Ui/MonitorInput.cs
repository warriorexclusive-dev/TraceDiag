using TraceDiag.Models;

namespace TraceDiag.Ui;

/// <summary>Values read from the input panel when monitoring is selected.</summary>
internal sealed record MonitorInput(
    string Host,
    int PacketSize,
    int LossDelayMs,
    bool DontFragment = false,
    IpFamily Family = IpFamily.Auto,
    bool RipeLookup = true,
    bool RoutePrecheck = true,
    bool DeepRoutes = false,
    bool AlternativeDns = true,
    bool AsnLookup = true,
    string? AdapterId = null,
    string? DnsFilePath = null,
    int ProbeTimeoutMs = 3000);
