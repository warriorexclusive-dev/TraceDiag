using System.Net;
using TraceDiag.Models;
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

    [Fact]
    public async Task ResolveAsync_WithIpv6Literal_AndIpv4Family_ReturnsNull()
    {
        var address = await HostResolver.ResolveAsync("2001:db8::1", IpFamily.IPv4, CancellationToken.None);
        Assert.Null(address);
    }

    [Fact]
    public async Task ResolveAsync_WithIpv4Literal_AndIpv6Family_ReturnsNull()
    {
        var address = await HostResolver.ResolveAsync("10.0.0.1", IpFamily.IPv6, CancellationToken.None);
        Assert.Null(address);
    }

    [Fact]
    public async Task ResolveAsync_WithIpv6Literal_AndIpv6Family_ReturnsAddress()
    {
        var address = await HostResolver.ResolveAsync("2001:db8::1", IpFamily.IPv6, CancellationToken.None);
        Assert.Equal("2001:db8::1", address?.ToString());
    }

    [Fact]
    public void PickAddress_Auto_PrefersIpv4()
    {
        var addresses = new[] { IPAddress.Parse("2001:db8::1"), IPAddress.Parse("10.0.0.1") };
        Assert.Equal("10.0.0.1", HostResolver.PickAddress(addresses, IpFamily.Auto)?.ToString());
    }

    [Fact]
    public void PickAddress_Ipv6_SkipsIpv4()
    {
        var addresses = new[] { IPAddress.Parse("10.0.0.1"), IPAddress.Parse("2001:db8::1") };
        Assert.Equal("2001:db8::1", HostResolver.PickAddress(addresses, IpFamily.IPv6)?.ToString());
    }
}
