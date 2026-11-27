using System.Net;
using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Central place where triggers become concrete probe policies.</summary>
internal sealed class ProbeJobFactory : IProbeJobFactory
{
    public ProbeJob Create(IProbeTrigger trigger, IPAddress address, int ttl,
        int bufferSize, int timeoutMs, string context)
    {
        var bounded = Compat.Clamp(bufferSize, ProbeLimits.MinPacketSize, ProbeLimits.MaxPacketSize);
        var policy = trigger.Apply(new ProbePolicy(bounded, false));
        return new ProbeJob(address, ttl, timeoutMs, context, policy, trigger);
    }
}
