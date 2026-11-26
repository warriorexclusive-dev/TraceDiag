using System.Net;
using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Tests.TestData;

internal static class TestFactories
{
    internal static IPAddress Address(int last) => IPAddress.Parse($"10.0.0.{last}");

    internal static ProbeResult Success(ProbeCall call, long rtt = 7) =>
        new(ProbeOutcome.Success, call.Address, rtt, call.BufferSize, call.Ttl);

    internal static ProbeResult Expired(ProbeCall call, long rtt = 3) =>
        new(ProbeOutcome.TtlExpired, call.Address, rtt, call.BufferSize, call.Ttl);

    internal static ProbeResult ExpiredFrom(ProbeCall call, IPAddress address) =>
        new(ProbeOutcome.TtlExpired, address, 3, call.BufferSize, call.Ttl);

    internal static ProbeResult Timeout(ProbeCall call) =>
        new(ProbeOutcome.TimedOut, null, 0, call.BufferSize, call.Ttl);

    internal static HopInfo Hop(int ttl, int address, string? name = null) => new()
    {
        Ttl = ttl,
        Address = Address(address),
        HostName = name
    };

    internal static DiagnosticContext Context(FakeProbeClient probe, FakeDiagnosticLogger? logger = null) =>
        new(probe, new FakeDnsResolver(address => address.ToString()), logger);

    internal static DiscoverySettings Discovery(
        int roundLimit = 1,
        int intervalMs = 0,
        int packetSize = 64,
        int timeoutMs = 100,
        bool dontFragment = true,
        int delayMs = 0) =>
        new(packetSize, timeoutMs, intervalMs, roundLimit, dontFragment, delayMs);

    internal static CollectingProgress<string> TextProgress() => new();

    internal static CollectingProgress<IReadOnlyList<HopStatSnapshot>> StatsProgress() => new();
}
