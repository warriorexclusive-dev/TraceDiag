using System.Diagnostics;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Resolves hop ASNs in parallel with bounded concurrency.</summary>
internal static class AsnResolverController
{
    private const int MaxParallel = 8;

    internal static async Task ResolveAsync(DiagnosticContext context, IReadOnlyList<HopInfo> hops,
        IProgress<string> progress, CancellationToken cancellationToken)
    {
        if (!context.AsnLookupEnabled || !context.ExternalSourcesEnabled) return;
        var targets = hops.Where(h => NetworkAddressPolicy.IsProbeTarget(h.Address)).ToArray();
        using var gate = new SemaphoreSlim(MaxParallel);
        var tasks = targets.Select(h => ResolveOneAsync(context, h, gate, progress, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private static async Task ResolveOneAsync(DiagnosticContext context, HopInfo hop, SemaphoreSlim gate,
        IProgress<string> progress, CancellationToken token)
    {
        await gate.WaitAsync(token);
        try
        {
            var sw = Stopwatch.StartNew();
            hop.Asn = await context.DnsResolver.ResolveAsnAsync(hop.Address, token) ?? string.Empty;
            ReportSlow(context, hop, sw);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { HopNameResolver.ReportDnsError(context, hop, ex, progress); }
        finally { gate.Release(); }
    }

    private static void ReportSlow(DiagnosticContext context, HopInfo hop, Stopwatch sw)
    {
        if (sw.ElapsedMilliseconds <= DiagnosticConstants.SlowDnsThresholdMs) return;
        context.Logger.WarnFmt(Strings.Diag.SlowDnsFormat, hop.Address, sw.ElapsedMilliseconds);
    }
}
