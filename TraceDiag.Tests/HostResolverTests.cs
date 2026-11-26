using TraceDiag.Services;

namespace TraceDiag.Tests;

public sealed class HostResolverTests
{
    [Fact]
    public async Task ResolveAsync_WithValidIp_ReturnsAddress()
    {
        var address = await HostResolver.ResolveAsync("10.0.0.1", CancellationToken.None);
        Assert.Equal("10.0.0.1", address?.ToString());
    }

    [Fact]
    public async Task ResolveAsync_WithIpv4Unspecified_ReturnsNull()
    {
        var address = await HostResolver.ResolveAsync("0.0.0.0", CancellationToken.None);
        Assert.Null(address);
    }

    [Fact]
    public async Task ResolveAsync_WithIpv6Unspecified_ReturnsNull()
    {
        var address = await HostResolver.ResolveAsync("::", CancellationToken.None);
        Assert.Null(address);
    }
}
