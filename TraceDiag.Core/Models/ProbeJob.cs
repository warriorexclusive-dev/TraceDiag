using System.Net;

namespace TraceDiag.Models;

/// <summary>Immutable description of one probe, including its trigger.</summary>
internal sealed record ProbeJob(
    IPAddress Address,
    int Ttl,
    int TimeoutMs,
    string Context,
    ProbePolicy Policy,
    IProbeTrigger Trigger)
{
    internal int BufferSize => Policy.BufferSize;
    internal bool DontFragment => Policy.DontFragment;
}
