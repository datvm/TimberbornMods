namespace TimberPipes.Tests;

public class PipeFlowSolverTests
{
    [Fact]
    public void EqualizeHorizontalChain()
    {
        float[] volumes = [1f, 0f, 0f, 0f, 0f];
        float[] capacities = [1f, 1f, 1f, 1f, 1f];
        PipeFlowEdge[] edges =
        [
            new(0, 1, true, true),
            new(1, 2, true, true),
            new(2, 3, true, true),
            new(3, 4, true, true),
        ];

        StepUntilSettled(volumes, capacities, edges, z: [0, 0, 0, 0, 0]);

        Assert.Equal(1f, volumes.Sum(), 4);
        foreach (var v in volumes)
        {
            Assert.InRange(v, 0.15f, 0.25f);
        }
    }

    [Fact]
    public void NoUpwardFlowWithoutPump()
    {
        float[] volumes = [1f, 0f];
        float[] capacities = [1f, 1f];
        PipeFlowEdge[] edges = [new(0, 1, true, true)];

        StepUntilSettled(volumes, capacities, edges, z: [0, 1]);

        Assert.InRange(volumes[0], 0.99f, 1f);
        Assert.InRange(volumes[1], 0f, 0.01f);
    }

    [Fact]
    public void GravityFillsBottomOfColumn()
    {
        float[] volumes = [0.7f, 0.6f, 0.5f];
        float[] capacities = [1f, 1f, 1f];
        PipeFlowEdge[] edges =
        [
            new(0, 1, true, true),
            new(1, 2, true, true),
        ];

        StepUntilSettled(volumes, capacities, edges, z: [0, 1, 2]);

        Assert.Equal(1.8f, volumes.Sum(), 4);
        Assert.InRange(volumes[0], 0.99f, 1f);
        Assert.InRange(volumes[1], 0.79f, 0.81f);
        Assert.InRange(volumes[2], 0f, 0.01f);
    }

    [Fact]
    public void ExcessStaysAboveWhenLowerFull()
    {
        float[] volumes = [1f, 0.4f];
        float[] capacities = [1f, 1f];
        PipeFlowEdge[] edges = [new(0, 1, true, true)];

        StepUntilSettled(volumes, capacities, edges, z: [0, 1]);

        Assert.InRange(volumes[0], 0.99f, 1f);
        Assert.InRange(volumes[1], 0.39f, 0.41f);
    }

    [Fact]
    public void RemainingLiftByElevation()
    {
        Assert.Equal(2f, PipeFlowSolver.RemainingLift(0, 0, 2f));
        Assert.Equal(1f, PipeFlowSolver.RemainingLift(1, 0, 2f));
        Assert.Equal(0f, PipeFlowSolver.RemainingLift(2, 0, 2f));
        Assert.Equal(0f, PipeFlowSolver.RemainingLift(3, 0, 2f));
    }

    [Fact]
    public void DownwardFlowByGravity()
    {
        float[] volumes = [0f, 1f];
        float[] capacities = [1f, 1f];
        PipeFlowEdge[] edges = [new(0, 1, true, true)];

        StepUntilSettled(volumes, capacities, edges, z: [0, 1]);

        Assert.InRange(volumes[0], 0.99f, 1f);
        Assert.InRange(volumes[1], 0f, 0.01f);
    }

    [Fact]
    public void OneWayBlocksReverse()
    {
        float[] volumes = [0f, 1f];
        float[] capacities = [1f, 1f];
        PipeFlowEdge[] edges = [new(0, 1, true, false)];

        StepUntilSettled(volumes, capacities, edges, z: [0, 0]);

        Assert.Equal(0f, volumes[0], 4);
        Assert.Equal(1f, volumes[1], 4);
    }

    [Fact]
    public void OneWayAllowsForward()
    {
        float[] volumes = [1f, 0f];
        float[] capacities = [1f, 1f];
        PipeFlowEdge[] edges = [new(0, 1, true, false)];

        StepUntilSettled(volumes, capacities, edges, z: [0, 0]);

        Assert.InRange(volumes[0], 0.45f, 0.55f);
        Assert.InRange(volumes[1], 0.45f, 0.55f);
    }

