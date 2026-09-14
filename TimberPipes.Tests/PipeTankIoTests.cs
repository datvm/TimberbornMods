namespace TimberPipes.Tests;

public class PipeTankIoTests
{
    [Fact]
    public void FullyReservedTankIsNotDrainableOrFillable()
    {
        Assert.Equal(0, PipeTankIo.DrainableGoods(0));
        Assert.Equal(0, PipeTankIo.FillableGoods(0));
        Assert.Equal(0, PipeTankIo.FillableGoods(30, 30, 0));
        Assert.Equal(0, PipeTankIo.FillableGoods(30, 0, 30));
        Assert.Equal(0f, PipeTankIo.FlowVolume(0, 0f));
        Assert.Equal(0f, PipeTankIo.FlowCapacity(0, 0));
        Assert.False(PipeTankIo.CanTake(0));
        Assert.False(PipeTankIo.CanGive(0));
    }

    [Fact]
    public void ReservedStockIsNotTaken()
    {
        Assert.Equal(0, PipeTankIo.TakeCount(8, 0));
        Assert.Equal(3, PipeTankIo.TakeCount(8, 3));
        Assert.False(PipeTankIo.CanTake(0));
        Assert.True(PipeTankIo.CanTake(1));
    }

    [Fact]
    public void FullOrReservedCapacityIsNotFilled()
    {
        Assert.Equal(0, PipeTankIo.GiveCount(8, 0));
        Assert.Equal(2, PipeTankIo.GiveCount(8, 2));
        Assert.False(PipeTankIo.CanGive(0));
        Assert.True(PipeTankIo.CanGive(1));
    }

    [Fact]
    public void EmptyTankCanFillButNotSiphon()
    {
        Assert.Equal(0, PipeTankIo.TakeCount(5, 0));
        Assert.Equal(5, PipeTankIo.GiveCount(5, 30));
        Assert.Equal(0f, PipeTankIo.FlowVolume(0, 0f));
        Assert.Equal(PipeFlowSolver.GoodsToVolume(30), PipeTankIo.FlowCapacity(0, 30));
    }

    [Fact]
    public void FlowUsesUnreservedStockOnly()
    {
        var volume = PipeTankIo.FlowVolume(drainableGoods: 10, pending: 0f);
        var capacity = PipeTankIo.FlowCapacity(drainableGoods: 10, fillableGoods: 5);

        Assert.Equal(PipeFlowSolver.GoodsToVolume(10), volume);
        Assert.Equal(PipeFlowSolver.GoodsToVolume(15), capacity);
        Assert.Equal(0f, PipeTankIo.ClampFlowVolume(-1f, capacity));
        Assert.Equal(capacity, PipeTankIo.ClampFlowVolume(volume + 1f, capacity));
    }

    [Fact]
    public void NegativePendingDoesNotSiphonEmptyTank()
    {
        Assert.Equal(0f, PipeTankIo.FlowVolume(0, -PipeFlowSolver.GoodsToVolume(10)));
        Assert.Equal(0f, PipeTankIo.PendingFromSolver(0f, 0));
        Assert.Equal(-PipeFlowSolver.GoodsToVolume(4), PipeTankIo.PendingFromSolver(0f, 4));
    }
}
