using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class ProbeExecutor
{
    internal static async Task<ProbeResult> SendAsync(DiagnosticContext context, ProbeRequest request, IProgress<string> progress, CancellationToken cancellationToken)
    {
        try { return await ExecuteAsync(context, request, progress, cancellationToken).ConfigureAwait(false); }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return HandleFailure(context, request, progress, ex); }
    }

    private static async Task<ProbeResult> ExecuteAsync(DiagnosticContext context, ProbeRequest request, IProgress<string> progress, CancellationToken cancellationToken)
    {
        var probeTask = context.ProbeClient.SendAsync(request.Address, request.Ttl, request.BufferSize, request.TimeoutMs, cancellationToken);
        var watchdog = Task.Delay(request.TimeoutMs + DiagnosticConstants.WatchdogExtraDelayMs, cancellationToken);
        var completed = await Task.WhenAny(probeTask, watchdog).ConfigureAwait(false);
        return completed == probeTask
            ? await probeTask.ConfigureAwait(false)
            : HandleTimeout(context, request, progress);
    }

    private static ProbeResult HandleTimeout(DiagnosticContext context, ProbeRequest request, IProgress<string> progress)
    {
        var message = TextFormatter.Fmt(Strings.Diag.WatchdogFormat, request.Context);
        context.Logger?.Warn(message);
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.ConsoleWarningFormat, message), isWarn: true);
        return TimedOut(request);
    }

    private static ProbeResult HandleFailure(DiagnosticContext context, ProbeRequest request, IProgress<string> progress, Exception ex)
    {
        context.Logger?.Error(TextFormatter.Fmt(Strings.Diag.ProbeSendFailedFormat, request.Context), ex);
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.ProbeSendFailedConsoleFormat, request.Context, ex.Message), isError: true, ex: ex);
        return Failed(request);
    }

    private static ProbeResult TimedOut(ProbeRequest request) =>
        new(ProbeOutcome.TimedOut, null, 0, Math.Max(0, request.BufferSize), request.Ttl);

    private static ProbeResult Failed(ProbeRequest request) =>
        new(ProbeOutcome.Failed, null, 0, Math.Max(0, request.BufferSize), request.Ttl);
}
