using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Monitoring;

/// <summary>One independent periodic monitoring task driven by a probe job.</summary>
internal sealed class MonitorHopTask(
    IProbeJobFactory factory,
    IProbeJobRunner runner,
    IRouteStabilityController? stability = null) : IMonitorHopTask
{
    private readonly IRouteStabilityController _stability = stability ?? new RouteStabilityController();

    public async Task RunAsync(DiagnosticContext context, HopInfo hop, int packetSize,
        IProbeTrigger trigger, IProgress<string> progress, IProgress<HopStatSnapshot> stats,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await CheckAsync(context, hop, packetSize, trigger, progress, stats, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(DiagnosticConstants.MonitorIntervalMs, cancellationToken);
        }
    }

    private async Task CheckAsync(DiagnosticContext context, HopInfo hop, int packetSize,
        IProbeTrigger trigger, IProgress<string> progress, IProgress<HopStatSnapshot> stats,
        CancellationToken cancellationToken)
    {
        var job = factory.Create(trigger, hop.Address, DiagnosticConstants.DirectProbeTtl, packetSize,
            DiagnosticConstants.ProbeTimeoutMs,
            TextFormatter.Fmt(Strings.Diag.MonitorContextFormat, hop.Ttl, hop.Address));
        var result = await runner.RunAsync(context, job, progress, cancellationToken);
        hop.RecordOutcome(result.Outcome);
        Apply(hop, result, context, progress, stats);
    }

    private void Apply(HopInfo hop, ProbeResult result, DiagnosticContext context,
        IProgress<string> progress, IProgress<HopStatSnapshot> stats)
    {
        var success = result.Outcome == ProbeOutcome.Success;
        hop.ApplyPing(success, result.RoundtripTimeMs, result.Address);
        _stability.Apply(hop);
        stats.Report(DiagnosticReporter.Snapshot(hop));
        Report(context, hop, result, success, progress);
    }

    private static void Report(DiagnosticContext context, HopInfo hop, ProbeResult result, bool success,
        IProgress<string> progress)
    {
        var rtt = success
            ? DiagnosticText.MsArg(result.RoundtripTimeMs)
            : LocalizedArg.Of(Strings.Diag.NoAnswer);
        var args = new object?[] { hop.Ttl, DiagnosticText.HopArg(hop), result.BufferSize, rtt, hop.Sent, hop.Received };
        if (success) DiagnosticReporter.ReportFormat(context, progress, Strings.Diag.MonitorCycleFormat, args);
        else DiagnosticReporter.ReportFormatWarn(context, progress, Strings.Diag.MonitorCycleFormat, args);
    }
}
