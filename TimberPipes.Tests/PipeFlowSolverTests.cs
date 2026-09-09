namespace TimberPipes.Tests;

public class PipeFlowSolverTests
{
    [Fact]
    public void EqualizeHorizontalChain()
    {
        float[] volumes = [1f, 0f, 0f, 0f, 0f];
        SettleGravity(volumes, Caps(5), Chain(4), z: [0, 0, 0, 0, 0]);

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
        Settle(volumes, Caps(2), [new(0, 1, true, true)], z: [0, 1]);

        Assert.InRange(volumes[0], 0.99f, 1f);
        Assert.InRange(volumes[1], 0f, 0.01f);
    }

    [Fact]
    public void GravityFillsBottomOfColumn()
    {
        float[] volumes = [0.7f, 0.6f, 0.5f];
        SettleGravity(volumes, Caps(3), [new(0, 1, true, true), new(1, 2, true, true)], z: [0, 1, 2]);

        Assert.Equal(1.8f, volumes.Sum(), 4);
        Assert.InRange(volumes[0], 0.99f, 1f);
        Assert.InRange(volumes[1], 0.79f, 0.81f);
        Assert.InRange(volumes[2], 0f, 0.01f);
    }

    [Fact]
    public void ExcessStaysAboveWhenLowerFull()
    {
        float[] volumes = [1f, 0.4f];
        SettleGravity(volumes, Caps(2), [new(0, 1, true, true)], z: [0, 1]);

        Assert.InRange(volumes[0], 0.99f, 1f);
        Assert.InRange(volumes[1], 0.39f, 0.41f);
    }

    [Fact]
    public void DownwardFlowByGravity()
    {
        float[] volumes = [0f, 1f];
        SettleGravity(volumes, Caps(2), [new(0, 1, true, true)], z: [0, 1]);

        Assert.InRange(volumes[0], 0.99f, 1f);
        Assert.InRange(volumes[1], 0f, 0.01f);
    }

    [Fact]
    public void OneWayBlocksReverse()
    {
        float[] volumes = [0f, 1f];
        SettleGravity(volumes, Caps(2), [new(0, 1, true, false)], z: [0, 0]);

        Assert.Equal(0f, volumes[0], 4);
        Assert.Equal(1f, volumes[1], 4);
    }

    [Fact]
    public void OneWayAllowsForward()
    {
        float[] volumes = [1f, 0f];
        SettleGravity(volumes, Caps(2), [new(0, 1, true, false)], z: [0, 0]);

        Assert.InRange(volumes[0], 0.45f, 0.55f);
        Assert.InRange(volumes[1], 0.45f, 0.55f);
    }

    [Fact]
    public void ConservesVolumeWhenClamped()
    {
        float[] volumes = [1f, 0.9f, 0f];
        float[] capacities = [1f, 1f, 0.2f];
        var before = volumes.Sum();
        SettleGravity(volumes, capacities, [new(0, 1, true, true), new(1, 2, true, true)], z: [0, 0, 0]);

        Assert.Equal(before, volumes.Sum(), 4);
        Assert.InRange(volumes[2], 0.19f, 0.2f);
    }

    [Fact]
    public void PackSlicesBottomFirst()
    {
        float[] slices = [0f, 0f];
        PipeFlowSolver.PackSlices(4f, slices, 3f);

        Assert.Equal(3f, slices[0]);
        Assert.Equal(1f, slices[1]);
        Assert.Equal(4f, PipeFlowSolver.UnpackSlices(slices));
        Assert.Equal(0, PipeFlowSolver.SliceIndex(4, 4, 2));
        Assert.Equal(1, PipeFlowSolver.SliceIndex(5, 4, 2));
        Assert.Equal(0, PipeFlowSolver.SliceIndex(3, 4, 2));
        Assert.Equal(1, PipeFlowSolver.SliceIndex(9, 4, 2));
        Assert.Equal(3f, PipeFlowSolver.SliceCapacity(6f, 2));
    }

