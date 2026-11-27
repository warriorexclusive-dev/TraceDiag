using System.Net;
using TraceDiag.Models;

namespace TraceDiag.Console.Tests;

public sealed class OutputWriterTests
{
    [Fact]
    public void ReportAndStats_DoNotWriteBeforeSummary()
    {
        using var text = new StringWriter();
        var writer = new OutputWriter(Strings.Console.FormatPlain, text);
        writer.Report("hello");
        writer.Stats([Snapshot()]);
        Assert.Equal(string.Empty, text.ToString());
    }

    [Fact]
    public void Plain_LogLine_WritesImmediately()
    {
        using var text = new StringWriter();
        var writer = new OutputWriter(Strings.Console.FormatPlain, text);
        writer.LogLine("first");
        writer.Stats([Snapshot()]);
        writer.LogLine("second");
        writer.WriteSummary();
        var value = text.ToString();
        Assert.True(value.IndexOf("first", StringComparison.Ordinal) <
            value.IndexOf(Strings.Console.SummaryTitle, StringComparison.Ordinal));
        Assert.Contains("second", value);
        Assert.Contains("127.0.0.1", value);
    }

    [Fact]
    public void Plain_SummaryIsIdempotentAndKeepsLatest()
    {
        using var text = new StringWriter();
        var writer = new OutputWriter(Strings.Console.FormatPlain, text);
        writer.Stats([Snapshot(1)]);
        writer.Stats([Snapshot(2)]);
        writer.WriteSummary();
        writer.WriteSummary();
        Assert.Equal(1, Count(text.ToString(), Strings.Console.SummaryTitle));
        Assert.Equal(1, Count(text.ToString(), Strings.Console.StatsHeader));
        Assert.Contains("127.0.0.2", text.ToString());
    }

    [Fact]
    public void Csv_LogLineAndSummary()
    {
        using var text = new StringWriter();
        var writer = new OutputWriter(Strings.Console.FormatCsv, text);
        writer.LogLine("hello");
        writer.Stats([Snapshot()]);
        writer.WriteSummary();
        Assert.Contains("\"message\",\"hello\"", text.ToString());
        Assert.Contains(Strings.Console.CsvStatsHeader, text.ToString());
    }

    [Fact]
    public void Json_LogLineAndSummary()
    {
        using var text = new StringWriter();
        var writer = new OutputWriter(Strings.Console.FormatJson, text);
        writer.LogLine("hello");
        writer.Stats([Snapshot()]);
        writer.WriteSummary();
        Assert.Contains("\"kind\":\"message\"", text.ToString());
        Assert.Contains("\"kind\":\"stats\"", text.ToString());
    }

    [Fact]
    public void WriteSummary_WithoutStats_DoesNothing()
    {
        using var text = new StringWriter();
        new OutputWriter(Strings.Console.FormatPlain, text).WriteSummary();
        Assert.Equal(string.Empty, text.ToString());
    }

    private static int Count(string value, string pattern) =>
        System.Text.RegularExpressions.Regex.Matches(
            value, System.Text.RegularExpressions.Regex.Escape(pattern)).Count;

    private static HopStatSnapshot Snapshot(int lastOctet = 1) => new(
        1, IPAddress.Parse($"127.0.0.{lastOctet}"), "localhost", 1, 1, 0, 0, 0, 0,
        Strings.Status.Normal, Strings.Diagnosis.None);
}
