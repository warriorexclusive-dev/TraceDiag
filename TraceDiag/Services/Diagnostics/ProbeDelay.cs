using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class ProbeDelay
{
    internal static async Task AfterReplyAsync(ProbeResult result, int delayMs, CancellationToken cancellationToken)
    {
        if (delayMs <= 0 || !HasReply(result)) return;
        await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
    }

    private static bool HasReply(ProbeResult result) =>
        result.Outcome is ProbeOutcome.Success or ProbeOutcome.TtlExpired;
}
