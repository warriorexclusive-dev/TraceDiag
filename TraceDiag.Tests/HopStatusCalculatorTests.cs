using TraceDiag.Models;

namespace TraceDiag.Tests;

public sealed class HopStatusCalculatorTests
{
    [Fact]
    public void Calculate_WithoutReplies_ReturnsNoAnswer()
    {
        Assert.Equal(Strings.Status.NoAnswer, HopStatusCalculator.Calculate(0, 5, 100));
    }

    [Fact]
    public void Calculate_WithoutLoss_ReturnsNormal()
    {
        Assert.Equal(Strings.Status.Normal, HopStatusCalculator.Calculate(5, 0, 0));
    }

    [Fact]
    public void Calculate_WithLoss_FormatsPercent()
    {
        var status = HopStatusCalculator.Calculate(3, 1, 25);
        Assert.Equal("потеря пакетов (25%)", status);
    }
}
