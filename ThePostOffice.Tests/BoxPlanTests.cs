namespace ThePostOffice.Tests;

public class BoxPlanTests
{
    [Fact]
    public void OfficeSlotsStayInTheYard()
    {
        Assert.Equal(54, PostOfficeBoxPlan.OfficeSlots.Count);
        var heights = PostOfficeBoxPlan.OfficeSlots.Select(slot => slot.Y).Distinct().ToArray();
        Assert.Equal(PostOfficeBoxPlan.OfficeLayers, heights.Length);
        foreach (var slot in PostOfficeBoxPlan.OfficeSlots)
        {
            Assert.InRange(slot.X, 0.12f, 2.88f);
            Assert.InRange(slot.Z, 2.12f, 2.88f);
            Assert.InRange(slot.Y, 0.02f, 0.30f);
        }
    }

    [Fact]
    public void LodgeSlotsFillUnderTheRoof()
    {
        Assert.Equal(15 * 9 * PostOfficeBoxPlan.LodgeLayers, PostOfficeBoxPlan.LodgeSlots.Count);
        foreach (var slot in PostOfficeBoxPlan.LodgeSlots)
        {
            var inDoor = slot.X < 1f && slot.Z < 1f;
            Assert.False(inDoor);
            Assert.InRange(slot.X, 0.12f, 3.88f);
            Assert.InRange(slot.Z, 0.12f, 3.88f);
            Assert.InRange(slot.Y, 0.05f, 3.80f);
        }
    }

    [Fact]
    public void ClampLodgeFillAtFull()
    {
        Assert.Equal(0, PostOfficeBoxPlan.VisibleCount(0, 600, 10));
        Assert.Equal(0, PostOfficeBoxPlan.VisibleCount(10, 0, 10));
        Assert.Equal(5, PostOfficeBoxPlan.VisibleCount(300, 600, 10));
        Assert.Equal(10, PostOfficeBoxPlan.VisibleCount(600, 600, 10));
        Assert.Equal(10, PostOfficeBoxPlan.VisibleCount(900, 600, 10));
    }

    [Fact]
    public void KeepExactCountsWhenTheyFit()
    {
        var shares = PostOfficeBoxPlan.FromAmounts([new("Log", 4), new("Plank", 2)], 54);

        Assert.Equal([new BoxShare("Log", 4), new BoxShare("Plank", 2)], shares);
    }

    [Fact]
    public void ShrinkMixedStockToTheSlotCap()
    {
        var shares = PostOfficeBoxPlan.FromAmounts([new("Log", 40), new("Plank", 40)], 54);

        Assert.Equal(54, shares.Sum(share => share.Count));
        Assert.Equal(2, shares.Count);
        Assert.All(shares, share => Assert.True(share.Count > 0));
    }

    [Fact]
    public void FillOfficeByStoredWeight()
    {
        var shares = PostOfficeBoxPlan.FromCapacity(
            [new("Plank", 78)],
            totalWeight: 78,
            capacity: 80,
            slots: 54);

        Assert.Equal(53, shares.Sum(share => share.Count));
    }

    [Fact]
    public void ShareCapacityByWeight()
    {
        var shares = PostOfficeBoxPlan.FromCapacity(
            [new("Log", 1), new("Metal", 3)],
            totalWeight: 4,
            capacity: 8,
            slots: 10);

        Assert.Equal(5, shares.Sum(share => share.Count));
        Assert.Equal(new BoxShare("Log", 1), shares[0]);
        Assert.Equal(new BoxShare("Metal", 4), shares[1]);
    }
}
