using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Abstractions;

/// <summary>Creates immutable probe jobs with a caller-selected trigger.</summary>
internal interface IProbeJobFactory
{
    ProbeJob Create(
        IProbeTrigger trigger,
        IPAddress address,
        int ttl,
        int bufferSize,
        int timeoutMs,
        string context);
}
