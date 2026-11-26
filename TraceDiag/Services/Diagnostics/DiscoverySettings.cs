namespace TraceDiag.Services.Diagnostics;

internal sealed record DiscoverySettings(
    int PacketSize,
    int TimeoutMs,
    int IntervalMs,
    int RoundLimit,
    bool DontFragment,
    int DelayMs)
{
    internal static DiscoverySettings CreateDefault(int delayMs) =>
        new(DiagnosticConstants.DiscoveryPacketSize,
            DiagnosticConstants.DiscoveryTimeoutMs,
            Math.Max(0, delayMs),
            DiagnosticConstants.DiscoveryRoundLimit,
            true,
            Math.Max(0, delayMs));
}
