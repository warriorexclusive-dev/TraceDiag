using TraceDiag.Models;
using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Executes a job through the existing socket boundary.</summary>
internal sealed class ProbeJobRunner : IProbeJobRunner
{
    public Task<ProbeResult> RunAsync(DiagnosticContext context, ProbeJob job,
        IProgress<string> progress, CancellationToken cancellationToken) =>
        ProbeExecutor.SendAsync(context, ToRequest(job), progress, cancellationToken);

    private static ProbeRequest ToRequest(ProbeJob job) =>
        new(job.Address, job.Ttl, job.BufferSize, job.TimeoutMs, job.Context, job.DontFragment);
}
