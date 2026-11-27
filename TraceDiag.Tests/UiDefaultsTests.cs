using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class UiDefaultsTests
{
    [Fact]
    public void DefaultAttempts_Equals100ForProviderReports()
    {
        Assert.Equal(100, UiDefaults.DefaultAttempts);
    }

    [Fact]
    public void DefaultLossDelayMs_Equals1000AfterPacketLoss()
    {
        Assert.Equal(1000, UiDefaults.DefaultLossDelayMs);
    }

    [Fact]
    public void PacketBounds_UseSafeIcmpPayloadMaximum()
    {
        Assert.Equal(64, UiDefaults.MinPacketSize);
        Assert.Equal(65500, UiDefaults.MaxPacketSize);
        Assert.Equal(1024, UiDefaults.DefaultMaxPacketSize);
    }
}
