namespace TraceDiag.Services.Diagnostics;

internal static class ProbeDelay
{
    internal static Task ApplyAsync(bool enabled, int lossDelayMs, CancellationToken cancellationToken) =>
        enabled && lossDelayMs > 0
            ? Task.Delay(lossDelayMs, cancellationToken)
            : Task.CompletedTask;
}
