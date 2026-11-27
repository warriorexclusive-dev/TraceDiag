using TraceDiag.Services;

namespace TraceDiag.Tests;

public sealed class RipeWhoisParserTests
{
    [Fact]
    public void Parse_PrefersRegistrationOwner()
    {
        var json = """
            {"status":"ok","data":{"records":[[
              {"key":"NetName","value":"GOGL"},
              {"key":"Organization","value":"Google LLC (GOGL)"},
              {"key":"OrgName","value":"Google LLC"}
            ]],"irr_records":[[{"key":"descr","value":"Google"}]]}}
            """;
        Assert.Equal("Google LLC", RipeWhoisParser.Parse(json));
    }

    [Fact]
    public void Parse_UsesIrrDescriptionWhenNoOwner()
    {
        var json = """
            {"status":"ok","data":{"records":[],"irr_records":[[{"key":"descr","value":"Example Net"}]]}}
            """;
        Assert.Equal("Example Net", RipeWhoisParser.Parse(json));
    }

    [Theory]
    [InlineData("{\"status\":\"error\",\"data\":{}}")]
    [InlineData("not json")]
    public void Parse_ReturnsNullOnBadResponse(string json) =>
        Assert.Null(RipeWhoisParser.Parse(json));
}
