using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal sealed record LossSample(int Sent, int Lost, double Percent);

internal sealed record PingCounters(int Sent, int Received)
{
    internal static PingCounters Capture(HopInfo hop) => new(hop.Sent, hop.Received);

    internal LossSample Measure(HopInfo hop)
    {
        var sent = hop.Sent - Sent;
        var received = hop.Received - Received;
        var lost = sent - received;
        var percent = sent == 0 ? 0 : lost * 100.0 / sent;
        return new LossSample(sent, lost, percent);
    }
}

internal static class LossMeasurement
{
    internal static async Task<LossSample> MeasureAsync(DiagnosticContext context, HopInfo hop, IReadOnlyList<HopInfo> allHops, int packetSize, int attempts, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> stats, CancellationToken cancellationToken)
    {
        var count = Math.Max(1, attempts);
        var probe = VerificationHop.Create(hop);
        var before = PingCounters.Capture(probe);
        for (var i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await SendVerifyAsync(context, probe, packetSize, i, count, progress, cancellationToken);
            probe.ApplyPing(result.Success, result.RttMs);
            DiagnosticReporter.ReportStats(stats, allHops);
        }
        return before.Measure(probe);
    }

    private static async Task<(bool Success, long RttMs)> SendVerifyAsync(DiagnosticContext context, HopInfo hop, int packetSize, int index, int count, IProgress<string> progress, CancellationToken cancellationToken)
    {
        try
        {
            var contextText = TextFormatter.Fmt(Strings.Diag.VerifyContextFormat, hop.Address, packetSize, index + 1, count);
            var request = new ProbeRequest(hop.Address, DiagnosticConstants.DirectProbeTtl, packetSize, DiagnosticConstants.ProbeTimeoutMs, contextText);
            var result = await ProbeExecutor.SendAsync(context, request, progress, cancellationToken);
            return (result.Outcome == ProbeOutcome.Success, result.RoundtripTimeMs);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { ReportError(context, hop, index, ex, progress); return (false, 0); }
    }

    private static void ReportError(DiagnosticContext context, HopInfo hop, int index, Exception ex, IProgress<string> progress)
    {
        context.Logger?.Error(TextFormatter.Fmt(Strings.Diag.VerifyErrorFormat, hop.Address, index + 1), ex);
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(Strings.Diag.VerifyErrorConsoleFormat, ex.Message), isError: true, ex: ex);
    }
}
