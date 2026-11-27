using TraceDiag.Services.Abstractions;

namespace TraceDiag.Services.Diagnostics;

internal sealed record DiagnosticContext(
    IIcmpProbeClient ProbeClient,
    IDnsResolverService DnsResolver,
    IDiagnosticLogger? Logger,
    bool RipeLookupEnabled = true,
    IRipeOwnerService? RipeOwner = null,
    IDnsAnswerService? DnsAnswers = null,
    bool AsnLookupEnabled = true,
    bool ExternalSourcesEnabled = true,
    string? AdapterId = null,
    string? DnsFilePath = null);
