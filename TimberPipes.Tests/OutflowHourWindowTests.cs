namespace TimberPipes.Tests;

public class OutflowHourWindowTests
{
    [Fact]
    public void DropsSamplesOlderThanHour()
    {
        var window = new OutflowHourWindow();
        window.Add(0f, 2f);
        Assert.Equal(2f, window.HourVolume, 4);

        window.Prune(OutflowHourWindow.HourInDays + 0.0001f);
        Assert.Equal(0f, window.HourVolume, 4);
    }

    [Fact]
    public void KeepsSamplesInsideHour()
    {
        var window = new OutflowHourWindow();
        window.Add(0.01f, 1f);
        window.Add(0.02f, 3f);
        window.Prune(0.02f + OutflowHourWindow.HourInDays - 0.005f);
        Assert.Equal(3f, window.HourVolume, 4);
    }

    [Fact]
    public void UnlimitedWhenLimitIsZero()
    {
        Assert.False(FlowLimitIo.IsClosed(0f, 100f));
        Assert.True(float.IsPositiveInfinity(FlowLimitIo.Remaining(0f, 100f)));
    }

    [Fact]
    public void ClosesAtHourlyLimit()
    {
        Assert.True(FlowLimitIo.IsClosed(1f, 1f));
        Assert.False(FlowLimitIo.IsClosed(1f, 0.5f));
        Assert.Equal(0.25f, FlowLimitIo.Remaining(1f, 0.75f), 4);
    }

    [Fact]
    public void ConvertsTickVolumeToRealtimeCms()
    {
        Assert.Equal(2f, FlowLimitIo.TickVolumeToM3PerSecond(0.2f, tickIntervalSeconds: 0.1f), 4);
        Assert.Equal(0f, FlowLimitIo.TickVolumeToM3PerSecond(0.2f, tickIntervalSeconds: 0f), 4);
    }

    [Fact]
    public void CapsRemainingOnEqualize()
    {
        float[] volumes = [1f, 0f];
        float[] heads = [1f, 0f];
        float[] capacities = [1f, 1f];
        PipeFlowEdge[] edges = [new(0, 1, true, false)];
        float[] counted = [0f, 0f];
        float[] remaining = [0.1f, float.PositiveInfinity];

        PipeFlowSolver.Equalize(volumes, heads, capacities, edges, 10f, countedOutflow: counted, remainingOutflow: remaining);

        Assert.InRange(counted[0], 0.09f, 0.11f);
        Assert.Equal(0f, counted[1], 4);
        Assert.InRange(volumes[1], 0.09f, 0.11f);
        Assert.InRange(remaining[0], 0f, 0.01f);
    }

    [Fact]
    public void OpenOutOnlyIgnoresReverse()
    {
        float[] volumes = [0f, 1f];
        float[] heads = [0f, 1f];
        float[] capacities = [1f, 1f];
        PipeFlowEdge[] edges = [new(0, 1, true, false)];
        float[] counted = [0f, 0f];
        float[] remaining = [float.PositiveInfinity, float.PositiveInfinity];

        PipeFlowSolver.Equalize(volumes, heads, capacities, edges, 10f, countedOutflow: counted, remainingOutflow: remaining);

        Assert.Equal(0f, counted[0], 4);
        Assert.Equal(0f, counted[1], 4);
        Assert.Equal(0f, volumes[0], 4);
        Assert.Equal(1f, volumes[1], 4);
    }

    [Fact]
    public void ReverseTwoWayCountsSource()
    {
        float[] volumes = [0f, 1f];
        float[] heads = [0f, 1f];
        float[] capacities = [1f, 1f];
        PipeFlowEdge[] edges = [new(0, 1, true, true)];
        float[] counted = [0f, 0f];
        float[] remaining = [float.PositiveInfinity, float.PositiveInfinity];

        PipeFlowSolver.Equalize(volumes, heads, capacities, edges, 10f, countedOutflow: counted, remainingOutflow: remaining);

        Assert.Equal(0f, counted[0], 4);
        Assert.True(counted[1] > PipeFluids.MoveEpsilon);
        Assert.True(volumes[0] > PipeFluids.MoveEpsilon);
    }

    [Fact]
    public void UnlimitedRemainingStillCounts()
    {
        float[] volumes = [1f, 0f];
        float[] heads = [1f, 0f];
        float[] capacities = [1f, 1f];
        PipeFlowEdge[] edges = [new(0, 1, true, false)];
        float[] counted = [0f, 0f];
        float[] remaining = [float.PositiveInfinity, float.PositiveInfinity];

        PipeFlowSolver.Equalize(volumes, heads, capacities, edges, 10f, countedOutflow: counted, remainingOutflow: remaining);

        Assert.True(counted[0] > PipeFluids.MoveEpsilon);
        Assert.Equal(0f, counted[1], 4);
        Assert.True(float.IsPositiveInfinity(remaining[0]));
    }
}
