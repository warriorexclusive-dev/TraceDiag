using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

internal sealed record DiagnosticContext(
    IIcmpProbeClient ProbeClient,
    IDnsResolverService DnsResolver,
    IDiagnosticLogger? Logger);
