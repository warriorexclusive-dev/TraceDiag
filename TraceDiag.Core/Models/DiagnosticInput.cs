using System.Net;

namespace TraceDiag.Models;

public sealed record DiagnosticInput(
    IPAddress RemoteAddress,
    string RemoteHostLabel,
    int MaxPacketSize,
    int Attempts,
    int LossDelayMs,
    bool DynamicPacketSize,
    bool DontFragment = false,
    bool RipeLookup = true,
    bool RoutePrecheck = true,
    bool DeepRoutes = false,
    int ProbeTimeoutMs = 3000);
