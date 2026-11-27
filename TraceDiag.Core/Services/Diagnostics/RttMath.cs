namespace TraceDiag.Services.Diagnostics;

/// <summary>Variability metrics calculated from raw RTT samples.</summary>
internal static class RttMath
{
    /// <summary>Mean absolute difference between consecutive successful samples.</summary>
    internal static double Jitter(IReadOnlyList<long> samples)
    {
        if (samples.Count < 2) return 0;
        double sum = 0;
        for (var i = 1; i < samples.Count; i++) sum += Math.Abs(samples[i] - samples[i - 1]);
        return sum / (samples.Count - 1);
    }

    /// <summary>Population standard deviation of all successful samples.</summary>
    internal static double StdDev(IReadOnlyList<long> samples)
    {
        if (samples.Count == 0) return 0;
        var mean = Mean(samples);
        double sum = 0;
        foreach (var value in samples) sum += (value - mean) * (value - mean);
        return Math.Sqrt(sum / samples.Count);
    }

    private static double Mean(IReadOnlyList<long> samples)
    {
        long sum = 0;
        foreach (var value in samples) sum += value;
        return (double)sum / samples.Count;
    }
}
