using System.Net;

namespace TraceDiag.Services.Diagnostics;

internal sealed record ProbeRequest(IPAddress Address, int Ttl, int BufferSize, int TimeoutMs, string Context, bool DontFragment = false);
