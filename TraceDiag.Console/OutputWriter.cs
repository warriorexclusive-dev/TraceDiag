using System.Globalization;
using System.Text.Json;
using TraceDiag.Models;
using TraceDiag.Services.Diagnostics;

namespace TraceDiag.Console;

/// <summary>Prints log lines as they happen; the statistics table comes last.</summary>
internal sealed class OutputWriter(string format, TextWriter output) : IProgress<string>
{
    private readonly StatsCollector _collector = new();
    private bool _summaryWritten;

    public void Report(string value) { }

    internal void Stats(IReadOnlyList<HopStatSnapshot> hops) => _collector.Add(hops);

    internal void Stats(HopStatSnapshot hop) => _collector.Add(hop);

    internal void LogLine(string line)
    {
        if (format == Strings.Console.FormatJson) output.WriteLine(JsonMessage(line));
        else if (format == Strings.Console.FormatCsv) output.WriteLine(Csv(Strings.Console.KindMessage, line));
        else output.WriteLine(line);
        output.Flush();
    }

    internal void WriteSummary()
    {
        if (_summaryWritten) return;
        var hops = _collector.Snapshot();
        if (hops.Count == 0) return;
        _summaryWritten = true;
        if (format == Strings.Console.FormatJson) { output.WriteLine(JsonStats(hops)); return; }
        if (format == Strings.Console.FormatCsv) { CsvStats(hops); return; }
        PlainStats(hops);
        output.Flush();
    }

    private void PlainStats(IReadOnlyList<HopStatSnapshot> hops)
    {
        output.WriteLine(Strings.T(Strings.Console.SummaryTitle));
        output.WriteLine(Strings.T(Strings.Console.StatsHeader));
        foreach (var hop in hops) output.WriteLine(TextFormatter.Fmt(Strings.Console.StatsRowFormat,
            hop.Ttl, hop.Address, Host(hop), Average(hop), Metric(hop, hop.JitterMs),
            Metric(hop, hop.StdDevMs), Text(hop.MinPingMs), Text(hop.MaxPingMs),
            Text(hop.LastPingMs), hop.Sent, hop.Received, Status(hop), Diagnosis(hop)));
    }

    private void CsvStats(IReadOnlyList<HopStatSnapshot> hops)
    {
        output.WriteLine(Strings.Console.CsvStatsHeader);
        foreach (var hop in hops) output.WriteLine(Csv(Strings.Console.KindStats, hop.Ttl, hop.Address,
            Host(hop), Average(hop), Metric(hop, hop.JitterMs), Metric(hop, hop.StdDevMs),
            Text(hop.MinPingMs), Text(hop.MaxPingMs), Text(hop.LastPingMs),
            hop.Sent, hop.Received, Status(hop), Diagnosis(hop)));
    }

    private string JsonMessage(string value) =>
        JsonSerializer.Serialize(new Dictionary<string, string>
        {
            [Strings.Console.ColumnKind] = Strings.Console.KindMessage,
            [Strings.Console.ColumnText] = value
        });

    private string JsonStats(IReadOnlyList<HopStatSnapshot> hops) =>
        JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            [Strings.Console.ColumnKind] = Strings.Console.KindStats,
            [Strings.Console.ColumnHops] = hops.Select(HopFields).ToArray()
        });

    private static Dictionary<string, object?> HopFields(HopStatSnapshot hop) => new()
    {
        [Strings.Console.ColumnTtl] = hop.Ttl,
        [Strings.Console.ColumnIp] = hop.Address.ToString(),
        [Strings.Console.ColumnHost] = Host(hop),
        [Strings.Console.ColumnAvg] = Average(hop),
        [Strings.Console.ColumnJitter] = Metric(hop, hop.JitterMs),
        [Strings.Console.ColumnStdDev] = Metric(hop, hop.StdDevMs),
        [Strings.Console.ColumnMin] = Text(hop.MinPingMs),
        [Strings.Console.ColumnMax] = Text(hop.MaxPingMs),
        [Strings.Console.ColumnLast] = Text(hop.LastPingMs),
        [Strings.Console.ColumnSent] = hop.Sent,
        [Strings.Console.ColumnReceived] = hop.Received,
        [Strings.Console.ColumnStatus] = Status(hop),
        [Strings.Console.ColumnDiagnosis] = Diagnosis(hop)
    };

    private static string Csv(params object?[] values) =>
        string.Join(",", values.Select(value => "\"" + (value?.ToString() ?? string.Empty).Replace("\"", "\"\"") + "\""));

    private static string Host(HopStatSnapshot hop) =>
        string.IsNullOrWhiteSpace(hop.HostName) ? Strings.T(Strings.Ui.NoDns) : hop.HostName!;

    private static string Status(HopStatSnapshot hop) => Strings.T(hop.Status);

    private static string Diagnosis(HopStatSnapshot hop) => DiagnosisText.Localize(hop.Diagnosis);

    private static string Average(HopStatSnapshot hop) => Metric(hop, hop.AvgPingMs);

    private static string Metric(HopStatSnapshot hop, double value) =>
        hop.Received == 0 ? Strings.Ui.Dash
            : value.ToString(Strings.Ui.AvgPingFormat, CultureInfo.InvariantCulture);

    private static string Text(long? value) =>
        value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
}
