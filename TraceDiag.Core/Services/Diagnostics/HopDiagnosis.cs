using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal sealed record HopSizeResult(
    int Size, int Sent, int Received, IReadOnlyList<IPAddress>? Responders = null);

internal static class HopDiagnosis
{
    internal static void Apply(HopInfo hop, IReadOnlyList<HopSizeResult> results)
    {
        hop.SetDiagnosis(Strings.Diagnosis.None);
        foreach (var reason in Collect(hop.IsSkipped, results)) hop.AddDiagnosis(reason);
    }

    private static IReadOnlyList<string> Collect(bool skipped, IReadOnlyList<HopSizeResult> results)
    {
        if (results.Count == 0) return [skipped ? Strings.Diagnosis.IcmpDisabled : Strings.Diagnosis.None];
        if (results.All(result => result.Received == 0)) return [Strings.Diagnosis.IcmpDisabled];
        var reasons = new List<string>();
        if (HasSizeCut(results)) reasons.Add(Strings.Diagnosis.PacketSizeLimit);
        if (HasPartialLoss(results)) reasons.Add(Strings.Diagnosis.AntiFlood);
        return reasons.Count == 0 ? [Strings.Diagnosis.None] : reasons;
    }

    private static bool HasSizeCut(IReadOnlyList<HopSizeResult> results) =>
        results.Any(larger => IsFailed(larger) || DegradedAfterClean(results, larger));

    private static bool IsFailed(HopSizeResult result) =>
        result.Sent > 0 && result.Received == 0;

    private static bool DegradedAfterClean(IReadOnlyList<HopSizeResult> results, HopSizeResult larger) =>
        results.Any(smaller =>
            smaller.Size < larger.Size &&
            smaller.Sent > 0 &&
            smaller.Received == smaller.Sent &&
            larger.Received < larger.Sent);

    private static bool HasPartialLoss(IReadOnlyList<HopSizeResult> results) =>
        results.Any(result => result.Received > 0 && result.Received < result.Sent);
}
