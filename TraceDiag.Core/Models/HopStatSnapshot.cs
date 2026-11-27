using System.Net;

namespace TraceDiag.Models;

/// <summary>UI-safe copy of a node's current statistics.</summary>
public sealed record HopStatSnapshot(
    int Ttl,
    IPAddress Address,
    string? HostName,
    int Sent,
    int Received,
    double AvgPingMs,
    long? MinPingMs,
    long? MaxPingMs,
    long? LastPingMs,
    string Status,
    string Diagnosis,
    double JitterMs = 0,
    double StdDevMs = 0,
    string Asn = "",
    int RouteUid = 0,
    string RouteTarget = "",
    string RouteResolver = "")
{
    public int Lost => Math.Max(0, Sent - Received);
}
