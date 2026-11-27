using System.Net;

namespace TraceDiag.Models;

public sealed record ProbeResult(
    ProbeOutcome Outcome,
    IPAddress? Address,
    long RoundtripTimeMs,
    int BufferSize,
    int Ttl);
