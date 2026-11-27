using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Monitoring;

/// <summary>Decides which discovered nodes take part in continuous monitoring.</summary>
internal static class MonitorEligibility
{
    internal static bool IsEligible(HopInfo hop) =>
        !DiagnosisText.Contains(hop.Diagnosis, Strings.Diagnosis.IcmpDisabled);
}
