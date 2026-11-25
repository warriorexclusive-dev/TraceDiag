namespace TraceDiag.Services.Diagnostics;

internal sealed record DiscoverySettings(
    int PacketSize,
    int TimeoutMs,
    int IntervalMs,
    int RoundLimit,
    bool DontFragment)
{
    internal static DiscoverySettings CreateDefault() =>
        new(DiagnosticConstants.DiscoveryPacketSize,
            DiagnosticConstants.DiscoveryTimeoutMs,
            DiagnosticConstants.DiscoveryIntervalMs,
            DiagnosticConstants.DiscoveryRoundLimit,
            true);
}
