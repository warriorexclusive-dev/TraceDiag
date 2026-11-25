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
        var hops = await DiscoverAndResolveAsync(input, sizes[0], progress, statsProgress, cancellationToken);
        if (hops.Count == 0) return;
        await DiagnosticBatch.ProbeAllAsync(_context, sizes, hops, input.Attempts, progress, statsProgress, cancellationToken);
        DiagnosticReporter.Separator(_context, progress);
        await LossAnalyzer.AnalyzeAsync(_context, input.RemoteAddress, hops, sizes[^1], input.Attempts, progress, statsProgress, cancellationToken);
        DiagnosticReporter.ReportStats(statsProgress, hops);
        DiagnosticReporter.Report(_context, progress, Strings.Diag.RunFinished);
    }

    private async Task<List<HopInfo>> DiscoverAndResolveAsync(DiagnosticInput input, int discoveryPacketSize, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> statsProgress, CancellationToken cancellationToken)
    {
        var hops = await RouteDiscovery.DiscoverAsync(_context, input.RemoteAddress, discoveryPacketSize, progress, statsProgress, cancellationToken);
        if (hops.Count == 0) { DiagnosticReporter.Report(_context, progress, Strings.Diag.NoRouteHops, isWarn: true); return hops; }
        await HopNameResolver.ResolveAsync(_context, hops, progress, statsProgress, cancellationToken);
        DiagnosticReporter.ReportStats(statsProgress, hops);
        DiagnosticReporter.Separator(_context, progress);
        return hops;
    }

    private void ReportFailure(IProgress<string> progress, Exception ex)
    {
        _context.Logger?.Error(Strings.Diag.RunFailed, ex);
        DiagnosticReporter.Report(_context, progress, TextFormatter.Fmt(Strings.Diag.FatalErrorFormat, ex.GetType().Name, ex.Message), isError: true, ex: ex);
    }
}
