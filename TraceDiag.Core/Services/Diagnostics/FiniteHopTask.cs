using System.Net;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Finite per-hop probe task with attempts and stop rules.</summary>
internal sealed class FiniteHopTask(
    IProbeJobFactory factory,
    IProbeJobRunner runner) : IFiniteHopTask
{
    public async Task<bool> ProbeAsync(DiagnosticContext context, HopInfo hop,
        IReadOnlyList<HopInfo> allHops, ProbeSettings settings, IProbeTrigger trigger,
        IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken)
    {
        var state = new ProbeState(hop, settings.Attempts);
        ReportStart(context, hop, state, settings, progress);
        for (var i = 0; i < settings.Attempts; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await ProcessAttemptAsync(context, hop, allHops, state, settings, trigger, i, progress, stats, cancellationToken)) return true;
        }
        return Complete(context, hop, allHops, state, settings, progress, stats);
    }

    private static void ReportStart(DiagnosticContext context, HopInfo hop, ProbeState state,
        ProbeSettings settings, IProgress<string> progress) =>
        DiagnosticReporter.ReportFormat(context, progress, Strings.Diag.ProbingFormat,
            hop.Ttl, hop.Address, state.NameSuffix, settings.PacketSize, settings.Attempts);

    private async Task<bool> ProcessAttemptAsync(DiagnosticContext context, HopInfo hop,
        IReadOnlyList<HopInfo> allHops, ProbeState state, ProbeSettings settings, IProbeTrigger trigger,
        int index, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats,
        CancellationToken cancellationToken)
    {
        var result = await SendAttemptAsync(context, hop, settings, trigger, index, progress, cancellationToken);
        hop.ApplyPing(result.Success, result.RttMs, result.Address);
        await ProbeDelay.ApplyAsync(hop.LossDelayEnabled, settings.LossDelayMs, cancellationToken);
        state.ConsecutiveLoss = result.Success ? 0 : state.ConsecutiveLoss + 1;
        DiagnosticReporter.ReportStats(stats, allHops);
        ReportProgress(context, hop, state, index, settings.Attempts, progress);
        return StopOnLoss(context, hop, allHops, state, settings, index, progress, stats);
    }

    private async Task<(bool Success, long RttMs, IPAddress? Address)> SendAttemptAsync(DiagnosticContext context, HopInfo hop,
        ProbeSettings settings, IProbeTrigger trigger, int index, IProgress<string> progress,
        CancellationToken cancellationToken)
    {
        try
        {
            var job = CreateJob(hop, settings, trigger, index);
            var result = await runner.RunAsync(context, job, progress, cancellationToken);
            hop.RecordOutcome(result.Outcome);
            return (result.Outcome == ProbeOutcome.Success, result.RoundtripTimeMs, result.Address);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { ReportAttemptError(context, hop, settings, index, ex, progress); return (false, 0, null); }
    }

    private ProbeJob CreateJob(HopInfo hop, ProbeSettings settings, IProbeTrigger trigger, int index) =>
        factory.Create(trigger, hop.Address, DiagnosticConstants.DirectProbeTtl, settings.PacketSize,
            DiagnosticConstants.ProbeTimeoutMs, CreateContext(hop, settings, index));

    private static string CreateContext(HopInfo hop, ProbeSettings settings, int index) =>
        TextFormatter.Fmt(Strings.Diag.ProbeContextFormat, hop.Ttl, hop.Address, settings.PacketSize, index + 1, settings.Attempts);

    private static void ReportAttemptError(DiagnosticContext context, HopInfo hop, ProbeSettings settings,
        int index, Exception ex, IProgress<string> progress)
    {
        var args = new object?[] { hop.Ttl, hop.Address, settings.PacketSize, index + 1 };
        context.Logger.ErrorFmt(Strings.Diag.ProbeErrorFormat, ex, args);
        DiagnosticReporter.ReportFormatError(context, progress, Strings.Diag.ProbeErrorConsoleFormat,
            ex, index + 1, settings.Attempts, ex.Message);
    }

    private static void ReportProgress(DiagnosticContext context, HopInfo hop, ProbeState state, int index,
        int attempts, IProgress<string> progress)
    {
        if ((index + 1) % state.ProgressStep != 0 || index + 1 >= attempts) return;
        DiagnosticReporter.ReportFormat(context, progress, Strings.Diag.ProgressFormat,
            index + 1, attempts, DiagnosticText.LastArg(hop), hop.AvgPingMs,
            DiagnosticText.MinArg(hop), DiagnosticText.MaxArg(hop), hop.Received, hop.Lost);
    }

    private static bool StopOnLoss(DiagnosticContext context, HopInfo hop, IReadOnlyList<HopInfo> allHops,
        ProbeState state, ProbeSettings settings, int index, IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        if (state.ConsecutiveLoss < DiagnosticConstants.MaxLostPingsBeforeSkip) return false;
        MarkStopped(hop, allHops, stats);
        ReportWarning(context, Strings.Diag.StopWarningFirstFormat,
            [hop.Ttl, hop.Address, settings.PacketSize, index + 1], progress);
        return true;
    }

    private static void MarkStopped(HopInfo hop, IReadOnlyList<HopInfo> allHops,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        hop.Status = Strings.Status.NoAnswer;
        DiagnosticReporter.ReportStats(stats, allHops);
    }

    private static void ReportWarning(DiagnosticContext context, string canonical,
        object?[] args, IProgress<string> progress) =>
        DiagnosticReporter.ReportFormatWarn(context, progress, canonical, args);

    private static bool Complete(DiagnosticContext context, HopInfo hop, IReadOnlyList<HopInfo> allHops,
        ProbeState state, ProbeSettings settings, IProgress<string> progress,
        IProgress<IReadOnlyList<HopStatSnapshot>> stats)
    {
        var sent = hop.Sent - state.SentBefore;
        var received = hop.Received - state.ReceivedBefore;
        if (received == 0 && sent > 0)
        {
            MarkStopped(hop, allHops, stats);
            ReportWarning(context, Strings.Diag.StopWarningAllFormat,
                [hop.Ttl, hop.Address, settings.PacketSize, sent], progress);
            return true;
        }
        ReportSummary(context, hop, state, settings, sent, received, progress);
        return false;
    }

    private static void ReportSummary(DiagnosticContext context, HopInfo hop, ProbeState state,
        ProbeSettings settings, int sent, int received, IProgress<string> progress)
    {
        var lost = sent - received;
        var percent = sent == 0 ? 0 : lost * 100.0 / sent;
        var avg = received == 0
            ? LocalizedArg.Of(Strings.Diag.Dash)
            : DiagnosticText.MsArg((double)(hop.TotalRttMs - state.RttBefore) / received);
        DiagnosticReporter.ReportFormat(context, progress, Strings.Diag.HopSummaryFormat,
            hop.Ttl, hop.Address, state.NameSuffix, settings.PacketSize, sent, received, lost, percent, avg);
    }
}
