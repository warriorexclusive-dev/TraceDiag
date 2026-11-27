namespace TraceDiag.Console.Tests;

public sealed class OptionSuggestionsTests
{
    [Fact]
    public void Closest_SingleDashOption_ReturnsCanonical() =>
        Assert.Equal(ConsoleOptions.Attempts, OptionSuggestions.Closest("-attempts"));

    [Fact]
    public void Closest_Typo_ReturnsCanonical() =>
        Assert.Equal(ConsoleOptions.Format, OptionSuggestions.Closest("--formt"));

    [Fact]
    public void Closest_UnrelatedOption_ReturnsNull() =>
        Assert.Null(OptionSuggestions.Closest("--unknown"));

    [Fact]
    public void List_ContainsSuggestionAndAllOptions()
    {
        var text = OptionSuggestions.List(ConsoleOptions.Attempts);
        Assert.Contains(TextFormatter.Fmt(Strings.Console.DidYouMeanFormat, ConsoleOptions.Attempts), text);
        Assert.Contains(Strings.Console.OptionsTitle, text);
        foreach (var usage in ConsoleOptions.Usage) Assert.Contains(usage, text);
    }
}
