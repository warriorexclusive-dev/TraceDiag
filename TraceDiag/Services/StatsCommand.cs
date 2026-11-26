using TraceDiag.Models;

namespace TraceDiag.Services;

/// <summary>Commands processed by the statistics worker, one per published update.</summary>
internal abstract record StatsCommand
{
    internal sealed record Replace(IReadOnlyList<HopStatSnapshot> Items) : StatsCommand;

    internal sealed record Append(IReadOnlyList<HopStatSnapshot> Items) : StatsCommand;

    internal sealed record Upsert(HopStatSnapshot Item) : StatsCommand;

    internal sealed record Clear : StatsCommand;
}
