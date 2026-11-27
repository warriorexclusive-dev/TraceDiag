using System.Net;

namespace TraceDiag.Models;

/// <summary>Input contract for continuous monitoring: one fixed packet size, no attempt limit.</summary>
public sealed record MonitoringInput(
    IPAddress RemoteAddress,
    string RemoteHostLabel,
    int PacketSize,
    int LossDelayMs,
    bool DontFragment = false,
    bool RipeLookup = true,
    bool RoutePrecheck = true,
    bool DeepRoutes = false,
    int ProbeTimeoutMs = 3000,
    bool AlternativeDns = true,
    bool AsnLookup = true,
    string? AdapterId = null,
    string? DnsFilePath = null);
