using System.Net;
using TraceDiag.Models;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class HopInfoTests
{
    [Fact]
    public void ApplyPing_StoresExactRttStatistics()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.ApplyPing(true, 10);
        hop.ApplyPing(true, 4);
        hop.ApplyPing(true, 20);
        Assert.Equal(3, hop.Sent);
        Assert.Equal(3, hop.Received);
        Assert.Equal((10.0 + 4 + 20) / 3, hop.AvgPingMs, 10);
        Assert.Equal(4, hop.MinPingMs);
        Assert.Equal(20, hop.MaxPingMs);
        Assert.Equal(20, hop.LastPingMs);
    }

    [Fact]
    public void ApplyPing_WithLoss_ClearsLastPingAndCalculatesLoss()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.ApplyPing(true, 12);
        hop.ApplyPing(false, 0);
        Assert.Null(hop.LastPingMs);
        Assert.Equal(1, hop.Lost);
        Assert.Equal(50, hop.LossPercent);
        Assert.True(hop.HasLoss);
    }

    [Fact]
    public void ApplyPing_AllLost_SetsNoAnswerStatus()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.ApplyPing(false, 0);
        Assert.Equal(Strings.Status.NoAnswer, hop.Status);
        Assert.False(hop.Responded);
    }

    [Fact]
    public void ApplyPing_AfterLoss_KeepsDelayEnabledForWholeHost()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.ApplyPing(true, 10);
        Assert.False(hop.LossDelayEnabled);
        hop.ApplyPing(false, 0);
        Assert.True(hop.LossDelayEnabled);
        hop.ApplyPing(true, 10);
        Assert.True(hop.LossDelayEnabled);
    }

    [Fact]
    public void RttSamples_StoresEverySuccessfulMeasurement()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.ApplyPing(true, 5);
        hop.ApplyPing(false, 0);
        hop.ApplyPing(true, 8);
        Assert.Equal([5, 8], hop.RttSamples);
    }
}
