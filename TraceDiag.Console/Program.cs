using TraceDiag.Services;

namespace TraceDiag.Console;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (!TryParse(args, out var options, out var code)) return code;
        if (options!.Help) { Help(); return 0; }
        using var cancellation = new CancellationTokenSource();
        System.Console.CancelKeyPress += (_, eventArgs) => Cancel(eventArgs, cancellation);
        return Execute(options, cancellation.Token);
    }

    private static bool TryParse(string[] args, out Invocation? options, out int code)
    {
        try { options = CommandLineParser.Parse(args); code = 0; return true; }
        catch (CommandLineException ex) { Report(ex); options = null; code = 2; return false; }
    }

    private static void Report(CommandLineException ex)
    {
        System.Console.Error.WriteLine(ex.Message);
        if (ex.Details is not null) System.Console.Error.WriteLine(ex.Details);
        else Help();
    }

    private static void Cancel(ConsoleCancelEventArgs eventArgs, CancellationTokenSource cancellation)
    {
        eventArgs.Cancel = true;
        cancellation.Cancel();
    }

    private static int Execute(Invocation options, CancellationToken token)
    {
        var appData = AppFiles.GetAppDataDirectory();
        if (options.Reset) AppFiles.CreateUiStateStore(appData).Reset();
        if (options.Command == "reset") return Reset();
        using var file = AppFiles.CreateLogger(appData);
        var writer = new OutputWriter(options.Format, System.Console.Out);
        var logger = new ConsoleLogger(file, writer.LogLine);
        WriteStartup(logger, file);
        return new ConsoleRunner(options, writer, logger).RunAsync(token).GetAwaiter().GetResult();
    }

    private static int Reset()
    {
        System.Console.Out.WriteLine(Strings.Console.ResetDone);
        return 0;
    }

    private static void WriteStartup(ConsoleLogger logger, FileDiagnosticLogger file)
    {
        logger.Info(Strings.Log.ApplicationStarted);
        if (Environment.OSVersion.Platform == PlatformID.Unix) logger.Info(Strings.Console.LinuxIcmpHint);
        logger.Info(TextFormatter.Fmt(Strings.Console.LogStartedFormat, file.LogFilePath));
    }

    private static void Help()
    {
        System.Console.Out.WriteLine(Strings.Console.Usage);
        System.Console.Out.WriteLine(Strings.Console.HelpRun);
        System.Console.Out.WriteLine(Strings.Console.HelpMonitor);
        System.Console.Out.WriteLine(Strings.Console.HelpReset);
        OptionSuggestions.Write(System.Console.Out, null);
    }
}
