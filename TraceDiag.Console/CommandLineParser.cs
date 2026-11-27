using System.Globalization;
using TraceDiag.Localization;
using TraceDiag.Models;

namespace TraceDiag.Console;

internal static class CommandLineParser
{
    internal static Invocation Parse(string[] args)
    {
        Translator.SetLanguage(ReadLanguage(args));
        if (args.Length == 0 || args.Any(IsHelp)) return Help();
        RequireCommand(args[0]);
        var values = Read(args, 1);
        values.Language = Translator.Current;
        Validate(args[0], values);
        return Build(args[0], values);
    }

    private static Invocation Help() =>
        new("help", string.Empty, ConsoleDefaults.PacketSize, ConsoleDefaults.Attempts,
            ConsoleDefaults.LossDelayMs, false, false, IpFamily.Auto, ConsoleDefaults.Format,
            Translator.Current, false, true);

    private static Language ReadLanguage(string[] args)
    {
        for (var index = 0; index < args.Length - 1; index++)
            if (args[index] == CommandLineSettingsArg.Language) return ParseLanguage(args[index + 1]);
        return ConsoleDefaults.Language;
    }

    private static Language ParseLanguage(string value) =>
        Enum.TryParse(value, true, out Language language) && Enum.IsDefined(typeof(Language), language)
            ? language
            : throw new CommandLineException(TextFormatter.Fmt(Strings.T(Strings.Console.InvalidLanguageFormat), value));

    private static bool NeedsHost(string command) =>
        command is CommandLineSettingsArg.Run or CommandLineSettingsArg.Monitor;

    private static void RequireCommand(string command)
    {
        if (CommandLineSettingsArg.IsCommand(command)) return;
        throw new CommandLineException(TextFormatter.Fmt(Strings.T(Strings.Console.InvalidCommandFormat), command));
    }

    private static OptionsBag Read(string[] args, int start)
    {
        var bag = new OptionsBag();
        for (var index = start; index < args.Length; index++) Apply(args, ref index, bag);
        return bag;
    }

    private static void Apply(string[] args, ref int index, OptionsBag bag)
    {
        switch (args[index])
        {
            case CommandLineSettingsArg.Dynamic: bag.Dynamic = true; return;
            case CommandLineSettingsArg.DontFragment: bag.DontFragment = true; return;
            case CommandLineSettingsArg.NoRipe: bag.RipeLookup = false; return;
            case CommandLineSettingsArg.NoRoutePrecheck: bag.RoutePrecheck = false; return;
            case CommandLineSettingsArg.NoAltDns: bag.AlternativeDns = false; return;
            case CommandLineSettingsArg.DeepRoutes: bag.DeepRoutes = true; return;
            case CommandLineSettingsArg.Ipv4: bag.Family = IpFamily.IPv4; return;
            case CommandLineSettingsArg.Ipv6: bag.Family = IpFamily.IPv6; return;
            case CommandLineSettingsArg.Reset: bag.Reset = true; return;
            default: ApplyValue(args, ref index, bag); return;
        }
    }

    private static void ApplyValue(string[] args, ref int index, OptionsBag bag)
    {
        var option = args[index];
        switch (option)
        {
            case CommandLineSettingsArg.Host: bag.Host = Next(args, ref index, option); return;
            case CommandLineSettingsArg.PacketSize: bag.PacketSize = Number(option, Next(args, ref index, option)); return;
            case CommandLineSettingsArg.Attempts: bag.Attempts = Number(option, Next(args, ref index, option)); return;
            case CommandLineSettingsArg.LossDelay: bag.LossDelayMs = Number(option, Next(args, ref index, option)); return;
            case CommandLineSettingsArg.ProbeTimeout: bag.ProbeTimeoutMs = Number(option, Next(args, ref index, option)); return;
            case CommandLineSettingsArg.Format: bag.Format = Next(args, ref index, option); return;
            case CommandLineSettingsArg.Language: bag.Language = ParseLanguage(Next(args, ref index, option)); return;
            default: throw Unknown(option);
        }
    }

    private static CommandLineException Unknown(string option) =>
        new(TextFormatter.Fmt(Strings.T(Strings.Console.UnknownOptionFormat), option),
            OptionSuggestions.List(OptionSuggestions.Closest(option)));

    private static string Next(string[] args, ref int index, string option)
    {
        if (++index >= args.Length)
            throw new CommandLineException(TextFormatter.Fmt(Strings.T(Strings.Console.MissingValueFormat), option));
        return args[index];
    }

    private static int Number(string option, string value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            ? number
            : throw new CommandLineException(TextFormatter.Fmt(Strings.T(Strings.Console.InvalidNumberFormat), option, value));

    private static bool IsHelp(string argument) =>
        argument is CommandLineSettingsArg.Help or Strings.Console.HelpShort;

    private static void Validate(string command, OptionsBag bag)
    {
        if (NeedsHost(command) && string.IsNullOrWhiteSpace(bag.Host))
            throw new CommandLineException(Strings.T(Strings.Console.HostRequired));
        ValidateRanges(bag);
        if (bag.Format is not (Strings.Console.FormatPlain or Strings.Console.FormatCsv or Strings.Console.FormatJson))
            throw new CommandLineException(Strings.T(Strings.Console.InvalidFormat));
    }

    private static void ValidateRanges(OptionsBag bag)
    {
        Range(CommandLineSettingsArg.PacketSize, bag.PacketSize, ProbeLimits.MinPacketSize, ProbeLimits.MaxPacketSize);
        Range(CommandLineSettingsArg.Attempts, bag.Attempts, 1, 1000);
        Range(CommandLineSettingsArg.LossDelay, bag.LossDelayMs, 0, 60000);
        Range(CommandLineSettingsArg.ProbeTimeout, bag.ProbeTimeoutMs, 100, 3000);
    }

    private static void Range(string name, int value, int min, int max)
    {
        if (value < min || value > max)
            throw new CommandLineException(TextFormatter.Fmt(Strings.T(Strings.Console.RangeFormat), name, min, max));
    }

    private static Invocation Build(string command, OptionsBag bag) =>
        new(command, bag.Host, bag.PacketSize, bag.Attempts, bag.LossDelayMs,
            bag.Dynamic, bag.DontFragment, bag.Family, bag.Format, bag.Language, bag.Reset, false,
            bag.RipeLookup, bag.RoutePrecheck, bag.DeepRoutes, bag.ProbeTimeoutMs, bag.AlternativeDns);
}
