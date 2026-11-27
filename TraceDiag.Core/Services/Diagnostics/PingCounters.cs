using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal sealed record PingCounters(int Sent, int Received, IReadOnlyList<IPAddress> Responders)
{
    internal static PingCounters Capture(HopInfo hop) =>
        new(hop.Sent, hop.Received, hop.Responders.ToArray());
}
