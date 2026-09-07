namespace TimberPipes.Tests;

public class PipeHeadliftTests
{
    [Fact]
    public void PickInjectFillsLowestFirst()
    {
        int[] z = [2, 0, 1];
        float[] free = [1f, 0.5f, 1f];

        var i = PipeHeadlift.PickInjectIndex(z, free, outletZ: 0, ratedLift: 2f, packet: 0.2f);

        Assert.Equal(1, i);
    }

    [Fact]
    public void PickInjectSkipsFullLowerPipe()
    {
        int[] z = [0, 1];
        float[] free = [0f, 1f];

        var i = PipeHeadlift.PickInjectIndex(z, free, outletZ: 0, ratedLift: 2f, packet: 0.2f);

        Assert.Equal(1, i);
    }

    [Fact]
    public void PickInjectRespectsHeadliftCeiling()
    {
        int[] z = [0, 3];
        float[] free = [0f, 1f];

        var i = PipeHeadlift.PickInjectIndex(z, free, outletZ: 0, ratedLift: 2f, packet: 0.2f);

        Assert.Equal(-1, i);
    }

    [Fact]
    public void TankHeadliftIsSurfaceMinusPipeZ()
    {
        List<PipeHeadliftSource> sources = [new(4f, 0)];

        Assert.Equal(4f, PipeHeadlift.ExtraAt(0, sources));
        Assert.Equal(2f, PipeHeadlift.ExtraAt(2, sources));
        Assert.Equal(0f, PipeHeadlift.ExtraAt(4, sources));
    }

    [Fact]
    public void CanLiftToIncludesRatedHeight()
    {
        Assert.True(PipeHeadlift.CanLiftTo(2, 0, 2f));
        Assert.False(PipeHeadlift.CanLiftTo(3, 0, 2f));
    }
}
