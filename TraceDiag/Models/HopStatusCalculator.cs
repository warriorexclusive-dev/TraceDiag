namespace TraceDiag.Models;

internal static class HopStatusCalculator
{
    internal static string Calculate(int received, int lost, double lossPercent)
    {
        if (received == 0) return Strings.Status.NoAnswer;
        if (lost == 0) return Strings.Status.Normal;
        return TextFormatter.Fmt(Strings.Status.PacketLossFormat, lossPercent);
    }
}
