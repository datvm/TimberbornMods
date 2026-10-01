namespace ConveyorBelt.Tests;

public class BeltGraphTests
{
    [Fact]
    public void ConveyorOfferBeatsInventory()
    {
        var picked = BeltConnectionPick.Highest(
        [
            (BeltConnectionPriority.Building, 1, true),
            (BeltConnectionPriority.Conveyor, 2, true),
        ]);

        Assert.Equal(2, picked);
    }

    [Fact]
    public void InventoryOfferStandsAlone()
    {
        var picked = BeltConnectionPick.Highest(
        [
            (BeltConnectionPriority.Building, 1, true),
            (BeltConnectionPriority.Conveyor, 2, false),
        ]);

        Assert.Equal(1, picked);
    }

    [Fact]
    public void StopsHalfwayOnTheThirdBelt()
    {
        var hours = BeltTravel.Hours(100f, 2.5f);
        var stop = BeltTravel.Cross([100f, 100f, 100f], hours);

        Assert.Equal(2, stop.Index);
        Assert.Equal(0.5f, stop.Position, 3);
    }

    [Fact]
    public void SlowerMiddleBeltStopsTheGood()
    {
        var hours = BeltTravel.Hours(100f, 2.5f);
        var stop = BeltTravel.Cross([100f, 10f, 100f], hours);

        Assert.Equal(1, stop.Index);
        Assert.True(stop.Position < BeltRates.End);
    }
}
