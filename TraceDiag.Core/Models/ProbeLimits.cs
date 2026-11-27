namespace TraceDiag.Models;

/// <summary>Packet-size limits and the fixed DF precheck ladder.</summary>
internal static class ProbeLimits
{
    internal const int MinPacketSize = 64;
    internal const int MaxPacketSize = 65500;
    internal const int DontFragmentPacketSize = 1024;

    internal static IReadOnlyList<int> PrecheckPacketSizes => [64, 128, 256, 1024, 1400];
}
