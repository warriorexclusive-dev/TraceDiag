namespace TraceDiag.Models;

public sealed record HostDiscoveryResult(
    IReadOnlyList<HopInfo> Hops,
    IReadOnlySet<int> SkippedTtls,
    bool TargetReached);