    [Fact]
    public void EmptyTankConflictsWhenItDoesNotTakePipeGood()
    {
        Assert.False(PipeFlowSolver.TankConflictsWithPipe(null, tankTakesPipeGood: true, "Water"));
        Assert.True(PipeFlowSolver.TankConflictsWithPipe(null, tankTakesPipeGood: false, "Water"));
        Assert.True(PipeFlowSolver.TankConflictsWithPipe("Biofuel", tankTakesPipeGood: true, "Water"));
        Assert.False(PipeFlowSolver.TankConflictsWithPipe("Water", tankTakesPipeGood: true, "Water"));
        Assert.False(PipeFlowSolver.TankConflictsWithPipe("Biofuel", tankTakesPipeGood: true, null));
        Assert.False(PipeFlowSolver.WellConflictsWithPipe(null, "Water"));
        Assert.False(PipeFlowSolver.WellConflictsWithPipe("Water", "Water"));
        Assert.True(PipeFlowSolver.WellConflictsWithPipe("Biofuel", "Water"));
        Assert.False(PipeFlowSolver.WellConflictsWithPipe("Water", null));
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
        Settle(
            volumes,
            capacities,
            UBendWithEnd(),
            z: [0, 1, 2, 1, 0, 0],
            pipeCount: 5,
            headAt: (i, v) => i < 5
                ? PipeFlowSolver.PipeHead(PipeZ(i), v)
                : PipeFlowSolver.TankHead(0, v, 1f, 1));

        Assert.Equal(1f, volumes.Sum(), 3);
        Assert.InRange(volumes[4], 0.45f, 0.55f);
        Assert.InRange(volumes[5], 0.45f, 0.55f);
        Assert.InRange(volumes[3], 0f, 0.02f);
        Assert.InRange(volumes[0], 0f, 0.02f);
    }

    [Fact]
    public void GroundTankFillsRiserThroughSlices()
    {
        var sliceCap = PipeFlowSolver.SliceCapacity(10f, 3);
        float[] volumes = [0f, 0f, 0f, 0f, 0f];
        PipeFlowSolver.PackSlices(10f, volumes.AsSpan(2, 3), sliceCap);
        float[] capacities = [1f, 1f, sliceCap, sliceCap, sliceCap];
        Settle(
            volumes,
            capacities,
            [
                new(0, 1, true, true),
                new(0, 2, true, true),
                new(2, 3, true, true),
                new(3, 4, true, true),
            ],
            z: [0, 1, 0, 1, 2],
            pipeCount: 2,
            flowCount: 5,
            headAt: (i, v) => i < 2
                ? PipeFlowSolver.PipeHead(i == 1 ? 1 : 0, v)
                : PipeFlowSolver.Surface(i - 2, v, capacities[i]));

        Assert.Equal(10f, volumes.Sum(), 3);
        Assert.InRange(volumes[0], 0.95f, 1f);
        Assert.InRange(volumes[1], 0.95f, 1f);
        Assert.True(PipeFlowSolver.UnpackSlices(volumes.AsSpan(2, 3)) < 8.1f);
    }

    [Fact]
    public void BottomOnlyTankFillsLowerSliceWithoutPump()
    {
        float[] volumes = [0f, 0f, 0f, 10f];
        float[] capacities = [1f, 3f, 3f, 10f];
        Settle(
            volumes,
            capacities,
            [new(0, 1, true, true), new(1, 2, true, true), new(0, 3, true, true)],
            z: [0, 0, 1, 0],
            pipeCount: 1,
            flowCount: 3,
            headAt: BottomOnlyTankHead);

        Assert.Equal(10f, volumes.Sum(), 3);
        Assert.InRange(volumes[0], 0.95f, 1f);
        Assert.InRange(volumes[1], 2.9f, 3f);
        Assert.InRange(volumes[2], 0f, 0.05f);
    }

