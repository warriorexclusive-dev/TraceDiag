using TraceDiag.Models;

namespace TraceDiag.Ui;

internal sealed record RunInput(
    string Host,
    int MaxPacketSize,
    int Attempts,
    int LossDelayMs,
    bool DynamicPacketSize,
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
