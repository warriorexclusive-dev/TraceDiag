using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Tests;

public sealed class PacketSizePlannerTests
{
    [Fact]
    public void DynamicBuild_UsesDoublingSequenceFrom64()
    {
        var sizes = PacketSizePlanner.Build(256, true);
        Assert.Equal([64, 128, 256], sizes);
    }

    [Fact]
    public void DynamicBuild_AppendsMaxWhenNotPowerOfTwo()
    {
        var sizes = PacketSizePlanner.Build(1000, true);
        Assert.Equal([64, 128, 256, 512, 1000], sizes);
    }

    [Fact]
    public void DynamicBuild_WithMax64_ReturnsSingleSize()
    {
        Assert.Equal([64], PacketSizePlanner.Build(64, true));
    }

    [Fact]
    public void FixedBuild_ReturnsOnlyMaxSize()
    {
        Assert.Equal([4096], PacketSizePlanner.Build(4096, false));
    }
}
