namespace TraceDiag.Console;

/// <summary>Canonical option names and their help text, kept in one place.</summary>
internal static class ConsoleOptions
{
    internal const string Host = "--host";
    internal const string PacketSize = "--packet-size";
    internal const string Attempts = "--attempts";
    internal const string LossDelay = "--loss-delay";
    internal const string Dynamic = "--dynamic";
    internal const string DontFragment = "--df";
    internal const string NoRipe = "--no-ripe";
    internal const string Ipv4 = "--ipv4";
    internal const string Ipv6 = "--ipv6";
    internal const string Format = "--format";
    internal const string Language = "--language";
    internal const string Reset = "--reset";
    internal const string Help = "--help";

    internal static readonly string[] Names =
        [Host, PacketSize, Attempts, LossDelay, Dynamic, DontFragment, NoRipe, Ipv4, Ipv6, Format, Language, Reset, Help];

    internal static readonly string[] Usage =
    [
        Strings.Console.HelpHost,
        Strings.Console.HelpPacketSize,
        Strings.Console.HelpAttempts,
        Strings.Console.HelpLossDelay,
        Strings.Console.HelpDynamic,
        Strings.Console.HelpDontFragment,
        Strings.Console.HelpNoRipe,
        Strings.Console.HelpIpv4,
        Strings.Console.HelpIpv6,
        Strings.Console.HelpFormat,
        Strings.Console.OptionLanguage,
        Strings.Console.OptionResetHelp,
        Strings.Console.HelpHelp
    ];
}
