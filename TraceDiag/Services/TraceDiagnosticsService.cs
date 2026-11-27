using TraceDiag.Models;
using TraceDiag.Services.Abstractions;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services;

public sealed class TraceDiagnosticsService : ITraceDiagnosticsService
{
    private readonly DiagnosticContext _context;
    private readonly IFiniteCheckController _controller;

    public TraceDiagnosticsService(IIcmpProbeClient probeClient, IDnsResolverService dnsResolver, IDiagnosticLogger? logger = null)
    {
        _context = new DiagnosticContext(probeClient, dnsResolver, logger);
        _controller = CreateController();
    }

    public async Task RunAsync(DiagnosticInput input, IProgress<string> progress, IProgress<IReadOnlyList<HopStatSnapshot>> statsProgress, CancellationToken cancellationToken)
    {
        Validate(input, progress, statsProgress);
        ReportStart(progress);
        try { await _controller.RunAsync(_context, input, progress, statsProgress, cancellationToken); }
        catch (OperationCanceledException) { _context.Logger?.Warn(Strings.Diag.RunCancelled); throw; }
        catch (Exception ex) { ReportFailure(progress, ex); throw; }
    }

    private static IFiniteCheckController CreateController()
    {
        var factory = new ProbeJobFactory();
        var runner = new ProbeJobRunner();
        var precheck = new FragmentationPrecheckController(factory, runner);
        var hopTask = new FiniteHopTask(factory, runner);
        return new FiniteCheckController(factory, runner, precheck, hopTask);
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

    private void ReportFailure(IProgress<string> progress, Exception ex)
    {
        _context.Logger?.Error(Strings.Diag.RunFailed, ex);
        DiagnosticReporter.Report(_context, progress, TextFormatter.Fmt(Strings.Diag.FatalErrorFormat, ex.GetType().Name, ex.Message));
    }
}
