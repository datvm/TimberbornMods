namespace ConveyorBelt.Tests;

public class BeltMotionTests
{
    [Fact]
    public void FloodedEntryDisappears()
    {
        var belt = new SimBelt { ItemsPerHour = 10f, Flooded = true };
        belt.Items.Add(new("Log", 0.2f));
        belt.Clear();

        Assert.True(belt.TryAccept("Plank", 0f, 0f));
        Assert.Empty(belt.Items);
    }

    [Fact]
    public void LogSpeedCrossesOneBeltPerHour()
    {
        var delta = BeltRates.Delta(1f, 1f);
        Assert.Equal(1f, delta, 3);
    }

    [Fact]
    public void AdvanceKeepsThePreviousPosition()
    {
        List<BeltGood> items = [new("Log", 0.2f)];
        BeltMotion.Advance(items, 0.1f, _ => false);

        Assert.Equal(0.2f, items[0].Previous, 3);
        Assert.Equal(0.3f, items[0].Position, 3);
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
    public void AdvanceNotesTheMove()
    {
        List<BeltGood> items = [new("Log", 0.2f)];
        BeltSpan? step = null;
        BeltMotion.Advance(items, 0.1f, 0, null, span => step = span);

        Assert.Equal(0.2f, step!.Value.From, 3);
        Assert.Equal(0.3f, step.Value.To, 3);
        Assert.True(step.Value.Rest);
    }

    [Fact]
    public void AdvanceNotesTheExit()
    {
        List<BeltGood> items = [new("Plank", 0.9f)];
        BeltSpan? step = null;
        BeltMotion.Advance(items, 0.5f, 0, (_, _, _) => true, span => step = span);

        Assert.Empty(items);
        Assert.Equal(0.9f, step!.Value.From, 3);
        Assert.Equal(BeltRates.End, step.Value.To, 3);
        Assert.False(step.Value.Rest);
    }

    [Fact]
    public void TrailCrossesThreeBeltsInOneTick()
    {
        var iph = 1f;
        var tick = 3f;
        var hour = BeltTravel.Hours(iph, 1f);
        var first = BeltTrail.Slice("Log", 0f, 1f, 0f, hour, tick, false);
        var middle = BeltTrail.Slice("Log", 0f, 1f, hour, hour, tick, false);
        var last = BeltTrail.Slice("Log", 0f, 0.5f, hour * 2f, BeltTravel.Hours(iph, 0.5f), tick, true);

        Assert.True(first.Shows(0.1f));
        Assert.False(middle.Shows(0.1f));
        Assert.True(middle.Shows(0.5f));
        Assert.Equal(0.5f, middle.Along(0.5f), 3);
        Assert.False(middle.Shows(0.8f));
        Assert.True(last.Shows(0.9f));
        Assert.Equal(0.5f, last.Along(0.9f), 3);
        Assert.Equal(middle.Start, first.End, 3);
        Assert.Equal(last.Start, middle.End, 3);
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
