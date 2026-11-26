namespace TraceDiag.Services.Diagnostics;

internal sealed record DiscoverySettings(
    int PacketSize,
    int TimeoutMs,
    int RoundLimit,
    bool DontFragment,
    int LossDelayMs)
{
    internal static DiscoverySettings CreateDefault(int lossDelayMs) =>
        new(DiagnosticConstants.DiscoveryPacketSize,
            DiagnosticConstants.DiscoveryTimeoutMs,
            DiagnosticConstants.DiscoveryRoundLimit,
            true,
            Math.Max(0, lossDelayMs));
}
