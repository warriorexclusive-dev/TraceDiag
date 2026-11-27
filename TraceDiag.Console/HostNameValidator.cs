using System.Net;

namespace TraceDiag.Console;

/// <summary>Syntax check for an interactively entered IP address or host name.</summary>
internal static class HostNameValidator
{
    private const int MaxLength = 253;

    internal static bool IsValid(string? value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value)) return false;
        var host = value.Trim().TrimEnd('.');
        if (host.Length == 0 || host.Length > MaxLength || host.Any(char.IsWhiteSpace)) return false;
        if (IPAddress.TryParse(host, out _)) return true;
        return !host.All(character => char.IsDigit(character) || character == '.') && Labels(host);
    }

    private static bool Labels(string host) =>
        host.Split('.').All(label => label.Length is > 0 and <= 63
            && char.IsLetterOrDigit(label[0])
            && char.IsLetterOrDigit(label[label.Length - 1])
            && label.All(character => char.IsLetterOrDigit(character) || character == '-'));
}
