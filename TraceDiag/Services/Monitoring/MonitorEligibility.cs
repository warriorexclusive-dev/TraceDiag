using TraceDiag.Models;

namespace TraceDiag.Services.Monitoring;

/// <summary>Decides which discovered nodes take part in continuous monitoring.</summary>
internal static class MonitorEligibility
{
    internal static bool IsEligible(HopInfo hop) =>
        hop.Diagnosis != Strings.Diagnosis.IcmpDisabled;
}
