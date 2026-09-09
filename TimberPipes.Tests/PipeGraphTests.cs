namespace TimberPipes.Tests;

public class PipeGraphTests
{
    [Fact]
    public void FlushClearsFlagEvenIfRefreshRunsAgain()
    {
        var graph = new PipeGraph(FrozenDictionary<Vector3Int, BuildingPipe>.Empty);
        graph.Contaminate(new("Water", "Badwater"));
        Assert.True(graph.Cause.HasPair);
        graph.Flush();
        graph.RefreshContamination();

        Assert.False(graph.Contaminated);
        Assert.False(graph.Cause.HasPair);
        graph.RefreshContamination();

        Assert.False(graph.Contaminated);
        Assert.Null(graph.FluidGoodId);
    }

    [Fact]
    public void AdoptFluidStaysOnGraphUntilFlush()
    {
        var graph = new PipeGraph(FrozenDictionary<Vector3Int, BuildingPipe>.Empty);
        graph.AdoptFluid("Water");
        graph.AdoptFluid("Water");

        Assert.Equal("Water", graph.FluidGoodId);
        Assert.False(graph.Contaminated);

        graph.AdoptFluid("Badwater");
        Assert.True(graph.Contaminated);
        Assert.Equal("Water", graph.Cause.GoodA);
        Assert.Equal("Badwater", graph.Cause.GoodB);
    }
}
