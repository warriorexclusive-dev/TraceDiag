namespace TraceDiag.Services.Diagnostics;

internal sealed record DiscoverySettings(
    int PacketSize,
    int TimeoutMs,
    int RoundLimit,
    bool DontFragment,
    int LossDelayMs)
{
    internal static DiscoverySettings CreateDefault(int lossDelayMs, bool dontFragment = true) =>
        new(DiagnosticConstants.DiscoveryPacketSize,
            DiagnosticConstants.DiscoveryTimeoutMs,
            DiagnosticConstants.DiscoveryRoundLimit,
            dontFragment,
            Math.Max(0, lossDelayMs));
}
