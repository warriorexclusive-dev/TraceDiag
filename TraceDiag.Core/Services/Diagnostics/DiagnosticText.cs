using System.Globalization;
using TraceDiag.Models;

namespace TraceDiag.Services.Diagnostics;

internal static class DiagnosticText
{
    internal static string Hop(HopInfo hop) =>
        TextFormatter.Fmt(Strings.Diag.HopFormat, hop.Address, Name(hop));

    internal static LocalizedArg HopArg(HopInfo hop)
    {
        var canonical = TextFormatter.Fmt(Strings.Diag.HopFormat, hop.Address, Name(hop));
        var name = string.IsNullOrWhiteSpace(hop.HostName) ? Strings.T(Strings.Diag.NoDns) : hop.HostName!;
        var localized = TextFormatter.Fmt(Strings.T(Strings.Diag.HopFormat), hop.Address, name);
        return LocalizedArg.Create(canonical, localized);
    }

    internal static string Last(HopInfo hop) =>
        hop.LastPingMs?.ToString(CultureInfo.InvariantCulture) ?? Strings.Diag.NoAnswer;

    internal static LocalizedArg LastArg(HopInfo hop) => LocalizedArg.Create(
        Last(hop), hop.LastPingMs?.ToString(CultureInfo.InvariantCulture) ?? Strings.T(Strings.Diag.NoAnswer));

    internal static string Min(HopInfo hop) =>
        hop.MinPingMs?.ToString(CultureInfo.InvariantCulture) ?? Strings.Diag.Dash;

    internal static LocalizedArg MinArg(HopInfo hop) => LocalizedArg.Create(
        Min(hop), hop.MinPingMs?.ToString(CultureInfo.InvariantCulture) ?? Strings.T(Strings.Diag.Dash));

    internal static string Max(HopInfo hop) =>
        hop.MaxPingMs?.ToString(CultureInfo.InvariantCulture) ?? Strings.Diag.Dash;

    internal static LocalizedArg MaxArg(HopInfo hop) => LocalizedArg.Create(
        Max(hop), hop.MaxPingMs?.ToString(CultureInfo.InvariantCulture) ?? Strings.T(Strings.Diag.Dash));

    internal static string Ms(double value) =>
        TextFormatter.Fmt(Strings.Diag.MsFormat, value);

    internal static LocalizedArg MsArg(double value) => LocalizedArg.Format(Strings.Diag.MsFormat, value);

    private static string Name(HopInfo hop) =>
        string.IsNullOrWhiteSpace(hop.HostName) ? Strings.Diag.NoDns : hop.HostName!;
}
