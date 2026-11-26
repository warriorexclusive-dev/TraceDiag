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
    public void DefaultDelayMs_Equals1000AfterIcmpReply()
    {
        Assert.Equal(1000, UiDefaults.DefaultDelayMs);
    }
}
