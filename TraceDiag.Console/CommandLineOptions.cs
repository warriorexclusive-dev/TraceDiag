using TraceDiag.Localization;
using TraceDiag.Models;

namespace TraceDiag.Console;

internal sealed record Invocation(
    string Command,
    string Host,
    int PacketSize,
    int Attempts,
    int LossDelayMs,
    bool Dynamic,
    bool DontFragment,
    IpFamily Family,
    string Format,
    Language Language,
    bool Reset,
    bool Help);

internal sealed class CommandLineException(string message, string? details = null) : Exception(message)
{
    internal string? Details { get; } = details;
}

internal static class ConsoleDefaults
{
    internal const int Attempts = 100;
    internal const int LossDelayMs = 1000;
    internal static int PacketSize => ProbeLimits.DontFragmentPacketSize;
    internal const string Format = Strings.Console.FormatPlain;
    internal const Language Language = TraceDiag.Localization.Language.En;
}

internal sealed class OptionsBag
{
    internal string Host { get; set; } = string.Empty;
    internal int PacketSize { get; set; } = ConsoleDefaults.PacketSize;
    internal int Attempts { get; set; } = ConsoleDefaults.Attempts;
    internal int LossDelayMs { get; set; } = ConsoleDefaults.LossDelayMs;
    internal bool Dynamic { get; set; }
    internal bool DontFragment { get; set; }
    internal IpFamily Family { get; set; } = IpFamily.Auto;
    internal string Format { get; set; } = ConsoleDefaults.Format;
    internal Language Language { get; set; } = ConsoleDefaults.Language;
    internal bool Reset { get; set; }
}
