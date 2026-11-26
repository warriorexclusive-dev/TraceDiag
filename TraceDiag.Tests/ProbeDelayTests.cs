using System.Diagnostics;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Tests;

public sealed class ProbeDelayTests
{
    [Fact]
    public async Task ApplyAsync_WhenEnabled_WaitsConfiguredTime()
    {
        var stopwatch = Stopwatch.StartNew();
        await ProbeDelay.ApplyAsync(true, 60, CancellationToken.None);
        Assert.True(stopwatch.ElapsedMilliseconds >= 45);
    }

    [Fact]
    public async Task ApplyAsync_WhenDisabled_DoesNotWait()
    {
        var stopwatch = Stopwatch.StartNew();
        await ProbeDelay.ApplyAsync(false, 500, CancellationToken.None);
        Assert.True(stopwatch.ElapsedMilliseconds < 200);
    }

    [Fact]
    public async Task ApplyAsync_WhenZero_DoesNotWait()
    {
        var stopwatch = Stopwatch.StartNew();
        await ProbeDelay.ApplyAsync(true, 0, CancellationToken.None);
        Assert.True(stopwatch.ElapsedMilliseconds < 200);
    }
}
