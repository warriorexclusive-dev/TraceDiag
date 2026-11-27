using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Final target status and summary; loss causes come from the diagnosis column.</summary>
internal static class RunSummary
{
    internal static void Report(DiagnosticContext context, IPAddress target, IReadOnlyList<HopInfo> hops, IProgress<string> progress)
    {
        ReportTargetStatus(context, target, hops, progress);
        ReportLossHint(context, hops, progress);
        ReportRows(context, hops, progress);
    }

    private static void ReportTargetStatus(DiagnosticContext context, IPAddress target, IReadOnlyList<HopInfo> hops, IProgress<string> progress)
    {
        var targetHop = hops.LastOrDefault(h => h.Address.Equals(target)) ?? hops.LastOrDefault();
        if (IsRespondingTarget(targetHop, target))
        {
            targetHop!.Status = Strings.Status.Responds;
            DiagnosticReporter.Report(context, progress, Strings.Diag.RemoteResponds);
            return;
        }
        if (targetHop is not null) targetHop.Status = Strings.Status.NoAnswer;
        DiagnosticReporter.Report(context, progress, Strings.Diag.RemoteNotResponse, isWarn: true);
    }

    private static bool IsRespondingTarget(HopInfo? hop, IPAddress target) =>
        hop is not null && hop.Address.Equals(target) && hop.Responded;

    private static void ReportLossHint(DiagnosticContext context, IReadOnlyList<HopInfo> hops, IProgress<string> progress)
    {
        if (hops.All(hop => !hop.HasLoss))
            DiagnosticReporter.Report(context, progress, Strings.Diag.NoLossDetected);
    }

    private static void ReportRows(DiagnosticContext context, IReadOnlyList<HopInfo> hops, IProgress<string> progress)
    {
        DiagnosticReporter.Separator(context, progress);
        DiagnosticReporter.Report(context, progress, Strings.Diag.Summary);
        foreach (var hop in hops)
            DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.SummaryRowFormat, hop.Ttl, DiagnosticText.Hop(hop), hop.Sent, hop.Received, hop.Lost, hop.LossPercent));
    }
}
