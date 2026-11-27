using System.Globalization;
using TraceDiag.Models;

namespace TraceDiag.Console;

internal static class CommandLineParser
{
    internal static Invocation Parse(string[] args)
    {
        if (args.Length == 0 || args.Any(IsHelp)) return Help();
        RequireCommand(args[0]);
        var values = Read(args, 1);
        Validate(args[0], values);
        return Build(args[0], values);
    }

    private static Invocation Help() =>
        new("help", string.Empty, ConsoleDefaults.PacketSize, ConsoleDefaults.Attempts,
            ConsoleDefaults.LossDelayMs, false, false, ConsoleDefaults.Format, false, true);

    private static bool NeedsHost(string command) => command is "run" or "monitor";

    private static void RequireCommand(string command)
    {
        if (command is "run" or "monitor" or "reset") return;
        throw new CommandLineException(TextFormatter.Fmt(Strings.Console.InvalidCommandFormat, command));
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
            case ConsoleOptions.Dynamic: bag.Dynamic = true; return;
            case ConsoleOptions.DontFragment: bag.DontFragment = true; return;
            case ConsoleOptions.Reset: bag.Reset = true; return;
            default: ApplyValue(args, ref index, bag); return;
        }
    }

    private static void ApplyValue(string[] args, ref int index, OptionsBag bag)
    {
        var option = args[index];
        switch (option)
        {
            case ConsoleOptions.Host: bag.Host = Next(args, ref index, option); return;
            case ConsoleOptions.PacketSize: bag.PacketSize = Number(option, Next(args, ref index, option)); return;
            case ConsoleOptions.Attempts: bag.Attempts = Number(option, Next(args, ref index, option)); return;
            case ConsoleOptions.LossDelay: bag.LossDelayMs = Number(option, Next(args, ref index, option)); return;
            case ConsoleOptions.Format: bag.Format = Next(args, ref index, option); return;
            default: throw Unknown(option);
        }
    }

    private static CommandLineException Unknown(string option) =>
        new(TextFormatter.Fmt(Strings.Console.UnknownOptionFormat, option),
            OptionSuggestions.List(OptionSuggestions.Closest(option)));

    private static string Next(string[] args, ref int index, string option)
    {
        if (++index >= args.Length) throw new CommandLineException(TextFormatter.Fmt(Strings.Console.MissingValueFormat, option));
        return args[index];
    }

    private static int Number(string option, string value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            ? number
            : throw new CommandLineException(TextFormatter.Fmt(Strings.Console.InvalidNumberFormat, option, value));

    private static bool IsHelp(string argument) =>
        argument is ConsoleOptions.Help or Strings.Console.HelpShort;

    private static void Validate(string command, OptionsBag bag)
    {
        if (NeedsHost(command) && string.IsNullOrWhiteSpace(bag.Host))
            throw new CommandLineException(Strings.Console.HostRequired);
        ValidateRanges(bag);
        if (bag.Format is not (Strings.Console.FormatPlain or Strings.Console.FormatCsv or Strings.Console.FormatJson))
            throw new CommandLineException(Strings.Console.InvalidFormat);
    }

    private static void ValidateRanges(OptionsBag bag)
    {
        Range(ConsoleOptions.PacketSize, bag.PacketSize, ProbeLimits.MinPacketSize, ProbeLimits.MaxPacketSize);
        Range(ConsoleOptions.Attempts, bag.Attempts, 1, 1000);
        Range(ConsoleOptions.LossDelay, bag.LossDelayMs, 0, 60000);
    }

    private static void Range(string name, int value, int min, int max)
    {
        if (value < min || value > max)
            throw new CommandLineException(TextFormatter.Fmt(Strings.Console.RangeFormat, name, min, max));
    }

    private static Invocation Build(string command, OptionsBag bag) =>
        new(command, bag.Host, bag.PacketSize, bag.Attempts, bag.LossDelayMs,
            bag.Dynamic, bag.DontFragment, bag.Format, bag.Reset, false);
}
