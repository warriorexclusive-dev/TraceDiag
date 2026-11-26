using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class LossAnalyzer
{
    internal static async Task AnalyzeAsync(DiagnosticContext context, IPAddress target, IReadOnlyList<HopInfo> hops, int verifyPacketSize, int attempts, int lossDelayMs, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        ReportTargetStatus(context, target, hops, progress, stats);
        await AnalyzeLossesAsync(context, hops, verifyPacketSize, attempts, lossDelayMs, progress, stats, cancellationToken);
        ReportSummary(context, hops, progress);
    }

    private static void ReportTargetStatus(DiagnosticContext context, IPAddress target, IReadOnlyList<HopInfo> hops, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        var targetHop = hops.LastOrDefault(h => h.Address.Equals(target)) ?? hops.LastOrDefault();
        if (IsRespondingTarget(targetHop, target))
        {
            SetStatus(targetHop!, Strings.Status.Responds, hops, stats);
            DiagnosticReporter.Report(context, progress, Strings.Diag.RemoteResponds);
            return;
        }
        if (targetHop is not null) SetStatus(targetHop, Strings.Status.NoAnswer, hops, stats);
        DiagnosticReporter.Report(context, progress, Strings.Diag.RemoteNotResponse, isWarn: true);
    }

    private static bool IsRespondingTarget(HopInfo? hop, IPAddress target) =>
        hop is not null && hop.Address.Equals(target) && hop.Responded;

    private static void SetStatus(HopInfo hop, string status, IReadOnlyList<HopInfo> hops, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        hop.Status = status;
        DiagnosticReporter.ReportStats(stats, hops);
    }

    private static async Task AnalyzeLossesAsync(DiagnosticContext context, IReadOnlyList<HopInfo> hops, int verifyPacketSize, int attempts, int lossDelayMs, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        for (var i = 0; i < hops.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var hop = hops[i];
            if (!hop.HasLoss) continue;
            await AnalyzeHopLossAsync(context, hops, hop, i, verifyPacketSize, attempts, lossDelayMs, progress, stats, cancellationToken);
        }
        if (hops.All(h => !h.HasLoss)) DiagnosticReporter.Report(context, progress, Strings.Diag.NoLossDetected);
    }

    private static async Task AnalyzeHopLossAsync(DiagnosticContext context, IReadOnlyList<HopInfo> hops, HopInfo hop, int index, int verifyPacketSize, int attempts, int lossDelayMs, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        if (!hop.Responded) { MarkNoAnswer(context, hop, hops, progress, stats); return; }
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.LossDetectedFormat, hop.Ttl, DiagnosticText.Hop(hop), hop.Lost, hop.Sent), isWarn: true);
        if (index == 0) { MarkFirstHopLoss(context, hop, hops, progress, stats); return; }
        await CompareWithPreviousAsync(context, hops, index, verifyPacketSize, attempts, lossDelayMs, progress, stats, cancellationToken);
    }

    private static void MarkNoAnswer(DiagnosticContext context, HopInfo hop, IReadOnlyList<HopInfo> hops, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        SetStatus(hop, Strings.Status.NoAnswer, hops, stats);
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.HopNoResponseAnalysisFormat, hop.Ttl, DiagnosticText.Hop(hop)), isWarn: true);
    }

    private static void MarkFirstHopLoss(DiagnosticContext context, HopInfo hop, IReadOnlyList<HopInfo> hops, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        SetStatus(hop, Strings.Status.FirstHopLoss, hops, stats);
        DiagnosticReporter.Report(context, progress, Strings.Diag.FirstHopLossStatus, isWarn: true);
    }

    private static async Task CompareWithPreviousAsync(DiagnosticContext context, IReadOnlyList<HopInfo> hops, int index, int verifyPacketSize, int attempts, int lossDelayMs, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        var previous = FindPreviousHop(hops, index);
        var hop = hops[index];
        if (previous is null) { MarkNoPrevious(context, hops, hop, progress, stats); return; }
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.VerifyingPreviousFormat, previous.Ttl, DiagnosticText.Hop(previous)));
        var limit = Math.Min(attempts, DiagnosticConstants.VerifyAttemptLimit);
        var previousLoss = await LossMeasurement.MeasureAsync(context, previous, hops, verifyPacketSize, limit, lossDelayMs, progress, stats, cancellationToken);
        var currentLoss = await LossMeasurement.MeasureAsync(context, hop, hops, verifyPacketSize, limit, lossDelayMs, progress, stats, cancellationToken);
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.PreviousCheckFormat, previousLoss.Lost, previousLoss.Sent, previousLoss.Percent));
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.CurrentCheckFormat, currentLoss.Lost, currentLoss.Sent, currentLoss.Percent));
        ApplyComparison(context, hops, hop, previous, previousLoss, currentLoss, progress, stats);
    }

    private static HopInfo? FindPreviousHop(IReadOnlyList<HopInfo> hops, int index)
    {
        for (var i = index - 1; i >= 0; i--)
        {
            var candidate = hops[i];
            if (candidate.Responded && NetworkAddressPolicy.IsProbeTarget(candidate.Address)) return candidate;
        }
        return null;
    }

    private static void MarkNoPrevious(DiagnosticContext context, IReadOnlyList<HopInfo> hops, HopInfo hop, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        SetStatus(hop, Strings.Status.NoPreviousHop, hops, stats);
        DiagnosticReporter.Report(context, progress, Strings.Diag.NoPreviousHopStatus, isWarn: true);
    }

    private static void ApplyComparison(DiagnosticContext context, IReadOnlyList<HopInfo> hops, HopInfo hop, HopInfo previous, LossSample previousLoss, LossSample currentLoss, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        if (previousLoss.Lost == 0 && currentLoss.Lost > 0)
        {
            SetStatus(hop, Strings.Status.DamagedLoss, hops, stats);
            DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.DamagedStatusFormat, DiagnosticText.Hop(previous), DiagnosticText.Hop(hop)), isWarn: true);
            return;
        }
        if (previousLoss.Lost > 0) { SetStatus(hop, Strings.Status.PreviousAlsoLoss, hops, stats); DiagnosticReporter.Report(context, progress, Strings.Diag.PreviousLossStatus, isWarn: true); return; }
        SetStatus(hop, Strings.Status.LossNotConfirmed, hops, stats);
        DiagnosticReporter.Report(context, progress, Strings.Diag.TransientLossStatus, isWarn: true);
    }

    private static void ReportSummary(DiagnosticContext context, IReadOnlyList<HopInfo> hops, IProgress<string> progress)
    {
        DiagnosticReporter.Separator(context, progress);
        DiagnosticReporter.Report(context, progress, Strings.Diag.Summary);
        foreach (var hop in hops)
            DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.SummaryRowFormat, hop.Ttl, DiagnosticText.Hop(hop), hop.Sent, hop.Received, hop.Lost, hop.LossPercent));
    }
}
