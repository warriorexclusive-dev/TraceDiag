namespace TraceDiag.Console.Tests;

public sealed class OptionSuggestionsTests
{
    [Fact]
    public void Closest_SingleDashOption_ReturnsCanonical() =>
        Assert.Equal(CommandLineSettingsArg.Attempts, OptionSuggestions.Closest("-attempts"));

    [Fact]
    public void Closest_Typo_ReturnsCanonical() =>
        Assert.Equal(CommandLineSettingsArg.Format, OptionSuggestions.Closest("--formt"));

    [Fact]
    public void Closest_UnrelatedOption_ReturnsNull() =>
        Assert.Null(OptionSuggestions.Closest("--unknown"));

    [Fact]
    public void List_ContainsSuggestionAndAllOptions()
    {
        var text = OptionSuggestions.List(CommandLineSettingsArg.Attempts);
        Assert.Contains(TextFormatter.Fmt(Strings.Console.DidYouMeanFormat, CommandLineSettingsArg.Attempts), text);
        Assert.Contains(Strings.Console.OptionsTitle, text);
        foreach (var usage in OptionSuggestions.Usage) Assert.Contains(usage, text);
    }
}
