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

    [Fact]
    public void WaterPumpFloorOmitsPipeTileAndEntranceFace()
    {
        var spec = BuildingPipeLayout.FromOccupied(
            [
                new(0, 1, 1),
                new(0, 2, 0), new(0, 2, 1), new(0, 2, 2),
                new(1, 0, 0), new(1, 0, 1),
                new(1, 1, 0), new(1, 1, 1), new(1, 1, 2),
                new(1, 2, 0),
            ],
            PipePortState.OpenOut,
            floorZ: 1,
            entrance: new(1, -1, 1),
            ignored: [new(0, 2, 1)]);

        Assert.NotNull(spec);
        Assert.Equal(3, spec!.Ports.Length);
        Assert.All(spec.Ports, port => Assert.Equal(1, port.Coordinates.z));
        Assert.DoesNotContain(spec.Ports, port => port.Coordinates == new Vector3Int(0, 2, 1));

        var walkway = PortAt(spec, 0, 1, 1);
        Assert.Equal(
            Directions3D.Down | Directions3D.Left | Directions3D.Bottom | Directions3D.Top,
            walkway.Directions);

        var front = PortAt(spec, 1, 0, 1);
        Assert.False(front.Directions.HasFlag(Directions3D.Down));
        Assert.Equal(Directions3D.Left | Directions3D.Right | Directions3D.Top, front.Directions);

        var body = PortAt(spec, 1, 1, 1);
        Assert.Equal(Directions3D.Up | Directions3D.Right, body.Directions);
    }

    [Fact]
    public void DischargeFloorOmitsChuteAndEntranceFace()
    {
        var spec = BuildingPipeLayout.FromOccupied(
            [new(0, 0, 0), new(0, 1, 0)],
            PipePortState.OpenIn,
            floorZ: 0,
            entrance: new(0, -1, 0),
            ignored: [new(0, 1, 0)]);

        Assert.NotNull(spec);
        Assert.Single(spec!.Ports);
        Assert.Equal(new Vector3Int(0, 0, 0), spec.Ports[0].Coordinates);
        Assert.Equal(PipePortState.OpenIn, spec.Ports[0].State);
        Assert.Equal(
            Directions3D.Left | Directions3D.Right | Directions3D.Bottom | Directions3D.Top,
            spec.Ports[0].Directions);
    }

    static PipePortSpec PortAt(BuildingPipeSpec spec, int x, int y, int z)
        => spec.Ports.Single(port => port.Coordinates == new Vector3Int(x, y, z));
}
