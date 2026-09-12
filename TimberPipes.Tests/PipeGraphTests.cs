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

    [Fact]
    public void StatusCoordinatesPicksLowestXyz()
    {
        Assert.Null(PipeGraph.StatusCoordinates([]));
        Assert.Equal(
            new Vector3Int(0, 1, 2),
            PipeGraph.StatusCoordinates([
                new Vector3Int(2, 0, 0),
                new Vector3Int(0, 1, 2),
                new Vector3Int(0, 2, 0),
                new Vector3Int(1, 0, 0),
            ]));
        Assert.True(PipeGraph.CompareCoordinates(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 1)) < 0);
        Assert.True(PipeGraph.CompareCoordinates(new Vector3Int(0, 1, 0), new Vector3Int(0, 0, 1)) > 0);
    }
}
