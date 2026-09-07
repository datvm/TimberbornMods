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
    public void QuantizePendingToGoods()
    {
        var pending = 0.45f;
        var delta = PipeFlowSolver.QuantizePending(ref pending);

        Assert.Equal(2, delta);
        Assert.InRange(pending, 0.049f, 0.051f);
    }

    static void StepUntilSettled(float[] volumes, float[] capacities, PipeFlowEdge[] edges, int[] z)
    {
        var heads = new float[volumes.Length];
        for (var i = 0; i < 40; i++)
        {
            for (var n = 0; n < volumes.Length; n++)
            {
                heads[n] = PipeFlowSolver.PipeHead(z[n], volumes[n]);
            }

            PipeFlowSolver.Equalize(volumes, heads, capacities, edges, 0.25f);
        }
    }
}
