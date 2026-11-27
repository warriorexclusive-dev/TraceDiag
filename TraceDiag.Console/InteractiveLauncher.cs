namespace TraceDiag.Console;

/// <summary>Turns a no-argument launch into a default run with the entered host.</summary>
internal static class InteractiveLauncher
{
    internal static string[]? Prepare(string[] args, TextReader input, TextWriter output)
    {
        if (args.Length != 0) return args;
        var host = new InteractiveHostPrompt(input, output).Read();
        return host is null ? null : [CommandLineSettingsArg.Run, CommandLineSettingsArg.Host, host];
    }
}
