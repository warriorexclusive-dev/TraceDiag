using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class DiagnosticTextAndReporterTests
{
    [Fact]
    public void Hop_UsesNoDnsFallback()
    {
        var text = DiagnosticText.Hop(TestFactories.Hop(1, 1));
        Assert.Contains(Strings.Diag.NoDns, text);
        Assert.Contains("10.0.0.1", text);
    }

    [Fact]
    public void PingAccessors_UseFallbacksForMissingSamples()
    {
        var hop = TestFactories.Hop(1, 1);
        Assert.Equal(Strings.Diag.NoAnswer, DiagnosticText.Last(hop));
        Assert.Equal(Strings.Diag.Dash, DiagnosticText.Min(hop));
        Assert.Equal(Strings.Diag.Dash, DiagnosticText.Max(hop));
    }

    [Fact]
    public void Ms_FormatsWithOneDecimal()
    {
        Assert.Equal("12.3 ms", DiagnosticText.Ms(12.34));
    }

    [Fact]
    public void Report_SendsStampedMessageAndLogsIt()
    {
        var logger = new FakeDiagnosticLogger();
        var context = CreateContext(logger);
        var progress = TestFactories.TextProgress();
        DiagnosticReporter.Report(context, progress, "message", isWarn: true);
        Assert.Single(progress.Values);
        Assert.Contains("message", progress.Values[0]);
        Assert.Single(logger.Warnings);
    }

    [Fact]
    public void ReportHeader_DynamicListsAllSizes()
    {
        var progress = TestFactories.TextProgress();
        var input = CreateInput(true);
        DiagnosticReporter.ReportHeader(CreateContext(null), progress, input, [64, 128]);
        Assert.Contains(progress.Values, value => value.Contains("Packet sizes (x2): 64, 128"));
    }

    [Fact]
    public void ReportHeader_FixedListsOnlyMaxSize()
    {
        var progress = TestFactories.TextProgress();
        var input = CreateInput(false);
        DiagnosticReporter.ReportHeader(CreateContext(null), progress, input, [512]);
        Assert.Contains(progress.Values, value => value.Contains("Packet size (fixed): 512 bytes"));
    }

    [Fact]
    public void ReportStats_MapsHopsToSnapshots()
    {
        var progress = TestFactories.StatsProgress();
        var hop = TestFactories.Hop(1, 1);
        hop.ApplyPing(true, 10);
        hop.Diagnosis = Strings.Diagnosis.AntiFlood;
        DiagnosticReporter.ReportStats(progress, [hop]);
        var snapshot = Assert.Single(progress.Values[0]);
        Assert.Equal(10, snapshot.AvgPingMs);
        Assert.Equal(1, snapshot.Received);
        Assert.Equal(Strings.Diagnosis.AntiFlood, snapshot.Diagnosis);
    }

    private static DiagnosticContext CreateContext(FakeDiagnosticLogger? logger) =>
        TestFactories.Context(new FakeProbeClient(_ => TestFactories.Success(new ProbeCall(TestFactories.Address(1), 64, 64))), logger);

    private static DiagnosticInput CreateInput(bool dynamic) =>
        new(TestFactories.Address(2), "target", 512, 2, 0, dynamic);
}
