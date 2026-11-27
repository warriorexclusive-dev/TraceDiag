using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Converts between batch and per-hop progress channels.</summary>
internal sealed class BatchStatsAdapter(IProgress<HopStatSnapshot> target) : IProgress<IReadOnlyList<HopStatSnapshot>>
{
    public void Report(IReadOnlyList<HopStatSnapshot> value)
    {
        foreach (var hop in value) target.Report(hop);
    }
}

internal sealed class SingleHopAdapter(IProgress<IReadOnlyList<HopStatSnapshot>> target) : IProgress<HopStatSnapshot>
{
    public void Report(HopStatSnapshot value) => target.Report([value]);
}