    [Fact]
    public void ConservesVolumeWhenClamped()
    {
        float[] volumes = [1f, 0.9f, 0f];
        float[] capacities = [1f, 1f, 0.2f];
        PipeFlowEdge[] edges =
        [
            new(0, 1, true, true),
            new(1, 2, true, true),
        ];

        var before = volumes.Sum();
        StepUntilSettled(volumes, capacities, edges, z: [0, 0, 0]);

        Assert.Equal(before, volumes.Sum(), 4);
        Assert.InRange(volumes[2], 0.19f, 0.2f);
    }

    [Fact]
    public void TankHeadFromFill()
    {
        var empty = PipeFlowSolver.TankHead(5, 0f, 240f, 3);
        var half = PipeFlowSolver.TankHead(5, 120f, 240f, 3);
        var full = PipeFlowSolver.TankHead(5, 240f, 240f, 3);

        Assert.Equal(5f, empty);
        Assert.Equal(6.5f, half);
        Assert.Equal(8f, full);
    }

    [Fact]
    public void EmptyTankConflictsWhenItDoesNotTakePipeGood()
    {
        Assert.False(PipeFlowSolver.TankConflictsWithPipe(null, tankTakesPipeGood: true, "Water"));
        Assert.True(PipeFlowSolver.TankConflictsWithPipe(null, tankTakesPipeGood: false, "Water"));
        Assert.True(PipeFlowSolver.TankConflictsWithPipe("Biofuel", tankTakesPipeGood: true, "Water"));
        Assert.False(PipeFlowSolver.TankConflictsWithPipe("Water", tankTakesPipeGood: true, "Water"));
        Assert.False(PipeFlowSolver.TankConflictsWithPipe("Biofuel", tankTakesPipeGood: true, null));
    }

    [Fact]
    public void PumpHeadCapsAtOneMeter()
    {
        Assert.Equal(5.2f, PipeFlowSolver.PumpHead(5, PipeFlowSolver.GoodsToVolume(1)));
        Assert.Equal(6f, PipeFlowSolver.PumpHead(5, PipeFlowSolver.GoodsToVolume(5)));
        Assert.Equal(6f, PipeFlowSolver.PumpHead(5, PipeFlowSolver.GoodsToVolume(10)));
    }

    [Fact]
    public void QuantizePendingToGoods()
    {
        var pending = 0.45f;
        var delta = PipeFlowSolver.QuantizePending(ref pending);

        Assert.Equal(2, delta);
        Assert.InRange(pending, 0.049f, 0.051f);
    }

    [Fact]
    public void UBendBlocksWhenSourceHeadIsBelowCrest()
    {
        float[] volumes = [0f, 0f, 0f, 0f, 0f, 1f];
        float[] capacities = [1f, 1f, 1f, 1f, 1f, 1f];
        PipeFlowEdge[] edges =
        [
            new(0, 1, true, true),
            new(1, 2, true, true),
            new(2, 3, true, true),
            new(3, 4, true, true),
            new(4, 5, true, true),
        ];

        StepMixed(
            volumes,
            capacities,
            edges,
            (i, v) => i < 5
                ? PipeFlowSolver.PipeHead(PipeZ(i), v)
                : PipeFlowSolver.TankHead(0, v, 1f, 1));

        Assert.Equal(1f, volumes.Sum(), 3);
        Assert.InRange(volumes[4], 0.45f, 0.55f);
        Assert.InRange(volumes[5], 0.45f, 0.55f);
        Assert.InRange(volumes[3], 0f, 0.02f);
        Assert.InRange(volumes[2], 0f, 0.02f);
        Assert.InRange(volumes[0], 0f, 0.02f);
        Assert.InRange(volumes[1], 0f, 0.02f);
    }

    [Fact]
    public void GroundTankDoesNotFillRiser()
    {
        float[] volumes = [0f, 0f, 10f];
        float[] capacities = [1f, 1f, 10f];
        PipeFlowEdge[] edges =
        [
            new(0, 1, true, true),
            new(0, 2, true, true),
        ];

        StepMixed(
            volumes,
            capacities,
            edges,
            (i, v) => i < 2
                ? PipeFlowSolver.PipeHead(i, v)
                : PipeFlowSolver.TankHead(0, v, 10f, 3));

        Assert.Equal(10f, volumes.Sum(), 3);
        Assert.InRange(volumes[0], 0.95f, 1f);
        Assert.InRange(volumes[1], 0f, 0.05f);
    }

