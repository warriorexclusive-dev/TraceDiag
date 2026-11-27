using TraceDiag.Models;

namespace TraceDiag.Console.Tests;

public sealed class CommandLineParserTests
{
    [Fact]
    public void Parse_Run_UsesDefaults()
    {
        var options = CommandLineParser.Parse(["run", "--host", "example.com"]);
        Assert.Equal("run", options.Command);
        Assert.Equal("example.com", options.Host);
        Assert.Equal(ConsoleDefaults.PacketSize, options.PacketSize);
        Assert.Equal(ConsoleDefaults.Attempts, options.Attempts);
        Assert.False(options.Dynamic);
        Assert.Equal(IpFamily.Auto, options.Family);
        Assert.True(options.RipeLookup);
        Assert.True(options.RoutePrecheck);
        Assert.False(options.DeepRoutes);
        Assert.Equal(3000, options.ProbeTimeoutMs);
    }

    [Fact]
    public void Parse_Run_AppliesNoRipe()
    {
        var options = CommandLineParser.Parse(["run", "--host", "example.com", "--no-ripe"]);
        Assert.False(options.RipeLookup);
    }

    [Fact]
    public void Parse_Run_AppliesRoutePrecheckOptions()
    {
        var options = CommandLineParser.Parse(["run", "--host", "example.com",
            "--no-route-precheck", "--deep-routes", "--probe-timeout", "1200"]);
        Assert.False(options.RoutePrecheck);
        Assert.True(options.DeepRoutes);
        Assert.Equal(1200, options.ProbeTimeoutMs);
    }

    [Theory]
    [InlineData("--ipv4", IpFamily.IPv4)]
    [InlineData("--ipv6", IpFamily.IPv6)]
    public void Parse_Run_AppliesIpFamily(string option, IpFamily expected)
    {
        var options = CommandLineParser.Parse(["run", "--host", "example.com", option]);
        Assert.Equal(expected, options.Family);
    }

    [Fact]
    public void Parse_Run_AppliesFlagsAndValues()
    {
        var options = CommandLineParser.Parse(["run", "--host", "example.com", "--packet-size", "512",
            "--attempts", "5", "--loss-delay", "20", "--dynamic", "--df", "--format", "json"]);
        Assert.Equal(512, options.PacketSize);
        Assert.Equal(5, options.Attempts);
        Assert.Equal(20, options.LossDelayMs);
        Assert.True(options.Dynamic);
        Assert.True(options.DontFragment);
        Assert.Equal(Strings.Console.FormatJson, options.Format);
    }

    [Fact]
    public void Parse_Reset_NeedsNoHost() =>
        Assert.Equal("reset", CommandLineParser.Parse(["reset"]).Command);

    [Fact]
    public void Parse_Help_ShortCircuits() =>
        Assert.True(CommandLineParser.Parse(["run", "--help"]).Help);

    [Theory]
    [InlineData("--packet-size", "63")]
    [InlineData("--attempts", "1001")]
    [InlineData("--loss-delay", "60001")]
    [InlineData("--probe-timeout", "3001")]
    public void Parse_OutOfRange_Throws(string option, string value) =>
        Assert.Throws<CommandLineException>(() =>
            CommandLineParser.Parse(["run", "--host", "example.com", option, value]));

    [Fact]
    public void Parse_MissingHost_Throws() =>
        Assert.Throws<CommandLineException>(() => CommandLineParser.Parse(["run"]));

    [Fact]
    public void Parse_UnknownOption_Throws() =>
        Assert.Throws<CommandLineException>(() =>
            CommandLineParser.Parse(["run", "--host", "example.com", "--unknown"]));

    [Fact]
    public void Parse_UnknownOption_AttachesSuggestionAndOptionList()
    {
        var error = Assert.Throws<CommandLineException>(() =>
            CommandLineParser.Parse(["run", "--host", "example.com", "-attempts", "5"]));
        Assert.Equal(TextFormatter.Fmt(Strings.Console.UnknownOptionFormat, "-attempts"), error.Message);
        Assert.Contains(CommandLineSettingsArg.Attempts, error.Details!);
        Assert.Contains(Strings.Console.OptionsTitle, error.Details!);
    }
}
