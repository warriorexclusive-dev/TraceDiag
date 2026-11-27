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
    internal const string Format = "--format";
    internal const string Reset = "--reset";
    internal const string Help = "--help";

    internal static readonly string[] Names =
        [Host, PacketSize, Attempts, LossDelay, Dynamic, DontFragment, Format, Reset, Help];

    internal static readonly string[] Usage =
    [
        Strings.Console.OptionHost,
        Strings.Console.OptionPacketSize,
        Strings.Console.OptionAttempts,
        Strings.Console.OptionLossDelay,
        Strings.Console.OptionDynamic,
        Strings.Console.OptionDontFragment,
        Strings.Console.OptionFormat,
        Strings.Console.OptionReset,
        Strings.Console.OptionHelp
    ];
}
