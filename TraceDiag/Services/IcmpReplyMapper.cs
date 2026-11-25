using System.Net.NetworkInformation;
using TraceDiag.Models;

namespace TraceDiag.Services;

internal static class IcmpReplyMapper
{
    internal static ProbeResult Map(PingReply reply, int bufferSize, int ttl)
    {
        if (reply.Status == IPStatus.Success) return Success(reply, bufferSize, ttl);
        if (reply.Status is IPStatus.TtlExpired or IPStatus.TimeExceeded) return Expired(reply, bufferSize, ttl);
        if (reply.Status == IPStatus.TimedOut) return TimedOut(bufferSize, ttl);
        if (IsUnreachable(reply.Status)) return Unreachable(reply, bufferSize, ttl);
        return FailedWithReply(reply, bufferSize, ttl);
    }

    internal static ProbeResult Failed(int bufferSize, int ttl) =>
        new(ProbeOutcome.Failed, null, 0, bufferSize, ttl);

    private static bool IsUnreachable(IPStatus status) =>
        status is IPStatus.DestinationUnreachable
            or IPStatus.DestinationNetworkUnreachable
            or IPStatus.DestinationHostUnreachable
            or IPStatus.DestinationPortUnreachable
            or IPStatus.DestinationProhibited;

    private static ProbeResult Success(PingReply reply, int bufferSize, int ttl) =>
        new(ProbeOutcome.Success, reply.Address, reply.RoundtripTime, bufferSize, ttl);

    private static ProbeResult Expired(PingReply reply, int bufferSize, int ttl) =>
        new(ProbeOutcome.TtlExpired, reply.Address, reply.RoundtripTime, bufferSize, ttl);

    private static ProbeResult TimedOut(int bufferSize, int ttl) =>
        new(ProbeOutcome.TimedOut, null, 0, bufferSize, ttl);

    private static ProbeResult Unreachable(PingReply reply, int bufferSize, int ttl) =>
        new(ProbeOutcome.DestinationUnreachable, reply.Address, reply.RoundtripTime, bufferSize, ttl);

    private static ProbeResult FailedWithReply(PingReply reply, int bufferSize, int ttl) =>
        new(ProbeOutcome.Failed, reply.Address, reply.RoundtripTime, bufferSize, ttl);
}
