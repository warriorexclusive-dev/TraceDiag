using System.Net;

namespace TraceDiag.Models;

/// <summary>One address returned by a configured DNS resolver.</summary>
public sealed record DnsAnswer(IPAddress Address, string Resolver, bool IsIpv6);