    [Fact]
    public void PipePumpFillsUpperTankSlice()
    {
        float[] volumes = [1f, 0f, 0f, 10f];
        float[] capacities = [1f, 3f, 3f, 10f];
        Settle(
            volumes,
            capacities,
            [new(0, 1, true, true), new(1, 2, true, true), new(0, 3, true, true)],
            z: [0, 0, 1, 0],
            pipeCount: 1,
            flowCount: 3,
            sourceLift: [2f, 0f, 0f, 0f],
            qMax: [0.2f, 0f, 0f, 0f],
            headAt: BottomOnlyTankHead,
            ticks: 80);

        Assert.Equal(11f, volumes.Sum(), 3);
        Assert.InRange(volumes[1], 2.9f, 3f);
        Assert.InRange(volumes[2], 2.9f, 3f);
    }

    [Fact]
    public void HighTankDrainsDownBothSidesOfArch()
    {
        float[] volumes = [0f, 0f, 0f, 0f, 0f, 3f];
        float[] capacities = [1f, 1f, 1f, 1f, 1f, 3f];
        Settle(
            volumes,
            capacities,
            [
                new(0, 1, true, true),
                new(1, 2, true, true),
                new(2, 3, true, true),
                new(3, 4, true, true),
                new(2, 5, true, true),
            ],
            z: [0, 1, 2, 1, 0, 2],
            pipeCount: 5,
            headAt: (i, v) => i < 5
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
        Settle(
            volumes,
            capacities,
            [new(0, 1, true, true), new(0, 2, true, true)],
            z: [0, 0, 0],
            pipeCount: 2,
            headAt: (i, v) => i < 2
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
        Settle(
            volumes,
            capacities,
            UBendFromWell(),
            z: [0, 1, 2, 1, 0, 0],
            pipeCount: 5,
            headAt: GravityUBendWithWell);

        Assert.Equal(2f, volumes.Sum(), 3);
        Assert.InRange(volumes[4], 0.95f, 1f);
        Assert.InRange(volumes[3], 0f, 0.05f);
        Assert.InRange(volumes[0], 0f, 0.05f);
    }

    [Fact]
    public void PumpColumnFillsOutletPastPacketStall()
    {
        float[] volumes = [0.85f, 2f];
        float[] capacities = [1f, 6f];
        Settle(
            volumes,
            capacities,
            [new(0, 1, false, true)],
            z: [0, 0],
            pipeCount: 1,
            headAt: (i, v) => i == 0
                ? PipeFlowSolver.PipeHead(0, v)
                : PipeFlowSolver.PumpHead(0, v));

        Assert.InRange(volumes[0], 0.95f, 1f);
        Assert.InRange(volumes[1], 1.85f, 1.9f);
    }

    [Fact]
    public void WellAtLowerOccupiedFaceDoesNotFillUpperPipe()
    {
        float[] volumes = [0f, 2f];
        float[] capacities = [1f, 6f];
        Settle(
            volumes,
            capacities,
            [new(0, 1, false, true)],
            z: [1, 0],
            pipeCount: 1,
            headAt: (i, v) => i == 0
                ? PipeFlowSolver.PipeHead(1, v)
                : PipeFlowSolver.PumpHead(0, v));

        Assert.InRange(volumes[0], 0f, 0.05f);
        Assert.InRange(volumes[1], 1.95f, 2f);
    }

    [Fact]
    public void WellFillsPipeAtConnectedOutlet()
    {
        float[] volumes = [0f, 2f];
        float[] capacities = [1f, 6f];
        Settle(
            volumes,
            capacities,
            [new(0, 1, false, true)],
            z: [1, 1],
            pipeCount: 1,
            headAt: (i, v) => i == 0
                ? PipeFlowSolver.PipeHead(1, v)
                : PipeFlowSolver.PumpHead(1, v));

        Assert.InRange(volumes[0], 0.95f, 1f);
        Assert.InRange(volumes[1], 0.95f, 1.05f);
    }

    [Fact]
    public void WaterPumpZeroLiftDoesNotClimbUBend()
    {
        float[] volumes = [0f, 0f, 0f, 0f, 0f, 2f];
        float[] capacities = [1f, 1f, 1f, 1f, 1f, 6f];
        Settle(
            volumes,
            capacities,
            UBendFromWell(),
            z: [0, 1, 2, 1, 0, 0],
            pipeCount: 5,
            headAt: GravityUBendWithWell);

        Assert.Equal(2f, volumes.Sum(), 3);
        Assert.InRange(volumes[4], 0.95f, 1f);
        Assert.InRange(volumes[3], 0f, 0.05f);
        Assert.InRange(volumes[2], 0f, 0.05f);
        Assert.InRange(volumes[0], 0f, 0.05f);
    }

    [Fact]
    public void FullRiserDrainsIntoEmptyRiserThroughFullMain()
    {
        float[] volumes = [1f, 1f, 1f, 0f];
        Settle(
            volumes,
            Caps(4),
            [
                new(0, 1, true, true),
                new(1, 2, true, true),
                new(2, 3, true, true),
            ],
            z: [1, 0, 0, 1]);

        Assert.Equal(3f, volumes.Sum(), 4);
        Assert.InRange(volumes[1], 0.99f, 1f);
        Assert.InRange(volumes[2], 0.99f, 1f);
        Assert.Equal(1f, volumes[0] + volumes[3], 3);
        Assert.InRange(volumes[0], 0.45f, 0.55f);
        Assert.InRange(volumes[3], 0.45f, 0.55f);
    }

    [Fact]
    public void HighFullColumnFillsLowerEmptyBranch()
    {
        // (17,25) stack z=4..7, main to (20,25,4), empty six-way (20,25,5) and four-way (21,25,5).
        float[] volumes = [1f, 1f, 1f, 0.99f, 1f, 1f, 0f, 0f];
        Settle(
            volumes,
            Caps(8),
            [
                new(0, 1, true, true),
                new(1, 2, true, true),
                new(2, 3, true, true),
                new(0, 4, true, true),
                new(4, 5, true, true),
                new(5, 6, true, true),
                new(6, 7, true, true),
            ],
            z: [4, 5, 6, 7, 4, 4, 5, 5]);

        Assert.Equal(5.99f, volumes.Sum(), 3);
        Assert.True(volumes[6] + volumes[7] > 0.5f, $"branch stayed empty: {volumes[6]:0.00} {volumes[7]:0.00}");
        Assert.True(volumes[3] < 0.5f, $"riser did not drop: {volumes[3]:0.00}");
    }

    [Fact]
    public void FullMainEqualizesTwoRisers()
    {
        float[] volumes = [0.6f, 1f, 1f, 1f, 0f];
        Settle(
            volumes,
            Caps(5),
            [
                new(0, 1, true, true),
                new(1, 2, true, true),
                new(2, 3, true, true),
                new(3, 4, true, true),
            ],
            z: [1, 0, 0, 0, 1]);

        Assert.Equal(3.6f, volumes.Sum(), 4);
        Assert.InRange(volumes[1], 0.99f, 1f);
        Assert.InRange(volumes[2], 0.99f, 1f);
        Assert.InRange(volumes[3], 0.99f, 1f);
        Assert.Equal(0.6f, volumes[0] + volumes[4], 3);
        Assert.InRange(volumes[0], 0.25f, 0.35f);
        Assert.InRange(volumes[4], 0.25f, 0.35f);
    }

    [Fact]
    public void ValveBlocksReverseVessels()
    {
        float[] volumes = [0f, 1f, 1f, 0.8f];
        Settle(
            volumes,
            Caps(4),
            [
                new(0, 1, true, true),
                new(1, 2, true, false),
                new(2, 3, true, true),
            ],
            z: [1, 0, 0, 1]);

        Assert.InRange(volumes[0], 0f, 0.02f);
        Assert.InRange(volumes[3], 0.78f, 0.82f);
    }

    [Fact]
    public void ValveAllowsForwardVessels()
    {
        float[] volumes = [0.8f, 1f, 1f, 0f];
        Settle(
            volumes,
            Caps(4),
            [
                new(0, 1, true, true),
                new(1, 2, true, false),
                new(2, 3, true, true),
            ],
            z: [1, 0, 0, 1]);

        Assert.InRange(volumes[0], 0.35f, 0.45f);
        Assert.InRange(volumes[3], 0.35f, 0.45f);
    }

    [Fact]
    public void ValveHighTopFillsLowerReachableTop()
    {
        float[] volumes = [0f, 1f, 1f, 0.8f, 0f];
        Settle(
            volumes,
            Caps(5),
            [
                new(0, 1, true, true),
                new(1, 2, true, false),
                new(2, 3, true, true),
                new(2, 4, true, true),
            ],
            z: [1, 0, 0, 1, 0]);

        Assert.InRange(volumes[0], 0f, 0.02f);
        Assert.True(volumes[4] > 0.3f);
        Assert.True(volumes[3] < 0.5f);
    }

    [Fact]
    public void PipePumpDoesNotSkipEmptyCrest()
    {
        float[] volumes = [0f, 0f, 0f, 0f, 1f, 2f];
        float[] capacities = [1f, 1f, 1f, 1f, 1f, 6f];
        float[] sourceLift = [0f, 0f, 0f, 0f, 2f, 0f];
        float[] qMax = [0f, 0f, 0f, 0f, 0.2f, 0f];
        var extra = new float[6];

        for (var i = 0; i < 8; i++)
        {
            PipeFlowSolver.Run(
                volumes,
                capacities,
                [0, 1, 2, 1, 0, 0],
                UBendFromWell(),
                pipeCount: 5,
                flowCount: 5,
                sourceLift,
                qMax,
                gravitySubsteps: 4,
                kDt: 0.25f,
                extra,
                FillUBendWithWell);

            if (volumes[2] < 0.01f)
            {
                Assert.InRange(volumes[0], 0f, 0.05f);
                Assert.InRange(volumes[1], 0f, 0.05f);
            }

            Assert.Equal(0f, extra[0]);
        }

        Assert.Equal(3f, volumes.Sum(), 3);
        Assert.True(volumes[4] + volumes[3] + volumes[5] > 1.4f);
    }

    [Fact]
    public void PipePumpFillsFarSideAfterCrest()
    {
        float[] volumes = [0f, 0f, 0f, 0f, 1f, 8f];
        float[] capacities = [1f, 1f, 1f, 1f, 1f, 8f];
        Settle(
            volumes,
            capacities,
            UBendFromWell(),
            z: [0, 1, 2, 1, 0, 0],
            pipeCount: 5,
            sourceLift: [0f, 0f, 0f, 0f, 2f, 0f],
            qMax: [0f, 0f, 0f, 0f, 0.2f, 0f],
            headAt: GravityUBendWithWell,
            ticks: 80);

        Assert.Equal(9f, volumes.Sum(), 3);
        Assert.InRange(volumes[0], 0.9f, 1f);
        Assert.InRange(volumes[4], 0.9f, 1f);

        var extra = new float[6];
        PipeFlowSolver.ComputeRemainingLift(
            [0, 1, 2, 1, 0, 0],
            volumes,
            capacities,
            UBendFromWell(),
            [0f, 0f, 0f, 0f, 2f, 0f],
            extra,
            pipeCount: 5);
        Assert.Equal(0f, extra[0]);
        Assert.Equal(0f, extra[1]);
    }

    [Fact]
    public void PipePumpInletAcceptsWell()
    {
        float[] volumes = [0f, 0f, 1f];
        float[] capacities = [1f, 1f, 1f];
        Settle(
            volumes,
            capacities,
            [
                new(0, 1, true, false),
                new(0, 2, false, true),
            ],
            z: [0, 0, 0],
            pipeCount: 2,
            sourceLift: [2f, 0f, 0f],
            qMax: [0.2f, 0f, 0f],
            headAt: (i, v) => i < 2
                ? PipeFlowSolver.PipeHead(0, v)
                : PipeFlowSolver.PumpHead(0, v));

        Assert.True(volumes[0] + volumes[1] > 0.5f);
        Assert.InRange(volumes[2], 0f, 0.55f);
    }

    [Fact]
    public void PipePumpFillsRiser()
    {
        float[] volumes = [1f, 0f];
        Settle(
            volumes,
            Caps(2),
            [new(0, 1, true, false)],
            z: [0, 1],
            sourceLift: [2f, 0f],
            qMax: [0.2f, 0f],
            ticks: 12);

        Assert.Equal(1f, volumes.Sum(), 4);
        Assert.InRange(volumes[1], 0.99f, 1f);
    }

    [Fact]
    public void EmptySourcePressurizesOnlyTheNextPipe()
    {
        float[] extra = [0f, 0f, 0f];
        PipeFlowSolver.ComputeRemainingLift(
            [0, 0, 0],
            [0f, 0f, 0f],
            Caps(3),
            [new(0, 1, true, true), new(1, 2, true, true)],
            [2f, 0f, 0f],
            extra,
            pipeCount: 3);

        Assert.Equal(2f, extra[0]);
        Assert.Equal(2f, extra[1]);
        Assert.Equal(0f, extra[2]);
    }

    [Fact]
    public void FullPathForwardsLiftHorizontally()
    {
        float[] extra = [0f, 0f, 0f];
        PipeFlowSolver.ComputeRemainingLift(
            [0, 0, 0],
            [1f, 1f, 0f],
            Caps(3),
            [new(0, 1, true, true), new(1, 2, true, true)],
            [2f, 0f, 0f],
            extra,
            pipeCount: 3);

        Assert.Equal(2f, extra[0]);
        Assert.Equal(2f, extra[1]);
        Assert.Equal(2f, extra[2]);
    }

    [Fact]
    public void EmptyCrestDoesNotPressurizeFarSide()
    {
        float[] extra = [0f, 0f, 0f, 0f, 0f];
        PipeFlowSolver.ComputeRemainingLift(
            [0, 1, 2, 1, 0],
            [0f, 0f, 0f, 0f, 1f],
            Caps(5),
            UBend(),
            [0f, 0f, 0f, 0f, 2f],
            extra,
            pipeCount: 5);

        Assert.Equal(2f, extra[4]);
        Assert.Equal(1f, extra[3]);
        Assert.Equal(0f, extra[2]);
        Assert.Equal(0f, extra[0]);
    }

    [Fact]
    public void SpentLiftDoesNotReturnOnTheFarDescent()
    {
        float[] extra = [0f, 0f, 0f, 0f, 0f];
        PipeFlowSolver.ComputeRemainingLift(
            [0, 1, 2, 1, 0],
            [1f, 1f, 1f, 1f, 1f],
            Caps(5),
            UBend(),
            [0f, 0f, 0f, 0f, 2f],
            extra,
            pipeCount: 5);

        Assert.Equal(2f, extra[4]);
        Assert.Equal(1f, extra[3]);
        Assert.Equal(0f, extra[2]);
        Assert.Equal(0f, extra[1]);
        Assert.Equal(0f, extra[0]);
    }

    [Fact]
    public void OneWayBlocksReverseLift()
    {
        float[] extra = [0f, 0f];
        PipeFlowSolver.ComputeRemainingLift(
            [0, 0],
            [0f, 1f],
            Caps(2),
            [new(0, 1, true, false)],
            [2f, 0f],
            extra,
            pipeCount: 2);

        Assert.Equal(2f, extra[0]);
        Assert.Equal(2f, extra[1]);

        extra.AsSpan().Clear();
        PipeFlowSolver.ComputeRemainingLift(
            [0, 0],
            [0f, 1f],
            Caps(2),
            [new(0, 1, false, true)],
            [2f, 0f],
            extra,
            pipeCount: 2);

        Assert.Equal(2f, extra[0]);
        Assert.Equal(0f, extra[1]);
    }

    [Fact]
    public void ExtraDoesNotEnterNonPipeNodes()
    {
        float[] extra = [0f, 0f];
        PipeFlowSolver.ComputeRemainingLift(
            [0, 0],
            [1f, 10f],
            [1f, 10f],
            [new(0, 1, true, true)],
            [2f, 0f],
            extra,
            pipeCount: 1);

        Assert.Equal(2f, extra[0]);
        Assert.Equal(0f, extra[1]);
    }

    static float BottomOnlyTankHead(int i, float v)
        => i switch
        {
            0 => PipeFlowSolver.PipeHead(0, v),
            1 => PipeFlowSolver.Surface(0, v, 3f),
            2 => PipeFlowSolver.Surface(1, v, 3f),
            _ => PipeFlowSolver.PumpHead(0, v),
        };

    static int PipeZ(int i) => i switch
    {
        0 => 0,
        1 => 1,
        2 => 2,
        3 => 1,
        4 => 0,
        _ => 0,
    };

    static float GravityUBendWithWell(int i, float v)
        => i < 5
            ? PipeFlowSolver.PipeHead(PipeZ(i), v)
            : PipeFlowSolver.PumpHead(0, v);

    static void FillUBendWithWell(Span<float> heads, ReadOnlySpan<float> volumes)
    {
        for (var i = 0; i < volumes.Length; i++)
        {
            heads[i] = GravityUBendWithWell(i, volumes[i]);
        }
    }

    static PipeFlowEdge[] UBend() =>
    [
        new(0, 1, true, true),
        new(1, 2, true, true),
        new(2, 3, true, true),
        new(3, 4, true, true),
    ];

    static PipeFlowEdge[] UBendWithEnd() =>
    [
        .. UBend(),
        new(4, 5, true, true),
    ];

    static PipeFlowEdge[] UBendFromWell() =>
    [
        new(0, 1, true, true),
        new(1, 2, true, true),
        new(2, 3, true, true),
        new(3, 4, false, true),
        new(4, 5, false, true),
    ];

    static PipeFlowEdge[] Chain(int edgeCount)
    {
        var edges = new PipeFlowEdge[edgeCount];
        for (var i = 0; i < edgeCount; i++)
        {
            edges[i] = new(i, i + 1, true, true);
        }

        return edges;
    }

    static float[] Caps(int n)
    {
        var caps = new float[n];
        Array.Fill(caps, 1f);
        return caps;
    }

    static void SettleGravity(float[] volumes, float[] capacities, PipeFlowEdge[] edges, int[] z)
        => Settle(volumes, capacities, edges, z, volumes.Length, volumes.Length, null, null, 80, true);

    static void Settle(
        float[] volumes,
        float[] capacities,
        PipeFlowEdge[] edges,
        int[] z,
        int pipeCount = -1,
        int flowCount = -1,
        float[]? sourceLift = null,
        float[]? qMax = null,
        Func<int, float, float>? headAt = null,
        int ticks = 40)
        => Settle(volumes, capacities, edges, z, pipeCount, flowCount, sourceLift, qMax, ticks, gravityOnly: false, headAt);

    static void Settle(
        float[] volumes,
        float[] capacities,
        PipeFlowEdge[] edges,
        int[] z,
        int pipeCount,
        int flowCount,
        float[]? sourceLift,
        float[]? qMax,
        int ticks,
        bool gravityOnly,
        Func<int, float, float>? headAt = null)
    {
        var n = volumes.Length;
        var pipes = pipeCount < 0 ? n : pipeCount;
        var flowNodes = flowCount < 0 ? pipes : flowCount;
        var lift = sourceLift ?? new float[n];
        var flow = qMax ?? new float[n];
        var extra = new float[n];
        PipeHeadFill fill = (heads, current) =>
        {
            for (var i = 0; i < current.Length; i++)
            {
                heads[i] = headAt is null
                    ? PipeFlowSolver.PipeHead(z[i], current[i])
                    : headAt(i, current[i]);
            }
        };

        for (var i = 0; i < ticks; i++)
        {
            if (gravityOnly)
            {
                var heads = new float[n];
                for (var step = 0; step < 4; step++)
                {
                    fill(heads, volumes);
                    PipeFlowSolver.Equalize(volumes, heads, capacities, edges, 0.25f);
                }

                continue;
            }

            PipeFlowSolver.Run(volumes, capacities, z, edges, pipes, flowNodes, lift, flow, 4, 0.25f, extra, fill);
        }
    }
}
