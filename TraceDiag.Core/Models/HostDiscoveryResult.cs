namespace TraceDiag.Models;

public sealed record HostDiscoveryResult(
    IReadOnlyList<HopInfo> Hops,
    IReadOnlyCollection<int> SkippedTtls,
    bool TargetReached);
