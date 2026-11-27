namespace TraceDiag.Console;

/// <summary>Asks for a host until a valid one is entered or input is closed.</summary>
internal sealed class InteractiveHostPrompt(TextReader input, TextWriter output)
{
    internal string? Read()
    {
        while (true)
        {
            output.Write(Strings.T(Strings.Console.EnterHost));
            var line = input.ReadLine();
            if (line is null) return Closed();
            var host = line.Trim();
            if (HostNameValidator.IsValid(host)) return host;
            output.WriteLine(Strings.T(Strings.Console.InvalidHost));
        }
    }

    private string? Closed()
    {
        output.WriteLine(Strings.T(Strings.Console.InputClosed));
        return null;
    }
}
