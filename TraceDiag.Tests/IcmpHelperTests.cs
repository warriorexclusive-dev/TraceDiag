using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using TraceDiag.Models;
using TraceDiag.Services;

namespace TraceDiag.Tests;

public sealed class IcmpHelperTests
{
    [Fact]
    public void CreatePayload_FillsDeterministicPattern()
    {
        var payload = IcmpPacketFactory.CreatePayload(258);
        Assert.Equal(258, payload.Length);
        Assert.Equal(0, payload[0]);
        Assert.Equal(255, payload[255]);
        Assert.Equal(0, payload[256]);
        Assert.Equal(1, payload[257]);
    }

    [Fact]
    public void CreatePayload_WithNegativeSize_ReturnsEmpty()
    {
        Assert.Empty(IcmpPacketFactory.CreatePayload(-5));
    }

    [Fact]
    public void CreateOptions_UsesRequestedTtl()
    {
        Assert.Equal(7, IcmpPacketFactory.CreateOptions(7).Ttl);
    }

    [Fact]
    public void Map_SuccessReply_ProducesSuccessResult()
    {
        var result = IcmpReplyMapper.Map(CreateReply(IPStatus.Success, 12), 64, 5);
        Assert.Equal(ProbeOutcome.Success, result.Outcome);
        Assert.Equal(12, result.RoundtripTimeMs);
        Assert.Equal(64, result.BufferSize);
        Assert.Equal(5, result.Ttl);
    }

    [Theory]
    [InlineData(IPStatus.TtlExpired)]
    [InlineData(IPStatus.TimeExceeded)]
    public void Map_TtlReplies_ProduceExpired(IPStatus status)
    {
        var result = IcmpReplyMapper.Map(CreateReply(status, 4), 32, 3);
        Assert.Equal(ProbeOutcome.TtlExpired, result.Outcome);
        Assert.NotNull(result.Address);
    }

    [Fact]
    public void Map_TimedOutReply_ProducesTimedOut()
    {
        var result = IcmpReplyMapper.Map(CreateReply(IPStatus.TimedOut, 0), 32, 3);
        Assert.Equal(ProbeOutcome.TimedOut, result.Outcome);
        Assert.Null(result.Address);
    }

    [Fact]
    public void Map_UnreachableReply_ProducesDestinationUnreachable()
    {
        var result = IcmpReplyMapper.Map(CreateReply(IPStatus.DestinationHostUnreachable, 0), 32, 3);
        Assert.Equal(ProbeOutcome.DestinationUnreachable, result.Outcome);
    }

    [Fact]
    public void Map_OtherStatus_ProducesFailed()
    {
        var result = IcmpReplyMapper.Map(CreateReply(IPStatus.Unknown, 0), 32, 3);
        Assert.Equal(ProbeOutcome.Failed, result.Outcome);
    }

    [Fact]
    public void Failed_ProducesFailedResultWithoutAddress()
    {
        var result = IcmpReplyMapper.Failed(48, 9);
        Assert.Equal(ProbeOutcome.Failed, result.Outcome);
        Assert.Null(result.Address);
        Assert.Equal(48, result.BufferSize);
    }

    private static PingReply CreateReply(IPStatus status, long rtt)
    {
        var constructor = typeof(PingReply)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(c => c.GetParameters().Length == 5);
        object?[] args = [IPAddress.Parse("10.0.0.1"), new PingOptions(5, false), status, rtt, new byte[64]];
        return (PingReply)constructor.Invoke(args)!;
    }
}
