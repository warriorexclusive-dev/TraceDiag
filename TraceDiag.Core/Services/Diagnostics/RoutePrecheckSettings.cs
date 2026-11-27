namespace TraceDiag.Services.Diagnostics;

/// <summary>Limits for the optional route precheck; every query stays under the cap.</summary>
internal sealed record RoutePrecheckSettings(
    bool Enabled,
    bool Deep,
    int ProbeTimeoutMs,
    int MaxHops = 20,
    int Concurrency = 6,
    int DeadlineMs = 150000)
{
    internal int FlowCount => Deep ? 8 : 4;

    internal static RoutePrecheckSettings Create(bool enabled, bool deep, int probeTimeoutMs) =>
        new(enabled, deep, Compat.Clamp(probeTimeoutMs, 100, DiagnosticConstants.ProbeTimeoutMs));
}
