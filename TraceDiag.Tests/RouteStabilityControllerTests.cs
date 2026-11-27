using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class RouteStabilityControllerTests
{
    private readonly RouteStabilityController _controller = new();

    [Fact]
    public void Apply_WithBaselineResponderOnly_LeavesDiagnosisEmpty()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.ApplyPing(true, 5, TestFactories.Address(1));
        _controller.Apply(hop);
        Assert.Equal(Strings.Diagnosis.None, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithNewResponder_DetectsRouteChange()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.ApplyPing(true, 5, TestFactories.Address(2));
        _controller.Apply(hop);
        Assert.Equal(Strings.Diagnosis.RouteChange, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithAlternatingResponders_DetectsEcmpBalancing()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.ApplyPing(true, 5, TestFactories.Address(2));
        hop.ApplyPing(true, 5, TestFactories.Address(1));
        hop.ApplyPing(true, 5, TestFactories.Address(2));
        _controller.Apply(hop);
        Assert.Equal(Strings.Diagnosis.EcmpBalancing, hop.Diagnosis);
    }

    [Fact]
    public void Apply_WithSequentialNewResponders_DetectsRouteChange()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.ApplyPing(true, 5, TestFactories.Address(2));
        hop.ApplyPing(true, 5, TestFactories.Address(3));
        _controller.Apply(hop);
        Assert.Equal(Strings.Diagnosis.RouteChange, hop.Diagnosis);
    }

    [Fact]
    public void Apply_DoesNotOverrideOtherDiagnosis()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.Diagnosis = Strings.Diagnosis.AntiFlood;
        hop.ApplyPing(true, 5, TestFactories.Address(2));
        _controller.Apply(hop);
        Assert.Equal(Strings.Diagnosis.AntiFlood, hop.Diagnosis);
    }

    [Fact]
    public void Apply_UpdatesEarlierRouteDiagnosis()
    {
        var hop = TestFactories.Hop(1, 1);
        hop.Diagnosis = Strings.Diagnosis.RouteChange;
        hop.ApplyPing(true, 5, TestFactories.Address(2));
        hop.ApplyPing(true, 5, TestFactories.Address(1));
        hop.ApplyPing(true, 5, TestFactories.Address(2));
        _controller.Apply(hop);
        Assert.Equal(Strings.Diagnosis.EcmpBalancing, hop.Diagnosis);
    }
}
