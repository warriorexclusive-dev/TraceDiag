namespace TraceDiag.Console.Tests;

public sealed class CompatibilityTests
{
    [Fact]
    public void Clamp_ReturnsValueInsideRange() =>
        Assert.Equal(5, Compat.Clamp(12, 1, 5));

    [Fact]
    public void Clamp_ReturnsOriginalValueInsideRange() =>
        Assert.Equal(3, Compat.Clamp(3, 1, 5));

    [Fact]
    public void MonotonicMilliseconds_IsPositive() =>
        Assert.True(Compat.MonotonicMilliseconds() > 0);

    [Fact]
    public async Task WithCancellation_ThrowsWhenTokenCancelled()
    {
        using var source = new CancellationTokenSource();
        var pending = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        source.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => Compat.WithCancellation(pending.Task, source.Token));
    }
}
