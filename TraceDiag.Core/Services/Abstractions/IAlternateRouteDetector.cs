using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Abstractions;

internal interface IAlternateRouteDetector
{
    Task<RouteDiscoveryResult> DetectAsync(DiagnosticContext context, string host,
        IReadOnlyList<DnsAnswer> targets, RoutePrecheckSettings settings,
        RouteRegistry registry, CancellationToken cancellationToken);

    RouteDiscoveryResult CreateFallback(string host, IReadOnlyList<DnsAnswer> targets, RouteRegistry registry);
}
