namespace TraceDiag.Services.Diagnostics;

internal static class DiagnosticConstants
{
    internal const int MaxHops = 255;
    internal const int ProbeTimeoutMs = 3000;
    internal const int WatchdogExtraDelayMs = 2500;
    internal const int MaxLostPingsBeforeSkip = 10;
    internal const int DiscoveryPacketSize = 64;
    internal const int DiscoveryTimeoutMs = 5000;
    internal const int DiscoveryRoundLimit = 3;
    internal const int VerifyAttemptLimit = 10;
    internal const int MonitorIntervalMs = 1000;
    internal const int SlowDnsThresholdMs = 2000;
    internal const int DirectProbeTtl = 64;
}
