using System.Net;
using TraceDiag.Services.Diagnostics;

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
    private readonly List<IPAddress> _responders = new();

    public required int Ttl { get; init; }
    public required IPAddress Address { get; init; }
    public string? HostName { get; set; }

    public int Sent { get; private set; }
    public int Received { get; private set; }
    public long TotalRttMs { get; private set; }
    public string Status { get; set; } = Strings.Status.Pending;
    public bool IsSkipped { get; set; }
    public string Diagnosis { get; set; } = Strings.Diagnosis.None;

    /// <summary>Set after a lost ping; all later probes for this host are paced.</summary>
    public bool LossDelayEnabled { get; internal set; }

    /// <summary>Every successful RTT measurement, in milliseconds.</summary>
    public IReadOnlyList<long> RttSamples => _rttSamples;

    /// <summary>Source addresses of successful replies, with consecutive duplicates collapsed.</summary>
    public IReadOnlyList<IPAddress> Responders => _responders;

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

    /// <summary>Mean absolute change between consecutive successful RTT samples.</summary>
    public double JitterMs => RttMath.Jitter(_rttSamples);

    /// <summary>Population standard deviation of all successful RTT samples.</summary>
    public double StdDevMs => RttMath.StdDev(_rttSamples);

    public bool HasLoss => Lost > 0;
    public bool Responded => Received > 0;

    /// <summary>Corrects this node's statistics after every single ping.</summary>
    public void ApplyPing(bool success, long roundtripTimeMs, IPAddress? address = null)
    {
        Sent++;
        if (success) ApplySuccess(roundtripTimeMs, address);
        else ApplyLoss();
        RefreshStatus();
    }

    private void ApplyLoss()
    {
        LastPingMs = null;
        LossDelayEnabled = true;
    }

    private void ApplySuccess(long roundtripTimeMs, IPAddress? address)
    {
        var rtt = Math.Max(0, roundtripTimeMs);
        Received++;
        TotalRttMs += rtt;
        _rttSamples.Add(rtt);
        LastPingMs = rtt;
        RecordResponder(address);
    }

    private void RecordResponder(IPAddress? address)
    {
        if (address is null || (_responders.Count > 0 && _responders[_responders.Count - 1].Equals(address))) return;
        _responders.Add(address);
    }

    private void RefreshStatus() =>
        Status = HopStatusCalculator.Calculate(Received, Lost, LossPercent);
}
