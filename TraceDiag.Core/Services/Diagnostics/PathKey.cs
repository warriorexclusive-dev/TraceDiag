using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

/// <summary>Builds comparable route signatures from probe replies.</summary>
internal static class PathKey
{
    internal static string Format(ProbeResult result, IPAddress target)
    {
        if (result.Address is null) return Strings.Diag.RouteMiss;
        return result.Address.Equals(target)
            ? target + "!"
            : result.Address.ToString();
    }

    internal static string Build(IReadOnlyList<string> replies) =>
        string.Join(Strings.Diag.RouteSeparator, replies);

    internal static IReadOnlyList<string> Split(string key) =>
        key.Split(new[] { Strings.Diag.RouteSeparator }, StringSplitOptions.None);
}
