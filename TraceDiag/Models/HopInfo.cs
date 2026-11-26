using System.Net;

namespace TraceDiag.Models;

/// <summary>
/// In-memory statistics for one traced node. One instance per hop is kept in a list
/// and corrected on every ICMP ping via <see cref="ApplyPing"/>.
/// All successful RTT samples are stored in <see cref="RttSamples"/> so average/min/max
/// are calculated exactly from raw data instead of approximated values.
/// </summary>
public sealed class HopInfo
{
    private readonly List<long> _rttSamples = new();

    public required int Ttl { get; init; }
    public required IPAddress Address { get; init; }
    public string? HostName { get; set; }

    public int Sent { get; private set; }
    public int Received { get; private set; }
    public long TotalRttMs { get; private set; }
    public string Status { get; set; } = Strings.Status.Pending;
    public bool IsSkipped { get; set; }

    /// <summary>Set after a lost ping; all later probes for this host are paced.</summary>
    public bool LossDelayEnabled { get; internal set; }

    /// <summary>Every successful RTT measurement, in milliseconds.</summary>
    public IReadOnlyList<long> RttSamples => _rttSamples;

    /// <summary>RTT of the latest ping; null means the last request timed out.</summary>
    public long? LastPingMs { get; private set; }

    public int Lost => Math.Max(0, Sent - Received);
    public double LossPercent => Sent == 0 ? 0 : (Lost * 100.0 / Sent);

    /// <summary>Exact arithmetic mean of all stored RTT samples.</summary>
    public double AvgPingMs
    {
        get
        {
            if (_rttSamples.Count == 0) return 0;
            long sum = 0;
            foreach (var sample in _rttSamples) sum += sample;
            return (double)sum / _rttSamples.Count;
        }
    }

    public long? MinPingMs => _rttSamples.Count == 0 ? null : _rttSamples.Min();
    public long? MaxPingMs => _rttSamples.Count == 0 ? null : _rttSamples.Max();

    public bool HasLoss => Lost > 0;
    public bool Responded => Received > 0;

    /// <summary>Corrects this node's statistics after every single ping.</summary>
    public void ApplyPing(bool success, long roundtripTimeMs)
    {
        Sent++;
        if (success) ApplySuccess(roundtripTimeMs);
        else ApplyLoss();
        RefreshStatus();
    }

    private void ApplyLoss()
    {
        LastPingMs = null;
        LossDelayEnabled = true;
    }

    /// <summary>Resets per-size statistics so the table shows only the last packet size results.</summary>
    public void ResetStats()
    {
        Sent = 0;
        Received = 0;
        TotalRttMs = 0;
        _rttSamples.Clear();
        LastPingMs = null;
        Status = Strings.Status.Pending;
    }

    private void ApplySuccess(long roundtripTimeMs)
    {
        var rtt = Math.Max(0, roundtripTimeMs);
        Received++;
        TotalRttMs += rtt;
        _rttSamples.Add(rtt);
        LastPingMs = rtt;
    }

    private void RefreshStatus() =>
        Status = HopStatusCalculator.Calculate(Received, Lost, LossPercent);
}
