namespace ConveyorBelt.Tests;

public class BeltFillTests
{
    public BeltFillTests() => BeltRates.Use(5);

    [Fact]
    public void FillEmptyBeltToCapacity()
    {
        List<BeltGood> items = [];
        BeltFill.AddLogs(items);

        Assert.Equal(BeltRates.Capacity, items.Count);
        Assert.Equal(BeltRates.End, items[0].Position, 3);
        Assert.Equal(BeltRates.Spacing, items[^1].Position, 3);
        Assert.All(items, item => Assert.Equal(BeltFill.LogId, item.Id));
    }

    [Fact]
    public void FillStopsBehindTheLastGood()
    {
        List<BeltGood> items = [new("Plank", BeltRates.Spacing)];
        BeltFill.AddLogs(items);

        Assert.Equal(2, items.Count);
        Assert.Equal("Plank", items[0].Id);
        Assert.Equal(0f, items[1].Position, 3);
    }

    [Fact]
    public void FillSkipsAGoodTheBeltCannotCarry()
    {
        List<BeltGood> items = [];
        BeltFill.AddLogs(items, _ => false);

        Assert.Empty(items);
    }
}
