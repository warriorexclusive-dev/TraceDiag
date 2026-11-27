using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class HopDiagnosisTests
{
    [Fact]
    public void Apply_WithSkippedHopMarksIcmpDisabled()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.IsSkipped = true;
        HopDiagnosis.Apply(hop, []);
        Assert.Equal(Strings.Diagnosis.IcmpDisabled, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithNoRepliesMarksIcmpDisabled()
    {
        var hop = TestFactories.Hop(1, 1);
        HopDiagnosis.Apply(hop, [new HopSizeResult(64, 4, 0), new HopSizeResult(128, 4, 0)]);
        Assert.Equal(Strings.Diagnosis.IcmpDisabled, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithLargerSizeFailureMarksPacketSizeLimit()
    {
        var hop = TestFactories.Hop(1, 1);
        HopDiagnosis.Apply(hop, [new HopSizeResult(64, 4, 4), new HopSizeResult(128, 4, 0)]);
        Assert.Equal(Strings.Diagnosis.PacketSizeLimit, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithDegradedLargerSizeReportsBothReasons()
    {
        var hop = TestFactories.Hop(1, 1);
        HopDiagnosis.Apply(hop, [new HopSizeResult(64, 4, 4), new HopSizeResult(128, 4, 2)]);
        Assert.Equal(string.Join(Strings.Diagnosis.Separator,
            Strings.Diagnosis.PacketSizeLimit, Strings.Diagnosis.AntiFlood), hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithPartialLossMarksAntiFlood()
    {
        var hop = TestFactories.Hop(1, 1);
        HopDiagnosis.Apply(hop, [new HopSizeResult(64, 4, 3), new HopSizeResult(128, 4, 4)]);
        Assert.Equal(Strings.Diagnosis.AntiFlood, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithCleanRepliesLeavesDiagnosisEmpty()
    {
        var hop = TestFactories.Hop(1, 1);
        HopDiagnosis.Apply(hop, [new HopSizeResult(64, 4, 4), new HopSizeResult(128, 4, 4)]);
        Assert.Equal(Strings.Diagnosis.None, hop.Diagnosis);
    }

    [Fact]
    public void Apply_ReportsSizeLimitAndAntiFloodTogether()
    {
        var hop = TestFactories.Hop(1, 1);
        HopDiagnosis.Apply(hop, [new HopSizeResult(64, 4, 3), new HopSizeResult(128, 4, 0)]);
        Assert.Equal(string.Join(Strings.Diagnosis.Separator,
            Strings.Diagnosis.PacketSizeLimit, Strings.Diagnosis.AntiFlood), hop.Diagnosis);
    }
}
