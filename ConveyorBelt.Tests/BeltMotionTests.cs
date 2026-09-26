namespace ConveyorBelt.Tests;

public class BeltMotionTests
{
    [Fact]
    public void LogSpeedAdvancesOneSpacingPerHour()
    {
        var delta = BeltRates.Delta(1f, 1f);
        Assert.Equal(BeltRates.Spacing, delta, 3);
    }

    [Fact]
    public void ItemBehindStopsAtSpacing()
    {
        List<BeltGood> items = [new("Log", 1f), new("Log", 0.9f)];
        var stuck = BeltMotion.Advance(items, 0.5f, _ => false);

        Assert.True(stuck);
        Assert.Equal(1f, items[0].Position, 3);
        Assert.Equal(1f - BeltRates.Spacing, items[1].Position, 3);
    }

    [Fact]
    public void HeadLeavesAndTheNextGoodFollows()
    {
        List<BeltGood> items = [new("Plank", 0.9f), new("Gear", 0.2f)];
        var left = new List<string>();
        BeltMotion.Advance(items, 0.5f, id =>
        {
            left.Add(id);
            return true;
        });

        Assert.Equal(["Plank"], left);
        Assert.Single(items);
        Assert.Equal("Gear", items[0].Id);
        Assert.Equal(0.7f, items[0].Position, 3);
    }

    [Fact]
    public void FastBeltAcceptsSeveralGoodsInOneTick()
    {
        List<BeltGood> items = [new("Log", 0.5f)];

        Assert.True(BeltMotion.TryArrival(items, 0.5f, out var first));
        items.Add(new("Plank", first));
        Assert.True(BeltMotion.TryArrival(items, 0.5f, out var second));
        items.Add(new("Gear", second));
        Assert.False(BeltMotion.TryArrival(items, 0.5f, out _));

        Assert.Equal(0.3f, first, 3);
        Assert.Equal(0.1f, second, 3);
    }

    [Fact]
    public void NewGoodNeedsAGapAtTheTail()
    {
        List<BeltGood> crowded = [new("Log", 0.05f)];
        List<BeltGood> open = [new("Log", BeltRates.Spacing)];

        Assert.False(BeltMotion.CanAccept(crowded));
        Assert.True(BeltMotion.CanAccept(open));
        Assert.False(BeltMotion.CanAccept([new("a", 1f), new("b", 0.8f), new("c", 0.6f), new("d", 0.4f), new("e", 0.2f)]));
    }
}
