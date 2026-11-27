using TraceDiag.Models;

namespace TraceDiag.Services.Abstractions;

/// <summary>Detects route changes and ECMP balancing from ICMP reply sources.</summary>
internal interface IRouteStabilityController
{
    void Apply(HopInfo hop);
}
