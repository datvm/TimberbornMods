namespace ConveyorBelt.Tests;

public class BeltPortIoTests
{
    [Fact]
    public void RequiredNeighborKindMirrorsInAndOut()
    {
        Assert.Equal(BeltPortKind.Out, BeltPortIo.RequiredNeighborKind(BeltPortKind.In));
        Assert.Equal(BeltPortKind.In, BeltPortIo.RequiredNeighborKind(BeltPortKind.Out));
        Assert.Equal(BeltPortKind.Both, BeltPortIo.RequiredNeighborKind(BeltPortKind.Both));
        Assert.Equal(BeltPortKind.None, BeltPortIo.RequiredNeighborKind(BeltPortKind.None));
    }

    [Fact]
    public void ImportPortAcceptsBeltOutput()
    {
        var import = Port(Vector3Int.zero, Direction3D.Down, BeltPortKind.In);
        var from = import.Target;
        ImmutableArray<BeltPort> ports = [import];

        Assert.True(BeltPortIo.HasFacingPort(ports, from, BeltPortKind.In));
        Assert.False(BeltPortIo.HasFacingPort(ports, from, BeltPortKind.Out));
        Assert.True(BeltPortIo.HasFacingPort(ports, from, BeltPortKind.Both));
    }

    [Fact]
    public void ExportPortAcceptsBeltInput()
    {
        var export = Port(Vector3Int.zero, Direction3D.Up, BeltPortKind.Out);
        var from = export.Target;
        ImmutableArray<BeltPort> ports = [export];

        Assert.True(BeltPortIo.HasFacingPort(ports, from, BeltPortKind.Out));
        Assert.False(BeltPortIo.HasFacingPort(ports, from, BeltPortKind.In));
    }

    [Fact]
    public void BidirectionalPortAcceptsEitherKind()
    {
        var both = Port(new Vector3Int(2, 1, 0), Direction3D.Left, BeltPortKind.Both);
        var from = both.Target;
        ImmutableArray<BeltPort> ports = [both];

        Assert.True(BeltPortIo.HasFacingPort(ports, from, BeltPortKind.In));
        Assert.True(BeltPortIo.HasFacingPort(ports, from, BeltPortKind.Out));
    }

    [Fact]
    public void EmptyPortsConnectNowhere()
    {
        var from = new Vector3Int(0, 0, 0);
        Assert.False(BeltPortIo.HasFacingPort([], from, BeltPortKind.In));
        Assert.False(BeltPortIo.HasFacingPort([], from, BeltPortKind.Out));
        Assert.False(BeltPortIo.HasFacingPort([], from, BeltPortKind.Both));
    }

    [Fact]
    public void WrongCellDoesNotMatch()
    {
        var import = Port(Vector3Int.zero, Direction3D.Down, BeltPortKind.In);
        ImmutableArray<BeltPort> ports = [import];
        Assert.False(BeltPortIo.HasFacingPort(ports, new Vector3Int(9, 9, 9), BeltPortKind.In));
    }

    [Fact]
    public void SingleCellOccupancyExposesEveryFace()
    {
        var ports = ConveyorConnection.GenerateFromOccupancy([Vector3Int.zero]);
        Assert.Single(ports);
        Assert.Equal(BeltPortKind.Both, ports[0].Kind);
        Assert.Equal(
            Directions3D.Down | Directions3D.Left | Directions3D.Up | Directions3D.Right | Directions3D.Bottom | Directions3D.Top,
            ports[0].Directions);
    }

    [Fact]
    public void OccupiedNeighborIsNotAPort()
    {
        var ports = ConveyorConnection.GenerateFromOccupancy(
        [
            Vector3Int.zero,
            new Vector3Int(0, 0, 1),
        ]);

        Assert.Equal(2, ports.Length);
        var lower = ports.Single(p => p.Coordinates == Vector3Int.zero);
        var upper = ports.Single(p => p.Coordinates == new Vector3Int(0, 0, 1));
        Assert.False(lower.Directions.HasFlag(Directions3D.Top));
        Assert.True(lower.Directions.HasFlag(Directions3D.Bottom));
        Assert.False(upper.Directions.HasFlag(Directions3D.Bottom));
        Assert.True(upper.Directions.HasFlag(Directions3D.Top));
    }

    static BeltPort Port(Vector3Int coordinates, Direction3D direction, BeltPortKind kind)
        => new(coordinates, direction, kind);
}
