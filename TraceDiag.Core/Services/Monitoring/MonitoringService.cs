using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Monitoring;

public sealed class MonitoringService : IMonitoringService
{
    private readonly DiagnosticContext _context;
    private readonly IMonitorController _controller;
    private readonly RouteRunCoordinator _routes;
    private readonly IRipeOwnerService? _ripeOwner;

    public MonitoringService(IIcmpProbeClient probeClient, IDnsResolverService dnsResolver,
        IDiagnosticLogger? logger = null, IRipeOwnerService? ripeOwner = null)
    {
        _context = new DiagnosticContext(probeClient, dnsResolver, logger,
            DnsAnswers: new DnsAnswerService());
        _controller = CreateController();
        _routes = new RouteRunCoordinator(null!, _controller, new AlternateRouteDetector());
        _ripeOwner = ripeOwner;
    }

    public async Task RunAsync(MonitoringInput input, IProgress<string> progress,
        IProgress<HopStatSnapshot> stats, CancellationToken cancellationToken)
    {
        Validate(input, progress, stats);
        ReportStart(progress);
        var context = _context with { RipeLookupEnabled = input.RipeLookup, RipeOwner = _ripeOwner };
        try { await _routes.RunMonitorAsync(context, input, progress, stats, cancellationToken); }
        catch (OperationCanceledException) { ReportCancelled(progress); throw; }
        catch (Exception ex) { ReportFailure(progress, ex); throw; }
    }

    private static IMonitorController CreateController()
    {
        var factory = new ProbeJobFactory();
        var runner = new ProbeJobRunner();
        var precheck = new FragmentationPrecheckController(factory, runner);
        var hopTask = new MonitorHopTask(factory, runner);
        return new MonitorController(factory, runner, precheck, hopTask);
    }

    private static void Validate(MonitoringInput input, IProgress<string> progress, IProgress<HopStatSnapshot> stats)
    {
        Compat.ThrowIfNull(input, nameof(input));
        Compat.ThrowIfNull(progress, nameof(progress));
        Compat.ThrowIfNull(stats, nameof(stats));
    }

    private void ReportStart(IProgress<string> progress) =>
        DiagnosticReporter.Report(_context, progress, Strings.Diag.MonitorStarted);

    private void ReportCancelled(IProgress<string> progress) =>
        DiagnosticReporter.ReportFormatWarn(_context, progress, Strings.Diag.MonitorCancelled);

    private void ReportFailure(IProgress<string> progress, Exception ex)
    {
        _context.Logger.ErrorFmt(Strings.Diag.RunFailed, ex);
        DiagnosticReporter.ReportFormat(_context, progress, Strings.Diag.FatalErrorFormat,
            ex.GetType().Name, ex.Message);
    }
}
