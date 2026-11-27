namespace TraceDiag.Console.Tests;

public sealed class InteractiveHostPromptTests
{
    [Fact]
    public void Read_InvalidHost_AsksAgain()
    {
        using var input = new StringReader($"bad host{Environment.NewLine}10.0.0.1{Environment.NewLine}");
        using var output = new StringWriter();
        var host = new InteractiveHostPrompt(input, output).Read();

        Assert.Equal("10.0.0.1", host);
        Assert.Equal(2, Count(output.ToString(), Strings.T(Strings.Console.EnterHost)));
        Assert.Contains(Strings.T(Strings.Console.InvalidHost), output.ToString());
    }

    [Fact]
    public void Read_ValidDomain_ReturnsIt()
    {
        using var input = new StringReader($"example.com{Environment.NewLine}");
        using var output = new StringWriter();

        Assert.Equal("example.com", new InteractiveHostPrompt(input, output).Read());
    }

    [Fact]
    public void Read_ClosedInput_ReturnsNullAndExplains()
    {
        using var input = new StringReader(string.Empty);
        using var output = new StringWriter();

        Assert.Null(new InteractiveHostPrompt(input, output).Read());
        Assert.Contains(Strings.T(Strings.Console.InputClosed), output.ToString());
    }

    [Theory]
    [InlineData("127.0.0.1", true)]
    [InlineData("::1", true)]
    [InlineData("example.com", true)]
    [InlineData("sub.example.co.uk", true)]
    [InlineData("localhost", true)]
    [InlineData("example.com.", true)]
    [InlineData("999.999.999.999", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("bad host", false)]
    [InlineData("-bad.com", false)]
    [InlineData("bad-.com", false)]
    [InlineData("a..b", false)]
    [InlineData("a_b.com", false)]
    public void IsValid_ChecksSyntax(string value, bool expected) =>
        Assert.Equal(expected, HostNameValidator.IsValid(value));

    [Fact]
    public void Prepare_NoArguments_UsesDefaultsWithEnteredHost()
    {
        using var input = new StringReader($"example.com{Environment.NewLine}");
        using var output = new StringWriter();

        var args = InteractiveLauncher.Prepare([], input, output);

        Assert.Equal([CommandLineSettingsArg.Run, CommandLineSettingsArg.Host, "example.com"], args!);
        var options = CommandLineParser.Parse(args!);
        Assert.Equal("example.com", options.Host);
        Assert.Equal(ConsoleDefaults.Attempts, options.Attempts);
        Assert.Equal(ConsoleDefaults.PacketSize, options.PacketSize);
        Assert.True(options.RoutePrecheck);
    }

    [Fact]
    public void Prepare_WithArguments_KeepsThemUntouched()
    {
        string[] args = ["run", "--host", "example.com", "--attempts", "5"];
        using var input = new StringReader(string.Empty);
        using var output = new StringWriter();

        Assert.Same(args, InteractiveLauncher.Prepare(args, input, output));
        Assert.Equal(string.Empty, output.ToString());
    }

    [Fact]
    public void Prepare_ClosedInput_ReturnsNull()
    {
        using var input = new StringReader(string.Empty);
        using var output = new StringWriter();

        Assert.Null(InteractiveLauncher.Prepare([], input, output));
    }

    private static int Count(string text, string value) =>
        value.Length == 0 ? 0 : (text.Length - text.Replace(value, string.Empty).Length) / value.Length;
}
