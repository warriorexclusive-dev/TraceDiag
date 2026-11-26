using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Monitoring;

public sealed class MonitoringService : IMonitoringService
{
    private readonly DiagnosticContext _context;

    public MonitoringService(IIcmpProbeClient probeClient, IDnsResolverService dnsResolver, IDiagnosticLogger? logger = null) =>
        _context = new DiagnosticContext(probeClient, dnsResolver, logger);

    public async Task RunAsync(MonitoringInput input, IProgress<string> progress,
        IProgress<HopStatSnapshot> stats, CancellationToken cancellationToken)
    {
        Validate(input, progress, stats);
        ReportStart(input, progress);
        try { await ExecuteAsync(input, progress, stats, cancellationToken); }
        catch (OperationCanceledException) { ReportCancelled(progress); throw; }
        catch (Exception ex) { ReportFailure(progress, ex); throw; }
    }

    private static void Validate(MonitoringInput input, IProgress<string> progress, IProgress<HopStatSnapshot> stats)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(stats);
    }

    private void ReportStart(MonitoringInput input, IProgress<string> progress)
    {
        DiagnosticReporter.Report(_context, progress, Strings.Diag.MonitorStarted);
        MonitoringReporter.Header(_context, progress, input);
    }

    private async Task ExecuteAsync(MonitoringInput input, IProgress<string> progress,
        IProgress<HopStatSnapshot> stats, CancellationToken cancellationToken)
    {
        var listing = new MonitoringStatsAdapter(stats);
        var discovery = await RouteDiscovery.DiscoverAsync(_context, input.RemoteAddress, input.LossDelayMs, progress, listing, cancellationToken);
        if (discovery.Hops.Count == 0) return;
        await MonitorController.RunAsync(_context, discovery.Hops, input.PacketSize, progress, stats, cancellationToken);
    }

    private void ReportCancelled(IProgress<string> progress) =>
        DiagnosticReporter.Report(_context, progress, Strings.Diag.MonitorCancelled, isWarn: true);

    private void ReportFailure(IProgress<string> progress, Exception ex)
    {
        _context.Logger?.Error(Strings.Diag.RunFailed, ex);
        DiagnosticReporter.Report(_context, progress, TextFormatter.Fmt(Strings.Diag.FatalErrorFormat, ex.GetType().Name, ex.Message));
    }
}
