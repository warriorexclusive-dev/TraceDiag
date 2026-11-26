using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class HopProber
{
    internal static async Task<bool> ProbeAsync(DiagnosticContext context, HopInfo hop, IReadOnlyList<HopInfo> allHops, ProbeSettings settings, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        var state = new ProbeState(hop, settings.Attempts);
        ReportStart(context, hop, state, settings, progress);
        for (var i = 0; i < settings.Attempts; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await ProcessAttemptAsync(context, hop, allHops, state, settings, i, progress, stats, cancellationToken)) return true;
        }
        return Complete(context, hop, allHops, state, settings, progress, stats);
    }

    private static void ReportStart(DiagnosticContext context, HopInfo hop, ProbeState state, ProbeSettings settings, IProgress<string> progress) =>
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.ProbingFormat, hop.Ttl, hop.Address, state.NameSuffix, settings.PacketSize, settings.Attempts));

    private static async Task<bool> ProcessAttemptAsync(DiagnosticContext context, HopInfo hop, IReadOnlyList<HopInfo> allHops, ProbeState state, ProbeSettings settings, int index, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        var result = await SendAttemptAsync(context, hop, settings, index, progress, cancellationToken);
        hop.ApplyPing(result.Success, result.RttMs);
        state.ConsecutiveLoss = result.Success ? 0 : state.ConsecutiveLoss + 1;
        DiagnosticReporter.ReportStats(stats, allHops);
        ReportProgress(context, hop, state, index, settings.Attempts, progress);
        return StopOnLoss(context, hop, allHops, state, settings, index, progress, stats);
    }

    private static async Task<(bool Success, long RttMs)> SendAttemptAsync(DiagnosticContext context, HopInfo hop, ProbeSettings settings, int index, IProgress<string> progress, CancellationToken cancellationToken)
    {
        try
        {
            var request = new ProbeRequest(hop.Address, DiagnosticConstants.DirectProbeTtl, settings.PacketSize, DiagnosticConstants.ProbeTimeoutMs, CreateContext(hop, settings, index), DelayMs: settings.DelayMs);
            var result = await ProbeExecutor.SendAsync(context, request, progress, cancellationToken);
            return (result.Outcome == ProbeOutcome.Success, result.RoundtripTimeMs);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { ReportAttemptError(context, hop, settings, index, ex, progress); return (false, 0); }
    }

    private static string CreateContext(HopInfo hop, ProbeSettings settings, int index) =>
        TextFormatter.Fmt(Strings.Diag.ProbeContextFormat, hop.Ttl, hop.Address, settings.PacketSize, index + 1, settings.Attempts);

    private static void ReportAttemptError(DiagnosticContext context, HopInfo hop, ProbeSettings settings, int index, Exception ex, IProgress<string> progress)
    {
        context.Logger?.Error(TextFormatter.Fmt(Strings.Diag.ProbeErrorFormat, hop.Ttl, hop.Address, settings.PacketSize, index + 1), ex);
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.ProbeErrorConsoleFormat, index + 1, settings.Attempts, ex.Message), isError: true, ex: ex);
    }

    private static void ReportProgress(DiagnosticContext context, HopInfo hop, ProbeState state, int index, int attempts, IProgress<string> progress)
    {
        if ((index + 1) % state.ProgressStep != 0 || index + 1 >= attempts) return;
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(
            Strings.Diag.ProgressFormat,
            index + 1,
            attempts,
            DiagnosticText.Last(hop),
            hop.AvgPingMs,
            DiagnosticText.Min(hop),
            DiagnosticText.Max(hop),
            hop.Received,
            hop.Lost));
    }

    private static bool StopOnLoss(DiagnosticContext context, HopInfo hop, IReadOnlyList<HopInfo> allHops, ProbeState state, ProbeSettings settings, int index, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        if (state.ConsecutiveLoss < DiagnosticConstants.MaxLostPingsBeforeSkip) return false;
        MarkStopped(hop, allHops, stats);
        var warning = TextFormatter.Fmt(Strings.Diag.StopWarningFirstFormat, hop.Ttl, hop.Address, settings.PacketSize, index + 1);
        ReportWarning(context, warning, progress);
        return true;
    }

    private static void MarkStopped(HopInfo hop, IReadOnlyList<HopInfo> allHops, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        hop.Status = Strings.Status.NoAnswer;
        DiagnosticReporter.ReportStats(stats, allHops);
    }

    private static void ReportWarning(DiagnosticContext context, string warning, IProgress<string> progress)
    {
        context.Logger?.Warn(warning);
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.ConsoleWarningFormat, warning), isWarn: true);
    }

    private static bool Complete(DiagnosticContext context, HopInfo hop, IReadOnlyList<HopInfo> allHops, ProbeState state, ProbeSettings settings, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        var sent = hop.Sent - state.SentBefore;
        var received = hop.Received - state.ReceivedBefore;
        if (received == 0 && sent > 0)
        {
            MarkStopped(hop, allHops, stats);
            var warning = TextFormatter.Fmt(Strings.Diag.StopWarningAllFormat, hop.Ttl, hop.Address, settings.PacketSize, sent);
            ReportWarning(context, warning, progress);
            return true;
        }
        ReportSummary(context, hop, state, settings, sent, received, progress);
        return false;
    }

    private static void ReportSummary(DiagnosticContext context, HopInfo hop, ProbeState state, ProbeSettings settings, int sent, int received, IProgress<string> progress)
    {
        var lost = sent - received;
        var percent = sent == 0 ? 0 : lost * 100.0 / sent;
        var avg = received == 0
            ? Strings.Diag.Dash
            : DiagnosticText.Ms((double)(hop.TotalRttMs - state.RttBefore) / received);
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.HopSummaryFormat, hop.Ttl, hop.Address, state.NameSuffix, settings.PacketSize, sent, received, lost, percent, avg));
    }
}
