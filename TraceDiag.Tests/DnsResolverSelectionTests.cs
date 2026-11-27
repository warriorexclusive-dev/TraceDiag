using System.Net;
using TraceDiag.Services;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Tests;

public sealed class DnsResolverSelectionTests
{
    private static readonly IPAddress First = IPAddress.Parse("10.0.0.1");
    private static readonly IPAddress Second = IPAddress.Parse("10.0.0.2");
    private static readonly IPAddress Third = IPAddress.Parse("2001:db8::1");

    [Fact]
    public void Merge_KeepsFirstOccurrenceAndOrder()
    {
        var result = DnsResolverSelection.Merge([First, Second], [Second, Third, First]);

        Assert.Equal([First, Second, Third], result);
    }

    [Fact]
    public void Get_WithoutAlternatives_UsesOnlySystemServers()
    {
        var result = DnsResolverSelection.Get(false);

        Assert.All(result, server => Assert.Contains(server, DnsServers.Get()));
    }

    [Fact]
    public void Create_PreservesAlternativeDnsFlag() =>
        Assert.False(RoutePrecheckSettings.Create(true, false, 3000, false).AlternativeDns);
}