    [Fact]
    public void HighTankDrainsDownBothSidesOfArch()
    {
        float[] volumes = [0f, 0f, 0f, 0f, 0f, 3f];
        float[] capacities = [1f, 1f, 1f, 1f, 1f, 3f];
        PipeFlowEdge[] edges =
        [
            new(0, 1, true, true),
            new(1, 2, true, true),
            new(2, 3, true, true),
            new(3, 4, true, true),
            new(2, 5, true, true),
        ];

        StepMixed(
            volumes,
            capacities,
            edges,
            (i, v) => i < 5
                ? PipeFlowSolver.PipeHead(PipeZ(i), v)
                : PipeFlowSolver.TankHead(2, v, 3f, 1));

        Assert.Equal(3f, volumes.Sum(), 3);
        Assert.InRange(volumes[0], 0.95f, 1f);
        Assert.InRange(volumes[4], 0.95f, 1f);
        Assert.InRange(volumes[2], 0f, 0.05f);
    }

    [Fact]
    public void TankFillsOnlyAdjacentPipeThenSpreads()
    {
        float[] volumes = [0f, 0f, 2f];
        float[] capacities = [1f, 1f, 2f];
        PipeFlowEdge[] edges =
        [
            new(0, 1, true, true),
            new(0, 2, true, true),
        ];

        StepMixed(
            volumes,
            capacities,
            edges,
            (i, v) => i < 2
                ? PipeFlowSolver.PipeHead(0, v)
                : PipeFlowSolver.TankHead(0, v, 2f, 1));

        Assert.Equal(2f, volumes.Sum(), 3);
        Assert.InRange(volumes[0], 0.45f, 0.55f);
        Assert.InRange(volumes[1], 0.45f, 0.55f);
    }

    [Fact]
    public void PumpColumnFillsOutletWithoutCrossingUBend()
    {
        float[] volumes = [0f, 0f, 0f, 0f, 0f, 2f];
        float[] capacities = [1f, 1f, 1f, 1f, 1f, 6f];
        PipeFlowEdge[] edges =
        [
            new(0, 1, true, true),
            new(1, 2, true, true),
            new(2, 3, true, true),
            new(3, 4, true, true),
            new(4, 5, false, true),
        ];

        StepMixed(
            volumes,
            capacities,
            edges,
            (i, v) => i < 5
                ? PipeFlowSolver.PipeHead(PipeZ(i), v)
                : PipeFlowSolver.PumpHead(0, v));

        Assert.Equal(2f, volumes.Sum(), 3);
        Assert.InRange(volumes[4], 0.95f, 1f);
        Assert.InRange(volumes[3], 0f, 0.05f);
        Assert.InRange(volumes[0], 0f, 0.05f);
        Assert.InRange(volumes[2], 0f, 0.05f);
    }

    [Fact]
    public void PumpColumnFillsOutletPastPacketStall()
    {
        float[] volumes = [0.85f, 2f];
        float[] capacities = [1f, 6f];
        PipeFlowEdge[] edges = [new(0, 1, false, true)];

        StepMixed(
            volumes,
            capacities,
            edges,
            (i, v) => i == 0
                ? PipeFlowSolver.PipeHead(0, v)
                : PipeFlowSolver.PumpHead(0, v));

        Assert.InRange(volumes[0], 0.95f, 1f);
        Assert.InRange(volumes[1], 1.85f, 1.9f);
    }

    static int PipeZ(int i) => i switch
    {
        0 => 0,
        1 => 1,
        2 => 2,
        3 => 1,
        4 => 0,
        _ => 0,
    };

    static void StepUntilSettled(float[] volumes, float[] capacities, PipeFlowEdge[] edges, int[] z)
        => StepMixed(volumes, capacities, edges, (i, v) => PipeFlowSolver.PipeHead(z[i], v));

    static void StepMixed(
        float[] volumes,
        float[] capacities,
        PipeFlowEdge[] edges,
        Func<int, float, float> headAt)
    {
        var heads = new float[volumes.Length];
        for (var i = 0; i < 200; i++)
        {
            for (var n = 0; n < volumes.Length; n++)
            {
                heads[n] = headAt(n, volumes[n]);
            }

            PipeFlowSolver.Equalize(volumes, heads, capacities, edges, 0.25f);
        }
    }
}
