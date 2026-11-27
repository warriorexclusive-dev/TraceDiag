using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Monitoring;

public sealed class MonitoringService : IMonitoringService
{
    private readonly DiagnosticContext _context;
    private readonly IMonitorController _controller;

    public MonitoringService(IIcmpProbeClient probeClient, IDnsResolverService dnsResolver, IDiagnosticLogger? logger = null)
    {
        _context = new DiagnosticContext(probeClient, dnsResolver, logger);
        _controller = CreateController();
    }

    public async Task RunAsync(MonitoringInput input, IProgress<string> progress,
        IProgress<HopStatSnapshot> stats, CancellationToken cancellationToken)
    {
        Validate(input, progress, stats);
        ReportStart(progress);
        try { await _controller.RunAsync(_context, input, progress, stats, cancellationToken); }
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
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(stats);
    }

    private void ReportStart(IProgress<string> progress) =>
        DiagnosticReporter.Report(_context, progress, Strings.Diag.MonitorStarted);

    private void ReportCancelled(IProgress<string> progress) =>
        DiagnosticReporter.Report(_context, progress, Strings.Diag.MonitorCancelled, isWarn: true);

    private void ReportFailure(IProgress<string> progress, Exception ex)
    {
        _context.Logger?.Error(Strings.Diag.RunFailed, ex);
        DiagnosticReporter.Report(_context, progress, TextFormatter.Fmt(Strings.Diag.FatalErrorFormat, ex.GetType().Name, ex.Message));
    }
}
