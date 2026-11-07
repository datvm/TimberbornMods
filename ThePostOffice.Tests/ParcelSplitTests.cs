namespace ThePostOffice.Tests;

public class ParcelSplitTests
{
    readonly OfficeWorld world = OfficeWorld.Create(
        goods: [new("Log", 1), new("Plank", 1), new("Metal", 80), new("Gear", 50), new("Water", 0)]);

    [Fact]
    public void SplitInListOrder()
    {
        var parcels = world.Offices.Split([new("Log", 50), new("Plank", 20)]);

        Assert.Equal(2, parcels.Count);
        DeskProbe.AssertGoods(parcels[0], ("Log", 50), ("Plank", 10));
        DeskProbe.AssertGoods(parcels[1], ("Plank", 10));
        Assert.False(parcels[0].Auto);
    }

    [Fact]
    public void FillAParcelExactly()
    {
        var parcels = world.Offices.Split([new("Log", 60), new("Plank", 1)]);

        Assert.Equal(2, parcels.Count);
        DeskProbe.AssertGoods(parcels[0], ("Log", 60));
        DeskProbe.AssertGoods(parcels[1], ("Plank", 1));
    }

    [Fact]
    public void HeavyUnitIsItsOwnParcel()
    {
        var parcels = world.Offices.Split([new("Log", 30), new("Metal", 2), new("Log", 10)]);

        Assert.Equal(4, parcels.Count);
        DeskProbe.AssertGoods(parcels[0], ("Log", 30));
        DeskProbe.AssertGoods(parcels[1], ("Metal", 1));
        DeskProbe.AssertGoods(parcels[2], ("Metal", 1));
        DeskProbe.AssertGoods(parcels[3], ("Log", 10));
    }

    [Fact]
    public void SkipEmptyAndWeightlessGoods()
    {
        var parcels = world.Offices.Split([new("Log", 0), new("Water", 4), new("Plank", 3)]);

        Assert.Single(parcels);
        DeskProbe.AssertGoods(parcels[0], ("Plank", 3));
    }

    [Fact]
    public void PackOnlyTheFirstParcel()
    {
        var parcel = world.Offices.PackOne([new("Log", 50), new("Plank", 20)], true);

        Assert.True(parcel.HasValue);
        Assert.True(parcel.Value.Auto);
        DeskProbe.AssertGoods(parcel.Value, ("Log", 50), ("Plank", 10));
    }

    [Fact]
    public void PackSkipsAGoodThatDoesNotFit()
    {
        var parcel = world.Offices.PackOne([new("Gear", 2), new("Log", 5)], false);

        Assert.True(parcel.HasValue);
        DeskProbe.AssertGoods(parcel.Value, ("Gear", 1), ("Log", 5));
    }

    [Fact]
    public void PackHeavyUnitWhenItIsFirst()
    {
        var parcel = world.Offices.PackOne([new("Metal", 2)], true);

        Assert.True(parcel.HasValue);
        DeskProbe.AssertGoods(parcel.Value, ("Metal", 1));
    }

    [Fact]
    public void PackSkipsHeavyUnitAfterLighterGoods()
    {
        var parcel = world.Offices.PackOne([new("Log", 10), new("Metal", 1)], false);

        Assert.True(parcel.HasValue);
        DeskProbe.AssertGoods(parcel.Value, ("Log", 10));
    }

    [Fact]
    public void PackReturnsNullWhenNothingFits()
    {
        var parcel = world.Offices.PackOne([new("Water", 3), new("Log", 0)], true);

        Assert.False(parcel.HasValue);
    }
}
