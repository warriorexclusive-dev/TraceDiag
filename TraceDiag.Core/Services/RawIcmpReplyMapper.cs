using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services;

/// <summary>Maps raw ICMP reply types onto the shared probe outcomes.</summary>
internal static class RawIcmpReplyMapper
{
    internal static ProbeResult Map(byte type, IPAddress sender, long rtt, int bufferSize,
        int ttl, bool ipv6)
    {
        if (IsEchoReply(type, ipv6)) return Result(ProbeOutcome.Success, sender, rtt, bufferSize, ttl);
        if (IsTimeExceeded(type, ipv6)) return Result(ProbeOutcome.TtlExpired, sender, rtt, bufferSize, ttl);
        if (IsUnreachable(type, ipv6)) return Result(ProbeOutcome.DestinationUnreachable, sender, rtt, bufferSize, ttl);
        return Result(ProbeOutcome.Failed, sender, rtt, bufferSize, ttl);
    }

    private static bool IsEchoReply(byte type, bool ipv6) =>
        type == (ipv6 ? RawIcmpPacket.EchoReply6 : RawIcmpPacket.EchoReply4);

    private static bool IsTimeExceeded(byte type, bool ipv6) =>
        type == (ipv6 ? RawIcmpPacket.TimeExceeded6 : RawIcmpPacket.TimeExceeded4);

    private static bool IsUnreachable(byte type, bool ipv6) =>
        type == (ipv6 ? RawIcmpPacket.Unreachable6 : RawIcmpPacket.Unreachable4);

    private static ProbeResult Result(ProbeOutcome outcome, IPAddress sender, long rtt,
        int bufferSize, int ttl) => new(outcome, sender, rtt, bufferSize, ttl);
}
