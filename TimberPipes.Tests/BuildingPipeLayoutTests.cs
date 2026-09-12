namespace TimberPipes.Tests;

public class BuildingPipeLayoutTests
{
    [Fact]
    public void SingleCellExposesEveryFace()
    {
        var spec = BuildingPipeLayout.FromOccupied([new Vector3Int(0, 0, 0)], PipePortState.Open);

        Assert.NotNull(spec);
        Assert.Single(spec!.Ports);
        Assert.Equal(new Vector3Int(0, 0, 0), spec.Ports[0].Coordinates);
        Assert.Equal(PipePortState.Open, spec.Ports[0].State);
        Assert.Equal(
            Directions3D.Down | Directions3D.Left | Directions3D.Up | Directions3D.Right | Directions3D.Bottom | Directions3D.Top,
            spec.Ports[0].Directions);
    }

    [Fact]
    public void StackOmitsInternalVerticalFaces()
    {
        var spec = BuildingPipeLayout.FromOccupied(
            [new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 1)],
            PipePortState.Open);

        Assert.NotNull(spec);
        Assert.Equal(2, spec!.Ports.Length);
        Assert.Equal(new Vector3Int(0, 0, 0), spec.Ports[0].Coordinates);
        Assert.Equal(
            Directions3D.Down | Directions3D.Left | Directions3D.Up | Directions3D.Right | Directions3D.Bottom,
            spec.Ports[0].Directions);
        Assert.Equal(new Vector3Int(0, 0, 1), spec.Ports[1].Coordinates);
        Assert.Equal(
            Directions3D.Down | Directions3D.Left | Directions3D.Up | Directions3D.Right | Directions3D.Top,
            spec.Ports[1].Directions);
    }

    [Fact]
    public void AdjacentCellsOmitSharedHorizontalFace()
    {
        var spec = BuildingPipeLayout.FromOccupied(
            [new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0)],
            PipePortState.OpenOut);

        Assert.NotNull(spec);
        Assert.Equal(PipePortState.OpenOut, spec!.Ports[0].State);
        Assert.False(spec.Ports[0].Directions.HasFlag(Directions3D.Right));
        Assert.True(spec.Ports[0].Directions.HasFlag(Directions3D.Left));
        Assert.False(spec.Ports[1].Directions.HasFlag(Directions3D.Left));
        Assert.True(spec.Ports[1].Directions.HasFlag(Directions3D.Right));
    }

    [Fact]
    public void EmptyOccupiedReturnsNull()
        => Assert.Null(BuildingPipeLayout.FromOccupied([], PipePortState.Open));
}
