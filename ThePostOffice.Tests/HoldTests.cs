namespace ThePostOffice.Tests;

public class HoldTests
{
    [Fact]
    public void ReserveIncomingWeight()
    {
        var world = OfficeWorld.Create(capacity: 100, goods: new Good("Log", 1));

        var id = world.Inventory.TryReserveIncoming([new("Log", 40)]);

        Assert.NotNull(id);
        Assert.Equal(60, world.Inventory.RemainingCapacity);
        Assert.Equal(40, world.Inventory.IncomingWeight);
    }

    [Fact]
    public void RejectIncomingOverCapacity()
    {
        var world = OfficeWorld.Create(capacity: 100, goods: new Good("Log", 1, 70));

        var id = world.Inventory.TryReserveIncoming([new("Log", 40)]);

        Assert.Null(id);
        Assert.Equal(0, world.Inventory.IncomingWeight);
        Assert.Equal(30, world.Inventory.RemainingCapacity);
    }

    [Fact]
    public void ReserveOutgoingAmount()
    {
        var world = OfficeWorld.Create(goods: new Good("Log", 1, 10));

        var id = world.Inventory.TryReserveOutgoing([new("Log", 4)]);

        Assert.NotNull(id);
        Assert.Equal(6, world.Inventory.Available("Log"));
        Assert.Equal(10, world.Offices.GlobalAmount("Log"));
    }

    [Fact]
    public void RejectOutgoingWhenShort()
    {
        var world = OfficeWorld.Create(goods: new Good("Log", 1, 3));

        var first = world.Inventory.TryReserveOutgoing([new("Log", 2)]);
        var second = world.Inventory.TryReserveOutgoing([new("Log", 2)]);

        Assert.NotNull(first);
        Assert.Null(second);
        Assert.Equal(1, world.Inventory.Available("Log"));
    }

    [Fact]
    public void ReleaseRestoresTheBudget()
    {
        var world = OfficeWorld.Create(capacity: 100, goods: new Good("Log", 1, 10));
        var incoming = world.Inventory.TryReserveIncoming([new("Log", 25)])!.Value;
        var outgoing = world.Inventory.TryReserveOutgoing([new("Log", 4)])!.Value;

        world.Inventory.Release(incoming);
        world.Inventory.Release(outgoing);

        Assert.Equal(90, world.Inventory.RemainingCapacity);
        Assert.Equal(10, world.Inventory.Available("Log"));
    }

    [Fact]
    public void CommitIncomingStoresTheGoods()
    {
        var world = OfficeWorld.Create(capacity: 100, goods: new Good("Log", 1));
        var id = world.Inventory.TryReserveIncoming([new("Log", 10)])!.Value;

        Assert.True(world.Inventory.Commit(id));

        Assert.Equal(10, world.Offices.GlobalAmount("Log"));
        Assert.Equal(0, world.Inventory.IncomingWeight);
        Assert.Equal(90, world.Inventory.RemainingCapacity);
        Assert.False(world.Inventory.CanCommit(id));
    }

    [Fact]
    public void CommitOutgoingRemovesTheGoods()
    {
        var world = OfficeWorld.Create(goods: new Good("Log", 1, 10));
        var id = world.Inventory.TryReserveOutgoing([new("Log", 4)])!.Value;

        Assert.True(world.Inventory.Commit(id));

        Assert.Equal(6, world.Offices.GlobalAmount("Log"));
        Assert.Equal(6, world.Inventory.Available("Log"));
    }

    [Fact]
    public void CommitIncomingFailsWhenCapacityShrinks()
    {
        var world = OfficeWorld.Create(capacity: 100, goods: new Good("Log", 1));
        var id = world.Inventory.TryReserveIncoming([new("Log", 60)])!.Value;
        world.Inventory.RemoveCapacity(50);

        Assert.False(world.Inventory.CanCommit(id));
        Assert.False(world.Inventory.Commit(id));
        Assert.Equal(0, world.Offices.GlobalAmount("Log"));
        Assert.Equal(60, world.Inventory.IncomingWeight);
    }

    [Fact]
    public void DropNewestOutgoingFirst()
    {
        var world = OfficeWorld.Create(goods: new Good("Log", 1, 10));
        var older = world.Inventory.TryReserveOutgoing([new("Log", 4)])!.Value;
        var newer = world.Inventory.TryReserveOutgoing([new("Log", 4)])!.Value;
        var stalled = new StallListener();
        world.Events.Register(stalled);
        world.Handle("Log").Remove(5);

        Assert.Equal(newer, Assert.Single(stalled.Ids));
        Assert.Equal(1, world.Inventory.Available("Log"));
        Assert.True(world.Inventory.CanCommit(older));
        Assert.False(world.Inventory.CanCommit(newer));
    }

    [Fact]
    public void DropNewestIncomingUntilItFits()
    {
        var world = OfficeWorld.Create(capacity: 100, goods: new Good("Log", 1));
        var older = world.Inventory.TryReserveIncoming([new("Log", 30)])!.Value;
        var newer = world.Inventory.TryReserveIncoming([new("Log", 30)])!.Value;
        world.Inventory.RemoveCapacity(60);

        var dropped = world.Inventory.DropHoldsThatDoNotFit();

        Assert.Equal([newer], dropped);
        Assert.Equal(30, world.Inventory.IncomingWeight);
        Assert.True(world.Inventory.CanCommit(older));
    }

    [Fact]
    public void SpillFreeGoodsBeforeReservedGoods()
    {
        var world = OfficeWorld.Create(capacity: 100, goods: [new("Log", 2, 10), new("Metal", 5, 12)]);
        world.Inventory.TryReserveOutgoing([new("Metal", 12)]);

        var removed = world.Inventory.GetGoodRemoval(30).ToArray();

        Assert.Equal([new GoodAmount("Log", 5)], removed);
    }

    [Fact]
    public void RemoveCapacityRejectsStoredWeight()
    {
        var world = OfficeWorld.Create(capacity: 100, goods: new Good("Log", 1, 80));

        Assert.Throws<InvalidOperationException>(() => world.Inventory.RemoveCapacity(30));
    }

    sealed class StallListener
    {
        public ImmutableArray<int> Ids { get; private set; }

        [OnEvent]
        public void OnStalled(OnPostOfficeHoldsStalled stalled)
        {
            Ids = stalled.Ids;
        }
    }
}
