using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Runs one route-discovery probe with a hard per-request timeout.</summary>
internal static class RouteProbe
{
    internal static async Task<ProbeResult> SendAsync(DiagnosticContext context, IPAddress address,
        int ttl, int bufferSize, int timeoutMs, bool dontFragment, CancellationToken token)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        cts.CancelAfter(timeoutMs);
        try
        {
            return await context.ProbeClient.SendAsync(address, ttl, bufferSize, timeoutMs, cts.Token, dontFragment);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            return Result(ProbeOutcome.TimedOut, ttl, bufferSize);
        }
        catch (Exception) { return Result(ProbeOutcome.Failed, ttl, bufferSize); }
    }

    private static ProbeResult Result(ProbeOutcome outcome, int ttl, int size) =>
        new(outcome, null, 0, Math.Max(0, size), ttl);
}
