using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal sealed record ProbeSettings(int PacketSize, int Attempts, int LossDelayMs);

internal sealed class ProbeState
{
    internal ProbeState(HopInfo hop, int attempts)
    {
        SentBefore = hop.Sent;
        ReceivedBefore = hop.Received;
        RttBefore = hop.TotalRttMs;
        NameSuffix = string.IsNullOrWhiteSpace(hop.HostName)
            ? string.Empty
            : TextFormatter.Fmt(Strings.Diag.NameSuffixFormat, hop.HostName);
        ProgressStep = GetProgressStep(attempts);
    }

    internal int SentBefore { get; }
    internal int ReceivedBefore { get; }
    internal long RttBefore { get; }
    internal string NameSuffix { get; }
    internal int ProgressStep { get; }
    internal int ConsecutiveLoss { get; set; }

    private static int GetProgressStep(int attempts)
    {
        if (attempts <= 20) return 5;
        if (attempts <= 100) return 10;
        if (attempts <= 300) return 25;
        return 50;
    }
}
