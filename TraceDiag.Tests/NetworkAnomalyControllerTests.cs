using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class NetworkAnomalyControllerTests
{
    private readonly NetworkAnomalyController _controller = new();

    [Fact]
    public void Apply_WithDontFragmentAndFullLossAboveLimit_AddsMtuBlackHole()
    {
        var hop = TestFactories.Hop(1, 1);
        var results = new[]
        {
            new HopSizeResult(64, 4, 4, [TestFactories.Address(2)]),
            new HopSizeResult(128, 4, 0)
        };
        _controller.Apply(hop, results, true, []);
        Assert.Equal(Strings.Diagnosis.MtuBlackHole, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithoutDontFragment_SkipsMtuBlackHole()
    {
        var hop = TestFactories.Hop(1, 1);
        var results = new[]
        {
            new HopSizeResult(64, 4, 4, [TestFactories.Address(2)]),
            new HopSizeResult(128, 4, 0)
        };
        _controller.Apply(hop, results, false, []);
        Assert.Equal(Strings.Diagnosis.None, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithDifferentRespondersPerSize_AddsRouteAsymmetry()
    {
        var hop = TestFactories.Hop(1, 1);
        var results = new[]
        {
            new HopSizeResult(64, 4, 4, [TestFactories.Address(2)]),
            new HopSizeResult(128, 4, 4, [TestFactories.Address(3)])
        };
        _controller.Apply(hop, results, false, []);
        Assert.Equal(Strings.Diagnosis.RouteAsymmetry, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithSameRespondersPerSize_SkipsRouteAsymmetry()
    {
        var hop = TestFactories.Hop(1, 1);
        var results = new[]
        {
            new HopSizeResult(64, 4, 4, [TestFactories.Address(2)]),
            new HopSizeResult(128, 4, 4, [TestFactories.Address(2)])
        };
        _controller.Apply(hop, results, false, []);
        Assert.Equal(Strings.Diagnosis.None, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithBlockedTtlReplacesDisabledWithIcmpBlocked()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.IsSkipped = true;
        hop.Diagnosis = Strings.Diagnosis.IcmpDisabled;
        _controller.Apply(hop, [], false, [1]);
        Assert.Equal(Strings.Diagnosis.IcmpBlocked, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithoutLaterReply_KeepsDisabledDiagnosis()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.IsSkipped = true;
        hop.Diagnosis = Strings.Diagnosis.IcmpDisabled;
        _controller.Apply(hop, [], false, []);
        Assert.Equal(Strings.Diagnosis.IcmpDisabled, hop.Diagnosis);
    }

    [Theory]
    [InlineData(ProbeOutcome.PortUnreachable)]
    [InlineData(ProbeOutcome.Prohibited)]
    public void Apply_WithFilteringOutcome_AddsPortFiltered(ProbeOutcome outcome)
    {
        var hop = TestFactories.Hop(1, 1);
        hop.RecordOutcome(outcome);
        _controller.Apply(hop, [], false, []);
        Assert.Equal(Strings.Diagnosis.PortFiltered, hop.Diagnosis);
    }
}
