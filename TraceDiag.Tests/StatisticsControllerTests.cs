using TraceDiag.Models;
using TraceDiag.Services;
using TraceDiag.Tests.TestData;

namespace TraceDiag.Tests;

public sealed class StatisticsControllerTests
{
    [Fact]
    public void Append_DropsOldestEntriesWhenOverLimit()
    {
        using var controller = new StatisticsController(3);
        for (var i = 1; i <= 5; i++) controller.Append(Stat(i));
        controller.Flush();
        Assert.Equal(3, controller.Count);
    }

    [Fact]
    public void Replace_ThenClear_UpdatesHistory()
    {
        using var controller = new StatisticsController(10);
        controller.Replace([Stat(1), Stat(2)]);
        controller.Flush();
        Assert.Equal(2, controller.Count);
        controller.Clear();
        controller.Flush();
        Assert.Equal(0, controller.Count);
    }

    [Fact]
    public void Upsert_ReplacesSameHopInsteadOfAddingRow()
    {
        IReadOnlyList<HopStatSnapshot>? published = null;
        using var controller = new StatisticsController(10, items => published = items);
        controller.Upsert(Stat(1));
        controller.Upsert(Stat(1) with { Sent = 2, Received = 2 });
        controller.Flush();
        Assert.Equal(1, controller.RowCount);
        Assert.Equal(2, controller.Count);
        Assert.Equal(2, published![0].Sent);
    }

    [Fact]
    public void Append_PublishesSnapshotsToConsumer()
    {
        var published = 0;
        using var controller = new StatisticsController(10, _ => published++);
        controller.Append(Stat(1));
        controller.Flush();
        Assert.Equal(1, published);
    }

    private static HopStatSnapshot Stat(int address) =>
        new(1, TestFactories.Address(address), null, 1, 1, 5, 5, 5, 5, Strings.Status.Normal, Strings.Diagnosis.None);
}
