using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Runs the DF ladder before the main test without changing counters.</summary>
internal sealed class FragmentationPrecheckController(
    IProbeJobFactory factory,
    IProbeJobRunner runner) : IFragmentationPrecheckController
{
    private readonly IProbeTrigger _trigger = new DontFragmentProbeTrigger();

    public async Task RunAsync(DiagnosticContext context, IReadOnlyList<HopInfo> hops,
        IProgress<string> progress, CancellationToken cancellationToken)
    {
        ReportHeader(context, progress);
        foreach (var hop in hops)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var size in ProbeLimits.PrecheckPacketSizes)
                await CheckAsync(context, hop, size, progress, cancellationToken);
        }
    }

    private void ReportHeader(DiagnosticContext context, IProgress<string> progress) =>
        DiagnosticReporter.Report(context, progress, TextFormatter.Fmt(
            Strings.Diag.PrecheckHeaderFormat,
            string.Join(Strings.Diag.PacketSizeListSeparator, ProbeLimits.PrecheckPacketSizes)));

    private async Task CheckAsync(DiagnosticContext context, HopInfo hop, int size,
        IProgress<string> progress, CancellationToken cancellationToken)
    {
        var job = factory.Create(_trigger, hop.Address, DiagnosticConstants.DirectProbeTtl,
            size, DiagnosticConstants.ProbeTimeoutMs, CreateContext(hop, size));
        var result = await runner.RunAsync(context, job, progress, cancellationToken);
        Report(context, hop, size, result, progress);
    }

    private static string CreateContext(HopInfo hop, int size) =>
        TextFormatter.Fmt(Strings.Diag.PrecheckContextFormat, hop.Ttl, hop.Address, size);

    private static void Report(DiagnosticContext context, HopInfo hop, int size,
        ProbeResult result, IProgress<string> progress)
    {
        var message = result.Outcome == ProbeOutcome.Success
            ? TextFormatter.Fmt(Strings.Diag.PrecheckSuccessFormat, hop.Ttl, DiagnosticText.Hop(hop), size,
                TextFormatter.Fmt(Strings.Diag.MsFormat, result.RoundtripTimeMs))
            : TextFormatter.Fmt(Strings.Diag.PrecheckFailureFormat, hop.Ttl, DiagnosticText.Hop(hop), size,
                Strings.Diag.NoAnswer);
        DiagnosticReporter.Report(context, progress, message, isWarn: result.Outcome != ProbeOutcome.Success);
    }
}
