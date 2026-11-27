using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class RttMathTests
{
    [Fact]
    public void Jitter_IsMeanAbsoluteChangeBetweenSamples()
    {
        var hop = TestFactories.Hop(1, 1);
        foreach (var rtt in new long[] { 10, 14, 11 }) hop.ApplyPing(true, rtt);
        Assert.Equal((4.0 + 3) / 2, hop.JitterMs, 10);
    }

    [Fact]
    public void StdDev_IsPopulationDeviation()
    {
        var hop = TestFactories.Hop(1, 1);
        foreach (var rtt in new long[] { 2, 4, 4, 4, 5, 5, 7, 9 }) hop.ApplyPing(true, rtt);
        Assert.Equal(2, hop.StdDevMs, 10);
    }

    [Fact]
    public void Metrics_WithoutEnoughSamples_AreZero()
    {
        var hop = TestFactories.Hop(1, 1);
        Assert.Equal(0, hop.JitterMs);
        Assert.Equal(0, hop.StdDevMs);
        hop.ApplyPing(true, 7);
        Assert.Equal(0, hop.JitterMs);
        Assert.Equal(0, hop.StdDevMs);
    }

    [Fact]
    public void Snapshot_CarriesVariabilityMetrics()
    {
        var hop = TestFactories.Hop(1, 1);
        foreach (var rtt in new long[] { 10, 14, 11 }) hop.ApplyPing(true, rtt);
        var snapshot = DiagnosticReporter.Snapshot(hop);
        Assert.Equal(hop.JitterMs, snapshot.JitterMs, 10);
        Assert.Equal(hop.StdDevMs, snapshot.StdDevMs, 10);
    }
}
