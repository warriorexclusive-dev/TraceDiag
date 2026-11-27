using TraceDiag.Models;

namespace TraceDiag.Services.Monitoring;

/// <summary>Adapts list-based discovery reports to the per-snapshot monitoring channel.</summary>
internal sealed class MonitoringStatsAdapter(IProgress<HopStatSnapshot> target) : IProgress<IReadOnlyList<HopStatSnapshot>>
{
    public void Report(IReadOnlyList<HopStatSnapshot> value)
    {
        foreach (var stat in value) target.Report(stat);
    }
}
