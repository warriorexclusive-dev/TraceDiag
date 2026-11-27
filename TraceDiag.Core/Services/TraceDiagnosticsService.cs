using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services;

public sealed class TraceDiagnosticsService : ITraceDiagnosticsService
{
    private readonly DiagnosticContext _context;
    private readonly IFiniteCheckController _controller;
    private readonly RouteRunCoordinator _routes;
    private readonly IRipeOwnerService? _ripeOwner;

    public TraceDiagnosticsService(IIcmpProbeClient probeClient, IDnsResolverService dnsResolver,
        IDiagnosticLogger? logger = null, IRipeOwnerService? ripeOwner = null)
    {
        _context = new DiagnosticContext(probeClient, dnsResolver, logger,
            DnsAnswers: new DnsAnswerService());
        _controller = CreateController();
        _routes = new RouteRunCoordinator(_controller, null!, new AlternateRouteDetector());
        _ripeOwner = ripeOwner;
    }

    public async Task RunAsync(DiagnosticInput input, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> statsProgress, CancellationToken cancellationToken)
    {
        Validate(input, progress, statsProgress);
        ReportStart(progress);
        var context = _context with { RipeLookupEnabled = input.RipeLookup, RipeOwner = _ripeOwner };
        try { await _routes.RunFiniteAsync(context, input, progress, statsProgress, cancellationToken); }
        catch (OperationCanceledException) { _context.Logger.WarnFmt(Strings.Diag.RunCancelled); throw; }
        catch (Exception ex) { ReportFailure(progress, ex); throw; }
    }

    private static IFiniteCheckController CreateController()
    {
        var factory = new ProbeJobFactory();
        var runner = new ProbeJobRunner();
        var precheck = new FragmentationPrecheckController(factory, runner);
        var hopTask = new FiniteHopTask(factory, runner);
        var stability = new RouteStabilityController();
        return new FiniteCheckController(factory, runner, precheck, hopTask, stability);
    }

    private static void Validate(DiagnosticInput input, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> statsProgress)
    {
        Compat.ThrowIfNull(input, nameof(input));
        Compat.ThrowIfNull(progress, nameof(progress));
        Compat.ThrowIfNull(statsProgress, nameof(statsProgress));
    }

    private void ReportStart(IProgress<string> progress)
    {
        DiagnosticReporter.Report(_context, progress, Strings.Diag.RunStarted);
        if (_context.Logger is null) return;
        DiagnosticReporter.ReportFormat(_context, progress, Strings.Diag.LogFileFormat, GetLogPathHint());
    }

    private string GetLogPathHint() =>
        _context.Logger is ILogPathProvider provider ? provider.LogFilePath : Strings.Diag.LogEnabledHint;

    private void ReportFailure(IProgress<string> progress, Exception ex)
    {
        _context.Logger.ErrorFmt(Strings.Diag.RunFailed, ex);
        DiagnosticReporter.ReportFormat(_context, progress, Strings.Diag.FatalErrorFormat,
            ex.GetType().Name, ex.Message);
    }
}
