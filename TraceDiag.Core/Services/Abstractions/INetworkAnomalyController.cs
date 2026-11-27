using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Services.Abstractions;

/// <summary>Detects MTU, routing and filtering anomalies from collected probe data.</summary>
internal interface INetworkAnomalyController
{
    void Apply(HopInfo hop, IReadOnlyList<HopSizeResult> results,
        bool dontFragment, IReadOnlyCollection<int> blockedTtls);
}
