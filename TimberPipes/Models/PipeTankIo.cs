namespace TimberPipes.Models;

public static class PipeTankIo
{
    public static int DrainableGoods(int unreservedTakeable)
        => Math.Max(0, unreservedTakeable);

    public static int FillableGoods(int unreservedCapacity)
        => Math.Max(0, unreservedCapacity);

    public static int FillableGoods(int capacity, int stored, int reservedCapacity)
        => Math.Max(0, capacity - stored - reservedCapacity);

    public static float FlowVolume(int drainableGoods, float pending)
        => Math.Max(0f, PipeFlowSolver.GoodsToVolume(DrainableGoods(drainableGoods)) + pending);

    public static float FlowCapacity(int drainableGoods, int fillableGoods)
        => PipeFlowSolver.GoodsToVolume(DrainableGoods(drainableGoods) + FillableGoods(fillableGoods));

    public static float ClampFlowVolume(float volume, float capacity)
        => Math.Clamp(volume, 0f, Math.Max(0f, capacity));

    public static float PendingFromSolver(float solverVolumeM3, int drainableGoods)
        => solverVolumeM3 - PipeFlowSolver.GoodsToVolume(DrainableGoods(drainableGoods));

    public static int GiveCount(int requested, int unreservedCapacity)
    {
        if (requested < 1 || unreservedCapacity < 1)
        {
            return 0;
        }

        return Math.Min(requested, unreservedCapacity);
    }

    public static int TakeCount(int requested, int unreservedTakeable)
    {
        if (requested < 1 || unreservedTakeable < 1)
        {
            return 0;
        }

        return Math.Min(requested, unreservedTakeable);
    }

    public static bool CanGive(int unreservedCapacity)
        => GiveCount(1, unreservedCapacity) > 0;

    public static bool CanTake(int unreservedTakeable)
        => TakeCount(1, unreservedTakeable) > 0;
}
