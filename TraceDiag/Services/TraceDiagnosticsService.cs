using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services;

public sealed class TraceDiagnosticsService : ITraceDiagnosticsService
{
    private readonly DiagnosticContext _context;

    public TraceDiagnosticsService(IIcmpProbeClient probeClient, IDnsResolverService dnsResolver, IDiagnosticLogger? logger = null)
    {
        _context = new DiagnosticContext(probeClient, dnsResolver, logger);
    }

    public async Task RunAsync(DiagnosticInput input, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> statsProgress, CancellationToken cancellationToken)
    {
        Validate(input, progress, statsProgress);
        ReportStart(progress);
        try { await ExecuteAsync(input, progress, statsProgress, cancellationToken); }
        catch (OperationCanceledException) { _context.Logger?.Warn(Strings.Diag.RunCancelled); throw; }
        catch (Exception ex) { ReportFailure(progress, ex); throw; }
    }

    private static void Validate(DiagnosticInput input, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> statsProgress)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(statsProgress);
    }

    private void ReportStart(IProgress<string> progress)
    {
        DiagnosticReporter.Report(_context, progress, Strings.Diag.RunStarted);
        if (_context.Logger is null) return;
        DiagnosticReporter.Report(_context, progress, TextFormatter.Fmt(Strings.Diag.LogFileFormat, GetLogPathHint()));
    }

    private string GetLogPathHint() =>
        _context.Logger is FileDiagnosticLogger fileLogger ? fileLogger.LogFilePath : Strings.Diag.LogEnabledHint;

    private async Task ExecuteAsync(DiagnosticInput input, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> statsProgress, CancellationToken cancellationToken)
    {
        var sizes = PacketSizePlanner.Build(input.MaxPacketSize, input.DynamicPacketSize);
        DiagnosticReporter.ReportHeader(_context, progress, input, sizes);
        var discovery = await DiscoverAndResolveAsync(input, DiscoverySettings.CreateDefault(input.LossDelayMs), progress, statsProgress, cancellationToken);
        if (discovery.Hops.Count == 0) return;
        await DiagnosticBatch.ProbeAllAsync(_context, sizes, discovery.Hops, discovery.SkippedTtls, input.Attempts, input.LossDelayMs, progress, statsProgress, cancellationToken);
        DiagnosticReporter.Separator(_context, progress);
        RunSummary.Report(_context, input.RemoteAddress, discovery.Hops, progress);
        DiagnosticReporter.ReportStats(statsProgress, discovery.Hops);
        DiagnosticReporter.Report(_context, progress, Strings.Diag.RunFinished);
    }

    private async Task<HostDiscoveryResult> DiscoverAndResolveAsync(DiagnosticInput input, DiscoverySettings settings, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> statsProgress, CancellationToken cancellationToken)
    {
        var result = await HostDiscovery.DiscoverAsync(_context, input.RemoteAddress, settings, progress, statsProgress, cancellationToken);
        if (result.Hops.Count == 0) { DiagnosticReporter.Report(_context, progress, Strings.Diag.NoRouteHops, isWarn: true); return result; }
        await HopNameResolver.ResolveAsync(_context, result.Hops, progress, statsProgress, cancellationToken);
        DiagnosticReporter.ReportStats(statsProgress, result.Hops);
        DiagnosticReporter.Separator(_context, progress);
        return result;
    }

    private void ReportFailure(IProgress<string> progress, Exception ex)
    {
        _context.Logger?.Error(Strings.Diag.RunFailed, ex);
        DiagnosticReporter.Report(_context, progress, TextFormatter.Fmt(Strings.Diag.FatalErrorFormat, ex.GetType().Name, ex.Message));
    }
}
