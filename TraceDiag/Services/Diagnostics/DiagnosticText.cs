using System.Globalization;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class DiagnosticText
{
    internal static string Hop(HopInfo hop)
    {
        var name = string.IsNullOrWhiteSpace(hop.HostName) ? Strings.Diag.NoDns : hop.HostName;
        return TextFormatter.Fmt(Strings.Diag.HopFormat, hop.Address, name);
    }

    internal static string Last(HopInfo hop) =>
        hop.LastPingMs?.ToString(CultureInfo.InvariantCulture) ?? Strings.Diag.NoAnswer;

    internal static string Min(HopInfo hop) =>
        hop.MinPingMs?.ToString(CultureInfo.InvariantCulture) ?? Strings.Diag.Dash;

    internal static string Max(HopInfo hop) =>
        hop.MaxPingMs?.ToString(CultureInfo.InvariantCulture) ?? Strings.Diag.Dash;

    internal static string Ms(double value) =>
        TextFormatter.Fmt(Strings.Diag.MsFormat, value);
}
