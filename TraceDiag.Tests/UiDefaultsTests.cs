using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class UiDefaultsTests
{
    [Fact]
    public void DefaultAttempts_Equals100ForProviderReports()
    {
        Assert.Equal(100, UiDefaults.DefaultAttempts);
    }
}
