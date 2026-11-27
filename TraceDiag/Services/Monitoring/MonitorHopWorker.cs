using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Monitoring;

/// <summary>Compatibility facade for the monitoring hop task.</summary>
internal static class MonitorHopWorker
{
    private static readonly IMonitorHopTask TaskImpl =
        new MonitorHopTask(new ProbeJobFactory(), new ProbeJobRunner());

    internal static IMonitorHopTask Default => TaskImpl;

    internal static Task RunAsync(DiagnosticContext context, HopInfo hop, int packetSize,
        IProgress<string> progress, IProgress<HopStatSnapshot> stats, CancellationToken cancellationToken) =>
        RunAsync(context, hop, packetSize, new DefaultProbeTrigger(), progress, stats, cancellationToken);

    internal static Task RunAsync(DiagnosticContext context, HopInfo hop, int packetSize,
        IProbeTrigger trigger, IProgress<string> progress, IProgress<HopStatSnapshot> stats,
        CancellationToken cancellationToken) =>
        TaskImpl.RunAsync(context, hop, packetSize, trigger, progress, stats, cancellationToken);
}
